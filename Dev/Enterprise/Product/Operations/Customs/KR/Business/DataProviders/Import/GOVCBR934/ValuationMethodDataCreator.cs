using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business
{
	public class ValuationMethodDataCreator : ValuationMethodDataCreator<ValuationMethodData>
	{
		protected override void PopulateMoreMethodData(ValuationMethodData valuation, JobComInvoiceHeader invoice, CurrencyConverter converter)
		{
			valuation.BaseAmountCurrency = Core.Constants.CurrencyCodes.KoreaRepublicOf;
		}
	}
}
