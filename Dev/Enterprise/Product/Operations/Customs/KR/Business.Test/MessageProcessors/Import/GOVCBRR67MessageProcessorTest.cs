using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBRR67MessageProcessorTest : XMLMessageTestHelper<GOVCBRR67MessageProcessorTest>
	{
		public void TestR67()
		{
			var incomingMessage = CreateMessageForTest("GOVCBRR67_CUS.xml");
			CreateEntryWithOutgoingMessageForImport();
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			AssertEquals("PreCondition: CusEntryNum Status is Empty", ZString.Empty, importEntryNum.CE_EntryStatus);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);
			Factory.Save();

			AssertEquals("entry is located", miscHeader.PK, incomingMessage.EM_LinkUniqueID);
			miscHeader.Reload();
			var outgoingMessage = miscHeader.Messages.LastOutgoingMessage;

			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalAccepted, miscHeader.CMR_Status);
			AssertEquals(outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("확정가격신고 기간연장신청", email.Body);
			AssertContains("2014-05-06 12:30:46", email.Body);
			AssertContains("5SG-41644-2020-X000003", email.Body);
			AssertContains("<td>신청문서 제출차수</td><td>1</td>", email.Body);
			AssertContains("2014-05-06 12:30:45", email.Body);
			AssertContains("접수결과내역등 결과내역을 기재(자유기재)", email.Body);

			AssertContains("확정가격신고 기간연장신청", incomingMessage.EM_MessageInterpretation);
			AssertContains("2014-05-06 12:30:46", incomingMessage.EM_MessageInterpretation);
			AssertContains("5SG-41644-2020-X000003", incomingMessage.EM_MessageInterpretation);
			AssertContains("<td>신청문서 제출차수</td><td>1</td>", incomingMessage.EM_MessageInterpretation);
			AssertContains("2014-05-06 12:30:45", incomingMessage.EM_MessageInterpretation);
			AssertContains("접수결과내역등 결과내역을 기재(자유기재)", incomingMessage.EM_MessageInterpretation);
		}
		public void TestEmpty()
		{
			var incomingMessage = CreateMessageForTest("GOVCBRR67_Empty.xml");
			CreateEntryWithOutgoingMessageForImport();
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			AssertEquals("PreCondition: CusEntryNum Status is Empty", ZString.Empty, importEntryNum.CE_EntryStatus);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);

			AssertEquals("entry is located", miscHeader.PK, incomingMessage.EM_LinkUniqueID);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertNotContains("<td>신청문서 제출차수</td><td>1</td>", email.Body);
			AssertNotContains("2014-05-06 12:30:45", email.Body);
			AssertNotContains("접수결과내역등 결과내역을 기재(자유기재)", email.Body);

			AssertContains("확정가격신고 기간연장신청", incomingMessage.EM_MessageInterpretation);
			AssertContains("2014-05-06 12:30:46", incomingMessage.EM_MessageInterpretation);
			AssertContains("5SG-41644-2020-X000003", incomingMessage.EM_MessageInterpretation);
			AssertContains("<td>신청문서 제출차수</td><td>0</td>", incomingMessage.EM_MessageInterpretation);
			AssertContains("<td>신청문서 수신일시</td><td>&nbsp;</td>", incomingMessage.EM_MessageInterpretation);
			AssertContains("<td>접수내역</td><td>&nbsp;</td>", incomingMessage.EM_MessageInterpretation);
		}

		public void TestImportR67_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ImportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				var incomingMessage = CreateMessageForTest("GOVCBRR67_CUS.xml");
				Factory.Save();
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [심사 접수통보]5SG416442020X000003 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);

				AssertContains("확정가격신고 기간연장신청", incomingMessage.EM_MessageInterpretation);
				AssertContains("2014-05-06 12:30:46", incomingMessage.EM_MessageInterpretation);
				AssertContains("5SG-41644-2020-X000003", incomingMessage.EM_MessageInterpretation);
				AssertContains("<td>신청문서 제출차수</td><td>1</td>", incomingMessage.EM_MessageInterpretation);
				AssertContains("2014-05-06 12:30:45", incomingMessage.EM_MessageInterpretation);
				AssertContains("접수결과내역등 결과내역을 기재(자유기재)", incomingMessage.EM_MessageInterpretation);
			}
		}

		public void TestImportR67_NotificationSendertWithEntryButNoOutgoingMessage()
		{
			using (KRCustomsRegistry.Instance.ImportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				var incomingMessage = CreateMessageForTest("GOVCBRR67_CUS.xml");
				CreateEntryForImport(false);
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[심사 접수통보] Response for Declaration Number: MSC00000001 / 제출번호: 5SG416442020X000003", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [확정가격신고 기간연장신청]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);

				AssertContains("확정가격신고 기간연장신청", incomingMessage.EM_MessageInterpretation);
				AssertContains("2014-05-06 12:30:46", incomingMessage.EM_MessageInterpretation);
				AssertContains("5SG-41644-2020-X000003", incomingMessage.EM_MessageInterpretation);
				AssertContains("<td>신청문서 제출차수</td><td>1</td>", incomingMessage.EM_MessageInterpretation);
				AssertContains("2014-05-06 12:30:45", incomingMessage.EM_MessageInterpretation);
				AssertContains("접수결과내역등 결과내역을 기재(자유기재)", incomingMessage.EM_MessageInterpretation);
			}
		}

		public void TestImportR67_NotificationSendertWithEntryButNoOutgoingMessageWithBroker()
		{
			using (KRCustomsRegistry.Instance.ImportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				var incomingMessage = CreateMessageForTest("GOVCBRR67_CUS.xml");
				CreateEntryForImport(true);

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[심사 접수통보] Response for Declaration Number: MSC00000001 / 제출번호: 5SG416442020X000003", email.Subject);
				AssertEquals("CusAgent@wisetechglobal.com", email.Recipients[0].Email);

				AssertContains("확정가격신고 기간연장신청", incomingMessage.EM_MessageInterpretation);
				AssertContains("2014-05-06 12:30:46", incomingMessage.EM_MessageInterpretation);
				AssertContains("5SG-41644-2020-X000003", incomingMessage.EM_MessageInterpretation);
				AssertContains("<td>신청문서 제출차수</td><td>1</td>", incomingMessage.EM_MessageInterpretation);
				AssertContains("2014-05-06 12:30:45", incomingMessage.EM_MessageInterpretation);
				AssertContains("접수결과내역등 결과내역을 기재(자유기재)", incomingMessage.EM_MessageInterpretation);
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

		void CreateEntryForImport(bool hasBroker)
		{
			miscHeader = Factory.New<CusMiscRequestHeader>();
			miscHeader.CMR_RequestDate = ZDateTime.UtcNow;
			miscHeader.CMR_MessageType = ElectronicDocumentTypeList.Codes._5SG;
			miscHeader.CMR_CustomsOffice = "01020";
			miscHeader.CMR_GB = GlbBranch.CurrentBranch.PK;
			if (hasBroker)
			{
				miscHeader.CMR_GS_NKBroker = "AG";
			}

			importEntryNum = miscHeader.CreateCusEntryNumber();
			importEntryNum.CE_EntryNum = "5SG416442020X000003";
			importEntryNum.CE_EntryType = ElectronicDocumentTypeList.Codes._5SG;
			importEntryNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			importEntryNum.CE_ParentID = miscHeader.PK;
			importEntryNum.CE_ParentTable = CusMiscRequestHeader.Schema.TableName;
			importEntryNum.CE_EntryStatus = ZString.Empty;
			Factory.Save();
		}
		CusMiscRequestHeader miscHeader;
		CusEntryNumber importEntryNum;
		void CreateEntryWithOutgoingMessageForImport()
		{
			if (miscHeader == null)
			{
				CreateEntryForImport(false);
			}
			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5SG;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_LinkTable = CusEntryHeader.Schema.TableName;
			outgoingMessage.EM_LinkUniqueID = miscHeader.PK;
			outgoingMessage.EM_LinkedObject = miscHeader;
		}

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBRR67MessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._R67;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			return incomingMessage;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
	}
}
