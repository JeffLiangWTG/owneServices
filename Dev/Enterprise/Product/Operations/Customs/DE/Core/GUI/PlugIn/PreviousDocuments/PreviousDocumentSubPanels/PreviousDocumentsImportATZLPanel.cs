using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.PlugIn
{
	public partial class PreviousDocumentsImportATZLPanel : ZUserControl
	{
		public PreviousDocumentsImportATZLPanel(string prefix)
		{
			InitializeComponent();
			this.BindingSource.SetBindingMember(this.localReferenceTextBox, prefix + ".PreviousDocumentMaster.CSI_ReferenceNumber2");
			this.BindingSource.SetBindingMember(this.authorizationNumberDropDown, prefix + ".PreviousDocumentMaster.AuthorizationNumber");
		}
	}
}
