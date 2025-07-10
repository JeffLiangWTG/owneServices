using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.HRM;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.HRM
{
	[TestedType(typeof(HRMStaffHistoryBase))]
	class HRMRemStaffHistoryBaseTest : DbCreateScriptTest
	{
		public void TestExecute()
		{
			var staff0 = Guid.NewGuid();
			var staff1 = Guid.NewGuid();

			var geh0 = Guid.NewGuid();
			var geh1 = Guid.NewGuid();
			var get0 = Guid.NewGuid();
			var get1 = Guid.NewGuid();
			var ghb0 = Guid.NewGuid();
			var ghb1 = Guid.NewGuid();
			var gbb0 = Guid.NewGuid();
			var gbb1 = Guid.NewGuid();
			var gel0 = Guid.NewGuid();
			var gel1 = Guid.NewGuid();

			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) 
VALUES
	(@staff0, 'GS1', 'Staff1', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@staff1, 'GS2', 'Staff2', GETUTCDATE(), 'E', GETUTCDATE(), 'E');

--Prep for GlbEmploymentHistory
DECLARE @jobRole1Pk UNIQUEIDENTIFIER = NEWID();
DECLARE @jobRole2Pk UNIQUEIDENTIFIER = NEWID();

INSERT INTO dbo.HRJobRole (HJ_PK, HJ_JobTitle, HJ_JobRoleDescription, HJ_IsTemplated, HJ_IsActive, HJ_SystemCreateTimeUtc, HJ_SystemCreateUser, HJ_SystemLastEditTimeUtc, HJ_SystemLastEditUser)
VALUES
	(@jobRole1Pk, 'JobRoleDescriptionFromHRJobRole1', 'TestJobTitleFromHRJobRole1', 0, 1, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@jobRole2Pk, 'JobRoleDescriptionFromHRJobRole2', 'TestJobTitleFromHRJobRole2', 0, 1, GETUTCDATE(), 'E', GETUTCDATE(), 'E');

--Prep for GlbEmployingBranchDepartment
DECLARE @homeCompany1PK UNIQUEIDENTIFIER = NEWID();
DECLARE @homeBranch1PK UNIQUEIDENTIFIER = NEWID();
DECLARE @homeBranch2PK UNIQUEIDENTIFIER = NEWID();
DECLARE @homeDepartment1PK UNIQUEIDENTIFIER = NEWID();
DECLARE @homeDepartment2PK UNIQUEIDENTIFIER = NEWID();

DECLARE @benCompany1PK UNIQUEIDENTIFIER = NEWID();
DECLARE @benBranch1PK UNIQUEIDENTIFIER = NEWID();
DECLARE @benBranch2PK UNIQUEIDENTIFIER = NEWID();
DECLARE @benDepartment1PK UNIQUEIDENTIFIER = NEWID();
DECLARE @benDepartment2PK UNIQUEIDENTIFIER = NEWID();

INSERT INTO dbo.GlbCompany(GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name) 
VALUES
	(@homeCompany1PK, 'US', 'USD', 'HOC', 'US company'),
	(@benCompany1PK, 'AU', 'AUD', 'BEC', 'AU company')

INSERT INTO dbo.GlbBranch(GB_PK, GB_GC, GB_BranchName, GB_Code)
VALUES
	(@homeBranch1PK, @homeCompany1PK, 'HomeBranch1', 'HB1'),
	(@homeBranch2PK, @homeCompany1PK, 'HomeBranch2', 'HB2'),
	(@benBranch1PK, @benCompany1PK, 'BeneficiaryBranch1', 'BB1'),
	(@benBranch2PK, @benCompany1PK, 'BeneficiaryBranch2','BB2');

INSERT INTO dbo.GlbDepartment (GE_PK, GE_Code, GE_Desc) 
values
	(@homeDepartment1PK, 'HD1', 'HD1DES'),
	(@homeDepartment2PK, 'HD2', 'HD2DES'),
	(@benDepartment1PK, 'BD1', 'BD1DES'),
	(@benDepartment2PK, 'BD2', 'BD2DES');

--GlbEmploymentHistory
INSERT INTO dbo.GlbEmploymentHistory (GEH_PK, GEH_GS_Staff, GEH_EffectiveDate, GEH_JobTitle, GEH_HJ_JobRole, GEH_JobFamily, GEH_IsInternalPosition, GEH_WorksOutsideBranch, GEH_JobDescription, GEH_EmploymentType, GEH_CompanyName, GEH_DepartureReason, GEH_DepartureComments, GEH_SystemCreateTimeUtc, GEH_SystemCreateUser, GEH_SystemLastEditTimeUtc, GEH_SystemLastEditUser)
VALUES
	(@geh0, @staff0, '2015-05-07', 'TestJobTitle1', @jobRole1Pk, 'JF1', 0, 0, 'TestJobDescription1', 'PER', 'TestCompanyName1', 'TD1', 'TestDepartureComment1', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@geh1, @staff1, '2015-06-01', 'TestJobTitle6', @jobRole2Pk, 'JO2', 0, 0, 'TestJobDescription6', 'PER', 'TestCompanyName6', 'TD6', 'TestDepartureComment6', GETUTCDATE(), 'E', GETUTCDATE(), 'E');

--GlbEmploymentTeam
INSERT INTO dbo.GlbEmploymentTeam (GET_PK, GET_GS_Staff, GET_EffectiveDate, GET_GST_NKTeamCode, GET_SystemCreateTimeUtc, GET_SystemCreateUser, GET_SystemLastEditTimeUtc, GET_SystemLastEditUser)
VALUES
	(@get0, @staff0, '2015-05-05 10:05:23', 'TN1', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@get1, @staff1, '2015-06-16 10:05:23', 'TN6', GETUTCDATE(), 'E', GETUTCDATE(), 'E');

--GlbEmployingBranchDepartment
INSERT INTO dbo.GlbEmployingBranchDepartment (GHB_PK, GHB_GS_Staff, GHB_EffectiveDate, GHB_GE_Department, GHB_GB_Branch, GHB_SystemCreateTimeUtc, GHB_SystemCreateUser, GHB_SystemLastEditTimeUtc, GHB_SystemLastEditUser)
VALUES
	(@ghb0, @staff0, '2015-05-04 10:05:23', @homeDepartment1PK, @homeBranch1PK, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@ghb1, @staff1, '2015-06-21 10:05:23', @homeDepartment2PK, @homeBranch2PK, GETUTCDATE(), 'E', GETUTCDATE(), 'E');

--GlbBeneficiaryBranchDepartment
INSERT INTO dbo.GlbBeneficiaryBranchDepartment (GBB_PK, GBB_GS_Staff, GBB_EffectiveDate, GBB_GB_Branch, GBB_GE_Department, GBB_SystemCreateTimeUtc, GBB_SystemCreateUser, GBB_SystemLastEditTimeUtc, GBB_SystemLastEditUser)
VALUES
	(@gbb0, @staff0, '2015-05-02 10:05:23', @benBranch1PK, @benDepartment1PK, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@gbb1, @staff1, '2015-06-17 10:05:23', @benBranch2PK, @benDepartment2PK, GETUTCDATE(), 'E', GETUTCDATE(), 'E');

--GlbEmploymentLocation
INSERT INTO dbo.GlbEmploymentLocation (GEL_PK, GEL_GS_Staff, GEL_EffectiveDate, GEL_LocationSource, GEL_GB_SourceBranch, GEL_Address1, GEL_Address2, GEL_City, GEL_State, GEL_PostCode, GEL_RN_NKCountryCode, GEL_ValidationStatus, GEL_SystemCreateTimeUtc, GEL_SystemCreateUser, GEL_SystemLastEditTimeUtc, GEL_SystemLastEditUser)
VALUES
	(@gel0, @staff0, '2015-04-30 10:05:23', 'WFO', @benBranch1PK, '72 ORiordan Street', 'AlexandriOne', 'Sydney', 'NSW', '2015', 'AU', 'MAN', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
	(@gel1, @staff1, '2015-06-20 10:05:23', 'WFO', @benBranch2PK, '73 ORiordan Street', 'AlexandriTwo', 'Sydney 02', 'NSW 02', '2018', 'UA', 'CNA', GETUTCDATE(), 'E', GETUTCDATE(), 'E');

			", p =>
			{
				p.AddParameter("@staff0", SqlDbType.UniqueIdentifier, staff0);
				p.AddParameter("@staff1", SqlDbType.UniqueIdentifier, staff1);
				p.AddParameter("@geh0", SqlDbType.UniqueIdentifier, geh0);
				p.AddParameter("@geh1", SqlDbType.UniqueIdentifier, geh1);
				p.AddParameter("@get0", SqlDbType.UniqueIdentifier, get0);
				p.AddParameter("@get1", SqlDbType.UniqueIdentifier, get1);
				p.AddParameter("@ghb0", SqlDbType.UniqueIdentifier, ghb0);
				p.AddParameter("@ghb1", SqlDbType.UniqueIdentifier, ghb1);
				p.AddParameter("@gbb0", SqlDbType.UniqueIdentifier, gbb0);
				p.AddParameter("@gbb1", SqlDbType.UniqueIdentifier, gbb1);
				p.AddParameter("@gel0", SqlDbType.UniqueIdentifier, gel0);
				p.AddParameter("@gel1", SqlDbType.UniqueIdentifier, gel1);
			});

			var results = Execute();
			var resultRows = results.Select();

			var columns = new string[] { "EHV_PK", "EHV_ParentID", "EHV_StartDate", "EHV_GEH", "EHV_JobTitle", "EHV_GET", "EHV_TeamName", "EHV_GHB", "EHV_EmployingBranch", "EHV_GBB", "EHV_BeneficiaryBranch", "EHV_GEL", "EHV_WorkAddress1", "EHV_WorkAddress2", "EHV_WorkAddressCity", "EHV_WorkAddressState", "EHV_WorkAddressCountry", "EHV_WorkAddressSourceBranch", "EHV_WorkAddressLocationSource" };
			var expectedValues = new object[,]
			{
				{ $"{staff0}:2015-04-30", staff0, new DateTime(2015, 04, 30), DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, gel0, "72 ORiordan Street", "AlexandriOne", "Sydney", "NSW", "AU", "BeneficiaryBranch1", "WFO" },
				{ $"{staff0}:2015-05-02", staff0, new DateTime(2015, 05, 02), DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, gbb0, "BeneficiaryBranch1", gel0, "72 ORiordan Street", "AlexandriOne", "Sydney", "NSW", "AU", "BeneficiaryBranch1", "WFO" },
				{ $"{staff0}:2015-05-04", staff0, new DateTime(2015, 05, 04), DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, ghb0, "HomeBranch1", gbb0, "BeneficiaryBranch1", gel0, "72 ORiordan Street", "AlexandriOne", "Sydney", "NSW", "AU", "BeneficiaryBranch1", "WFO" },
				{ $"{staff0}:2015-05-05", staff0, new DateTime(2015, 05, 05), DBNull.Value, DBNull.Value, get0, "TN1", ghb0, "HomeBranch1", gbb0, "BeneficiaryBranch1", gel0, "72 ORiordan Street", "AlexandriOne", "Sydney", "NSW", "AU", "BeneficiaryBranch1", "WFO" },
				{ $"{staff0}:2015-05-07", staff0, new DateTime(2015, 05, 07), geh0, "TestJobTitle1", get0, "TN1", ghb0, "HomeBranch1", gbb0, "BeneficiaryBranch1", gel0, "72 ORiordan Street", "AlexandriOne", "Sydney", "NSW", "AU", "BeneficiaryBranch1", "WFO" },
				{ $"{staff1}:2015-06-01", staff1, new DateTime(2015, 06, 01), geh1, "TestJobTitle6", DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value },
				{ $"{staff1}:2015-06-16", staff1, new DateTime(2015, 06, 16), geh1, "TestJobTitle6", get1, "TN6", DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value },
				{ $"{staff1}:2015-06-17", staff1, new DateTime(2015, 06, 17), geh1, "TestJobTitle6", get1, "TN6", DBNull.Value, DBNull.Value, gbb1, "BeneficiaryBranch2", DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value },
				{ $"{staff1}:2015-06-20", staff1, new DateTime(2015, 06, 20), geh1, "TestJobTitle6", get1, "TN6", DBNull.Value, DBNull.Value, gbb1, "BeneficiaryBranch2", gel1, "73 ORiordan Street", "AlexandriTwo", "Sydney 02", "NSW 02", "UA", "BeneficiaryBranch2", "WFO" },
				{ $"{staff1}:2015-06-21", staff1, new DateTime(2015, 06, 21), geh1, "TestJobTitle6", get1, "TN6", ghb1, "HomeBranch2", gbb1, "BeneficiaryBranch2", gel1, "73 ORiordan Street", "AlexandriTwo", "Sydney 02", "NSW 02", "UA", "BeneficiaryBranch2", "WFO" },
			};

			AssertEquals(columns.Length, results.Columns.Count);
			foreach (var column in columns)
			{
				Assert(results.Columns.Contains(column));
			}

			AssertEquals(expectedValues.GetLength(0), resultRows.Length);
			for (int rowIndex = 0; rowIndex < expectedValues.GetLength(0); rowIndex++)
			{
				var matchedRow = resultRows.FirstOrDefault(r => r["EHV_PK"].ToString().Trim().Equals(expectedValues[rowIndex, 0].ToString().ToUpper()));
				AssertNotNull(matchedRow);

				for (int colIndex = 1; colIndex < columns.Length; colIndex++)
				{
					AssertEquals(columns[colIndex], expectedValues[rowIndex, colIndex], matchedRow[columns[colIndex]]);
				}
			}
		}

		DataTable Execute()
		{
			using (var command = TestConnection.Command($"SELECT * FROM hrm.HRMStaffHistoryBase"))
			{
				return DataUtils.GetDataTableFromCommand(command);
			}
		}

		protected override DbConnection TestConnection => adminConnection ?? (adminConnection = Db.NewAdminConnection());
		AdminConnection adminConnection;
	}
}
