using System;
using System.Diagnostics;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.ActivityLogging;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Test.Forms.ActivityLogger
{
	public class ZFormActivityLoggerTest : TestCaseWithFactory
	{
#if !WINZOR

		[TestDate(2006, 1, 1, 9, 10, 0)]
		public void TestExternalProcessLogging_RegistryOff()
		{
			var originalValue = EnvProxy.Instance.Registry.UserEventTrackingExternal;
			EnvProxy.Instance.Registry.UserEventTrackingExternal = false;
			try
			{
				ZFormActivityLogger.Instance.EnableActivityLogger();
				((TestFormActivityLogger)ZFormActivityLogger.Instance).RemoveLogsOnSaving = false;

				Form1.Show();
				Form2.Show();
				ZFormActivityLogger.Instance.StatLogs.Clear();

				ONativeWindow.ActiveTopLevelWindowForTest = Form2.Handle;
				DoEventsUntilTimerTicked(TestFormActivityLogger.Instance.externalProcessTimer);
				AssertEquals("Registry is turned off", 0, ZFormActivityLogger.Instance.StatLogs.Count);
			}
			finally
			{
				EnvProxy.Instance.Registry.UserEventTrackingExternal = originalValue;
				((TestFormActivityLogger)ZFormActivityLogger.Instance).RemoveLogsOnSaving = true;
			}
		}

#endif

		public void TestTimersShouldBeDisabledDuringDatabaseUpgrade()
		{
			var logger = TestFormActivityLogger.Instance;

			var stats1 = new FormUserStatistics();
			stats1.NotifyFormShownUtc("Hello", "", ZDateTime.UtcNow.ToDateTime());
			stats1.NotifyFormClosed(Guid.Empty, "");
			ZFormActivityLogger.Instance.StatLogs.Add(stats1);

			using (DbEnv.Instance.DisableTimerDuringDbUpgrade())
			using (RawDataRegistry.Instance.InternalApplicationActivityTrackingInterval.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 10))
			{
				logger.EnableActivityLogger();

				var saveTimerTicked = DoEventsUntilTimerTickedOrTimeout(logger.SaveTimer, 15);
				var externalTimerTicked = DoEventsUntilTimerTickedOrTimeout(logger.externalProcessTimer, logger.MaxExternalProcessPollTimerSeconds);

				AssertEquals("Stop the save timer when the db is being upgraded - we cant touch the DB anyway", false, saveTimerTicked);
				AssertEquals("Stop the external process timer when the db is being upgraded - we cant touch the DB anyway", false, externalTimerTicked);
			}
		}

		public void TestExternalActivityLoggerStoppedWhenDatabaseUpgradedExceptionHasBeenThrown()
		{
			var logger = TestFormActivityLogger.Instance;

			var stats1 = new FormUserStatistics();
			stats1.NotifyFormShownUtc("Hello", "", ZDateTime.UtcNow.ToDateTime());
			stats1.NotifyFormClosed(Guid.Empty, "");
			ZFormActivityLogger.Instance.StatLogs.Add(stats1);

			var eventTrackingExternal = EnvProxy.Instance.Registry.UserEventTrackingExternal;
			var externalLoggerMock = new Mock<IExternalActivityLogger>();
			externalLoggerMock.Setup(x => x.Start()).Verifiable();
			externalLoggerMock.Setup(x => x.Stop()).Verifiable();
			externalLoggerMock.Setup(x => x.DequeueActivityInfo()).Verifiable();

			using (new DisposableAction(() => EnvProxy.Instance.Registry.UserEventTrackingExternal = eventTrackingExternal))
			using (RawDataRegistry.Instance.InternalApplicationActivityTrackingInterval.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 10))
			using (ObjectFactory.Substitute(externalLoggerMock.Object))
			using (Db.Connection.SetDatabaseUpgradedExceptionHasBeenThrown_ForTest())
			{
				EnvProxy.Instance.Registry.UserEventTrackingExternal = true;
				logger.EnableActivityLogger();

				var saveTimerTicked = DoEventsUntilTimerTickedOrTimeout(logger.SaveTimer, 15);
				var externalTimerTicked = DoEventsUntilTimerTickedOrTimeout(logger.externalProcessTimer, logger.MaxExternalProcessPollTimerSeconds);

				AssertNoExceptionThrown(() =>
				{
					AssertEquals("Save timer has ticked", true, saveTimerTicked);
					AssertEquals("External process timer has ticked", true, externalTimerTicked);

					externalLoggerMock.Verify(x => x.Start(), Times.Never);
					externalLoggerMock.Verify(x => x.Stop(), Times.Never);
					externalLoggerMock.Verify(x => x.DequeueActivityInfo(), Times.Never);
				});
			}
		}

		[TestDate(2006, 1, 1, 9, 10, 0)]
		public void TestExternalProcessLogging()
		{
			if (ZFormActivityLogger.Instance.ExternalActivityLogger.OtherInstanceOfActivityLoggerIsRunning())
			{
				Assert("Other instance of ediEnterprise already attached syshook. Test cannot be run.", true);
				return;
			}

			ZFormActivityLogger.Instance.StatLogs.Clear();
			var originalValue = EnvProxy.Instance.Registry.UserEventTrackingExternal;
			EnvProxy.Instance.Registry.UserEventTrackingExternal = true;
			try
			{
				using (RawDataRegistry.Instance.InternalApplicationActivityTrackingInterval.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 10))
				{
					ZFormActivityLogger.Instance.EnableActivityLogger();
					((TestFormActivityLogger)ZFormActivityLogger.Instance).RemoveLogsOnSaving = false;

					ZFormActivityLogger.Instance.ExternalActivityLogger.AddActivityInfoTestOnly("Process1", "Form1");
					ZFormActivityLogger.Instance.ExternalActivityLogger.AddActivityInfoTestOnly("Process2", "Form2");

					//Should be ignored logs
					ZFormActivityLogger.Instance.ExternalActivityLogger.AddActivityInfoTestOnly("rdpinit", "RemoteApp Marker Window");

					DoEventsUntilTimerTicked(TestFormActivityLogger.Instance.externalProcessTimer);
					Assert(ZFormActivityLogger.Instance.ExternalActivityLogger.Started);
					AssertEquals(2, ZFormActivityLogger.Instance.StatLogs.Count);

					AssertEquals("Process1", ZFormActivityLogger.Instance.StatLogs[0].ModuleName);
					AssertEquals("Form1", ZFormActivityLogger.Instance.StatLogs[0].FormCaption);
					AssertEquals("Process2", ZFormActivityLogger.Instance.StatLogs[1].ModuleName);
					AssertEquals("Form2", ZFormActivityLogger.Instance.StatLogs[1].FormCaption);
				}
			}
			finally
			{
				EnvProxy.Instance.Registry.UserEventTrackingExternal = originalValue;
				((TestFormActivityLogger)ZFormActivityLogger.Instance).RemoveLogsOnSaving = true;
			}
		}

		[TestDate(2006, 1, 1, 9, 10, 0)]
		public void TestExternalProcessLogging_DoesNotSaveEmptyActivityLog()
		{
			ZFormActivityLogger.Instance.StatLogs.Clear();
			ZFormActivityLogger.Instance.EnableActivityLogger();
			AssertEquals("Activity Logger Enabled", true, ZFormActivityLogger.Instance.IsEnabled);

			var stats1 = new FormUserStatistics();
			stats1.NotifyFormClosed(Guid.Empty, "");
			ZFormActivityLogger.Instance.StatLogs.Add(stats1);

			var stats2 = new FormUserStatistics();
			stats2.NotifyFormShownUtc("NotValid", "", DateTime.MinValue);
			ZFormActivityLogger.Instance.StatLogs.Add(stats2);
			AssertEquals("StatLogs added", 2, ZFormActivityLogger.Instance.StatLogs.Count);

			ZFormActivityLogger.Instance.SavePendingLogs(true);
			AssertEquals("Stat Logs removed", 0, ZFormActivityLogger.Instance.StatLogs.Count);
		}

		[TestDate(2006, 1, 1, 9, 10, 0)]
		public void TestEnableActivityLogger()
		{
			using (RawDataRegistry.Instance.InternalApplicationActivityTrackingInterval.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 10))
			{
				var loadingFactory = new BusinessObjectFactory();

				TestCaseHelper.ClearTable(StmActivityLogSchema.Constants.TableName);
				ZFormActivityLogger.Instance.StatLogs.Clear();

				ZFormActivityLogger.Instance.EnableActivityLogger();
				AssertEquals(true, ZFormActivityLogger.Instance.IsEnabled);

				var stats1 = new FormUserStatistics();
				stats1.NotifyFormShownUtc("Hello", "", ZDateTime.UtcNow.ToDateTime());
				stats1.NotifyFormClosed(Guid.Empty, "");

				var stats2 = new FormUserStatistics();
				stats2.NotifyFormShownUtc("Bye", "", ZDateTime.UtcNow.ToDateTime());
				stats2.NotifyFormClosed(Guid.Empty, "");

				var stats3 = new FormUserStatistics();
				stats3.NotifyFormShownUtc("Hello Again", "Zubin", new DateTime(2006, 1, 1, 9, 0, 0));
				stats3.IsExternalProcess = false;
				stats3.KeyPresses = 20;
				stats3.MouseClicks = 11;
				stats3.ControlFocusChanges = 13;

				ZFormActivityLogger.Instance.StatLogs.Add(stats1);
				ZFormActivityLogger.Instance.StatLogs.Add(stats2);
				ZFormActivityLogger.Instance.StatLogs.Add(stats3);

				DoEventsUntilTimerTicked(TestFormActivityLogger.Instance.SaveTimer);

				AssertEquals(1, ZFormActivityLogger.Instance.StatLogs.Count);
				AssertEquals("Hello Again", ZFormActivityLogger.Instance.StatLogs[0].FormCaption);
				var logs = loadingFactory.Load<StmActivityLog>(new ZQuery(StmActivityLogSchema.S7_OpenDateTimeUtc, SQLComparisonOperator.GreaterThan, ZDateTime.UtcNow.AddHours(-1)));
				AssertEquals(2, logs.Length);

				var someGuid = Guid.NewGuid();
				stats3.NotifyFormClosed(someGuid, "ZA");
				ZFormActivityLogger.Instance.SavePendingLogs(false);
				AssertEquals(0, ZFormActivityLogger.Instance.StatLogs.Count);
				logs = loadingFactory.Load<StmActivityLog>(new ZQuery(StmActivityLogSchema.S7_OpenDateTimeUtc, SQLComparisonOperator.GreaterThan, ZDateTime.UtcNow.AddHours(-1)));
				AssertEquals(3, logs.Length);

				var lastLog = loadingFactory.LoadTop1<StmActivityLog>(new ZQuery(StmActivityLogSchema.S7_FormCaption, "Hello Again"));
				AssertNotNull(lastLog);
				AssertEquals(20, lastLog.S7_KeyStrokes);
				AssertEquals(11, lastLog.S7_MouseClicks);
				AssertEquals(13, lastLog.S7_ControlChanges);
				AssertEquals("Zubin", lastLog.S7_ControllerID);
				AssertEquals(new DateTime(2006, 1, 1, 9, 0, 0), lastLog.S7_OpenDateTime);
				AssertEquals(new DateTime(2006, 1, 1, 9, 10, 0), lastLog.S7_CloseDateTime);
				AssertEquals(600, lastLog.S7_ActiveTime);
				AssertEquals(EnvProxy.Instance.CurrentUser.Initials.Trim(), lastLog.S7_GS_NKUser.Trim());
			}
		}

		public void TestDisableActivityLogger()
		{
			TestCaseHelper.ClearTable(StmActivityLogSchema.Constants.TableName);
			ZFormActivityLogger.Instance.StatLogs.Clear();

			ZFormActivityLogger.Instance.EnableActivityLogger();
			AssertEquals(true, ZFormActivityLogger.Instance.IsEnabled);

			ZFormActivityLogger.Instance.DisableActivityLogger();
			AssertEquals(false, ZFormActivityLogger.Instance.IsEnabled);
			AssertEquals(false, ZFormActivityLogger.Instance.SaveTimer.Enabled);
			AssertEquals(false, ZFormActivityLogger.Instance.externalProcessTimer.Enabled);
		}

		[TestDate(2007, 5, 28, 9, 32, 0)]
		public void TestForceSavingNotClosedLogs()
		{
			ZFormActivityLogger.Instance.StatLogs.Clear();
			ZFormActivityLogger.Instance.EnableActivityLogger();

			var stats1 = new FormUserStatistics();
			stats1.NotifyFormShownUtc("Hello", "", ZDateTime.UtcNow.ToDateTime());
			stats1.NotifyFormClosed(Guid.Empty, "");

			var stats2 = new FormUserStatistics();
			stats2.NotifyFormShownUtc("Bye", "", ZDateTime.UtcNow.ToDateTime());

			ZFormActivityLogger.Instance.StatLogs.Add(stats1);
			ZFormActivityLogger.Instance.StatLogs.Add(stats2);

			ZFormActivityLogger.Instance.SavePendingLogs(false);
			AssertEquals("Not closed form remains", 1, ZFormActivityLogger.Instance.StatLogs.Count);

			ZFormActivityLogger.Instance.SavePendingLogs(true);
			AssertEquals("Not closed form is forced out to DB", 0, ZFormActivityLogger.Instance.StatLogs.Count);

			var lastLog = Factory.LoadTop1<StmActivityLog>(new ZQuery(StmActivityLogSchema.S7_FormCaption, "Bye"));
			AssertEquals("Uses current date for forced out forms that are not closed", new DateTime(2007, 5, 28, 9, 32, 0), lastLog.S7_CloseDateTime);
		}

		public void TestReportOnceWhenWritingLogsFails()
		{
			ZFormActivityLogger.Instance.StatLogs.Clear();
			ZFormActivityLogger.Instance.EnableActivityLogger();

			var stats1 = new FormUserStatistics();
			stats1.NotifyFormShownUtc("Hello", "", ZDateTime.UtcNow.ToDateTime());
			stats1.NotifyFormClosed(Guid.Empty, "");

			ZFormActivityLogger.Instance.StatLogs.Add(stats1);

			try
			{
				((TestFormActivityLogger)ZFormActivityLogger.Instance).ExceptionToThrowOnSaving = new ApplicationException("Some test exception");
				ZFormActivityLogger.Instance.SavePendingLogs(true);
				AssertEquals("Unable to save Activity Logs", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
			finally
			{
				((TestFormActivityLogger)ZFormActivityLogger.Instance).ExceptionToThrowOnSaving = null;
			}
		}

		public void TestNoExceptionWhenSavePendingLogs()
		{
			ZFormActivityLogger.Instance.StatLogs.Clear();
			ZFormActivityLogger.Instance.EnableActivityLogger();

			var stats = new FormUserStatistics();
			stats.NotifyFormShownUtc("Hello", "", ZDateTime.UtcNow.ToDateTime());
			stats.NotifyFormClosed(Guid.Empty, "");

			ZFormActivityLogger.Instance.StatLogs.Add(stats);
			ZFormActivityLogger.Instance.StatLogs.Add(null);

			AssertNoExceptionThrown(() => ZFormActivityLogger.Instance.SavePendingLogs(true));
		}

		public void TestSetSaveTimerIntervalByInternalApplicationActivityTrackingIntervalRegistry()
		{
			using (RawDataRegistry.Instance.InternalApplicationActivityTrackingInterval.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 500))
			{
				ZFormActivityLogger.Instance.EnableActivityLogger();
				AssertEquals(500000, ZFormActivityLogger.Instance.SaveTimer.Interval);
			}
		}

		#region Test Classes

		class TestFormActivityLogger : ZFormActivityLogger
		{
			#region Instance

			public new static TestFormActivityLogger Instance
			{
				get { return (TestFormActivityLogger)ZFormActivityLogger.Instance; }
			}

			public static void Register()
			{
				lock (instanceLock)
				{
					if (instance_ != null)
					{
						wasEnabled = instance_.IsEnabled;
						if (wasEnabled)
						{
							instance_.DisableActivityLogger();
						}
					}
					oldInstance = instance_;
					instance_ = new TestFormActivityLogger();
				}
			}

			public static void Unregister()
			{
				lock (instanceLock)
				{
					if (instance_ != null)
					{
						instance_.DisableActivityLogger();
					}
					instance_ = oldInstance;
					if (instance_ != null && wasEnabled)
					{
						instance_.EnableActivityLogger();
					}
				}
			}

			static ZFormActivityLogger oldInstance;
			static bool wasEnabled;

			#endregion

#if DEBUG
			internal
#else
			protected
#endif
			override int MaxExternalProcessPollTimerSeconds
			{
				get { return 3; }
			}

			protected override void SaveWrittenLogs(BusinessObjectFactory factory)
			{
				if (ExceptionToThrowOnSaving != null)
				{
					throw ExceptionToThrowOnSaving;
				}

				base.SaveWrittenLogs(factory);
			}

			public Exception ExceptionToThrowOnSaving { get; set; }

			protected override void RemoveLog(FormUserStatistics log)
			{
				if (RemoveLogsOnSaving)
				{
					StatLogs.Remove(log);
				}
			}

			internal bool RemoveLogsOnSaving = true;
		}

		#endregion

		#region Implementation

#if !WINZOR
		ZFormWithSettableHeading Form1
		{
			get
			{
				if (form1 == null)
				{
					form1 = new ZFormWithSettableHeading();
					form1.SetFormHeading("Form1");
				}
				return form1;
			}
		}
#endif //!WINZOR
#pragma warning disable IDE0044 //Object form1 is getting modified, conflicting readonly property
		ZFormWithSettableHeading form1;
#pragma warning restore IDE0044

#if !WINZOR
		ZFormWithSettableHeading Form2
		{
			get
			{
				if (form2 == null)
				{
					form2 = new ZFormWithSettableHeading();
					form2.SetFormHeading("Form2");
				}
				return form2;
			}
		}
#endif //!WINZOR
#pragma warning disable IDE0044 //Object form2 is getting modified, conflicting readonly property
		ZFormWithSettableHeading form2;
#pragma warning restore IDE0044

		class ZFormWithSettableHeading : ZForm
		{
			public override string FormHeading
			{
				get { return formHeading; }
			}

			internal void SetFormHeading(string value)
			{
				formHeading = value;
			}

			string formHeading;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		bool DoEventsUntilTimerTickedOrTimeout(IWindowsTimer timer, int timeoutSeconds)
		{
			var ticked = false;
			var stopwatch = new Stopwatch();
			stopwatch.Start();
			EventHandler tickDelegate = delegate { ticked = true; };
			timer.Tick += tickDelegate;
			while (!ticked)
			{
				Application.DoEvents();
				System.Threading.Thread.Sleep(50);
				if (stopwatch.Elapsed.TotalSeconds >= timeoutSeconds)
				{
					break;
				}
			}
			timer.Tick -= tickDelegate;
			return ticked;
		}

		void DoEventsUntilTimerTicked(IWindowsTimer timer)
		{
			var timeoutSeconds = 30;
			if (!DoEventsUntilTimerTickedOrTimeout(timer, timeoutSeconds))
			{
				throw new TimeoutException($"Timer did not tick after {timeoutSeconds} seconds");
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestFormActivityLogger.Register();
			TestCaseHelper.ClearTable(StmActivityLogSchema.Constants.TableName);
			ZFormActivityLogger.AllowActivityLogSavesInTests = true;
		}

		protected override void TearDown()
		{
			base.TearDown();
			TestFormActivityLogger.Unregister();
			ZFormActivityLogger.AllowActivityLogSavesInTests = false;

			form1?.Dispose();
			form2?.Dispose();
		}

		#endregion
	}
}
