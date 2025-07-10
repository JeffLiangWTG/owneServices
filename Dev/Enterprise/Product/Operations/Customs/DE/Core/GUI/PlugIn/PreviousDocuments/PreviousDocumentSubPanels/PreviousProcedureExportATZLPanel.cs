using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.PlugIn
{
	public partial class PreviousProcedureExportATZLPanel : ZUserControl
	{
		public PreviousProcedureExportATZLPanel(string prefix)
		{
			InitializeComponent();
			this.BindingSource.SetBindingMember(LocalReferenceTextBox, prefix + ".PreviousProcedureMaster.CSI_ReferenceNumber2");
			this.BindingSource.SetBindingMember(AuthorizationNumberDropDown, prefix + ".PreviousProcedureMaster.AuthorizationNumber");
		}
	}
}
