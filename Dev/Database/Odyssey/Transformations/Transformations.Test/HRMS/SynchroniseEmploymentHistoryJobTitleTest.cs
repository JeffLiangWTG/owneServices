using System;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;
namespace Enterprise.DbUpgrader.Transformation.DataModification.Public.HRMS.Testing
{
	[TestedType(typeof(SynchroniseEmploymentHistoryJobTitle))]
	class SynchroniseEmploymentHistoryJobTitleTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new SynchroniseEmploymentHistoryJobTitle();
		protected override void PrepareTestData()
		{
			staff1Pk = Guid.NewGuid();
			staff2Pk = Guid.NewGuid();
			staff3Pk = Guid.NewGuid();
			var futureDate = DateTimeOffset.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss.fffffff zzz");
			var sqlInsertTemplate = $@"
			
			INSERT INTO dbo.GlbStaff([GS_PK], [GS_Code], [GS_LoginName], [GS_Title], [GS_SystemCreateTimeUtc], [GS_SystemCreateUser], [GS_SystemLastEditTimeUtc], [GS_SystemLastEditUser]) VALUES ('{staff1Pk}', 'Te1', 'StaffTest1', 'Test Title T1', GETDATE(), 'TU1', GETDATE(), 'TU2');
			INSERT INTO[dbo].GlbEmploymentHistory(GEH_PK, GEH_GS_Staff, GEH_EffectiveDate, GEH_JobTitle, GEH_IsApproved, GEH_SystemCreateTimeUtc, GEH_SystemCreateUser, GEH_SystemLastEditTimeUtc, GEH_SystemLastEditUser)
			VALUES
			(NEWID(), '{staff1Pk}', '2010-04-01', 'Test Title1', 1, GETDATE(), 'TU1', GETDATE(), 'TU2'),
			(NEWID(), '{staff1Pk}', '2018-10-06', 'Title1', 1, GETDATE(), 'TU1', GETDATE(), 'TU2'),
			(NEWID(), '{staff1Pk}', '2020-12-30', 'Latest Title1', 1, GETDATE(), 'TU1', GETDATE(), 'TU2'),
			(NEWID(), '{staff1Pk}', '2020-12-31', 'Unapproved', 0, GETDATE(), 'TU1', GETDATE(), 'TU2');
			
			INSERT INTO dbo.GlbStaff([GS_PK], [GS_Code], [GS_LoginName], [GS_Title], [GS_SystemCreateTimeUtc], [GS_SystemCreateUser], [GS_SystemLastEditTimeUtc], [GS_SystemLastEditUser]) VALUES ('{staff2Pk}', 'Te2', 'StaffTest2', 'Test Title T2', GETDATE(), 'TU1', GETDATE(), 'TU2');
			INSERT INTO[dbo].GlbEmploymentHistory(GEH_PK, GEH_GS_Staff, GEH_EffectiveDate, GEH_JobTitle, GEH_IsApproved, GEH_SystemCreateTimeUtc, GEH_SystemCreateUser, GEH_SystemLastEditTimeUtc, GEH_SystemLastEditUser)
			VALUES
			(NEWID(), '{staff2Pk}', '2010-04-01', 'Test Title2', 1, GETDATE(), 'TU1', GETDATE(), 'TU2'),
			(NEWID(), '{staff2Pk}', '2018-10-02', 'Latest Title2', 1, GETDATE(), 'TU1', GETDATE(), 'TU2'),
			(NEWID(), '{staff2Pk}', '2018-10-03', 'Unapproved', 0, GETDATE(), 'TU1', GETDATE(), 'TU2'),
			(NEWID(), '{staff2Pk}', '{futureDate}', 'Future Title', 1, GETDATE(), 'TU1', GETDATE(), 'TU2');

			INSERT INTO dbo.GlbStaff([GS_PK], [GS_Code], [GS_LoginName], [GS_Title], [GS_SystemCreateTimeUtc], [GS_SystemCreateUser], [GS_SystemLastEditTimeUtc], [GS_SystemLastEditUser]) VALUES ('{staff3Pk}', 'Te3', 'StaffTest3', 'Test Title T3', GETDATE(), 'TU1', GETDATE(), 'TU2');
			INSERT INTO[dbo].GlbEmploymentHistory(GEH_PK, GEH_GS_Staff, GEH_EffectiveDate, GEH_JobTitle, GEH_IsApproved, GEH_SystemCreateTimeUtc, GEH_SystemCreateUser, GEH_SystemLastEditTimeUtc, GEH_SystemLastEditUser)
			VALUES
			(NEWID(), '{staff3Pk}', '2010-04-01', 'Test Title3', 1, GETDATE(), 'TU1', GETDATE(), 'TU2'),
			(NEWID(), '{staff3Pk}', '2018-10-02', 'Latest Title3', 1, GETDATE(), 'TU1', GETDATE(), 'TU2'),
			(NEWID(), '{staff3Pk}', '2018-10-03', 'Unapproved', 0, GETDATE(), 'TU1', GETDATE(), 'TU2');
			";
			TestConnection.ExecuteNonQuery(sqlInsertTemplate);
		}
		protected override void AssertTransformationResults()
		{
			var selectTemplate = @"SELECT GS_Title FROM dbo.GlbStaff WHERE GS_PK = '{0}'";
			var actualStaffJobTitle = TestConnection.ExecuteScalar<string>(string.Format(selectTemplate, staff1Pk));
			var actualStaffJobTitle2 = TestConnection.ExecuteScalar<string>(string.Format(selectTemplate, staff2Pk));
			var actualStaffJobTitle3 = TestConnection.ExecuteScalar<string>(string.Format(selectTemplate, staff3Pk));
			CombineAssertions(() =>
			{
				AssertEquals("Update Staff GS_Title to most recent GEH_JobTitle in EmploymentHistory", "Latest Title1", actualStaffJobTitle);
				AssertEquals("Update Staff GS_Title2 to most recent GEH_JobTitle2 in EmploymentHistory", "Latest Title2", actualStaffJobTitle2);
				AssertEquals("Update Staff GS_Title3 to most recent GEH_JobTitle3 in EmploymentHistory", "Latest Title3", actualStaffJobTitle3);
			});
		}
		Guid staff1Pk = Guid.Empty;
		Guid staff2Pk = Guid.Empty;
		Guid staff3Pk = Guid.Empty;
	}
}
