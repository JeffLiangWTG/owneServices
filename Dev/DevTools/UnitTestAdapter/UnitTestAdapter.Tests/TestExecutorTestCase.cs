using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common.Testing;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Moq;
using TestCaseBaseClass = NUnit.Framework.TestCase;

namespace CWNUnit.TestAdapter.Tests
{
	public class TestExecutorTestCase : TestCaseBaseClass
	{
		public void TestRunTests()
		{
			NestedTestCase.MethodAWasCalled = false;
			NestedTestCase.MethodBWasCalled = false;
			NestedTestCase.MethodCWasCalled = false;
			NestedTestCase.MethodWithFailWasCalled = false;

			var testCaseA = new TestCase(typeof(NestedTestCase).FullName + "." + nameof(NestedTestCase.TestMethodA), TestExecutor.ExecutorUri, GetType().Assembly.Location);
			var testCaseC = new TestCase(typeof(NestedTestCase).FullName + "." + nameof(NestedTestCase.TestMethodC), TestExecutor.ExecutorUri, GetType().Assembly.Location);

			var testCases = new[]
			{
				testCaseA,
				testCaseC,
			};

			var dummyFrameworkHandle = new DummyFrameworkHandle();
			new TestExecutor().RunTests(testCases, new DummyRunContext(), dummyFrameworkHandle);

			AssertEquals(true, NestedTestCase.MethodAWasCalled);
			AssertEquals(false, NestedTestCase.MethodBWasCalled);
			AssertEquals(true, NestedTestCase.MethodCWasCalled);
			AssertEquals(false, NestedTestCase.MethodWithFailWasCalled);

			AssertEquals(2, dummyFrameworkHandle.TestResults.Count);

			var testResultA = dummyFrameworkHandle.TestResults.FirstOrDefault(testResult => testResult.TestCase == testCaseA);
			AssertNotNull(testResultA);
			AssertEquals(TestOutcome.Passed, testResultA.Outcome);

			var testResultC = dummyFrameworkHandle.TestResults.FirstOrDefault(testResult => testResult.TestCase == testCaseC);
			AssertNotNull(testResultC);
			AssertEquals(TestOutcome.Passed, testResultC.Outcome);

			AssertEquals(2, dummyFrameworkHandle.EndOutcomes.Count);
			AssertEquals(TestOutcome.Passed, dummyFrameworkHandle.EndOutcomes[testCaseA]);
			AssertEquals(TestOutcome.Passed, dummyFrameworkHandle.EndOutcomes[testCaseC]);
		}

		public void TestCancel()
		{
			NestedTestCase.MethodAWasCalled = false;
			NestedTestCase.MethodBWasCalled = false;
			NestedTestCase.MethodCWasCalled = false;
			NestedTestCase.MethodWithFailWasCalled = false;

			var testCaseA = new TestCase(typeof(NestedTestCase).FullName + "." + nameof(NestedTestCase.TestMethodA), TestExecutor.ExecutorUri, GetType().Assembly.Location);
			var testCaseC = new TestCase(typeof(NestedTestCase).FullName + "." + nameof(NestedTestCase.TestMethodC), TestExecutor.ExecutorUri, GetType().Assembly.Location);

			var testCases = new[]
			{
				testCaseA,
				testCaseC,
			};

			var dummyFrameworkHandle = new DummyFrameworkHandle();

			var testExecutor = new TestExecutor();
			testExecutor.Cancel(); // Call Cancel before running tests, as it would be difficult to do it is multi-threading emulation
			testExecutor.RunTests(testCases, new DummyRunContext(), dummyFrameworkHandle);

			AssertEquals(false, NestedTestCase.MethodAWasCalled);
			AssertEquals(false, NestedTestCase.MethodBWasCalled);
			AssertEquals(false, NestedTestCase.MethodCWasCalled);
			AssertEquals(false, NestedTestCase.MethodWithFailWasCalled);

			AssertEquals(2, dummyFrameworkHandle.TestResults.Count);

			var testResultA = dummyFrameworkHandle.TestResults.FirstOrDefault(testResult => testResult.TestCase == testCaseA);
			AssertNotNull(testResultA);
			AssertEquals(TestOutcome.Skipped, testResultA.Outcome);

			var testResultC = dummyFrameworkHandle.TestResults.FirstOrDefault(testResult => testResult.TestCase == testCaseC);
			AssertNotNull(testResultC);
			AssertEquals(TestOutcome.Skipped, testResultC.Outcome);

			AssertEquals(0, dummyFrameworkHandle.EndOutcomes.Count); // Nothing run so there will be no RecordEnd() calls
		}

		public void TestRunTests_Fail()
		{
			NestedTestCase.MethodAWasCalled = false;
			NestedTestCase.MethodBWasCalled = false;
			NestedTestCase.MethodCWasCalled = false;
			NestedTestCase.MethodWithFailWasCalled = false;

			var testCaseA = new TestCase(typeof(NestedTestCase).FullName + "." + nameof(NestedTestCase.TestMethodA), TestExecutor.ExecutorUri, GetType().Assembly.Location);
			var testCaseWithFail = new TestCase(typeof(NestedTestCase).FullName + "." + nameof(NestedTestCase.TestMethodWithFail), TestExecutor.ExecutorUri, GetType().Assembly.Location);

			var testCases = new[]
			{
				testCaseWithFail, // Test with fail first
				testCaseA,
			};

			var dummyFrameworkHandle = new DummyFrameworkHandle();
			new TestExecutor().RunTests(testCases, new DummyRunContext(), dummyFrameworkHandle);

			AssertEquals(true, NestedTestCase.MethodAWasCalled);
			AssertEquals(false, NestedTestCase.MethodBWasCalled);
			AssertEquals(false, NestedTestCase.MethodCWasCalled);
			AssertEquals(true, NestedTestCase.MethodWithFailWasCalled);

			AssertEquals(2, dummyFrameworkHandle.TestResults.Count);

			var testResultA = dummyFrameworkHandle.TestResults.FirstOrDefault(testResult => testResult.TestCase == testCaseA);
			AssertNotNull(testResultA);
			AssertEquals(TestOutcome.Passed, testResultA.Outcome);

			var testResultWithFail = dummyFrameworkHandle.TestResults.FirstOrDefault(testResult => testResult.TestCase == testCaseWithFail);
			AssertNotNull(testResultWithFail);
			AssertEquals(TestOutcome.Failed, testResultWithFail.Outcome);
			AssertEquals("Hallo!", testResultWithFail.ErrorMessage);

			AssertEquals(2, dummyFrameworkHandle.EndOutcomes.Count);
			AssertEquals(TestOutcome.Passed, dummyFrameworkHandle.EndOutcomes[testCaseA]);
			AssertEquals(TestOutcome.Failed, dummyFrameworkHandle.EndOutcomes[testCaseWithFail]);
		}

		public void TestRunTests_IncorrectTestCase()
		{
			var testCaseX = new TestCase(typeof(NestedTestCase).FullName + ".NewMethod", TestExecutor.ExecutorUri, GetType().Assembly.Location);

			var testCases = new[]
			{
				testCaseX,
			};

			var dummyFrameworkHandle = new DummyFrameworkHandle();
			new TestExecutor().RunTests(testCases, new DummyRunContext(), dummyFrameworkHandle);

			AssertEquals(1, dummyFrameworkHandle.TestResults.Count);
			var testResultX = dummyFrameworkHandle.TestResults.FirstOrDefault(testResult => testResult.TestCase == testCaseX);
			AssertNotNull(testResultX);
			AssertEquals(TestOutcome.Failed, testResultX.Outcome);
			AssertContains("NewMethod", testResultX.ErrorMessage);

			AssertEquals(1, dummyFrameworkHandle.EndOutcomes.Count);
			AssertEquals(TestOutcome.Failed, dummyFrameworkHandle.EndOutcomes[testCaseX]);
		}

		public void TestRunTest_WithDisposable()
		{
			var testCaseDisposable = new TestCase(typeof(NestedTestCase).FullName + "." + nameof(NestedTestCase.TestMethodWithDisposable), TestExecutor.ExecutorUri, GetType().Assembly.Location);
			var testCases = new[]
			{
				testCaseDisposable,
			};

			var dummyFrameworkHandle = new DummyFrameworkHandle();
			new TestExecutor().RunTests(testCases, new DummyRunContext(), dummyFrameworkHandle);

			var testResultDisposable = dummyFrameworkHandle.TestResults.FirstOrDefault(testResult => testResult.TestCase == testCaseDisposable);
			AssertNotNull(testResultDisposable);
			AssertEquals(TestOutcome.Failed, testResultDisposable.Outcome);
			AssertContains("undisposed object", testResultDisposable.ErrorMessage);
			AssertContains(typeof(DummyDisposable).FullName, testResultDisposable.ErrorMessage);

			DisposableLeakListener.Instance.IgnoreObject(NestedTestCase.Disposable);
		}

		[NUnit.Framework.DeveloperOnlyTest]
		public void TestRuntTest_InThread()
		{
			NestedTestCase.MethodInThreadWasCalled = false;

			var testCaseInThread = new TestCase(typeof(NestedTestCase).FullName + "." + nameof(NestedTestCase.TestMethodInThread), TestExecutor.ExecutorUri, GetType().Assembly.Location);

			var testCases = new[]
			{
				testCaseInThread,
			};

			var dummyFrameworkHandle = new DummyFrameworkHandle();
			new TestExecutor().RunTests(testCases, new DummyRunContext(), dummyFrameworkHandle);

			AssertEquals(true, NestedTestCase.MethodInThreadWasCalled);

			AssertEquals(1, dummyFrameworkHandle.TestResults.Count);

			var testResultInThread = dummyFrameworkHandle.TestResults.FirstOrDefault(testResult => testResult.TestCase == testCaseInThread);
			AssertNotNull(testResultInThread);
			AssertEquals(TestOutcome.Failed, testResultInThread.Outcome);
			AssertContains("Hallo!", testResultInThread.ErrorMessage);
		}

		public void TestTestsAreRunInASingleBatch()
		{
			var mockTestListner = new Mock<NUnit.Framework.ITestListener>();
			ObjectFactory.Substitute("TestListeners", new[] { mockTestListner.Object });

			var testCaseA = new TestCase(typeof(NestedTestCase).FullName + "." + nameof(NestedTestCase.TestMethodA), TestExecutor.ExecutorUri, GetType().Assembly.Location);
			var testCaseB = new TestCase(typeof(NestedTestCase).FullName + "." + nameof(NestedTestCase.TestMethodB), TestExecutor.ExecutorUri, GetType().Assembly.Location);
			var testCaseC = new TestCase(typeof(NestedTestCase).FullName + "." + nameof(NestedTestCase.TestMethodC), TestExecutor.ExecutorUri, GetType().Assembly.Location);

			var testCases = new[]
			{
				testCaseA,
				testCaseB,
				testCaseC,
			};

			var dummyFrameworkHandle = new DummyFrameworkHandle();
			new TestExecutor().RunTests(testCases, new DummyRunContext(), dummyFrameworkHandle);

			mockTestListner.Verify(l => l.StartAllTests(It.IsAny<DateTime>()), Times.Once());
			mockTestListner.Verify(l => l.EndAllTests(It.IsAny<DateTime>()), Times.Once());
		}

		#region Test Bindable Dll Redirect

		public void TestUsingBindableDllRedirectShouldNotFailInVSTestAdapter()
		{
			IDisposable connection = null;
			try
			{
				AssertNoExceptionThrown("Should redirect to Newtonsoft.Json 12.0.0.0 and proceed without errors", () =>
				{
					// HubConnection references Newtonsoft.Json 6.0.0.0, without binding redirect this line will fail.
					connection = new Microsoft.AspNet.SignalR.Client.HubConnection("http://localhost");
				});
			}
			finally
			{
				connection?.Dispose();
			}
		}

		#endregion

		#region Testable methods

		[NUnit.Framework.DoNotAddToTestTree]
		public class NestedTestCase : TestCaseBaseClass
		{
			public void TestMethodA()
			{
				MethodAWasCalled = true;
				Assert(true);
			}
			public static bool MethodAWasCalled { get; set; }

			public void TestMethodB()
			{
				MethodBWasCalled = true;
				Assert(true);
			}
			public static bool MethodBWasCalled { get; set; }

			public void TestMethodC()
			{
				MethodCWasCalled = true;
				Assert(true);
			}
			public static bool MethodCWasCalled { get; set; }

			public void TestMethodWithFail()
			{
				MethodWithFailWasCalled = true;
				Fail("Hallo!");
			}
			public static bool MethodWithFailWasCalled { get; set; }

			public void TestMethodWithDisposable()
			{
				Disposable = new DummyDisposable();
				DisposableLeakListenerWrapper.RegisterDisposable(Disposable);
				Assert(true);
			}
			public static IDisposable Disposable;

			public void TestMethodInThread()
			{
				var thread = new Thread(() =>
				{
					MethodInThreadWasCalled = true;
					Fail("Hallo!");
				});
				thread.Start();
				thread.Join();
			}
			public static bool MethodInThreadWasCalled { get; set; }
		}

		#endregion

		#region Dummy classes

		class DummyRunContext : IRunContext
		{
			public ITestCaseFilterExpression GetTestCaseFilter(IEnumerable<string> supportedProperties, Func<string, TestProperty> propertyProvider)
			{
				throw new NotImplementedException();
			}

			public IRunSettings RunSettings { get; }
			public bool KeepAlive { get; }
			public bool InIsolation { get; }
			public bool IsDataCollectionEnabled { get; }
			public bool IsBeingDebugged { get; }
			public string TestRunDirectory { get; }
			public string SolutionDirectory { get; }
		}

		class DummyFrameworkHandle : IFrameworkHandle
		{
			public void SendMessage(TestMessageLevel testMessageLevel, string message)
			{
			}

			public void RecordResult(TestResult testResult)
			{
				TestResults.Add(testResult);
			}
			public List<TestResult> TestResults { get; } = new List<TestResult>();

			public void RecordStart(TestCase testCase)
			{
			}

			public void RecordEnd(TestCase testCase, TestOutcome outcome)
			{
				EndOutcomes[testCase] = outcome;
			}
			public Dictionary<TestCase, TestOutcome> EndOutcomes { get; } = new Dictionary<TestCase, TestOutcome>();

			public void RecordAttachments(IList<AttachmentSet> attachmentSets)
			{
			}

			public int LaunchProcessWithDebuggerAttached(string filePath, string workingDirectory, string arguments, IDictionary<string, string> environmentVariables)
			{
				return 0;
			}

			public bool EnableShutdownAfterTestRun { get; set; }
		}

		class DummyDisposable : IDisposable
		{
			public void Dispose() { }
		}

		#endregion
	}
}
