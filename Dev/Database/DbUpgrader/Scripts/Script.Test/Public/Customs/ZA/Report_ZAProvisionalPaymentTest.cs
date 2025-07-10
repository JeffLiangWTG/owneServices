using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZA;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;
using static NUnit.Framework.Assertion;
using static NUnit.Framework.AssertionWithHtml;

namespace Enterprise.Build.Database.Script.Public.Customs.ZA.Testing
{
	class ReportParams
	{
		public string ShipmentType { get; set; }
		public string TransportMode { get; set; }
		public string CustomsOffice { get; set; }
		public string ProcedureCode { get; set; }
		public string EntryStatus { get; set; }
		public string ProvisionalPaymentType { get; set; }
		public DateTime? JobRegisteredOnFromUtc { get; set; }
		public DateTime? JobRegisteredOnToUtc { get; set; }
		public DateTime? EntrySubmittedDateFrom { get; set; }
		public DateTime? EntrySubmittedDateTo { get; set; }
		public DateTime? AssessmentDateFrom { get; set; }
		public DateTime? AssessmentDateTo { get; set; }
		public DateTime? EntryReleaseDateFrom { get; set; }
		public DateTime? EntryReleaseDateTo { get; set; }
		public DateTime? ExpiryDateFrom { get; set; }
		public DateTime? ExpiryDateTo { get; set; }
		public DateTime? LiquidationDateFrom { get; set; }
		public DateTime? LiquidationDateTo { get; set; }
	}

	abstract class RepTestCase
	{
		public abstract ReportParams Parameters { get; }
		public abstract void RunTestAssertions(string testName, IDataReader reader);
	}

	class TCCheckColumns : RepTestCase
	{
		public override ReportParams Parameters
		{
			get
			{
				var dt1 = new DateTime(2020, 06, 01);
				var dtNext = new DateTime(2020, 06, 02);

				return new ReportParams
				{
					ShipmentType = "EXP",
					TransportMode = "AIR",
					CustomsOffice = "CTN",
					ProcedureCode = "001",
					EntryStatus = "001",
					ProvisionalPaymentType = "PPA",
					JobRegisteredOnFromUtc = dt1,
					JobRegisteredOnToUtc = dtNext,
					EntrySubmittedDateFrom = dt1,
					EntrySubmittedDateTo = dtNext,
					AssessmentDateFrom = dt1,
					AssessmentDateTo = dtNext,
					EntryReleaseDateFrom = dt1,
					EntryReleaseDateTo = dtNext,
					ExpiryDateFrom = dt1,
					ExpiryDateTo = dtNext,
					LiquidationDateFrom = dt1,
					LiquidationDateTo = dtNext
				};
			}
		}

		public override void RunTestAssertions(string testName, IDataReader reader)
		{
			AssertNotNull(testName + " expecting reader", reader);
			AssertEquals("Expecting 1 row", true, reader.Read());
			AssertEquals("Expecting 27 columns", 27, reader.FieldCount);

			CombineAssertions("Checking columns", () =>
			{
				AssertEquals("Col  0:", "JobNumber", reader.GetName(0));
				AssertEquals("Col  1:", "CustomsOffice", reader.GetName(1));
				AssertEquals("Col  2:", "ShipmentType", reader.GetName(2));
				AssertEquals("Col  3:", "TransportMode", reader.GetName(3));
				AssertEquals("Col  4:", "VoyageFlightNo", reader.GetName(4));
				AssertEquals("Col  5:", "MasterBill", reader.GetName(5));
				AssertEquals("Col  6:", "HouseBill", reader.GetName(6));
				AssertEquals("Col  7:", "JobRegisteredDate", reader.GetName(7));
				AssertEquals("Col  8:", "ProcedureCode", reader.GetName(8));
				AssertEquals("Col  9:", "EntryInstructionDescription", reader.GetName(9));
				AssertEquals("Col 10:", "ImporterCode", reader.GetName(10));
				AssertEquals("Col 11:", "ImporterFullName", reader.GetName(11));
				AssertEquals("Col 12:", "SupplierCode", reader.GetName(12));
				AssertEquals("Col 13:", "SupplierFullName", reader.GetName(13));
				AssertEquals("Col 14:", "LRNNumber", reader.GetName(14));
				AssertEquals("Col 15:", "MRNNumber", reader.GetName(15));
				AssertEquals("Col 16:", "ProvisionalPaymentNumber", reader.GetName(16));
				AssertEquals("Col 17:", "ProvisionalPaymentType", reader.GetName(17));
				AssertEquals("Col 18:", "ProvisionalPaymentAmount", reader.GetName(18));
				AssertEquals("Col 19:", "ExpiryDate", reader.GetName(19));
				AssertEquals("Col 20:", "PaymentStatus", reader.GetName(20));
				AssertEquals("Col 21:", "LiquidationDate", reader.GetName(21));
				AssertEquals("Col 22:", "EntryLineNumber", reader.GetName(22));
				AssertEquals("Col 23:", "PaymentStatusDescription", reader.GetName(23));
				AssertEquals("Col 24:", "BranchPK", reader.GetName(24));
				AssertEquals("Col 25:", "ImporterPK", reader.GetName(25));
				AssertEquals("Col 26:", "SupplierPK", reader.GetName(26));
			});

			CombineAssertions("Checking Content", () =>
			{
				var dt = new DateTime(2020, 06, 01);

				AssertEquals("JobNumber", "JB001", reader[0]);
				AssertEquals("CustomsOffice", "CTN", reader[1]);
				AssertEquals("ShipmentType", "EXP", reader[2]);
				AssertEquals("TransportMode", "AIR", reader[3]);
				AssertEquals("VoyageFlightNo", "VES001", reader[4]);
				AssertEquals("MasterBill", "MB001", reader[5]);
				AssertEquals("HouseBill", "HB001", reader[6]);
				AssertEquals("JobRegisteredDate", dt, reader.GetDateTime(7));
				AssertEquals("ProcedureCode", "001", reader[8]);
				AssertEquals("EntryInstructionDescription", "CEI 001", reader[9]);
				AssertEquals("ImporterCode", "MDORG001", reader[10]);
				AssertEquals("ImporterFullName", "MD001", reader[11]);
				AssertEquals("SupplierCode", "MDORG002", reader[12]);
				AssertEquals("SupplierFullName", "MD002", reader[13]);
				AssertEquals("LRNNumber", "BGM001", reader[14]);
				AssertEquals("MRNNumber", "MRN001", reader[15]);
				AssertEquals("ProvisionalPaymentNumber", "PAY001", reader[16]);
				AssertEquals("ProvisionalPaymentType", "PPA", reader[17]);
				AssertEquals("ProvisionalPaymentAmount", 101m, reader.GetDecimal(18));
				AssertEquals("ExpiryDate", dt, reader.GetDateTime(19));
				AssertEquals("PaymentStatus", "CLR", reader[20]);
				AssertEquals("LiquidationDate", dt, reader.GetDateTime(21));
				AssertEquals("EntryLineNumber", "101", reader[22]);
				AssertEquals("PaymentStatusDescription", "Clear", reader[23]);

				AssertNotEquals("BranchPK", Guid.Empty, reader.GetGuid(24));
				AssertNotEquals("ImporterPK", Guid.Empty, reader.GetGuid(25));
				AssertNotEquals("SupplierPK", Guid.Empty, reader.GetGuid(26));
			});

			var rows = 1;
			while (reader.Read())
			{
				rows++;
			}

			AssertEquals("Expecting 1 row", 1, rows);
		}
	}

	class TSCheckPaymentTypes : RepTestCase
	{
		public override ReportParams Parameters => new ReportParams { ProcedureCode = "001" };

		public override void RunTestAssertions(string testName, IDataReader reader)
		{
			AssertNotNull(testName + " expecting reader", reader);

			var ptData = new Dictionary<string, int>();

			while (reader.Read())
			{
				var pt = reader["ProvisionalPaymentType"].ToString();
				if (ptData.ContainsKey(pt))
				{
					ptData[pt]++;
				}
				else
				{
					ptData.Add(pt, 1);
				}
			}

			CombineAssertions(testName, () =>
			{
				AssertEquals("Expecting 8 Types", 8, ptData.Count);
				AssertEquals("No XXX", false, ptData.ContainsKey("XXX"));
				ptData.ForEach(x => AssertEquals($"Count PT: {x.Key}", 1, x.Value));
			});
		}
	}

	class TCFilterTest : RepTestCase
	{
		readonly int _expectedRows;
		readonly ReportParams _filter;

		public TCFilterTest(ReportParams filter, int expectedRowCount)
		{
			_filter = filter;
			_expectedRows = expectedRowCount;
		}

		public override ReportParams Parameters => _filter;

		public override void RunTestAssertions(string testName, IDataReader reader)
		{
			AssertNotNull(testName + " expecting reader", reader);

			int rows = 0;
			while (reader.Read())
			{
				rows++;
			}

			AssertEquals($"Expected rows for {testName}", _expectedRows, rows);
		}
	}

	[TestedType(typeof(Report_ZAProvisionalPayment))]
	class Report_ZAProvisionalPaymentTest : DbCreateScriptTest
	{
		public void TestReportFunction()
		{
			var gcPK = PrepareData();
			RunTest("Check Columns", gcPK, new TCCheckColumns());
			RunTest("Payment Type", gcPK, new TSCheckPaymentTypes());

			RunTest("Filter ShipmentType IMP", gcPK, new TCFilterTest(new ReportParams { ShipmentType = "IMP" }, 8));
			RunTest("Filter ShipmentType EXP", gcPK, new TCFilterTest(new ReportParams { ShipmentType = "EXP" }, 16));
			RunTest("Filter ShipmentType XXX", gcPK, new TCFilterTest(new ReportParams { ShipmentType = "XXX" }, 0));
			RunTest("Filter TransportMode SEA", gcPK, new TCFilterTest(new ReportParams { TransportMode = "SEA" }, 8));
			RunTest("Filter TransportMode RAI", gcPK, new TCFilterTest(new ReportParams { TransportMode = "RAI" }, 0));
			RunTest("Filter CustomsOffice JHB", gcPK, new TCFilterTest(new ReportParams { CustomsOffice = "JHB" }, 8));
			RunTest("Filter CustomsOffice XXX", gcPK, new TCFilterTest(new ReportParams { CustomsOffice = "XXX" }, 0));
			RunTest("Filter CustomsOffice DUR: ignored as has override customs office", gcPK, new TCFilterTest(new ReportParams { CustomsOffice = "DUR" }, 0));
			RunTest("Filter CustomsOffice CTN: override customs office", gcPK, new TCFilterTest(new ReportParams { CustomsOffice = "CTN" }, 16));
			RunTest("Filter ProcedureCode 001", gcPK, new TCFilterTest(new ReportParams { ProcedureCode = "001" }, 8));
			RunTest("Filter ProcedureCode XXX", gcPK, new TCFilterTest(new ReportParams { ProcedureCode = "XXX" }, 0));
			RunTest("Filter EntryStatus 001", gcPK, new TCFilterTest(new ReportParams { EntryStatus = "001" }, 8));
			RunTest("Filter EntryStatus XXX", gcPK, new TCFilterTest(new ReportParams { EntryStatus = "XXX" }, 0));
			RunTest("Filter ProvisionalPaymentType FOR", gcPK, new TCFilterTest(new ReportParams { ProvisionalPaymentType = "FOR" }, 3));

			RunTest("Filter JobRegisteredOnFromUtc", gcPK, new TCFilterTest(new ReportParams { JobRegisteredOnFromUtc = new DateTime(2020, 06, 02) }, 16));
			RunTest("Filter JobRegisteredOnToUtc", gcPK, new TCFilterTest(new ReportParams { JobRegisteredOnToUtc = new DateTime(2020, 06, 03) }, 16));
			RunTest("Filter JobRegisteredOnFromUtc To", gcPK, new TCFilterTest(new ReportParams { JobRegisteredOnFromUtc = new DateTime(2020, 06, 02), JobRegisteredOnToUtc = new DateTime(2020, 06, 03) }, 8));

			RunTest("Filter EntrySubmittedDateFrom", gcPK, new TCFilterTest(new ReportParams { EntrySubmittedDateFrom = new DateTime(2020, 06, 02) }, 16));
			RunTest("Filter EntrySubmittedDateTo", gcPK, new TCFilterTest(new ReportParams { EntrySubmittedDateTo = new DateTime(2020, 06, 03) }, 16));
			RunTest("Filter EntrySubmittedDateFrom To", gcPK, new TCFilterTest(new ReportParams { EntrySubmittedDateFrom = new DateTime(2020, 06, 02), EntrySubmittedDateTo = new DateTime(2020, 06, 03) }, 8));

			RunTest("Filter AssessmentDateFrom", gcPK, new TCFilterTest(new ReportParams { AssessmentDateFrom = new DateTime(2020, 06, 02) }, 16));
			RunTest("Filter AssessmentDateTo", gcPK, new TCFilterTest(new ReportParams { AssessmentDateTo = new DateTime(2020, 06, 03) }, 16));
			RunTest("Filter AssessmentDateFrom To", gcPK, new TCFilterTest(new ReportParams { AssessmentDateFrom = new DateTime(2020, 06, 02), AssessmentDateTo = new DateTime(2020, 06, 03) }, 8));

			RunTest("Filter EntryReleaseDateFrom", gcPK, new TCFilterTest(new ReportParams { EntryReleaseDateFrom = new DateTime(2020, 06, 02) }, 16));
			RunTest("Filter EntryReleaseDateTo", gcPK, new TCFilterTest(new ReportParams { EntryReleaseDateTo = new DateTime(2020, 06, 03) }, 16));
			RunTest("Filter EntryReleaseDateFrom To", gcPK, new TCFilterTest(new ReportParams { EntryReleaseDateFrom = new DateTime(2020, 06, 02), EntryReleaseDateTo = new DateTime(2020, 06, 03) }, 8));

			RunTest("Filter ExpiryDateFrom", gcPK, new TCFilterTest(new ReportParams { ExpiryDateFrom = new DateTime(2020, 06, 02) }, 16));
			RunTest("Filter ExpiryDateTo", gcPK, new TCFilterTest(new ReportParams { ExpiryDateTo = new DateTime(2020, 06, 03) }, 16));
			RunTest("Filter ExpiryDateFrom To", gcPK, new TCFilterTest(new ReportParams { ExpiryDateFrom = new DateTime(2020, 06, 02), ExpiryDateTo = new DateTime(2020, 06, 03) }, 8));

			RunTest("Filter LiquidationDateFrom", gcPK, new TCFilterTest(new ReportParams { LiquidationDateFrom = new DateTime(2020, 06, 02) }, 16));
			RunTest("Filter LiquidationDateTo", gcPK, new TCFilterTest(new ReportParams { LiquidationDateTo = new DateTime(2020, 06, 03) }, 16));
			RunTest("Filter LiquidationDateFrom To", gcPK, new TCFilterTest(new ReportParams { LiquidationDateFrom = new DateTime(2020, 06, 02), LiquidationDateTo = new DateTime(2020, 06, 03) }, 8));
		}

		void RunTest(string testName, Guid currentCompany, RepTestCase testCase)
		{
			RunTestAndAssert(testName, currentCompany, testCase, DBNull.Value);
			RunTestAndAssert(testName, currentCompany, testCase, defaultDateTimeValueWhenReportParameterIsEmptyString);
		}

		void RunTestAndAssert(string testName, Guid currentCompany, RepTestCase testCase, object emptyDateTimeValue)
		{
			var sql = @"SELECT * FROM Report_ZAProvisionalPayment(
						@CompanyPK,
						@ShipmentType,
						@TransportMode,
						@CustomsOffice,
						@ProcedureCode,
						@EntryStatus,
						@ProvisionalPaymentType,
						@JobRegisteredOnFromUtc,
						@JobRegisteredOnToUtc,
						@EntrySubmittedDateFrom,
						@EntrySubmittedDateTo,
						@AssessmentDateFrom,
						@AssessmentDateTo,
						@EntryReleaseDateFrom,
						@EntryReleaseDateTo,
						@ExpiryDateFrom,
						@ExpiryDateTo,
						@LiquidationDateFrom,
						@LiquidationDateTo)";

			using (var command = Db.Connection.Command(sql))
			{
				var rp = testCase.Parameters;
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, currentCompany);

				if (!string.IsNullOrEmpty(rp.ShipmentType)) { command.AddParameter("@ShipmentType", SqlDbType.VarChar, rp.ShipmentType); } else { command.AddParameter("@ShipmentType", SqlDbType.VarChar, DBNull.Value); }
				if (!string.IsNullOrEmpty(rp.TransportMode)) { command.AddParameter("@TransportMode", SqlDbType.VarChar, rp.TransportMode); } else { command.AddParameter("@TransportMode", SqlDbType.VarChar, DBNull.Value); }
				if (!string.IsNullOrEmpty(rp.CustomsOffice)) { command.AddParameter("@CustomsOffice", SqlDbType.VarChar, rp.CustomsOffice); } else { command.AddParameter("@CustomsOffice", SqlDbType.VarChar, DBNull.Value); }
				if (!string.IsNullOrEmpty(rp.ProcedureCode)) { command.AddParameter("@ProcedureCode", SqlDbType.VarChar, rp.ProcedureCode); } else { command.AddParameter("@ProcedureCode", SqlDbType.VarChar, DBNull.Value); }
				if (!string.IsNullOrEmpty(rp.EntryStatus)) { command.AddParameter("@EntryStatus", SqlDbType.VarChar, rp.EntryStatus); } else { command.AddParameter("@EntryStatus", SqlDbType.VarChar, DBNull.Value); }
				if (!string.IsNullOrEmpty(rp.ProvisionalPaymentType)) { command.AddParameter("@ProvisionalPaymentType", SqlDbType.VarChar, rp.ProvisionalPaymentType); } else { command.AddParameter("@ProvisionalPaymentType", SqlDbType.VarChar, DBNull.Value); }

				if (rp.JobRegisteredOnFromUtc.HasValue) { command.AddParameter("@JobRegisteredOnFromUtc", SqlDbType.SmallDateTime, rp.JobRegisteredOnFromUtc); } else { command.AddParameter("@JobRegisteredOnFromUtc", SqlDbType.SmallDateTime, emptyDateTimeValue); }
				if (rp.JobRegisteredOnToUtc.HasValue) { command.AddParameter("@JobRegisteredOnToUtc", SqlDbType.SmallDateTime, rp.JobRegisteredOnToUtc); } else { command.AddParameter("@JobRegisteredOnToUtc", SqlDbType.SmallDateTime, emptyDateTimeValue); }
				if (rp.EntrySubmittedDateFrom.HasValue) { command.AddParameter("@EntrySubmittedDateFrom", SqlDbType.SmallDateTime, rp.EntrySubmittedDateFrom); } else { command.AddParameter("@EntrySubmittedDateFrom", SqlDbType.SmallDateTime, emptyDateTimeValue); }
				if (rp.EntrySubmittedDateTo.HasValue) { command.AddParameter("@EntrySubmittedDateTo", SqlDbType.SmallDateTime, rp.EntrySubmittedDateTo); } else { command.AddParameter("@EntrySubmittedDateTo", SqlDbType.SmallDateTime, emptyDateTimeValue); }
				if (rp.AssessmentDateFrom.HasValue) { command.AddParameter("@AssessmentDateFrom", SqlDbType.SmallDateTime, rp.AssessmentDateFrom); } else { command.AddParameter("@AssessmentDateFrom", SqlDbType.SmallDateTime, emptyDateTimeValue); }
				if (rp.AssessmentDateTo.HasValue) { command.AddParameter("@AssessmentDateTo", SqlDbType.SmallDateTime, rp.AssessmentDateTo); } else { command.AddParameter("@AssessmentDateTo", SqlDbType.SmallDateTime, emptyDateTimeValue); }
				if (rp.EntryReleaseDateFrom.HasValue) { command.AddParameter("@EntryReleaseDateFrom", SqlDbType.SmallDateTime, rp.EntryReleaseDateFrom); } else { command.AddParameter("@EntryReleaseDateFrom", SqlDbType.SmallDateTime, emptyDateTimeValue); }
				if (rp.EntryReleaseDateTo.HasValue) { command.AddParameter("@EntryReleaseDateTo", SqlDbType.SmallDateTime, rp.EntryReleaseDateTo); } else { command.AddParameter("@EntryReleaseDateTo", SqlDbType.SmallDateTime, emptyDateTimeValue); }
				if (rp.ExpiryDateFrom.HasValue) { command.AddParameter("@ExpiryDateFrom", SqlDbType.SmallDateTime, rp.ExpiryDateFrom); } else { command.AddParameter("@ExpiryDateFrom", SqlDbType.SmallDateTime, emptyDateTimeValue); }
				if (rp.ExpiryDateTo.HasValue) { command.AddParameter("@ExpiryDateTo", SqlDbType.SmallDateTime, rp.ExpiryDateTo); } else { command.AddParameter("@ExpiryDateTo", SqlDbType.SmallDateTime, emptyDateTimeValue); }
				if (rp.LiquidationDateFrom.HasValue) { command.AddParameter("@LiquidationDateFrom", SqlDbType.SmallDateTime, rp.LiquidationDateFrom); } else { command.AddParameter("@LiquidationDateFrom", SqlDbType.SmallDateTime, emptyDateTimeValue); }
				if (rp.LiquidationDateTo.HasValue) { command.AddParameter("@LiquidationDateTo", SqlDbType.SmallDateTime, rp.LiquidationDateTo); } else { command.AddParameter("@LiquidationDateTo", SqlDbType.SmallDateTime, emptyDateTimeValue); }

				using (var reader = command.ExecuteReader())
				{
					testCase.RunTestAssertions(testName, reader);
				}
			}
		}

		readonly DateTime defaultDateTimeValueWhenReportParameterIsEmptyString = new DateTime(1900, 1, 1);

		#region Data Prep
		Guid PrepareData()
		{
			TestDataCreator.CreateRefDbDataGroupingCodeTypeAndListItems("ZA", "CSTA", ("PEN", "Pending"), ("CLR", "Clear"));

			var companyPK1 = TestDataCreator.CreateCompany($"TC1", "ZA", "ZAR");
			var companyPK2 = TestDataCreator.CreateCompany($"TC2", "ZA", "ZAR");

			var branchPK1 = CreateBranch(1, companyPK1);
			var branchPK2 = CreateBranch(2, companyPK2);

			var importerPK = TestDataCreator.CreateOrganisation("MDORG001", "MD001");
			var supplierPK = TestDataCreator.CreateOrganisation("MDORG002", "MD002");

			CreateDetails(1, branchPK1, companyPK1, importerPK, supplierPK, 1);
			CreateDetails(2, branchPK1, companyPK1, importerPK, supplierPK, 2);
			CreateDetails(3, branchPK1, companyPK1, importerPK, supplierPK, 3);
			CreateDetails(4, branchPK2, companyPK2, importerPK, supplierPK, 4);

			return companyPK1;
		}

		Guid CreateBranch(int i, Guid companyPK)
		{
			var branchPK = Guid.NewGuid();
			var sql = $@"INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_RL_NKHomePort, GB_GC, GB_RN_NKCountryCode) VALUES (@branchPK, 'TB{i}', 'ZAJNB', @companyPK, 'ZA')";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.ExecuteNonQuery();
			}

			return branchPK;
		}

		void CreateDetails(int i, Guid branchPK, Guid companyPK, Guid importerPK, Guid supplierPK, int clusterKey)
		{
			var code = i.ToString("000");

			var shipType = (i % 2 == 0 ? "IMP" : "EXP");

			var dt = new DateTime(2020, 06, 01).AddDays(i - 1);
			var decPK = TestDataCreator.CreateJobDeclaration($"JB{code}", branchPK, companyPK, "BLT", importerPK, supplierPK, (i % 2 == 0 ? "JHB" : "DUR"), $"MB{code}", "ZADUR", "GBLON", (i % 2 == 0 ? "SEA" : "AIR"), $"VES{code}", $"HB{code}", dt, shipType, clusterKey);
			var ceiPK = TestDataCreator.CreateCusEntryInstruction(decPK, code, $"CEI {code}", dt, clusterKey, (i % 2 == 0 ? "CustomsOfficeOverride=" : "CustomsOfficeOverride=CTN"));
			var chPK1 = TestDataCreator.CreateCusEntryHeader(true, shipType, code, code, $"BGM{code}", 0, 1, string.Empty, decPK, dt, dt, ceiPK, string.Empty, dt, dt, dt, clusterKey);
			TestDataCreator.CreateCusEntryNum(chPK1, "CusEntryHeader", $"MRN{code}", "MRN", string.Empty, "ZA");
			var chPK2 = TestDataCreator.CreateCusEntryHeader(true, shipType, code, code, $"BGM{code}", 0, 1, string.Empty, decPK, dt, dt, ceiPK, string.Empty, dt, dt, dt, clusterKey);
			TestDataCreator.CreateCusEntryNum(chPK2, "CusEntryHeader", $"MRN{code}", "MRN", string.Empty, "ZA");

			var pNum = 0;
			foreach (var pt in new[] { "PPA", "PPC", "PPG", "PPT", "PPR", "PPE", "PEN", "FOR", "XXX" })
			{
				pNum++;
				TestDataCreator.CreateCusEntryPayInfo((100 * i + pNum).ToString(), 100.0m * i + pNum, pt, dt, $"PAY{code}", "C", (pNum % 2 == 0 ? "PEN" : "CLR"), (i % 2 == 0 ? chPK1 : chPK2), dt, clusterKey);
			}

			pNum++;
			TestDataCreator.CreateCusEntryPayInfo((100 * i + pNum).ToString(), 100.0m * i + pNum, "PEN", dt, $"PAY{code}", "F", (pNum % 2 == 0 ? "PEN" : "CLR"), chPK1, dt, clusterKey);
			pNum++;
			TestDataCreator.CreateCusEntryPayInfo((100 * i + pNum).ToString(), 100.0m * i + pNum, "PEN", dt, "", "C", (pNum % 2 == 0 ? "PEN" : "CLR"), chPK2, dt, clusterKey);
		}
		#endregion
	}
}

