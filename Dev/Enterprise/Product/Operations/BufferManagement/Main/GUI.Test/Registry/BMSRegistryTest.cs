using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using static Enterprise.BufferManagement.Business.BMSRegistry;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(BMSRegistry))]
	class BMSRegistryTest : RegistryItemSetTestCaseWithFactory<BMSRegistry>
	{
		#region Buffer Management Enabled / Workflow Management Mode

		public void TestBufferManagementEnabled_DefaultValue_WhenProductivityWiseModeEnabled_ShouldBePLN()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;

			AssertEquals(WorkflowManagementModes.Codes.PlanningManagement, ItemSet.WorkflowManagementMode.DefaultValue);
		}

		public void TestBufferManagementEnabled_DefaultValue_WhenProductivityWiseModeDisabled_ShouldBeBWF()
		{
			AssertEquals(false, DataRegistry.Instance.ProductivityWiseModeEnabled);
			AssertEquals(WorkflowManagementModes.Codes.BasicWorkflow, ItemSet.WorkflowManagementMode.DefaultValue);
		}

		public void TestBufferManagementEnabled_OnUpdateAction_ShouldModifyValuesInDatabase()
		{
			ItemSet.WorkflowManagementMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WorkflowManagementModes.Codes.PlanningManagement);

			const string goodTaskCode = "~~1";
			const string rejectTaskCode_BadCategory = "~~2";
			var governorMock = new Mock<IServiceManagerGovernor>();
			var querierMock = new Mock<IServiceManagerQuerier>();

			querierMock
				.Setup(q => q.GetServiceTasksByCategory(BMSServiceTaskBase.Category))
				.Returns(new[] { goodTaskCode });

			querierMock
				.Setup(q => q.GetServiceTasksByCategory("BLE"))
				.Returns(new[] { rejectTaskCode_BadCategory });

			using (ObjectFactory.Substitute(governorMock.Object))
			using (ObjectFactory.Substitute(querierMock.Object))
			{
				CombineAssertions(() =>
				{
					SetBufferManagementEnabledRegistryItemAndAssertItChangesOnSave(isActive: true, WorkflowManagementModes.Codes.PlanningManagement);
					SetBufferManagementEnabledRegistryItemAndAssertItChangesOnSave(isActive: false, WorkflowManagementModes.Codes.BasicWorkflow);
				});

				void SetBufferManagementEnabledRegistryItemAndAssertItChangesOnSave(bool isActive, string workflowMode)
				{
					SetRegistryItemToInvokeOnUpdateAction(ItemSet.WorkflowManagementMode, workflowMode);

					AssertNoExceptionThrown(() =>
					{
						governorMock.Verify(g => g.SetServiceTaskIsActive(goodTaskCode, isActive), Times.Once);
					});
				}
			}
		}

		[ExpectNoExceptions]
		public void TestWorkflowManagementMode_WhenValueChanges_ShouldActivateOrDeactivateTagServiceTask()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.PlanningManagement);
			var tagServiceTask = TagServiceTask.Code;
			var bmmServiceTask = TagMonitorServiceTask.Code;
			var bmsServiceTask = TransferRuleRunnerServiceTask.Code;
			var bmgServiceTask = ReleaseGateRunnerServiceTask.Code;
			var bmcServiceTask = CapacityConstrainedResourceStatusServiceTask.Code;
			var bmdServiceTask = StaggeredReleaseDelayCalculatorTask.Code;
			var bmlServiceTask = SchematicTransferLoopMonitorServiceTask.Code;
			var bmtServiceTask = CapabilityTaskAutoAssignmentServiceTask.Code;

			var governorMock = new Mock<IServiceManagerGovernor>();
			var querierMock = new Mock<IServiceManagerQuerier>();

			querierMock
				.Setup(q => q.GetServiceTasksByCategory(BMSServiceTaskBase.Category))
				.Returns(new[] { tagServiceTask, bmmServiceTask, bmsServiceTask, bmgServiceTask, bmcServiceTask, bmdServiceTask, bmlServiceTask, bmtServiceTask });

			using (ObjectFactory.Substitute(governorMock.Object))
			using (ObjectFactory.Substitute(querierMock.Object))
			{
				SetRegistryItemToInvokeOnUpdateAction(BMSRegistry.Instance.WorkflowManagementMode, WorkflowManagementModes.Codes.IncludesBufferManagement);
				governorMock.Verify(g => g.SetServiceTaskIsActive(tagServiceTask, true), Times.Once, "Tag Rules need BUF or higher, so it should still be active since BUF was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmmServiceTask, true), Times.Once, "Tag Rules need BUF or higher, so it should still be active since BUF was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmsServiceTask, true), Times.Once, "BMS Transfers need Enhanced Workflow Management or better, so it should remain enabled.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmgServiceTask, true), Times.Once, "Release Gate Runner needs BUF or higher, so it should still be active since BUF was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmcServiceTask, true), Times.Once, "Capacity constrained resource status updating needs BUF or higher, so it should still be active since BUF was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmdServiceTask, true), Times.Once, "Staggered release delay calculating needs BUF or higher, so it should still be active since BUF was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmlServiceTask, true), Times.Once, "Schematic transfer loop monitoring needs Enhanced Workflow Management or higher, so it should still be active since BUF was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmtServiceTask, true), Times.Once, "Capability task auto assignment needs BUF or higher, so it should still be active since BUF was selected.");
				governorMock.Reset();

				SetRegistryItemToInvokeOnUpdateAction(BMSRegistry.Instance.WorkflowManagementMode, WorkflowManagementModes.Codes.EnhancedWorkflow);
				governorMock.Verify(g => g.SetServiceTaskIsActive(tagServiceTask, false), Times.Once, "Tag Rules need BUF or higher, so it should be deactivated since EWF was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmmServiceTask, false), Times.Once, "Tag Rules need BUF or higher, so it should be deactivated since EWF was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmsServiceTask, true), Times.Once, "BMS Transfers need Enhanced Workflow Management or better, so it should remain enabled.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmgServiceTask, false), Times.Once, "Release Gate Runner needs BUF or higher, so it should be deactivated since EWF was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmcServiceTask, false), Times.Once, "Capacity constrained resource status updating needs BUF or higher, so it should be deactivated since EWF was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmdServiceTask, false), Times.Once, "Staggered release delay calculating needs BUF or higher, so it should be deactivated since EWF was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmlServiceTask, true), Times.Once, "Schematic transfer loop monitoring needs Enhanced Workflow Management or higher, so it should still be active since EWF was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmtServiceTask, false), Times.Once, "Capability task auto assignment needs BUF or higher, so it should be deactivated since EWF was selected.");
				governorMock.Reset();

				SetRegistryItemToInvokeOnUpdateAction(BMSRegistry.Instance.WorkflowManagementMode, WorkflowManagementModes.Codes.PlanningManagement);
				governorMock.Verify(g => g.SetServiceTaskIsActive(tagServiceTask, true), Times.Once, "Tag Rules need BUF or higher, so it should be re-activated since PLN was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmmServiceTask, true), Times.Once, "Tag Rules need BUF or higher, so it should be re-activated since PLN was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmsServiceTask, true), Times.Once, "BMS Transfers need Enhanced Workflow Management or better, so it should remain enabled.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmgServiceTask, true), Times.Once, "Release Gate Runner needs BUF or higher, so it should be re-activated since PLN was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmcServiceTask, true), Times.Once, "Capacity constrained resource status updating needs BUF or higher, so it should be reactivated since PLN was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmdServiceTask, true), Times.Once, "Staggered release delay calculating needs BUF or higher, so it should be reactivated since PLN was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmlServiceTask, true), Times.Once, "Schematic transfer loop monitoring needs Enhanced Workflow Management or higher, so it should still be active since PLN was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmtServiceTask, true), Times.Once, "Capability task auto assignment needs BUF or higher, so it should be reactivated since PLN was selected.");
				governorMock.Reset();

				SetRegistryItemToInvokeOnUpdateAction(BMSRegistry.Instance.WorkflowManagementMode, WorkflowManagementModes.Codes.BasicWorkflow);
				governorMock.Verify(g => g.SetServiceTaskIsActive(tagServiceTask, false), Times.Once, "Tag Rules need BUF or higher, so it should be deactivated since BWF was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmmServiceTask, false), Times.Once, "Tag Rules need BUF or higher, so it should be deactivated since BWF was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmsServiceTask, false), Times.Once, "BMS Transfers need Enhanced Workflow Management or better, so it should be disabled.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmgServiceTask, false), Times.Once, "Release Gate Runner needs BUF or higher, so it should be deaactivated since BWF was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmcServiceTask, false), Times.Once, "Capacity constrained resource status updating needs BUF or higher, so it should be deactivated since BWF was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmdServiceTask, false), Times.Once, "Staggered release delay calculating needs BUF or higher, so it should be deactivated since BWF was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmlServiceTask, false), Times.Once, "Schematic transfer loop monitoring needs Enhanced Workflow Management or higher, so it should be deactivated since BWF was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmtServiceTask, false), Times.Once, "Capability task auto assignment needs BUF or higher, so it should be deactivated since BWF was selected.");
			}
		}

		[ExpectNoExceptions]
		public void TestWorkflowManagementMode_WhenValueChangesAndServiceTasksEnabled_ShouldNotReEnableTasksDisabledByDisableCapacityCalculationsRegistryItem()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.BasicWorkflow);
			var tagServiceTask = TagServiceTask.Code;
			var bmgServiceTask = ReleaseGateRunnerServiceTask.Code;

			var governorMock = new Mock<IServiceManagerGovernor>();
			var querierMock = new Mock<IServiceManagerQuerier>();

			querierMock
				.Setup(q => q.GetServiceTasksByCategory(BMSServiceTaskBase.Category))
				.Returns(new[] { tagServiceTask, bmgServiceTask });

			using (ObjectFactory.Substitute(governorMock.Object))
			using (ObjectFactory.Substitute(querierMock.Object))
			{
				SetRegistryItemToInvokeOnUpdateAction(BMSRegistry.Instance.DisableCapacityCalculations, true);
				governorMock.Verify(g => g.SetServiceTaskIsActive(tagServiceTask, false), Times.Never);
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmgServiceTask, false), Times.Once, "The Release Gate service task should have been disabled when we disabled capacity calculations.");
				governorMock.Reset();

				SetRegistryItemToInvokeOnUpdateAction(BMSRegistry.Instance.WorkflowManagementMode, WorkflowManagementModes.Codes.IncludesBufferManagement);
				governorMock.Verify(g => g.SetServiceTaskIsActive(tagServiceTask, true), Times.Once, "We selected Planning Management so all BMS service tasks should have been enabled (except the capacity ones).");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmgServiceTask, false), Times.Once, "Capacity calculations are still off, so the capacity-dependent service tasks should not have been re-enabled.");
			}
		}

		[ExpectNoExceptions]
		public void TestWorkflowManagementMode_WhenValueChanges_ShouldActivateOrDeactivateBMBServiceTask()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.PlanningManagement);
			var bmbServiceTask = "BMB";
			var bmsServiceTask = TransferRuleRunnerServiceTask.Code; // control
			var governorMock = new Mock<IServiceManagerGovernor>();
			var querierMock = new Mock<IServiceManagerQuerier>();

			querierMock
				.Setup(q => q.GetServiceTasksByCategory(BMSServiceTaskBase.Category))
				.Returns(new[] { bmbServiceTask, bmsServiceTask });

			using (ObjectFactory.Substitute(governorMock.Object))
			using (ObjectFactory.Substitute(querierMock.Object))
			{
				SetRegistryItemToInvokeOnUpdateAction(BMSRegistry.Instance.WorkflowManagementMode, WorkflowManagementModes.Codes.IncludesBufferManagement);
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmbServiceTask, false), Times.Once(), "Buffer penetration service task deals with shapes, so PLN is needed, so it should be disabled since BUF was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmsServiceTask, true), Times.Once(), "BMS Transfers need Enhanced Workflow Management or better, so it should remain enabled.");
				governorMock.Reset();

				SetRegistryItemToInvokeOnUpdateAction(BMSRegistry.Instance.WorkflowManagementMode, WorkflowManagementModes.Codes.PlanningManagement);
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmbServiceTask, true), Times.Once(), "Buffer penetration service task deals with shapes, so PLN is needed, so it should be re-activated since PLN was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmsServiceTask, true), Times.Once(), "BMS Transfers need Enhanced Workflow Management or better, so it should remain enabled.");
				governorMock.Reset();

				SetRegistryItemToInvokeOnUpdateAction(BMSRegistry.Instance.WorkflowManagementMode, WorkflowManagementModes.Codes.EnhancedWorkflow);
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmbServiceTask, false), Times.Once(), "Buffer penetration service task deals with shapes, so PLN is needed, so it should be disabled since EWF was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmsServiceTask, true), Times.Once(), "BMS Transfers need Enhanced Workflow Management or better, so it should remain enabled.");
				governorMock.Reset();

				SetRegistryItemToInvokeOnUpdateAction(BMSRegistry.Instance.WorkflowManagementMode, WorkflowManagementModes.Codes.PlanningManagement);
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmbServiceTask, true), Times.Once(), "Buffer penetration service task deals with shapes, so PLN is needed, so it should be re-activated since PLN was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmsServiceTask, true), Times.Once(), "BMS Transfers need Enhanced Workflow Management or better, so it should remain enabled.");
				governorMock.Reset();

				SetRegistryItemToInvokeOnUpdateAction(BMSRegistry.Instance.WorkflowManagementMode, WorkflowManagementModes.Codes.BasicWorkflow);
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmbServiceTask, false), Times.Once(), "Buffer penetration service task deals with shapes, so PLN is needed, so it should be disabled since BWF was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmsServiceTask, false), Times.Once(), "BMS Transfers need Enhanced Workflow Management or better, so it should be disabled.");
			}
		}

		[ExpectNoExceptions]
		public void TestWorkflowManagementMode_WhenValueChanges_ShouldActivateOrDeactivateMENTServiceTasks()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.PlanningManagement);
			var asmServiceTask = TagServiceTask.Code;
			var bmpServiceTask = TagMonitorServiceTask.Code;
			var bmsServiceTask = TransferRuleRunnerServiceTask.Code; // control
			var governorMock = new Mock<IServiceManagerGovernor>();
			var querierMock = new Mock<IServiceManagerQuerier>();

			querierMock
				.Setup(q => q.GetServiceTasksByCategory(BMSServiceTaskBase.Category))
				.Returns(new[] { asmServiceTask, bmpServiceTask, bmsServiceTask });

			using (ObjectFactory.Substitute(governorMock.Object))
			using (ObjectFactory.Substitute(querierMock.Object))
			{
				SetRegistryItemToInvokeOnUpdateAction(BMSRegistry.Instance.WorkflowManagementMode, WorkflowManagementModes.Codes.IncludesBufferManagement);
				governorMock.Verify(g => g.SetServiceTaskIsActive(asmServiceTask, true), Times.Once(), "MENT needs BUF or higher, so it should still be active since BUF was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmpServiceTask, true), Times.Once(), "MENT needs BUF or higher, so it should still be active since BUF was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmsServiceTask, true), Times.Once(), "BMS Transfers need Enhanced Workflow Management or better, so it should remain enabled.");
				governorMock.Reset();

				SetRegistryItemToInvokeOnUpdateAction(BMSRegistry.Instance.WorkflowManagementMode, WorkflowManagementModes.Codes.EnhancedWorkflow);
				governorMock.Verify(g => g.SetServiceTaskIsActive(asmServiceTask, false), Times.Once(), "MENT needs BUF or higher, so it should be deactivated since EWF was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmpServiceTask, false), Times.Once(), "MENT needs BUF or higher, so it should be deactivated since EWF was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmsServiceTask, true), Times.Once(), "BMS Transfers need Enhanced Workflow Management or better, so it should remain enabled.");
				governorMock.Reset();

				SetRegistryItemToInvokeOnUpdateAction(BMSRegistry.Instance.WorkflowManagementMode, WorkflowManagementModes.Codes.PlanningManagement);
				governorMock.Verify(g => g.SetServiceTaskIsActive(asmServiceTask, true), Times.Once(), "MENT needs BUF or higher, so it should be re-activated since PLN was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmpServiceTask, true), Times.Once(), "MENT needs BUF or higher, so it should be re-activated since PLN was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmsServiceTask, true), Times.Once(), "BMS Transfers need Enhanced Workflow Management or better, so it should remain enabled.");
				governorMock.Reset();

				SetRegistryItemToInvokeOnUpdateAction(BMSRegistry.Instance.WorkflowManagementMode, WorkflowManagementModes.Codes.BasicWorkflow);
				governorMock.Verify(g => g.SetServiceTaskIsActive(asmServiceTask, false), Times.Once(), "MENT needs BUF or higher, so it should be deactivated since BWF was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmpServiceTask, false), Times.Once(), "MENT needs BUF or higher, so it should be deactivated since BWF was selected.");
				governorMock.Verify(g => g.SetServiceTaskIsActive(bmsServiceTask, false), Times.Once(), "BMS Transfers need Enhanced Workflow Management or better, so it should be disabled.");
			}
		}

		public void TestWorkflowManagementModes_ShouldBeInCorrectOrder()
		{
			var modes = new WorkflowManagementModes();
			var codes = modes.CodesAsString;

			AssertEquals("We need the options to appear in the registry in this order, since the order is significant.", "BWF, EWF, BUF, PLN", codes);
		}

		public void TestProvider_IsEnhancedWorkflowManagementOrBetterEnabled()
		{
			AssertEquals(WorkflowManagementModes.Codes.BasicWorkflow, BMSRegistry.Instance.WorkflowManagementMode.Value);
			AssertEquals(false, BMSRegistryProvider.IsEnhancedWorkflowManagementOrBetterEnabled);

			BMSRegistry.Instance.WorkflowManagementMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WorkflowManagementModes.Codes.EnhancedWorkflow);
			AssertEquals(true, BMSRegistryProvider.IsEnhancedWorkflowManagementOrBetterEnabled);

			BMSRegistry.Instance.WorkflowManagementMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WorkflowManagementModes.Codes.IncludesBufferManagement);
			AssertEquals(true, BMSRegistryProvider.IsEnhancedWorkflowManagementOrBetterEnabled);

			BMSRegistry.Instance.WorkflowManagementMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WorkflowManagementModes.Codes.PlanningManagement);
			AssertEquals(true, BMSRegistryProvider.IsEnhancedWorkflowManagementOrBetterEnabled);
		}

		public void TestProvider_IsBufferManagementWorkflowModeOrBetterEnabled()
		{
			AssertEquals(WorkflowManagementModes.Codes.BasicWorkflow, BMSRegistry.Instance.WorkflowManagementMode.Value);
			AssertEquals(false, BMSRegistryProvider.IsBufferManagementWorkflowModeOrBetterEnabled);

			BMSRegistry.Instance.WorkflowManagementMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WorkflowManagementModes.Codes.EnhancedWorkflow);
			AssertEquals(false, BMSRegistryProvider.IsBufferManagementWorkflowModeOrBetterEnabled);

			BMSRegistry.Instance.WorkflowManagementMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WorkflowManagementModes.Codes.IncludesBufferManagement);
			AssertEquals(true, BMSRegistryProvider.IsBufferManagementWorkflowModeOrBetterEnabled);

			BMSRegistry.Instance.WorkflowManagementMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WorkflowManagementModes.Codes.PlanningManagement);
			AssertEquals(true, BMSRegistryProvider.IsBufferManagementWorkflowModeOrBetterEnabled);
		}

		public void TestProvider_IsPlanningManagementEnabled()
		{
			AssertEquals(WorkflowManagementModes.Codes.BasicWorkflow, BMSRegistry.Instance.WorkflowManagementMode.Value);
			AssertEquals(false, BMSRegistryProvider.IsPlanningManagementEnabled);

			BMSRegistry.Instance.WorkflowManagementMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WorkflowManagementModes.Codes.EnhancedWorkflow);
			AssertEquals(false, BMSRegistryProvider.IsPlanningManagementEnabled);

			BMSRegistry.Instance.WorkflowManagementMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WorkflowManagementModes.Codes.IncludesBufferManagement);
			AssertEquals(false, BMSRegistryProvider.IsPlanningManagementEnabled);

			BMSRegistry.Instance.WorkflowManagementMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WorkflowManagementModes.Codes.PlanningManagement);
			AssertEquals(true, BMSRegistryProvider.IsPlanningManagementEnabled);
		}

		#endregion

		public void TestAllowCompanyFiltersInTagRules()
		{
			AssertEquals("Name", "AllowCompanyFiltersInTagRules", ItemSet.AllowCompanyFiltersInTagRules.Name);
			AssertEquals("Category", Categories.WorkflowManager_BufferManagement_Tags, ItemSet.AllowCompanyFiltersInTagRules.Category);
			AssertEquals("Caption", "Allow Company Filters in Tag Rules", ItemSet.AllowCompanyFiltersInTagRules.Caption);
			AssertEquals("Description", "When enabled, workflows may be excluded from being tagged based on the company of the Branch selected on the Tag Rule. For example, workflows belonging to Customs Declarations may not be tagged if the declaration was created in a different company to the one the Tag Rule's Branch belongs to. When this registry item is set to No, this company filtering will not be applied, so more workflows may be tagged than are shown in the Tag Rule's Preview popup. When set to Yes, this filtering will be applied.", ItemSet.AllowCompanyFiltersInTagRules.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.AllowCompanyFiltersInTagRules.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.AllowCompanyFiltersInTagRules.Options);
			AssertEquals("Default Value", false, ItemSet.AllowCompanyFiltersInTagRules.DefaultValue);
		}

		public void TestCapacityCalculatorStaffBatchSize()
		{
			AssertEquals(200, ItemSet.CapacityCalculatorStaffBatchSize.DefaultValue);
			AssertEquals((double)20, ((IntRegistryDataType)ItemSet.CapacityCalculatorStaffBatchSize.DataType).LowerBound);
			AssertEquals((double)1000, ((IntRegistryDataType)ItemSet.CapacityCalculatorStaffBatchSize.DataType).UpperBound);
		}

		public void TestLogWorkflowLoadTime_DefaultValue()
		{
			AssertEquals(true, ItemSet.LogWorkflowLoadTime.Value);
		}

		public void TestLogWorkflowInfoOnTransfer()
		{
			AssertEquals(false, ItemSet.LogWorkflowInfoOnTransfer.Value);
			AssertEquals("When enabled, the description and job number of workflows will be logged to the BMS and PVE service task log as they are moved between components.", ItemSet.LogWorkflowInfoOnTransfer.Hint);
		}

		public void TestNumberOfRefreshesBeforeFullRefresh()
		{
			AssertEquals(10, ItemSet.NumberOfRefreshesBeforeFullRefresh.DefaultValue);
			AssertEquals((double)0, ((IntRegistryDataType)ItemSet.NumberOfRefreshesBeforeFullRefresh.DataType).LowerBound);
			AssertEquals((double)1000, ((IntRegistryDataType)ItemSet.NumberOfRefreshesBeforeFullRefresh.DataType).UpperBound);
		}

		public void TestNumberOffBoardsAllowedToCacheInSlideShowRefresh()
		{
			AssertEquals(5, ItemSet.MaxNumberOfBoardsAllowedToCacheInSlideShow.DefaultValue);
			AssertEquals((double)0, ((IntRegistryDataType)ItemSet.MaxNumberOfBoardsAllowedToCacheInSlideShow.DataType).LowerBound);
			AssertEquals((double)30, ((IntRegistryDataType)ItemSet.MaxNumberOfBoardsAllowedToCacheInSlideShow.DataType).UpperBound);
		}

		public void TestBufferManagementEnabledDefaultDisabled()
		{
			AssertEquals(WorkflowManagementModes.Codes.BasicWorkflow, ItemSet.WorkflowManagementMode.DefaultValue);
		}

		public void TestReleaseGateBatchSize_ShouldBeTheSameAsMaxElementsForParameterisation()
		{
			AssertEquals(ZSQLInFilter.MAXIMUM_ELEMENTS_FOR_PARAMETERISATION, ItemSet.ReleaseGateBatchSize.DefaultValue);
			AssertEquals((double)ZSQLInFilter.MAXIMUM_ELEMENTS_FOR_PARAMETERISATION, ((IntRegistryDataType)ItemSet.ReleaseGateBatchSize.DataType).UpperBound);
		}

		public void TestReleaseGateUseDateOrderingForReleaseGate()
		{
			AssertEquals(false, ItemSet.UseDateOrderingForReleaseGate.DefaultValue);
		}

		public void TestResourceLeaveWindowforReleasingWork()
		{
			AssertEquals("Location of registry item", Categories.WorkflowManager_BufferManagement_ReleaseGate, ItemSet.ResourceLeaveWindowforReleasingWork.Category);
			AssertEquals("Name of registry item", "Resource Leave Window for Releasing Work", ItemSet.ResourceLeaveWindowforReleasingWork.Caption);
			AssertEquals("Description of registry item", @"The size of the window (as a percentage of a buffer) during which work can be released to a resource who is away on leave.
For example, with a 6 day buffer and a 'Resource Leave Window for Releasing Work' of 40%, work involving this resource will only be released when the resource is returning from leave within 2.4 days, and normal Release Gate rules are met.
A value of 0 indicates no work can be released to a resource whilst they are on leave.
A value of 100 indicates that work can always be released to a resource whilst they are on leave, contingent upon normal Release Gate rules.", ItemSet.ResourceLeaveWindowforReleasingWork.Hint);
			AssertEquals("Initial value should be 40", 40, ItemSet.ResourceLeaveWindowforReleasingWork.Value);
			AssertEquals("Default value should be 40", 40, ItemSet.ResourceLeaveWindowforReleasingWork.DefaultValue);
			AssertEquals("Percent min value is 0", 0.0, ((IntRegistryDataType)ItemSet.ResourceLeaveWindowforReleasingWork.DataType).LowerBound);
			AssertEquals("Percent max value is 100", 100.0, ((IntRegistryDataType)ItemSet.ResourceLeaveWindowforReleasingWork.DataType).UpperBound);
		}

		public void TestBoardEnableSecondaryServer()
		{
			AssertEquals(false, ItemSet.BoardOnSecondaryServer.DefaultValue);
		}

		public void TestSynchroniseBufferPenetration()
		{
			AssertEquals(true, ItemSet.SynchroniseBufferPenetration.DefaultValue);
		}

		public void TestBoardShowTaskTags()
		{
			AssertEquals(false, ItemSet.BoardShowTaskTags.DefaultValue);
		}

		public void TestDisallowDBHitsOnBoardGUIThreadDefaultDisabled()
		{
			AssertEquals(false, ItemSet.DisallowDBHitsOnBoardGUIThread.DefaultValue);
		}

		public void TestBoardBatchTagDBHitsDefaultDisabled()
		{
			AssertEquals(false, ItemSet.BoardBatchTagDBHits.DefaultValue);
		}

		public void TestAllowedTablesDBHitsOnBoardGUIThreadDefaultEmpty()
		{
			string[] emptyStringArray = Array.Empty<string>();
			AssertArrayEqualsByElements(emptyStringArray, ItemSet.AllowedTablesDBHitsOnBoardGUIThread.DefaultValue);
		}

		public void TestWorkflowStatusUpdaterPrerequisiteDepth_ShouldRemainAtSetDefaultValue()
		{
			AssertEquals(@"Please, please, please change the values in: AcceptabilityBandCalculator
 • GetBufferPenetration.sql, and
 • GetTasksForWorkflowAndChildren.sql!
These are sql functions that require the MaximumDepthOfAnalyzedWorkflowHierarchy default value to be 30, however they had to be hardcoded due to reasons.
If you change the default value, CHANGE THESE VALUES!", 30, ItemSet.MaximumDepthOfAnalyzedWorkflowHierarchy.DefaultValue);
		}

		public void TestChannelHeadingsUsePreferredName_DefaultValue()
		{
			AssertEquals(true, ItemSet.ChannelHeadingsUsePreferredName.DefaultValue);
		}

		public void TestNotificationGroup()
		{
			AssertEquals("BMSNotificationGroup", ItemSet.NotificationGroup.Name);
			AssertEquals("Location of registry item", Categories.WorkflowManager_BufferManagement_Notification, Categories.WorkflowManager_BufferManagement_Notification);
			AssertEquals("System Notification Group", ItemSet.NotificationGroup.Caption);
			AssertEquals("Description of registry item", "The staff group that will be notified about issues relating to Buffer Management Systems.", ItemSet.NotificationGroup.Hint);
			AssertEquals("Default value for " + ItemSet.NotificationGroup.Name, Core.Constants.Groups.PostMastersGroupPK, ItemSet.NotificationGroup.DefaultValue);
			var newValue = Guid.NewGuid();
			ItemSet.NotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
			AssertEquals("Value for " + ItemSet.NotificationGroup.Name, newValue, ItemSet.NotificationGroup.Value);
		}

		public void TestNotificationPeriod()
		{
			AssertEquals("BMSNotificationPeriod", ItemSet.NotificationPeriod.Name);
			AssertEquals("Location of registry item", Categories.WorkflowManager_BufferManagement_Notification, ItemSet.NotificationPeriod.Category);
			AssertEquals("System Notification Period", ItemSet.NotificationPeriod.Caption);
			AssertEquals("Description of registry item", @"This defines the time period in minutes to accumulate errors of the same kind before they will be sent to the Buffer Management System Notification Group in a form of a digest notification email.
Errors of a kind which have been registered a long time ago will be ignored. 
Entering a value of zero means immediate notification without accumulating.", ItemSet.NotificationPeriod.Hint);
			AssertEquals("Initial value should be 60", 60, ItemSet.NotificationPeriod.Value);
			AssertEquals("Default value should be 60", 60, ItemSet.NotificationPeriod.DefaultValue);
			AssertEquals("Min value is 0", 0, (int)((IntRegistryDataType)ItemSet.NotificationPeriod.DataType).LowerBound);
			AssertEquals("Max value is 2880", 2880, (int)((IntRegistryDataType)ItemSet.NotificationPeriod.DataType).UpperBound);
			AssertEquals("RegistryOptions is Default", RegistryOptions.Default, ItemSet.NotificationPeriod.Options);
		}

		public void TestNotificationThreshold()
		{
			AssertEquals("BMSNotificationThreshold", ItemSet.NotificationThreshold.Name);
			AssertEquals("Location of registry item", Categories.WorkflowManager_BufferManagement_Notification, ItemSet.NotificationThreshold.Category);
			AssertEquals("System Notification Threshold", ItemSet.NotificationThreshold.Caption);
			AssertEquals("Description of registry item", @"This defines how many times an error must occur before the system sends a notification email.
If the number of accumulated errors is lower than this value when the notification period elapses, then a notification email will not be reported. 
If the error occurred a long time ago it will be ignored. Entering a value 1 means email notifications will be sent immediately without accumulating.", ItemSet.NotificationThreshold.Hint);
			AssertEquals("Initial value should be 30", 30, ItemSet.NotificationThreshold.Value);
			AssertEquals("Default value should be 30", 30, ItemSet.NotificationThreshold.DefaultValue);
			AssertEquals("Min value is 1", 1, (int)((IntRegistryDataType)ItemSet.NotificationThreshold.DataType).LowerBound);
			AssertEquals("Max value is" + int.MaxValue, int.MaxValue, (int)((IntRegistryDataType)ItemSet.NotificationThreshold.DataType).UpperBound);
			AssertEquals("RegistryOptions is Default", RegistryOptions.Default, ItemSet.NotificationThreshold.Options);
		}

		public void TestAcceptabilityBandExecutionTimeouts()
		{
			AssertEquals("Default value should be 30, which is treated as unset.", 30, ItemSet.AcceptabilityBandCalculationServiceExecutionTimeout.DefaultValue);
			AssertEquals("Default value should be 30, which is treated as unset.", 30, ItemSet.AcceptabilityBandLocalCalculationExecutionTimeout.DefaultValue);

			ItemSet.AcceptabilityBandCalculationServiceExecutionTimeout.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);
			ItemSet.AcceptabilityBandLocalCalculationExecutionTimeout.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 200);

			AssertNotEquals("Registry items are independently defined.", ItemSet.AcceptabilityBandCalculationServiceExecutionTimeout.Value, ItemSet.AcceptabilityBandLocalCalculationExecutionTimeout.Value);
		}

		public void TestRegistryItems_ShouldNotUseNotCachedOption()
		{
			var exceptedRegistryItems = new IRegistryItem[]
			{
				ItemSet.ProcessAllTransferRulesLinksOnNextBMSRun,
			};

			var registryItemsToCheck = this.AllItems.Except(exceptedRegistryItems);

			CombineAssertions(() =>
			{
				foreach (var registry in registryItemsToCheck)
				{
					Assert($"Registry {registry.Name} should not use NotCached Option", !registry.Options.HasFlag(RegistryOptions.NotCached));
				}
			});
		}

		public void TestBoardsAutoCalculateCapacityWhenNotInCache()
		{
			var boardsAutoCalculateCapacityWhenNotInCache = ItemSet.BoardsAutoCalculateCapacityWhenNotInCache;

			AssertEquals("BoardsAutoCalculateCapacityWhenNotInCache", boardsAutoCalculateCapacityWhenNotInCache.Name);
			AssertEquals("Correct location", Categories.WorkflowManager_BufferManagement_VisualBoards, boardsAutoCalculateCapacityWhenNotInCache.Category);
			AssertEquals("Boards Auto-Calculate Non-cached Capacity", boardsAutoCalculateCapacityWhenNotInCache.Caption);
			AssertEquals("When enabled, a board will calculate capacity for any resource channel that needs capacity calculation but does not have an existing cached value. This can be disabled to reduce load on database servers, however some channels may not have capacity data to show until the Release Gate runs for them.", boardsAutoCalculateCapacityWhenNotInCache.Hint);
			AssertEquals("Has the right flags", RegistryStorageFlags.System, boardsAutoCalculateCapacityWhenNotInCache.Storage);
			AssertEquals("Is Only For Support", RegistryOptions.IsOnlyForSupport, boardsAutoCalculateCapacityWhenNotInCache.Options);
			AssertEquals("Default Value true!", true, boardsAutoCalculateCapacityWhenNotInCache.DefaultValue);
		}

		public void TestEnableMENTSections()
		{
			AssertEquals("EnableMENTSections", ItemSet.EnableMENTSections.Name);
			AssertEquals("Location of registry item", Categories.WorkflowManager_BufferManagement_VisualBoards, ItemSet.EnableMENTSections.Category);
			AssertEquals("Enable MENT Sections on Visual Boards", ItemSet.EnableMENTSections.Caption);
			AssertEquals("Description of registry item", "Enable MENT Sections on Visual Boards.", ItemSet.EnableMENTSections.Hint);
			AssertEquals("Registry Storage Flag", RegistryStorageFlags.System, ItemSet.EnableMENTSections.Storage);
			AssertEquals("Default Value", false, ItemSet.EnableMENTSections.DefaultValue);
		}

		public void TestDeferralReasons()
		{
			AssertEquals("DeferralReasons", ItemSet.DeferralReasons.Name);
			AssertEquals("Location of registry item", Categories.WorkflowManager_BufferManagement, ItemSet.DeferralReasons.Category);
			AssertEquals("Caption", "Deferral Reasons", ItemSet.DeferralReasons.Caption);
			AssertEquals("Description", "Reasons for deferring a workflow.", ItemSet.DeferralReasons.Hint);
			AssertEquals("Registry Storage Flag", RegistryStorageFlags.System, ItemSet.DeferralReasons.Storage);

			AssertEquals(4, ItemSet.DeferralReasons.DefaultValue.Count);
			AssertEquals(true, ItemSet.DeferralReasons.DefaultValue.CodesAsString.Contains("PRI"));
			AssertEquals(true, ItemSet.DeferralReasons.DefaultValue.CodesAsString.Contains("EST"));
			AssertEquals(true, ItemSet.DeferralReasons.DefaultValue.CodesAsString.Contains("LEV"));
			AssertEquals(true, ItemSet.DeferralReasons.DefaultValue.CodesAsString.Contains("REL"));
		}

		public void TestPAVEOnTheWeb()
		{
			var paveOnTheWeb = ItemSet.PAVEOnTheWeb;

			AssertEquals("PAVEOnTheWeb", paveOnTheWeb.Name);
			AssertEquals("Correct location", Categories.WorkflowManager_BufferManagement, paveOnTheWeb.Category);
			AssertEquals("Enable PAVE On The Web", paveOnTheWeb.Caption);
			AssertEquals("Allows users to perform PAVE operations on the Web using GLOW.", paveOnTheWeb.Hint);
			AssertEquals("Has the right flags", RegistryStorageFlags.System, paveOnTheWeb.Storage);
			AssertEquals("Permissions", RegistryOptions.IsOnlyForSupport, paveOnTheWeb.Options);
			AssertEquals("Default Value!", false, paveOnTheWeb.DefaultValue);
		}

		public void TestRequireReleaseGroup()
		{
			TestRegistryItem(ItemSet.RequireReleaseGroup, "RequireReleaseGroup", BMSRegistry.Categories.WorkflowManager_BufferManagement, "Require Release Group", "Specify whether Release Groups are mandatory on workflows. If activated, a validation error will be applied if no Release Group is present and groups are defined, otherwise a warning will be applied.", RegistryStorageFlags.System, true);
		}

		public void TestReleaseSequencesModuleEnabled()
		{
			AssertEquals("ReleaseSequencesModuleEnabled", ItemSet.ReleaseSequencesModuleEnabled.Name);
			AssertEquals("Location of registry item", Categories.WorkflowManager_BufferManagement_ReleaseSequences, ItemSet.ReleaseSequencesModuleEnabled.Category);
			AssertEquals("Release Sequences Module Enabled", ItemSet.ReleaseSequencesModuleEnabled.Caption);
			AssertEquals("Description of registry item", "Specify whether Release Sequences Module is enabled.", ItemSet.ReleaseSequencesModuleEnabled.Hint);
			AssertEquals("Initial value should be FALSE", false, ItemSet.ReleaseSequencesModuleEnabled.Value);
			AssertEquals("Default value should be FALSE", false, ItemSet.ReleaseSequencesModuleEnabled.DefaultValue);
			AssertEquals("Has the right flags", RegistryStorageFlags.System, ItemSet.ReleaseSequencesModuleEnabled.Storage);
			AssertEquals("Is Only For Support", RegistryOptions.IsOnlyForSupport, ItemSet.ReleaseSequencesModuleEnabled.Options);
			AssertEquals("RegistryOptions is Default", RegistryOptions.Default, ItemSet.NotificationThreshold.Options);
		}

		public void TestReleaseSequenceDefaultNudge()
		{
			AssertEquals("ReleaseSequenceDefaultNudge", ItemSet.ReleaseSequenceDefaultNudge.Name);
			AssertEquals("Location of registry item", Categories.WorkflowManager_BufferManagement_ReleaseSequences, ItemSet.ReleaseSequenceDefaultNudge.Category);
			AssertEquals("Release Sequence Default Nudge", ItemSet.ReleaseSequenceDefaultNudge.Caption);
			AssertEquals("Description of registry item", "Specify Release Sequence Default Nudge.", ItemSet.ReleaseSequenceDefaultNudge.Hint);
			AssertEquals("Initial value should be 1000", 1000, ItemSet.ReleaseSequenceDefaultNudge.Value);
			AssertEquals("Default value should be 1000", 1000, ItemSet.ReleaseSequenceDefaultNudge.DefaultValue);
			AssertEquals("Has the right flags", RegistryStorageFlags.System, ItemSet.ReleaseSequenceDefaultNudge.Storage);
			AssertEquals("Is Only For Support", RegistryOptions.IsOnlyForSupport, ItemSet.ReleaseSequenceDefaultNudge.Options);
			AssertEquals("RegistryOptions is Default", RegistryOptions.Default, ItemSet.NotificationThreshold.Options);
		}

		public void TestDefectTagPKAsAccessedThroughRegistryProvider()
		{
			var tagGroup1 = BMSTestHelper.CreateTagDefinition(Factory, "G1");
			var tagGroup2 = BMSTestHelper.CreateTagDefinition(Factory, "G2");
			var tag1 = BMSTestHelper.CreateTagMagnitude(tagGroup1, "FIX");
			var tag2 = BMSTestHelper.CreateTagMagnitude(tagGroup2, "FIX");

			Factory.Save();
			var registry = ObjectFactory.Get<IBMSRegistry>();

			registry.DefectTagPK = tag1.PK.ToGuid();
			AssertEquals(tag1.PK.ToGuid(), registry.DefectTagPK);
			AssertEquals(tag1.PK.ToGuid(), ItemSet.DefectTagPK.Value);

			registry.DefectTagPK = tag2.PK.ToGuid();
			AssertEquals(tag2.PK.ToGuid(), registry.DefectTagPK);
			AssertEquals(tag2.PK.ToGuid(), ItemSet.DefectTagPK.Value);
		}

		public void TestDefectTagPKFindBoxCollection()
		{
			AssertEquals(RegistryFindBoxCollection.TagMagnitude, ((GuidFindBoxRegistryEditorInfo)ItemSet.DefectTagPK.EditorInfo).FindBoxCollection);
		}

		public void TestDisableCapacityCalculations_WhenEnabled_ShouldDisableServiceTasks()
		{
			var serviceTaskCodes = new[] { "BMG", "BMC", "BMD", "BMT" };
			var governorMock = new Mock<IServiceManagerGovernor>();
			var querierMock = new Mock<IServiceManagerQuerier>();

			querierMock
				.Setup(q => q.GetServiceTasksByCategory(BMSServiceTaskBase.Category))
				.Returns(serviceTaskCodes);

			using (ObjectFactory.Substitute(querierMock.Object))
			using (ObjectFactory.Substitute(governorMock.Object))
			{
				BMSTestHelper.EnableBMSInRegistry();
				AssertCapacityRelatedServiceTasksEnabled(null, Times.Never);
				AssertEquals(false, BMSRegistry.Instance.DisableCapacityCalculations.Value);

				SetRegistryItemToInvokeOnUpdateAction(BMSRegistry.Instance.DisableCapacityCalculations, true);
				AssertCapacityRelatedServiceTasksEnabled(false, Times.Once);

				SetRegistryItemToInvokeOnUpdateAction(BMSRegistry.Instance.DisableCapacityCalculations, false);
				AssertCapacityRelatedServiceTasksEnabled(null, Times.Never); // turning off the registry item shouldn't re-enable them.
			}

			void AssertCapacityRelatedServiceTasksEnabled(bool? shouldBeEnabled, Func<Times> timesInvoked)
			{
				var activeInactive = "not updated";
				if (shouldBeEnabled.HasValue)
				{
					activeInactive = shouldBeEnabled.Value ? "active" : "inactive";
				}
				var expectedBool = shouldBeEnabled ?? It.IsAny<bool>();

				AssertNoExceptionThrown(() =>
				{
					foreach (var task in serviceTaskCodes)
					{
						governorMock.Verify(g => g.SetServiceTaskIsActive(task, expectedBool), timesInvoked, $"We expected all four service tasks (BMG, BMC, BMD, and BMT) to be {activeInactive}.");
					}
				});

				governorMock.Reset();
			}
		}

		public void TestAlwaysViewWorkflowManagementTab()
		{
			AssertEquals("Name", "AlwaysViewWorkflowManagementTab", ItemSet.AlwaysViewWorkflowManagementTab.Name);
			AssertEquals("Category", Categories.WorkflowManager, ItemSet.AlwaysViewWorkflowManagementTab.Category);
			AssertEquals("Caption", "Always View Workflow Management Tab", ItemSet.AlwaysViewWorkflowManagementTab.Caption);
			AssertEquals("Description", "When enabled, Workflow Management tab is visible.", ItemSet.AlwaysViewWorkflowManagementTab.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.AlwaysViewWorkflowManagementTab.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.AlwaysViewWorkflowManagementTab.Options);
			AssertEquals("Default Value", false, ItemSet.AlwaysViewWorkflowManagementTab.DefaultValue);
		}

		public void TestAutomaticallyValidateWorkflowLoopsOnSave()
		{
			AssertEquals("Name", "AutomaticallyValidateWorkflowLoopsOnSave", ItemSet.AutomaticallyValidateWorkflowLoopsOnSave.Name);
			AssertEquals("Category", Categories.WorkflowManager_BufferManagement, ItemSet.AutomaticallyValidateWorkflowLoopsOnSave.Category);
			AssertEquals("Caption", "Automatically Validate Workflow Loops on Save", ItemSet.AutomaticallyValidateWorkflowLoopsOnSave.Caption);
			AssertEquals("Description", "When enabled, validation to check for workflow dependency and logical loops will occur every time a workflow is saved. This can be slow for large workflow networks, even if the workflow doesn't appear on a network diagram. When disabled, this validation will not occur on save. It can be run manually using the 'Validate Workflow Loops' action on a Network Diagram or from the context menu of the Workflows grid in Workflow & Tracking -> Management.", ItemSet.AutomaticallyValidateWorkflowLoopsOnSave.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.AutomaticallyValidateWorkflowLoopsOnSave.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.AutomaticallyValidateWorkflowLoopsOnSave.Options);
			AssertEquals("Default Value", false, ItemSet.AutomaticallyValidateWorkflowLoopsOnSave.DefaultValue);
		}

		public void TestTimeBeforeDeletingOldScheduledTasks()
		{
			AssertEquals("Name", "TimeBeforeDeletingOldScheduledTasks", ItemSet.TimeBeforeDeletingOldScheduledTasks.Name);
			AssertEquals("Category", Categories.WorkflowManager_BufferManagement_ServiceTasks, ItemSet.TimeBeforeDeletingOldScheduledTasks.Category);
			AssertEquals("Caption", "Time Before Deleting Old Scheduled Tasks", ItemSet.TimeBeforeDeletingOldScheduledTasks.Caption);
			AssertEquals("Description", "Any closed tasks in the Time Action Schedule that are at least this old (in months) will be deleted whenever TAS is executed. If set to 0, executed actions will be deleted immediately and not saved as Closed.", ItemSet.TimeBeforeDeletingOldScheduledTasks.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.TimeBeforeDeletingOldScheduledTasks.Storage);
			AssertEquals("Options", RegistryOptions.Default, ItemSet.TimeBeforeDeletingOldScheduledTasks.Options);
			AssertEquals("Default Value", 0, ItemSet.TimeBeforeDeletingOldScheduledTasks.DefaultValue);
		}

		public void TestDisplayResponsiveReleaseGateUiSettings()
		{
			AssertEquals("Name", "DisplayResponsiveReleaseGateUiSettings", ItemSet.DisplayResponsiveReleaseGateUiSettings.Name);
			AssertEquals("Category", Categories.WorkflowManager_BufferManagement_ReleaseGate, ItemSet.DisplayResponsiveReleaseGateUiSettings.Category);
			AssertEquals("Caption", "Display Responsive Release Gate UI Settings", ItemSet.DisplayResponsiveReleaseGateUiSettings.Caption);
			AssertEquals("Description", "When enabled, the UI settings for the responsive release gate will be displayed.", ItemSet.DisplayResponsiveReleaseGateUiSettings.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, ItemSet.DisplayResponsiveReleaseGateUiSettings.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyEditableBySupportIfHosted, ItemSet.DisplayResponsiveReleaseGateUiSettings.Options);
			AssertEquals("Default Value", false, ItemSet.DisplayResponsiveReleaseGateUiSettings.DefaultValue);
		}

		#region Responsive PAVE Data Processing

		public void TestEnableSynchronousPAVEDataProcessing()
		{
			var enableSynchronousPAVEDataProcessing = ItemSet.EnableSynchronousPAVEDataProcessing;

			AssertEquals("EnableSynchronousPAVEDataProcessing", enableSynchronousPAVEDataProcessing.Name);
			AssertEquals("Correct location", Categories.WorkflowManager_BufferManagement_ResponsivePAVEDataProcessing, enableSynchronousPAVEDataProcessing.Category);
			AssertEquals("Enable Synchronous PAVE Data Processing", enableSynchronousPAVEDataProcessing.Caption);
			AssertEquals(@"Specify whether workflows should be transferred synchronously when saving.", enableSynchronousPAVEDataProcessing.Hint);
			AssertEquals("Has the right flags", RegistryStorageFlags.System, enableSynchronousPAVEDataProcessing.Storage);
			AssertEquals("Is Only For Support", RegistryOptions.IsOnlyEditableBySupportIfHosted, enableSynchronousPAVEDataProcessing.Options);
			AssertEquals("Disabled by default", false, enableSynchronousPAVEDataProcessing.DefaultValue);
		}

		public void TestEnableResponsivePAVEDataProcessing()
		{
			var enableResponsivePAVEDataProcessing = ItemSet.EnableResponsivePAVEDataProcessing;

			AssertEquals("EnableResponsivePAVEDataProcessing", enableResponsivePAVEDataProcessing.Name);
			AssertEquals("Correct location", Categories.WorkflowManager_BufferManagement_ResponsivePAVEDataProcessing, enableResponsivePAVEDataProcessing.Category);
			AssertEquals("Enable Responsive PAVE Data Processing", enableResponsivePAVEDataProcessing.Caption);
			AssertEquals(@"Specify whether to enable responsive PAVE data processing (such as responsive workflow transfer and alike).", enableResponsivePAVEDataProcessing.Hint);
			AssertEquals("Has the right flags", RegistryStorageFlags.System, enableResponsivePAVEDataProcessing.Storage);
			AssertEquals("Is Only For Support", RegistryOptions.IsOnlyEditableBySupportIfHosted, enableResponsivePAVEDataProcessing.Options);
			AssertEquals("Enabled by default", true, enableResponsivePAVEDataProcessing.DefaultValue);
		}

		#region Transfer

		public void TestDynamicallyFilterTransferRules()
		{
			var dynamicallyFilterTransferRules = ItemSet.DynamicallyFilterTransferRules;

			AssertEquals("DynamicallyFilterTransferRules", dynamicallyFilterTransferRules.Name);
			AssertEquals("Correct location", Categories.WorkflowManager_BufferManagement_ResponsivePAVEDataProcessing, dynamicallyFilterTransferRules.Category);
			AssertEquals("Dynamically filter transfer rules for BMS service Task", dynamicallyFilterTransferRules.Caption);
			AssertEquals(@"When enabled transfer rules will be dynamically filtered and workflow related filters won't be run through the BMS service task. Only through Responsive Data Processing.
When disabled the BMS service task will not dynamically filter which Transfer Rules to run and will instead run all transfer rules. This will increase server resource consumption.", dynamicallyFilterTransferRules.Hint);
			AssertType<DynamicallyFilterTransferRulesRegistryEditorInfo>("Has the editor info", dynamicallyFilterTransferRules.EditorInfo);
			AssertEquals("Has the right flags", RegistryStorageFlags.System, dynamicallyFilterTransferRules.Storage);
			AssertEquals("Is Only For Support", RegistryOptions.IsOnlyEditableBySupportIfHosted, dynamicallyFilterTransferRules.Options);
			AssertEquals("Enabled by default", true, dynamicallyFilterTransferRules.DefaultValue);
		}

		public void TestProcessAllTransferRulesLinksOnNextBMSRun()
		{
			var processAllTransferRulesLinksOnNextBMSRun = ItemSet.ProcessAllTransferRulesLinksOnNextBMSRun;

			AssertEquals("ProcessAllTransferRulesLinksOnNextBMSRun", processAllTransferRulesLinksOnNextBMSRun.Name);
			AssertEquals("Correct location", Categories.WorkflowManager_BufferManagement_ResponsivePAVEDataProcessing, processAllTransferRulesLinksOnNextBMSRun.Category);
			AssertEquals("ProcessAllTransferRulesLinksOnNextBMSRun", processAllTransferRulesLinksOnNextBMSRun.Caption);
			AssertEquals("Ignore filtering and process all transfer rules next time BMS runs.", processAllTransferRulesLinksOnNextBMSRun.Hint);
			AssertEquals("Has the right flags", RegistryStorageFlags.System, processAllTransferRulesLinksOnNextBMSRun.Storage);
			AssertEquals("Is Hidden and Not Cached", RegistryOptions.IsHidden | RegistryOptions.NotCached, processAllTransferRulesLinksOnNextBMSRun.Options);
			AssertEquals("Enabled by default", false, processAllTransferRulesLinksOnNextBMSRun.DefaultValue);
		}

		public void TestTransferWorkflowComponentOnChanges()
		{
			var transferWorkflowComponentOnChanges = ItemSet.TransferWorkflowComponentOnChanges;

			AssertEquals("TransferWorkflowComponentOnChanges", transferWorkflowComponentOnChanges.Name);
			AssertEquals("Correct location", Categories.WorkflowManager_BufferManagement_ResponsivePAVEDataProcessing, transferWorkflowComponentOnChanges.Category);
			AssertEquals("Transfer Workflow Component On Changes", transferWorkflowComponentOnChanges.Caption);
			AssertEquals(@"When enabled, the system will track changes in workflows and transfer workflows between components immediately.", transferWorkflowComponentOnChanges.Hint);
			AssertEquals("Has the right flags", RegistryStorageFlags.System, transferWorkflowComponentOnChanges.Storage);
			AssertEquals("Is Only For Support", RegistryOptions.IsOnlyEditableBySupportIfHosted, transferWorkflowComponentOnChanges.Options);
			AssertEquals("Enabled by default", true, transferWorkflowComponentOnChanges.DefaultValue);
		}

		public void TestTransferWorkflowComponentMaximumNumberOfCDCChanges()
		{
			var transferWorkflowComponentMaximumNumberOfCDCChanges = ItemSet.TransferWorkflowComponentMaximumNumberOfCDCChanges;

			AssertEquals("TransferWorkflowComponentMaximumNumberOfCDCChanges", transferWorkflowComponentMaximumNumberOfCDCChanges.Name);
			AssertEquals(Categories.WorkflowManager_BufferManagement_ResponsivePAVEDataProcessing, transferWorkflowComponentMaximumNumberOfCDCChanges.Category);
			AssertEquals("Transfer Workflow Component Maximum Number of Changes to Process through BI CDC", transferWorkflowComponentMaximumNumberOfCDCChanges.Caption);
			AssertEquals(@"Specifies a maximum number of changes to processed through CDC for workflow transfer. If the number of changes is greater than this value the changes will be ignored and will be processed through the Buffer Management Schematic Transfer Runner service task instead.

Please, before changing it, contact the PAVE team for a better evaluation.", transferWorkflowComponentMaximumNumberOfCDCChanges.Hint);
			AssertEquals("Has the right flags", RegistryStorageFlags.System, transferWorkflowComponentMaximumNumberOfCDCChanges.Storage);
			AssertEquals("Is Only For Support", RegistryOptions.IsOnlyEditableBySupportIfHosted, transferWorkflowComponentMaximumNumberOfCDCChanges.Options);
			AssertEquals("Default Value is 1000", 1000, transferWorkflowComponentMaximumNumberOfCDCChanges.DefaultValue);
		}

		#endregion

		#region Auto-Assignment

		public void TestAutoAssignmentCapabilityTasksFailure()
		{
			var autoAssignmentCapabilityTasksFailure = ItemSet.AutoAssignmentCapabilityTasksFailure;

			AssertEquals("AutoAssignmentCapabilityTasksFailure", autoAssignmentCapabilityTasksFailure.Name);
			AssertEquals("Correct location", Categories.WorkflowManager_BufferManagement_ServiceTasks, autoAssignmentCapabilityTasksFailure.Category);
			AssertEquals("Auto Assignment Capability Tasks Failure", autoAssignmentCapabilityTasksFailure.Caption);
			AssertEquals(@"Automatically create a Work Item (matching the specified template) to aid the visibility of tasks auto-assignment failure due to capability / release group misconfiguration.", autoAssignmentCapabilityTasksFailure.Hint);
			AssertEquals("Has the right flags", RegistryStorageFlags.System, autoAssignmentCapabilityTasksFailure.Storage);
			AssertEquals("Disabled by default", false, autoAssignmentCapabilityTasksFailure.DefaultValue.Enabled);
			AssertEquals("Empty by default", ZString.Empty, autoAssignmentCapabilityTasksFailure.DefaultValue.Criterion1);
			AssertEquals("Empty by default", ZString.Empty, autoAssignmentCapabilityTasksFailure.DefaultValue.Criterion2);
			AssertEquals("Empty by default", ZString.Empty, autoAssignmentCapabilityTasksFailure.DefaultValue.Criterion3);
			AssertEquals("Empty by default", ZString.Empty, autoAssignmentCapabilityTasksFailure.DefaultValue.Criterion4);
			AssertEquals("Empty by default", ZString.Empty, autoAssignmentCapabilityTasksFailure.DefaultValue.Criterion5);
		}

		public void TestAutoAssignCapabilityTasksOnChanges()
		{
			var autoAssignCapabilityTasksOnChanges = ItemSet.AutoAssignCapabilityTasksOnChanges;

			AssertEquals("AutoAssignCapabilityTasksOnChanges", autoAssignCapabilityTasksOnChanges.Name);
			AssertEquals("Correct location", Categories.WorkflowManager_BufferManagement_ResponsivePAVEDataProcessing, autoAssignCapabilityTasksOnChanges.Category);
			AssertEquals("Auto Assign Capability Tasks On Changes", autoAssignCapabilityTasksOnChanges.Caption);
			AssertEquals(@"When enabled, the system will track changes in workflows and auto assign capability tasks immediately if needed (or schedule the auto assignment for later if it is too early for auto assignment).", autoAssignCapabilityTasksOnChanges.Hint);
			AssertEquals("Has the right flags", RegistryStorageFlags.System, autoAssignCapabilityTasksOnChanges.Storage);
			AssertEquals("Is Only Editable By Support If Hosted", RegistryOptions.IsOnlyEditableBySupportIfHosted, autoAssignCapabilityTasksOnChanges.Options);
			AssertEquals("Enabled by default", true, autoAssignCapabilityTasksOnChanges.DefaultValue);
		}

		public void TestAutoAssignCapabilityTasksMaxNumberOfCDCChanges()
		{
			var maxNumberOfCDCChangesToProcessForCapabilityAssignment = ItemSet.AutoAssignCapabilityTasksMaxNumberOfCDCChanges;

			AssertEquals("AutoAssignCapabilityTasksMaxNumberOfCDCChanges", maxNumberOfCDCChangesToProcessForCapabilityAssignment.Name);
			AssertEquals(Categories.WorkflowManager_BufferManagement_ResponsivePAVEDataProcessing, maxNumberOfCDCChangesToProcessForCapabilityAssignment.Category);
			AssertEquals("Auto Assign Capability Tasks Maximum Number of Changes to Process through CDC", maxNumberOfCDCChangesToProcessForCapabilityAssignment.Caption);
			AssertEquals(@"Specifies a maximum number of changes to be processed through CDC for capability task auto assignment. If the number of changes is greater than this value the changes will be ignored and will be processed through the Capability Task Auto-Assignment service task instead.

Please, before changing it, contact the PAVE team for a better evaluation.", maxNumberOfCDCChangesToProcessForCapabilityAssignment.Hint);
			AssertEquals("Has the right flags", RegistryStorageFlags.System, maxNumberOfCDCChangesToProcessForCapabilityAssignment.Storage);
			AssertEquals("Is Only Editable By Support If Hosted", RegistryOptions.IsOnlyEditableBySupportIfHosted, maxNumberOfCDCChangesToProcessForCapabilityAssignment.Options);
			AssertEquals("Default Value is 1000", 1000, maxNumberOfCDCChangesToProcessForCapabilityAssignment.DefaultValue);
		}

		#endregion

		#endregion

		#region Responsive Release

		#region Workflow Status Update

		public void TestReleaseWorkflowsOnChanges()
		{
			var releaseWorkflowsOnChanges = ItemSet.ReleaseWorkflowsOnChanges;

			AssertEquals("ReleaseWorkflowsOnChanges", releaseWorkflowsOnChanges.Name);
			AssertEquals("Correct location", Categories.WorkflowManager_BufferManagement_ResponsivePAVEDataProcessing, releaseWorkflowsOnChanges.Category);
			AssertEquals("Release Workflows On Changes", releaseWorkflowsOnChanges.Caption);
			AssertEquals(@"When enabled, the system will track changes in workflows and release eligible workflows immediately.", releaseWorkflowsOnChanges.Hint);
			AssertEquals("Has the right flags", RegistryStorageFlags.System, releaseWorkflowsOnChanges.Storage);
			AssertEquals("Is Only Editable By Support If Hosted", RegistryOptions.IsOnlyEditableBySupportIfHosted, releaseWorkflowsOnChanges.Options);
			AssertEquals("Enabled by default", true, releaseWorkflowsOnChanges.DefaultValue);
		}

		#endregion

		#endregion

		#region Experimental

		public void TestEnablePaveExperimentalFeatures()
		{
			var enablePaveExperimentalFeatures = ItemSet.EnablePaveExperimentalFeatures;

			AssertEquals("EnablePaveExperimentalFeatures", enablePaveExperimentalFeatures.Name);
			AssertEquals("Correct location", Categories.WorkflowManager_BufferManagement_Experimental, enablePaveExperimentalFeatures.Category);
			AssertEquals("Enable PAVE Experimental Features (Global)", enablePaveExperimentalFeatures.Caption);
			AssertEquals(@"Specify whether to enable the new PAVE experimental features. This is a global setting that affects the ability to specify PAVE experimental settings per individual BM System / Board.

Please contact the PAVE team before enabling this.", enablePaveExperimentalFeatures.Hint);
			AssertEquals("Has the right flags", RegistryStorageFlags.System, enablePaveExperimentalFeatures.Storage);
			AssertEquals("Is Only For Support", RegistryOptions.IsOnlyForSupport, enablePaveExperimentalFeatures.Options);
			AssertEquals("Default Value false!", false, enablePaveExperimentalFeatures.DefaultValue);
		}

		public void TestIgnoreIterationsWhenCalculatingStartability()
		{
			var ignoreIterations = ItemSet.IgnoreIterationsWhenCalculatingStartability;

			AssertEquals("IgnoreIterationsWhenCalculatingStartability", ignoreIterations.Name);
			AssertEquals("Correct location", Categories.WorkflowManager_BufferManagement_Experimental, ignoreIterations.Category);
			AssertEquals("Ignore iterations when calculating startability", ignoreIterations.Caption);
			AssertEquals("When enabled, tasks in the original workflow that has containment barrier task will become startable when the containment barrier task is closed, irrespective whether a QI sub-workflow is created or not. This will present startable tasks that belong to the original and iterated workflows.", ignoreIterations.Hint);
			AssertEquals("Has the right flags", RegistryStorageFlags.System, ignoreIterations.Storage);
			AssertEquals("Is Only For Support", RegistryOptions.IsOnlyForSupport, ignoreIterations.Options);
			AssertEquals("Default Value false!", false, ignoreIterations.DefaultValue);
		}

		#endregion

		#region Implementation

		//registry items based on Workflow Types populated from the global workflow descriptors list
		//where Description has a resource string.
		//therefore we need to add some exclusions to allow for resource string loading.
		protected override IEnumerable<string> AllowDemandResourceStrings { get; } = new[]
		{
			nameof(BMSRegistry.WorkflowCategories)
		};

		static void SetRegistryItemToInvokeOnUpdateAction(RegistryItemWrapper item, object valueToSet)
		{
			var tag = new RegistryItemTag(item);
			tag.NewValue = valueToSet;
			tag.IsChanged = true;
			tag.HasValue = true;
			tag.SaveAllValues();
		}

		#endregion
	}
}
