using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public static class InvoiceLineDetailsUserControlExtensionMethod
	{
		public static void SetTariffFindBoxDelegates(this ZUserControl userControl, TariffFindBox tariffFindBox)
		{
			tariffFindBox.GetTariffType = () => userControl.CurrentDataItem is JobComInvoiceLine line ? line.UniversalTariffType : ZString.Empty;
			tariffFindBox.GetDataGrouping = () => userControl.CurrentDataItem is JobComInvoiceLine line ? line.GetDefaultDataGroupingCode() : ZString.Empty;
			tariffFindBox.GetEffectiveDate = () => userControl.CurrentDataItem is JobComInvoiceLine line ? line.EffectiveAssessmentDate : ZDateTime.Today;
		}
	}
}
