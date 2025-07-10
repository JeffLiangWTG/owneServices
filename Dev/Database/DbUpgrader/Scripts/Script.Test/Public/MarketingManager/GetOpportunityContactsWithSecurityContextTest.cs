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
	[TestedType(typeof(GetOpportunityContactsWithSecurityContext))]
	class GetOpportunityContactsWithSecurityContextTest : DbCreateScriptTest
	{
		public void TestAdditionalColumns()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "AU", "AUD");
			var orgPK = TestDataCreator.CreateOrganisation("OH1", "Test Org");
			var contact1 = TestDataCreator.CreateContact(orgPK,"Tester1","1234567890");
			var contact2 = TestDataCreator.CreateContact(orgPK, "Tester2", "1234567891");
			var contact3 = TestDataCreator.CreateContact(orgPK, "Tester3", "1234567892");
			var loggedInStaff = TestDataCreator.CreateGlbStaff("GS1", "Test Staff 1");
			var otherStaff = TestDataCreator.CreateGlbStaff("GS2", "Test Staff 2");
			var parentGroupPK = TestDataCreator.CreateGlbGroup("GP1", null, true);
			var groupPK = TestDataCreator.CreateGlbGroup("GP2", parentGroupPK, true);
			_ = TestDataCreator.CreateGlbGroupLink(parentGroupPK, loggedInStaff);
			_ = TestDataCreator.CreateGlbGroupLink(groupPK, otherStaff);

			var opp1 = TestDataCreator.CreateCrmOpportunity("O001", "Test Opportunity 1", orgPK, companyPK, salesPerson: "GS2");
			var opp2 = TestDataCreator.CreateCrmOpportunity("O002", "Test Opportunity 2", orgPK, companyPK, salesPerson: "GS1");
			var opp3 = TestDataCreator.CreateCrmOpportunity("O003", "Test Opportunity 3", orgPK, companyPK);
			var crmContact1 = TestDataCreator.CreateCrmOpportunityContact(opp1, contact1, "DCM", true);
			var crmContact2 = TestDataCreator.CreateCrmOpportunityContact(opp2, contact2, "DCM", true);
			var crmContact3 = TestDataCreator.CreateCrmOpportunityContact(opp3, contact3, "DCM", true);

			var result = RunOpportunityContactQuery(loggedInStaff);
			AssertIsSelf(result, crmContact1, "Tester1", isSelf: false);
			AssertIsSelf(result, crmContact2, "Tester2", isSelf: true);
			AssertIsSelf(result, crmContact3, "Tester3", isSelf: false);

			AssertIsTeam(result, crmContact1, "Tester1", isTeam: true);
			AssertIsTeam(result, crmContact2, "Tester2", isTeam: true);
			AssertIsTeam(result, crmContact3, "Tester3", isTeam: false);
		}

		public void TestReturnsCrmOpportunityContactsColumns()
		{
			var fromCrmOpportunityContact = GetColumnNames("SELECT * FROM dbo.CrmOpportunityContact");
			var fromTvf = GetColumnNames("SELECT * FROM dbo.GetOpportunityContactsWithSecurityContext(NEWID())");

			var missingColumns = fromCrmOpportunityContact.Except(fromTvf);
			var errorMessage = $"The following columns are missing and should be added to the function: {string.Join(", ", missingColumns)}";
			Assert(errorMessage, !missingColumns.Any());
		}

		public void TestRestrictedOpportunitiesAreExcluded()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "AU", "AUD");
			var orgPK = TestDataCreator.CreateOrganisation("OH1", "Test Org");
			var contact1 = TestDataCreator.CreateContact(orgPK, "Tester1", "1234567890");
			var contact2 = TestDataCreator.CreateContact(orgPK, "Tester2", "1234567891");
			var staff1 = TestDataCreator.CreateGlbStaff("GS1", "Test Staff 1");
			var staff2 = TestDataCreator.CreateGlbStaff("GS2", "Test Staff 2");
			var controller = TestDataCreator.CreateGlbStaff("GS3", "Test Controller", isController: true);

			var opp1 = TestDataCreator.CreateCrmOpportunity("O001", "Test Opportunity 1", orgPK, companyPK, isRestricted: true);
			var opp2 = TestDataCreator.CreateCrmOpportunity("O002", "Test Opportunity 2", orgPK, companyPK, isRestricted: true);
			var crmContact1 = TestDataCreator.CreateCrmOpportunityContact(opp1, contact1, "DCM", true);
			var crmContact2 = TestDataCreator.CreateCrmOpportunityContact(opp2, contact2, "DCM", true);
			TestDataCreator.CreateEntityStaffRestriction("COP", opp1, "GS2");
			TestDataCreator.CreateEntityStaffRestriction("COP", opp2, "GS1");

			var staff1Result = RunOpportunityContactQuery(staff1);
			AssertRowExists(staff1Result, crmContact1, "Tester1", shouldExist: false);
			AssertRowExists(staff1Result, crmContact2, "Tester2", shouldExist: true);

			var staff2Result = RunOpportunityContactQuery(staff2);
			AssertRowExists(staff2Result, crmContact1, "Tester1", shouldExist: true);
			AssertRowExists(staff2Result, crmContact2, "Tester2", shouldExist: false);

			var controllerResult = RunOpportunityContactQuery(controller);
			AssertRowExists(controllerResult, crmContact1, "Tester1", shouldExist: true);
			AssertRowExists(controllerResult, crmContact2, "Tester2", shouldExist: true);
		}

		void AssertIsSelf(DataRow[] rows, Guid contactPK, string contactName, bool isSelf)
		{
			var row = rows.Single(r => (Guid)r["COC_PK"] == contactPK);
			AssertEquals("Contact " + contactName, (bool)row["IsSelf"], isSelf);
		}

		void AssertIsTeam(DataRow[] rows, Guid contactPK, string contactName, bool isTeam)
		{
			var row = rows.Single(r => (Guid)r["COC_PK"] == contactPK);
			AssertEquals("Contact " + contactName, (bool)row["IsTeam"], isTeam);
		}

		void AssertRowExists(DataRow[] rows, Guid contactPK, string contactName, bool shouldExist)
		{
			var result = rows.Any(r => (Guid)r["COC_PK"] == contactPK);
			AssertEquals("Contact" + contactName, shouldExist, result);
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

		DataRow[] RunOpportunityContactQuery(Guid loggedInStaff)
		{
			using (var command = TestConnection.Command("SELECT * FROM dbo.GetOpportunityContactsWithSecurityContext(@loggedInStaff)"))
			{
				command.AddParameter("@loggedInStaff", SqlDbType.UniqueIdentifier, loggedInStaff);
				return DataUtils.GetDataTableFromCommand(command).Select();
			}
		}
	}
}
