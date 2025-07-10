using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5TFMessageProcessorTest : XMLMessageTestHelper<GOVCBR5TFMessageProcessorTest>
	{
		public void Test5TF()
		{
			var entry = CreateEntryForTest();
			var incomingMessage = CreateMessageForTest("GOVCBR5TF_CUS.xml");
			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("SeokWon.Oh@wisetechglobal.com", recipient.Email);
			AssertContains("수입신고서", email.Body);
			AssertContains("41634-20-021969M", email.Body);
			AssertContains("2020-10-13", email.Body);
			AssertContains("관세법인 에이원 부산총괄본부/이성욱이흥대", email.Body);
			AssertContains("(주)디엠씨영천공장", email.Body);
		}
		public void TestEmpty()
		{
			var entry = CreateEntryForTest();
			var incomingMessage = CreateMessageForTest("GOVCBR5TF_Empty.xml");
			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertNotContains("(주)디엠씨영천공장", email.Body);
		}

		CusEntryHeader CreateEntryForTest()
		{
			var entry = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();

			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryNum = "4163420021969M";
			entryNumber.CE_EntryType = "IMP";
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNumber.CE_ParentID = entry.PK;
			entryNumber.CE_ParentTable = CusEntryHeader.Schema.TableName;
			entryNumber.CE_EntryStatus = ZString.Empty;

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "SOH";
			staff.GS_EmailAddress = "SeokWon.Oh@wisetechglobal.com";

			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._929;
			outgoingMessage.EM_SystemCreateUser = "SOH";
			outgoingMessage.EM_LinkTable = CusEntryHeader.Schema.TableName;
			outgoingMessage.EM_LinkUniqueID = entry.PK;
			outgoingMessage.EM_LinkedObject = entry;
			Factory.Save();

			return entry;
		}

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBR5TFMessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5TF;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			Factory.Save();
			return incomingMessage;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
	}
}
