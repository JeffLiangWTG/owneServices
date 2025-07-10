using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class IM446MessageProviderTest : DataProviderTestCase<IM446MessageProvider>
	{
		protected override IM446MessageProvider GetProvider()
		{
			var sendingObject = new AdditionalInfoSendingObject(entryHeader, null, document);
			sendingObject.CCQualifier = "IE";
			sendingObject.ReferenceNumber = "ABC123";

			var supportingDocument = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "SuppDoc.pdf", "CIV");
			var docSendingObject = new DocumentSendingObject(entryHeader, null);
			docSendingObject.EDoc = supportingDocument.UniqueKey;

			var provider = new IM446MessageProvider(new UploadDocumentsSendingAction(entryHeader));
			provider.AddData(sendingObject, new[] { docSendingObject.Document });
			return provider;
		}

		public void TestIM446Header()
		{
			Assert("Should implement IIM415Header", Provider is IIM446Header);
		}

		public void TestIM446ImportOperation()
		{
			Assert("Should implement IIM446ImportOperation", Provider is IIM446ImportOperation);
		}

		public void TestImportOperation()
		{
			Assert("Should implement IIM446ImportOperation", Provider.ImportOperation is IIM446ImportOperation);
		}

		public void TestCustomsOfficeLodgement()
		{
			AssertEquals("Customs Office of Lodgement", "IEDUB100", Provider.CustomsOfficeLodgement);
		}

		public void TestRequestedDocuments()
		{
			AssertEquals("Requested Document Count", 1, Provider.RequestedDocuments.Count);
		}

		public void TestFallbackProcedure()
		{
			AssertType<FallbackProcedureProvider>("IIM446MessageProvider.FallbackProcedure should return object of FallbackProcedure", Provider.FallbackProcedure);
		}

		public void TestDocumentsAvailable()
		{
			AssertEquals("Documents Available", true, Provider.DocumentsAvailable);
		}

		public void TestMRN()
		{
			AssertEquals("MRN", "MRN123", Provider.ImportOperation.MRN);
		}

		protected override void SetUp()
		{
			declaration = Factory.New<Declaration.JobDeclaration>();
			declaration.JE_CustomsOffice = "IEDUB100";
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			document = entryInstruction.RequestedDocuments.AddNew();
			document.CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.MovementReferenceNumberSetter("MRN123");
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
		}
		Declaration.JobDeclaration declaration;
		Declaration.CusEntryHeader entryHeader;
		RequestedDocument document;
	}
}
