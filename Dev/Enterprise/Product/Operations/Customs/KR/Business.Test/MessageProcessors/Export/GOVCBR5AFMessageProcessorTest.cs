using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5AFMessageProcessorTest : XMLMessageTestHelper<GOVCBR5AFMessageProcessorTest>
	{
		public void Test830()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5AF_830.xml");
			var outgoingMessage = CreateEntryWithOutgoingMessageForExport(ElectronicDocumentTypeList.Codes._830, "6N00220000052X");
			outgoingMessage.EM_ApplicationReference = "3";
			exportEntry.CH_VersionID = 2;
			Factory.Save();

			AssertEquals("PreCondition: Message ApplicationReference is Empty", ZString.Empty, incomingMessage.EM_ApplicationReference);
			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			exportEntry.Reload();
			incomingMessage.Reload();
			exportEntryNum.Reload();

			AssertEquals("entry is located", exportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to OAC", CustomsMessageStatusTypeList.Codes.OriginalAccepted, exportEntry.CH_Status);
			AssertEquals("EXP CusEntryHeader.CH_VersionID is updated 5AS EDIMessage.EM_ApplicationReference.", outgoingMessage.EM_ApplicationReference, exportEntry.CH_VersionID.ToString());
			AssertEquals("20200805123000", exportEntryNum.CE_IssueDate.ToString("yyyyMMddhhmmss"));

			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalAccepted, outgoingMessage.MessageOrEntryStatus);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("수출신고서", email.Body);
			AssertContains("6N002-20-000052X", email.Body);
			AssertContains("2020-08-05 12:30:12", email.Body);
			AssertContains("110835 김승옥", email.Body);
		}
		public void Test5AS()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5AF_5AS.xml");
			var outgoingMessage = CreateEntryWithOutgoingMessageForExport(ElectronicDocumentTypeList.Codes._5AS, "6N00220000051X");
			outgoingMessage.EM_ApplicationReference = "3";
			exportEntry.CH_VersionID = 2;
			Factory.Save();

			AssertEquals("PreCondition: Message ApplicationReference is Empty", ZString.Empty, incomingMessage.EM_ApplicationReference);
			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			AssertEquals("It means the amendment version of the transmitted 5AS EDIMessage.", "3", outgoingMessage.EM_ApplicationReference);
			AssertEquals("2 until 5AF is received.", 2u, exportEntry.CH_VersionID);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			exportEntry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", exportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to AAC", CustomsMessageStatusTypeList.Codes.AmendmentAccepted, exportEntry.CH_Status);
			AssertEquals("EXP CusEntryHeader.CH_VersionID is updated 5AS EDIMessage.EM_ApplicationReference.", outgoingMessage.EM_ApplicationReference, exportEntry.CH_VersionID.ToString());

			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, outgoingMessage.MessageOrEntryStatus);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("수출정정신고서", email.Body);
			AssertContains("6N002-20-000051X", email.Body);
			AssertContains("110633 이민우", email.Body);
			AssertContains("서류심사", email.Body);
			AssertContains("2020-08-06 12:30:12", email.Body);
			AssertContains("110835 김승옥", email.Body);
		}
		public void TestWithoutDeclarationAuthenticator()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5AF_WithoutDeclarationAuthenticator.xml");
			CreateEntryWithOutgoingMessageForExport(ElectronicDocumentTypeList.Codes._5AS, "6N00220000051X");
			Factory.Save();

			AssertNoExceptionThrown("When no Response.Declaration.Authenticator is there, system should still proceed successfully", () =>
			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage));

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("수출정정신고서", email.Body);
			AssertContains("6N002-20-000051X", email.Body);
			AssertNotContains("110633 이민우", email.Body);
			AssertNotContains("110835 김승옥", email.Body);
		}
		public void TestExport5AF_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				CreateMessageForTest("GOVCBR5AF_5AS_CUS.xml");
				Factory.Save();
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [수출 접수 통보]6N00220000052X 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ExportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
			}
		}
		public void TestExport5AF_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBR5AF_5AS_CUS.xml");
				CreateEntryForExport(false, "6N00220000052X");
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수출 접수 통보] Response for Declaration Number: B00001000 / 제출번호: 6N00220000052X", email.Subject);
				AssertEquals("ExportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [수출정정신고서]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);
			}
		}
		public void TestExport5AF_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBR5AF_5AS_CUS.xml");
				CreateEntryForExport(true, "6N00220000052X");
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수출 접수 통보] Response for Declaration Number: B00001000 / 제출번호: 6N00220000052X", email.Subject);
				AssertEquals("CusAgent@wisetechglobal.com", email.Recipients[0].Email);
			}
		}
		public void Test5ASExtension()
		{
			TestAssertionBy5ASAmendType(_5ASAmendmentType.Codes.Extension, CustomsMessageStatusTypeList.Codes.AmendmentAccepted);
		}
		public void Test5ASAmendment()
		{
			TestAssertionBy5ASAmendType(_5ASAmendmentType.Codes.Amendment, CustomsMessageStatusTypeList.Codes.AmendmentAccepted);
		}
		void TestAssertionBy5ASAmendType(string messageSubType, string status)
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5AF_5AS.xml");
			var outgoingMessage = CreateEntryWithOutgoingMessageForExport(ElectronicDocumentTypeList.Codes._5AS, "6N00220000051X");
			outgoingMessage.EM_MessageSubType = messageSubType;
			outgoingMessage.EM_ApplicationReference = "1";
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			AssertEquals("0 until 5AF is received.", 0u, exportEntry.CH_VersionID);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			exportEntry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", exportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals(status, exportEntry.CH_Status);
			AssertEquals(1u, exportEntry.CH_VersionID);
		}

		public void TestEmpty5ASMessage()
		{
			var outgoingMessage = CreateEntryWithOutgoingMessageForExport(ElectronicDocumentTypeList.Codes._5AS, "6N00220000051X");
			var fileReader = new TestFileReader(typeof(GOVCBR5AFMessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText("Enterprise.Customs.KR.Business.Testing.TestFiles.Export.Outgoing", "GOVCBR5AS_Empty.xml");
			outgoingMessage.EM_MessageText = messageText;
			exportEntry.CH_VersionID = 1;
			outgoingMessage.EM_ApplicationReference = "2";

			Factory.Save();
			AssertEquals(1, exportEntry.ExportAmendmentDetailsCollection.Count);
			var amendmentDetails = exportEntry.ExportAmendmentDetailsCollection[0];

			AssertEquals(ZInt.Zero, amendmentDetails.AmendSequenceNo);
			AssertEquals(ZString.Empty, amendmentDetails.MessageStatus);
			AssertEquals(ZDateTime.Empty, amendmentDetails.SubmissionDate);
			AssertEquals(ZString.Empty, amendmentDetails.AmendmentType);
			AssertEquals(ZString.Empty, amendmentDetails.AmendmentTypeDescription);
			AssertEquals(ZString.Empty, amendmentDetails.FaultParty);
			AssertEquals(ZString.Empty, amendmentDetails.FaultPartyOtherDescription);
			AssertEquals(ZString.Empty, amendmentDetails.ReasonCode);
			AssertEquals(ZString.Empty, amendmentDetails.AmendReasonDescription);
			AssertEquals(ZString.Empty, amendmentDetails.NoticeType);
			AssertEquals(ZString.Empty, amendmentDetails.NoticeDescription);
			AssertEquals(ZString.Empty, amendmentDetails.ApprovalNo);
			AssertEquals(ZDateTime.Empty, amendmentDetails.DecisionDate);
			AssertEquals(ZString.Empty, amendmentDetails.CustomerOfficerIDAndName);
		}

		public void TestAmendmentDetailsWith5AF()
		{
			var fileReader = new TestFileReader(typeof(GOVCBR5AFMessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText("Enterprise.Customs.KR.Business.Testing.TestFiles.Export.Outgoing", "GOVCBR5AS_Extend.xml");
			var outgoingMessage = CreateEntryWithOutgoingMessageForExport(ElectronicDocumentTypeList.Codes._5AS, "6N00220000051X");
			outgoingMessage.EM_MessageSubType = "C";
			outgoingMessage.EM_MessageText = messageText;
			exportEntry.CH_VersionID = 1;
			outgoingMessage.EM_ApplicationReference = "2";

			var incomingMessage = CreateMessageForTest("GOVCBR5AF_5AS.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			exportEntry.Reload();
			incomingMessage.Reload();

			AssertEquals(1, exportEntry.ExportAmendmentDetailsCollection.Count);

			var amendmentDetails = exportEntry.ExportAmendmentDetailsCollection[0];
			AssertEquals(1, amendmentDetails.AmendSequenceNo);
			AssertEquals("AAC", amendmentDetails.MessageStatus);
			AssertEquals(new ZDateTime(2021, 2, 2), amendmentDetails.SubmissionDate);
			AssertEquals("C", amendmentDetails.AmendmentType);
			AssertEquals("기간연장", amendmentDetails.AmendmentTypeDescription);
			AssertEquals("C", amendmentDetails.FaultParty);
			AssertEquals("해외거래처/구매자", amendmentDetails.FaultPartyOtherDescription);
			AssertEquals("23", amendmentDetails.ReasonCode);
			AssertEquals("선적지연", amendmentDetails.AmendReasonDescription);
			AssertEquals("110835/김승옥", amendmentDetails.CustomerOfficerIDAndName);
		}

		public void TestDKJ()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5AF_DKJ.xml");
			var outgoingMessage = CreateEntryWithOutgoingMessageForExport(ElectronicDocumentTypeList.Codes._DKJ, "6N00220000051X");
			outgoingMessage.EM_ApplicationReference = "3";
			exportEntry.CH_VersionID = 2;
			Factory.Save();

			AssertEquals("PreCondition: Message ApplicationReference is Empty", ZString.Empty, incomingMessage.EM_ApplicationReference);
			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			AssertEquals("It means the amendment version of the transmitted 5AS EDIMessage.", "3", outgoingMessage.EM_ApplicationReference);
			AssertEquals("2 until 5AF is received.", 2u, exportEntry.CH_VersionID);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			exportEntry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", exportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to AAC", CustomsMessageStatusTypeList.Codes.CancellationAccepted, exportEntry.CH_Status);
			AssertEquals(3u, exportEntry.CH_VersionID);
			AssertEquals("EXP CusEntryHeader.CH_VersionID is updated 5AS EDIMessage.EM_ApplicationReference.", outgoingMessage.EM_ApplicationReference, exportEntry.CH_VersionID.ToString());

			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals(CustomsMessageStatusTypeList.Codes.CancellationAccepted, outgoingMessage.MessageOrEntryStatus);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("수출취하신청서", email.Body);
			AssertContains("6N002-20-000051X", email.Body);
			AssertContains("110633 이민우", email.Body);
			AssertContains("서류심사", email.Body);
			AssertContains("2020-08-06 12:30:12", email.Body);
			AssertContains("110835 김승옥", email.Body);
		}
		protected override void SetUp()
		{
			base.SetUp();
			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "ORG";
			staff1.GS_LoginName = "Origin";
			staff1.GS_EmailAddress = "OriginalSender@wisetechglobal.com";

			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "T1";
			staff1.GS_LoginName = "Test1";
			staff2.GS_EmailAddress = "ExportGroupTest@wisetechglobal.com";
			exportGroup = Factory.New<GlbGroup>();
			var link1 = Factory.New<GlbGroupLink>();
			link1.GK_GG = exportGroup.PK;
			link1.GK_GS = staff2.PK;

			var staff3 = Factory.New<GlbStaff>();
			staff3.GS_Code = "AG";
			staff3.GS_LoginName = "Agent";
			staff3.GS_EmailAddress = "CusAgent@wisetechglobal.com";
			Factory.Save();
		}
		GlbGroup exportGroup;

		void CreateEntryForExport(bool setCusAgent, string entryNum)
		{
			var declaration = Factory.New<JobDeclaration>();
			if (setCusAgent)
			{
				declaration.JE_GS_NKCusAgent = "AG";
			}
			exportEntry = declaration.CustomsEntryHeaders.AddNew();
			exportEntryNum = exportEntry.EntryNumbers.AddNew();
			exportEntryNum.CE_EntryNum = entryNum;
			exportEntryNum.CE_EntryType = "EXP";
			exportEntryNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			exportEntryNum.CE_ParentID = exportEntry.PK;
			exportEntryNum.CE_ParentTable = CusEntryHeader.Schema.TableName;
			exportEntryNum.CE_EntryStatus = ZString.Empty;
			Factory.Save();
		}
		CusEntryHeader exportEntry;
		CusEntryNumber exportEntryNum;
		EDIMessage CreateEntryWithOutgoingMessageForExport(string em_MessageType, string entryNum)
		{
			if (exportEntry == null)
			{
				CreateEntryForExport(true, entryNum);
			}
			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = em_MessageType;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_LinkTable = CusEntryHeader.Schema.TableName;
			outgoingMessage.EM_LinkUniqueID = exportEntry.PK;
			outgoingMessage.EM_LinkedObject = exportEntry;
			outgoingMessage.EM_MessageNum = "62252";

			return outgoingMessage;
		}

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBR5AFMessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5AF;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			return incomingMessage;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Export.Incoming";
	}
}
