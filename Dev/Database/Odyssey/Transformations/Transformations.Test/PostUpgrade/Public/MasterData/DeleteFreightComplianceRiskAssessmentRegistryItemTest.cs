using Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.MasterDataComplianceWise.Testing
{
	[TestedType(typeof(DeleteFreightComplianceRiskAssessmentRegistryItem))]
	class DeleteFreightComplianceRiskAssessmentRegistryItemTest : DeleteRegistryItemTest
	{
		protected override string[] GetRegistryItemNames() => new[] { "FreightComplianceRiskAssessment" };
	}
}
