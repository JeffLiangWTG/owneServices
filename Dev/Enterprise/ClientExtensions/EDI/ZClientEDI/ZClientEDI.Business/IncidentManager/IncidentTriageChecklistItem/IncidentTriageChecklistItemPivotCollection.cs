using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentTriageChecklistItemPivotCollection : DependentBusinessObjectCollection<IncidentTriageChecklistItemPivot, IncidentTriage>
	{
		public IncidentTriageChecklistItemPivotCollection(IncidentTriage triage, BusinessObjectFactory factory)
			: base(triage, factory)
		{
			Sort(IncidentTriageChecklistItemPivotSchema.Constants.IMP_Sequence);
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return IncidentTriageChecklistItemPivotSchema.IMP_IMT_Triage; }
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
