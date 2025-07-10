using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.Testing
{
	public abstract class CommonDocumentSendingObjectValidationTest : TestCaseWithFactory
	{
		public void TestCheckEDocs_Unsupported()
		{
			var validation = documentsSendingObject.Validation;
			var eDoc1 = declaration.DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ANastyVirus.bat", Core.Constants.FileFormats.PDF);
			Factory.Save();
			documentsSendingObject.EDoc = eDoc1.UniqueKey;
			validation.ValidateEDoc();
			AssertHasError("Error when file type is not supported.", documentsSendingObject.EDocInfo, ".bat is not supported.");
		}

		public void TestCheckEDocs_Duplication()
		{
			var validation = documentsSendingObject.Validation;
			var eDoc1 = declaration.DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, "Document1.Docx", Core.Constants.FileFormats.PDF);
			Factory.Save();
			documentsSendingObject.EDoc = eDoc1.UniqueKey;
			var documentsSendingObject2 = additionalInfoSendingObject.EDocsCollection.AddNew();
			documentsSendingObject2.EDoc = eDoc1.UniqueKey;
			validation.ValidateEDoc();
			AssertHasError("Error when the same document is selected twice", documentsSendingObject2.EDocInfo, "You have already selected this eDoc.");
		}

		protected override void SetUp()
		{
			base.SetUp();
			var testBizObjs = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
			var entryHeader = testBizObjs.entryHeaderWrapper.EntryHeader;
			var requestedDocument = entryHeader.EntryInstruction.RequestedDocuments.AddNew();
			declaration = testBizObjs.entryHeaderWrapper.Declaration;
			requestedDocument.CSI_Code = "9002";
			requestedDocument.CSI_Description = "9002 Desc";
			sendingAction = new DocumentsSendingAction(entryHeader);
			sendingAction.ShouldSend = true;
			additionalInfoSendingObject = new AdditionalInfoSendingObject(entryHeader, sendingAction, requestedDocument);
			documentsSendingObject = additionalInfoSendingObject.EDocsCollection.AddNew();
		}

		protected AdditionalInfoSendingObject additionalInfoSendingObject;
		protected DocumentSendingObject documentsSendingObject;
		protected JobDeclaration declaration;
		protected DocumentsSendingAction sendingAction;
	}
}
