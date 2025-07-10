using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5AAMessageProcessorTest : XMLMessageTestHelper<GOVCBR5AAMessageProcessorTest>
	{
		public void TestOriginal()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5AA_CUS.xml");
			CreateEntryWithOutgoingMessageForExport();
			var entryNum = exportEntry.EntryNumbers.GetOrCreateCusEntryNum("EXP");
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			AssertEquals("CusENtryNum CE_ExpiryDate is Empty", ZString.Empty, entryNum.CE_ExpiryDate.ToString(DateFormatType.DateKorean));
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			exportEntry.Reload();
			incomingMessage.Reload();
			entryNum.Reload();

			AssertEquals("entry is located", exportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to CLR", "CLR", exportEntry.CH_EntryStatus);
			AssertEquals("entry issuedatetiem is updated correctly to 2020-08-10 12:30", "2020-08-10 12:30", exportEntry.CH_EntryReleaseDate.ToString(DateFormatType.DateTimeKoreanNoSecond));
			AssertEquals("CusENtryNum CE_ExpiryDate is updated correctly to 2020-09-09", "2020-09-09", entryNum.CE_ExpiryDate.ToString(DateFormatType.DateKorean));

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("2020-08-10 12:30", email.Body);
			AssertContains("6N00220000052X", email.Body);
			AssertContains("2020-09-09", email.Body);
			AssertContains("10700000", email.Body);
			AssertContains("8983", email.Body);
			AssertContains("중고자동차가 컨테이너(FR컨테이너 포함)에 적입되어 수출되는 경우에는 관세법 제243조 제4항에 따른 보세구역 반입 후 수출신고 대상이므로 이를 위반할 경우 동법 제277조제3항제3호에 따라 과태료가 부과될 수 있음을 알려드립니다.", email.Body);
			AssertContains("특이사항", email.Body);
			AssertContains("도로명표기안내", email.Body);
		}
		public void TestWithConditionalElements()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5AA_CUS_Empty.xml");
			CreateEntryWithOutgoingMessageForExport();
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			exportEntry.Reload();
			incomingMessage.Reload();

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("2020-08-10 12:30", email.Body);
			AssertContains("6N00220000052X", email.Body);
			AssertContains("2020-09-09", email.Body);
			AssertContains("10700000", email.Body);
			AssertContains("8983", email.Body);
			AssertNotContains("중고자동차가 컨테이너(FR컨테이너 포함)에 적입되어 수출되는 경우에는 관세법 제243조 제4항에 따른 보세구역 반입 후 수출신고 대상이므로 이를 위반할 경우 동법 제277조제3항제3호에 따라 과태료가 부과될 수 있음을 알려드립니다.", email.Body);
			AssertEquals("entry customs message is not updated", ZString.Empty, exportEntry.CH_CustomsMessageRemarks);
		}

		public void TestExport5AA_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				CreateMessageForTest("GOVCBR5AA_CUS.xml");
				Factory.Save();
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [수출 수리통보]6N00220000052X 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ExportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		public void TestExport5AA_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBR5AA_CUS.xml");
				CreateEntryForExport(false);
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수출 수리통보] Response for Declaration Number: B00001000 / 제출번호: 6N00220000052X", email.Subject);
				AssertEquals("ExportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [수출신고서]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);
			}
		}

		public void TestExport5AA_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBR5AA_CUS.xml");
				CreateEntryForExport(true);
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수출 수리통보] Response for Declaration Number: B00001000 / 제출번호: 6N00220000052X", email.Subject);
				AssertEquals("CusAgent@wisetechglobal.com", email.Recipients[0].Email);
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

		void CreateEntryForExport(bool setCusAgent)
		{
			var declaration = Factory.New<JobDeclaration>();
			if (setCusAgent)
			{
				declaration.JE_GS_NKCusAgent = "AG";
			}
			exportEntry = declaration.CustomsEntryHeaders.AddNew();
			var entryNumber = exportEntry.EntryNumbers.AddNew();
			entryNumber.CE_EntryNum = "6N00220000052X";
			entryNumber.CE_EntryType = "EXP";
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNumber.CE_ParentID = exportEntry.PK;
			entryNumber.CE_ParentTable = CusEntryHeader.Schema.TableName;
			entryNumber.CE_EntryStatus = ZString.Empty;
			Factory.Save();
		}
		CusEntryHeader exportEntry;

		void CreateEntryWithOutgoingMessageForExport()
		{
			if (exportEntry == null)
			{
				CreateEntryForExport(true);
			}
			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._830;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_LinkTable = CusEntryHeader.Schema.TableName;
			outgoingMessage.EM_LinkUniqueID = exportEntry.PK;
			outgoingMessage.EM_LinkedObject = exportEntry;
		}

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBR5AAMessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5AA;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;

			return incomingMessage;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Export.Incoming";
	}
}
