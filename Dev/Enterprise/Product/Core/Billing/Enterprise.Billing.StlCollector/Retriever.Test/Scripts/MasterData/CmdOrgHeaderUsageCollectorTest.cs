using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.MasterData;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.MasterData
{
	[TestedType(typeof(CmdOrgHeaderUsageCollector))]
	class CmdOrgHeaderUsageCollectorTest : RefStlScriptWithDefaultsTest
	{
		readonly string okPk1 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string okPk2 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string okPk3 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string okPk4 = Guid.NewGuid().ToString().ToUpperInvariant();

		readonly string compPk1 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string compPk2 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string branchPk1 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string branchPk2 = Guid.NewGuid().ToString().ToUpperInvariant();

		protected override bool IsMandatoryForMilestones => false;
		protected override void PrepareTestData()
		{
			var sqlQuery = $@"
                INSERT INTO dbo.OrgHeader (
                    OH_PK, OH_IsActive, OH_Code, OH_FullName, OH_Language, OH_RL_NKClosestPort,
                    OH_IsGlobalAccount, OH_IsNationalAccount, OH_IsForwarder, OH_IsShippingProvider,
                    OH_IsAirWholesaler, OH_IsSeaWholesaler, OH_IsRailProvider, OH_IsLineHaulProvider,
                    OH_IsMiscFreightServices, OH_IsAirCTO, OH_IsAirLine, OH_IsBroker, OH_IsContainerYard,
                    OH_IsLocalTransport, OH_IsPackDepot, OH_IsSeaCTO, OH_IsShippingLine, OH_IsUnpackDepot,
                    OH_IsRailHead, OH_IsRoadFreightDepot, OH_IsShippingConsortium, OH_IsFumigationContractor,
                    OH_IsSalesLead, OH_IsCompetitor, OH_IsTempAccount, OH_IsPersonalEffectsAccount,
                    OH_IsConsignee, OH_IsConsignor, OH_IsTransportClient, OH_IsWarehouseClient,
                    OH_IsDistributionCentre, OH_IsControllingCustomer, OH_IsControllingAgent,
                    OH_IsFerryWaterTerminal, OH_IsContainerLeasingCompany, OH_IsInlandWaterwayProvider,
                    OH_IsVGMContractor, OH_SystemLastEditTimeUtc
                ) VALUES
                    ('{okPk1}', 1, 'VML CCC', 'VML CCC', 'EN', 'Port1', 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, '2003-03-22 00:00:00'),
                    ('{okPk2}', 1, 'VML C1C', 'VML C1C', 'FR', 'Port2', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, '2023-03-22 00:00:00'),
                    ('{okPk3}', 1, 'VML C3C', N'臺北供應商 – 應付', 'ZH-TW', 'Port2', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, '2023-03-22 00:00:00'),
					('{okPk4}', 1, 'VML C4C', N'臺北供應商 – 應付', 'ZH-TW', 'Port2', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, '2023-03-22 00:00:00');

				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode, GC_OH_OrgProxy) VALUES
						('{compPk1}', 'SD1', 'Test SYD Company', 'AUD', 'AU', '{okPk1}'),
						('{compPk2}', 'JTK', 'Test TYO Company', 'JPY', 'JP', '{okPk2}');

				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_BranchName, GB_OH_OrgProxy, GB_IsActive, GB_GeoLocation, GB_SystemLastEditTimeUtc, GB_RL_NKHomePort) VALUES
						('{branchPk1}', 'SD1', '{compPk1}', 'UwBEADEATgBhAG0AZQA=', NULL, 1, geography::STGeomFromText('POINT (151.2093 -33.8688)', 4326), '2003-03-22 00:00:00', 'AUSYD');


				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_Address1, GB_WebAddress, GB_GC, GB_BranchName, GB_OH_OrgProxy, GB_IsActive, GB_GeoLocation, GB_SystemLastEditTimeUtc) VALUES
						('{branchPk2}', 'TK1', 'Test Address 1', 'www.test.au', '{compPk2}', 'TK1Name', '{okPk3}', 0, geography::STGeomFromText('POINT (0 0)', 4326), '2023-03-23 00:00:00');
";

			TestConnection.ExecuteNonQuery(sqlQuery);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2023, 3);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			var transaction1 = transactions.Single(t => t.Reference5 == okPk1);
			var transaction2 = transactions.Single(t => t.Reference5 == okPk2);
			var transaction3 = transactions.Single(t => t.Reference5 == okPk3);

			AssertEquals("AdditionalRefs should be as expected",
				"{\"CompanyName\":\"VgBNAEwAIABDAEMAQwA=\"," +
				"\"OrgCategory\":\"BUS\"," +
				"\"PortCode\":\"Port1\"," +
				"\"IsGlobalAccount\":true," +
				"\"IsNationalAccount\":true," +
				"\"IsForwarder\":true," +
				"\"IsShippingProvider\":true," +
				"\"IsAirWholesaler\":true," +
				"\"IsSeaWholesaler\":true," +
				"\"IsRailProvider\":true," +
				"\"IsLineHaulProvider\":true," +
				"\"IsMiscFreightServices\":true," +
				"\"IsAirCTO\":true," +
				"\"IsAirLine\":true," +
				"\"IsBroker\":true," +
				"\"IsContainerYard\":true," +
				"\"IsLocalTransport\":true," +
				"\"IsPackDepot\":true," +
				"\"IsSeaCTO\":true," +
				"\"IsShippingLine\":true," +
				"\"IsUnpackDepot\":true," +
				"\"IsRailHead\":true," +
				"\"IsRoadFreightDepot\":true," +
				"\"IsShippingConsortium\":true," +
				"\"IsFumigationContractor\":true," +
				"\"IsSalesLead\":true," +
				"\"IsCompetitor\":true," +
				"\"IsTempAccount\":true," +
				"\"IsPersonalEffectsAccount\":true," +
				"\"IsConsignee\":true," +
				"\"IsConsignor\":true," +
				"\"IsTransportClient\":true," +
				"\"IsWarehouseClient\":true," +
				"\"IsDistributionCentre\":true," +
				"\"IsControllingCustomer\":true," +
				"\"IsControllingAgent\":true," +
				"\"IsFerryWaterTerminal\":true," +
				"\"IsContainerLeasingCompany\":true," +
				"\"IsInlandWaterwayProvider\":true," +
				"\"IsVGMContractor\":true," +
				"\"SystemLastEditTimeUtc\":\"2003-03-22T00:00:00\"}",
				transaction1.AdditionalRefs);

			AssertEquals("AdditionalRefs should be as expected",
			   "{\"CompanyName\":\"VgBNAEwAIABDADEAQwA=\"," +
			   "\"OrgCategory\":\"BUS\"," +
			   "\"Language\":\"FR\"," +
			   "\"PortCode\":\"Port2\"," +
			   "\"SystemLastEditTimeUtc\":\"2023-03-22T00:00:00\"}",
			   transaction2.AdditionalRefs);

			AssertEquals("AdditionalRefs should be as expected",
			   "{\"CompanyName\":\"+oEXU5tPyWFGVSAAEyAgAMlh2E4=\"," +
			   "\"OrgCategory\":\"BUS\"," +
			   "\"Language\":\"ZH-TW\"," +
			   "\"PortCode\":\"Port2\"," +
			   "\"SystemLastEditTimeUtc\":\"2023-03-22T00:00:00\"}",
			   transaction3.AdditionalRefs);

			AssertEquals("should only contain OrgHeader with OH_PK in GlbBranch or GlbCompany", transactions.Any(t => t.Reference5 == okPk4), false);
		}
	}
}

