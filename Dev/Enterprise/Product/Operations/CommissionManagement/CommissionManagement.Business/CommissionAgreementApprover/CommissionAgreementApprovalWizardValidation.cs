using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.CommissionManagement.Business
{
	public class CommissionAgreementApprovalWizardValidation : AutoCommissionAgreementApprovalWizardValidation
	{
		public CommissionAgreementApprovalWizardValidation(AutoCommissionAgreementApprovalWizard parent)
			: base(parent)
		{
		}

		new CommissionAgreementApprovalWizard Parent
		{
			get { return (CommissionAgreementApprovalWizard)base.Parent; }
		}

		#region From Type

		protected override void CheckFromType()
		{
			base.CheckFromType();
			ListValidation.ErrorIfInvalidCode(Parent.FromTypeInfo);
			if (Parent.Action == CommissionAgreementApprovalWizard.ActionType.Approve)
			{
				MandatoryValidation.CheckEntered(Parent.FromTypeInfo);
			}
		}

		#endregion

		#region From Date

		protected override void CheckFromDateIsValidZDateTimeRange()
		{
			// Allow any date range
		}

		protected override void CheckFromDate()
		{
			base.CheckFromDate();

			if (Parent.FromTypeRequiresDate)
			{
				MandatoryValidation.CheckEntered(Parent.FromDateInfo);
				if (Parent.FromDate.Date > ZDateTime.Now.Date)
				{
					Parent.FromDateInfo.AddError(Res.GetString("38292ef9-414f-483c-a39d-39aa9f3a8bcc", "From date can not be in the future."));
				}
			}
		}

		#endregion

		protected override void ValidateAllCore()
		{
			Parent.ClearRowNotifications();

			base.ValidateAllCore();

			if (Parent.Action == CommissionAgreementApprovalWizard.ActionType.Approve)
			{
				RunApproveValidation();
			}
			else if (Parent.Action == CommissionAgreementApprovalWizard.ActionType.Disapprove)
			{
				RunDisapproveValidation();
			}
		}

		#region Approve Validation

		void RunApproveValidation()
		{
			if (!Parent.CommissionAgreementApprovalItems.Any())
			{
				Parent.AddRowError(NoUnapprovedCommissionAgreementMessage);
				return;
			}

			var itemsToApprove = Parent.CommissionAgreementApprovalItems.Where(x => x.IsInclude).ToList();
			if (itemsToApprove.Count == 0)
			{
				Parent.AddRowError(IncludeAsLeastOneCommissionAgreementMessage);
				return;
			}
		}

		#endregion

		#region Disapprove Validation

		void RunDisapproveValidation()
		{
			if (!Parent.CommissionAgreementApprovalItems.Any())
			{
				Parent.AddRowError(NoUnapprovedCommissionAgreementMessage);
				return;
			}

			var itemsToDisapprove = Parent.CommissionAgreementApprovalItems.Where(x => x.IsInclude).ToList();
			if (itemsToDisapprove.Count == 0)
			{
				Parent.AddRowError(IncludeAsLeastOneCommissionAgreementMessage);
				return;
			}
		}

		#endregion

		#region Messages

		static string NoUnapprovedCommissionAgreementMessage
		{
			get { return Res.GetString("5470acbb-207b-4f25-ac8b-cd9bae706164", "There are no new or changed commission agreements that require approval."); }
		}

		static string IncludeAsLeastOneCommissionAgreementMessage
		{
			get { return Res.GetString("2c3539ba-5029-4e06-826f-72ebae8bf722", "Please include at least one commission agreement."); }
		}

		#endregion
	}
}
