using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class CalculateFreightBizObjValidation : AutoCalculateFreightBizObjValidation
	{
		public CalculateFreightBizObjValidation(AutoCalculateFreightBizObj parent)
			: base(parent)
		{
		}

		public new CalculateFreightBizObj Parent => (CalculateFreightBizObj)base.Parent;

		protected override void CheckPercentage()
		{
			base.CheckPercentage();
			if (!Parent.Percentage.IsInRange(ZDecimal.Zero, 100m))
			{
				Parent.PercentageInfo.AddError(PercentageShouldBeBetween);
			}
		}

		public static string PercentageShouldBeBetween => Res.GetString("{D083D09D-46DB-4499-B165-8F50C4019B33}", "Percentage value should be between 0 and 100.");

		public static string InconsistentCurrencies => Res.GetString("4B25D8CC-D95A-42BB-987B-3D1BF297A4AC", "Transport charge currencies inconsistency detected in charge grid.");

		protected override void CheckCurrency()
		{
			base.CheckCurrency();
			MandatoryValidation.CheckEntered(Parent.CurrencyInfo);
			ListValidation.ErrorIfInvalidCode(Parent.CurrencyInfo);
			if (Parent.Currency.IsEmpty && Parent.Amount == 0m)
			{
				Parent.CurrencyInfo.AddError(InconsistentCurrencies);
			}
		}
	}
}
