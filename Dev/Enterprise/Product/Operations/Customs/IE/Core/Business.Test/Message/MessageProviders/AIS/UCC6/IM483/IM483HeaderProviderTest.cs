using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	public class IM483HeaderProviderTest : DataProviderTestCase<IM483HeaderProvider>
	{
		public void TestInterfaces()
		{
			Assert("Should implement IIM415Header", Provider is IIM483Header);
			Assert("Should implement IIM483ImportOperation", Provider is IIM483ImportOperation);
			Assert("Should implement IDocumentSendingMapper", Provider is IDocumentSendingMapper);
		}

		public void TestImportOperation()
		{
			SetUpTestData();
			AssertSame(Provider, Provider.ImportOperation);
		}

		public void TestLRN()
		{
			SetUpTestData();
			entryHeader.CH_BGMReference = "LRN001";
			AssertEquals("LRN", "LRN001", Provider.LRN);
		}

		public void TestMRN()
		{
			SetUpTestData();
			entryHeader.MovementReferenceNumberSetter("MRN001");
			AssertEquals("MRN", "MRN001", Provider.MRN);
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
			var supportingDocument = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "SuppDoc.pdf", "CIV");
			var sendingObject = new DocumentSendingObject(entryHeader, null);
			sendingObject.EDoc = supportingDocument.UniqueKey;

			((IDocumentSendingMapper)Provider).AddSupportingDocument(sendingObject);
			AssertType<SupportingDocumentWithImageProvider>(Provider.SupportingDocuments.Single());
		}

		public void TestFallbackProcedure()
		{
			AssertType<FallbackProcedureProvider>(Provider.FallbackProcedure);
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
				invoice = declaration.Invoices.AddNew();
				invoiceLine = invoice.JobComInvoiceLines.AddNew();
				entryInstruction = invoiceLine.EntryInstruction;
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
