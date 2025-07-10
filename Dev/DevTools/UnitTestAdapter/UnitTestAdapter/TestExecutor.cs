using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using Dat.Integration;
using Enterprise.ZArchitecture.Core.Testing;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using TestCase = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase;
using TestResult = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult;

namespace CWNUnit.TestAdapter
{
	[ExtensionUri(ExecutorUriString)]
	public class TestExecutor : ITestExecutor
	{
		public const string ExecutorUriString = "executor://cwnu_testexecutor";
		public static Uri ExecutorUri => executorUri ?? (executorUri = new Uri(ExecutorUriString));
		[ThreadStatic]
		static Uri executorUri;

		bool testingStopped;

		public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
		{
			RunTests(TestDiscoverer.GetTestCases(sources, null), runContext, frameworkHandle);
		}

		public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
		{
			var testCases = tests.ToList();
			if (testCases.Count == 0)
			{
				return;
			}

			var testOptions = TestOptionsManager.LoadOptions();
			if (!testOptions.Enabled)
			{
				return;
			}

			SetNUnitOptions(testOptions);

			var testAdapterContextOptions = new Dictionary<string, string>();
			if (!NUnit.Framework.TestingState.IsRunningTests)
			{
				// Do not change database if already running as test
				testAdapterContextOptions.Add(SystemAdapterOptions.DatabaseName, testOptions.DatabaseName);
			}

			var binPath = Path.GetDirectoryName(GetType().Assembly.Location);
			var sourcePath = Directory.GetParent(binPath)?.FullName ?? binPath;

			using (RedirectAssemblyResolver.HookCW1AssemblyBindingRedirects())
			{
				var testAdapterContext = new TestAdapterContext(testAdapterContextOptions);
				var testClient = new Enterprise.Dat.Implementation.TestClient(testAdapterContext, new TaskLogger());
				testClient.IsDat = false;
				var testRunner = testClient.StartTestRunner(sourcePath, binPath);

				var testErrorListener = new TestErrorListener();
				var datTestRunner = (Enterprise.Dat.Implementation.TestRunner)testRunner;
				datTestRunner.AddTestListener(testErrorListener);
				if (!testOptions.EnableTaskTestListener)
				{
					// When debugging in VS, it will complain about not finished tasks started by VS debugger
					datTestRunner.RemoveTestListener<TaskTestListener>();
				}

				RunTestsCore(testCases, datTestRunner, frameworkHandle, testErrorListener);
			}
		}

		void RunTestsCore(IEnumerable<TestCase> tests, Enterprise.Dat.Implementation.TestRunner datTestRunner, IFrameworkHandle frameworkHandle, TestErrorListener testErrorListener)
		{
			datTestRunner.InvokeOnDatForm(() =>
			{
				var datTestListener = datTestRunner.CreateTestRunnerTestListener();
				using (var datTestResult = datTestRunner.CreateNUnitTestResult(datTestListener))
				{
					foreach (var testCase in tests)
					{
						var testResult = new TestResult(testCase)
						{
							Outcome = TestOutcome.None
						};

						if (testingStopped)
						{
							testResult.Outcome = TestOutcome.Skipped;
							frameworkHandle.RecordResult(testResult);
							continue;
						}

						frameworkHandle.RecordStart(testCase);

						try
						{
							var testDescriptor = TestUtilities.TestCaseToTestDescriptor(testCase);
							RunTest(testDescriptor, datTestRunner, datTestListener, datTestResult, testResult, testErrorListener);
						}
						catch (Exception ex)
						{
							var errorException = TestUtilities.AggregateExceptions(ex, testErrorListener.Errors);
							TestUtilities.ExceptionToTestResult(errorException, testResult);
						}
						finally
						{
							frameworkHandle.RecordResult(testResult);
							frameworkHandle.RecordEnd(testCase, testResult.Outcome);
						}
					}
				}
				return null;
			});
		}

		void RunTest(TestDescriptor testDescriptor, Enterprise.Dat.Implementation.TestRunner datTestRunner, Enterprise.Dat.Implementation.TestRunnerTestListener datTestListener, NUnit.Framework.TestResult datTestResult, TestResult testResult, TestErrorListener testErrorListener)
		{
			var result = datTestRunner.DoRunTests(new[] { testDescriptor }, datTestListener, datTestResult)[0];

			if (testErrorListener.Skipped)
			{
				testResult.Outcome = TestOutcome.Skipped;
				testResult.ErrorMessage = testErrorListener.Message;
				return;
			}

			testResult.Outcome = result.Passed ? TestOutcome.Passed : TestOutcome.Failed;
			testResult.Duration = result.TimeTaken;

			if (!result.Passed)
			{
				if (testErrorListener.Errors.Any())
				{
					throw new TargetInvocationException(testErrorListener.Errors.First());
				}

				if (result.FailureDetails != null && result.FailureDetails.Length > 0)
				{
					testResult.ErrorMessage = string.Join("\r\n", result.FailureDetails);
				}
			}
		}

		void SetNUnitOptions(ITestOptions testOptions)
		{
			if (NUnit.Framework.TestingState.IsRunningTests)
			{
				// Do not change test runner options if already running as test
				return;
			}

			NUnit.Framework.TestingState.BreakOnPerformanceIssues = testOptions.BreakOnPerformanceIssues;
			NUnit.Framework.ITestAmnestyExtension.IncludeAmnestyTests = testOptions.IncludeAmnestyTests;
			NUnit.Framework.SnailTestAttribute.IncludeSnailTests = true;
			IncludeDeveloperOnlyTests(testOptions.IncludeDeveloperOnlyTests);
		}

		internal static void IncludeDeveloperOnlyTests(bool includeDeveloperOnlyTests)
		{
			SetInternalStaticValue(typeof(NUnit.Framework.DeveloperOnlyTestAttribute), "IncludeDeveloperOnlyTests", includeDeveloperOnlyTests);
		}

		static void SetInternalStaticValue(Type type, string propertyName, object value)
		{
			try
			{
				var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
				if (property != null)
				{
					property.SetValue(null, value);
				}
				else
				{
					var field = type.GetField(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
					field?.SetValue(null, value);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
			}
		}

		public void Cancel()
		{
			testingStopped = true;
		}
	}
}
