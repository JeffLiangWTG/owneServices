using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ReportTesting;

namespace Enterprise.Customs.GB.Business.ReportTesting
{
	class Report_GBCustomsEntryPayment : ReportFunctionalTestCase
	{
		public void TestReportWithConfirmedFees()
		{
			PrepareTestData();

			var data = FireReportAndReturnDataTable();
			AssertTestResults(data);
		}

		public void TestReportWithFeesWithLandedCost()
		{
			PrepareTestData();

			hideLandedCostOnly = false;
			var data = FireReportAndReturnDataTable();
			AssertTestResults(data);

			hideLandedCostOnly = true;
			data = FireReportAndReturnDataTable();
			AssertTestResults(data);
		}

		protected override ZString ObjectName => "Report_GBCustomsEntryPayment";

		protected override List<ReportSchemaColumn> ExpectedColumnsInAnyOrder
		{
			get
			{
				var allCols = new List<ReportSchemaColumn>();
				foreach (var stringCol in new string[] { "JE_MessageType", "JE_TransportMode" })
				{
					allCols.Add(new ReportSchemaColumn(typeof(string), stringCol));
				}

				foreach (var decimalCol in new string[] { "A00", "A10", "A20", "A30", "A35", "A40", "A45", "B00", "B10", "B20", "D00", "D10" })
				{
					allCols.Add(new ReportSchemaColumn(typeof(decimal), decimalCol));
				}

				return allCols;
			}
		}

		protected override SqlObjectType SqlObjectType => Enterprise.ReportTesting.SqlObjectType.FunctionTable;

		protected override void AssertTestResults(DataTable resultsOrderedByExpectedColumnNames)
		{
			AssertEquals(1, resultsOrderedByExpectedColumnNames.Rows.Count);
			var row1 = FormatRowsValues(resultsOrderedByExpectedColumnNames.Rows[0], resultsOrderedByExpectedColumnNames);
			AssertContainsMoreHelpfully(hideLandedCostOnly ? "[A00]='200.0000'; [D10]='300.0000'" : "[A00]='200.0000'; [B00]='100.0000'; [D10]='300.0000'; [A40]='400.0000'", row1);
		}

		protected override List<string> ParametersValuesList
		{
			get
			{
				return new List<string>() { "''", string.Format(CultureInfo.InvariantCulture, "'{0}'", GlbBranch.CurrentBranch.Company.PK), QuoteParameter(hideLandedCostOnly) };
			}
		}

		protected override void PrepareTestData()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = "CLR";
			var entryLine = entryHeader.AllEntryLines.AddNew();
			_ = FeeHelper.AddFees(entryLine);

			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_EntryStatus = "CLR";
			var entryLine2 = entryHeader2.AllEntryLines.AddNew();
			_ = FeeHelper.AddFees2(entryLine2);

			Factory.Save();
		}

		protected override bool AllowColumnCountMismatch => true;

		bool hideLandedCostOnly;
	}

	static class FeeHelper
	{
		public static CusEntryLine AddFees(CusEntryLine cusEntryLine)
		{
			cusEntryLine.Fees.AddOrUpdate("B00", 100M, true);
			cusEntryLine.ConfirmedFees.AddOrUpdate("B00", 60M);
			cusEntryLine.ConfirmedFees.AddOrUpdate("A00", 200M);
			cusEntryLine.ConfirmedFees.AddOrUpdate("D10", 300M);
			cusEntryLine.ConfirmedFees.AddOrUpdate("A40", 400M, true);
			return cusEntryLine;
		}

		public static CusEntryLine AddFees2(CusEntryLine cusEntryLine)
		{
			cusEntryLine.Fees.AddOrUpdate("B00", 40M);
			return cusEntryLine;
		}
	}
}
