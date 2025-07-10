using System;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.MasterDataComplianceWise;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.PreUpgrade.Testing
{
	[TestedType(typeof(UpdateComplianceCommodityRiskStatusToNotChecked))]
	class UpdateComplianceCommodityRiskStatusToNotCheckedTest : DataTransformationTestCase
	{
		readonly Guid complianceCommodityDetailPK = Guid.NewGuid();
		readonly Guid compliancePK = Guid.NewGuid();

		protected override void PrepareTestData()
		{
			new DbColumnDependencyRemover(ComplianceCommodityDetailSchema.Constants.TableName, ComplianceCommodityDetailSchema.Constants.CCD_RiskStatus).DropRelateObjects(TestConnection);

			TestConnection.ExecuteNonQuery($@"
ALTER TABLE dbo.ComplianceCommodityDetail DROP CONSTRAINT IF EXISTS Constraint_CCD_RiskStatus

ALTER TABLE dbo.ComplianceCommodityDetail WITH NOCHECK
ADD CONSTRAINT [Constraint_CCD_RiskStatus] CHECK ([CCD_RiskStatus] IN ('UNK', 'PSK', 'CLR', 'REL', 'BLK'))

INSERT INTO dbo.ComplianceRiskStatus
(COR_PK, COR_ParentTableCode, COR_ParentID, COR_PartyRisk, COR_LocationRisk, COR_OverallRisk, COR_CommodityRisk, COR_SystemCreateTimeUtc, COR_SystemLastEditTimeUtc, COR_SystemCreateUser, COR_SystemLastEditUser)
VALUES
('{compliancePK}', 'JS', NEWID(), 'PSK', 'PSK', 'PSK', 'PSK', GETUTCDATE(), GETUTCDATE(), '~BP', '~BP')
INSERT INTO dbo.ComplianceCommodityDetail
(CCD_PK, CCD_COR_ComplianceRisk, CCD_HarmonizedCode, CCD_CountryOrGrouping, CCD_SystemCreateTimeUtc, CCD_SystemCreateUser, CCD_SystemLastEditTimeUtc, CCD_SystemLastEditUser, CCD_RiskStatus, CCD_AssessmentNotes)
VALUES
('{complianceCommodityDetailPK}', '{compliancePK}', '8485', 'AU', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP', 'UNK', 'TEST')");
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateComplianceCommodityRiskStatusToNotChecked();
		}

		protected override void AssertTransformationResults()
		{
			var commodityRiskStatus = TestConnection.ExecuteScalar<string>($"SELECT CCD_RiskStatus FROM dbo.ComplianceCommodityDetail WHERE CCD_PK='{complianceCommodityDetailPK}'");

			AssertEquals("Should have the not checked risk status code", "NCH", commodityRiskStatus);
		}
	}
}
