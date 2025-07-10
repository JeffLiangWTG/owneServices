using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public partial class ProfessionalServicesQuoteValidation
	{
		protected override void CheckIM_CloseTimeUtc()
		{
			base.CheckIM_CloseTimeUtc();

			if (!Parent.IM_CloseTimeUtc.IsEmpty && Parent.IM_CloseTimeUtc < Parent.IM_SystemCreateTimeUtc)
			{
				Parent.IM_CloseTimeUtcInfo.AddError("The Close Date must be greater than the Start Date.");
			}
		}

		protected override void CheckIM_Status()
		{
			base.CheckIM_Status();

			MandatoryValidation.CheckEntered(Parent.IM_StatusInfo);
			ListValidation.ErrorIfInvalidCode(Parent.IM_StatusInfo, Parent.Lookups.StatusList);
		}

		protected override void CheckIM_GG_Team()
		{
			base.CheckIM_GG_Team();

			MandatoryValidation.CheckEntered(Parent.IM_GG_TeamInfo);

			ListValidation.ErrorIfInvalidPK(Parent.IM_GG_TeamInfo, Parent.Lookups.ListHelper.StaffDependentGroupList);

			if (Parent.AssignedToCurrent != null && Parent.Team != null && (Parent.AssignedToCurrent.Groups.FindByPK(Parent.IM_GG_Team) == null))
			{
				Parent.IM_GG_TeamInfo.AddWarning("The Assigned User '" + Parent.IM_CurrentlyAssignedToInitials +
					"' is not a member of the Team '" + Parent.Team.GG_Code + "'.");
			}
		}

		protected override void CheckIM_Description()
		{
			base.CheckIM_Description();

			if (Parent.IM_Description.Length < 10)
			{
				Parent.IM_DescriptionInfo.AddError("You must enter a Description of more than 10 characters.");
			}
		}

		protected override void CheckIM_Priority()
		{
			base.CheckIM_Priority();
			ListValidation.ErrorIfInvalidCode(Parent.IM_PriorityInfo, Parent.Lookups.PriorityList);
		}

		protected override void CheckIM_EstimatedHours()
		{
			base.CheckIM_EstimatedHours();
			CompareValidation.CheckNumberNotNegative(Parent.IM_EstimatedHoursInfo);
		}

		protected override void CheckIM_WorkItemType()
		{
			base.CheckIM_WorkItemType();
			ListValidation.ErrorIfInvalidCode(Parent.IM_WorkItemTypeInfo, Parent.Lookups.WorkItemTypeList);
		}

		protected new ProfessionalServicesQuote Parent
		{
			get { return (ProfessionalServicesQuote)base.Parent; }
		}
	}
}

