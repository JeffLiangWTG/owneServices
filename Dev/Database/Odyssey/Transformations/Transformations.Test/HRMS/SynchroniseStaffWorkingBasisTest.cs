using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Public.HRMS.Testing
{
	[TestedType(typeof(SynchroniseStaffWorkingBasis))]
	[UseSnapshotProtection]
	class SynchroniseStaffWorkingBasisTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new SynchroniseStaffWorkingBasis();

		protected override void PrepareTestData()
		{
			staff1Pk = Guid.NewGuid();
			staff2Pk = Guid.NewGuid();

			var futureDate = DateTimeOffset.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss.fffffff zzz");
			var sqlInsertTemplate = $@"
			
			INSERT INTO dbo.GlbStaff([GS_PK], [GS_Code], [GS_LoginName], [GS_EmploymentBasis], [GS_SystemCreateTimeUtc], [GS_SystemCreateUser], [GS_SystemLastEditTimeUtc], [GS_SystemLastEditUser]) VALUES ('{staff1Pk}', 'Te1', 'StaffTest1', 'TS1', GETDATE(), 'TU1', GETDATE(), 'TU2');
			INSERT INTO [hrm].[GlbStaffWorkingBasis] (GSW_PK, GSW_GS_Staff, GSW_EffectiveDate, GSW_WorkingBasis, GSW_SystemCreateTimeUtc, GSW_SystemCreateUser, GSW_SystemLastEditTimeUtc, GSW_SystemLastEditUser)
			VALUES
			(NEWID(), '{staff1Pk}', '2010-04-01', 'OT1', GETDATE(), 'TU1', GETDATE(), 'TU2'),
			(NEWID(), '{staff1Pk}', '2018-10-06', 'OT2', GETDATE(), 'TU1', GETDATE(), 'TU2'),
			(NEWID(), '{staff1Pk}', '2020-12-30', 'LT1', GETDATE(), 'TU1', GETDATE(), 'TU2');
			
			INSERT INTO dbo.GlbStaff([GS_PK], [GS_Code], [GS_LoginName], [GS_EmploymentBasis], [GS_SystemCreateTimeUtc], [GS_SystemCreateUser], [GS_SystemLastEditTimeUtc], [GS_SystemLastEditUser]) VALUES ('{staff2Pk}', 'Te2', 'StaffTest2', 'TS2', GETDATE(), 'TU1', GETDATE(), 'TU2');
			INSERT INTO [hrm].[GlbStaffWorkingBasis] (GSW_PK, GSW_GS_Staff, GSW_EffectiveDate, GSW_WorkingBasis, GSW_SystemCreateTimeUtc, GSW_SystemCreateUser, GSW_SystemLastEditTimeUtc, GSW_SystemLastEditUser)
			VALUES
			(NEWID(), '{staff2Pk}', '2010-04-01', 'OT1', GETDATE(), 'TU1', GETDATE(), 'TU2'),
			(NEWID(), '{staff2Pk}', '2018-10-06', 'LT2', GETDATE(), 'TU1', GETDATE(), 'TU2'),
			(NEWID(), '{staff2Pk}', '{futureDate}', 'FT1', GETDATE(), 'TU1', GETDATE(), 'TU2');
			";
			TestConnection.ExecuteNonQuery(sqlInsertTemplate);
		}

		protected override void AssertTransformationResults()
		{
			var selectTemplate = @"SELECT GS_EmploymentBasis FROM dbo.GlbStaff WHERE GS_PK = '{0}'";
			var actualEmploymentBasis = TestConnection.ExecuteScalar<string>(string.Format(selectTemplate, staff1Pk));
			var actualEmploymentBasis2 = TestConnection.ExecuteScalar<string>(string.Format(selectTemplate, staff2Pk));

			CombineAssertions(() =>
			{
				AssertEquals("Update Staff GS_EmploymentBasis1 to most recent GSW_WorkingBasis1 in StaffWorkingBasis", "LT1", actualEmploymentBasis);
				AssertEquals("Update Staff GS_EmploymentBasis2 to most recent GSW_WorkingBasis2 in StaffWorkingBasis", "LT2", actualEmploymentBasis2);
			});
		}

		Guid staff1Pk = Guid.Empty;
		Guid staff2Pk = Guid.Empty;

		protected override void SetUp()
		{
			base.SetUp();

			restoreConnection = ((IDbUpgradeSupport)Db.Instance).ElevateToAdminConnectionForUpgrade();
		}

		protected override void TearDown()
		{
			restoreConnection?.Dispose();
			base.TearDown();
		}

		IDisposable restoreConnection;
	}
}
