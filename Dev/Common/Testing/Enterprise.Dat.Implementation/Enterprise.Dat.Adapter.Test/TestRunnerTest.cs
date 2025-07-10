using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Dat.Integration;
using Enterprise.Startup;
using NUnit.Framework;

namespace Enterprise.Dat.Implementation.Testing
{
	sealed class TestRunnerTest : TestCase
	{
		public void TestRunnerIsIntialised()
		{
			var testRunner = new TestRunner();
			AssertEquals(true, testRunner.IsInitialised);
		}

		public void TestRunTests()
		{
			var testRunner = new TestRunner();
			testRunner.Initialise(new ITestListener[] { new TestTestListener() }, new ErrorDescriptionListFactory());

			var results = testRunner.RunTests(new[] { CreateTestDescriptor("TestPass"), CreateTestDescriptor("TestFail"), CreateTestDescriptor("TestFailInListener") });

			AssertEquals(3, results.Length);
			AssertEquals("TestPass", results[0].TestDescriptor.Identifier.TargetName);
			AssertEquals(true, results[0].Passed);
			AssertEquals("TestFail", results[1].TestDescriptor.Identifier.TargetName);
			AssertEquals(false, results[1].Passed);
			AssertEquals("TestFailInListener", results[2].TestDescriptor.Identifier.TargetName);
			AssertEquals(false, results[2].Passed);
		}

		public void TestRunTests_ForDescriptorNotMatchingAssembly()
		{
			var testRunner = new TestRunner();
			testRunner.Initialise(new ITestListener[] { new TestTestListener() }, new ErrorDescriptionListFactory());

			var results = testRunner.RunTests(new[] { new TestDescriptor(new TestIdentifier("NonExistentAssembly.dll", "TestClass", "TestMethod")) });

			AssertEquals(1, results.Length);
			AssertEquals(false, results[0].Passed);
			AssertContains("Exception invoking test: <NonExistentAssembly.dll;TestClass.TestMethod>", results[0].FailureDetails.Single());
		}

		public void TestInvokeFailure()
		{
			var testRunner = new TestRunner();
			testRunner.Initialise(new ITestListener[] { new TestTestListener() }, new ErrorDescriptionListFactory());

			var results = testRunner.RunTests(new[] { CreateTestDescriptor("TestDoesNotExist") });

			AssertEquals(1, results.Length);
			AssertEquals("TestDoesNotExist", results[0].TestDescriptor.Identifier.TargetName);
			AssertEquals(false, results[0].Passed);
		}

		public void TestErrorDetailsAreFormatted()
		{
			var testRunner = new TestRunner();
			testRunner.Initialise(new ITestListener[] { new TestTestListener() }, new ErrorDescriptionListFactory());

			var results = testRunner.RunTests(new[] { CreateTestDescriptor("TestFail") });
			Assert("Expect errors details, actual details:\r\n" + results[0].FailureDetails[0], results[0].FailureDetails[0].StartsWith("Exception Type: NUnit.Framework.AssertionFailedError<br>Message: "));
		}

		public void TestCtorExceptionError()
		{
			var testRunner = new TestRunner();

			var testDescriptor = new TestDescriptor(typeof(TestClassCtorExceptionTest).Assembly.GetName().Name, typeof(TestClassCtorExceptionTest).FullName,
				"TestDummyFunction", 0, DatTestFlags.Default);
			testRunner.Initialise(new ITestListener[] { new TestTestListener() }, new ErrorDescriptionListFactory());

			var results = testRunner.RunTests(new[] { testDescriptor });

			AssertEquals(1, results.Length);
			AssertEquals("TestDummyFunction", results[0].TestDescriptor.Identifier.TargetName);
			AssertEquals(false, results[0].Passed);
			Assert("Actual details:\r\n" + results[0].FailureDetails[0], results[0].FailureDetails[0].Contains("Exception in Ctor"));
		}

		public void TestStartEndAllTestsCalledOnceForBatch()
		{
			var testRunner = new TestRunner();
			var listener = new TestTestListener();
			testRunner.Initialise(new ITestListener[] { listener }, new ErrorDescriptionListFactory());

			var results = testRunner.RunTests(new[] { CreateTestDescriptor("TestPass"), CreateTestDescriptor("TestFail") });
			AssertArrayEqualsByElements(new[]
			{
				"StartAllTests",
				"BeforeEachTest",
				"StartTest",
				"EndTest",
				"AfterEachTest",
				"BeforeEachTest",
				"StartTest",
				"EndTest",
				"AfterEachTest",
				"EndAllTests"
			}, listener.Actions.ToArray());
		}

		[ALPOnly]
		public void TestNet8TestFailure()
		{
			var testRunner = new TestRunner();

			var testDescriptor = new TestDescriptor(typeof(TestClassNet8FailureTest).Assembly.GetName().Name, typeof(TestClassNet8FailureTest).FullName,
				nameof(TestClassNet8FailureTest.TestFail) + "[NetCore]", 0, DatTestFlags.Default);
			testRunner.Initialise(new ITestListener[] { new TestTestListener() }, new ErrorDescriptionListFactory());

			var results = testRunner.RunTests(new[] { testDescriptor });

			AssertEquals(1, results.Length);
			AssertEquals(nameof(TestClassNet8FailureTest.TestFail) + "[NetCore]", results[0].TestDescriptor.Identifier.TargetName);
			AssertEquals(false, results[0].Passed);
			Assert("Actual details:\r\n" + results[0].FailureDetails[0], results[0].FailureDetails[0].Contains("8"));
		}

		protected override void SetUp()
		{
			if (DatForm.Instance == null)
			{
				(datFormForTest = new DatForm()).Show();
			}
			base.SetUp();
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (datFormForTest != null)
			{
				DatForm.Instance = null;
				datFormForTest.Dispose();
			}
		}

		DatForm datFormForTest;

		TestDescriptor CreateTestDescriptor(string methodName)
		{
			return new TestDescriptor(typeof(TestClassForTestRunnerTest).Assembly.GetName().Name, typeof(TestClassForTestRunnerTest).FullName, methodName, 0, DatTestFlags.Default);
		}

		[DoNotAddToTestTree]
		class TestClassForTestRunnerTest : TestCase
		{
			public void TestPass()
			{
				Assert(true);
			}

			public void TestFail()
			{
				Assert(false);
			}

			public void TestFailInListener()
			{
				Assert(true);
			}
		}

		class TestTestListener : ITestListener
		{
			public void AddError(Exception e, ITest test)
			{
			}

			public void AddSlowTest(TimeSpan testDuration)
			{
			}

			public void AfterEachTest(DateTime endTime)
			{
				Actions.Add(nameof(AfterEachTest));
			}

			public void BeforeEachTest(DateTime startTime)
			{
				Actions.Add(nameof(BeforeEachTest));
			}

			public void EndAllTests(DateTime endTime)
			{
				Actions.Add(nameof(EndAllTests));
			}

			public void EndTest(TestCase test, DateTime endTime)
			{
				Actions.Add(nameof(EndTest));
				AssertNotEquals("TestFailInListener", test.Name);
			}

			public void StartAllTests(DateTime startTime)
			{
				Actions.Add(nameof(StartAllTests));
			}

			public void StartTest(TestCase test, DateTime startTime)
			{
				Actions.Add(nameof(StartTest));
			}

			public readonly List<string> Actions = new List<string>();
		}

		[DoNotAddToTestTree]
		class TestClassCtorExceptionTest : TestCase
		{
			public TestClassCtorExceptionTest()
			{
				throw new Exception("Exception in Ctor.");
			}

			public void TestDummyFunction()
			{
				return;
			}
		}

		[DoNotAddToTestTree]
		class TestClassNet8FailureTest : TestCase
		{
			[TargetFrameworks(TargetFramework.NetCore)]
			public void TestFail()
			{
				AssertNotContains("8", RuntimeInformation.FrameworkDescription);
			}

			public void TestDummyFunction()
			{
				return;
			}
		}
	}
}
