using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.MasterData;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.MasterData
{
	[TestedType(typeof(CmdOrgAddressUsageCollector))]
	class CmdOrgAddressUsageCollectorTest : RefStlScriptWithDefaultsTest
	{
		readonly string oaPk1 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string oaPk2 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string oaPk3 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string orgPk1 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string orgPk2 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string orgPk3 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string compPk1 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string compPk2 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string branchPk1 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string branchPk2 = Guid.NewGuid().ToString().ToUpperInvariant();

		protected override bool IsMandatoryForMilestones => false;
		protected override void PrepareTestData()
		{
			var sqlQuery = $@"
				INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_IsActive) VALUES
					('{orgPk1}', 'TESTORG1', 1),
					('{orgPk2}', 'TESTORG2', 0),
					('{orgPk3}', 'TESTORG3', 0);

				INSERT INTO dbo.OrgAddress (OA_PK, OA_OH, OA_IsActive, OA_Address1, OA_Address2, OA_City, OA_PostCode, OA_RN_NKCountryCode, OA_GeoLocation, OA_SystemLastEditTimeUtc) VALUES
					('{oaPk1}', '{orgPk1}', 1, '10', 'Pit St', 'SYD', 2000, 'AU', geography::STGeomFromText('POINT (151.2093 -33.8688)', 4326), '2003-03-22 00:00:00');

				INSERT INTO dbo.OrgAddress (OA_PK, OA_OH, OA_IsActive, OA_Address1, OA_Address2, OA_City, OA_PostCode, OA_RN_NKCountryCode, OA_GeoLocation, OA_SystemLastEditTimeUtc, OA_CompanyNameOverride, OA_State) VALUES
					('{oaPk2}', '{orgPk2}', 0, '20', 'Ge St', '高雄市', 4000, 'AU', geography::STGeomFromText('POINT (151.2093 -33.8688)', 4326), '2023-03-22 00:00:00', 'test company name', 'NSW'),
					('{oaPk3}', '{orgPk3}', 0, '20', 'Ge St', '高雄市', 4000, 'AU', geography::STGeomFromText('POINT (151.2093 -33.8688)', 4326), '2023-03-22 00:00:00', 'test company name', 'NSW');

				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode, GC_OH_OrgProxy) VALUES
						('{compPk1}', 'SD1', 'Test SYD Company', 'AUD', 'AU', '{orgPk1}'),
						('{compPk2}', 'JTK', 'Test TYO Company', 'JPY', 'JP', NULL);

				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_BranchName, GB_OH_OrgProxy, GB_IsActive, GB_GeoLocation, GB_SystemLastEditTimeUtc, GB_RL_NKHomePort) VALUES
						('{branchPk1}', 'SD1', '{compPk1}', 'UwBEADEATgBhAG0AZQA=', NULL, 1, geography::STGeomFromText('POINT (151.2093 -33.8688)', 4326), '2003-03-22 00:00:00', 'AUSYD');


				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_Address1, GB_WebAddress, GB_GC, GB_BranchName, GB_OH_OrgProxy, GB_IsActive, GB_GeoLocation, GB_SystemLastEditTimeUtc) VALUES
						('{branchPk2}', 'TK1', 'Test Address 1', 'www.test.au', '{compPk2}', 'TK1Name', '{orgPk2}', 0, geography::STGeomFromText('POINT (0 0)', 4326), '2023-03-23 00:00:00');
";
			TestConnection.ExecuteNonQuery(sqlQuery);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2023, 3);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			var transaction1 = transactions.Single(t => t.Reference5 == oaPk1);
			var transaction2 = transactions.Single(t => t.Reference5 == oaPk2);

			AssertEquals("AdditionalRefs should be as expected",
				"{\"Address1\":\"MQAwAA==\"," +
				"\"Address2\":\"UABpAHQAIABTAHQA\"," +
				"\"PostCode\":\"MgAwADAAMAA=\"," +
				$"\"OrgUniqueId\":\"{orgPk1}\"," +
				"\"CountryCode\":\"AU\"," +
				"\"City\":\"UwBZAEQA\"," +
				"\"GeoLocation\":\"POINT (151.2093 -33.8688)\"," +
				"\"SystemLastEditTimeUtc\":\"2003-03-22T00:00:00\"}",
				transaction1.AdditionalRefs);

			AssertEquals("AdditionalRefs should be as expected",
				"{\"CompanyName\":\"dABlAHMAdAAgAGMAbwBtAHAAYQBuAHkAIABuAGEAbQBlAA==\"," +
				"\"Address1\":\"MgAwAA==\"," +
				"\"Address2\":\"RwBlACAAUwB0AA==\"," +
				"\"State\":\"TgBTAFcA\"," +
				"\"PostCode\":\"NAAwADAAMAA=\"," +
				$"\"OrgUniqueId\":\"{orgPk2}\"," +
				"\"IsActive\":false," +
				"\"CountryCode\":\"AU\"," +
				"\"City\":\"PwA/AD8A\"," +
				"\"GeoLocation\":\"POINT (151.2093 -33.8688)\"," +
				"\"SystemLastEditTimeUtc\":\"2023-03-22T00:00:00\"}",
				transaction2.AdditionalRefs);

			AssertEquals("should only contain OrgAddress with OA_OH in GlbBranch or GlbCompany", transactions.Any(t => t.Reference5 == oaPk3), false);
		}
	}
}

