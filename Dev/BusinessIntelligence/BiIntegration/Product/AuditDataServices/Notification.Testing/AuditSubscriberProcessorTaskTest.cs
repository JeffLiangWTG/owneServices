using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.Bi.Common.Testing;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Data.Utils;
using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.AuditDataServices.Accounting;
using Enterprise.AuditDataServices.ArchiveManager;
using Enterprise.AuditDataServices.BorderWise;
using Enterprise.AuditDataServices.Core;
using Enterprise.AuditDataServices.DataScience;
using Enterprise.AuditDataServices.DevTools;
using Enterprise.AuditDataServices.EServices;
using Enterprise.AuditDataServices.Glow;
using Enterprise.AuditDataServices.HVLV;
using Enterprise.AuditDataServices.MDM;
using Enterprise.AuditDataServices.PAVE;
using Enterprise.AuditDataServices.PAVE.Subscribers;
using Enterprise.AuditDataServices.Subscription;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.AuditDataServices.Telematics;
using Enterprise.AuditDataServices.TransportBooking;
using Enterprise.AuditDataServices.XT;
using Enterprise.Integration;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(BorderWiseSubscriberServiceTask))]
[assembly: UsesConstants(typeof(GlowSubscriberServiceTask))]
[assembly: UsesConstants(typeof(DeleteOrphanSubscriberServiceTask))]
[assembly: UsesConstants(typeof(DataScienceSubscriberServiceTask))]
[assembly: UsesConstants(typeof(DevToolsSubscriberServiceTask))]
[assembly: UsesConstants(typeof(EServicesSubscriberServiceTask))]
[assembly: UsesConstants(typeof(TelematicsSubscriberServiceTask))]
[assembly: UsesConstants(typeof(XTSubscriberServiceTask))]
[assembly: UsesConstants(typeof(HVLVSubscriberServiceTask))]

namespace Enterprise.AuditDataServices.Notification.Testing
{
	[TestedType(typeof(AuditSubscriberProcessorTask))]
	partial class AuditSubscriberProcessorTaskTest : ServiceTaskTestCase<AuditSubscriberProcessorTask>
	{
		public void TestInitialiseSchedule()
		{
			var testTask = new AuditSubscriberProcessorTask();
			InitialiseTaskSchedule(testTask, out StmServiceTask taskSchedule);
			AssertEquals("ScheduleTask - IsActive", ZBool.True, taskSchedule.SST_Active);
			Assert("ScheduleTask - TaskPeriod", taskSchedule.Recurrence.MinutesRange);
			AssertEquals("ScheduleTask - TaskPeriodCount", 5, taskSchedule.Recurrence.Period);
			AssertEquals("ScheduleTask - WeekDaysOnly", ZBool.False, taskSchedule.Recurrence.WeekDaysOnly);
			AssertEquals("DailyStartTime - Default", ZDateTime.Empty, taskSchedule.Recurrence.CalcDailyStartTimeUtc);
		}

		[UseSnapshotProtection(new[] { DatabaseType.Audit })]
		public void TestNudgingSubscribers()
		{
			using (var auditConnection = Db.NewAdminConnection(Db.AuditDatabaseName))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var auditSubscriberProcessorTask = new AuditSubscriberProcessorTaskSkipMaintenanceForTest();
				var testLogger = new LoggerForTest();
				auditSubscriberProcessorTask.ServiceLogger = testLogger;

				auditSubscriberProcessorTask.RunTask();

				CombineAssertions($"Actual logs:\r\n\r\n{string.Join("\r\n", testLogger.LogEntries)}", () =>
				{
					var nudgingLogEntries = ExtractNudgingLogEntries(testLogger);
					var expectedNudgingLogEntries = new[]
					{
						"Nudging " + GlowSubscriberServiceTask.Description,
						"Nudging " + HVLVSubscriberServiceTask.Description,
						"Nudging " + MDMSubscriberServiceTask.Description,
						"Nudging PAVE subscriber service task",
						"Nudging " + GeneralLedgerDataSubscriberServiceTask.Description,
						"Nudging " + DeleteOrphanSubscriberServiceTask.Description,
						"Nudging " + TelematicsSubscriberServiceTask.Description,
						"Nudging " + DtbMasterBookingReplicationSubscriberServiceTask.Description,
						"Nudging " + CoreSubscriberServiceTask.Description,
					};
					AssertContainsExactElementsInAnyOrder("ASP should only nudge expected tasks", expectedNudgingLogEntries, nudgingLogEntries);
				});
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Audit })]
		public void TestNudgingEDISubscribers()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ClientHookLoader.Instance.OverrideClientHookForTest(new TestClientOverride(Clients.EDI)))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var auditSubscriberProcessorTask = new AuditSubscriberProcessorTaskSkipMaintenanceForTest();
				var testLogger = new LoggerForTest();
				auditSubscriberProcessorTask.ServiceLogger = testLogger;

				auditSubscriberProcessorTask.RunTask();

				CombineAssertions($"Actual logs:\r\n\r\n{string.Join("\r\n", testLogger.LogEntries)}", () =>
				{
					var nudgingLogEntries = ExtractNudgingLogEntries(testLogger);
					var expectedNudgingLogEntries = new[]
					{
						"Nudging " + GlowSubscriberServiceTask.Description,
						"Nudging " + HVLVSubscriberServiceTask.Description,
						"Nudging " + MDMSubscriberServiceTask.Description,
						"Nudging " + DataScienceSubscriberServiceTask.Description,
						"Nudging " + DevToolsSubscriberServiceTask.Description,
						"Nudging " + EServicesSubscriberServiceTask.Description,
						"Nudging " + XTSubscriberServiceTask.Description,
						"Nudging " + BorderWiseSubscriberServiceTask.Description,
						"Nudging " + PAVESubscriberServiceTask.Description,
						"Nudging " + GeneralLedgerDataSubscriberServiceTask.Description,
						"Nudging " + DeleteOrphanSubscriberServiceTask.Description,
						"Nudging " + TelematicsSubscriberServiceTask.Description,
						"Nudging " + DtbMasterBookingReplicationSubscriberServiceTask.Description,
						"Nudging " + CoreSubscriberServiceTask.Description,
					};
					AssertContainsExactElementsInAnyOrder("ASP should only nudge expected tasks", expectedNudgingLogEntries, nudgingLogEntries);
				});
			}
		}

		public void TestAuditServiceTasksListNonEdiClient()
		{
			var auditSubscriberProcessorTask = new AuditSubscriberProcessorTask();
			var auditServiceTaskTypes = auditSubscriberProcessorTask.AuditServiceTasks.Select(t => t.GetType().FullName).OrderBy(t => t);

			var expectedTaskTypes = new List<string>
			{
				"Enterprise.AuditDataServices.Accounting.GeneralLedgerDataSubscriberServiceTask",
				"Enterprise.AuditDataServices.ArchiveManager.DeleteOrphanSubscriberServiceTask",
				"Enterprise.AuditDataServices.Core.CoreSubscriberServiceTask",
				"Enterprise.AuditDataServices.Glow.GlowSubscriberServiceTask",
				"Enterprise.AuditDataServices.HVLV.HVLVSubscriberServiceTask",
				"Enterprise.AuditDataServices.MDM.MDMSubscriberServiceTask",
				"Enterprise.AuditDataServices.PAVE.PAVESubscriberServiceTask",
				"Enterprise.AuditDataServices.Telematics.TelematicsSubscriberServiceTask",
				"Enterprise.AuditDataServices.TransportBooking.DtbMasterBookingReplicationSubscriberServiceTask"
			};

			AssertContainsExactElementsInAnyOrder("ASP should only contain expected service tasks", expectedTaskTypes, auditServiceTaskTypes);
		}

		public void TestAuditServiceTasksListEdiClient()
		{
			using (ClientHookLoader.Instance.OverrideClientHookForTest(new TestClientOverride(Clients.EDI)))
			{
				var auditSubscriberProcessorTask = new AuditSubscriberProcessorTask();
				var auditServiceTaskTypes = auditSubscriberProcessorTask.AuditServiceTasks.Select(t => t.GetType().FullName).OrderBy(t => t);

				var expectedTaskTypes = new List<string>
				{
					"Enterprise.AuditDataServices.Accounting.GeneralLedgerDataSubscriberServiceTask",
					"Enterprise.AuditDataServices.ArchiveManager.DeleteOrphanSubscriberServiceTask",
					"Enterprise.AuditDataServices.BorderWise.BorderWiseSubscriberServiceTask",
					"Enterprise.AuditDataServices.Core.CoreSubscriberServiceTask",
					"Enterprise.AuditDataServices.DataScience.DataScienceSubscriberServiceTask",
					"Enterprise.AuditDataServices.DevTools.DevToolsSubscriberServiceTask",
					"Enterprise.AuditDataServices.EServices.EServicesSubscriberServiceTask",
					"Enterprise.AuditDataServices.Glow.GlowSubscriberServiceTask",
					"Enterprise.AuditDataServices.HVLV.HVLVSubscriberServiceTask",
					"Enterprise.AuditDataServices.MDM.MDMSubscriberServiceTask",
					"Enterprise.AuditDataServices.PAVE.PAVESubscriberServiceTask",
					"Enterprise.AuditDataServices.Telematics.TelematicsSubscriberServiceTask",
					"Enterprise.AuditDataServices.TransportBooking.DtbMasterBookingReplicationSubscriberServiceTask",
					"Enterprise.AuditDataServices.XT.XTSubscriberServiceTask"
				};

				AssertContainsExactElementsInAnyOrder("ASP should only contain expected service tasks", expectedTaskTypes, auditServiceTaskTypes);
			}
		}

		IEnumerable<string> ExtractNudgingLogEntries(LoggerForTest testLogger)
		{
			var result = new List<string>();
			foreach (var logEntry in testLogger.LogEntries)
			{
				if (logEntry.StartsWith("Nudging "))
				{
					result.Add(logEntry);
				}
			}
			return result;
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestRunMaintenance()
		{
			using (var adminConnection = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				SetUpMasterState(adminConnection);

				var auditSubscriberProcessorTask = new AuditSubscriberProcessorTask();
				var testLogger = new LoggerForTest();
				auditSubscriberProcessorTask.ServiceLogger = testLogger;

				auditSubscriberProcessorTask.RunTask();

				AssertCollectionContains("ASP should run audit database maintenance", "Running Audit database maintenance", testLogger.LogEntries);

				testLogger.ClearLog();

				auditSubscriberProcessorTask.RunTask();

				AssertCollectionNotContains("ASP should not run audit database maintenance", "Running Audit database maintenance", testLogger.LogEntries);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestAspLockCorrectly()
		{
			var logger = new LoggerForTest();
			var asp = new AuditSubscriberProcessorTaskForLockTest();
			var subscriberTask = new PAVESubscriberServiceTask();
			asp.ServiceLogger = logger;
			asp.AuditServiceTasks = new List<AuditSubscriberTask>
			{
				subscriberTask,
			};
			var manager = new SubscriberManager(logger, subscriberTask.ServiceTaskCode);

			using (var mainDbConnection = Db.NewAdminConnection(Db.DatabaseName))
			using (var wrapperConnection = Db.NewAdminConnection(Db.AuditDatabaseName))
			{
				wrapperConnection.ExecuteNonQuery("INSERT INTO biadmin.LsnTimeMapping (StartLsn, TranEndTimeUtc) VALUES (0x01, '2017-12-03');");
				using (BiTemporaryMasterState.SetParameterTemporaryValue(wrapperConnection, BiConstants.LastMaxLsnProcessed, "0x01"))
				using (BiTemporaryMasterState.SetParameterTemporaryValue(wrapperConnection, BiConstants.LastMaxLsnTimeProcessed, "2017-12-03 00:00:00.0000"))
				{
					bool locksAcquired;
					using (asp.TryAndAcquireAuditSubscriberLocksForTest(mainDbConnection, out locksAcquired))
					{
						manager.RunSubscribers(new IAuditSubscriberWrapper[] { new ProcessHeaderResponsiveAssignmentSubscriber().GetWrapper(wrapperConnection, logger) }, new CancellationToken());
					}
				}

				AssertContainsExactElementsInAnyOrder(new string[] {
					$"Acquiring lock for {BiConstants.AspMaintainanceLockKey + subscriberTask.ServiceTaskCode}",
					"> Audit DB HWM: (LSN: 01000000000000000000, Period: 1712)",
					$"Subscriber task {subscriberTask.ServiceTaskCode} run skipped due to lock",
				},
				logger.LogEntries);
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestAcquireLockFailImmediatly()
		{
			using (var mockSubscriberConnecion = Db.NewAdminConnection())
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var auditSubscriberProcessorTask = new AuditSubscriberProcessorTask();
				SqlApplicationLock aspLock = null;
				var lockKey = BiConstants.AspMaintainanceLockKey + auditSubscriberProcessorTask.AuditServiceTasks.First().ServiceTaskCode;

				Assert("Failed to acquire subsriber lock", mockSubscriberConnecion.TryGetLock(lockKey, out aspLock));

				using (new DisposableAction(() => { aspLock?.Dispose(); }))
				using (var adminConnection = Db.NewAdminConnection())
				{
					SetUpMasterState(adminConnection);

					var testLogger = new LoggerForTest();
					auditSubscriberProcessorTask.ServiceLogger = testLogger;

					var sw = new Stopwatch();
					sw.Start();

					auditSubscriberProcessorTask.RunTask();
					sw.Stop();

					CombineAssertions($"Actual logs:\r\n\r\n{string.Join("\r\n", testLogger.LogEntries)}", () =>
					{
						AssertCollectionContains($"Acquiring lock for {lockKey} should fail.", $"Acquiring lock for {lockKey} Failed.", testLogger.LogEntries);
						AssertLessThan("ASP should fail immediatly.", sw.ElapsedMilliseconds, TimeSpan.FromSeconds(7).TotalMilliseconds);
					});
				}
			}
		}

		void SetUpMasterState(AdminConnection biConnection)
		{
			using (((ICurrentDbControl)biConnection).UseDatabase(Db.AuditDatabaseName))
			{
				BiMasterState.DeleteParameter(biConnection, BiConstants.LastIndexRebuildUtcDt);
			}
		}

		class AuditSubscriberProcessorTaskForLockTest : AuditSubscriberProcessorTask
		{
			public IDisposable TryAndAcquireAuditSubscriberLocksForTest(DbConnection connection, out bool locksAcquired)
			{
				return TryAndAcquireAuditSubscriberLocks(connection, out locksAcquired);
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		class AuditSubscriberProcessorTaskSkipMaintenanceForTest : AuditSubscriberProcessorTask
		{
			internal override bool ShouldRunMaintenance(DbConnection biConnection)
			{
				return false;
			}
		}
	}
}
