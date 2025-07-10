using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CalculateFreightBizObj))]
class CalculateFreightBizObjTest : NonPersistentBusinessObjectTestCase
{
	public void TestTotalAmount_Caption() => AssertEquals(CalculateFreight.TotalAmountInfo.Name, "Total Amount", CalculateFreight.TotalAmountInfo.Description);

	public void TestTotalAmount_ReadOnly() => AssertEquals(CalculateFreight.TotalAmountInfo.Name, false, CalculateFreight.TotalAmountInfo.ReadOnly);

	public void TestPercentageToCHBoarder_Caption() => AssertEquals(CalculateFreight.PercentageToCHBoarderInfo.Name, "%Freight to CH Border", CalculateFreight.PercentageToCHBoarderInfo.Description);

	public void TestPercentageToCHBoarder_ReadOnly() => AssertEquals(CalculateFreight.PercentageToCHBoarderInfo.Name, false, CalculateFreight.PercentageToCHBoarderInfo.ReadOnly);

	public void TestPercentageToFinalDestination_Caption() => AssertEquals(CalculateFreight.PercentageToFinalDestinationInfo.Name, "%Freight to Final Destination", CalculateFreight.PercentageToFinalDestinationInfo.Description);

	public void TestPercentageToFinalDestination_ReadOnly() => AssertEquals(CalculateFreight.PercentageToFinalDestinationInfo.Name, true, CalculateFreight.PercentageToFinalDestinationInfo.ReadOnly);

	public void TestAmountToCHBorder_Caption() => AssertEquals(CalculateFreight.AmountToCHBorderInfo.Name, "Amount to CH Border", CalculateFreight.AmountToCHBorderInfo.Description);

	public void TestAmountToCHBorder_ReadOnly() => AssertEquals(CalculateFreight.AmountToCHBorderInfo.Name, true, CalculateFreight.AmountToCHBorderInfo.ReadOnly);

	public void TestAmountToFinalDestination_Caption() => AssertEquals(CalculateFreight.AmountToFinalDestinationInfo.Name, "Amount to Final Destination", CalculateFreight.AmountToFinalDestinationInfo.Description);

	public void TestAmountToFinalDestination_ReadOnly() => AssertEquals(CalculateFreight.AmountToFinalDestinationInfo.Name, true, CalculateFreight.AmountToFinalDestinationInfo.ReadOnly);

	public void TestCalculation()
	{
		CalculateFreight.Currency = Core.Constants.CurrencyCodes.Switzerland;
		CombineAssertions(() =>
		{
			CalculateFreight.TotalAmount = 100;

			CalculateFreight.PercentageToCHBoarder = 20;
			AssertFreightChargeCalculationValues(80, 20, 80);
			CalculateFreight.PercentageToCHBoarder = 60;
			AssertFreightChargeCalculationValues(40, 60, 40);

			CalculateFreight.TotalAmount = 127.25;

			CalculateFreight.PercentageToCHBoarder = 14;
			AssertFreightChargeCalculationValues(86, 17.82M, 109.43M);
			CalculateFreight.PercentageToCHBoarder = 63;
			AssertFreightChargeCalculationValues(37, 80.17M, 47.08M);
		});
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		return new CalculateFreightBizObj(invoice.Charges);
	}

	CalculateFreightBizObj CalculateFreight => calculateFreight ?? (calculateFreight = (CalculateFreightBizObj)GetNewBusinessObject());
	CalculateFreightBizObj calculateFreight;

	void AssertFreightChargeCalculationValues(decimal percentageToFinalDestination, decimal amountToCHBorder, decimal amountToFinalDestination)
	{
		AssertEquals(percentageToFinalDestination, CalculateFreight.PercentageToFinalDestination);
		AssertEquals(amountToCHBorder, CalculateFreight.AmountToCHBorder);
		AssertEquals(amountToFinalDestination, CalculateFreight.AmountToFinalDestination);
	}
}
