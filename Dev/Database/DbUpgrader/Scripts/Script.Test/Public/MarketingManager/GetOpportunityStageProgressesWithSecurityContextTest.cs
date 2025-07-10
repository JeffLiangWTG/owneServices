using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MarketingManager;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MarketingManager.Test
{
	[TestedType(typeof(GetOpportunityStageProgressesWithSecurityContext))]
	class GetOpportunityStageProgressesWithSecurityContextTest : DbCreateScriptTest
	{
		public void TestAdditionalColumns()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "AU", "AUD");
			var orgPK = TestDataCreator.CreateOrganisation("OH1", "Test Org");
			var loggedInStaff = TestDataCreator.CreateGlbStaff("GS1", "Test Staff 1");
			var otherStaff = TestDataCreator.CreateGlbStaff("GS2", "Test Staff 2");
			var parentGroupPK = TestDataCreator.CreateGlbGroup("GP1", null, true);
			var groupPK = TestDataCreator.CreateGlbGroup("GP2", parentGroupPK, true);
			_ = TestDataCreator.CreateGlbGroupLink(parentGroupPK, loggedInStaff);
			_ = TestDataCreator.CreateGlbGroupLink(groupPK, otherStaff);

			var opp1 = TestDataCreator.CreateCrmOpportunity("O001", "Test Opportunity 1", orgPK, companyPK, salesPerson: "GS2");
			var opp2 = TestDataCreator.CreateCrmOpportunity("O002", "Test Opportunity 2", orgPK, companyPK, salesPerson: "GS1");
			var opp3 = TestDataCreator.CreateCrmOpportunity("O003", "Test Opportunity 3", orgPK, companyPK);
			var stage1 = TestDataCreator.CreateCrmOpportunityStageProgress(opp1, "NEW", "new stage", DateTimeOffset.Now);
			var stage2 = TestDataCreator.CreateCrmOpportunityStageProgress(opp2, "CRT", "current stage", DateTimeOffset.Now);
			var stage3 = TestDataCreator.CreateCrmOpportunityStageProgress(opp3, "COM", "complete stage", DateTimeOffset.Now);

			var result = RunOpportunityStageProgressQuery(loggedInStaff);
			AssertIsSelf(result, stage1, "new stage", isSelf: false);
			AssertIsSelf(result, stage2, "current stage", isSelf: true);
			AssertIsSelf(result, stage3, "complete stage", isSelf: false);

			AssertIsTeam(result, stage1, "new stage", isTeam: true);
			AssertIsTeam(result, stage2, "current stage", isTeam: true);
			AssertIsTeam(result, stage3, "complete stage", isTeam: false);
		}

		public void TestReturnsCrmOpportunityStageProgressesColumns()
		{
			var fromCrmOpportunityStageProgress = GetColumnNames("SELECT * FROM dbo.CrmOpportunityStageProgress");
			var fromTvf = GetColumnNames("SELECT * FROM dbo.GetOpportunityStageProgressesWithSecurityContext(NEWID())");

			var missingColumns = fromCrmOpportunityStageProgress.Except(fromTvf);
			var errorMessage = $"The following columns are missing and should be added to the function: {string.Join(", ", missingColumns)}";
			Assert(errorMessage, !missingColumns.Any());
		}

		public void TestRestrictedOpportunitiesAreExcluded()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "AU", "AUD");
			var orgPK = TestDataCreator.CreateOrganisation("OH1", "Test Org");
			var staff1 = TestDataCreator.CreateGlbStaff("GS1", "Test Staff 1");
			var staff2 = TestDataCreator.CreateGlbStaff("GS2", "Test Staff 2");
			var controller = TestDataCreator.CreateGlbStaff("GS3", "Test Controller", isController: true);

			var opp1 = TestDataCreator.CreateCrmOpportunity("O001", "Test Opportunity 1", orgPK, companyPK, isRestricted: true);
			var opp2 = TestDataCreator.CreateCrmOpportunity("O002", "Test Opportunity 2", orgPK, companyPK, isRestricted: true);
			var stage1 = TestDataCreator.CreateCrmOpportunityStageProgress(opp1, "NEW", "new stage", DateTimeOffset.Now);
			var stage2 = TestDataCreator.CreateCrmOpportunityStageProgress(opp2, "COM", "complete stage", DateTimeOffset.Now);
			TestDataCreator.CreateEntityStaffRestriction("COP", opp1, "GS2");
			TestDataCreator.CreateEntityStaffRestriction("COP", opp2, "GS1");

			var staff1Result = RunOpportunityStageProgressQuery(staff1);
			AssertRowExists(staff1Result, stage1, "new stage", shouldExist: false);
			AssertRowExists(staff1Result, stage2, "complete stage", shouldExist: true);

			var staff2Result = RunOpportunityStageProgressQuery(staff2);
			AssertRowExists(staff2Result, stage1, "new stage", shouldExist: true);
			AssertRowExists(staff2Result, stage2, "complete stage", shouldExist: false);

			var controllerResult = RunOpportunityStageProgressQuery(controller);
			AssertRowExists(controllerResult, stage1, "new stage", shouldExist: true);
			AssertRowExists(controllerResult, stage2, "complete stage", shouldExist: true);
		}

		void AssertIsSelf(DataRow[] rows, Guid stagePK, string stageDescription, bool isSelf)
		{
			var row = rows.Single(r => (Guid)r["CSP_PK"] == stagePK);
			AssertEquals("Stage " + stageDescription, (bool)row["IsSelf"], isSelf);
		}

		void AssertIsTeam(DataRow[] rows, Guid stagePK, string stageDescription, bool isTeam)
		{
			var row = rows.Single(r => (Guid)r["CSP_PK"] == stagePK);
			AssertEquals("Stage " + stageDescription, (bool)row["IsTeam"], isTeam);
		}

		void AssertRowExists(DataRow[] rows, Guid stagePK, string stageDescription, bool shouldExist)
		{
			var result = rows.Any(r => (Guid)r["CSP_PK"] == stagePK);
			AssertEquals("Stage " + stageDescription, shouldExist, result);
		}

		IEnumerable<string> GetColumnNames(string commandText)
		{
			using (var command = TestConnection.Command(commandText))
			using (var reader = command.ExecuteReader(CommandBehavior.SchemaOnly))
			{
				return reader.GetSchemaTable()
					.Select()
					.Select(x => (string)x["ColumnName"]);
			}
		}

		DataRow[] RunOpportunityStageProgressQuery(Guid loggedInStaff)
		{
			using (var command = TestConnection.Command("SELECT * FROM dbo.GetOpportunityStageProgressesWithSecurityContext(@loggedInStaff)"))
			{
				command.AddParameter("@loggedInStaff", SqlDbType.UniqueIdentifier, loggedInStaff);
				return DataUtils.GetDataTableFromCommand(command).Select();
			}
		}
	}
}
