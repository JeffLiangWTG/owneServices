using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.MasterData;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.MasterData
{
	[TestedType(typeof(CmdGlbBranchUsageCollector))]
	class CmdGlbBranchUsageCollectorTest : RefStlScriptWithDefaultsTest
	{
		readonly string branchPk1 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string branchPk2 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string branchPk3 = Guid.NewGuid().ToString().ToUpperInvariant();

		readonly string compPk1 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string compPk2 = Guid.NewGuid().ToString().ToUpperInvariant();
		readonly string compPk3 = Guid.NewGuid().ToString().ToUpperInvariant();

		protected override bool IsMandatoryForMilestones => false;
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2023, 3);
		protected override void PrepareTestData()
		{
			var sqlQuery = $@"
				DECLARE @SYDOrgProxyPK UNIQUEIDENTIFIER = NEWID();
				DECLARE @TYOOrgProxyPK UNIQUEIDENTIFIER = NEWID();
				DECLARE @CHEOrgProxyPK UNIQUEIDENTIFIER = NEWID();

				INSERT dbo.OrgHeader (OH_PK, OH_Code) VALUES
						(@SYDOrgProxyPK, 'SYD100'),
						(@TYOOrgProxyPK, 'TOKYOJ'),
						(@CHEOrgProxyPK, 'CHEYOJ');

				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode, GC_OH_OrgProxy) VALUES
						('{compPk1}', 'SD1', 'Test SYD Company', 'AUD', 'AU', @SYDOrgProxyPK),
						('{compPk2}', 'JTK', 'Test TYO Company', 'JPY', 'JP', @TYOOrgProxyPK),
						('{compPk3}', 'CHE', 'Test CHE Company', 'JPY', 'JP', @CHEOrgProxyPK);



				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_BranchName, GB_OH_OrgProxy, GB_IsActive, GB_GeoLocation, GB_SystemLastEditTimeUtc, GB_RL_NKHomePort) VALUES
						('{branchPk1}', 'SD1', '{compPk1}', 'UwBEADEATgBhAG0AZQA=', NULL, 1, geography::STGeomFromText('POINT (151.2093 -33.8688)', 4326), '2003-03-22 00:00:00', 'AUSYD');


				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_Address1, GB_WebAddress, GB_GC, GB_BranchName, GB_OH_OrgProxy, GB_IsActive, GB_GeoLocation, GB_SystemLastEditTimeUtc) VALUES
						('{branchPk2}', 'TK1', 'Test Address 1', 'www.test.au', '{compPk2}', 'TK1Name', NULL, 0, geography::STGeomFromText('POINT (0 0)', 4326), '2023-03-23 00:00:00');

				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_Address1, GB_WebAddress, GB_GC, GB_BranchName, GB_OH_OrgProxy, GB_IsActive, GB_GeoLocation, GB_SystemLastEditTimeUtc) VALUES
						('{branchPk3}', 'CH1', N'TW 2 臺北進口商/出口商公司b', 'www.test.au', '{compPk3}', 'CH1Name', NULL, 0, geography::STGeomFromText('POINT (0 0)', 4326), '2023-03-23 00:00:00');

";

			TestConnection.ExecuteNonQuery(sqlQuery);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			var transaction1 = transactions.Single(t => t.Reference5 == branchPk1);
			var transaction2 = transactions.Single(t => t.Reference5 == branchPk2);
			var transaction3 = transactions.Single(t => t.Reference5 == branchPk3);

			AssertEquals("AddtionalRefs should be as expected",
				"{\"BranchCode\":\"SD1\"," +
				"\"BranchName\":\"VQB3AEIARQBBAEQARQBBAFQAZwBCAGgAQQBHADAAQQBaAFEAQQA9AA==\"," +
				$"\"CompanyUniqueId\":\"{compPk1}\"," +
				"\"PortCode\":\"AUSYD\"," +
				"\"GeoLocation\":\"POINT (151.2093 -33.8688)\"," +
				"\"SystemLastEditTimeUtc\":\"2003-03-22T00:00:00\"}",
				transaction1.AdditionalRefs);

			AssertEquals("AddtionalRefs should be as expected",
				"{\"BranchCode\":\"TK1\"," +
				"\"BranchName\":\"VABLADEATgBhAG0AZQA=\"," +
				"\"Address1\":\"VABlAHMAdAAgAEEAZABkAHIAZQBzAHMAIAAxAA==\"," +
				"\"WebAddress\":\"www.test.au\"," +
				$"\"CompanyUniqueId\":\"{compPk2}\"," +
				"\"IsActive\":false," +
				"\"SystemLastEditTimeUtc\":\"2023-03-23T00:00:00\"}",
				transaction2.AdditionalRefs);

			AssertEquals("AddtionalRefs should be as expected",
				"{\"BranchCode\":\"CH1\"," +
				"\"BranchName\":\"QwBIADEATgBhAG0AZQA=\"," +
				"\"Address1\":\"VABXACAAMgAgAPqBF1MykONTRlUvAPpR41NGVWxR+FNiAA==\"," +
				"\"WebAddress\":\"www.test.au\"," +
				$"\"CompanyUniqueId\":\"{compPk3}\"," +
				"\"IsActive\":false," +
				"\"SystemLastEditTimeUtc\":\"2023-03-23T00:00:00\"}",
				transaction3.AdditionalRefs);
		}
	}
}

