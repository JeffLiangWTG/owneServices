using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class DiagnosticCriteriaLinkResponseResultPivotCollection : DependentBusinessObjectCollection<DiagnosticCriteriaInvestigationResult, DiagnosticCriteriaInvestigationItemLink>
	{
		public DiagnosticCriteriaLinkResponseResultPivotCollection(DiagnosticCriteriaInvestigationItemLink diagnosticCriteriaInvestigationItemLink, BusinessObjectFactory factory)
			: base(diagnosticCriteriaInvestigationItemLink, factory)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => DiagnosticCriteriaInvestigationResultSchema.DCR_DIL_ParentLink;

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
