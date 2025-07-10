using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Client.EDI.IssueManager.Business.Test;
using Enterprise.Client.EDI.Registry.Business;

namespace Enterprise.Client.EDI.IssueManager.Business.Tests
{
	class StackLinesWeightsLogAutoAssignerTests : AssignmentTestHelper
	{
		protected override void SetUp()
		{
			base.SetUp();
			EDIDataRegistry.Instance.StackLineCountNumberOfImportedLogs = 100;
		}

		public void TestIssueAssignmentEquality()
		{
			AssertEquals(new IssueAssignment("A", "B", "C"), new IssueAssignment("A", "B", "C"));
			AssertNotEquals(new IssueAssignment("A", "B", "C"), new IssueAssignment("A", "B", "X"));
			AssertNotEquals(new IssueAssignment("A", "B", "C"), new IssueAssignment("A", "X", "C"));
			AssertNotEquals(new IssueAssignment("A", "B", "C"), new IssueAssignment("X", "B", "C"));
			AssertNotEquals(new IssueAssignment("A", "B", "C"), new IssueAssignment("C", "B", "A"));
			AssertEquals(new IssueAssignment("A", "B", null), new IssueAssignment("A", "B", null));
			AssertNotEquals(new IssueAssignment("A", "B", null), new IssueAssignment("A", "B", "X"));
			AssertNotEquals(new IssueAssignment("A", "B", ""), new IssueAssignment("A", "X", null));
			AssertNotEquals(new IssueAssignment("A", "B", ""), new IssueAssignment("X", "B", null));
			AssertEquals(new IssueAssignment("A", null, "C"), new IssueAssignment("A", null, "C"));
			AssertNotEquals(new IssueAssignment("A", null, "C"), new IssueAssignment("A", "X", "C"));
			AssertNotEquals(new IssueAssignment("A", null, "C"), new IssueAssignment("A", null, "X"));
			AssertNotEquals(new IssueAssignment("A", null, "C"), new IssueAssignment("X", null, "C"));
		}

		public void TestGetAssignment_GenericMethodInCallButNormalMethodInDB()
		{
			var report = GetExceptionXml(
				@"<Call>   at MyCode.MyClass.MyFunction[T](Stream jsonStream)</Call>");
			var publishedAssemblyPK = AddPublishedAssembly("MyAssembly.dll", "$/MyCode/MyAssembly");
			var publishedClassPK = AddPublishedClass(publishedAssemblyPK, "MyCode.MyClass");
			AddPublishedMethod(publishedClassPK, "MyFunction");
			AddSourceTreeResponsibility("$/MyCode", "A", "B", "C");

			AssertLogAssignment(new IssueAssignment("A", "B", "C"), report);
		}

		public void TestGetAssignment()
		{
			var report = GetExceptionXml(@"
				<Call Assembly=""MyAssembly.dll"">   at MyCode.MyClass.MyFunction()</Call>");
			AddStackLineCount("MyAssembly.dll", "MyCode.MyClass.MyFunction()", 1);
			AddPublishedAssembly("MyAssembly.dll", "$/MyCode/MyAssembly");
			AddSourceTreeResponsibility("$/MyCode", "A", "B", "C");
			AssertLogAssignment(new IssueAssignment("A", "B", "C"), report);
		}

		public void TestGetAssignment_LowerWeightExceptionStackLineRegexes()
		{
			IList<(ExceptionKeyRegexCollection regexCollection, IssueAssignment expectedIssueAssignment)> testCaseItems =
			[
				(
				// one regex and matches
					[
						new ExceptionKeyRegex
						{
							Regex = "SampleNamespace1.SampleNamespace2.SampleClass.*", Description = ""
						}
					],
					new IssueAssignment("A1", "B1", "C1")
				),
				(
				// multiple regex and one matches
					[
						new ExceptionKeyRegex
						{
							Regex = "SampleNamespace1.SampleNamespace7.SampleClass.*", Description = ""
						},
						new ExceptionKeyRegex
						{
							Regex = "SampleNamespace1.SampleNamespace2.SampleClass.*", Description = ""
						},
						new ExceptionKeyRegex
						{
							Regex = "SampleNamespace1.SampleNamespace4.SampleClass.*", Description = ""
						}
					],
					new IssueAssignment("A1", "B1", "C1")
				),
				(
				// no matches regex 
					[
						new ExceptionKeyRegex
						{
							Regex = "SampleNamespace1.SampleNamespace5.SampleClass.*", Description = ""
						},
						new ExceptionKeyRegex
						{
							Regex = "SampleNamespace1.SampleNamespace4.SampleClass.*", Description = ""
						}
					],
					new IssueAssignment("A", "B", "C")
				),
				(
				// all regex matches, all stack line will have the same weight
					[
						new ExceptionKeyRegex
						{
							Regex = "SampleNamespace1.SampleNamespace2.SampleClass.*", Description = ""
						},
						new ExceptionKeyRegex
						{
							Regex = "BusinessNamespace1.BusinessNamespace2.BusinessClass.*", Description = ""
						}
					],
					null
				),
			];

			var sampleCodePublishedAssembly = AddPublishedAssembly("SampleAssembly.dll", "$/SampleCode/SampleAssembly");
			var sampleCodePublishedClass = AddPublishedClass(sampleCodePublishedAssembly, "SampleNamespace1.SampleNamespace2.SampleClass");
			AddPublishedMethod(sampleCodePublishedClass, "SampleMethod");
			AddStackLineCount("SampleAssembly.dll", "SampleNamespace1.SampleNamespace2.SampleClass.SampleMethod()", 1);
			AddSourceTreeResponsibility("$/SampleCode", "A", "B", "C");

			var businessCodePublishedAssembly = AddPublishedAssembly("BusinessAssembly.dll", "$/BusinessCode/BusinessAssembly");
			var businessCodePublishedClass = AddPublishedClass(businessCodePublishedAssembly, "BusinessNamespace1.BusinessNamespace2.BusinessClass");
			AddPublishedMethod(businessCodePublishedClass, "BusinessMethod");
			AddStackLineCount("BusinessAssembly.dll", "BusinessNamespace1.BusinessNamespace2.BusinessClass.BusinessMethod()", 10);
			AddSourceTreeResponsibility("$/BusinessCode", "A1", "B1", "C1");

			testCaseItems.ForEach(testcase =>
			{
				var (regexCollection, expectedIssueAssignment) = testcase;
				using var disposeHandle = EDIDataRegistry.Instance.IgnoredExceptionStackLineRegexes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, regexCollection);
				var exceptionXml = GetExceptionXml(@"<Call> at SampleNamespace1.SampleNamespace2.SampleClass.SampleMethod()</Call>
<Call> at BusinessNamespace1.BusinessNamespace2.BusinessClass.BusinessMethod()</Call>");

				var log = CreateLog();
				CreateLogOccurrence(log, exceptionXml);
				Factory.Save();

				AssertLogAssignment(log, expectedIssueAssignment, null, null, false, HttpStatusCode.OK);
			});
		}

		public void TestGetAssignmentWithSystemFunction()
		{
			var report = GetExceptionXml(@"
				<Call Assembly=""System.Stuff.dll"">   at System.Somthing.Whatever()</Call>
				<Call Assembly=""MyAssembly.dll"">   at MyCode.MyClass.MyFunction()</Call>");
			AddStackLineCount("MyAssembly.dll", "MyCode.MyClass.MyFunction()", 1);
			AddPublishedAssembly("MyAssembly.dll", "$/MyCode/MyAssembly");
			AddSourceTreeResponsibility("$/MyCode", "A", "B", "C");
			AssertLogAssignment(new IssueAssignment("A", "B", "C"), report);
		}

		public void TestStackLineCountDoesNotExist()
		{
			var report = GetExceptionXml(@"
				<Call Assembly=""MyAssembly.dll"">   at MyCode.MyClass.MyFunction()</Call>");
			AddPublishedAssembly("MyAssembly.dll", "$/MyCode/MyAssembly");
			AddSourceTreeResponsibility("$/MyCode", "A", "B", "C");
			AssertLogAssignment(new IssueAssignment("A", "B", "C"), report);
		}

		public void TestAssemblyNotPopulated()
		{
			var report = GetExceptionXml(@"
				<Call>   at MyCode.MyClass.MyFunction()</Call>");
			AddStackLineCount("MyAssembly.dll", "MyCode.MyClass.MyFunction()", 1);
			AddPublishedAssembly("MyAssembly.dll", "$/MyCode/MyAssembly");
			AddSourceTreeResponsibility("$/MyCode", "A", "B", "C");
			AssertLogAssignment(new IssueAssignment("A", "B", "C"), report);
		}

		public void TestAssemblyNotPopulatedAndNotInCount()
		{
			var report = GetExceptionXml(@"
				<Call>   at MyCode.MyClass.MyFunction()</Call>");
			AddStackLineCount(string.Empty, "MyCode.MyClass.MyFunction()", 1);
			AddPublishedMethod(AddPublishedClass(AddPublishedAssembly("MyAssembly.dll", "$/MyCode/MyAssembly"), "MyCode.MyClass"), "MyFunction");
			AddSourceTreeResponsibility("$/MyCode", "A", "B", "C");
			AssertLogAssignment(new IssueAssignment("A", "B", "C"), report);
		}

		public void TestAssemblyNotPopulatedAndNoCount()
		{
			var report = GetExceptionXml(@"
				<Call>   at MyCode.MyClass.MyFunction()</Call>");
			AddPublishedMethod(AddPublishedClass(AddPublishedAssembly("MyAssembly.dll", "$/MyCode/MyAssembly"), "MyCode.MyClass"), "MyFunction");
			AddSourceTreeResponsibility("$/MyCode", "A", "B", "C");
			AssertLogAssignment(new IssueAssignment("A", "B", "C"), report);
		}

		public void TestAssemblyNotPopulatedAndNoCountMatchesCorrectClass()
		{
			var report = GetExceptionXml(@"
				<Call>   at MyCode.MyClass.MyFunction()</Call>");
			AddPublishedMethod(AddPublishedClass(AddPublishedAssembly("One.dll", "$/One"), "MyCode.MyOtherClass"), "MyFunction");
			AddPublishedMethod(AddPublishedClass(AddPublishedAssembly("Two.dll", "$/Two"), "MyCode.MyClass"), "MyFunction");
			AddSourceTreeResponsibility("$/One", "1", "1", "1");
			AddSourceTreeResponsibility("$/Two", "2", "2", "2");
			AssertLogAssignment(new IssueAssignment("2", "2", "2"), report);
		}

		public void TestAssemblyNotPopulatedAndNoCountMatchesCorrectMethod()
		{
			var report = GetExceptionXml(@"
				<Call>   at MyCode.MyClass.MyFunction()</Call>");
			AddPublishedMethod(AddPublishedClass(AddPublishedAssembly("One.dll", "$/One"), "MyCode.MyClass"), "MyFunction");
			AddPublishedMethod(AddPublishedClass(AddPublishedAssembly("Two.dll", "$/Two"), "MyCode.MyClass"), "MyOtherFunction");
			AddSourceTreeResponsibility("$/One", "1", "1", "1");
			AddSourceTreeResponsibility("$/Two", "2", "2", "2");
			AssertLogAssignment(new IssueAssignment("1", "1", "1"), report);
		}

		public void TestAssemblyNotPopulatedAmbigousAssemblyMatchesMostRecentlySeen()
		{
			var report = GetExceptionXml(@"
				<Call>   at MyCode.MyClass.MyFunction()</Call>");
			AddPublishedMethod(AddPublishedClass(AddPublishedAssembly("One.dll", "https://devops.wisetechglobal.com/wtg/MyCollection/MyProject/_git/One"), "MyCode.MyClass"), "MyFunction", DateTimeOffset.Now.AddDays(-2));
			AddPublishedMethod(AddPublishedClass(AddPublishedAssembly("Two.dll", "https://devops.wisetechglobal.com/wtg/MyCollection/MyProject/_git/Two"), "MyCode.MyClass"), "MyFunction", DateTimeOffset.Now.AddDays(-1));
			AddSourceTreeResponsibility("https://devops.wisetechglobal.com/wtg/MyCollection/MyProject/_git/One", "1", "1", "1");
			AddSourceTreeResponsibility("https://devops.wisetechglobal.com/wtg/MyCollection/MyProject/_git/Two", "2", "2", "2");
			AssertLogAssignment(new IssueAssignment("2", "2", "2"), report);
		}

		public void TestMoreSpecificSourceTreeResponsibilityUsed()
		{
			var report = GetExceptionXml(@"
				<Call Assembly=""MyAssembly.dll"">   at MyCode.MyClass.MyFunction()</Call>");
			AddStackLineCount("MyAssembly.dll", "MyCode.MyClass.MyFunction()", 1);
			AddPublishedAssembly("MyAssembly.dll", "$/Project/MyCode/MyAssembly");
			AddSourceTreeResponsibility("$/Project", "A", "A", "A");
			AddSourceTreeResponsibility("$/Project/MyCode", "B", "B", "B");
			AssertLogAssignment(new IssueAssignment("B", "B", "B"), report);
		}

		public void TestExactSourceTreePathMatched()
		{
			var report = GetExceptionXml(@"
				<Call Assembly=""MyAssembly.dll"">   at MyCode.MyClass.MyFunction()</Call>");
			AddStackLineCount("MyAssembly.dll", "MyCode.MyClass.MyFunction()", 1);
			AddPublishedAssembly("MyAssembly.dll", "$/MyCode/MyAssembly");
			AddSourceTreeResponsibility("$/MyCode", "A", "A", "A");
			AddSourceTreeResponsibility("$/MyCode/MyAssembly", "B", "B", "B");
			AssertLogAssignment(new IssueAssignment("B", "B", "B"), report);
		}

		public void TestSourceTreePartialPathNotMatched()
		{
			var report = GetExceptionXml(@"
				<Call Assembly=""MyAssembly.dll"">   at MyCode.MyClass.MyFunction()</Call>");
			AddStackLineCount("MyAssembly.dll", "MyCode.MyClass.MyFunction()", 1);
			AddPublishedAssembly("MyAssembly.dll", "$/MyCode/MyAssembly");
			AddSourceTreeResponsibility("$/MyCode/MyAss", "A", "A", "A");
			AddSourceTreeResponsibility("$/MyCode/MyAssembly", "B", "B", "B");
			AssertLogAssignment(new IssueAssignment("B", "B", "B"), report);
		}

		public void TestDevopsUrlIsMatched()
		{
			var report = GetExceptionXml(@"
				<Call Assembly=""WTG.ErrorReporting.dll"">  at WTG.ErrorReporting.ErrorReporter.Report(string report)</Call>");
			AddPublishedAssembly("WTG.ErrorReporting.dll", "https://devops.wisetechglobal.com/wtg/Shared/_git/WTG.ErrorReporting?path=/src/WTG.ErrorReporting");
			AddSourceTreeResponsibility("https://devops.wisetechglobal.com/wtg/Shared/_git/WTG.ErrorReporting", "A", "B", "C");
			AssertLogAssignment(new IssueAssignment("A", "B", "C"), report);
		}

		public void TestMoreSpecificDevopsUrlIsMatched()
		{
			var report = GetExceptionXml(@"
				<Call Assembly=""WTG.ErrorReporting.NLog.dll"">  at WTG.ErrorReporting.NLog.Logger.ReportError()</Call>");
			AddPublishedAssembly("WTG.ErrorReporting.NLog.dll", "https://devops.wisetechglobal.com/wtg/Shared/_git/WTG.ErrorReporting?path=/src/WTG.ErrorReporting.NLog");
			AddSourceTreeResponsibility("https://devops.wisetechglobal.com/wtg/Shared/_git/WTG.ErrorReporting", "A", "B", "C");
			AddSourceTreeResponsibility("https://devops.wisetechglobal.com/wtg/Shared/_git/WTG.ErrorReporting?path=/src/WTG.ErrorReporting.Nlog", "X", "Y", "Z");
			AssertLogAssignment(new IssueAssignment("X", "Y", "Z"), report);
		}

		public void TestGithubUrlIsMatched()
		{
			var report = GetExceptionXml(@"
				<Call Assembly=""CrikeyMonitor.Common.dll"">  at CrikeyMonitor.Common.CommandListener.StartAsync()</Call>");
			AddPublishedAssembly("CrikeyMonitor.Common.dll", "https://github.com/WiseTechGlobal/DevTools?path=Main/CrikeyMonitor/CrikeyMonitor.Common/");
			AddSourceTreeResponsibility("https://github.com/WiseTechGlobal/DevTools", "DAT", "DAT", "DAT");
			AssertLogAssignment(new IssueAssignment("DAT", "DAT", "DAT"), report);
		}

		public void TestMoreSpecificGithubUrlIsMatched()
		{
			var report = GetExceptionXml(@"
				<Call Assembly=""QuickGetLatest.Common.dll"">  at QuickGetLatest.Common.OutputLogger.WriteLine(string text)</Call>");
			AddPublishedAssembly("QuickGetLatest.Common.dll", "https://github.com/WiseTechGlobal/DevTools?path=Main/QuickGetLatest/QuickGetLatest");
			AddSourceTreeResponsibility("https://github.com/WiseTechGlobal/DevTools?path=Main", "IDT", "BLD", "QGL");
			AddSourceTreeResponsibility("https://github.com/WiseTechGlobal/DevTools", "DAT", "DAT", "DAT");
			AssertLogAssignment(new IssueAssignment("IDT", "BLD", "QGL"), report);
		}

		public void TestCorrectUrlMatched_WhenLongerIncorrectPartialPathExists()
		{
			var report = GetExceptionXml(@"
				<Call Assembly=""CrikeyMonitor.dll""> at MyAssembly.MyClass.MyMethod()</Call>");
			AddPublishedAssembly("CrikeyMonitor.dll", "https://github.com/WiseTechGlobal/DevTools?path=CrikeyMonitor");
			AddSourceTreeResponsibility("https://github.com/WiseTechGlobal/DevTools", "IDT", "BLD", "DAT");
			AddSourceTreeResponsibility("https://github.com/WiseTechGlobal/DevTools.Janitor", "GLW", "GLW", "BLD");
			AssertLogAssignment(new IssueAssignment("IDT", "BLD", "DAT"), report);
		}

		public void TestCorrectUrlMatched_WhenShorterIncorrectPartialPathExists()
		{
			var report = GetExceptionXml(@"
				<Call Assembly=""WTG.Janitor.Deployment.dll""> at MyAssembly.MyClass.MyMethod()</Call>");
			AddPublishedAssembly("WTG.Janitor.Deployment.dll", "https://github.com/WiseTechGlobal/DevTools.Janitor?path=src/WTG.Janitor.Deployment");
			AddSourceTreeResponsibility("https://github.com/WiseTechGlobal/DevTools", "IDT", "BLD", "DAT");
			AddSourceTreeResponsibility("https://github.com/WiseTechGlobal/DevTools.Janitor", "GLW", "GLW", "BLD");
			AssertLogAssignment(new IssueAssignment("GLW", "GLW", "BLD"), report);
		}

		public void TestSiblingWithMultipleDotSeperatorIsNotMatchedWithGithubPath_WhenNodeHasNoResponsibility()
		{
			var report = GetExceptionXml(@"
				<Call Assembly=""Modernization.Team.Admin.dll""> at MyAssembly.MyClass.MyMethod()</Call>");
			AddPublishedAssembly("Modernization.Team.Admin.dll", "https://github.com/WiseTechGlobal/Modernization.Team.Admin");
			AddSourceTreeResponsibility("https://github.com/WiseTechGlobal/Modernization.Team", "A", "A", "A");
			AddSourceTreeResponsibility("https://github.com/WiseTechGlobal/Modernization", "B", "B", "B");
			AssertLogAssignment(new IssueAssignment("B", "B", "B"), report);
		}

		public void TestSiblingWithMultipleDotSeperatorIsNotMatchedWithGithubPath_WhenNodeHasNoResponsibilityAndNoParentResponsibility()
		{
			var report = GetExceptionXml(@"
				<Call Assembly=""Modernization.Team.Admin.dll""> at MyAssembly.MyClass.MyMethod()</Call>");
			AddPublishedAssembly("Modernization.Team.Admin.dll", "https://github.com/WiseTechGlobal/Modernization.Team.Admin");
			AddSourceTreeResponsibility("https://github.com/WiseTechGlobal/Modernization.Team", "A", "A", "A");
			AssertLogAssignment(null, report, true);
		}

		public void TestSiblingWithDotSeperatorIsNotMatchedWithDevopsPath_WhenNodeHasNoResponsibility()
		{
			var report = GetExceptionXml(@"
				<Call Assembly=""BorderWise.Common.ErrorReporter.Tests.dll""> at MyAssembly.MyClass.MyMethod()</Call>");
			AddPublishedAssembly("BorderWise.Common.ErrorReporter.Tests.dll", "https://devops.wisetechglobal.com/wtg/BorderWise/_git/Common?path=/Libraries/ErrorReporter.Tests");
			AddSourceTreeResponsibility("https://devops.wisetechglobal.com/wtg/BorderWise/_git/Common?path=/Libraries/ErrorReporter", "A", "A", "A");
			AddSourceTreeResponsibility("https://devops.wisetechglobal.com/wtg/BorderWise/_git/Common", "B", "B", "B");
			AddSourceTreeResponsibility("https://devops.wisetechglobal.com/wtg/BorderWise", "C", "C", "C");
			AssertLogAssignment(new IssueAssignment("B", "B", "B"), report);
		}

		public void TestNodeWithGithubPath_MatchesSelfAsParent()
		{
			var report = GetExceptionXml(@"
				<Call Assembly = ""Dat.ImplHost.dll""> at MyAssembly.MyClass.MyMethod()</Call>");
			AddPublishedAssembly("Dat.ImplHost.dll", "https://github.com/WiseTechGlobal/DevTools?path=Main/Dat/Dat.ImplHost");
			AddSourceTreeResponsibility("https://github.com/WiseTechGlobal/DevTools", "A", "A", "A");
			AssertLogAssignment(new IssueAssignment("A", "A", "A"), report);
		}

		public void TestSiblingWithDotSeperatorIsMatchedWithGithubPath_WhenNodeHasNoResponsibility()
		{
			var report = GetExceptionXml(@"
				<Call Assembly=""Modernization.Team.dll""> at MyAssembly.MyClass.MyMethod()</Call>");
			AddPublishedAssembly("Modernization.Team.dll", "https://github.com/WiseTechGlobal/Modernization.Team");
			AddSourceTreeResponsibility("https://github.com/WiseTechGlobal/Modernization", "B", "B", "B");
			AssertLogAssignment(new IssueAssignment("B", "B", "B"), report);
		}

		public void TestSiblingWithMultipleDotSeperatorIsNotMatchedWithDevopsPath_WhenNodeHasNoResponsibility()
		{
			var report = GetExceptionXml(@"
				<Call Assembly=""WTG.Analyzers.Utils.Test.dll""> at MyAssembly.MyClass.MyMethod()</Call>");
			AddPublishedAssembly("WTG.Analyzers.Utils.Test.dll", "http://tfs.wtg.zone:8080/tfs/CargoWise/CWShared/_git/WTG.Analyzers?path=/WTG.Analyzers.Utils.Test");
			AddSourceTreeResponsibility("http://tfs.wtg.zone:8080/tfs/CargoWise/CWShared/_git/WTG.Analyzers", "A", "A", "A");
			AddSourceTreeResponsibility("http://tfs.wtg.zone:8080/tfs/CargoWise/CWShared/_git/WTG.Analyzers?path=/WTG.Analyzers.Utils", "B", "B", "B");
			AddSourceTreeResponsibility("http://tfs.wtg.zone:8080/tfs/CargoWise/CWShared/_git/WTG.Analyzers?path=/WTG.Analyzers", "C", "C", "C");
			AddSourceTreeResponsibility("http://tfs.wtg.zone:8080/tfs/CargoWise/CWShared/_git/WTG.Analyzers?path=/WTG.Analyzers.Test", "D", "D", "D");
			AssertLogAssignment(new IssueAssignment("A", "A", "A"), report);
		}

		public void TestDuplicatedStackLine()
		{
			var report = GetExceptionXml(@"
				<Call Assembly=""MyAssembly.dll"">   at MyCode.MyClass.MyFunction()</Call>
				<Call Assembly=""MyAssembly.dll"">   at MyCode.MyClass.MyFunction()</Call>");
			AddStackLineCount("MyAssembly.dll", "MyCode.MyClass.MyFunction()", 1);
			AddPublishedAssembly("MyAssembly.dll", "$/MyCode/MyAssembly");
			AddSourceTreeResponsibility("$/MyCode", "A", "B", "C");
			AssertLogAssignment(new IssueAssignment("A", "B", "C"), report);
		}

		public void TestAssignNonArchitecturePoductArea()
		{
			var report = GetExceptionXml(@"
				<Call Assembly=""Achitecture.dll"">   at Architecture.Often()</Call>
				<Call Assembly=""Product.dll"">   at Product.Meaningful()</Call>");
			AddStackLineCount("Achitecture.dll", "Architecture.Often()", 70);
			AddStackLineCount("Product.dll", "Product.Meaningful()", 3);
			AddPublishedAssembly("Achitecture.dll", "$/Architecture");
			AddPublishedAssembly("Product.dll", "$/Product");
			AddSourceTreeResponsibility("$/Architecture", "ARC", "ARC", "ARC");
			AddSourceTreeResponsibility("$/Product", "PRD", "PRD", "PRD");
			AssertLogAssignment(new IssueAssignment("PRD", "PRD", "PRD"), report);
		}

		public void TestAssignNonArchitecturePoductAreaWhenStackLineCountDoesNotExist()
		{
			var report = GetExceptionXml(@"
				<Call Assembly=""Achitecture.dll"">   at Architecture.Often()</Call>
				<Call Assembly=""Product.dll"">   at Product.Meaningful()</Call>");
			AddStackLineCount("Achitecture.dll", "Architecture.Often()", 70);
			AddPublishedAssembly("Achitecture.dll", "$/Architecture");
			AddPublishedAssembly("Product.dll", "$/Product");
			AddSourceTreeResponsibility("$/Architecture", "ARC", "ARC", "ARC");
			AddSourceTreeResponsibility("$/Product", "PRD", "PRD", "PRD");
			AssertLogAssignment(new IssueAssignment("PRD", "PRD", "PRD"), report);
		}

		public void TestAssignNonArchitecturePoductAreaWithDuplicatedStackLine()
		{
			var report = GetExceptionXml(@"
				<Call Assembly=""Achitecture.dll"">   at Architecture.Often()</Call>
				<Call Assembly=""Product.dll"">   at Product.Meaningful()</Call>
				<Call Assembly=""Achitecture.dll"">   at Architecture.Often()</Call>
				<Call Assembly=""Product.dll"">   at Product.Meaningful()</Call>");
			AddStackLineCount("Achitecture.dll", "Architecture.Often()", 70);
			AddStackLineCount("Product.dll", "Product.Meaningful()", 3);
			AddPublishedAssembly("Achitecture.dll", "$/Architecture");
			AddPublishedAssembly("Product.dll", "$/Product");
			AddSourceTreeResponsibility("$/Architecture", "ARC", "ARC", "ARC");
			AddSourceTreeResponsibility("$/Product", "PRD", "PRD", "PRD");
			AssertLogAssignment(new IssueAssignment("PRD", "PRD", "PRD"), report);
		}

		public void TestAssignBasedOnStackPosition()
		{
			var report = GetExceptionXml(@"
				<Call Assembly=""One.dll"">   at One.Top()</Call>
				<Call Assembly=""Two.dll"">   at Two.Bottom()</Call>");
			AddStackLineCount("One.dll", "One.Top()", 1);
			AddStackLineCount("Two.dll", "Two.Bottom()", 1);
			AddPublishedAssembly("One.dll", "$/One");
			AddPublishedAssembly("Two.dll", "$/Two");
			AddSourceTreeResponsibility("$/One", "1", "1", "1");
			AddSourceTreeResponsibility("$/Two", "2", "2", "2");
			AssertLogAssignment(new IssueAssignment("1", "1", "1"), report);
		}

		public void TestMostRecentExeUsed()
		{
			var report1 = GetExceptionXml(@"
				<Call Assembly=""One.dll"">   at One.Something()</Call>");

			var report2 = GetExceptionXml(@"
				<Call Assembly=""Two.dll"">   at Two.Something()</Call>");

			AddStackLineCount("One.dll", "One.Something()", 1);
			AddStackLineCount("Two.dll", "Two.Something()", 1);
			AddPublishedAssembly("One.dll", "$/One");
			AddPublishedAssembly("Two.dll", "$/Two");
			AddSourceTreeResponsibility("$/One", "1", "1", "1");
			AddSourceTreeResponsibility("$/Two", "2", "2", "2");

			var log = CreateLog();
			CreateLogOccurrence(log, report1, new ZDateTime(2017, 10, 11));
			CreateLogOccurrence(log, report2, new ZDateTime(2017, 11, 8));
			Factory.Save();
			var assignment = new StackLinesWeightsLogAutoAssigner().GetAssignment(log, new DataFormatter());
			AssertEquals(new IssueAssignment("2", "2", "2"), assignment);
		}

		public void TestNoStackLines()
		{
			var report = GetExceptionXml(string.Empty);
			AssertLogAssignment(null, report, true);
		}

		public void TestLongStackLine()
		{
			AddStackLineCount("MyAssembly.dll", FormattableString.Invariant($"{new string('x', 896)}.x(S"), 1);
			AddPublishedAssembly("MyAssembly.dll", "$/Code");
			AddSourceTreeResponsibility("$/Code", "A", "A", "A");
			AssertLogAssignment(new IssueAssignment("A", "A", "A"), GetExceptionXml(FormattableString.Invariant($@"<Call Assembly=""MyAssembly.dll"">   at {new string('x', 896)}.x(String parameter)</Call>")));
		}

		public void TestAssignNonArchitecturePoductAreaWithManyArchitectureLines()
		{
			var report = GetExceptionXml(@"
				<Call Assembly=""Achitecture.dll"">   at Architecture.Often()</Call>
				<Call Assembly=""Product.dll"">   at Product.Meaningful()</Call>
				<Call Assembly=""Achitecture.dll"">   at Architecture.Usually()</Call>
				<Call Assembly=""Achitecture.dll"">   at Architecture.Frequently()</Call>
				<Call Assembly=""Achitecture.dll"">   at Architecture.VeryOften()</Call>");
			AddStackLineCount("Achitecture.dll", "Architecture.Often()", 70);
			AddStackLineCount("Achitecture.dll", "Architecture.Usually()", 50);
			AddStackLineCount("Achitecture.dll", "Architecture.Frequently()", 30);
			AddStackLineCount("Achitecture.dll", "Architecture.VeryOften()", 80);
			AddStackLineCount("Product.dll", "Product.Meaningful()", 3);
			AddPublishedAssembly("Achitecture.dll", "$/Architecture");
			AddPublishedAssembly("Product.dll", "$/Product");
			AddSourceTreeResponsibility("$/Architecture", "ARC", "ARC", "ARC");
			AddSourceTreeResponsibility("$/Product", "PRD", "PRD", "PRD");
			AssertLogAssignment(new IssueAssignment("PRD", "PRD", "PRD"), report);
		}

		public void TestAssignNonArchitectureProductAreaOnExponentialScale()
		{
			EDIDataRegistry.Instance.StackLineCountNumberOfImportedLogs = 100000;
			var report = GetExceptionXml(@"
				<Call Assembly=""Achitecture.dll"">   at Architecture.Sometimes()</Call>
				<Call Assembly=""Product.dll"">   at Product.Meaningful()</Call>
				<Call Assembly=""Achitecture.dll"">   at Architecture.Always()</Call>
				<Call Assembly=""Achitecture.dll"">   at Architecture.Always()</Call>
				<Call Assembly=""Achitecture.dll"">   at Architecture.Always()</Call>
				<Call Assembly=""Achitecture.dll"">   at Architecture.Always()</Call>
				<Call Assembly=""Achitecture.dll"">   at Architecture.Always()</Call>
				<Call Assembly=""Achitecture.dll"">   at Architecture.Always()</Call>");
			AddStackLineCount("Achitecture.dll", "Architecture.Sometimes()", 1000);
			AddStackLineCount("Product.dll", "Product.Meaningful()", 100);
			AddStackLineCount("Achitecture.dll", "Architecture.Always()", 100000);
			AddPublishedAssembly("Achitecture.dll", "$/Architecture");
			AddPublishedAssembly("Product.dll", "$/Product");
			AddSourceTreeResponsibility("$/Architecture", "ARC", "ARC", "ARC");
			AddSourceTreeResponsibility("$/Product", "PRD", "PRD", "PRD");
			AssertLogAssignment(new IssueAssignment("PRD", "PRD", "PRD"), report);
		}

		public void TestAssignBasedOnInnerException()
		{
			var report = @"<EDI_Exception_Report>				
				<ExceptionDetails>
					<StackTrace>
						<Call Assembly=""Outer.dll"">   at Outer.Function()</Call>
					</StackTrace>
					<InnerException>
						<StackTrace>
							<Call Assembly=""Inner.dll"">   at Inner.Function()</Call>
						</StackTrace>
					</InnerException>
				</ExceptionDetails>				
			</EDI_Exception_Report>";

			AddStackLineCount("Outer.dll", "Outer.Function()", 1);
			AddStackLineCount("Inner.dll", "Inner.Function()", 1);
			AddPublishedAssembly("Outer.dll", "$/Outer");
			AddPublishedAssembly("Inner.dll", "$/Inner");
			AddSourceTreeResponsibility("$/Outer", "O", "O", "O");
			AddSourceTreeResponsibility("$/Inner", "I", "I", "I");
			AssertLogAssignment(new IssueAssignment("I", "I", "I"), report);
		}

		public void TestAssignBasedOnPositionInInnerException()
		{
			var report = @"<EDI_Exception_Report>				
				<ExceptionDetails>
					<StackTrace>
						<Call Assembly=""One.dll"">   at One.Function()</Call>
					</StackTrace>
					<InnerException>
						<StackTrace>
							<Call Assembly=""Two.dll"">   at Two.Function()</Call>
							<Call Assembly=""Three.dll"">   at Three.Function()</Call>
						</StackTrace>
					</InnerException>
				</ExceptionDetails>				
			</EDI_Exception_Report>";

			AddStackLineCount("One.dll", "One.Function()", 1);
			AddStackLineCount("Two.dll", "Two.Function()", 1);
			AddStackLineCount("Three.dll", "Three.Function()", 1);
			AddPublishedAssembly("One.dll", "$/One");
			AddPublishedAssembly("Two.dll", "$/Two");
			AddPublishedAssembly("Three.dll", "$/Three");
			AddSourceTreeResponsibility("$/One", "1", "1", "1");
			AddSourceTreeResponsibility("$/Two", "2", "2", "2");
			AddSourceTreeResponsibility("$/Three", "3", "3", "3");
			AssertLogAssignment(new IssueAssignment("2", "2", "2"), report);
		}

		public void TestAssignBasedOnInnermostException()
		{
			var report = @"<EDI_Exception_Report>				
				<ExceptionDetails>
					<StackTrace>
						<Call Assembly=""One.dll"">   at One.Function()</Call>
					</StackTrace>
					<InnerException>
						<StackTrace>
							<Call Assembly=""Two.dll"">   at Two.Function()</Call>
						</StackTrace>
						<InnerException>
							<StackTrace>
								<Call Assembly=""Three.dll"">   at Three.Function()</Call>
							</StackTrace>
						</InnerException>
					</InnerException>
				</ExceptionDetails>				
			</EDI_Exception_Report>";

			AddStackLineCount("One.dll", "One.Function()", 1);
			AddStackLineCount("Two.dll", "Two.Function()", 1);
			AddStackLineCount("Three.dll", "Three.Function()", 1);
			AddPublishedAssembly("One.dll", "$/One");
			AddPublishedAssembly("Two.dll", "$/Two");
			AddPublishedAssembly("Three.dll", "$/Three");
			AddSourceTreeResponsibility("$/One", "1", "1", "1");
			AddSourceTreeResponsibility("$/Two", "2", "2", "2");
			AddSourceTreeResponsibility("$/Three", "3", "3", "3");
			AssertLogAssignment(new IssueAssignment("3", "3", "3"), report);
		}

		#region Tests with Real Data

		public void TestRealData_ZFilterMaxLengthIssue01080102()
		{
			var report = GetExceptionXml(@"
			<Call>----- Exception caught and reported here -----</Call>
			<Call>   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)</Call>
			<Call>   at System.Environment.get_StackTrace()</Call>
			<Call Assembly=""CargoWise.Common.dll"" Type=""CargoWise.Common.ErrorReporter"" Method=""ReportOnce"" ILOffset=""318"" Parameters=""System.String;System.String;System.Exception"">   at CargoWise.Common.ErrorReporter.ReportOnce(String key, String message, Exception exception)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.ZSqlParameter"" Method="".ctor"" ILOffset=""382"" Parameters=""System.String;System.Object;CargoWise.Schema.SchemaColumn;CargoWise.EntityFramework.SQLComparisonOperator;CargoWise.EntityFramework.ComparisonOptions;System.Boolean"">   at CargoWise.EntityFramework.ZSqlParameter..ctor(String parameterName, Object value, SchemaColumn schemaColumn, SQLComparisonOperator comparisonOperator, ComparisonOptions options, Boolean isTableValued)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.ZQuery"" Method=""AddParameter"" ILOffset=""646"" Parameters=""System.String;System.Object;CargoWise.Schema.SchemaColumn;CargoWise.EntityFramework.SQLComparisonOperator;CargoWise.EntityFramework.JoinCondition;CargoWise.EntityFramework.ComparisonOptions"">   at CargoWise.EntityFramework.ZQuery.AddParameter(String ParameterName, Object Value, SchemaColumn schemaColumn, SQLComparisonOperator ComparisonOperator, JoinCondition JoinOperator, ComparisonOptions Options)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.ZQuery"" Method=""AddParameter"" ILOffset=""18"" Parameters=""System.Object;CargoWise.Schema.SchemaColumn;CargoWise.EntityFramework.SQLComparisonOperator;CargoWise.EntityFramework.JoinCondition;CargoWise.EntityFramework.ComparisonOptions"">   at CargoWise.EntityFramework.ZQuery.AddParameter(Object Value, SchemaColumn schemaColumn, SQLComparisonOperator ComparisonOperator, JoinCondition Condition, ComparisonOptions Options)</Call>
			<Call Assembly=""Enterprise.Customs.Module.dll"" Type=""Enterprise.Customs.Module.JobDeclarationFilterBusinessObject"" Method=""GetDeclarationReferenceQuery"" ILOffset=""528"" Parameters=""CargoWise.EntityFramework.SQLComparisonOperator;CargoWise.Types.ZString"">   at Enterprise.Customs.Module.JobDeclarationFilterBusinessObject.GetDeclarationReferenceQuery(SQLComparisonOperator comparisonOperator, ZString value)</Call>
			<Call>   at System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor)</Call>
			<Call Assembly=""mscorlib.dll"" Type=""System.Reflection.RuntimeMethodInfo"" Method=""UnsafeInvokeInternal"" ILOffset=""22"" Parameters=""System.Object;System.Object[];System.Object[]"">   at System.Reflection.RuntimeMethodInfo.UnsafeInvokeInternal(Object obj, Object[] parameters, Object[] arguments)</Call>
			<Call Assembly=""mscorlib.dll"" Type=""System.Delegate"" Method=""DynamicInvokeImpl"" ILOffset=""36"" Parameters=""System.Object[]"">   at System.Delegate.DynamicInvokeImpl(Object[] args)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.Business.ModuleFilter"" Method=""RunQueryDelegate"" ILOffset=""0"" Parameters=""NoParameters"">   at Enterprise.ZArchitecture.Business.ModuleFilter.RunQueryDelegate()</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.Business.ModuleFilter"" Method=""GetQuery"" ILOffset=""67"" Parameters=""NoParameters"">   at Enterprise.ZArchitecture.Business.ModuleFilter.GetQuery()</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.Business.ModuleFilterCombiner"" Method=""ExtractAndGroupFilterSetsByOuterGroup"" ILOffset=""18"" Parameters=""System.Collections.Generic.IEnumerable`1[Enterprise.ZArchitecture.Business.ModuleFilter]"">   at Enterprise.ZArchitecture.Business.ModuleFilterCombiner.ExtractAndGroupFilterSetsByOuterGroup(IEnumerable`1 activeModuleFiltersForQuery)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.Business.ModuleFilterCombiner"" Method=""GetCombinedFilter"" ILOffset=""6"" Parameters=""System.Collections.Generic.IEnumerable`1[Enterprise.ZArchitecture.Business.ModuleFilter]"">   at Enterprise.ZArchitecture.Business.ModuleFilterCombiner.GetCombinedFilter(IEnumerable`1 activeModuleFiltersForQuery)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.Business.FilterStripBusinessObject"" Method=""get_Filter"" ILOffset=""0"" Parameters=""NoParameters"">   at Enterprise.ZArchitecture.Business.FilterStripBusinessObject.get_Filter()</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.Modules.ZFilterModule"" Method=""get_FilterWithMaximumRows"" ILOffset=""0"" Parameters=""NoParameters"">   at Enterprise.ZArchitecture.Modules.ZFilterModule.get_FilterWithMaximumRows()</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.Modules.ZFilterModule"" Method=""GetDisplayResultsQuery_ForLegacyGridCollection"" ILOffset=""0"" Parameters=""CargoWise.EntityFramework.BusinessObjectCollection"">   at Enterprise.ZArchitecture.Modules.ZFilterModule.GetDisplayResultsQuery_ForLegacyGridCollection(BusinessObjectCollection collection)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.Modules.ZFilterModule"" Method=""GetDisplayResultsQuery"" ILOffset=""18"" Parameters=""NoParameters"">   at Enterprise.ZArchitecture.Modules.ZFilterModule.GetDisplayResultsQuery()</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.Modules.ZFilterGridModule"" Method=""GetDisplayResultsQuery"" ILOffset=""0"" Parameters=""NoParameters"">   at Enterprise.ZArchitecture.Modules.ZFilterGridModule.GetDisplayResultsQuery()</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.Modules.ZFilterModule"" Method=""DoLoadCollection"" ILOffset=""0"" Parameters=""NoParameters"">   at Enterprise.ZArchitecture.Modules.ZFilterModule.DoLoadCollection()</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.Modules.ZFilterModule"" Method=""RunLimitedLoader"" ILOffset=""34"" Parameters=""NoParameters"">   at Enterprise.ZArchitecture.Modules.ZFilterModule.RunLimitedLoader()</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.Modules.ZFilterModule"" Method=""PerformSearch"" ILOffset=""108"" Parameters=""NoParameters"">   at Enterprise.ZArchitecture.Modules.ZFilterModule.PerformSearch()</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.Modules.ZFilterGridModule"" Method=""PerformSearch"" ILOffset=""6"" Parameters=""NoParameters"">   at Enterprise.ZArchitecture.Modules.ZFilterGridModule.PerformSearch()</Call>
			<Call Assembly=""Enterprise.Customs.Module.dll"" Type=""Enterprise.Customs.Module.JobDeclarationModule"" Method=""PerformSearch"" ILOffset=""72"" Parameters=""NoParameters"">   at Enterprise.Customs.Module.JobDeclarationModule.PerformSearch()</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZFilterStripBaseControl"" Method=""FirePerformSearch"" ILOffset=""180"" Parameters=""System.Boolean;System.Boolean"">   at Enterprise.ZArchitecture.GUI.ZFilterStripBaseControl.FirePerformSearch(Boolean showError, Boolean isManualSearch)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZFilterStripBaseControl"" Method=""Find"" ILOffset=""0"" Parameters=""System.Boolean"">   at Enterprise.ZArchitecture.GUI.ZFilterStripBaseControl.Find(Boolean isManualSearch)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZFilterStripBaseControl"" Method=""ProcessDialogKey"" ILOffset=""79"" Parameters=""System.Windows.Forms.Keys"">   at Enterprise.ZArchitecture.GUI.ZFilterStripBaseControl.ProcessDialogKey(Keys keyData)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.ContainerControl"" Method=""ProcessDialogKey"" ILOffset=""92"" Parameters=""System.Windows.Forms.Keys"">   at System.Windows.Forms.ContainerControl.ProcessDialogKey(Keys keyData)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.TextBoxBase"" Method=""ProcessDialogKey"" ILOffset=""39"" Parameters=""System.Windows.Forms.Keys"">   at System.Windows.Forms.TextBoxBase.ProcessDialogKey(Keys keyData)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Control"" Method=""PreProcessMessage"" ILOffset=""117"" Parameters=""System.Windows.Forms.Message&amp;"">   at System.Windows.Forms.Control.PreProcessMessage(Message&amp; msg)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Control"" Method=""PreProcessControlMessageInternal"" ILOffset=""140"" Parameters=""System.Windows.Forms.Control;System.Windows.Forms.Message&amp;"">   at System.Windows.Forms.Control.PreProcessControlMessageInternal(Control target, Message&amp; msg)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Application.ThreadContext"" Method=""PreTranslateMessage"" ILOffset=""182"" Parameters=""System.Windows.Forms.NativeMethods.MSG&amp;"">   at System.Windows.Forms.Application.ThreadContext.PreTranslateMessage(MSG&amp; msg)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Application.ComponentManager"" Method=""System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.FPushMessageLoop"" ILOffset=""349"" Parameters=""System.IntPtr;System.Int32;System.Int32"">   at System.Windows.Forms.Application.ComponentManager.System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.FPushMessageLoop(IntPtr dwComponentID, Int32 reason, Int32 pvLoopData)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Application.ThreadContext"" Method=""RunMessageLoopInner"" ILOffset=""484"" Parameters=""System.Int32;System.Windows.Forms.ApplicationContext"">   at System.Windows.Forms.Application.ThreadContext.RunMessageLoopInner(Int32 reason, ApplicationContext context)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Application.ThreadContext"" Method=""RunMessageLoop"" ILOffset=""20"" Parameters=""System.Int32;System.Windows.Forms.ApplicationContext"">   at System.Windows.Forms.Application.ThreadContext.RunMessageLoop(Int32 reason, ApplicationContext context)</Call>
			<Call Assembly=""CargoWise.WindowsDesktop.exe"" Type=""Enterprise.Startup.ApplicationStartupDirector"" Method=""StartEnterprise"" ILOffset=""49"" Parameters=""System.String[]"">   at Enterprise.Startup.ApplicationStartupDirector.StartEnterprise(String[] args)</Call>
			<Call Assembly=""CargoWise.WindowsDesktop.exe"" Type=""Enterprise.Startup.ApplicationStartupDirector"" Method=""Main"" ILOffset=""36"" Parameters=""System.String[]"">   at Enterprise.Startup.ApplicationStartupDirector.Main(String[] args)</Call>");

			AddPublishedAssembly("CargoWise.Common.dll", "$/Dev/Common/Architecture/Common");
			AddPublishedAssembly("CargoWise.EntityFramework.dll", "$/Dev/Common/Architecture/EntityFramework");
			AddPublishedAssembly("Enterprise.Customs.Module.dll", "$/Dev/Enterprise/Product/Operations/Customs/Shared/Module");
			AddPublishedAssembly("Enterprise.ZArchitecture.GUI.dll", "$/Dev/Enterprise/Architecture/GUI");
			AddPublishedAssembly("CargoWise.WindowsDesktop.exe", "$/Dev/Enterprise/Product/Main/Enterprise.Main");

			AddSourceTreeResponsibility("$/Dev/Common", "ENT", "ARC", "ARC");
			AddSourceTreeResponsibility("$/Dev/Enterprise/Product/Operations/Customs/Shared", "ENT", "CUS", "BRB");
			AddSourceTreeResponsibility("$/Dev/Enterprise/Architecture", "ENT", "ARC", "ARC");
			AddSourceTreeResponsibility("$/Dev/Enterprise/Product/Main", "ENT", "ARC", "COR");

			EDIDataRegistry.Instance.StackLineCountNumberOfImportedLogs = 208477;

			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.Business.ModuleFilter.GetQuery()", 1487);
			AddStackLineCount("Enterprise.Customs.Module.dll", "Enterprise.Customs.Module.JobDeclarationFilterBusinessObject.GetDeclarationReferenceQuery(SQLComparisonOperator comparisonOperator, ZString value)", 11);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.Modules.ZFilterModule.get_FilterWithMaximumRows()", 1131);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.Business.ModuleFilterCombiner.ExtractAndGroupFilterSetsByOuterGroup(IEnumerable`1 activeModuleFiltersForQuery)", 864);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Application.ThreadContext.PreTranslateMessage(MSG& msg)", 11327);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.Business.ModuleFilterCombiner.GetCombinedFilter(IEnumerable`1 activeModuleFiltersForQuery)", 1487);
			AddStackLineCount("mscorlib.dll", "System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)", 136622);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.Business.ModuleFilter.RunQueryDelegate()", 1202);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.Modules.ZFilterModule.RunLimitedLoader()", 1902);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZFilterStripBaseControl.ProcessDialogKey(Keys keyData)", 532);
			AddStackLineCount("CargoWise.WindowsDesktop.exe", "Enterprise.Startup.ApplicationStartupDirector.Main(String[] args)", 80618);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.ZQuery.AddParameter(Object Value, SchemaColumn schemaColumn, SQLComparisonOperator ComparisonOperator, JoinCondition Condition, ComparisonOptions Options)", 1469);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.Modules.ZFilterGridModule.GetDisplayResultsQuery()", 1142);
			AddStackLineCount("mscorlib.dll", "System.Environment.get_StackTrace()", 136470);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Control.PreProcessControlMessageInternal(Control target, Message& msg)", 9398);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.TextBoxBase.ProcessDialogKey(Keys keyData)", 1359);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.Modules.ZFilterModule.DoLoadCollection()", 1674);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.Modules.ZFilterModule.GetDisplayResultsQuery()", 1130);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Control.PreProcessMessage(Message& msg)", 9396);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZFilterStripBaseControl.FirePerformSearch(Boolean showError, Boolean isManualSearch)", 1998);
			AddStackLineCount("mscorlib.dll", "System.Reflection.RuntimeMethodInfo.UnsafeInvokeInternal(Object obj, Object[] parameters, Object[] arguments)", 14380);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.ContainerControl.ProcessDialogKey(Keys keyData)", 2103);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.Business.FilterStripBusinessObject.get_Filter()", 1209);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Application.ThreadContext.RunMessageLoopInner(Int32 reason, ApplicationContext context)", 102482);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Application.ThreadContext.RunMessageLoop(Int32 reason, ApplicationContext context)", 102480);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.ZSqlParameter..ctor(String parameterName, Object value, SchemaColumn schemaColumn, SQLComparisonOperator comparisonOperator, ComparisonOptions options, Boolean isTableValued)", 2570);
			AddStackLineCount("CargoWise.Common.dll", "CargoWise.Common.ErrorReporter.ReportOnce(String key, String message, Exception exception)", 66030);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.Modules.ZFilterModule.GetDisplayResultsQuery_ForLegacyGridCollection(BusinessObjectCollection collection)", 878);
			AddStackLineCount("", "System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor)", 15762);
			AddStackLineCount("Enterprise.Customs.Module.dll", "Enterprise.Customs.Module.JobDeclarationModule.PerformSearch()", 191);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZFilterStripBaseControl.Find(Boolean isManualSearch)", 1998);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.Modules.ZFilterModule.PerformSearch()", 2588);
			AddStackLineCount("CargoWise.WindowsDesktop.exe", "Enterprise.Startup.ApplicationStartupDirector.StartEnterprise(String[] args)", 80620);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.Modules.ZFilterGridModule.PerformSearch()", 2593);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.ZQuery.AddParameter(String ParameterName, Object Value, SchemaColumn schemaColumn, SQLComparisonOperator ComparisonOperator, JoinCondition JoinOperator, ComparisonOptions Options)", 1685);
			AddStackLineCount("mscorlib.dll", "System.Delegate.DynamicInvokeImpl(Object[] args)", 5329);

			AssertLogAssignment(new IssueAssignment("ENT", "CUS", "BRB"), report);
		}

		public void TestRealData_AccessingPropertyOnDeletedJobShipment00980850()
		{
			var report = GetExceptionXml(@"
			<Call Assembly=""System.Data.dll"" Type=""System.Data.DataRow"" Method=""GetDefaultRecord"" ILOffset=""47"" Parameters=""NoParameters"">   at System.Data.DataRow.GetDefaultRecord()</Call>
			<Call Assembly=""System.Data.dll"" Type=""System.Data.DataRow"" Method=""get_Item"" ILOffset=""0"" Parameters=""System.Data.DataColumn;System.Data.DataRowVersion"">   at System.Data.DataRow.get_Item(DataColumn column, DataRowVersion version)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObject"" Method=""GetValueFromRowSafely"" ILOffset=""29"" Parameters=""System.Data.DataColumn;System.Data.DataRowVersion"">   at CargoWise.EntityFramework.BusinessObject.GetValueFromRowSafely(DataColumn column, DataRowVersion version)</Call>
			<Call>----- Exception caught and reported here -----</Call>
			<Call>   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)</Call>
			<Call>   at System.Environment.get_StackTrace()</Call>
			<Call Assembly=""CargoWise.Common.dll"" Type=""CargoWise.Common.ErrorReporter"" Method=""ReportOnce"" ILOffset=""318"" Parameters=""System.String;System.String;System.Exception"">   at CargoWise.Common.ErrorReporter.ReportOnce(String key, String message, Exception exception)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObject"" Method=""ReportRowError"" ILOffset=""372"" Parameters=""System.String;System.Exception;System.Data.DataRowVersion;System.Data.DataRowVersion;System.String"">   at CargoWise.EntityFramework.BusinessObject.ReportRowError(String columnName, Exception ex, DataRowVersion versionToUse, DataRowVersion version, String message)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObject"" Method=""ReportRowDeletedError"" ILOffset=""16"" Parameters=""System.String;System.Exception;System.Data.DataRowVersion;System.Data.DataRowVersion"">   at CargoWise.EntityFramework.BusinessObject.ReportRowDeletedError(String columnName, Exception ex, DataRowVersion versionToUse, DataRowVersion version)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObject"" Method=""GetValueFromRowSafely"" ILOffset=""29"" Parameters=""System.Data.DataColumn;System.Data.DataRowVersion"">   at CargoWise.EntityFramework.BusinessObject.GetValueFromRowSafely(DataColumn column, DataRowVersion version)</Call>
			<Call Assembly=""Enterprise.Freight.Common.Business.dll"" Type=""Enterprise.Freight.Common.Business.AutoJobShipment"" Method=""get_JS_TransportMode"" ILOffset=""0"" Parameters=""NoParameters"">   at Enterprise.Freight.Common.Business.AutoJobShipment.get_JS_TransportMode()</Call>
			<Call Assembly=""Enterprise.Freight.Forwarding.Business.dll"" Type=""Enterprise.Freight.Forwarding.Business.ForwardingShipment"" Method=""get_JS_TransportMode"" ILOffset=""0"" Parameters=""NoParameters"">   at Enterprise.Freight.Forwarding.Business.ForwardingShipment.get_JS_TransportMode()</Call>
			<Call Assembly=""Enterprise.Freight.dll"" Type=""Enterprise.Freight.Business.CommonShipmentDocumentSupporter"" Method=""GetFilterValue"" ILOffset=""551"" Parameters=""Enterprise.DocumentEngineCore.DocumentSupport.DocumentFilters"">   at Enterprise.Freight.Business.CommonShipmentDocumentSupporter.GetFilterValue(DocumentFilters filterName)</Call>
			<Call Assembly=""Enterprise.DocumentEngine.dll"" Type=""Enterprise.DocumentEngine.DataProviders.ZExpressionEvaluator"" Method=""Evaluate"" ILOffset=""123"" Parameters=""System.String;Enterprise.DocumentEngineCore.DocumentSupport.IDocumentSupportable;Enterprise.DocumentEngineCore.DocWrappers.IBODocDataProvider[]"">   at Enterprise.DocumentEngine.DataProviders.ZExpressionEvaluator.Evaluate(String expression, IDocumentSupportable docSupportable, IBODocDataProvider[] providers)</Call>
			<Call Assembly=""Enterprise.DocumentEngine.dll"" Type=""Enterprise.DocumentEngine.DocumentCommand"" Method=""get_IsApplicable"" ILOffset=""17"" Parameters=""NoParameters"">   at Enterprise.DocumentEngine.DocumentCommand.get_IsApplicable()</Call>
			<Call Assembly=""Enterprise.DocumentEngine.dll"" Type=""Enterprise.DocumentEngine.DocumentNote"" Method=""GetListOfUDFListsFromAllDocumentCommands"" ILOffset=""66"" Parameters=""Enterprise.DocumentEngineCore.DocumentSupport.IDocumentSupportable"">   at Enterprise.DocumentEngine.DocumentNote.GetListOfUDFListsFromAllDocumentCommands(IDocumentSupportable mainBusinessObject)</Call>
			<Call Assembly=""Enterprise.DocumentEngine.dll"" Type=""Enterprise.DocumentEngine.DocumentNote"" Method=""GetNewUserDefinedFieldList"" ILOffset=""14"" Parameters=""Enterprise.ZArchitecture.Business.IStmNoteParent"">   at Enterprise.DocumentEngine.DocumentNote.GetNewUserDefinedFieldList(IStmNoteParent mainBusinessObject)</Call>
			<Call Assembly=""Enterprise.DocumentEngine.dll"" Type=""Enterprise.DocumentEngine.DocumentNote"" Method=""get_UserDefinedFieldList"" ILOffset=""0"" Parameters=""NoParameters"">   at Enterprise.DocumentEngine.DocumentNote.get_UserDefinedFieldList()</Call>
			<Call Assembly=""Enterprise.DocumentEngine.dll"" Type=""Enterprise.DocumentEngine.DocumentNote"" Method=""UpdateUDFsFromMainBusinessObjectInternal"" ILOffset=""66"" Parameters=""NoParameters"">   at Enterprise.DocumentEngine.DocumentNote.UpdateUDFsFromMainBusinessObjectInternal()</Call>
			<Call Assembly=""Enterprise.DocumentEngine.dll"" Type=""Enterprise.DocumentEngine.DocumentNote"" Method=""UpdateUDFsFromMainBusinessObjectIfFactoryContentsChangedSinceLastUpdate"" ILOffset=""60"" Parameters=""NoParameters"">   at Enterprise.DocumentEngine.DocumentNote.UpdateUDFsFromMainBusinessObjectIfFactoryContentsChangedSinceLastUpdate()</Call>
			<Call Assembly=""Enterprise.DocumentEngine.dll"" Type=""Enterprise.DocumentEngine.DocumentNote"" Method=""OnFactorySaving"" ILOffset=""28"" Parameters=""NoParameters"">   at Enterprise.DocumentEngine.DocumentNote.OnFactorySaving()</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObject"" Method=""OnFactorySavingInternal"" ILOffset=""28"" Parameters=""NoParameters"">   at CargoWise.EntityFramework.BusinessObject.OnFactorySavingInternal()</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObjectFactory.BOMethodCaller"" Method=""CallMethodOnAllBusinessObjects"" ILOffset=""102"" Parameters=""CargoWise.EntityFramework.BusinessObjectFactory"">   at CargoWise.EntityFramework.BusinessObjectFactory.BOMethodCaller.CallMethodOnAllBusinessObjects(BusinessObjectFactory factory)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObjectFactory"" Method=""SaveInTransactionCore"" ILOffset=""69"" Parameters=""NoParameters"">   at CargoWise.EntityFramework.BusinessObjectFactory.SaveInTransactionCore()</Call>
			<Call Assembly=""System.Core.dll"" Type=""System.Linq.Enumerable.WhereSelectArrayIterator`2[TSource,TResult]"" Method=""MoveNext"" ILOffset=""65"" Parameters=""NoParameters"">   at System.Linq.Enumerable.WhereSelectArrayIterator`2[TSource,TResult].MoveNext()</Call>
			<Call Assembly=""System.Core.dll"" Type=""System.Linq.Buffer`1[TElement]"" Method="".ctor"" ILOffset=""114"" Parameters=""System.Collections.Generic.IEnumerable`1[TElement]"">   at System.Linq.Buffer`1[TElement]..ctor(IEnumerable`1 source)</Call>
			<Call Assembly=""System.Core.dll"" Type=""System.Linq.Enumerable"" Method=""ToArray"" ILOffset=""20"" Parameters=""System.Collections.Generic.IEnumerable`1[TSource]"">   at System.Linq.Enumerable.ToArray(IEnumerable`1 source)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.TransactionCoordinator"" Method=""SaveInTransactions"" ILOffset=""42"" Parameters=""NoParameters"">   at CargoWise.EntityFramework.TransactionCoordinator.SaveInTransactions()</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.RowFactory"" Method=""SaveTogether"" ILOffset=""0"" Parameters=""CargoWise.EntityFramework.TransactionCoordinator"">   at CargoWise.EntityFramework.RowFactory.SaveTogether(TransactionCoordinator coordinator)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObjectFactory"" Method=""SaveTogether"" ILOffset=""29"" Parameters=""CargoWise.Integration.ITransactionParticipant[]"">   at CargoWise.EntityFramework.BusinessObjectFactory.SaveTogether(ITransactionParticipant[] Factories)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZForm"" Method=""SaveInternal"" ILOffset=""156"" Parameters=""NoParameters"">   at Enterprise.ZArchitecture.GUI.ZForm.SaveInternal()</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZForm"" Method=""PerformSave"" ILOffset=""28"" Parameters=""NoParameters"">   at Enterprise.ZArchitecture.GUI.ZForm.PerformSave()</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZForm"" Method=""ValidateAndSave"" ILOffset=""265"" Parameters=""NoParameters"">   at Enterprise.ZArchitecture.GUI.ZForm.ValidateAndSave()</Call>
			<Call Assembly=""Enterprise.Freight.Forwarding.GUI.dll"" Type=""Enterprise.Freight.Forwarding.GUI.ConsolForm"" Method=""ValidateAndSave"" ILOffset=""17"" Parameters=""NoParameters"">   at Enterprise.Freight.Forwarding.GUI.ConsolForm.ValidateAndSave()</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZForm"" Method=""FireSaveButtonIndirectly"" ILOffset=""2"" Parameters=""System.Boolean"">   at Enterprise.ZArchitecture.GUI.ZForm.FireSaveButtonIndirectly(Boolean closeOnSave)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZForm"" Method=""HandleApplyPostingButtonClickUnsafe"" ILOffset=""293"" Parameters=""System.Boolean"">   at Enterprise.ZArchitecture.GUI.ZForm.HandleApplyPostingButtonClickUnsafe(Boolean closeOnSave)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZForm"" Method=""HandleApplyPostingButtonClick"" ILOffset=""219"" Parameters=""System.Boolean;System.Boolean"">   at Enterprise.ZArchitecture.GUI.ZForm.HandleApplyPostingButtonClick(Boolean closeOnSave, Boolean saveOnlyMode)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZForm"" Method=""SaveFormProper"" ILOffset=""53"" Parameters=""System.Boolean;System.Object;System.Boolean"">   at Enterprise.ZArchitecture.GUI.ZForm.SaveFormProper(Boolean closeOnSave, Object sender, Boolean saveOnlyMode)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.ToolStripItem"" Method=""RaiseEvent"" ILOffset=""21"" Parameters=""System.Object;System.EventArgs"">   at System.Windows.Forms.ToolStripItem.RaiseEvent(Object key, EventArgs e)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.ToolStripButton"" Method=""OnClick"" ILOffset=""30"" Parameters=""System.EventArgs"">   at System.Windows.Forms.ToolStripButton.OnClick(EventArgs e)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZToolStripButton"" Method=""OnClick"" ILOffset=""119"" Parameters=""System.EventArgs"">   at Enterprise.ZArchitecture.GUI.ZToolStripButton.OnClick(EventArgs e)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.ToolStripItem"" Method=""HandleClick"" ILOffset=""66"" Parameters=""System.EventArgs"">   at System.Windows.Forms.ToolStripItem.HandleClick(EventArgs e)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.ToolStripItem"" Method=""PerformClick"" ILOffset=""23"" Parameters=""NoParameters"">   at System.Windows.Forms.ToolStripItem.PerformClick()</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZFormMenuStrategy"" Method=""RaiseButtonClick"" ILOffset=""139"" Parameters=""System.Windows.Forms.Form;Enterprise.ZArchitecture.GUI.IButton;System.String"">   at Enterprise.ZArchitecture.GUI.ZFormMenuStrategy.RaiseButtonClick(Form form, IButton button, String fieldName)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZFormMenuStrategy.&lt;&gt;c__DisplayClass21_0"" Method=""&lt;InitialiseMainMenu&gt;b__1"" ILOffset=""87"" Parameters=""System.Object;System.EventArgs"">   at Enterprise.ZArchitecture.GUI.ZFormMenuStrategy.&lt;&gt;c__DisplayClass21_0.&lt;InitialiseMainMenu&gt;b__1(Object &lt;p0&gt;, EventArgs &lt;p1&gt;)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.MenuItem"" Method=""OnClick"" ILOffset=""104"" Parameters=""System.EventArgs"">   at System.Windows.Forms.MenuItem.OnClick(EventArgs e)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZMenuItem"" Method=""OnClick"" ILOffset=""152"" Parameters=""System.EventArgs"">   at Enterprise.ZArchitecture.GUI.ZMenuItem.OnClick(EventArgs e)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.MenuItem"" Method=""ShortcutClick"" ILOffset=""94"" Parameters=""NoParameters"">   at System.Windows.Forms.MenuItem.ShortcutClick()</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZMainMenu"" Method=""ProcessCmdKey"" ILOffset=""32"" Parameters=""System.Windows.Forms.Message&amp;;System.Windows.Forms.Keys"">   at Enterprise.ZArchitecture.GUI.ZMainMenu.ProcessCmdKey(Message&amp; msg, Keys keyData)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Form"" Method=""ProcessCmdKey"" ILOffset=""37"" Parameters=""System.Windows.Forms.Message&amp;;System.Windows.Forms.Keys"">   at System.Windows.Forms.Form.ProcessCmdKey(Message&amp; msg, Keys keyData)</Call>
			<Call Assembly=""CargoWise.Windows.UI.dll"" Type=""CargoWise.Windows.UI.KForm"" Method=""ProcessCmdKey"" ILOffset=""0"" Parameters=""System.Windows.Forms.Message&amp;;System.Windows.Forms.Keys"">   at CargoWise.Windows.UI.KForm.ProcessCmdKey(Message&amp; msg, Keys keyData)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZForm"" Method=""ProcessCmdKey"" ILOffset=""108"" Parameters=""System.Windows.Forms.Message&amp;;System.Windows.Forms.Keys"">   at Enterprise.ZArchitecture.GUI.ZForm.ProcessCmdKey(Message&amp; msg, Keys keyData)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Control"" Method=""ProcessCmdKey"" ILOffset=""46"" Parameters=""System.Windows.Forms.Message&amp;;System.Windows.Forms.Keys"">   at System.Windows.Forms.Control.ProcessCmdKey(Message&amp; msg, Keys keyData)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Control"" Method=""ProcessCmdKey"" ILOffset=""46"" Parameters=""System.Windows.Forms.Message&amp;;System.Windows.Forms.Keys"">   at System.Windows.Forms.Control.ProcessCmdKey(Message&amp; msg, Keys keyData)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Control"" Method=""ProcessCmdKey"" ILOffset=""46"" Parameters=""System.Windows.Forms.Message&amp;;System.Windows.Forms.Keys"">   at System.Windows.Forms.Control.ProcessCmdKey(Message&amp; msg, Keys keyData)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Control"" Method=""ProcessCmdKey"" ILOffset=""46"" Parameters=""System.Windows.Forms.Message&amp;;System.Windows.Forms.Keys"">   at System.Windows.Forms.Control.ProcessCmdKey(Message&amp; msg, Keys keyData)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.ContainerControl"" Method=""ProcessCmdKey"" ILOffset=""0"" Parameters=""System.Windows.Forms.Message&amp;;System.Windows.Forms.Keys"">   at System.Windows.Forms.ContainerControl.ProcessCmdKey(Message&amp; msg, Keys keyData)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZUserControl"" Method=""ProcessCmdKey"" ILOffset=""8"" Parameters=""System.Windows.Forms.Message&amp;;System.Windows.Forms.Keys"">   at Enterprise.ZArchitecture.GUI.ZUserControl.ProcessCmdKey(Message&amp; msg, Keys keyData)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Control"" Method=""ProcessCmdKey"" ILOffset=""46"" Parameters=""System.Windows.Forms.Message&amp;;System.Windows.Forms.Keys"">   at System.Windows.Forms.Control.ProcessCmdKey(Message&amp; msg, Keys keyData)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.ContainerControl"" Method=""ProcessCmdKey"" ILOffset=""0"" Parameters=""System.Windows.Forms.Message&amp;;System.Windows.Forms.Keys"">   at System.Windows.Forms.ContainerControl.ProcessCmdKey(Message&amp; msg, Keys keyData)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZUserControl"" Method=""ProcessCmdKey"" ILOffset=""8"" Parameters=""System.Windows.Forms.Message&amp;;System.Windows.Forms.Keys"">   at Enterprise.ZArchitecture.GUI.ZUserControl.ProcessCmdKey(Message&amp; msg, Keys keyData)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Control"" Method=""ProcessCmdKey"" ILOffset=""46"" Parameters=""System.Windows.Forms.Message&amp;;System.Windows.Forms.Keys"">   at System.Windows.Forms.Control.ProcessCmdKey(Message&amp; msg, Keys keyData)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Control"" Method=""ProcessCmdKey"" ILOffset=""46"" Parameters=""System.Windows.Forms.Message&amp;;System.Windows.Forms.Keys"">   at System.Windows.Forms.Control.ProcessCmdKey(Message&amp; msg, Keys keyData)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.ZGrid"" Method=""ProcessCmdKey"" ILOffset=""111"" Parameters=""System.Windows.Forms.Message&amp;;System.Windows.Forms.Keys"">   at Enterprise.ZArchitecture.ZGrid.ProcessCmdKey(Message&amp; msg, Keys keyData)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Control"" Method=""ProcessCmdKey"" ILOffset=""46"" Parameters=""System.Windows.Forms.Message&amp;;System.Windows.Forms.Keys"">   at System.Windows.Forms.Control.ProcessCmdKey(Message&amp; msg, Keys keyData)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.TextBoxBase"" Method=""ProcessCmdKey"" ILOffset=""0"" Parameters=""System.Windows.Forms.Message&amp;;System.Windows.Forms.Keys"">   at System.Windows.Forms.TextBoxBase.ProcessCmdKey(Message&amp; msg, Keys keyData)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.TextBox"" Method=""ProcessCmdKey"" ILOffset=""0"" Parameters=""System.Windows.Forms.Message&amp;;System.Windows.Forms.Keys"">   at System.Windows.Forms.TextBox.ProcessCmdKey(Message&amp; m, Keys keyData)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Control"" Method=""PreProcessMessage"" ILOffset=""65"" Parameters=""System.Windows.Forms.Message&amp;"">   at System.Windows.Forms.Control.PreProcessMessage(Message&amp; msg)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Control"" Method=""PreProcessControlMessageInternal"" ILOffset=""140"" Parameters=""System.Windows.Forms.Control;System.Windows.Forms.Message&amp;"">   at System.Windows.Forms.Control.PreProcessControlMessageInternal(Control target, Message&amp; msg)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Application.ThreadContext"" Method=""PreTranslateMessage"" ILOffset=""182"" Parameters=""System.Windows.Forms.NativeMethods.MSG&amp;"">   at System.Windows.Forms.Application.ThreadContext.PreTranslateMessage(MSG&amp; msg)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Application.ComponentManager"" Method=""System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.FPushMessageLoop"" ILOffset=""349"" Parameters=""System.IntPtr;System.Int32;System.Int32"">   at System.Windows.Forms.Application.ComponentManager.System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.FPushMessageLoop(IntPtr dwComponentID, Int32 reason, Int32 pvLoopData)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Application.ThreadContext"" Method=""RunMessageLoopInner"" ILOffset=""484"" Parameters=""System.Int32;System.Windows.Forms.ApplicationContext"">   at System.Windows.Forms.Application.ThreadContext.RunMessageLoopInner(Int32 reason, ApplicationContext context)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Application.ThreadContext"" Method=""RunMessageLoop"" ILOffset=""20"" Parameters=""System.Int32;System.Windows.Forms.ApplicationContext"">   at System.Windows.Forms.Application.ThreadContext.RunMessageLoop(Int32 reason, ApplicationContext context)</Call>
			<Call Assembly=""CargoWise.WindowsDesktop.exe"" Type=""Enterprise.Startup.ApplicationStartupDirector"" Method=""StartEnterprise"" ILOffset=""49"" Parameters=""System.String[]"">   at Enterprise.Startup.ApplicationStartupDirector.StartEnterprise(String[] args)</Call>
			<Call Assembly=""CargoWise.WindowsDesktop.exe"" Type=""Enterprise.Startup.ApplicationStartupDirector"" Method=""Main"" ILOffset=""36"" Parameters=""System.String[]"">   at Enterprise.Startup.ApplicationStartupDirector.Main(String[] args)</Call>");

			AddPublishedAssembly("CargoWise.Common.dll", "$/Dev/Common/Architecture/Common");
			AddPublishedAssembly("CargoWise.EntityFramework.dll", "$/Dev/Common/Architecture/EntityFramework");
			AddPublishedAssembly("Enterprise.Freight.Common.Business.dll", "$/Dev/Enterprise/Product/Operations/Freight/Common/Business");
			AddPublishedAssembly("Enterprise.Freight.Forwarding.Business.dll", "$/Dev/Enterprise/Product/Operations/Freight/Forwarding/Forwarding.Business");
			AddPublishedAssembly("Enterprise.Freight.dll", "$/Dev/Enterprise/Product/Operations/Freight/Shared/Freight.Business");
			AddPublishedAssembly("Enterprise.DocumentEngine.dll", "$/Dev/Enterprise/Product/Documents/DocumentEngine");
			AddPublishedAssembly("Enterprise.Freight.Forwarding.GUI.dll", "$/Dev/Enterprise/Product/Operations/Freight/Forwarding/Forwarding.GUI");
			AddPublishedAssembly("Enterprise.ZArchitecture.GUI.dll", "$/Dev/Enterprise/Architecture/GUI");
			AddPublishedAssembly("CargoWise.WindowsDesktop.exe", "$/Dev/Enterprise/Product/Main/Enterprise.Main");

			AddSourceTreeResponsibility("$/Dev/Common", "ENT", "ARC", "ARC");
			AddSourceTreeResponsibility("$/Dev/Enterprise/Product/Operations/Freight", "ENT", "INT", "FOR");
			AddSourceTreeResponsibility("$/Dev/Enterprise/Product/Documents", "ENT", "ARC", "DOC");
			AddSourceTreeResponsibility("$/Dev/Enterprise/Architecture", "ENT", "ARC", "ARC");
			AddSourceTreeResponsibility("$/Dev/Enterprise/Product/Main", "ENT", "ARC", "COR");

			EDIDataRegistry.Instance.StackLineCountNumberOfImportedLogs = 208477;

			AddStackLineCount("Enterprise.DocumentEngine.dll", "Enterprise.DocumentEngine.DocumentNote.GetNewUserDefinedFieldList(IStmNoteParent mainBusinessObject)", 720);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Application.ComponentManager.System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.FPushMessageLoop(IntPtr dwComponentID, Int32 reason, Int32 pvLoopData)", 101654);
			AddStackLineCount("System.Core.dll", "System.Linq.Enumerable.WhereSelectArrayIterator`2[TSource,TResult].MoveNext()", 7819);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZForm.PerformSave()", 14150);
			AddStackLineCount("Enterprise.DocumentEngine.dll", "Enterprise.DocumentEngine.DocumentNote.GetListOfUDFListsFromAllDocumentCommands(IDocumentSupportable mainBusinessObject)", 704);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZForm.ValidateAndSave()", 17673);
			AddStackLineCount("Enterprise.DocumentEngine.dll", "Enterprise.DocumentEngine.DocumentCommand.get_IsApplicable()", 1504);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.ToolStripItem.PerformClick()", 4591);
			AddStackLineCount("Enterprise.Freight.Forwarding.GUI.dll", "Enterprise.Freight.Forwarding.GUI.ConsolForm.ValidateAndSave()", 1914);
			AddStackLineCount("Enterprise.Freight.Forwarding.Business.dll", "Enterprise.Freight.Forwarding.Business.ForwardingShipment.get_JS_TransportMode()", 19);
			AddStackLineCount("System.Data.dll", "System.Data.DataRow.GetDefaultRecord()", 2714);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZToolStripButton.OnClick(EventArgs e)", 17714);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZForm.ProcessCmdKey(Message& msg, Keys keyData)", 4974);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Application.ThreadContext.PreTranslateMessage(MSG& msg)", 11327);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Form.ProcessCmdKey(Message& msg, Keys keyData)", 4925);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObject.OnFactorySavingInternal()", 1389);
			AddStackLineCount("Enterprise.DocumentEngine.dll", "Enterprise.DocumentEngine.DocumentNote.get_UserDefinedFieldList()", 680);
			AddStackLineCount("System.Core.dll", "System.Linq.Enumerable.ToArray(IEnumerable`1 source)", 10789);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZForm.FireSaveButtonIndirectly(Boolean closeOnSave)", 1648);
			AddStackLineCount("mscorlib.dll", "System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)", 136622);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Control.ProcessCmdKey(Message& msg, Keys keyData)", 54084);
			AddStackLineCount("System.Data.dll", "System.Data.DataRow.get_Item(DataColumn column, DataRowVersion version)", 2201);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.TransactionCoordinator.SaveInTransactions()", 15481);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZFormMenuStrategy.RaiseButtonClick(Form form, IButton button, String fieldName)", 4214);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZUserControl.ProcessCmdKey(Message& msg, Keys keyData)", 5124);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObjectFactory.BOMethodCaller.CallMethodOnAllBusinessObjects(BusinessObjectFactory factory)", 3506);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZMenuItem.OnClick(EventArgs e)", 20640);
			AddStackLineCount("Enterprise.Freight.Common.Business.dll", "Enterprise.Freight.Common.Business.AutoJobShipment.get_JS_TransportMode()", 16);
			AddStackLineCount("CargoWise.WindowsDesktop.exe", "Enterprise.Startup.ApplicationStartupDirector.Main(String[] args)", 80618);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZForm.SaveInternal()", 14486);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.TextBox.ProcessCmdKey(Message& m, Keys keyData)", 678);
			AddStackLineCount("Enterprise.Freight.dll", "Enterprise.Freight.Business.CommonShipmentDocumentSupporter.GetFilterValue(DocumentFilters filterName)", 17);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObject.ReportRowDeletedError(String columnName, Exception ex, DataRowVersion versionToUse, DataRowVersion version)", 1631);
			AddStackLineCount("CargoWise.Windows.UI.dll", "CargoWise.Windows.UI.KForm.ProcessCmdKey(Message& msg, Keys keyData)", 2524);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObjectFactory.SaveTogether(ITransactionParticipant[] Factories)", 37582);
			AddStackLineCount("mscorlib.dll", "System.Environment.get_StackTrace()", 136470);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Control.PreProcessControlMessageInternal(Control target, Message& msg)", 9398);
			AddStackLineCount("Enterprise.DocumentEngine.dll", "Enterprise.DocumentEngine.DocumentNote.OnFactorySaving()", 95);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Control.PreProcessMessage(Message& msg)", 9396);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZForm.SaveFormProper(Boolean closeOnSave, Object sender, Boolean saveOnlyMode)", 1886);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZFormMenuStrategy.<>c__DisplayClass21_0.<InitialiseMainMenu>b__1(Object <p0>, EventArgs <p1>)", 394);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.RowFactory.SaveTogether(TransactionCoordinator coordinator)", 37776);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObject.ReportRowError(String columnName, Exception ex, DataRowVersion versionToUse, DataRowVersion version, String message)", 2773);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZForm.HandleApplyPostingButtonClickUnsafe(Boolean closeOnSave)", 1484);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.MenuItem.OnClick(EventArgs e)", 19783);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.ZGrid.ProcessCmdKey(Message& msg, Keys keyData)", 3822);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObjectFactory.SaveInTransactionCore()", 25725);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZMainMenu.ProcessCmdKey(Message& msg, Keys keyData)", 4916);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.ToolStripButton.OnClick(EventArgs e)", 16151);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Application.ThreadContext.RunMessageLoopInner(Int32 reason, ApplicationContext context)", 102482);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.ContainerControl.ProcessCmdKey(Message& msg, Keys keyData)", 18232);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Application.ThreadContext.RunMessageLoop(Int32 reason, ApplicationContext context)", 102480);
			AddStackLineCount("Enterprise.DocumentEngine.dll", "Enterprise.DocumentEngine.DocumentNote.UpdateUDFsFromMainBusinessObjectIfFactoryContentsChangedSinceLastUpdate()", 69);
			AddStackLineCount("CargoWise.Common.dll", "CargoWise.Common.ErrorReporter.ReportOnce(String key, String message, Exception exception)", 66030);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZForm.HandleApplyPostingButtonClick(Boolean closeOnSave, Boolean saveOnlyMode)", 3194);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.MenuItem.ShortcutClick()", 4949);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.ToolStripItem.HandleClick(EventArgs e)", 20403);
			AddStackLineCount("System.Core.dll", "System.Linq.Buffer`1[TElement]..ctor(IEnumerable`1 source)", 11235);
			AddStackLineCount("CargoWise.WindowsDesktop.exe", "Enterprise.Startup.ApplicationStartupDirector.StartEnterprise(String[] args)", 80620);
			AddStackLineCount("Enterprise.DocumentEngine.dll", "Enterprise.DocumentEngine.DocumentNote.UpdateUDFsFromMainBusinessObjectInternal()", 98);
			AddStackLineCount("Enterprise.DocumentEngine.dll", "Enterprise.DocumentEngine.DataProviders.ZExpressionEvaluator.Evaluate(String expression, IDocumentSupportable docSupportable, IBODocDataProvider[] providers)", 1497);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.TextBoxBase.ProcessCmdKey(Message& msg, Keys keyData)", 4302);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObject.GetValueFromRowSafely(DataColumn Column, DataRowVersion Version)", 6251);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.ToolStripItem.RaiseEvent(Object key, EventArgs e)", 18597);

			AssertLogAssignment(new IssueAssignment("ENT", "INT", "FOR"), report);
		}

		public void TestRealData_CustomsSendMessageWithBondedWarehouseAutomationError01079725()
		{
			var report = GetExceptionXml(@"
			<Call>----- Exception caught and reported here -----</Call>
			<Call>   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)</Call>
			<Call>   at System.Environment.get_StackTrace()</Call>
			<Call Assembly=""CargoWise.Common.dll"" Type=""CargoWise.Common.ErrorReporter"" Method=""ReportOnce"" ILOffset=""318"" Parameters=""System.String;System.String;System.Exception"">   at CargoWise.Common.ErrorReporter.ReportOnce(String key, String message, Exception exception)</Call>
			<Call Assembly=""Enterprise.Customs.Business.dll"" Type=""Enterprise.Customs.Business.BaseJobDeclaration"" Method=""SendMessageWithBondedWarehouseAutomation"" ILOffset=""41"" Parameters=""Enterprise.Customs.Business.WarehouseExtensions.IWarehouseIntegrationSupporter;System.Boolean;System.Boolean;System.Func`1[System.Boolean];Enterprise.Customs.Business.WarehouseExtensions.MessageAction;System.Action;System.Boolean;System.Boolean"">   at Enterprise.Customs.Business.BaseJobDeclaration.SendMessageWithBondedWarehouseAutomation(IWarehouseIntegrationSupporter supporter, Boolean isOutward, Boolean isChangeOfOwnership, Func`1 sendMessage, MessageAction action, Action saveFactory, Boolean additionalBondedWarehouseRequirement, Boolean reportHasChanges)</Call>
			<Call Assembly=""Enterprise.Customs.GUI.dll"" Type=""Enterprise.Customs.GUI.SendsMessagesToCustomsGUI"" Method=""SendAmendmentMessage"" ILOffset=""55"" Parameters=""Enterprise.Customs.Business.BaseJobDeclaration"">   at Enterprise.Customs.GUI.SendsMessagesToCustomsGUI.SendAmendmentMessage(BaseJobDeclaration declaration)</Call>
			<Call Assembly=""Enterprise.Customs.GUI.dll"" Type=""Enterprise.Customs.GUI.SendsMessagesToCustomsGUI"" Method=""SendMessageOnSaved"" ILOffset=""133"" Parameters=""CargoWise.EntityFramework.BusinessObjectFactory;System.Boolean"">   at Enterprise.Customs.GUI.SendsMessagesToCustomsGUI.SendMessageOnSaved(BusinessObjectFactory factory, Boolean SavedSuccessfully)</Call>
			<Call>   at CargoWise.EntityFramework.BusinessObjectFactory.SavedEventHandler.Invoke(BusinessObjectFactory factory, Boolean SavedSuccessfully)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObjectFactory"" Method=""OnSaved"" ILOffset=""16"" Parameters=""System.Boolean"">   at CargoWise.EntityFramework.BusinessObjectFactory.OnSaved(Boolean SavedSuccessfully)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObjectFactory"" Method=""CargoWise.Integration.ITransactionParticipant.OnAllTransactionsCommitted"" ILOffset=""201"" Parameters=""CargoWise.Integration.IChangedTableNames"">   at CargoWise.EntityFramework.BusinessObjectFactory.CargoWise.Integration.ITransactionParticipant.OnAllTransactionsCommitted(IChangedTableNames changedTableNames)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.TransactionCoordinator.TransactionManager"" Method=""CommitTransaction"" ILOffset=""36"" Parameters=""CargoWise.EntityFramework.SaveInTransactionResult"">   at CargoWise.EntityFramework.TransactionCoordinator.TransactionManager.CommitTransaction(SaveInTransactionResult saveInTransactionResult)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.RowFactory"" Method=""SaveTogether"" ILOffset=""0"" Parameters=""CargoWise.EntityFramework.TransactionCoordinator"">   at CargoWise.EntityFramework.RowFactory.SaveTogether(TransactionCoordinator coordinator)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObjectFactory"" Method=""SaveTogether"" ILOffset=""29"" Parameters=""CargoWise.Integration.ITransactionParticipant[]"">   at CargoWise.EntityFramework.BusinessObjectFactory.SaveTogether(ITransactionParticipant[] Factories)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZForm"" Method=""SaveInternal"" ILOffset=""156"" Parameters=""NoParameters"">   at Enterprise.ZArchitecture.GUI.ZForm.SaveInternal()</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZForm"" Method=""PerformSave"" ILOffset=""28"" Parameters=""NoParameters"">   at Enterprise.ZArchitecture.GUI.ZForm.PerformSave()</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZForm"" Method=""ValidateAndSave"" ILOffset=""265"" Parameters=""NoParameters"">   at Enterprise.ZArchitecture.GUI.ZForm.ValidateAndSave()</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZForm"" Method=""FireSaveButtonIndirectly"" ILOffset=""2"" Parameters=""System.Boolean"">   at Enterprise.ZArchitecture.GUI.ZForm.FireSaveButtonIndirectly(Boolean closeOnSave)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZForm"" Method=""HandleApplyPostingButtonClickUnsafe"" ILOffset=""293"" Parameters=""System.Boolean"">   at Enterprise.ZArchitecture.GUI.ZForm.HandleApplyPostingButtonClickUnsafe(Boolean closeOnSave)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZForm"" Method=""HandleApplyPostingButtonClick"" ILOffset=""219"" Parameters=""System.Boolean;System.Boolean"">   at Enterprise.ZArchitecture.GUI.ZForm.HandleApplyPostingButtonClick(Boolean closeOnSave, Boolean saveOnlyMode)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZForm"" Method=""SaveFormProper"" ILOffset=""53"" Parameters=""System.Boolean;System.Object;System.Boolean"">   at Enterprise.ZArchitecture.GUI.ZForm.SaveFormProper(Boolean closeOnSave, Object sender, Boolean saveOnlyMode)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.ToolStripItem"" Method=""RaiseEvent"" ILOffset=""21"" Parameters=""System.Object;System.EventArgs"">   at System.Windows.Forms.ToolStripItem.RaiseEvent(Object key, EventArgs e)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.ToolStripButton"" Method=""OnClick"" ILOffset=""30"" Parameters=""System.EventArgs"">   at System.Windows.Forms.ToolStripButton.OnClick(EventArgs e)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZToolStripButton"" Method=""OnClick"" ILOffset=""119"" Parameters=""System.EventArgs"">   at Enterprise.ZArchitecture.GUI.ZToolStripButton.OnClick(EventArgs e)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.ToolStripItem"" Method=""HandleClick"" ILOffset=""66"" Parameters=""System.EventArgs"">   at System.Windows.Forms.ToolStripItem.HandleClick(EventArgs e)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.ToolStripItem"" Method=""HandleMouseUp"" ILOffset=""203"" Parameters=""System.Windows.Forms.MouseEventArgs"">   at System.Windows.Forms.ToolStripItem.HandleMouseUp(MouseEventArgs e)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.ToolStrip"" Method=""OnMouseUp"" ILOffset=""111"" Parameters=""System.Windows.Forms.MouseEventArgs"">   at System.Windows.Forms.ToolStrip.OnMouseUp(MouseEventArgs mea)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Control"" Method=""WmMouseUp"" ILOffset=""388"" Parameters=""System.Windows.Forms.Message&amp;;System.Windows.Forms.MouseButtons;System.Int32"">   at System.Windows.Forms.Control.WmMouseUp(Message&amp; m, MouseButtons button, Int32 clicks)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Control"" Method=""WndProc"" ILOffset=""1250"" Parameters=""System.Windows.Forms.Message&amp;"">   at System.Windows.Forms.Control.WndProc(Message&amp; m)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.ToolStrip"" Method=""WndProc"" ILOffset=""231"" Parameters=""System.Windows.Forms.Message&amp;"">   at System.Windows.Forms.ToolStrip.WndProc(Message&amp; m)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZToolStrip"" Method=""WndProc"" ILOffset=""7"" Parameters=""System.Windows.Forms.Message&amp;"">   at Enterprise.ZArchitecture.GUI.ZToolStrip.WndProc(Message&amp; m)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.NativeWindow"" Method=""Callback"" ILOffset=""37"" Parameters=""System.IntPtr;System.Int32;System.IntPtr;System.IntPtr"">   at System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)</Call>
			<Call>   at System.Windows.Forms.UnsafeNativeMethods.DispatchMessageW(MSG&amp; msg)</Call>
			<Call>   at System.Windows.Forms.UnsafeNativeMethods.DispatchMessageW(MSG&amp; msg)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Application.ComponentManager"" Method=""System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.FPushMessageLoop"" ILOffset=""552"" Parameters=""System.IntPtr;System.Int32;System.Int32"">   at System.Windows.Forms.Application.ComponentManager.System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.FPushMessageLoop(IntPtr dwComponentID, Int32 reason, Int32 pvLoopData)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Application.ThreadContext"" Method=""RunMessageLoopInner"" ILOffset=""484"" Parameters=""System.Int32;System.Windows.Forms.ApplicationContext"">   at System.Windows.Forms.Application.ThreadContext.RunMessageLoopInner(Int32 reason, ApplicationContext context)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Application.ThreadContext"" Method=""RunMessageLoop"" ILOffset=""20"" Parameters=""System.Int32;System.Windows.Forms.ApplicationContext"">   at System.Windows.Forms.Application.ThreadContext.RunMessageLoop(Int32 reason, ApplicationContext context)</Call>
			<Call Assembly=""CargoWise.WindowsDesktop.exe"" Type=""Enterprise.Startup.ApplicationStartupDirector"" Method=""StartEnterprise"" ILOffset=""49"" Parameters=""System.String[]"">   at Enterprise.Startup.ApplicationStartupDirector.StartEnterprise(String[] args)</Call>
			<Call Assembly=""CargoWise.WindowsDesktop.exe"" Type=""Enterprise.Startup.ApplicationStartupDirector"" Method=""Main"" ILOffset=""36"" Parameters=""System.String[]"">   at Enterprise.Startup.ApplicationStartupDirector.Main(String[] args)</Call>");

			AddPublishedAssembly("CargoWise.Common.dll", "$/Dev/Common/Architecture/Common");
			AddPublishedAssembly("CargoWise.EntityFramework.dll", "$/Dev/Common/Architecture/EntityFramework");
			AddPublishedAssembly("Enterprise.Customs.Business.dll", "$/Dev/Enterprise/Product/Operations/Customs/Shared/Business");
			AddPublishedAssembly("Enterprise.Customs.GUI.dll", "$/Dev/Enterprise/Product/Operations/Customs/Shared/Gui");
			AddPublishedAssembly("Enterprise.ZArchitecture.GUI.dll", "$/Dev/Enterprise/Architecture/GUI");
			AddPublishedAssembly("CargoWise.WindowsDesktop.exe", "$/Dev/Enterprise/Product/Main/Enterprise.Main");

			AddSourceTreeResponsibility("$/Dev/Common", "ENT", "ARC", "ARC");
			AddSourceTreeResponsibility("$/Dev/Enterprise/Product/Operations/Customs/Shared", "ENT", "CUS", "BRB");
			AddSourceTreeResponsibility("$/Dev/Enterprise/Architecture", "ENT", "ARC", "ARC");
			AddSourceTreeResponsibility("$/Dev/Enterprise/Product/Main", "ENT", "ARC", "COR");

			EDIDataRegistry.Instance.StackLineCountNumberOfImportedLogs = 208477;

			AddStackLineCount("", "CargoWise.EntityFramework.BusinessObjectFactory.SavedEventHandler.Invoke(BusinessObjectFactory factory, Boolean SavedSuccessfully)", 7854);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Application.ComponentManager.System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.FPushMessageLoop(IntPtr dwComponentID, Int32 reason, Int32 pvLoopData)", 101654);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZForm.PerformSave()", 14150);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZToolStrip.WndProc(Message& m)", 14878);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZForm.ValidateAndSave()", 17673);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZToolStripButton.OnClick(EventArgs e)", 17714);
			AddStackLineCount("", "System.Windows.Forms.UnsafeNativeMethods.DispatchMessageW(MSG& msg)", 114554);
			AddStackLineCount("Enterprise.Customs.GUI.dll", "Enterprise.Customs.GUI.SendsMessagesToCustomsGUI.SendAmendmentMessage(BaseJobDeclaration declaration)", 31);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZForm.FireSaveButtonIndirectly(Boolean closeOnSave)", 1648);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.ToolStrip.WndProc(Message& m)", 17647);
			AddStackLineCount("mscorlib.dll", "System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)", 136622);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObjectFactory.OnSaved(Boolean SavedSuccessfully)", 8200);
			AddStackLineCount("Enterprise.Customs.Business.dll", "Enterprise.Customs.Business.BaseJobDeclaration.SendMessageWithBondedWarehouseAutomation(IWarehouseIntegrationSupporter supporter, Boolean isOutward, Boolean isChangeOfOwnership, Func`1 sendMessage, MessageAction action, Action saveFactory, Boolean additionalBondedWarehouseRequirement, Boolean reportHasChanges)", 33);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.ToolStripItem.HandleMouseUp(MouseEventArgs e)", 17198);
			AddStackLineCount("CargoWise.WindowsDesktop.exe", "Enterprise.Startup.ApplicationStartupDirector.Main(String[] args)", 80618);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZForm.SaveInternal()", 14486);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.TransactionCoordinator.TransactionManager.CommitTransaction(SaveInTransactionResult saveInTransactionResult)", 350);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObjectFactory.SaveTogether(ITransactionParticipant[] Factories)", 37582);
			AddStackLineCount("mscorlib.dll", "System.Environment.get_StackTrace()", 136470);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Control.WndProc(Message& m)", 104261);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Control.WmMouseUp(Message& m, MouseButtons button, Int32 clicks)", 46837);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZForm.SaveFormProper(Boolean closeOnSave, Object sender, Boolean saveOnlyMode)", 1886);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObjectFactory.CargoWise.Integration.ITransactionParticipant.OnAllTransactionsCommitted(IChangedTableNames changedTableNames)", 2314);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.RowFactory.SaveTogether(TransactionCoordinator coordinator)", 37776);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZForm.HandleApplyPostingButtonClickUnsafe(Boolean closeOnSave)", 1484);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.ToolStripButton.OnClick(EventArgs e)", 16151);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Application.ThreadContext.RunMessageLoopInner(Int32 reason, ApplicationContext context)", 102482);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Application.ThreadContext.RunMessageLoop(Int32 reason, ApplicationContext context)", 102480);
			AddStackLineCount("Enterprise.Customs.GUI.dll", "Enterprise.Customs.GUI.SendsMessagesToCustomsGUI.SendMessageOnSaved(BusinessObjectFactory factory, Boolean SavedSuccessfully)", 38);
			AddStackLineCount("CargoWise.Common.dll", "CargoWise.Common.ErrorReporter.ReportOnce(String key, String message, Exception exception)", 66030);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZForm.HandleApplyPostingButtonClick(Boolean closeOnSave, Boolean saveOnlyMode)", 3194);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)", 145005);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.ToolStripItem.HandleClick(EventArgs e)", 20403);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.ToolStrip.OnMouseUp(MouseEventArgs mea)", 17198);
			AddStackLineCount("CargoWise.WindowsDesktop.exe", "Enterprise.Startup.ApplicationStartupDirector.StartEnterprise(String[] args)", 80620);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.ToolStripItem.RaiseEvent(Object key, EventArgs e)", 18597);

			AssertLogAssignment(new IssueAssignment("ENT", "CUS", "BRB"), report);
		}

		public void TestRealData_HasChangesSetInConsolShipmentModuleButtonGrid00836477()
		{
			var report = GetExceptionXml(@"
			<Calls>95</Calls>
			<Call>----- Exception caught and reported here -----</Call>
			<Call>   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)</Call>
			<Call>   at System.Environment.get_StackTrace()</Call>
			<Call Assembly=""CargoWise.Common.dll"" Type=""CargoWise.Common.ErrorReporter"" Method=""ReportOnce"" ILOffset=""318"" Parameters=""System.String;System.String;System.Exception"">   at CargoWise.Common.ErrorReporter.ReportOnce(String key, String message, Exception exception)</Call>
			<Call Assembly=""Enterprise.Freight.Forwarding.GUI.dll"" Type=""Enterprise.Freight.Forwarding.GUI.ConsolShipmentModuleButtonGrid"" Method=""ShipmentOrConsolChanged"" ILOffset=""56"" Parameters=""System.Object;CargoWise.EntityFramework.HasChangesChangedEventArgs"">   at Enterprise.Freight.Forwarding.GUI.ConsolShipmentModuleButtonGrid.ShipmentOrConsolChanged(Object sender, HasChangesChangedEventArgs e)</Call>
			<Call>   at System.EventHandler`1[TEventArgs].Invoke(Object sender, TEventArgs e)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObject"" Method=""OnHasChangesChanged"" ILOffset=""18"" Parameters=""CargoWise.EntityFramework.HasChangesChangedEventArgs"">   at CargoWise.EntityFramework.BusinessObject.OnHasChangesChanged(HasChangesChangedEventArgs e)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObject"" Method=""HandleUpdateOfChildHasChanges"" ILOffset=""15"" Parameters=""System.Object;CargoWise.EntityFramework.HasChangesChangedEventArgs"">   at CargoWise.EntityFramework.BusinessObject.HandleUpdateOfChildHasChanges(Object Sender, HasChangesChangedEventArgs e)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObjectCollection"" Method=""UpdateHasChanges"" ILOffset=""21"" Parameters=""CargoWise.EntityFramework.HasChangesChangedEventArgs"">   at CargoWise.EntityFramework.BusinessObjectCollection.UpdateHasChanges(HasChangesChangedEventArgs e)</Call>
			<Call>   at System.EventHandler`1[TEventArgs].Invoke(Object sender, TEventArgs e)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObject"" Method=""OnHasChangesChanged"" ILOffset=""18"" Parameters=""CargoWise.EntityFramework.HasChangesChangedEventArgs"">   at CargoWise.EntityFramework.BusinessObject.OnHasChangesChanged(HasChangesChangedEventArgs e)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObject"" Method=""HandleUpdateOfChildHasChanges"" ILOffset=""15"" Parameters=""System.Object;CargoWise.EntityFramework.HasChangesChangedEventArgs"">   at CargoWise.EntityFramework.BusinessObject.HandleUpdateOfChildHasChanges(Object Sender, HasChangesChangedEventArgs e)</Call>
			<Call>   at System.EventHandler`1[TEventArgs].Invoke(Object sender, TEventArgs e)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObject"" Method=""OnHasChangesChanged"" ILOffset=""18"" Parameters=""CargoWise.EntityFramework.HasChangesChangedEventArgs"">   at CargoWise.EntityFramework.BusinessObject.OnHasChangesChanged(HasChangesChangedEventArgs e)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObject"" Method=""HandleUpdateOfChildHasChanges"" ILOffset=""15"" Parameters=""System.Object;CargoWise.EntityFramework.HasChangesChangedEventArgs"">   at CargoWise.EntityFramework.BusinessObject.HandleUpdateOfChildHasChanges(Object Sender, HasChangesChangedEventArgs e)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObjectCollection"" Method=""UpdateHasChanges"" ILOffset=""21"" Parameters=""CargoWise.EntityFramework.HasChangesChangedEventArgs"">   at CargoWise.EntityFramework.BusinessObjectCollection.UpdateHasChanges(HasChangesChangedEventArgs e)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObject"" Method=""OnHasChangesChanged"" ILOffset=""18"" Parameters=""CargoWise.EntityFramework.HasChangesChangedEventArgs"">   at CargoWise.EntityFramework.BusinessObject.OnHasChangesChanged(HasChangesChangedEventArgs e)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObject"" Method=""set_HasChanges"" ILOffset=""0"" Parameters=""System.Boolean"">   at CargoWise.EntityFramework.BusinessObject.set_HasChanges(Boolean value)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObject"" Method=""SetPropertyValue"" ILOffset=""216"" Parameters=""CargoWise.EntityFramework.ZPropertyInfo;CargoWise.Types.IZType;System.Boolean"">   at CargoWise.EntityFramework.BusinessObject.SetPropertyValue(ZPropertyInfo info, IZType value, Boolean setValueOnlyIfDifferentToGetter)</Call>
			<Call Assembly=""Enterprise.Freight.dll"" Type=""Enterprise.Freight.Business.AutoJobOrderItem"" Method=""set_JT_JP"" ILOffset=""0"" Parameters=""CargoWise.Types.ZGuid"">   at Enterprise.Freight.Business.AutoJobOrderItem.set_JT_JP(ZGuid value)</Call>
			<Call>   at System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor)</Call>
			<Call Assembly=""mscorlib.dll"" Type=""System.Reflection.RuntimeMethodInfo"" Method=""UnsafeInvokeInternal"" ILOffset=""22"" Parameters=""System.Object;System.Object[];System.Object[]"">   at System.Reflection.RuntimeMethodInfo.UnsafeInvokeInternal(Object obj, Object[] parameters, Object[] arguments)</Call>
			<Call Assembly=""mscorlib.dll"" Type=""System.Reflection.RuntimeMethodInfo"" Method=""Invoke"" ILOffset=""108"" Parameters=""System.Object;System.Reflection.BindingFlags;System.Reflection.Binder;System.Object[];System.Globalization.CultureInfo"">   at System.Reflection.RuntimeMethodInfo.Invoke(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture)</Call>
			<Call Assembly=""CargoWise.ComponentModel.dll"" Type=""CargoWise.ComponentModel.KReflectPropertyDescriptor"" Method=""SetValue"" ILOffset=""46"" Parameters=""System.Object;System.Object"">   at CargoWise.ComponentModel.KReflectPropertyDescriptor.SetValue(Object component, Object value)</Call>
			<Call Assembly=""CargoWise.ComponentModel.dll"" Type=""CargoWise.ComponentModel.KPropertyDescriptor"" Method=""SetValueCore"" ILOffset=""101"" Parameters=""System.Object;System.Object"">   at CargoWise.ComponentModel.KPropertyDescriptor.SetValueCore(Object component, Object value)</Call>
			<Call Assembly=""CargoWise.ComponentModel.dll"" Type=""CargoWise.ComponentModel.KPropertyDescriptor"" Method=""SetValue"" ILOffset=""0"" Parameters=""System.Object;System.Object"">   at CargoWise.ComponentModel.KPropertyDescriptor.SetValue(Object component, Object value)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.DependentBusinessObjectCollection"" Method=""RemoveCollectionRelationshipsCore"" ILOffset=""81"" Parameters=""CargoWise.EntityFramework.BusinessObject;System.Boolean"">   at CargoWise.EntityFramework.DependentBusinessObjectCollection.RemoveCollectionRelationshipsCore(BusinessObject Dependent, Boolean forDelete)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObjectCollection"" Method=""RemoveCollectionRelationships"" ILOffset=""31"" Parameters=""CargoWise.EntityFramework.BusinessObject;System.Boolean"">   at CargoWise.EntityFramework.BusinessObjectCollection.RemoveCollectionRelationships(BusinessObject child, Boolean forDelete)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObjectCollection"" Method=""RemoveAndDelete"" ILOffset=""203"" Parameters=""CargoWise.EntityFramework.BusinessObject"">   at CargoWise.EntityFramework.BusinessObjectCollection.RemoveAndDelete(BusinessObject ElementToDelete)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObjectCollection"" Method=""RemoveAndDeleteAll"" ILOffset=""86"" Parameters=""NoParameters"">   at CargoWise.EntityFramework.BusinessObjectCollection.RemoveAndDeleteAll()</Call>
			<Call Assembly=""Enterprise.Freight.dll"" Type=""Enterprise.Freight.Business.OrderItemCollection"" Method=""set_AsString"" ILOffset=""0"" Parameters=""CargoWise.Types.ZString"">   at Enterprise.Freight.Business.OrderItemCollection.set_AsString(ZString value)</Call>
			<Call Assembly=""Enterprise.Freight.dll"" Type=""Enterprise.Freight.Business.JobDocsAndCartage"" Method=""set_JP_OrderItemsAsString"" ILOffset=""45"" Parameters=""CargoWise.Types.ZString"">   at Enterprise.Freight.Business.JobDocsAndCartage.set_JP_OrderItemsAsString(ZString value)</Call>
			<Call Assembly=""Enterprise.Freight.Forwarding.Business.dll"" Type=""Enterprise.Freight.Forwarding.Business.ForwardingShipment"" Method=""OnOrderCollectionCountChanged"" ILOffset=""44"" Parameters=""NoParameters"">   at Enterprise.Freight.Forwarding.Business.ForwardingShipment.OnOrderCollectionCountChanged()</Call>
			<Call>   at System.EventHandler.Invoke(Object sender, EventArgs e)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.ActiveBusinessObjectCollection`1[T]"" Method=""ListChanged_ToMaintainCountChanged"" ILOffset=""91"" Parameters=""System.Object;System.ComponentModel.ListChangedEventArgs"">   at CargoWise.EntityFramework.ActiveBusinessObjectCollection`1[T].ListChanged_ToMaintainCountChanged(Object sender, ListChangedEventArgs e)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.ActiveBusinessObjectCollection`1[T]"" Method=""OnListChanged"" ILOffset=""29"" Parameters=""System.ComponentModel.ListChangedEventArgs"">   at CargoWise.EntityFramework.ActiveBusinessObjectCollection`1[T].OnListChanged(ListChangedEventArgs e)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.ActiveBusinessObjectCollectionIndex`1[T]"" Method=""FireChangeEvents_Delayed"" ILOffset=""16"" Parameters=""System.Object;System.ComponentModel.ListChangedEventArgs"">   at CargoWise.EntityFramework.ActiveBusinessObjectCollectionIndex`1[T].FireChangeEvents_Delayed(Object sender, ListChangedEventArgs e)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.ListChangedDelegateInvoker"" Method=""Invoke"" ILOffset=""20"" Parameters=""System.ComponentModel.ListChangedEventHandler;System.Object;System.ComponentModel.ListChangedEventArgs"">   at CargoWise.EntityFramework.ListChangedDelegateInvoker.Invoke(ListChangedEventHandler method, Object sender, ListChangedEventArgs e)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.CollectionListChangedSuspender.ListChangedEventsDelayer"" Method=""FireChangeEvents"" ILOffset=""29"" Parameters=""NoParameters"">   at CargoWise.EntityFramework.CollectionListChangedSuspender.ListChangedEventsDelayer.FireChangeEvents()</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.CollectionListChangedSuspender"" Method=""&lt;DelayListChangedEvents&gt;b__7_0"" ILOffset=""29"" Parameters=""NoParameters"">   at CargoWise.EntityFramework.CollectionListChangedSuspender.&lt;DelayListChangedEvents&gt;b__7_0()</Call>
			<Call Assembly=""CargoWise.Common.dll"" Type=""CargoWise.Common.DisposableList"" Method=""Dispose"" ILOffset=""32"" Parameters=""NoParameters"">   at CargoWise.Common.DisposableList.Dispose()</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObjectFactory"" Method=""CargoWise.Integration.ITransactionParticipant.OnAllTransactionsCommitted"" ILOffset=""0"" Parameters=""CargoWise.Integration.IChangedTableNames"">   at CargoWise.EntityFramework.BusinessObjectFactory.CargoWise.Integration.ITransactionParticipant.OnAllTransactionsCommitted(IChangedTableNames changedTableNames)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.TransactionCoordinator.TransactionManager"" Method=""CommitTransaction"" ILOffset=""36"" Parameters=""CargoWise.EntityFramework.SaveInTransactionResult"">   at CargoWise.EntityFramework.TransactionCoordinator.TransactionManager.CommitTransaction(SaveInTransactionResult saveInTransactionResult)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.RowFactory"" Method=""SaveTogether"" ILOffset=""0"" Parameters=""CargoWise.EntityFramework.TransactionCoordinator"">   at CargoWise.EntityFramework.RowFactory.SaveTogether(TransactionCoordinator coordinator)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObjectFactory"" Method=""SaveTogether"" ILOffset=""29"" Parameters=""CargoWise.Integration.ITransactionParticipant[]"">   at CargoWise.EntityFramework.BusinessObjectFactory.SaveTogether(ITransactionParticipant[] Factories)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZForm"" Method=""SaveInternal"" ILOffset=""156"" Parameters=""NoParameters"">   at Enterprise.ZArchitecture.GUI.ZForm.SaveInternal()</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZForm"" Method=""PerformSave"" ILOffset=""28"" Parameters=""NoParameters"">   at Enterprise.ZArchitecture.GUI.ZForm.PerformSave()</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZForm"" Method=""ValidateAndSave"" ILOffset=""265"" Parameters=""NoParameters"">   at Enterprise.ZArchitecture.GUI.ZForm.ValidateAndSave()</Call>
			<Call Assembly=""Enterprise.Freight.Forwarding.GUI.dll"" Type=""Enterprise.Freight.Forwarding.GUI.ConsolForm"" Method=""ValidateAndSave"" ILOffset=""17"" Parameters=""NoParameters"">   at Enterprise.Freight.Forwarding.GUI.ConsolForm.ValidateAndSave()</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZForm"" Method=""FireSaveButtonIndirectly"" ILOffset=""2"" Parameters=""System.Boolean"">   at Enterprise.ZArchitecture.GUI.ZForm.FireSaveButtonIndirectly(Boolean closeOnSave)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZForm"" Method=""HandleApplyPostingButtonClick"" ILOffset=""209"" Parameters=""System.Boolean;System.Boolean"">   at Enterprise.ZArchitecture.GUI.ZForm.HandleApplyPostingButtonClick(Boolean closeOnSave, Boolean saveOnlyMode)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZForm"" Method=""SaveFormProper"" ILOffset=""53"" Parameters=""System.Boolean;System.Object;System.Boolean"">   at Enterprise.ZArchitecture.GUI.ZForm.SaveFormProper(Boolean closeOnSave, Object sender, Boolean saveOnlyMode)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZForm"" Method=""FireSaveButton"" ILOffset=""2"" Parameters=""System.Object"">   at Enterprise.ZArchitecture.GUI.ZForm.FireSaveButton(Object sender)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZModuleButtonGrid"" Method=""Edit"" ILOffset=""201"" Parameters=""CargoWise.EntityFramework.BusinessObject;System.Object"">   at Enterprise.ZArchitecture.GUI.ZModuleButtonGrid.Edit(BusinessObject selected, Object sender)</Call>
			<Call Assembly=""Enterprise.Freight.Forwarding.GUI.dll"" Type=""Enterprise.Freight.Forwarding.GUI.ConsolShipmentModuleButtonGrid"" Method=""Edit"" ILOffset=""79"" Parameters=""CargoWise.EntityFramework.BusinessObject;System.Object"">   at Enterprise.Freight.Forwarding.GUI.ConsolShipmentModuleButtonGrid.Edit(BusinessObject selected, Object sender)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZModuleButtonGrid"" Method=""InnerGrid_MouseDown"" ILOffset=""118"" Parameters=""System.Object;System.Windows.Forms.MouseEventArgs"">   at Enterprise.ZArchitecture.GUI.ZModuleButtonGrid.InnerGrid_MouseDown(Object sender, MouseEventArgs e)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Control"" Method=""OnMouseDown"" ILOffset=""33"" Parameters=""System.Windows.Forms.MouseEventArgs"">   at System.Windows.Forms.Control.OnMouseDown(MouseEventArgs e)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.DataGrid"" Method=""OnMouseDown"" ILOffset=""7"" Parameters=""System.Windows.Forms.MouseEventArgs"">   at System.Windows.Forms.DataGrid.OnMouseDown(MouseEventArgs e)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.ZGrid"" Method=""OnMouseDown"" ILOffset=""1016"" Parameters=""System.Windows.Forms.MouseEventArgs"">   at Enterprise.ZArchitecture.ZGrid.OnMouseDown(MouseEventArgs e)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Control"" Method=""WmMouseDown"" ILOffset=""154"" Parameters=""System.Windows.Forms.Message&amp;;System.Windows.Forms.MouseButtons;System.Int32"">   at System.Windows.Forms.Control.WmMouseDown(Message&amp; m, MouseButtons button, Int32 clicks)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Control"" Method=""WndProc"" ILOffset=""1194"" Parameters=""System.Windows.Forms.Message&amp;"">   at System.Windows.Forms.Control.WndProc(Message&amp; m)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.ZGrid"" Method=""WndProc"" ILOffset=""116"" Parameters=""System.Windows.Forms.Message&amp;"">   at Enterprise.ZArchitecture.ZGrid.WndProc(Message&amp; m)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.NativeWindow"" Method=""Callback"" ILOffset=""37"" Parameters=""System.IntPtr;System.Int32;System.IntPtr;System.IntPtr"">   at System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)</Call>
			<Call>   at System.Windows.Forms.UnsafeNativeMethods.DispatchMessageW(MSG&amp; msg)</Call>
			<Call>   at System.Windows.Forms.UnsafeNativeMethods.DispatchMessageW(MSG&amp; msg)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Application.ComponentManager"" Method=""System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.FPushMessageLoop"" ILOffset=""552"" Parameters=""System.IntPtr;System.Int32;System.Int32"">   at System.Windows.Forms.Application.ComponentManager.System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.FPushMessageLoop(IntPtr dwComponentID, Int32 reason, Int32 pvLoopData)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Application.ThreadContext"" Method=""RunMessageLoopInner"" ILOffset=""484"" Parameters=""System.Int32;System.Windows.Forms.ApplicationContext"">   at System.Windows.Forms.Application.ThreadContext.RunMessageLoopInner(Int32 reason, ApplicationContext context)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Application.ThreadContext"" Method=""RunMessageLoop"" ILOffset=""20"" Parameters=""System.Int32;System.Windows.Forms.ApplicationContext"">   at System.Windows.Forms.Application.ThreadContext.RunMessageLoop(Int32 reason, ApplicationContext context)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Form"" Method=""ShowDialog"" ILOffset=""580"" Parameters=""System.Windows.Forms.IWin32Window"">   at System.Windows.Forms.Form.ShowDialog(IWin32Window owner)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZFormModaliser"" Method=""ShowDialogInternalWithoutDispose"" ILOffset=""28"" Parameters=""System.Windows.Forms.Form;System.Windows.Forms.Form"">   at Enterprise.ZArchitecture.GUI.ZFormModaliser.ShowDialogInternalWithoutDispose(Form form, Form parentForm)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZFormModaliser"" Method=""ShowDialogWithoutDispose"" ILOffset=""76"" Parameters=""System.Windows.Forms.Form;System.Windows.Forms.Form"">   at Enterprise.ZArchitecture.GUI.ZFormModaliser.ShowDialogWithoutDispose(Form form, Form parentForm)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZFormModaliser"" Method=""ShowDialogAndDispose"" ILOffset=""6"" Parameters=""System.Windows.Forms.Form;System.Windows.Forms.Form"">   at Enterprise.ZArchitecture.GUI.ZFormModaliser.ShowDialogAndDispose(Form form, Form parentForm)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.Modules.ZController"" Method=""ShowForm"" ILOffset=""255"" Parameters=""Enterprise.ZArchitecture.GUI.IZForm;Enterprise.Integration.Licensing.ILicenceCheckpoint"">   at Enterprise.ZArchitecture.Modules.ZController.ShowForm(IZForm formToShow, ILicenceCheckpoint licenceCheckpointOverride)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.Modules.ZController"" Method=""ShowFormForNewEntityCore"" ILOffset=""75"" Parameters=""CargoWise.EntityFramework.IBusiness"">   at Enterprise.ZArchitecture.Modules.ZController.ShowFormForNewEntityCore(IBusiness BusinessEntity)</Call>
			<Call Assembly=""Enterprise.Freight.Forwarding.GUI.dll"" Type=""Enterprise.Freight.Forwarding.Orders.GUI.PreAdviceConversionHelper.&lt;&gt;c"" Method=""&lt;ShowFormWithNewlyCreatedConsolIfHasErros&gt;b__0_0"" ILOffset=""41"" Parameters=""Enterprise.Freight.Forwarding.Business.ForwardingConsol"">   at Enterprise.Freight.Forwarding.Orders.GUI.PreAdviceConversionHelper.&lt;&gt;c.&lt;ShowFormWithNewlyCreatedConsolIfHasErros&gt;b__0_0(ForwardingConsol consol)</Call>
			<Call Assembly=""Enterprise.Freight.Forwarding.Business.dll"" Type=""Enterprise.Freight.Forwarding.Orders.Business.JobShipmentPreplanning"" Method=""CreateConsolAndShipment"" ILOffset=""244"" Parameters=""Enterprise.Freight.Forwarding.Orders.Business.JobShipmentPreplanning.OrderShipmentCreationMode;CargoWise.ComponentModel.INotifications;System.Action`1[Enterprise.Freight.Forwarding.Business.ForwardingConsol]"">   at Enterprise.Freight.Forwarding.Orders.Business.JobShipmentPreplanning.CreateConsolAndShipment(OrderShipmentCreationMode creationMode, INotifications notification, Action`1 onCreateConsol)</Call>
			<Call Assembly=""Enterprise.Freight.Forwarding.GUI.dll"" Type=""Enterprise.Freight.Forwarding.Orders.GUI.OrdersForm"" Method=""CreateShipment_Click"" ILOffset=""135"" Parameters=""System.Object;System.EventArgs"">   at Enterprise.Freight.Forwarding.Orders.GUI.OrdersForm.CreateShipment_Click(Object sender, EventArgs args)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.MenuItem"" Method=""OnClick"" ILOffset=""104"" Parameters=""System.EventArgs"">   at System.Windows.Forms.MenuItem.OnClick(EventArgs e)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZMenuItem"" Method=""OnClick"" ILOffset=""152"" Parameters=""System.EventArgs"">   at Enterprise.ZArchitecture.GUI.ZMenuItem.OnClick(EventArgs e)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.MenuItem.MenuItemData"" Method=""Execute"" ILOffset=""24"" Parameters=""NoParameters"">   at System.Windows.Forms.MenuItem.MenuItemData.Execute()</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Command"" Method=""Invoke"" ILOffset=""28"" Parameters=""NoParameters"">   at System.Windows.Forms.Command.Invoke()</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Control"" Method=""WmCommand"" ILOffset=""59"" Parameters=""System.Windows.Forms.Message&amp;"">   at System.Windows.Forms.Control.WmCommand(Message&amp; m)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Control"" Method=""WndProc"" ILOffset=""866"" Parameters=""System.Windows.Forms.Message&amp;"">   at System.Windows.Forms.Control.WndProc(Message&amp; m)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Form"" Method=""WndProc"" ILOffset=""720"" Parameters=""System.Windows.Forms.Message&amp;"">   at System.Windows.Forms.Form.WndProc(Message&amp; m)</Call>
			<Call Assembly=""CargoWise.Windows.UI.dll"" Type=""CargoWise.Windows.UI.KForm"" Method=""WndProc"" ILOffset=""220"" Parameters=""System.Windows.Forms.Message&amp;"">   at CargoWise.Windows.UI.KForm.WndProc(Message&amp; m)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZForm"" Method=""WndProc"" ILOffset=""376"" Parameters=""System.Windows.Forms.Message&amp;"">   at Enterprise.ZArchitecture.GUI.ZForm.WndProc(Message&amp; m)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.NativeWindow"" Method=""Callback"" ILOffset=""37"" Parameters=""System.IntPtr;System.Int32;System.IntPtr;System.IntPtr"">   at System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)</Call>
			<Call>   at System.Windows.Forms.UnsafeNativeMethods.DispatchMessageW(MSG&amp; msg)</Call>
			<Call>   at System.Windows.Forms.UnsafeNativeMethods.DispatchMessageW(MSG&amp; msg)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Application.ComponentManager"" Method=""System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.FPushMessageLoop"" ILOffset=""552"" Parameters=""System.IntPtr;System.Int32;System.Int32"">   at System.Windows.Forms.Application.ComponentManager.System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.FPushMessageLoop(IntPtr dwComponentID, Int32 reason, Int32 pvLoopData)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Application.ThreadContext"" Method=""RunMessageLoopInner"" ILOffset=""484"" Parameters=""System.Int32;System.Windows.Forms.ApplicationContext"">   at System.Windows.Forms.Application.ThreadContext.RunMessageLoopInner(Int32 reason, ApplicationContext context)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Application.ThreadContext"" Method=""RunMessageLoop"" ILOffset=""20"" Parameters=""System.Int32;System.Windows.Forms.ApplicationContext"">   at System.Windows.Forms.Application.ThreadContext.RunMessageLoop(Int32 reason, ApplicationContext context)</Call>
			<Call Assembly=""CargoWise.WindowsDesktop.exe"" Type=""Enterprise.Startup.ApplicationStartupDirector"" Method=""StartEnterprise"" ILOffset=""49"" Parameters=""System.String[]"">   at Enterprise.Startup.ApplicationStartupDirector.StartEnterprise(String[] args)</Call>
			<Call Assembly=""CargoWise.WindowsDesktop.exe"" Type=""Enterprise.Startup.ApplicationStartupDirector"" Method=""Main"" ILOffset=""36"" Parameters=""System.String[]"">   at Enterprise.Startup.ApplicationStartupDirector.Main(String[] args)</Call>");

			AddPublishedAssembly("CargoWise.Common.dll", "$/Dev/Common/Architecture/Common");
			AddPublishedAssembly("CargoWise.EntityFramework.dll", "$/Dev/Common/Architecture/EntityFramework");
			AddPublishedAssembly("Enterprise.Freight.Forwarding.GUI.dll", "$/Dev/Enterprise/Product/Operations/Freight/Forwarding/Forwarding.GUI");
			AddPublishedAssembly("CargoWise.ComponentModel.dll", "$/Dev/Common/Architecture/ComponentModel");
			AddPublishedAssembly("Enterprise.Freight.dll", "$/Dev/Enterprise/Product/Operations/Freight/Shared/Freight.Business");
			AddPublishedAssembly("Enterprise.Freight.Forwarding.Business.dll", "$/Dev/Enterprise/Product/Operations/Freight/Forwarding/Forwarding.Business");
			AddPublishedAssembly("Enterprise.ZArchitecture.GUI.dll", "$/Dev/Enterprise/Architecture/GUI");
			AddPublishedAssembly("CargoWise.WindowsDesktop.exe", "$/Dev/Enterprise/Product/Main/Enterprise.Main");

			AddSourceTreeResponsibility("$/Dev/Common", "ENT", "ARC", "ARC");
			AddSourceTreeResponsibility("$/Dev/Enterprise/Product/Operations/Freight", "ENT", "INT", "FOR");
			AddSourceTreeResponsibility("$/Dev/Enterprise/Architecture", "ENT", "ARC", "ARC");
			AddSourceTreeResponsibility("$/Dev/Enterprise/Product/Main", "ENT", "ARC", "COR");

			EDIDataRegistry.Instance.StackLineCountNumberOfImportedLogs = 208477;

			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObjectCollection.RemoveAndDelete(BusinessObject ElementToDelete)", 468);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.Modules.ZController.ShowFormForNewEntityCore(IBusiness BusinessEntity)", 509);
			AddStackLineCount("Enterprise.Freight.Forwarding.GUI.dll", "Enterprise.Freight.Forwarding.Orders.GUI.PreAdviceConversionHelper.<>c.<ShowFormWithNewlyCreatedConsolIfHasErros>b__0_0(ForwardingConsol consol)", 52);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZFormModaliser.ShowDialogAndDispose(Form form, Form parentForm)", 3638);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Application.ComponentManager.System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.FPushMessageLoop(IntPtr dwComponentID, Int32 reason, Int32 pvLoopData)", 101654);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZForm.PerformSave()", 14150);
			AddStackLineCount("Enterprise.Freight.Forwarding.Business.dll", "Enterprise.Freight.Forwarding.Business.ForwardingShipment.OnOrderCollectionCountChanged()", 2);
			AddStackLineCount("Enterprise.Freight.Forwarding.Business.dll", "Enterprise.Freight.Forwarding.Orders.Business.JobShipmentPreplanning.CreateConsolAndShipment(OrderShipmentCreationMode creationMode, INotifications notification, Action`1 onCreateConsol)", 28);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.ListChangedDelegateInvoker.Invoke(ListChangedEventHandler method, Object sender, ListChangedEventArgs e)", 675);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZForm.ValidateAndSave()", 17673);
			AddStackLineCount("Enterprise.Freight.dll", "Enterprise.Freight.Business.JobDocsAndCartage.set_JP_OrderItemsAsString(ZString value)", 1);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Command.Invoke()", 15891);
			AddStackLineCount("Enterprise.Freight.Forwarding.GUI.dll", "Enterprise.Freight.Forwarding.GUI.ConsolForm.ValidateAndSave()", 1914);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.ZGrid.OnMouseDown(MouseEventArgs e)", 4244);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.Modules.ZController.ShowForm(IZForm formToShow, ILicenceCheckpoint licenceCheckpointOverride)", 1957);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZModuleButtonGrid.Edit(BusinessObject selected, Object sender)", 43);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Control.WmCommand(Message& m)", 16019);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.CollectionListChangedSuspender.ListChangedEventsDelayer.FireChangeEvents()", 698);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Control.WmMouseDown(Message& m, MouseButtons button, Int32 clicks)", 7482);
			AddStackLineCount("", "System.Windows.Forms.UnsafeNativeMethods.DispatchMessageW(MSG& msg)", 114554);
			AddStackLineCount("CargoWise.ComponentModel.dll", "CargoWise.ComponentModel.KPropertyDescriptor.SetValueCore(Object component, Object value)", 5971);
			AddStackLineCount("CargoWise.Windows.UI.dll", "CargoWise.Windows.UI.KForm.WndProc(Message& m)", 20002);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZForm.FireSaveButtonIndirectly(Boolean closeOnSave)", 1648);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.DependentBusinessObjectCollection.RemoveCollectionRelationshipsCore(BusinessObject Dependent, Boolean forDelete)", 33);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZFormModaliser.ShowDialogWithoutDispose(Form form, Form parentForm)", 5841);
			AddStackLineCount("mscorlib.dll", "System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)", 136622);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObject.SetPropertyValue(ZPropertyInfo info, IZType value, Boolean setValueOnlyIfDifferentToGetter)", 1579);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZFormModaliser.ShowDialogInternalWithoutDispose(Form form, Form parentForm)", 8167);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.DataGrid.OnMouseDown(MouseEventArgs e)", 2703);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.ZGrid.WndProc(Message& m)", 15129);
			AddStackLineCount("CargoWise.Common.dll", "CargoWise.Common.DisposableList.Dispose()", 645);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Control.OnMouseDown(MouseEventArgs e)", 2059);
			AddStackLineCount("Enterprise.Freight.dll", "Enterprise.Freight.Business.OrderItemCollection.set_AsString(ZString value)", 1);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZMenuItem.OnClick(EventArgs e)", 20640);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZForm.FireSaveButton(Object sender)", 143);
			AddStackLineCount("CargoWise.WindowsDesktop.exe", "Enterprise.Startup.ApplicationStartupDirector.Main(String[] args)", 80618);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZForm.SaveInternal()", 14486);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.TransactionCoordinator.TransactionManager.CommitTransaction(SaveInTransactionResult saveInTransactionResult)", 350);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObject.set_HasChanges(Boolean value)", 161);
			AddStackLineCount("Enterprise.Freight.Forwarding.GUI.dll", "Enterprise.Freight.Forwarding.GUI.ConsolShipmentModuleButtonGrid.Edit(BusinessObject selected, Object sender)", 24);
			AddStackLineCount("mscorlib.dll", "System.Reflection.RuntimeMethodInfo.Invoke(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture)", 13504);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObjectFactory.SaveTogether(ITransactionParticipant[] Factories)", 37582);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.ActiveBusinessObjectCollectionIndex`1[T].FireChangeEvents_Delayed(Object sender, ListChangedEventArgs e)", 853);
			AddStackLineCount("mscorlib.dll", "System.Environment.get_StackTrace()", 136470);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObjectCollection.UpdateHasChanges(HasChangesChangedEventArgs e)", 43);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Control.WndProc(Message& m)", 104261);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Form.WndProc(Message& m)", 28546);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZForm.SaveFormProper(Boolean closeOnSave, Object sender, Boolean saveOnlyMode)", 1886);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.ActiveBusinessObjectCollection`1[T].OnListChanged(ListChangedEventArgs e)", 909);
			AddStackLineCount("Enterprise.Freight.Forwarding.GUI.dll", "Enterprise.Freight.Forwarding.Orders.GUI.OrdersForm.CreateShipment_Click(Object sender, EventArgs args)", 118);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObjectFactory.CargoWise.Integration.ITransactionParticipant.OnAllTransactionsCommitted(IChangedTableNames changedTableNames)", 2314);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZForm.WndProc(Message& m)", 27230);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.RowFactory.SaveTogether(TransactionCoordinator coordinator)", 37776);
			AddStackLineCount("mscorlib.dll", "System.Reflection.RuntimeMethodInfo.UnsafeInvokeInternal(Object obj, Object[] parameters, Object[] arguments)", 14380);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObject.HandleUpdateOfChildHasChanges(Object Sender, HasChangesChangedEventArgs e)", 147);
			AddStackLineCount("CargoWise.ComponentModel.dll", "CargoWise.ComponentModel.KPropertyDescriptor.SetValue(Object component, Object value)", 6027);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Form.ShowDialog(IWin32Window owner)", 20172);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.MenuItem.OnClick(EventArgs e)", 19783);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObject.OnHasChangesChanged(HasChangesChangedEventArgs e)", 196);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Application.ThreadContext.RunMessageLoopInner(Int32 reason, ApplicationContext context)", 102482);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.CollectionListChangedSuspender.<DelayListChangedEvents>b__7_0()", 520);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.ActiveBusinessObjectCollection`1[T].ListChanged_ToMaintainCountChanged(Object sender, ListChangedEventArgs e)", 206);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Application.ThreadContext.RunMessageLoop(Int32 reason, ApplicationContext context)", 102480);
			AddStackLineCount("CargoWise.ComponentModel.dll", "CargoWise.ComponentModel.KReflectPropertyDescriptor.SetValue(Object component, Object value)", 5589);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObjectCollection.RemoveAndDeleteAll()", 146);
			AddStackLineCount("CargoWise.Common.dll", "CargoWise.Common.ErrorReporter.ReportOnce(String key, String message, Exception exception)", 66030);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.MenuItem.MenuItemData.Execute()", 15193);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZModuleButtonGrid.InnerGrid_MouseDown(Object sender, MouseEventArgs e)", 636);
			AddStackLineCount("", "System.RuntimeMethodHandle.InvokeMethod(Object target, Object[] arguments, Signature sig, Boolean constructor)", 15762);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZForm.HandleApplyPostingButtonClick(Boolean closeOnSave, Boolean saveOnlyMode)", 3194);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObjectCollection.RemoveCollectionRelationships(BusinessObject child, Boolean forDelete)", 79);
			AddStackLineCount("", "System.EventHandler.Invoke(Object sender, EventArgs e)", 19164);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)", 145005);
			AddStackLineCount("CargoWise.WindowsDesktop.exe", "Enterprise.Startup.ApplicationStartupDirector.StartEnterprise(String[] args)", 80620);
			AddStackLineCount("", "System.EventHandler`1[TEventArgs].Invoke(Object sender, TEventArgs e)", 453);

			AssertLogAssignment(new IssueAssignment("ENT", "INT", "FOR"), report);
		}

		public void TestRealData_RatingIssue01001580()
		{
			// working fine in initial implementation, added for regression testing

			var report = GetExceptionXml(@"
			<Call Assembly=""System.Data.dll"" Type=""System.Data.DataRow"" Method=""GetDefaultRecord"" ILOffset=""47"" Parameters=""NoParameters"">   at System.Data.DataRow.GetDefaultRecord()</Call>
			<Call Assembly=""System.Data.dll"" Type=""System.Data.DataRow"" Method=""get_Item"" ILOffset=""0"" Parameters=""System.Data.DataColumn;System.Data.DataRowVersion"">   at System.Data.DataRow.get_Item(DataColumn column, DataRowVersion version)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObject"" Method=""GetValueFromRowSafely"" ILOffset=""29"" Parameters=""System.Data.DataColumn;System.Data.DataRowVersion"">   at CargoWise.EntityFramework.BusinessObject.GetValueFromRowSafely(DataColumn column, DataRowVersion version)</Call>
			<Call>----- Exception caught and reported here -----</Call>
			<Call>   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)</Call>
			<Call>   at System.Environment.get_StackTrace()</Call>
			<Call Assembly=""CargoWise.Common.dll"" Type=""CargoWise.Common.ErrorReporter"" Method=""ReportOnce"" ILOffset=""318"" Parameters=""System.String;System.String;System.Exception"">   at CargoWise.Common.ErrorReporter.ReportOnce(String key, String message, Exception exception)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObject"" Method=""ReportRowError"" ILOffset=""372"" Parameters=""System.String;System.Exception;System.Data.DataRowVersion;System.Data.DataRowVersion;System.String"">   at CargoWise.EntityFramework.BusinessObject.ReportRowError(String columnName, Exception ex, DataRowVersion versionToUse, DataRowVersion version, String message)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObject"" Method=""ReportRowDeletedError"" ILOffset=""16"" Parameters=""System.String;System.Exception;System.Data.DataRowVersion;System.Data.DataRowVersion"">   at CargoWise.EntityFramework.BusinessObject.ReportRowDeletedError(String columnName, Exception ex, DataRowVersion versionToUse, DataRowVersion version)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObject"" Method=""GetValueFromRowSafely"" ILOffset=""29"" Parameters=""System.Data.DataColumn;System.Data.DataRowVersion"">   at CargoWise.EntityFramework.BusinessObject.GetValueFromRowSafely(DataColumn column, DataRowVersion version)</Call>
			<Call Assembly=""Enterprise.MasterFiles.Business.dll"" Type=""Enterprise.MasterFiles.Business.AutoJobCharge"" Method=""get_JR_AC"" ILOffset=""0"" Parameters=""NoParameters"">   at Enterprise.MasterFiles.Business.AutoJobCharge.get_JR_AC()</Call>
			<Call Assembly=""Enterprise.Accounting.Business.dll"" Type=""Enterprise.Accounting.Business.JobInvoicing.Charge"" Method=""get_JR_AC"" ILOffset=""0"" Parameters=""NoParameters"">   at Enterprise.Accounting.Business.JobInvoicing.Charge.get_JR_AC()</Call>
			<Call Assembly=""Enterprise.MasterFiles.Business.dll"" Type=""Enterprise.MasterFiles.Business.AutoJobCharge"" Method=""get_ChargeCode"" ILOffset=""0"" Parameters=""NoParameters"">   at Enterprise.MasterFiles.Business.AutoJobCharge.get_ChargeCode()</Call>
			<Call Assembly=""Enterprise.Rating.Business.dll"" Type=""Enterprise.Rating.Business.DependentCalculatorExtensions"" Method=""IsApplicable"" ILOffset=""6"" Parameters=""Enterprise.Rating.Business.IRateLineItem;Enterprise.Accounting.Integration.IAutoRatingChargeInfo;Enterprise.Rating.Business.AutoRatingCalculatorParameters"">   at Enterprise.Rating.Business.DependentCalculatorExtensions.IsApplicable(IRateLineItem applyToItem, IAutoRatingChargeInfo charge, AutoRatingCalculatorParameters parameters)</Call>
			<Call Assembly=""System.Core.dll"" Type=""System.Linq.Enumerable.WhereArrayIterator`1[TSource]"" Method=""MoveNext"" ILOffset=""43"" Parameters=""NoParameters"">   at System.Linq.Enumerable.WhereArrayIterator`1[TSource].MoveNext()</Call>
			<Call Assembly=""mscorlib.dll"" Type=""System.Collections.Generic.List`1[T]"" Method=""InsertRange"" ILOffset=""234"" Parameters=""System.Int32;System.Collections.Generic.IEnumerable`1[T]"">   at System.Collections.Generic.List`1[T].InsertRange(Int32 index, IEnumerable`1 collection)</Call>
			<Call Assembly=""Enterprise.Rating.Business.dll"" Type=""Enterprise.Rating.Business.DependentCalculatorExtensions"" Method=""MergeAutoRateInfosWithExistingJobCharges"" ILOffset=""213"" Parameters=""Enterprise.Rating.Business.IRateLineItem;Enterprise.Rating.Business.AutoRatingCalculatorParameters"">   at Enterprise.Rating.Business.DependentCalculatorExtensions.MergeAutoRateInfosWithExistingJobCharges(IRateLineItem applyToItem, AutoRatingCalculatorParameters parameters)</Call>
			<Call Assembly=""Enterprise.Rating.Business.dll"" Type=""Enterprise.Rating.Business.DependentCalculatorExtensions"" Method=""GetApplicableMoneyForItem"" ILOffset=""6"" Parameters=""Enterprise.Rating.Business.IRateLineItem;System.Boolean;Enterprise.Rating.Business.AutoRatingCalculatorParameters"">   at Enterprise.Rating.Business.DependentCalculatorExtensions.GetApplicableMoneyForItem(IRateLineItem applyToItem, Boolean includeGST, AutoRatingCalculatorParameters parameters)</Call>
			<Call Assembly=""Enterprise.Rating.Business.dll"" Type=""Enterprise.Rating.Business.DependentCalculatorExtensions"" Method=""GetAmountItemAppliesTo"" ILOffset=""317"" Parameters=""Enterprise.Rating.Business.IRateLineItem;System.Boolean;Enterprise.Rating.Business.AutoRatingCalculatorParameters"">   at Enterprise.Rating.Business.DependentCalculatorExtensions.GetAmountItemAppliesTo(IRateLineItem applyToItem, Boolean includeGST, AutoRatingCalculatorParameters parameters)</Call>
			<Call Assembly=""Enterprise.Rating.Business.dll"" Type=""Enterprise.Rating.Business.DependentCalculatorExtensions"" Method=""GetValueCalculatorAppliesTo"" ILOffset=""83"" Parameters=""Enterprise.Rating.Business.IDependentCalculator;Enterprise.Rating.Business.AutoRatingCalculatorParameters;Enterprise.Rating.Business.IRateLineItem&amp;;System.Boolean;System.Boolean"">   at Enterprise.Rating.Business.DependentCalculatorExtensions.GetValueCalculatorAppliesTo(IDependentCalculator calculator, AutoRatingCalculatorParameters parameters, IRateLineItem&amp; greaterChargeItem, Boolean includeGST, Boolean useGreaterCharge)</Call>
			<Call Assembly=""Enterprise.Rating.Business.dll"" Type=""Enterprise.Rating.Business.PercentageCalculator"" Method=""CalculateInternal"" ILOffset=""0"" Parameters=""Enterprise.Rating.Business.AutoRatingCalculatorParameters;Enterprise.Rating.Business.CalculationResult;Enterprise.Rating.Business.CalculationResult.DescriptionList"">   at Enterprise.Rating.Business.PercentageCalculator.CalculateInternal(AutoRatingCalculatorParameters parameters, CalculationResult calculationResult, DescriptionList description)</Call>
			<Call Assembly=""Enterprise.Rating.Business.dll"" Type=""Enterprise.Rating.Business.Calculator"" Method=""Calculate"" ILOffset=""54"" Parameters=""Enterprise.Rating.Business.AutoRatingCalculatorParameters;Enterprise.Rating.Business.IRateLine"">   at Enterprise.Rating.Business.Calculator.Calculate(AutoRatingCalculatorParameters parameters, IRateLine contextLine)</Call>
			<Call Assembly=""Enterprise.Rating.Business.dll"" Type=""Enterprise.Rating.Business.FreightAutoRater"" Method=""Calculate"" ILOffset=""19"" Parameters=""Enterprise.Rating.Business.AutoRatingCalculatorParameters;Enterprise.Rating.Business.IRateLine"">   at Enterprise.Rating.Business.FreightAutoRater.Calculate(AutoRatingCalculatorParameters parameters, IRateLine line)</Call>
			<Call Assembly=""Enterprise.Rating.Business.dll"" Type=""Enterprise.Rating.Business.FreightAutoRater"" Method=""CalculateBy"" ILOffset=""149"" Parameters=""Enterprise.Rating.Business.AutoRatingCalculatorParameters;Enterprise.Rating.Business.IRateLine;Enterprise.MasterFiles.Business.MeasureDimension[]"">   at Enterprise.Rating.Business.FreightAutoRater.CalculateBy(AutoRatingCalculatorParameters Parameters, IRateLine Line, MeasureDimension[] filterDimensions)</Call>
			<Call Assembly=""Enterprise.Rating.Business.dll"" Type=""Enterprise.Rating.Business.FreightAutoRater"" Method=""Calculate"" ILOffset=""66"" Parameters=""Enterprise.Rating.Business.AutoRatingCalculatorParameters"">   at Enterprise.Rating.Business.FreightAutoRater.Calculate(AutoRatingCalculatorParameters parameters)</Call>
			<Call Assembly=""Enterprise.Rating.Business.dll"" Type=""Enterprise.Rating.Business.FreightAutoRater"" Method=""AutoRate"" ILOffset=""205"" Parameters=""Enterprise.Rating.Business.RatingCriteria;Enterprise.MasterFiles.Business.CostSell;System.Boolean"">   at Enterprise.Rating.Business.FreightAutoRater.AutoRate(RatingCriteria mainCriteria, CostSell costOrSell, Boolean searchForAdditionalRates)</Call>
			<Call Assembly=""Enterprise.Rating.Business.dll"" Type=""Enterprise.Rating.Business.AutoRater"" Method=""AutoRate"" ILOffset=""65"" Parameters=""CargoWise.EntityFramework.BusinessObjectFactory;Enterprise.MasterFiles.Business.IAutoRating;Enterprise.Accounting.Integration.IAutoRatingAccountingInfo;Enterprise.MasterFiles.Business.CostSell;Enterprise.Rating.Business.IRatingContext"">   at Enterprise.Rating.Business.AutoRater.AutoRate(BusinessObjectFactory factory, IAutoRating itemToRate, IAutoRatingAccountingInfo accountingInfo, CostSell costOrSell, IRatingContext ratingContext)</Call>
			<Call Assembly=""Enterprise.Accounting.Business.dll"" Type=""Enterprise.Accounting.Business.JobInvoicing.AutoRatingRunner"" Method=""RateItem"" ILOffset=""16"" Parameters=""Enterprise.MasterFiles.Business.IAutoRating;Enterprise.Accounting.Integration.IAutoRatingAccountingInfo;Enterprise.MasterFiles.Business.CostSell"">   at Enterprise.Accounting.Business.JobInvoicing.AutoRatingRunner.RateItem(IAutoRating itemToRate, IAutoRatingAccountingInfo accountingInfo, CostSell costOrSell)</Call>
			<Call Assembly=""Enterprise.Accounting.Business.dll"" Type=""Enterprise.Accounting.Business.JobInvoicing.AutoRatingRunner"" Method=""RetrieveAllCharges"" ILOffset=""156"" Parameters=""Enterprise.Accounting.Integration.IAutoRatingAccountingInfo;Enterprise.MasterFiles.Business.IAutoRating[];Enterprise.MasterFiles.Business.CostSell"">   at Enterprise.Accounting.Business.JobInvoicing.AutoRatingRunner.RetrieveAllCharges(IAutoRatingAccountingInfo accountingInfo, IAutoRating[] nonCustomsJobsToRate, CostSell costOrSell)</Call>
			<Call Assembly=""Enterprise.Accounting.Business.dll"" Type=""Enterprise.Accounting.Business.AutoRatingStarterCore"" Method=""RateCharges"" ILOffset=""326"" Parameters=""Enterprise.MasterFiles.Business.CostSell;Enterprise.Accounting.Business.IAutoRatingStrategy"">   at Enterprise.Accounting.Business.AutoRatingStarterCore.RateCharges(CostSell costOrSell, IAutoRatingStrategy strategy)</Call>
			<Call Assembly=""Enterprise.Accounting.Business.dll"" Type=""Enterprise.Accounting.Business.AutoRatingStarterCore"" Method=""ExecuteAutorating"" ILOffset=""115"" Parameters=""Enterprise.MasterFiles.Business.CostSell"">   at Enterprise.Accounting.Business.AutoRatingStarterCore.ExecuteAutorating(CostSell costOrSell)</Call>
			<Call Assembly=""Enterprise.Accounting.Business.dll"" Type=""Enterprise.Accounting.Business.AutoRatingStarterCore"" Method=""ExecuteAutorating"" ILOffset=""122"" Parameters=""System.Boolean;System.Boolean"">   at Enterprise.Accounting.Business.AutoRatingStarterCore.ExecuteAutorating(Boolean autorateRevenue, Boolean autorateCosts)</Call>
			<Call Assembly=""Enterprise.Accounting.Business.dll"" Type=""Enterprise.Accounting.Business.AutoRatingStarter"" Method=""ExecuteTargets"" ILOffset=""138"" Parameters=""Enterprise.Rating.Business.IAutoRatingGUIInteractor;System.Collections.Generic.IEnumerable`1[CargoWise.EntityFramework.IBusiness];System.Boolean;System.Boolean;System.Boolean"">   at Enterprise.Accounting.Business.AutoRatingStarter.ExecuteTargets(IAutoRatingGUIInteractor interactorToUse, IEnumerable`1 targets, Boolean autorateRevenue, Boolean autorateCosts, Boolean includeAutoRatingExplorer)</Call>
			<Call Assembly=""Enterprise.Accounting.Business.dll"" Type=""Enterprise.Accounting.Business.AutoRatingStarter"" Method=""ExecuteAutorating"" ILOffset=""158"" Parameters=""System.Boolean;System.Boolean;System.Boolean"">   at Enterprise.Accounting.Business.AutoRatingStarter.ExecuteAutorating(Boolean autorateRevenue, Boolean autorateCosts, Boolean includeAutoRatingExplorer)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.MenuItem"" Method=""OnClick"" ILOffset=""104"" Parameters=""System.EventArgs"">   at System.Windows.Forms.MenuItem.OnClick(EventArgs e)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZMenuItem"" Method=""OnClick"" ILOffset=""152"" Parameters=""System.EventArgs"">   at Enterprise.ZArchitecture.GUI.ZMenuItem.OnClick(EventArgs e)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.MenuItem.MenuItemData"" Method=""Execute"" ILOffset=""24"" Parameters=""NoParameters"">   at System.Windows.Forms.MenuItem.MenuItemData.Execute()</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Command"" Method=""Invoke"" ILOffset=""28"" Parameters=""NoParameters"">   at System.Windows.Forms.Command.Invoke()</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Control"" Method=""WmCommand"" ILOffset=""59"" Parameters=""System.Windows.Forms.Message&amp;"">   at System.Windows.Forms.Control.WmCommand(Message&amp; m)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Control"" Method=""WndProc"" ILOffset=""866"" Parameters=""System.Windows.Forms.Message&amp;"">   at System.Windows.Forms.Control.WndProc(Message&amp; m)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Form"" Method=""WndProc"" ILOffset=""720"" Parameters=""System.Windows.Forms.Message&amp;"">   at System.Windows.Forms.Form.WndProc(Message&amp; m)</Call>
			<Call Assembly=""CargoWise.Windows.UI.dll"" Type=""CargoWise.Windows.UI.KForm"" Method=""WndProc"" ILOffset=""220"" Parameters=""System.Windows.Forms.Message&amp;"">   at CargoWise.Windows.UI.KForm.WndProc(Message&amp; m)</Call>
			<Call Assembly=""Enterprise.ZArchitecture.GUI.dll"" Type=""Enterprise.ZArchitecture.GUI.ZForm"" Method=""WndProc"" ILOffset=""376"" Parameters=""System.Windows.Forms.Message&amp;"">   at Enterprise.ZArchitecture.GUI.ZForm.WndProc(Message&amp; m)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.NativeWindow"" Method=""Callback"" ILOffset=""37"" Parameters=""System.IntPtr;System.Int32;System.IntPtr;System.IntPtr"">   at System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)</Call>
			<Call>   at System.Windows.Forms.UnsafeNativeMethods.DispatchMessageW(MSG&amp; msg)</Call>
			<Call>   at System.Windows.Forms.UnsafeNativeMethods.DispatchMessageW(MSG&amp; msg)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Application.ComponentManager"" Method=""System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.FPushMessageLoop"" ILOffset=""552"" Parameters=""System.IntPtr;System.Int32;System.Int32"">   at System.Windows.Forms.Application.ComponentManager.System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.FPushMessageLoop(IntPtr dwComponentID, Int32 reason, Int32 pvLoopData)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Application.ThreadContext"" Method=""RunMessageLoopInner"" ILOffset=""484"" Parameters=""System.Int32;System.Windows.Forms.ApplicationContext"">   at System.Windows.Forms.Application.ThreadContext.RunMessageLoopInner(Int32 reason, ApplicationContext context)</Call>
			<Call Assembly=""System.Windows.Forms.dll"" Type=""System.Windows.Forms.Application.ThreadContext"" Method=""RunMessageLoop"" ILOffset=""20"" Parameters=""System.Int32;System.Windows.Forms.ApplicationContext"">   at System.Windows.Forms.Application.ThreadContext.RunMessageLoop(Int32 reason, ApplicationContext context)</Call>
			<Call Assembly=""CargoWise.WindowsDesktop.exe"" Type=""Enterprise.Startup.ApplicationStartupDirector"" Method=""StartEnterprise"" ILOffset=""49"" Parameters=""System.String[]"">   at Enterprise.Startup.ApplicationStartupDirector.StartEnterprise(String[] args)</Call>
			<Call Assembly=""CargoWise.WindowsDesktop.exe"" Type=""Enterprise.Startup.ApplicationStartupDirector"" Method=""Main"" ILOffset=""36"" Parameters=""System.String[]"">   at Enterprise.Startup.ApplicationStartupDirector.Main(String[] args)</Call>");

			AddPublishedAssembly("CargoWise.Common.dll", "$/Dev/Common/Architecture/Common");
			AddPublishedAssembly("CargoWise.EntityFramework.dll", "$/Dev/Common/Architecture/EntityFramework");
			AddPublishedAssembly("Enterprise.MasterFiles.Business.dll", "$/Dev/Enterprise/Product/Operations/MasterFiles/Business");
			AddPublishedAssembly("Enterprise.Accounting.Business.dll", "$/Dev/Enterprise/Product/Operations/Accounting/Business");
			AddPublishedAssembly("Enterprise.Rating.Business.dll", "$/Dev/Enterprise/Product/Operations/Rating/Business");
			AddPublishedAssembly("Enterprise.ZArchitecture.GUI.dll", "$/Dev/Enterprise/Architecture/GUI");
			AddPublishedAssembly("CargoWise.Windows.UI.dll", "$/Dev/Common/Architecture/Windows.UI");
			AddPublishedAssembly("CargoWise.WindowsDesktop.exe", "$/Dev/Enterprise/Product/Main/Enterprise.Main");

			AddSourceTreeResponsibility("$/Dev/Common", "ENT", "ARC", "ARC");
			AddSourceTreeResponsibility("$/Dev/Enterprise/Product/Operations/MasterFiles", "ENT", "ARC", "COR");
			AddSourceTreeResponsibility("$/Dev/Enterprise/Product/Operations/Accounting", "ENT", "FIN", "ACC");
			AddSourceTreeResponsibility("$/Dev/Enterprise/Product/Operations/Rating", "ENT", "RAT", "SAL");
			AddSourceTreeResponsibility("$/Dev/Enterprise/Architecture", "ENT", "ARC", "ARC");
			AddSourceTreeResponsibility("$/Dev/Enterprise/Product/Main", "ENT", "ARC", "COR");

			EDIDataRegistry.Instance.StackLineCountNumberOfImportedLogs = 208477;

			AddStackLineCount("Enterprise.MasterFiles.Business.dll", "Enterprise.MasterFiles.Business.AutoJobCharge.get_JR_AC()", 26);
			AddStackLineCount("Enterprise.Rating.Business.dll", "Enterprise.Rating.Business.DependentCalculatorExtensions.GetValueCalculatorAppliesTo(IDependentCalculator calculator, AutoRatingCalculatorParameters parameters, IRateLineItem& greaterChargeItem, Boolean includeGST, Boolean useGreaterCharge)", 14);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Application.ComponentManager.System.Windows.Forms.UnsafeNativeMethods.IMsoComponentManager.FPushMessageLoop(IntPtr dwComponentID, Int32 reason, Int32 pvLoopData)", 101654);
			AddStackLineCount("Enterprise.Rating.Business.dll", "Enterprise.Rating.Business.Calculator.Calculate(AutoRatingCalculatorParameters parameters, IRateLine contextLine)", 87);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Command.Invoke()", 15891);
			AddStackLineCount("System.Data.dll", "System.Data.DataRow.GetDefaultRecord()", 2714);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Control.WmCommand(Message& m)", 16019);
			AddStackLineCount("Enterprise.Accounting.Business.dll", "Enterprise.Accounting.Business.AutoRatingStarterCore.ExecuteAutorating(CostSell costOrSell)", 696);
			AddStackLineCount("", "System.Windows.Forms.UnsafeNativeMethods.DispatchMessageW(MSG& msg)", 114554);
			AddStackLineCount("Enterprise.Accounting.Business.dll", "Enterprise.Accounting.Business.AutoRatingStarterCore.ExecuteAutorating(Boolean autorateRevenue, Boolean autorateCosts)", 725);
			AddStackLineCount("CargoWise.Windows.UI.dll", "CargoWise.Windows.UI.KForm.WndProc(Message& m)", 20002);
			AddStackLineCount("Enterprise.Accounting.Business.dll", "Enterprise.Accounting.Business.JobInvoicing.AutoRatingRunner.RetrieveAllCharges(IAutoRatingAccountingInfo accountingInfo, IAutoRating[] nonCustomsJobsToRate, CostSell costOrSell)", 280);
			AddStackLineCount("Enterprise.Rating.Business.dll", "Enterprise.Rating.Business.DependentCalculatorExtensions.MergeAutoRateInfosWithExistingJobCharges(IRateLineItem applyToItem, AutoRatingCalculatorParameters parameters)", 7);
			AddStackLineCount("mscorlib.dll", "System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)", 136622);
			AddStackLineCount("Enterprise.Rating.Business.dll", "Enterprise.Rating.Business.PercentageCalculator.CalculateInternal(AutoRatingCalculatorParameters parameters, CalculationResult calculationResult, DescriptionList description)", 28);
			AddStackLineCount("System.Data.dll", "System.Data.DataRow.get_Item(DataColumn column, DataRowVersion version)", 2201);
			AddStackLineCount("Enterprise.Rating.Business.dll", "Enterprise.Rating.Business.FreightAutoRater.Calculate(AutoRatingCalculatorParameters parameters)", 115);
			AddStackLineCount("Enterprise.Accounting.Business.dll", "Enterprise.Accounting.Business.AutoRatingStarter.ExecuteAutorating(Boolean autorateRevenue, Boolean autorateCosts, Boolean includeAutoRatingExplorer)", 11);
			AddStackLineCount("Enterprise.Rating.Business.dll", "Enterprise.Rating.Business.AutoRater.AutoRate(BusinessObjectFactory factory, IAutoRating itemToRate, IAutoRatingAccountingInfo accountingInfo, CostSell costOrSell, IRatingContext ratingContext)", 78);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZMenuItem.OnClick(EventArgs e)", 20640);
			AddStackLineCount("Enterprise.Rating.Business.dll", "Enterprise.Rating.Business.FreightAutoRater.Calculate(AutoRatingCalculatorParameters parameters, IRateLine line)", 93);
			AddStackLineCount("Enterprise.Rating.Business.dll", "Enterprise.Rating.Business.FreightAutoRater.CalculateBy(AutoRatingCalculatorParameters Parameters, IRateLine Line, MeasureDimension[] filterDimensions)", 93);
			AddStackLineCount("Enterprise.Rating.Business.dll", "Enterprise.Rating.Business.FreightAutoRater.AutoRate(RatingCriteria mainCriteria, CostSell costOrSell, Boolean searchForAdditionalRates)", 233);
			AddStackLineCount("Enterprise.Accounting.Business.dll", "Enterprise.Accounting.Business.AutoRatingStarterCore.RateCharges(CostSell costOrSell, IAutoRatingStrategy strategy)", 696);
			AddStackLineCount("CargoWise.WindowsDesktop.exe", "Enterprise.Startup.ApplicationStartupDirector.Main(String[] args)", 80618);
			AddStackLineCount("Enterprise.MasterFiles.Business.dll", "Enterprise.MasterFiles.Business.AutoJobCharge.get_ChargeCode()", 56);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObject.ReportRowDeletedError(String columnName, Exception ex, DataRowVersion versionToUse, DataRowVersion version)", 1631);
			AddStackLineCount("Enterprise.Accounting.Business.dll", "Enterprise.Accounting.Business.JobInvoicing.Charge.get_JR_AC()", 63);
			AddStackLineCount("mscorlib.dll", "System.Environment.get_StackTrace()", 136470);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Control.WndProc(Message& m)", 104261);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Form.WndProc(Message& m)", 28546);
			AddStackLineCount("Enterprise.Rating.Business.dll", "Enterprise.Rating.Business.DependentCalculatorExtensions.IsApplicable(IRateLineItem applyToItem, IAutoRatingChargeInfo charge, AutoRatingCalculatorParameters parameters)", 2);
			AddStackLineCount("Enterprise.ZArchitecture.GUI.dll", "Enterprise.ZArchitecture.GUI.ZForm.WndProc(Message& m)", 27230);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObject.ReportRowError(String columnName, Exception ex, DataRowVersion versionToUse, DataRowVersion version, String message)", 2773);
			AddStackLineCount("mscorlib.dll", "System.Collections.Generic.List`1[T].InsertRange(Int32 index, IEnumerable`1 collection)", 962);
			AddStackLineCount("Enterprise.Accounting.Business.dll", "Enterprise.Accounting.Business.JobInvoicing.AutoRatingRunner.RateItem(IAutoRating itemToRate, IAutoRatingAccountingInfo accountingInfo, CostSell costOrSell)", 268);
			AddStackLineCount("System.Core.dll", "System.Linq.Enumerable.WhereArrayIterator`1[TSource].MoveNext()", 586);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.MenuItem.OnClick(EventArgs e)", 19783);
			AddStackLineCount("Enterprise.Accounting.Business.dll", "Enterprise.Accounting.Business.AutoRatingStarter.ExecuteTargets(IAutoRatingGUIInteractor interactorToUse, IEnumerable`1 targets, Boolean autorateRevenue, Boolean autorateCosts, Boolean includeAutoRatingExplorer)", 11);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Application.ThreadContext.RunMessageLoopInner(Int32 reason, ApplicationContext context)", 102482);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.Application.ThreadContext.RunMessageLoop(Int32 reason, ApplicationContext context)", 102480);
			AddStackLineCount("CargoWise.Common.dll", "CargoWise.Common.ErrorReporter.ReportOnce(String key, String message, Exception exception)", 66030);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.MenuItem.MenuItemData.Execute()", 15193);
			AddStackLineCount("System.Windows.Forms.dll", "System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)", 145005);
			AddStackLineCount("CargoWise.WindowsDesktop.exe", "Enterprise.Startup.ApplicationStartupDirector.StartEnterprise(String[] args)", 80620);
			AddStackLineCount("Enterprise.Rating.Business.dll", "Enterprise.Rating.Business.DependentCalculatorExtensions.GetApplicableMoneyForItem(IRateLineItem applyToItem, Boolean includeGST, AutoRatingCalculatorParameters parameters)", 10);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObject.GetValueFromRowSafely(DataColumn Column, DataRowVersion Version)", 6251);
			AddStackLineCount("Enterprise.Rating.Business.dll", "Enterprise.Rating.Business.DependentCalculatorExtensions.GetAmountItemAppliesTo(IRateLineItem applyToItem, Boolean includeGST, AutoRatingCalculatorParameters parameters)", 14);

			AssertLogAssignment(new IssueAssignment("ENT", "RAT", "SAL"), report);
		}

		public void TestRealData_ClientSpecificServiceTaskIssue00227553()
		{
			// working fine in initial implementation, added for regression testing

			var report = GetExceptionXml(@"
			<Call>----- Exception caught and reported here -----</Call>
			<Call>   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)</Call>
			<Call>   at System.Environment.get_StackTrace()</Call>
			<Call Assembly=""CargoWise.Common.dll"" Type=""CargoWise.Common.ErrorReporter"" Method=""ReportOnce"" ILOffset=""318"" Parameters=""System.String;System.String;System.Exception"">   at CargoWise.Common.ErrorReporter.ReportOnce(String key, String message, Exception exception)</Call>
			<Call Assembly=""ZClientUPE.dll"" Type=""Enterprise.Client.UPE.Business.UPEDocumentAutoDelivery"" Method=""Deliver"" ILOffset=""81"" Parameters=""NoParameters"">   at Enterprise.Client.UPE.Business.UPEDocumentAutoDelivery.Deliver()</Call>
			<Call Assembly=""ZClientUPE.dll"" Type=""Enterprise.Client.UPE.Business.Callout"" Method=""AutoDeliverDocumentsOnBISIDownload"" ILOffset=""19"" Parameters=""NoParameters"">   at Enterprise.Client.UPE.Business.Callout.AutoDeliverDocumentsOnBISIDownload()</Call>
			<Call Assembly=""ZClientUPE.dll"" Type=""Enterprise.Client.UPE.Business.Callout"" Method=""OnFactorySaved"" ILOffset=""24"" Parameters=""System.Boolean"">   at Enterprise.Client.UPE.Business.Callout.OnFactorySaved(Boolean SaveSucceeded)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObject"" Method=""OnFactorySavedInternal"" ILOffset=""55"" Parameters=""System.Boolean"">   at CargoWise.EntityFramework.BusinessObject.OnFactorySavedInternal(Boolean saveSucceeded)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObjectFactory"" Method=""CallBusinessObjectsOnFactorySaved"" ILOffset=""66"" Parameters=""System.Boolean"">   at CargoWise.EntityFramework.BusinessObjectFactory.CallBusinessObjectsOnFactorySaved(Boolean saveSucceded)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObjectFactory"" Method=""CargoWise.Integration.ITransactionParticipant.OnAllTransactionsCommitted"" ILOffset=""113"" Parameters=""CargoWise.Integration.IChangedTableNames"">   at CargoWise.EntityFramework.BusinessObjectFactory.CargoWise.Integration.ITransactionParticipant.OnAllTransactionsCommitted(IChangedTableNames changedTableNames)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.TransactionCoordinator.TransactionManager"" Method=""CommitTransaction"" ILOffset=""36"" Parameters=""CargoWise.EntityFramework.SaveInTransactionResult"">   at CargoWise.EntityFramework.TransactionCoordinator.TransactionManager.CommitTransaction(SaveInTransactionResult saveInTransactionResult)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.RowFactory"" Method=""SaveTogether"" ILOffset=""342"" Parameters=""CargoWise.EntityFramework.TransactionCoordinator"">   at CargoWise.EntityFramework.RowFactory.SaveTogether(TransactionCoordinator coordinator)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObjectFactory"" Method=""SaveTogether"" ILOffset=""29"" Parameters=""CargoWise.Integration.ITransactionParticipant[]"">   at CargoWise.EntityFramework.BusinessObjectFactory.SaveTogether(ITransactionParticipant[] Factories)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObjectFactory"" Method=""SaveCore"" ILOffset=""28"" Parameters=""NoParameters"">   at CargoWise.EntityFramework.BusinessObjectFactory.SaveCore()</Call>
			<Call Assembly=""ZClientUPE.dll"" Type=""Enterprise.Client.UPE.Business.BISI.PWSFileImporter"" Method=""Import"" ILOffset=""79"" Parameters=""System.String;CargoWise.ComponentModel.INotifications"">   at Enterprise.Client.UPE.Business.BISI.PWSFileImporter.Import(String FileName, INotifications Notifications)</Call>
			<Call Assembly=""ZClientUPE.dll"" Type=""Enterprise.Client.UPE.ServiceTask.BISIDownloadServiceTask"" Method=""ImportCODFiles"" ILOffset=""29"" Parameters=""NoParameters"">   at Enterprise.Client.UPE.ServiceTask.BISIDownloadServiceTask.ImportCODFiles()</Call>
			<Call Assembly=""ZClientUPE.dll"" Type=""Enterprise.Client.UPE.ServiceTask.BISIDownloadServiceTask"" Method=""DownloadAndProcessBISIZipFiles"" ILOffset=""39"" Parameters=""CargoWise.EntityFramework.BusinessObjectFactoryProvider"">   at Enterprise.Client.UPE.ServiceTask.BISIDownloadServiceTask.DownloadAndProcessBISIZipFiles(BusinessObjectFactoryProvider factoryProvider)</Call>
			<Call Assembly=""ZClientUPE.dll"" Type=""Enterprise.Client.UPE.ServiceTask.BISIDownloadServiceTask"" Method=""RunTask"" ILOffset=""119"" Parameters=""NoParameters"">   at Enterprise.Client.UPE.ServiceTask.BISIDownloadServiceTask.RunTask()</Call>
			<Call Assembly=""Enterprise.ServiceManager.Business.dll"" Type=""Enterprise.ServiceManager.Business.ServiceProviderImplProxy"" Method=""RunTask"" ILOffset=""11"" Parameters=""NoParameters"">   at Enterprise.ServiceManager.Business.ServiceProviderImplProxy.RunTask()</Call>
			<Call Assembly=""Enterprise.ServiceManager.Business.dll"" Type=""Enterprise.ServiceManager.Business.BasicServiceProvider"" Method=""Enterprise.ServiceManager.Shared.IServiceTaskHandler.Run"" ILOffset=""62"" Parameters=""NoParameters"">   at Enterprise.ServiceManager.Business.BasicServiceProvider.Enterprise.ServiceManager.Shared.IServiceTaskHandler.Run()</Call>
			<Call Assembly=""CargoWiseOne.ServiceManager.Runner.x86.exe"" Type=""Enterprise.ServiceManager.Runner.IpcRunner"" Method=""RunInternal"" ILOffset=""856"" Parameters=""System.Boolean"">   at Enterprise.ServiceManager.Runner.IpcRunner.RunInternal(Boolean singleRun)</Call>
			<Call Assembly=""CargoWiseOne.ServiceManager.Runner.x86.exe"" Type=""Enterprise.ServiceManager.Runner.TaskRunner.&lt;&gt;c__DisplayClass0_0"" Method=""&lt;Run&gt;b__0"" ILOffset=""6"" Parameters=""NoParameters"">   at Enterprise.ServiceManager.Runner.TaskRunner.&lt;&gt;c__DisplayClass0_0.&lt;Run&gt;b__0()</Call>
			<Call Assembly=""mscorlib.dll"" Type=""System.Threading.ThreadHelper"" Method=""ThreadStart_Context"" ILOffset=""59"" Parameters=""System.Object"">   at System.Threading.ThreadHelper.ThreadStart_Context(Object state)</Call>
			<Call Assembly=""mscorlib.dll"" Type=""System.Threading.ExecutionContext"" Method=""RunInternal"" ILOffset=""121"" Parameters=""System.Threading.ExecutionContext;System.Threading.ContextCallback;System.Object;System.Boolean"">   at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state, Boolean preserveSyncCtx)</Call>
			<Call Assembly=""mscorlib.dll"" Type=""System.Threading.ExecutionContext"" Method=""Run"" ILOffset=""9"" Parameters=""System.Threading.ExecutionContext;System.Threading.ContextCallback;System.Object;System.Boolean"">   at System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state, Boolean preserveSyncCtx)</Call>
			<Call Assembly=""mscorlib.dll"" Type=""System.Threading.ExecutionContext"" Method=""Run"" ILOffset=""52"" Parameters=""System.Threading.ExecutionContext;System.Threading.ContextCallback;System.Object"">   at System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state)</Call>
			<Call Assembly=""mscorlib.dll"" Type=""System.Threading.ThreadHelper"" Method=""ThreadStart"" ILOffset=""42"" Parameters=""NoParameters"">   at System.Threading.ThreadHelper.ThreadStart()</Call>");

			AddPublishedAssembly("CargoWise.Common.dll", "$/Dev/Common/Architecture/Common");
			AddPublishedAssembly("CargoWise.EntityFramework.dll", "$/Dev/Common/Architecture/EntityFramework");
			AddPublishedAssembly("ZClientUPE.dll", "$/Dev/Enterprise/ClientExtensions/UPE/ZClientUPE");
			AddPublishedAssembly("Enterprise.ServiceManager.Business.dll", "$/Dev/Enterprise/Product/Core/ServiceManager/ServiceManager/ServiceManager.Business");
			AddPublishedAssembly("CargoWiseOne.ServiceManager.Runner.x86.exe", "$/Dev/Enterprise/Product/Core/ServiceManager/ServiceManager/ServiceManager.Runner");

			AddSourceTreeResponsibility("$/Dev/Common", "ENT", "ARC", "ARC");
			AddSourceTreeResponsibility("$/Dev/Enterprise/ClientExtensions", "ENT", "", "CPJ");
			AddSourceTreeResponsibility("$/Dev/Enterprise/Product/Core/ServiceManager/ServiceManager", "ENT", "ARC", "COR");

			EDIDataRegistry.Instance.StackLineCountNumberOfImportedLogs = 208477;

			AddStackLineCount("Enterprise.ServiceManager.Business.dll", "Enterprise.ServiceManager.Business.BasicServiceProvider.Enterprise.ServiceManager.Shared.IServiceTaskHandler.Run()", 24141);
			AddStackLineCount("mscorlib.dll", "System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state, Boolean preserveSyncCtx)", 42733);
			AddStackLineCount("Enterprise.ServiceManager.Business.dll", "Enterprise.ServiceManager.Business.ServiceProviderImplProxy.RunTask()", 12558);
			AddStackLineCount("mscorlib.dll", "System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)", 136622);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObjectFactory.SaveCore()", 17147);
			AddStackLineCount("mscorlib.dll", "System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state)", 26427);
			AddStackLineCount("mscorlib.dll", "System.Threading.ThreadHelper.ThreadStart()", 19914);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.TransactionCoordinator.TransactionManager.CommitTransaction(SaveInTransactionResult saveInTransactionResult)", 350);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObjectFactory.CallBusinessObjectsOnFactorySaved(Boolean saveSucceded)", 489);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObjectFactory.SaveTogether(ITransactionParticipant[] Factories)", 37582);
			AddStackLineCount("mscorlib.dll", "System.Environment.get_StackTrace()", 136470);
			AddStackLineCount("CargoWiseOne.ServiceManager.Runner.exe", "Enterprise.ServiceManager.Runner.TaskRunner.<>c__DisplayClass0_0.<Run>b__0()", 15200);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObjectFactory.CargoWise.Integration.ITransactionParticipant.OnAllTransactionsCommitted(IChangedTableNames changedTableNames)", 2314);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.RowFactory.SaveTogether(TransactionCoordinator coordinator)", 37776);
			AddStackLineCount("mscorlib.dll", "System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state, Boolean preserveSyncCtx)", 42739);
			AddStackLineCount("mscorlib.dll", "System.Threading.ThreadHelper.ThreadStart_Context(Object state)", 8300);
			AddStackLineCount("CargoWise.Common.dll", "CargoWise.Common.ErrorReporter.ReportOnce(String key, String message, Exception exception)", 66030);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObject.OnFactorySavedInternal(Boolean saveSucceeded)", 477);
			AddStackLineCount("CargoWiseOne.ServiceManager.Runner.exe", "Enterprise.ServiceManager.Runner.IpcRunner.RunInternal(Boolean singleRun)", 27326);

			AssertLogAssignment(new IssueAssignment("ENT", "", "CPJ"), report);
		}

		public void TestRealData_PaveIssue01046755()
		{
			// working fine in initial implementation, added for regression testing

			var report = GetExceptionXml(@"
			<Call Assembly=""System.Data.dll"" Type=""System.Data.DataRow"" Method=""GetDefaultRecord"" ILOffset=""47"" Parameters=""NoParameters"">   at System.Data.DataRow.GetDefaultRecord()</Call>
			<Call Assembly=""System.Data.dll"" Type=""System.Data.DataRow"" Method=""get_Item"" ILOffset=""0"" Parameters=""System.Data.DataColumn;System.Data.DataRowVersion"">   at System.Data.DataRow.get_Item(DataColumn column, DataRowVersion version)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObject"" Method=""GetValueFromRowSafely"" ILOffset=""29"" Parameters=""System.Data.DataColumn;System.Data.DataRowVersion"">   at CargoWise.EntityFramework.BusinessObject.GetValueFromRowSafely(DataColumn column, DataRowVersion version)</Call>
			<Call>----- Exception caught and reported here -----</Call>
			<Call>   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)</Call>
			<Call>   at System.Environment.get_StackTrace()</Call>
			<Call Assembly=""CargoWise.Common.dll"" Type=""CargoWise.Common.ErrorReporter"" Method=""ReportOnce"" ILOffset=""318"" Parameters=""System.String;System.String;System.Exception"">   at CargoWise.Common.ErrorReporter.ReportOnce(String key, String message, Exception exception)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObject"" Method=""ReportRowError"" ILOffset=""372"" Parameters=""System.String;System.Exception;System.Data.DataRowVersion;System.Data.DataRowVersion;System.String"">   at CargoWise.EntityFramework.BusinessObject.ReportRowError(String columnName, Exception ex, DataRowVersion versionToUse, DataRowVersion version, String message)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObject"" Method=""ReportRowDeletedError"" ILOffset=""16"" Parameters=""System.String;System.Exception;System.Data.DataRowVersion;System.Data.DataRowVersion"">   at CargoWise.EntityFramework.BusinessObject.ReportRowDeletedError(String columnName, Exception ex, DataRowVersion versionToUse, DataRowVersion version)</Call>
			<Call Assembly=""CargoWise.EntityFramework.dll"" Type=""CargoWise.EntityFramework.BusinessObject"" Method=""GetValueFromRowSafely"" ILOffset=""29"" Parameters=""System.Data.DataColumn;System.Data.DataRowVersion"">   at CargoWise.EntityFramework.BusinessObject.GetValueFromRowSafely(DataColumn column, DataRowVersion version)</Call>
			<Call Assembly=""Enterprise.BufferManagement.Business.dll"" Type=""Enterprise.BufferManagement.Business.AutoProcessHeader"" Method=""get_FH_SystemLastEditTimeUtc"" ILOffset=""0"" Parameters=""NoParameters"">   at Enterprise.BufferManagement.Business.AutoProcessHeader.get_FH_SystemLastEditTimeUtc()</Call>
			<Call Assembly=""Enterprise.BufferManagement.Business.dll"" Type=""Enterprise.BufferManagement.Business.WorkflowStartingComponentCache"" Method=""NotifyBatchFinishedProcessing"" ILOffset=""42"" Parameters=""NoParameters"">   at Enterprise.BufferManagement.Business.WorkflowStartingComponentCache.NotifyBatchFinishedProcessing()</Call>
			<Call Assembly=""Enterprise.BufferManagement.Business.dll"" Type=""Enterprise.BufferManagement.Business.TransferRuleRunnerBase"" Method=""ProcessLink"" ILOffset=""395"" Parameters=""Enterprise.BufferManagement.Business.BMComponentLink;Enterprise.BufferManagement.Business.WorkflowStartingComponentCache"">   at Enterprise.BufferManagement.Business.TransferRuleRunnerBase.ProcessLink(BMComponentLink link, WorkflowStartingComponentCache startingComponentCache)</Call>
			<Call Assembly=""Enterprise.BufferManagement.Business.dll"" Type=""Enterprise.BufferManagement.Business.TransferRuleRunnerBase"" Method=""Process"" ILOffset=""120"" Parameters=""System.Threading.CancellationToken"">   at Enterprise.BufferManagement.Business.TransferRuleRunnerBase.Process(CancellationToken token)</Call>
			<Call Assembly=""Enterprise.BufferManagement.Business.dll"" Type=""Enterprise.BufferManagement.Business.ReleaseGateKeeper"" Method=""GetWorkflowsEligibleForRelease"" ILOffset=""23"" Parameters=""System.Threading.CancellationToken"">   at Enterprise.BufferManagement.Business.ReleaseGateKeeper.GetWorkflowsEligibleForRelease(CancellationToken token)</Call>
			<Call Assembly=""Enterprise.BufferManagement.Business.dll"" Type=""Enterprise.BufferManagement.Business.ReleaseGateKeeper"" Method=""GetRequestsByWorkflowPK"" ILOffset=""5"" Parameters=""System.Threading.CancellationToken"">   at Enterprise.BufferManagement.Business.ReleaseGateKeeper.GetRequestsByWorkflowPK(CancellationToken token)</Call>
			<Call Assembly=""Enterprise.BufferManagement.Business.dll"" Type=""Enterprise.BufferManagement.Business.ReleaseGateKeeper"" Method=""Process"" ILOffset=""55"" Parameters=""System.Threading.CancellationToken"">   at Enterprise.BufferManagement.Business.ReleaseGateKeeper.Process(CancellationToken token)</Call>
			<Call Assembly=""Enterprise.BufferManagement.Business.dll"" Type=""Enterprise.BufferManagement.Business.ReleaseGateDirector.&lt;&gt;c__DisplayClass10_0"" Method=""&lt;Process&gt;b__2"" ILOffset=""145"" Parameters=""Enterprise.BufferManagement.Business.BMComponent"">   at Enterprise.BufferManagement.Business.ReleaseGateDirector.&lt;&gt;c__DisplayClass10_0.&lt;Process&gt;b__2(BMComponent buffer_unsafe)</Call>
			<Call Assembly=""CargoWise.Async.dll"" Type=""CargoWise.Async.DefaultAsyncStrategy.&lt;&gt;c__DisplayClass6_0`1[T]"" Method=""&lt;CargoWise.Async.IAsyncStrategy.ParallelForEach&gt;b__0"" ILOffset=""41"" Parameters=""T"">   at CargoWise.Async.DefaultAsyncStrategy.&lt;&gt;c__DisplayClass6_0`1[T].&lt;CargoWise.Async.IAsyncStrategy.ParallelForEach&gt;b__0(T item)</Call>
			<Call Assembly=""mscorlib.dll"" Type=""System.Threading.Tasks.Parallel.&lt;&gt;c__DisplayClass17_0`1[TLocal]"" Method=""&lt;ForWorker&gt;b__1"" ILOffset=""282"" Parameters=""NoParameters"">   at System.Threading.Tasks.Parallel.&lt;&gt;c__DisplayClass17_0`1[TLocal].&lt;ForWorker&gt;b__1()</Call>
			<Call Assembly=""mscorlib.dll"" Type=""System.Threading.Tasks.Task"" Method=""InnerInvokeWithArg"" ILOffset=""6"" Parameters=""System.Threading.Tasks.Task"">   at System.Threading.Tasks.Task.InnerInvokeWithArg(Task childTask)</Call>
			<Call Assembly=""mscorlib.dll"" Type=""System.Threading.Tasks.Task.&lt;&gt;c__DisplayClass176_0"" Method=""&lt;ExecuteSelfReplicating&gt;b__0"" ILOffset=""134"" Parameters=""System.Object"">   at System.Threading.Tasks.Task.&lt;&gt;c__DisplayClass176_0.&lt;ExecuteSelfReplicating&gt;b__0(Object )</Call>
			<Call Assembly=""mscorlib.dll"" Type=""System.Threading.Tasks.Task"" Method=""Execute"" ILOffset=""16"" Parameters=""NoParameters"">   at System.Threading.Tasks.Task.Execute()</Call>
			<Call Assembly=""mscorlib.dll"" Type=""System.Threading.ExecutionContext"" Method=""RunInternal"" ILOffset=""121"" Parameters=""System.Threading.ExecutionContext;System.Threading.ContextCallback;System.Object;System.Boolean"">   at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state, Boolean preserveSyncCtx)</Call>
			<Call Assembly=""mscorlib.dll"" Type=""System.Threading.ExecutionContext"" Method=""Run"" ILOffset=""9"" Parameters=""System.Threading.ExecutionContext;System.Threading.ContextCallback;System.Object;System.Boolean"">   at System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state, Boolean preserveSyncCtx)</Call>
			<Call Assembly=""mscorlib.dll"" Type=""System.Threading.Tasks.Task"" Method=""ExecuteWithThreadLocal"" ILOffset=""225"" Parameters=""System.Threading.Tasks.Task&amp;"">   at System.Threading.Tasks.Task.ExecuteWithThreadLocal(Task&amp; currentTaskSlot)</Call>
			<Call Assembly=""mscorlib.dll"" Type=""System.Threading.Tasks.Task"" Method=""ExecuteEntry"" ILOffset=""150"" Parameters=""System.Boolean"">   at System.Threading.Tasks.Task.ExecuteEntry(Boolean bPreventDoubleExecution)</Call>
			<Call Assembly=""mscorlib.dll"" Type=""System.Threading.ThreadPoolWorkQueue"" Method=""Dispatch"" ILOffset=""164"" Parameters=""NoParameters"">   at System.Threading.ThreadPoolWorkQueue.Dispatch()</Call>");

			AddPublishedAssembly("CargoWise.Common.dll", "$/Dev/Common/Architecture/Common");
			AddPublishedAssembly("CargoWise.EntityFramework.dll", "$/Dev/Common/Architecture/EntityFramework");
			AddPublishedAssembly("Enterprise.BufferManagement.Business.dll", "$/Dev/Enterprise/Product/Operations/BufferManagement/Business");
			AddPublishedAssembly("CargoWise.Async.dll", "$/Dev/Common/Architecture/Async/CargoWise.Async");

			AddSourceTreeResponsibility("$/Dev/Common", "ENT", "ARC", "ARC");
			AddSourceTreeResponsibility("$/Dev/Enterprise/Product/Operations/BufferManagement", "ENT", "PAV", "BUF");

			EDIDataRegistry.Instance.StackLineCountNumberOfImportedLogs = 208477;

			AddStackLineCount("Enterprise.BufferManagement.Business.dll", "Enterprise.BufferManagement.Business.ReleaseGateKeeper.Process(CancellationToken token)", 2);
			AddStackLineCount("System.Data.dll", "System.Data.DataRow.GetDefaultRecord()", 2714);
			AddStackLineCount("Enterprise.BufferManagement.Business.dll", "Enterprise.BufferManagement.Business.ReleaseGateDirector.<>c__DisplayClass10_0.<Process>b__2(BMComponent buffer_unsafe)", 15);
			AddStackLineCount("mscorlib.dll", "System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state, Boolean preserveSyncCtx)", 42733);
			AddStackLineCount("mscorlib.dll", "System.Threading.Tasks.Parallel.<>c__DisplayClass17_0`1[TLocal].<ForWorker>b__1()", 4542);
			AddStackLineCount("CargoWise.Async.dll", "CargoWise.Async.DefaultAsyncStrategy.<>c__DisplayClass6_0`1[T].<CargoWise.Async.IAsyncStrategy.ParallelForEach>b__0(T item)", 4466);
			AddStackLineCount("mscorlib.dll", "System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)", 136622);
			AddStackLineCount("System.Data.dll", "System.Data.DataRow.get_Item(DataColumn column, DataRowVersion version)", 2201);
			AddStackLineCount("mscorlib.dll", "System.Threading.Tasks.Task.ExecuteEntry(Boolean bPreventDoubleExecution)", 12176);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObject.ReportRowDeletedError(String columnName, Exception ex, DataRowVersion versionToUse, DataRowVersion version)", 1631);
			AddStackLineCount("Enterprise.BufferManagement.Business.dll", "Enterprise.BufferManagement.Business.WorkflowStartingComponentCache.NotifyBatchFinishedProcessing()", 2);
			AddStackLineCount("mscorlib.dll", "System.Threading.Tasks.Task.ExecuteWithThreadLocal(Task& currentTaskSlot)", 12177);
			AddStackLineCount("mscorlib.dll", "System.Environment.get_StackTrace()", 136470);
			AddStackLineCount("Enterprise.BufferManagement.Business.dll", "Enterprise.BufferManagement.Business.TransferRuleRunnerBase.Process(CancellationToken token)", 5);
			AddStackLineCount("mscorlib.dll", "System.Threading.Tasks.Task.Execute()", 12212);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObject.ReportRowError(String columnName, Exception ex, DataRowVersion versionToUse, DataRowVersion version, String message)", 2773);
			AddStackLineCount("mscorlib.dll", "System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state, Boolean preserveSyncCtx)", 42739);
			AddStackLineCount("Enterprise.BufferManagement.Business.dll", "Enterprise.BufferManagement.Business.TransferRuleRunnerBase.ProcessLink(BMComponentLink link, WorkflowStartingComponentCache startingComponentCache)", 219);
			AddStackLineCount("mscorlib.dll", "System.Threading.Tasks.Task.<>c__DisplayClass176_0.<ExecuteSelfReplicating>b__0(Object )", 4554);
			AddStackLineCount("mscorlib.dll", "System.Threading.Tasks.Task.InnerInvokeWithArg(Task childTask)", 5113);
			AddStackLineCount("CargoWise.Common.dll", "CargoWise.Common.ErrorReporter.ReportOnce(String key, String message, Exception exception)", 66030);
			AddStackLineCount("mscorlib.dll", "System.Threading.ThreadPoolWorkQueue.Dispatch()", 10485);
			AddStackLineCount("Enterprise.BufferManagement.Business.dll", "Enterprise.BufferManagement.Business.AutoProcessHeader.get_FH_SystemLastEditTimeUtc()", 2);
			AddStackLineCount("CargoWise.EntityFramework.dll", "CargoWise.EntityFramework.BusinessObject.GetValueFromRowSafely(DataColumn Column, DataRowVersion Version)", 6251);

			AssertLogAssignment(new IssueAssignment("ENT", "PAV", "BUF"), report);
		}

		#endregion

		public void TestGitTeamProjectCollectionResponsibility()
		{
			var report = GetExceptionXml(@"
				<Call Assembly=""MyAssembly.dll"">   at MyCode.MyClass.MyFunction()</Call>");
			AddStackLineCount("MyAssembly.dll", "MyCode.MyClass.MyFunction()", 1);
			AddPublishedAssembly("MyAssembly.dll", "http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject/_git/MyRepository?path=/mycode/myfunction");
			AddSourceTreeResponsibility("http://tfs.wtg.zone:8080/tfs/MyCollection", "A", "B", "C");
			AssertLogAssignment(new IssueAssignment("A", "B", "C"), report);
		}

		public void TestGitTeamProjectResponsibility()
		{
			var report = GetExceptionXml(@"
				<Call Assembly=""MyAssembly.dll"">   at MyCode.MyClass.MyFunction()</Call>");
			AddStackLineCount("MyAssembly.dll", "MyCode.MyClass.MyFunction()", 1);
			AddPublishedAssembly("MyAssembly.dll", "http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject/_git/MyRepository?path=/mycode/myfunction");
			AddSourceTreeResponsibility("http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject", "A", "B", "C");
			AssertLogAssignment(new IssueAssignment("A", "B", "C"), report);
		}

		public void TestGitRepositoryResponsibility()
		{
			var report = GetExceptionXml(@"
				<Call Assembly=""MyAssembly.dll"">   at MyCode.MyClass.MyFunction()</Call>");
			AddStackLineCount("MyAssembly.dll", "MyCode.MyClass.MyFunction()", 1);
			AddPublishedAssembly("MyAssembly.dll", "http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject/_git/MyRepository?path=/mycode/myfunction");
			AddSourceTreeResponsibility("http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject/_git/MyRepository", "A", "B", "C");
			AssertLogAssignment(new IssueAssignment("A", "B", "C"), report);
		}

		public void TestGitRepositoryPathResponsibility()
		{
			var report = GetExceptionXml(@"
				<Call Assembly=""MyAssembly.dll"">   at MyCode.MyClass.MyFunction()</Call>");
			AddStackLineCount("MyAssembly.dll", "MyCode.MyClass.MyFunction()", 1);
			AddPublishedAssembly("MyAssembly.dll", "http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject/_git/MyRepository?path=/mycode/myfunction");
			AddSourceTreeResponsibility("http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject/_git/MyRepository?path=/mycode", "A", "B", "C");
			AssertLogAssignment(new IssueAssignment("A", "B", "C"), report);
		}

		public void TestGitRepositoryPathResponsibilityExactMatch()
		{
			var report = GetExceptionXml(@"
				<Call Assembly=""MyAssembly.dll"">   at MyCode.MyClass.MyFunction()</Call>");
			AddStackLineCount("MyAssembly.dll", "MyCode.MyClass.MyFunction()", 1);
			AddPublishedAssembly("MyAssembly.dll", "http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject/_git/MyRepository?path=/mycode/myfunction");
			AddSourceTreeResponsibility("http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject/_git/MyRepository?path=/mycode/myfunction", "A", "B", "C");
			AssertLogAssignment(new IssueAssignment("A", "B", "C"), report);
		}

		public void TestMoreSpecificGitSourceTreeReposibilityUsed()
		{
			var report = GetExceptionXml(@"
				<Call Assembly=""MyAssembly.dll"">   at MyCode.MyClass.MyFunction()</Call>");
			AddStackLineCount("MyAssembly.dll", "MyCode.MyClass.MyFunction()", 1);
			AddPublishedAssembly("MyAssembly.dll", "http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject/_git/MyRepository?path=/mycode/myfunction");
			AddSourceTreeResponsibility("http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject", "A", "A", "A");
			AddSourceTreeResponsibility("http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject/_git/MyRepository", "B", "B", "B");
			AssertLogAssignment(new IssueAssignment("B", "B", "B"), report);
		}

		public void TestMoreSpecificGitSourceTreeReposibilityUsedWithPath()
		{
			var report = GetExceptionXml(@"
				<Call Assembly=""MyAssembly.dll"">   at MyCode.MyClass.MyFunction()</Call>");
			AddStackLineCount("MyAssembly.dll", "MyCode.MyClass.MyFunction()", 1);
			AddPublishedAssembly("MyAssembly.dll", "http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject/_git/MyRepository?path=/mycode/myfunction");
			AddSourceTreeResponsibility("http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject/_git/MyRepository?path=/mycode", "A", "A", "A");
			AddSourceTreeResponsibility("http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject/_git/MyRepository", "B", "B", "B");
			AssertLogAssignment(new IssueAssignment("A", "A", "A"), report);
		}

		public void TestGitSourceTreePartialPathNotMatched()
		{
			var report = GetExceptionXml(@"
				<Call Assembly=""MyAssembly.dll"">   at MyCode.MyClass.MyFunction()</Call>");
			AddStackLineCount("MyAssembly.dll", "MyCode.MyClass.MyFunction()", 1);
			AddPublishedAssembly("MyAssembly.dll", "http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject/_git/MyRepository?path=/mycode/myfunction");
			AddSourceTreeResponsibility("http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject/_git/MyRepository?path=/mycode", "A", "A", "A");
			AddSourceTreeResponsibility("http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject/_git/MyRepository?path=/my", "B", "B", "B");
			AddSourceTreeResponsibility("http://tfs.wtg.zone:8080/tfs/MyCollection/MyProject/_git/My", "C", "C", "C");
			AddSourceTreeResponsibility("http://tfs.wtg.zone:8080/tfs/MyCollection/My", "D", "D", "D");
			AssertLogAssignment(new IssueAssignment("A", "A", "A"), report);
		}

		#region Helpers

		void AssertLogAssignment(IssueAssignment expectedAssigned, string errorText, bool hasInvalidParameters = false)
		{
			var log = CreateLog();
			CreateLogOccurrence(log, errorText);
			Factory.Save();

			if (!hasInvalidParameters)
			{
				AssertLogAssignment(
					log: log,
					expectedAssigned: expectedAssigned,
					expectedLastMessageReported: null,
					expectedLastExceptionReportedType: null,
					apiEnabled: false,
					apiResponseCode: HttpStatusCode.OK);

				AssertLogAssignment(
					log: log,
					expectedAssigned: expectedAssigned,
					expectedLastMessageReported: null,
					expectedLastExceptionReportedType: null,
					apiEnabled: true,
					apiResponseCode: HttpStatusCode.OK);

				AssertLogAssignment(
					log: log,
					expectedAssigned: expectedAssigned,
					expectedLastMessageReported: "The assignment retrieved from the API does not exist in the candidates.",
					expectedLastExceptionReportedType: null,
					apiEnabled: true,
					apiResponseCode: HttpStatusCode.OK,
					apiResponseJson: "{\"status\":\"success\",\"assignment\":{\"product\":\"AAA\",\"productArea\":\"PPP\",\"module\":\"III\"}}");

				AssertLogAssignment(
					log: log,
					expectedAssigned: expectedAssigned,
					expectedLastMessageReported: null,
					expectedLastExceptionReportedType: null,
					apiEnabled: true,
					apiResponseCode: HttpStatusCode.OK,
					apiResponseJson: "{\"status\":\"error\",\"message\":\"No assignment found.\"}");

				AssertLogAssignment(
					log: log,
					expectedAssigned: expectedAssigned,
					expectedLastMessageReported: "Response status code does not indicate success: 400 (Bad Request).",
					expectedLastExceptionReportedType: typeof(HttpRequestException),
					apiEnabled: true,
					apiResponseCode: HttpStatusCode.BadRequest,
					apiResponseJson: "{\"status\":\"error\",\"message\":\"So your request is so bad.\"}");

				AssertLogAssignment(
					log: log,
					expectedAssigned: expectedAssigned,
					expectedLastMessageReported: "Response status code does not indicate success: 500 (Internal Server Error).",
					expectedLastExceptionReportedType: typeof(HttpRequestException),
					apiEnabled: true,
					apiResponseCode: HttpStatusCode.InternalServerError,
					apiResponseJson: "{\"status\":\"error\",\"message\":\"The server is on CR1!\"}");

				AssertLogAssignment(
					log: log,
					expectedAssigned: expectedAssigned,
					expectedLastMessageReported: "Request timeout error (5 seconds).",
					expectedLastExceptionReportedType: typeof(TimeoutException),
					apiEnabled: true,
					exceptionToThrow: new TaskCanceledException());
			}
			else
			{
				AssertLogAssignment(
					log: log,
					expectedAssigned: expectedAssigned,
					expectedLastMessageReported: null,
					expectedLastExceptionReportedType: null,
					apiEnabled: true,
					apiResponseCode: null,
					apiResponseJson: null,
					hasInvalidParameters: true);
			}
		}

		void AssertLogAssignment(EdiHelpErrorLog log, IssueAssignment expectedAssigned, string expectedLastMessageReported, Type expectedLastExceptionReportedType, bool apiEnabled, HttpStatusCode? apiResponseCode = null, string apiResponseJson = "", Exception exceptionToThrow = null, bool hasInvalidParameters = false)
		{
			if (expectedAssigned != null && string.IsNullOrEmpty(apiResponseJson))
			{
				apiResponseJson = string.Format("{{\"status\":\"success\",\"assignment\":{{\"product\":\"{0}\",\"productArea\":\"{1}\",\"module\":\"{2}\"}}}}", expectedAssigned.Product, expectedAssigned.ProductArea, expectedAssigned.Module);
			}

			using (EDIDataRegistry.Instance.EnableMachineLearningTeamAssignment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, apiEnabled))
			using (EDIDataRegistry.Instance.MachineLearningTeamAssignmentApiUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, FakeApiUrl))
			using (EDIDataRegistry.Instance.MachineLearningTeamAssignmentApiRequestTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ApiTimeout))
			{
				var apiWasCalled = false;
				var assignment = new StackLinesWeightsLogAutoAssigner
				{
					MachineLearningTeamAssignmentRetriever = new MachineLearningTeamAssignmentRetriever
					{
						HttpMessageHandler = MachineLearningTeamAssignmentTestHelper.GetHttpMessageHandler(apiResponseCode, apiResponseJson, exceptionToThrow, assertSendAsyncAction: m =>
						{
							apiWasCalled = true;
						}),
					}
				}.GetAssignment(log, new DataFormatter());

				AssertEquals("API should be called when it is enabled and the params are valid.", apiEnabled && !hasInvalidParameters, apiWasCalled);

				AssertEquals("Assignment.Product should be same as expected.", expectedAssigned?.Product, assignment?.Product);
				AssertEquals("Assignment.ProductArea should be same as expected.", expectedAssigned?.ProductArea, assignment?.ProductArea);
				AssertEquals("Assignment.Module should be same as expected.", expectedAssigned?.Module, assignment?.Module);

				if (!string.IsNullOrEmpty(expectedLastMessageReported))
				{
					AssertContains("ErrorReporter.LastMessageReported should contain expected message when error is expected.", expectedLastMessageReported, ErrorReporter.LastMessageReported);
				}
				else
				{
					AssertNullOrEmpty("ErrorReporter.LastMessageReported should be null when no error.", ErrorReporter.LastMessageReported);
				}

				if (expectedLastExceptionReportedType != null)
				{
					AssertType("The type of ErrorReporter.LastExceptionReported should be same as expected when error is expected.", expectedLastExceptionReportedType, ErrorReporter.LastExceptionReported);
				}
				else
				{
					AssertNull("ErrorReporter.LastExceptionReported should be null when no error.", ErrorReporter.LastExceptionReported);
				}
			}

			ErrorReporter.Clear();
		}

		string FakeApiUrl => "https://localhost/api/v1/assignment";
		int ApiTimeout => 5;
	}

	#endregion

}
