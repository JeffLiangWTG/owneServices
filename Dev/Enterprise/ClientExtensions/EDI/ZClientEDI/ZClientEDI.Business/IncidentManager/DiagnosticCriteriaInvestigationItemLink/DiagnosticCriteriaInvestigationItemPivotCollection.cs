using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class DiagnosticCriteriaInvestigationItemPivotCollection : DependentBusinessObjectCollection<DiagnosticCriteriaInvestigationItemLink, IncidentDiagnosticCriteria>
	{
		public DiagnosticCriteriaInvestigationItemPivotCollection(IncidentDiagnosticCriteria incidentDiagnosticCriteria, BusinessObjectFactory factory)
			: base(incidentDiagnosticCriteria, factory)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => DiagnosticCriteriaInvestigationItemLinkSchema.DIL_IMD_DiagnosticCriteria;

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
