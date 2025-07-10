using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	sealed class IM483HeaderProviderTest : DataProviderTestCase<IM483HeaderProvider>
	{
		public void TestInterfaces()
		{
			Assert("Should implement IIM483Header", Provider is IIM483Header);
			Assert("Should implement IDocumentSendingMapper", Provider is IDocumentSendingMapper);
		}

		public void TestDeclaration()
		{
			var declarant = Provider.Declaration;
			AssertType<IM483DeclarationTypeProvider>(declarant);
			AssertSame("Cached", declarant, Provider.Declaration);
		}

		public void TestAdditionalInformations()
		{
			SetUpTestData();
			var requestedDocument = entryInstruction.RequestedDocuments.AddNew();
			requestedDocument.CSI_Code = "9002";
			requestedDocument.CSI_Description = "9002 Desc";
			var sendingObject = new AdditionalInfoSendingObject(entryInstruction.EntryHeader, null, requestedDocument);

			((IDocumentSendingMapper)Provider).AddAdditionalInfomation(sendingObject);
			AssertType<AdditionalInfoSendingObjectProvider>(Provider.AdditionalInformations.Single());
		}

		public void TestSupportingDocuments()
		{
			SetUpTestData();
			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "SuppDoc.pdf", "CIV");
			var sendingObject = new DocumentSendingObject(entryHeader, null);
			sendingObject.EDoc = eDoc.UniqueKey;

			((IDocumentSendingMapper)Provider).AddSupportingDocument(sendingObject);
			AssertType<SupportingDocumentWithImageProvider>(Provider.SupportingDocuments.Single());
		}

		protected override IM483HeaderProvider GetProvider()
		{
			SetUpTestData();
			return new IM483HeaderProvider(new UploadDocumentsSendingAction(entryHeader));
		}

		void SetUpTestData()
		{
			if (entryHeader == null)
			{
				declaration = Factory.New<JobDeclaration>();
				entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				invoice = declaration.Invoices.AddNew();
				invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = entryInstruction.PK;
				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				entryLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
			}
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
	}
}
