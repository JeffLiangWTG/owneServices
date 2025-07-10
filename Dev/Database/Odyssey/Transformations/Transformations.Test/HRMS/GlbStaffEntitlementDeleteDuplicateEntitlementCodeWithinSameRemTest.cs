using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.HRMS;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.HRMS;

[TestedType(typeof(GlbStaffEntitlementDeleteDuplicateEntitlementCodeWithinSameRem))]
[UseSnapshotProtection]
public class GlbStaffEntitlementDeleteDuplicateEntitlementCodeWithinSameRemTest : DataTransformationTestCase
{
	protected override DataTransformation GetNewTestTransformationInstance() =>
		new GlbStaffEntitlementDeleteDuplicateEntitlementCodeWithinSameRem();

	protected override void PrepareTestData()
	{
		var sqlInsertData = $@"

			DECLARE @GS_PK1 uniqueidentifier = NEWID();

			INSERT INTO dbo.GlbStaff([GS_PK], [GS_Code], [GS_LoginName], [GS_SystemCreateTimeUtc], [GS_SystemCreateUser], [GS_SystemLastEditTimeUtc], [GS_SystemLastEditUser])
			VALUES
				(@GS_PK1, 'Te1', 'StaffTest1', GETDATE(), 'TU1', GETDATE(), 'TU2');

			DECLARE @GSR_PK1 uniqueidentifier = NEWID();

			INSERT INTO [hrm].[GlbStaffRemuneration] ([GSR_PK], [GSR_GS_Staff], [GSR_FullTimeEquivalent], [GSR_RN_NKCountry], [GSR_RX_NKCurrency], [GSR_EffectiveDate], [GSR_AutoEffectiveEndDate], [GSR_LeaveLiabilityHourlyRate], [GSR_SystemCreateUser], [GSR_SystemCreateTimeUtc], [GSR_SystemLastEditUser], [GSR_SystemLastEditTimeUtc])
			VALUES
				(@GSR_PK1, @GS_PK1, 1.0, 'US', 'USD', '2015-06-01', NULL, 20.00, 'ZZN', GETDATE(), 'U02', GETDATE());

			ALTER INDEX FK_UX__GSI_GSR_Remuneration_GSI_EntitlementCode ON hrm.GlbStaffEntitlement DISABLE;

			INSERT INTO [hrm].[GlbStaffEntitlement] ([GSI_PK], [GSI_GSR_Remuneration], [GSI_EntitlementCode], [GSI_Value], [GSI_Comment], [GSI_Frequency], [GSI_GrantDate], [GSI_IsFTEScalable], [GSI_IsPartOfPackage], [GSI_SystemCreateUser], [GSI_SystemCreateTimeUtc], [GSI_SystemLastEditUser], [GSI_SystemLastEditTimeUtc])
			VALUES
				(NEWID(), @GSR_PK1, 'ABC', 100.00, 'Comment 01', 'OOF', NULL, 1, 0, 'ZZN', GETDATE(), 'U01', GETDATE()),
				(NEWID(), @GSR_PK1, 'ABC', 110.00, 'Comment 02', 'YRL', NULL, 0, 1, 'ZZN', GETDATE(), 'U02', GETDATE());
			";

		TestConnection.ExecuteNonQuery(sqlInsertData);
	}

	protected override void AssertTransformationResults()
	{
		AssertEquals("Should have no duplicate", 0, TestConnection.ExecuteScalar<int>("WITH DUP AS (SELECT GSI_GSR_Remuneration, GSI_EntitlementCode, COUNT(*) AS C FROM [hrm].[GlbStaffEntitlement] GROUP BY GSI_GSR_Remuneration, GSI_EntitlementCode HAVING COUNT(*) > 1) SELECT COUNT(1) from DUP"));
	}

	protected override void SetUp()
	{
		base.SetUp();
		disposableAdminConnection = ((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade();
	}

	protected override sealed void TearDown()
	{
		TestConnection.ExecuteNonQuery("ALTER INDEX FK_UX__GSI_GSR_Remuneration_GSI_EntitlementCode ON hrm.GlbStaffEntitlement REBUILD");
		disposableAdminConnection.Dispose();
		base.TearDown();
	}

	IDisposable disposableAdminConnection;
}
