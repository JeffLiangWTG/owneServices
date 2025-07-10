using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.MasterDataComplianceWise;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.PostUpgrade.Public.MasterData.MasterDataComplianceWise
{
	[TestedType(typeof(CopyCommodityRiskCheckBypassFeatureToCommodityRiskAssessmentDecisionRegistryItem))]
	public class CopyCommodityRiskCheckBypassFeatureNoRowToCommodityRiskAssessmentDecisionTrueRegistryItemTest : RegistryDataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			const string registryName = "AllowComplianceCommodityRiskAssessment";
			AssertEquals(0, Helper.GetStmDataRowCount(registryName));
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new CopyCommodityRiskCheckBypassFeatureToCommodityRiskAssessmentDecisionRegistryItem();

		protected override void PrepareTestData()
		{
		}
	}
}
