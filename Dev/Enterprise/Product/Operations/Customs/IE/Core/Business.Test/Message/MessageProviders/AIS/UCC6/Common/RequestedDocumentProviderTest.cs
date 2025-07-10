using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class RequestedDocumentProviderTest : DataProviderTestCase<RequestedDocumentProvider>
	{
		protected override RequestedDocumentProvider GetProvider()
		{
			var sendingObject = new AdditionalInfoSendingObject(entryHeader, null, document);
			sendingObject.CCQualifier = "IE";
			sendingObject.ReferenceNumber = "ABC123";

			var supportingDocument = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "SuppDoc.pdf", "CIV");
			var docSendingObject = new DocumentSendingObject(entryHeader, null);
			docSendingObject.EDoc = supportingDocument.UniqueKey;
			return new RequestedDocumentProvider(sendingObject, new [] { docSendingObject.Document });
		}

		public void TestType()
		{
			AssertEquals("Type", "123", Provider.Type);
		}

		public void TestQualifier()
		{
			AssertEquals("Qualifier", "IE", Provider.Qualifier);
		}

		public void TestReference()
		{
			AssertEquals("Reference Number", "ABC123", Provider.Reference);
		}

		public void TestDescription()
		{
			AssertEquals("Description", "Sample Text", Provider.Description);
		}

		public void TestDocumentsImage()
		{
			AssertEquals("Documents Image", 1, Provider.DocumentsImage.Count);
		}

		protected override void SetUp()
		{
			declaration = Factory.New<Declaration.JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader = Factory.New<Declaration.CusEntryHeader>();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.CH_JE = declaration.PK;
			document = entryInstruction.RequestedDocuments.AddNew();
			document.CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;
			document.CSI_ReferenceNumber = "ABC123";
			document.CSI_Description = "Sample Text";
			document.CSI_LineNo = 1;
			document.CSI_Code = "123";
		}
		Declaration.JobDeclaration declaration;
		RequestedDocument document;
		Declaration.CusEntryHeader entryHeader;
	}
}
