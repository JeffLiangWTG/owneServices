using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class ReExportReductionDetailsUserControl : ZUserControl
	{
		public ReExportReductionDetailsUserControl()
		{
			InitializeComponent();
		}
		public void BindToMessageSendingObject()
		{
			BindingSource.DataSourceType = typeof(Business.JobDeclarationMiscMessageSendingObjectParent);
			BindingSource.SetBindingMember(CustomsOfficeCodeFindBox, "SendingObjectsCollection.MessageSendingEntryLines.ReExportCustomsOffice");
			BindingSource.SetBindingMember(DestCountryCodeFindBox, "SendingObjectsCollection.MessageSendingEntryLines.DestinationCountry");
			BindingSource.SetBindingMember(EstimateDateEdit, "SendingObjectsCollection.MessageSendingEntryLines.EstimateDate");
			CustomsOfficeCodeFindBox.ReadOnly = true;
			DestCountryCodeFindBox.ReadOnly = true;
			EstimateDateEdit.ReadOnly = true;
		}
	}
}
