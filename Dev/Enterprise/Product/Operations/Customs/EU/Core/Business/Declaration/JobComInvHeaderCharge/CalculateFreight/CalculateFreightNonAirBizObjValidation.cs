using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CalculateFreightNonAirBizObjValidation : AutoCalculateFreightNonAirBizObjValidation
	{
		public CalculateFreightNonAirBizObjValidation(AutoCalculateFreightNonAirBizObj parent)
			: base(parent)
		{
		}

		public new CalculateFreightNonAirBizObj Parent => (CalculateFreightNonAirBizObj)base.Parent;

		protected override void CheckTotalAmount()
		{
			base.CheckTotalAmount();
			MandatoryValidation.CheckEntered(Parent.TotalAmountInfo);
		}

		protected override void CheckPercentageFreightToEUBorder()
		{
			base.CheckPercentageFreightToEUBorder();
			if (!Parent.PercentageFreightToEUBorder.IsInRange(ZDecimal.Zero, 100m))
			{
				Parent.PercentageFreightToEUBorderInfo.AddError(Res.GetString("C6835521-4B85-4C2E-B69A-9026D60512DB", "%Freight to EU Border must be between 0 and 100."));
			}
			else if ((Parent.PercentageFreightToEUBorder + Parent.PercentageFreightEUToDestinationCountry) > 100)
			{
				Parent.PercentageFreightToEUBorderInfo.AddError(PercentageSumErrorMessage);
			}
		}

		protected override void CheckPercentageFreightEUToDestinationCountry()
		{
			base.CheckPercentageFreightEUToDestinationCountry();
			if (!Parent.PercentageFreightEUToDestinationCountry.IsInRange(ZDecimal.Zero, 100m))
			{
				Parent.PercentageFreightEUToDestinationCountryInfo.AddError(Res.GetString("030A1191-FE9D-4010-80EF-AEC3A197AE53", "%Freight EU to Destination Country must be between 0 and 100."));
			}
			else if ((Parent.PercentageFreightToEUBorder + Parent.PercentageFreightEUToDestinationCountry) > 100)
			{
				Parent.PercentageFreightEUToDestinationCountryInfo.AddError(PercentageSumErrorMessage);
			}
		}

		protected override void CheckCurrency()
		{
			base.CheckCurrency();
			MandatoryValidation.CheckEntered(Parent.CurrencyInfo);
			ListValidation.ErrorIfInvalidCode(Parent.CurrencyInfo);
			if (Parent.Currency.IsEmpty && Parent.TotalAmount == 0m)
			{
				Parent.CurrencyInfo.AddError(InconsistentCurrencies);
			}
		}

		string PercentageSumErrorMessage => Res.GetString("8FB72164-B05B-46C7-B041-AD91E7FFDC1A", "%Freight to EU Border + %Freight EU to Destination Country cannot be greater than 100.");

		public static string InconsistentCurrencies => Res.GetString("EC99A4CB-DF8A-4A1E-9599-2B81F011A164", "Transport charge currencies inconsistency detected in charge grid.");
	}
}
