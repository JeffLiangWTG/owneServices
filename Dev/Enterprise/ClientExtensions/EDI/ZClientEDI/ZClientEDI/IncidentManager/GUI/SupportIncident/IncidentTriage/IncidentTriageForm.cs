using System.Windows.Forms;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class IncidentTriageForm : ZTemplateForm
	{
		public IncidentTriageForm(IncidentTriage incidentTriage)
			: base(incidentTriage)
		{
			ControllerID = ClientControllerRegistration.IncidentTriage;
			WorkflowTabPage.Initialize(incidentTriage);
			PlugIns.Add(ControllerIDs.Audit);
		}

		protected override bool SupportsEDocs => false;

		protected override bool ShowNotesTab => false;

		internal IncidentTriage IncidentTriage => BusinessEntity as IncidentTriage;

		#region For Testing
#if DEBUG

		public ZTabControl TopLevelTabControl_Exposed => TopLevelTabControl;

		public EDIWorkflowTabPage ExposedWorkflowTabPageForTest => WorkflowTabPage;

#endif
		#endregion

		void ShowMenuItemsLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var finder = new SourceModuleFinder(IncidentTriage.IMT_Product, IncidentTriage.ModuleType, "", IncidentTriage.Factory);
			var form = new SourceModuleFinderForm(finder) { ShowCloseButtonOnly = true };
			ZFormModaliser.ShowDialogAndDispose(form);
		}
	}
}
