using CargoWise.Common;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

public class CHConditionCalcDataForInvoiceLineTest : TestCaseWithFactory
{
	public void TestCountrySpecificValueList()
	{
		var invoice = Factory.New<JobComInvoiceHeader>();
		invoice.JZ_IncoTerm = "FOB";
		var invoiceLine = invoice.InvoiceLines.AddNew();

		invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Switzerland;
		invoiceLine.JI_LinePrice = 100;

		var calcData = new CHConditionCalcDataForInvoiceLine(invoiceLine);

		AssertEquals(nameof(UniversalReferenceConstants.FormulaPlaceholder.StatisticalValue), 100m, calcData.CountrySpecificValueList.GetValueSafe(UniversalReferenceConstants.FormulaPlaceholder.StatisticalValue));
	}
}
