using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentDiagnosticCriteriaTriagePivotCollection : DependentBusinessObjectCollection<IncidentTriageDiagnosticCriteriaPivot, IncidentDiagnosticCriteria>
	{
		public IncidentDiagnosticCriteriaTriagePivotCollection(IncidentDiagnosticCriteria criteria, BusinessObjectFactory factory)
			: base(criteria, factory)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => IncidentTriageDiagnosticCriteriaPivotSchema.IMO_IMD_DiagnosticCriteria;

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
