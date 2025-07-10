using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.EmploymentHistory.Testing
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.EmploymentHistory.EmploymentHistory))]
	class EmploymentHistoryTest : DbCreateScriptTest
	{
		public void TestVaryingTimeMergedToSameDate()
		{
			var dateFormat = "yyyy-MM-dd HH:mm:ss";
			TestDbHelper helper;
			Guid staff, jobRole, branch, department;
			InsertSimpleTestPrep(out helper, out staff, out jobRole, out branch, out department);

			_ = helper.InsertEmploymentHistory(staff, "2015-05-07", "TestJobTitle1", jobRole, "JF1", "TestJobDescription1", "TestCompanyName1", "TD1", "TestDepartureComment1");
			_ = helper.InsertEmploymentTeam(staff, "2015-05-07 10:05:23", "TN1");
			_ = helper.InsertHomeBranchDepartment(staff, "2015-05-07 03:05:23", department, branch);

			var employmentHistoryViewRecords = GetAllEmploymentHistory(staff);
			AssertEquals(1, employmentHistoryViewRecords.Count);

			var history = employmentHistoryViewRecords.Single();
			AssertEquals("2015-05-07 00:00:00", history.StartDate.ToString(dateFormat));
		}

		public void TestEarlierStartDateThanHistory()
		{
			TestDbHelper helper;
			Guid staff, jobRole, branch, department;
			InsertSimpleTestPrep(out helper, out staff, out jobRole, out branch, out department);

			_ = helper.InsertEmploymentHistory(staff, "2015-05-08 12:00:00", "TestJobTitle1", jobRole, "JF1", "TestJobDescription1", "TestCompanyName1", "TD1", "TestDepartureComment1");
			_ = helper.InsertEmploymentTeam(staff, "2015-05-05 10:05:23", "TN1");
			_ = helper.InsertHomeBranchDepartment(staff, "2015-05-04 03:05:23", department, branch);

			var employmentHistoryViewRecords = GetAllEmploymentHistory(staff);
			AssertEquals(3, employmentHistoryViewRecords.Count);
			AssertHistory(employmentHistoryViewRecords[0], staff, "2015-05-04", jobTitle: null, teamName: null, homeBranch: "HomeBranch1");
			AssertHistory(employmentHistoryViewRecords[1], staff, "2015-05-05", jobTitle: null, teamName: "TN1", homeBranch: "HomeBranch1");
			AssertHistory(employmentHistoryViewRecords[2], staff, "2015-05-08", jobTitle: "TestJobTitle1", teamName: "TN1", homeBranch: "HomeBranch1");
		}

		public void TestLaterStartDateThanHistoryRecords()
		{
			TestDbHelper helper;
			Guid staff, jobRole, branch, department;
			InsertSimpleTestPrep(out helper, out staff, out jobRole, out branch, out department);

			_ = helper.InsertEmploymentHistory(staff, "2015-05-08 12:00:00", "TestJobTitle1", jobRole, "JF1", "TestJobDescription1", "TestCompanyName1", "TD1", "TestDepartureComment1");
			_ = helper.InsertEmploymentTeam(staff, "2015-05-09 10:05:23", "TN1");
			_ = helper.InsertHomeBranchDepartment(staff, "2015-05-10 03:05:23", department, branch);

			var employmentHistoryViewRecords = GetAllEmploymentHistory(staff);
			AssertEquals(3, employmentHistoryViewRecords.Count);
			AssertHistory(employmentHistoryViewRecords[0], staff, "2015-05-08", jobTitle: "TestJobTitle1", teamName: null, homeBranch: null);
			AssertHistory(employmentHistoryViewRecords[1], staff, "2015-05-09", jobTitle: "TestJobTitle1", teamName: "TN1", homeBranch: null);
			AssertHistory(employmentHistoryViewRecords[2], staff, "2015-05-10", jobTitle: "TestJobTitle1", teamName: "TN1", homeBranch: "HomeBranch1");
		}

		public void TestReturningCorrespondingPks()
		{
			TestDbHelper helper;
			Guid staff, jobRole1, jobRole2, branch, branch2, department1, department2, company;
			Guid gehPk1 = Guid.NewGuid();
			Guid gehPk2 = Guid.NewGuid();
			Guid getPk = Guid.NewGuid();
			Guid ghbPk = Guid.NewGuid();
			Guid gbbPk = Guid.NewGuid();
			Guid gelPk = Guid.NewGuid();

			InsertSimpleTestPrep(out helper, out staff, out jobRole1, out branch, out department1);
			company = helper.InsertCompany("BOC", "BeneficiaryCompany", "USD", "US", true, true);
			branch2 = helper.InsertBranch("BB1", company, "BeneficiaryBranch1");
			department2 = helper.InsertDepartment("BD1", "BD1DES");
			jobRole2 = helper.InsertJobRole("JobRoleDescriptionFromHRJobRole2", "TestJobTitleFromHRJobRole2");

			_ = helper.InsertEmploymentHistory(staff, "2015-05-08", "TestJobTitle1", jobRole1, "JF1", "TestJobDescription1", "TestCompanyName1", "TD1", "TestDepartureComment1", geh_pk: gehPk1);
			_ = helper.InsertEmploymentHistory(staff, "2015-05-15", "TestJobTitle2", jobRole2, "JF2", "TestJobDescription2", "TestCompanyName2", "TD2", "TestDepartureComment2", geh_pk: gehPk2);

			_ = helper.InsertEmploymentTeam(staff, "2015-05-08 10:05:23", "TN1", get_pk: getPk);
			_ = helper.InsertHomeBranchDepartment(staff, "2015-05-08 03:05:23", department1, branch, ghb_pk: ghbPk);
			_ = helper.InsertBeneficiaryBranchDepartment(staff, "2015-05-08 03:05:23", department2, branch2, gbb_pk: gbbPk);
			_ = helper.InsertEmploymentLocation(staff, "2015-05-08 03:05:23", "72 ORiordan Street", "AlexandriOne", "Sydney", "NSW", "2015", "AU", "MAN", gel_pk: gelPk);

			var employmentHistoryViewRecords = GetAllEmploymentHistory(staff);
			AssertEquals(2, employmentHistoryViewRecords.Count);
			AssertHistory(employmentHistoryViewRecords[0], staff, "2015-05-08", jobTitle: "TestJobTitle1", teamName: "TN1", homeBranch: "HomeBranch1", geh_pk: gehPk1, get_pk: getPk, ghb_pk: ghbPk, gbb_pk: gbbPk, gel_pk: gelPk);
			AssertHistory(employmentHistoryViewRecords[1], staff, "2015-05-15", jobTitle: "TestJobTitle2", teamName: "TN1", homeBranch: "HomeBranch1", geh_pk: gehPk2, get_pk: getPk, ghb_pk: ghbPk, gbb_pk: gbbPk, gel_pk: gelPk);
		}

		public void TestGeneralUsage()
		{
			InsertTestData();
			var staff1Pk = Guid.Parse("719165bc-1a95-450b-a63d-179ad782e9b7");
			var staff2Pk = Guid.Parse("8a466ce7-ad74-4e17-8ea5-8f96289a85ee");

			var staff1History = GetAllEmploymentHistory(staff1Pk).ToList();
			var staff2History = GetAllEmploymentHistory(staff2Pk).ToList();

			AssertEquals(15, staff1History.Count);
			AssertEquals(9, staff2History.Count);

			AssertHistoryAll(staff1History[0], staff1Pk, "2015-04-30", jobTitle: null, teamName: null, homeBranch: null, benBranch: null, workAddress1: "72 ORiordan Street", workAddress2: "AlexandriOne", workAddressCity: "Sydney", workAddressState: "NSW", workAddressCountry: "AU", workAddressSourceBranch: "BeneficiaryBranch1", workAddressLocationSource: "WFO");
			AssertHistoryAll(staff1History[1], staff1Pk, "2015-05-02", jobTitle: null, teamName: null, homeBranch: null, benBranch: "BeneficiaryBranch1", workAddress1: "72 ORiordan Street", workAddress2: "AlexandriOne", workAddressCity: "Sydney", workAddressState: "NSW", workAddressCountry: "AU", workAddressSourceBranch: "BeneficiaryBranch1", workAddressLocationSource: "WFO");
			AssertHistoryAll(staff1History[2], staff1Pk, "2015-05-04", jobTitle: null, teamName: null, homeBranch: "HomeBranch1", benBranch: "BeneficiaryBranch1", workAddress1: "72 ORiordan Street", workAddress2: "AlexandriOne", workAddressCity: "Sydney", workAddressState: "NSW", workAddressCountry: "AU", workAddressSourceBranch: "BeneficiaryBranch1", workAddressLocationSource: "WFO");
			AssertHistoryAll(staff1History[3], staff1Pk, "2015-05-05", jobTitle: null, teamName: "TN1", homeBranch: "HomeBranch1", benBranch: "BeneficiaryBranch1", workAddress1: "72 ORiordan Street", workAddress2: "AlexandriOne", workAddressCity: "Sydney", workAddressState: "NSW", workAddressCountry: "AU", workAddressSourceBranch: "BeneficiaryBranch1", workAddressLocationSource: "WFO");
			AssertHistoryAll(staff1History[4], staff1Pk, "2015-05-07", jobTitle: "TestJobTitle1", teamName: "TN1", homeBranch: "HomeBranch1", benBranch: "BeneficiaryBranch1", workAddress1: "72 ORiordan Street", workAddress2: "AlexandriOne", workAddressCity: "Sydney", workAddressState: "NSW", workAddressCountry: "AU", workAddressSourceBranch: "BeneficiaryBranch1", workAddressLocationSource: "WFO");
			AssertHistoryAll(staff1History[5], staff1Pk, "2015-05-13", jobTitle: "TestJobTitle2", teamName: "TN1", homeBranch: "HomeBranch1", benBranch: "BeneficiaryBranch1", workAddress1: "72 ORiordan Street", workAddress2: "AlexandriOne", workAddressCity: "Sydney", workAddressState: "NSW", workAddressCountry: "AU", workAddressSourceBranch: "BeneficiaryBranch1", workAddressLocationSource: "WFO");
			AssertHistoryAll(staff1History[6], staff1Pk, "2015-05-15", jobTitle: "TestJobTitle2", teamName: "TN1", homeBranch: "HomeBranch1", benBranch: "BeneficiaryBranch1", workAddress1: "73 ORiordan Street", workAddress2: "AlexandriTwo", workAddressCity: "Sydney", workAddressState: "NSW", workAddressCountry: "AU", workAddressSourceBranch: null, workAddressLocationSource: "OTH");
			AssertHistoryAll(staff1History[7], staff1Pk, "2015-05-16", jobTitle: "TestJobTitle2", teamName: "TN2", homeBranch: "HomeBranch1", benBranch: "BeneficiaryBranch1", workAddress1: "73 ORiordan Street", workAddress2: "AlexandriTwo", workAddressCity: "Sydney", workAddressState: "NSW", workAddressCountry: "AU", workAddressSourceBranch: null, workAddressLocationSource: "OTH");
			AssertHistoryAll(staff1History[8], staff1Pk, "2015-05-17", jobTitle: "TestJobTitle3", teamName: "TN2", homeBranch: "HomeBranch2", benBranch: "BeneficiaryBranch1", workAddress1: "73 ORiordan Street", workAddress2: "AlexandriTwo", workAddressCity: "Sydney", workAddressState: "NSW", workAddressCountry: "AU", workAddressSourceBranch: null, workAddressLocationSource: "OTH");
			AssertHistoryAll(staff1History[9], staff1Pk, "2015-05-19", jobTitle: "TestJobTitle3", teamName: "TN2", homeBranch: "HomeBranch2", benBranch: "BeneficiaryBranch2", workAddress1: "73 ORiordan Street", workAddress2: "AlexandriTwo", workAddressCity: "Sydney", workAddressState: "NSW", workAddressCountry: "AU", workAddressSourceBranch: null, workAddressLocationSource: "OTH");
			AssertHistoryAll(staff1History[10], staff1Pk, "2015-05-21", jobTitle: "TestJobTitle3", teamName: "TN3", homeBranch: "HomeBranch2", benBranch: "BeneficiaryBranch2", workAddress1: "74 ORiordan Street", workAddress2: "AlexandriThree", workAddressCity: "Sydney", workAddressState: "NSW", workAddressCountry: "AU", workAddressSourceBranch: null, workAddressLocationSource: "OTH");
			AssertHistoryAll(staff1History[11], staff1Pk, "2015-05-30", jobTitle: "TestJobTitle3", teamName: "TN3", homeBranch: "HomeBranch2", benBranch: "BeneficiaryBranch3", workAddress1: "74 ORiordan Street", workAddress2: "AlexandriThree", workAddressCity: "Sydney", workAddressState: "NSW", workAddressCountry: "AU", workAddressSourceBranch: null, workAddressLocationSource: "OTH");
			AssertHistoryAll(staff1History[12], staff1Pk, "2015-05-31", jobTitle: "TestJobTitle3", teamName: "TN4", homeBranch: "HomeBranch2", benBranch: "BeneficiaryBranch3", workAddress1: "74 ORiordan Street", workAddress2: "AlexandriThree", workAddressCity: "Sydney", workAddressState: "NSW", workAddressCountry: "AU", workAddressSourceBranch: null, workAddressLocationSource: "OTH");
			AssertHistoryAll(staff1History[13], staff1Pk, "2015-06-01", jobTitle: "TestJobTitle4", teamName: "TN4", homeBranch: "HomeBranch3", benBranch: "BeneficiaryBranch3", workAddress1: "74 ORiordan Street", workAddress2: "AlexandriThree", workAddressCity: "Sydney", workAddressState: "NSW", workAddressCountry: "AU", workAddressSourceBranch: null, workAddressLocationSource: "OTH");
			AssertHistoryAll(staff1History[14], staff1Pk, "2015-06-17", jobTitle: "TestJobTitle5", teamName: "TN4", homeBranch: "HomeBranch3", benBranch: "BeneficiaryBranch3", workAddress1: "75 ORiordan Street", workAddress2: "AlexandriFour", workAddressCity: "Sydney", workAddressState: "NSW", workAddressCountry: "AU", workAddressSourceBranch: null, workAddressLocationSource: "OTH");

			AssertHistoryAll(staff2History[0], staff2Pk, "2015-05-30", jobTitle: null, teamName: "TN5", homeBranch: null, benBranch: null, workAddress1: null, workAddress2: null, workAddressCity: null, workAddressState: null, workAddressCountry: null, workAddressSourceBranch: null, workAddressLocationSource: null);
			AssertHistoryAll(staff2History[1], staff2Pk, "2015-05-31", jobTitle: null, teamName: "TN5", homeBranch: "HomeBranch1", benBranch: null, workAddress1: null, workAddress2: null, workAddressCity: null, workAddressState: null, workAddressCountry: null, workAddressSourceBranch: null, workAddressLocationSource: null);
			AssertHistoryAll(staff2History[2], staff2Pk, "2015-06-01", jobTitle: "TestJobTitle6", teamName: "TN5", homeBranch: "HomeBranch1", benBranch: null, workAddress1: null, workAddress2: null, workAddressCity: null, workAddressState: null, workAddressCountry: null, workAddressSourceBranch: null, workAddressLocationSource: null);
			AssertHistoryAll(staff2History[3], staff2Pk, "2015-06-02", jobTitle: "TestJobTitle6", teamName: "TN5", homeBranch: "HomeBranch1", benBranch: "BeneficiaryBranch1", workAddress1: null, workAddress2: null, workAddressCity: null, workAddressState: null, workAddressCountry: null, workAddressSourceBranch: null, workAddressLocationSource: null);
			AssertHistoryAll(staff2History[4], staff2Pk, "2015-06-15", jobTitle: "TestJobTitle6", teamName: "TN5", homeBranch: "HomeBranch1", benBranch: "BeneficiaryBranch1", workAddress1: "72 ORiordan Street", workAddress2: "AlexandriOne", workAddressCity: "Sydney", workAddressState: "NSW", workAddressCountry: "AU", workAddressSourceBranch: null, workAddressLocationSource: "OTH");
			AssertHistoryAll(staff2History[5], staff2Pk, "2015-06-16", jobTitle: "TestJobTitle6", teamName: "TN6", homeBranch: "HomeBranch1", benBranch: "BeneficiaryBranch1", workAddress1: "72 ORiordan Street", workAddress2: "AlexandriOne", workAddressCity: "Sydney", workAddressState: "NSW", workAddressCountry: "AU", workAddressSourceBranch: null, workAddressLocationSource: "OTH");
			AssertHistoryAll(staff2History[6], staff2Pk, "2015-06-17", jobTitle: "TestJobTitle7", teamName: "TN6", homeBranch: "HomeBranch1", benBranch: "BeneficiaryBranch2", workAddress1: "72 ORiordan Street", workAddress2: "AlexandriOne", workAddressCity: "Sydney", workAddressState: "NSW", workAddressCountry: "AU", workAddressSourceBranch: null, workAddressLocationSource: "OTH");
			AssertHistoryAll(staff2History[7], staff2Pk, "2015-06-20", jobTitle: "TestJobTitle7", teamName: "TN6", homeBranch: "HomeBranch1", benBranch: "BeneficiaryBranch2", workAddress1: "73 ORiordan Street", workAddress2: "AlexandriTwo", workAddressCity: "Sydney", workAddressState: "NSW", workAddressCountry: "AU", workAddressSourceBranch: "BeneficiaryBranch3", workAddressLocationSource: "WFO");
			AssertHistoryAll(staff2History[8], staff2Pk, "2015-06-21", jobTitle: "TestJobTitle7", teamName: "TN6", homeBranch: "HomeBranch2", benBranch: "BeneficiaryBranch2", workAddress1: "73 ORiordan Street", workAddress2: "AlexandriTwo", workAddressCity: "Sydney", workAddressState: "NSW", workAddressCountry: "AU", workAddressSourceBranch: "BeneficiaryBranch3", workAddressLocationSource: "WFO");
		}

		void AssertHistory(Employment employment, Guid staffPk, string startDate, string jobTitle = null, string teamName = null, string homeBranch = null, Guid? geh_pk = null, Guid? get_pk = null, Guid? ghb_pk = null, Guid? gbb_pk = null, Guid? gel_pk = null)
		{
			var dateFormat = "yyyy-MM-dd";
			geh_pk = geh_pk ?? employment.EHV_GEH;
			get_pk = get_pk ?? employment.EHV_GET;
			ghb_pk = ghb_pk ?? employment.EHV_GHB;
			gbb_pk = gbb_pk ?? employment.EHV_GBB;
			gel_pk = gel_pk ?? employment.EHV_GEL;

			CombineAssertions(() =>
			{
				AssertEquals(startDate, employment.StartDate.ToString(dateFormat));
				AssertEquals(staffPk, employment.ParentId);
				AssertEquals(jobTitle, employment.JobTitle);
				AssertEquals(teamName, employment.TeamName);
				AssertEquals(homeBranch, employment.HomeBranchName);

				AssertEquals("EmploymnetHistory Pk", geh_pk, employment.EHV_GEH);
				AssertEquals("EmploymnetTeam Pk", get_pk, employment.EHV_GET);
				AssertEquals("HomeBranchDepartment Pk", ghb_pk, employment.EHV_GHB);
				AssertEquals("BeneficiaryBranchDepartment Pk", gbb_pk, employment.EHV_GBB);
				AssertEquals("EmploymnetLocation Pk", gel_pk, employment.EHV_GEL);
			});
		}

		void AssertHistoryAll(Employment employment, Guid staffPk, string startDate, string jobTitle = null, string teamName = null, string homeBranch = null, string benBranch = null, string workAddress1 = null, string workAddress2 = null, string workAddressCity = null, string workAddressState = null, string workAddressCountry = null, string workAddressSourceBranch = null, string workAddressLocationSource = null)
		{
			AssertHistory(employment, staffPk, startDate, jobTitle, teamName, homeBranch);

			CombineAssertions(() =>
			{
				AssertEquals(benBranch, employment.BenBranchName);
				AssertEquals(workAddress1, employment.WorkAddress1);
				AssertEquals(workAddress2, employment.WorkAddress2);
				AssertEquals(workAddressCity, employment.WorkAddressCity);
				AssertEquals(workAddressState, employment.WorkAddressState);
				AssertEquals(workAddressCountry, employment.WorkAddressCountry);
				AssertEquals(workAddressSourceBranch, employment.WorkAddressSourceBranchName);
				AssertEquals(workAddressLocationSource, employment.WorkAddressLocationSource);
			});
		}

		#region Implementation

		void InsertTestData()
		{
			var insertSql = @"
--Prep for tables
DECLARE @staff1Pk UNIQUEIDENTIFIER = '719165bc-1a95-450b-a63d-179ad782e9b7';
DECLARE @staff2Pk UNIQUEIDENTIFIER = '8a466ce7-ad74-4e17-8ea5-8f96289a85ee';

INSERT INTO dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) 
VALUES
(@staff1Pk, 'GS1', 'Staff1', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
(@staff2Pk, 'GS2', 'Staff2', GETUTCDATE(), 'E', GETUTCDATE(), 'E');

--Prep for GlbEmploymentHistory
DECLARE @jobRole1Pk UNIQUEIDENTIFIER = '0818e5e1-6848-43f0-81fb-d3172c3340a7';
DECLARE @jobRole2Pk UNIQUEIDENTIFIER = '087789f1-ef03-4da5-a6c0-ceb597546e9a';
DECLARE @jobRole3Pk UNIQUEIDENTIFIER = '12ec0bb9-d100-4df9-9e18-4612ef394b39';
DECLARE @jobRole4Pk UNIQUEIDENTIFIER = '1fe54825-04a7-4244-8c87-9536fefed471';
DECLARE @jobRole5Pk UNIQUEIDENTIFIER = '50374bc6-9d45-43af-8824-ff77479ee5db';

INSERT INTO dbo.HRJobRole (HJ_PK, HJ_JobTitle, HJ_JobRoleDescription, HJ_IsTemplated, HJ_IsActive, HJ_SystemCreateTimeUtc, HJ_SystemCreateUser, HJ_SystemLastEditTimeUtc, HJ_SystemLastEditUser)
VALUES
(@jobRole1Pk, 'JobRoleDescriptionFromHRJobRole1', 'TestJobTitleFromHRJobRole1', 0, 1, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
(@jobRole2Pk, 'JobRoleDescriptionFromHRJobRole2', 'TestJobTitleFromHRJobRole2', 0, 1, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
(@jobRole3Pk, 'JobRoleDescriptionFromHRJobRole3', 'TestJobTitleFromHRJobRole3', 0, 1, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
(@jobRole4Pk, 'JobRoleDescriptionFromHRJobRole4', 'TestJobTitleFromHRJobRole4', 0, 1, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
(@jobRole5Pk, 'JobRoleDescriptionFromHRJobRole5', 'TestJobTitleFromHRJobRole5', 0, 1, GETUTCDATE(), 'E', GETUTCDATE(), 'E')

--Prep for GlbEmployingBranchDepartment
DECLARE @homeCompany1PK UNIQUEIDENTIFIER = '04db6012-7cb6-4a81-8212-1d384508a49a';
DECLARE @homeBranch1PK UNIQUEIDENTIFIER = '1aebad4b-addf-4afd-a453-8488f102c2dd';
DECLARE @homeBranch2PK UNIQUEIDENTIFIER = '14635db3-b4df-4d4f-9660-d358aefdf8f1';
DECLARE @homeBranch3PK UNIQUEIDENTIFIER = '47a235ea-ed04-4551-a05d-88adefe94ee2';
DECLARE @homeDepartment1PK UNIQUEIDENTIFIER = '42184539-f063-4182-bb8f-0f9a4b51870f';
DECLARE @homeDepartment2PK UNIQUEIDENTIFIER = 'f520b212-dd78-4c71-8b3c-d1a9457351fa';
DECLARE @homeDepartment3PK UNIQUEIDENTIFIER = '9058841c-d9fd-4206-bd6c-957f1f9384d1';

DECLARE @benCompany1PK UNIQUEIDENTIFIER = 'f2bf9b65-90d2-4492-acc5-536e6b39685c';
DECLARE @benBranch1PK UNIQUEIDENTIFIER = '1bc88fb5-6f09-49d5-b564-22188691e649';
DECLARE @benBranch2PK UNIQUEIDENTIFIER = 'ccd26dca-b322-4d1b-9a19-8c132c3f5d5e';
DECLARE @benBranch3PK UNIQUEIDENTIFIER = 'afa76dd5-880d-4fc1-9f80-7d8cba97cf45';
DECLARE @benDepartment1PK UNIQUEIDENTIFIER = '7b45fef8-21a9-455f-a74b-56a7d717eb45';
DECLARE @benDepartment2PK UNIQUEIDENTIFIER = 'ae203f13-ddfd-4e2e-9962-45b7aa542846';
DECLARE @benDepartment3PK UNIQUEIDENTIFIER = 'c46516e3-e2c3-4df6-b5e2-ceb5293195d2';

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
(@benBranch3PK, @benCompany1PK, 'BeneficiaryBranch3','BB3')

INSERT INTO dbo.GlbDepartment (GE_PK, GE_Code, GE_Desc) 
values
(@homeDepartment1PK, 'HD1', 'HD1DES'),
(@homeDepartment2PK, 'HD2', 'HD2DES'),
(@homeDepartment3PK, 'HD3', 'HD3DES'),
(@benDepartment1PK, 'BD1', 'BD1DES'),
(@benDepartment2PK, 'BD2', 'BD2DES'),
(@benDepartment3PK, 'BD3', 'BD3DES')

--GlbEmploymentHistory
INSERT INTO dbo.GlbEmploymentHistory (GEH_PK, GEH_GS_Staff, GEH_EffectiveDate, GEH_JobTitle, GEH_HJ_JobRole, GEH_JobFamily, GEH_IsInternalPosition, GEH_WorksOutsideBranch, GEH_JobDescription, GEH_EmploymentType, GEH_CompanyName, GEH_DepartureReason, GEH_DepartureComments, GEH_SystemCreateTimeUtc, GEH_SystemCreateUser, GEH_SystemLastEditTimeUtc, GEH_SystemLastEditUser)
VALUES
(NEWID(), @staff1Pk, '2015-05-07', 'TestJobTitle1', @jobRole1Pk, 'JF1', 0, 0, 'TestJobDescription1', 'PER', 'TestCompanyName1', 'TD1', 'TestDepartureComment1', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
(NEWID(), @staff1Pk, '2015-05-13', 'TestJobTitle2', @jobRole2Pk, 'JF2', 0, 0, 'TestJobDescription2', 'PER', 'TestCompanyName2', 'TD2', 'TestDepartureComment2', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
(NEWID(), @staff1Pk, '2015-05-17', 'TestJobTitle3', @jobRole3Pk, 'JF3', 0, 0, 'TestJobDescription3', 'PER', 'TestCompanyName3', 'TD3', 'TestDepartureComment3', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
(NEWID(), @staff1Pk, '2015-06-01', 'TestJobTitle4', @jobRole4Pk, 'JF4', 0, 0, 'TestJobDescription4', 'PER', 'TestCompanyName4', 'TD4', 'TestDepartureComment4', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
(NEWID(), @staff1Pk, '2015-06-17', 'TestJobTitle5', @jobRole5Pk, 'JF5', 0, 0, 'TestJobDescription5', 'PER', 'TestCompanyName5', 'TD5', 'TestDepartureComment5', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),

(NEWID(), @staff2Pk, '2015-06-01', 'TestJobTitle6', @jobRole1Pk, 'JF1', 0, 0, 'TestJobDescription6', 'PER', 'TestCompanyName6', 'TD6', 'TestDepartureComment6', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
(NEWID(), @staff2Pk, '2015-06-17', 'TestJobTitle7', @jobRole2Pk, 'JF2', 0, 0, 'TestJobDescription7', 'PER', 'TestCompanyName7', 'TD7', 'TestDepartureComment7', GETUTCDATE(), 'E', GETUTCDATE(), 'E')


--GlbEmploymentTeam
INSERT INTO dbo.GlbEmploymentTeam (GET_PK, GET_GS_Staff, GET_EffectiveDate, GET_GST_NKTeamCode, GET_SystemCreateTimeUtc, GET_SystemCreateUser, GET_SystemLastEditTimeUtc, GET_SystemLastEditUser)
VALUES
(NEWID(), @staff1Pk, '2015-05-05 10:05:23', 'TN1', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
(NEWID(), @staff1Pk, '2015-05-16 10:05:23', 'TN2', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
(NEWID(), @staff1Pk, '2015-05-21 10:05:23', 'TN3', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
(NEWID(), @staff1Pk, '2015-05-31 10:05:23', 'TN4', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
(NEWID(), @staff2Pk, '2015-05-30 10:05:23', 'TN5', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
(NEWID(), @staff2Pk, '2015-06-16 10:05:23', 'TN6', GETUTCDATE(), 'E', GETUTCDATE(), 'E')

--GlbEmployingBranchDepartment
INSERT INTO dbo.GlbEmployingBranchDepartment (GHB_PK, GHB_GS_Staff, GHB_EffectiveDate, GHB_GE_Department, GHB_GB_Branch, GHB_SystemCreateTimeUtc, GHB_SystemCreateUser, GHB_SystemLastEditTimeUtc, GHB_SystemLastEditUser)
VALUES
(NEWID(), @staff1Pk, '2015-05-04 10:05:23', @homeDepartment1PK, @homeBranch1PK, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
(NEWID(), @staff1Pk, '2015-05-17 10:05:23', @homeDepartment2PK, @homeBranch2PK, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
(NEWID(), @staff1Pk, '2015-06-01 10:05:23', @homeDepartment3PK, @homeBranch3PK, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
(NEWID(), @staff2Pk, '2015-05-31 10:05:23', @homeDepartment1PK, @homeBranch1PK, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
(NEWID(), @staff2Pk, '2015-06-21 10:05:23', @homeDepartment2PK, @homeBranch2PK, GETUTCDATE(), 'E', GETUTCDATE(), 'E')

--GlbBeneficiaryBranchDepartment
INSERT INTO dbo.GlbBeneficiaryBranchDepartment (GBB_PK, GBB_GS_Staff, GBB_EffectiveDate, GBB_GB_Branch, GBB_GE_Department, GBB_SystemCreateTimeUtc, GBB_SystemCreateUser, GBB_SystemLastEditTimeUtc, GBB_SystemLastEditUser)
VALUES
(NEWID(), @staff1Pk, '2015-05-02 10:05:23', @benBranch1PK, @benDepartment1PK, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
(NEWID(), @staff1Pk, '2015-05-19 10:05:23', @benBranch2PK, @benDepartment2PK, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
(NEWID(), @staff1Pk, '2015-05-30 10:05:23', @benBranch3PK, @benDepartment3PK, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
(NEWID(), @staff2Pk, '2015-06-02 10:05:23', @benBranch1PK, @benDepartment1PK, GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
(NEWID(), @staff2Pk, '2015-06-17 10:05:23', @benBranch2PK, @benDepartment2PK, GETUTCDATE(), 'E', GETUTCDATE(), 'E')

--GlbEmploymentLocation
INSERT INTO dbo.GlbEmploymentLocation (GEL_PK, GEL_GS_Staff, GEL_EffectiveDate, GEL_LocationSource, GEL_GB_SourceBranch, GEL_Address1, GEL_Address2, GEL_City, GEL_State, GEL_PostCode, GEL_RN_NKCountryCode, GEL_ValidationStatus, GEL_SystemCreateTimeUtc, GEL_SystemCreateUser, GEL_SystemLastEditTimeUtc, GEL_SystemLastEditUser)
VALUES
(NEWID(), @staff1Pk, '2015-04-30 10:05:23', 'WFO', @benBranch1PK, '72 ORiordan Street', 'AlexandriOne', 'Sydney', 'NSW', '2015', 'AU', 'MAN', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
(NEWID(), @staff1Pk, '2015-05-15 10:05:23', 'OTH', NULL, '73 ORiordan Street', 'AlexandriTwo', 'Sydney', 'NSW', '2016', 'AU', 'MAN', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
(NEWID(), @staff1Pk, '2015-05-21 10:05:23', 'OTH', NULL, '74 ORiordan Street', 'AlexandriThree', 'Sydney', 'NSW', '2017', 'AU', 'MAN', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
(NEWID(), @staff1Pk, '2015-06-17 10:05:23', 'OTH', NULL, '75 ORiordan Street', 'AlexandriFour', 'Sydney', 'NSW', '2018', 'AU', 'MAN', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
(NEWID(), @staff2Pk, '2015-06-15 10:05:23', 'OTH', NULL, '72 ORiordan Street', 'AlexandriOne', 'Sydney', 'NSW', '2015', 'AU', 'MAN', GETUTCDATE(), 'E', GETUTCDATE(), 'E'),
(NEWID(), @staff2Pk, '2015-06-20 10:05:23', 'WFO', @benBranch3PK, '73 ORiordan Street', 'AlexandriTwo', 'Sydney', 'NSW', '2016', 'AU', 'MAN', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
";
			using (var command = TestConnection.Command(insertSql))
			{
				command.ExecuteNonQuery();
			}
		}

		void InsertSimpleTestPrep(out TestDbHelper helper, out Guid staff, out Guid jobRole, out Guid branch, out Guid department)
		{
			helper = new TestDbHelper(TestConnection);
			staff = helper.InsertStaff("GS1", "Staff1");
			jobRole = helper.InsertJobRole("JobRoleDescriptionFromHRJobRole1", "TestJobTitleFromHRJobRole1");
			var company = helper.InsertCompany("HOC", "HomeCompany", "USD", "US", true, true);
			branch = helper.InsertBranch("HB1", company, "HomeBranch1");
			department = helper.InsertDepartment("HD1", "HD1DES");
		}

		List<Employment> GetAllEmploymentHistory(Guid staff)
		{
			using (var command = TestConnection.Command(string.Format(@"Select * from dbo.EmploymentHistory('{0}')", staff)))
			{
				using (var reader = command.ExecuteReader())
				{
					var result = new List<Employment>();
					while (reader.Read())
					{
						var parentId = (Guid)reader["EHV_ParentID"];
						var ehv_geh = reader["EHV_GEH"] == DBNull.Value ? null : (Guid?)reader["EHV_GEH"];
						var ehv_get = reader["EHV_GET"] == DBNull.Value ? null : (Guid?)reader["EHV_GET"];
						var ehv_ghb = reader["EHV_GHB"] == DBNull.Value ? null : (Guid?)reader["EHV_GHB"];
						var ehv_gbb = reader["EHV_GBB"] == DBNull.Value ? null : (Guid?)reader["EHV_GBB"];
						var ehv_gel = reader["EHV_GEL"] == DBNull.Value ? null : (Guid?)reader["EHV_GEL"];

						var startDate = (DateTime)reader["EHV_StartDate"];
						var jobTitle = reader["EHV_JobTitle"] == DBNull.Value ? null : (string)reader["EHV_JobTitle"];
						var teamName = reader["EHV_TeamName"] == DBNull.Value ? null : (string)reader["EHV_TeamName"];
						var homeBranchName = reader["EHV_EmployingBranch"] == DBNull.Value ? null : (string)reader["EHV_EmployingBranch"];
						var banBranchName = reader["EHV_BeneficiaryBranch"] == DBNull.Value ? null : (string)reader["EHV_BeneficiaryBranch"];
						var workAddress1 = reader["EHV_WorkAddress1"] == DBNull.Value ? null : (string)reader["EHV_WorkAddress1"];
						var workAddress2 = reader["EHV_WorkAddress2"] == DBNull.Value ? null : (string)reader["EHV_WorkAddress2"];
						var workAddressCity = reader["EHV_WorkAddressCity"] == DBNull.Value ? null : (string)reader["EHV_WorkAddressCity"];
						var workAddressState = reader["EHV_WorkAddressState"] == DBNull.Value ? null : (string)reader["EHV_WorkAddressState"];
						var workAddressCountry = reader["EHV_WorkAddressCountry"] == DBNull.Value ? null : (string)reader["EHV_WorkAddressCountry"];
						var workAddressSourceBranchName = reader["EHV_WorkAddressSourceBranch"] == DBNull.Value ? null : (string)reader["EHV_WorkAddressSourceBranch"];
						var workAddressLocationSource = reader["EHV_WorkAddressLocationSource"] == DBNull.Value ? null : (string)reader["EHV_WorkAddressLocationSource"];

						result.Add(new Employment(parentId, ehv_geh, ehv_get, ehv_ghb, ehv_gbb, ehv_gel, startDate, jobTitle, teamName, homeBranchName, banBranchName, workAddress1, workAddress2, workAddressCity, workAddressState, workAddressCountry, workAddressSourceBranchName, workAddressLocationSource));
					}

					return result;
				}
			}
		}

		class Employment
		{
			public Employment(Guid parentId, Guid? ehv_geh, Guid? ehv_get, Guid? ehv_ghb, Guid? ehv_gbb, Guid? ehv_gel, DateTime startDate, string jobTitle, string teamName, string homeBranchName, string banBranchName, string workAddress1, string workAddress2, string workAddressCity, string workAddressState, string workAddressCountry, string workAddressSourceBranchName, string workAddressLocationSource)
			{
				ParentId = parentId;
				EHV_GEH = ehv_geh;
				EHV_GET = ehv_get;
				EHV_GHB = ehv_ghb;
				EHV_GBB = ehv_gbb;
				EHV_GEL = ehv_gel;
				StartDate = startDate;
				JobTitle = jobTitle;
				TeamName = teamName;
				HomeBranchName = homeBranchName;
				BenBranchName = banBranchName;
				WorkAddress1 = workAddress1;
				WorkAddress2 = workAddress2;
				WorkAddressCity = workAddressCity;
				WorkAddressState = workAddressState;
				WorkAddressCountry = workAddressCountry;
				WorkAddressSourceBranchName = workAddressSourceBranchName;
				WorkAddressLocationSource = workAddressLocationSource;
			}

			public readonly Guid ParentId;
			public readonly Guid? EHV_GEH;
			public readonly Guid? EHV_GET;
			public readonly Guid? EHV_GHB;
			public readonly Guid? EHV_GBB;
			public readonly Guid? EHV_GEL;

			public readonly DateTime StartDate;
			public readonly string JobTitle;
			public readonly string TeamName;
			public readonly string HomeBranchName;
			public readonly string BenBranchName;
			public readonly string WorkAddress1;
			public readonly string WorkAddress2;
			public readonly string WorkAddressCity;
			public readonly string WorkAddressState;
			public readonly string WorkAddressCountry;
			public readonly string WorkAddressSourceBranchName;
			public readonly string WorkAddressLocationSource;
		}
		#endregion
	}
}

