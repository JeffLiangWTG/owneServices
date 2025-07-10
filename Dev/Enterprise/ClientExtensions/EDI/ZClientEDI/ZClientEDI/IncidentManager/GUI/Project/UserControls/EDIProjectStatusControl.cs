
namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class EDIProjectStatusControl : ProcessManagement.GUI.ProjectStatusControl
	{
		public EDIProjectStatusControl()
		{
			InitializeComponent();
			VisibilityRelationshipProvider.SetDependency(InstallDateEdit, PlannedInstallDateEdit);
			VisibilityRelationshipProvider.SetDependency(GoLiveCompleteDateEdit, PlannedGoLiveDateEdit);
		}
	}
}
