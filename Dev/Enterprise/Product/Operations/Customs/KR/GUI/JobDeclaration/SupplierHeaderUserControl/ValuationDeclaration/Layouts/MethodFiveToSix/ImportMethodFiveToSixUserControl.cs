using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class ImportMethodFiveToSixUserControl : ZUserControl
	{
		public ImportMethodFiveToSixUserControl()
		{
			InitializeComponent();
		}

		public void BindToMessageSendingObject()
		{
			BindingSource.DataSourceType = typeof(Business.ValuationDeclarationMessageSendingObjectParent);
			BindingSource.SetBindingMember(AmountAgreedUponWithCustomsKRWCalcEdit, "SendingObjectsCollection.InvoiceHeader.AmountAgreedUponWithCustomsKRW");
			BindingSource.SetBindingMember(AdditionalCostFreightToArrivalPortCalcEdit, "SendingObjectsCollection.InvoiceHeader.AdditionalCostFreightToArrivalPort");
			BindingSource.SetBindingMember(AdditionalCostFreightToDeparturePortCalcEdit, "SendingObjectsCollection.InvoiceHeader.AdditionalCostFreightToDeparturePort");
			BindingSource.SetBindingMember(AdditionalCostInsuranceCalcEdit, "SendingObjectsCollection.InvoiceHeader.AdditionalCostInsurance");
			BindingSource.SetBindingMember(AdditionalCostTotalAdditionalAmountCalcEdit, "SendingObjectsCollection.InvoiceHeader.AdditionalCostTotalAdditionalAmount");
		}
	}
}
