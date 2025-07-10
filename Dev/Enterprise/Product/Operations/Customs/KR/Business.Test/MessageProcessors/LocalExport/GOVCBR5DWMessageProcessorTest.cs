using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5DWMessageProcessorTest : XMLMessageTestHelper<GOVCBR5DWMessageProcessorTest>
	{
		public void Test5DW()
		{
			CreateEntryWithOutgoingMessageForLocalExport();
			var incomingMessage = CreateMessageForTest("GOVCBR5DW_CUS.xml");
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			localExportEntry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("환급대상수출물품 반입확인 제출", email.Body);
			AssertContains("0102014000001A", email.Body);
			AssertContains("2015-07-31", email.Body);
			AssertContains("10836-99-012345", email.Body);
			AssertContains("공급신청자 상호입니다.", email.Body);
			AssertContains("제조자 상호입니다.", email.Body);
			AssertContains("양수자 상호입니다.", email.Body);
			AssertContains("2014-05-06", email.Body);
			AssertContains("A01020", email.Body);
			AssertContains("김직원", email.Body);
			AssertContains("12", email.Body);
			AssertContains("99", email.Body);
			AssertContains("999", email.Body);
			AssertContains("99", email.Body);
			AssertContains("세관기재란입니다.", email.Body);
			AssertContains("나머지 내역은 프로그램에서 확인 하십시오.", email.Body);

			EM_MessageInterpretation_Check(incomingMessage);
		}
		public void TestEmpty()
		{
			CreateEntryWithOutgoingMessageForLocalExport();
			var incomingMessage = CreateMessageForTest("GOVCBR5DW_Empty.xml");
			Factory.Save();

			AssertNoExceptionThrown("When Xml Element Values is Empty, system should still proceed successfully", () =>
			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch());
		}

		public void Test5DW_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.LocalExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, localExportGroup.PK.ToGuid()))
			{
				var incomingMessage = CreateMessageForTest("GOVCBR5DW_CUS.xml");
				Factory.Save();
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [환급대상수출물품 반입확인 양수인통보]1083699012345 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("LocalExportGroupTest@wisetechglobal.com", email.Recipients[0].Email);

				EM_MessageInterpretation_Check(incomingMessage);
			}
		}

		public void Test5DW_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.LocalExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, localExportGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				var incomingMessage = CreateMessageForTest("GOVCBR5DW_CUS.xml");
				CreateEntryForLocalExport(false);
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[환급대상수출물품 반입확인 양수인통보] Response for Declaration Number: B00001000 / 제출번호: 1083699012345", email.Subject);
				AssertEquals("LocalExportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [환급대상수출물품 반입확인 제출]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);

				EM_MessageInterpretation_Check(incomingMessage);
			}
		}

		public void Test5DW_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.LocalExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, localExportGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				var incomingMessage = CreateMessageForTest("GOVCBR5DW_CUS.xml");
				CreateEntryForLocalExport(true);
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[환급대상수출물품 반입확인 양수인통보] Response for Declaration Number: B00001000 / 제출번호: 1083699012345", email.Subject);
				AssertEquals("CusAgent@wisetechglobal.com", email.Recipients[0].Email);

				EM_MessageInterpretation_Check(incomingMessage);
			}
		}

		void EM_MessageInterpretation_Check(EDIMessage message)
		{
			AssertContains("환급대상수출물품 반입확인 제출", message.EM_MessageInterpretation);
			AssertContains("0102014000001A", message.EM_MessageInterpretation);
			AssertContains("2015-07-31", message.EM_MessageInterpretation);
			AssertContains("10836-99-012345", message.EM_MessageInterpretation);
			AssertContains("공급신청자 상호입니다.", message.EM_MessageInterpretation);
			AssertContains("제조자 상호입니다.", message.EM_MessageInterpretation);
			AssertContains("양수자 상호입니다.", message.EM_MessageInterpretation);
			AssertContains("2014-05-06", message.EM_MessageInterpretation);
			AssertContains("A01020", message.EM_MessageInterpretation);
			AssertContains("김직원", message.EM_MessageInterpretation);
			AssertContains("12", message.EM_MessageInterpretation);
			AssertContains("999", message.EM_MessageInterpretation);
			AssertContains("99", message.EM_MessageInterpretation);
			AssertContains("세관기재란입니다.", message.EM_MessageInterpretation);
			AssertContains("나머지 내역은 프로그램에서 확인 하십시오.", message.EM_MessageInterpretation);
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
			staff2.GS_EmailAddress = "LocalExportGroupTest@wisetechglobal.com";
			localExportGroup = Factory.New<GlbGroup>();
			var link1 = Factory.New<GlbGroupLink>();
			link1.GK_GG = localExportGroup.PK;
			link1.GK_GS = staff2.PK;

			var staff3 = Factory.New<GlbStaff>();
			staff3.GS_Code = "AG";
			staff3.GS_LoginName = "Agent";
			staff3.GS_EmailAddress = "CusAgent@wisetechglobal.com";
			Factory.Save();
		}
		GlbGroup localExportGroup;

		void CreateEntryForLocalExport(bool setCusAgent)
		{
			var declaration = Factory.New<JobDeclaration>();
			if (setCusAgent)
			{
				declaration.JE_GS_NKCusAgent = "AG";
			}
			localExportEntry = declaration.CustomsEntryHeaders.AddNew();
			var entryNumber = localExportEntry.EntryNumbers.AddNew();
			entryNumber.CE_EntryNum = "1083699012345";
			entryNumber.CE_EntryType = "LEX";
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNumber.CE_ParentID = localExportEntry.PK;
			entryNumber.CE_ParentTable = CusEntryHeader.Schema.TableName;
			entryNumber.CE_EntryStatus = ZString.Empty;
			Factory.Save();
		}
		CusEntryHeader localExportEntry;

		void CreateEntryWithOutgoingMessageForLocalExport()
		{
			if (localExportEntry == null)
			{
				CreateEntryForLocalExport(true);
			}
			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5DP;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_LinkTable = CusEntryHeader.Schema.TableName;
			outgoingMessage.EM_LinkUniqueID = localExportEntry.PK;
			outgoingMessage.EM_LinkedObject = localExportEntry;
		}

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBR5DWMessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5DW;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;

			return incomingMessage;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.LocalExport.Incoming";
	}
}
