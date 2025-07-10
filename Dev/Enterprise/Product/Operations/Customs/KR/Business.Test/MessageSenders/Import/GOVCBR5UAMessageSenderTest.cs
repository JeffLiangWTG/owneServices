using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5UAMessageSenderTest : TestCaseWithFactory
	{
		public void TestValidationMode()
		{
			AssertEquals("Pre-condition", "Import", declaration.ValidationMode.ToString());
			var sender = GetMessageSender();
			AssertEquals("Import, PenaltyExemption", declaration.ValidationMode.ToString());
			sender.Send();
			AssertEquals("Import", declaration.ValidationMode.ToString());
		}

		MessageSender GetMessageSender() => IsExceptionTest ? new GOVCBR5UASenderForTest(MessageSendingObjects, Factory) : new GOVCBR5UASender(MessageSendingObjects, Factory);
		public ZBool IsExceptionTest;

		public void TestSend()
		{
			AssertEquals(2, MessageSendingObjects.Count());
			foreach (var sendingObject in MessageSendingObjects)
			{
				sendingObject.ShouldSend = true;
			}
			var sender = GetMessageSender();
			sender.Send();
			var entry = MessageSendingObjects.First().Header;
			var entryNumbers5UA = entry.EntryNumbers.Cast<CusEntryNumber>().Where(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5UA).OrderBy(x => x.CE_EntryLineReference).ToList();
			var messages5UA = entry.Messages.Cast<EDIMessage>().Where(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5UA).OrderBy(x => x.EM_ApplicationReference).ToList();
			var messages5FE = entry.Messages.Cast<EDIMessage>().Where(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5FE).OrderBy(x => x.EM_MessageNum).ToList();

			AssertEquals("Two entry num rows are created", 2, entryNumbers5UA.Count);
			AssertEquals("Two 5UA message rows are created", 2, messages5UA.Count);

			AssertEquals("Version Number in EDIMessage", "1", messages5UA[0].EM_ApplicationReference);
			AssertEquals("Version Number in CusEntryNum", "1", entryNumbers5UA[0].CE_EntryLineReference);
			AssertEquals("Message Status", CustomsMessageStatusTypeList.Codes.OriginalSent, entryNumbers5UA[0].CE_EntryStatus);
			AssertXMLContains("<VersionID>1</VersionID>", messages5UA[0].EM_MessageText);

			AssertEquals("Version Number in EDIMessage", "2", messages5UA[1].EM_ApplicationReference);
			AssertEquals("Version Number in CusEntryNum", "2", entryNumbers5UA[1].CE_EntryLineReference);
			AssertEquals("Message Status", CustomsMessageStatusTypeList.Codes.OriginalSent, entryNumbers5UA[1].CE_EntryStatus);
			AssertXMLContains("<VersionID>2</VersionID>", messages5UA[1].EM_MessageText);

			AssertEquals("5FE EDIMessage.EM_MessageNum is copied to 5UA EDIMessage.EM_MessageOwner", "1001", messages5UA[0].EM_MessageOwner);
			AssertEquals("5FE EDIMessage.EM_MessageNum is copied to 5UA EDIMessage.EM_MessageOwner", "1003", messages5UA[1].EM_MessageOwner);
		}

		IEnumerable<PenaltyExemptionRequestMessageSendingObject> MessageSendingObjects
		{
			get
			{
				if (messageSendingObjects == null)
				{
					var sendingObjectParent = new PenaltyExemptionRequestMessageSendingObjectParent(declaration);
					messageSendingObjects = sendingObjectParent.SendingObjectsCollection.Cast<PenaltyExemptionRequestMessageSendingObject>();
				}
				return messageSendingObjects;
			}
		}
		IEnumerable<PenaltyExemptionRequestMessageSendingObject> messageSendingObjects;

		JobDeclaration declaration;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.AllEntryLines.AddNew();
			entry.EntryNumber = "1234520000045M";

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;

			var message5FE = entry.Messages.AddNew();
			message5FE.EM_MessageType = ElectronicDocumentTypeList.Codes._5FE;
			message5FE.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message5FE.EM_MessageSubType = "AX";
			message5FE.EM_MessageNum = "1001";
			message5FE.EM_ApplicationReference = "2";
			message5FE.EM_SystemCreateTimeUtc = ZDateTime.Today;

			var messageR99 = entry.Messages.AddNew();
			messageR99.EM_MessageType = ElectronicDocumentTypeList.Codes._R99;
			messageR99.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			messageR99.EM_MessageSubType = ElectronicDocumentTypeList.Codes._5FE;
			messageR99.EM_MessageNum = "1002";
			messageR99.EM_ApplicationReference = "1001";
			messageR99.EM_SystemCreateTimeUtc = ZDateTime.Today.AddMinutes(1);

			var fileReader = new TestFileReader(typeof(GOVCBR5UAMessageSenderTest));
			var testFilesPath = "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
			var messageText = fileReader.GetEmbeddedFileText(testFilesPath, "GOVCBR5FK.xml");
			var message5FK = entry.Messages.AddNew();
			message5FK.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message5FK.EM_MessageType = ElectronicDocumentTypeList.Codes._5FK;
			message5FK.EM_MessageOwner = CustomsEntryStatusTypeList.Codes.ANT;
			message5FK.EM_ApplicationReference = "1001";
			message5FK.EM_MessageText = messageText;
			message5FK.EM_SystemCreateTimeUtc = ZDateTime.Today.AddMinutes(2);

			var sessionalData = instruction.AmendmentSessionalDataCollection.AddNew();
			sessionalData.CSI_LineNo = ZInt.ParseEmptyAsZero(message5FE.EM_ApplicationReference);
			sessionalData.CSI_Code = DutyTaxCorrectionCodeList.Codes.A;

			message5FE = entry.Messages.AddNew();
			message5FE.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message5FE.EM_MessageType = ElectronicDocumentTypeList.Codes._5FE;
			message5FE.EM_MessageSubType = "BX";
			message5FE.EM_MessageNum = "1003";
			message5FE.EM_ApplicationReference = "3";
			message5FE.EM_SystemCreateTimeUtc = ZDateTime.Today.AddMinutes(3);

			messageR99 = entry.Messages.AddNew();
			messageR99.EM_MessageType = ElectronicDocumentTypeList.Codes._R99;
			messageR99.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			messageR99.EM_MessageSubType = ElectronicDocumentTypeList.Codes._5FE;
			messageR99.EM_MessageNum = "1004";
			messageR99.EM_ApplicationReference = "1003";
			messageR99.EM_SystemCreateTimeUtc = ZDateTime.Today.AddMinutes(4);

			message5FK = entry.Messages.AddNew();
			message5FK.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message5FK.EM_MessageType = ElectronicDocumentTypeList.Codes._5FK;
			message5FK.EM_MessageOwner = CustomsEntryStatusTypeList.Codes.ANT;
			message5FK.EM_ApplicationReference = "1003";
			message5FK.EM_MessageText = messageText;
			message5FK.EM_SystemCreateTimeUtc = ZDateTime.Today.AddMinutes(5);

			sessionalData = instruction.AmendmentSessionalDataCollection.AddNew();
			sessionalData.CSI_LineNo = ZInt.ParseEmptyAsZero(message5FE.EM_ApplicationReference);
			sessionalData.CSI_Code = DutyTaxCorrectionCodeList.Codes.B;
		}
	}

	class GOVCBR5UASenderForTest : GOVCBR5UASender
	{
		public GOVCBR5UASenderForTest(IEnumerable<PenaltyExemptionRequestMessageSendingObject> entries, BusinessObjectFactory factory)
			: base(entries, factory)
		{
		}

		protected override Import5UAHeader GetMessageDataProvider(CusEntryHeader parent, ZString messageID) => throw new System.Exception();
	}
}
