using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.PlugIn
{
	public partial class PreviousProcedureExportATAVPanel : ZUserControl
	{
		public PreviousProcedureExportATAVPanel(string prefix)
		{
			InitializeComponent();
			BindingSource.SetBindingMember(SimplifiedGrantAuthorizationCheckBox, prefix + ".PreviousProcedureMaster.SimplifiedGrantAuthorizationFlag");
			BindingSource.SetBindingMember(MonitoringCustomsOfficeFindBox, prefix + ".PreviousProcedureMaster.CSI_CustomsOffice");
			BindingSource.SetBindingMember(AuthorizationNumberDropEdit, prefix + ".PreviousProcedureMaster.AuthorizationNumber");
		}
	}
}
