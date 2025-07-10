using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise;
using CargoWise.EntityFramework;
using CWNUnit.TestAdapter;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using TestCase = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase;
using TestResult = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestResult;

namespace CWNetCoreTest.TestAdapter
{
	[ExtensionUri(NetCoreTestDiscoverer.Nunit3ExecutorUri)]
	public class NetCoreTestExecutor : ITestExecutor
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Used to test setup")]
		public void RunTests(IEnumerable<string>? sources, IRunContext? runContext, IFrameworkHandle? frameworkHandle)
		{
			var testCases = NetCoreTestDiscoverer.DiscoverTests(sources).ToList();
			RunTests(testCases, runContext, frameworkHandle);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Used to test setup")]
		public void RunTests(IEnumerable<TestCase>? tests, IRunContext? runContext, IFrameworkHandle? frameworkHandle)
		{
			var testOptions = TestOptionsManager.LoadOptions();
			if (!testOptions.Enabled)
			{
				return;
			}
			SetNUnitOptions(testOptions);

			Application.ThreadException += Application_ThreadException;

			SetupCargoWise(testOptions);
			SetupTestRunner();
			try
			{
				ArgumentNullException.ThrowIfNull(frameworkHandle);
				ArgumentNullException.ThrowIfNull(tests);
				ArgumentNullException.ThrowIfNull(cwTestResult);
				ArgumentNullException.ThrowIfNull(testListener);
				NUnit.Framework.TestSuite? testSuite = null;
				foreach (var test in tests)
				{
					if (isCancelled)
					{
						var result = new TestResult(test)
						{
							Outcome = TestOutcome.Skipped
						};
						frameworkHandle.RecordResult(result);
						continue;
					}

					var outcome = TestOutcome.None;
					frameworkHandle.RecordStart(test);
					var stopwatch = Stopwatch.StartNew();
					try
					{
						var testDescriptor = TestUtilities.TestCaseToTestDescriptor(test);
						var assemblyToTest = Assembly.Load(testDescriptor.Identifier.ScopeName);
						var typeToTest = assemblyToTest.GetType(testDescriptor.Identifier.ElementName, true);
						if (testSuite?.TestClass != typeToTest)
						{
							testSuite = new NUnit.Framework.TestSuite(typeToTest);
						}

						cwTestResult.BeforeTestInstantiated();
						DoRunTest(testSuite!, testDescriptor.Identifier.TargetName);
						//Task.Delay(1000).Wait();
						cwTestResult.AfterTestSetToNull();

						if (testListener.Errors != null && testListener.Errors.Count > 0)
						{
							outcome = TestOutcome.Failed;
							foreach (var exception in testListener.Errors)
							{
								var result = new TestResult(test)
								{
									Duration = stopwatch.Elapsed,
								};
								TestUtilities.ExceptionToTestResult(exception, result);
								frameworkHandle.RecordResult(result);
							}
						}
						else if (testListener.Skipped && testListener.Message == "Developer Only test")
						{
							var result = new TestResult(test)
							{
								Outcome = TestOutcome.Passed
							};
							frameworkHandle.RecordResult(result);
							continue;
						}
						else
						{
							outcome = TestOutcome.Passed;
							var result = new TestResult(test)
							{
								Outcome = TestOutcome.Passed,
								Duration = stopwatch.Elapsed,
							};
							frameworkHandle.RecordResult(result);
						}
					}
					catch (Exception ex)
					{
						outcome = TestOutcome.Failed;
						var result = new TestResult(test)
						{
							Duration = stopwatch.Elapsed,
						};
						var errorException = TestUtilities.AggregateExceptions(ex, testListener.Errors);
						TestUtilities.ExceptionToTestResult(errorException, result);
						frameworkHandle.RecordResult(result);
					}
					finally
					{
						frameworkHandle.RecordEnd(test, outcome);
					}
				}
			}
			finally
			{
				Application.ThreadException -= Application_ThreadException;
				((IDisposable?)cwTestResult)?.Dispose();
			}
		}

		static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
		{
			// There appears to be a difference in how net48 and net8.0 handle WinForms exceptions.
			// Adding this global empty exception handler to avoid popping the exception dialog which hangs DAT until the test timesout.
			// PRs enabling net8.0 in a project will often encounter many such timeouts, and wreak havoc on DAT by triggering High Test Failure issues and combine splits.
			// This will allow net8.0 migrations to proceed without taking down DAT until this is investigated further.
		}

		void DoRunTest(NUnit.Framework.TestSuite testSuite, string testName)
		{
			var test = testSuite.NewTest(testName);
			try
			{
				test.Run(cwTestResult);
			}
			catch (Exception ex)
			{
				((NUnit.Framework.ITestListener?)testListener)!.AddError(ex, test);
			}
		}

		TestErrorListener? testListener;
		NUnit.Framework.TestResult? cwTestResult;

#pragma warning disable CW1021 // Static Fields Are Thread Static Rule
		static bool IsSetupCargoWiseCalled;
#pragma warning restore CW1021 // Static Fields Are Thread Static Rule
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Used to test setup")]
		static void SetupCargoWise(ITestOptions testOptions)
		{
			if (!IsSetupCargoWiseCalled)
			{
				IsSetupCargoWiseCalled = true;

				Globals.IsUserInteractive = true;
				Initialization.ConfigureCargoWise(testOptions);

				NUnit.Framework.TestingState.IsRunningTests = true;
				NUnit.Framework.TestingState.IsRunningOnDAT = Initialization.GetDatIsTesting();
				var datSourcePath = Environment.GetEnvironmentVariable("DAT_TestSourcePath");
				if (!string.IsNullOrEmpty(datSourcePath))
				{
					if (!System.IO.Path.EndsInDirectorySeparator(datSourcePath))
					{
						datSourcePath += System.IO.Path.DirectorySeparatorChar;
					}
					NUnit.Framework.TestCase.BaseSourcePath = datSourcePath;
				}

				var datSupplementaryContentPath = Environment.GetEnvironmentVariable("DAT_TestSupplementaryContentPath");
				if (!string.IsNullOrEmpty(datSupplementaryContentPath))
				{
					NUnit.Framework.TestCase.SetSupplementaryContentPath(datSupplementaryContentPath);
				}
			}

			NotificationHandler.Instance = new ZGUINotificationHandler();
		}

		void SetupTestRunner()
		{
			var testListeners = new List<NUnit.Framework.ITestListener>();
			testListener = new TestErrorListener();
			testListeners.Add(testListener);
			testListeners.AddRange(UnitTestListenersFactory.GetTestListeners());
			cwTestResult = new NUnit.Framework.TestResult(testListeners.ToArray());
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
			IncludeDeveloperOnlyTests(testOptions.IncludeDeveloperOnlyTests && !datIsTesting);
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
			catch (Exception)
			{
			}
		}

		readonly bool datIsTesting = Initialization.GetDatIsTesting();

		bool isCancelled;
		public void Cancel()
		{
			isCancelled = true;
		}

		static NetCoreTestExecutor()
		{
			NetCoreAssemblyResolver.Setup();
		}
	}
}
