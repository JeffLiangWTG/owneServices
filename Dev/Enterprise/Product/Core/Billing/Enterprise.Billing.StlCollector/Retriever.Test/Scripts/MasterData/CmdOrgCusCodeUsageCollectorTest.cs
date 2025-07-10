using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.MasterData;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.MasterData
{
	[TestedType(typeof(CmdOrgCusCodeUsageCollector))]
	class CmdOrgCusCodeUsageCollectorTest : RefStlScriptWithDefaultsTest
	{
		readonly string okPk1 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string okPk2 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string okPk3 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string okPk4 = Guid.NewGuid().ToString().ToUpperInvariant();

		readonly string okPh1 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string okPh2 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string okPh3 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string okPh4 = Guid.NewGuid().ToString().ToUpperInvariant();

		readonly string okPa1 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string okPa2 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string okPa3 = Guid.NewGuid().ToString().ToUpperInvariant();

		readonly string compPk1 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string compPk2 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string branchPk1 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string branchPk2 = Guid.NewGuid().ToString().ToUpperInvariant();

		protected override bool IsMandatoryForMilestones => false;
		protected override void PrepareTestData()
		{
			var sqlQuery = $@"
				INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_IsActive) VALUES
					('{okPh1}', 'TESTORG1', 1),
					('{okPh2}', 'TESTORG2', 0),
					('{okPh3}', 'TESTORG3', 0),
					('{okPh4}', 'TESTORG4', 0);

				INSERT INTO dbo.OrgAddress (OA_PK, OA_OH, OA_IsActive, OA_Address1, OA_Address2, OA_City, OA_PostCode, OA_RN_NKCountryCode, OA_GeoLocation, OA_SystemLastEditTimeUtc) VALUES
					('{okPa1}', '{okPh1}', 1, '10', 'Pit St', 'SYD', 2000, 'AU', geography::STGeomFromText('POINT (151.2093 -33.8688)', 4326), '2023-03-22 00:00:00'),
					('{okPa2}', '{okPh2}', 2, '20', 'Ge St', 'Bri', 4000, 'AU', geography::STGeomFromText('POINT (151.2093 -33.8688)', 4326), '2023-03-22 00:00:00'),
					('{okPa3}', '{okPh3}', 2, '20', 'Ge St', 'Bri', 4000, 'AU', geography::STGeomFromText('POINT (151.2093 -33.8688)', 4326), '2023-03-22 00:00:00');

				INSERT INTO dbo.OrgCusCode (OK_PK, OK_CustomsRegNo, OK_CodeType, OK_OH, OK_OA_PremisesAddress, OK_RN_NKCodeCountry, OK_SystemLastEditTimeUtc) VALUES
                    ('{okPk1}', N'C1H1', N'C1C', '{okPh1}', '{okPa1}', 'SY', '2003-03-22 00:00:00'),
                    ('{okPk2}', N'C2H2', N'C1C', '{okPh2}', '{okPa2}', 'SY', '2023-03-22 00:00:00'),
					('{okPk3}', N'臺北進口商', N'C1C', '{okPh3}', '{okPa3}', 'ZH', '2023-03-22 00:00:00'),
					('{okPk4}', N'臺北進口商', N'C1C', '{okPh4}', '{okPa3}', 'ZH', '2023-03-22 00:00:00');

				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode, GC_OH_OrgProxy) VALUES
						('{compPk1}', 'SD1', 'Test SYD Company', 'AUD', 'AU', '{okPh1}'),
						('{compPk2}', 'JTK', 'Test TYO Company', 'JPY', 'JP', '{okPh2}');

				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_BranchName, GB_OH_OrgProxy, GB_IsActive, GB_GeoLocation, GB_SystemLastEditTimeUtc, GB_RL_NKHomePort) VALUES
						('{branchPk1}', 'SD1', '{compPk1}', 'UwBEADEATgBhAG0AZQA=', NULL, 1, geography::STGeomFromText('POINT (151.2093 -33.8688)', 4326), '2003-03-22 00:00:00', 'AUSYD');


				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_Address1, GB_WebAddress, GB_GC, GB_BranchName, GB_OH_OrgProxy, GB_IsActive, GB_GeoLocation, GB_SystemLastEditTimeUtc) VALUES
						('{branchPk2}', 'TK1', 'Test Address 1', 'www.test.au', '{compPk2}', 'TK1Name', '{okPh3}', 0, geography::STGeomFromText('POINT (0 0)', 4326), '2023-03-23 00:00:00');
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
				"{\"RegistrationNumber\":\"QwAxAEgAMQA=\"," +
				"\"RegistrationNumberType\":\"C1C\"," +
				$"\"OrgUniqueId\":\"{okPh1}\"," +
				$"\"OrgAddressUniqueId\":\"{okPa1}\"," +
				"\"CountryCode\":\"SY\"," +
				"\"SystemLastEditTimeUtc\":\"2003-03-22T00:00:00\"}",
				transaction1.AdditionalRefs);

			AssertEquals("AdditionalRefs should be as expected",
				"{\"RegistrationNumber\":\"QwAyAEgAMgA=\"," +
				"\"RegistrationNumberType\":\"C1C\"," +
				$"\"OrgUniqueId\":\"{okPh2}\"," +
				$"\"OrgAddressUniqueId\":\"{okPa2}\"," +
				"\"CountryCode\":\"SY\"," +
				"\"SystemLastEditTimeUtc\":\"2023-03-22T00:00:00\"}",
				transaction2.AdditionalRefs);

			AssertEquals("AdditionalRefs should be as expected",
				"{\"RegistrationNumber\":\"+oEXUzKQ41NGVQ==\"," +
				"\"RegistrationNumberType\":\"C1C\"," +
				$"\"OrgUniqueId\":\"{okPh3}\"," +
				$"\"OrgAddressUniqueId\":\"{okPa3}\"," +
				"\"CountryCode\":\"ZH\"," +
				"\"SystemLastEditTimeUtc\":\"2023-03-22T00:00:00\"}",
				transaction3.AdditionalRefs);

			AssertEquals("should only contain OrgCusCode with OK_OH in GlbBranch or GlbCompany", transactions.Any(t => t.Reference5 == okPk4), false);
		}
	}
}

