using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5UNMessageProcessorTest : XMLMessageTestHelper<GOVCBR5UNMessageProcessorTest>
	{
		public void Test5UN()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR5UN_CUS.xml");
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);

			var outgoingMessage = entry.Messages.LastOutgoingMessage;
			AssertEquals(outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);

			AssertContains("과오납 환급 신청서", incomingMessage.EM_MessageInterpretation);
			AssertContains("416341950319U", incomingMessage.EM_MessageInterpretation);
			AssertContains("엘지전자(주)", incomingMessage.EM_MessageInterpretation);
			AssertContains("정도현조성진", incomingMessage.EM_MessageInterpretation);
			AssertContains("서울특별시 영등포구 여의대로 128(여의도동)", incomingMessage.EM_MessageInterpretation);
			AssertContains("상세주소입니다.", incomingMessage.EM_MessageInterpretation);
			AssertContains("부산세관", incomingMessage.EM_MessageInterpretation);
			AssertContains("심사정보과", incomingMessage.EM_MessageInterpretation);
			AssertContains("2019-12-19", incomingMessage.EM_MessageInterpretation);
			AssertContains("030751927452", incomingMessage.EM_MessageInterpretation);
			AssertContains("030601", incomingMessage.EM_MessageInterpretation);
			AssertContains("030211900030036", incomingMessage.EM_MessageInterpretation);
			AssertContains("결정액", incomingMessage.EM_MessageInterpretation);
			AssertContains("충당액", incomingMessage.EM_MessageInterpretation);
			AssertContains("잔액", incomingMessage.EM_MessageInterpretation);
			AssertContains("<tr><td align=\"left\">관세</td><td align=\"right\">1094050</td><td align=\"right\">1094270</td><td align=\"right\">0</td></tr>", incomingMessage.EM_MessageInterpretation);
			AssertContains("<tr><td align=\"left\">부가세</td><td align=\"right\">2388680</td><td align=\"right\">2388460</td><td align=\"right\">0</td></tr>", incomingMessage.EM_MessageInterpretation);
			AssertContains("<tr><td align=\"left\">합계</td><td align=\"right\">3482730</td><td align=\"right\">3482730</td><td align=\"right\">0</td></tr></table>", incomingMessage.EM_MessageInterpretation);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("과오납 환급 신청서", email.Body);
			AssertContains("416341950319U", email.Body);
			AssertContains("엘지전자(주)", email.Body);
			AssertContains("정도현조성진", email.Body);
			AssertContains("서울특별시 영등포구 여의대로 128(여의도동)", email.Body);
			AssertContains("상세주소입니다.", email.Body);
			AssertContains("부산세관", email.Body);
			AssertContains("심사정보과", email.Body);
			AssertContains("2019-12-19", email.Body);
			AssertContains("030751927452", email.Body);
			AssertContains("030601", email.Body);
			AssertContains("030211900030036", email.Body);
			AssertContains("결정액", email.Body);
			AssertContains("충당액", email.Body);
			AssertContains("잔액", email.Body);
			AssertContains("<tr><td align=\"left\">관세</td><td align=\"right\">1094050</td><td align=\"right\">1094270</td><td align=\"right\">0</td></tr>", email.Body);
			AssertContains("<tr><td align=\"left\">부가세</td><td align=\"right\">2388680</td><td align=\"right\">2388460</td><td align=\"right\">0</td></tr>", email.Body);
			AssertContains("<tr><td align=\"left\">합계</td><td align=\"right\">3482730</td><td align=\"right\">3482730</td><td align=\"right\">0</td></tr></table>", email.Body);
		}

		public void Test5UN_RefundDeclaration()
		{
			var refundDeclaration = new TestDataSetupHelper(Factory).CreateRefundDeclarationWithOutgoingMessage("416341950319U");
			var incomingMessage = CreateMessageForTest("GOVCBR5UN_CUS.xml");
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			refundDeclaration.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", refundDeclaration.PK, incomingMessage.EM_LinkUniqueID);

			var outgoingMessage = (EDIMessage)refundDeclaration.Messages.LastOutgoingMessage;
			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("과오납 환급 신청서", email.Body);
			AssertContains("416341950319U", email.Body);
		}

		public void TestEmpty()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR5UN_Empty.xml");
			Factory.Save();

			AssertNoExceptionThrown("When Xml Element Values is Empty, system should still proceed successfully", () =>
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch());
			incomingMessage.Reload();

			AssertContains("incomingMessage EM_MessageInterpretation is updated", "과오납 환급 신청서", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("상세주소입니다.", incomingMessage.EM_MessageInterpretation);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertNotContains("상세주소입니다.", email.Body);
		}

		public void TestDutyTaxFeeIsRandom()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR5UN_DutyTaxFeeIsRandom.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertContains("과오납 환급 신청서", incomingMessage.EM_MessageInterpretation);
			AssertContains("<tr><td align=\"left\">관세</td><td align=\"right\">1094050</td><td align=\"right\">1094270</td><td align=\"right\">0</td></tr>", incomingMessage.EM_MessageInterpretation);
			AssertContains("<tr><td align=\"left\">부가세</td><td align=\"right\">2388680</td><td align=\"right\">2388460</td><td align=\"right\">0</td></tr>", incomingMessage.EM_MessageInterpretation);
			AssertContains("<tr><td align=\"left\">합계</td><td align=\"right\">3482730</td><td align=\"right\">3482730</td><td align=\"right\">0</td></tr></table>", incomingMessage.EM_MessageInterpretation);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertContains("<tr><td align=\"left\">관세</td><td align=\"right\">1094050</td><td align=\"right\">1094270</td><td align=\"right\">0</td></tr>", email.Body);
			AssertContains("<tr><td align=\"left\">부가세</td><td align=\"right\">2388680</td><td align=\"right\">2388460</td><td align=\"right\">0</td></tr>", email.Body);
			AssertContains("<tr><td align=\"left\">합계</td><td align=\"right\">3482730</td><td align=\"right\">3482730</td><td align=\"right\">0</td></tr></table>", email.Body);
		}

		public void TestImport5UN_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				CreateMessageForTest("GOVCBR5UN_CUS.xml");
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [충당통지서]416341950319U 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		public void TestImport5UN_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBR5UN_CUS.xml");
				CreateEntryForImport(false);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[충당통지서] Response for Declaration Number: B00001000 / 제출번호: 416341950319U", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [과오납 환급신청서]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);
			}
		}

		public void TestImport5UN_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBR5UN_CUS.xml");
				CreateEntryForImport(true);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[충당통지서] Response for Declaration Number: B00001000 / 제출번호: 416341950319U", email.Subject);
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
			staff2.GS_Code = "T2";
			staff2.GS_LoginName = "Test2";
			staff2.GS_EmailAddress = "ImportGroupTest@wisetechglobal.com";
			importGroup = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			var link2 = Factory.New<GlbGroupLink>();
			link2.GK_GG = importGroup.PK;
			link2.GK_GS = staff2.PK;

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
			entryNumber.CE_EntryNum = "416341950319U";
			entryNumber.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNumber.CE_ParentID = importEntry.PK;
			entryNumber.CE_ParentTable = CusEntryHeader.Schema.TableName;
		}
		CusEntryHeader importEntry;

		CusEntryHeader CreateEntryWithOutgoingMessageForImport()
		{
			if (importEntry == null)
			{
				CreateEntryForImport(true);
			}
			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5UL;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_LinkTable = CusEntryHeader.Schema.TableName;
			outgoingMessage.EM_LinkedObject = importEntry;
			outgoingMessage.EM_MessageOwner = "416341950319U";

			return importEntry;
		}

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBR5UNMessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5UN;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;

			return incomingMessage;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
	}
}
