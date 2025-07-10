using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business
{
	public class Valuation4_MethodDataCreator : ValuationMethodDataCreator<Valuation4_MethodData>
	{
		protected override void PopulateMoreMethodData(Valuation4_MethodData valuation, JobComInvoiceHeader invoice, CurrencyConverter converter)
		{
			valuation.BaseAmountCurrency = invoice.JZ_RX_NKInvoice_Currency;
			valuation.DeductionCustomsReferenceNo = invoice.CustomsReferenceNumber;
			valuation.DeductionGeneralCostPercentage = invoice.JZ_DeductionRate;
			valuation.DeductionGeneralCostPercentageType = invoice.JZ_DeductionType;
		}
	}
}
