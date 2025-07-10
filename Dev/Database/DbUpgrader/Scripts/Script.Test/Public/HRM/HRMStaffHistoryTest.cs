using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.HRM;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.HRM
{
	[TestedType(typeof(HRMStaffHistory))]
	class HRMStaffHistoryTest : DbCreateScriptTest
	{
		public void TestExecute()
		{
			var staff0 = Guid.NewGuid();
			var staff1 = Guid.NewGuid();
			var staff2 = Guid.NewGuid();
			var staff3 = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) 
VALUES
	(@staff0, 'GS1', 'Staff1', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@staff1, 'GS2', 'Staff2', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@staff2, 'GS3', 'Staff3', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@staff3, 'GS4', 'Staff4', GETUTCDATE(), 'E', GETUTCDATE(), 'E');

INSERT INTO dbo.GlbStaffManager
	(GSM_PK, GSM_ManagerType, GSM_GS_Manager, GSM_GS_Staff, GSM_EffectiveDate, GSM_EndDate, GSM_SystemCreateTimeUtc, GSM_SystemLastEditTimeUtc, GSM_SystemCreateUser, GSM_SystemLastEditUser)
VALUES
	(newid(), 'PPL', @staff0, @staff1, '2021-07-28', null, GETDATE(), GETDATE(), 'E', 'E'),
	(newid(), 'HRM', @staff0, @staff2, '2021-07-28', null, GETDATE(), GETDATE(), 'E', 'E'),
	(newid(), 'DRM', @staff0, @staff3, '2021-07-28', null, GETDATE(), GETDATE(), 'E', 'E');

--Prep for GlbEmploymentHistory
DECLARE @jobRole1Pk UNIQUEIDENTIFIER = NEWID();
DECLARE @jobRole2Pk UNIQUEIDENTIFIER = NEWID();
DECLARE @jobRole3Pk UNIQUEIDENTIFIER = NEWID();
DECLARE @jobRole4Pk UNIQUEIDENTIFIER = NEWID();
DECLARE @jobRole5Pk UNIQUEIDENTIFIER = NEWID();

INSERT INTO dbo.HRJobRole (HJ_PK, HJ_JobTitle, HJ_JobRoleDescription, HJ_IsTemplated, HJ_IsActive, HJ_SystemCreateTimeUtc, HJ_SystemCreateUser, HJ_SystemLastEditTimeUtc, HJ_SystemLastEditUser)
VALUES
	(@jobRole1Pk, 'JobRoleDescriptionFromHRJobRole1', 'TestJobTitleFromHRJobRole1', 0, 1, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@jobRole2Pk, 'JobRoleDescriptionFromHRJobRole2', 'TestJobTitleFromHRJobRole2', 0, 1, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@jobRole3Pk, 'JobRoleDescriptionFromHRJobRole3', 'TestJobTitleFromHRJobRole3', 0, 1, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@jobRole4Pk, 'JobRoleDescriptionFromHRJobRole4', 'TestJobTitleFromHRJobRole4', 0, 1, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@jobRole5Pk, 'JobRoleDescriptionFromHRJobRole5', 'TestJobTitleFromHRJobRole5', 0, 1, GETUTCDATE(), 'E', GETUTCDATE(), 'E')

--Prep for GlbEmployingBranchDepartment
DECLARE @homeCompany1PK UNIQUEIDENTIFIER = NEWID();
DECLARE @homeBranch1PK UNIQUEIDENTIFIER = NEWID();
DECLARE @homeBranch2PK UNIQUEIDENTIFIER = NEWID();
DECLARE @homeBranch3PK UNIQUEIDENTIFIER = NEWID();
DECLARE @homeDepartment1PK UNIQUEIDENTIFIER = NEWID();
DECLARE @homeDepartment2PK UNIQUEIDENTIFIER = NEWID();
DECLARE @homeDepartment3PK UNIQUEIDENTIFIER = NEWID();

DECLARE @benCompany1PK UNIQUEIDENTIFIER = NEWID();
DECLARE @benBranch1PK UNIQUEIDENTIFIER = NEWID();
DECLARE @benBranch2PK UNIQUEIDENTIFIER = NEWID();
DECLARE @benBranch3PK UNIQUEIDENTIFIER = NEWID();
DECLARE @benDepartment1PK UNIQUEIDENTIFIER = NEWID();
DECLARE @benDepartment2PK UNIQUEIDENTIFIER = NEWID();
DECLARE @benDepartment3PK UNIQUEIDENTIFIER = NEWID();

INSERT INTO dbo.GlbCompany(GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name) 
VALUES
	(@homeCompany1PK, 'US', 'USD', 'HOC', 'US company'),
	(@benCompany1PK, 'AU', 'AUD', 'BEC', 'AU company')

INSERT INTO dbo.GlbBranch(GB_PK, GB_GC, GB_BranchName, GB_Code)
VALUES
	(@homeBranch1PK, @homeCompany1PK, 'HomeBranch1', 'HB1'),
	(@homeBranch2PK, @homeCompany1PK, 'HomeBranch2', 'HB2'),
	(@homeBranch3PK, @homeCompany1PK, 'HomeBranch3','HB3'),
	(@benBranch1PK, @benCompany1PK, 'BeneficiaryBranch1', 'BB1'),
	(@benBranch2PK, @benCompany1PK, 'BeneficiaryBranch2','BB2'),
	(@benBranch3PK, @benCompany1PK, 'BeneficiaryBranch3','BB3');

INSERT INTO dbo.GlbDepartment (GE_PK, GE_Code, GE_Desc) 
values
	(@homeDepartment1PK, 'HD1', 'HD1DES'),
	(@homeDepartment2PK, 'HD2', 'HD2DES'),
	(@homeDepartment3PK, 'HD3', 'HD3DES'),
	(@benDepartment1PK, 'BD1', 'BD1DES'),
	(@benDepartment2PK, 'BD2', 'BD2DES'),
	(@benDepartment3PK, 'BD3', 'BD3DES');

--GlbEmploymentHistory
INSERT INTO dbo.GlbEmploymentHistory (GEH_PK, GEH_GS_Staff, GEH_EffectiveDate, GEH_JobTitle, GEH_HJ_JobRole, GEH_JobFamily, GEH_IsInternalPosition, GEH_WorksOutsideBranch, GEH_JobDescription, GEH_EmploymentType, GEH_CompanyName, GEH_DepartureReason, GEH_DepartureComments, GEH_SystemCreateTimeUtc, GEH_SystemCreateUser, GEH_SystemLastEditTimeUtc, GEH_SystemLastEditUser)
VALUES
	(NEWID(), @staff0, '2015-05-07', 'TestJobTitle1', @jobRole1Pk, 'JF1', 0, 0, 'TestJobDescription1', 'PER', 'TestCompanyName1', 'TD1', 'TestDepartureComment1', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(NEWID(), @staff0, '2015-05-13', 'TestJobTitle2', @jobRole2Pk, 'JF2', 0, 0, 'TestJobDescription2', 'PER', 'TestCompanyName2', 'TD2', 'TestDepartureComment2', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(NEWID(), @staff0, '2015-05-17', 'TestJobTitle3', @jobRole3Pk, 'JF3', 0, 0, 'TestJobDescription3', 'PER', 'TestCompanyName3', 'TD3', 'TestDepartureComment3', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(NEWID(), @staff0, '2015-06-01', 'TestJobTitle4', @jobRole4Pk, 'JF4', 0, 0, 'TestJobDescription4', 'PER', 'TestCompanyName4', 'TD4', 'TestDepartureComment4', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(NEWID(), @staff0, '2015-06-17', 'TestJobTitle5', @jobRole5Pk, 'JF5', 0, 0, 'TestJobDescription5', 'PER', 'TestCompanyName5', 'TD5', 'TestDepartureComment5', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),

	(NEWID(), @staff1, '2015-06-01', 'TestJobTitle6', @jobRole1Pk, 'JF1', 0, 0, 'TestJobDescription6', 'PER', 'TestCompanyName6', 'TD6', 'TestDepartureComment6', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(NEWID(), @staff1, '2015-06-17', 'TestJobTitle7', @jobRole2Pk, 'JF2', 0, 0, 'TestJobDescription7', 'PER', 'TestCompanyName7', 'TD7', 'TestDepartureComment7', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),

	(NEWID(), @staff2, '2016-06-01', 'TestJobTitle8', @jobRole1Pk, 'JF3', 0, 0, 'TestJobDescription8', 'PER', 'TestCompanyNameA', 'TDa', 'TestDepartureComment61', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(NEWID(), @staff2, '2017-06-17', 'TestJobTitle9', @jobRole2Pk, 'JF4', 0, 0, 'TestJobDescription9', 'PER', 'TestCompanyNameB', 'TDb', 'TestDepartureComment71', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),

	(NEWID(), @staff3, '2018-06-01', 'TestJobTitle10', @jobRole1Pk, 'JF5', 0, 0, 'TestJobDescription0', 'PER', 'TestCompanyNameC', 'TDc', 'TestDepartureComment62', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(NEWID(), @staff3, '2019-06-17', 'TestJobTitle11', @jobRole2Pk, 'JF6', 0, 0, 'TestJobDescription1', 'PER', 'TestCompanyNameD', 'TDd', 'TestDepartureComment72', GETUTCDATE(), 'E', GETUTCDATE(), 'E');


--GlbEmploymentTeam
INSERT INTO dbo.GlbEmploymentTeam (GET_PK, GET_GS_Staff, GET_EffectiveDate, GET_GST_NKTeamCode, GET_SystemCreateTimeUtc, GET_SystemCreateUser, GET_SystemLastEditTimeUtc, GET_SystemLastEditUser)
VALUES
	(NEWID(), @staff0, '2015-05-05 10:05:23', 'TN1', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(NEWID(), @staff0, '2015-05-16 10:05:23', 'TN2', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(NEWID(), @staff0, '2015-05-21 10:05:23', 'TN3', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(NEWID(), @staff0, '2015-05-31 10:05:23', 'TN4', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(NEWID(), @staff1, '2015-05-30 10:05:23', 'TN5', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(NEWID(), @staff1, '2015-06-16 10:05:23', 'TN6', GETUTCDATE(), 'E', GETUTCDATE(), 'E');

--GlbEmployingBranchDepartment
INSERT INTO dbo.GlbEmployingBranchDepartment (GHB_PK, GHB_GS_Staff, GHB_EffectiveDate, GHB_GE_Department, GHB_GB_Branch, GHB_SystemCreateTimeUtc, GHB_SystemCreateUser, GHB_SystemLastEditTimeUtc, GHB_SystemLastEditUser)
VALUES
	(NEWID(), @staff0, '2015-05-04 10:05:23', @homeDepartment1PK, @homeBranch1PK, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(NEWID(), @staff0, '2015-05-17 10:05:23', @homeDepartment2PK, @homeBranch2PK, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(NEWID(), @staff0, '2015-06-01 10:05:23', @homeDepartment3PK, @homeBranch3PK, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(NEWID(), @staff1, '2015-05-31 10:05:23', @homeDepartment1PK, @homeBranch1PK, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(NEWID(), @staff1, '2015-06-21 10:05:23', @homeDepartment2PK, @homeBranch2PK, GETUTCDATE(), 'E', GETUTCDATE(), 'E');

--GlbBeneficiaryBranchDepartment
INSERT INTO dbo.GlbBeneficiaryBranchDepartment (GBB_PK, GBB_GS_Staff, GBB_EffectiveDate, GBB_GB_Branch, GBB_GE_Department, GBB_SystemCreateTimeUtc, GBB_SystemCreateUser, GBB_SystemLastEditTimeUtc, GBB_SystemLastEditUser)
VALUES
	(NEWID(), @staff0, '2015-05-02 10:05:23', @benBranch1PK, @benDepartment1PK, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(NEWID(), @staff0, '2015-05-19 10:05:23', @benBranch2PK, @benDepartment2PK, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(NEWID(), @staff0, '2015-05-30 10:05:23', @benBranch3PK, @benDepartment3PK, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(NEWID(), @staff1, '2015-06-02 10:05:23', @benBranch1PK, @benDepartment1PK, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(NEWID(), @staff1, '2015-06-17 10:05:23', @benBranch2PK, @benDepartment2PK, GETUTCDATE(), 'E', GETUTCDATE(), 'E');

--GlbEmploymentLocation
INSERT INTO dbo.GlbEmploymentLocation (GEL_PK, GEL_GS_Staff, GEL_EffectiveDate, GEL_LocationSource, GEL_GB_SourceBranch, GEL_Address1, GEL_Address2, GEL_City, GEL_State, GEL_PostCode, GEL_RN_NKCountryCode, GEL_ValidationStatus, GEL_SystemCreateTimeUtc, GEL_SystemCreateUser, GEL_SystemLastEditTimeUtc, GEL_SystemLastEditUser)
VALUES
	(NEWID(), @staff0, '2015-04-30 10:05:23', 'WFO', @benBranch1PK, '72 ORiordan Street', 'AlexandriOne', 'Sydney', 'NSW', '2015', 'AU', 'MAN', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(NEWID(), @staff0, '2015-05-15 10:05:23', 'OTH', NULL, '73 ORiordan Street', 'AlexandriTwo', 'Sydney', 'NSW', '2016', 'AU', 'MAN', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(NEWID(), @staff0, '2015-05-21 10:05:23', 'OTH', NULL, '74 ORiordan Street', 'AlexandriThree', 'Sydney', 'NSW', '2017', 'AU', 'MAN', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(NEWID(), @staff0, '2015-06-17 10:05:23', 'OTH', NULL, '75 ORiordan Street', 'AlexandriFour', 'Sydney', 'NSW', '2018', 'AU', 'MAN', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(NEWID(), @staff1, '2015-06-15 10:05:23', 'OTH', NULL, '72 ORiordan Street', 'AlexandriOne', 'Sydney', 'NSW', '2015', 'AU', 'MAN', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(NEWID(), @staff1, '2015-06-20 10:05:23', 'WFO', @benBranch3PK, '73 ORiordan Street', 'AlexandriTwo', 'Sydney', 'NSW', '2016', 'AU', 'MAN', GETUTCDATE(), 'E', GETUTCDATE(), 'E');

			", p =>
			{
				p.AddParameter("@staff0", SqlDbType.UniqueIdentifier, staff0);
				p.AddParameter("@staff1", SqlDbType.UniqueIdentifier, staff1);
				p.AddParameter("@staff2", SqlDbType.UniqueIdentifier, staff2);
				p.AddParameter("@staff3", SqlDbType.UniqueIdentifier, staff3);
			});

			var results = Execute();
			var resultRows = results.Select();

			DataTable baseview;
			using (var command = TestConnection.Command("SELECT * FROM hrm.HRMStaffHistoryBase"))
			{
				baseview = DataUtils.GetDataTableFromCommand(command);
			}

			var hrmColumns = new[] { "IsSelf", "IsManaged1", "IsManaged2", "IsManaged3" };

			AssertEquals(baseview.Columns.Count + hrmColumns.Length, results.Columns.Count);
			foreach (var column in baseview.Columns)
			{
				Assert(results.Columns.Contains((column as DataColumn).ColumnName));
			}
			foreach (var colName in hrmColumns)
			{
				Assert(results.Columns.Contains(colName));
			}

			var flags = new[] { false, true };
			var securityColumnValues =
				from isSelf in flags
				from isManaged1 in flags
				from isManaged2 in flags
				from isManaged3 in flags
				select new object[] { isSelf, isManaged1, isManaged2, isManaged3 };

			foreach (var baseviewRow in baseview.Select())
			{
				var matchedRows = resultRows.Where(r => r["EHV_PK"].Equals(baseviewRow["EHV_PK"]));
				AssertEquals(16, matchedRows.Count());

				matchedRows.ForEach(row =>
				{
					foreach (var col in baseview.Columns)
					{
						var colName = (col as DataColumn).ColumnName;
						AssertEquals(colName, baseviewRow[colName], row[colName]);
					}
				});

				foreach (var securityValues in securityColumnValues)
				{
					var matchedRow = matchedRows.
						FirstOrDefault(r => r["IsSelf"].Equals(securityValues[0]) &&
											r["IsManaged1"].Equals(securityValues[1]) &&
											r["IsManaged2"].Equals(securityValues[2]) &&
											r["IsManaged3"].Equals(securityValues[3]));

					AssertNotNull(matchedRow);
				}
			}
		}

		public void TestSecurityColumnTypesNotNull()
		{
			var securityColumns = HRSecurityColumns.GetColumnNames();

			DataTable result;
			using (var command = TestConnection.Command($@"
SELECT 
    c.name AS column_name,
    type_name(user_type_id) AS data_type,
    c.is_nullable
FROM 
    sys.columns c
    JOIN sys.views v ON v.object_id = c.object_id
WHERE 
    schema_name(v.schema_id) = 'hrm'
    AND object_name(c.object_id) = 'HRMStaffHistory'
	AND c.name IN ('{string.Join("', '", securityColumns)}')
"))
			{
				result = DataUtils.GetDataTableFromCommand(command);
			}
			var valueArrays = result.Rows.Cast<DataRow>().Select(r => r.ItemArray).ToArray();

			AssertContainsExactElementsInAnyOrder(securityColumns, valueArrays.Select(item => item[0]).ToArray());
			Assert("All column types are BIT", valueArrays.All(item => ((string)item[1]) == "bit"));
			Assert("All columns are NOT NULL", valueArrays.All(item => !((bool)item[2])));
		}

		DataTable Execute()
		{
			using (var command = TestConnection.Command($"SELECT * FROM hrm.HRMStaffHistory"))
			{
				return DataUtils.GetDataTableFromCommand(command);
			}
		}

		protected override DbConnection TestConnection => adminConnection ?? (adminConnection = Db.NewAdminConnection());
		AdminConnection adminConnection;
	}
}
