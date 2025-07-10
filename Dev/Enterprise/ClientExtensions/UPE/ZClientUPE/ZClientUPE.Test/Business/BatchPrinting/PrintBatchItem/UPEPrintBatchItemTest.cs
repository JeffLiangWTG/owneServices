using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.Testing;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business
{
	[TestedType(typeof(UPEPrintBatchItem))]
	sealed class UPEPrintBatchItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCascadeDeleteItems()
		{
			Callout parent = Factory.New<Callout>();
			UPEPrintBatch printBatch = new UPEPrintBatch.Loader(Factory).CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.TaxInvoice);
			UPEPrintBatchItem printItem = printBatch.PrintItems.AddNew(new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoice(), parent);
			AssertEquals("PrintItem should not be deleted initially for the test", false, printItem.IsDeleted);
			UPEPrintBatchItem.CascadeDeleteItems(parent);
			AssertEquals("All print batch items the business object is attached to should be deleted", true, printItem.IsDeleted);
		}

		public void TestCreateDocumentPack()
		{
			UPECusHAWB airCargo = Factory.New<UPECusHAWB>();
			UPEPrintBatch printBatch = new UPEPrintBatch.Loader(Factory).CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.TaxInvoice);
			UPEPrintBatchItem printItem = printBatch.PrintItems.AddNew(new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoice(), airCargo);
			using (DocumentPack pack = printItem.CreateDocumentPack())
			{
				AssertEquals("Should populate the correct business object", airCargo.PK, pack.DocumentSupporter.PK);
			}
		}

		public void TestPrintItemQueuedMessage()
		{
			Callout parent = Factory.New<Callout>();
			UPEPrintBatch printBatch = new UPEPrintBatch.Loader(Factory).CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.TaxInvoice);
			UPEPrintBatchItem printItem = printBatch.PrintItems.AddNew(new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoice(), parent);
			AssertEquals("The document has been queued for batch print (batch number 1)", printItem.PrintItemQueuedMessage);
		}

		#region Business Object Overrides
		public void TestQueuedByDefaultValue()
		{
			UPEPrintBatchItem item = Factory.New<UPEPrintBatchItem>();
			AssertEquals("T6_GS_QueuedBy should default from the current user", GlbStaff.CurrentUser.PK, item.T6_GS_QueuedBy);
		}

		#endregion
		#region Related Business Objects
		public void TestPrintBatch()
		{
			UPEPrintBatch printBatch = Factory.New<UPEPrintBatch>();
			UPEPrintBatchItem item = printBatch.PrintItems.AddNew();
			AssertEquals("PrintBatch should return the correct object", printBatch.PK, item.PrintBatch.PK);
		}

		public void TestParent_ForTaxInvoice()
		{
			TestParent("TaxInvoice requires a Callout", new UPEDocumentMenuItemLoader(Factory).LoadUPSTaxInvoice(), typeof(Callout));
		}

		public void TestParent_ForAlternateBrokerSplitNotification()
		{
			TestParent("AlternateBrokerSplitNotification requires a UPECusHAWB", new UPEDocumentMenuItemLoader(Factory).LoadAlternateBrokerSplitNotification(), typeof(UPECusHAWB));
		}

		public void TestParent_ForCusHAWBShipmentHeldLetter()
		{
			TestParent("UPECusHAWB type is required so the correct note description for deserializing ShipmentHeldLetterBusinessObject is used", new UPEDocumentMenuItemLoader(Factory).LoadCusHAWBHeldLetter(ShipmentHeldLetterRecipient.Consignee), typeof(UPECusHAWB));
			TestParent("UPECusHAWB type is required so the correct note description for deserializing ShipmentHeldLetterBusinessObject is used", new UPEDocumentMenuItemLoader(Factory).LoadCusHAWBHeldLetter(ShipmentHeldLetterRecipient.Consignor), typeof(UPECusHAWB));
		}

		public void TestParent_ForJobDeclarationShipmentHeldLetter()
		{
			TestParent("Expected UPEJobDeclaration", new UPEDocumentMenuItemLoader(Factory).LoadJobDeclarationHeldLetter(ShipmentHeldLetterRecipient.Consignee), typeof(UPEJobDeclaration));
			TestParent("Expected UPEJobDeclaration", new UPEDocumentMenuItemLoader(Factory).LoadJobDeclarationHeldLetter(ShipmentHeldLetterRecipient.Consignor), typeof(UPEJobDeclaration));
		}

		public void TestParent_ForFinanceShipmentHeldLetter()
		{
			TestParent("Callout type is required so the correct note description for deserializing ShipmentHeldLetterBusinessObject is used", new UPEDocumentMenuItemLoader(Factory).LoadFinanceHeldLetter(ShipmentHeldLetterRecipient.Consignee), typeof(Callout));
			TestParent("Callout type is required so the correct note description for deserializing ShipmentHeldLetterBusinessObject is used", new UPEDocumentMenuItemLoader(Factory).LoadFinanceHeldLetter(ShipmentHeldLetterRecipient.Consignor), typeof(Callout));
		}

		void TestParent(ZString failureMessage, DocumentCommand document, Type businessObjectType)
		{
			BusinessObject parent = Factory.New(businessObjectType);
			UPEPrintBatchItem printBatchItem = Factory.New<UPEPrintBatchItem>();
			printBatchItem.T6_ParentID = parent.PK;
			printBatchItem.T6_SU = document.PK;
			AssertEquals(failureMessage, businessObjectType, printBatchItem.Parent.GetType());
			AssertEquals("Should return the correct business object", parent.PK, printBatchItem.Parent.PK);
		}

		#endregion
		#region Implementation
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
		}
		#endregion
	}
}
