using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.PlugIn
{
	public partial class PreviousDocumentsImportATAVPanel : ZUserControl
	{
		public PreviousDocumentsImportATAVPanel(string prefix)
		{
			InitializeComponent();
			this.BindingSource.SetBindingMember(this.SimplifiedGrantAuthorizationCheckBox, prefix + ".PreviousDocumentMaster.SimplifiedGrantAuthorizationFlag");
			this.BindingSource.SetBindingMember(this.MonitoringCustomsOfficeFindBox, prefix + ".PreviousDocumentMaster.CSI_CustomsOffice");
			this.BindingSource.SetBindingMember(this.AuthorizationNumberDropEdit, prefix + ".PreviousDocumentMaster.AuthorizationNumber");
		}
	}
}
