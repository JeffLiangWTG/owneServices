using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.EInvoicing.PenaltyTaxMessage
{
	public sealed class KoreaSouthEInvoicingPenaltyTaxInfoFormProvider : IEInvoicingPenaltyTaxInfoProvider
	{
		public void ShowPenaltyTaxInfoForm()
		{
			var penaltyTaxInfo = new KoreaSouthEInvoicingPenaltyTaxInfo();
			ZFormModaliser.ShowDialogAndDispose(new KoreaSouthEInvoicingPenaltyTaxInfoForm(penaltyTaxInfo));
		}

		public string PenaltyTaxInfoMenuName => ResString.GetMultilingualString("aed58f9a-fad4-4e22-9924-e0ee9eef897d", "Additional Tax Information");
	}
}
