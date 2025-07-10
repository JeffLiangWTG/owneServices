using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business;

public class CalculateFreightBizObjValidation : AutoCalculateFreightBizObjValidation
{
	public CalculateFreightBizObjValidation(AutoCalculateFreightBizObj parent)
		: base(parent)
	{
	}

	public new CalculateFreightBizObj Parent => (CalculateFreightBizObj)base.Parent;

	protected override void CheckTotalAmount()
	{
		base.CheckTotalAmount();
		MandatoryValidation.CheckEntered(Parent.TotalAmountInfo);
	}

	protected override void CheckCurrency()
	{
		base.CheckCurrency();
		ListValidation.ErrorIfInvalidCode(Parent.CurrencyInfo);
	}

	public static string PercentageToCHBoarderRangeErrorMessage => Res.GetString("BBD53479-DBE0-4D56-A436-EEEED5F2019C", "%Freight to CH Boarder must be between 0 and 100.");

	protected override void CheckPercentageToCHBoarder()
	{
		base.CheckPercentageToCHBoarder();
		MandatoryValidation.CheckEntered(Parent.PercentageToCHBoarderInfo);
		if (!Parent.PercentageToCHBoarder.IsInRange(ZDecimal.Zero, 100m))
		{
			Parent.PercentageToCHBoarderInfo.AddError(PercentageToCHBoarderRangeErrorMessage);
		}
	}
}
