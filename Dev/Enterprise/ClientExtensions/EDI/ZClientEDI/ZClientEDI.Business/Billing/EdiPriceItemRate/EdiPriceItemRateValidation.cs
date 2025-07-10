//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiPriceItemRateValidation
//
//    This class should be used for overriding validation in AutoEdiPriceItemRateValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.Billing.Business
{
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;

	public class EdiPriceItemRateValidation : AutoEdiPriceItemRateValidation
	{
		public EdiPriceItemRateValidation(AutoEdiPriceItemRate parent) : base(parent)
		{
		}

		protected override void CheckPIR_RX_NKCurrency()
		{
			base.CheckPIR_RX_NKCurrency();
			MandatoryValidation.CheckEntered(Parent.PIR_RX_NKCurrencyInfo);
			ListValidation.ErrorIfInvalidCode(Parent.PIR_RX_NKCurrencyInfo);
			if (!Parent.PIR_RX_NKCurrencyInfo.HasErrors() && !IsCurrencyUnique())
			{
				Parent.PIR_RX_NKCurrencyInfo.AddError("Currency must be unique.");
			}
		}

		bool IsCurrencyUnique()
		{
			var parent = (EdiPriceItemRate)Parent;

			foreach (var rate in parent.PriceItem.CurrencyRates)
			{
				if (rate.PK != parent.PK && rate.PIR_RX_NKCurrency == parent.PIR_RX_NKCurrency)
				{
					return false;
				}
			}

			return true;
		}

		void ValidateNoConflictWithPriceItemCurrency()
		{
			Parent.ClearRowNotifications();
			var rate = (EdiPriceItemRate)Parent;
			if (!rate.PriceItem.L7_RX_NKCurrency.IsEmpty)
			{
				rate.AddRowError("Should not add price in other currencies when price item already has a currency.");
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateNoConflictWithPriceItemCurrency();
		}
	}
}

