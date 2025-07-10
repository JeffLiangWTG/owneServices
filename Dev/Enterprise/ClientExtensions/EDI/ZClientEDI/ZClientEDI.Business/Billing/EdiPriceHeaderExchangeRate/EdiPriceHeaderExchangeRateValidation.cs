//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiPriceHeaderExchangeRateValidation
//
//    This class should be used for overriding validation in AutoEdiPriceHeaderExchangeRateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.Billing.Business
{
	using System.Linq;
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;

	public class EdiPriceHeaderExchangeRateValidation : AutoEdiPriceHeaderExchangeRateValidation
	{
		public EdiPriceHeaderExchangeRateValidation(AutoEdiPriceHeaderExchangeRate parent) : base(parent)
		{
		}

		protected override void CheckPHE_GroupCode()
		{
			base.CheckPHE_GroupCode();
			ListValidation.ErrorIfInvalidCode(Parent.PHE_GroupCodeInfo);

			if (!Parent.PHE_GroupCodeInfo.HasErrors() && !IsGroupCodeCurrencyUnique())
			{
				Parent.PHE_GroupCodeInfo.AddError("Group Code / Currency must be unique.");
			}
		}

		protected override void CheckPHE_RX_NKCurrency()
		{
			base.CheckPHE_RX_NKCurrency();
			MandatoryValidation.CheckEntered(Parent.PHE_RX_NKCurrencyInfo);
			ListValidation.ErrorIfInvalidCode(Parent.PHE_RX_NKCurrencyInfo);
		}

		protected override void CheckPHE_Rate()
		{
			base.CheckPHE_Rate();
			MandatoryValidation.CheckEntered(Parent.PHE_RateInfo);
			MandatoryValidation.CheckNotZero(Parent.PHE_RateInfo);
			MandatoryValidation.CheckNotNegative(Parent.PHE_RateInfo);
		}

		bool IsGroupCodeCurrencyUnique()
		{
			var parent = (EdiPriceHeaderExchangeRate)Parent;
			return !parent.PriceHeader.ExchangeRates.Any(x => x.PK != parent.PK
				&& x.PHE_GroupCode == parent.PHE_GroupCode && x.PHE_RX_NKCurrency == parent.PHE_RX_NKCurrency);
		}
	}
}


