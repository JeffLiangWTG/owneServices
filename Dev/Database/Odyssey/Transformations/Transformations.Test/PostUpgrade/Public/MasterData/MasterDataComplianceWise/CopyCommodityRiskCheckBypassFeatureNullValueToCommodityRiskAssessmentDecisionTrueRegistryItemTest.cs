using System;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.MasterDataComplianceWise;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.PostUpgrade.Public.MasterData.MasterDataComplianceWise
{
	[TestedType(typeof(CopyCommodityRiskCheckBypassFeatureToCommodityRiskAssessmentDecisionRegistryItem))]
	public class CopyCommodityRiskCheckBypassFeatureNullValueToCommodityRiskAssessmentDecisionTrueRegistryItemTest : RegistryDataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			const string registryName = "AllowComplianceCommodityRiskAssessment";
			AssertEquals(0, Helper.GetStmDataRowCount(registryName));
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new CopyCommodityRiskCheckBypassFeatureToCommodityRiskAssessmentDecisionRegistryItem();

		protected override void PrepareTestData()
		{
			Helper.InsertStmDataRow("FeatureControlRuleContent", Guid.Empty, Guid.Empty,  "STR", StmDataSchema.SD_BinaryValue, null);
		}
	}
}
