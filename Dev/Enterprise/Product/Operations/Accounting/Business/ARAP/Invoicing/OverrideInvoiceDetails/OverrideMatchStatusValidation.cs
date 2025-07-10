using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using static Enterprise.Accounting.Business.AccountingConstants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	class OverrideMatchStatusValidation : OverrideInvoiceDetailValidation
	{
		public OverrideMatchStatusValidation(InvoicingBase parent)
			: base(parent)
		{
		}

		#region CheckAH_MatchStatus

		protected override void CheckAH_MatchStatus()
		{
			base.CheckAH_MatchStatus();
			ListValidation.ErrorIfInvalidCode(Parent.AH_MatchStatusInfo, Parent.MatchStatusList);
		}

		#endregion

		#region CheckAH_MatchStatusReasonCode

		protected override void CheckAH_MatchStatusReasonCode()
		{
			base.CheckAH_MatchStatusReasonCode();
			ListValidation.ErrorIfInvalidCode(Parent.AH_MatchStatusReasonCodeInfo, Parent.MatchStatusReasonCodeList);

			if (!Parent.AH_MatchStatusReasonCodeInfo.HasErrors())
			{
				if (!Parent.AH_MatchStatus.IsEmpty)
				{
					MandatoryValidation.CheckEntered(Parent.AH_MatchStatusReasonCodeInfo);
				}
				else if (!Parent.AH_MatchStatusReasonCode.IsEmpty)
				{
					Parent.AH_MatchStatusReasonCodeInfo.AddError(AccountingMatchStatusReasonCodeErrorMessage.MatchStatusReasonCodeShouldNotSpecified);
				}
			}
		}

		#endregion
	}
}
