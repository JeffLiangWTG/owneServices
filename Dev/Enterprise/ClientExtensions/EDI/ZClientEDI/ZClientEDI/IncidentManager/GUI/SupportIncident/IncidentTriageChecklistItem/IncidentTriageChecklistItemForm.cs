using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class IncidentTriageChecklistItemForm : ZTemplateForm
	{
		public IncidentTriageChecklistItemForm(IncidentTriageChecklistItem incidentTriageChecklist)
			: base(incidentTriageChecklist)
		{
			ControllerID = Modules.ClientControllerRegistration.IncidentTriageChecklistItem;
		}

		internal IncidentTriageChecklistItem IncidentTriageChecklistItem => BusinessEntity as IncidentTriageChecklistItem;

		protected override bool SupportsEDocs => false;

		protected override bool ShowNotesTab => false;

#if DEBUG

		public ZTabControl TopLevelTabControl_Exposed => TopLevelTabControl;

#endif
	}
}
