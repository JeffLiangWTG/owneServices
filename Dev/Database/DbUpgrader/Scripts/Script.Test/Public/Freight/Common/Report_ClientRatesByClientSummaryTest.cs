using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Common;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Common.Testing
{
	[TestedType(typeof(Report_ClientRatesByClientSummary))]
	class Report_ClientRatesByClientSummaryTest : DbCreateScriptTest
	{
		[TestDate(2015, 01, 01)]
		public void TestClientRatesGroupedByMode()
		{
			var supplier = helper.InsertOrgHeader("SUPPLIER", "Supplier");
			CreateRateEntryWithFlatRateLine(clientRate, "AIR", "LSE", supplier, "10");
			CreateRateEntryWithFlatRateLine(clientRate, "AIR", "ULD", supplier, "14");
			CreateRateEntryWithFlatRateLine(clientRate, "DST", "FCL", supplier, "55");
			CreateRateEntryWithFlatRateLine(clientRate, "DST", "LSE", supplier, "45");
			CreateRateEntryWithFlatRateLine(clientRate, "DST", "ULD", supplier, "85");

			var result = DataUtils.GetDataTableFromQuery(TestConnection, QueryString("ALL", "", Guid.Empty));

			AssertEquals("Expected three distinct categories", 5, result.Rows.Count);
			AssertEquals("AIRLSEUSLAX-AUSYD--SUPPLIER", result.Rows[0]["RateGroup"]);
			AssertEquals("AIRULDUSLAX-AUSYD--SUPPLIER", result.Rows[1]["RateGroup"]);
			AssertEquals("DSTFCLUSLAX-AUSYD--SUPPLIER", result.Rows[2]["RateGroup"]);
			AssertEquals("DSTLSEUSLAX-AUSYD--SUPPLIER", result.Rows[3]["RateGroup"]);
			AssertEquals("DSTULDUSLAX-AUSYD--SUPPLIER", result.Rows[4]["RateGroup"]);
		}

		#region Include Global

		[TestDate(2015, 01, 01)]
		public void TestIncludeGlobalTrue()
			=> TestIncludeGlobal
			(
				includeGlobal: true,
				expectedRows: new[]
				{
					$"AIRLSEUSLAX-AUSYD--SUPPLIER-Local-{chargeCode}",
					$"AIRULDUSLAX-AUSYD--SUPPLIER-Local-{chargeCode}",
					$"FCLSEAUSLAX-AUSYD--SUPPLIER-Local-{chargeCode}",
					$"DSTFCLUSLAX-AUSYD--SUPPLIER-Local-{chargeCode}",
					$"DSTLSEUSLAX-AUSYD--SUPPLIER-Local-{chargeCode}",
					$"DSTULDUSLAX-AUSYD--SUPPLIER-Local-{chargeCode}",
					$"ORGFCLUSLAX-AUSYD--SUPPLIER-Local-{chargeCode}",
					$"ORGLSEUSLAX-AUSYD--SUPPLIER-Local-{chargeCode}",
					$"ORGULDUSLAX-AUSYD--SUPPLIER-Local-{chargeCode}",
					$"AIRLSEUSLAX-AUSYD--SUPPLIER-Global-{chargeCode}",
					$"AIRULDUSLAX-AUSYD--SUPPLIER-Global-{chargeCode}",
					$"FCLSEAUSLAX-AUSYD--SUPPLIER-Global-{chargeCode}",
					$"DSTFCLUSLAX-AUSYD--SUPPLIER-Global-{chargeCode}",
					$"DSTLSEUSLAX-AUSYD--SUPPLIER-Global-{chargeCode}",
					$"DSTULDUSLAX-AUSYD--SUPPLIER-Global-{chargeCode}",
					$"ORGFCLUSLAX-AUSYD--SUPPLIER-Global-{chargeCode}",
					$"ORGLSEUSLAX-AUSYD--SUPPLIER-Global-{chargeCode}",
					$"ORGULDUSLAX-AUSYD--SUPPLIER-Global-{chargeCode}",
				}
			);

		[TestDate(2015, 01, 01)]
		public void TestIncludeGlobalFalse()
			=> TestIncludeGlobal
			(
				includeGlobal: false,
				expectedRows: new[]
				{
					$"AIRLSEUSLAX-AUSYD--SUPPLIER-Local-{chargeCode}",
					$"AIRULDUSLAX-AUSYD--SUPPLIER-Local-{chargeCode}",
					$"FCLSEAUSLAX-AUSYD--SUPPLIER-Local-{chargeCode}",
					$"DSTFCLUSLAX-AUSYD--SUPPLIER-Local-{chargeCode}",
					$"DSTLSEUSLAX-AUSYD--SUPPLIER-Local-{chargeCode}",
					$"DSTULDUSLAX-AUSYD--SUPPLIER-Local-{chargeCode}",
					$"ORGFCLUSLAX-AUSYD--SUPPLIER-Local-{chargeCode}",
					$"ORGLSEUSLAX-AUSYD--SUPPLIER-Local-{chargeCode}",
					$"ORGULDUSLAX-AUSYD--SUPPLIER-Local-{chargeCode}",
				}
			);

		void TestIncludeGlobal(bool includeGlobal, string[] expectedRows)
		{
			var supplier = helper.InsertOrgHeader("SUPPLIER", "Supplier");

			CreateRateEntryWithFlatRateLine(clientRate, "AIR", "LSE", supplier, "10");
			CreateRateEntryWithFlatRateLine(clientRate, "AIR", "ULD", supplier, "20");
			CreateRateEntryWithFlatRateLine(clientRate, "FCL", "SEA", supplier, "30");
			CreateRateEntryWithFlatRateLine(clientRate, "DST", "FCL", supplier, "40");
			CreateRateEntryWithFlatRateLine(clientRate, "DST", "LSE", supplier, "50");
			CreateRateEntryWithFlatRateLine(clientRate, "DST", "ULD", supplier, "60");
			CreateRateEntryWithFlatRateLine(clientRate, "ORG", "FCL", supplier, "70");
			CreateRateEntryWithFlatRateLine(clientRate, "ORG", "LSE", supplier, "80");
			CreateRateEntryWithFlatRateLine(clientRate, "ORG", "ULD", supplier, "90");

			CreateRateEntryWithFlatRateLine(globalClientRate, "AIR", "LSE", supplier, "100", isGlobal: true);
			CreateRateEntryWithFlatRateLine(globalClientRate, "AIR", "ULD", supplier, "200", isGlobal: true);
			CreateRateEntryWithFlatRateLine(globalClientRate, "FCL", "SEA", supplier, "300", isGlobal: true);
			CreateRateEntryWithFlatRateLine(globalClientRate, "DST", "FCL", supplier, "400", isGlobal: true);
			CreateRateEntryWithFlatRateLine(globalClientRate, "DST", "LSE", supplier, "500", isGlobal: true);
			CreateRateEntryWithFlatRateLine(globalClientRate, "DST", "ULD", supplier, "600", isGlobal: true);
			CreateRateEntryWithFlatRateLine(globalClientRate, "ORG", "FCL", supplier, "700", isGlobal: true);
			CreateRateEntryWithFlatRateLine(globalClientRate, "ORG", "LSE", supplier, "800", isGlobal: true);
			CreateRateEntryWithFlatRateLine(globalClientRate, "ORG", "ULD", supplier, "900", isGlobal: true);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, QueryString("ALL", "", Guid.Empty, includeGlobal));
			AssertContainsExactElementsInAnyOrder
			(
				expectedRows,
				result.Rows.Cast<DataRow>().Select(row => $"{row["RateGroup"]}-{row["Published"]}-{row["LocalChargePK"]}")
			);
		}

		#endregion

		[TestDate(2015, 01, 01)]
		public void TestClientRatesGroupedBySupplier()
		{
			var supplier1 = helper.InsertOrgHeader("SUP1", "Supplier 1");
			var supplier2 = helper.InsertOrgHeader("SUP2", "Supplier 2");
			CreateRateEntryWithFlatRateLine(clientRate, "AIR", "LSE", supplier1, "10");
			CreateRateEntryWithFlatRateLine(clientRate, "AIR", "LSE", supplier2, "14");

			var result = DataUtils.GetDataTableFromQuery(TestConnection, QueryString("AIR", "LSE", Guid.Empty));

			AssertEquals(2, result.Rows.Count);
			AssertEquals("AIRLSEUSLAX-AUSYD--SUP1", result.Rows[0]["RateGroup"]);
			AssertEquals("AIRLSEUSLAX-AUSYD--SUP2", result.Rows[1]["RateGroup"]);
		}

		public void TestClientRatesGroupedByModeHasBranchCFX()
		{
			var supplier = helper.InsertOrgHeader("SUPPLIER", "Supplier");
			CreateRateEntryWithFlatRateLine(clientRate, "AIR", "LSE", supplier, "10");
			CreateRateEntryWithFlatRateLine(clientRate, "AIR", "ULD", supplier, "14");
			CreateRateEntryWithFlatRateLine(clientRate, "DST", "FCL", supplier, "55");
			CreateRateEntryWithFlatRateLine(clientRate, "DST", "LSE", supplier, "45");
			CreateRateEntryWithFlatRateLine(clientRate, "DST", "ULD", supplier, "85");

			branchPk = helper.InsertBranch("BBB", TestDbHelper.DefaultCompanyPK);

			AddCFXEntry("GB", branchPk, "IMP", "SEA", 2.00);
			AddCFXEntry("GB", branchPk, "EXP", "SEA", 2.01);
			AddCFXEntry("GB", branchPk, "IMP", "AIR", 2.02);
			AddCFXEntry("GB", branchPk, "EXP", "AIR", 2.03);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, QueryString("ALL", "", branchPk));

			AssertEquals("Expected three distinct categories", 5, result.Rows.Count);

			var cFXImpSeaResult = (from DataRow row in result.Rows select row["ClientSeaCFX"].ToString());
			var cFXExpSeaResult = (from DataRow row in result.Rows select row["ClientExportSeaCFX"].ToString());
			var cFXImpAirResult = (from DataRow row in result.Rows select row["ClientAirCFX"].ToString());
			var cFXExpAirResult = (from DataRow row in result.Rows select row["ClientExportAirCFX"].ToString());

			AssertEquals("2.00", cFXImpSeaResult.FirstOrDefault());
			AssertEquals("2.01", cFXExpSeaResult.FirstOrDefault());
			AssertEquals("2.02", cFXImpAirResult.FirstOrDefault());
			AssertEquals("2.03", cFXExpAirResult.FirstOrDefault());
		}

		public void TestClientRatesGroupedBySupplierHasBranchCFX()
		{
			var supplier1 = helper.InsertOrgHeader("SUP1", "Supplier 1");
			var supplier2 = helper.InsertOrgHeader("SUP2", "Supplier 2");
			CreateRateEntryWithFlatRateLine(clientRate, "AIR", "LSE", supplier1, "10");
			CreateRateEntryWithFlatRateLine(clientRate, "AIR", "LSE", supplier2, "14");

			branchPk = helper.InsertBranch("BBB", TestDbHelper.DefaultCompanyPK);

			AddCFXEntry("GB", branchPk, "IMP", "SEA", 2.00);
			AddCFXEntry("GB", branchPk, "EXP", "SEA", 2.01);
			AddCFXEntry("GB", branchPk, "IMP", "AIR", 2.02);
			AddCFXEntry("GB", branchPk, "EXP", "AIR", 2.03);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, QueryString("AIR", "LSE", branchPk));

			AssertEquals(2, result.Rows.Count);

			var cFXImpSeaResult = (from DataRow row in result.Rows select row["ClientSeaCFX"].ToString());
			var cFXExpSeaResult = (from DataRow row in result.Rows select row["ClientExportSeaCFX"].ToString());
			var cFXImpAirResult = (from DataRow row in result.Rows select row["ClientAirCFX"].ToString());
			var cFXExpAirResult = (from DataRow row in result.Rows select row["ClientExportAirCFX"].ToString());

			AssertEquals("2.00", cFXImpSeaResult.FirstOrDefault());
			AssertEquals("2.01", cFXExpSeaResult.FirstOrDefault());
			AssertEquals("2.02", cFXImpAirResult.FirstOrDefault());
			AssertEquals("2.03", cFXExpAirResult.FirstOrDefault());
		}

		#region Implementation

		Guid CreateClientRate(bool isGlobal = false)
		{
			var pk = Guid.NewGuid();

			if (isGlobal)
			{
				helper.Insert("RatingHeader", new
				{
					TH_PK = pk,
					TH_RateType = "SAL",
					TH_OH = client,
				});
			}
			else
			{
				helper.Insert("RatingHeader", new
				{
					TH_PK = pk,
					TH_RateType = "SAL",
					TH_OH = client,
					TH_GC = TestDbHelper.DefaultCompanyPK,
				});
			}

			return pk;
		}

		void CreateRateEntryWithFlatRateLine(Guid header, string category, string mode, Guid supplier, string flatAmount, bool isGlobal = false)
		{
			var entry = Guid.NewGuid();
			helper.Insert("RateEntry", new
			{
				TI_PK = entry,
				TI_TH = header,
				TI_GC_Publisher = TestDbHelper.DefaultCompanyPK,
				TI_RateCategory = category,
				TI_Mode = mode,
				TI_RateStartDate = helper.ToDate("2014-05-25"),
				TI_OH_Supplier = supplier,
				TI_OriginLRC = "USLAX",
				TI_DestinationLRC = "AUSYD",
			});

			var rateLine = Guid.NewGuid();
			helper.Insert("RateLines", new
			{
				TL_PK = rateLine,
				TL_TI = entry,
				TL_AC = isGlobal ? globalChargeCode : chargeCode,
				TL_RateCalculator = "FLT",
				TL_RX_NKCurrency = "USD",
			});

			var rateItem = Guid.NewGuid();
			helper.Insert("RateLineItems", new
			{
				TM_PK = rateItem,
				TM_TL = rateLine,
				TM_Type = "BAS",
				TM_Value = flatAmount,
			});
		}

		string QueryString(string category, string mode, Guid branch, bool includeGlobal = false)
		{
			var result = string.Format
				(
					"SELECT * FROM Report_ClientRatesByClientSummary('{0}', '{1}', '{2}', '{3}', 'N', {4}, '{5}')",
					TestDbHelper.DefaultCompanyPK,
					helper.ToDate("2015-01-01"),
					category,
					mode,
					(branch == Guid.Empty) ? "null" : "'" + branch.ToString() + "'",
					includeGlobal ? "Y" : ""
				);

			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();

			helper = new TestDbHelper(TestConnection);
			client = helper.InsertOrgHeader("CLIENT", "Client");

			chargeCode = helper.InsertChargeCode(TestDbHelper.DefaultCompanyPK, "NEWCHG");
			globalChargeCode = helper.InsertChargeCode(null, "NEWCHG");

			clientRate = CreateClientRate();
			globalClientRate = CreateClientRate(isGlobal: true);
		}

		void AddCFXEntry(string parentTableCode, Guid parentID, string serviceDirection, string transportMode, double percentage)
		{
			var cfxConfigPK = Guid.NewGuid();
			var cfxConfigQuery = string.Format("INSERT INTO dbo.AccJobConfig (JCF_PK, JCF_ConfigType, JCF_GC, JCF_Ledger, JCF_ParentTableCode, JCF_ParentID, JCF_JobType, JCF_ServiceDirection, JCF_TransportMode, JCF_Percentage, JCF_Amount) " +
				"VALUES ('{0}', 'CFX', '{1}', 'AR', '{2}', {3}, 'ALL', '{4}', '{5}', '{6}', 0)",
				cfxConfigPK,
				TestDbHelper.DefaultCompanyPK,
				parentTableCode,
				(parentID == Guid.Empty) ? "null" : "'" + parentID.ToString() + "'",
				serviceDirection,
				transportMode,
				percentage);
			TestConnection.ExecuteNonQuery(cfxConfigQuery);
		}

		TestDbHelper helper;
		Guid client, chargeCode, globalChargeCode, clientRate, globalClientRate, branchPk;
		#endregion
	}
}

