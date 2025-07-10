using System;
using System.Linq;
using System.Reflection;
using Dat.Integration;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using TestCaseBaseClass = NUnit.Framework.TestCase;
using TestResult = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult;

namespace CWNUnit.TestAdapter.Tests
{
	public class TestUtilitiesTestCase : TestCaseBaseClass
	{
		public void TestTestDescriptorToTestCase()
		{
			var testDescriptor = new TestDescriptor("Scope1", "Element1", "Target1", 1, DatTestFlags.FrequentlyFailing | DatTestFlags.GUITest, new[] { "Capability1", "Capability2" });
			var testCase = TestUtilities.TestDescriptorToTestCase(testDescriptor, "Assembly1", TestExecutor.ExecutorUri);

			AssertEquals("Assembly1", testCase.Source);
			AssertEquals("Element1.Target1", testCase.FullyQualifiedName);

			var property1 = testCase.GetProperties().FirstOrDefault(p => p.Key.Id == nameof(TestDescriptor.ProjectDefinedCapabilityRequirements));
			var property2 = testCase.GetProperties().FirstOrDefault(p => p.Key.Id == nameof(TestDescriptor.DatTestFlags));
			var property3 = testCase.GetProperties().FirstOrDefault(p => p.Key.Id == nameof(TestDescriptor.CapabilityRequirements));

			AssertEquals(1, (int)property1.Value);
			AssertEquals((int)(DatTestFlags.FrequentlyFailing | DatTestFlags.GUITest), (int)property2.Value);
			AssertEquals("Capability1,Capability2", string.Join(",", (string[])property3.Value));
		}

		public void TestTestCaseToTestDescriptor()
		{
			var testDescriptor = new TestDescriptor("Scope1", "Element1", "Target1", 1, DatTestFlags.FrequentlyFailing | DatTestFlags.GUITest, new[] { "Capability1", "Capability2" });
			var testCase = TestUtilities.TestDescriptorToTestCase(testDescriptor, @"Dev\Bin\Scope2.dll", TestExecutor.ExecutorUri);

			var newTestDescriptor = TestUtilities.TestCaseToTestDescriptor(testCase);

			AssertEquals("Scope should be calculated from TestCase.Source", "Scope2", newTestDescriptor.Identifier.ScopeName);
			AssertEquals("Element1", newTestDescriptor.Identifier.ElementName);
			AssertEquals("Target1", newTestDescriptor.Identifier.TargetName);

			AssertEquals(1, newTestDescriptor.ProjectDefinedCapabilityRequirements);
			AssertEquals(DatTestFlags.FrequentlyFailing | DatTestFlags.GUITest, newTestDescriptor.DatTestFlags);
			AssertEquals("Capability1,Capability2", string.Join(",", newTestDescriptor.CapabilityRequirements));
		}

		public void TestExceptionToTestResult()
		{
			var ex = new Exception("Hallo!");

			var testCase = new TestCase("test", TestExecutor.ExecutorUri, "c:");
			var testResult = new TestResult(testCase);

			TestUtilities.ExceptionToTestResult(ex, testResult);

			AssertEquals("Hallo!", testResult.ErrorMessage);
			AssertEquals(ex.StackTrace, testResult.ErrorStackTrace);
			AssertEquals(TestOutcome.Failed, testResult.Outcome);
		}

		public void TestExceptionToTestResult_TargetInvocationException()
		{
			var ex = new Exception("Hallo!");

			var testCase = new TestCase("test", TestExecutor.ExecutorUri, "c:");
			var testResult = new TestResult(testCase);

			var tiex = new TargetInvocationException("Bye!", ex);
			TestUtilities.ExceptionToTestResult(tiex, testResult);

			AssertEquals("Hallo!", testResult.ErrorMessage);
			AssertEquals(ex.StackTrace, testResult.ErrorStackTrace);
			AssertEquals(TestOutcome.Failed, testResult.Outcome);

			tiex = new TargetInvocationException("Bye!", null);
			TestUtilities.ExceptionToTestResult(tiex, testResult);

			AssertEquals("Bye!", testResult.ErrorMessage);
			AssertEquals(tiex.StackTrace, testResult.ErrorStackTrace);
			AssertEquals(TestOutcome.Failed, testResult.Outcome);
		}
	}
}
