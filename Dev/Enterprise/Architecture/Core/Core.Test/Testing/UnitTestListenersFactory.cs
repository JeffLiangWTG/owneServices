using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Common.Testing.MemoryManagement;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.EntityFramework.Testing.DataAccess;
using CargoWise.ResourceStrings.Cache.Testing;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public static class UnitTestListenersFactory
	{
		public static ITestListener[] GetTestListeners()
		{
			return ObjectFactory
				.Get<IEnumerable>("TestListeners")
				.Cast<ITestListener>()
				.Concat(new ITestListener[]
				{
					new TableInfoProviderTestListener(),
					new AssertionProviderTestListener(),
					HeartbeatTestListener.Instance,
					SourceControlInstanceTestListener.Instance,
					BuildXmlInstanceTestListener.Instance,
					DefaultWebProxyListener.Instance,
					ProgressFormManagerListener.Instance,
					TaskTestListener.Instance,
					TempFilesTestListener.Instance,
					NotificationTestListener.Instance,
					ClientOverrideInitUninitTestListener.Instance,
					new ObjectFactoryCleanupListener(),
					GetZEnvironmentListener(),
					(BaseTestListener)ObjectFactory.Get<Enterprise.Integration.ZArchitecture.IZFormActivityLoggerTestListener>(),
					new MemoryCacheTestListener(),
					TableHitCounterListener.Instance,
					new NotDeletingEnterpriseTempPathListener(),
					(BaseTestListener)ObjectFactory.Get<Enterprise.Integration.ZArchitecture.IBackgroundAppDomainWorkItemLeakListener>(),
					(BaseTestListener)ObjectFactory.Get<Enterprise.Integration.ZArchitecture.IUserIdleWorkerLeakListener>(),
					(BaseTestListener)ObjectFactory.Get<Enterprise.Integration.ZArchitecture.IColumnLayoutCacheTestListener>(),
					(BaseTestListener)ObjectFactory.Get<Enterprise.Integration.ZArchitecture.IRecentItemManagerTestListener>(),
					TransactionStatusListener.Instance,
					FountainTestListener.Instance,	// must run after TransactionStatusListener
					ExceptionReporterTestListener.Instance, //Must run just before CommittedDbUpdatesListener
					CommittedDbUpdatesListener.Instance,
					new ResourceStringContentTestTracker(),
					new ClearExtraSubscriptionListener(),
					new OverridableTestListener(),
					ObjectFactory.Get<BaseTestListener>("BGMReferenceCounterTestListener"),
					StaticRegisteredResetTestListener.Instance,
					DateSetInRelevantTestListener.Instance,
					new CurrentCultureTestListener(),
					new BrandingFactoryInstanceTestListener(),
					PerformanceStatisticsListener.Instance,
					RegisteredResetTestListener.Instance,
					new MainDatabaseTestListener(),
					new SecurityProtocolTestListener(),
					new SnapshotLeakListener(),
					new ZQueryInstanceCheckListener(),
					new TestLeakListener(), // TestLeakListener should be run last
				}).ToArray();
		}

		class NotDeletingEnterpriseTempPathListener : BaseTestListener
		{
			public override void EndTest(TestCase test, DateTime endTime)
			{
				if (!Directory.Exists(EnvProxy.Instance.TempPath))
				{
					Directory.CreateDirectory(EnvProxy.Instance.TempPath);
					throw new Exception("TempPath was deleted by this test [" + test.Name + "].");
				}

				base.EndTest(test, endTime);
			}
		}

		static ITestListener GetZEnvironmentListener()
		{
			const string MasterFilesAssembly = "\\Enterprise.MasterFiles.Business.Test.dll";

			var executableDirectory = Path.GetDirectoryName(typeof(UnitTestListenersFactory).Assembly.Location);
			if (string.IsNullOrEmpty(executableDirectory) || !File.Exists(executableDirectory + MasterFilesAssembly))
			{
				executableDirectory = AssemblyLoader.GetBinPath();
			}

			Assembly masterFilesAssembly = Assembly.LoadFrom(executableDirectory + MasterFilesAssembly);
			Type environmentListenerType = masterFilesAssembly.GetType("Enterprise.MasterFiles.Business.Testing.ZEnvironmentListener");
			PropertyInfo environmentListenerInstance = environmentListenerType.GetProperty("Instance", BindingFlags.GetProperty | BindingFlags.Static | BindingFlags.Public);
			BaseTestListener result = (BaseTestListener)environmentListenerInstance.GetValue(null, null);

			return result;
		}

		#region Tests

		public class UnitTestListenersFactoryTest : TestCase
		{
			public void TestListenersDoNotHitDbUnnecessarily()
			{
				RunAllListeners();

				using (Db.Connection.TrackExecutedCommands(includeStackTrace: true))
				{
					RunAllListeners();
					AssertContainsExactElementsInAnyOrder("Test listeners should not hit the DB when no state changed", Array.Empty<string>(), Db.Connection.ExecutedCommands);
				}
			}

			void RunAllListeners()
			{
				var listeners = UnitTestListenersFactory.GetTestListeners();

				foreach (var listener in listeners)
				{
					listener.StartAllTests(DateTime.Now);
				}

				foreach (var listener in listeners)
				{
					listener.BeforeEachTest(DateTime.Now);
				}

				foreach (var listener in listeners)
				{
					listener.StartTest(this, DateTime.Now);
				}

				foreach (var listener in listeners)
				{
					listener.EndTest(this, DateTime.Now);
				}

				foreach (var listener in listeners)
				{
					listener.AfterEachTest(DateTime.Now);
				}

				foreach (var listener in listeners)
				{
					listener.EndAllTests(DateTime.Now);
				}
			}

			public void TestZFormActivityLoggerTestListenerIsContained()
			{
				var listeners = GetTestListeners();
				AssertEquals(true, listeners.Any(x => x.GetType().Name == "ZFormActivityLoggerTestListener"));
			}

			public void TestZFormActivityLoggerTestListenerPrecedesZEnvironmentListener()
			{
				var listeners = GetTestListeners();
				var indexOfZFormActivityLoggerTestListener = listeners.IndexOf(x => x.GetType().Name == "ZFormActivityLoggerTestListener");
				var indexOfZEnvironmentListener = listeners.IndexOf(x => x.GetType().Name == "ZEnvironmentListener");

				AssertLessThan(
					"ZFormActivityLoggerTestListener's StartAllTests method should be run before ZEnvironmentListener's" +
					"because we have to disable ActivityLogger before setting TestHelper.KeyForTest to null, " +
					"otherwise the timer in ActivityLogger might set TestHelper.KeyForTest in the meanwhile.",
					indexOfZEnvironmentListener,
					indexOfZFormActivityLoggerTestListener);
			}

			public void TestMainDatabaseTestListenerIsIncluded()
			{
				// Arrange
				var list = GetTestListeners();

				// Act
				// Assert
				AssertNoExceptionThrown(() => list
					.OfType<MainDatabaseTestListener>()
					.Single());
			}
		}

		#endregion
	}
}
