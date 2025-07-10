using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
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
	[TestedType(typeof(BMComponentResponsiveWorkflowUpdateSubscriber))]
	class BMComponentResponsiveWorkflowUpdateSubscriberTest : PAVESubscriberBaseTest
	{
		protected override string ExpectedCode => "CPW";

		protected override ITableSchema ExpectedTable => BMComponentSchema.Instance;

		protected override IEnumerable<SchemaColumn> ExpectedSpecificColumns => [BMComponentSchema.FC_IsActive, BMComponentSchema.FC_Type, BMComponentSchema.FC_GB_AgingBranch, BMComponentSchema.FC_GE_AgingDepartment];

		public override void TestCustomFilter()
		{
			var subscriber = new BMComponentResponsiveWorkflowUpdateSubscriber();
			AssertNull(subscriber.CustomFilter);
		}

		public override void TestIsEnabled()
		{
			void AssertDisabled()
			{
				var changeTable = CreateAuditBMComponentTable(5);
				ProcessChanges(changeTable);

				AssertMultilineASCIIEquals(@"5 changes were skipped as the CPW subscriber is disabled.
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
			var changeTable = CreateAuditBMComponentTable(5);
			ProcessChanges(changeTable);
			AssertNotContains("changes were skipped", Logger.ToString());

			// other flags should not affect
			BMSRegistry.EnablePaveExperimentalFeatures = false;
			BMSRegistry.AutoAssignCapabilityTasksOnChanges = false;
			changeTable = CreateAuditBMComponentTable(5);
			ProcessChanges(changeTable);
			AssertNotContains("changes were skipped", Logger.ToString());

			ErrorReporter.Clear();
		}

		#region Dedicated Buffer Update On Component Enabling / Disabling

		[TestDate(2025, 1, 30)]
		public void TestShouldScheduleDedicatedBufferUpdate_WhenEnablingComponent_ForActiveBMS()
		{
			AssertSchedulesDedicatedBufferUpdate_WhenEnablingComponent(liveSystem: true);
		}

		[TestDate(2025, 1, 30)]
		public void TestShouldScheduleDedicatedBufferUpdate_WhenEnablingComponent_ForInactiveBMS()
		{
			AssertSchedulesDedicatedBufferUpdate_WhenEnablingComponent(liveSystem: false);
		}

		void AssertSchedulesDedicatedBufferUpdate_WhenEnablingComponent(bool liveSystem)
		{
			SetupConfig();
			system.FS_IsLive = liveSystem;
			factory.Save();

			var changeTable = CreateAuditBMComponentTable();
			var row = CreateAuditBMComponentRow(changeTable, componentPK: componentTo1.PK, componentType: "BUC", isActive: false);
			row.AcceptChanges();
			row.SetModified();
			row[BMComponentSchema.Constants.FC_IsActive] = true;

			AssertEquals("Precondition: should be modified", DataRowState.Modified, row.RowState);
			Assert("Precondition: should have original version", row.HasVersion(DataRowVersion.Original));
			Assert("Precondition: should have current version", row.HasVersion(DataRowVersion.Current));
			AssertEquals("Precondition: original FC_IsActive", false, row[BMComponentSchema.Constants.FC_IsActive, DataRowVersion.Original]);
			AssertEquals("Precondition: current FC_IsActive", true, row[BMComponentSchema.Constants.FC_IsActive, DataRowVersion.Current]);

			ProcessChanges(changeTable);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var collection = new TimeActionScheduleCollection(newFactory);

			var scheduledPKs = collection.Select(a => a.TAS_TargetPK).ToArray();
			Assert("Should schedule actions for the modified component", scheduledPKs.Contains(componentTo1.PK));
			Assert("Should not schedule actions for inactive preceding components because inactive components either 1) were already inactive when the target To component was modified and therefore do not require dedicated buffer update or 2) were made deactivated later - in this case making a component deactivated should've itself scheduled an action for that component - no need to duplicate the action", !scheduledPKs.Contains(componentFrom1dInactive.PK));
			Assert("Should schedule actions for all active components preceding via active links", new[] { componentFrom1a.PK, componentFrom1b.PK }.All(c => scheduledPKs.Contains(c)));
			Assert("Should not schedule actions for components preceding via inactive links because that inactive links either 1) were already inactive when the target To component was modified and therefore do not require dedicated buffer update or 2) were made deactivated later - in this case making a link deactivated should've itself scheduled an action for that component From - no need to duplicate the action", !scheduledPKs.Contains(componentFrom1c.PK));
			AssertContainsExactElementsInAnyOrder("All scheduled actions", [componentTo1.PK, componentFrom1a.PK, componentFrom1b.PK], scheduledPKs);

			Assert("Should schedule actions targeting BMComponent", collection.All(s => s.TAS_TargetTableCode == BMComponentSchema.Constants.Prefix));
			Assert("Should schedule actions with proper code", collection.All(s => s.TAS_ActionCode == "PHU"));
			Assert("Should schedule actions with proper parameter", collection.All(s => s.TAS_JsonParameter == ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer));
			Assert("Should schedule actions as active or suspended", collection.All(s => s.TAS_ExecutionStatus == (liveSystem ? "SCH" : "SUS")));
			Assert("Should store system's PK as a token", collection.All(s => s.TAS_Token == system.PK.ToString()));
			Assert("Should schedule actions ready for execution", collection.All(s => s.TAS_ExecutionDateTimeUtc == ZDateTime.UtcNow));

			AssertMultilineASCIIEquals($@"Component To 1 (PK = {componentTo1.PK}) changed its activity status (activated): scheduled responsive update of dedicated buffers (BUF responsive action) on workflows situated in component To 1 (PK = {componentTo1.PK}) of system Test (IsLive = {(liveSystem ? "Y" : "N")}) with status {(liveSystem ? "SCH" : "SUS")}.
An active component preceding to the modified component via active links: scheduled responsive update of dedicated buffers (BUF responsive action) on workflows situated in component From 1a (PK = {componentFrom1a.PK}) of system Test (IsLive = {(liveSystem ? "Y" : "N")}) with status {(liveSystem ? "SCH" : "SUS")}.
An active component preceding to the modified component via active links: scheduled responsive update of dedicated buffers (BUF responsive action) on workflows situated in component From 1b (PK = {componentFrom1b.PK}) of system Test (IsLive = {(liveSystem ? "Y" : "N")}) with status {(liveSystem ? "SCH" : "SUS")}.
", Logger.ToString());
		}

		[TestDate(2025, 1, 30)]
		public void TestShouldScheduleDedicatedBufferUpdate_WhenDisablingComponent_ForActiveBMS()
		{
			AssertSchedulesDedicatedBufferUpdate_WhenDisablingComponent(liveSystem: true);
		}

		[TestDate(2025, 1, 30)]
		public void TestShouldScheduleDedicatedBufferUpdate_WhenDisablingComponent_ForInactiveBMS()
		{
			AssertSchedulesDedicatedBufferUpdate_WhenDisablingComponent(liveSystem: false);
		}

		void AssertSchedulesDedicatedBufferUpdate_WhenDisablingComponent(bool liveSystem)
		{
			SetupConfig();
			system.FS_IsLive = liveSystem;
			factory.Save();

			var changeTable = CreateAuditBMComponentTable();
			var row = CreateAuditBMComponentRow(changeTable, componentPK: componentTo1.PK, componentType: "BUC", isActive: true);
			row.AcceptChanges();
			row.SetModified();
			row[BMComponentSchema.Constants.FC_IsActive] = false;

			AssertEquals("Precondition: should be modified", DataRowState.Modified, row.RowState);
			Assert("Precondition: should have original version", row.HasVersion(DataRowVersion.Original));
			Assert("Precondition: should have current version", row.HasVersion(DataRowVersion.Current));
			AssertEquals("Precondition: original FC_IsActive", true, row[BMComponentSchema.Constants.FC_IsActive, DataRowVersion.Original]);
			AssertEquals("Precondition: current FC_IsActive", false, row[BMComponentSchema.Constants.FC_IsActive, DataRowVersion.Current]);

			ProcessChanges(changeTable);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var collection = new TimeActionScheduleCollection(newFactory);

			var scheduledPKs = collection.Select(a => a.TAS_TargetPK).ToArray();
			Assert("Should schedule actions for the modified component", scheduledPKs.Contains(componentTo1.PK));
			Assert("Should not schedule actions for inactive preceding components because inactive components either 1) were already inactive when the target To component was modified and therefore do not require dedicated buffer update or 2) were made deactivated later - in this case making a component deactivated should've itself scheduled an action for that component - no need to duplicate the action", !scheduledPKs.Contains(componentFrom1dInactive.PK));
			Assert("Should schedule actions for all active components preceding via active links", new[] { componentFrom1a.PK, componentFrom1b.PK }.All(c => scheduledPKs.Contains(c)));
			Assert("Should not schedule actions for components preceding via inactive links because that inactive links either 1) were already inactive when the target To component was modified and therefore do not require dedicated buffer update or 2) were made deactivated later - in this case making a link deactivated should've itself scheduled an action for that component From - no need to duplicate the action", !scheduledPKs.Contains(componentFrom1c.PK));
			AssertContainsExactElementsInAnyOrder("All scheduled actions", [componentTo1.PK, componentFrom1a.PK, componentFrom1b.PK], scheduledPKs);

			Assert("Should schedule actions targeting BMComponent", collection.All(s => s.TAS_TargetTableCode == BMComponentSchema.Constants.Prefix));
			Assert("Should schedule actions with proper code", collection.All(s => s.TAS_ActionCode == "PHU"));
			Assert("Should schedule actions with proper parameter", collection.All(s => s.TAS_JsonParameter == ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer));
			Assert("Should schedule actions as active or suspended", collection.All(s => s.TAS_ExecutionStatus == (liveSystem ? "SCH" : "SUS")));
			Assert("Should store system's PK as a token", collection.All(s => s.TAS_Token == system.PK.ToString()));
			Assert("Should schedule actions ready for execution", collection.All(s => s.TAS_ExecutionDateTimeUtc == ZDateTime.UtcNow));

			AssertMultilineASCIIEquals($@"Component To 1 (PK = {componentTo1.PK}) changed its activity status (deactivated): scheduled responsive update of dedicated buffers (BUF responsive action) on workflows situated in component To 1 (PK = {componentTo1.PK}) of system Test (IsLive = {(liveSystem ? "Y" : "N")}) with status {(liveSystem ? "SCH" : "SUS")}.
An active component preceding to the modified component via active links: scheduled responsive update of dedicated buffers (BUF responsive action) on workflows situated in component From 1a (PK = {componentFrom1a.PK}) of system Test (IsLive = {(liveSystem ? "Y" : "N")}) with status {(liveSystem ? "SCH" : "SUS")}.
An active component preceding to the modified component via active links: scheduled responsive update of dedicated buffers (BUF responsive action) on workflows situated in component From 1b (PK = {componentFrom1b.PK}) of system Test (IsLive = {(liveSystem ? "Y" : "N")}) with status {(liveSystem ? "SCH" : "SUS")}.
", Logger.ToString());
		}

		[TestDate(2025, 1, 30)]
		public void TestShouldNotSchedulesDedicatedBufferUpdate_WhenComponentDoesNotExist()
		{
			SetupConfig();
			factory.Save();

			var nonExistentComponentPK = Guid.NewGuid();
			var changeTable = CreateAuditBMComponentTable();
			var row = CreateAuditBMComponentRow(changeTable, componentPK: nonExistentComponentPK, componentType: "BUC", isActive: false);
			row.AcceptChanges();
			row.SetModified();
			row[BMComponentSchema.Constants.FC_IsActive] = true;

			AssertEquals("Precondition: should be modified", DataRowState.Modified, row.RowState);
			Assert("Precondition: should have original version", row.HasVersion(DataRowVersion.Original));
			Assert("Precondition: should have current version", row.HasVersion(DataRowVersion.Current));
			AssertEquals("Precondition: original FC_IsActive", false, row[BMComponentSchema.Constants.FC_IsActive, DataRowVersion.Original]);
			AssertEquals("Precondition: current FC_IsActive", true, row[BMComponentSchema.Constants.FC_IsActive, DataRowVersion.Current]);

			ProcessChanges(changeTable);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var collection = new TimeActionScheduleCollection(newFactory);

			AssertContainsExactElementsInAnyOrder("Should not schedule actions when component is deleted", [], collection.Select(a => a.TAS_TargetPK));

			AssertMultilineASCIIEquals($@"Component changed its activity status (activated): no need to schedule responsive update of workflows as component with PK = {nonExistentComponentPK} does not exist.
", Logger.ToString());
		}

		#endregion

		#region Effective Branch / Department Update On Aging Branch / Department Change

		[TestDate(2025, 2, 18)]
		public void TestShouldScheduleEffectiveBranchDepartmentUpdate_WhenChangingAgingBranchOnBuffer_ForActiveBMS()
		{
			AssertSchedulesEffectiveBranchDepartmentUpdate_WhenChangingAgingBranchOnBuffer(liveSystem: true);
		}

		[TestDate(2025, 2, 18)]
		public void TestShouldScheduleEffectiveBranchDepartmentUpdate_WhenChangingAgingBranchOnBuffer_ForInactiveBMS()
		{
			AssertSchedulesEffectiveBranchDepartmentUpdate_WhenChangingAgingBranchOnBuffer(liveSystem: false);
		}

		void AssertSchedulesEffectiveBranchDepartmentUpdate_WhenChangingAgingBranchOnBuffer(bool liveSystem)
		{
			SetupConfig();
			system.FS_IsLive = liveSystem;
			factory.Save();

			var oldBranchPK = Guid.NewGuid();
			var newBranchPK = Guid.NewGuid();
			var changeTable = CreateAuditBMComponentTable();
			var row = CreateAuditBMComponentRow(changeTable, componentPK: componentTo1.PK, componentType: "BUF", agingBranchPK: oldBranchPK);
			row.AcceptChanges();
			row.SetModified();
			row[BMComponentSchema.Constants.FC_GB_AgingBranch] = newBranchPK;

			AssertEquals("Precondition: should be modified", DataRowState.Modified, row.RowState);
			Assert("Precondition: should have original version", row.HasVersion(DataRowVersion.Original));
			Assert("Precondition: should have current version", row.HasVersion(DataRowVersion.Current));
			AssertEquals("Precondition: original FC_GB_AgingBranch", oldBranchPK, row[BMComponentSchema.Constants.FC_GB_AgingBranch, DataRowVersion.Original]);
			AssertEquals("Precondition: current FC_GB_AgingBranch", newBranchPK, row[BMComponentSchema.Constants.FC_GB_AgingBranch, DataRowVersion.Current]);

			ProcessChanges(changeTable);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var collection = new TimeActionScheduleCollection(newFactory);

			var scheduledPKs = collection.Select(a => a.TAS_TargetPK).ToArray();
			Assert("Should schedule actions for the modified component", scheduledPKs.Contains(componentTo1.PK));
			Assert("Should not schedule actions for inactive preceding components because inactive components either 1) were already inactive when the component was modified and therefore this component cannot have dedicated buffers set and effective branch/department cannot change as the result of this modification or 2) were made deactivated later - in this case making a component deactivated should've itself scheduled a dedicated buffer update (and possible effective branch and department change as the result) for that component - no need to duplicate the action", !scheduledPKs.Contains(componentFrom1dInactive.PK));
			Assert("Should schedule actions for all active components preceding via active links", new[] { componentFrom1a.PK, componentFrom1b.PK }.All(c => scheduledPKs.Contains(c)));
			Assert("Should not schedule actions for components preceding via inactive links because that inactive links either 1) were already inactive when the component was modified and therefore these links do not contribute to the dedicated buffer and effective branch/department calculations or 2) were made deactivated later - in this case making a link deactivated should've itself scheduled a dedicated buffer update (and possible effective branch and department change as the result) for that component From - no need to duplicate the action", !scheduledPKs.Contains(componentFrom1c.PK));
			AssertContainsExactElementsInAnyOrder("All scheduled actions", [componentTo1.PK, componentFrom1a.PK, componentFrom1b.PK], scheduledPKs);

			Assert("Should schedule actions targeting BMComponent", collection.All(s => s.TAS_TargetTableCode == BMComponentSchema.Constants.Prefix));
			Assert("Should schedule actions with proper code", collection.All(s => s.TAS_ActionCode == "PHU"));
			Assert("Should schedule actions with proper parameter", collection.All(s => s.TAS_JsonParameter == ProcessHeaderResponsiveActionConstants.UpdateEffectiveBranchAndDepartment));
			Assert("Should schedule actions as active or suspended", collection.All(s => s.TAS_ExecutionStatus == (liveSystem ? "SCH" : "SUS")));
			Assert("Should store system's PK as a token", collection.All(s => s.TAS_Token == system.PK.ToString()));
			Assert("Should schedule actions ready for execution", collection.All(s => s.TAS_ExecutionDateTimeUtc == ZDateTime.UtcNow));

			AssertMultilineASCIIEquals($@"Component To 1 (PK = {componentTo1.PK}) changed its aging branch: scheduled responsive update of effective branches and departments (EBD responsive action) on workflows situated in component To 1 (PK = {componentTo1.PK}) of system Test (IsLive = {(liveSystem ? "Y" : "N")}) with status {(liveSystem ? "SCH" : "SUS")}.
An active component preceding to the modified component via active links: scheduled responsive update of effective branches and departments (EBD responsive action) on workflows situated in component From 1a (PK = {componentFrom1a.PK}) of system Test (IsLive = {(liveSystem ? "Y" : "N")}) with status {(liveSystem ? "SCH" : "SUS")}.
An active component preceding to the modified component via active links: scheduled responsive update of effective branches and departments (EBD responsive action) on workflows situated in component From 1b (PK = {componentFrom1b.PK}) of system Test (IsLive = {(liveSystem ? "Y" : "N")}) with status {(liveSystem ? "SCH" : "SUS")}.
", Logger.ToString());
		}

		[TestDate(2025, 2, 18)]
		public void TestShouldNotScheduleEffectiveBranchDepartmentUpdate_WhenChangingAgingBranchOnBucket()
		{
			SetupConfig();
			factory.Save();

			var oldBranchPK = Guid.NewGuid();
			var newBranchPK = Guid.NewGuid();
			var changeTable = CreateAuditBMComponentTable();
			var row = CreateAuditBMComponentRow(changeTable, componentPK: componentTo1.PK, componentType: "BUC", agingBranchPK: oldBranchPK);
			row.AcceptChanges();
			row.SetModified();
			row[BMComponentSchema.Constants.FC_GB_AgingBranch] = newBranchPK;

			AssertEquals("Precondition: should be modified", DataRowState.Modified, row.RowState);
			Assert("Precondition: should have original version", row.HasVersion(DataRowVersion.Original));
			Assert("Precondition: should have current version", row.HasVersion(DataRowVersion.Current));
			AssertEquals("Precondition: original FC_GB_AgingBranch", oldBranchPK, row[BMComponentSchema.Constants.FC_GB_AgingBranch, DataRowVersion.Original]);
			AssertEquals("Precondition: current FC_GB_AgingBranch", newBranchPK, row[BMComponentSchema.Constants.FC_GB_AgingBranch, DataRowVersion.Current]);

			ProcessChanges(changeTable);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var collection = new TimeActionScheduleCollection(newFactory);

			AssertContainsExactElementsInAnyOrder("Should not schedule actions when component is a bucket", [], collection.Select(a => a.TAS_TargetPK));

			AssertMultilineASCIIEquals($@"Component To 1 (PK = {componentTo1.PK}) changed its aging branch: no need to schedule responsive update of effective branch/department on workflows as the component is not a buffer.
", Logger.ToString());
		}

		[TestDate(2025, 2, 18)]
		public void TestShouldScheduleEffectiveBranchDepartmentUpdate_WhenChangingAgingDepartmentOnBuffer_ForActiveBMS()
		{
			AssertSchedulesEffectiveBranchDepartmentUpdate_WhenChangingAgingDepartmentOnBuffer(liveSystem: true);
		}

		[TestDate(2025, 2, 18)]
		public void TestShouldScheduleEffectiveBranchDepartmentUpdate_WhenChangingAgingDepartmentOnBuffer_ForInactiveBMS()
		{
			AssertSchedulesEffectiveBranchDepartmentUpdate_WhenChangingAgingDepartmentOnBuffer(liveSystem: false);
		}

		void AssertSchedulesEffectiveBranchDepartmentUpdate_WhenChangingAgingDepartmentOnBuffer(bool liveSystem)
		{
			SetupConfig();
			system.FS_IsLive = liveSystem;
			factory.Save();

			var oldDepartmentPK = Guid.NewGuid();
			var newDepartmentPK = Guid.NewGuid();
			var changeTable = CreateAuditBMComponentTable();
			var row = CreateAuditBMComponentRow(changeTable, componentPK: componentTo1.PK, componentType: "BUF", agingDepartmentPK: oldDepartmentPK);
			row.AcceptChanges();
			row.SetModified();
			row[BMComponentSchema.Constants.FC_GE_AgingDepartment] = newDepartmentPK;

			AssertEquals("Precondition: should be modified", DataRowState.Modified, row.RowState);
			Assert("Precondition: should have original version", row.HasVersion(DataRowVersion.Original));
			Assert("Precondition: should have current version", row.HasVersion(DataRowVersion.Current));
			AssertEquals("Precondition: original FC_GE_AgingDepartment", oldDepartmentPK, row[BMComponentSchema.Constants.FC_GE_AgingDepartment, DataRowVersion.Original]);
			AssertEquals("Precondition: current FC_GE_AgingDepartment", newDepartmentPK, row[BMComponentSchema.Constants.FC_GE_AgingDepartment, DataRowVersion.Current]);

			ProcessChanges(changeTable);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var collection = new TimeActionScheduleCollection(newFactory);

			var scheduledPKs = collection.Select(a => a.TAS_TargetPK).ToArray();
			Assert("Should schedule actions for the modified component", scheduledPKs.Contains(componentTo1.PK));
			Assert("Should not schedule actions for inactive preceding components because inactive components either 1) were already inactive when the component was modified and therefore this component cannot have dedicated buffers set and effective branch/department cannot change as the result of this modification or 2) were made deactivated later - in this case making a component deactivated should've itself scheduled a dedicated buffer update (and possible effective branch and department change as the result) for that component - no need to duplicate the action", !scheduledPKs.Contains(componentFrom1dInactive.PK));
			Assert("Should schedule actions for all active components preceding via active links", new[] { componentFrom1a.PK, componentFrom1b.PK }.All(c => scheduledPKs.Contains(c)));
			Assert("Should not schedule actions for components preceding via inactive links because that inactive links either 1) were already inactive when the component was modified and therefore these links do not contribute to the dedicated buffer and effective branch/department calculations or 2) were made deactivated later - in this case making a link deactivated should've itself scheduled a dedicated buffer update (and possible effective branch and department change as the result) for that component From - no need to duplicate the action", !scheduledPKs.Contains(componentFrom1c.PK));
			AssertContainsExactElementsInAnyOrder("All scheduled actions", [componentTo1.PK, componentFrom1a.PK, componentFrom1b.PK], scheduledPKs);

			Assert("Should schedule actions targeting BMComponent", collection.All(s => s.TAS_TargetTableCode == BMComponentSchema.Constants.Prefix));
			Assert("Should schedule actions with proper code", collection.All(s => s.TAS_ActionCode == "PHU"));
			Assert("Should schedule actions with proper parameter", collection.All(s => s.TAS_JsonParameter == ProcessHeaderResponsiveActionConstants.UpdateEffectiveBranchAndDepartment));
			Assert("Should schedule actions as active or suspended", collection.All(s => s.TAS_ExecutionStatus == (liveSystem ? "SCH" : "SUS")));
			Assert("Should store system's PK as a token", collection.All(s => s.TAS_Token == system.PK.ToString()));
			Assert("Should schedule actions ready for execution", collection.All(s => s.TAS_ExecutionDateTimeUtc == ZDateTime.UtcNow));

			AssertMultilineASCIIEquals($@"Component To 1 (PK = {componentTo1.PK}) changed its aging department: scheduled responsive update of effective branches and departments (EBD responsive action) on workflows situated in component To 1 (PK = {componentTo1.PK}) of system Test (IsLive = {(liveSystem ? "Y" : "N")}) with status {(liveSystem ? "SCH" : "SUS")}.
An active component preceding to the modified component via active links: scheduled responsive update of effective branches and departments (EBD responsive action) on workflows situated in component From 1a (PK = {componentFrom1a.PK}) of system Test (IsLive = {(liveSystem ? "Y" : "N")}) with status {(liveSystem ? "SCH" : "SUS")}.
An active component preceding to the modified component via active links: scheduled responsive update of effective branches and departments (EBD responsive action) on workflows situated in component From 1b (PK = {componentFrom1b.PK}) of system Test (IsLive = {(liveSystem ? "Y" : "N")}) with status {(liveSystem ? "SCH" : "SUS")}.
", Logger.ToString());
		}

		[TestDate(2025, 2, 18)]
		public void TestShouldNotScheduleEffectiveBranchDepartmentUpdate_WhenChangingAgingDepartmentOnBucket()
		{
			SetupConfig();
			factory.Save();

			var oldDepartmentPK = Guid.NewGuid();
			var newDepartmentPK = Guid.NewGuid();
			var changeTable = CreateAuditBMComponentTable();
			var row = CreateAuditBMComponentRow(changeTable, componentPK: componentTo1.PK, componentType: "BUC", agingDepartmentPK: oldDepartmentPK);
			row.AcceptChanges();
			row.SetModified();
			row[BMComponentSchema.Constants.FC_GE_AgingDepartment] = newDepartmentPK;

			AssertEquals("Precondition: should be modified", DataRowState.Modified, row.RowState);
			Assert("Precondition: should have original version", row.HasVersion(DataRowVersion.Original));
			Assert("Precondition: should have current version", row.HasVersion(DataRowVersion.Current));
			AssertEquals("Precondition: original FC_GE_AgingDepartment", oldDepartmentPK, row[BMComponentSchema.Constants.FC_GE_AgingDepartment, DataRowVersion.Original]);
			AssertEquals("Precondition: current FC_GE_AgingDepartment", newDepartmentPK, row[BMComponentSchema.Constants.FC_GE_AgingDepartment, DataRowVersion.Current]);

			ProcessChanges(changeTable);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var collection = new TimeActionScheduleCollection(newFactory);

			AssertContainsExactElementsInAnyOrder("Should not schedule actions when component is a bucket", [], collection.Select(a => a.TAS_TargetPK));

			AssertMultilineASCIIEquals($@"Component To 1 (PK = {componentTo1.PK}) changed its aging department: no need to schedule responsive update of effective branch/department on workflows as the component is not a buffer.
", Logger.ToString());
		}

		#endregion

		#region Dedicated Buffer And Effective Branch / Department Update On Component Type Change

		[TestDate(2025, 2, 18)]
		public void TestShouldScheduleDedicatedBufferAndEffectiveBranchDepartmentUpdate_WhenChangingComponentTypeFromBucketToBuffer_ForActiveBMS()
		{
			AssertSchedulesDedicatedBufferAndEffectiveBranchDepartmentUpdate_WhenChangingComponentType_ForActiveBMS(liveSystem: true, previousType: "BUC", currentType: "BUF");
		}

		[TestDate(2025, 2, 18)]
		public void TestShouldScheduleDedicatedBufferAndEffectiveBranchDepartmentUpdate_WhenChangingComponentTypeBucketToBuffer_ForInactiveBMS()
		{
			AssertSchedulesDedicatedBufferAndEffectiveBranchDepartmentUpdate_WhenChangingComponentType_ForActiveBMS(liveSystem: false, previousType: "BUC", currentType: "BUF");
		}

		[TestDate(2025, 2, 18)]
		public void TestShouldScheduleDedicatedBufferAndEffectiveBranchDepartmentUpdate_WhenChangingComponentTypeFromBufferToBucket_ForActiveBMS()
		{
			AssertSchedulesDedicatedBufferAndEffectiveBranchDepartmentUpdate_WhenChangingComponentType_ForActiveBMS(liveSystem: true, previousType: "BUF", currentType: "BUC");
		}

		[TestDate(2025, 2, 18)]
		public void TestShouldScheduleDedicatedBufferAndEffectiveBranchDepartmentUpdate_WhenChangingComponentTypeBufferToBucket_ForInactiveBMS()
		{
			AssertSchedulesDedicatedBufferAndEffectiveBranchDepartmentUpdate_WhenChangingComponentType_ForActiveBMS(liveSystem: false, previousType: "BUF", currentType: "BUC");
		}

		void AssertSchedulesDedicatedBufferAndEffectiveBranchDepartmentUpdate_WhenChangingComponentType_ForActiveBMS(bool liveSystem, string previousType, string currentType)
		{
			SetupConfig();
			system.FS_IsLive = liveSystem;
			factory.Save();

			var changeTable = CreateAuditBMComponentTable();
			var row = CreateAuditBMComponentRow(changeTable, componentPK: componentTo1.PK, componentType: previousType);
			row.AcceptChanges();
			row.SetModified();
			row[BMComponentSchema.Constants.FC_Type] = currentType;

			AssertEquals("Precondition: should be modified", DataRowState.Modified, row.RowState);
			Assert("Precondition: should have original version", row.HasVersion(DataRowVersion.Original));
			Assert("Precondition: should have current version", row.HasVersion(DataRowVersion.Current));
			AssertEquals("Precondition: original FC_Type", previousType, row[BMComponentSchema.Constants.FC_Type, DataRowVersion.Original]);
			AssertEquals("Precondition: current FC_Type", currentType, row[BMComponentSchema.Constants.FC_Type, DataRowVersion.Current]);

			ProcessChanges(changeTable);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var collection = new TimeActionScheduleCollection(newFactory);

			var scheduledPKs = collection.Select(a => a.TAS_TargetPK).ToArray();
			Assert("Should schedule actions for the modified component", scheduledPKs.Contains(componentTo1.PK));
			Assert("Should not schedule actions for inactive preceding components because inactive components either 1) were already inactive when the type was modified and therefore do not require dedicated buffer update or 2) were made deactivated later - in this case making a component deactivated should've itself scheduled an action for that component - no need to duplicate the action", !scheduledPKs.Contains(componentFrom1dInactive.PK));
			Assert("Should schedule actions for all active components preceding via active links", new[] { componentFrom1a.PK, componentFrom1b.PK }.All(c => scheduledPKs.Contains(c)));
			Assert("Should not schedule actions for components preceding via inactive links because that inactive links either 1) were already inactive when the target To component was modified and therefore do not require dedicated buffer update or 2) were made deactivated later - in this case making a link deactivated should've itself scheduled an action for that component From - no need to duplicate the action", !scheduledPKs.Contains(componentFrom1c.PK));
			AssertContainsExactElementsInAnyOrder("All scheduled actions", [componentTo1.PK, componentFrom1a.PK, componentFrom1b.PK], scheduledPKs);

			Assert("Should schedule actions targeting BMComponent", collection.All(s => s.TAS_TargetTableCode == BMComponentSchema.Constants.Prefix));
			Assert("Should schedule actions with proper code", collection.All(s => s.TAS_ActionCode == "PHU"));
			Assert("Should schedule actions as active or suspended", collection.All(s => s.TAS_ExecutionStatus == (liveSystem ? "SCH" : "SUS")));
			Assert("Should store system's PK as a token", collection.All(s => s.TAS_Token == system.PK.ToString()));
			Assert("Should schedule actions ready for execution", collection.All(s => s.TAS_ExecutionDateTimeUtc == ZDateTime.UtcNow));

			var modifiedComponentSchedule = collection.Single(s => s.TAS_TargetPK == componentTo1.PK);
			AssertEquals("Should update effective branch/departments only for the modified component", ProcessHeaderResponsiveActionConstants.UpdateEffectiveBranchAndDepartment, modifiedComponentSchedule.TAS_JsonParameter);

			var precedingComponentsSchedules = collection.Except(modifiedComponentSchedule);
			Assert("Should update dedicated buffers only for the preceding components", precedingComponentsSchedules.All(s => s.TAS_JsonParameter == ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer));

			AssertMultilineASCIIEquals($@"Component To 1 (PK = {componentTo1.PK}) changed its type: scheduled responsive update of effective branches and departments (EBD responsive action) on workflows situated in component To 1 (PK = {componentTo1.PK}) of system Test (IsLive = {(liveSystem ? "Y" : "N")}) with status {(liveSystem ? "SCH" : "SUS")}.
An active component preceding to the modified component via active links: scheduled responsive update of dedicated buffers (BUF responsive action) on workflows situated in component From 1a (PK = {componentFrom1a.PK}) of system Test (IsLive = {(liveSystem ? "Y" : "N")}) with status {(liveSystem ? "SCH" : "SUS")}.
An active component preceding to the modified component via active links: scheduled responsive update of dedicated buffers (BUF responsive action) on workflows situated in component From 1b (PK = {componentFrom1b.PK}) of system Test (IsLive = {(liveSystem ? "Y" : "N")}) with status {(liveSystem ? "SCH" : "SUS")}.
", Logger.ToString());
		}

		#endregion

		#region Combinations Of Changes

		[TestDate(2025, 1, 30)]
		public void TestShouldSchedule_WhenChangingActiveStatusAndType()
		{
			SetupConfig();
			factory.Save();

			var changeTable = CreateAuditBMComponentTable();
			var row = CreateAuditBMComponentRow(changeTable, componentPK: componentTo1.PK, componentType: "BUC", isActive: false);
			row.AcceptChanges();
			row.SetModified();
			row[BMComponentSchema.Constants.FC_IsActive] = true;
			row[BMComponentSchema.Constants.FC_Type] = "BUF";

			AssertEquals("Precondition: should be modified", DataRowState.Modified, row.RowState);
			Assert("Precondition: should have original version", row.HasVersion(DataRowVersion.Original));
			Assert("Precondition: should have current version", row.HasVersion(DataRowVersion.Current));
			AssertEquals("Precondition: original FC_IsActive", false, row[BMComponentSchema.Constants.FC_IsActive, DataRowVersion.Original]);
			AssertEquals("Precondition: current FC_IsActive", true, row[BMComponentSchema.Constants.FC_IsActive, DataRowVersion.Current]);
			AssertEquals("Precondition: original FC_Type", "BUC", row[BMComponentSchema.Constants.FC_Type, DataRowVersion.Original]);
			AssertEquals("Precondition: current FC_Type", "BUF", row[BMComponentSchema.Constants.FC_Type, DataRowVersion.Current]);

			ProcessChanges(changeTable);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var collection = new TimeActionScheduleCollection(newFactory);

			var scheduledPKs = collection.Select(a => a.TAS_TargetPK).ToArray();
			Assert("Should schedule actions for the modified component", scheduledPKs.Contains(componentTo1.PK));
			Assert("Should not schedule actions for inactive preceding components", !scheduledPKs.Contains(componentFrom1dInactive.PK));
			Assert("Should schedule actions for all active components preceding via active links", new[] { componentFrom1a.PK, componentFrom1b.PK }.All(c => scheduledPKs.Contains(c)));
			Assert("Should not schedule actions for components preceding via inactive links", !scheduledPKs.Contains(componentFrom1c.PK));
			AssertContainsExactElementsInAnyOrder("All scheduled actions", [componentTo1.PK, componentFrom1a.PK, componentFrom1b.PK], scheduledPKs);

			Assert("Should schedule actions targeting BMComponent", collection.All(s => s.TAS_TargetTableCode == BMComponentSchema.Constants.Prefix));
			Assert("Should schedule actions with proper code", collection.All(s => s.TAS_ActionCode == "PHU"));
			Assert("Should schedule actions as active or suspended", collection.All(s => s.TAS_ExecutionStatus == "SCH"));
			Assert("Should store system's PK as a token", collection.All(s => s.TAS_Token == system.PK.ToString()));
			Assert("Should schedule actions ready for execution", collection.All(s => s.TAS_ExecutionDateTimeUtc == ZDateTime.UtcNow));

			var modifiedComponentSchedule = collection.Single(s => s.TAS_TargetPK == componentTo1.PK);
			AssertEquals("Should update both dedicated buffers and effective branch/departments for the modified component", ProcessHeaderResponsiveActionConstants.UpdateDedicatedBufferEffectiveBranchAndDepartment, modifiedComponentSchedule.TAS_JsonParameter);

			var precedingComponentsSchedules = collection.Except(modifiedComponentSchedule);
			Assert("Should update dedicated buffers only for the preceding components", precedingComponentsSchedules.All(s => s.TAS_JsonParameter == ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer));

			AssertMultilineASCIIEquals($@"Component To 1 (PK = {componentTo1.PK}) changed its activity status (activated) and type: scheduled responsive update of both dedicated buffers and effective branches and departments (BBD responsive action) on workflows situated in component To 1 (PK = {componentTo1.PK}) of system Test (IsLive = Y) with status SCH.
An active component preceding to the modified component via active links: scheduled responsive update of dedicated buffers (BUF responsive action) on workflows situated in component From 1a (PK = {componentFrom1a.PK}) of system Test (IsLive = Y) with status SCH.
An active component preceding to the modified component via active links: scheduled responsive update of dedicated buffers (BUF responsive action) on workflows situated in component From 1b (PK = {componentFrom1b.PK}) of system Test (IsLive = Y) with status SCH.
", Logger.ToString());
		}

		[TestDate(2025, 1, 30)]
		public void TestShouldSchedule_WhenChangingTypeAndBranch()
		{
			SetupConfig();
			factory.Save();

			var oldBranchPK = Guid.NewGuid();
			var newBranchPK = Guid.NewGuid();
			var changeTable = CreateAuditBMComponentTable();
			var row = CreateAuditBMComponentRow(changeTable, componentPK: componentTo1.PK, componentType: "BUC", agingBranchPK: oldBranchPK);
			row.AcceptChanges();
			row.SetModified();
			row[BMComponentSchema.Constants.FC_Type] = "BUF";
			row[BMComponentSchema.Constants.FC_GB_AgingBranch] = newBranchPK;

			AssertEquals("Precondition: should be modified", DataRowState.Modified, row.RowState);
			Assert("Precondition: should have original version", row.HasVersion(DataRowVersion.Original));
			Assert("Precondition: should have current version", row.HasVersion(DataRowVersion.Current));
			AssertEquals("Precondition: original FC_Type", "BUC", row[BMComponentSchema.Constants.FC_Type, DataRowVersion.Original]);
			AssertEquals("Precondition: current FC_Type", "BUF", row[BMComponentSchema.Constants.FC_Type, DataRowVersion.Current]);
			AssertEquals("Precondition: original FC_GB_AgingBranch", oldBranchPK, row[BMComponentSchema.Constants.FC_GB_AgingBranch, DataRowVersion.Original]);
			AssertEquals("Precondition: current FC_GB_AgingBranch", newBranchPK, row[BMComponentSchema.Constants.FC_GB_AgingBranch, DataRowVersion.Current]);

			ProcessChanges(changeTable);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var collection = new TimeActionScheduleCollection(newFactory);

			var scheduledPKs = collection.Select(a => a.TAS_TargetPK).ToArray();
			Assert("Should schedule actions for the modified component", scheduledPKs.Contains(componentTo1.PK));
			Assert("Should not schedule actions for inactive preceding components", !scheduledPKs.Contains(componentFrom1dInactive.PK));
			Assert("Should schedule actions for all active components preceding via active links", new[] { componentFrom1a.PK, componentFrom1b.PK }.All(c => scheduledPKs.Contains(c)));
			Assert("Should not schedule actions for components preceding via inactive links", !scheduledPKs.Contains(componentFrom1c.PK));
			AssertContainsExactElementsInAnyOrder("All scheduled actions", [componentTo1.PK, componentFrom1a.PK, componentFrom1b.PK], scheduledPKs);

			Assert("Should schedule actions targeting BMComponent", collection.All(s => s.TAS_TargetTableCode == BMComponentSchema.Constants.Prefix));
			Assert("Should schedule actions with proper code", collection.All(s => s.TAS_ActionCode == "PHU"));
			Assert("Should schedule actions as active or suspended", collection.All(s => s.TAS_ExecutionStatus == "SCH"));
			Assert("Should store system's PK as a token", collection.All(s => s.TAS_Token == system.PK.ToString()));
			Assert("Should schedule actions ready for execution", collection.All(s => s.TAS_ExecutionDateTimeUtc == ZDateTime.UtcNow));

			var modifiedComponentSchedule = collection.Single(s => s.TAS_TargetPK == componentTo1.PK);
			AssertEquals("Should update effective branch/departments only for the modified component", ProcessHeaderResponsiveActionConstants.UpdateEffectiveBranchAndDepartment, modifiedComponentSchedule.TAS_JsonParameter);

			var precedingComponentsSchedules = collection.Except(modifiedComponentSchedule);
			Assert("Should update dedicated buffers only for the preceding components", precedingComponentsSchedules.All(s => s.TAS_JsonParameter == ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer));

			AssertMultilineASCIIEquals($@"Component To 1 (PK = {componentTo1.PK}) changed its type and aging branch: scheduled responsive update of effective branches and departments (EBD responsive action) on workflows situated in component To 1 (PK = {componentTo1.PK}) of system Test (IsLive = Y) with status SCH.
An active component preceding to the modified component via active links: scheduled responsive update of dedicated buffers (BUF responsive action) on workflows situated in component From 1a (PK = {componentFrom1a.PK}) of system Test (IsLive = Y) with status SCH.
An active component preceding to the modified component via active links: scheduled responsive update of dedicated buffers (BUF responsive action) on workflows situated in component From 1b (PK = {componentFrom1b.PK}) of system Test (IsLive = Y) with status SCH.
", Logger.ToString());
		}

		[TestDate(2025, 1, 30)]
		public void TestShouldSchedule_WhenChangingActivityStatusTypeAndBranch()
		{
			SetupConfig();
			factory.Save();

			var oldBranchPK = Guid.NewGuid();
			var newBranchPK = Guid.NewGuid();
			var changeTable = CreateAuditBMComponentTable();
			var row = CreateAuditBMComponentRow(changeTable, componentPK: componentTo1.PK, componentType: "BUC", isActive: false, agingBranchPK: oldBranchPK);
			row.AcceptChanges();
			row.SetModified();
			row[BMComponentSchema.Constants.FC_Type] = "BUF";
			row[BMComponentSchema.Constants.FC_IsActive] = true;
			row[BMComponentSchema.Constants.FC_GB_AgingBranch] = newBranchPK;

			AssertEquals("Precondition: should be modified", DataRowState.Modified, row.RowState);
			Assert("Precondition: should have original version", row.HasVersion(DataRowVersion.Original));
			Assert("Precondition: should have current version", row.HasVersion(DataRowVersion.Current));
			AssertEquals("Precondition: original FC_Type", "BUC", row[BMComponentSchema.Constants.FC_Type, DataRowVersion.Original]);
			AssertEquals("Precondition: current FC_Type", "BUF", row[BMComponentSchema.Constants.FC_Type, DataRowVersion.Current]);
			AssertEquals("Precondition: original FC_IsActive", false, row[BMComponentSchema.Constants.FC_IsActive, DataRowVersion.Original]);
			AssertEquals("Precondition: current FC_IsActive", true, row[BMComponentSchema.Constants.FC_IsActive, DataRowVersion.Current]);
			AssertEquals("Precondition: original FC_GB_AgingBranch", oldBranchPK, row[BMComponentSchema.Constants.FC_GB_AgingBranch, DataRowVersion.Original]);
			AssertEquals("Precondition: current FC_GB_AgingBranch", newBranchPK, row[BMComponentSchema.Constants.FC_GB_AgingBranch, DataRowVersion.Current]);

			ProcessChanges(changeTable);

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var collection = new TimeActionScheduleCollection(newFactory);

			var scheduledPKs = collection.Select(a => a.TAS_TargetPK).ToArray();
			Assert("Should schedule actions for the modified component", scheduledPKs.Contains(componentTo1.PK));
			Assert("Should not schedule actions for inactive preceding components", !scheduledPKs.Contains(componentFrom1dInactive.PK));
			Assert("Should schedule actions for all active components preceding via active links", new[] { componentFrom1a.PK, componentFrom1b.PK }.All(c => scheduledPKs.Contains(c)));
			Assert("Should not schedule actions for components preceding via inactive links", !scheduledPKs.Contains(componentFrom1c.PK));
			AssertContainsExactElementsInAnyOrder("All scheduled actions", [componentTo1.PK, componentFrom1a.PK, componentFrom1b.PK], scheduledPKs);

			Assert("Should schedule actions targeting BMComponent", collection.All(s => s.TAS_TargetTableCode == BMComponentSchema.Constants.Prefix));
			Assert("Should schedule actions with proper code", collection.All(s => s.TAS_ActionCode == "PHU"));
			Assert("Should schedule actions as active or suspended", collection.All(s => s.TAS_ExecutionStatus == "SCH"));
			Assert("Should store system's PK as a token", collection.All(s => s.TAS_Token == system.PK.ToString()));
			Assert("Should schedule actions ready for execution", collection.All(s => s.TAS_ExecutionDateTimeUtc == ZDateTime.UtcNow));

			var modifiedComponentSchedule = collection.Single(s => s.TAS_TargetPK == componentTo1.PK);
			AssertEquals("Should update both dedicated buffer and effective branch/departments for the modified component", ProcessHeaderResponsiveActionConstants.UpdateDedicatedBufferEffectiveBranchAndDepartment, modifiedComponentSchedule.TAS_JsonParameter);

			var precedingComponentsSchedules = collection.Except(modifiedComponentSchedule);
			Assert("Should update dedicated buffers only for the preceding components", precedingComponentsSchedules.All(s => s.TAS_JsonParameter == ProcessHeaderResponsiveActionConstants.UpdateDedicatedBuffer));

			AssertMultilineASCIIEquals($@"Component To 1 (PK = {componentTo1.PK}) changed its activity status (activated), type and aging branch: scheduled responsive update of both dedicated buffers and effective branches and departments (BBD responsive action) on workflows situated in component To 1 (PK = {componentTo1.PK}) of system Test (IsLive = Y) with status SCH.
An active component preceding to the modified component via active links: scheduled responsive update of dedicated buffers (BUF responsive action) on workflows situated in component From 1a (PK = {componentFrom1a.PK}) of system Test (IsLive = Y) with status SCH.
An active component preceding to the modified component via active links: scheduled responsive update of dedicated buffers (BUF responsive action) on workflows situated in component From 1b (PK = {componentFrom1b.PK}) of system Test (IsLive = Y) with status SCH.
", Logger.ToString());
		}

		#endregion

		[TestDate(2025, 1, 30)]
		public void TestDBHits()
		{
			SetupConfig();
			system.FS_IsLive = true;
			factory.Save();

			var linkCount = 15;

			var changeTable = CreateAuditBMComponentTable();

			for (int i = 0; i < linkCount; i++)
			{
				var row = CreateAuditBMComponentRow(changeTable, componentPK: componentTo1.PK, componentType: "BUC", isActive: false);
				row.AcceptChanges();
				row.SetModified();
				row[BMComponentSchema.Constants.FC_IsActive] = true;
			}

			var expectedHits = new Dictionary<string, int>
			{
				{ BMComponentLinkSchema.Constants.TableName, 1 },
				{ BMComponentSchema.Constants.TableName, 1 },
				{ BMSystemSchema.Constants.TableName, 1 }, // for identifying the system PK to use it as a token for schedules 
				{ TimeActionScheduleSchema.Constants.TableName, 1 },
			};

			using (TestCaseWithFactory.AssertDbHitsForAllFactories(expectedHits, useOnlyNewFactories: true, ignoreHitsFromTablesCachedInUberFactory: true))
			{
				ProcessChanges(changeTable);
			}

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var collection = new TimeActionScheduleCollection(newFactory);
			AssertContainsExactElementsInAnyOrder("Should schedule actions", [componentTo1.PK, componentFrom1a.PK, componentFrom1b.PK], collection.Select(a => a.TAS_TargetPK));
		}

		#region Implementation

		ILogger logger;

		protected override ILogger Logger => logger;

		void ResetLogger()
		{
			logger = new SimpleLogger();
		}

		DataTable CreateAuditBMComponentTable(int numEntries = 0)
		{
			var changeTable = new DataTable();

			changeTable.Columns.Add(BMComponentSchema.Constants.PK, typeof(Guid));
			changeTable.Columns.Add(BMComponentSchema.Constants.FC_IsActive, typeof(bool));
			changeTable.Columns.Add(BMComponentSchema.Constants.FC_Type, typeof(string));
			changeTable.Columns.Add(BMComponentSchema.Constants.FC_GB_AgingBranch, typeof(Guid));
			changeTable.Columns.Add(BMComponentSchema.Constants.FC_GE_AgingDepartment, typeof(Guid));
			changeTable.Columns.Add(BMComponentSchema.Constants.FC_Name, typeof(string));
			changeTable.Columns.Add("TranEndTimeUtc", typeof(DateTime));

			for (var i = 0; i < numEntries; i++)
			{
				var row = CreateAuditBMComponentRow(changeTable, componentPK: Guid.NewGuid(), componentType: "BUC", isActive: true);
				row.AcceptChanges();
				row.SetModified();
			}

			return changeTable;
		}

		protected override DataTable GetTestDataTable() => CreateAuditBMComponentTable();

		DataRow CreateAuditBMComponentRow(DataTable dataTable, ZGuid componentPK, string componentType, bool? isActive = null, ZGuid? agingBranchPK = null, ZGuid? agingDepartmentPK = null, string name = null, DateTime? tranEndTimeUtc = null)
		{
			var row = dataTable.NewRow();

			row["TranEndTimeUtc"] = tranEndTimeUtc ?? ZDateTime.UtcNow.ToDateTime();

			row[BMComponentSchema.Constants.PK] = componentPK.ToGuid();
			row[BMComponentSchema.Constants.FC_IsActive] = isActive.HasValue ? isActive : true;
			row[BMComponentSchema.Constants.FC_Type] = componentType;
			row[BMComponentSchema.Constants.FC_GB_AgingBranch] = agingBranchPK.HasValue ? agingBranchPK.Value.ToGuid() : Guid.NewGuid();
			row[BMComponentSchema.Constants.FC_GE_AgingDepartment] = agingDepartmentPK.HasValue ? agingDepartmentPK.Value.ToGuid() : Guid.NewGuid();
			row[BMComponentSchema.Constants.FC_Name] = name;

			dataTable.Rows.Add(row);

			AssertEquals("Precondition: should be added", DataRowState.Added, row.RowState);
			Assert("Precondition: should have no original version", !row.HasVersion(DataRowVersion.Original));
			Assert("Precondition: should have current version", row.HasVersion(DataRowVersion.Current));

			return row;
		}

		BusinessObjectFactory factory;
		IBMTestHelper helper;
		IBMSystem system;
		IBMComponent componentFrom1a;
		IBMComponent componentFrom1b;
		IBMComponent componentFrom1c;
		IBMComponent componentFrom1dInactive;
		IBMComponent componentFrom2;
		IBMComponent componentTo1;
		IBMComponent componentTo2;
		IBMComponent terminalComponent;

		void SetupConfig()
		{
			factory = new BusinessObjectFactory();
			helper = ObjectFactory.Get<IBMTestHelper>();
			system = helper.CreateSystem(factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			system.FS_Name = "Test";

			componentFrom1a = helper.CreateBucket(system, "From 1a");
			componentFrom1a.FC_DisplaySequence = 0;

			componentFrom1b = helper.CreateBucket(system, "From 1b");
			componentFrom1b.FC_DisplaySequence = 1;

			componentFrom1c = helper.CreateBucket(system, "From 1c");
			componentFrom1c.FC_DisplaySequence = 2;

			componentFrom1dInactive = helper.CreateBucket(system, "From 1d (inactive)");
			componentFrom1dInactive.FC_IsActive = false;
			componentFrom1dInactive.FC_DisplaySequence = 3;

			componentFrom2 = helper.CreateBucket(system, "From 2");

			componentTo1 = helper.CreateBucket(system, "To 1");
			componentTo2 = helper.CreateBucket(system, "To 2");

			terminalComponent = helper.CreateBucket(system, "Terminal");

			var link1a = helper.LinkComponents(componentFrom1a, componentTo1);
			var link1b = helper.LinkComponents(componentFrom1b, componentTo1);
			var link1cInactive = helper.LinkComponents(componentFrom1c, componentTo1);
			link1cInactive.FL_TransferRulesEnabled = false;
			var link1d = helper.LinkComponents(componentFrom1dInactive, componentTo1);

			var link2 = helper.LinkComponents(componentFrom2, componentTo2);

			var terminalLink = helper.LinkComponents(componentTo1, terminalComponent);
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
