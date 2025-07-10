using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5GZMessageProcessorTest : XMLMessageTestHelper<GOVCBR5GZMessageProcessorTest>
	{
		public void Test5GZ()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5GZ_0.xml");
			CreateEntryWithOutgoingMessageForImport();
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", importEntry.CH_Status.IsEmpty);
			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);

			AssertEquals("entry is located", importEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to NDM", CustomsEntryStatusTypeList.Codes.NDM, importEntry.CH_Status);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("수입신고서", email.Body);
			AssertContains("2020-09-07 15:18:50", email.Body);
			AssertContains("12345-20-000045M", email.Body);
			AssertContains("김현우", email.Body);
			AssertContains("[23] 신고가이드 미준수", email.Body);
			AssertContains("[98] 기타-전자통보", email.Body);
			AssertContains("신고서에 수입심사에 필요한 정보가 기재되지 않았습니다. 예상오류통보내역에 따라 신고정정 조치하여 주시기 바랍니다.", email.Body);
			AssertContains("bl번호및 invoice 확인 부탁드립니다", email.Body);
			AssertNotContains("나머지 내역은 프로그램에서 확인 하십시오.", email.Body);

			AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-09-07 15:18:50", incomingMessage.EM_MessageInterpretation);
			AssertContains("12345-20-000045M", incomingMessage.EM_MessageInterpretation);
			AssertContains("김현우", incomingMessage.EM_MessageInterpretation);
			AssertContains("[23] 신고가이드 미준수", incomingMessage.EM_MessageInterpretation);
			AssertContains("[98] 기타-전자통보", incomingMessage.EM_MessageInterpretation);
			AssertContains("신고서에 수입심사에 필요한 정보가 기재되지 않았습니다. 예상오류통보내역에 따라 신고정정 조치하여 주시기 바랍니다.", incomingMessage.EM_MessageInterpretation);
			AssertContains("bl번호및 invoice 확인 부탁드립니다", incomingMessage.EM_MessageInterpretation);
		}

		public void Test5GZ_11ReasonCodes()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5GZ_11Reasons.xml");
			CreateEntryWithOutgoingMessageForImport();
			Factory.Save();
			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("나머지 내역은 프로그램에서 확인 하십시오.", email.Body);

			AssertNotContains("나머지 내역은 프로그램에서 확인 하십시오.", incomingMessage.EM_MessageInterpretation);
		}

		public void TestEmptyDescription()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5GZ_WithoutDescription.xml");
			CreateEntryWithOutgoingMessageForImport();
			Factory.Save();
			AssertNoExceptionThrown(() => new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage));
		}

		public void TestImport5GZ_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ImportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				var incomingMessage = CreateMessageForTest("GOVCBR5GZ_0.xml");
				Factory.Save();
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [수입 미결사유 통보]1234520000045M 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);

				AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);
				AssertContains("2020-09-07 15:18:50", incomingMessage.EM_MessageInterpretation);
				AssertContains("12345-20-000045M", incomingMessage.EM_MessageInterpretation);
				AssertContains("김현우", incomingMessage.EM_MessageInterpretation);
				AssertContains("[23] 신고가이드 미준수", incomingMessage.EM_MessageInterpretation);
				AssertContains("[98] 기타-전자통보", incomingMessage.EM_MessageInterpretation);
				AssertContains("신고서에 수입심사에 필요한 정보가 기재되지 않았습니다. 예상오류통보내역에 따라 신고정정 조치하여 주시기 바랍니다.", incomingMessage.EM_MessageInterpretation);
				AssertContains("bl번호및 invoice 확인 부탁드립니다", incomingMessage.EM_MessageInterpretation);
			}
		}

		public void TestImport5GZ_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.ImportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				var incomingMessage = CreateMessageForTest("GOVCBR5GZ_0.xml");
				CreateEntryForImport(false);
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수입 미결사유 통보] Response for Declaration Number: B00001000 / 제출번호: 1234520000045M", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [수입신고서]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);

				AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);
				AssertContains("2020-09-07 15:18:50", incomingMessage.EM_MessageInterpretation);
				AssertContains("12345-20-000045M", incomingMessage.EM_MessageInterpretation);
				AssertContains("김현우", incomingMessage.EM_MessageInterpretation);
				AssertContains("[23] 신고가이드 미준수", incomingMessage.EM_MessageInterpretation);
				AssertContains("[98] 기타-전자통보", incomingMessage.EM_MessageInterpretation);
				AssertContains("신고서에 수입심사에 필요한 정보가 기재되지 않았습니다. 예상오류통보내역에 따라 신고정정 조치하여 주시기 바랍니다.", incomingMessage.EM_MessageInterpretation);
				AssertContains("bl번호및 invoice 확인 부탁드립니다", incomingMessage.EM_MessageInterpretation);
			}
		}

		public void TestImport5GZ_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.ImportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				var incomingMessage = CreateMessageForTest("GOVCBR5GZ_0.xml");
				CreateEntryForImport(true);
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수입 미결사유 통보] Response for Declaration Number: B00001000 / 제출번호: 1234520000045M", email.Subject);
				AssertEquals("CusAgent@wisetechglobal.com", email.Recipients[0].Email);

				AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);
				AssertContains("2020-09-07 15:18:50", incomingMessage.EM_MessageInterpretation);
				AssertContains("12345-20-000045M", incomingMessage.EM_MessageInterpretation);
				AssertContains("김현우", incomingMessage.EM_MessageInterpretation);
				AssertContains("[23] 신고가이드 미준수", incomingMessage.EM_MessageInterpretation);
				AssertContains("[98] 기타-전자통보", incomingMessage.EM_MessageInterpretation);
				AssertContains("신고서에 수입심사에 필요한 정보가 기재되지 않았습니다. 예상오류통보내역에 따라 신고정정 조치하여 주시기 바랍니다.", incomingMessage.EM_MessageInterpretation);
				AssertContains("bl번호및 invoice 확인 부탁드립니다", incomingMessage.EM_MessageInterpretation);
			}
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
			staff2.GS_EmailAddress = "ImportGroupTest@wisetechglobal.com";
			importGroup = Factory.New<GlbGroup>();
			var link1 = Factory.New<GlbGroupLink>();
			link1.GK_GG = importGroup.PK;
			link1.GK_GS = staff2.PK;

			var staff3 = Factory.New<GlbStaff>();
			staff3.GS_Code = "AG";
			staff3.GS_LoginName = "Agent";
			staff3.GS_EmailAddress = "CusAgent@wisetechglobal.com";
			Factory.Save();
		}
		GlbGroup importGroup;

		void CreateEntryForImport(bool setCusAgent)
		{
			var declaration = Factory.New<JobDeclaration>();
			if (setCusAgent)
			{
				declaration.JE_GS_NKCusAgent = "AG";
			}
			importEntry = declaration.CustomsEntryHeaders.AddNew();
			var entryNumber = importEntry.EntryNumbers.AddNew();
			entryNumber.CE_EntryNum = "1234520000045M";
			entryNumber.CE_EntryType = "IMP";
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNumber.CE_ParentID = importEntry.PK;
			entryNumber.CE_ParentTable = CusEntryHeader.Schema.TableName;
			entryNumber.CE_EntryStatus = ZString.Empty;
			Factory.Save();
		}
		CusEntryHeader importEntry;

		void CreateEntryWithOutgoingMessageForImport()
		{
			if (importEntry == null)
			{
				CreateEntryForImport(true);
			}
			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._929;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_LinkTable = CusEntryHeader.Schema.TableName;
			outgoingMessage.EM_LinkUniqueID = importEntry.PK;
			outgoingMessage.EM_LinkedObject = importEntry;
		}

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBR5GZMessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5GZ;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			return incomingMessage;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
	}
}
