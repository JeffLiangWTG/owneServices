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
	[TestedType(typeof(GetOpportunityScopesWithSecurityContext))]
	class GetOpportunityScopesWithSecurityContextTest : DbCreateScriptTest
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
			var scope1 = TestDataCreator.CreateCrmOpportunityScope(1, opp1, "AUSYD", "AUMEL");
			var scope2 = TestDataCreator.CreateCrmOpportunityScope(2, opp2, "AUMEL", "AUPER");
			var scope3 = TestDataCreator.CreateCrmOpportunityScope(3, opp3, "AUPER", "AUSYD");

			var result = RunOpportunityScopeQuery(loggedInStaff);
			AssertIsSelf(result, scope1, 1, isSelf: false);
			AssertIsSelf(result, scope2, 2, isSelf: true);
			AssertIsSelf(result, scope3, 3, isSelf: false);

			AssertIsTeam(result, scope1, 1, isTeam: true);
			AssertIsTeam(result, scope2, 2, isTeam: true);
			AssertIsTeam(result, scope3, 3, isTeam: false);
		}

		public void TestReturnsCrmOpportunityScopesColumns()
		{
			var fromCrmOpportunityScope = GetColumnNames("SELECT * FROM dbo.CrmOpportunityScope");
			var fromTvf = GetColumnNames("SELECT * FROM dbo.GetOpportunityScopesWithSecurityContext(NEWID())");

			var missingColumns = fromCrmOpportunityScope.Except(fromTvf);
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
			var scope1 = TestDataCreator.CreateCrmOpportunityScope(1, opp1, "AUSYD", "AUMEL");
			var scope2 = TestDataCreator.CreateCrmOpportunityScope(2, opp2, "AUMEL", "AUSYD");
			TestDataCreator.CreateEntityStaffRestriction("COP", opp1, "GS2");
			TestDataCreator.CreateEntityStaffRestriction("COP", opp2, "GS1");

			var staff1Result = RunOpportunityScopeQuery(staff1);
			AssertRowExists(staff1Result, scope1, 1, shouldExist: false);
			AssertRowExists(staff1Result, scope2, 2, shouldExist: true);

			var staff2Result = RunOpportunityScopeQuery(staff2);
			AssertRowExists(staff2Result, scope1, 1, shouldExist: true);
			AssertRowExists(staff2Result, scope2, 2, shouldExist: false);

			var controllerResult = RunOpportunityScopeQuery(controller);
			AssertRowExists(controllerResult, scope1, 1, shouldExist: true);
			AssertRowExists(controllerResult, scope2, 2, shouldExist: true);
		}

		void AssertIsSelf(DataRow[] rows, Guid scopePK, int scopeID, bool isSelf)
		{
			var row = rows.Single(r => (Guid)r["COS_PK"] == scopePK);
			AssertEquals("Scope" + scopeID, (bool)row["IsSelf"], isSelf);
		}

		void AssertIsTeam(DataRow[] rows, Guid scopePK, int scopeID, bool isTeam)
		{
			var row = rows.Single(r => (Guid)r["COS_PK"] == scopePK);
			AssertEquals("Scope" + scopeID, (bool)row["IsTeam"], isTeam);
		}

		void AssertRowExists(DataRow[] rows, Guid scopePK, int scopeID, bool shouldExist)
		{
			var result = rows.Any(r => (Guid)r["COS_PK"] == scopePK);
			AssertEquals("Scope" + scopeID, shouldExist, result);
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

		DataRow[] RunOpportunityScopeQuery(Guid loggedInStaff)
		{
			using (var command = TestConnection.Command("SELECT * FROM dbo.GetOpportunityScopesWithSecurityContext(@loggedInStaff)"))
			{
				command.AddParameter("@loggedInStaff", SqlDbType.UniqueIdentifier, loggedInStaff);
				return DataUtils.GetDataTableFromCommand(command).Select();
			}
		}
	}
}
