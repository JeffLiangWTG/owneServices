using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.AuditDataServices.PAVE.Subscribers;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.TimeEngineScheduler.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.PAVE.Test
{
	[TestedType(typeof(BMComponentLinkResponsiveWorkflowUpdateSubscriber))]
	class BMComponentLinkResponsiveWorkflowUpdateSubscriberTest : PAVESubscriberBaseTest
	{
		protected override string ExpectedCode => "CLW";

		protected override ITableSchema ExpectedTable => BMComponentLinkSchema.Instance;

		protected override IEnumerable<SchemaColumn> ExpectedSpecificColumns => [BMComponentLinkSchema.FL_TransferRulesEnabled, BMComponentLinkSchema.FL_FC_ComponentTo, BMComponentLinkSchema.FL_IsReleaseGateRuleApplied];

		public override void TestCustomFilter()
		{
			var subscriber = new BMComponentLinkResponsiveWorkflowUpdateSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		public override void TestIsEnabled()
		{
			void AssertDisabled()
			{
				var changeTable = CreateAuditBMComponentLinkTable(5);
				ProcessChanges(changeTable);

				AssertMultilineASCIIEquals(@"5 changes were skipped as the CLW subscriber is disabled.
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
			var changeTable = CreateAuditBMComponentLinkTable(5);
			ProcessChanges(changeTable);
			AssertNotContains("changes were skipped", Logger.ToString());

			// other flags should not affect
			BMSRegistry.EnablePaveExperimentalFeatures = false;
			BMSRegistry.AutoAssignCapabilityTasksOnChanges = false;
			changeTable = CreateAuditBMComponentLinkTable(5);
			ProcessChanges(changeTable);
			AssertNotContains("changes were skipped", Logger.ToString());
		}

		[TestDate(2025, 1, 13)]
		public void TestShouldScheduleDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenCreatingEnabledLink_ForActiveBMS()
		{
			SetupConfig();
			system.FS_IsLive = true;
			factory.Save();

			CombineAssertions("Adding one link", () =>
			{
				AssertSchedulesDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenCreatingEnabledLink(liveSystem: true, shouldReschedule: false);
			});
			CombineAssertions("Adding another link: should not duplicate already scheduled actions, but reschedule instead", () =>
			{
				AssertSchedulesDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenCreatingEnabledLink(liveSystem: true, shouldReschedule: true);
			});
		}

		[TestDate(2025, 1, 13)]
		public void TestShouldScheduleDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenCreatingEnabledLink_ForInactiveBMS()
		{
			SetupConfig();
			system.FS_IsLive = false;
			factory.Save();

			CombineAssertions("Adding one link", () =>
			{
				AssertSchedulesDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenCreatingEnabledLink(liveSystem: false, shouldReschedule: false);
			});
			CombineAssertions("Adding another link: should not duplicate already scheduled actions, but reschedule instead", () =>
			{
				AssertSchedulesDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenCreatingEnabledLink(liveSystem: false, shouldReschedule: true);
			});
		}

		void AssertSchedulesDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenCreatingEnabledLink(bool liveSystem, bool shouldReschedule)
		{
			var changeTable = CreateAuditBMComponentLinkTable();
			CreateAuditBMComponentLinkRow(changeTable, componentFromPK: componentFrom.PK, transferRulesEnabled: true);

			ProcessChanges(changeTable);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var collection = new TimeActionScheduleCollection(newFactory);

			AssertContainsExactElementsInAnyOrder("Should schedule actions for the ComponentFrom", [componentFrom.PK], collection.Select(a => a.TAS_TargetPK));
			var schedule = collection.Single();
			AssertEquals("Should schedule actions targeting BMComponent", BMComponentSchema.Constants.Prefix, schedule.TAS_TargetTableCode);
			AssertEquals("Should schedule actions with proper code", "PHU", schedule.TAS_ActionCode);
			AssertEquals("Should schedule actions with proper parameter", ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer, schedule.TAS_JsonParameter);
			AssertEquals("Should schedule actions as scheduled or suspended", liveSystem ? "SCH" : "SUS", schedule.TAS_ExecutionStatus);
			AssertEquals("Should store system's PK as a token", system.PK.ToString(), schedule.TAS_Token);
			AssertEquals("Should schedule actions ready for execution", ZDateTime.UtcNow, schedule.TAS_ExecutionDateTimeUtc);

			AssertMultilineASCIIEquals($@"A new component link From -> To, Sequence: 0 (PK = {link.PK}) was added: {(shouldReschedule ? "rescheduled" : "scheduled")} responsive update of dedicated buffers (BUF responsive action) on workflows situated in component From (PK = {componentFrom.PK}) of system Test (IsLive = {(liveSystem ? "Y" : "N")}) with status {(liveSystem ? "SCH" : "SUS")}.
", Logger.ToString());
			ResetLogger();
		}

		[TestDate(2025, 1, 13)]
		public void TestShouldNotSchedulesDedicatedBufferUpdate_WhenComponentFromDoesNotExist()
		{
			SetupConfig();
			factory.Save();

			var nonExistentComponentPK = Guid.NewGuid();

			var changeTable = CreateAuditBMComponentLinkTable();
			var row = CreateAuditBMComponentLinkRow(changeTable, nonExistentComponentPK, transferRulesEnabled: true);
			ProcessChanges(changeTable);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var collection = new TimeActionScheduleCollection(newFactory);

			AssertContainsExactElementsInAnyOrder("Should not schedule actions for the ComponentFrom", [], collection.Select(a => a.TAS_TargetPK));

			AssertMultilineASCIIEquals($@"A new component link From -> To, Sequence: 0 (PK = {link.PK}) was added: no need to schedule responsive update of workflows as component with PK = {nonExistentComponentPK} does not exist.
", Logger.ToString());
		}

		[TestDate(2025, 1, 13)]
		public void TestShouldScheduleDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenEnablingLink_ForActiveBMS()
		{
			SetupConfig();
			system.FS_IsLive = true;
			factory.Save();

			CombineAssertions("Enabling one link", () =>
			{
				AssertSchedulesDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenEnablingLink(liveSystem: true, shouldReschedule: false);
			});
			CombineAssertions("Enabling another link: should not duplicate already scheduled actions", () =>
			{
				AssertSchedulesDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenEnablingLink(liveSystem: true, shouldReschedule: true);
			});
		}

		[TestDate(2025, 1, 13)]
		public void TestShouldScheduleDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenEnablingLink_ForInactiveBMS()
		{
			SetupConfig();
			system.FS_IsLive = false;
			factory.Save();

			CombineAssertions("Enabling one link", () =>
			{
				AssertSchedulesDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenEnablingLink(liveSystem: false, shouldReschedule: false);
			});
			CombineAssertions("Enabling another link: should not duplicate already scheduled actions", () =>
			{
				AssertSchedulesDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenEnablingLink(liveSystem: false, shouldReschedule: true);
			});
		}

		void AssertSchedulesDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenEnablingLink(bool liveSystem, bool shouldReschedule)
		{
			var originalComponentFromPK = Guid.NewGuid();
			var changeTable = CreateAuditBMComponentLinkTable();
			var row = CreateAuditBMComponentLinkRow(changeTable, componentFromPK: originalComponentFromPK, transferRulesEnabled: false);
			row.AcceptChanges();
			row.SetModified();
			row[BMComponentLinkSchema.Constants.FL_TransferRulesEnabled] = true;
			row[BMComponentLinkSchema.Constants.FL_FC_ComponentFrom] = componentFrom.PK.ToGuid(); // we need to check that componentFrom is taken from the current version, not from the original one

			AssertEquals("Precondition: should be modified", DataRowState.Modified, row.RowState);
			Assert("Precondition: should have original version", row.HasVersion(DataRowVersion.Original));
			Assert("Precondition: should have current version", row.HasVersion(DataRowVersion.Current));
			AssertEquals("Precondition: original FL_TransferRulesEnabled", false, row[BMComponentLinkSchema.Constants.FL_TransferRulesEnabled, DataRowVersion.Original]);
			AssertEquals("Precondition: current FL_TransferRulesEnabled", true, row[BMComponentLinkSchema.Constants.FL_TransferRulesEnabled, DataRowVersion.Current]);
			AssertEquals("Precondition: original FL_FC_ComponentFrom", originalComponentFromPK, row[BMComponentLinkSchema.Constants.FL_FC_ComponentFrom, DataRowVersion.Original]);
			AssertEquals("Precondition: current FL_FC_ComponentFrom", componentFrom.PK.ToGuid(), row[BMComponentLinkSchema.Constants.FL_FC_ComponentFrom, DataRowVersion.Current]);

			ProcessChanges(changeTable);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var collection = new TimeActionScheduleCollection(newFactory);

			AssertContainsExactElementsInAnyOrder("Should schedule actions for the ComponentFrom", [componentFrom.PK], collection.Select(a => a.TAS_TargetPK));
			var schedule = collection.Single();
			AssertEquals("Should schedule actions targeting BMComponentLinks", BMComponentSchema.Constants.Prefix, schedule.TAS_TargetTableCode);
			AssertEquals("Should schedule actions with proper code", "PHU", schedule.TAS_ActionCode);
			AssertEquals("Should schedule actions with proper parameter", ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer, schedule.TAS_JsonParameter);
			AssertEquals("Should schedule actions as scheduled or suspended", liveSystem ? "SCH" : "SUS", schedule.TAS_ExecutionStatus);
			AssertEquals("Should store system's PK as a token", system.PK.ToString(), schedule.TAS_Token);
			AssertEquals("Should schedule actions ready for execution", ZDateTime.UtcNow, schedule.TAS_ExecutionDateTimeUtc);

			AssertMultilineASCIIEquals($@"The From -> To, Sequence: 0 (PK = {link.PK}) component link was enabled: {(shouldReschedule ? "rescheduled" : "scheduled")} responsive update of dedicated buffers (BUF responsive action) on workflows situated in component From (PK = {componentFrom.PK}) of system Test (IsLive = {(liveSystem ? "Y" : "N")}) with status {(liveSystem ? "SCH" : "SUS")}.
", Logger.ToString());
			ResetLogger();
		}

		[TestDate(2025, 1, 13)]
		public void TestShouldNotScheduleDedicatedBufferUpdate_OnWorkflows_WhenCreatingDisabledLink()
		{
			SetupConfig();
			factory.Save();

			var changeTable = CreateAuditBMComponentLinkTable();
			var row = CreateAuditBMComponentLinkRow(changeTable, componentFromPK: componentFrom.PK, transferRulesEnabled: false);

			ProcessChanges(changeTable);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var collection = new TimeActionScheduleCollection(newFactory);

			AssertContainsExactElementsInAnyOrder("Should not schedule actions", [], collection.Select(a => a.TAS_TargetPK));
			AssertNullOrEmpty(logger.ToString());
		}

		[TestDate(2025, 1, 13)]
		public void TestShouldScheduleDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenRemovingEnabledLink_ForActiveBMS()
		{
			SetupConfig();
			system.FS_IsLive = true;
			factory.Save();

			CombineAssertions("Removing an enabled link", () =>
			{
				AssertSchedulesDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenRemovingEnabledLink(liveSystem: true, shouldReschedule: false);
			});
			CombineAssertions("Removing another enabled link: should not duplicate already scheduled actions", () =>
			{
				AssertSchedulesDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenRemovingEnabledLink(liveSystem: true, shouldReschedule: true);
			});
		}

		[TestDate(2025, 1, 13)]
		public void TestShouldScheduleDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenRemovingEnabledLink_ForInactiveBMS()
		{
			SetupConfig();
			system.FS_IsLive = false;
			factory.Save();

			CombineAssertions("Removing an enabled link", () =>
			{
				AssertSchedulesDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenRemovingEnabledLink(liveSystem: false, shouldReschedule: false);
			});
			CombineAssertions("Removing another enabled link: should not duplicate already scheduled actions", () =>
			{
				AssertSchedulesDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenRemovingEnabledLink(liveSystem: false, shouldReschedule: true);
			});
		}

		void AssertSchedulesDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenRemovingEnabledLink(bool liveSystem, bool shouldReschedule)
		{
			var changeTable = CreateAuditBMComponentLinkTable();
			var row = CreateAuditBMComponentLinkRow(changeTable, componentFromPK: componentFrom.PK, transferRulesEnabled: true);
			row.AcceptChanges();
			row.Delete();

			AssertEquals("Precondition: should be deleted", DataRowState.Deleted, row.RowState);
			Assert("Precondition: should have original version", row.HasVersion(DataRowVersion.Original));
			Assert("Precondition: should not have current version", !row.HasVersion(DataRowVersion.Current));
			AssertEquals("Precondition: original FL_TransferRulesEnabled", true, row[BMComponentLinkSchema.Constants.FL_TransferRulesEnabled, DataRowVersion.Original]);
			AssertEquals("Precondition: original FL_FC_ComponentFrom", componentFrom.PK, row[BMComponentLinkSchema.Constants.FL_FC_ComponentFrom, DataRowVersion.Original]);

			ProcessChanges(changeTable);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var collection = new TimeActionScheduleCollection(newFactory);

			AssertContainsExactElementsInAnyOrder("Should schedule actions for the ComponentFrom", [componentFrom.PK], collection.Select(a => a.TAS_TargetPK));
			var schedule = collection.Single();
			AssertEquals("Should schedule actions targeting BMComponentLinks", BMComponentSchema.Constants.Prefix, schedule.TAS_TargetTableCode);
			AssertEquals("Should schedule actions with proper code", "PHU", schedule.TAS_ActionCode);
			AssertEquals("Should schedule actions with proper parameter", ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer, schedule.TAS_JsonParameter);
			AssertEquals("Should schedule actions as scheduled or suspended", liveSystem ? "SCH" : "SUS", schedule.TAS_ExecutionStatus);
			AssertEquals("Should store system's PK as a token", system.PK.ToString(), schedule.TAS_Token);
			AssertEquals("Should schedule actions ready for execution", ZDateTime.UtcNow, schedule.TAS_ExecutionDateTimeUtc);

			AssertMultilineASCIIEquals($@"The From -> To, Sequence: 0 (PK = {link.PK}) component link was deleted: {(shouldReschedule ? "rescheduled" : "scheduled")} responsive update of dedicated buffers (BUF responsive action) on workflows situated in component From (PK = {componentFrom.PK}) of system Test (IsLive = {(liveSystem ? "Y" : "N")}) with status {(liveSystem ? "SCH" : "SUS")}.
", Logger.ToString());
			ResetLogger();
		}

		[TestDate(2025, 1, 13)]
		public void TestShouldScheduleDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenDisablingLink_ForActiveBMS()
		{
			SetupConfig();
			system.FS_IsLive = true;
			factory.Save();

			CombineAssertions("Disabling one link", () =>
			{
				AssertSchedulesDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenDisablingLink(liveSystem: true, shouldReschedule: false);
			});
			CombineAssertions("Disabling another link: should not duplicate already scheduled actions", () =>
			{
				AssertSchedulesDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenDisablingLink(liveSystem: true, shouldReschedule: true);
			});
		}

		[TestDate(2025, 1, 13)]
		public void TestShouldScheduleDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenDisablingLink_ForInactiveBMS()
		{
			SetupConfig();
			system.FS_IsLive = false;
			factory.Save();

			CombineAssertions("Disabling one link", () =>
			{
				AssertSchedulesDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenDisablingLink(liveSystem: false, shouldReschedule: false);
			});
			CombineAssertions("Disabling another link: should not duplicate already scheduled actions", () =>
			{
				AssertSchedulesDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenDisablingLink(liveSystem: false, shouldReschedule: true);
			});
		}

		void AssertSchedulesDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenDisablingLink(bool liveSystem, bool shouldReschedule)
		{
			var newComponentFromPK = Guid.NewGuid();
			var changeTable = CreateAuditBMComponentLinkTable();
			var row = CreateAuditBMComponentLinkRow(changeTable, componentFromPK: componentFrom.PK.ToGuid(), transferRulesEnabled: true);
			row.AcceptChanges();
			row.SetModified();
			row[BMComponentLinkSchema.Constants.FL_TransferRulesEnabled] = false;
			row[BMComponentLinkSchema.Constants.FL_FC_ComponentFrom] = newComponentFromPK; // we need to check that componentFrom is taken from the original version, not from the current one

			AssertEquals("Precondition: should be modified", DataRowState.Modified, row.RowState);
			Assert("Precondition: should have original version", row.HasVersion(DataRowVersion.Original));
			Assert("Precondition: should have current version", row.HasVersion(DataRowVersion.Current));
			AssertEquals("Precondition: original FL_TransferRulesEnabled", true, row[BMComponentLinkSchema.Constants.FL_TransferRulesEnabled, DataRowVersion.Original]);
			AssertEquals("Precondition: current FL_TransferRulesEnabled", false, row[BMComponentLinkSchema.Constants.FL_TransferRulesEnabled, DataRowVersion.Current]);
			AssertEquals("Precondition: original FL_FC_ComponentFrom", componentFrom.PK.ToGuid(), row[BMComponentLinkSchema.Constants.FL_FC_ComponentFrom, DataRowVersion.Original]);
			AssertEquals("Precondition: current FL_FC_ComponentFrom", newComponentFromPK, row[BMComponentLinkSchema.Constants.FL_FC_ComponentFrom, DataRowVersion.Current]);

			ProcessChanges(changeTable);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var collection = new TimeActionScheduleCollection(newFactory);

			AssertContainsExactElementsInAnyOrder("Should schedule actions for the ComponentFrom", [componentFrom.PK], collection.Select(a => a.TAS_TargetPK));
			var schedule = collection.Single();
			AssertEquals("Should schedule actions targeting BMComponentLinks", BMComponentSchema.Constants.Prefix, schedule.TAS_TargetTableCode);
			AssertEquals("Should schedule actions with proper code", "PHU", schedule.TAS_ActionCode);
			AssertEquals("Should schedule actions with proper parameter", ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer, schedule.TAS_JsonParameter);
			AssertEquals("Should schedule actions as scheduled or suspended", liveSystem ? "SCH" : "SUS", schedule.TAS_ExecutionStatus);
			AssertEquals("Should store system's PK as a token", system.PK.ToString(), schedule.TAS_Token);
			AssertEquals("Should schedule actions ready for execution", ZDateTime.UtcNow, schedule.TAS_ExecutionDateTimeUtc);

			AssertMultilineASCIIEquals($@"The From -> To, Sequence: 0 (PK = {link.PK}) component link was disabled: {(shouldReschedule ? "rescheduled" : "scheduled")} responsive update of dedicated buffers (BUF responsive action) on workflows situated in component From (PK = {componentFrom.PK}) of system Test (IsLive = {(liveSystem ? "Y" : "N")}) with status {(liveSystem ? "SCH" : "SUS")}.
", Logger.ToString());
			ResetLogger();
		}

		[TestDate(2025, 1, 13)]
		public void TestShouldNotScheduleDedicatedBufferUpdate_OnWorkflows_WhenRemovingDisabledLink()
		{
			SetupConfig();
			factory.Save();

			var changeTable = CreateAuditBMComponentLinkTable();
			var row = CreateAuditBMComponentLinkRow(changeTable, componentFromPK: componentFrom.PK, transferRulesEnabled: false);
			row.AcceptChanges();
			row.Delete();

			AssertEquals("Precondition: should be deleted", DataRowState.Deleted, row.RowState);
			Assert("Precondition: should have original version", row.HasVersion(DataRowVersion.Original));
			Assert("Precondition: should not have current version", !row.HasVersion(DataRowVersion.Current));
			AssertEquals("Precondition: original FL_TransferRulesEnabled", false, row[BMComponentLinkSchema.Constants.FL_TransferRulesEnabled, DataRowVersion.Original]);
			AssertEquals("Precondition: original FL_FC_ComponentFrom", componentFrom.PK, row[BMComponentLinkSchema.Constants.FL_FC_ComponentFrom, DataRowVersion.Original]);

			ProcessChanges(changeTable);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var collection = new TimeActionScheduleCollection(newFactory);

			AssertContainsExactElementsInAnyOrder("Should not schedule actions", [], collection.Select(a => a.TAS_TargetPK));
			AssertNullOrEmpty(logger.ToString());
		}

		[TestDate(2025, 1, 13)]
		public void TestShouldScheduleDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenChangingComponentToOnEnabledLink_ForActiveBMS()
		{
			SetupConfig();
			system.FS_IsLive = true;
			factory.Save();

			CombineAssertions("Changing ComponentTo on one link", () =>
			{
				AssertSchedulesDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenWhenChangingComponentToOnEnabledLink(liveSystem: true, shouldReschedule: false);
			});
			CombineAssertions("Changing ComponentTo on another link with the same ComponentFrom: should not duplicate already scheduled actions", () =>
			{
				AssertSchedulesDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenWhenChangingComponentToOnEnabledLink(liveSystem: true, shouldReschedule: true);
			});
		}

		[TestDate(2025, 1, 13)]
		public void TestShouldScheduleDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenChangingComponentToOnEnabledLink_ForInactiveBMS()
		{
			SetupConfig();
			system.FS_IsLive = false;
			factory.Save();

			CombineAssertions("Changing ComponentTo on one link", () =>
			{
				AssertSchedulesDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenWhenChangingComponentToOnEnabledLink(liveSystem: false, shouldReschedule: false);
			});
			CombineAssertions("Changing ComponentTo on another link with the same ComponentFrom: should not duplicate already scheduled actions", () =>
			{
				AssertSchedulesDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenWhenChangingComponentToOnEnabledLink(liveSystem: false, shouldReschedule: true);
			});
		}

		void AssertSchedulesDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenWhenChangingComponentToOnEnabledLink(bool liveSystem, bool shouldReschedule)
		{
			var originalComponentFromPK = Guid.NewGuid();
			var originalComponentToPK = Guid.NewGuid();
			var newComponentToPK = Guid.NewGuid();
			var changeTable = CreateAuditBMComponentLinkTable();
			var row = CreateAuditBMComponentLinkRow(changeTable, componentFromPK: originalComponentFromPK, componentToPK: originalComponentToPK, transferRulesEnabled: true);
			row.AcceptChanges();
			row.SetModified();
			row[BMComponentLinkSchema.Constants.FL_FC_ComponentTo] = newComponentToPK;
			row[BMComponentLinkSchema.Constants.FL_FC_ComponentFrom] = componentFrom.PK.ToGuid(); // we need to check that componentFrom is taken from the current version, not from the original one

			AssertEquals("Precondition: should be modified", DataRowState.Modified, row.RowState);
			Assert("Precondition: should have original version", row.HasVersion(DataRowVersion.Original));
			Assert("Precondition: should have current version", row.HasVersion(DataRowVersion.Current));
			AssertEquals("Precondition: original FL_FC_ComponentFrom", originalComponentToPK, row[BMComponentLinkSchema.Constants.FL_FC_ComponentTo, DataRowVersion.Original]);
			AssertEquals("Precondition: current FL_FC_ComponentFrom", newComponentToPK, row[BMComponentLinkSchema.Constants.FL_FC_ComponentTo, DataRowVersion.Current]);
			AssertEquals("Precondition: original FL_FC_ComponentFrom", originalComponentFromPK, row[BMComponentLinkSchema.Constants.FL_FC_ComponentFrom, DataRowVersion.Original]);
			AssertEquals("Precondition: current FL_FC_ComponentFrom", componentFrom.PK.ToGuid(), row[BMComponentLinkSchema.Constants.FL_FC_ComponentFrom, DataRowVersion.Current]);

			ProcessChanges(changeTable);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var collection = new TimeActionScheduleCollection(newFactory);

			AssertContainsExactElementsInAnyOrder("Should schedule actions for the ComponentFrom", [componentFrom.PK], collection.Select(a => a.TAS_TargetPK));
			var schedule = collection.Single();
			AssertEquals("Should schedule actions targeting BMComponentLinks", BMComponentSchema.Constants.Prefix, schedule.TAS_TargetTableCode);
			AssertEquals("Should schedule actions with proper code", "PHU", schedule.TAS_ActionCode);
			AssertEquals("Should schedule actions with proper parameter", ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer, schedule.TAS_JsonParameter);
			AssertEquals("Should schedule actions as scheduled or suspended", liveSystem ? "SCH" : "SUS", schedule.TAS_ExecutionStatus);
			AssertEquals("Should store system's PK as a token", system.PK.ToString(), schedule.TAS_Token);
			AssertEquals("Should schedule actions ready for execution", ZDateTime.UtcNow, schedule.TAS_ExecutionDateTimeUtc);

			AssertMultilineASCIIEquals($@"The From -> To, Sequence: 0 (PK = {link.PK}) component link changed ComponentTo: {(shouldReschedule ? "rescheduled" : "scheduled")} responsive update of dedicated buffers (BUF responsive action) on workflows situated in component From (PK = {componentFrom.PK}) of system Test (IsLive = {(liveSystem ? "Y" : "N")}) with status {(liveSystem ? "SCH" : "SUS")}.
", Logger.ToString());
			ResetLogger();
		}

		[TestDate(2025, 1, 13)]
		public void TestShouldNotScheduleDedicatedBufferUpdate_OnWorkflows_WhenChangingComponentToOnDisabledLink()
		{
			SetupConfig();
			factory.Save();

			var originalComponentToPK = Guid.NewGuid();
			var newComponentToPK = Guid.NewGuid();
			var changeTable = CreateAuditBMComponentLinkTable();
			var row = CreateAuditBMComponentLinkRow(changeTable, componentFromPK: componentFrom.PK, componentToPK: originalComponentToPK, transferRulesEnabled: false);
			row.AcceptChanges();
			row.SetModified();
			row[BMComponentLinkSchema.Constants.FL_FC_ComponentTo] = newComponentToPK;

			AssertEquals("Precondition: should be modified", DataRowState.Modified, row.RowState);
			Assert("Precondition: should have original version", row.HasVersion(DataRowVersion.Original));
			Assert("Precondition: should have current version", row.HasVersion(DataRowVersion.Current));
			AssertEquals("Precondition: original FL_FC_ComponentFrom", originalComponentToPK, row[BMComponentLinkSchema.Constants.FL_FC_ComponentTo, DataRowVersion.Original]);
			AssertEquals("Precondition: current FL_FC_ComponentFrom", newComponentToPK, row[BMComponentLinkSchema.Constants.FL_FC_ComponentTo, DataRowVersion.Current]);

			ProcessChanges(changeTable);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var collection = new TimeActionScheduleCollection(newFactory);

			AssertContainsExactElementsInAnyOrder("Should not schedule actions", [], collection.Select(a => a.TAS_TargetPK));
			AssertNullOrEmpty(logger.ToString());
		}

		[TestDate(2025, 1, 13)]
		public void TestShouldScheduleDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenEnablingReleaseGateOnLink_ForActiveBMS()
		{
			SetupConfig();
			system.FS_IsLive = true;
			factory.Save();

			CombineAssertions("Enabling release gate on one link", () =>
			{
				AssertSchedulesDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenEnablingReleaseGateOnLink(liveSystem: true, shouldReschedule: false);
			});
			CombineAssertions("Enabling release gate on another link: should not duplicate already scheduled actions", () =>
			{
				AssertSchedulesDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenEnablingReleaseGateOnLink(liveSystem: true, shouldReschedule: true);
			});
		}

		[TestDate(2025, 1, 13)]
		public void TestShouldScheduleDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenEnablingReleaseGateOnLink_ForInactiveBMS()
		{
			SetupConfig();
			system.FS_IsLive = false;
			factory.Save();

			CombineAssertions("Enabling release gate on one link", () =>
			{
				AssertSchedulesDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenEnablingReleaseGateOnLink(liveSystem: false, shouldReschedule: false);
			});
			CombineAssertions("Enabling release gate on another link: should not duplicate already scheduled actions", () =>
			{
				AssertSchedulesDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenEnablingReleaseGateOnLink(liveSystem: false, shouldReschedule: true);
			});
		}

		void AssertSchedulesDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenEnablingReleaseGateOnLink(bool liveSystem, bool shouldReschedule)
		{
			var changeTable = CreateAuditBMComponentLinkTable();
			var row = CreateAuditBMComponentLinkRow(changeTable, componentFromPK: componentFrom.PK, isReleaseGateRuleApplied: false);
			row.AcceptChanges();
			row.SetModified();
			row[BMComponentLinkSchema.Constants.FL_IsReleaseGateRuleApplied] = true;

			AssertEquals("Precondition: should be modified", DataRowState.Modified, row.RowState);
			Assert("Precondition: should have original version", row.HasVersion(DataRowVersion.Original));
			Assert("Precondition: should have current version", row.HasVersion(DataRowVersion.Current));
			AssertEquals("Precondition: original FL_IsReleaseGateRuleApplied", false, row[BMComponentLinkSchema.Constants.FL_IsReleaseGateRuleApplied, DataRowVersion.Original]);
			AssertEquals("Precondition: current FL_IsReleaseGateRuleApplied", true, row[BMComponentLinkSchema.Constants.FL_IsReleaseGateRuleApplied, DataRowVersion.Current]);

			ProcessChanges(changeTable);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var collection = new TimeActionScheduleCollection(newFactory);

			AssertContainsExactElementsInAnyOrder("Should schedule actions for the ComponentFrom", [componentFrom.PK], collection.Select(a => a.TAS_TargetPK));
			var schedule = collection.Single();
			AssertEquals("Should schedule actions targeting BMComponentLinks", BMComponentSchema.Constants.Prefix, schedule.TAS_TargetTableCode);
			AssertEquals("Should schedule actions with proper code", "PHU", schedule.TAS_ActionCode);
			AssertEquals("Should schedule actions with proper parameter", ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer, schedule.TAS_JsonParameter);
			AssertEquals("Should schedule actions as scheduled or suspended", liveSystem ? "SCH" : "SUS", schedule.TAS_ExecutionStatus);
			AssertEquals("Should store system's PK as a token", system.PK.ToString(), schedule.TAS_Token);
			AssertEquals("Should schedule actions ready for execution", ZDateTime.UtcNow, schedule.TAS_ExecutionDateTimeUtc);

			AssertMultilineASCIIEquals($@"Release gate on the From -> To, Sequence: 0 (PK = {link.PK}) component link was enabled: {(shouldReschedule ? "rescheduled" : "scheduled")} responsive update of dedicated buffers (BUF responsive action) on workflows situated in component From (PK = {componentFrom.PK}) of system Test (IsLive = {(liveSystem ? "Y" : "N")}) with status {(liveSystem ? "SCH" : "SUS")}.
", Logger.ToString());
			ResetLogger();
		}

		[TestDate(2025, 1, 13)]
		public void TestShouldNotScheduleDedicatedBufferUpdate_OnWorkflows_WhenEnablingReleaseGateOnDisabledLink()
		{
			SetupConfig();
			factory.Save();

			var changeTable = CreateAuditBMComponentLinkTable();
			var row = CreateAuditBMComponentLinkRow(changeTable, componentFromPK: componentFrom.PK, isReleaseGateRuleApplied: false, transferRulesEnabled: false);
			row.AcceptChanges();
			row.SetModified();
			row[BMComponentLinkSchema.Constants.FL_IsReleaseGateRuleApplied] = true;

			AssertEquals("Precondition: should be modified", DataRowState.Modified, row.RowState);
			Assert("Precondition: should have original version", row.HasVersion(DataRowVersion.Original));
			Assert("Precondition: should have current version", row.HasVersion(DataRowVersion.Current));
			AssertEquals("Precondition: original FL_IsReleaseGateRuleApplied", false, row[BMComponentLinkSchema.Constants.FL_IsReleaseGateRuleApplied, DataRowVersion.Original]);
			AssertEquals("Precondition: current FL_IsReleaseGateRuleApplied", true, row[BMComponentLinkSchema.Constants.FL_IsReleaseGateRuleApplied, DataRowVersion.Current]);

			ProcessChanges(changeTable);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var collection = new TimeActionScheduleCollection(newFactory);

			AssertContainsExactElementsInAnyOrder("Should not schedule actions", [], collection.Select(a => a.TAS_TargetPK));
			AssertNullOrEmpty(logger.ToString());
		}

		[TestDate(2025, 1, 13)]
		public void TestShouldScheduleDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenDisablingReleaseGateOnLink_ForActiveBMS()
		{
			SetupConfig();
			system.FS_IsLive = true;
			factory.Save();

			CombineAssertions("Disabling release gate on one link", () =>
			{
				AssertSchedulesDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenDisablingReleaseGateOnLink(liveSystem: true, shouldReschedule: false);
			});
			CombineAssertions("Disabling release gate on another link: should not duplicate already scheduled actions", () =>
			{
				AssertSchedulesDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenDisablingReleaseGateOnLink(liveSystem: true, shouldReschedule: true);
			});
		}

		[TestDate(2025, 1, 13)]
		public void TestShouldScheduleDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenDisablingReleaseGateOnLink_ForInactiveBMS()
		{
			SetupConfig();
			system.FS_IsLive = false;
			factory.Save();

			CombineAssertions("Disabling release gate on one link", () =>
			{
				AssertSchedulesDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenDisablingReleaseGateOnLink(liveSystem: false, shouldReschedule: false);
			});
			CombineAssertions("Disabling release gate on another link: should not duplicate already scheduled actions", () =>
			{
				AssertSchedulesDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenDisablingReleaseGateOnLink(liveSystem: false, shouldReschedule: true);
			});
		}

		void AssertSchedulesDedicatedBufferUpdate_OnWorkflowsInComponentFrom_WhenDisablingReleaseGateOnLink(bool liveSystem, bool shouldReschedule)
		{
			var changeTable = CreateAuditBMComponentLinkTable();
			var row = CreateAuditBMComponentLinkRow(changeTable, componentFromPK: componentFrom.PK, isReleaseGateRuleApplied: true);
			row.AcceptChanges();
			row.SetModified();
			row[BMComponentLinkSchema.Constants.FL_IsReleaseGateRuleApplied] = false;

			AssertEquals("Precondition: should be modified", DataRowState.Modified, row.RowState);
			Assert("Precondition: should have original version", row.HasVersion(DataRowVersion.Original));
			Assert("Precondition: should have current version", row.HasVersion(DataRowVersion.Current));
			AssertEquals("Precondition: original FL_IsReleaseGateRuleApplied", true, row[BMComponentLinkSchema.Constants.FL_IsReleaseGateRuleApplied, DataRowVersion.Original]);
			AssertEquals("Precondition: current FL_IsReleaseGateRuleApplied", false, row[BMComponentLinkSchema.Constants.FL_IsReleaseGateRuleApplied, DataRowVersion.Current]);

			ProcessChanges(changeTable);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var collection = new TimeActionScheduleCollection(newFactory);

			AssertContainsExactElementsInAnyOrder("Should schedule actions for the ComponentFrom", [componentFrom.PK], collection.Select(a => a.TAS_TargetPK));
			var schedule = collection.Single();
			AssertEquals("Should schedule actions targeting BMComponentLinks", BMComponentSchema.Constants.Prefix, schedule.TAS_TargetTableCode);
			AssertEquals("Should schedule actions with proper code", "PHU", schedule.TAS_ActionCode);
			AssertEquals("Should schedule actions with proper parameter", ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer, schedule.TAS_JsonParameter);
			AssertEquals("Should schedule actions as scheduled or suspended", liveSystem ? "SCH" : "SUS", schedule.TAS_ExecutionStatus);
			AssertEquals("Should store system's PK as a token", system.PK.ToString(), schedule.TAS_Token);
			AssertEquals("Should schedule actions ready for execution", ZDateTime.UtcNow, schedule.TAS_ExecutionDateTimeUtc);

			AssertMultilineASCIIEquals($@"Release gate on the From -> To, Sequence: 0 (PK = {link.PK}) component link was disabled: {(shouldReschedule ? "rescheduled" : "scheduled")} responsive update of dedicated buffers (BUF responsive action) on workflows situated in component From (PK = {componentFrom.PK}) of system Test (IsLive = {(liveSystem ? "Y" : "N")}) with status {(liveSystem ? "SCH" : "SUS")}.
", Logger.ToString());
			ResetLogger();
		}

		[TestDate(2025, 1, 13)]
		public void TestShouldNotScheduleDedicatedBufferUpdate_OnWorkflows_WhenDisablingReleaseGateOnDisabledLink()
		{
			SetupConfig();
			factory.Save();

			var changeTable = CreateAuditBMComponentLinkTable();
			var row = CreateAuditBMComponentLinkRow(changeTable, componentFromPK: componentFrom.PK, isReleaseGateRuleApplied: true, transferRulesEnabled: false);
			row.AcceptChanges();
			row.SetModified();
			row[BMComponentLinkSchema.Constants.FL_IsReleaseGateRuleApplied] = false;

			AssertEquals("Precondition: should be modified", DataRowState.Modified, row.RowState);
			Assert("Precondition: should have original version", row.HasVersion(DataRowVersion.Original));
			Assert("Precondition: should have current version", row.HasVersion(DataRowVersion.Current));
			AssertEquals("Precondition: original FL_IsReleaseGateRuleApplied", true, row[BMComponentLinkSchema.Constants.FL_IsReleaseGateRuleApplied, DataRowVersion.Original]);
			AssertEquals("Precondition: current FL_IsReleaseGateRuleApplied", false, row[BMComponentLinkSchema.Constants.FL_IsReleaseGateRuleApplied, DataRowVersion.Current]);

			ProcessChanges(changeTable);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var collection = new TimeActionScheduleCollection(newFactory);

			AssertContainsExactElementsInAnyOrder("Should not schedule actions", [], collection.Select(a => a.TAS_TargetPK));
			AssertNullOrEmpty(logger.ToString());
		}

		[TestDate(2025, 1, 13)]
		public void TestShouldNotScheduleDedicatedBufferUpdate_OnWorkflows_WhenChangingLinkName()
		{
			SetupConfig();
			factory.Save();

			var originalComponentFromPK = Guid.NewGuid();
			var changeTable = CreateAuditBMComponentLinkTable();
			var row = CreateAuditBMComponentLinkRow(changeTable, componentFromPK: componentFrom.PK, transferRulesEnabled: true, name: "Old name");
			row.AcceptChanges();
			row.SetModified();
			row[BMComponentLinkSchema.Constants.FL_Name] = "New name";

			AssertEquals("Precondition: should be modified", DataRowState.Modified, row.RowState);
			Assert("Precondition: should have original version", row.HasVersion(DataRowVersion.Original));
			Assert("Precondition: should have current version", row.HasVersion(DataRowVersion.Current));
			AssertEquals("Precondition: original FL_Name", "Old name", row[BMComponentLinkSchema.Constants.FL_Name, DataRowVersion.Original]);
			AssertEquals("Precondition: current FL_Name", "New name", row[BMComponentLinkSchema.Constants.FL_Name, DataRowVersion.Current]);

			ProcessChanges(changeTable);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var collection = new TimeActionScheduleCollection(newFactory);

			AssertContainsExactElementsInAnyOrder("Should not schedule actions", [], collection.Select(a => a.TAS_TargetPK));
			AssertNullOrEmpty(logger.ToString());
		}

		[TestDate(2025, 1, 13)]
		public void TestDBHits()
		{
			SetupConfig();
			system.FS_IsLive = true;
			factory.Save();

			var linkCount = 15;

			var changeTable = CreateAuditBMComponentLinkTable();

			for (int i = 0; i < linkCount; i++)
			{
				CreateAuditBMComponentLinkRow(changeTable, componentFromPK: componentFrom.PK, transferRulesEnabled: true);
			}

			var expectedHits = new Dictionary<string, int>
			{
				{ BMComponentLinkSchema.Constants.TableName, 1 },
				{ BMComponentSchema.Constants.TableName, 1 }, // to load ComponentFrom and ComponentTo to display the link name in logs
				{ BMSystemSchema.Constants.TableName, 1 }, // for identifying the system PK to use it as a token for schedules 
				{ TimeActionScheduleSchema.Constants.TableName, 1 },
			};

			using (TestCaseWithFactory.AssertDbHitsForAllFactories(expectedHits, useOnlyNewFactories: true, ignoreHitsFromTablesCachedInUberFactory: true))
			{
				ProcessChanges(changeTable);
			}

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var collection = new TimeActionScheduleCollection(newFactory);
			AssertContainsExactElementsInAnyOrder("Should schedule actions for the ComponentFrom", [componentFrom.PK], collection.Select(a => a.TAS_TargetPK));
		}

		#region Implementation

		ILogger logger;

		protected override ILogger Logger => logger;

		void ResetLogger()
		{
			logger = new SimpleLogger();
		}

		DataTable CreateAuditBMComponentLinkTable(int numEntries = 0)
		{
			var changeTable = new DataTable();

			changeTable.Columns.Add(BMComponentLinkSchema.Constants.PK, typeof(Guid));
			changeTable.Columns.Add(BMComponentLinkSchema.Constants.FL_FC_ComponentFrom, typeof(Guid));
			changeTable.Columns.Add(BMComponentLinkSchema.Constants.FL_FC_ComponentTo, typeof(Guid));
			changeTable.Columns.Add(BMComponentLinkSchema.Constants.FL_TransferRulesEnabled, typeof(bool));
			changeTable.Columns.Add(BMComponentLinkSchema.Constants.FL_IsReleaseGateRuleApplied, typeof(bool));
			changeTable.Columns.Add(BMComponentLinkSchema.Constants.FL_Name, typeof(string));
			changeTable.Columns.Add("TranEndTimeUtc", typeof(DateTime));

			for (var i = 0; i < numEntries; i++)
			{
				var row = CreateAuditBMComponentLinkRow(changeTable, componentFromPK: Guid.NewGuid(), transferRulesEnabled: true);
				row.AcceptChanges();
				row.SetModified();
			}

			return changeTable;
		}

		DataRow CreateAuditBMComponentLinkRow(DataTable dataTable, ZGuid componentFromPK, ZGuid? componentToPK = null, bool transferRulesEnabled = true, bool isReleaseGateRuleApplied = true, string name = null, DateTime? tranEndTimeUtc = null)
		{
			var row = dataTable.NewRow();

			row["TranEndTimeUtc"] = tranEndTimeUtc ?? ZDateTime.UtcNow.ToDateTime();

			row[BMComponentLinkSchema.Constants.PK] = link != null ? link.PK.ToGuid() : Guid.NewGuid();
			row[BMComponentLinkSchema.Constants.FL_FC_ComponentFrom] = componentFromPK.ToGuid();
			row[BMComponentLinkSchema.Constants.FL_FC_ComponentTo] = componentToPK.HasValue ? componentToPK.Value.ToGuid() : Guid.NewGuid();
			row[BMComponentLinkSchema.Constants.FL_TransferRulesEnabled] = transferRulesEnabled;
			row[BMComponentLinkSchema.Constants.FL_IsReleaseGateRuleApplied] = isReleaseGateRuleApplied;
			row[BMComponentLinkSchema.Constants.FL_Name] = name;

			dataTable.Rows.Add(row);

			AssertEquals("Precondition: should be added", DataRowState.Added, row.RowState);
			Assert("Precondition: should have no original version", !row.HasVersion(DataRowVersion.Original));
			Assert("Precondition: should have current version", row.HasVersion(DataRowVersion.Current));

			return row;
		}

		protected override DataTable GetTestDataTable() => CreateAuditBMComponentLinkTable();

		BusinessObjectFactory factory;
		IBMTestHelper helper;
		IBMSystem system;
		IBMComponent componentFrom;
		IBMComponent componentTo;
		IBMComponentLink link;

		void SetupConfig()
		{
			factory = new BusinessObjectFactory();
			helper = ObjectFactory.Get<IBMTestHelper>();
			system = helper.CreateSystem(factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test";
			componentFrom = helper.CreateBucket(system, "From");
			componentTo = helper.CreateBucket(system, "To");
			link = helper.LinkComponents(componentFrom, componentTo);
		}

		#endregion

		#region Setup And TearDown

		protected override void SetUp()
		{
			base.SetUp();

			BMSRegistry.BufferManagementEnabled = true;
			BMSRegistry.EnableResponsivePAVEDataProcessing = true;
			BMSRegistry.EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges = true;

			ResetLogger();
		}

		#endregion
	}
}
