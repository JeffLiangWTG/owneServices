using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business.CommercialInvoice;
using Enterprise.Client.UPE.Business.Testing;
using Enterprise.Client.UPE.Testing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Client.UPE.Business.BatchPrinting.Testing
{
	sealed class UPEAlternateBrokerDocumentPackAutoDeliveryTest : UPEDocumentAutoDeliveryTest
	{
		public void TestAddNoteToJobIfCommercialInvoiceImageNotDeliveredToAlternateBroker()
		{
			Factory.Save();
			Assert("No commercial invoice image initially for the test", !Declaration.HasCommercialInvoice);
			Delivery.Deliver();
			UPEJobDeclaration loadedDeclaration = new BusinessObjectFactory().Load<UPEJobDeclaration>(Declaration.PK);
			AssertEquals("Note added to the job if there is no commercial invoice image to send to the alternate broker", 1, loadedDeclaration.Notes.FindByDescription(TestUPEJobDeclarationDocumentSupporter.CommercialInvoiceNotAvailableNoteDescription).Length);
			Delivery.Deliver();
			loadedDeclaration = new BusinessObjectFactory().Load<UPEJobDeclaration>(Declaration.PK);
			AssertEquals("Second note added to the job if there is no commercial invoice image to send to the alternate broker", 2, loadedDeclaration.Notes.FindByDescription(TestUPEJobDeclarationDocumentSupporter.CommercialInvoiceNotAvailableNoteDescription).Length);
			SetupCommercialInvoiceImage();
			loadedDeclaration = new BusinessObjectFactory().Load<UPEJobDeclaration>(Declaration.PK);
			AssertEquals("Should not add a note to the job if there is a commercial invoice available", 2, loadedDeclaration.Notes.FindByDescription(TestUPEJobDeclarationDocumentSupporter.CommercialInvoiceNotAvailableNoteDescription).Length);
		}

		protected override UPEDocumentAutoDelivery NewDocumentAutoDelivery()
		{
			return new UPEAlternateBrokerDocumentPackAutoDelivery((UPEJobDeclaration)DocumentSupportable);
		}

		protected override IUPEDocumentSupportable NewDocumentSupportable()
		{
			UPEJobDeclaration result = Factory.NewWithValidTestData<UPEJobDeclaration>();
			result.JE_OH_Importer = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			result.Importer.SetRelatedParty(DeliveryOrganisation, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, ZString.Empty);
			return result;
		}

		protected override DocumentCommand ExpectedDocumentCommand
		{
			get
			{
				return DocumentLoader.LoadAlternateBrokerDocumentPack();
			}
		}

		protected override ZString ExpectedPrintBatchType
		{
			get
			{
				return ZString.Empty;
			}
		}

		protected override ZString ExpectedDeliveryFailureEmailSubject
		{
			get
			{
				return "Delivery instructions incomplete for Alternate Broker Documents - " + DeliveryOrganisation.OH_Code;
			}
		}

		protected override ZString ExpectedDeliveryFailureEmailBody
		{
			get
			{
				return @"
Delivery instructions incomplete for Alternate Broker Documents; Generated 11-Nov-05 00:00:00

CargoWise One Code : DLVORG

Error: DeliveryAddress: Please enter a Fax Number.
";
			}
		}

		#region Implementation
		UPEJobDeclaration Declaration
		{
			get
			{
				return (UPEJobDeclaration)DocumentSupportable;
			}
		}

		class TestUPEJobDeclarationDocumentSupporter : UPEJobDeclarationDocumentSupporter
		{
			public TestUPEJobDeclarationDocumentSupporter(UPEJobDeclaration declaration) : base(declaration)
			{
			}

			public new static string CommercialInvoiceNotAvailableNoteDescription
			{
				get
				{
					return UPEJobDeclarationDocumentSupporter.CommercialInvoiceNotAvailableNoteDescription;
				}
			}
		}

		void SetupCommercialInvoiceImage()
		{
			DocumentFactory factory = new DocumentFactoryProvider().GetFactory(this.Factory);
			StorageMain parent = factory.New<StorageMain>();
			parent.SM_ParentFK = Declaration.PK;
			StorageDocs document = parent.Documents.AddNew();
			document.SC_Date = ZDateTime.Now;
			document.SC_ImageData = UPETestHelper.TestFiles.CommercialInvoiceBmpBytes;
			document.SC_DocType = CommercialInvoiceDocManager.commercialInvoiceDocType;
			Declaration.DocManagerInfo.Documents.Add(document);
			Declaration.DocManagerInfo.Save();
			AssertNotNull("CommercialInvoiceImage should exist for the test", Declaration.CommercialInvoiceImage);
		}
		#endregion
	}
}
