using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ExemptionRequestOfPenalty))]
	sealed class ExemptionRequestOfPenaltyTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => new ExemptionRequestOfPenalty(Create5UAMessage());

		public void TestGetOtherData()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryNum = entry.EntryNumbers.AddNew();
			entryNum.CE_EntryType = ElectronicDocumentTypeList.Codes._5UA;
			entryNum.CE_IssueDate = new ZDateTime(2024, 06, 03);
			entryNum.CE_EntryLineReference = "1";

			var outgoingMessage = Create5UAMessage();
			entry.Messages.Add(outgoingMessage);

			IsIncoming = true;
			var fileReader = new TestFileReader(typeof(EDIMessageWrapperTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5UB_0.xml");
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5UB;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			incomingMessage.EM_ApplicationReference = outgoingMessage.EM_MessageNum;
			entry.Messages.Add(incomingMessage);
			Factory.Save();

			var wrapper = new ExemptionRequestOfPenalty(outgoingMessage);
			AssertEquals(new ZDateTime(2024, 06, 03), wrapper.AcceptedDate);
			AssertEquals(new ZDateTime(2020, 06, 01), wrapper.ReviewDate);
			AssertEquals("승인", wrapper.ReviewResultDescription);
		}

		EDIMessage Create5UAMessage()
		{
			IsIncoming = false;
			var fileReader = new TestFileReader(typeof(EDIMessageWrapperTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5UA_D1.xml");
			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5UA;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageText = messageText;
			outgoingMessage.EM_ApplicationReference = "1";
			outgoingMessage.EM_MessageNum = "1";

			return outgoingMessage;
		}
		bool IsIncoming { get; set; }
		public string TestFilesPath => IsIncoming ? "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming" : "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";
	}
}
