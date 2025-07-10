using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class EUUniversalRateCalcData_DoNotInheritTest : TestCaseWithFactory
	{
		public void TestValueForDuty()
		{
			var invLine = Factory.New<JobComInvoiceLine>();
			invLine.JI_CustomsQuantity = 3;
			invLine.JI_CustomsUnitQty = "KGM";
			var entryLine = Factory.New<CusEntryLine>();
			entryLine.InvoiceLines.Add(invLine);

			var rateCalcData = new EUUniversalRateCalcData(entryLine, Factory.New<RateView>());
			AssertEquals("ValueForDuty", 0m, rateCalcData.ValueForDuty);

			entryLine.CL_CustomsValue = 2.34;
			AssertEquals("ValueForDuty", 2.34m, rateCalcData.ValueForDuty);

			rateCalcData.CustomsValueFormula = "1.23456 * [KGM]";
			AssertEquals("ValueForDuty", 3.704m, rateCalcData.ValueForDuty);
		}

		public void TestCustomsValue()
		{
			var entryLine = Factory.New<CusEntryLine>();
			var rateCalcData = new EUUniversalRateCalcData(entryLine, Factory.New<RateView>());
			AssertEquals("CustomsValue", 0m, rateCalcData.CustomsValue);

			entryLine.CL_CustomsValue = 11;
			AssertEquals("CustomsValue", 11m, rateCalcData.CustomsValue);
		}
	}
}
