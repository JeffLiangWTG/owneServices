using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.TransactionApproval
{
	public class ARTransactionApprovalRequestValidation : TransactionApprovalRequestValidation
	{
		public ARTransactionApprovalRequestValidation(GenApprovalRequest parent)
			: base(parent)
		{
		}

		protected override void CheckXP_ReasonCode()
		{
			base.CheckXP_ReasonCode();

			if (!Parent.IsInDatabase && Parent.XP_ParentTableCode != GenApprovalRequestSchema.Constants.Prefix)
			{
				ListValidation.ErrorIfInvalidCode(Parent.XP_ReasonCodeInfo, GenApprovalRequestHelper.GetReasonCodeList((GenApprovalRequest)Parent));
				MandatoryValidation.CheckEntered(Parent.XP_ReasonCodeInfo);
			}
		}

		protected override void CheckXP_ReasonDescription()
		{
			if (Parent.XP_ParentTableCode != GenApprovalRequestSchema.Constants.Prefix
				&& (Parent.XP_ReasonDescriptionInfo.HasChanges || !Parent.IsInDatabase))
			{
				base.CheckXP_ReasonDescription();
			}
		}
	}
}
