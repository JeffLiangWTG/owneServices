using System.Linq;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(StatisticalValueFeeCalculator))]
	sealed class StatisticalValueFeeCalculatorTest : TestCaseWithFactory
	{
		public void TestBaseValueisSetFromCLCalcStatisticalBasisExcludingSTACharge()
		{
			var dec = Factory.New<JobDeclaration>();
			var entryHeader = dec.ActiveEntryHeaders.AddNew();
			var entryLine = Factory.New<CusEntryLine>();
			entryLine.CL_CH = entryHeader.PK;

			dec.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = dec.LocalCurrencyCode;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 1000m;
			invoiceLine.JI_LinePrice = 1000m;

			AssertEquals(invoiceLine.JI_Calc_StatisticalValue, 1000m);
			invoiceLine.ZG_StatisticalValueManualOverride = false;

			var staCharge = invoiceLine.Charges.AddNew();
			staCharge.J7_ChargeType = "STA";
			staCharge.J7_RX_NKCurrency = dec.LocalCurrencyCode;
			staCharge.J7_Amount = 200m;

			AssertEquals("Stat value  = 1000+100= 1100", 1200m, invoiceLine.JI_Calc_StatisticalValue);
			AssertEquals(invoiceLine.JI_Calc_StatisticalBasisExcludingSTACharge, 1000m);

			invoiceLine.JI_CL = entryLine.PK;

			AssertEquals(entryLine.CL_Calc_StatisticalBasisExcludingSTACharge, 1000m);

			var calculatedFees = new StatisticalValueFeeCalculator(entryLine).CalculateExtraFees().Single();

			AssertEquals("basevalue is base on CL_Calc_StatisticalBasisExcludingSTACharge", 1000m, calculatedFees.BaseValue);
		}
	}
}
