using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.MasterDataComplianceWise
{
	class DeleteFreightComplianceRiskAssessmentRegistryItem : DeleteRegistryItem
	{
		protected override string[] GetRegistryItemNames() => new[] { "FreightComplianceRiskAssessment" };
	}
}
