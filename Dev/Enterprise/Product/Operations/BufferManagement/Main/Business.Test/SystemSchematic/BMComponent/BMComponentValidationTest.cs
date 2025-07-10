using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business.Test
{
	class BMComponentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckFC_TaskAssignTaskAgeInvalidFormat()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, "buffer", 200);

			buffer.FC_AutoAssignTasksAge = ZDateTime.Invalid;
			buffer.Validation.ValidateAll();
			AssertHasError(buffer.FC_AutoAssignTasksAgeInfo, "Enter a valid Auto Assign Tasks Age. Correct format should be 000:00.");
		}

		public void TestCheckAgingBranchAndDepartment()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var bucket = BMSTestHelper.CreateBucket(system, "bucket");
			bucket.FC_GB_AgingBranch = ZGuid.Empty;
			bucket.FC_GE_AgingDepartment = ZGuid.Empty;

			AssertNoErrors(bucket.FC_GE_AgingDepartmentInfo);
			AssertNoErrors(bucket.FC_GB_AgingBranchInfo);

			var buffer = BMSTestHelper.CreateBuffer(system, "buffer");
			buffer.FC_GB_AgingBranch = ZGuid.Empty;
			buffer.FC_GE_AgingDepartment = ZGuid.Empty;

			AssertHasError(buffer.FC_GB_AgingBranchInfo, "Aging Branch cannot be empty while component type is a buffer.");
			AssertHasError(buffer.FC_GE_AgingDepartmentInfo, "Aging Department cannot be empty while component type is a buffer.");
		}

		public void TestConstrainedBuffers_HaveSubComponenetBeforeAndAfter()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, "Pillaging england England", 200);

			var preConstraintComponent = BMSTestHelper.CreateBucket(system, name: "Home Town", sequence: 1);
			var constraint = BMSTestHelper.CreateConstraint(buffer, "Sailing to england", offsetMinutes: 100, sequence: 2);
			var postConstraintComponent = BMSTestHelper.CreateBucket(system, name: "Pillage", sequence: 3);

			constraint.Validation.ValidateFC_Type();
			AssertHasWarning("Constraint with no other components", constraint.FC_TypeInfo, "Buffer sub-components should be added before and after the Constraint Component Offset in order to draw attention to items causing risk to Capacity Constrained Resources.");

			buffer.ChildComponents.Add(preConstraintComponent);
			constraint.Validation.ValidateFC_Type();
			AssertHasWarning("Constraint with pre constraint before it", constraint.FC_TypeInfo, "Buffer sub-components should be added before and after the Constraint Component Offset in order to draw attention to items causing risk to Capacity Constrained Resources.");

			buffer.ChildComponents.Add(postConstraintComponent);
			constraint.Validation.ValidateFC_Type();
			AssertHasWarning("Constraint with two components before it", constraint.FC_TypeInfo, "Buffer sub-components should be added before and after the Constraint Component Offset in order to draw attention to items causing risk to Capacity Constrained Resources.");

			postConstraintComponent.FC_OffsetInMinutes = 100;
			constraint.Validation.ValidateFC_Type();
			AssertNoWarnings("Constraint with Post constraint on the same offset amount", constraint);

			preConstraintComponent.FC_OffsetInMinutes = 100;
			constraint.Validation.ValidateFC_Type();
			AssertHasWarning("Constraint with two components after it", constraint.FC_TypeInfo, "Buffer sub-components should be added before and after the Constraint Component Offset in order to draw attention to items causing risk to Capacity Constrained Resources.");
		}

		public void TestConstrainedBuffers_PreAndPostConstraintsLineUp()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, "Pillaging england England", 200);

			var preConstraintComponent = BMSTestHelper.CreateBucket(system, name: "Home Town", sequence: 1);
			var constraint = BMSTestHelper.CreateConstraint(buffer, "Sailing to england", offsetMinutes: 100, sequence: 2);
			var postConstraintComponent = BMSTestHelper.CreateBucket(system, name: "Pillage", sequence: 3);

			constraint.Validation.ValidateFC_Type();
			AssertHasWarning("Constraint - Pre Constraint: None. Post Constraint: None", constraint.FC_TypeInfo, "A pre-constraint buffer should be configured so its end lines up with the Constraint component, and a post-constraint buffer should be configured so its start lines up with the Constraint on the Graphical Schematic. This is to detect when tasks involving a CCR are in a risky state that threatens CCR throughput.");

			buffer.ChildComponents.Add(preConstraintComponent);
			constraint.Validation.ValidateFC_Type();
			preConstraintComponent.FC_BufferTimespanInMinutes = 100;
			AssertHasWarning("Constraint - Pre Constraint: Lines up. Post Constraint: None", constraint.FC_TypeInfo, "A pre-constraint buffer should be configured so its end lines up with the Constraint component, and a post-constraint buffer should be configured so its start lines up with the Constraint on the Graphical Schematic. This is to detect when tasks involving a CCR are in a risky state that threatens CCR throughput.");

			buffer.ChildComponents.Add(postConstraintComponent);
			postConstraintComponent.FC_BufferTimespanInMinutes = 100;
			postConstraintComponent.FC_OffsetInMinutes = 100;
			constraint.Validation.ValidateFC_Type();
			AssertNoWarnings("Constraint - Pre Constraint: Lines up. Post Constraint: Lines up", constraint);

			preConstraintComponent.FC_BufferTimespanInMinutes = 101;
			constraint.Validation.ValidateFC_Type();
			AssertHasWarning("Constraint - Pre Constraint: Goes past constraint. Post Constraint: Lines up", constraint.FC_TypeInfo, "A pre-constraint buffer should be configured so its end lines up with the Constraint component, and a post-constraint buffer should be configured so its start lines up with the Constraint on the Graphical Schematic. This is to detect when tasks involving a CCR are in a risky state that threatens CCR throughput.");

			preConstraintComponent.FC_BufferTimespanInMinutes = 99;
			constraint.Validation.ValidateFC_Type();
			AssertHasWarning("Constraint - Pre Constraint: Does not reach constraint. Post Constraint: Lines up", constraint.FC_TypeInfo, "A pre-constraint buffer should be configured so its end lines up with the Constraint component, and a post-constraint buffer should be configured so its start lines up with the Constraint on the Graphical Schematic. This is to detect when tasks involving a CCR are in a risky state that threatens CCR throughput.");

			preConstraintComponent.FC_BufferTimespanInMinutes = 100;
			constraint.Validation.ValidateFC_Type();
			AssertNoWarnings("Constraint - Pre Constraint: Lines up. Post Constraint: Lines up", constraint);

			postConstraintComponent.FC_OffsetInMinutes = 101;
			constraint.Validation.ValidateFC_Type();
			AssertHasWarning("Constraint - Pre Constraint: Lines up. Post Constraint: Goes past constraint", constraint.FC_TypeInfo, "A pre-constraint buffer should be configured so its end lines up with the Constraint component, and a post-constraint buffer should be configured so its start lines up with the Constraint on the Graphical Schematic. This is to detect when tasks involving a CCR are in a risky state that threatens CCR throughput.");

			postConstraintComponent.FC_OffsetInMinutes = 99;
			constraint.Validation.ValidateFC_Type();
			AssertHasWarning("Constraint - Pre Constraint: Lines up. Post Constraint: Does not reach constraint", constraint.FC_TypeInfo, "A pre-constraint buffer should be configured so its end lines up with the Constraint component, and a post-constraint buffer should be configured so its start lines up with the Constraint on the Graphical Schematic. This is to detect when tasks involving a CCR are in a risky state that threatens CCR throughput.");

			postConstraintComponent.FC_OffsetInMinutes = 100;
			constraint.Validation.ValidateFC_Type();
			AssertNoWarnings("Constraint - Pre Constraint: Lines up. Post Constraint: Lines up", constraint);

			buffer.ChildComponents.RemoveFromRelationship(preConstraintComponent);
			AssertEquals(2, buffer.ChildComponents.Count);
			constraint.Validation.ValidateFC_Type();
			AssertHasWarning("Constraint - Pre Constraint: None. Post Constraint: Lines up", constraint.FC_TypeInfo, "A pre-constraint buffer should be configured so its end lines up with the Constraint component, and a post-constraint buffer should be configured so its start lines up with the Constraint on the Graphical Schematic. This is to detect when tasks involving a CCR are in a risky state that threatens CCR throughput.");
		}

		public void TestConstraintThresholdMultiple()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var component = BMSTestHelper.CreateBucket(system);

			AssertNoErrors(component.FC_BufferTimeCapacityConstraintThresholdMultipleInfo);

			component.FC_BufferTimeCapacityConstraintThresholdMultiple = 2;
			AssertHasError(component.FC_BufferTimeCapacityConstraintThresholdMultipleInfo, "Constraint Threshold Multiple has an effect on Buffer components only.");

			component.FC_Type = BMComponentTypeList.Codes.Buffer;
			component.FC_BufferTimeCapacityConstraintThresholdMultiple = 0;
			AssertHasError(component.FC_BufferTimeCapacityConstraintThresholdMultipleInfo, "Buffer components should have a Constraint Threshold Multiple greater than zero.");

			component.FC_BufferTimeCapacityConstraintThresholdMultiple = 0.1;
			AssertNoErrors(component.FC_BufferTimeCapacityConstraintThresholdMultipleInfo);
		}

		public void TestNonCCRMultiplier()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var component = BMSTestHelper.CreateBucket(system);

			AssertNoErrors(component.FC_NonCCRTemporaryOverloadLimitMultiplierInfo);

			component.FC_NonCCRTemporaryOverloadLimitMultiplier = 2;
			AssertHasError(component.FC_NonCCRTemporaryOverloadLimitMultiplierInfo, "Non-CCR Overload Limit has an effect on Buffer components only.");

			component.FC_Type = BMComponentTypeList.Codes.Buffer;
			component.FC_NonCCRTemporaryOverloadLimitMultiplier = 0;
			AssertHasError(component.FC_NonCCRTemporaryOverloadLimitMultiplierInfo, "Buffer components should have a Non-CCR Overload Limit greater than zero.");

			component.FC_NonCCRTemporaryOverloadLimitMultiplier = 0.1;
			AssertNoErrors(component.FC_NonCCRTemporaryOverloadLimitMultiplierInfo);
		}

		public void TestConstraintComponentType_ShouldBeValidOnSubComponentsOnly()
		{
			var system = Factory.New<BMSystem>();
			var component = system.Components.AddNew();

			component.FC_Type = BMComponentTypeList.Codes.Constraint;
			AssertHasError(component.FC_TypeInfo, "Constraint component type is only valid on sub-components.");

			var subComponent = component.ChildComponents.AddNew();
			subComponent.FC_Type = BMComponentTypeList.Codes.Constraint;
			AssertNoErrors(subComponent.FC_TypeInfo);
		}

		#region Name

		public void TestNameValidation()
		{
			var system = Factory.NewWithValidTestData<BMSystem>();
			var component1 = system.Components.AddNew();

			//Name is mandatory
			component1.FC_Name = "";
			AssertHasErrors(component1.FC_NameInfo);

			component1.FC_Name = "Cmp1";
			AssertNoErrors(component1.FC_NameInfo);

			var component2 = system.Components.AddNew();

			//name must be unique in the system
			component2.FC_Name = "Cmp1";
			AssertHasErrors(component2.FC_NameInfo);

			component2.FC_Name = "Cmp2";
			AssertNoErrors(component2.FC_NameInfo);

			var anotherSystem = Factory.NewWithValidTestData<BMSystem>();
			var anotherComponent = anotherSystem.Components.AddNew();

			//Same name, but on another system => all good
			anotherComponent.FC_Name = "Cmp1";
			AssertNoErrors(anotherComponent.FC_NameInfo);
		}

		#endregion

		#region System

		public void TestSystem_ShouldHaveSystem()
		{
			var component = Factory.New<BMComponent>();
			component.FC_Type = BMComponentTypeList.Codes.Bucket;
			component.Validation.ValidateFC_FS_System();
			AssertHasError("System is mandatory",
				component.FC_FS_SystemInfo,
				"Please enter a Buffer Management System.");
		}

		#endregion

		public void TestTypeValidation()
		{
			var component = Factory.NewWithValidTestData<BMComponent>();

			//Type is mandatory
			component.FC_Type = "";
			AssertHasErrors(component.FC_TypeInfo);

			//Type must be valid
			component.FC_Type = "XXX";
			AssertHasErrors(component.FC_TypeInfo);

			component.FC_Type = BMComponentTypeList.Codes.Bucket;
			AssertNoErrors(component.FC_TypeInfo);
		}

		public void TestOnlyBucketsMayBeDefined_WhenInEnhancedWorkflowMode()
		{
			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.BasicWorkflow);

			var system = BMSTestHelper.CreateSystem(Factory);
			var component = BMSTestHelper.CreateBuffer(system);
			component.Validation.ValidateFC_Type();

			AssertNoErrors("There's no need to validate for component type when in Basic Workflow mode, since BM systems cannot be defined and/or are inoperable at this level", component.FC_TypeInfo);

			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.EnhancedWorkflow);
			component.Validation.ValidateFC_Type();

			AssertHasError(component.FC_TypeInfo, "Only Bucket components may be defined when the registry item [Workflow Manager -> Buffer Management -> Workflow Management Mode] is set to EWF - Enhanced Workflow Management.");

			component.FC_Type = BMComponentTypeList.Codes.Bucket;
			AssertNoErrors("We should have no trouble defining a bucket in Enhanced Workflow mode", component.FC_TypeInfo);

			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.IncludesBufferManagement);
			component.FC_Type = BMComponentTypeList.Codes.Buffer;
			AssertNoErrors("We should have no trouble defining a buffer in Buffer Management mode", component.FC_TypeInfo);

			component.FC_Type = BMComponentTypeList.Codes.Bucket;
			AssertNoErrors("We should have no trouble defining a bucket in Buffer Management mode", component.FC_TypeInfo);

			BMSTestHelper.SetWorkflowManagementModeInRegistry(WorkflowManagementModes.Codes.PlanningManagement);
			component.FC_Type = BMComponentTypeList.Codes.Buffer;
			AssertNoErrors("We should have no trouble defining a buffer in Planning Management mode", component.FC_TypeInfo);

			component.FC_Type = BMComponentTypeList.Codes.Bucket;
			AssertNoErrors("We should have no trouble defining a bucket in Planning Management mode", component.FC_TypeInfo);
		}

		public void TestParentComponent_WhenPresent_ShouldBeBuffer()
		{
			var childComponent = Factory.NewWithValidTestData<BMComponent>();
			AssertNoErrors(childComponent.FC_FC_ParentComponentInfo);

			var parentComponent = Factory.NewWithValidTestData<BMComponent>();
			parentComponent.FC_Type = BMComponentTypeList.Codes.Bucket;
			childComponent.FC_FC_ParentComponent = parentComponent.PK;
			AssertHasError(childComponent.FC_FC_ParentComponentInfo, "Cannot add child components to a non-Buffer parent component.");

			parentComponent.FC_Type = BMComponentTypeList.Codes.Buffer;
			childComponent.Validation.ValidateAll();
			AssertNoErrors(childComponent.FC_FC_ParentComponentInfo);
		}

		public void TestBufferTimespan_BufferType_ShouldBeMandatoryAndGreaterThan3Minutes()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var buffer = BMSTestHelper.CreateBuffer(system, timespanMinutes: 0);
			AssertHasError(buffer.FC_BufferTimespanInMinutesInfo, "Please enter a Buffer Timespan.");

			buffer.FC_BufferTimespanInMinutes = 1;
			AssertHasError(buffer.FC_BufferTimespanInMinutesInfo, "The buffer timespan should be at least three minutes, since there are always three zones in a buffer, which are each one minute at the smallest.");

			buffer.FC_BufferTimespanInMinutes = 3;
			AssertNoErrors(buffer.FC_BufferTimespanInMinutesInfo);

			buffer.FC_Type = BMComponentTypeList.Codes.Bucket;
			buffer.FC_BufferTimespanInMinutes = 0;
			AssertNoErrors(buffer.FC_BufferTimespanInMinutesInfo);

			buffer.FC_BufferTimespanInMinutes = 1;
			AssertHasError(buffer.FC_BufferTimespanInMinutesInfo, "A buffer timespan should only be entered for Buffer components.");
		}

		public void TestCheckFC_Type_ForChildComponents_ShouldNotAllowBucket()
		{
			var system = Factory.New<BMSystem>();
			var parentComponent = system.Components.AddNew();
			parentComponent.FC_Type = BMComponentTypeList.Codes.Buffer;
			var childComponent = parentComponent.ChildComponents.AddNew();

			childComponent.FC_Type = BMComponentTypeList.Codes.Bucket;
			AssertHasError(childComponent.FC_TypeInfo, "Sub-components of a Buffer cannot be of type Bucket.");

			childComponent.FC_Type = BMComponentTypeList.Codes.Buffer;
			AssertNoErrors(childComponent.FC_TypeInfo);
		}

		public void TestBufferLoadLimit()
		{
			var system = BMSTestHelper.CreateSystem(Factory);

			var component = system.Components.AddNew();
			component.FC_Type = BMComponentTypeList.Codes.Buffer;
			AssertEquals((ZByte)50, component.FC_BufferLoadLimitPercent);

			component.FC_Type = BMComponentTypeList.Codes.Bucket;
			AssertEquals((ZByte)0, component.FC_BufferLoadLimitPercent);

			component.FC_BufferLoadLimitPercent = 20;
			AssertHasError(component.FC_BufferLoadLimitPercentInfo, "A Buffer Load Limit Percent is only valid on a Buffer component.");

			component.FC_Type = BMComponentTypeList.Codes.Buffer;
			AssertEquals((ZByte)20, component.FC_BufferLoadLimitPercent);

			component.FC_BufferLoadLimitPercent = 0;
			AssertHasError("A buffer with no load is silly", component.FC_BufferLoadLimitPercentInfo, "Please enter a Buffer Load Limit Percent.");

			component.FC_BufferLoadLimitPercent = 101;
			AssertHasError(component.FC_BufferLoadLimitPercentInfo, "Please enter a 'Buffer Load Limit Percent' less than or equal to 100.");

			component.FC_BufferLoadLimitPercent = 100;
			AssertNoErrors(component.FC_BufferLoadLimitPercentInfo);
		}

		public void TestBufferLoadLimit_ShouldOnlyValidateOnTopLevelBuffers()
		{
			var buffer = Factory.New<BMSystem>().Components.AddNew();
			buffer.FC_Type = BMComponentTypeList.Codes.Buffer;
			buffer.FC_BufferLoadLimitPercent = 0;

			var subBuffer = buffer.ChildComponents.AddNew();
			subBuffer.FC_Type = BMComponentTypeList.Codes.Buffer;
			subBuffer.FC_BufferLoadLimitPercent = 0;

			buffer.Validation.ValidateAll();
			subBuffer.Validation.ValidateAll();

			AssertHasError(buffer.FC_BufferLoadLimitPercentInfo, "Please enter a Buffer Load Limit Percent.");
			AssertNoErrors(subBuffer.FC_BufferLoadLimitPercentInfo);
		}

		public void TestAutoAssignTasksAfterAge()
		{
			var component = Factory.New<BMSystem>().Components.AddNew();
			AssertEquals(ZDateTime.Empty, component.FC_AutoAssignTasksAge);

			component.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();
			AssertHasError(component.FC_AutoAssignTasksAgeInfo, "Auto Assign Tasks Age is only valid on a Buffer component.");

			component.FC_Type = BMComponentTypeList.Codes.Buffer;
			component.FC_AutoAssignTasksAge = ZDateTime.Empty;
			AssertNoErrors("Should not be mandatory", component.FC_AutoAssignTasksAgeInfo);

			component.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();
			AssertNoErrors(component.FC_AutoAssignTasksAgeInfo);
		}
	}
}
