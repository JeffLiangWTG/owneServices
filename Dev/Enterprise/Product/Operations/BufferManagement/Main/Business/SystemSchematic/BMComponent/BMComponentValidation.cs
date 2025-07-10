using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class BMComponentValidation : AutoBMComponentValidation
	{
		public BMComponentValidation(AutoBMComponent parent)
			: base(parent)
		{
		}

		new BMComponent Parent
		{
			get { return (BMComponent)base.Parent; }
		}

		protected override void CheckFC_Name()
		{
			base.CheckFC_Name();
			MandatoryValidation.CheckEntered(Parent.FC_NameInfo);

			VerifyFC_NameIsUnique();
		}

		protected virtual void VerifyFC_NameIsUnique()
		{
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.FC_NameInfo, Parent.System.Components);
		}

		protected override void CheckFC_FS_System()
		{
			MandatoryValidation.CheckEntered(Parent.FC_FS_SystemInfo);
		}

		protected override void CheckFC_Type()
		{
			base.CheckFC_Type();
			MandatoryValidation.CheckEntered(Parent.FC_TypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.FC_TypeInfo);

			if (BMSRegistry.Instance.WorkflowManagementMode.Value == WorkflowManagementModes.Codes.EnhancedWorkflow && Parent.FC_Type != BMComponentTypeList.Codes.Bucket)
			{
				var path = BMSRegistry.Instance.WorkflowManagementMode.GetLocation();
				Parent.FC_TypeInfo.AddError(Res.GetString("1c55f292-0c82-4791-a9ac-f9e72e1b2037", "Only Bucket components may be defined when the registry item [{0}] is set to EWF - Enhanced Workflow Management.", path));
			}

			if (Parent.FC_FC_ParentComponent.IsValid)
			{
				if (Parent.FC_Type == BMComponentTypeList.Codes.Bucket)
				{
					Parent.FC_TypeInfo.AddError(Res.GetString("cfcb5cdc-dc71-43be-bf42-c92fe372e60e", "Sub-components of a Buffer cannot be of type Bucket."));
				}

				if (Parent.FC_Type == BMComponentTypeList.Codes.Constraint)
				{
					CheckPreAndPostConstraintComponents();
				}
			}
			else if (Parent.FC_Type == BMComponentTypeList.Codes.Constraint)
			{
				Parent.FC_TypeInfo.AddError(Res.GetString("e09edf72-4475-4a71-949f-0dbd1e79719f", "Constraint component type is only valid on sub-components."));
			}
		}

		protected override void CheckFC_GE_AgingDepartment()
		{
			base.CheckFC_GE_AgingDepartment();
			if (Parent.FC_Type == BMComponentTypeList.Codes.Buffer && Parent.FC_GE_AgingDepartment.IsEmpty)
			{
				Parent.FC_GE_AgingDepartmentInfo.AddError(Res.GetString("1786FDF1-207C-4AD9-80C4-F73D59340E4D", "Aging Department cannot be empty while component type is a buffer."));
			}
		}

		protected override void CheckFC_GB_AgingBranch()
		{
			base.CheckFC_GB_AgingBranch();
			if (Parent.FC_Type == BMComponentTypeList.Codes.Buffer && Parent.FC_GB_AgingBranch.IsEmpty)
			{
				Parent.FC_GB_AgingBranchInfo.AddError(Res.GetString("31F20120-3615-4D07-8B84-AC7149618261", "Aging Branch cannot be empty while component type is a buffer."));
			}
		}

		void CheckPreAndPostConstraintComponents()
		{
			var isComponentBeforeConstraint = Parent.ParentComponent.ChildComponents.Where(c => c != Parent).Any(c => c.FC_OffsetInMinutes < Parent.FC_OffsetInMinutes);
			var isComponentAfterConstraint = Parent.ParentComponent.ChildComponents.Where(c => c != Parent).Any(c => c.FC_OffsetInMinutes >= Parent.FC_OffsetInMinutes);
			var isPreConstraintInlineWithConstraint = Parent.ParentComponent.ChildComponents.Where(c => c != Parent).Any(BufferEndLinesUpWithConstraint);
			var isPostConstraintInlineWithConstraint = Parent.ParentComponent.ChildComponents.Where(c => c != Parent).Any(BufferStartLinesUpWithConstraint);

			if (Parent.ParentComponent.ChildComponents.Count < 3 || !isComponentBeforeConstraint || !isComponentAfterConstraint)
			{
				Parent.FC_TypeInfo.AddWarning(Res.GetString("57459AC0-F61C-498D-91D6-DDA26242B77F", "Buffer sub-components should be added before and after the Constraint Component Offset in order to draw attention to items causing risk to Capacity Constrained Resources."));
			}
			if (Parent.ParentComponent.ChildComponents.Count < 3 || !isPreConstraintInlineWithConstraint || !isPostConstraintInlineWithConstraint)
			{
				Parent.FC_TypeInfo.AddWarning(Res.GetString("7C49F45F-E8FC-48A8-BD4F-732E9987B03A", "A pre-constraint buffer should be configured so its end lines up with the Constraint component, and a post-constraint buffer should be configured so its start lines up with the Constraint on the Graphical Schematic. This is to detect when tasks involving a CCR are in a risky state that threatens CCR throughput."));
			}
		}

		bool BufferStartLinesUpWithConstraint(BMComponent component)
		{
			return component.FC_OffsetInMinutes == Parent.FC_OffsetInMinutes;
		}

		bool BufferEndLinesUpWithConstraint(BMComponent component)
		{
			return component.FC_OffsetInMinutes + component.FC_BufferTimespanInMinutes == Parent.FC_OffsetInMinutes;
		}

		protected override void CheckFC_FC_ParentComponent()
		{
			base.CheckFC_FC_ParentComponent();

			if (Parent.ParentComponent != null && !Parent.ParentComponent.IsBuffer)
			{
				Parent.FC_FC_ParentComponentInfo.AddError(Res.GetString("b72a3971-df40-4ca8-a225-3261be01d573", "Cannot add child components to a non-Buffer parent component."));
			}
		}

		protected override void CheckFC_BufferTimespanInMinutes()
		{
			base.CheckFC_BufferTimespanInMinutes();

			if (Parent.IsBuffer)
			{
				MandatoryValidation.CheckEntered(Parent.FC_BufferTimespanInMinutesInfo);
				if (Parent.FC_BufferTimespanInMinutes > 0 && Parent.FC_BufferTimespanInMinutes < 3)
				{
					Parent.FC_BufferTimespanInMinutesInfo.AddError(Res.GetString("efef6ee4-8948-4779-9ddb-5703727c164f", "The buffer timespan should be at least three minutes, since there are always three zones in a buffer, which are each one minute at the smallest."));
				}
			}
			else if (Parent.FC_BufferTimespanInMinutes != 0)
			{
				Parent.FC_BufferTimespanInMinutesInfo.AddError(Res.GetString("55bf75b8-13bc-4968-99df-191356bd2810", "A buffer timespan should only be entered for Buffer components."));
			}
		}

		protected override void CheckFC_BufferLoadLimitPercent()
		{
			base.CheckFC_BufferLoadLimitPercent();

			if (Parent.IsBuffer)
			{
				if (Parent.FC_FC_ParentComponent.IsEmpty)
				{
					MandatoryValidation.CheckEntered(Parent.FC_BufferLoadLimitPercentInfo);
					CompareValidation.CheckLessThanOrEqualTo(Parent.FC_BufferLoadLimitPercentInfo, 100m);
				}
			}
			else if (Parent.FC_BufferLoadLimitPercent > 0)
			{
				Parent.FC_BufferLoadLimitPercentInfo.AddError(Res.GetString("1f0cb4b9-83f2-4fa0-9e13-c9664164e729", "A Buffer Load Limit Percent is only valid on a Buffer component."));
			}
		}

		protected override void CheckFC_AutoAssignTasksAge()
		{
			base.CheckFC_AutoAssignTasksAge();

			if (!Parent.IsBuffer && !Parent.FC_AutoAssignTasksAge.IsEmpty)
			{
				Parent.FC_AutoAssignTasksAgeInfo.AddError(Res.GetString("af849d9f-7278-45f6-9bea-056ec9ab6ba5", "Auto Assign Tasks Age is only valid on a Buffer component."));
			}
		}

		protected override void CheckFC_AutoAssignTasksAgeIsValidZDateTime()
		{
			var value = Parent.FC_AutoAssignTasksAgeInfo.Value;
			if (!value.IsEmpty && !value.IsValid)
			{
				var description = ResString.GetMultilingualString("3CC20FFA-C5FD-4AE4-92F8-A27A67C07C0B", "Auto Assign Tasks Age. Correct format should be 000:00");
				var message = string.Format(CultureInfo.InvariantCulture, TypeValidation.InvalidTypeMessage, description);
				Parent.FC_AutoAssignTasksAgeInfo.AddError(message);
			}
		}

		protected override void CheckFC_BufferTimeCapacityConstraintThresholdMultiple()
		{
			base.CheckFC_BufferTimeCapacityConstraintThresholdMultiple();

			if (Parent.IsBuffer)
			{
				if (Parent.FC_BufferTimeCapacityConstraintThresholdMultiple <= 0)
				{
					Parent.FC_BufferTimeCapacityConstraintThresholdMultipleInfo.AddError(Res.GetString("5216c3a9-396f-426b-9116-98521a4c5403", "Buffer components should have a Constraint Threshold Multiple greater than zero."));
				}
			}
			else if (Parent.FC_BufferTimeCapacityConstraintThresholdMultiple != 1)
			{
				Parent.FC_BufferTimeCapacityConstraintThresholdMultipleInfo.AddError(Res.GetString("10cc9d6a-5581-47ed-af47-444ce08fc69a", "Constraint Threshold Multiple has an effect on Buffer components only."));
			}
		}

		protected override void CheckFC_NonCCRTemporaryOverloadLimitMultiplier()
		{
			base.CheckFC_NonCCRTemporaryOverloadLimitMultiplier();

			if (Parent.IsBuffer)
			{
				if (Parent.FC_NonCCRTemporaryOverloadLimitMultiplier <= 0)
				{
					Parent.FC_NonCCRTemporaryOverloadLimitMultiplierInfo.AddError(Res.GetString("7774d32c-bd5b-4a78-9cf8-03330a8936b4", "Buffer components should have a Non-CCR Overload Limit greater than zero."));
				}
			}
			else if (Parent.FC_NonCCRTemporaryOverloadLimitMultiplier != 1)
			{
				Parent.FC_NonCCRTemporaryOverloadLimitMultiplierInfo.AddError(Res.GetString("69d09fe3-7760-4be9-9d8b-1c1027722ad5", "Non-CCR Overload Limit has an effect on Buffer components only."));
			}
		}
	}
}
