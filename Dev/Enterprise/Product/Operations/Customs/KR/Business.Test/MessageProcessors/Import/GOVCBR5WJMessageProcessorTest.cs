using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5WJMessageProcessorTest : XMLMessageTestHelper<GOVCBR5WJMessageProcessorTest>
	{
		public void Test929()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR5WJ_CUS.xml");
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);

			AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);
			AssertContains("12345-20-000045M", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-09-07", incomingMessage.EM_MessageInterpretation);
			AssertContains("B-20-03805", incomingMessage.EM_MessageInterpretation);
			AssertContains("<td>란번호</td><td>1</td>", incomingMessage.EM_MessageInterpretation);
			AssertContains("<td>규격번호</td><td>1</td>", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-08-18", incomingMessage.EM_MessageInterpretation);
			AssertContains("BASES FOR BEVERAGE, NONALCOHOLIC", incomingMessage.EM_MessageInterpretation);
			AssertContains("ELDERFLOWER SYRUP 500ML", incomingMessage.EM_MessageInterpretation);
			AssertContains("NO", incomingMessage.EM_MessageInterpretation);
			AssertContains("SHAPE(02)/3244401 ELDERFLOWER DRINK CONCENTRATE PACKING:12JAR X 500ML/CT, USE:EDIBLE", incomingMessage.EM_MessageInterpretation);
			AssertContains("자세한 사항은 유니패스 Home&gt;정보조회&gt;통관정보&gt;수입&gt;분석진행사항 화면에서 조회하십시오.", incomingMessage.EM_MessageInterpretation);
			AssertContains("폐기", incomingMessage.EM_MessageInterpretation);
			AssertContains("즉시", incomingMessage.EM_MessageInterpretation);
			AssertContains("관세법인루시엔/최형수", incomingMessage.EM_MessageInterpretation);
			AssertContains("이케아코리아유한회사", incomingMessage.EM_MessageInterpretation);
			AssertContains("2106901090", incomingMessage.EM_MessageInterpretation);
			AssertContains("2106901090", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("12345-20-000045M", email.Body);
			AssertContains("2020-09-07", email.Body);
			AssertContains("B-20-03805", email.Body);
			AssertContains("001", email.Body);
			AssertContains("01", email.Body);
			AssertContains("2020-08-18", email.Body);
			AssertContains("BASES FOR BEVERAGE, NONALCOHOLIC", email.Body);
			AssertContains("ELDERFLOWER SYRUP 500ML", email.Body);
			AssertContains("NO", email.Body);
			AssertContains("SHAPE(02)/3244401 ELDERFLOWER DRINK CONCENTRATE PACKING:12JAR X 500ML/CT, USE:EDIBLE", email.Body);
			AssertContains("자세한 사항은 유니패스 Home&gt;정보조회&gt;통관정보&gt;수입&gt;분석진행사항 화면에서 조회하십시오.", email.Body);
			AssertContains("폐기", email.Body);
			AssertContains("즉시", email.Body);
			AssertContains("관세법인루시엔/최형수", email.Body);
			AssertContains("이케아코리아유한회사", email.Body);
			AssertContains("2106901090", email.Body);
			AssertContains("2106901090", email.Body);
		}

		public void TestEmpty()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR5WJ_Empty.xml");
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);

			AssertNoExceptionThrown("When Xml Element Values is Empty, system should still proceed successfully", () =>
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch());
			incomingMessage.Reload();

			AssertContains("incomingMessage EM_MessageInterpretation is updated", "수입신고서", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("BASES FOR BEVERAGE, NONALCOHOLIC", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("ELDERFLOWER SYRUP 500ML", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("NO", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("SHAPE(02)/3244401 ELDERFLOWER DRINK CONCENTRATE PACKING:12JAR X 500ML/CT, USE:EDIBLE", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("자세한 사항은 유니패스 Home&gt;정보조회&gt;통관정보&gt;수입&gt;분석진행사항 화면에서 조회하십시오.", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("폐기", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("즉시", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("관세법인루시엔/최형수", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("이케아코리아유한회사", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("2106901090", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertNotContains("BASES FOR BEVERAGE, NONALCOHOLIC", email.Body);
			AssertNotContains("ELDERFLOWER SYRUP 500ML", email.Body);
			AssertNotContains("NO", email.Body);
			AssertNotContains("SHAPE(02)/3244401 ELDERFLOWER DRINK CONCENTRATE PACKING:12JAR X 500ML/CT, USE:EDIBLE", email.Body);
			AssertNotContains("자세한 사항은 유니패스 Home&gt;정보조회&gt;통관정보&gt;수입&gt;분석진행사항 화면에서 조회하십시오.", email.Body);
			AssertNotContains("폐기", email.Body);
			AssertNotContains("즉시", email.Body);
			AssertNotContains("관세법인루시엔/최형수", email.Body);
			AssertNotContains("이케아코리아유한회사", email.Body);
			AssertNotContains("2106901090", email.Body);
		}

		public void TestImport5WJ_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				CreateMessageForTest("GOVCBR5WJ_CUS.xml");
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [분석결과안내문]1234520000045M 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		public void TestImport5WJ_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBR5WJ_CUS.xml");
				CreateEntryForImport(false);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[분석결과안내문] Response for Declaration Number: B00001000 / 제출번호: 1234520000045M", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [수입신고서]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);
			}
		}

		public void TestImport5WJ_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBR5WJ_CUS.xml");
				CreateEntryForImport(true);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[분석결과안내문] Response for Declaration Number: B00001000 / 제출번호: 1234520000045M", email.Subject);
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
			entryNumber.CE_EntryNum = "1234520000045M";
			entryNumber.CE_EntryType = "IMP";
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
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._929;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_LinkTable = CusEntryHeader.Schema.TableName;
			outgoingMessage.EM_LinkedObject = importEntry;

			return importEntry;
		}

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBR5WJMessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5WJ;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;

			return incomingMessage;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
	}
}
