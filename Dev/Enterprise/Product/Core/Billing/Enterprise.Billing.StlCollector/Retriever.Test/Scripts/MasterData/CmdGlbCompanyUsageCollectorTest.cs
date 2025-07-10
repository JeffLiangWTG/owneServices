using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.MasterData;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.MasterData
{
	[TestedType(typeof(CmdGlbCompanyUsageCollector))]
	class CmdGlbCompanyUsageCollectorTest : RefStlScriptWithDefaultsTest
	{
		readonly string companyPk1 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string companyPk2 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string companyPk3 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string orgProxyPk1 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string orgProxyPk2 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string orgProxyPk3 = Guid.NewGuid().ToString().ToUpperInvariant();

		protected override bool IsMandatoryForMilestones => false;
		protected override void PrepareTestData()
		{
			var sqlQuery = $@"
				INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES
						('{orgProxyPk1}', 'SYD100'),
						('{orgProxyPk2}', 'TOKYOJ'),
						('{orgProxyPk3}', 'CHEYOJ');


				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode, GC_OH_OrgProxy, GC_GeoLocation, GC_SystemLastEditTimeUtc) VALUES
					('{companyPk1}', 'SD1', 'Test SYD Company', 'AUD', 'AU', '{orgProxyPk1}', geography::STGeomFromText('POINT (151.2093 -33.8688)', 4326), '2003-03-22 00:00:00');


				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode, GC_OH_OrgProxy, GC_GeoLocation, GC_SystemLastEditTimeUtc, GC_IsActive, GC_WebAddress, GC_Address1, GC_City, GC_State, GC_BusinessRegNo) VALUES
					('{companyPk2}', 'JTK', 'Test TYO Company', 'JPY', 'JP', '{orgProxyPk2}', geography::STGeomFromText('POINT (0 0)', 4326), '2023-03-22 00:00:00', 0, 'www.test.au', 'test address', 'SYD', 'NSW', 9090);

				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode, GC_OH_OrgProxy, GC_GeoLocation, GC_SystemLastEditTimeUtc, GC_IsActive, GC_WebAddress, GC_Address1, GC_City, GC_State, GC_BusinessRegNo) VALUES
					('{companyPk3}', 'CHE', N'臺灣港務 - 基隆港務分公司', 'JPY', 'JP', '{orgProxyPk3}', geography::STGeomFromText('POINT (0 0)', 4326), '2023-03-22 00:00:00', 0, 'www.test.au', 'test address', 'SYD', 'NSW', 9090);
";
			TestConnection.ExecuteNonQuery(sqlQuery);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2023, 3);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			var transaction1 = transactions.Single(t => t.Reference5 == companyPk1);
			var transaction2 = transactions.Single(t => t.Reference5 == companyPk2);
			var transaction3 = transactions.Single(t => t.Reference5 == companyPk3);

			AssertEquals("AdditionalRefs should be as expected",
				"{\"CompanyCode\":\"SD1\"," +
				$"\"OrgUniqueId\":\"{orgProxyPk1}\"," +
				"\"CountryCode\":\"AU\"," +
				"\"CompanyName\":\"VABlAHMAdAAgAFMAWQBEACAAQwBvAG0AcABhAG4AeQA=\"," +
				"\"GeoLocation\":\"POINT (151.2093 -33.8688)\"," +
				"\"SystemLastEditTimeUtc\":\"2003-03-22T00:00:00\"}",
				transaction1.AdditionalRefs);

			AssertEquals("AdditionalRefs should be as expected",
				"{\"CompanyCode\":\"JTK\"," +
				"\"BusinessRegistrationNumber\":\"OQAwADkAMAA=\"," +
				"\"Address1\":\"dABlAHMAdAAgAGEAZABkAHIAZQBzAHMA\"," +
				"\"City\":\"UwBZAEQA\"," +
				"\"State\":\"TgBTAFcA\"," +
				$"\"OrgUniqueId\":\"{orgProxyPk2}\"," +
				"\"CountryCode\":\"JP\"," +
				"\"IsActive\":false," +
				"\"CompanyName\":\"VABlAHMAdAAgAFQAWQBPACAAQwBvAG0AcABhAG4AeQA=\"," +
				"\"WebAddress\":\"www.test.au\"," +
				"\"SystemLastEditTimeUtc\":\"2023-03-22T00:00:00\"}",
				transaction2.AdditionalRefs);

			AssertEquals("AdditionalRefs should be as expected",
				"{\"CompanyCode\":\"CHE\"," +
				"\"BusinessRegistrationNumber\":\"OQAwADkAMAA=\"," +
				"\"Address1\":\"dABlAHMAdAAgAGEAZABkAHIAZQBzAHMA\"," +
				"\"City\":\"UwBZAEQA\"," +
				"\"State\":\"TgBTAFcA\"," +
				$"\"OrgUniqueId\":\"{orgProxyPk3}\"," +
				"\"CountryCode\":\"JP\"," +
				"\"IsActive\":false," +
				"\"CompanyName\":\"+oFjcC9u2VIgAC0AIAD6V4aWL27ZUgZSbFH4Uw==\"," +
				"\"WebAddress\":\"www.test.au\"," +
				"\"SystemLastEditTimeUtc\":\"2023-03-22T00:00:00\"}",
				transaction3.AdditionalRefs);
		}
	}
}

