using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business
{
	public class Valuation1_MethodDataCreator : ValuationMethodDataCreator<Valuation1_MethodData>
	{
		protected override void PopulateMoreMethodData(Valuation1_MethodData valuation, JobComInvoiceHeader invoice, CurrencyConverter converter)
		{
			valuation.BaseAmountCurrency = invoice.JZ_RX_NKInvoice_Currency;
			valuation.IndirectPaymentAmount = invoice.IndirectAmount;
		}
	}
}
