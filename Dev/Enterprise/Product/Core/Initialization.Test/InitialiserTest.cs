using System;
using System.Globalization;
using System.IO;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.Environment.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Initialisation
{
	public class InitialiserTest : BaseInitialiserTest
	{
		public void TestInitialiseWinForms()
		{
			AssertEquals("ExceptionReporter setup", typeof(BaseExceptionReporter), ExceptionReporter.Instance.GetType());
			AssertEquals("Environment of correct type", typeof(WinFormsEnvironmentProvider), Env.GetCurrentProvider().GetType());
			AssertEquals("PropertyChangeLogger setup", expected: true, Enterprise.MasterFiles.Business.PropertyChangeLogger.IsInitialized);
			AssertNotNull("InteractiveNotification", Globals.InteractiveNotification);
		}

#if !WINZOR
		public void TestEnvironmentPropertiesAreSetToTheCorrectValueOnStartup()
		{
			// NOTE: Requires net8.0\Enterprise.ZArchitecture.Web.Utilities.dll for this to work for net8.0.
			CombineAssertions(() =>
			{
				AssertEnvironmentProperties("InitialiseBatchProcessor", expectedIsUserInteractive: false, expectedIsPostableProcess: false, typeof(BaseExceptionReporter), typeof(WinFormsEnvironment), typeof(BaseDbEnvironment));
				AssertEnvironmentProperties("InitialiseConsoleApp", expectedIsUserInteractive: true, expectedIsPostableProcess: false, null, typeof(NullEnvProvider.NullEnv), typeof(BaseDbEnvironment));
				AssertEnvironmentProperties("InitialiseWinForms", expectedIsUserInteractive: true, expectedIsPostableProcess: true, typeof(BaseExceptionReporter), typeof(WinFormsEnvironment), typeof(WinFormsDbEnvironment));
				AssertEnvironmentProperties("InitialiseWebForms", expectedIsUserInteractive: true, expectedIsPostableProcess: false, null, typeof(NullEnvProvider.NullEnv), typeof(BaseDbEnvironment));
				AssertEnvironmentProperties("InitialiseWeb", expectedIsUserInteractive: false, expectedIsPostableProcess: false, null, typeof(WinFormsEnvironment), typeof(BaseDbEnvironment));
			});
		}

		public void TestSessionIdIsSetToAppServerProcessCorrelationId_WhenPassedToInitialiseWebForms()
		{
			var appServerProcessCorrelationId = new Guid("4e49e939-5d48-42ea-ba98-98a4a14918f1");
			Initialiser.InitialiseWinForms(appServerProcessCorrelationId);
			AssertEquals("ExceptionReporter setup", appServerProcessCorrelationId, ExceptionReporter.SessionId);
		}

		public void TestSessionIdIsStillSet_WhenNoArgPassedToInitialiseWebForms()
		{
			Initialiser.InitialiseWinForms();
			AssertNotNull("ExceptionReporter setup", ExceptionReporter.SessionId);
		}

		public void TestInitialiseServiceManagerConnectionPooling()
		{
			// NOTE: Requires net8.0\Enterprise.ZArchitecture.Web.Utilities.dll for this to work for net8.0.
			CombineAssertions(() =>
			{
				AssertEnvironmentProperties("InitialiseServiceManagerPooledConnection", expectedIsUserInteractive: false, expectedIsPostableProcess: false, typeof(BaseExceptionReporter), typeof(ServiceTaskEnvironment), typeof(ServiceManagerDbEnvironmentPooled));
				AssertEnvironmentProperties("InitialiseServiceManager", expectedIsUserInteractive: false, expectedIsPostableProcess: false, typeof(BaseExceptionReporter), typeof(ServiceTaskEnvironment), typeof(ServiceManagerDbEnvironmentUnpooled));
			});
		}

		public void TestCanAccessWTGAppDomainWrapperRunner()
		{
			// Arrange & Act
			var actual = WTG.AppDomainWrappers.Net.AppDomainWrapper.HasAccessToRunner();
			// Assert
			AssertEquals("Project requires access to the Runner executables. Ensure the Runner project has been added to the entry point.", expected: true, File.Exists(actual));
		}

		public void TestCurrentUICultureIsAlwaysDefaultBatchProcessor()
		{
			using (var appDomainWrapper = new WTG.AppDomainWrappers.Net.AppDomainWrapper("TestAppManagerTypesCanBeLoaded"))
			{
				var config = new WTG.AppDomainWrappers.Net.ProcessConfig
				{
					NamespacePath = "Enterprise.Initialisation",
					ClassName = nameof(InitialiserTest),
					MethodName = nameof(AssertCurrentUICultureBatchProcessor)
				};

				config.AssemblyFile = Path.Combine(config.BinFolder, "Enterprise.Initialisation.Test.dll");
				var result = appDomainWrapper.RunMethodInProcess(config);
				AssertEquals(string.Empty, result);
			}

			CultureInfo.CurrentCulture = DefaultCulture.Instance;
		}

		public void TestCurrentUICultureIsAlwaysDefaultWinForms()
		{
			// Requires removal of BinarySerialization from 
			using var appDomainWrapper = new WTG.AppDomainWrappers.Net.AppDomainWrapper("TestAppManagerTypesCanBeLoaded");
			var config = new WTG.AppDomainWrappers.Net.ProcessConfig
			{
				NamespacePath = "Enterprise.Initialisation",
				ClassName = nameof(InitialiserTest),
				MethodName = nameof(AssertCurrentUICultureWinForms)
			};

			config.AssemblyFile = Path.Combine(config.BinFolder, "Enterprise.Initialisation.Test.dll");
			var result = appDomainWrapper.RunMethodInProcess(config);
			AssertEquals(string.Empty, result);
		}

		public static void AssertCurrentUICultureWinForms()
		{
			static void initializer() => Initialiser.InitialiseWinForms(null);
			AssertCurrentUICulture(initializer);
		}

		public static void AssertCurrentUICultureBatchProcessor()
		{
			var initializer = Initialiser.InitialiseBatchProcessor;
			AssertCurrentUICulture(initializer);
		}

		static void AssertCurrentUICulture(Action initializer)
		{
			var de = new CultureInfo("de-DE");
			Thread.CurrentThread.CurrentUICulture = de;
			initializer.Invoke();
			AssertNotNull(Env.Instance);
			AssertEquals(Culture.Default, Thread.CurrentThread.CurrentUICulture);
			using (Culture.SetTemporarily(de))
			{
				AssertEquals(de, Thread.CurrentThread.CurrentCulture);
				AssertEquals(Culture.Default, Thread.CurrentThread.CurrentUICulture);
			}
		}
#endif
	}
}
