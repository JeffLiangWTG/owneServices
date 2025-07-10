using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Common;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Common.Testing
{
	[TestedType(typeof(Report_ClientRatesSummary))]
	class Report_ClientRatesSummaryTest : DbCreateScriptTest
	{
		public void TestModeAll()
		{
			PrepareTestData();

			AssertEquals(97, GetRowsModeAll(includeGlobal: 'N').Count);
			AssertEquals(194, GetRowsModeAll(includeGlobal: 'Y').Count);
		}

		DataRowCollection GetRowsModeAll(char includeGlobal)
		{
			var reportSql = $"SELECT 1 FROM Report_ClientRatesSummary('{companyPK}', '', '{DateTime.Now: yyyy-MM-dd hh:mm:ss}', 'Y', 'ALL', '{branchPK}', '{includeGlobal}')";
			var data = DataUtils.GetDataTableFromQuery(TestConnection, reportSql);

			return data.Rows;
		}

		public void TestModeAirFreight()
		{
			PrepareTestData();
			AssertReportClientRatesSummary
			(
				filterOption: "AIR",
				expectedResult: new[]
				{
					"Forwarding Air Freight ULD",
					"Forwarding Air Freight LSE",
					"Forwarding Origin Air Freight",
					"Forwarding Origin Air Freight ULD",
					"Forwarding Origin Air Freight LSE",
					"Forwarding Destination Air Freight",
					"Forwarding Destination Air Freight ULD",
					"Forwarding Destination Air Freight LSE",
					"CFS Packing Air Freight",
					"CFS Packing Air Freight ULD",
					"CFS Packing Air Freight LSE",
					"CFS Unpacking Air Freight",
					"CFS Unpacking Air Freight ULD",
					"CFS Unpacking Air Freight LSE",
					"CFS Storage Air Freight"
				}
			);
		}

		void AssertReportClientRatesSummary(string filterOption, string[] expectedResult)
		{
			var localExpectedResult = GetExpectedResults(expectedResult, "Local", chargeCodePK);
			AssertReportClientRatesSummary(filterOption, 'N', localExpectedResult);

			var globalExpectedResult = GetExpectedResults(expectedResult, "Global", chargeCodePK);
			AssertReportClientRatesSummary(filterOption, 'Y', localExpectedResult.Concat(globalExpectedResult).ToArray());
		}

		IEnumerable<string> GetExpectedResults(IEnumerable<string> expectedResult, string localOrGlobalDescription, Guid chargePK)
			=> expectedResult.Select(result => $"{result} ({localOrGlobalDescription})-{localOrGlobalDescription}-{chargePK}");

		public void TestModeAirFreightLSE()
		{
			PrepareTestData();
			AssertReportClientRatesSummary
			(
				filterOption: "LSE",
				expectedResult: new[]
				{
					"Forwarding Air Freight LSE",
					"Forwarding Origin Air Freight LSE",
					"Forwarding Destination Air Freight LSE",
					"CFS Packing Air Freight LSE",
					"CFS Unpacking Air Freight LSE"
				}
			);
		}

		public void TestModeAirFreightULD()
		{
			PrepareTestData();
			AssertReportClientRatesSummary
			(
				filterOption: "ULD",
				expectedResult: new[]
				{
					"Forwarding Air Freight ULD",
					"Forwarding Origin Air Freight ULD",
					"Forwarding Destination Air Freight ULD",
					"CFS Packing Air Freight ULD",
					"CFS Unpacking Air Freight ULD"
				}
			);
		}

		public void TestModeSeaFreight()
		{
			PrepareTestData();
			AssertReportClientRatesSummary
			(
				filterOption: "SEA",
				expectedResult: new[]
				{
					"Forwarding Sea Freight FCL",
					"Forwarding Sea Freight LCL",
					"Forwarding Origin Sea Freight",
					"Forwarding Origin Sea Freight FCL",
					"Forwarding Origin Sea Freight LCL",
					"Forwarding Destination Sea Freight",
					"Forwarding Destination Sea Freight FCL",
					"Forwarding Destination Sea Freight LCL",
					"CFS Packing Sea Freight",
					"CFS Packing Sea Freight FCL",
					"CFS Packing Sea Freight LCL",
					"CFS Unpacking Sea Freight",
					"CFS Unpacking Sea Freight FCL",
					"CFS Unpacking Sea Freight LCL",
					"CFS Storage Sea Freight"
				}
			);
		}

		public void TestModeSeaFreightLCL()
		{
			PrepareTestData();
			AssertReportClientRatesSummary
			(
				filterOption: "LCL",
				expectedResult: new[]
				{
					"Forwarding Sea Freight LCL",
					"Forwarding Origin Sea Freight LCL",
					"Forwarding Destination Sea Freight LCL",
					"CFS Packing Sea Freight LCL",
					"CFS Unpacking Sea Freight LCL",
				}
			);
		}

		public void TestModeSeaFreightFCL()
		{
			PrepareTestData();
			AssertReportClientRatesSummary
			(
				filterOption: "FCL",
				expectedResult: new[]
				{
					"Forwarding Sea Freight FCL",
					"Forwarding Origin Sea Freight FCL",
					"Forwarding Destination Sea Freight FCL",
					"CFS Packing Sea Freight FCL",
					"CFS Unpacking Sea Freight FCL"
				}
			);
		}

		public void TestModeSeaShipping()
		{
			PrepareTestData();
			AssertReportClientRatesSummary
			(
				filterOption: "SHP",
				expectedResult: new[]
				{
					"Shipping Containerized Freight",
					"Shipping Non-Containerized Freight",
					"Shipping Origin FCL",
					"Shipping Origin LCL",
					"Shipping Origin All",
					"Shipping Destination FCL",
					"Shipping Destination LCL",
					"Shipping Destination All",
					"Shipping Export Container Detention",
					"Shipping Import Container Detention"
				}
			);
		}

		public void TestModeSeaShippingNonContainerised()
		{
			PrepareTestData();
			AssertReportClientRatesSummary
			(
				filterOption: "SNC",
				expectedResult: new[]
				{
					"Shipping Non-Containerized Freight",
					"Shipping Origin LCL",
					"Shipping Destination LCL"
				}
			);
		}

		public void TestModeSeaShippingContainerised()
		{
			PrepareTestData();
			AssertReportClientRatesSummary
			(
				filterOption: "SCO",
				expectedResult: new[]
				{
					"Shipping Containerized Freight",
					"Shipping Origin FCL",
					"Shipping Destination FCL",
					"Shipping Export Container Detention",
					"Shipping Import Container Detention"
				}
			);
		}

		public void TestModeRailFreight()
		{
			PrepareTestData();
			AssertReportClientRatesSummary
			(
				filterOption: "RAI",
				expectedResult: new[]
				{
					"Forwarding Rail Freight FCL",
					"Forwarding Rail Freight LWL",
					"Forwarding Rail Freight FWL",
					"Forwarding Origin Rail Freight",
					"Forwarding Origin Rail Freight FCL",
					"Forwarding Origin Rail Freight LCL",
					"Forwarding Origin Rail Freight FWL",
					"Forwarding Destination Rail Freight",
					"Forwarding Destination Rail Freight FCL",
					"Forwarding Destination Rail Freight LCL",
					"Forwarding Destination Rail Freight FWL",
					"CFS Packing Rail Freight",
					"CFS Packing Rail Freight FCL",
					"CFS Packing Rail Freight LCL",
					"CFS Packing Rail Freight FWL",
					"CFS Unpacking Rail Freight",
					"CFS Unpacking Rail Freight FCL",
					"CFS Unpacking Rail Freight LCL",
					"CFS Unpacking Rail Freight FWL",
					"CFS Storage Rail Freight"
				}
			);
		}

		public void TestModeRailFreightFCL()
		{
			PrepareTestData();
			AssertReportClientRatesSummary
			(
				filterOption: "FRA",
				expectedResult: new[]
				{
					"Forwarding Rail Freight FCL",
					"Forwarding Origin Rail Freight FCL",
					"Forwarding Destination Rail Freight FCL",
					"CFS Packing Rail Freight FCL",
					"CFS Unpacking Rail Freight FCL"
				}
			);
		}

		public void TestModeRailFreightLCL()
		{
			PrepareTestData();
			AssertReportClientRatesSummary
			(
				filterOption: "LWL",
				expectedResult: new[]
				{
					"Forwarding Rail Freight LWL",
					"Forwarding Origin Rail Freight LCL",
					"Forwarding Destination Rail Freight LCL",
					"CFS Packing Rail Freight LCL",
					"CFS Unpacking Rail Freight LCL"
				}
			);
		}

		public void TestModeRailFreightFWL()
		{
			PrepareTestData();
			AssertReportClientRatesSummary
			(
				filterOption: "FWL",
				expectedResult: new[]
				{
					"Forwarding Rail Freight FWL",
					"Forwarding Origin Rail Freight FWL",
					"Forwarding Destination Rail Freight FWL",
					"CFS Packing Rail Freight FWL",
					"CFS Unpacking Rail Freight FWL"
				}
			);
		}

		public void TestModeRoadFreight()
		{
			PrepareTestData();
			AssertReportClientRatesSummary
			(
				filterOption: "ROA",
				expectedResult: new[]
				{
					"Forwarding Road Freight FCL",
					"Forwarding Road Freight LTL",
					"Forwarding Road Freight FTL",
					"Forwarding Origin Road Freight",
					"Forwarding Origin Road Freight FCL",
					"Forwarding Origin Road Freight LCL",
					"Forwarding Origin Road Freight FTL",
					"Forwarding Destination Road Freight",
					"Forwarding Destination Road Freight FCL",
					"Forwarding Destination Road Freight LCL",
					"Forwarding Destination Road Freight FTL",
					"CFS Packing Road Freight",
					"CFS Packing Road Freight FCL",
					"CFS Packing Road Freight LCL",
					"CFS Packing Road Freight FTL",
					"CFS Unpacking Road Freight",
					"CFS Unpacking Road Freight FCL",
					"CFS Unpacking Road Freight LCL",
					"CFS Unpacking Road Freight FTL",
					"CFS Storage Road Freight",
					"Transport Road Freight",
					"Transport Road Freight FCL",
					"Transport Road Freight LCL"
				}
			);
		}

		public void TestModeRoadFreightFCL()
		{
			PrepareTestData();
			AssertReportClientRatesSummary
			(
				filterOption: "FRO",
				expectedResult: new[]
				{
					"Forwarding Road Freight FCL",
					"Forwarding Origin Road Freight FCL",
					"Forwarding Destination Road Freight FCL",
					"CFS Packing Road Freight FCL",
					"CFS Unpacking Road Freight FCL",
					"Transport Road Freight FCL"
				}
			);
		}

		public void TestModeRoadFreightLCL()
		{
			PrepareTestData();
			AssertReportClientRatesSummary
			(
				filterOption: "LTL",
				expectedResult: new[]
				{
					"Forwarding Road Freight LTL",
					"Forwarding Origin Road Freight LCL",
					"Forwarding Destination Road Freight LCL",
					"CFS Packing Road Freight LCL",
					"CFS Unpacking Road Freight LCL",
					"Transport Road Freight LCL"
				}
			);
		}

		public void TestModeRoadFreightFTL()
		{
			PrepareTestData();
			AssertReportClientRatesSummary
			(
				filterOption: "FTL",
				expectedResult: new[]
				{
					"Forwarding Road Freight FTL",
					"Forwarding Origin Road Freight FTL",
					"Forwarding Destination Road Freight FTL",
					"CFS Packing Road Freight FTL",
					"CFS Unpacking Road Freight FTL"
				}
			);
		}

		public void TestModePost()
		{
			PrepareTestData();
			AssertReportClientRatesSummary
			(
				filterOption: "MAI",
				expectedResult: new[]
				{
					"Forwarding Origin Post",
					"Forwarding Destination Post",
				}
			);
		}

		public void TestModeWarehouse()
		{
			PrepareTestData();
			AssertReportClientRatesSummary
			(
				filterOption: "WHS",
				expectedResult: new[]
				{
					"Warehouse"
				}
			);
		}

		#region TestAllRateTypesAreCovered

		public void TestAllRateTypesAreCovered_NotIncludeGlobal() => TestAllRateTypesAreCovered(includeGlobal: 'N', expectedCount: 97);

		public void TestAllRateTypesAreCovered_IncludeGlobal() => TestAllRateTypesAreCovered(includeGlobal: 'Y', expectedCount: 194);

		void TestAllRateTypesAreCovered(char includeGlobal, int expectedCount)
		{
			var typesSet = new HashSet<string>();

			PrepareTestData();
			AddRateTypesFromFilterResult(typesSet, "ALL", includeGlobal);
			AddRateTypesFromFilterResult(typesSet, "AIR", includeGlobal);
			AddRateTypesFromFilterResult(typesSet, "LSE", includeGlobal);
			AddRateTypesFromFilterResult(typesSet, "ULD", includeGlobal);
			AddRateTypesFromFilterResult(typesSet, "SEA", includeGlobal);
			AddRateTypesFromFilterResult(typesSet, "LCL", includeGlobal);
			AddRateTypesFromFilterResult(typesSet, "FCL", includeGlobal);
			AddRateTypesFromFilterResult(typesSet, "SHP", includeGlobal);
			AddRateTypesFromFilterResult(typesSet, "SNC", includeGlobal);
			AddRateTypesFromFilterResult(typesSet, "SCO", includeGlobal);
			AddRateTypesFromFilterResult(typesSet, "RAI", includeGlobal);
			AddRateTypesFromFilterResult(typesSet, "FRA", includeGlobal);
			AddRateTypesFromFilterResult(typesSet, "LWL", includeGlobal);
			AddRateTypesFromFilterResult(typesSet, "FWL", includeGlobal);
			AddRateTypesFromFilterResult(typesSet, "ROA", includeGlobal);
			AddRateTypesFromFilterResult(typesSet, "FRO", includeGlobal);
			AddRateTypesFromFilterResult(typesSet, "LTL", includeGlobal);
			AddRateTypesFromFilterResult(typesSet, "FTL", includeGlobal);
			AddRateTypesFromFilterResult(typesSet, "MAI", includeGlobal);
			AddRateTypesFromFilterResult(typesSet, "WHS", includeGlobal);

			AssertEquals(expectedCount, typesSet.Count);
		}

		#endregion

		public void TestCFXHierarchy()
		{
			PrepareTestData();

			AddCFXEntry("", Guid.Empty, "IMP", "SEA", 3.00);
			AddCFXEntry("", Guid.Empty, "EXP", "SEA", 3.01);
			AddCFXEntry("", Guid.Empty, "IMP", "AIR", 3.02);
			AddCFXEntry("", Guid.Empty, "EXP", "AIR", 3.03);

			var sql = string.Format("SELECT * FROM Report_ClientRatesSummary('{0}', '', '{1}', 'Y', '{2}', '{3}', 'N')", companyPK, DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), "ALL", branchPK);
			var data = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			Assert(data.Rows.Count > 0);
			var cFXImpSeaResult = (from DataRow row in data.Rows select row["ClientSeaCFX"].ToString());
			var cFXExpSeaResult = (from DataRow row in data.Rows select row["ClientExportSeaCFX"].ToString());
			var cFXImpAirResult = (from DataRow row in data.Rows select row["ClientAirCFX"].ToString());
			var cFXExpAirResult = (from DataRow row in data.Rows select row["ClientExportAirCFX"].ToString());

			AssertEquals("3.00", cFXImpSeaResult.FirstOrDefault());
			AssertEquals("3.01", cFXExpSeaResult.FirstOrDefault());
			AssertEquals("3.02", cFXImpAirResult.FirstOrDefault());
			AssertEquals("3.03", cFXExpAirResult.FirstOrDefault());

			AddCFXEntry("GB", branchPK, "IMP", "SEA", 2.00);
			AddCFXEntry("GB", branchPK, "EXP", "SEA", 2.01);
			AddCFXEntry("GB", branchPK, "IMP", "AIR", 2.02);
			AddCFXEntry("GB", branchPK, "EXP", "AIR", 2.03);

			data = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			Assert(data.Rows.Count > 0);
			cFXImpSeaResult = (from DataRow row in data.Rows select row["ClientSeaCFX"].ToString());
			cFXExpSeaResult = (from DataRow row in data.Rows select row["ClientExportSeaCFX"].ToString());
			cFXImpAirResult = (from DataRow row in data.Rows select row["ClientAirCFX"].ToString());
			cFXExpAirResult = (from DataRow row in data.Rows select row["ClientExportAirCFX"].ToString());

			AssertEquals("2.00", cFXImpSeaResult.FirstOrDefault());
			AssertEquals("2.01", cFXExpSeaResult.FirstOrDefault());
			AssertEquals("2.02", cFXImpAirResult.FirstOrDefault());
			AssertEquals("2.03", cFXExpAirResult.FirstOrDefault());

			AddCFXEntry("OH", orgPK, "IMP", "SEA", 1.00);
			AddCFXEntry("OH", orgPK, "EXP", "SEA", 1.01);
			AddCFXEntry("OH", orgPK, "IMP", "AIR", 1.02);
			AddCFXEntry("OH", orgPK, "EXP", "AIR", 1.03);

			data = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			Assert(data.Rows.Count > 0);
			cFXImpSeaResult = (from DataRow row in data.Rows select row["ClientSeaCFX"].ToString());
			cFXExpSeaResult = (from DataRow row in data.Rows select row["ClientExportSeaCFX"].ToString());
			cFXImpAirResult = (from DataRow row in data.Rows select row["ClientAirCFX"].ToString());
			cFXExpAirResult = (from DataRow row in data.Rows select row["ClientExportAirCFX"].ToString());

			AssertEquals("1.00", cFXImpSeaResult.FirstOrDefault());
			AssertEquals("1.01", cFXExpSeaResult.FirstOrDefault());
			AssertEquals("1.02", cFXImpAirResult.FirstOrDefault());
			AssertEquals("1.03", cFXExpAirResult.FirstOrDefault());
		}

		public void TestCFXWhenMissingCombinationNotExistsStillReturnAvailable()
		{
			PrepareTestData();

			AddCFXEntry("", Guid.Empty, "IMP", "SEA", 3.00);
			AddCFXEntry("", Guid.Empty, "EXP", "SEA", 3.01);

			var sql = string.Format("SELECT * FROM Report_ClientRatesSummary('{0}', '', '{1}', 'Y', '{2}', '{3}', 'N')", companyPK, DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), "ALL", branchPK);
			var data = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			Assert(data.Rows.Count > 0);
			var cFXImpSeaResult = (from DataRow row in data.Rows select row["ClientSeaCFX"].ToString());
			var cFXExpSeaResult = (from DataRow row in data.Rows select row["ClientExportSeaCFX"].ToString());
			var cFXImpAirResult = (from DataRow row in data.Rows select row["ClientAirCFX"].ToString());
			var cFXExpAirResult = (from DataRow row in data.Rows select row["ClientExportAirCFX"].ToString());

			AssertEquals("3.00", cFXImpSeaResult.FirstOrDefault());
			AssertEquals("3.01", cFXExpSeaResult.FirstOrDefault());
			AssertEquals("0.00", cFXImpAirResult.FirstOrDefault());
			AssertEquals("0.00", cFXExpAirResult.FirstOrDefault());
		}

		void AssertReportClientRatesSummary(string filterOption, char includeGlobal, IEnumerable<string> expectedResult)
		{
			var sql = string.Format("SELECT * FROM Report_ClientRatesSummary('{0}', '', '{1}', 'Y', '{2}', '{3}', '{4}')", companyPK, DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), filterOption, branchPK, includeGlobal);
			var data = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			var actualResult = (from DataRow row in data.Rows select $"{row["LineDesc"]}-{row["Published"]}-{row["LocalChargePK"]}");

			AssertContainsExactElementsInAnyOrder(expectedResult, actualResult);
		}

		void AddRateTypesFromFilterResult(HashSet<string> typesSet, string filterOption, char includeGlobal)
		{
			var reportSql = string.Format("SELECT * FROM Report_ClientRatesSummary('{0}', '', '{1}', 'Y', '{2}', '{3}', '{4}')", companyPK, DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"), filterOption, branchPK, includeGlobal);
			var data = DataUtils.GetDataTableFromQuery(TestConnection, reportSql);

			foreach (DataRow row in data.Rows)
			{
				typesSet.Add(row["LineDesc"].ToString());
			}
		}

		void PrepareTestData()
		{
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES ('{0}', 'DDD', 'AU company', 'AU', 'AUD')", companyPK));
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName) VALUES ('{0}', 'TESTORG', 'Test Organisation')", orgPK));
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.RatingHeader (TH_PK, TH_OH, TH_GC, TH_RateType, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser, TH_SystemCreateTimeUtc, TH_SystemCreateUser) VALUES ('{0}', '{1}', '{2}', 'SAL', GetUtcDate(), '~BP', GetUtcDate(), '~BP')", ratingHeaderPK, orgPK, companyPK));
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.AccChargeCode (AC_PK, AC_GC, AC_ChargeGroup) VALUES ('{0}', '{1}', 'FRT')", chargeCodePK, companyPK));
			TestConnection.ExecuteNonQuery(string.Format("INSERT INTO dbo.GlbBranch (GB_PK, GB_IsValid, GB_Code, GB_GC) VALUES ('{0}', 1, 'BBB', '{1}')", branchPK, companyPK));

			helper = new TestDbHelper(TestConnection);
			helper.Insert("RatingHeader", new { TH_PK = globalRatingHeaderPK, TH_OH = orgPK, TH_RateType = "SAL", });
			TestConnection.Command($"INSERT INTO dbo.AccChargeCode (AC_PK, AC_ChargeGroup) VALUES ('{globalChargeCodePK}', 'FRT')").ExecuteNonQuery();

			var rateEntryInfos = new (Guid ratingHeaderPK, Guid chargeCodePK, string description)[]
			{
				(ratingHeaderPK, chargeCodePK, "Local"),
				(globalRatingHeaderPK, globalChargeCodePK, "Global")
			};

			foreach (var rateEntryInfo in rateEntryInfos)
			{
				AddRateEntry("SEA", "SCO", $"Shipping Containerized Freight ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("LCL", "SNC", $"Shipping Non-Containerized Freight ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("FCL", "SOR", $"Shipping Origin FCL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("LCL", "SOR", $"Shipping Origin LCL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("ALL", "SOR", $"Shipping Origin All ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("FCL", "SDE", $"Shipping Destination FCL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("LCL", "SDE", $"Shipping Destination LCL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("ALL", "SDE", $"Shipping Destination All ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("SEA", "SED", $"Shipping Export Container Detention ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("SEA", "SID", $"Shipping Import Container Detention ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);

				AddRateEntry("ULD", "AIR", $"Forwarding Air Freight ULD ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("LSE", "AIR", $"Forwarding Air Freight LSE ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("SEA", "FCL", $"Forwarding Sea Freight FCL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("ROA", "FCL", $"Forwarding Road Freight FCL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("RAI", "FCL", $"Forwarding Rail Freight FCL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("LCL", "LCL", $"Forwarding Sea Freight LCL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("LRO", "LCL", $"Forwarding Road Freight LTL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("FTL", "LCL", $"Forwarding Road Freight FTL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("LRA", "LCL", $"Forwarding Rail Freight LWL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("FWL", "LCL", $"Forwarding Rail Freight FWL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);

				AddRateEntry("ALL", "ORG", $"Forwarding Origin All ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("AIR", "ORG", $"Forwarding Origin Air Freight ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("ULD", "ORG", $"Forwarding Origin Air Freight ULD ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("LSE", "ORG", $"Forwarding Origin Air Freight LSE ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("SEA", "ORG", $"Forwarding Origin Sea Freight ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("FCL", "ORG", $"Forwarding Origin Sea Freight FCL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("LCL", "ORG", $"Forwarding Origin Sea Freight LCL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("ROA", "ORG", $"Forwarding Origin Road Freight ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("FRO", "ORG", $"Forwarding Origin Road Freight FCL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("LRO", "ORG", $"Forwarding Origin Road Freight LCL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("FTL", "ORG", $"Forwarding Origin Road Freight FTL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("RAI", "ORG", $"Forwarding Origin Rail Freight ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("FRA", "ORG", $"Forwarding Origin Rail Freight FCL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("LRA", "ORG", $"Forwarding Origin Rail Freight LCL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("FWL", "ORG", $"Forwarding Origin Rail Freight FWL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("MAI", "ORG", $"Forwarding Origin Post ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);

				AddRateEntry("ALL", "DST", $"Forwarding Destination All ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("AIR", "DST", $"Forwarding Destination Air Freight ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("ULD", "DST", $"Forwarding Destination Air Freight ULD ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("LSE", "DST", $"Forwarding Destination Air Freight LSE ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("SEA", "DST", $"Forwarding Destination Sea Freight ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("FCL", "DST", $"Forwarding Destination Sea Freight FCL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("LCL", "DST", $"Forwarding Destination Sea Freight LCL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("ROA", "DST", $"Forwarding Destination Road Freight ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("FRO", "DST", $"Forwarding Destination Road Freight FCL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("LRO", "DST", $"Forwarding Destination Road Freight LCL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("FTL", "DST", $"Forwarding Destination Road Freight FTL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("RAI", "DST", $"Forwarding Destination Rail Freight ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("FRA", "DST", $"Forwarding Destination Rail Freight FCL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("LRA", "DST", $"Forwarding Destination Rail Freight LCL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("FWL", "DST", $"Forwarding Destination Rail Freight FWL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("MAI", "DST", $"Forwarding Destination Post ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);

				AddRateEntry("ALL", "PAC", $"CFS Packing All ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("AIR", "PAC", $"CFS Packing Air Freight ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("ULD", "PAC", $"CFS Packing Air Freight ULD ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("LSE", "PAC", $"CFS Packing Air Freight LSE ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("SEA", "PAC", $"CFS Packing Sea Freight ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("FCL", "PAC", $"CFS Packing Sea Freight FCL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("LCL", "PAC", $"CFS Packing Sea Freight LCL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("ROA", "PAC", $"CFS Packing Road Freight ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("FRO", "PAC", $"CFS Packing Road Freight FCL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("LRO", "PAC", $"CFS Packing Road Freight LCL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("FTL", "PAC", $"CFS Packing Road Freight FTL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("RAI", "PAC", $"CFS Packing Rail Freight ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("FRA", "PAC", $"CFS Packing Rail Freight FCL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("LRA", "PAC", $"CFS Packing Rail Freight LCL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("FWL", "PAC", $"CFS Packing Rail Freight FWL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);

				AddRateEntry("ALL", "UNP", $"CFS Unpacking All ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("AIR", "UNP", $"CFS Unpacking Air Freight ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("ULD", "UNP", $"CFS Unpacking Air Freight ULD ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("LSE", "UNP", $"CFS Unpacking Air Freight LSE ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("SEA", "UNP", $"CFS Unpacking Sea Freight ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("FCL", "UNP", $"CFS Unpacking Sea Freight FCL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("LCL", "UNP", $"CFS Unpacking Sea Freight LCL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("ROA", "UNP", $"CFS Unpacking Road Freight ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("FRO", "UNP", $"CFS Unpacking Road Freight FCL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("LRO", "UNP", $"CFS Unpacking Road Freight LCL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("FTL", "UNP", $"CFS Unpacking Road Freight FTL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("RAI", "UNP", $"CFS Unpacking Rail Freight ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("FRA", "UNP", $"CFS Unpacking Rail Freight FCL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("LRA", "UNP", $"CFS Unpacking Rail Freight LCL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("FWL", "UNP", $"CFS Unpacking Rail Freight FWL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);

				AddRateEntry("ALL", "CST", $"CFS Storage All ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("AIR", "CST", $"CFS Storage Air Freight ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("SEA", "CST", $"CFS Storage Sea Freight ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("ROA", "CST", $"CFS Storage Road Freight ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("RAI", "CST", $"CFS Storage Rail Freight ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);

				AddRateEntry("ALL", "WHS", $"Warehouse ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);

				AddRateEntry("ALL", "TBC", $"Transport All ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("ROA", "TBC", $"Transport Road Freight ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("FRO", "TBC", $"Transport Road Freight FCL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("LRO", "TBC", $"Transport Road Freight LCL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("FTL", "TBC", $"Transport Road Freight FTL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("RAI", "TBC", $"Transport Road Freight RAI ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("LRA", "TBC", $"Transport Road Freight LRA ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("FRA", "TBC", $"Transport Road Freight FRA ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
				AddRateEntry("FWL", "TBC", $"Transport Road Freight FWL ({rateEntryInfo.description})", rateEntryInfo.ratingHeaderPK, rateEntryInfo.chargeCodePK);
			}
		}

		TestDbHelper helper;

		void AddRateEntry(string transportMode, string freightMode, string description, Guid ratingHeaderPK, Guid chargeCodePK)
		{
			var entryPK = Guid.NewGuid();
			var entryQuery = string.Format("INSERT INTO dbo.RateEntry (TI_PK, TI_TH, TI_Mode, TI_RateCategory, TI_RateStartDate, TI_GC_Publisher, TI_SystemLastEditTimeUtc, TI_SystemLastEditUser, TI_SystemCreateTimeUtc, TI_SystemCreateUser) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')",
				entryPK,
				ratingHeaderPK,
				transportMode,
				freightMode,
				DateTime.Today.AddMonths(-1).ToString("yyyy-MM-dd hh:mm:ss"),
				companyPK);
			TestConnection.ExecuteNonQuery(entryQuery);

			var linePK = Guid.NewGuid();
			var lineQuery = string.Format("INSERT INTO dbo.RateLines (TL_PK, TL_TI, TL_AC, TL_RateCalculator, TL_RateDesc, TL_SystemLastEditTimeUtc, TL_SystemLastEditUser, TL_SystemCreateTimeUtc, TL_SystemCreateUser, TL_RX_NKCurrency) VALUES ('{0}', '{1}', '{2}', 'FLT', '{3}', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'USD')",
				linePK,
				entryPK,
				chargeCodePK,
				description);
			TestConnection.ExecuteNonQuery(lineQuery);

			var itemPK = Guid.NewGuid();
			var itemQuery = string.Format("INSERT INTO dbo.RateLineItems (TM_PK, TM_TL, TM_Type, TM_Value, TM_SystemLastEditTimeUtc, TM_SystemLastEditUser, TM_SystemCreateTimeUtc, TM_SystemCreateUser) VALUES ('{0}', '{1}', 'BAS', 20, GetUtcDate(), '~BP', GetUtcDate(), '~BP')",
				itemPK,
				linePK);
			TestConnection.ExecuteNonQuery(itemQuery);
		}

		void AddCFXEntry(string parentTableCode, Guid parentID, string serviceDirection, string transportMode, double percentage)
		{
			var cfxConfigPK = Guid.NewGuid();
			var cfxConfigQuery = string.Format("INSERT INTO dbo.AccJobConfig (JCF_PK, JCF_ConfigType, JCF_GC, JCF_Ledger, JCF_ParentTableCode, JCF_ParentID, JCF_JobType, JCF_ServiceDirection, JCF_TransportMode, JCF_Percentage, JCF_Amount) " +
				"VALUES ('{0}', 'CFX', '{1}', 'AR', '{2}', {3}, 'ALL', '{4}', '{5}', '{6}', 0)",
				cfxConfigPK,
				companyPK,
				parentTableCode,
				(parentID == Guid.Empty) ? "null" : "'" + parentID.ToString() + "'",
				serviceDirection,
				transportMode,
				percentage);
			TestConnection.ExecuteNonQuery(cfxConfigQuery);
		}

		readonly Guid orgPK = Guid.NewGuid();
		readonly Guid ratingHeaderPK = Guid.NewGuid();
		readonly Guid globalRatingHeaderPK = Guid.NewGuid();
		readonly Guid chargeCodePK = Guid.NewGuid();
		readonly Guid globalChargeCodePK = Guid.NewGuid();
		readonly Guid companyPK = Guid.NewGuid();
		readonly Guid branchPK = Guid.NewGuid();
	}
}

