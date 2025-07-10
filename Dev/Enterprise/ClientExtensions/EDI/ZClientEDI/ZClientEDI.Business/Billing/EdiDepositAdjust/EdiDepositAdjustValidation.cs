//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiDepositAdjustValidation
//
//    This class should be used for overriding validation in AutoEdiDepositAdjustValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.Billing.Business
{
	using System.Linq;
	using Enterprise.Client.EDI.MasterFiles.Business;

	public class EdiDepositAdjustValidation : AutoEdiDepositAdjustValidation
	{
		public EdiDepositAdjustValidation(AutoEdiDepositAdjust parent) : base(parent)
		{
		}

		protected override void CheckDEA_RX_NKCurrency()
		{
			var parent = (EdiDepositAdjust)Parent;
			if (!parent.IsInDatabase)
			{
				var company = ((EDIOrgHeader)(parent.Header))?.LicCompany;
				if (company != null)
				{
					var balance = company.DepositBalances.Cast<DepositBalance>().FirstOrDefault(x => x.ChargeCode == parent.DEA_ChargeCode);
					if (balance != null && balance.Amount != 0 && parent.DEA_RX_NKCurrency != balance.CurrencyCode)
					{
						parent.DEA_RX_NKCurrencyInfo.AddError("Currency must match balance currency");
					}
				}
			}
		}
	}
}

