using Enterprise.Client.EDI.Modules;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	/// <summary>
	/// The Generic contact panel with extra field for "Client Licence"
	/// </summary>
	public partial class EDIProjectContactControl : ProcessManagement.GUI.ProjectContactControl
	{
		public EDIProjectContactControl()
		{
			InitializeComponent();
			SetControlModuleIDs();
		}

		void SetControlModuleIDs()
		{
			LicenceGuidFindBox.ModuleID = ClientModuleRegistration.LicenceHeader;
		}
	}
}
