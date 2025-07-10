using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5TVMessageProcessorTest : XMLMessageTestHelper<GOVCBR5TVMessageProcessorTest>
	{
		public void Test5TV()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR5TV_CUS.xml");
			SampleCodeType();
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);

			AssertContains("수입(납세)신고 정정 신청서", incomingMessage.EM_MessageInterpretation);
			AssertContains("010-14-546123", incomingMessage.EM_MessageInterpretation);
			AssertContains("[01020] 서울세관 내륙기지통관과", incomingMessage.EM_MessageInterpretation);
			AssertContains("나세관", incomingMessage.EM_MessageInterpretation);
			AssertContains("나과장", incomingMessage.EM_MessageInterpretation);
			AssertContains("041-658-9856", incomingMessage.EM_MessageInterpretation);
			AssertContains("6N002-20-000084M", incomingMessage.EM_MessageInterpretation);
			AssertContains("2014-01-01", incomingMessage.EM_MessageInterpretation);
			AssertContains("123", incomingMessage.EM_MessageInterpretation);
			AssertContains("9999999", incomingMessage.EM_MessageInterpretation);
			AssertContains("보정사유(유형)기재", incomingMessage.EM_MessageInterpretation);
			AssertContains("2014-05-06", incomingMessage.EM_MessageInterpretation);
			AssertContains("2014-08-09", incomingMessage.EM_MessageInterpretation);
			AssertContains("[010] 서울세관", incomingMessage.EM_MessageInterpretation);
			AssertContains("땡땡상사", incomingMessage.EM_MessageInterpretation);
			AssertContains("김땡땡", incomingMessage.EM_MessageInterpretation);
			AssertContains("첨부서류 기재", incomingMessage.EM_MessageInterpretation);
			AssertContains("12345", incomingMessage.EM_MessageInterpretation);
			AssertContains("보정품목 내역은 프로그램에서 확인 하십시오.", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("수입(납세)신고 정정 신청서", email.Body);
			AssertContains("010-14-546123", email.Body);
			AssertContains("[01020] 서울세관 내륙기지통관과", email.Body);
			AssertContains("나세관", email.Body);
			AssertContains("나과장", email.Body);
			AssertContains("041-658-9856", email.Body);
			AssertContains("6N002-20-000084M", email.Body);
			AssertContains("2014-01-01", email.Body);
			AssertContains("123", email.Body);
			AssertContains("9999999", email.Body);
			AssertContains("보정사유(유형)기재", email.Body);
			AssertContains("2014-05-06", email.Body);
			AssertContains("2014-08-09", email.Body);
			AssertContains("[010] 서울세관", email.Body);
			AssertContains("땡땡상사", email.Body);
			AssertContains("김땡땡", email.Body);
			AssertContains("첨부서류 기재", email.Body);
			AssertContains("12345", email.Body);
			AssertContains("보정품목 내역은 프로그램에서 확인 하십시오.", email.Body);
		}

		public void TestEmpty()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR5TV_Empty.xml");
			Factory.Save();

			AssertNoExceptionThrown("When Xml Element Values is Empty, system should still proceed successfully", () =>
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch());
			incomingMessage.Reload();

			AssertContains("incomingMessage EM_MessageInterpretation is updated", "수입(납세)신고 정정 신청서", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("나세관", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("나과장", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("첨부서류 기재", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("12345", incomingMessage.EM_MessageInterpretation);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertNotContains("나세관", email.Body);
			AssertNotContains("나과장", email.Body);
			AssertNotContains("첨부서류 기재", email.Body);
			AssertNotContains("12345", email.Body);
		}

		public void TestImport5TV_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				CreateMessageForTest("GOVCBR5TV_CUS.xml");
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [보정신청통지서]6N00220000084M 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		public void TestImport5TV_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBR5TV_CUS.xml");
				CreateEntryForImport(false);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[보정신청통지서] Response for Declaration Number: B00001000 / 제출번호: 6N00220000084M", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [수입정정신고서]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);
			}
		}

		public void TestImport5TV_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBR5TV_CUS.xml");
				CreateEntryForImport(true);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[보정신청통지서] Response for Declaration Number: B00001000 / 제출번호: 6N00220000084M", email.Subject);
				AssertEquals("CusAgent@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		public void Test5TV_CreateAndUpdate()
		{
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR5TV_CUS.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			var entryLoaded = new BusinessObjectFactory().Load<CusEntryHeader>(entry.PK);
			var cusEntryNumber = entryLoaded.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5TV);

			AssertEquals("01014546123", cusEntryNumber.CE_EntryNum);
			AssertEquals("2014-05-06", cusEntryNumber.CE_IssueDate.ToString(DateFormatType.DateKorean));
			AssertEquals("2014-08-09", cusEntryNumber.CE_ExpiryDate.ToString(DateFormatType.DateKorean));
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
			entryNumber.CE_EntryNum = "6N00220000084M";
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
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5FE;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_LinkTable = CusEntryHeader.Schema.TableName;
			outgoingMessage.EM_LinkedObject = importEntry;

			return importEntry;
		}

		void SampleCodeType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "010", "서울세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "20", "내륙기지통관과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
		}

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBR5TVMessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5TV;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			return incomingMessage;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
	}
}
