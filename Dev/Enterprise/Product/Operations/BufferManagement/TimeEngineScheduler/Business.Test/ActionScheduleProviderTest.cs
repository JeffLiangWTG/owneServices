using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.TimeEngineScheduler.Integration;
using NUnit.Framework;

namespace Enterprise.TimeEngineScheduler.Business.Test
{
	public class ActionScheduleProviderTest : NonTransactionedTestCase
	{
		public void TestGetsScheduledAction()
		{
			// Scheduled
			var s1 = Factory.NewWithValidTestData<TimeActionSchedule>();
			s1.TAS_ExecutionStatus = Constants.TimeActionScheduleStatus.Scheduled;
			s1.TAS_ExecutionDateTimeUtc = ZDateTime.UtcNow.Add(TimeSpan.FromHours(-1));

			// Failed with retry
			var s2 = Factory.NewWithValidTestData<TimeActionSchedule>();
			s2.TAS_ExecutionStatus = Constants.TimeActionScheduleStatus.Scheduled;
			s2.TAS_RetryAttempts = (byte)(Constants.MaxRetryAttempts - 1);
			s2.TAS_ExecutionDateTimeUtc = ZDateTime.UtcNow.Add(TimeSpan.FromHours(-1));

			// Scheduled in the future
			var ns1 = Factory.NewWithValidTestData<TimeActionSchedule>();
			ns1.TAS_ExecutionStatus = Constants.TimeActionScheduleStatus.Scheduled;
			ns1.TAS_ExecutionDateTimeUtc = ZDateTime.UtcNow.Add(TimeSpan.FromHours(1));

			// Closed
			var ns2 = Factory.NewWithValidTestData<TimeActionSchedule>();
			ns2.TAS_ExecutionStatus = Constants.TimeActionScheduleStatus.Closed;
			ns2.TAS_ExecutionDateTimeUtc = ZDateTime.UtcNow.Add(TimeSpan.FromHours(-1));

			// Failed with retry in the future
			var ns3 = Factory.NewWithValidTestData<TimeActionSchedule>();
			ns3.TAS_ExecutionStatus = Constants.TimeActionScheduleStatus.Scheduled;
			ns3.TAS_RetryAttempts = (byte)(Constants.MaxRetryAttempts - 1);
			ns3.TAS_ExecutionDateTimeUtc = ZDateTime.UtcNow.Add(TimeSpan.FromHours(1));

			// Failed with exceeded retry attempts
			var ns4 = Factory.NewWithValidTestData<TimeActionSchedule>();
			ns4.TAS_ExecutionStatus = Constants.TimeActionScheduleStatus.Failed;
			ns4.TAS_RetryAttempts = Constants.MaxRetryAttempts;
			ns4.TAS_ExecutionDateTimeUtc = ZDateTime.UtcNow.Add(TimeSpan.FromHours(-1));

			// Suspended
			var ns5 = Factory.NewWithValidTestData<TimeActionSchedule>();
			ns5.TAS_ExecutionStatus = Constants.TimeActionScheduleStatus.Suspended;
			ns5.TAS_ExecutionDateTimeUtc = ZDateTime.UtcNow.Add(TimeSpan.FromHours(-1));

			Factory.Save();

			var scheduleFactory = new ActionScheduleProvider();
			var schedules = scheduleFactory.GetRunnableSchedules(new BusinessObjectFactory()).ToArray();

			AssertContainsExactElementsInAnyOrder(new[] { s1.PK, s2.PK }, schedules.Select(p => p.PK));
			Assert(schedules.All(s => s.NonPersistentSchedulingState == TimeActionSchedulingState.Loaded));
		}

		public void TestScheduleActions()
		{
			var scheduleFactory = new ActionScheduleProvider();

			var targetPk = ZGuid.NewZGuid();
			var dateTime = ZDateTime.UtcNow.ToSmallDateTimeFloor();
			var schedule = scheduleFactory.ScheduleAction("TST", dateTime, targetPk, targetTableCode: "TGT", jsonParameter: "{}", token: "token");
			AssertEquals(TimeActionSchedulingState.Scheduled, schedule.NonPersistentSchedulingState);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedSchedule = newFactory.Load<TimeActionSchedule>(schedule.PK);
			AssertEquals(Constants.TimeActionScheduleStatus.Scheduled, loadedSchedule.TAS_ExecutionStatus);
			AssertEquals("TST", loadedSchedule.TAS_ActionCode);
			AssertEquals(dateTime, loadedSchedule.TAS_ExecutionDateTimeUtc);
			AssertEquals(targetPk, loadedSchedule.TAS_TargetPK);
			AssertEquals("TGT", loadedSchedule.TAS_TargetTableCode);
			AssertEquals("{}", loadedSchedule.TAS_JsonParameter);
			AssertEquals("token", loadedSchedule.TAS_Token);
			AssertEquals("", loadedSchedule.TAS_ExecutionResult);
			AssertEquals((byte)0, loadedSchedule.TAS_RetryAttempts);
			AssertEquals(TimeActionSchedulingState.Loaded, loadedSchedule.NonPersistentSchedulingState);
		}

		public void TestScheduleSuspendedActions()
		{
			var scheduleFactory = new ActionScheduleProvider();

			var targetPk = ZGuid.NewZGuid();
			var dateTime = ZDateTime.UtcNow.ToSmallDateTimeFloor();
			var schedule = scheduleFactory.ScheduleAction("TST", dateTime, targetPk, targetTableCode: "TGT", jsonParameter: "{}", token: "token", scheduleSuspended: true);
			AssertEquals(TimeActionSchedulingState.Scheduled, schedule.NonPersistentSchedulingState);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedSchedule = newFactory.Load<TimeActionSchedule>(schedule.PK);
			AssertEquals(Constants.TimeActionScheduleStatus.Suspended, loadedSchedule.TAS_ExecutionStatus);
			AssertEquals("TST", loadedSchedule.TAS_ActionCode);
			AssertEquals(dateTime, loadedSchedule.TAS_ExecutionDateTimeUtc);
			AssertEquals(targetPk, loadedSchedule.TAS_TargetPK);
			AssertEquals("TGT", loadedSchedule.TAS_TargetTableCode);
			AssertEquals("{}", loadedSchedule.TAS_JsonParameter);
			AssertEquals("token", loadedSchedule.TAS_Token);
			AssertEquals("", loadedSchedule.TAS_ExecutionResult);
			AssertEquals((byte)0, loadedSchedule.TAS_RetryAttempts);
			AssertEquals(TimeActionSchedulingState.Loaded, loadedSchedule.NonPersistentSchedulingState);
		}

		#region Execution Context

		public void TestScheduleActions_ShouldStoreExplicitlySetContext()
		{
			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			branch1.GB_Code = "BR1";
			branch1.GB_RL_NKHomePort = "AUMEL";

			var department1 = Factory.New<GlbDepartment>();
			department1.GE_Code = "DP1";

			Factory.Save();

			var scheduleFactory = new ActionScheduleProvider();

			var targetPk = ZGuid.NewZGuid();
			var dateTime = ZDateTime.UtcNow.ToSmallDateTimeFloor();

			var schedule = scheduleFactory.ScheduleAction("TST", dateTime, targetPk, targetTableCode: "TGT", jsonParameter: "{}", branch1.PK, department1.PK);

			var loadedSchedule = Factory.Load<TimeActionSchedule>(schedule.PK);
			AssertEquals("Should store explicitly defined branch", branch1.PK, loadedSchedule.TAS_GB_Branch);
			AssertEquals("Should store explicitly defined department", department1.PK, loadedSchedule.TAS_GE_Department);
		}

		public void TestScheduleActions_ShouldStoreCurrentBranch_WhenBranchIsNotSetExplicitly()
		{
			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			branch1.GB_Code = "BR1";
			branch1.GB_RL_NKHomePort = "AUMEL";

			var department1 = Factory.New<GlbDepartment>();
			department1.GE_Code = "DP1";

			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK)))
			{
				var scheduleFactory = new ActionScheduleProvider();

				var targetPk = ZGuid.NewZGuid();
				var dateTime = ZDateTime.UtcNow.ToSmallDateTimeFloor();

				var schedule = scheduleFactory.ScheduleAction("TST", dateTime, targetPk, targetTableCode: "TGT", jsonParameter: "{}", executionDepartmentPk: department1.PK);

				var loadedSchedule = Factory.Load<TimeActionSchedule>(schedule.PK);
				AssertEquals("Should store current branch", branch1.PK, loadedSchedule.TAS_GB_Branch);
				AssertEquals("Should store explicitly defined department", department1.PK, loadedSchedule.TAS_GE_Department);
			}
		}

		public void TestScheduleActions_ShouldStoreCurrentDepartment_WhenDepartmentIsNotSetExplicitly()
		{
			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			branch1.GB_Code = "BR1";
			branch1.GB_RL_NKHomePort = "AUMEL";

			var department1 = Factory.New<GlbDepartment>();
			department1.GE_Code = "DP1";

			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUserPK, Env.CurrentBranchPK, department1.PK.ToGuid())))
			{
				var scheduleFactory = new ActionScheduleProvider();

				var targetPk = ZGuid.NewZGuid();
				var dateTime = ZDateTime.UtcNow.ToSmallDateTimeFloor();

				var schedule = scheduleFactory.ScheduleAction("TST", dateTime, targetPk, targetTableCode: "TGT", jsonParameter: "{}", executionBranchPk: branch1.PK);

				var loadedSchedule = Factory.Load<TimeActionSchedule>(schedule.PK);
				AssertEquals("Should store explicitly defined branch", branch1.PK, loadedSchedule.TAS_GB_Branch);
				AssertEquals("Should store current department", department1.PK, loadedSchedule.TAS_GE_Department);
			}
		}

		#endregion

		[TestDate(2024, 12, 24)]
		public void TestScheduleOrRescheduleActions()
		{
			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			branch1.GB_Code = "BR1";
			branch1.GB_RL_NKHomePort = "AUMEL";

			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_Code = "BR2";
			branch2.GB_RL_NKHomePort = "AUMEL";

			var department1 = Factory.New<GlbDepartment>();
			department1.GE_Code = "DP1";

			var department2 = Factory.New<GlbDepartment>();
			department2.GE_Code = "DP2";

			Factory.Save();

			var scheduleFactory = new ActionScheduleProvider();

			var targetPk = ZGuid.NewZGuid();
			var schedule = scheduleFactory.ScheduleOrRescheduleAction("TST", ZDateTime.UtcNow, targetPk, targetTableCode: "TGT", jsonParameter: "{}", executionBranchPk: branch1.PK, executionDepartmentPk: department1.PK, token: "token", scheduleSuspended: true);
			AssertEquals(TimeActionSchedulingState.Scheduled, schedule.NonPersistentSchedulingState);

			var collection = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false });
			AssertEquals(1, collection.Count);
			var loadedSchedule = collection.Single();

			CombineAssertions("Should schedule", () =>
			{
				AssertEquals(Constants.TimeActionScheduleStatus.Suspended, loadedSchedule.TAS_ExecutionStatus);
				AssertEquals("TST", loadedSchedule.TAS_ActionCode);
				AssertEquals(ZDateTime.UtcNow, loadedSchedule.TAS_ExecutionDateTimeUtc);
				AssertEquals(targetPk, loadedSchedule.TAS_TargetPK);
				AssertEquals("TGT", loadedSchedule.TAS_TargetTableCode);
				AssertEquals("{}", loadedSchedule.TAS_JsonParameter);
				AssertEquals(branch1.PK, loadedSchedule.TAS_GB_Branch);
				AssertEquals(department1.PK, loadedSchedule.TAS_GE_Department);
				AssertEquals("token", loadedSchedule.TAS_Token);
				AssertEquals("", loadedSchedule.TAS_ExecutionResult);
				AssertEquals((byte)0, loadedSchedule.TAS_RetryAttempts);
				AssertEquals(TimeActionSchedulingState.Loaded, loadedSchedule.NonPersistentSchedulingState);
			});

			var scheduleWithDifferentParameters = scheduleFactory.ScheduleOrRescheduleAction("TST", ZDateTime.UtcNow.AddDays(1), targetPk, targetTableCode: "TGT", jsonParameter: "{}", executionBranchPk: branch2.PK, executionDepartmentPk: department2.PK, token: "Another token", scheduleSuspended: false);
			AssertEquals(TimeActionSchedulingState.Rescheduled, scheduleWithDifferentParameters.NonPersistentSchedulingState);

			collection = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false });
			AssertEquals(1, collection.Count);
			loadedSchedule = collection.Single();

			CombineAssertions("Should reschedule with different parameters", () =>
			{
				AssertEquals(Constants.TimeActionScheduleStatus.Scheduled, loadedSchedule.TAS_ExecutionStatus);
				AssertEquals("TST", loadedSchedule.TAS_ActionCode);
				AssertEquals("Should reschedule with another execution time", ZDateTime.UtcNow.AddDays(1), loadedSchedule.TAS_ExecutionDateTimeUtc);
				AssertEquals(targetPk, loadedSchedule.TAS_TargetPK);
				AssertEquals("TGT", loadedSchedule.TAS_TargetTableCode);
				AssertEquals("{}", loadedSchedule.TAS_JsonParameter);
				AssertEquals("Should reschedule with another execution branch", branch2.PK, loadedSchedule.TAS_GB_Branch);
				AssertEquals("Should reschedule with another execution department", department2.PK, loadedSchedule.TAS_GE_Department);
				AssertEquals("Should reschedule with another token", "Another token", loadedSchedule.TAS_Token);
				AssertEquals("", loadedSchedule.TAS_ExecutionResult);
				AssertEquals((byte)0, loadedSchedule.TAS_RetryAttempts);
				AssertEquals(TimeActionSchedulingState.Loaded, loadedSchedule.NonPersistentSchedulingState);
			});

			var scheduleWithDifferentActionCode = scheduleFactory.ScheduleOrRescheduleAction("NEW", ZDateTime.UtcNow.AddDays(2), targetPk, targetTableCode: "TGT", jsonParameter: "{}", executionBranchPk: branch2.PK, executionDepartmentPk: department2.PK, token: "Another token");
			AssertEquals(TimeActionSchedulingState.Scheduled, scheduleWithDifferentActionCode.NonPersistentSchedulingState);

			collection = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false });
			AssertEquals(2, collection.Count);

			loadedSchedule = collection.SingleOrDefault(s => s.TAS_ActionCode == "TST");
			AssertNotNull(loadedSchedule);

			var loadedScheduleWithDifferentActionCode = collection.SingleOrDefault(s => s.TAS_ActionCode == "NEW");
			AssertNotNull(loadedScheduleWithDifferentActionCode);

			CombineAssertions("Old schedule should stay untouched", () =>
			{
				AssertEquals(Constants.TimeActionScheduleStatus.Scheduled, loadedSchedule.TAS_ExecutionStatus);
				AssertEquals("TST", loadedSchedule.TAS_ActionCode);
				AssertEquals(ZDateTime.UtcNow.AddDays(1), loadedSchedule.TAS_ExecutionDateTimeUtc);
				AssertEquals(targetPk, loadedSchedule.TAS_TargetPK);
				AssertEquals("TGT", loadedSchedule.TAS_TargetTableCode);
				AssertEquals("{}", loadedSchedule.TAS_JsonParameter);
				AssertEquals(branch2.PK, loadedSchedule.TAS_GB_Branch);
				AssertEquals(department2.PK, loadedSchedule.TAS_GE_Department);
				AssertEquals("Another token", loadedSchedule.TAS_Token);
				AssertEquals("", loadedSchedule.TAS_ExecutionResult);
				AssertEquals((byte)0, loadedSchedule.TAS_RetryAttempts);
				AssertEquals(TimeActionSchedulingState.Loaded, loadedSchedule.NonPersistentSchedulingState);
			});

			CombineAssertions("Should schedule another action with different action code", () =>
			{
				AssertEquals(Constants.TimeActionScheduleStatus.Scheduled, loadedScheduleWithDifferentActionCode.TAS_ExecutionStatus);
				AssertEquals("NEW", loadedScheduleWithDifferentActionCode.TAS_ActionCode);
				AssertEquals(ZDateTime.UtcNow.AddDays(2), loadedScheduleWithDifferentActionCode.TAS_ExecutionDateTimeUtc);
				AssertEquals(targetPk, loadedScheduleWithDifferentActionCode.TAS_TargetPK);
				AssertEquals("TGT", loadedScheduleWithDifferentActionCode.TAS_TargetTableCode);
				AssertEquals("{}", loadedScheduleWithDifferentActionCode.TAS_JsonParameter);
				AssertEquals(branch2.PK, loadedScheduleWithDifferentActionCode.TAS_GB_Branch);
				AssertEquals(department2.PK, loadedScheduleWithDifferentActionCode.TAS_GE_Department);
				AssertEquals("Another token", loadedScheduleWithDifferentActionCode.TAS_Token);
				AssertEquals("", loadedScheduleWithDifferentActionCode.TAS_ExecutionResult);
				AssertEquals((byte)0, loadedScheduleWithDifferentActionCode.TAS_RetryAttempts);
				AssertEquals(TimeActionSchedulingState.Loaded, loadedScheduleWithDifferentActionCode.NonPersistentSchedulingState);
			});

			var newTargetPk = Guid.NewGuid();
			var scheduleWithDifferentTargetPK = scheduleFactory.ScheduleOrRescheduleAction("TST", ZDateTime.UtcNow.AddDays(3), newTargetPk, targetTableCode: "TGT", jsonParameter: "{}", executionBranchPk: branch2.PK, executionDepartmentPk: department2.PK, token: "Another token");
			AssertEquals(TimeActionSchedulingState.Scheduled, scheduleWithDifferentTargetPK.NonPersistentSchedulingState);

			collection = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false });
			AssertEquals(3, collection.Count);
			AssertEquals(2, collection.Count(s => s.TAS_TargetPK == targetPk));

			var loadedScheduleWithDifferentTargetPK = collection.SingleOrDefault(s => s.TAS_TargetPK == newTargetPk);
			AssertNotNull(loadedScheduleWithDifferentTargetPK);

			CombineAssertions("Should schedule another action with different target PK", () =>
			{
				AssertEquals(Constants.TimeActionScheduleStatus.Scheduled, loadedScheduleWithDifferentTargetPK.TAS_ExecutionStatus);
				AssertEquals("TST", loadedScheduleWithDifferentTargetPK.TAS_ActionCode);
				AssertEquals(ZDateTime.UtcNow.AddDays(3), loadedScheduleWithDifferentTargetPK.TAS_ExecutionDateTimeUtc);
				AssertEquals(newTargetPk, loadedScheduleWithDifferentTargetPK.TAS_TargetPK);
				AssertEquals("TGT", loadedScheduleWithDifferentTargetPK.TAS_TargetTableCode);
				AssertEquals("{}", loadedScheduleWithDifferentTargetPK.TAS_JsonParameter);
				AssertEquals(branch2.PK, loadedScheduleWithDifferentTargetPK.TAS_GB_Branch);
				AssertEquals(department2.PK, loadedScheduleWithDifferentTargetPK.TAS_GE_Department);
				AssertEquals("Another token", loadedScheduleWithDifferentTargetPK.TAS_Token);
				AssertEquals("", loadedScheduleWithDifferentTargetPK.TAS_ExecutionResult);
				AssertEquals((byte)0, loadedScheduleWithDifferentTargetPK.TAS_RetryAttempts);
				AssertEquals(TimeActionSchedulingState.Loaded, loadedScheduleWithDifferentTargetPK.NonPersistentSchedulingState);
			});

			var scheduleWithDifferentTargetTableCode = scheduleFactory.ScheduleOrRescheduleAction("TST", ZDateTime.UtcNow.AddDays(4), targetPk, targetTableCode: "NEW", jsonParameter: "{}", executionBranchPk: branch2.PK, executionDepartmentPk: department2.PK, token: "Another token");
			AssertEquals(TimeActionSchedulingState.Scheduled, scheduleWithDifferentTargetTableCode.NonPersistentSchedulingState);

			collection = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false });
			AssertEquals(4, collection.Count);
			AssertEquals(3, collection.Count(s => s.TAS_TargetTableCode == "TGT"));

			var loadedScheduleWithDifferentTargetTableCode = collection.SingleOrDefault(s => s.TAS_TargetTableCode == "NEW");
			AssertNotNull(loadedScheduleWithDifferentTargetTableCode);

			CombineAssertions("Should schedule another action with different target table code", () =>
			{
				AssertEquals(Constants.TimeActionScheduleStatus.Scheduled, loadedScheduleWithDifferentTargetTableCode.TAS_ExecutionStatus);
				AssertEquals("TST", loadedScheduleWithDifferentTargetTableCode.TAS_ActionCode);
				AssertEquals(ZDateTime.UtcNow.AddDays(4), loadedScheduleWithDifferentTargetTableCode.TAS_ExecutionDateTimeUtc);
				AssertEquals(targetPk, loadedScheduleWithDifferentTargetTableCode.TAS_TargetPK);
				AssertEquals("NEW", loadedScheduleWithDifferentTargetTableCode.TAS_TargetTableCode);
				AssertEquals("{}", loadedScheduleWithDifferentTargetTableCode.TAS_JsonParameter);
				AssertEquals(branch2.PK, loadedScheduleWithDifferentTargetTableCode.TAS_GB_Branch);
				AssertEquals(department2.PK, loadedScheduleWithDifferentTargetTableCode.TAS_GE_Department);
				AssertEquals("Another token", loadedScheduleWithDifferentTargetTableCode.TAS_Token);
				AssertEquals("", loadedScheduleWithDifferentTargetTableCode.TAS_ExecutionResult);
				AssertEquals((byte)0, loadedScheduleWithDifferentTargetTableCode.TAS_RetryAttempts);
				AssertEquals(TimeActionSchedulingState.Loaded, loadedScheduleWithDifferentTargetTableCode.NonPersistentSchedulingState);
			});

			var scheduleWithDifferentJsonParameter = scheduleFactory.ScheduleOrRescheduleAction("TST", ZDateTime.UtcNow.AddDays(5), targetPk, targetTableCode: "TGT", jsonParameter: "NEW", executionBranchPk: branch2.PK, executionDepartmentPk: department2.PK, token: "Another token");
			AssertEquals(TimeActionSchedulingState.Scheduled, scheduleWithDifferentJsonParameter.NonPersistentSchedulingState);

			collection = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false });
			AssertEquals(5, collection.Count);
			AssertEquals(4, collection.Count(s => s.TAS_JsonParameter == "{}"));

			var loadedScheduleWithDifferentJsonParameter = collection.SingleOrDefault(s => s.TAS_JsonParameter == "NEW");
			AssertNotNull(loadedScheduleWithDifferentJsonParameter);

			CombineAssertions("Should schedule another action with different json parameter", () =>
			{
				AssertEquals(Constants.TimeActionScheduleStatus.Scheduled, loadedScheduleWithDifferentJsonParameter.TAS_ExecutionStatus);
				AssertEquals("TST", loadedScheduleWithDifferentJsonParameter.TAS_ActionCode);
				AssertEquals(ZDateTime.UtcNow.AddDays(5), loadedScheduleWithDifferentJsonParameter.TAS_ExecutionDateTimeUtc);
				AssertEquals(targetPk, loadedScheduleWithDifferentJsonParameter.TAS_TargetPK);
				AssertEquals("TGT", loadedScheduleWithDifferentJsonParameter.TAS_TargetTableCode);
				AssertEquals("NEW", loadedScheduleWithDifferentJsonParameter.TAS_JsonParameter);
				AssertEquals(branch2.PK, loadedScheduleWithDifferentJsonParameter.TAS_GB_Branch);
				AssertEquals(department2.PK, loadedScheduleWithDifferentJsonParameter.TAS_GE_Department);
				AssertEquals("Another token", loadedScheduleWithDifferentJsonParameter.TAS_Token);
				AssertEquals("", loadedScheduleWithDifferentJsonParameter.TAS_ExecutionResult);
				AssertEquals((byte)0, loadedScheduleWithDifferentJsonParameter.TAS_RetryAttempts);
				AssertEquals(TimeActionSchedulingState.Loaded, loadedScheduleWithDifferentJsonParameter.NonPersistentSchedulingState);
			});
		}

		public void TestShouldNotRescheduleClosedOrFailedActions()
		{
			var scheduleFactory = new ActionScheduleProvider();

			var targetPk = ZGuid.NewZGuid();
			var dateTime = ZDateTime.UtcNow.ToSmallDateTimeFloor();
			var schedule = scheduleFactory.ScheduleOrRescheduleAction("TST", dateTime, targetPk, targetTableCode: "TGT", jsonParameter: "{}");
			AssertEquals(TimeActionSchedulingState.Scheduled, schedule.NonPersistentSchedulingState);

			var collection = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false });
			AssertEquals(1, collection.Count);
			var loadedSchedule = collection.Single();

			CombineAssertions("Should schedule", () =>
			{
				AssertEquals(Constants.TimeActionScheduleStatus.Scheduled, loadedSchedule.TAS_ExecutionStatus);
				AssertEquals("TST", loadedSchedule.TAS_ActionCode);
				AssertEquals(dateTime, loadedSchedule.TAS_ExecutionDateTimeUtc);
				AssertEquals(targetPk, loadedSchedule.TAS_TargetPK);
				AssertEquals("TGT", loadedSchedule.TAS_TargetTableCode);
				AssertEquals("{}", loadedSchedule.TAS_JsonParameter);
			});

			schedule.ExecutionStatus = Constants.TimeActionScheduleStatus.Closed;
			scheduleFactory.ChangeState(schedule);

			schedule = scheduleFactory.ScheduleOrRescheduleAction("TST", dateTime, targetPk, targetTableCode: "TGT", jsonParameter: "{}");
			AssertEquals(TimeActionSchedulingState.Scheduled, schedule.NonPersistentSchedulingState);

			collection = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false });
			AssertEquals(2, collection.Count);

			var loadedScheduleClosed = collection.SingleOrDefault(s => s.TAS_ExecutionStatus == Constants.TimeActionScheduleStatus.Closed);
			AssertNotNull(loadedScheduleClosed);

			var loadedScheduleNew = collection.SingleOrDefault(s => s.TAS_ExecutionStatus == Constants.TimeActionScheduleStatus.Scheduled);
			AssertNotNull(loadedScheduleNew);

			CombineAssertions("Should leave the closed schedule untouched", () =>
			{
				AssertEquals(Constants.TimeActionScheduleStatus.Closed, loadedScheduleClosed.TAS_ExecutionStatus);
				AssertEquals("TST", loadedScheduleClosed.TAS_ActionCode);
				AssertEquals(dateTime, loadedScheduleClosed.TAS_ExecutionDateTimeUtc);
				AssertEquals(targetPk, loadedScheduleClosed.TAS_TargetPK);
				AssertEquals("TGT", loadedScheduleClosed.TAS_TargetTableCode);
				AssertEquals("{}", loadedScheduleClosed.TAS_JsonParameter);
			});

			CombineAssertions("Should schedule a new action", () =>
			{
				AssertEquals(Constants.TimeActionScheduleStatus.Scheduled, loadedScheduleNew.TAS_ExecutionStatus);
				AssertEquals("TST", loadedScheduleNew.TAS_ActionCode);
				AssertEquals(dateTime, loadedScheduleNew.TAS_ExecutionDateTimeUtc);
				AssertEquals(targetPk, loadedScheduleNew.TAS_TargetPK);
				AssertEquals("TGT", loadedScheduleNew.TAS_TargetTableCode);
				AssertEquals("{}", loadedScheduleNew.TAS_JsonParameter);
			});

			schedule.ExecutionStatus = Constants.TimeActionScheduleStatus.Failed;
			scheduleFactory.ChangeState(schedule);

			schedule = scheduleFactory.ScheduleOrRescheduleAction("TST", dateTime, targetPk, targetTableCode: "TGT", jsonParameter: "{}");
			AssertEquals(TimeActionSchedulingState.Scheduled, schedule.NonPersistentSchedulingState);

			collection = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false });
			AssertEquals(3, collection.Count);

			loadedScheduleClosed = collection.SingleOrDefault(s => s.TAS_ExecutionStatus == Constants.TimeActionScheduleStatus.Closed);
			AssertNotNull(loadedScheduleClosed);

			var loadedScheduleFailed = collection.SingleOrDefault(s => s.TAS_ExecutionStatus == Constants.TimeActionScheduleStatus.Failed);
			AssertNotNull(loadedScheduleFailed);

			loadedScheduleNew = collection.SingleOrDefault(s => s.TAS_ExecutionStatus == Constants.TimeActionScheduleStatus.Scheduled);
			AssertNotNull(loadedScheduleNew);

			CombineAssertions("Should schedule a new action", () =>
			{
				AssertEquals(Constants.TimeActionScheduleStatus.Scheduled, loadedScheduleNew.TAS_ExecutionStatus);
				AssertEquals("TST", loadedScheduleNew.TAS_ActionCode);
				AssertEquals(dateTime, loadedScheduleNew.TAS_ExecutionDateTimeUtc);
				AssertEquals(targetPk, loadedScheduleNew.TAS_TargetPK);
				AssertEquals("TGT", loadedScheduleNew.TAS_TargetTableCode);
				AssertEquals("{}", loadedScheduleNew.TAS_JsonParameter);
			});

			schedule.ExecutionStatus = Constants.TimeActionScheduleStatus.Suspended;
			scheduleFactory.ChangeState(schedule);

			schedule = scheduleFactory.ScheduleOrRescheduleAction("TST", dateTime, targetPk, targetTableCode: "TGT", jsonParameter: "{}");
			AssertEquals(TimeActionSchedulingState.Rescheduled, schedule.NonPersistentSchedulingState);

			collection = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false });
			AssertEquals("Should reschedule rather than scheduling a new action", 3, collection.Count);

			schedule = scheduleFactory.ScheduleOrRescheduleAction("TST", dateTime, targetPk, targetTableCode: "TGT", jsonParameter: "{}");
			AssertEquals(TimeActionSchedulingState.Rescheduled, schedule.NonPersistentSchedulingState);

			collection = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false });
			AssertEquals("Should reschedule rather than scheduling a new action", 3, collection.Count);
		}

		[TestDate(2024, 12, 24)]
		public void TestScheduleActionIfNotScheduled()
		{
			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			branch1.GB_Code = "BR1";
			branch1.GB_RL_NKHomePort = "AUMEL";

			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;
			branch2.GB_Code = "BR2";
			branch2.GB_RL_NKHomePort = "AUMEL";

			var department1 = Factory.New<GlbDepartment>();
			department1.GE_Code = "DP1";

			var department2 = Factory.New<GlbDepartment>();
			department2.GE_Code = "DP2";

			Factory.Save();

			var scheduleFactory = new ActionScheduleProvider();

			var targetPk = ZGuid.NewZGuid();
			var schedule = scheduleFactory.ScheduleActionIfNotScheduled("TST", ZDateTime.UtcNow, targetPk, targetTableCode: "TGT", jsonParameter: "{}", executionBranchPk: branch1.PK, executionDepartmentPk: department1.PK, token: "token", scheduleSuspended: true);
			AssertEquals(TimeActionSchedulingState.Scheduled, schedule.NonPersistentSchedulingState);

			var collection = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false });
			AssertEquals(1, collection.Count);
			var loadedSchedule = collection.Single();

			CombineAssertions("Should schedule", () =>
			{
				AssertEquals(Constants.TimeActionScheduleStatus.Suspended, loadedSchedule.TAS_ExecutionStatus);
				AssertEquals("TST", loadedSchedule.TAS_ActionCode);
				AssertEquals(ZDateTime.UtcNow, loadedSchedule.TAS_ExecutionDateTimeUtc);
				AssertEquals(targetPk, loadedSchedule.TAS_TargetPK);
				AssertEquals("TGT", loadedSchedule.TAS_TargetTableCode);
				AssertEquals("{}", loadedSchedule.TAS_JsonParameter);
				AssertEquals(branch1.PK, loadedSchedule.TAS_GB_Branch);
				AssertEquals(department1.PK, loadedSchedule.TAS_GE_Department);
				AssertEquals("token", loadedSchedule.TAS_Token);
				AssertEquals("", loadedSchedule.TAS_ExecutionResult);
				AssertEquals((byte)0, loadedSchedule.TAS_RetryAttempts);
				AssertEquals(TimeActionSchedulingState.Loaded, loadedSchedule.NonPersistentSchedulingState);
			});

			var scheduleWithDifferentParameters = scheduleFactory.ScheduleActionIfNotScheduled("TST", ZDateTime.UtcNow.AddDays(1), targetPk, targetTableCode: "TGT", jsonParameter: "{}", executionBranchPk: branch2.PK, executionDepartmentPk: department2.PK, token: "Another token", scheduleSuspended: false);
			AssertEquals(TimeActionSchedulingState.Loaded, scheduleWithDifferentParameters.NonPersistentSchedulingState);

			collection = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false });
			AssertEquals(1, collection.Count);
			loadedSchedule = collection.Single();

			CombineAssertions("Should not schedule new actions or reschedule existing ones - the old schedule should stay untouched", () =>
			{
				AssertEquals(Constants.TimeActionScheduleStatus.Suspended, loadedSchedule.TAS_ExecutionStatus);
				AssertEquals("TST", loadedSchedule.TAS_ActionCode);
				AssertEquals(ZDateTime.UtcNow, loadedSchedule.TAS_ExecutionDateTimeUtc);
				AssertEquals(targetPk, loadedSchedule.TAS_TargetPK);
				AssertEquals("TGT", loadedSchedule.TAS_TargetTableCode);
				AssertEquals("{}", loadedSchedule.TAS_JsonParameter);
				AssertEquals(branch1.PK, loadedSchedule.TAS_GB_Branch);
				AssertEquals(department1.PK, loadedSchedule.TAS_GE_Department);
				AssertEquals("token", loadedSchedule.TAS_Token);
				AssertEquals("", loadedSchedule.TAS_ExecutionResult);
				AssertEquals((byte)0, loadedSchedule.TAS_RetryAttempts);
				AssertEquals(TimeActionSchedulingState.Loaded, loadedSchedule.NonPersistentSchedulingState);
			});

			var scheduleWithDifferentActionCode = scheduleFactory.ScheduleActionIfNotScheduled("NEW", ZDateTime.UtcNow.AddDays(2), targetPk, targetTableCode: "TGT", jsonParameter: "{}", executionBranchPk: branch2.PK, executionDepartmentPk: department2.PK, token: "Another token");
			AssertEquals(TimeActionSchedulingState.Scheduled, scheduleWithDifferentActionCode.NonPersistentSchedulingState);

			collection = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false });
			AssertEquals(2, collection.Count);

			loadedSchedule = collection.SingleOrDefault(s => s.TAS_ActionCode == "TST");
			AssertNotNull(loadedSchedule);

			var loadedScheduleWithDifferentActionCode = collection.SingleOrDefault(s => s.TAS_ActionCode == "NEW");
			AssertNotNull(loadedScheduleWithDifferentActionCode);

			CombineAssertions("Old schedule should stay untouched", () =>
			{
				AssertEquals(Constants.TimeActionScheduleStatus.Suspended, loadedSchedule.TAS_ExecutionStatus);
				AssertEquals("TST", loadedSchedule.TAS_ActionCode);
				AssertEquals(ZDateTime.UtcNow, loadedSchedule.TAS_ExecutionDateTimeUtc);
				AssertEquals(targetPk, loadedSchedule.TAS_TargetPK);
				AssertEquals("TGT", loadedSchedule.TAS_TargetTableCode);
				AssertEquals("{}", loadedSchedule.TAS_JsonParameter);
				AssertEquals(branch1.PK, loadedSchedule.TAS_GB_Branch);
				AssertEquals(department1.PK, loadedSchedule.TAS_GE_Department);
				AssertEquals("token", loadedSchedule.TAS_Token);
				AssertEquals("", loadedSchedule.TAS_ExecutionResult);
				AssertEquals((byte)0, loadedSchedule.TAS_RetryAttempts);
				AssertEquals(TimeActionSchedulingState.Loaded, loadedSchedule.NonPersistentSchedulingState);
			});

			CombineAssertions("Should schedule another action with different action code", () =>
			{
				AssertEquals(Constants.TimeActionScheduleStatus.Scheduled, loadedScheduleWithDifferentActionCode.TAS_ExecutionStatus);
				AssertEquals("NEW", loadedScheduleWithDifferentActionCode.TAS_ActionCode);
				AssertEquals(ZDateTime.UtcNow.AddDays(2), loadedScheduleWithDifferentActionCode.TAS_ExecutionDateTimeUtc);
				AssertEquals(targetPk, loadedScheduleWithDifferentActionCode.TAS_TargetPK);
				AssertEquals("TGT", loadedScheduleWithDifferentActionCode.TAS_TargetTableCode);
				AssertEquals("{}", loadedScheduleWithDifferentActionCode.TAS_JsonParameter);
				AssertEquals(branch2.PK, loadedScheduleWithDifferentActionCode.TAS_GB_Branch);
				AssertEquals(department2.PK, loadedScheduleWithDifferentActionCode.TAS_GE_Department);
				AssertEquals("Another token", loadedScheduleWithDifferentActionCode.TAS_Token);
				AssertEquals("", loadedScheduleWithDifferentActionCode.TAS_ExecutionResult);
				AssertEquals((byte)0, loadedScheduleWithDifferentActionCode.TAS_RetryAttempts);
				AssertEquals(TimeActionSchedulingState.Loaded, loadedScheduleWithDifferentActionCode.NonPersistentSchedulingState);
			});

			var newTargetPk = Guid.NewGuid();
			var scheduleWithDifferentTargetPK = scheduleFactory.ScheduleActionIfNotScheduled("TST", ZDateTime.UtcNow.AddDays(3), newTargetPk, targetTableCode: "TGT", jsonParameter: "{}", executionBranchPk: branch2.PK, executionDepartmentPk: department2.PK, token: "Another token");
			AssertEquals(TimeActionSchedulingState.Scheduled, scheduleWithDifferentTargetPK.NonPersistentSchedulingState);

			collection = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false });
			AssertEquals(3, collection.Count);
			AssertEquals(2, collection.Count(s => s.TAS_TargetPK == targetPk));

			var loadedScheduleWithDifferentTargetPK = collection.SingleOrDefault(s => s.TAS_TargetPK == newTargetPk);
			AssertNotNull(loadedScheduleWithDifferentTargetPK);

			CombineAssertions("Should schedule another action with different target PK", () =>
			{
				AssertEquals(Constants.TimeActionScheduleStatus.Scheduled, loadedScheduleWithDifferentTargetPK.TAS_ExecutionStatus);
				AssertEquals("TST", loadedScheduleWithDifferentTargetPK.TAS_ActionCode);
				AssertEquals(ZDateTime.UtcNow.AddDays(3), loadedScheduleWithDifferentTargetPK.TAS_ExecutionDateTimeUtc);
				AssertEquals(newTargetPk, loadedScheduleWithDifferentTargetPK.TAS_TargetPK);
				AssertEquals("TGT", loadedScheduleWithDifferentTargetPK.TAS_TargetTableCode);
				AssertEquals("{}", loadedScheduleWithDifferentTargetPK.TAS_JsonParameter);
				AssertEquals(branch2.PK, loadedScheduleWithDifferentTargetPK.TAS_GB_Branch);
				AssertEquals(department2.PK, loadedScheduleWithDifferentTargetPK.TAS_GE_Department);
				AssertEquals("Another token", loadedScheduleWithDifferentTargetPK.TAS_Token);
				AssertEquals("", loadedScheduleWithDifferentTargetPK.TAS_ExecutionResult);
				AssertEquals((byte)0, loadedScheduleWithDifferentTargetPK.TAS_RetryAttempts);
				AssertEquals(TimeActionSchedulingState.Loaded, loadedScheduleWithDifferentTargetPK.NonPersistentSchedulingState);
			});

			var scheduleWithDifferentTargetTableCode = scheduleFactory.ScheduleActionIfNotScheduled("TST", ZDateTime.UtcNow.AddDays(4), targetPk, targetTableCode: "NEW", jsonParameter: "{}", executionBranchPk: branch2.PK, executionDepartmentPk: department2.PK, token: "Another token");
			AssertEquals(TimeActionSchedulingState.Scheduled, scheduleWithDifferentTargetTableCode.NonPersistentSchedulingState);

			collection = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false });
			AssertEquals(4, collection.Count);
			AssertEquals(3, collection.Count(s => s.TAS_TargetTableCode == "TGT"));

			var loadedScheduleWithDifferentTargetTableCode = collection.SingleOrDefault(s => s.TAS_TargetTableCode == "NEW");
			AssertNotNull(loadedScheduleWithDifferentTargetTableCode);

			CombineAssertions("Should schedule another action with different target table code", () =>
			{
				AssertEquals(Constants.TimeActionScheduleStatus.Scheduled, loadedScheduleWithDifferentTargetTableCode.TAS_ExecutionStatus);
				AssertEquals("TST", loadedScheduleWithDifferentTargetTableCode.TAS_ActionCode);
				AssertEquals(ZDateTime.UtcNow.AddDays(4), loadedScheduleWithDifferentTargetTableCode.TAS_ExecutionDateTimeUtc);
				AssertEquals(targetPk, loadedScheduleWithDifferentTargetTableCode.TAS_TargetPK);
				AssertEquals("NEW", loadedScheduleWithDifferentTargetTableCode.TAS_TargetTableCode);
				AssertEquals("{}", loadedScheduleWithDifferentTargetTableCode.TAS_JsonParameter);
				AssertEquals(branch2.PK, loadedScheduleWithDifferentTargetTableCode.TAS_GB_Branch);
				AssertEquals(department2.PK, loadedScheduleWithDifferentTargetTableCode.TAS_GE_Department);
				AssertEquals("Another token", loadedScheduleWithDifferentTargetTableCode.TAS_Token);
				AssertEquals("", loadedScheduleWithDifferentTargetTableCode.TAS_ExecutionResult);
				AssertEquals((byte)0, loadedScheduleWithDifferentTargetTableCode.TAS_RetryAttempts);
				AssertEquals(TimeActionSchedulingState.Loaded, loadedScheduleWithDifferentTargetTableCode.NonPersistentSchedulingState);
			});

			var scheduleWithDifferentJsonParameter = scheduleFactory.ScheduleActionIfNotScheduled("TST", ZDateTime.UtcNow.AddDays(5), targetPk, targetTableCode: "TGT", jsonParameter: "NEW", executionBranchPk: branch2.PK, executionDepartmentPk: department2.PK, token: "Another token");
			AssertEquals(TimeActionSchedulingState.Scheduled, scheduleWithDifferentJsonParameter.NonPersistentSchedulingState);

			collection = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false });
			AssertEquals(5, collection.Count);
			AssertEquals(4, collection.Count(s => s.TAS_JsonParameter == "{}"));

			var loadedScheduleWithDifferentJsonParameter = collection.SingleOrDefault(s => s.TAS_JsonParameter == "NEW");
			AssertNotNull(loadedScheduleWithDifferentJsonParameter);

			CombineAssertions("Should schedule another action with different json parameter", () =>
			{
				AssertEquals(Constants.TimeActionScheduleStatus.Scheduled, loadedScheduleWithDifferentJsonParameter.TAS_ExecutionStatus);
				AssertEquals("TST", loadedScheduleWithDifferentJsonParameter.TAS_ActionCode);
				AssertEquals(ZDateTime.UtcNow.AddDays(5), loadedScheduleWithDifferentJsonParameter.TAS_ExecutionDateTimeUtc);
				AssertEquals(targetPk, loadedScheduleWithDifferentJsonParameter.TAS_TargetPK);
				AssertEquals("TGT", loadedScheduleWithDifferentJsonParameter.TAS_TargetTableCode);
				AssertEquals("NEW", loadedScheduleWithDifferentJsonParameter.TAS_JsonParameter);
				AssertEquals(branch2.PK, loadedScheduleWithDifferentJsonParameter.TAS_GB_Branch);
				AssertEquals(department2.PK, loadedScheduleWithDifferentJsonParameter.TAS_GE_Department);
				AssertEquals("Another token", loadedScheduleWithDifferentJsonParameter.TAS_Token);
				AssertEquals("", loadedScheduleWithDifferentJsonParameter.TAS_ExecutionResult);
				AssertEquals((byte)0, loadedScheduleWithDifferentJsonParameter.TAS_RetryAttempts);
				AssertEquals(TimeActionSchedulingState.Loaded, loadedScheduleWithDifferentJsonParameter.NonPersistentSchedulingState);
			});
		}

		public void TestChangeStatusByToken()
		{
			var scheduleFactory = new ActionScheduleProvider();
			var schedule1 = scheduleFactory.ScheduleAction("TST", ZDateTime.UtcNow, ZGuid.NewZGuid(), "TGT", jsonParameter: "1", token: null, scheduleSuspended: true, factory: Factory);
			var schedule2 = scheduleFactory.ScheduleAction("TST", ZDateTime.UtcNow, ZGuid.NewZGuid(), "TGT", jsonParameter: "2", token: "", scheduleSuspended: true, factory: Factory);

			var schedule3 = scheduleFactory.ScheduleAction("TST", ZDateTime.UtcNow, ZGuid.NewZGuid(), "TGT", jsonParameter: "3", token: "AAA", scheduleSuspended: true, factory: Factory);
			var schedule4 = scheduleFactory.ScheduleAction("TST", ZDateTime.UtcNow, ZGuid.NewZGuid(), "TGT", jsonParameter: "4", token: "AAA", scheduleSuspended: false, factory: Factory);
			var schedule5 = scheduleFactory.ScheduleAction("TST", ZDateTime.UtcNow, ZGuid.NewZGuid(), "TGT", jsonParameter: "5", token: "AAA", scheduleSuspended: true, factory: Factory);
			var schedule6 = scheduleFactory.ScheduleAction("TST", ZDateTime.UtcNow, ZGuid.NewZGuid(), "TGT", jsonParameter: "6", token: "AAA", scheduleSuspended: true, factory: Factory);
			schedule5.ExecutionStatus = Constants.TimeActionScheduleStatus.Closed;
			schedule6.ExecutionStatus = Constants.TimeActionScheduleStatus.Failed;

			var schedule7 = scheduleFactory.ScheduleAction("TST", ZDateTime.UtcNow, ZGuid.NewZGuid(), "TGT", jsonParameter: "7", token: "BBB", scheduleSuspended: true, factory: Factory);
			var schedule8 = scheduleFactory.ScheduleAction("TST", ZDateTime.UtcNow, ZGuid.NewZGuid(), "TGT", jsonParameter: "8", token: "BBB", scheduleSuspended: false, factory: Factory);
			var schedule9 = scheduleFactory.ScheduleAction("TST", ZDateTime.UtcNow, ZGuid.NewZGuid(), "TGT", jsonParameter: "9", token: "BBB", scheduleSuspended: true, factory: Factory);
			var schedule10 = scheduleFactory.ScheduleAction("TST", ZDateTime.UtcNow, ZGuid.NewZGuid(), "TGT", jsonParameter: "10", token: "BBB", scheduleSuspended: true, factory: Factory);
			schedule9.ExecutionStatus = Constants.TimeActionScheduleStatus.Closed;
			schedule10.ExecutionStatus = Constants.TimeActionScheduleStatus.Failed;

			var schedule11 = scheduleFactory.ScheduleAction("TST", ZDateTime.UtcNow, ZGuid.NewZGuid(), "TGT", jsonParameter: "11", token: "CCC", scheduleSuspended: true, factory: Factory);

			Factory.Save();

			scheduleFactory.ChangeStatusByToken("AAA", Constants.TimeActionScheduleStatus.Scheduled);

			var collection = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false });
			var loadedScheduledPKs = collection.Where(s => s.TAS_ExecutionStatus == Constants.TimeActionScheduleStatus.Scheduled).Select(s => s.PK).ToArray();
			var loadedSuspendedPKs = collection.Where(s => s.TAS_ExecutionStatus == Constants.TimeActionScheduleStatus.Suspended).Select(s => s.PK).ToArray();
			var loadedClosedPKs = collection.Where(s => s.TAS_ExecutionStatus == Constants.TimeActionScheduleStatus.Closed).Select(s => s.PK).ToArray();
			var loadedFailedPKs = collection.Where(s => s.TAS_ExecutionStatus == Constants.TimeActionScheduleStatus.Failed).Select(s => s.PK).ToArray();
			AssertContainsExactElementsInAnyOrder("Schedules with token AAA should become activated", [schedule3.PK, schedule4.PK, schedule8.PK], loadedScheduledPKs);
			AssertContainsExactElementsInAnyOrder("Other schedules should stay suspended", [schedule1.PK, schedule2.PK, schedule7.PK, schedule11.PK], loadedSuspendedPKs);
			AssertContainsExactElementsInAnyOrder("Closed schedules should stay untouched", [schedule5.PK, schedule9.PK], loadedClosedPKs);
			AssertContainsExactElementsInAnyOrder("Failed schedules should stay untouched", [schedule6.PK, schedule10.PK], loadedFailedPKs);

			scheduleFactory.ChangeStatusByToken("BBB", Constants.TimeActionScheduleStatus.Suspended);

			collection = new TimeActionScheduleCollection(new BusinessObjectFactory() { RefreshEnabled = false });
			loadedScheduledPKs = collection.Where(s => s.TAS_ExecutionStatus == Constants.TimeActionScheduleStatus.Scheduled).Select(s => s.PK).ToArray();
			loadedSuspendedPKs = collection.Where(s => s.TAS_ExecutionStatus == Constants.TimeActionScheduleStatus.Suspended).Select(s => s.PK).ToArray();
			loadedClosedPKs = collection.Where(s => s.TAS_ExecutionStatus == Constants.TimeActionScheduleStatus.Closed).Select(s => s.PK).ToArray();
			loadedFailedPKs = collection.Where(s => s.TAS_ExecutionStatus == Constants.TimeActionScheduleStatus.Failed).Select(s => s.PK).ToArray();
			AssertContainsExactElementsInAnyOrder("Schedules with token BBB should become suspended", [schedule1.PK, schedule2.PK, schedule7.PK, schedule8.PK, schedule11.PK], loadedSuspendedPKs);
			AssertContainsExactElementsInAnyOrder("Other schedules should stay activated", [schedule3.PK, schedule4.PK], loadedScheduledPKs);
			AssertContainsExactElementsInAnyOrder("Closed schedules should stay untouched", [schedule5.PK, schedule9.PK], loadedClosedPKs);
			AssertContainsExactElementsInAnyOrder("Failed schedules should stay untouched", [schedule6.PK, schedule10.PK], loadedFailedPKs);
		}

		public void TestChangesState()
		{
			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			branch1.GB_Code = "BR1";
			branch1.GB_RL_NKHomePort = "AUMEL";

			var department1 = Factory.New<GlbDepartment>();
			department1.GE_Code = "DP1";

			var schedule = Factory.NewWithValidTestData<TimeActionSchedule>();
			schedule.TAS_ExecutionStatus = Constants.TimeActionScheduleStatus.Scheduled;
			Factory.Save();

			schedule.TAS_ExecutionStatus = Constants.TimeActionScheduleStatus.Closed;
			schedule.TAS_ExecutionResult = "Success";
			schedule.TAS_RetryAttempts = 1;
			var newTime = ZDateTime.Now.AddHours(1).ToSmallDateTimeFloor();
			schedule.TAS_ExecutionDateTimeUtc = newTime;
			schedule.TAS_GB_Branch = branch1.PK;
			schedule.TAS_GE_Department = department1.PK;
			schedule.TAS_Token = "token";

			var scheduleFactory = new ActionScheduleProvider();
			scheduleFactory.ChangeState(schedule);

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedSchedule = factory.Load<TimeActionSchedule>(schedule.PK);

			AssertEquals(Constants.TimeActionScheduleStatus.Closed, loadedSchedule.TAS_ExecutionStatus);
			AssertEquals(newTime, loadedSchedule.TAS_ExecutionDateTimeUtc);
			AssertEquals("Success", loadedSchedule.TAS_ExecutionResult);
			AssertEquals((ZByte)1, loadedSchedule.TAS_RetryAttempts);
			AssertEquals(branch1.PK, loadedSchedule.TAS_GB_Branch);
			AssertEquals(department1.PK, loadedSchedule.TAS_GE_Department);
			AssertEquals("token", loadedSchedule.TAS_Token);
		}

		[TestDate(2022, 6, 8)]
		public void TestGetSchedules()
		{
			var executionDateTime = ZDateTime.UtcNow.AddMonths(-1);
			var referenceTime = ZDateTime.UtcNow.AddDays(1).ToDateTime();
			var pk1 = ZGuid.NewZGuid();
			var pk2 = ZGuid.NewZGuid();

			var scheduleFactory = new ActionScheduleProvider();

			TestDateAttribute.Date = referenceTime;
			var schedule1 = scheduleFactory.ScheduleAction("AAA", executionDateTime, pk1, "01", jsonParameter: "parameter1");

			TestDateAttribute.Date = referenceTime.AddMinutes(1);
			var schedule2 = scheduleFactory.ScheduleAction("BBB", executionDateTime, pk1, "01");

			TestDateAttribute.Date = referenceTime.AddMinutes(2);
			var schedule3 = scheduleFactory.ScheduleAction("AAA", executionDateTime, pk1, "01");

			TestDateAttribute.Date = referenceTime.AddMinutes(3);
			var schedule4 = scheduleFactory.ScheduleAction("CCC", executionDateTime, pk1, "02");

			TestDateAttribute.Date = referenceTime.AddMinutes(4);
			var schedule5 = scheduleFactory.ScheduleAction("CCC", executionDateTime, pk1, "02");

			TestDateAttribute.Date = referenceTime.AddHours(1);
			var schedule6 = scheduleFactory.ScheduleAction("AAA", executionDateTime, pk2, "02");
			schedule6.ExecutionStatus = Constants.TimeActionScheduleStatus.Closed;
			scheduleFactory.ChangeState(schedule6);

			TestDateAttribute.Date = referenceTime.AddHours(2);
			var schedule7 = scheduleFactory.ScheduleAction("BBB", executionDateTime, pk2, "03");

			TestDateAttribute.Date = referenceTime.AddHours(3);
			var schedule8 = scheduleFactory.ScheduleAction("DDD", executionDateTime, pk2, "03");
			schedule8.ExecutionStatus = Constants.TimeActionScheduleStatus.Failed;
			scheduleFactory.ChangeState(schedule8);

			TestDateAttribute.Date = referenceTime.AddHours(4);
			var schedule9 = scheduleFactory.ScheduleAction("AAA", executionDateTime, pk2, "02");

			AssertGetSchedules(new[] { schedule3 }, "AAA", referenceTime, pk1, "01");
			AssertGetSchedules(new[] { schedule1, schedule3 }, "AAA", referenceTime.AddSeconds(-1), pk1, "01");
			AssertGetSchedules(new[] { schedule3 }, "AAA", referenceTime.AddMinutes(1), pk1, "01");
			AssertGetSchedules(Array.Empty<IActionSchedule>(), "AAA", referenceTime.AddMinutes(2), pk1, "01");
			AssertGetSchedules(new[] { schedule2 }, "BBB", referenceTime, pk1, "01");
			AssertGetSchedules(new[] { schedule4, schedule5 }, "CCC", referenceTime, pk1, "02");
			AssertGetSchedules(new[] { schedule5 }, "CCC", referenceTime.AddMinutes(3), pk1, "02");
			AssertGetSchedules(Array.Empty<IActionSchedule>(), "CCC", referenceTime.AddMinutes(4), pk1, "02");
			AssertGetSchedules(new[] { schedule6, schedule9 }, "AAA", referenceTime, pk2, "02");
			AssertGetSchedules(new[] { schedule7 }, "BBB", referenceTime, pk2, "03");
			AssertGetSchedules(new[] { schedule8 }, "DDD", referenceTime, pk2, "03");

			AssertGetSchedules(new[] { schedule1 }, "AAA", referenceTime.AddSeconds(-1), pk1, "01", jsonParameter: "parameter1");
		}

		void AssertGetSchedules(IEnumerable<IActionSchedule> expectedSchedules, string actionCode, ZDateTime scheduledLaterThanDateTimeUtc, ZGuid targetPk, string targetTableCode)
		{
			var scheduleFactory = new ActionScheduleProvider();
			AssertGetSchedules(expectedSchedules, () => scheduleFactory.GetSchedules(new BusinessObjectFactory(), actionCode, targetPk, targetTableCode, scheduledLaterThanDateTimeUtc: scheduledLaterThanDateTimeUtc));
		}

		void AssertGetSchedules(IEnumerable<IActionSchedule> expectedSchedules, string actionCode, ZDateTime scheduledLaterThanDateTimeUtc, ZGuid targetPk, string targetTableCode, string jsonParameter)
		{
			var scheduleFactory = new ActionScheduleProvider();
			AssertGetSchedules(expectedSchedules, () => scheduleFactory.GetSchedules(new BusinessObjectFactory(), actionCode, targetPk, targetTableCode, jsonParameter, scheduledLaterThanDateTimeUtc: scheduledLaterThanDateTimeUtc));
		}

		void AssertGetSchedules(IEnumerable<IActionSchedule> expectedSchedules, Func<IReadOnlyCollection<IActionSchedule>> actualSchedulesProvider)
		{
			var actualSchedules = actualSchedulesProvider.Invoke();
			AssertContainsExactElementsInAnyOrder($@"Expected schedules:
{string.Join(System.Environment.NewLine, expectedSchedules)}

Actual schedules:
{string.Join(System.Environment.NewLine, actualSchedules)}",
				expectedSchedules.Select(s => s.PK), actualSchedules.Select(s => s.PK));
		}

		protected override void SetUp()
		{
			base.SetUp();
			Factory.RefreshEnabled = false;
		}
	}
}
