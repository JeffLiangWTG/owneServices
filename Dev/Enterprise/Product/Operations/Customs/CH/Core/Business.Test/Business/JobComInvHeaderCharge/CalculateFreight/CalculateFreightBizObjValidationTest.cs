using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

class CalculateFreightBizObjValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckTotalAmount() => ValidationTestHelper.AssertErrorIfNotEntered(CalculateFreight.TotalAmountInfo);

	public void TestCheckCurrency()
	{
		CombineAssertions(() =>
		{
			AssertEquals(Core.Constants.CurrencyCodes.Switzerland, CalculateFreight.Currency);
			ValidationTestHelper.AssertErrorIfInvalidCode(CalculateFreight.CurrencyInfo, "XYZ", Core.Constants.CurrencyCodes.Germany);
		});
	}

	public void TestCheckPercentage_Input() => ValidationTestHelper.AssertErrorIfNotEntered(CalculateFreight.PercentageToCHBoarderInfo);

	public void TestCheckPercentage_Range()
	{
		CombineAssertions(() =>
		{
			CalculateFreight.PercentageToCHBoarder = -1;
			AssertHasError("Less than min", CalculateFreight.PercentageToCHBoarderInfo, CalculateFreightBizObjValidation.PercentageToCHBoarderRangeErrorMessage);
			CalculateFreight.PercentageToCHBoarder = 0;
			AssertNoError("Min", CalculateFreight.PercentageToCHBoarderInfo, CalculateFreightBizObjValidation.PercentageToCHBoarderRangeErrorMessage);
			CalculateFreight.PercentageToCHBoarder = 101;
			AssertHasError("Greater than max", CalculateFreight.PercentageToCHBoarderInfo, CalculateFreightBizObjValidation.PercentageToCHBoarderRangeErrorMessage);
			CalculateFreight.PercentageToCHBoarder = 100;
			AssertNoError("Max", CalculateFreight.PercentageToCHBoarderInfo, CalculateFreightBizObjValidation.PercentageToCHBoarderRangeErrorMessage);
		});
	}

	protected CalculateFreightBizObj GetNewBusinessObject()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		return new CalculateFreightBizObj(invoice.Charges);
	}

	CalculateFreightBizObj CalculateFreight => calculateFreight ?? (calculateFreight = GetNewBusinessObject());
	CalculateFreightBizObj calculateFreight;
}
