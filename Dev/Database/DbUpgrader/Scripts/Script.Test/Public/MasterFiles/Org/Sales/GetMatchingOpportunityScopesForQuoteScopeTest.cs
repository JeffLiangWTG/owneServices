using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.Org.Sales;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Org.Sales.Testing
{
	[TestedType(typeof(GetMatchingOpportunityScopesForQuoteScope))]
	class GetMatchingOpportunityScopesForQuoteScopeTest : DbCreateScriptTest
	{
		public void TestMatchOnMandatory()
		{
			var newOrg1 = TestDataCreator.CreateOrganisation("BR2", "Test Organisation 1");
			var newOrg2 = TestDataCreator.CreateOrganisation("BR3", "Test Organisation 2");
			var companyPk = TestDataCreator.CreateCompany("TC1", "IT", "EUR");
			var opp1Pk = TestDataCreator.CreateCrmOpportunity("O10000000", "Test Opportunity", newOrg1, companyPk);
			var opp2Pk = TestDataCreator.CreateCrmOpportunity("O10000001", "Test Opportunity Two", newOrg2, companyPk);
			var oppScope1Pk = TestDataCreator.CreateCrmOpportunityScope(oppPK: opp1Pk, scopeID: 1, productCode: "FWD", origin: "AUSYD", destination: "ADCAN", transportMode: "AIR", containerMode: "ULD");
			var oppScope2Pk = TestDataCreator.CreateCrmOpportunityScope(oppPK: opp1Pk, scopeID: 2, productCode: "FWD", origin: "AUMEL", destination: "AUSYD", transportMode: "SEA", containerMode: "ALL");
			var oppScope3Pk = TestDataCreator.CreateCrmOpportunityScope(oppPK: opp1Pk, scopeID: 3, productCode: "FWD", origin: "AU", destination: "ADCAN", transportMode: "ALL", containerMode: "ALL");
			var oppScope4Pk = TestDataCreator.CreateCrmOpportunityScope(oppPK: opp1Pk, scopeID: 4, productCode: "FWD", origin: "AUSYD", destination: "AD", transportMode: "AIR", containerMode: "LSE");
			var oppScope5Pk = TestDataCreator.CreateCrmOpportunityScope(oppPK: opp2Pk, scopeID: 5, productCode: "FWD", origin: "AUSYD", destination: "ADCAN", transportMode: "AIR", containerMode: "ULD");

			var ratingHeaderPk = TestDataCreator.CreateRatingHeader("I0100000", companyPk, newOrg1);
			var quoteScope1Pk = TestDataCreator.CreateQuoteScope(ratingHeaderPk, 1, productCode: "FWD", origin: "AUSYD", destination: "ADCAN", transportMode: "AIR", containerMode: "ULD");
			var quoteScope2Pk = TestDataCreator.CreateQuoteScope(ratingHeaderPk, 2, productCode: "FWD", origin: "AUSYD", destination: "ADCAN", transportMode: "ALL", containerMode: "ALL");
			var quoteScope3Pk = TestDataCreator.CreateQuoteScope(ratingHeaderPk, 3, productCode: "FWD", origin: "AUSYD", destination: "ADCAN", transportMode: "AIR", containerMode: "LSE");

			var table = RunFunction(quoteScope1Pk);

			var rows = table.Select("");
			AssertEquals("First quote scope return count", 2, table.Rows.Count);
			AssertContainsExactElementsInExactOrder("First quote scope row", new[] { oppScope1Pk, oppScope3Pk }, rows.Select(r => (Guid)r["COS_PK"]));

			table = RunFunction(quoteScope2Pk);

			AssertEquals("Return Count", 1, table.Rows.Count);
			AssertEquals("Second Quote Scope Row", oppScope3Pk, table.Rows[0]["COS_PK"]);

			table = RunFunction(quoteScope3Pk);

			rows = table.Select("");
			AssertEquals("Return Count", 2, table.Rows.Count);
			AssertContainsExactElementsInExactOrder("Third quote scope row", new[] { oppScope3Pk, oppScope4Pk }, rows.Select(r => (Guid)r["COS_PK"]));
		}

		public void TestMatchOnAddressesAndMandatory()
		{
			var newOrg = TestDataCreator.CreateOrganisation("BR2", "Test Organisation 2");
			var companyPk = TestDataCreator.CreateCompany("TC1", "IT", "EUR");
			var pickupAddress = TestDataCreator.CreateAddress(newOrg, "OA1", "pickup address");
			var deliveryAddress = TestDataCreator.CreateAddress(newOrg, "OA2", "delivery address");
			var wrongAddress = TestDataCreator.CreateAddress(newOrg, "BAD", "wrong address");
			var pickupAddressPostCode = "2000";
			var deliveryAddressPostCode = "3000";

			var oppPk = TestDataCreator.CreateCrmOpportunity("O01000000", "Test Opportunity", newOrg, companyPk);
			var oppScope1Pk = TestDataCreator.CreateCrmOpportunityScope(oppPK: oppPk, scopeID: 1, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "AIR", containerMode: "ULD", pickupAddress: pickupAddress, deliveryAddress: deliveryAddress);
			var oppScope2Pk = TestDataCreator.CreateCrmOpportunityScope(oppPK: oppPk, scopeID: 2, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "AIR", containerMode: "ULD", pickupAddressPostCode: pickupAddressPostCode, deliveryAddressPostCode: deliveryAddressPostCode);
			var oppScope3Pk = TestDataCreator.CreateCrmOpportunityScope(oppPK: oppPk, scopeID: 3, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "AIR", containerMode: "ULD", pickupAddressPostCode: "1000", deliveryAddressPostCode: "1000");
			var oppScope4Pk = TestDataCreator.CreateCrmOpportunityScope(oppPK: oppPk, scopeID: 4, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "AIR", containerMode: "ULD", pickupAddress: wrongAddress, deliveryAddress: wrongAddress);

			var ratingHeaderPk = TestDataCreator.CreateRatingHeader("I0100000", companyPk, newOrg);
			var quoteScope1Pk = TestDataCreator.CreateQuoteScope(ratingHeaderPk, 1, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "AIR", containerMode: "ULD", pickupAddress: pickupAddress, deliveryAddress: deliveryAddress);

			var table = RunFunction(quoteScope1Pk);

			AssertEquals("Return Count", 1, table.Rows.Count);
			AssertEquals("First Scope Row", oppScope1Pk, table.Rows[0]["COS_PK"]);

			var quoteScope2Pk = TestDataCreator.CreateQuoteScope(ratingHeaderPk, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "AIR", containerMode: "ULD", pickupAddressPostCode: pickupAddressPostCode, deliveryAddressPostCode: deliveryAddressPostCode, iDNumber: 2);

			table = RunFunction(quoteScope2Pk);

			AssertEquals("Return Count", 1, table.Rows.Count);
			AssertEquals("Second Scope Row", oppScope2Pk, table.Rows[0]["COS_PK"]);
		}

		public void TestMatchOnEffectiveDateAndMandatory()
		{
			var newOrg = TestDataCreator.CreateOrganisation("BR2", "Test Organisation 2");
			var companyPk = TestDataCreator.CreateCompany("TC1", "IT", "EUR");
			var opp1Pk = TestDataCreator.CreateCrmOpportunity("O01000000", "Test Opportunity", newOrg, companyPk, startDate: "2024-10-11", endDate: "2024-10-30");
			var opp2Pk = TestDataCreator.CreateCrmOpportunity("O01000001", "Test Opportunity 1", newOrg, companyPk, startDate: "2025-1-11", endDate: "2025-1-31");
			var opp3Pk = TestDataCreator.CreateCrmOpportunity("O01000002", "Test Opportunity 2", newOrg, companyPk, startDate: "2024-10-11");
			var opp4Pk = TestDataCreator.CreateCrmOpportunity("O01000003", "Test Opportunity 3", newOrg, companyPk, endDate: "2024-10-28");
			var oppScope1Pk = TestDataCreator.CreateCrmOpportunityScope(oppPK: opp1Pk, scopeID: 1, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "AIR", containerMode: "ULD");
			var oppScope2Pk = TestDataCreator.CreateCrmOpportunityScope(oppPK: opp2Pk, scopeID: 2, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "AIR", containerMode: "ULD");
			var oppScope3Pk = TestDataCreator.CreateCrmOpportunityScope(oppPK: opp2Pk, scopeID: 3, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "SEA", containerMode: "ALL");
			var oppScope4Pk = TestDataCreator.CreateCrmOpportunityScope(oppPK: opp3Pk, scopeID: 4, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "AIR", containerMode: "ULD");
			var oppScope5Pk = TestDataCreator.CreateCrmOpportunityScope(oppPK: opp4Pk, scopeID: 5, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "SEA", containerMode: "ALL");

			var ratingHeaderPk = TestDataCreator.CreateRatingHeader("I0100000", companyPk, newOrg, "2024-10-20", "2024-10-21");
			var quoteScope1Pk = TestDataCreator.CreateQuoteScope(ratingHeaderPk, 1, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "AIR", containerMode: "ULD");
			var quoteScope2Pk = TestDataCreator.CreateQuoteScope(ratingHeaderPk, 2, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "SEA", containerMode: "ALL");

			var table = RunFunction(quoteScope1Pk);

			var rows = table.Select("");
			AssertEquals("First quote scope return count", 2, table.Rows.Count);
			AssertContainsExactElementsInExactOrder("First quote scope row", new[] { oppScope1Pk, oppScope4Pk }, rows.Select(r => (Guid)r["COS_PK"]));

			table = RunFunction(quoteScope2Pk);

			AssertEquals("Second quote scope return count", 1, table.Rows.Count);
			AssertEquals("Second quote Scope row", oppScope5Pk, table.Rows[0]["COS_PK"]);
		}

		public void TestMatchOnExtrasAndMandatory()
		{
			const string commodityCode1 = "COM";
			const string incoterm1 = "INC";
			const string serviceLevel1 = "SER";

			const string commodityCode2 = "COM2";
			const string incoterm2 = "IN2";
			const string serviceLevel2 = "SE2";

			var newOrg = TestDataCreator.CreateOrganisation("BR2", "Test Organisation 2");
			var companyPk = TestDataCreator.CreateCompany("TC1", "IT", "EUR");
			var oppPk = TestDataCreator.CreateCrmOpportunity("O01000000", "Test Opportunity", newOrg, companyPk);
			var oppScope1Pk = TestDataCreator.CreateCrmOpportunityScope(oppPK: oppPk, scopeID: 1, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "AIR", containerMode: "ULD", commodityCode: commodityCode1, incoTerm: incoterm1, serviceLevel: serviceLevel1);
			var oppScope2Pk = TestDataCreator.CreateCrmOpportunityScope(oppPK: oppPk, scopeID: 2, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "AIR", containerMode: "ULD", commodityCode: commodityCode2, incoTerm: incoterm2, serviceLevel: serviceLevel2);
			var oppScope3Pk = TestDataCreator.CreateCrmOpportunityScope(oppPK: oppPk, scopeID: 3, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "AIR", containerMode: "ULD", commodityCode: commodityCode1, incoTerm: incoterm1, serviceLevel: serviceLevel2);
			var oppScope4Pk = TestDataCreator.CreateCrmOpportunityScope(oppPK: oppPk, scopeID: 4, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "AIR", containerMode: "ULD", commodityCode: commodityCode1, incoTerm: incoterm2, serviceLevel: serviceLevel1);
			var oppScope5Pk = TestDataCreator.CreateCrmOpportunityScope(oppPK: oppPk, scopeID: 5, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "AIR", containerMode: "ULD", commodityCode: commodityCode2, incoTerm: incoterm1, serviceLevel: serviceLevel1);

			var ratingHeaderPk = TestDataCreator.CreateRatingHeader("I0100000", companyPk, newOrg);
			var quoteScope1Pk = TestDataCreator.CreateQuoteScope(ratingHeaderPk, 1, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "AIR", containerMode: "ULD", commodityCode: commodityCode1, incoTerm: incoterm1, serviceLevel: serviceLevel1);

			var table = RunFunction(quoteScope1Pk);

			AssertEquals("Return Count", 1, table.Rows.Count);
			AssertEquals("First Scope Row", oppScope1Pk, table.Rows[0]["COS_PK"]);

			var quoteScope2Pk = TestDataCreator.CreateQuoteScope(ratingHeaderPk, 2, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "AIR", containerMode: "ULD", commodityCode: commodityCode2, incoTerm: incoterm2, serviceLevel: serviceLevel2);

			table = RunFunction(quoteScope2Pk);

			AssertEquals("Return Count", 1, table.Rows.Count);
			AssertEquals("First Scope Row", oppScope2Pk, table.Rows[0]["COS_PK"]);
		}

		DataTable RunFunction(Guid quoteScopePk)
		{
			using (var command = Db.Connection.Command("SELECT * FROM GetMatchingOpportunityScopesForQuoteScope(@QuoteScopePK)"))
			{
				command.CommandType = CommandType.Text;
				command.AddParameter("@QuoteScopePK", SqlDbType.UniqueIdentifier, quoteScopePk);
				var result = DataUtils.GetDataTableFromCommand(command);

				return result;
			}
		}
	}
}
