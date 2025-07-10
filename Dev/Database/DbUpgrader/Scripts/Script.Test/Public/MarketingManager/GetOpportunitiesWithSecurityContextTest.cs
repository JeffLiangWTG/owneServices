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
	[TestedType(typeof(GetOpportunitiesWithSecurityContext))]
	class GetOpportunitiesWithSecurityContextTest : DbCreateScriptTest
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

			_ = TestDataCreator.CreateCrmOpportunity("O001", "Test Opportunity 1", orgPK, companyPK, salesPerson: "GS2");
			_ = TestDataCreator.CreateCrmOpportunity("O002", "Test Opportunity 2", orgPK, companyPK, salesPerson: "GS1");
			_ = TestDataCreator.CreateCrmOpportunity("O003", "Test Opportunity 3", orgPK, companyPK);

			var result = RunOpportunityQuery(loggedInStaff);
			AssertIsSelf(result, "O001", isSelf: false);
			AssertIsSelf(result, "O002", isSelf: true);
			AssertIsSelf(result, "O003", isSelf: false);

			AssertIsTeam(result, "O001", isTeam: true);
			AssertIsTeam(result, "O002", isTeam: true);
			AssertIsTeam(result, "O003", isTeam: false);
		}

		public void TestReturnsCrmOpportunityColumns()
		{
			var fromCrmOpportunity = GetColumnNames("SELECT * FROM dbo.CrmOpportunity");
			var fromTvf = GetColumnNames("SELECT * FROM dbo.GetOpportunitiesWithSecurityContext(NEWID())");

			var missingColumns = fromCrmOpportunity.Except(fromTvf);
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
			TestDataCreator.CreateEntityStaffRestriction("COP", opp1, "GS2");
			TestDataCreator.CreateEntityStaffRestriction("COP", opp2, "GS1");

			var staff1Result = RunOpportunityQuery(staff1);
			AssertRowExists(staff1Result, "O001", shouldExist: false);
			AssertRowExists(staff1Result, "O002", shouldExist: true);

			var staff2Result = RunOpportunityQuery(staff2);
			AssertRowExists(staff2Result, "O001", shouldExist: true);
			AssertRowExists(staff2Result, "O002", shouldExist: false);

			var controllerResult = RunOpportunityQuery(controller);
			AssertRowExists(controllerResult, "O001", shouldExist: true);
			AssertRowExists(controllerResult, "O002", shouldExist: true);
		}

		void AssertIsSelf(DataRow[] rows, string opportunityID, bool isSelf)
		{
			var row = rows.Single(r => (string)r["COP_OpportunityID"] == opportunityID);
			AssertEquals(opportunityID, (bool)row["IsSelf"], isSelf);
		}

		void AssertIsTeam(DataRow[] rows, string opportunityID, bool isTeam)
		{
			var row = rows.Single(r => (string)r["COP_OpportunityID"] == opportunityID);
			AssertEquals(opportunityID, (bool)row["IsTeam"], isTeam);
		}

		void AssertRowExists(DataRow[] rows, string opportunityID, bool shouldExist)
		{
			var result = rows.Any(r => (string)r["COP_OpportunityID"] == opportunityID);
			AssertEquals(opportunityID, shouldExist, result);
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

		DataRow[] RunOpportunityQuery(Guid loggedInStaff)
		{
			using (var command = TestConnection.Command("SELECT * FROM dbo.GetOpportunitiesWithSecurityContext(@loggedInStaff)"))
			{
				command.AddParameter("@loggedInStaff", SqlDbType.UniqueIdentifier, loggedInStaff);
				return DataUtils.GetDataTableFromCommand(command).Select();
			}
		}
	}
}
