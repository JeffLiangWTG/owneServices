using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	public class InvoiceSummaryTest : ScriptTest
	{
		[SuspendCriticalValidation]
		public void TestNoExceptionThrow_WhenSumLineAmount()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = Creator.CreateJob(shipment);
			var arInvoice = Creator.CreateARInvoice<ARInvoice>("AR100001", Creator.AUD, 1.0m, Creator.Debtor);
			arInvoice.AH_JH = job.PK;

			Creator.CC1.AC_ChargeGroup = ChargeCodeGroupList.Codes.NotGrouped;
			Creator.CC2.AC_ChargeGroup = ChargeCodeGroupList.Codes.Destination;

			var line1 = Creator.CreateARInvoiceLineWithJobCharge(arInvoice, job, Creator.CC1, Creator.AUD, 1.0m, "AR Line 001", 1m, creator.FREEVAT.PK);
			var line2 = Creator.CreateARInvoiceLineWithJobCharge(arInvoice, job, Creator.CC2, Creator.AUD, 1.0m, "AR Line 002", 1m, creator.FREEVAT.PK);
			var line3 = Creator.CreateARInvoiceLineWithJobCharge(arInvoice, job, Creator.CC1, Creator.AUD, 1.0m, "AR Line 003", 1m, creator.FREEVAT.PK);

			line1.AL_OSExTaxAmount = 922337203685477.00m;
			line2.AL_OSExTaxAmount = -922337203685477.00m;
			line3.AL_OSExTaxAmount = 2.00m;
			job.Charges.OfType<JobCharge>().ForEach(x => x.SetAmountsToLinkedLinesForTests());

			Factory.Save();

			var resultTable = RunScript(arInvoice.AH_InvoiceDate.AddDays(-1), arInvoice.AH_InvoiceDate.AddDays(1));
			AssertData(resultTable, new Dictionary<string, decimal>()
			{
				{ "NGC",  922337203685479.00m },
				{ "DST", -922337203685477.00m },
			});
		}
		public void TestInvoiceDateWithinLatestValueInTheRangeOneInvoiceReturned()
		{
			var fromDate = new ZDateTime(2023, 03, 01);
			var toDate = new ZDateTime(2023, 03, 31); //This toDate value is set by the Macro ToDate
			var invoiceDate = new ZDateTime(2023, 03, 31, 23, 59, 00);

			var resultTable = GetTestInvoiceSummaryReportRecords(invoiceDate, fromDate, toDate);
			AssertEquals("One row of data returned", 1, resultTable.Rows.Count);
		}

		public void TestInvoiceDateOutOfTheRangeNoInvoiceReturned()
		{
			var fromDate = new ZDateTime(2023, 04, 01);
			var toDate = new ZDateTime(2023, 04, 30); //This toDate value is set by the Macro ToDate
			var invoiceDate = new ZDateTime(2023, 05, 01);

			var resultTable = GetTestInvoiceSummaryReportRecords(invoiceDate, fromDate, toDate);
			AssertEquals("No row of data returned", 0, resultTable.Rows.Count);
		}

		DataTable GetTestInvoiceSummaryReportRecords(ZDateTime invoiceDate, ZDateTime fromDate, ZDateTime toDate)
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = Creator.CreateJob(shipment);
			var arInvoice = Creator.CreateARInvoice<ARInvoice>("AR100001", Creator.AUD, 1.0m, Creator.Debtor);
			arInvoice.AH_JH = job.PK;

			arInvoice.AH_InvoiceDate = invoiceDate;

			Creator.CreateARInvoiceLineWithJobCharge(arInvoice, job, Creator.CC1, Creator.AUD, 1.0m, "AR Line 001", 1m, creator.FREEVAT.PK);

			Factory.Save();

			return RunScript(fromDate, toDate);
		}

		void AssertData(DataTable table, Dictionary<string, decimal> expectedValues)
		{
			AssertEquals("One row of data returned", 1, table.Rows.Count);
			var row = table.Rows[0];

			foreach (var pair in expectedValues)
			{
				var testMessage = string.Format("Testing column {0} set to correct value {1}.", pair.Key, pair.Value);
				AssertEquals(testMessage, pair.Value, row[pair.Key]);
			}
		}

		DataTable RunScript(ZDateTime dateFrom, ZDateTime dateTo)
		{
			string sql = string.Format(@"Select * From InvoiceSummary (
			NULL, --@ConsigneePK
			NULL, --@ConsignorPK
			NULL, --@DebtorPK
			'', --@TransportMode
			NULL, --@Dest
			NULL, --@Origin
			'{0}', --@DateFrom
			'{1}', --@DateTo
			'', --@TransactionType
			'{2}', --@CurrentBranchCode
			'' --@Quantity
			)",
			 GetMinDateTimeString(dateFrom),
			 GetMaxDateTimeString(dateTo),
			 GlbBranch.CurrentBranch.GB_Code);

			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}

		protected TestObjectCreator Creator
		{
			get	{ return creator ?? (creator = new TestObjectCreator(Factory)); }
		}

		TestObjectCreator creator;
	}
}
