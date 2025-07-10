using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class IncidentDiagnosticCriteriaForm : ZTemplateForm
	{
		public IncidentDiagnosticCriteriaForm(IncidentDiagnosticCriteria incidentDiagnosticCriteria)
			: base(incidentDiagnosticCriteria)
		{
		}

		protected override bool SupportsEDocs => true;

		protected override bool ShowNotesTab => false;

		internal IncidentDiagnosticCriteria incidentDiagnosticCriteria => BusinessEntity as IncidentDiagnosticCriteria;
	}
}
