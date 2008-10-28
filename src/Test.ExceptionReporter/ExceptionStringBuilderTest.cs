using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using ExceptionReporting.Core;
using ExceptionReporting.Extensions;
using ExceptionReporting.SystemInfo;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace Test.ExceptionReporter
{
	[TestFixture]
	public class ExceptionStringBuilderTest
	{
		[Test]
		public void CanBuild_ReferencedAssemblies_String()
		{
			Assembly dataAssembly = Assembly.GetExecutingAssembly();

			ExceptionReportInfo reportInfo = CreateReportInfo();
			reportInfo.ShowAssembliesTab = true;
			reportInfo.AppAssembly = dataAssembly;

			var stringBuilder = new ExceptionStringBuilder(reportInfo);

			string assembliesString = stringBuilder.Build();

			Assert.That(assembliesString, Is.Not.Null);
			Assert.That(assembliesString.Length, Is.GreaterThan(0));

			StringAssert.Contains("nunit", assembliesString);	// coupled to NUnit, but better than nothing
			StringAssert.Contains("ExceptionReporter, Version=", assembliesString);
			StringAssert.Contains("System, Version=", assembliesString);
			StringAssert.Contains(Environment.NewLine, assembliesString);
		}

		private static ExceptionReportInfo CreateReportInfo()
		{
			return new ExceptionReportInfo
	        {
				ShowAssembliesTab = false,
	       		ShowConfigTab = false,
	       		ShowGeneralTab = false,
	       		ShowSysInfoTab = false,
	       		ShowExceptionsTab = false,
	        };
		}

		[Test]
		public void CanBuild_HierarchyString_With_Root_And_InnerException()
		{
			ExceptionReportInfo reportInfo = CreateReportInfo();
			reportInfo.ShowExceptionsTab = true;
			reportInfo.Exception = new ArgumentOutOfRangeException("OuterException", new ArgumentNullException("Inner" + "Exception"));

			var stringBuilder = new ExceptionStringBuilder(reportInfo);
			string hierarchyString = stringBuilder.Build();

			// created expected string
			StringBuilder expectedString = new StringBuilder().AppendDottedLine()
				.AppendLine("[Exception Info]").AppendLine()
				.AppendLine("Top-level Exception")
				.AppendLine("Type:        System.ArgumentOutOfRangeException")
				.AppendLine("Message:     OuterException")
				.AppendLine("Source:      ")
				.AppendLine()
				.AppendLine("Inner Exception 1")
				.AppendLine("Type:        System.ArgumentNullException")
				.AppendLine("Message:     Value cannot be null.")
				.AppendLine("Parameter name: InnerException")
				.AppendLine("Source:")
				.AppendLine().AppendDottedLine().AppendLine();

			Assert.That(hierarchyString, Is.EqualTo(expectedString.ToString()));
		}

		[Test]
		public void CanBuild_SysInfoString()
		{
			//setup SysInfoResult object
			IList<SysInfoResult> results = new List<SysInfoResult>();
			var result = new SysInfoResult("Memory");
			result.Nodes.Add("Physical Memory");
			var resultChild = new SysInfoResult("Bla");
			result.ChildResults.Add(resultChild);
			resultChild.Nodes.Add("Version:2.66");
			results.Add(result);

			// created expected string
			StringBuilder expectedString = new StringBuilder().AppendDottedLine();
			expectedString.AppendLine("[System Info]").AppendLine();
			expectedString.AppendLine("Memory");
			expectedString.AppendLine("-Physical Memory");
			expectedString.AppendLine("--Version:2.66");
			expectedString.AppendLine().AppendDottedLine().AppendLine();

			// we force only ShowSysInfoTab to "build a string" by passing an ExceptionReportInfo object with ShowSysInfoTab true
			ExceptionReportInfo reportInfo = CreateReportInfo();
			reportInfo.ShowSysInfoTab = true;

			var stringBuilder = new ExceptionStringBuilder(reportInfo, results);
			string sysInfoString = stringBuilder.Build();

			Assert.That(sysInfoString, Is.EqualTo(expectedString.ToString()));
		}
	}
}