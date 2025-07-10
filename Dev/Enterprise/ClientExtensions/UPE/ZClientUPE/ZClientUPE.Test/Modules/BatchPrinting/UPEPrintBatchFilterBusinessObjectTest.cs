using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Client.UPE.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module.Testing
{
	[TestedType(typeof(UPEPrintBatchFilterBusinessObject))]
	sealed class UPEPrintBatchFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestPrintBatchType()
		{
			UPEPrintBatchItem printItem1 = CreatePrintBatchItem(1, UPEPrintBatchTypes.Codes.TaxInvoice);
			UPEPrintBatchItem printItem2 = CreatePrintBatchItem(2, UPEPrintBatchTypes.Codes.ShipmentHeldLetter);
			FilterBizObj["Document Type"].IsActive = true;
			((ModuleTextFilter)FilterBizObj["Document Type"]).Property = UPEPrintBatchTypes.Codes.TaxInvoice;
			AssertFilterMatches("Filter by TaxInvoice", FilterBizObj.Filter, 1);
			((ModuleTextFilter)FilterBizObj["Document Type"]).Property = UPEPrintBatchTypes.Codes.ShipmentHeldLetter;
			AssertFilterMatches("Filter by ShipmentHeldLetter", FilterBizObj.Filter, 2);
		}

		public void TestBatchNumber()
		{
			CreatePrintBatchItem(1);
			CreatePrintBatchItem(2);
			Factory.Save();
			FilterBizObj["Batch Number"].IsActive = true;
			((ModuleNumberFilter)FilterBizObj["Batch Number"]).Property = "0";
			AssertFilterMatches("All callout items should match initially", FilterBizObj.Filter, 1, 2);
			((ModuleNumberFilter)FilterBizObj["Batch Number"]).Property = "1";
			AssertFilterMatches("Only callout items with their invoice never printed should match", FilterBizObj.Filter, 1);
		}

		public void TestBatchNumber_ExcludesOtherFilters()
		{
			CreatePrintBatchItem(1);
			CreatePrintBatchItem(2);
			Factory.Save();
			((ModuleTextFilter)FilterBizObj["Print Status"]).IsActive = true;
			((ModuleTextFilter)FilterBizObj["Print Status"]).Property = "Failed";
			FilterBizObj["Batch Number"].IsActive = true;
			((ModuleNumberFilter)FilterBizObj["Batch Number"]).Property = "1";
			AssertFilterMatches("Callout item with batch number 1 should still match even though there are other filter criteria", FilterBizObj.Filter, 1);
		}

		public void TestUnprinted()
		{
			UPEPrintBatchItem item1 = CreatePrintBatchItem(1);
			item1.PrintBatch.T7_LastPrintedDate = new ZDateTime(2005, 1, 1);
			UPEPrintBatchItem item2 = CreatePrintBatchItem(1);
			item2.PrintBatch.T7_LastPrintedDate = ZDateTime.Empty;
			UPEPrintBatchItem decoyItem = CreatePrintBatchItem(2);
			decoyItem.PrintBatch.T7_LastPrintedDate = new ZDateTime(2005, 1, 1);
			Factory.Save();
			AssertFilterMatches("All print batch items should match initially", FilterBizObj.Filter, 1, 2);
			((ModuleTextFilter)FilterBizObj["Print Status"]).IsActive = true;
			((ModuleTextFilter)FilterBizObj["Print Status"]).Property = "Unprinted";
			AssertFilterMatches("Only print batch items with their invoice never printed should match", FilterBizObj.Filter, 1);
		}

		public void TestFailed()
		{
			UPEPrintBatchItem item = CreatePrintBatchItem(1);
			item.PrintBatch.T7_LastPrintedDate = ZDateTime.Now.AddDays(-1); // printed before it failed
			item.Parent.GetLogs().AddNew(Events.DocumentNotDelivered, UPEDocumentMenuItemLoader.UPSTaxInvoiceMenuName);
			UPEPrintBatchItem decoyItem1 = CreatePrintBatchItem(2);
			decoyItem1.Logs.AddNew(Events.DocumentNotDelivered, "some other document");
			UPEPrintBatchItem decoyItem2 = CreatePrintBatchItem(3);
			decoyItem2.Logs.AddNew(Events.DocumentNotDelivered, UPEDocumentMenuItemLoader.UPSTaxInvoiceMenuName);
			decoyItem2.PrintBatch.T7_LastPrintedDate = ZDateTime.Now.AddDays(1); // re-printed
			Factory.Save();
			AssertFilterMatches("All callout items should match initially", FilterBizObj.Filter, 1, 2, 3);
			((ModuleTextFilter)FilterBizObj["Print Status"]).IsActive = true;
			((ModuleTextFilter)FilterBizObj["Print Status"]).Property = "Failed";
			AssertFilterMatches("Only callout items that have failed to print should match", FilterBizObj.Filter, 1);
		}

		void AssertFilterMatches(string message, ZQuery filter, params ZInt[] expectedMatchingBatchNumbers)
		{
			UPEPrintBatchCollection collection = new UPEPrintBatchCollection(Factory);
			collection.Load(filter);
			AssertEquals(message + "; Expected number of batches to match", expectedMatchingBatchNumbers.Length, collection.Count);
			for (int i = 0; i < collection.Count; i++)
			{
				AssertCollectionContains(message, collection[i].T7_BatchNumber, expectedMatchingBatchNumbers);
			}
		}

		#region Implementation
		UPEPrintBatchItem CreatePrintBatchItem(int batchNumber)
		{
			return CreatePrintBatchItem(batchNumber, UPEPrintBatchTypes.Codes.TaxInvoice);
		}

		UPEPrintBatchItem CreatePrintBatchItem(int batchNumber, ZString printBatchType)
		{
			UPEPrintBatch printBatch = FindOrCreatePrintBatch(batchNumber, printBatchType);
			Callout bizObj = Factory.NewWithValidTestData<Callout>();
			UPEPrintBatchItem result = printBatch.PrintItems.AddNew(new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoice(), bizObj);
			return result;
		}

		UPEPrintBatch FindOrCreatePrintBatch(int batchNumber, ZString batchType)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(ClientPrintBatchSchema.T7_BatchType, UPEPrintBatchTypes.Codes.TaxInvoice);
			filter.AddToFilter(ClientPrintBatchSchema.T7_BatchNumber, batchNumber);
			UPEPrintBatch printBatch = Factory.LoadTop1<UPEPrintBatch>(filter);
			if (printBatch == null)
			{
				printBatch = Factory.New<UPEPrintBatch>();
				printBatch.T7_BatchType = batchType;
				printBatch.T7_BatchNumber = batchNumber;
			}

			return printBatch;
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new UPEPrintBatchFilterBusinessObject();
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			FilterBizObj = new UPEPrintBatchFilterBusinessObject();
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
		}

		UPEPrintBatchFilterBusinessObject FilterBizObj;
		#endregion
	}
}
