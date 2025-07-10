using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(UPECusHAWBDocumentSupporter))]
	sealed class UPECusHAWBDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestGetDocBusinessObjects()
		{
			TestUPECusHAWB cusHAWB = Factory.NewWithValidTestData<TestUPECusHAWB>();
			UPECusHAWBDocumentSupporter documentSupporter = (UPECusHAWBDocumentSupporter)cusHAWB.DocumentSupporter;
			DocumentWrapper[] wrappers = documentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CusHAWB, null);
			AssertEquals("Should return 1 UPEDocCusHAWB doc wrapper", 1, wrappers.Length);
			AssertEquals("Should return 1 UPEDocCusHAWB doc wrapper", true, wrappers[0] is UPEDocCusHAWB);
		}

		public void TestGetContactOrganisation()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			TestGetContactOrganisation_ForCusHAWBConsignee(new UPEDocumentMenuItemLoader(Factory).LoadCusHAWBHeldLetter(ShipmentHeldLetterRecipient.Consignee));
			TestGetContactOrganisation_ForCusHAWBConsignor(new UPEDocumentMenuItemLoader(Factory).LoadCusHAWBHeldLetter(ShipmentHeldLetterRecipient.Consignor));
			TestGetContactOrganisation_ForDeclarationImporter(new UPEDocumentMenuItemLoader(Factory).LoadCusHAWBHeldLetter(ShipmentHeldLetterRecipient.Consignor));
			TestGetContactOrganisation_ForCusHAWBConsignee(new UPEDocumentMenuItemLoader(Factory).LoadJobDeclarationHeldLetter(ShipmentHeldLetterRecipient.Consignee));
			TestGetContactOrganisation_ForCusHAWBConsignor(new UPEDocumentMenuItemLoader(Factory).LoadJobDeclarationHeldLetter(ShipmentHeldLetterRecipient.Consignee));
			TestGetContactOrganisation_ForDeclarationImporter(new UPEDocumentMenuItemLoader(Factory).LoadFinanceHeldLetter(ShipmentHeldLetterRecipient.Consignor));
			TestGetContactOrganisation_ForCusHAWBConsignee(new UPEDocumentMenuItemLoader(Factory).LoadJobDeclarationHeldLetter(ShipmentHeldLetterRecipient.Consignee));
			TestGetContactOrganisation_ForCusHAWBConsignor(new UPEDocumentMenuItemLoader(Factory).LoadJobDeclarationHeldLetter(ShipmentHeldLetterRecipient.Consignee));
			TestGetContactOrganisation_ForDeclarationImporter(new UPEDocumentMenuItemLoader(Factory).LoadJobDeclarationHeldLetter(ShipmentHeldLetterRecipient.Consignor));
		}

		void TestGetContactOrganisation_ForCusHAWBConsignee(DocumentCommand documentCommand)
		{
			var cusHAWB = Factory.NewWithValidTestData<TestUPECusHAWB>();
			cusHAWB.CS_OA_ConsigneeAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			cusHAWB.CS_OA_ConsignorAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			var deliveryContact = cusHAWB.DocumentSupporter.GetContactOrganisation(documentCommand.SU_MenuName, ContactType.Consignee, DocumentDirection.ANY);
			AssertEquals("GetContactOrganisation for document " + documentCommand.SU_MenuName, cusHAWB.Consignee.PK, deliveryContact.OrgHeader.PK);
		}

		void TestGetContactOrganisation_ForCusHAWBConsignor(DocumentCommand documentCommand)
		{
			var cusHAWB = Factory.NewWithValidTestData<TestUPECusHAWB>();
			cusHAWB.CS_JE_CustomsFormalEntry = Factory.NewWithValidTestData(typeof(BaseJobDeclaration)).PK;
			cusHAWB.Declaration.JE_OH_Importer = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			cusHAWB.Declaration.JE_OH_Supplier = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			cusHAWB.CS_OA_ConsigneeAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			cusHAWB.CS_OA_ConsignorAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			var deliveryContact = cusHAWB.DocumentSupporter.GetContactOrganisation(documentCommand.SU_MenuName, ContactType.Consignor, DocumentDirection.ANY);
			AssertEquals("GetContactOrganisation for document " + documentCommand.SU_MenuName, cusHAWB.Consignor.PK, deliveryContact.OrgHeader.PK);
		}

		void TestGetContactOrganisation_ForDeclarationImporter(DocumentCommand documentCommand)
		{
			var cusHAWB = Factory.NewWithValidTestData<TestUPECusHAWB>();
			cusHAWB.CS_JE_CustomsFormalEntry = Factory.NewWithValidTestData(typeof(BaseJobDeclaration)).PK;
			cusHAWB.Declaration.JE_OH_Importer = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			cusHAWB.Declaration.JE_OH_Supplier = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			cusHAWB.CS_OA_ConsigneeAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			cusHAWB.CS_OA_ConsignorAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;
			var deliveryContact = cusHAWB.DocumentSupporter.GetContactOrganisation(documentCommand.SU_MenuName, ContactType.Consignee, DocumentDirection.ANY);
			AssertEquals("GetContactOrganisation for document " + documentCommand.SU_MenuName, cusHAWB.Declaration.Importer.PK, deliveryContact.OrgHeader.PK);
		}

		public void TestGetContactOrganisation_ForAlternateBrokerSplitNotificationDocument()
		{
			var deliveryContact = DocumentSupporter.GetContactOrganisation(UPEDocumentMenuItemLoader.AlternateBrokerSplitNotificationMenuName, ContactType.Consignee, DocumentDirection.ANY);
			AssertEquals("When no declaration", null, deliveryContact.OrgHeader);
			CusHAWB.CS_JE_CustomsFormalEntry = Factory.NewWithValidTestData(typeof(BaseJobDeclaration)).PK;
			deliveryContact = DocumentSupporter.GetContactOrganisation(UPEDocumentMenuItemLoader.AlternateBrokerSplitNotificationMenuName, ContactType.Consignee, DocumentDirection.ANY);
			AssertEquals("When no declaration importer", null, deliveryContact.OrgHeader);
			CusHAWB.Declaration.JE_OH_Importer = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			deliveryContact = DocumentSupporter.GetContactOrganisation(UPEDocumentMenuItemLoader.AlternateBrokerSplitNotificationMenuName, ContactType.Consignee, DocumentDirection.ANY);
			AssertEquals("When no importer air broker", null, deliveryContact.OrgHeader);
			CusHAWB.Declaration.Importer.SetRelatedParty(Factory.NewWithValidTestData<OrgHeader>(), RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, ZString.Empty);
			deliveryContact = DocumentSupporter.GetContactOrganisation(UPEDocumentMenuItemLoader.AlternateBrokerSplitNotificationMenuName, ContactType.Consignee, DocumentDirection.ANY);
			AssertEquals("Importer air broker should be the delivery organisation", CusHAWB.Declaration.Importer.DeliveryAirCustomsBroker, deliveryContact.OrgHeader);
		}

		#region Shipment Held Letter

		public void TestShouldCallQueryForShipmentHeldLetterDetails()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			CusHAWB.QueryShipmentHeldLetterDetails += new QueryShipmentHeldLetterDetailsEventHandler(OnHAWB_QueryShipmentHeldLetterDetails);
			DocumentCancelEventArgs e = new DocumentCancelEventArgs(ShipmentHeldLetterForConsignor);
			DocumentEventSource.FireDocumentPrintRequested(e);
			AssertEquals("Should populate the hold letter details business object", "I want to", CusHAWB.ShipmentHeldLetterDetails.ReasonText);
			AssertEquals("Should not cancel the delivery of the document", false, e.Cancel);
			e.Cancel = false;
			QueryShipmentHeldLetterDetails_Cancel = true;
			QueryShipmentHeldLetterDetails_ShouldDeliverByEmailFax = true;
			DocumentEventSource.FireDocumentPrintRequested(e);
			AssertEquals("Should cancel delivery of the document", true, e.Cancel);
			e.Cancel = false;
			QueryShipmentHeldLetterDetails_Cancel = false;
			QueryShipmentHeldLetterDetails_ShouldDeliverByEmailFax = false;
			DocumentEventSource.FireDocumentPrintRequested(e);
			AssertEquals("Should cancel delivery of the document", true, e.Cancel);
		}

		public void TestShipmentHeldLetterDocument_QueueForBatchPrintOnly()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			CusHAWB.QueryShipmentHeldLetterDetails += new QueryShipmentHeldLetterDetailsEventHandler(OnHAWB_QueryShipmentHeldLetterDetails);
			DocumentCancelEventArgs args = new DocumentCancelEventArgs(ShipmentHeldLetterForConsignee);
			QueryShipmentHeldLetterDetails_ShouldQueueForBatchPrint = false;
			QueryShipmentHeldLetterDetails_ShouldDeliverByEmailFax = false;
			DocumentEventSource.FireDocumentPrintRequested(args);
			AssertEquals("No documents should be queued for batch print", 0, CurrentPrintBatch.PrintItems.Count);
			QueryShipmentHeldLetterDetails_ShouldQueueForBatchPrint = true;
			QueryShipmentHeldLetterDetails_ShouldDeliverByEmailFax = false;
			DocumentEventSource.FireDocumentPrintRequested(args);
			AssertEquals("A new shipment held letter should be queued", 1, CurrentPrintBatch.PrintItems.Count);
			AssertEquals("A new shipment held letter should be queued", CusHAWB.PK, CurrentPrintBatch.PrintItems[0].Parent.PK);
		}

		public void TestShipmentHeldLetterDocument_DeliverByEmailFaxOnly()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			CusHAWB.QueryShipmentHeldLetterDetails += new QueryShipmentHeldLetterDetailsEventHandler(OnHAWB_QueryShipmentHeldLetterDetails);
			DocumentCancelEventArgs args = new DocumentCancelEventArgs(ShipmentHeldLetterForConsignee);
			QueryShipmentHeldLetterDetails_ShouldQueueForBatchPrint = false;
			QueryShipmentHeldLetterDetails_ShouldDeliverByEmailFax = true;
			DocumentEventSource.FireDocumentPrintRequested(args);
			AssertEquals("No documents should be queued for batch print", 0, CurrentPrintBatch.PrintItems.Count);
			AssertEquals("Should show the document delivery form when ShouldDeliverByEmailFax=true", false, args.Cancel);
			QueryShipmentHeldLetterDetails_ShouldQueueForBatchPrint = false;
			QueryShipmentHeldLetterDetails_ShouldDeliverByEmailFax = false;
			DocumentEventSource.FireDocumentPrintRequested(args);
			AssertEquals("No documents should be queued for batch print", 0, CurrentPrintBatch.PrintItems.Count);
			AssertEquals("Should not proceed when ShouldDeliverByEmailFax=false", true, args.Cancel);
		}

		public void TestShipmentHeldLetterDocument_QueueForBatchPrintAndDeliverByEmailFax()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			CusHAWB.QueryShipmentHeldLetterDetails += new QueryShipmentHeldLetterDetailsEventHandler(OnHAWB_QueryShipmentHeldLetterDetails);
			QueryShipmentHeldLetterDetails_ShouldQueueForBatchPrint = true;
			QueryShipmentHeldLetterDetails_ShouldDeliverByEmailFax = true;
			DocumentEventSource.FireDocumentPrintRequested(new DocumentCancelEventArgs(ShipmentHeldLetterForConsignee));
			AssertEquals("No documents should be queued for batch print until after document delivery has occurred", 0, CurrentPrintBatch.PrintItems.Count);
			DocumentEventSource.FireDocumentPrinted(new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, ShipmentHeldLetterForConsignee));
			AssertEquals("The document should be queued for batch print now that delivery has occurred", 1, CurrentPrintBatch.PrintItems.Count);
		}

		public void TestShipmentHeldLetterDocument_QueueForBatchPrintAndDeliverByEmailFax_DeliveryCancelledByUser()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			CusHAWB.QueryShipmentHeldLetterDetails += new QueryShipmentHeldLetterDetailsEventHandler(OnHAWB_QueryShipmentHeldLetterDetails);
			QueryShipmentHeldLetterDetails_ShouldQueueForBatchPrint = true;
			QueryShipmentHeldLetterDetails_ShouldDeliverByEmailFax = true;
			DocumentEventSource.FireDocumentPrintRequested(new DocumentCancelEventArgs(ShipmentHeldLetterForConsignee));
			AssertEquals("No document should be queued for batch print until after document delivery has occurred", 0, CurrentPrintBatch.PrintItems.Count);
			DocumentEventSource.FireDocumentPrinted(new DocumentPrintedEventArgs(DeliveryInstructionDestination.UserCancelled, ShipmentHeldLetterForConsignee));
			AssertEquals("No document should be queued for batch print if document delivery has been cancelled", 0, CurrentPrintBatch.PrintItems.Count);
		}

		public void TestShipmentHeldLetterDocument_QueueForBatchPrintAndDeliverByEmailFax_FromDeclarationForConsignor()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			TestShipmentHeldLetterDocument_QueueForBatchPrintAndDeliverByEmailFax_FromDeclaration(ShipmentHeldLetterForConsignor, ShipmentHeldLetterForConsignorOnDeclaration);
		}

		public void TestShipmentHeldLetterDocument_QueueForBatchPrintAndDeliverByEmailFax_FromDeclarationForConsignee()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			TestShipmentHeldLetterDocument_QueueForBatchPrintAndDeliverByEmailFax_FromDeclaration(ShipmentHeldLetterForConsignee, ShipmentHeldLetterForConsigneeOnDeclaration);
		}

		void TestShipmentHeldLetterDocument_QueueForBatchPrintAndDeliverByEmailFax_FromDeclaration(DocumentCommand shipmentHeldLetterForCusHAWB, DocumentCommand shipmentHeldLetterForDeclaration)
		{
			CusHAWB.QueryShipmentHeldLetterDetails += new QueryShipmentHeldLetterDetailsEventHandler(OnHAWB_QueryShipmentHeldLetterDetails);
			QueryShipmentHeldLetterDetails_ShouldQueueForBatchPrint = true;
			DocumentEventSource.FireDocumentPrinted(new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, shipmentHeldLetterForDeclaration));
			AssertEquals("The document should be queued for batch print now that delivery has occurred", 1, CurrentPrintBatch.PrintItems.Count);
			AssertEquals("The declaration customer notification menu item should link the print batch item to the CusHAWB StmMenuItem with the CusHAWB PK", shipmentHeldLetterForCusHAWB.PK, CurrentPrintBatch.PrintItems[0].T6_SU);
			AssertEquals("The declaration customer notification menu item should link the print batch item to the CusHAWB StmMenuItem with the CusHAWB PK", CusHAWB.PK, CurrentPrintBatch.PrintItems[0].T6_ParentID);
		}

		void OnHAWB_QueryShipmentHeldLetterDetails(object sender, QueryShipmentHeldLetterDetailsEventArgs e)
		{
			if (QueryShipmentHeldLetterDetails_Cancel)
			{
				e.Cancel = true;
			}

			e.BizObj.DeliverByEmailFax = QueryShipmentHeldLetterDetails_ShouldDeliverByEmailFax;
			e.BizObj.QueueForBatchPrint = QueryShipmentHeldLetterDetails_ShouldQueueForBatchPrint;
			e.BizObj.ReasonText = "I want to";
		}

		bool QueryShipmentHeldLetterDetails_ShouldDeliverByEmailFax = true;
		bool QueryShipmentHeldLetterDetails_ShouldQueueForBatchPrint;
		bool QueryShipmentHeldLetterDetails_Cancel;
		DocumentCommand ShipmentHeldLetterForConsignee
		{
			get
			{
				return new UPEDocumentMenuItemLoader(Factory).LoadCusHAWBHeldLetter(ShipmentHeldLetterRecipient.Consignee);
			}
		}

		DocumentCommand ShipmentHeldLetterForConsignor
		{
			get
			{
				return new UPEDocumentMenuItemLoader(Factory).LoadCusHAWBHeldLetter(ShipmentHeldLetterRecipient.Consignor);
			}
		}

		DocumentCommand ShipmentHeldLetterForConsigneeOnDeclaration
		{
			get
			{
				return new UPEDocumentMenuItemLoader(Factory).LoadJobDeclarationHeldLetter(ShipmentHeldLetterRecipient.Consignee);
			}
		}

		DocumentCommand ShipmentHeldLetterForConsignorOnDeclaration
		{
			get
			{
				return new UPEDocumentMenuItemLoader(Factory).LoadJobDeclarationHeldLetter(ShipmentHeldLetterRecipient.Consignor);
			}
		}

		#endregion
		#region Test Classes
		class TestUPECusHAWB : UPECusHAWB
		{
			public TestUPECusHAWB(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new CusHAWBDocumentSupporter DocumentSupporter
			{
				get
				{
					return base.DocumentSupporter;
				}
			}
		}

		class MockDocumentEventSource : IDocumentEvents
		{
			public event DocumentCancelEventHandler DocumentPrintRequested;
			public event DocumentPrintedEventHandler DocumentPrePreviewed;
			public event DocumentPrintedEventHandler DocumentPrePrinted;
			public event DocumentPrintedEventHandler DocumentPrinted;
			public void FireDocumentPrintRequested(DocumentCancelEventArgs e)
			{
				DocumentPrintRequested(this, e);
			}

			public void FireDocumentPrePreviewed(DocumentPrintedEventArgs e)
			{
				DocumentPrePreviewed(this, e);
			}

			public void FireDocumentPrePrinted(DocumentPrintedEventArgs e)
			{
				DocumentPrePrinted(this, e);
			}

			public void FireDocumentPrinted(DocumentPrintedEventArgs e)
			{
				DocumentPrinted(this, e);
			}
		}

		#endregion
		#region Implementation
		TestUPECusHAWB CusHAWB;
		UPECusHAWBDocumentSupporter DocumentSupporter;
		MockDocumentEventSource DocumentEventSource;
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			CusHAWB = Factory.NewWithValidTestData<TestUPECusHAWB>();
			DocumentSupporter = (UPECusHAWBDocumentSupporter)CusHAWB.DocumentSupporter;
			DocumentEventSource = new MockDocumentEventSource();
			DocumentSupporter.Initialise(DocumentEventSource);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.New<UPECusHAWB>();
		}

		UPEPrintBatch CurrentPrintBatch
		{
			get
			{
				return new UPEPrintBatch.Loader(Factory).CreateOrLoadLatestBatch(UPEPrintBatchTypes.Codes.ShipmentHeldLetter);
			}
		}
		#endregion
	}
}
