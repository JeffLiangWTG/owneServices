using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class MergeOnAddInfoValueTestCase : TestCaseWithFactory
	{
		protected abstract void SetPropertyWithValue1(JobComInvoiceLine line);
		protected abstract void SetPropertyWithValue2(JobComInvoiceLine line);

		public void TestCanMergeInvoiceLinesWithSameProperty()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0000.00.00 00";
			SetPropertyWithValue1(invoiceLine1);

			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0000.00.00 00";
			SetPropertyWithValue1(invoiceLine2);

			LineMerger merger = new LineMerger(testDec);

			merger.DoMerge();
			AssertEquals("Merged", false, invoiceLine1.JI_CL.IsEmpty);
			AssertEquals("Merged", false, invoiceLine2.JI_CL.IsEmpty);
			AssertEquals("One merged line", invoiceLine1.JI_CL, invoiceLine2.JI_CL);
			AssertEquals("1 Merged lines", 1, testDec.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("1 header", 1, testDec.CustomsEntryHeaders.Count);
		}

		public void TestCannotMergeInvoiceLinesWithDifferentProperty()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0000.00.00 00";
			SetPropertyWithValue1(invoiceLine1);

			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0000.00.00 00";
			SetPropertyWithValue2(invoiceLine2);

			LineMerger merger = new LineMerger(testDec);

			merger.DoMerge();
			AssertEquals("Merged", false, invoiceLine1.JI_CL.IsEmpty);
			AssertEquals("Merged", false, invoiceLine2.JI_CL.IsEmpty);
			Assert("Two merged line", invoiceLine1.JI_CL != invoiceLine2.JI_CL);
			AssertEquals("2 Merged lines", 2, testDec.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("1 header", 1, testDec.CustomsEntryHeaders.Count);
		}
	}
}
