using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.Org.Sales;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Org.Sales.Testing
{
	[TestedType(typeof(GetMatchingQuoteScopesForOpportunityScope))]
	class GetMatchingQuoteScopesForOpportunityScopeTest : DbCreateScriptTest
	{
		public void TestMatchOnMandatory()
		{
			var newOrg1 = TestDataCreator.CreateOrganisation("BR2", "Test Organisation 1");
			var newOrg2 = TestDataCreator.CreateOrganisation("BR3", "Test Organisation 2");
			var companyPk = TestDataCreator.CreateCompany("TC1", "IT", "EUR");
			var opp1Pk = TestDataCreator.CreateCrmOpportunity("O10000000", "Test Opportunity", newOrg1, companyPk);
			var opp2Pk = TestDataCreator.CreateCrmOpportunity("O10000001", "Test Opportunity Two", newOrg2, companyPk);
			var oppScope1Pk = TestDataCreator.CreateCrmOpportunityScope(oppPK: opp1Pk, scopeID: 1, productCode: "FWD", origin: "AUSYD", destination: "ADCAN", transportMode: "AIR", containerMode: "ULD");
			var oppScope2Pk = TestDataCreator.CreateCrmOpportunityScope(oppPK: opp1Pk, scopeID: 2, productCode: "FWD", origin: "AU", destination: "ADCAN", transportMode: "ALL", containerMode: "ALL");
			var oppScope3Pk = TestDataCreator.CreateCrmOpportunityScope(oppPK: opp1Pk, scopeID: 3, productCode: "FWD", origin: "AUSYD", destination: "AD", transportMode: "AIR", containerMode: "LSE");

			var ratingHeaderPk = TestDataCreator.CreateRatingHeader("I0100000", companyPk, newOrg1);
			var quoteScope1Pk = TestDataCreator.CreateQuoteScope(ratingHeaderPk, 1, productCode: "FWD", origin: "AUSYD", destination: "ADCAN", transportMode: "AIR", containerMode: "ULD");
			var quoteScope2Pk = TestDataCreator.CreateQuoteScope(ratingHeaderPk, 2, productCode: "FWD", origin: "AU", destination: "ADCAN", transportMode: "AIR", containerMode: "ULD");
			var quoteScope3Pk = TestDataCreator.CreateQuoteScope(ratingHeaderPk, 3, productCode: "FWD", origin: "AUSYD", destination: "ADCAN", transportMode: "ALL", containerMode: "ALL");
			var quoteScope4Pk = TestDataCreator.CreateQuoteScope(ratingHeaderPk, 4, productCode: "FWD", origin: "AUSYD", destination: "ADCAN", transportMode: "AIR", containerMode: "LSE");
			var quoteScope5Pk = TestDataCreator.CreateQuoteScope(ratingHeaderPk, 5, productCode: "FWD", origin: "AUSYD", destination: "AD", transportMode: "AIR", containerMode: "ULD");
			var table = RunFunction(oppScope1Pk);

			AssertEquals("Return Count", 1, table.Rows.Count);
			AssertEquals("First Opp Scope Row", quoteScope1Pk, table.Rows[0]["QS_PK"]);

			table = RunFunction(oppScope2Pk);

			var rows = table.Select("");
			AssertEquals("Return Count", 4, table.Rows.Count);
			AssertArrayEqualsByElements("Second Opp Scope Row", new[] { quoteScope1Pk, quoteScope2Pk, quoteScope3Pk, quoteScope4Pk }, rows.Select(r => (Guid)r["QS_PK"]).ToArray());

			table = RunFunction(oppScope3Pk);

			AssertEquals("Return Count", 1, table.Rows.Count);
			AssertEquals("Third Opp Scope Row", quoteScope4Pk, table.Rows[0]["QS_PK"]);
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

			var ratingHeaderPk = TestDataCreator.CreateRatingHeader("I0100000", companyPk, newOrg);
			var quoteScope1Pk = TestDataCreator.CreateQuoteScope(ratingHeaderPk, 1, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "AIR", containerMode: "ULD", pickupAddress: pickupAddress, deliveryAddress: deliveryAddress);
			var quoteScope2Pk = TestDataCreator.CreateQuoteScope(ratingHeaderPk, 2, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "AIR", containerMode: "ULD", pickupAddressPostCode: pickupAddressPostCode, deliveryAddressPostCode: deliveryAddressPostCode);
			var quoteScope3Pk = TestDataCreator.CreateQuoteScope(ratingHeaderPk, 3, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "AIR", containerMode: "ULD", pickupAddressPostCode: "1000", deliveryAddressPostCode: "1000");
			var quoteScope4Pk = TestDataCreator.CreateQuoteScope(ratingHeaderPk, 4, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "AIR", containerMode: "ULD", pickupAddress: wrongAddress, deliveryAddress: wrongAddress);

			var table = RunFunction(oppScope1Pk);

			AssertEquals("Return Count", 1, table.Rows.Count);
			AssertEquals("First Opp Scope Row", quoteScope1Pk, table.Rows[0]["QS_PK"]);

			table = RunFunction(oppScope2Pk);

			AssertEquals("Return Count", 1, table.Rows.Count);
			AssertEquals("Second Opp Scope Row", quoteScope2Pk, table.Rows[0]["QS_PK"]);
		}

		public void TestMatchOnEffectiveDateAndMandatory()
		{
			var newOrg = TestDataCreator.CreateOrganisation("BR2", "Test Organisation 2");
			var companyPk = TestDataCreator.CreateCompany("TC1", "IT", "EUR");
			var opp1Pk = TestDataCreator.CreateCrmOpportunity("O01000000", "Test Opportunity", newOrg, companyPk, startDate: "2024-10-11", endDate: "2024-10-30");
			var opp2Pk = TestDataCreator.CreateCrmOpportunity("O01000001", "Test Opportunity 1", newOrg, companyPk, startDate: "2025-1-11", endDate: "2025-1-31");
			var opp3Pk = TestDataCreator.CreateCrmOpportunity("O01000002", "Test Opportunity 2", newOrg, companyPk, startDate: "2024-10-28");
			var opp4Pk = TestDataCreator.CreateCrmOpportunity("O01000003", "Test Opportunity 3", newOrg, companyPk, endDate: "2024-10-21");
			var oppScope1Pk = TestDataCreator.CreateCrmOpportunityScope(oppPK: opp1Pk, scopeID: 1, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "AIR", containerMode: "ULD");
			var oppScope2Pk = TestDataCreator.CreateCrmOpportunityScope(oppPK: opp2Pk, scopeID: 2, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "AIR", containerMode: "ULD");
			var oppScope3Pk = TestDataCreator.CreateCrmOpportunityScope(oppPK: opp3Pk, scopeID: 3, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "SEA", containerMode: "ALL");
			var oppScope4Pk = TestDataCreator.CreateCrmOpportunityScope(oppPK: opp4Pk, scopeID: 4, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "SEA", containerMode: "ALL");

			var ratingHeader1Pk = TestDataCreator.CreateRatingHeader("I0100000", companyPk, newOrg, "2024-10-20", "2024-10-21");
			var ratingHeader2Pk = TestDataCreator.CreateRatingHeader("I0100001", companyPk, newOrg, "2024-10-29", "2024-10-30");
			var quoteScope1Pk = TestDataCreator.CreateQuoteScope(ratingHeader1Pk, 1, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "AIR", containerMode: "ULD");
			var quoteScope2Pk = TestDataCreator.CreateQuoteScope(ratingHeader1Pk, 2, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "SEA", containerMode: "ALL");
			var quoteScope3Pk = TestDataCreator.CreateQuoteScope(ratingHeader2Pk, 3, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "AIR", containerMode: "ULD");
			var quoteScope4Pk = TestDataCreator.CreateQuoteScope(ratingHeader2Pk, 4, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "SEA", containerMode: "ALL");

			var table = RunFunction(oppScope1Pk);

			var rows = table.Select("");
			AssertEquals("First opp scope return count", 2, table.Rows.Count);
			AssertContainsExactElementsInExactOrder("First quote scope row", new[] { quoteScope1Pk, quoteScope3Pk }, rows.Select(r => (Guid)r["QS_PK"]));

			table = RunFunction(oppScope2Pk);

			AssertEquals("Second opp scope return count", 0, table.Rows.Count);

			table = RunFunction(oppScope3Pk);

			AssertEquals("Third opp scope return count", 1, table.Rows.Count);
			AssertEquals("Third Opp Scope Row", quoteScope4Pk, table.Rows[0]["QS_PK"]);

			table = RunFunction(oppScope4Pk);

			AssertEquals("Fourth opp scope return count", 1, table.Rows.Count);
			AssertEquals("Fourth Opp Scope Row", quoteScope2Pk, table.Rows[0]["QS_PK"]);
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

			var ratingHeaderPk = TestDataCreator.CreateRatingHeader("I0100000", companyPk, newOrg);
			var quoteScope1Pk = TestDataCreator.CreateQuoteScope(ratingHeaderPk, 1, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "AIR", containerMode: "ULD", commodityCode: commodityCode1, incoTerm: incoterm1, serviceLevel: serviceLevel1);
			var quoteScope2Pk = TestDataCreator.CreateQuoteScope(ratingHeaderPk, 2, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "AIR", containerMode: "ULD", commodityCode: commodityCode2, incoTerm: incoterm2, serviceLevel: serviceLevel2);
			var quoteScope3Pk = TestDataCreator.CreateQuoteScope(ratingHeaderPk, 3, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "AIR", containerMode: "ULD", commodityCode: commodityCode1, incoTerm: incoterm1, serviceLevel: serviceLevel2);
			var quoteScope4Pk = TestDataCreator.CreateQuoteScope(ratingHeaderPk, 4, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "AIR", containerMode: "ULD", commodityCode: commodityCode1, incoTerm: incoterm2, serviceLevel: serviceLevel1);
			var quoteScope5Pk = TestDataCreator.CreateQuoteScope(ratingHeaderPk, 5, productCode: "FWD", origin: "AUSYD", destination: "AUBNE", transportMode: "AIR", containerMode: "ULD", commodityCode: commodityCode2, incoTerm: incoterm1, serviceLevel: serviceLevel1);

			var table = RunFunction(oppScope1Pk);

			AssertEquals("Return Count", 1, table.Rows.Count);
			AssertEquals("First Opp Scope Row", quoteScope1Pk, table.Rows[0]["QS_PK"]);

			table = RunFunction(oppScope2Pk);

			AssertEquals("Return Count", 1, table.Rows.Count);
			AssertEquals("First Opp Scope Row", quoteScope2Pk, table.Rows[0]["QS_PK"]);
		}

		DataTable RunFunction(Guid opportunityScopePK)
		{
			using (var command = Db.Connection.Command("SELECT * FROM GetMatchingQuoteScopesForOpportunityScope(@OpportunityScopePK)"))
			{
				command.CommandType = CommandType.Text;
				command.AddParameter("@OpportunityScopePK", SqlDbType.UniqueIdentifier, opportunityScopePK);
				var result = DataUtils.GetDataTableFromCommand(command);

				return result;
			}
		}
	}
}
