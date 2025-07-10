using CargoWise.EntityFramework.Testing;

namespace Enterprise.BufferManagement.Business.Test
{
	class StaticControlCustomisationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestControlType()
		{
			var customisation = Factory.New<BMControlCustomisation>();
			customisation.FM_ControlType = CustomisedControlTypeList.Codes.TaskCard;
			var controlCustomisation = customisation.CustomisedControls.AddNew();

			controlCustomisation.ControlType = StaticControlTypeList.Codes.StatusButtons;
			AssertHasError(controlCustomisation.ControlTypeInfo, "Cannot add interactive controls to a summary card.");

			controlCustomisation.ControlType = StaticControlTypeList.Codes.TaskStatusIndicator;
			AssertNoErrors(controlCustomisation.ControlTypeInfo);

			customisation.FM_ControlType = CustomisedControlTypeList.Codes.DetailedCard;

			controlCustomisation.ControlType = StaticControlTypeList.Codes.StatusButtons;
			AssertNoErrors(controlCustomisation.ControlTypeInfo);

			controlCustomisation.ControlType = StaticControlTypeList.Codes.TaskStatusIndicator;
			AssertNoErrors(controlCustomisation.ControlTypeInfo);
		}

		public void TestControlType_WorkflowDetailedCard()
		{
			var customisation = Factory.New<BMControlCustomisation>();
			customisation.FM_ControlType = CustomisedControlTypeList.Codes.WorkflowDetailedCard;
			var controlCustomisation = customisation.CustomisedControls.AddNew();

			controlCustomisation.ControlType = StaticControlTypeList.Codes.CapabilityAssignmentButton;
			AssertHasError(controlCustomisation.ControlTypeInfo, "Cannot add task-related controls to a workflow card.");

			controlCustomisation.ControlType = StaticControlTypeList.Codes.StatusButtons;
			AssertHasError(controlCustomisation.ControlTypeInfo, "Cannot add task-related controls to a workflow card.");

			controlCustomisation.ControlType = StaticControlTypeList.Codes.TaskStatusIndicator;
			AssertNoErrors(controlCustomisation.ControlTypeInfo);
		}

		public void TestControlType_WorkflowSummaryCard()
		{
			var customisation = Factory.New<BMControlCustomisation>();
			customisation.FM_ControlType = CustomisedControlTypeList.Codes.WorkflowSummaryCard;
			var controlCustomisation = customisation.CustomisedControls.AddNew();

			controlCustomisation.ControlType = StaticControlTypeList.Codes.CapabilityAssignmentButton;
			AssertHasError(controlCustomisation.ControlTypeInfo, "Cannot add task-related controls to a workflow card.");

			controlCustomisation.ControlType = StaticControlTypeList.Codes.StatusButtons;
			AssertHasError(controlCustomisation.ControlTypeInfo, "Cannot add task-related controls to a workflow card.");

			controlCustomisation.ControlType = StaticControlTypeList.Codes.TaskStatusIndicator;
			AssertNoErrors(controlCustomisation.ControlTypeInfo);
		}

		public void TestOrientation()
		{
			var customisation = Factory.New<BMControlCustomisation>();
			customisation.FM_ControlType = CustomisedControlTypeList.Codes.TaskCard;
			var controlCustomisation = customisation.CustomisedControls.AddNew();
			controlCustomisation.Orientation = "Vertical";

			controlCustomisation.ControlType = StaticControlTypeList.Codes.TaskStatusIndicator;
			AssertNoErrors(controlCustomisation.OrientationInfo);

			controlCustomisation.ControlType = StaticControlTypeList.Codes.Label;
			AssertNoErrors(controlCustomisation.OrientationInfo);

			controlCustomisation.ControlType = StaticControlTypeList.Codes.AttachedTagsIndicator;
			AssertNoErrors(controlCustomisation.OrientationInfo);
		}
	}
}
