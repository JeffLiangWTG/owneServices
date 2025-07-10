using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.MasterData;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.MasterData
{
	[TestedType(typeof(CmdOrgAddressCapabilityUsageCollector))]
	class CmdOrgAddressCapabilityUsageCollectorTest : RefStlScriptWithDefaultsTest
	{
		readonly string pz1 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string pz2 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string pz3 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string adPk1 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string adPk2 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string adPk3 = Guid.NewGuid().ToString().ToUpperInvariant();
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

				INSERT dbo.OrgAddress (OA_PK, OA_OH, OA_IsActive, OA_Address1, OA_Address2, OA_City, OA_PostCode, OA_RN_NKCountryCode, OA_GeoLocation, OA_SystemLastEditTimeUtc) VALUES
					('{adPk1}', '{orgPk1}', 1, '10', 'Pit St', 'SYD', 2000, 'AU', geography::STGeomFromText('POINT (151.2093 -33.8688)', 4326), '2023-03-22 00:00:00'),
					('{adPk2}', '{orgPk2}', 2, '20', 'Ge St', 'Bri', 4000, 'AU', geography::STGeomFromText('POINT (151.2093 -33.8688)', 4326), '2023-03-22 00:00:00'),
					('{adPk3}', '{orgPk3}', 2, '20', 'Ge St', 'Bri', 4000, 'AU', geography::STGeomFromText('POINT (151.2093 -33.8688)', 4326), '2023-03-22 00:00:00');

				INSERT dbo.OrgAddressCapability (PZ_PK, PZ_AddressType, PZ_OA, PZ_IsMainAddress, PZ_SystemLastEditTimeUtc) VALUES
					('{pz1}', 'OFC', '{adPk1}', 1, '2003-03-22 00:00:00'),
					('{pz2}', 'PSC', '{adPk2}', 0, '2023-03-22 00:00:00'),
					('{pz3}', 'PSC', '{adPk3}', 0, '2023-03-22 00:00:00');

				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode, GC_OH_OrgProxy) VALUES
						('{compPk1}', 'SD1', 'Test SYD Company', 'AUD', 'AU', '{orgPk1}'),
						('{compPk2}', 'JTK', 'Test TYO Company', 'JPY', 'JP', '{orgPk2}');

				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_BranchName, GB_OH_OrgProxy, GB_IsActive, GB_GeoLocation, GB_SystemLastEditTimeUtc, GB_RL_NKHomePort) VALUES
						('{branchPk1}', 'SD1', '{compPk1}', 'UwBEADEATgBhAG0AZQA=', NULL, 1, geography::STGeomFromText('POINT (151.2093 -33.8688)', 4326), '2003-03-22 00:00:00', 'AUSYD');


				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_Address1, GB_WebAddress, GB_GC, GB_BranchName, GB_OH_OrgProxy, GB_IsActive, GB_GeoLocation, GB_SystemLastEditTimeUtc) VALUES
						('{branchPk2}', 'TK1', 'Test Address 1', 'www.test.au', '{compPk2}', 'TK1Name', NULL, 0, geography::STGeomFromText('POINT (0 0)', 4326), '2023-03-23 00:00:00');
";

			TestConnection.ExecuteNonQuery(sqlQuery);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2023, 3);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			var transaction1 = transactions.Single(t => t.Reference5 == pz1);
			var transaction2 = transactions.Single(t => t.Reference5 == pz2);

			AssertEquals("AdditionalRefs should be as expected",
				"{\"AddressType\":\"OFC\"," +
				$"\"OrgAddressUniqueId\":\"{adPk1}\"," +
				"\"IsMainAddress\":true," +
				"\"SystemLastEditTimeUtc\":\"2003-03-22T00:00:00\"}",
				transaction1.AdditionalRefs);

			AssertEquals("AdditionalRefs should be as expected",
				"{\"AddressType\":\"PSC\"," +
				$"\"OrgAddressUniqueId\":\"{adPk2}\"," +
				"\"IsMainAddress\":false," +
				"\"SystemLastEditTimeUtc\":\"2023-03-22T00:00:00\"}",
				transaction2.AdditionalRefs);

			AssertEquals("should only contain OrgAddressCapability with PZ_OA with OA_OH in GlbBranch or GlbCompany", transactions.Any(t => t.Reference5 == pz3), false);
		}
	}
}

