using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX583;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Messaging.Business;
using XmlObjectSerializer = CargoWise.Customs.Shared.MessageContracts.XmlObjectSerializer;

namespace Enterprise.Customs.IE.Business.Testing
{
	class DocumentsSenderTest : TestCaseWithFactory
	{
		public void TestSend()
		{
			MessageProviderTestHelper.SetupForCusSupportingInfo(Factory, headerOnly: true, new[] { ("ADD1", "AddInfo 1 Desc"), ("ADD2", "AddInfo 2 Desc"), ("SUP1", "Suppli Doc 1"), ("SUP2", "Suppli Doc 2") });
			var testBizObjs = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory);
			var declaration = testBizObjs.entryHeaderWrapper.Declaration;
			var sup1 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Sup1.pdf", "CIV");
			var sup2 = declaration.DocManagerInfo.AddFileOrDocument(new byte[3], "Sup2.pdf", "CIV");

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

			var sendingParent = new DocumentsSendingActionParent(testBizObjs.entryHeaderWrapper.Declaration, AESOutgoingMessageTypeList.Codes.DocumentUpload);
			var sendingAction = (DocumentsSendingAction)sendingParent.SendingObjectsCollection.FirstOrDefault();
			var addInfo1SendingObject = sendingAction.AddInfoCollection.Cast<AdditionalInfoSendingObject>().First(addInfo => addInfo.DocumentType == "9001");
			addInfo1SendingObject.DocumentInformation = "Document Information 1";
			var addInfo2SendingObject = sendingAction.AddInfoCollection.Cast<AdditionalInfoSendingObject>().First(addInfo => addInfo.DocumentType == "9002");
			addInfo2SendingObject.DocumentInformation = "Document Information 2";
			sendingAction.SupportingDocuments.AddNew().EDoc = sup1.UniqueKey;
			sendingAction.SupportingDocuments.AddNew().EDoc = sup2.UniqueKey;

			sendingAction.CreateSender().Send();

			var createdMessage = (OutboundEDIMessage)testBizObjs.entryHeaderWrapper.EntryHeader.Messages.Single();
			var messageContent = XmlObjectSerializer.Deserialize<Ex583>(createdMessage.EM_MessageText);
			AssertNotNull("Created message should have a correct Text.", messageContent);
			AssertEquals("AdditionalInformation collection should contain DocumentType.", true, messageContent.AdditionalInformation.Any(a => a.DocumentType == "9001" && a.DocumentComplementaryInformation == "Document Information 1"));
			AssertEquals("AdditionalInformation collection should contain DocumentType.", true, messageContent.AdditionalInformation.Any(a => a.DocumentType == "9002" && a.DocumentComplementaryInformation == "Document Information 2"));
			AssertEquals("SupportingDocument collection should contain eDoc1 PK binaries.", true, messageContent.SupportingDocument.Any(d => d.DocumentImage.Document.SequenceEqual(sup1.UniqueKey.ToGuid().ToByteArray())));
			AssertEquals("SupportingDocument collection should contain eDoc2 PK binaries.", true, messageContent.SupportingDocument.Any(d => d.DocumentImage.Document.SequenceEqual(sup2.UniqueKey.ToGuid().ToByteArray())));

			var attaches = createdMessage.MessageAttachments.Cast<EDIMessageAttach>();
			AssertEquals("EDIMessageAttach count", 2, attaches.Count());
			AssertEquals("Sup1", true, attaches.Any(t => t.EG_StorageDocsGuid == sup1.UniqueKey));
			AssertEquals("Sup2", true, attaches.Any(t => t.EG_StorageDocsGuid == sup2.UniqueKey));
		}
	}
}
