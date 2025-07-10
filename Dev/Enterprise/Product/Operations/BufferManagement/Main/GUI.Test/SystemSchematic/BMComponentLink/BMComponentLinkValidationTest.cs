using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI.Test
{
	class BMComponentLinkValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateFilterStrips_ShouldDisallowEmptyFiltersForNonEntryComponentLinkAndReleaseGateLinks()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2");
			var bucket3 = BMSTestHelper.CreateBucket(system, "bucket3");
			var buffer = BMSTestHelper.CreateBuffer(system);
			var bucket4 = BMSTestHelper.CreateBucket(system, "bucket4");
			var bucket5 = BMSTestHelper.CreateBucket(system, "bucket5");

			var link1 = BMSTestHelper.LinkComponents(bucket1, bucket2, isReleaseGate: false);
			link1.Validation.ValidateAll();
			AssertNoRowErrors("Links from the entry component to another component are OK to have no filters", link1);

			var link2 = BMSTestHelper.LinkComponents(bucket2, bucket3, isReleaseGate: false);
			link2.Validation.ValidateAll();
			AssertHasRowError(link2, BMComponentLinkValidation.FilterRequiredError);

			var link3 = BMSTestHelper.LinkComponents(bucket3, buffer, isReleaseGate: true);
			link3.Validation.ValidateAll();
			AssertNoRowErrors("Links into buffers which are marked as 'release gate' links are OK to have no filters", link3);

			var link4 = BMSTestHelper.LinkComponents(buffer, bucket4, isReleaseGate: false);
			link4.Validation.ValidateAll();
			AssertHasRowError(link4, BMComponentLinkValidation.FilterRequiredError);

			var link5 = BMSTestHelper.LinkComponents(bucket4, bucket5, isActive: false);
			link5.Validation.ValidateAll();
			AssertNoRowErrors("Inactive links should not be validated. They're allowed to have no filters because they might be placeholders that aren't finished being configured yet.", link5);

			link2.FL_TransferRulesEnabled = false;
			system.RunPreSaveValidation();

			AssertNoErrors("Link is inactive, therefore no message errors.", link2.FL_IsReleaseGateRuleAppliedInfo);
			AssertHasRowError(link4, BMComponentLinkValidation.FilterRequiredError);

			AddLinkFilterForTest(link4.FilterRule, "Test");
			link4.Validation.ValidateAll();
			AssertNoRowError(link4, BMComponentLinkValidation.FilterRequiredError);

			link3.FL_IsReleaseGateRuleApplied = false;
			link3.Validation.ValidateAll();
			AssertHasRowError(link3, BMComponentLinkValidation.FilterRequiredError);
		}

		public void TestValidateFiltersForEntryComponentLink_InterSystemLinks()
		{
			var system1 = BMSTestHelper.CreateSystem(Factory);
			var inactiveComponent = BMSTestHelper.CreateBucket(system1, "Inactive Component", sequence: 1);
			var componentOne = BMSTestHelper.CreateBucket(system1, "One", sequence: 2);
			var componentTwo = BMSTestHelper.CreateBucket(system1, "Two", sequence: 3);
			var componentThree = BMSTestHelper.CreateBucket(system1, "Three", sequence: 4);
			var componentFour = BMSTestHelper.CreateBuffer(system1, "Four", sequence: 5);
			var componentFive = BMSTestHelper.CreateBucket(system1, "Five", sequence: 6);

			var link1_system1 = BMSTestHelper.LinkComponents(componentOne, componentTwo, isReleaseGate: false);
			var link2_system1 = BMSTestHelper.LinkComponents(componentTwo, componentThree, isReleaseGate: false);
			AddLinkFilterForTest(link2_system1.FilterRule, "Test 1");

			var link3_system1 = BMSTestHelper.LinkComponents(componentThree, componentFour, isReleaseGate: true);
			var link4_system1 = BMSTestHelper.LinkComponents(componentFour, componentFive, isReleaseGate: false);
			AddLinkFilterForTest(link4_system1.FilterRule, "Test 2");
			system1.RunPreSaveValidation();
			Assert("Validate system1: there should be no errors for components and their links", !system1.Notifications.Any(n => n.Type == NotificationType.Error));

			Factory.Save();

			var system2 = BMSTestHelper.CreateSystem(Factory);
			var one = BMSTestHelper.CreateBucket(system2, "1", sequence: 1);
			var two = BMSTestHelper.CreateBucket(system2, "2", sequence: 2);
			var three = BMSTestHelper.CreateBucket(system2, "3", sequence: 3);

			var link1_system2 = BMSTestHelper.LinkComponents(one, two, isReleaseGate: false);
			var link2_system2 = BMSTestHelper.LinkComponents(two, three, isReleaseGate: false);
			AddLinkFilterForTest(link2_system2.FilterRule, "Test 3");

			var link3_system2 = BMSTestHelper.LinkComponents(three, componentOne, isReleaseGate: false);
			AddLinkFilterForTest(link3_system2.FilterRule, "Test 4");

			system2.RunPreSaveValidation();
			Assert("Validate system2: there should be no errors for components and their links", !system2.Notifications.Any(n => n.Type == NotificationType.Error));
			Factory.Save();

			system1.RunPreSaveValidation();
			Assert("Revalidate system1: there should be no errors for components and their links", !system1.Notifications.Any(n => n.Type == NotificationType.Error));
		}

		public void TestValidateFilterStrips_WithUserDefinedFilter_ShouldNotHaveErrorRelatedToEmptyFilterLayout()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "INQ");
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket 1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket 2");
			var bucket3 = BMSTestHelper.CreateBucket(system, "bucket 3");
			var link1 = BMSTestHelper.LinkComponents(bucket1, bucket2);
			var link2 = BMSTestHelper.LinkComponents(bucket2, bucket3);

			link2.Validation.ValidateAll();
			AssertHasRowError(link2, "Filters are required for all active links, except a link between the entry component and another component, and links to buffer components marked as a 'release gate' link.");

			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.ProcessHeader))
			{
				module.FilterBusinessObject.AddTextFilterStrip(ProcessHeader.ModuleFilterConstants.CompletionStatement, "Squancy");
				FilterStripsTestHelper.SaveFilterLayout(module.FilterBusinessObject, "Squanchy filters inside", isPublished: true, isPublishedGlobal: true, isUserDefinedFilter: true);
			}

			FilterStripsTestHelper.AddFilterStrip<ModuleUserDefinedFilter>(link2.FilterRule, "[USR]Squanchy filters inside");

			link2.Validation.ValidateAll();
			AssertNoRowErrors("Adding a user-defined filter should remove the validation error. SAD!", link2);
		}

		void AddLinkFilterForTest(StmModuleFilter filterRule, ZString propertyValue)
		{
			FilterStripsTestHelper.AddFilterStrips(filterRule,
			new FilterStripsTestHelper.FilterStripDefinition
			{
				FilterStripName = ProcessHeader.ModuleFilterConstants.CompletionStatement,
				FilterStripValueSetter = f => ((ModuleTextFilter)f).Property = propertyValue,
			});
		}

		public void TestIsReleaseGate_ShouldBeAllowedForBuffersOnly()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket1 = BMSTestHelper.CreateBucket(system, "bucket1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "bucket2");
			var buffer = BMSTestHelper.CreateBuffer(system);

			var link = BMSTestHelper.LinkComponents(bucket1, bucket2, isReleaseGate: false);
			AssertNoErrors(link.FL_IsReleaseGateRuleAppliedInfo);

			link.FL_IsReleaseGateRuleApplied = true;
			AssertHasError(link.FL_IsReleaseGateRuleAppliedInfo, "Only links to a Buffer component can be marked as part of a Release Gate.");

			link.FL_FC_ComponentTo = buffer.PK;
			AssertNoErrors(link.FL_IsReleaseGateRuleAppliedInfo);
		}

		public void TestIsReleaseGate_WhenDisableCapacityCalculationsEnabled_ShouldShowError()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system);

			var link = BMSTestHelper.LinkComponents(bucket, buffer, isReleaseGate: true);
			AssertNoErrors(link.FL_IsReleaseGateRuleAppliedInfo);

			BMSRegistry.Instance.DisableCapacityCalculations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			link.Validation.ValidateAll();
			AssertHasError(link.FL_IsReleaseGateRuleAppliedInfo, "Release Gate links are not available when the [Workflow Manager -> Buffer Management -> Release Gate -> Disable Capacity Calculations] registry item is enabled.");

			link.FL_IsReleaseGateRuleApplied = false;
			AssertNoErrors(link.FL_IsReleaseGateRuleAppliedInfo);
		}

		public void TestIsReleaseGate_WhenDisableCapacityCalculationsEnabled_AndDestinationIsBucket_ShouldShowBothErrors()
		{
			BMSRegistry.Instance.DisableCapacityCalculations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket1 = BMSTestHelper.CreateBucket(system, "Bucket 1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "Bucket 2");

			var link = BMSTestHelper.LinkComponents(bucket1, bucket2, isReleaseGate: true);
			AssertHasError("It's not a big deal, but it's good for the user to know all the things they need to fix to make this work.", link.FL_IsReleaseGateRuleAppliedInfo,
				"Release Gate links are not available when the [Workflow Manager -> Buffer Management -> Release Gate -> Disable Capacity Calculations] registry item is enabled.");
			AssertHasError("It's not a big deal, but it's good for the user to know all the things they need to fix to make this work.", link.FL_IsReleaseGateRuleAppliedInfo,
				"Only links to a Buffer component can be marked as part of a Release Gate.");
		}

		public void TestCannotApplyReleaseGateRules_WhenInEnhancedWorkflowMode()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.BasicWorkflow);

			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket = BMSTestHelper.CreateBucket(system);
			var buffer = BMSTestHelper.CreateBuffer(system);
			var link = BMSTestHelper.LinkComponents(bucket, buffer);

			link.FL_IsReleaseGateRuleApplied = true;
			link.Validation.ValidateFL_IsReleaseGateRuleApplied();
			AssertNoErrors("There's no need to validate for releaese gate links when in Basic Workflow mode, since BM systems cannot be defined and/or are inoperable at this level", link.FL_IsReleaseGateRuleAppliedInfo);

			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.EnhancedWorkflow);
			link.Validation.ValidateFL_IsReleaseGateRuleApplied();
			AssertHasError(link.FL_IsReleaseGateRuleAppliedInfo, "Release Gate links are not available when the registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] is set to EWF - Enhanced Workflow Management.");

			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);
			link.Validation.ValidateFL_IsReleaseGateRuleApplied();
			AssertNoErrors("We should have no trouble defining a release gate in Buffer Management mode", link.FL_IsReleaseGateRuleAppliedInfo);

			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.PlanningManagement);
			link.Validation.ValidateFL_IsReleaseGateRuleApplied();
			AssertNoErrors("We should have no trouble defining a release gate in Planning Management mode", link.FL_IsReleaseGateRuleAppliedInfo);
		}

		public void TestFL_FC_ComponentFrom()
		{
			var system = Factory.New<BMSystem>();
			var component1 = system.Components.AddNew();
			var component2 = system.Components.AddNew();
			var component3 = system.Components.AddNew();
			var link2_1 = component1.FromOthersToMeLinks.AddNew();

			//From and To must be different
			link2_1.FL_FC_ComponentFrom = component1.PK;
			AssertHasErrors(link2_1.FL_FC_ComponentFromInfo);
			AssertNoErrors(link2_1.FL_FC_ComponentToInfo);

			link2_1.FL_FC_ComponentFrom = component2.PK;
			AssertNoErrors(link2_1.FL_FC_ComponentFromInfo);
			AssertNoErrors(link2_1.FL_FC_ComponentToInfo);

			// It's perfectly valid (now) to have two links from one component to another
			var anotherLink = component1.FromOthersToMeLinks.AddNew();
			anotherLink.FL_FC_ComponentFrom = component2.PK;
			AssertNoErrors(anotherLink.FL_FC_ComponentFromInfo);
			AssertNoErrors(anotherLink.FL_FC_ComponentToInfo);

			anotherLink.FL_FC_ComponentFrom = component3.PK;
			AssertNoErrors(anotherLink.FL_FC_ComponentFromInfo);
			AssertNoErrors(anotherLink.FL_FC_ComponentToInfo);
		}

		public void TestFL_FC_ComponentTo()
		{
			var system = Factory.New<BMSystem>();
			var component1 = system.Components.AddNew();
			var component2 = system.Components.AddNew();
			var component3 = system.Components.AddNew();
			var componentLinkFrom1To2 = component1.FromMeToOthersLinks.AddNew();

			//From and To must be different
			componentLinkFrom1To2.FL_FC_ComponentTo = component1.PK;
			AssertNoErrors(componentLinkFrom1To2.FL_FC_ComponentFromInfo);
			AssertHasErrors(componentLinkFrom1To2.FL_FC_ComponentToInfo);

			componentLinkFrom1To2.FL_FC_ComponentTo = component2.PK;
			AssertNoErrors(componentLinkFrom1To2.FL_FC_ComponentFromInfo);
			AssertNoErrors(componentLinkFrom1To2.FL_FC_ComponentToInfo);

			// It's perfectly valid (now) to have two links from one component to another
			var anotherLink = component1.FromMeToOthersLinks.AddNew();
			anotherLink.FL_FC_ComponentTo = component2.PK;
			AssertNoErrors(anotherLink.FL_FC_ComponentFromInfo);
			AssertNoErrors(anotherLink.FL_FC_ComponentToInfo);

			anotherLink.FL_FC_ComponentTo = component3.PK;
			AssertNoErrors(anotherLink.FL_FC_ComponentFromInfo);
			AssertNoErrors(anotherLink.FL_FC_ComponentToInfo);
		}

		public void TestComponentToSystemPKValidation()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			system.FS_Name = "System";
			system.FS_Description = "Description";

			var otherSystem = Factory.NewWithValidTestData<BMSystem>();
			otherSystem.FS_Name = "Not System";
			otherSystem.FS_Description = "Not Description";

			var component21 = BMSTestHelper.CreateBucket(otherSystem, "Component21");
			var component22 = BMSTestHelper.CreateBucket(otherSystem, "Component22");

			var componentLink21 = BMSTestHelper.LinkComponents(component21, component22);

			componentLink21.Validation.ValidateAll();

			AssertNoErrors(componentLink21.ComponentToSystemPKInfo);
			AssertNoErrors(componentLink21.FL_FC_ComponentToInfo);

			componentLink21.ComponentToSystemPK = ZGuid.Invalid;
			AssertHasError(componentLink21.ComponentToSystemPKInfo, "Enter a valid System.");
			AssertHasError(componentLink21.FL_FC_ComponentToInfo, "Enter a valid To Component.");

			componentLink21.ComponentToSystemPK = ZGuid.Empty;
			AssertHasError(componentLink21.ComponentToSystemPKInfo, "Please enter a System.");
			AssertHasError(componentLink21.FL_FC_ComponentToInfo, "Enter a valid To Component.");

			componentLink21.ComponentToSystemPK = system.PK;
			AssertNoErrors(componentLink21.ComponentToSystemPKInfo);
			AssertHasError(componentLink21.FL_FC_ComponentToInfo, "Enter a valid To Component.");

			componentLink21.FL_FC_ComponentTo = component22.PK;
			AssertNoErrors(componentLink21.ComponentToSystemPKInfo);
			AssertNoErrors(componentLink21.FL_FC_ComponentToInfo);
		}
	}
}
