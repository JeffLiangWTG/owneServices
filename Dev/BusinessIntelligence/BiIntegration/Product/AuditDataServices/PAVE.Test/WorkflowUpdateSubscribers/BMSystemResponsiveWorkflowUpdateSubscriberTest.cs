using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.AuditDataServices.PAVE.Subscribers;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.TimeEngineScheduler.Business;
using Enterprise.TimeEngineScheduler.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.PAVE.Test
{
	[TestedType(typeof(BMSystemResponsiveWorkflowUpdateSubscriber))]
	class BMSystemResponsiveWorkflowUpdateSubscriberTest : PAVESubscriberBaseTest
	{
		protected override string ExpectedCode => "BSW";

		protected override ITableSchema ExpectedTable => BMSystemSchema.Instance;

		protected override IEnumerable<SchemaColumn> ExpectedSpecificColumns => [BMSystemSchema.FS_IsLive];

		public override void TestCustomFilter()
		{
			var subscriber = new BMSystemResponsiveWorkflowUpdateSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		public override void TestIsEnabled()
		{
			void AssertDisabled()
			{
				var changeTable = CreateAuditBMSystemTable(5);
				ProcessChanges(changeTable);

				AssertMultilineASCIIEquals(@"5 changes were skipped as the BSW subscriber is disabled.
", Logger.ToString());
				ResetLogger();
			}

			BMSRegistry.BufferManagementEnabled = false;
			BMSRegistry.EnableResponsivePAVEDataProcessing = false;
			BMSRegistry.EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges = false;
			AssertDisabled();

			BMSRegistry.BufferManagementEnabled = false;
			BMSRegistry.EnableResponsivePAVEDataProcessing = false;
			BMSRegistry.EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges = true;
			AssertDisabled();

			BMSRegistry.BufferManagementEnabled = false;
			BMSRegistry.EnableResponsivePAVEDataProcessing = true;
			BMSRegistry.EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges = false;
			AssertDisabled();

			BMSRegistry.BufferManagementEnabled = false;
			BMSRegistry.EnableResponsivePAVEDataProcessing = true;
			BMSRegistry.EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges = true;
			AssertDisabled();

			BMSRegistry.BufferManagementEnabled = true;
			BMSRegistry.EnableResponsivePAVEDataProcessing = false;
			BMSRegistry.EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges = false;
			AssertDisabled();

			BMSRegistry.BufferManagementEnabled = true;
			BMSRegistry.EnableResponsivePAVEDataProcessing = false;
			BMSRegistry.EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges = true;
			AssertDisabled();

			BMSRegistry.BufferManagementEnabled = true;
			BMSRegistry.EnableResponsivePAVEDataProcessing = true;
			BMSRegistry.EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges = false;
			AssertDisabled();

			BMSRegistry.BufferManagementEnabled = true;
			BMSRegistry.EnableResponsivePAVEDataProcessing = true;
			BMSRegistry.EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges = true;
			var changeTable = CreateAuditBMSystemTable(5);
			ProcessChanges(changeTable);
			AssertNotContains("changes were skipped", Logger.ToString());

			// other flags should not affect
			BMSRegistry.EnablePaveExperimentalFeatures = false;
			BMSRegistry.AutoAssignCapabilityTasksOnChanges = false;
			changeTable = CreateAuditBMSystemTable(5);
			ProcessChanges(changeTable);
			AssertNotContains("changes were skipped", Logger.ToString());
		}

		public void TestShouldChangeScheduledActionsStatus()
		{
			var system1PK = Guid.NewGuid();
			var system2PK = Guid.NewGuid();
			var system1Schedules = new List<IActionSchedule>();
			var system2Schedules = new List<IActionSchedule>();
			const int system1SchedulesCount = 15;
			const int system2SchedulesCount = 5;

			for (int i = 0; i < system1SchedulesCount; i++)
			{
				system1Schedules.Add(ScheduleWorkflowsResponsiveUpdate(system1PK, scheduleSuspended: true));
			}

			for (int i = 0; i < system2SchedulesCount; i++)
			{
				system2Schedules.Add(ScheduleWorkflowsResponsiveUpdate(system2PK, scheduleSuspended: true));
			}
			var system1ActionPKs = system1Schedules.Select(s => s.PK).ToArray();
			var system2ActionPKs = system2Schedules.Select(s => s.PK).ToArray();

			factory.Save();

			var collection = new TimeActionScheduleCollection(new BusinessObjectFactory());
			Assert("Precondition: all actions should be suspended", collection.All(s => s.TAS_ExecutionStatus == Constants.TimeActionScheduleStatus.Suspended));

			var changeTable = CreateAuditBMSystemTable();
			var row = CreateAuditBMSystemRow(changeTable, system1PK, isLive: false);
			row.AcceptChanges();
			row.SetModified();
			row[BMSystemSchema.Constants.FS_IsLive] = true;

			AssertEquals("Precondition: should be modified", DataRowState.Modified, row.RowState);
			Assert("Precondition: should have original version", row.HasVersion(DataRowVersion.Original));
			Assert("Precondition: should have current version", row.HasVersion(DataRowVersion.Current));
			AssertEquals("Precondition: original FS_IsLive", false, row[BMSystemSchema.Constants.FS_IsLive, DataRowVersion.Original]);
			AssertEquals("Precondition: current FS_IsLive", true, row[BMSystemSchema.Constants.FS_IsLive, DataRowVersion.Current]);

			ProcessChanges(changeTable);

			collection = new TimeActionScheduleCollection(new BusinessObjectFactory());
			Assert("All actions related to system1 should be active now", collection.Where(s => system1ActionPKs.Contains(s.PK)).All(s => s.TAS_ExecutionStatus == Constants.TimeActionScheduleStatus.Scheduled));
			Assert("All actions related to system2 should be still suspended", collection.Where(s => system2ActionPKs.Contains(s.PK)).All(s => s.TAS_ExecutionStatus == Constants.TimeActionScheduleStatus.Suspended));
			AssertMultilineASCIIEquals(@"Activated scheduled responsive workflow updates for system Test (IsLive = True).
", Logger.ToString());
			ResetLogger();

			changeTable = CreateAuditBMSystemTable();
			row = CreateAuditBMSystemRow(changeTable, system1PK, isLive: true);
			row.AcceptChanges();
			row.SetModified();
			row[BMSystemSchema.Constants.FS_IsLive] = false;

			AssertEquals("Precondition: should be modified", DataRowState.Modified, row.RowState);
			Assert("Precondition: should have original version", row.HasVersion(DataRowVersion.Original));
			Assert("Precondition: should have current version", row.HasVersion(DataRowVersion.Current));
			AssertEquals("Precondition: original FS_IsLive", true, row[BMSystemSchema.Constants.FS_IsLive, DataRowVersion.Original]);
			AssertEquals("Precondition: current FS_IsLive", false, row[BMSystemSchema.Constants.FS_IsLive, DataRowVersion.Current]);

			ProcessChanges(changeTable);

			collection = new TimeActionScheduleCollection(new BusinessObjectFactory());
			Assert("All actions related to system1 should be suspended again", collection.Where(s => system1ActionPKs.Contains(s.PK)).All(s => s.TAS_ExecutionStatus == Constants.TimeActionScheduleStatus.Suspended));
			Assert("All actions related to system2 should be still suspended", collection.Where(s => system2ActionPKs.Contains(s.PK)).All(s => s.TAS_ExecutionStatus == Constants.TimeActionScheduleStatus.Suspended));
			AssertMultilineASCIIEquals(@"Suspended scheduled responsive workflow updates for system Test (IsLive = False).
", Logger.ToString());
		}

		#region Implementation

		ILogger logger;

		protected override ILogger Logger => logger;

		void ResetLogger()
		{
			logger = new SimpleLogger();
		}

		DataTable CreateAuditBMSystemTable(int numEntries = 0)
		{
			var changeTable = new DataTable();

			changeTable.Columns.Add(BMSystemSchema.Constants.PK, typeof(Guid));
			changeTable.Columns.Add(BMSystemSchema.Constants.FS_IsLive, typeof(bool));
			changeTable.Columns.Add(BMSystemSchema.Constants.FS_Name, typeof(string));
			changeTable.Columns.Add("TranEndTimeUtc", typeof(DateTime));

			for (var i = 0; i < numEntries; i++)
			{
				var row = CreateAuditBMSystemRow(changeTable);
				row.AcceptChanges();
				row.SetModified();
			}

			return changeTable;
		}

		protected override DataTable GetTestDataTable() => CreateAuditBMSystemTable();

		DataRow CreateAuditBMSystemRow(DataTable dataTable, Guid? systemPK = null, bool isLive = true, DateTime? tranEndTimeUtc = null)
		{
			var row = dataTable.NewRow();

			row["TranEndTimeUtc"] = tranEndTimeUtc ?? ZDateTime.UtcNow.ToDateTime();

			row[BMSystemSchema.Constants.PK] = systemPK ?? Guid.NewGuid();
			row[BMSystemSchema.Constants.FS_IsLive] = isLive;
			row[BMSystemSchema.Constants.FS_Name] = "Test";
			dataTable.Rows.Add(row);

			AssertEquals("Precondition: should be added", DataRowState.Added, row.RowState);
			Assert("Precondition: should have no original version", !row.HasVersion(DataRowVersion.Original));
			Assert("Precondition: should have current version", row.HasVersion(DataRowVersion.Current));

			return row;
		}

		IActionSchedule ScheduleWorkflowsResponsiveUpdate(Guid systemPK, bool scheduleSuspended)
		{
			var schedule = new ActionScheduleProvider().ScheduleOrRescheduleAction(ProcessHeaderResponsiveActionConstants.ProcessHeaderResponsiveUpdateSchedulerActionCode,
				ZDateTime.UtcNow,
				targetPk: Guid.NewGuid(),
				targetTableCode: BMComponentSchema.Constants.Prefix,
				jsonParameter: ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer,
				token: systemPK.ToString(),
				scheduleSuspended: scheduleSuspended,
				factory: factory);
			return schedule;
		}

		#endregion

		#region Setup And TearDown

		BusinessObjectFactory factory;

		protected override void SetUp()
		{
			base.SetUp();

			BMSRegistry.BufferManagementEnabled = true;
			BMSRegistry.EnableResponsivePAVEDataProcessing = true;
			BMSRegistry.EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges = true;

			ResetLogger();

			factory = new BusinessObjectFactory();
		}

		#endregion
	}
}
