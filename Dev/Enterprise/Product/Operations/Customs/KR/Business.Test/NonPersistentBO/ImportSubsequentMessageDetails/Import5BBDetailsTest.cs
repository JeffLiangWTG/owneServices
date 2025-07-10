using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(Import5BBDetails))]
	public class Import5BBDetailsTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new Import5BBDetails(Factory.New<EDIMessage>());
		}

		public void TestFullData()
		{
			var messageR99 = SetIncomingMessage(ElectronicDocumentTypeList.Codes._R99, "GOVCBRR99_5BB.xml");
			messageR99.EM_SystemCreateTimeUtc = new ZDateTime("2024-01-01");
			var message5BC = SetIncomingMessage(ElectronicDocumentTypeList.Codes._5BC, "GOVCBR5BC_AmendTypeIsA.xml");
			message5BC.EM_SystemCreateTimeUtc = new ZDateTime("2024-01-02");
			message5BC.EM_MessageOwner = CustomsEntryStatusTypeList.Codes.ANT;

			var details = new Import5BBDetails(message5BB);
			AssertEquals(1, details.SequenceNo);
			AssertEquals("ANT", details.MessageOrEntryStatus);
			AssertEquals("승인통보", details.MesageOrEntryStatusDescription);
			AssertEquals(new ZDateTime("2024-01-01"), details.AcceptedDate);
			AssertEquals(new ZDateTime("2021-05-06"), details.ReviewDate);
			AssertEquals("승인통보", details.ReviewResultDescription);
			AssertEquals("기재오류로 인한 정정", details.AmendReasonDescription);
		}

		public void TestNoExist5BC()
		{
			var messageR99 = SetIncomingMessage(ElectronicDocumentTypeList.Codes._R99, "GOVCBRR99_5BB.xml");
			messageR99.EM_SystemCreateTimeUtc = new ZDateTime("2024-01-01");

			var details = new Import5BBDetails(message5BB);
			AssertEquals(1, details.SequenceNo);
			AssertEquals("AAC", details.MessageOrEntryStatus);
			AssertEquals("정정접수통보", details.MesageOrEntryStatusDescription);
			AssertEquals(new ZDateTime("2024-01-01"), details.AcceptedDate);
			AssertEquals(ZDateTime.Empty, details.ReviewDate);
			AssertEquals(ZString.Empty, details.ReviewResultDescription);
			AssertEquals("기재오류로 인한 정정", details.AmendReasonDescription);
		}

		public void TestNoExist5BCAndR99()
		{
			var details = new Import5BBDetails(message5BB);
			AssertEquals(1, details.SequenceNo);
			AssertEquals(ZString.Empty, details.MessageOrEntryStatus);
			AssertEquals(ZString.Empty, details.MesageOrEntryStatusDescription);
			AssertEquals(ZDateTime.Empty, details.AcceptedDate);
			AssertEquals(ZDateTime.Empty, details.ReviewDate);
			AssertEquals(ZString.Empty, details.ReviewResultDescription);
			AssertEquals("기재오류로 인한 정정", details.AmendReasonDescription);
		}

		EDIMessage SetIncomingMessage(string messageType, string fileName)
		{
			var fileReader = new TestFileReader(typeof(EDIMessageWrapperTest));
			var testMsgFile = fileReader.GetEmbeddedFileData(TestImcomingFilesPath, fileName);
			var message = entry.Messages.AddNew();
			message.EM_MessageData = testMsgFile;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			message.EM_MessageType = messageType;
			message.EM_ApplicationReference = message5BB.EM_MessageNum;

			return message;
		}

		protected override void SetUp()
		{
			base.SetUp();
			entry = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			message5BB = entry.Messages.AddNew();
			message5BB.EM_MessageNum = "1";
			message5BB.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message5BB.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			message5BB.EM_MessageType = ElectronicDocumentTypeList.Codes._5BB;
			message5BB.EM_ApplicationReference = "1";
			var fileReader = new TestFileReader(typeof(EDIMessageWrapperTest));
			var testMsgFile = fileReader.GetEmbeddedFileData(TestOutGoingFilesPath, "GOVCBR5BB_Update.xml");
			message5BB.EM_MessageData = testMsgFile;

			Factory.Save();
		}
		CusEntryHeader entry;
		EDIMessage message5BB;

		const string TestOutGoingFilesPath = "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";
		const string TestImcomingFilesPath = "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
	}
}
