using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX583;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Messaging.Business;
using XmlObjectSerializer = CargoWise.Customs.Shared.MessageContracts.XmlObjectSerializer;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	class DocumentsSenderTest : TestCaseWithFactory
	{
		public void TestSend()
		{
			var exitHeader = Factory.New<CusExitHeader>();
			var consignment = exitHeader.CusExitConsignments.AddNew();
			consignment.CXC_MovementReference = "MRN001";
			consignment.CXC_UniqueConsignmentReference = "LRN001";
			var exitReport = exitHeader.CusExitReports.AddNew();
			exitReport.CER_CXC_Consignment = consignment.PK;
			var sup1 = exitHeader.DocManagerInfo.AddFileOrDocument(new byte[1], "Sup1.pdf", "CIV");
			var sup2 = exitHeader.DocManagerInfo.AddFileOrDocument(new byte[5], "Sup2.pdf", "CIV");

			var sendingParent = new DocumentsSendingActionParent(exitReport, AESOutgoingMessageTypeList.Codes.DocumentUpload);
			var sendingAction = (DocumentsSendingAction)sendingParent.SendingObjectsCollection.FirstOrDefault();
			var addInfo1SendingObject = sendingAction.AddInfoCollection.AddNew();
			var addInfo2SendingObject = sendingAction.AddInfoCollection.AddNew();
			addInfo1SendingObject.DocumentType = "9001";
			addInfo1SendingObject.DocumentInformation = "Document Information 1";
			addInfo2SendingObject.DocumentType = "9002";
			addInfo2SendingObject.DocumentInformation = "Document Information 2";
			sendingAction.SupportingDocuments.AddNew().EDoc = sup1.UniqueKey;
			sendingAction.SupportingDocuments.AddNew().EDoc = sup2.UniqueKey;

			sendingAction.CreateSender().Send();

			var createdMessage = (OutboundEDIMessage)exitReport.Messages.Single();
			var messageContent = XmlObjectSerializer.Deserialize<Ex583>(createdMessage.EM_MessageText);
			AssertNotNull("Created message should have a correct Text.", messageContent);
			AssertEquals("MRN", "MRN001", messageContent.ExportOperation.Mrn);
			AssertEquals("LRN", "LRN001", messageContent.ExportOperation.Lrn);
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
