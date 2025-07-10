using System;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.MasterDataComplianceWise;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.PostUpgrade.Public.MasterData.MasterDataComplianceWise
{
	[TestedType(typeof(UpdateEnableCompliancePotentialRiskMessageBannerRegistryItem))]
	public class UpdateEnableCompliancePotentialRiskMessageBannerRegistryItemTest : RegistryDataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertEquals(0, Helper.GetStmDataRowCount("EnableCompliancePotentialRiskMessageBanner"));
			AssertEquals(1, Helper.GetStmDataRowCount("EnableComplianceWarningMessage"));
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdateEnableCompliancePotentialRiskMessageBannerRegistryItem();

		protected override void PrepareTestData()
		{
			Helper.InsertStmDataRow("EnableCompliancePotentialRiskMessageBanner", Guid.Empty, Guid.Empty, true);
		}
	}
}
