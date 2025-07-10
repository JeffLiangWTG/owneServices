using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentTriageDiagnosticCriteriaPivotCollection : DependentBusinessObjectCollection<IncidentTriageDiagnosticCriteriaPivot, IncidentTriage>
	{
		public IncidentTriageDiagnosticCriteriaPivotCollection(IncidentTriage triage, BusinessObjectFactory factory)
			: base(triage, factory)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => IncidentTriageDiagnosticCriteriaPivotSchema.IMO_IMT_Triage;

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
