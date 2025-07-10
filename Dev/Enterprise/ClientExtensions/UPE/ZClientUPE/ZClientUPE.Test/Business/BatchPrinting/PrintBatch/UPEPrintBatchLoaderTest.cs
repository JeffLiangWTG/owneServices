using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business
{
	[TestedType(typeof(UPEPrintBatch.Loader))]
	class UPEPrintBatchLoaderTest : LoaderTestCase
	{
		public void TestCreateOrLoadLatestBatch_IncrementBatchNumberAfterMaximumNumberOfPrintItemsReached()
		{
			UPEPrintBatch createdPrintBatch1 = Loader.CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.TaxInvoice);
			AssertEquals("Should create the 1st print batch for the test", 1, createdPrintBatch1.T7_BatchNumber);
			for (int i = 0; i < 199; i++)
			{
				createdPrintBatch1.PrintItems.AddNew();
			}

			UPEPrintBatch loadedPrintBatch1 = Loader.CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.TaxInvoice);
			AssertEquals("Should load the 1st print batch as we have not reached the maximum", 1, loadedPrintBatch1.T7_BatchNumber);
			loadedPrintBatch1.PrintItems.AddNew();
			UPEPrintBatch createdPrintBatch2 = Loader.CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.TaxInvoice);
			AssertEquals("Should create the 2nd print batch as we have reached the maximum number of items for the batch", 2, createdPrintBatch2.T7_BatchNumber);
			UPEPrintBatch loadedPrintBatch2 = Loader.CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.TaxInvoice);
			AssertEquals("Should load the 2nd print batch as it is the latest", 2, loadedPrintBatch2.T7_BatchNumber);
			UPEPrintBatch differentPrintBatch = Loader.CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.ShipmentHeldLetter);
			AssertEquals("The print batch number of the different print batch should still be at 1", 1, differentPrintBatch.T7_BatchNumber);
		}

		public void TestCreateOrLoadLatestBatch_IncrementBatchNumberAfterCurrentBatchPrinted()
		{
			TestUPEPrintBatch initialPrintBatch = (TestUPEPrintBatch)Loader.CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.TaxInvoice);
			AssertEquals("Should create the 1st print batch for the test", 1, initialPrintBatch.T7_BatchNumber);
			initialPrintBatch.Print();
			UPEPrintBatch secondPrintBatch = (TestUPEPrintBatch)Loader.CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.TaxInvoice);
			AssertEquals("After the first batch is printed, the batch number should be incremented", 2, secondPrintBatch.T7_BatchNumber);
		}

		#region Implementation
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		TestUPEPrintBatch.Loader Loader
		{
			get
			{
				if (fLoader == null)
				{
					fLoader = new TestUPEPrintBatch.Loader(Factory);
				}

				return fLoader;
			}
		}

		TestUPEPrintBatch.Loader fLoader;
		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new UPEPrintBatch.Loader(Factory);
		}
		#endregion
	}
}
