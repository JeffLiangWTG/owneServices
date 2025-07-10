using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(UPEPrintBatchItemCollection))]
	sealed class UPEPrintBatchItemCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAddNew_WithCommandAndBizObj()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			Callout callout = Factory.New<Callout>();
			UPEPrintBatch printBatch = Factory.New<UPEPrintBatch>();
			UPEPrintBatchItem item = printBatch.PrintItems.AddNew(new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoice(), callout);
			AssertEquals("T6_SU should be set", new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoice().PK, item.T6_SU);
			AssertEquals("T6_ParentID should be set", callout.PK, item.T6_ParentID);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			UPEPrintBatch printBatch = new UPEPrintBatch.Loader(Factory).CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.TaxInvoice);
			return printBatch.PrintItems;
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}
	}
}
