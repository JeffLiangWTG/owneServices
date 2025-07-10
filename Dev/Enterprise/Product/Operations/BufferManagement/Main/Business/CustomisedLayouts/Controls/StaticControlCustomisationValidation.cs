
namespace Enterprise.BufferManagement.Business
{
	class StaticControlCustomisationValidation : ControlCustomisationValidationBase
	{
		public StaticControlCustomisationValidation(StaticControlCustomisation parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly StaticControlCustomisation parent;

		protected override void CheckControlType()
		{
			base.CheckControlType();
			var controlType = parent.Parent.FM_ControlType;

			if (controlType == CustomisedControlTypeList.Codes.TaskCard && !parent.IsReadOnlyControlType)
			{
				parent.ControlTypeInfo.AddError(Res.GetString("7ffd6d3a-1e35-46b4-b110-283d0f53e69c", "Cannot add interactive controls to a summary card."));
			}

			if (controlType == CustomisedControlTypeList.Codes.WorkflowDetailedCard || controlType == CustomisedControlTypeList.Codes.WorkflowSummaryCard)
			{
				switch (parent.ControlType)
				{
					case StaticControlTypeList.Codes.CapabilityAssignmentButton:
					case StaticControlTypeList.Codes.StatusButtons:
						parent.ControlTypeInfo.AddError(Res.GetString("195e7acb-4c25-4710-8bf1-2720f4e70d96", "Cannot add task-related controls to a workflow card."));
						break;
				}
			}
		}

		protected override void CheckOrientation()
		{
			base.CheckOrientation();

			if (parent.Orientation == nameof(BMBoardSectionOrientation.Vertical))
			{
				switch (parent.ControlType)
				{
					case StaticControlTypeList.Codes.Label:
					case StaticControlTypeList.Codes.AttachedTagsIndicator:
					case StaticControlTypeList.Codes.TaskStatusIndicator:
						break;

					default:
						parent.OrientationInfo.AddError(Res.GetString("56a0a13a-eb7f-4952-b9cf-01340584bb0c", "Vertical orientation is not supported for this type of control."));
						break;
				}
			}
		}
	}
}
