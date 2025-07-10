using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EntryLineAddInfoLineConstructorTest : TestCaseWithFactory
	{
		public void TestEmptyTILV()
		{
			JobDeclaration testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec.JE_MergeBy = "TRF";

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.AddInfo.ZA_TILV = "0.00AUD";

			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();

			testDec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("AddInfoLine calculated for TILV", true, invoiceLine.CusEntryLine.AddInfoLine.Contains("TILV="));
			AssertEquals("AddInfoLine calculated for TILV", false, invoiceLine2.CusEntryLine.AddInfoLine.Contains("TILV="));

			invoiceLine.AddInfo.ZA_TILV = "10.00AUD";
			invoiceLine.CusEntryLine.ResetTotalsAndCachedValues();
			invoiceLine.CusEntryLine.EntryLineAddInfo.ZA_TILV = "20.00AUD";
			AssertEquals("AddInfoLine calculated for TILV", true, invoiceLine.CusEntryLine.AddInfoLine.Contains("TILV=20AUD"));
		}

		public void TestAddInfoLineForFieldsWithCurrency()
		{
			AssertAddInfoLineForMoney(AUAddInfoSchema.ZA_TILV);
			AssertAddInfoLineForMoney(AUAddInfoSchema.ZA_ADJ);
			AssertAddInfoLineForMoney(AUAddInfoSchema.ZA_DXP);
		}

		public void TestAddInfoLineForValue()
		{
			AssertAddInfoLineForValue(AUAddInfoSchema.ZA_DTY);
			AssertAddInfoLineForValue(AUAddInfoSchema.ZA_STD);
			AssertAddInfoLineForValue(AUAddInfoSchema.ZA_WET);
			AssertAddInfoLineForValue(AUAddInfoSchema.ZA_ODF);
			AssertAddInfoLineForValue(AUAddInfoSchema.ZA_QT2);
			AssertAddInfoLineForValue(AUAddInfoSchema.ZA_WRQ);
		}

		public void TestAddInfoLineForDumpingDuty()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_MergeBy = "TRF";

			var invoice = testDec.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "AUD";
			invoice.Charges.AddNew("OFT", 50m, "AUD");

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.AddInfo.ZA_DMP = 250m;
			invoiceLine.AddInfo.ZA_RNO = "001";
			invoiceLine.AddInfo.ZA_ORG = "DE";

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 10000m;
			invoiceLine2.AddInfo.ZA_DMP = 150m;
			invoiceLine2.AddInfo.ZA_RNO = "001";
			invoiceLine2.AddInfo.ZA_ORG = "DE";

			LineMerger merger = new LineMerger(testDec);
			merger.DoMerge();

			AssertEquals("There should be one entry header", 1, testDec.CustomsEntryHeaders.Count);
			AssertEquals("There should be one entry line", 1, testDec.CustomsEntryHeaders[0].MergedLines.Count);

			CusEntryLine entryLine = testDec.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("AddInfoLine calculated for dumping duty", true, entryLine.AddInfoLine.Contains("DMP=400"));
			AssertEquals("AddInfoLine ordered and include other addinfo pairs, TILV should be printed only if necessary", "DMP=400*ORG=DE*RNO=001", entryLine.AddInfoLine);

			invoiceLine2.AddInfo.ZA_DMP = 150.50m;
			merger.DoMerge();//should have refreshed the calculation
			entryLine = testDec.CustomsEntryHeaders[0].MergedLines[0];
			AssertEquals("AddInfoLine calculated for dumping duty", true, entryLine.AddInfoLine.Contains("DMP=400.50"));
			AssertEquals("AddInfoLine ordered and include other addinfo pairs", "DMP=400.50*ORG=DE*RNO=001", entryLine.AddInfoLine);
		}

		JobDeclaration testDec;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine1;
		JobComInvoiceLine invoiceLine2;

		protected override void SetUp()
		{
			base.SetUp();
			testDec = Factory.New<JobDeclaration>();
			entryHeader = testDec.CustomsEntryHeaders.AddNew();
			entryLine = entryHeader.MergedLines.AddNew();
			invoice = testDec.Invoices.AddNew();
			invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;
		}

		void AssertAddInfoLineForValue(SchemaDecimalColumn addInfoFieldName)
		{
			invoiceLine1.AddInfo[addInfoFieldName.Name] = 120m;
			invoiceLine2.AddInfo[addInfoFieldName.Name] = 250m;
			entryLine.ResetTotalsAndCachedValues();
			AssertEquals("AddInfoLine calculated", true, entryLine.AddInfoLine.Contains(addInfoFieldName.Name.Substring(3) + "=370"));

			invoiceLine1.AddInfo[addInfoFieldName.Name] = 0m;
			invoiceLine2.AddInfo[addInfoFieldName] = 250.50m;
			entryLine.ResetTotalsAndCachedValues();//should refresh the calculation
			entryLine.InvoiceLines.Sort(JobComInvoiceLineSchema.JI_LineNo.Name, System.ComponentModel.ListSortDirection.Ascending);
			AssertEquals("PreCondition", invoiceLine1, entryLine.RandomLine);
			AssertEquals("AddInfoLine calculated", true, entryLine.AddInfoLine.Contains(addInfoFieldName.Name.Substring(3) + "=250.50"));
		}

		void AssertAddInfoLineForMoney(SchemaStringColumn addInfoFieldName)
		{
			invoiceLine1.AddInfo[addInfoFieldName.Name] = "120USD";
			invoiceLine2.AddInfo[addInfoFieldName.Name] = "250USD";
			entryLine.ResetTotalsAndCachedValues();
			AssertEquals("AddInfoLine calculated", true, entryLine.AddInfoLine.Contains(addInfoFieldName.Name.Substring(3) + "=370USD"));

			invoiceLine1.AddInfo[addInfoFieldName.Name] = "";
			invoiceLine2.AddInfo[addInfoFieldName] = "250.50USD";
			entryLine.ResetTotalsAndCachedValues();//should refresh the calculation
			entryLine.InvoiceLines.Sort(JobComInvoiceLineSchema.JI_LineNo.Name, System.ComponentModel.ListSortDirection.Ascending);
			AssertEquals("PreCondition", invoiceLine1, entryLine.RandomLine);
			AssertEquals("Should still print", true, entryLine.AddInfoLine.Contains(addInfoFieldName.Name.Substring(3) + "=250.50USD"));
		}
	}
}
