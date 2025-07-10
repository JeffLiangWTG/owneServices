using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(Report_JobProfitTransactionTotalsByBranchAndDepartment))]
	class Report_JobProfitTransactionTotalsByBranchAndDepartmentTest : DbCreateScriptTest
	{
		[TestDate(2020, 10, 13)]
		public void TestFromDateAndToDateFilters()
		{
			var today = new DateTime(2020, 10, 13);
			var dbHelper = new TestDbHelper(TestConnection);
			var chargeCodePK = dbHelper.InsertChargeCode(TestDbHelper.DefaultCompanyPK, "CHCODE");

			// Naming to identify test cases: <Type>_<AL_LineType>_<AL_PostDate>_<AL_ReverseDate>
			// -1 days = inside filter
			// +1 days = after filter
			// -31 days = before filter
			AddShipmentWithTransactionLine("SHP_REV_NULL_-01", "REV", 100m, reverseDate: today.AddDays(-1));
			AddShipmentWithTransactionLine("SHP_REV_NULL_+01", "REV", 101m, reverseDate: today.AddDays(1));
			AddShipmentWithTransactionLine("SHP_REV_NULL_-31", "REV", 102m, reverseDate: today.AddDays(-31));
			AddShipmentWithTransactionLine("SHP_CST_NULL_-01", "CST", 110m, reverseDate: today.AddDays(-1));
			AddShipmentWithTransactionLine("SHP_CST_NULL_+01", "CST", 111m, reverseDate: today.AddDays(1));
			AddShipmentWithTransactionLine("SHP_CST_NULL_-31", "CST", 112m, reverseDate: today.AddDays(-31));

			AddShipmentWithTransactionLine("SHP_ACR_-01_NULL", "ACR", 200m, postDate: today.AddDays(-1), reverseDate: null);
			AddShipmentWithTransactionLine("SHP_ACR_+01_NULL", "ACR", 201m, postDate: today.AddDays(1), reverseDate: null);
			AddShipmentWithTransactionLine("SHP_ACR_-31_NULL", "ACR", 202m, postDate: today.AddDays(-31), reverseDate: null);
			AddShipmentWithTransactionLine("SHP_ACR_-01_-01", "ACR", 210m, postDate: today.AddDays(-1), reverseDate: today.AddDays(-1));
			AddShipmentWithTransactionLine("SHP_ACR_-01_+01", "ACR", 211m, postDate: today.AddDays(-1), reverseDate: today.AddDays(1));
			AddShipmentWithTransactionLine("SHP_ACR_-01_-31", "ACR", 212m, postDate: today.AddDays(-1), reverseDate: today.AddDays(-31));
			AddShipmentWithTransactionLine("SHP_ACR_+01_-01", "ACR", 220m, postDate: today.AddDays(1), reverseDate: today.AddDays(-1));
			AddShipmentWithTransactionLine("SHP_ACR_+01_+01", "ACR", 221m, postDate: today.AddDays(1), reverseDate: today.AddDays(1));
			AddShipmentWithTransactionLine("SHP_ACR_+01_-31", "ACR", 222m, postDate: today.AddDays(1), reverseDate: today.AddDays(-31));
			AddShipmentWithTransactionLine("SHP_ACR_-31_-01", "ACR", 230m, postDate: today.AddDays(-31), reverseDate: today.AddDays(-1));
			AddShipmentWithTransactionLine("SHP_ACR_-31_+01", "ACR", 231m, postDate: today.AddDays(-31), reverseDate: today.AddDays(1));
			AddShipmentWithTransactionLine("SHP_ACR_-31_-31", "ACR", 232m, postDate: today.AddDays(-31), reverseDate: today.AddDays(-31));

			AddShipmentWithTransactionLine("SHP_WIP_-01_NULL", "WIP", 300m, postDate: today.AddDays(-1), reverseDate: null);
			AddShipmentWithTransactionLine("SHP_WIP_+01_NULL", "WIP", 301m, postDate: today.AddDays(1), reverseDate: null);
			AddShipmentWithTransactionLine("SHP_WIP_-31_NULL", "WIP", 302m, postDate: today.AddDays(-31), reverseDate: null);
			AddShipmentWithTransactionLine("SHP_WIP_-01_-01", "WIP", 310m, postDate: today.AddDays(-1), reverseDate: today.AddDays(-1));
			AddShipmentWithTransactionLine("SHP_WIP_-01_+01", "WIP", 311m, postDate: today.AddDays(-1), reverseDate: today.AddDays(1));
			AddShipmentWithTransactionLine("SHP_WIP_-01_-31", "WIP", 312m, postDate: today.AddDays(-1), reverseDate: today.AddDays(-31));
			AddShipmentWithTransactionLine("SHP_WIP_+01_-01", "WIP", 320m, postDate: today.AddDays(1), reverseDate: today.AddDays(-1));
			AddShipmentWithTransactionLine("SHP_WIP_+01_+01", "WIP", 321m, postDate: today.AddDays(1), reverseDate: today.AddDays(1));
			AddShipmentWithTransactionLine("SHP_WIP_+01_-31", "WIP", 322m, postDate: today.AddDays(1), reverseDate: today.AddDays(-31));
			AddShipmentWithTransactionLine("SHP_WIP_-31_-01", "WIP", 330m, postDate: today.AddDays(-31), reverseDate: today.AddDays(-1));
			AddShipmentWithTransactionLine("SHP_WIP_-31_+01", "WIP", 331m, postDate: today.AddDays(-31), reverseDate: today.AddDays(1));
			AddShipmentWithTransactionLine("SHP_WIP_-31_-31", "WIP", 332m, postDate: today.AddDays(-31), reverseDate: today.AddDays(-31));

			AddShipmentWithTransactionLine("SHP_DRC_-01_+01", "DRC", 350m, postDate: today.AddDays(-1), reverseDate: today.AddDays(1));

			var key = 0;
			AddDeclarationWithTransactionLine(++key, "DCL_REV_NULL_-01", "REV", 1000m, reverseDate: today.AddDays(-1));
			AddDeclarationWithTransactionLine(++key, "DCL_REV_NULL_+01", "REV", 1001m, reverseDate: today.AddDays(1));
			AddDeclarationWithTransactionLine(++key, "DCL_CST_NULL_-01", "CST", 1010m, reverseDate: today.AddDays(-1));
			AddDeclarationWithTransactionLine(++key, "DCL_CST_NULL_+01", "CST", 1011m, reverseDate: today.AddDays(1));

			AddDeclarationWithTransactionLine(++key, "DCL_ACR_-01_NULL", "ACR", 1200m, postDate: today.AddDays(-1), reverseDate: null);
			AddDeclarationWithTransactionLine(++key, "DCL_ACR_+01_NULL", "ACR", 1201m, postDate: today.AddDays(1), reverseDate: null);
			AddDeclarationWithTransactionLine(++key, "DCL_ACR_-31_NULL", "ACR", 1202m, postDate: today.AddDays(-31), reverseDate: null);
			AddDeclarationWithTransactionLine(++key, "DCL_ACR_-01_-01", "ACR", 1210m, postDate: today.AddDays(-1), reverseDate: today.AddDays(-1));
			AddDeclarationWithTransactionLine(++key, "DCL_ACR_-01_+01", "ACR", 1211m, postDate: today.AddDays(-1), reverseDate: today.AddDays(1));
			AddDeclarationWithTransactionLine(++key, "DCL_ACR_-01_-31", "ACR", 1212m, postDate: today.AddDays(-1), reverseDate: today.AddDays(-31));
			AddDeclarationWithTransactionLine(++key, "DCL_ACR_+01_-01", "ACR", 1220m, postDate: today.AddDays(1), reverseDate: today.AddDays(-1));
			AddDeclarationWithTransactionLine(++key, "DCL_ACR_+01_+01", "ACR", 1221m, postDate: today.AddDays(1), reverseDate: today.AddDays(1));
			AddDeclarationWithTransactionLine(++key, "DCL_ACR_+01_-31", "ACR", 1222m, postDate: today.AddDays(1), reverseDate: today.AddDays(-31));
			AddDeclarationWithTransactionLine(++key, "DCL_ACR_-31_-01", "ACR", 1230m, postDate: today.AddDays(-31), reverseDate: today.AddDays(-1));
			AddDeclarationWithTransactionLine(++key, "DCL_ACR_-31_+01", "ACR", 1231m, postDate: today.AddDays(-31), reverseDate: today.AddDays(1));
			AddDeclarationWithTransactionLine(++key, "DCL_ACR_-31_-31", "ACR", 1232m, postDate: today.AddDays(-31), reverseDate: today.AddDays(-31));

			AddDeclarationWithTransactionLine(++key, "DCL_WIP_-01_NULL", "WIP", 1300m, postDate: today.AddDays(-1), reverseDate: null);
			AddDeclarationWithTransactionLine(++key, "DCL_WIP_+01_NULL", "WIP", 1301m, postDate: today.AddDays(1), reverseDate: null);
			AddDeclarationWithTransactionLine(++key, "DCL_WIP_-31_NULL", "WIP", 1302m, postDate: today.AddDays(-31), reverseDate: null);
			AddDeclarationWithTransactionLine(++key, "DCL_WIP_-01_-01", "WIP", 1310m, postDate: today.AddDays(-1), reverseDate: today.AddDays(-1));
			AddDeclarationWithTransactionLine(++key, "DCL_WIP_-01_+01", "WIP", 1311m, postDate: today.AddDays(-1), reverseDate: today.AddDays(1));
			AddDeclarationWithTransactionLine(++key, "DCL_WIP_-01_-31", "WIP", 1312m, postDate: today.AddDays(-1), reverseDate: today.AddDays(-31));
			AddDeclarationWithTransactionLine(++key, "DCL_WIP_+01_-01", "WIP", 1320m, postDate: today.AddDays(1), reverseDate: today.AddDays(-1));
			AddDeclarationWithTransactionLine(++key, "DCL_WIP_+01_+01", "WIP", 1321m, postDate: today.AddDays(1), reverseDate: today.AddDays(1));
			AddDeclarationWithTransactionLine(++key, "DCL_WIP_+01_-31", "WIP", 1322m, postDate: today.AddDays(1), reverseDate: today.AddDays(-31));
			AddDeclarationWithTransactionLine(++key, "DCL_WIP_-31_-01", "WIP", 1330m, postDate: today.AddDays(-31), reverseDate: today.AddDays(-1));
			AddDeclarationWithTransactionLine(++key, "DCL_WIP_-31_+01", "WIP", 1331m, postDate: today.AddDays(-31), reverseDate: today.AddDays(1));
			AddDeclarationWithTransactionLine(++key, "DCL_WIP_-31_-31", "WIP", 1332m, postDate: today.AddDays(-31), reverseDate: today.AddDays(-31));

			AddDeclarationWithTransactionLine(++key, "DCL_DRC_-01_+01", "DRC", 1350m, postDate: today.AddDays(-1), reverseDate: today.AddDays(1));

			var result = RunWith(fromDate: today.AddDays(-30), toDate: today);
			var actualIdentifiers = GetIdentifiers(result);
			var expectedIdentifiers = new[]
			{
				"SHP_REV_NULL_-01",
				"SHP_CST_NULL_-01",

				"SHP_ACR_-01_NULL",
				"SHP_ACR_-01_+01",
				"SHP_ACR_-01_-31",
				"SHP_ACR_+01_-01",
				"SHP_ACR_-31_-01",
				"SHP_WIP_-01_NULL",
				"SHP_WIP_-01_+01",
				"SHP_WIP_-01_-31",
				"SHP_WIP_+01_-01",
				"SHP_WIP_-31_-01",

				"DCL_REV_NULL_-01",
				"DCL_CST_NULL_-01",

				"DCL_ACR_-01_NULL",
				"DCL_ACR_-01_+01",
				"DCL_ACR_-01_-31",
				"DCL_ACR_+01_-01",
				"DCL_ACR_-31_-01",
				"DCL_WIP_-01_NULL",
				"DCL_WIP_-01_+01",
				"DCL_WIP_-01_-31",
				"DCL_WIP_+01_-01",
				"DCL_WIP_-31_-01",
			};

			var expectedPostDateAndReverseDateBetween = new[]
			{
				"SHP_WIP_-01_-01",
				"SHP_ACR_-01_-01",
				"DCL_ACR_-01_-01",
				"DCL_WIP_-01_-01"
			};

			AssertContainsExactElementsInAnyOrder(expectedIdentifiers.Union(expectedPostDateAndReverseDateBetween), actualIdentifiers);

			result = RunWith(fromDate: today.AddDays(-30), toDate: today, outstandingWIPOnly: "Y");
			var actualIdentifiersWIPOnly = GetIdentifiers(result);
			var expectedWIPOnly = expectedIdentifiers.Where(x => x.Contains("_WIP_")).ToArray();
			AssertContainsExactElementsInAnyOrder(expectedWIPOnly, actualIdentifiersWIPOnly);

			result = RunWith(fromDate: today.AddDays(-30), toDate: today, outstandingACROnly: "Y");
			var actualIdentifiersACROnly = GetIdentifiers(result);
			var expectedACROnly = expectedIdentifiers.Where(x => x.Contains("_ACR_")).ToArray();
			AssertContainsExactElementsInAnyOrder(expectedACROnly, actualIdentifiersACROnly);

			#region Helpers

			void AddShipmentWithTransactionLine(string identifier, string lineType, decimal amount, DateTime? postDate = null, DateTime? reverseDate = null)
			{
				var shipmentPK = dbHelper.InsertShipment(identifier, today);
				var jobHeaderPK = dbHelper.InsertJob(identifier, TestDbHelper.DefaultCompanyPK, dbHelper.DefaultBranchPK, TestDbHelper.DepartmentBrnPK, "JS", shipmentPK, "CMP", today);

				dbHelper.InsertTransactionLine(null, jobHeaderPK, chargeCodePK, dbHelper.GLAccountPK1, dbHelper.DefaultBranchPK, TestDbHelper.DepartmentBrnPK, null, amount, lineType, postDate, reverseDate);
			}

			void AddDeclarationWithTransactionLine(int clusterKey, string identifier, string lineType, decimal amount, DateTime? postDate = null, DateTime? reverseDate = null)
			{
				var declarationPK = TestDataCreator.CreateJobDeclaration(dbHelper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK, identifier, "XXX", clusterKey);
				var jobHeaderPK = dbHelper.InsertJob(identifier, TestDbHelper.DefaultCompanyPK, dbHelper.DefaultBranchPK, TestDbHelper.DepartmentBrnPK, "JS", declarationPK, "CMP", today);

				dbHelper.InsertTransactionLine(null, jobHeaderPK, chargeCodePK, dbHelper.GLAccountPK1, dbHelper.DefaultBranchPK, TestDbHelper.DepartmentBrnPK, null, amount, lineType, postDate, reverseDate);
			}

			IEnumerable<string> GetIdentifiers(DataTable dt) =>
				dt.Rows.Cast<DataRow>()
				.Select(x => x.Field<string>("JH_JobNum"));
			#endregion
		}

		[TestDate(2020, 10, 15)]
		public void TestNoAccTransactionLinesTableScansForNarrowDateRanges()
		{
			var today = new DateTime(2020, 10, 13);
			var baseDate = today.AddDays(-50);
			var dbHelper = new TestDbHelper(TestConnection);
			var chargeCodePK = dbHelper.InsertChargeCode(TestDbHelper.DefaultCompanyPK, "CHCODE");
			for (int i = 0; i < 100; i++)
			{
				var shipmentPK = dbHelper.InsertShipment("SHP" + i.ToString("000", CultureInfo.InvariantCulture), baseDate);
				var shipmentJobHeaderPK = dbHelper.InsertJob("SHP" + i.ToString("000", CultureInfo.InvariantCulture), TestDbHelper.DefaultCompanyPK, dbHelper.DefaultBranchPK, TestDbHelper.DepartmentBrnPK, "JS", shipmentPK, "CMP", baseDate);
				for (int j = 0; j < 20; j++)
				{
					dbHelper.InsertTransactionLine(null, shipmentJobHeaderPK, chargeCodePK, dbHelper.GLAccountPK1, dbHelper.DefaultBranchPK, TestDbHelper.DepartmentBrnPK, null, i * j * 1.1m, "WIP", baseDate, null);
				}
				dbHelper.InsertTransactionLine(null, shipmentJobHeaderPK, chargeCodePK, null, dbHelper.DefaultBranchPK, TestDbHelper.DepartmentBrnPK, null, i * 1.5m, "REV", baseDate, baseDate.AddDays(5));

				var declarationPK = TestDataCreator.CreateJobDeclaration(dbHelper.DefaultBranchPK, TestDbHelper.DefaultCompanyPK, "DCL" + i.ToString("000", CultureInfo.InvariantCulture), "XXX", i + 1);
				var declarationJobHeaderPK = dbHelper.InsertJob("DCL" + i.ToString("000", CultureInfo.InvariantCulture), TestDbHelper.DefaultCompanyPK, dbHelper.DefaultBranchPK, TestDbHelper.DepartmentBrnPK, "JS", declarationPK, "CMP", baseDate);
				for (int j = 0; j < 20; j++)
				{
					dbHelper.InsertTransactionLine(null, declarationJobHeaderPK, chargeCodePK, dbHelper.GLAccountPK1, dbHelper.DefaultBranchPK, TestDbHelper.DepartmentBrnPK, null, i * j * 1.1m, "WIP", baseDate, null);
				}
				dbHelper.InsertTransactionLine(null, declarationJobHeaderPK, chargeCodePK, null, dbHelper.DefaultBranchPK, TestDbHelper.DepartmentBrnPK, null, i * 1.5m, "REV", baseDate, baseDate.AddDays(5));

				baseDate = baseDate.AddDays(1);
			}
			TestConnection.ExecuteNonQuery($"UPDATE STATISTICS JobShipment WITH FULLSCAN");
			TestConnection.ExecuteNonQuery($"UPDATE STATISTICS JobDeclaration WITH FULLSCAN");
			TestConnection.ExecuteNonQuery($"UPDATE STATISTICS JobHeader WITH FULLSCAN");
			TestConnection.ExecuteNonQuery($"UPDATE STATISTICS AccTransactionLines WITH FULLSCAN");

			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				var rowCount = 0;
				var query = GetQueryWith(fromDate: today.AddDays(-1), toDate: today);
				TestConnection.ExecuteReader(query, r => rowCount++);
				AssertEquals(4, rowCount);

				var queryPlan = TestConnection.ExecutedCommandsAndQueryPlans.First(t => t.Item1.Contains("Report_JobProfitTransactionTotalsByBranchAndDepartment"));
				var queryPlanAnalyzer = new QueryPlanalyzer(queryPlan.Item2.First());

				Assert("There should be no table scans on AccTransactionLines.", !queryPlanAnalyzer.TableScans.Any(x => x.TableName == "AccTransactionLines"));
			}
		}

		#region Helpers

		DataTable RunWith(
			DateTime? fromDate = null,
			DateTime? toDate = null,
			string outstandingWIPOnly = "",
			string outstandingACROnly = "")
		{
			var query = GetQueryWith(
				fromDate: fromDate,
				toDate: toDate,
				outstandingWIPOnly: outstandingWIPOnly,
				outstandingACROnly: outstandingACROnly);
			return DataUtils.GetDataTableFromQuery(TestConnection, query);
		}

		static string GetQueryWith(
			DateTime? fromDate = null,
			DateTime? toDate = null,
			string outstandingWIPOnly = "",
			string outstandingACROnly = "")
		{
			var today = new DateTime(2020, 10, 13);
			fromDate = fromDate ?? today.AddDays(-30);
			toDate = toDate ?? today;
			outstandingWIPOnly = outstandingWIPOnly ?? string.Empty;
			outstandingACROnly = outstandingACROnly ?? string.Empty;

			return $@"SELECT * FROM Report_JobProfitTransactionTotalsByBranchAndDepartment(
					'{TestDbHelper.DefaultCompanyPK}',	-- @JH_GC
					'A', -- @JobType

					'{outstandingWIPOnly}', -- @AL_OutstandingWIPOnly
					'{outstandingACROnly}', -- @AL_OutstandingACROnly
					'', -- @AC_ChargeGroup
					NULL, -- @AC_AR_SalesGroup
					NULL, -- @AC_AR_ExpenseGroup
					'', -- @AL_BranchPKList
					'', -- @AL_DepartmentPKList
					'', -- @AL_ChargeCodePKList
					'', -- @AL_ExcludedChargeCodePKList
					'', -- @AL_CreditorPKList
					'', -- @AL_DebtorPKList
					'{fromDate:yyyy-MM-dd}', -- @AL_FromDate
					'{toDate:yyyy-MM-dd}', -- @AL_ToDate

					'', -- @JK_ConsolMode
					'', -- @JK_TransportMode
					'', -- @JK_AgentType
					NULL, -- @SendingForwarderPK
					NULL, -- @ReceivingForwarderPK
					NULL, -- @CreditorPK
					NULL, -- @ShippingLinePK
					NULL, -- @JK_JX_LoadPort
					NULL, -- @JK_JX_DischargePort
					'Jan  1 1900 12:00:00:000AM', -- @JK_JX_FromETD
					'Jun  6 2079 11:59:29:000PM', -- @JK_JX_ToETD
					'Jan  1 1900 12:00:00:000AM', -- @JK_JX_FromETA
					'Jun  6 2079 11:59:29:000PM', -- @JK_JX_ToETA
					'AU', -- @CurrentCountry
					'1900-01-01 00:00:00', -- @RevRecogFrom
					'2079-06-06 23:59:29', -- @RevRecogTo
					'' -- @Gateway
)";
		}
		#endregion
	}
}
