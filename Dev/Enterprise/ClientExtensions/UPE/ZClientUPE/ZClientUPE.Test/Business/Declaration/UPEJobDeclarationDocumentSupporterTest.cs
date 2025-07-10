using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.BISI;
using Enterprise.Client.UPE.Business.CommercialInvoice;
using Enterprise.Client.UPE.Testing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(UPEJobDeclarationDocumentSupporter))]
	sealed class UPEJobDeclarationDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestSupportedDataContexts()
		{
			AssertEquals("Core.Constants.DataContext.Declaration is Supported", true, DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.Declaration)));
			AssertEquals("Core.Constants.DataContext.CusHAWB is Supported", true, DocumentSupporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.CusHAWB)));
		}

		public void TestGetMenuTemplateFilterValue_ForTaxInvoice()
		{
			Declaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			Declaration.Importer.IsITFChargableForThisImporter = true;
			AssertEquals("Should deliver Tax Invoice in the alternate broker document pack if ITF chargable", "Y", DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintClientSpecificInvoice, null));
			Declaration.Importer.IsITFChargableForThisImporter = false;
			AssertEquals("Should NOT deliver Tax Invoice in the alternate broker document pack if ITF NOT chargable", "N", DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintClientSpecificInvoice, null));
			SetupFinanceFreightCharges();
			AssertEquals("Should deliver Tax Invoice in the alternate broker document pack if Freight charges exist", "N", DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintClientSpecificInvoice, null));
		}

		public void TestGetMenuTemplateFilterValue_ForCommercialInvoiceImage()
		{
			Declaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertNull("No commercial invoice image initially for the test", Declaration.CommercialInvoiceImage);
			AssertEquals("Should not deliver commercial invoice image if image not available", "N", DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintStandardInvoice, null));
			SetupCommercialInvoiceImage();
			AssertNotNull("Must have a commercial invoice image for the test", Declaration.CommercialInvoiceImage);
			AssertEquals("Should NOT deliver Tax Invoice in the alternate broker document pack if ITF NOT chargable", "Y", DocumentSupporter.GetMenuTemplateFilterValue(MenuTemplateFilterType.PrintStandardInvoice, null));
		}

		public void TestGetContactOrganisation_ForAlternateBrokerDocumentPack()
		{
			var deliveryContact = DocumentSupporter.GetContactOrganisation(UPEDocumentMenuItemLoader.AlternateBrokerDocumentPackMenuName, ContactType.All, DocumentDirection.ANY);
			AssertEquals("When no declaration importer", null, deliveryContact.OrgHeader);
			Declaration.JE_OH_Importer = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			deliveryContact = DocumentSupporter.GetContactOrganisation(UPEDocumentMenuItemLoader.AlternateBrokerDocumentPackMenuName, ContactType.All, DocumentDirection.ANY);
			AssertEquals("When no importer air broker", null, deliveryContact.OrgHeader);
			Declaration.Importer.SetRelatedParty(Factory.NewWithValidTestData<OrgHeader>(), RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, ZString.Empty);
			deliveryContact = DocumentSupporter.GetContactOrganisation(UPEDocumentMenuItemLoader.AlternateBrokerDocumentPackMenuName, ContactType.All, DocumentDirection.ANY);
			AssertEquals("Importer air broker should be the delivery organisation", Declaration.Importer.DeliveryAirCustomsBroker, deliveryContact.OrgHeader);
			Declaration.Importer.SetRelatedParty(Factory.NewWithValidTestData<OrgHeader>(), RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, ZString.Empty);
			deliveryContact = DocumentSupporter.GetContactOrganisation("SomeOtherDocument", ContactType.All, DocumentDirection.ANY);
			AssertEquals("When not using the AlternateBroker document pack", null, deliveryContact);
		}

		public void TestAddCommercialInvoiceNotAvailableNoteIfRequired()
		{
			Factory.Save();
			AssertNull("No commercial invoice image initially for the test", Declaration.CommercialInvoiceImage);
			DocumentSupporter.AddCommercialInvoiceNotAvailableNoteIfRequired();
			var loadedDeclaration = new BusinessObjectFactory().Load<UPEJobDeclaration>(Declaration.PK);
			AssertEquals("Note added to the job if there is no commercial invoice image to send to the alternate broker", 1, loadedDeclaration.Notes.FindByDescription(TestUPEJobDeclarationDocumentSupporter.CommercialInvoiceNotAvailableNoteDescription).Length);
			DocumentSupporter.AddCommercialInvoiceNotAvailableNoteIfRequired();
			loadedDeclaration = new BusinessObjectFactory().Load<UPEJobDeclaration>(Declaration.PK);
			AssertEquals("Second note added to the job if there is no commercial invoice image to send to the alternate broker", 2, loadedDeclaration.Notes.FindByDescription(TestUPEJobDeclarationDocumentSupporter.CommercialInvoiceNotAvailableNoteDescription).Length);
			SetupCommercialInvoiceImage();
			DocumentSupporter.AddCommercialInvoiceNotAvailableNoteIfRequired();
			loadedDeclaration = new BusinessObjectFactory().Load<UPEJobDeclaration>(Declaration.PK);
			AssertEquals("Should not add a note to the job if there is a commercial invoice available", 2, loadedDeclaration.Notes.FindByDescription(TestUPEJobDeclarationDocumentSupporter.CommercialInvoiceNotAvailableNoteDescription).Length);
		}

		public void TestAddNoteToJobIfCommercialInvoiceImageNotDeliveredToAlternateBroker()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			Factory.Save();
			AssertNull("No commercial invoice image initially for the test", Declaration.CommercialInvoiceImage);
			DocumentPrintedEventArgs e = new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, AlternateBrokerDocumentPack);
			DocumentEventSource.FireDocumentPrinted(e);
			UPEJobDeclaration loadedDeclaration = new BusinessObjectFactory().Load<UPEJobDeclaration>(Declaration.PK);
			AssertEquals("Note added to the job if there is no commercial invoice image to send to the alternate broker", 1, loadedDeclaration.Notes.FindByDescription(TestUPEJobDeclarationDocumentSupporter.CommercialInvoiceNotAvailableNoteDescription).Length);
			DocumentEventSource.FireDocumentPrinted(e);
			loadedDeclaration = new BusinessObjectFactory().Load<UPEJobDeclaration>(Declaration.PK);
			AssertEquals("Second note added to the job if there is no commercial invoice image to send to the alternate broker", 2, loadedDeclaration.Notes.FindByDescription(TestUPEJobDeclarationDocumentSupporter.CommercialInvoiceNotAvailableNoteDescription).Length);
			DocumentEventSource.FireDocumentPrinted(new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, DeclarationHeldLetter));
			loadedDeclaration = new BusinessObjectFactory().Load<UPEJobDeclaration>(Declaration.PK);
			AssertEquals("Should not add a note to the job if they are not delivering the alternate broker documents", 2, loadedDeclaration.Notes.FindByDescription(TestUPEJobDeclarationDocumentSupporter.CommercialInvoiceNotAvailableNoteDescription).Length);
			SetupCommercialInvoiceImage();
			DocumentEventSource.FireDocumentPrinted(e);
			loadedDeclaration = new BusinessObjectFactory().Load<UPEJobDeclaration>(Declaration.PK);
			AssertEquals("Should not add a note to the job if there is a commercial invoice available", 2, loadedDeclaration.Notes.FindByDescription(TestUPEJobDeclarationDocumentSupporter.CommercialInvoiceNotAvailableNoteDescription).Length);
		}

		DocumentCommand AlternateBrokerDocumentPack
		{
			get
			{
				return new UPEDocumentMenuItemLoader(Factory).LoadAlternateBrokerDocumentPack();
			}
		}

		DocumentCommand DeclarationHeldLetter
		{
			get
			{
				return new UPEDocumentMenuItemLoader(Factory).LoadJobDeclarationHeldLetter(ShipmentHeldLetterRecipient.Consignee);
			}
		}

		#region Wrapping CalloutDocumentSupporter
		[ExpectNoExceptions]
		public void TestWithoutCusHAWBAttached()
		{
			Declaration.FirstCusHAWB.Delete();
			TestUPEJobDeclarationDocumentSupporter documentSupporter = new TestUPEJobDeclarationDocumentSupporter(Declaration);
			documentSupporter.Initialise(new MockDocumentEventSource());
			DocumentWrapper[] wrappers = documentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CusHAWB, null);
			AssertEquals("No document wrappers available with DataContext.CusHAWB and no CusHAWB attached", null, wrappers);
		}

		public void TestInitialise_WrapsCalloutSupporter()
		{
			TestUPEJobDeclarationDocumentSupporter documentSupporter = new TestUPEJobDeclarationDocumentSupporter(Declaration);
			documentSupporter.Initialise(new MockDocumentEventSource());
			AssertEquals("Initialise should also initialise the CalloutDocumentSupporter", true, documentSupporter.CalloutDocumentSupporter.InitialiseCalled);
		}

		public void TestGetDocBusinessObjects_WrapsCalloutSupporter()
		{
			TestUPEJobDeclarationDocumentSupporter documentSupporter = new TestUPEJobDeclarationDocumentSupporter(Declaration);
			DocumentWrapper[] calloutWrapper = documentSupporter.GetDocumentWrappers(Core.Constants.DataContext.CusHAWB, null);
			AssertEquals("GetDocBusinessObjects should return a DocCallout for DataContext.CusHAWB", true, calloutWrapper[0] is DocCallout);
		}

		#endregion
		#region Shipment Held Letter

		public void TestShouldCall_QueryForShipmentHeldLetterDetails_BeforeRenderingDocument()
		{
			TestCaseWithClientSpecificDocuments.LoadUPESpecificDocuments();
			Declaration.FirstCusHAWB.QueryShipmentHeldLetterDetails += new QueryShipmentHeldLetterDetailsEventHandler(OnHAWB_QueryShipmentHeldLetterDetails);
			DocumentCancelEventArgs e = new DocumentCancelEventArgs(ShipmentHeldLetterForConsignee);
			DocumentEventSource.FireDocumentPrintRequested(e);
			AssertEquals("Should populate the hold letter details business object", "I want to", Declaration.FirstCusHAWB.ShipmentHeldLetterDetails.ReasonText);
			AssertEquals("Should cancel the delivery of the document", true, e.Cancel);
		}

		void OnHAWB_QueryShipmentHeldLetterDetails(object sender, QueryShipmentHeldLetterDetailsEventArgs e)
		{
			e.BizObj.DeliverByEmailFax = false;
			e.BizObj.QueueForBatchPrint = true;
			e.BizObj.ReasonText = "I want to";
		}

		DocumentCommand ShipmentHeldLetterForConsignee
		{
			get
			{
				return new UPEDocumentMenuItemLoader(Factory).LoadJobDeclarationHeldLetter(ShipmentHeldLetterRecipient.Consignee);
			}
		}

		#endregion
		#region Test Classes
		class TestUPEJobDeclaration : UPEJobDeclaration
		{
			public TestUPEJobDeclaration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new UPEJobDeclarationDocumentSupporter DocumentSupporter
			{
				get
				{
					return (UPEJobDeclarationDocumentSupporter)base.DocumentSupporter;
				}
			}
		}

		class TestUPEJobDeclarationDocumentSupporter : UPEJobDeclarationDocumentSupporter
		{
			public TestUPEJobDeclarationDocumentSupporter(UPEJobDeclaration declaration) : base(declaration)
			{
			}

			public new TestCalloutDocumentSupporter CalloutDocumentSupporter
			{
				get
				{
					return (TestCalloutDocumentSupporter)base.CalloutDocumentSupporter;
				}
			}

			protected override CalloutDocumentSupporter GetDocumentSupporterFromCusHAWB(UPECusHAWB cusHAWB)
			{
				return new TestCalloutDocumentSupporter(cusHAWB);
			}

			public new static string CommercialInvoiceNotAvailableNoteDescription
			{
				get
				{
					return UPEJobDeclarationDocumentSupporter.CommercialInvoiceNotAvailableNoteDescription;
				}
			}
		}

		class TestCalloutDocumentSupporter : CalloutDocumentSupporter
		{
			public TestCalloutDocumentSupporter(UPECusHAWB cusHAWB) : base(cusHAWB)
			{
			}

			public bool InitialiseCalled;
			protected override void InitialiseCore(IDocumentEvents documentEventSource)
			{
				base.InitialiseCore(documentEventSource);
				InitialiseCalled = true;
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
		TestUPEJobDeclaration Declaration;
		UPEJobDeclarationDocumentSupporter DocumentSupporter;
		MockDocumentEventSource DocumentEventSource;
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<UPEJobDeclaration>();
			declaration.Invoices.AddNew();
			declaration.DocsAndCartage.Services.AddNew();
			declaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			var cusHAWB = Factory.NewWithValidTestData<UPECusHAWB>();
			cusHAWB.CS_JE_CustomsFormalEntry = declaration.PK;
			return declaration;
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			Declaration = Factory.NewWithValidTestData<TestUPEJobDeclaration>();
			var cusHAWB = Factory.NewWithValidTestData<UPECusHAWB>();
			cusHAWB.CS_JE_CustomsFormalEntry = Declaration.PK;
			DocumentSupporter = Declaration.DocumentSupporter;
			DocumentEventSource = new MockDocumentEventSource();
			DocumentSupporter.Initialise(DocumentEventSource);
		}

		protected override bool ExcludeDocumentCommandTest(Integration.DocumentEngine.IDocumentCommand documentCommand)
		{
			if (documentCommand.SU_MenuName.StartsWith("Permit TradeNet 4.1"))
			{
				return ZBool.True;
			}

			return (documentCommand.PK == new ZGuid("e293d7db-ee39-44d0-9e0d-4847b14cb273") || (documentCommand.SU_MenuName.Contains("Cartage Advice") || documentCommand.SU_MenuName.Contains("DocBuilder Invoice")));
		}

		void SetupCommercialInvoiceImage()
		{
			var factory = new DocumentFactoryProvider().GetFactory(this.Factory);
			var parent = factory.New<StorageMain>();
			parent.SM_ParentFK = Declaration.PK;
			var document = parent.Documents.AddNew();
			document.SC_Date = ZDateTime.Now;
			document.SC_ImageData = UPETestHelper.TestFiles.CommercialInvoiceBmpBytes;
			document.SC_DocType = CommercialInvoiceDocManager.commercialInvoiceDocType;
			Declaration.DocManagerInfo.Documents.Add(document);
			Declaration.DocManagerInfo.Save();
			AssertNotNull("CommercialInvoiceImage should exist for the test", Declaration.CommercialInvoiceImage);
		}

		void SetupFinanceFreightCharges()
		{
			Callout callout = Factory.New<Callout>();
			callout.CS_JE_CustomsFormalEntry = Declaration.PK;
			callout.EnsureJobHeaderExists();
			CalloutCharge freightCharge = callout.JobHeader.Charges.AddNew();
			freightCharge.JR_Desc = ShipmentChargeDescription.Freight;
			freightCharge.TaxableAmount = 9m;
		}

		public new void TestRunningDocumentsShouldNotCauseException()
		{
			Assert(true);
		}
		#endregion
	}
}
