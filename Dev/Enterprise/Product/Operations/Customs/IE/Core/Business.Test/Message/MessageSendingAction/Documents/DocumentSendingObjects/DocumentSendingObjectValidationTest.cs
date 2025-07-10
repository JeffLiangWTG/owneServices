using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.Testing
{
	sealed class DocumentSendingObjectValidationTest : TestCaseWithFactory
	{
		public void TestCheckFileDescription()
		{
			var targetInfo = documentsSendingObject.FileDescriptionInfo;
			var validation = documentsSendingObject.Validation;

			uploadDocumentsSendingAction.ShouldSend = false;
			validation.ValidateFileDescription();
			AssertNoErrors("No error if action is not selected for sending.", targetInfo);

			uploadDocumentsSendingAction.ShouldSend = true;
			ValidationTestHelper.AssertErrorIfNotEntered(targetInfo);
		}

		public void TestCheckEDocs_Unsupported()
		{
			var targetInfo = documentsSendingObject.EDocInfo;
			var validation = documentsSendingObject.Validation;
			var eDoc1 = declaration.DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ANastyVirus.bat", Core.Constants.FileFormats.PDF);
			Factory.Save();
			documentsSendingObject.EDoc = eDoc1.UniqueKey;

			uploadDocumentsSendingAction.ShouldSend = false;
			validation.ValidateEDoc();
			AssertHasErrors("The file type is not supported. Please convert document to one of the supported types - pdf, png, gif, jpeg, zip, 7z, docx, xlsx or pptx.", targetInfo);

			uploadDocumentsSendingAction.ShouldSend = true;
			validation.ValidateEDoc();
			AssertHasError("Error when file type is not supported.", targetInfo, ".bat is not supported.");
		}

		public void TestCheckEDocs_Duplication()
		{
			var targetInfo = documentsSendingObject.EDocInfo;
			var validation = documentsSendingObject.Validation;
			var eDoc1 = declaration.DocManagerInfo.AddFileOrDocument(new byte[] { 1, 2, 3 }, "Document1.Docx", Core.Constants.FileFormats.PDF);
			Factory.Save();
			documentsSendingObject.EDoc = eDoc1.UniqueKey;
			var documentsSendingObject2 = additionalInfoSendingObject.EDocsCollection.AddNew();
			documentsSendingObject2.EDoc = eDoc1.UniqueKey;
			validation.ValidateEDoc();

			uploadDocumentsSendingAction.ShouldSend = false;
			validation.ValidateEDoc();
			AssertNoErrors("No error if action is not selected for sending.", targetInfo);

			uploadDocumentsSendingAction.ShouldSend = true;
			validation.ValidateEDoc();
			AssertHasError("Error when the same document is selected twice", targetInfo, "You have already selected this eDoc.");
		}

		public void TestCheckEDocFileExtension()
		{
			var errMsg = "The file type is not supported. Please convert document to one of the supported types - pdf, png, gif, jpeg, zip, 7z, docx, xlsx or pptx.";
			var targetInfo = documentsSendingObject.EDocInfo;

			var pdfDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.pdf", "CIV");
			var xlsDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice.xls", "CIV");
			var noextDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice", "CIV");
			CombineAssertions(() =>
			{
				documentsSendingObject.EDoc = pdfDoc.UniqueKey;
				AssertNoErrorContaining(targetInfo, errMsg);

				documentsSendingObject.EDoc = xlsDoc.UniqueKey;
				AssertHasErrorContaining(targetInfo, errMsg);

				documentsSendingObject.EDoc = noextDoc.UniqueKey;
				AssertHasErrorContaining(targetInfo, errMsg);
			});
		}

		public void TestCheckEDocFileSizeInMB()
		{
			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[documentsSendingObject.Validation.MaxEDocFileSizeInBytes], "Document1.Docx", Core.Constants.FileFormats.PDF);
			documentsSendingObject.EDoc = eDoc.UniqueKey;
			documentsSendingObject.Validation.ValidateEDocFileSizeInMB();
			AssertNoMessageError(documentsSendingObject.EDocFileSizeInMBInfo, documentsSendingObject.Validation.EDocTooLargeError);

			var eDocTooLarge = declaration.DocManagerInfo.AddFileOrDocument(new byte[documentsSendingObject.Validation.MaxEDocFileSizeInBytes + 1], "DocumentTooLarge.Docx", Core.Constants.FileFormats.PDF);
			documentsSendingObject.EDoc = eDocTooLarge.UniqueKey;
			documentsSendingObject.Validation.ValidateEDocFileSizeInMB();
			AssertHasError(documentsSendingObject.EDocFileSizeInMBInfo, documentsSendingObject.Validation.EDocTooLargeError);
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
			uploadDocumentsSendingAction = new UploadDocumentsSendingAction(entryHeader);
			additionalInfoSendingObject = new AdditionalInfoSendingObject(entryHeader, uploadDocumentsSendingAction, requestedDocument);
			documentsSendingObject = additionalInfoSendingObject.EDocsCollection.AddNew();
		}

		UploadDocumentsSendingAction uploadDocumentsSendingAction;
		AdditionalInfoSendingObject additionalInfoSendingObject;
		DocumentSendingObject documentsSendingObject;
		JobDeclaration declaration;
	}
}
