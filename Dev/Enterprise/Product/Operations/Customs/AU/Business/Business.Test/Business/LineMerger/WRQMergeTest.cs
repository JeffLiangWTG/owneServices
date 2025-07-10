using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class WRQMergeTest : TestCaseWithFactory
	{
		void SetPropertyWithValue1(AUAddInfo addInfo)
		{
			addInfo.ZA_WRQ = 100m;
		}

		void SetPropertyWithValue2(AUAddInfo addInfo)
		{
			addInfo.ZA_WRQ = 200m;
		}

		public void TestDifferentWRQsGetMergedAndSummed()
		{
			JobDeclaration testDec = JobDeclaration.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			JobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "0000.00.00 00";
			SetPropertyWithValue1(invoiceLine1.AddInfo);

			JobComInvoiceLine invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "0000.00.00 00";
			SetPropertyWithValue2(invoiceLine2.AddInfo);

			LineMerger merger = new LineMerger(testDec);
			CMREntryCreationStrategy strategy = new CMREntryCreationStrategy(merger);

			merger.DoMerge();
			AssertEquals("Merged", false, invoiceLine1.JI_CL.IsEmpty);
			AssertEquals("Merged", false, invoiceLine2.JI_CL.IsEmpty);
			Assert("One merged line", invoiceLine1.JI_CL == invoiceLine2.JI_CL);
			AssertEquals("1 Merged lines", 1, testDec.CustomsEntryHeaders[0].MergedLines.Count);
			AssertEquals("total value", 300m, testDec.CustomsEntryHeaders[0].MergedLines[0].WRQ);
			AssertEquals("1 header", 1, testDec.CustomsEntryHeaders.Count);
		}
	}
}
