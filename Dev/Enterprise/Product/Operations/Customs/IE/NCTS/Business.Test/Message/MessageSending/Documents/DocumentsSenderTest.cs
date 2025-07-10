using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR083C;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class DocumentsSenderTest : TestCaseWithFactory
	{
		public void TestSend()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var add1 = nctsHeader.DocManagerInfo.AddFileOrDocument(new byte[1], "AddInfo1.pdf", "CIV");
			var add2 = nctsHeader.DocManagerInfo.AddFileOrDocument(new byte[1], "AddInfo2.pdf", "CIV");
			var sendingObject = new DocumentSendingObject(nctsHeader);
			sendingObject.EDoc = add1.UniqueKey;

			var sup1 = nctsHeader.DocManagerInfo.AddFileOrDocument(new byte[1], "Sup1.pdf", "CIV");
			var sup2 = nctsHeader.DocManagerInfo.AddFileOrDocument(new byte[5], "Sup2.pdf", "CIV");

			var sendingActionParent = new DocumentSendingActionParent(nctsHeader);
			var sendingAction = (DocumentSendingAction)sendingActionParent.SendingObjectsCollection.FirstOrDefault();
			var addInfo1SendingObject = sendingAction.AddInfoCollection.AddNew();
			addInfo1SendingObject.DocumentType = "9001";
			addInfo1SendingObject.DocumentInformation = "Document Information 1";
			var addInfo2SendingObject = sendingAction.AddInfoCollection.AddNew();
			addInfo2SendingObject.DocumentType = "9002";
			addInfo2SendingObject.DocumentInformation = "Document Information 2";
			sendingAction.SupportingDocuments.AddNew().EDoc = sup1.UniqueKey;
			sendingAction.SupportingDocuments.AddNew().EDoc = sup2.UniqueKey;

			sendingAction.CreateSender().Send();

			var createdMessage = (OutboundEDIMessage)nctsHeader.MovementHeader.Messages.Single();
			var messageContent = XmlObjectSerializer.Deserialize<Tr083C>(createdMessage.EM_MessageText);
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
