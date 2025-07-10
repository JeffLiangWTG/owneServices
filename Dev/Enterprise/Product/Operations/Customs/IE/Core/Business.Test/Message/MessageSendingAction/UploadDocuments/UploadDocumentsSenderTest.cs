using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.Testing
{
	sealed class UploadDocumentsSenderTest : TestCaseWithFactory
	{
		public void TestSend_UCC5() => TestSend(ImportDeclarationApplicationCodeList.Codes.V1, "IE5");

		public void TestSend_UCC6() => TestSend(ImportDeclarationApplicationCodeList.Codes.V2, "IEI");

		void TestSend(string je_ApplicationCode, string expectedEM_ApplicationCode)
		{
			MessageProviderTestHelper.SetupForCusSupportingInfo(Factory, headerOnly: true, new[] { ("ADD1", "AddInfo 1 Desc"), ("ADD2", "AddInfo 2 Desc"), ("SUP1", "Suppli Doc 1"), ("SUP2", "Suppli Doc 2") });
			var testBizObjs = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
			var declaration = testBizObjs.entryHeaderWrapper.Declaration;
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = je_ApplicationCode;
			var sup1 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Sup1.pdf", "CIV");
			var sup2 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Sup2.pdf", "CIV");

			var inv = testBizObjs.entryHeaderWrapper.RandomInvoiceHeader;
			var requestedDocuments = testBizObjs.entryHeaderWrapper.EntryHeader.EntryInstruction.RequestedDocuments;
			var requestedDocument1 = requestedDocuments.AddNew();
			requestedDocument1.CSI_Code = "9001";
			requestedDocument1.CSI_Description = "9001 Desc";
			requestedDocument1.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened;

			var requestedDocument2 = requestedDocuments.AddNew();
			requestedDocument2.CSI_Code = "9002";
			requestedDocument2.CSI_Description = "9002 Desc";
			requestedDocument2.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened;

			var sendingParent = new UploadDocumentsSendingActionParent(testBizObjs.entryHeaderWrapper.Declaration, AESOutgoingMessageTypeList.Codes.DocumentUpload);
			var sendingAction = (UploadDocumentsSendingAction)sendingParent.SendingObjectsCollection.FirstOrDefault();
			var addInfo1SendingObject = sendingAction.AddInfoCollection.Cast<AdditionalInfoSendingObject>().First(addInfo => addInfo.DocumentType == "9001");
			addInfo1SendingObject.DocumentInformation = "Document Information 1";
			var addInfo2SendingObject = sendingAction.AddInfoCollection.Cast<AdditionalInfoSendingObject>().First(addInfo => addInfo.DocumentType == "9002");
			addInfo2SendingObject.DocumentInformation = "Document Information 2";
			addInfo1SendingObject.EDocsCollection.AddNew().EDoc = sup1.UniqueKey;
			addInfo2SendingObject.EDocsCollection.AddNew().EDoc = sup2.UniqueKey;

			sendingAction.CreateSender().Send();

			var createdMessages = testBizObjs.entryHeaderWrapper.EntryHeader.Messages;
			AssertEquals("Created messages count", 1, createdMessages.Count);
			AssertEquals("Application code", expectedEM_ApplicationCode, createdMessages[0].EM_ApplicationCode);
		}
	}
}
