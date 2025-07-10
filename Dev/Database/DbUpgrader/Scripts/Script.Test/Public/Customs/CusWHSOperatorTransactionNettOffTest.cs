using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.Testing
{
	[TestedType(typeof(CusWHSOperatorTransactionNettOff))]
	class CusWHSOperatorTransactionNettOffTest : DbCreateScriptTest
	{
		public void TestColumnsReturned()
		{
			var result = RunProc(companyPK, warehouseAddressPK, productOwnerPK, "EXP");

			AssertNotNull(result);

			var columnNames = result.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToList();

			AssertContainsExactElementsInExactOrder(new[] { "WOL_PK", "WOL_WOT_WHSOperatorTransactionOrder", "WOL_WOT_WHSOperatorTransactionReceipt", "WOL_Quantity", "WOL_CustomsEntryLineNo" }, columnNames);
		}

		public void TestMultipleBatches()
		{
			var prod1PK = CreatePart("PROD1");
			var batch1PK = CreateWHSBatch("batch 1", companyPK, warehouseAddressPK);
			var batch2PK = CreateWHSBatch("batch 2", companyPK, warehouseAddressPK);
			var o1 = CreateWHSTransaction(batch1PK, 1, "ORD", "EXP", DateTime.Today, "REF1", productOwnerPK, 10, 100, "ZAR", "VAL", false, prod1PK);
			var o2 = CreateWHSTransaction(batch2PK, 1, "ORD", "EXP", DateTime.Today, "REF1", productOwnerPK, 5, 100, "ZAR", "VAL", false, prod1PK);
			var r1 = CreateWHSTransaction(batch1PK, 2, "REC", "", DateTime.Today, "REF1", productOwnerPK, 12, 100, "ZAR", "VAL", false, prod1PK);
			var r2 = CreateWHSTransaction(batch2PK, 2, "REC", "", DateTime.Today, "REF1", productOwnerPK, 1, 100, "ZAR", "VAL", false, prod1PK);

			var result = RunProc(companyPK, warehouseAddressPK, productOwnerPK, "EXP");

			CombineAssertions("Results", () =>
			{
				AssertNotNull("Null results", result);
				AssertEquals("3 rows should be returned", 3, result.Rows.Count);

				AssertTransactionStatus("B1-Ord1", o1, "CLS");
				AssertTransactionStatus("B1-Rec1", r1, "CLS");
				AssertTransactionStatus("B2-Ord2", o2, "VAL");
				AssertTransactionStatus("B2-Rec2", r2, "CLS");

				AssertTransactionLineRow("O1R1", result, o1, r1, 10);
				AssertTransactionLineRow("O2R1", result, o2, r1, 2);
				AssertTransactionLineRow("O2R2", result, o2, r2, 1);
				AssertTotals();
			});
		}

		public void TestBatchMatching()
		{
			var prod1PK = CreatePart("PROD1");
			var company2PK = CreateCompany("TS2");
			var warehouse2PK = CreateOrgHeader("WHS002");
			var warehouseAddress2PK = CreateWarehouseAddress(warehouse2PK);
			var productOwner2PK = CreateOrgHeader("PRO002");

			var batch1PK = CreateWHSBatch("batch 1", companyPK, warehouseAddressPK);
			var batch2PK = CreateWHSBatch("batch 2", company2PK, warehouseAddressPK);
			var batch3PK = CreateWHSBatch("batch 3", companyPK, warehouseAddress2PK);

			var o1 = CreateWHSTransaction(batch1PK, 1, "ORD", "EXP", DateTime.Today, "REF1", productOwnerPK, 10, 100, "ZAR", "VAL", false, prod1PK);
			var r1 = CreateWHSTransaction(batch2PK, 1, "REC", "", DateTime.Today, "REF1", productOwnerPK, 2, 100, "ZAR", "VAL", false, prod1PK);
			var r2 = CreateWHSTransaction(batch3PK, 1, "REC", "", DateTime.Today, "REF1", productOwnerPK, 2, 100, "ZAR", "VAL", false, prod1PK);
			var r3 = CreateWHSTransaction(batch1PK, 2, "REC", "", DateTime.Today, "REF1", productOwner2PK, 2, 100, "ZAR", "VAL", false, prod1PK);

			var result = RunProc(companyPK, warehouseAddressPK, productOwnerPK, "EXP");

			CombineAssertions("No matches", () =>
			{
				AssertNotNull("Null results", result);
				AssertEquals("0 rows should be returned", 0, result.Rows.Count);

				AssertTransactionStatus("B1-Ord1", o1, "VAL");
				AssertTransactionStatus("B1-Rec1", r1, "VAL");
				AssertTransactionStatus("B2-Ord2", r2, "VAL");
				AssertTotals();
			});
		}

		public void TestTransactionMatching()
		{
			var productOwner2PK = CreateOrgHeader("PRO002");
			var prod1PK = CreatePart("PROD1");
			var prod2PK = CreatePart("PROD2");
			var partPK = CreatePart("Part001");

			var batchPK = CreateWHSBatch("batch 1", companyPK, warehouseAddressPK);
			var t1 = CreateWHSTransaction(batchPK, 1, "ORD", "EXP", DateTime.Today, "REF1", productOwnerPK, 40, 100, "ZAR", "VAL", false, prod1PK);
			var t2 = CreateWHSTransaction(batchPK, 2, "REC", "", DateTime.Today, "REF2", productOwnerPK, 2, 70, "ZAR", "VAL", false, prod2PK);
			var t3 = CreateWHSTransaction(batchPK, 3, "REC", "", DateTime.Today, "REF1", productOwner2PK, 2, 70, "ZAR", "VAL", false, prod1PK);
			var t4 = CreateWHSTransaction(batchPK, 4, "REC", "", DateTime.Today, "REF1", productOwnerPK, 2, 70, "ZAR", "VAL", false, partPK);

			var result = RunProc(companyPK, warehouseAddressPK, productOwnerPK, "EXP");

			CombineAssertions(() =>
			{
				AssertNotNull("Null results", result);
				AssertEquals("No rows should be returned", 0, result.Rows.Count);

				AssertTransactionStatus("T1-ORD", t1, "VAL");
				AssertTransactionStatus("T2-REC", t2, "VAL");
				AssertTransactionStatus("T3-REC", t3, "VAL");
				AssertTransactionStatus("T3-REC", t4, "VAL");
				AssertTotals();
			});
		}

		public void TestReceiptsGreaterThanOrders()
		{
			var prod1PK = CreatePart("PROD1");
			var batchPK = CreateWHSBatch("batch 1", companyPK, warehouseAddressPK);
			var t1 = CreateWHSTransaction(batchPK, 1, "ORD", "EXP", DateTime.Today, "REF1", productOwnerPK, 10, 100, "ZAR", "VAL", false, prod1PK);
			var t2 = CreateWHSTransaction(batchPK, 2, "REC", "", DateTime.Today, "REF1", productOwnerPK, 17, 100, "ZAR", "VAL", false, prod1PK);

			var result = RunProc(companyPK, warehouseAddressPK, productOwnerPK, "EXP");

			CombineAssertions("Pre-reqs", () =>
			{
				AssertNotNull("Null results", result);
				AssertEquals("1 row should be returned", 1, result.Rows.Count);

				AssertTransactionStatus("T1-ORD", t1, "CLS");
				AssertTransactionStatus("T2-REC", t2, "VAL");
			});

			CombineAssertions("Results", () =>
			{
				AssertTransactionLineRow("T1T2", result, t1, t2, 10);
				AssertTotals();
			});
		}

		public void TestOrdersGreaterThanReceipts()
		{
			var prod1PK = CreatePart("PROD1");
			var batchPK = CreateWHSBatch("batch 1", companyPK, warehouseAddressPK);
			var t1 = CreateWHSTransaction(batchPK, 1, "ORD", "EXP", DateTime.Today, "REF1", productOwnerPK, 15, 100, "ZAR", "VAL", false, prod1PK);
			var t2 = CreateWHSTransaction(batchPK, 2, "REC", "", DateTime.Today, "REF1", productOwnerPK, 10, 100, "ZAR", "VAL", false, prod1PK);

			var result = RunProc(companyPK, warehouseAddressPK, productOwnerPK, "EXP");

			CombineAssertions("Pre-reqs", () =>
			{
				AssertNotNull("Null results", result);
				AssertEquals("1 row should be returned", 1, result.Rows.Count);

				AssertTransactionStatus("T1-ORD", t1, "VAL");
				AssertTransactionStatus("T2-REC", t2, "CLS");
			});

			CombineAssertions("Results", () =>
			{
				AssertTransactionLineRow("T1T2", result, t1, t2, 10);
				AssertTotals();
			});
		}

		public void TestMatchingOrdersAndReceipts()
		{
			var prod1PK = CreatePart("PROD1");
			var batchPK = CreateWHSBatch("batch 1", companyPK, warehouseAddressPK);
			var t1 = CreateWHSTransaction(batchPK, 1, "ORD", "EXP", DateTime.Today, "REF1", productOwnerPK, 10, 100, "ZAR", "VAL", false, prod1PK);
			var t2 = CreateWHSTransaction(batchPK, 2, "REC", "", DateTime.Today, "REF1", productOwnerPK, 10, 100, "ZAR", "VAL", false, prod1PK);

			var result = RunProc(companyPK, warehouseAddressPK, productOwnerPK, "EXP");

			CombineAssertions("Pre-reqs", () =>
			{
				AssertNotNull("Null results", result);
				AssertEquals("1 row should be returned", 1, result.Rows.Count);

				AssertTransactionStatus("T1-ORD", t1, "CLS");
				AssertTransactionStatus("T2-REC", t2, "CLS");
			});

			CombineAssertions("Results", () =>
			{
				AssertTransactionLineRow("T1T2", result, t1, t2, 10);
				AssertTotals();
			});
		}

		public void TestReceiptOrderingAsc()
		{
			var prod1PK = CreatePart("PROD1");
			var batchPK = CreateWHSBatch("batch 1", companyPK, warehouseAddressPK);
			var t1 = CreateWHSTransaction(batchPK, 1, "ORD", "EXP", DateTime.Today, "REF1", productOwnerPK, 10, 100, "ZAR", "VAL", false, prod1PK);
			var t2 = CreateWHSTransaction(batchPK, 2, "REC", "", DateTime.Today, "REF1", productOwnerPK, 8, 100, "ZAR", "VAL", false, prod1PK);
			var t3 = CreateWHSTransaction(batchPK, 3, "REC", "", DateTime.Today, "REF1", productOwnerPK, 5, 100, "ZAR", "VAL", true, prod1PK);

			var result = RunProc(companyPK, warehouseAddressPK, productOwnerPK, "EXP", true);

			CombineAssertions("Pre-reqs", () =>
			{
				AssertNotNull("Null results", result);
				AssertEquals("2 row should be returned", 2, result.Rows.Count);

				AssertTransactionStatus("T1-ORD", t1, "CLS");
				AssertTransactionStatus("T2-REC", t2, "CLS");
				AssertTransactionStatus("T3-REC", t3, "VAL");
			});

			CombineAssertions("Results", () =>
			{
				AssertTransactionLineRow("T1T2", result, t1, t2, 8);
				AssertTransactionLineRow("T1T3", result, t1, t3, 2);
				AssertTotals();
			});
		}

		public void TestReceiptOrderingDesc()
		{
			var prod1PK = CreatePart("PROD1");
			var batchPK = CreateWHSBatch("batch 1", companyPK, warehouseAddressPK);
			var t1 = CreateWHSTransaction(batchPK, 1, "ORD", "EXP", DateTime.Today, "REF1", productOwnerPK, 10, 100, "ZAR", "VAL", false, prod1PK);
			var t2 = CreateWHSTransaction(batchPK, 2, "REC", "", DateTime.Today, "REF1", productOwnerPK, 8, 100, "ZAR", "VAL", false, prod1PK);
			var t3 = CreateWHSTransaction(batchPK, 3, "REC", "", DateTime.Today, "REF1", productOwnerPK, 5, 100, "ZAR", "VAL", true, prod1PK);

			var result = RunProc(companyPK, warehouseAddressPK, productOwnerPK, "EXP", false);

			CombineAssertions("Pre-reqs", () =>
			{
				AssertNotNull("Null results", result);
				AssertEquals("2 row should be returned", 2, result.Rows.Count);

				AssertTransactionStatus("T1-ORD", t1, "CLS");
				AssertTransactionStatus("T2-REC", t2, "VAL");
				AssertTransactionStatus("T3-REC", t3, "CLS");
			});

			CombineAssertions("Results", () =>
			{
				AssertTransactionLineRow("T1T3", result, t1, t2, 5);
				AssertTransactionLineRow("T1T2", result, t1, t3, 5);
				AssertTotals();
			});
		}

		public void TestExportTypeFiltering()
		{
			var prod1PK = CreatePart("PROD1");
			var batchPK = CreateWHSBatch("batch 1", companyPK, warehouseAddressPK);
			var t1 = CreateWHSTransaction(batchPK, 1, "ORD", "EXP", DateTime.Today, "REF1", productOwnerPK, 10, 100, "ZAR", "VAL", false, prod1PK);
			var t2 = CreateWHSTransaction(batchPK, 2, "ORD", "BLN", DateTime.Today, "REF1", productOwnerPK, 9, 100, "ZAR", "VAL", false, prod1PK);
			var t3 = CreateWHSTransaction(batchPK, 3, "REC", "", DateTime.Today, "REF1", productOwnerPK, 100, 100, "ZAR", "VAL", false, prod1PK);

			var result = RunProc(companyPK, warehouseAddressPK, productOwnerPK, "EXP");

			CombineAssertions("EXP Run", () =>
			{
				AssertNotNull("Null results", result);
				AssertEquals("1 row should be returned", 1, result.Rows.Count);

				AssertTransactionStatus("T1-ORD", t1, "CLS");
				AssertTransactionStatus("T2-ORD", t2, "VAL");
				AssertTransactionStatus("T3-ORD", t3, "VAL");
			});

			CombineAssertions("Results", () =>
			{
				AssertTransactionLineRow("T1T3", result, t1, t3, 10);
				AssertTotals();
			});
		}

		public void TestOwnerReferenceFiltering()
		{
			var prod1PK = CreatePart("PROD1");
			var batchPK = CreateWHSBatch("batch 1", companyPK, warehouseAddressPK);
			var t1 = CreateWHSTransaction(batchPK, 1, "ORD", "EXP", DateTime.Today, "REF1", productOwnerPK, 15, 100, "ZAR", "VAL", false, prod1PK);
			var t2 = CreateWHSTransaction(batchPK, 2, "REC", "", DateTime.Today, "REF1", productOwnerPK, 10, 100, "ZAR", "VAL", false, prod1PK);
			var t3 = CreateWHSTransaction(batchPK, 3, "ORD", "EXP", DateTime.Today, "REF2", productOwnerPK, 15, 100, "ZAR", "VAL", false, prod1PK);
			var t4 = CreateWHSTransaction(batchPK, 4, "REC", "", DateTime.Today, "REF2", productOwnerPK, 10, 100, "ZAR", "VAL", false, prod1PK);
			var t5 = CreateWHSTransaction(batchPK, 5, "REC", "", DateTime.Today, "REF4", productOwnerPK, 10, 100, "ZAR", "VAL", false, prod1PK);

			var result = RunProc(companyPK, warehouseAddressPK, productOwnerPK, "EXP", true, "REF3");

			CombineAssertions("No match on filter", () =>
			{
				AssertNotNull("Null results", result);
				AssertEquals("0 row should be returned", 0, result.Rows.Count);

				AssertTransactionStatus("T1-ORD", t1, "VAL");
				AssertTransactionStatus("T2-REC", t2, "VAL");
				AssertTransactionStatus("T3-ORD", t3, "VAL");
				AssertTransactionStatus("T4-REC", t4, "VAL");
				AssertTransactionStatus("T5-REC", t5, "VAL");
				AssertTotals();
			});

			result = RunProc(companyPK, warehouseAddressPK, productOwnerPK, "EXP", true, "REF1");

			CombineAssertions("Filtered", () =>
			{
				AssertNotNull("Null results", result);
				AssertEquals("2 rows should be returned", 2, result.Rows.Count);

				AssertTransactionStatus("T1-ORD", t1, "CLS");
				AssertTransactionStatus("T2-REC", t2, "CLS");
				AssertTransactionStatus("T3-ORD", t3, "VAL");
				AssertTransactionStatus("T4-REC", t4, "VAL");
				AssertTransactionStatus("T5-REC", t5, "VAL");

				AssertTransactionLineRow("T1T2", result, t1, t2, 10);
				AssertTransactionLineRow("T1T4", result, t1, t4, 5);
				AssertTotals();
			});

			result = RunProc(companyPK, warehouseAddressPK, productOwnerPK, "EXP", true);

			CombineAssertions("No filter", () =>
			{
				AssertNotNull("Null results", result);
				AssertEquals("2 rows should be returned", 2, result.Rows.Count);

				AssertTransactionStatus("T3-ORD", t3, "CLS");
				AssertTransactionStatus("T4-REC", t4, "CLS");
				AssertTransactionStatus("T5-REC", t5, "CLS");

				AssertTransactionLineRow("T3T4", result, t3, t4, 5);
				AssertTransactionLineRow("T3T5", result, t3, t5, 10);
				AssertTotals();
			});
		}

		public void TestMultipleProductsAndLinesWithPreviousAllocations()
		{
			var prod1PK = CreatePart("PROD1");
			var prod2PK = CreatePart("PROD2");
			var prod3PK = CreatePart("PROD3");
			var batch1PK = CreateWHSBatch("Orders", companyPK, warehouseAddressPK);
			var batch2PK = CreateWHSBatch("Reeipts", companyPK, warehouseAddressPK);
			var o1 = CreateWHSTransaction(batch1PK, 1, "ORD", "EXP", DateTime.Today, "REF1", productOwnerPK, 50, 500, "ZAR", "VAL", false, prod1PK);
			var o2 = CreateWHSTransaction(batch1PK, 2, "ORD", "EXP", DateTime.Today, "REF1", productOwnerPK, 10, 100, "ZAR", "VAL", false, prod1PK);
			var o3 = CreateWHSTransaction(batch1PK, 3, "ORD", "EXP", DateTime.Today, "REF1", productOwnerPK, 50, 500, "ZAR", "CLS", false, prod1PK);
			var o4 = CreateWHSTransaction(batch1PK, 4, "ORD", "EXP", DateTime.Today, "REF1", productOwnerPK, 20, 200, "ZAR", "VAL", false, prod2PK);
			var o5 = CreateWHSTransaction(batch1PK, 5, "ORD", "EXP", DateTime.Today, "REF1", productOwnerPK, 30, 200, "ZAR", "VAL", false, prod2PK);
			var o6 = CreateWHSTransaction(batch1PK, 6, "ORD", "EXP", DateTime.Today, "REF1", productOwnerPK, 20, 200, "ZAR", "VAL", false, prod3PK); // Invalid Status
			var o7 = CreateWHSTransaction(batch1PK, 7, "ORD", "EXP", DateTime.Today, "REF1", productOwnerPK, 20, 200, "ZAR", "VAL", false, prod3PK);

			var r1 = CreateWHSTransaction(batch2PK, 1, "REC", "", DateTime.Today, "REF1", productOwnerPK, 20, 100, "ZAR", "VAL", false, prod1PK);
			var r2 = CreateWHSTransaction(batch2PK, 2, "REC", "", DateTime.Today, "REF1", productOwnerPK, 55, 100, "ZAR", "CLS", false, prod1PK);
			var r3 = CreateWHSTransaction(batch2PK, 3, "REC", "", DateTime.Today, "REF1", productOwnerPK, 15, 100, "ZAR", "VAL", false, prod1PK);
			var r4 = CreateWHSTransaction(batch2PK, 4, "REC", "", DateTime.Today, "REF1", productOwnerPK, 35, 100, "ZAR", "VAL", false, prod1PK);
			var r5 = CreateWHSTransaction(batch2PK, 5, "REC", "", DateTime.Today, "REF1", productOwnerPK, 45, 100, "ZAR", "VAL", false, prod2PK);
			var r6 = CreateWHSTransaction(batch2PK, 6, "REC", "", DateTime.Today, "REF1", productOwnerPK, 25, 100, "ZAR", "VAL", false, prod3PK); // Partially allocated

			CreateWHSTransactionLine(o6, r6, 20.0m);

			var result = RunProc(companyPK, warehouseAddressPK, productOwnerPK, "EXP");

			CombineAssertions("Results", () =>
			{
				AssertNotNull("Null results", result);
				
				AssertTransactionStatus("O1", o1, "CLS");
				AssertTransactionStatus("O2", o2, "CLS");
				AssertTransactionStatus("O3", o3, "CLS");
				AssertTransactionStatus("O4", o4, "CLS");
				AssertTransactionStatus("O5", o5, "VAL");
				AssertTransactionStatus("O6", o6, "VAL");
				AssertTransactionStatus("O7", o7, "VAL");
													 
				AssertTransactionStatus("R1", r1, "CLS");
				AssertTransactionStatus("R2", r2, "CLS");
				AssertTransactionStatus("R3", r3, "CLS");
				AssertTransactionStatus("R4", r4, "VAL");
				AssertTransactionStatus("R5", r5, "CLS");
				AssertTransactionStatus("R6", r6, "CLS");

				AssertEquals("7 rows should be returned", 7, result.Rows.Count);

				AssertTransactionLineRow("o1 r1", result, o1, r1, 20); // o1 => 30 r1 => 0
				AssertTransactionLineRow("o1 r3", result, o1, r3, 15); // o1 => 15 r3 => 0
				AssertTransactionLineRow("o1 r4", result, o1, r4, 15); // o1 => 0  r4 => 20
				AssertTransactionLineRow("o2 r4", result, o2, r4, 10); // o2 => 0  r4 => 25
				AssertTransactionLineRow("o4 r5", result, o4, r5, 20); // o4 => 0  r5 => 25
				AssertTransactionLineRow("o5 r5", result, o5, r5, 25); // o5 => 5  r5 => 0
				AssertTransactionLineRow("o7 r6", result, o7, r6, 5);  // o7 => 15 r6 => 0
				AssertTotals();
			});
		}

		public void TestExplicitTransactionProperlyManaged()
		{
			var prod1PK = CreatePart("PROD1");
			var batchPK = CreateWHSBatch("batch 1", companyPK, warehouseAddressPK);
			var t1 = CreateWHSTransaction(batchPK, 1, "ORD", "EXP", DateTime.Today, "REF1", productOwnerPK, 10, 100, "ZAR", "VAL", false, prod1PK);
			var t2 = CreateWHSTransaction(batchPK, 2, "REC", "", DateTime.Today, "REF1", productOwnerPK, 10, 100, "ZAR", "VAL", false, prod1PK);

			int tranCountBefore = Convert.ToInt32(TestConnection.ExecuteScalar("SELECT @@trancount"), CultureInfo.InvariantCulture);
			AssertEquals("Transaction count before execution", 1, tranCountBefore);

			var result = RunProc(companyPK, warehouseAddressPK, productOwnerPK, "EXP");

			int tranCountAfter = Convert.ToInt32(TestConnection.ExecuteScalar("SELECT @@trancount"), CultureInfo.InvariantCulture);
			AssertEquals("Transaction count after execution", tranCountBefore, tranCountAfter);
		}

		public void TestWithoutSqlTransaction()
		{
			var ex = AssertExceptionThrown<SqlException>(() =>
			{
				var connection = Db.NewExtraConnectionToMainDb();
				RunProc(connection, companyPK, warehouseAddressPK, productOwnerPK, "EXP");
			});

			AssertEquals("This procedure must be executed in a transaction.", ex.Message);
		}

		public void TestCustomsControlledStock_Ascending()
		{
			var prod1PK = CreatePart("PROD1");
			var batchPK = CreateWHSBatch("batch 1", companyPK, warehouseAddressPK);
			var o1 = CreateWHSTransaction(batchPK, 1, "ORD", "EXP", DateTime.Today, "REF1", productOwnerPK, 10, 10, "ZAR", "VAL", false, prod1PK);
			var r1 = CreateWHSTransaction(batchPK, 2, "REC", "", DateTime.Today, "REF1", productOwnerPK, 4, 4, "ZAR", "VAL", true, prod1PK);
			var r2 = CreateWHSTransaction(batchPK, 3, "REC", "", DateTime.Today, "REF1", productOwnerPK, 5, 5, "ZAR", "VAL", true, prod1PK);
			var r3 = CreateWHSTransaction(batchPK, 4, "REC", "", DateTime.Today, "REF1", productOwnerPK, 6, 6, "ZAR", "VAL", true, prod1PK);
			var r4 = CreateWHSTransaction(batchPK, 5, "REC", "", DateTime.Today, "REF1", productOwnerPK, 3, 3, "ZAR", "VAL", false, prod1PK);
			var r5 = CreateWHSTransaction(batchPK, 6, "REC", "", DateTime.Today, "REF1", productOwnerPK, 2, 2, "ZAR", "VAL", false, prod1PK);

			var result = RunProc(companyPK, warehouseAddressPK, productOwnerPK, "EXP");

			CombineAssertions(() =>
			{
				AssertNotNull("Null results", result);
				AssertEquals("4 rows should be returned", 4, result.Rows.Count);

				AssertTransactionStatus("O1-ORD", o1, "CLS");
				AssertTransactionStatus("R4-REC", r4, "CLS");
				AssertTransactionStatus("R5-REC", r5, "CLS");
				AssertTransactionStatus("R1-REC", r1, "CLS");
				AssertTransactionStatus("R2-REC", r2, "VAL");
				AssertTransactionStatus("R3-REC", r3, "VAL");

				AssertTransactionLineRow("O1R4", result, o1, r4, 3);
				AssertTransactionLineRow("O1R5", result, o1, r5, 2);
				AssertTransactionLineRow("O1R1", result, o1, r1, 4);
				AssertTransactionLineRow("O1R2", result, o1, r2, 1);
				AssertTotals();
			});
		}

		public void TestCustomsControlledStock_Descending()
		{
			var prod1PK = CreatePart("PROD1");
			var batchPK = CreateWHSBatch("batch 1", companyPK, warehouseAddressPK);
			var o1 = CreateWHSTransaction(batchPK, 1, "ORD", "EXP", DateTime.Today, "REF1", productOwnerPK, 17, 17, "ZAR", "VAL", false, prod1PK);
			var r1 = CreateWHSTransaction(batchPK, 2, "REC", "", DateTime.Today, "REF1", productOwnerPK, 4, 4, "ZAR", "VAL", true, prod1PK);
			var r2 = CreateWHSTransaction(batchPK, 3, "REC", "", DateTime.Today, "REF1", productOwnerPK, 5, 5, "ZAR", "VAL", true, prod1PK);
			var r3 = CreateWHSTransaction(batchPK, 4, "REC", "", DateTime.Today, "REF1", productOwnerPK, 6, 6, "ZAR", "VAL", true, prod1PK);
			var r4 = CreateWHSTransaction(batchPK, 5, "REC", "", DateTime.Today, "REF1", productOwnerPK, 3, 3, "ZAR", "VAL", false, prod1PK);
			var r5 = CreateWHSTransaction(batchPK, 6, "REC", "", DateTime.Today, "REF1", productOwnerPK, 2, 2, "ZAR", "VAL", false, prod1PK);

			var result = RunProc(companyPK, warehouseAddressPK, productOwnerPK, "EXP", sortAscending: false);

			CombineAssertions(() =>
			{
				AssertNotNull("Null results", result);
				AssertEquals("4 rows should be returned", 4, result.Rows.Count);

				AssertTransactionStatus("O1-ORD", o1, "CLS");
				AssertTransactionStatus("R1-REC", r1, "CLS");
				AssertTransactionStatus("R2-REC", r2, "CLS");
				AssertTransactionStatus("R3-REC", r3, "CLS");
				AssertTransactionStatus("R4-REC", r4, "VAL");
				AssertTransactionStatus("R5-REC", r5, "VAL");

				AssertTransactionLineRow("O1R1", result, o1, r1, 4);
				AssertTransactionLineRow("O1R2", result, o1, r2, 5);
				AssertTransactionLineRow("O1R3", result, o1, r3, 6);
				AssertTransactionLineRow("O1R4", result, o1, r4, 2);
				AssertTotals();
			});
		}

		public void TestPopulateLineNo_NoEntryRef()
		{
			var prod1PK = CreatePart("PROD1");
			var batchPK = CreateWHSBatch("batch 1", companyPK, warehouseAddressPK);
			var t1 = CreateWHSTransaction(batchPK, 1, "ORD", "EXP", DateTime.Today, "REF1", productOwnerPK, 10, 100, "ZAR", "VAL", false, prod1PK);
			var t2 = CreateWHSTransaction(batchPK, 2, "REC", "", DateTime.Today, "REF1", productOwnerPK, 10, 100, "ZAR", "VAL", true, prod1PK);

			var result = RunProc(companyPK, warehouseAddressPK, productOwnerPK, "EXP", true);

			CombineAssertions(() =>
			{
				AssertNotNull("Null results", result);
				AssertEquals("1 row should be returned", 1, result.Rows.Count);

				AssertTransactionStatus("T1-ORD", t1, "CLS");
				AssertTransactionStatus("T2-REC", t2, "CLS");
				AssertTransactionLineRow("T1T2", result, t1, t2, 10, 0);
				AssertTotals();
			});
		}

		public void TestPopulateLineNo_NoStock()
		{
			var prod1PK = CreatePart("PROD1");
			var batchPK = CreateWHSBatch("batch 1", companyPK, warehouseAddressPK);
			var t1 = CreateWHSTransaction(batchPK, 1, "ORD", "EXP", DateTime.Today, "REF1", productOwnerPK, 10, 100, "ZAR", "VAL", false, prod1PK);
			var t2 = CreateWHSTransaction(batchPK, 2, "REC", "", DateTime.Today, "REF1", productOwnerPK, 10, 100, "ZAR", "VAL", true, prod1PK, "CE00001");

			var result = RunProc(companyPK, warehouseAddressPK, productOwnerPK, "EXP", true);

			CombineAssertions(() =>
			{
				AssertNotNull("Null results", result);
				AssertEquals("1 row should be returned", 1, result.Rows.Count);

				AssertTransactionStatus("T1-ORD", t1, "CLS");
				AssertTransactionStatus("T2-REC", t2, "CLS");
				AssertTransactionLineRow("T1T2", result, t1, t2, 10, 0);
				AssertTotals();
			});
		}

		public void TestPopulateLineNo()
		{
			var prod1PK = CreatePart("PROD1");
			var batchPK = CreateWHSBatch("batch 1", companyPK, warehouseAddressPK);
			var p1 = CreateWHSTransaction(batchPK, 1, "ORD", "EXP", DateTime.Today, "REF1", productOwnerPK, 11, 100, "ZAR", "CLS", false, prod1PK);
			var p2 = CreateWHSTransaction(batchPK, 2, "REC", "", DateTime.Today, "REF1", productOwnerPK, 11, 100, "ZAR", "CLS", true, prod1PK, "CEN00001");
			var t1 = CreateWHSTransaction(batchPK, 3, "ORD", "EXP", DateTime.Today, "REF1", productOwnerPK, 10, 100, "ZAR", "VAL", false, prod1PK);
			var t2 = CreateWHSTransaction(batchPK, 4, "REC", "", DateTime.Today, "REF1", productOwnerPK, 10, 100, "ZAR", "VAL", true, prod1PK, "CEN00001");

			CreateStock(productOwnerPK, prod1PK, 11, warehousePK, warehouseLocPK, "stockP", "StockP", "CEN00001", 1);
			CreateStock(productOwnerPK, prod1PK, 6, warehousePK, warehouseLocPK, "stock1", "Stock1", "CEN00001", 2);
			CreateStock(productOwnerPK, prod1PK, 4, warehousePK, warehouseLocPK, "stock2", "Stock2", "CEN00001", 3);

			CreateWHSTransactionLine(p1, p2, 11.0m, 1);

			var result = RunProc(companyPK, warehouseAddressPK, productOwnerPK, "EXP", true);

			CombineAssertions(() =>
			{
				AssertNotNull("Null results", result);
				AssertEquals("2 rows should be returned", 2, result.Rows.Count);

				AssertTransactionStatus("T1-ORD", t1, "CLS");
				AssertTransactionStatus("T2-REC", t2, "CLS");

				AssertTransactionLineRow("T1T2S1", result, t1, t2, 6, 2);
				AssertTransactionLineRow("T1T2S2", result, t1, t2, 4, 3);
				AssertTotals();
			});
		}

		public void TestPopulateLineNo_InsufficientStock()
		{
			var prod1PK = CreatePart("PROD1");
			var batchPK = CreateWHSBatch("batch 1", companyPK, warehouseAddressPK);
			var t1 = CreateWHSTransaction(batchPK, 1, "ORD", "EXP", DateTime.Today, "REF1", productOwnerPK, 10, 100, "ZAR", "VAL", false, prod1PK);
			var t2 = CreateWHSTransaction(batchPK, 2, "REC", "", DateTime.Today, "REF1", productOwnerPK, 10, 100, "ZAR", "VAL", true, prod1PK, "CEN00001");

			CreateStock(productOwnerPK, prod1PK, 6, warehousePK, warehouseLocPK, "stock1", "Stock1", "CEN00001", 1);
			CreateStock(productOwnerPK, prod1PK, 1, warehousePK, warehouseLocPK, "stock2", "Stock2", "CEN00001", 2);

			var result = RunProc(companyPK, warehouseAddressPK, productOwnerPK, "EXP", true);

			CombineAssertions(() =>
			{
				AssertNotNull("Null results", result);
				AssertEquals("2 row should be returned", 2, result.Rows.Count);

				AssertTransactionStatus("T1-ORD", t1, "CLS");
				AssertTransactionStatus("T2-REC", t2, "CLS");

				AssertTransactionLineRow("T1T2S1", result, t1, t2, 6, 1);
				AssertTransactionLineRow("T1T2S2", result, t1, t2, 4, 2);
				AssertTotals();
			});
		}

		public void TestPopulateLineNo_SameProdDifferentEntryNumbers()
		{
			var prod1PK = CreatePart("PROD1");
			var batchPK = CreateWHSBatch("batch 1", companyPK, warehouseAddressPK);
			var t1 = CreateWHSTransaction(batchPK, 1, "ORD", "EXP", DateTime.Today, "REF1", productOwnerPK, 20, 100, "ZAR", "VAL", false, prod1PK);
			var t2 = CreateWHSTransaction(batchPK, 2, "REC", "", DateTime.Today, "REF1", productOwnerPK, 12, 100, "ZAR", "VAL", true, prod1PK, "CEN00001");
			var t3 = CreateWHSTransaction(batchPK, 3, "REC", "", DateTime.Today, "REF1", productOwnerPK, 8, 100, "ZAR", "VAL", true, prod1PK, "CEN00002");

			CreateStock(productOwnerPK, prod1PK, 7, warehousePK, warehouseLocPK, "stock1", "Stock1", "CEN00001", 1);
			CreateStock(productOwnerPK, prod1PK, 4, warehousePK, warehouseLocPK, "stock2", "Stock2", "CEN00001", 2);
			CreateStock(productOwnerPK, prod1PK, 9, warehousePK, warehouseLocPK, "stock3", "Stock3", "CEN00002", 3);

			var result = RunProc(companyPK, warehouseAddressPK, productOwnerPK, "EXP", true);

			CombineAssertions(() =>
			{
				AssertNotNull("Null results", result);
				AssertEquals("3 rows should be returned", 3, result.Rows.Count);

				AssertTransactionStatus("T1-ORD", t1, "CLS");
				AssertTransactionStatus("T2-REC", t2, "CLS");
				AssertTransactionStatus("T3-REC", t3, "CLS");

				AssertTransactionLineRow("T1T2S1", result, t1, t2, 7, 1);
				AssertTransactionLineRow("T1T2S2", result, t1, t2, 5, 2);
				AssertTransactionLineRow("T1T3S3", result, t1, t3, 8, 3);
				AssertTotals();
			});
		}

		public void TestTransactionStatusUpdate()
		{
			var prod1PK = CreatePart("PROD1");
			var batchPK = CreateWHSBatch("batch 1", companyPK, warehouseAddressPK);
			var t1 = CreateWHSTransaction(batchPK, 1, "ORD", "", DateTime.Today, "REF1", productOwnerPK, 5, 500, "ZAR", "VAL", false, prod1PK);
			var t2 = CreateWHSTransaction(batchPK, 2, "ORD", "", DateTime.Today, "REF1", productOwnerPK, 5, 500, "ZAR", "VAL", false, prod1PK);
			var t3 = CreateWHSTransaction(batchPK, 3, "REC", "", DateTime.Today, "REF1", productOwnerPK, 10, 1000, "ZAR", "VAL", true, prod1PK, "CEN00001");

			CreateStock(productOwnerPK, prod1PK, 3, warehousePK, warehouseLocPK, "stock1", "Stock1", "CEN00001", 1);

			var result = RunProc(companyPK, warehouseAddressPK, productOwnerPK, "");

			CombineAssertions("Pre-reqs", () =>
			{
				AssertNotNull("Null results", result);
				AssertEquals("1 row should be returned", 1, result.Rows.Count);

				AssertTransactionStatus("T1-ORD", t1, "CLS");
				AssertTransactionStatus("T2-ORD", t2, "VAL");
				AssertTransactionStatus("T3-REC", t3, "VAL");
			});

			CombineAssertions("Results", () =>
			{
				AssertTransactionLineRow("T1T3", result, t1, t3, 5, 1);
				AssertTotals();
			});
		}

		void AssertTransactionStatus(string assertMessage, Guid pk, string expectedStatus)
		{
			var status = TestConnection.ExecuteScalar($"SELECT TOP 1 WOT_Status FROM dbo.CusWHSOperatorTransaction WHERE WOT_PK = '{pk}'");

			AssertEquals($"Transaction Status: {assertMessage}", expectedStatus, status);
		}

		void AssertTransactionLineRow(string assertMsg, DataTable table, Guid expectedOrder, Guid expectedReceipt, decimal expectedQty, int expectedCustomsEntryLineNo = 0)
		{
			var filter = $"WOL_WOT_WHSOperatorTransactionOrder = '{expectedOrder}' AND WOL_WOT_WHSOperatorTransactionReceipt = '{expectedReceipt}' AND WOL_CustomsEntryLineNo = {expectedCustomsEntryLineNo}";
			var row = table.Select(filter).FirstOrDefault();

			if (row != null)
			{
				AssertEquals($"Quantity-{assertMsg}", expectedQty, (decimal)row["WOL_Quantity"]);
			}
			else
			{
				var results = string.Join("\n", table.Rows.Cast<DataRow>().Select(x => $"Order: {x["WOL_WOT_WHSOperatorTransactionOrder"]} Receipt: {x["WOL_WOT_WHSOperatorTransactionOrder"]} Qty: {x["WOL_Quantity"]} EntryLine: {x["WOL_CustomsEntryLineNo"]}"));
				Assert($"Could not find row for {assertMsg}: ExpectedQty: {expectedQty} Filter: {filter}\nResults\n{results}", false);
			}
		}

		void AssertTotals()
		{
			var sql = @"select 
							WOT_PK, WOT_Quantity, ISNULL(TotalQty, 0) as TotalQty
						from dbo.CusWHSOperatorTransaction
						join 
						(
							select WOL_WOT_WHSOperatorTransactionReceipt as PK,	sum(WOL_Quantity) as TotalQty
							from dbo.CusWHSOperatorTransactionLine
							group by WOL_WOT_WHSOperatorTransactionReceipt

							union

							select WOL_WOT_WHSOperatorTransactionOrder as PK, sum(WOL_Quantity) as TotalQty
							from dbo.CusWHSOperatorTransactionLine
							group by WOL_WOT_WHSOperatorTransactionOrder
						) L on L.PK = WOT_PK
						where TotalQty > WOT_Quantity";

			TestConnection.ExecuteReader(sql, (iDataRecord) =>
			{
				var data = $"Over Allocation: WOT_PK: {iDataRecord["WOT_PK"]} WOT_Quantity: {iDataRecord["WOT_Quantity"]} TotalAllocation: {iDataRecord["TotalQty"]}";
				Assert(data, false);
			});

			sql = @"select 
					WOT_PK, WOT_Quantity, ISNULL(TotalQty, 0) as TotalQty
				from dbo.CusWHSOperatorTransaction
				left join 
				(
					select WOL_WOT_WHSOperatorTransactionReceipt as PK,	sum(WOL_Quantity) as TotalQty
					from dbo.CusWHSOperatorTransactionLine
					group by WOL_WOT_WHSOperatorTransactionReceipt

					union

					select WOL_WOT_WHSOperatorTransactionOrder as PK, sum(WOL_Quantity) as TotalQty
					from dbo.CusWHSOperatorTransactionLine
					group by WOL_WOT_WHSOperatorTransactionOrder
				) L on L.PK = WOT_PK
				where
					WOT_Status = 'CLS'
					AND WOT_Quantity <> TotalQty";

			TestConnection.ExecuteReader(sql, (iDataRecord) =>
			{
				var data = $"Closed but not Allocated : WOT_PK: {iDataRecord["WOT_PK"]} WOT_Quantity: {iDataRecord["WOT_Quantity"]} TotalAllocation: {iDataRecord["TotalQty"]}";
				Assert(data, false);
			});
		}

		Guid CreateCompany(string companyCode, string countryCode = "AU", string currencyCode = "AUD")
		{
			var pk = Guid.NewGuid();

			TestConnection.ExecuteNonQuery($"INSERT INTO dbo.GlbCompany(GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES ('{pk}', '{companyCode}', 'AU company', '{countryCode}', '{currencyCode}')");

			return pk;
		}

		Guid CreateBranch(Guid companyPK, string code)
		{
			var pk = Guid.NewGuid();

			TestConnection.ExecuteNonQuery($"INSERT INTO dbo.GlbBranch(GB_PK, GB_GC, GB_Code) VALUES ('{pk}', '{companyPK}', '{code}')");

			return pk;
		}

		Guid CreateOrgHeader(string orgCode)
		{
			var pk = Guid.NewGuid();

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.OrgHeader(OH_PK, OH_Code) values ('{pk}', '{orgCode}')");

			return pk;
		}

		Guid CreateWarehouseAddress(Guid orgPK)
		{
			var pk = Guid.NewGuid();

			TestConnection.ExecuteNonQuery($"INSERT INTO dbo.OrgAddress(OA_PK, OA_OH, OA_Address1) values ('{pk}', '{orgPK}', 'Test Address')");

			return pk;
		}

		Guid CreateWHSBatch(string batch, Guid companyPK, Guid warehouseAddressPK)
		{
			var pk = Guid.NewGuid();

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.CusWHSOperatorTransactionBatch([WOB_PK], [WOB_Batch], [WOB_GC_Company], [WOB_OA_Warehouse], [WOB_SystemCreateTimeUtc], [WOB_SystemCreateUser], [WOB_SystemLastEditTimeUtc], [WOB_SystemLastEditUser])
												VALUES ('{pk}', '{batch}', '{companyPK}', '{warehouseAddressPK}', GetUtcDate(), 'E', GetUtcDate(), 'E')");

			return pk;
		}

		Guid CreateWHSTransaction(Guid batch, int batchLineNo, string transactionType, string exportType, DateTime transactionDate, string ownerReference, Guid productOwner, decimal quantity, decimal totalValue, string currency, string status, bool isCustomsControlled, Guid partPK, string customsEntryNumber = "")
		{
			var pk = Guid.NewGuid();

			if (transactionType != "REC" || !isCustomsControlled)
			{
				customsEntryNumber = "";
			}

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.CusWHSOperatorTransaction ([WOT_PK], [WOT_WOB_CusWHSTransactionBatch], [WOT_TransactionType], [WOT_ExportType], [WOT_TransactionDate], [WOT_OwnerReference], [WOT_OP_Product], [WOT_OH_ProductOwner], [WOT_Quantity], [WOT_TotalValue], [WOT_RX_NKCurrency], [WOT_Status], [WOT_IsCustomsControlled], [WOT_RN_NKOrigin], [WOT_BatchLineNo], [WOT_CustomsEntryNumber], [WOT_SystemCreateTimeUtc], [WOT_SystemCreateUser], [WOT_SystemLastEditTimeUtc], [WOT_SystemLastEditUser])
											VALUES ('{pk}','{batch}','{transactionType}','{exportType}', '{transactionDate:yyyy/MM/dd}', '{ownerReference}', '{partPK}', '{productOwner}',{quantity}, {totalValue}, '{currency}', '{status}', {(isCustomsControlled && transactionType == "REC" ? 1 : 0)}, '', {batchLineNo}, '{customsEntryNumber}', GETUTCDATE(), 'E', GETUTCDATE(), 'E')");

			return pk;
		}

		void CreateWHSTransactionLine(Guid order, Guid receipt, decimal qty, short cusEntryLineNo = 0)
		{
			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.CusWHSOperatorTransactionLine ([WOL_PK], [WOL_WOT_WHSOperatorTransactionOrder], [WOL_WOT_WHSOperatorTransactionReceipt], [WOL_Quantity], [WOL_CustomsEntryLineNo], [WOL_SystemCreateTimeUtc], [WOL_SystemCreateUser], [WOL_SystemLastEditTimeUtc], [WOL_SystemLastEditUser])
											VALUES (NEWID(),'{order}','{receipt}',{qty}, {cusEntryLineNo}, GETUTCDATE(), 'E', GETUTCDATE(), 'E')");
		}

		Guid CreatePart(string partNum)
		{
			var pk = Guid.NewGuid();

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.OrgSupplierPart ([OP_PK], [OP_PartNum]) VALUES ('{pk}','{partNum}')");

			return pk;
		}

		(Guid Warehouse, Guid Location) CreateWarehouse(Guid branchPK, Guid addressPK, string code)
		{
			var wwPK = Guid.NewGuid();
			var waPK = Guid.NewGuid();
			var wrPK = Guid.NewGuid();
			var wlPK = Guid.NewGuid();

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.WhsWarehouse([WW_PK], [WW_GB_RelatedCompanyBranch], [WW_OA_WarehouseAddress], [WW_WLT_DefaultLocationType], [WW_WarehouseCode], [WW_DefaultOutboundDockDoor], [WW_DefaultInboundDockDoor], [WW_SystemCreateTimeUtc], [WW_SystemCreateUser], [WW_SystemLastEditTimeUtc], [WW_SystemLastEditUser])
											VALUES ('{wwPK}','{branchPK}','{addressPK}','{wltPK}','{code}', NEWID(), NEWID(), GETUTCDATE(), 'E', GETUTCDATE(), 'E')");

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.WhsArea (WA_PK, WA_WW_Whs, WA_Name, [WA_SystemCreateTimeUtc], [WA_SystemCreateUser], [WA_SystemLastEditTimeUtc], [WA_SystemLastEditUser])
											VALUES ('{waPK}', '{wwPK}', 'WHS Area 1', GETUTCDATE(), 'E', GETUTCDATE(), 'E')");

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.WhsRow(WR_PK, WR_WW_Whs, WR_Name, WR_Columns, WR_Levels, WR_Trays, WR_PickPathSequence, [WR_SystemCreateTimeUtc], [WR_SystemCreateUser], [WR_SystemLastEditTimeUtc], [WR_SystemLastEditUser])
											VALUES ('{wrPK}', '{wwPK}', 'Row1', 1, 1, 1, 1, GETUTCDATE(), 'E', GETUTCDATE(), 'E')");

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.WhsLocation (WL_PK, WL_WA_PickingArea, WL_WA_PutawayArea, WL_WR, WL_WLT_LocationType, WL_PutawayPathSequence, [WL_SystemCreateTimeUtc], [WL_SystemCreateUser], [WL_SystemLastEditTimeUtc], [WL_SystemLastEditUser])
											VALUES ('{wlPK}', '{waPK}', '{waPK}', '{wrPK}', '{wltPK}', 1, GETUTCDATE(), 'E', GETUTCDATE(), 'E')");

			return (wwPK, wlPK);
		}

		void CreateStock(Guid owner, Guid productPK, decimal quantity, Guid warehousePK, Guid whsLocationPK, string docId, string uniqueId, string entryKey, int entryLineNo)
		{
			var wdPK = Guid.NewGuid();
			var wePK = Guid.NewGuid();
			var docDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.WhsDocket ([WD_PK], [WD_OH_Client],[WD_DocketID], [WD_ExternalReference], [WD_DocketStatus], [WD_FinalisedDate], [WD_BookingDate], [WD_WW_Whs], [WD_DocketType], WD_DocketSubType, WD_GS_NKFinalizedBy, [WD_SystemCreateTimeUtc], [WD_SystemCreateUser], [WD_SystemLastEditTimeUtc], [WD_SystemLastEditUser])
											VALUES ('{wdPK}', '{owner}', '{docId}', '{uniqueId}', 'FIN', '{docDate}', '{docDate}', '{warehousePK}', 'ADJ', 'NEA', 'E', GETUTCDATE(), 'E', GETUTCDATE(), 'E')");

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.WhsDocketLine ([WE_PK], [WE_WD], [WE_OP], [WE_F3_NKPackType], [WE_CurrentInventoryStatus], [WE_OriginalInventoryStatus], [WE_DocketLineType], [WE_TransactionQuantity], [WE_StockOnHand], [WE_DocketLineStatus], [WE_FinalisedDate], [WE_AdjustmentArrivalDate], WE_WL, WE_WE_OriginalDocketLineForRating, [WE_SystemCreateTimeUtc], [WE_SystemCreateUser], [WE_SystemLastEditTimeUtc], [WE_SystemLastEditUser])
											VALUES ('{wePK}', '{wdPK}', '{productPK}', 'UNT', 'AVL', 'AVL', 'ADJ', {quantity}, {quantity}, 'FIN', '{docDate}', '{docDate}', '{whsLocationPK}', '{wePK}', GETUTCDATE(), 'E', GETUTCDATE(), 'E')");

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.WhsBondedWarehouseAttribute([WB_PK], [WB_EntryKey], [WB_EntryLineNo], [WB_ParentID], [WB_ParentTableCode], [WB_SystemCreateTimeUtc], [WB_SystemCreateUser], [WB_SystemLastEditTimeUtc], [WB_SystemLastEditUser])
											 VALUES (NEWID(), '{entryKey}', {entryLineNo}, '{wePK}', 'WE', GETUTCDATE(), 'E', GETUTCDATE(), 'E')");
		}

		Guid CreateWhsLocationType()
		{
			var pk = Guid.NewGuid();

			TestConnection.ExecuteNonQuery($@"INSERT INTO dbo.WhsLocationType(WLT_PK, WLT_Code, WLT_Description, [WLT_SystemCreateTimeUtc], [WLT_SystemCreateUser], [WLT_SystemLastEditTimeUtc], [WLT_SystemLastEditUser])
											VALUES ('{pk}', 'WLT', 'WHS Loc Type', GETUTCDATE(), 'E', GETUTCDATE(), 'E')");

			return pk;
		}

		DataTable RunProc(Guid company, Guid warehouseAddress, Guid productOwner, string exportType, bool sortAscending = true, params string[] ownerReferences)
		{
			return RunProc(TestConnection, company, warehouseAddress, productOwner, exportType, sortAscending, ownerReferences);
		}

		DataTable RunProc(DbConnection connection, Guid company, Guid warehouseAddress, Guid productOwner, string exportType, bool sortAscending = true, params string[] ownerReferences)
		{
			var sql = $"EXEC {nameof(CusWHSOperatorTransactionNettOff)} @Company, @WarehouseAddress, @ProductOwner, @ExportType, @OwnerReferences, @SortAscending, @CurrentUser, @CurrentTimeUtc";

			using (var cmd = connection.Command(sql))
			{
				cmd.AddParameter("@Company", SqlDbType.UniqueIdentifier, 0, company);
				cmd.AddParameter("@WarehouseAddress", SqlDbType.UniqueIdentifier, 0, warehouseAddress);
				cmd.AddParameter("@ProductOwner", SqlDbType.UniqueIdentifier, 0, productOwner);
				cmd.AddParameter("@ExportType", SqlDbType.VarChar, 3, exportType);
				cmd.AddTableValuedParameter("@OwnerReferences", "TVP_Varchar_35", ownerReferences);
				cmd.AddParameter("@SortAscending", SqlDbType.Bit, 1, sortAscending);
				cmd.AddParameter("@CurrentUser", SqlDbType.VarChar, 'E');
				cmd.AddParameter("@CurrentTimeUtc", SqlDbType.SmallDateTime, DateTime.Now);

				return DataUtils.GetDataTableFromCommand(cmd);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			companyPK = CreateCompany("TS1");
			var branchPK = CreateBranch(companyPK, "BR1");
			warehouseOrgPK = CreateOrgHeader("WHS001");
			warehouseAddressPK = CreateWarehouseAddress(warehouseOrgPK);
			productOwnerPK = CreateOrgHeader("PRO001");
			wltPK = CreateWhsLocationType();
			(warehousePK, warehouseLocPK) = CreateWarehouse(branchPK, warehouseAddressPK, "W01");
		}

		Guid companyPK;
		Guid warehouseOrgPK;
		Guid warehouseAddressPK;
		Guid productOwnerPK;
		Guid wltPK;
		Guid warehousePK;
		Guid warehouseLocPK;
	}
}
