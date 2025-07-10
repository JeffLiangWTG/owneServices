using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using NotificationType = CargoWise.EntityFramework.NotificationType;

namespace Enterprise.CommissionManagement.Business
{
	public class CommissionAgreementApprovalItemValidation : AutoCommissionAgreementApprovalItemValidation
	{
		public CommissionAgreementApprovalItemValidation(AutoCommissionAgreementApprovalItem parent) : base(parent)
		{
		}

		public new CommissionAgreementApprovalItem Parent
		{
			get { return (CommissionAgreementApprovalItem)base.Parent; }
		}

		protected override void CheckIsInclude()
		{
			base.CheckIsInclude();

			if (Parent.IsInclude)
			{
				if (!Parent.CommissionAgreement.IsReversed)
				{
					var notificationType = (Parent.Wizard.Action == CommissionAgreementApprovalWizard.ActionType.Approve) ? NotificationType.Error : NotificationType.Warning;

					if (!Parent.CommissionAgreement.OpportunityIsEffective(GlbCompany.CurrentCompany.PK))
					{
						Parent.IsIncludeInfo.AddNotification(notificationType, Res.GetString("a8cd7b54-c9f3-4f97-b8cd-47bfeb3ebeb9", "Cannot approve agreement until opportunity is effective."));
					}
					else
					{
						Parent.CommissionAgreement.LoadChildEditableObjects();
						Parent.CommissionAgreement.RunFromApprovalItem = true;
						Parent.CommissionAgreement.RunPreSaveValidation();
						if (Parent.CommissionAgreement.HasErrors)
						{
							var errorMessage = new ZNotificationCollector(Parent.CommissionAgreement, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetErrors().ToUniqueMessageListString();
							Parent.IsIncludeInfo.AddNotification(notificationType, errorMessage);
						}
					}
				}

				if (Parent.Wizard.Action == CommissionAgreementApprovalWizard.ActionType.Disapprove)
				{
					if (!Parent.CommissionAgreement.IsDraft)
					{
						Parent.IsIncludeInfo.AddError(Res.GetString("c283de9f-be12-4d5f-acfa-06ea1fc5d394", "Cannot disapprove an approved agreement. Reverse or disable the agreement instead."));
					}
				}
			}
		}
	}
}
