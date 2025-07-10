using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business.Testing;
using Enterprise.DocumentEngine;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business
{
	[TestedType(typeof(UPEPrintBatchItem.Loader))]
	sealed class UPEPrintBatchLoaderItemTest : LoaderTestCase
	{
		public void TestLoadPrintBatchItem()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			UPECusHAWB parent = Factory.New<UPECusHAWB>();
			UPECusHAWB decoyParent = Factory.New<UPECusHAWB>();
			UPEPrintBatch printBatchForType1 = new UPEPrintBatch.Loader(Factory).CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.TaxInvoice);
			UPEPrintBatch printBatchForType2 = new UPEPrintBatch.Loader(Factory).CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.ShipmentHeldLetter);
			UPEPrintBatchItem printItemForType1 = printBatchForType1.PrintItems.AddNew(TaxInvoiceDocument, parent);
			UPEPrintBatchItem printItemForType2 = printBatchForType2.PrintItems.AddNew(ShipmentHeldLetterDocumentForConsignee, parent);
			UPEPrintBatchItem printItemForType2_DiffDocCommand = printBatchForType2.PrintItems.AddNew(ShipmentHeldLetterDocumentForConsignor, parent);
			UPEPrintBatchItem decoyPrintItemForType1 = printBatchForType1.PrintItems.AddNew(TaxInvoiceDocument, decoyParent);
			UPEPrintBatchItem decoyPrintItemForType2 = printBatchForType2.PrintItems.AddNew(ShipmentHeldLetterDocumentForConsignee, decoyParent);
			UPEPrintBatchItem decoyPrintItemForType2_DiffDocCommand = printBatchForType2.PrintItems.AddNew(ShipmentHeldLetterDocumentForConsignor, decoyParent);
			AssertEquals("LoadPrintBatchItem should load the correct print item", printItemForType1, Loader.LoadPrintBatchItem(UPEPrintBatchTypes.Codes.TaxInvoice, parent.PK, TaxInvoiceDocument.PK));
			AssertEquals("LoadPrintBatchItem should load the correct print item", printItemForType2, Loader.LoadPrintBatchItem(UPEPrintBatchTypes.Codes.ShipmentHeldLetter, parent.PK, ShipmentHeldLetterDocumentForConsignee.PK));
			AssertEquals("LoadPrintBatchItem should load the correct print item", printItemForType2_DiffDocCommand, Loader.LoadPrintBatchItem(UPEPrintBatchTypes.Codes.ShipmentHeldLetter, parent.PK, ShipmentHeldLetterDocumentForConsignor.PK));
		}

		#region DocumentCommands
		DocumentCommand TaxInvoiceDocument
		{
			get
			{
				return new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoice();
			}
		}

		DocumentCommand ShipmentHeldLetterDocumentForConsignee
		{
			get
			{
				return new UPEDocumentMenuItemLoader(Factory).LoadCusHAWBHeldLetter(ShipmentHeldLetterRecipient.Consignee);
			}
		}

		DocumentCommand ShipmentHeldLetterDocumentForConsignor
		{
			get
			{
				return new UPEDocumentMenuItemLoader(Factory).LoadCusHAWBHeldLetter(ShipmentHeldLetterRecipient.Consignor);
			}
		}

		#endregion
		#region Implementation
		UPEPrintBatchItem.Loader Loader
		{
			get
			{
				if (fLoader == null)
				{
					fLoader = new UPEPrintBatchItem.Loader(Factory);
				}

				return fLoader;
			}
		}

		UPEPrintBatchItem.Loader fLoader;

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new UPEPrintBatchItem.Loader(Factory);
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}
		#endregion
	}
}
