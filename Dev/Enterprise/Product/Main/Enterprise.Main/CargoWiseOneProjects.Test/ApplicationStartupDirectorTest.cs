using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using AppDomainWrappers.Net;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Dat.Integration;
using Enterprise.Environment;
using Enterprise.Initialisation;
using Enterprise.MasterFiles.Business;
using Enterprise.Startup.Tasks;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.ActivityLogging;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using NUnit.Framework;

#if !NET8_0_OR_GREATER
using CargoWise.Types;
using Enterprise.URLHandler;
#endif

namespace Enterprise.Startup.Testing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName", Justification = "Test code")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1015:ApplicationOpenForms", Justification = "Test code")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1157:Do not use System.AppDomain.", Justification = "Pending migration")] // WI00669071 - Do not use System.AppDomain.
	sealed class ApplicationStartupDirectorTest : TransactionedTestCase
	{
		#region Startup task mechanism

		public void TestWebProxy()
		{
#if NETFRAMEWORK
			AssertEquals("Expected DefaultWebProxy to have default credentials in .NET Framework", CredentialCache.DefaultCredentials, WebRequest.DefaultWebProxy.Credentials);
#else
			AssertEquals("Expected DefaultWebProxy to be System.Net.Http.HttpNoProxy in .NET Core and later", "System.Net.Http.HttpNoProxy", WebRequest.DefaultWebProxy.ToString());
#endif
		}

		public void TestRunEnterpriseStartupTasks()
		{
			TestStartupTask task1 = new TestStartupTask(true, TestStartupTask.Result.Pass);
			TestStartupTask task2 = new TestStartupTask(true, TestStartupTask.Result.Pass);
			TestStartupTask task3 = new TestStartupTask(true, TestStartupTask.Result.Pass);
			TestStartupTaskExecuteWithExectuteOnFailure task4 = new TestStartupTaskExecuteWithExectuteOnFailure(true, TestStartupTask.Result.Pass);
			Director.OverrideStartupTasks = new IApplicationStartupTask[] { task1, task2, task3, task4 };
			int result = Director.StartEnterprise(new string[] { "-SDir:foo" });
			Assert("Enterprise should run", Director.IsEnterpriseRun);
			AssertEquals("StartEnterprise should return 0", 0, result);
			Assert("Task1 should have executed", task1.Executed);
			AssertEquals("Arguments should be passed to task1", "foo", task1.PassedArguments["-SDir:"]);
			Assert("Task2 should have executed", task2.Executed);
			AssertEquals("Arguments should be passed to task2", "foo", task1.PassedArguments["-SDir:"]);
			Assert("Task3 should have executed", task3.Executed);
			AssertEquals("Arguments should be passed to task3", "foo", task1.PassedArguments["-SDir:"]);
			Assert("Task4 should have executed", task4.Executed);
			Assert("Task4 should not have executed failure function", !task4.ExecutedOnFailure);
		}

		public void TestRunEnterpriseStartupTasksAlsoSetsApplicationDispatcherCurrent()
		{
			var task = new TestStartupTask(true, TestStartupTask.Result.Pass);
			Director.OverrideStartupTasks = new IApplicationStartupTask[] { task };
			ApplicationDispatcher.Current = null;
			Director.RunEnterpriseStartupTasksForTest(new string[] { "-SDir:foo" });
			AssertNotNull("Current application dispatcher should have been set", ApplicationDispatcher.Current);
		}

		[ExpectNoExceptions]
		public void TestRunEnterpriseStartupTasks_WhenPerformanceStatisticsBeingCollected_ShouldNotThrowException()
		{
			var collectorMock = new Mock<IPerformanceStatisticsCollector>();

			collectorMock.Setup(c => c.StatisticMode).Returns(EnabledState.Simple);

			using (ObjectFactory.Substitute(collectorMock.Object))
			{
				PerformanceStatisticsCollector.ResetInstance();
				Director.StartEnterprise(new[] { "-SDir:" + Env.TempPath });
			}

			collectorMock.Verify(c =>
				c.StartMonitoring(
					It.Is<string>(s => string.IsNullOrEmpty(s)),
					It.IsAny<string>()
				), Times.Never(), "Empty string should never be passed to StartMonitoring");
		}

		public void TestDatabaseUpgradeExceptionsAreHandled()
		{
			var actualDbEnv = DbEnv.Instance;
			try
			{
				bool exceptionHandled = false;
				var mockGuidPlugin = new Mock<IDbConnectionGuiPlugin>();
				var mockDbEnv = new Mock<BaseDbEnvironment>();
				mockDbEnv.Setup(m => m.ConnectionGuiPlugin).Returns(mockGuidPlugin.Object);
				mockGuidPlugin.Setup(m => m.HandleDatabaseUpgradeException(It.IsAny<DatabaseUpgradeException>()))
					.Callback(() => exceptionHandled = true);
				DbEnv.SetDbEnvironment(mockDbEnv.Object);

				TestStartupTask task1 = new TestStartupTask(true, TestStartupTask.Result.Pass);
				TestStartupTaskExecuteWithExectuteOnFailure task2 = new TestStartupTaskExecuteWithExectuteOnFailure(true, TestStartupTask.Result.ThrowDbUpgradedException);
				Director.OverrideStartupTasks = new IApplicationStartupTask[] { new ApplicationStartupDirector.EnterStartupErrorHandler(), task1, task2 };
				int result = Director.StartEnterprise(new string[] { "-SDir:foo" });

				Assert("Enterprise should run", Director.IsEnterpriseRun);
				AssertEquals("StartEnterprise should return 0", 0, result);
				Assert(exceptionHandled);
				mockDbEnv.VerifyAll();
				mockGuidPlugin.VerifyAll();
			}
			finally
			{
				DbEnv.SetDbEnvironment(actualDbEnv);
			}
		}

		public void TestRunEnterpriseStartupTasksFailingTask()
		{
			TestStartupTask task1 = new TestStartupTask(true, TestStartupTask.Result.Pass);
			TestStartupTask task2 = new TestStartupTask(true, TestStartupTask.Result.Fail);
			TestStartupTask task3 = new TestStartupTask(true, TestStartupTask.Result.Pass);
			TestStartupTaskExecuteWithExectuteOnFailure task4 = new TestStartupTaskExecuteWithExectuteOnFailure(true, TestStartupTask.Result.Pass);
			Director.OverrideStartupTasks = new IApplicationStartupTask[] { task1, task2, task3, task4 };
			int result = Director.StartEnterprise(new string[] { "" });
			Assert("Enterprise should not run", !Director.IsEnterpriseRun);
			AssertEquals($"StartEnterprise should return ExitCodes.TestExitCode: {ExitCodes.TestExitCode}", ExitCodes.TestExitCode, result);
			Assert("Task1 should have executed", task1.Executed);
			Assert("Task2 should have executed", task2.Executed);
			Assert("Task3 should not have executed", !task3.Executed);
			Assert("Task4 should not have executed", !task4.Executed);
			Assert("Task4 should have executed failure function", task4.ExecutedOnFailure);
		}

		[ExpectException(typeof(Exception))]
		public void TestRunEnterpriseStartupTasksThrowingException()
		{
			TestStartupTask task1 = new TestStartupTask(true, TestStartupTask.Result.Pass);
			TestStartupTask task2 = new TestStartupTask(true, TestStartupTask.Result.Throw);
			TestStartupTask task3 = new TestStartupTask(true, TestStartupTask.Result.Pass);
			TestStartupTaskExecuteWithExectuteOnFailure task4 = new TestStartupTaskExecuteWithExectuteOnFailure(true, TestStartupTask.Result.Pass);
			Director.OverrideStartupTasks = new IApplicationStartupTask[] { task1, task2, task3, task4 };
			try
			{
				int result = Director.StartEnterprise(new string[] { "" });
			}
			finally
			{
				Assert("Enterprise should not run", !Director.IsEnterpriseRun);
				Assert("Task1 should have executed", task1.Executed);
				Assert("Task2 should have executed", task2.Executed);
				Assert("Task3 should not have executed", !task3.Executed);
				Assert("Task4 should not have executed", !task4.Executed);
				Assert("Task4 should have executed failure function", task4.ExecutedOnFailure);
			}
		}

		public void TestRunEnterpriseStartupTasksExceptionHandler()
		{
			TestStartupTaskExecuteWithExceptionHandler task1 = new TestStartupTaskExecuteWithExceptionHandler(true, TestStartupTask.Result.Pass, TestStartupTask.Result.Fail);
			TestStartupTask task2 = new TestStartupTask(true, TestStartupTask.Result.Throw);
			TestStartupTask task3 = new TestStartupTask(true, TestStartupTask.Result.Pass);
			TestStartupTaskExecuteWithExectuteOnFailure task4 = new TestStartupTaskExecuteWithExectuteOnFailure(true, TestStartupTask.Result.Pass);
			Director.OverrideStartupTasks = new IApplicationStartupTask[] { task1, task2, task3, task4 };
			int result = Director.StartEnterprise(new string[] { "" });
			Assert("Enterprise should not run", !Director.IsEnterpriseRun);
			AssertEquals($"StartEnterprise should return ExitCodes.TestExitCode: {ExitCodes.TestExitCode}", ExitCodes.TestExitCode, result);
			Assert("Task1 should have handled exception", task1.HandledException);
			Assert("Task1 should have executed", task1.Executed);
			Assert("Task2 should have executed", task2.Executed);
			Assert("Task3 should not have executed", !task3.Executed);
			Assert("Task4 should not have executed", !task4.Executed);
			Assert("Task4 should have executed failure function", task4.ExecutedOnFailure);
		}

		public void TestRunEnterpriseStartupTasksSkipShouldNotExecute()
		{
			TestStartupTask task1 = new TestStartupTask(true, TestStartupTask.Result.Pass);
			TestStartupTask task2 = new TestStartupTask(false, TestStartupTask.Result.Pass);
			TestStartupTask task3 = new TestStartupTask(true, TestStartupTask.Result.Pass);
			Director.OverrideStartupTasks = new IApplicationStartupTask[] { task1, task2, task3 };
			int result = Director.StartEnterprise(new string[] { "" });
			Assert("Enterprise should run", Director.IsEnterpriseRun);
			AssertEquals("StartEnterprise should return 0", 0, result);
			Assert("Task1 should have executed", task1.Executed);
			Assert("Task2 should not have executed", !task2.Executed);
			Assert("Task3 should have executed", task3.Executed);
		}

		public void TestRunEnterpriseStartupTasksWithSpecificErrorCodeOnFailure()
		{
			TestStartupTask task1 = new TestStartupTaskWithFailureExitCode(true, TestStartupTask.Result.Pass, -2);
			TestStartupTask task2 = new TestStartupTaskWithFailureExitCode(true, TestStartupTask.Result.Fail, -3);
			TestStartupTask task3 = new TestStartupTaskWithFailureExitCode(true, TestStartupTask.Result.Pass, -4);
			Director.OverrideStartupTasks = new IApplicationStartupTask[] { task1, task2, task3 };
			int result = Director.StartEnterprise(new string[] { "" });
			Assert("Enterprise should not run", !Director.IsEnterpriseRun);
			AssertEquals("StartEnterprise should return -3", -3, result);
			Assert("Task1 should have executed", task1.Executed);
			Assert("Task2 should have executed", task2.Executed);
			Assert("Task3 should not have executed", !task3.Executed);
		}

		public void TestRunEnterpriseStartupTasksWithSpecificErrorCodeOnException()
		{
			TestStartupTaskExecuteWithExceptionHandler exceptionHandler = new TestStartupTaskExecuteWithExceptionHandler(true, TestStartupTask.Result.Pass, TestStartupTask.Result.Fail);
			TestStartupTask task1 = new TestStartupTaskWithFailureExitCode(true, TestStartupTask.Result.Pass, -2);
			TestStartupTask task2 = new TestStartupTaskWithFailureExitCode(true, TestStartupTask.Result.Throw, -3);
			TestStartupTask task3 = new TestStartupTaskWithFailureExitCode(true, TestStartupTask.Result.Pass, -4);
			Director.OverrideStartupTasks = new IApplicationStartupTask[] { exceptionHandler, task1, task2, task3 };
			int result = Director.StartEnterprise(new string[] { "" });
			Assert("Enterprise should not run", !Director.IsEnterpriseRun);
			AssertEquals("StartEnterprise should return -3", -3, result);
			Assert("Task1 should have executed", task1.Executed);
			Assert("Task2 should have executed", task2.Executed);
			Assert("Task3 should not have executed", !task3.Executed);
		}

		public void TestRunEnterpriseStartupTasksThrowExceptionHandledByExceptionReporter()
		{
			var exceptionHandler = new ApplicationStartupDirector.EndStartupErrorHandler();
			TestStartupTask task1 = new TestStartupTask(true, TestStartupTask.Result.Throw);
			Director.OverrideStartupTasks = new IApplicationStartupTask[] { exceptionHandler, task1 };

			Director.StartEnterprise(new[] { "" });
			Assert("Exception should be handled by ExceptionReporter", ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Count == 1);
			ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestStartEnterpriseReturnsExitCodeOfTestClientWhenRunningTest()
		{
			//Arrange & Act
			var result = Director.StartEnterprise(new[] { TestClient.TestClientMode, TestClient.TestClientRunAction, "TestAdapterQualifiedName", "sourcePath", "binPath", "25875", TestAdapterContext.Empty().Encode() });

			//Assert
			AssertEquals(DatExitCodes.TestClientExecuteError, result);
		}

		public void TestProgressPercentage()
		{
			const int totalNumberOfTasks = 148;
			CombineAssertions(() =>
			{
				AssertEquals("6 percent", 6, ApplicationStartupDirector.ProgressPercentageComplete(10, totalNumberOfTasks));
				AssertEquals("23 percent", 23, ApplicationStartupDirector.ProgressPercentageComplete(35, totalNumberOfTasks));
				AssertEquals("46 percent", 46, ApplicationStartupDirector.ProgressPercentageComplete(69, totalNumberOfTasks));
				AssertEquals("62 percent", 62, ApplicationStartupDirector.ProgressPercentageComplete(93, totalNumberOfTasks));
				AssertEquals("74 percent", 74, ApplicationStartupDirector.ProgressPercentageComplete(110, totalNumberOfTasks));
				AssertEquals("89 percent", 89, ApplicationStartupDirector.ProgressPercentageComplete(133, totalNumberOfTasks));
				AssertEquals("100 percent", 100, ApplicationStartupDirector.ProgressPercentageComplete(totalNumberOfTasks, totalNumberOfTasks));
			});
		}

		#endregion

		#region Assembly loading / Task ordering

#if !NET8_0_OR_GREATER // Should be fixed in WI00669071: Remove usages of AppDomains
		public void TestAssembliesLoadedBeforeSplash()
		{
			AppDomain appDomain = AppDomain.CreateDomain("TestAssembliesLoadedBeforeSplash");
			appDomain.SetData("serverName", Db.ServerName);
			appDomain.SetData("databaseName", Db.DatabaseName);
			try
			{
				appDomain.DoCallBack(delegate
				{
					string serverName = (string)AppDomain.CurrentDomain.GetData("serverName");
					string databaseName = (string)AppDomain.CurrentDomain.GetData("databaseName");
					TestApplicationStartupDirector director = new TestApplicationStartupDirector();
					director.OverrideStartupTasks = Array.Empty<IApplicationStartupTask>();
					director.StartEnterprise(new string[] { serverName, databaseName });
				});

				var expectedAssemblies = new List<string>
				{
					"mscorlib",
					"CargoWise.Main",
					"NUnitCore",
					"Dat.Integration",
					"System",
					"System.Configuration",
					"System.Xml",
					"System.Windows.Forms",
					"System.Drawing",
					"WindowsBase",
					"CargoWise.Async",
					"CargoWise.Common",
					"CargoWise.Data",
					"CargoWise.Shared.40",
					"CargoWise.Loader.Common",
					"CargoWise.Windows.UI",
					"CargoWise.BrandManager",
					"Enterprise.Integration",
					"Enterprise.ZArchitecture.Core",
					"Enterprise.ZArchitecture.GUI",
					"Enterprise.ZArchitecture.Modules",
					"Enterprise.URLHandler",
					"Enterprise.URLHandler.Integration",
					Path.GetFileNameWithoutExtension(ExeFileNames.CargoWiseWindowsDesktopExe),
					"CargoWise.WindowsDesktop.Test"
				};

				var exceptedAssemblies = new List<string>()
				{
					"Accessibility",
					"System.Core",
					"netstandard"
				};

				Thread.Sleep(1000);
				var actualAssemblies = appDomain.GetAssemblies().Where(a => !CanIgnoreLoadedAssemblyBecauseDebugger(a)).Select(a => a.GetName().Name).Except(exceptedAssemblies);

				AssertContainsExactElementsInAnyOrder(expectedAssemblies, actualAssemblies);
			}
			finally
			{
				AppDomain.Unload(appDomain);
			}
		}
#endif

		public void TestConsoleDbUpgraderErrorMessageAreShownOnConsole()
		{
			var proc = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = Path.Combine(AssemblyLoader.GetBinPath(), ExeFileNames.CargoWiseWindowsDesktopExe),
					Arguments = Db.ServerName + " " + "InvalidDbName" + " " + ApplicationArguments.OptionConsoleUpgrader + " " + ApplicationArguments.OptionNoSplash,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					CreateNoWindow = true
				}
			};
			proc.Start();
			string line = null;
			while (!proc.StandardOutput.EndOfStream)
			{
				line = proc.StandardOutput.ReadLine();
			}
			AssertNotNull(line);
		}

#if !NET8_0_OR_GREATER // Should be fixed in WI00669071: Remove usages of AppDomains
		public void TestCallingTasksShouldExecuteDoesNothing()
		{
			AppDomain appDomain = AppDomain.CreateDomain("TestCallingShouldExecuteDoesNothing");
			appDomain.SetData("serverName", Db.ServerName);
			appDomain.SetData("databaseName", Db.DatabaseName);
			try
			{
				appDomain.DoCallBack(delegate
				{
					string serverName = (string)AppDomain.CurrentDomain.GetData("serverName");
					string databaseName = (string)AppDomain.CurrentDomain.GetData("databaseName");
					TestApplicationStartupDirector director = new TestApplicationStartupDirector();
					director.OverrideStartupTasks = Array.Empty<IApplicationStartupTask>();
					director.StartEnterprise(new string[] { serverName, databaseName });
				});

				Assembly[] loadedBeforeSplash = appDomain.GetAssemblies();

				appDomain.DoCallBack(delegate
				{
					TestApplicationStartupDirector director = new TestApplicationStartupDirector();
					IApplicationStartupTask[] tasks = director.GetEnterpriseStartupTasksForTest();
				});

				string[] allowedToLoadInConstruction = new string[]
				{
					"Enterprise.Initialisation",
					"System.Core",
					"Enterprise.Main.WebDeploy",
					"Enterprise.DbUpgrader.Shared",
				};

				Assert("Too many assemblies allowed to be loaded in task construction", allowedToLoadInConstruction.Length < 5);
				Assembly[] loadedInConstruction = appDomain.GetAssemblies();
				foreach (Assembly assembly in loadedInConstruction)
				{
					if (!CanIgnoreLoadedAssemblyBecauseDebugger(assembly) && Array.IndexOf(loadedBeforeSplash, assembly) == -1)
					{
						AssertCollectionContains("Assembly loaded in task construction: " + assembly.GetName().Name + "\r\n\r\nAllowed assemblies:\r\n" + string.Join("\r\n", allowedToLoadInConstruction), assembly.GetName().Name, allowedToLoadInConstruction);
					}
				}

				appDomain.DoCallBack(delegate
				{
					CommandLineArguments arguments = new ApplicationArguments(Array.Empty<string>());
					TestApplicationStartupDirector director = new TestApplicationStartupDirector();
					IApplicationStartupTask[] tasks = director.GetEnterpriseStartupTasksForTest();
					foreach (IApplicationStartupTask task in tasks)
					{
						task.ShouldExecute(arguments);
					}
				});

				string[] allowedToLoadInShouldExecute = new string[]
				{
					"Enterprise.Environment",
					"Enterprise.Licensing.Core",
					"CargoWise.IO",
					"CargoWise.Types",
					"CargoWise.ApplicationContext",
					"System.Collections.Immutable",
					"System.Runtime",
					"System.Collections",
					Path.GetFileNameWithoutExtension(ExeFileNames.CargoWiseWindowsDesktopExe) + ".Strong",
				};

				Assert("Too many assemblies allowed to be loaded in ShouldExecute()", allowedToLoadInShouldExecute.Length < 10);
				foreach (Assembly assembly in appDomain.GetAssemblies())
				{
					if (!CanIgnoreLoadedAssemblyBecauseDebugger(assembly) && Array.IndexOf(loadedBeforeSplash, assembly) == -1 && Array.IndexOf(loadedInConstruction, assembly) == -1)
					{
						AssertCollectionContains("Assembly loaded in call to should execute: " + assembly.GetName().Name + "\r\n\r\nAllowed assemblies:\r\n" + string.Join("\r\n", allowedToLoadInShouldExecute), assembly.GetName().Name, allowedToLoadInShouldExecute);
					}
				}
			}
			finally
			{
				AppDomain.Unload(appDomain);
			}
		}

		public void TestAssembliesLoadedBeforeMainForm()
		{
			AppDomain appDomain = AppDomain.CreateDomain("TestAssembliesLoadedBeforeMainForm");
			appDomain.SetData("serverName", Db.ServerName);
			appDomain.SetData("databaseName", Db.DatabaseName);
			URLHandlerServiceRegistration.UnregisterServer();
			try
			{
				appDomain.DoCallBack(delegate
				{
					NUnit.Framework.TestingState.Setup();
					string serverName = (string)AppDomain.CurrentDomain.GetData("serverName");
					string databaseName = (string)AppDomain.CurrentDomain.GetData("databaseName");
					TestApplicationStartupDirector director = new TestApplicationStartupDirector();
					director.StartEnterprise(new string[] { serverName, databaseName, ApplicationArguments.OptionNoSplash });
					URLHandlerServiceRegistration.UnregisterServer();
				});

				var type = typeof(AppDomainAssemblyHelper);
				var testDirector = (AppDomainAssemblyHelper)appDomain.CreateInstanceAndUnwrap(type.Assembly.GetName().Name, type.FullName);
				var assemblies = testDirector.LoadedAssemblies();

				var expectedAssemblies = new SortedSet<string>(new[] {
					// Microsoft
					"mscorlib",
					"netstandard",
					"Accessibility",
					"Microsoft.Extensions.DependencyInjection.Abstractions",
					"Microsoft.Extensions.DependencyInjection",
					"Microsoft.Extensions.Logging.Abstractions",
					"Microsoft.Bcl.AsyncInterfaces",
					"Microsoft.Bcl.TimeProvider",
					"Microsoft.SqlServer.Types",

					// 3rd Party
					"Accord",
					"NUnitCore",
					"Dat.Integration",
					"Moq",
					"YamlDotNet",
					"Newtonsoft.Json",

					// Blazor Hybrid
					"Enterprise.BlazorWinFormsInterop",
					"Enterprise.BlazorWinFormsInterop.Interfaces",

					// CargoWise Architecture
					"CargoWise.ActiveDirectory",
					"CargoWise.ApplicationContext",
					"CargoWise.ApplicationContext.XmlSerializers",
					"CargoWise.ApplicationManager.Common",
					"CargoWise.Async",
					"CargoWise.Authentication.Glow.Client", // required for Clear Glow User Data
					"CargoWise.Authentication.Primitives", // required for Clear Glow User Data
					"CargoWise.BrandManager",
					"CargoWise.CalendarArithmetic",
					"CargoWise.Common",
					"CargoWise.Common.Testing",
					"CargoWise.ComponentModel",
					"CargoWise.Definitions",
					"CargoWise.Definitions.XmlSerializers",
					"CargoWise.Database.Shared",
					"CargoWise.Data",
					"CargoWise.Data.Shared",
					"CargoWise.Data.Providers.Common",
					"CargoWise.Data.HttpClient",
					"CargoWise.Data.SqlProxy.Interface",
					"CargoWise.Database.Abstractions",
					"CargoWise.EntityFramework",
					"CargoWise.FeatureControl",
					"CargoWise.FeatureControl.Abstractions",
					"CargoWise.Integration",
					"CargoWise.Licensing",
					"CargoWise.Loader.Common",
					"CargoWise.Macros",
					"CargoWise.Main",
					"CargoWise.Main.Navigation",
					"CargoWise.Schema",
					"CargoWise.Odyssey.Schema",
					"CargoWise.Shared.40",
					"CargoWise.Types",
					"CargoWise.Windows.UI",
					"CargoWise.ResourceStrings.Cache",
					"CargoWise.DataProtection",
					"CargoWise.DataProtection.SqlExtensions",
					"CargoWise.DataProtection.Administration",
					"CargoWise.DataProtection.Administration.SqlServer",
					"CargoWise.ResourceStrings",

					// Enterprise Architecture
					"WTG.Rules",
					"Enterprise.ZArchitecture.Business",
					"Enterprise.ZArchitecture.Core",
					"Enterprise.ZArchitecture.Core.Test",
					"Enterprise.ZArchitecture.Favorites",
					"Enterprise.ZArchitecture.GlowInterop", // required for Clear Glow User Data
					"Enterprise.ZArchitecture.GUI",
					"Enterprise.ZArchitecture.Modules",
					"Enterprise.ZArchitecture.Modules.Testing", // For Dummy module listing
					"Enterprise.ZArchitecture.Schema",
					"Enterprise.ZArchitecture.Core.Encryption",

					// Module listing subset
					"Enterprise.Customs.Common.ModuleRegistration",
					"Enterprise.Customs.ZA.ModuleRegistration",

					// Enterprise General
					"BuildTools",
					"Resources",
					"Enterprise.Accounting.Business",
					"Enterprise.ActivityLogger",
					"Enterprise.BufferManagement.Integration",
					"Enterprise.Customs.Business",
					"Enterprise.Customs.Universal",
					"Enterprise.DbUpgrader.Resource.Version",
					"Enterprise.DbUpgrader.Shared",
					"Enterprise.DbUpgrader.Transformation.Common",
					"Enterprise.DocumentEngineCore",
					"Enterprise.DocumentEngine",
					"Enterprise.DocumentEngine.GUI",
					"Enterprise.DocumentEngineIntegration",
					"Enterprise.Environment",
					"Enterprise.Initialisation",
					"Enterprise.Integration",
					"Enterprise.Licensing",
					"Enterprise.Licensing.Billing.Business",
					"Enterprise.Licensing.Core",
					"Enterprise.Main.ModuleTreeLoader",
					"Enterprise.MasterData.Common",
					"Enterprise.MasterFiles.Business",
					"Enterprise.MasterFiles.Integration",
					"Enterprise.Messaging.Business",
					"Enterprise.Metadata.Integration",
					"Enterprise.ProductRegistration.Client",
					"Enterprise.Registry.Business",
					"Enterprise.RemoteDesktopServices.Shared",
					"Enterprise.RemoteDesktopServices.Server",
					"Enterprise.ResourceStrings.Business",
					"Enterprise.Security",
					"Enterprise.Security.ActiveDirectory",
					"Enterprise.Security.ActiveDirectory.GUI",
					"Enterprise.Security.ActiveDirectory.Integration",
					"Enterprise.Security.Core",
					"Enterprise.Semaphores.Common",
					"Enterprise.UniversalDataBuss.Integration",
					"Enterprise.Upgrades",
					"Enterprise.URLHandler",
					"Enterprise.URLHandler.Integration",
					"Enterprise.BufferManagement.Business",
					"Enterprise.TimeEngineScheduler.Integration",
					"Enterprise.UniversalCopy.Business",
					"Enterprise.ProductRegistration.GUI",
					"Enterprise.Rating.GUI",
					"Enterprise.TransportCommon.Integration",
					"Enterprise.TransportCommon.Registry",
					"Enterprise.Workflow.Business",
					"Enterprise.Workflow.Integration",
					"CargoWise.Glow.Model.Interfaces",
					"CargoWise.Tools.DuplicateDetector",
					"Enterprise.DeniedPartyScreening.Integration",
					"Enterprise.ComplianceRisk.Integration",
					"Enterprise.Freight",
					"Enterprise.Freight.CarbonEmissions.Business",
					"Enterprise.Freight.Integration",
					"Enterprise.Freight.Module",
					"WTG.Data.SqlClient", // Dependency of Enterprise.Upgrades

					// Tile Navigation Bar - WPF
					"CargoWise.GUI.TileBar",
					"UIAutomationProvider",
					"UIAutomationTypes",
					"WindowsBase",
					"WindowsFormsIntegration",
					"ServiceManager.Shared.Abstractions",
					"ServiceManager.Shared.CW.Abstractions",
					"ServiceManager.Shared.CW",
					"Enterprise.StabilityChecker",
					"Microsoft.Xaml.Behaviors",

					//HostsController (BizOFactory.OnSaved extension aka Nudging)
					"ServiceManager.Integration.NudgingClient.Abstractions",
					"ServiceManager.Integration.NudgingClient",
					"ServiceManager.Integration.ServiceHostClient.Abstractions",
					"ServiceManager.Integration.Abstractions",
					"ServiceManager.Integration.CW",
					"ServiceManager.Integration.ServiceTasks.CW",
					"WTG.StaticAnalysis.Annotation", // DEBUG only attributes - does not effect release build

					"WTG.AddressCleansing.Common", // Address Validation Service
					"WTG.Statistics",	// required to load data from database.
					"WTG.SecurityRights", //for SecurityCalculator for SecurityIterator
					"WTG.Foundation.Cryptography.UserSecrets", // required for password hashing
					"WTG.Foundation.FrameworkExtensions", // referenced by MasterFiles.Business; no further dependencies outside framework
					"WTG.Foundation.Http", // required for Clear Glow User Data
					"WTG.OpenIDConnect.Login", // required for OIDC login
					"WTG.TestHelpers", // required for tests
					"Enterprise.Recruitment.Registry",
					"AppDomainWrappers.Net",
					"WiseCloud.Shared.Security.ParameterVerification"
				});

				if (Enterprise.Registry.Business.GlowRegistry.Instance.GlowUseIndexingServiceForGlobalSearch.Value)
				{
					expectedAssemblies.Add("Newtonsoft.Json");
				}

				expectedAssemblies.Add(Path.GetFileNameWithoutExtension(ExeFileNames.CargoWiseWindowsDesktopExe));
				expectedAssemblies.Add("CargoWise.WindowsDesktop.Test");
				var actualAssemblies = new SortedSet<string>(assemblies.Where(assembly => !CanIgnoreLoadedAssembly(assembly) && !assembly.StartsWith("System") && !assembly.StartsWith("Presentation") && !assembly.StartsWith("Anonymously")));

				var notExpectedAssemblies = actualAssemblies.Except(expectedAssemblies).ToArray();
				var missingAssemblies = expectedAssemblies.Except(actualAssemblies).ToArray();

				var message = new ZStringBuilder();
				if (notExpectedAssemblies.Length > 0)
				{
					message.Append("The following assemblies were found but NOT expected:\r\n" + string.Join("\r\n", notExpectedAssemblies.ToArray()));

					if (missingAssemblies.Length > 0)
					{
						message.Append("\r\n\r\n");
					}
				}

				if (missingAssemblies.Length > 0)
				{
					message.Append("The following assemblies were NOT found but were expected:\r\n" + string.Join("\r\n", missingAssemblies.ToArray()));
				}

				Assert(message.ToString(), actualAssemblies.SequenceEqual(expectedAssemblies));
			}
			finally
			{
				AppDomain.Unload(appDomain);
				TaskTestListener.Instance.ExpectTask("await pipeService.WaitForConnectionAsync in URLHandlerServiceRegistration");
				TaskTestListener.Instance.ExpectTask("await pipeService.WaitForConnectionAsync in URLHandlerServiceRegistration");
				EnterpriseUrlHandlerService.Instance.RegisterRemotingServer();
				EnterpriseUrlHandlerService.Instance.RegisterInstance();
			}
		}
#endif

		public void TestGetNonInteractiveUpgradeStartupTasks()
		{
			// Arrange
			var tasks = new TestApplicationStartupDirector().GetEnterpriseStartupTasksForTest(new[] { ApplicationArguments.OptionScheduledDbUpgrader });

			// Act
			var result = tasks
				.Select(task => task.GetType())
				.ToArray();

			// Assert
			AssertArrayEqualsByElements(new[]
				{
					typeof(StartupNotification.InitializationTask),
					typeof(ApplicationStartupDirector.EnterStartupErrorHandler),
					typeof(ApplicationStartupDirector.EnterScheduledUpgraderErrorHandler),
					typeof(Initialiser.InitialiseWinFormsTask),
					typeof(ValidateDbArguments),
					typeof(SetupDbConnection),
					typeof(ValidatePurgeDataRunStatus),
					typeof(ProductBrandingRegistryDeterminerTask),
					typeof(InstallCurrentVersionTask),
					typeof(StartupCheckClientDll),
					typeof(ScheduledUpgraderDirector),
					typeof(RemoveOldUpgradePackagesTask),
					typeof(RemoveOldInstallationsTask),
					typeof(TempFileCleanupTask),
					typeof(ResourceStringsUpdaterTask),
					typeof(SlowBackgroundApplicationStartupTask),
					typeof(ExitApplicationTask),
				},
				result);
		}

#if !NET8_0_OR_GREATER // Should be fixed in WI00669071: Remove usages of AppDomains
		public void TestNoDbAccessBeforeSetupDbConnection()
		{
			string[] loadedAssemblyNames = RunStartupUntilTaskAndReturnLoadedAssemblyNames(typeof(SetupDbConnection));
			Assert("System.Data assembly should not be loaded before SetupDbConnection", Array.IndexOf(loadedAssemblyNames, "System.Data") == -1);
		}
#endif

		public void TestCheckClientDllAfterInstallCurrentVersionBeforeDbUpgrade()
		{
			bool checkedClientDll = false;
			foreach (IApplicationStartupTask task in new TestApplicationStartupDirector().GetEnterpriseStartupTasksForTest())
			{
				if (task is StartupCheckClientDll)
				{
					checkedClientDll = true;
				}
				else if (task is InstallCurrentVersionTask)
				{
					Assert("InstallCurrentVersion should go before client dll check to allow client dll transitions to be applied on clients without errors",
						!checkedClientDll);
				}
				else if (task is DbUpgraderDirector)
				{
					Assert("DbGraderDirector should go after client dll check to prevent upgrading to a incompatible package",
						checkedClientDll);
				}
			}
		}

		public void TestScreenResolutionCheckOnlyWhenInteractiveExecution()
		{
			var tasks = new TestApplicationStartupDirector().GetEnterpriseStartupTasksForTest();
			Assert(tasks.Where(t => t is ScreenResolutionChecker).FirstOrDefault() != null);

			tasks = new TestApplicationStartupDirector().GetEnterpriseStartupTasksForTest(new string[] { ApplicationArguments.OptionKeepConsoleOpenOnError });
			Assert(tasks.Where(t => t is ScreenResolutionChecker).FirstOrDefault() != null);

			tasks = new TestApplicationStartupDirector().GetEnterpriseStartupTasksForTest(new string[] { ApplicationArguments.OptionScheduledDbUpgrader });
			Assert(tasks.Where(t => t is ScreenResolutionChecker).FirstOrDefault() == null);
		}

		public void TestCheckAvailableDiskSpaceAfterSetupDbConnection()
		{
			var tasks = new TestApplicationStartupDirector().GetEnterpriseStartupTasksForTest();
			Assert("StartupCheckAvailableDiskSpace must come after SetupDbConnection", Array.FindIndex(tasks, t => t is SetupDbConnection) < Array.FindIndex(tasks, t => t is StartupCheckAvailableDiskSpace));
		}

		bool CanIgnoreLoadedAssemblyBecauseDebugger(Assembly assembly)
		{
			return CanIgnoreLoadedAssemblyBecauseDebugger(assembly.GetName().Name);
		}

		bool CanIgnoreLoadedAssembly(string assembly)
		{
			var ignoreAssemblies = new[] {
				"Microsoft.Win32.Registry", // It is loaded by ValidateDbArguments only when the version number is greater than ParameterVerificationConstants.VersionCutoff.
			};

			return ignoreAssemblies.Contains(assembly) || CanIgnoreLoadedAssemblyBecauseDebugger(assembly);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1058:DoNotUseDebuggerIsAttached", Justification = "Baseline")]
		bool CanIgnoreLoadedAssemblyBecauseDebugger(string assembly)
		{
			return System.Diagnostics.Debugger.IsAttached && assembly.StartsWith("Microsoft.VisualStudio");
		}

#if !NET8_0_OR_GREATER // Should be fixed in WI00669071: Remove usages of AppDomains
		string[] RunStartupUntilTaskAndReturnLoadedAssemblyNames(Type taskType)
		{
			AppDomain appDomain = AppDomain.CreateDomain("TestNoDbAccessBeforeSetupDbConnection");
			appDomain.SetData("currentUserName", Env.CurrentUser.LoginName);
			appDomain.SetData("currentBranch", Env.CurrentBranch.PK);
			appDomain.SetData("currentBranchCode", Env.CurrentBranch.Code);
			appDomain.SetData("currentDepartment", Env.CurrentDepartment.PK);
			appDomain.SetData("serverName", Db.ServerName);
			appDomain.SetData("databaseName", Db.DatabaseName);
			appDomain.SetData("taskTypeName", taskType.FullName);
			appDomain.SetData("serverDirectory", Env.TempPath);
			try
			{
				appDomain.DoCallBack(delegate
				{
					string currentBranchCode = (string)AppDomain.CurrentDomain.GetData("currentBranchCode");
					string serverName = (string)AppDomain.CurrentDomain.GetData("serverName");
					string databaseName = (string)AppDomain.CurrentDomain.GetData("databaseName");
					string taskTypeName = (string)AppDomain.CurrentDomain.GetData("taskTypeName");
					string serverDirectory = (string)AppDomain.CurrentDomain.GetData("serverDirectory");
					var args = new string[] { serverName, databaseName, ApplicationArguments.OptionNoSplash, @"-SDir:" + serverDirectory, "-Branch:" + currentBranchCode };

					TestApplicationStartupDirector director = new TestApplicationStartupDirector();
					IApplicationStartupTask[] tasks = director.GetEnterpriseStartupTasksForTest(args);

					int i;
					for (i = 0; i < tasks.Length; i++)
					{
						if (tasks[i].GetType().FullName == taskTypeName)
						{
							break;
						}
					}
					IApplicationStartupTask[] tasksToRun = new IApplicationStartupTask[i];
					Array.Copy(tasks, tasksToRun, i);
					director.OverrideStartupTasks = tasksToRun;

					int returnValue = director.StartEnterprise(args);

					var appDomainWrapper = new AppDomainWrapper();
					Assembly[] asms = appDomainWrapper.GetAssemblies();
					string[] asmNames = new string[asms.Length];
					for (i = 0; i < asms.Length; i++)
					{
						asmNames[i] = asms[i].GetName().Name;
					}
					AppDomain.CurrentDomain.SetData("loadedAssemblyNames", asmNames);
					AppDomain.CurrentDomain.SetData("returnValue", returnValue);
				});

				AssertEquals("all executed tasks should have succeeded", 0, (int)appDomain.GetData("returnValue"));
				string[] loadedAssemblyNames = (string[])appDomain.GetData("loadedAssemblyNames");
				Assert("loaded assembly array should contain all loaded assemblies", Array.IndexOf(loadedAssemblyNames, "System.Xml") > -1);
				return loadedAssemblyNames;
			}
			finally
			{
				AppDomain.Unload(appDomain);
			}
		}
#endif

		#endregion

		#region Executing ediEnterprise Url

		[GuiTest]
		public void TestExecuteUrl()
		{
			TaskTestListener.Instance.ExpectTask("await pipeService.WaitForConnectionAsync in URLHandlerServiceRegistration");

			EnterpriseUrlHandlerService.Instance.RegisterInstance();

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Factory.Save();
			AssertEquals("Form not be shown initially for the test", false, IsDummyFormOpen);

			var url = ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, dummy.PK);
			var threadCompleted = false;

			ThreadPool.QueueUserWorkItem(delegate
			{
				Director.StartEnterprise(new[] { url });
				threadCompleted = true;
			});

			while (!threadCompleted)
			{
				// allow the ui thread to process messages while the url request is processed by the EnterpriseUrlHandler remoting server
				Application.DoEvents();
				Thread.Sleep(100);
			}

			AssertEquals("Form opened for edit", true, IsDummyFormOpen);
		}

		bool IsDummyFormOpen
		{
			get { return OpenDummyForm != null; }
		}

		Form OpenDummyForm
		{
			get
			{
				foreach (Form form in Application.OpenForms)
				{
					if (form is ZDummyForm)
					{
						return form;
					}
				}
				return null;
			}
		}

		#endregion

		#region SlowBackgroundApplicationStartupTask

		public void TestSlowBackgroundApplicationStartupTaskFinshesWithoutGui()
		{
			var process = Process.Start(Path.Combine(AssemblyLoader.GetBinPath(), ExeFileNames.CargoWiseWindowsDesktopExe), Db.ServerName + " " + Db.DatabaseName + " " + ApplicationArguments.OptionConsoleUpgrader + " " + ApplicationArguments.OptionNoSplash + " " + ApplicationArguments.OptionSlowBackgroundApplicationStartupTask + " " + ApplicationArguments.OptionTestAdapter);
			process.WaitForExit();
			AssertEquals("Finished", File.ReadAllText(Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Temp.TempPath.Replace("CargoWiseOneDat", "CargoWiseOne"))), process.Id.ToString(), "SlowBackgroundApplicationStartupTask")));
		}

		#endregion

		#region BrandingTests

		[GuiTest]
		public void TestCommandArgumentBrandingProductName()
		{
			int result = Director.StartEnterprise(new string[] { "-PW" });
			AssertEquals("Branding not set correctly", "ProductivityWise", BrandingFactory.Instance.ProductName);

			result = Director.StartEnterprise(new string[] { "" });
			AssertEquals("Branding not set correctly", "CargoWise", BrandingFactory.Instance.ProductName);
		}

		public void TestBrandingProductNameIsConfiguredForNonInteractiveStartup()
		{
			const string testProductName = nameof(TestBrandingProductNameIsConfiguredForNonInteractiveStartup);
			var brandingFactoryMock = new Mock<IBranding>();
			brandingFactoryMock.Setup(x => x.ProductName).Returns(testProductName);

			Func<IApplicationStartupTask[], IApplicationStartupTask[]> afterGetEnterpriseStartupTasksAction = tasks =>
			{
				var filteredTasks = new List<IApplicationStartupTask>();

				foreach (var task in tasks)
				{
					if (task is ProductBrandingRegistryDeterminerTask brandingTask)
					{
						filteredTasks.Add(brandingTask);
					}
				}

				return filteredTasks.ToArray();
			};

			using (Director.SetAfterGetEnterpriseStartupTasksAction(afterGetEnterpriseStartupTasksAction))
			using (BrandingFactory.ConfigureTemporary(() => brandingFactoryMock.Object))
			{
				_ = Director.StartEnterprise(new[] { ApplicationArguments.OptionScheduledDbUpgrader });

				AssertNotEquals("Branding has not been initialized", testProductName, BrandingFactory.Instance.ProductName);
				AssertEquals("Branding not set correctly", "CargoWise", BrandingFactory.Instance.ProductName);
			}
		}

		#endregion

		#region Test Classes

		public class TestApplicationStartupDirector : ApplicationStartupDirector
		{
			public IApplicationStartupTask[] OverrideStartupTasks;
			public bool IsEnterpriseRun;

			public TestApplicationStartupDirector()
			{
			}

			protected override IApplicationStartupTask[] GetEnterpriseStartupTasks(CommandLineArguments arguments)
			{
				if (OverrideStartupTasks != null)
				{
					return OverrideStartupTasks;
				}
				else
				{
					var tasks = base.GetEnterpriseStartupTasks(arguments);

					if (afterGetEnterpriseStartupTasksAction != null)
					{
						tasks = afterGetEnterpriseStartupTasksAction.Invoke(tasks);
					}

					return tasks;
				}
			}

			public IDisposable SetAfterGetEnterpriseStartupTasksAction(Func<IApplicationStartupTask[], IApplicationStartupTask[]> func)
			{
				afterGetEnterpriseStartupTasksAction = func;
				return new DisposableAction(() => afterGetEnterpriseStartupTasksAction = null);
			}

			Func<IApplicationStartupTask[], IApplicationStartupTask[]> afterGetEnterpriseStartupTasksAction;

			public IApplicationStartupTask[] GetEnterpriseStartupTasksForTest()
			{
				return base.GetEnterpriseStartupTasks(new ApplicationArguments(Array.Empty<string>()));
			}

			public IApplicationStartupTask[] GetEnterpriseStartupTasksForTest(string[] args)
			{
				return base.GetEnterpriseStartupTasks(new ApplicationArguments(args));
			}

			public bool RunEnterpriseStartupTasksForTest(string[] args)
			{
				return RunEnterpriseStartupTasks(ParseArguments(args));
			}

			protected internal override void DisposeSplash()
			{
				splash?.Dispose();
			}

			protected override void Application_RunCore(Form mainForm)
			{
				IsEnterpriseRun = true;
			}

			public void Application_ThreadExit_Exposed(object sender, EventArgs e)
				=> Application_ThreadExit(sender, e);
		}

		public class TestStartupTask : AbstractApplicationStartupTask
		{
			public enum Result
			{
				Pass,
				Fail,
				Throw,
				ThrowDbUpgradedException
			}

			public TestStartupTask(bool shouldExecute, Result shouldo)
			{
				this.shouldExecute = shouldExecute;
				this.shouldo = shouldo;
			}

			readonly bool shouldExecute;

			readonly Result shouldo;
			public bool Executed
			{
				get;
				private set;
			}

			public CommandLineArguments PassedArguments
			{
				get;
				private set;
			}

			protected override bool GetShouldExecute(CommandLineArguments arguments)
			{
				return shouldExecute;
			}

			protected override bool DoExecute(CommandLineArguments arguments)
			{
				Executed = true;
				PassedArguments = arguments;
				switch (shouldo)
				{
					case Result.ThrowDbUpgradedException:
						throw new DatabaseUpgradedException();
					case Result.Throw:
						throw new Exception("Task exception");
					case Result.Fail:
						return false;
					case Result.Pass:
					default:
						return true;
				}
			}

			public override string TaskDescription => "Testing";

			public override int FailureExitCode => ExitCodes.TestExitCode;
		}

		public class TestStartupTaskExecuteWithExectuteOnFailure : TestStartupTask, IApplicationStartupTaskExecuteOnFailure
		{
			public TestStartupTaskExecuteWithExectuteOnFailure(bool shouldExecute, Result shouldo)
				: base(shouldExecute, shouldo)
			{ }

			public bool ExecutedOnFailure
			{
				get;
				private set;
			}

			public void ExecuteOnFailure(CommandLineArguments arguments)
			{
				ExecutedOnFailure = true;
			}
		}

		public class TestStartupTaskExecuteWithExceptionHandler : TestStartupTask, IApplicationStartupTaskExceptionHandler
		{
			public TestStartupTaskExecuteWithExceptionHandler(bool shouldExecute, Result shouldo, Result shouldoInHandleException)
				: base(shouldExecute, shouldo)
			{
				this.shouldoInHandleException = shouldoInHandleException;
			}

			readonly Result shouldoInHandleException;

			public bool HandledException
			{
				get;
				private set;
			}

			public bool HandleException(Exception e, CommandLineArguments arguments)
			{
				HandledException = true;

				switch (shouldoInHandleException)
				{
					case Result.ThrowDbUpgradedException:
						throw new DatabaseUpgradedException();
					case Result.Throw:
						throw new Exception("Task exception");
					case Result.Fail:
						return false;
					case Result.Pass:
					default:
						return true;
				}
			}
		}

		public class TestStartupTaskWithFailureExitCode : TestStartupTask
		{
			public TestStartupTaskWithFailureExitCode(bool shouldExecute, Result shouldo, int exitCode)
				: base(shouldExecute, shouldo)
			{
				expectedExitCode = exitCode;
			}

			readonly int expectedExitCode;
			public override int FailureExitCode { get => expectedExitCode; }
		}

		#endregion

		#region Implementation

		TestApplicationStartupDirector Director;
		BusinessObjectFactory Factory;

		protected override void SetUp()
		{
			Factory = new BusinessObjectFactory();
			base.SetUp();

			Director = new TestApplicationStartupDirector();
			GlbStaff nonDeveloperStaff = Factory.NewWithValidTestData<GlbStaff>();
			nonDeveloperStaff.GS_LoginName = "nondeveloper";

			var security1 = Factory.NewWithValidTestData<GlbSecurity>();
			security1.GU_GS = nonDeveloperStaff.PK;
			security1.GU_SecurityItemIsAllowed = true;
			security1.GU_SecurityRight = "Login";

			Factory.Save();
		}

		protected override void TearDown()
		{
			ZFormActivityLogger.Instance.DisableActivityLogger();
			base.TearDown();
			if (OpenDummyForm != null)
			{
				OpenDummyForm.Dispose();
			}
		}

		#endregion
	}

	public class AppDomainAssemblyHelper : MarshalByRefObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1157:Do not use System.AppDomain.", Justification = "Pending migration")] // WI00669071 - Do not use System.AppDomain.
		public string[] LoadedAssemblies()
		{
			var appDomainWrapper = new AppDomainWrapper();
			return appDomainWrapper.GetAssemblies().Select(x => x.GetName().Name).ToArray();
		}
	}
}
