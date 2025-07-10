using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class InvestigationItemDiagnosticCriteriaPivotCollection : DependentBusinessObjectCollection<DiagnosticCriteriaInvestigationItemLink, InvestigationItem>
	{
		public InvestigationItemDiagnosticCriteriaPivotCollection(InvestigationItem investigationItem, BusinessObjectFactory factory)
			: base(investigationItem, factory)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => DiagnosticCriteriaInvestigationItemLinkSchema.DIL_INV_InvestigationItem;

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
