using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5BEMessageProcessorTest : XMLMessageTestHelper<GOVCBR5BEMessageProcessorTest>
	{
		public void Test5BE()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR5BE_0.xml");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSOF", "Customs Office");
			helper.CreateNewOrGetExistingCusCodeType("CUSDP", "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, "CUSOF", "010", "서울세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, "CUSDP", "20", "내륙기지통관과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			var entryNumber = LoadCusEntryNum(entry);
			Assert("PreCondition: Entry Status is Empty", entryNumber.CE_EntryStatus.IsEmpty);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();
			entryNumber.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("message owner is updated", CustomsEntryStatusTypeList.Codes.ANT, incomingMessage.EM_MessageOwner);
			AssertNullOrEmpty("entry status is not updated", entryNumber.CE_EntryStatus);
			AssertContains("수입신고 수리전 반출 신고서", incomingMessage.EM_MessageInterpretation);
			AssertContains("6N002-20-000076M", incomingMessage.EM_MessageInterpretation);
			AssertContains("2014-05-06 12:30", incomingMessage.EM_MessageInterpretation);
			AssertContains("2014-05-06", incomingMessage.EM_MessageInterpretation);
			AssertContains("C 승인등록", incomingMessage.EM_MessageInterpretation);
			AssertContains("01 미조립상태 분할선적 물품", incomingMessage.EM_MessageInterpretation);
			AssertContains("승인기타사유", incomingMessage.EM_MessageInterpretation);
			AssertContains("11 승인전 납부", incomingMessage.EM_MessageInterpretation);
			AssertContains("[00] 과세보류", incomingMessage.EM_MessageInterpretation);
			AssertContains("[12] 수출보세공장반입", incomingMessage.EM_MessageInterpretation);
			AssertContains("김숙희", incomingMessage.EM_MessageInterpretation);
			AssertContains("[01020] 서울세관 내륙기지통관과", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("수입신고 수리전 반출 신고서", email.Body);
			AssertContains("6N002-20-000076M", email.Body);
			AssertContains("2014-05-06 12:30", email.Body);
			AssertContains("2014-05-06", email.Body);
			AssertContains("C 승인등록", email.Body);
			AssertContains("01 미조립상태 분할선적 물품", email.Body);
			AssertContains("승인기타사유", email.Body);
			AssertContains("11 승인전 납부", email.Body);
			AssertContains("[00] 과세보류", email.Body);
			AssertContains("[12] 수출보세공장반입", email.Body);
			AssertContains("김숙희", email.Body);
			AssertContains("[01020] 서울세관 내륙기지통관과", email.Body);
		}

		public void Test5BETestForEmptyData()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR5BE_OAC.xml");
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			var entryNumber = LoadCusEntryNum(entry);
			Assert("PreCondition: Entry Status is Empty", entryNumber.CE_EntryStatus.IsEmpty);

			AssertNoExceptionThrown(() => new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch());
			entry.Reload();
			incomingMessage.Reload();
			entryNumber.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertNullOrEmpty("incomingMessage EM_MessageOwner is not updated", incomingMessage.EM_MessageOwner);
			AssertNullOrEmpty("entry status is not updated", entryNumber.CE_EntryStatus);
			AssertEquals(incomingMessage.EM_ApplicationReference, entry.Messages[0].EM_MessageNum);

			AssertContains("수입신고 수리전 반출 신고서", incomingMessage.EM_MessageInterpretation);
			AssertContains("A 수신", incomingMessage.EM_MessageInterpretation);
			AssertContains("01", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("[]", incomingMessage.EM_MessageInterpretation);
			AssertContains("[010200]", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("A 수신", email.Body);
			AssertContains("01", email.Body);
			AssertNotContains("[]", email.Body);
			AssertContains("[010200]", email.Body);
		}

		public void Test5BEForDMS()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR5BE_DMS.xml");
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			var entryNumber = LoadCusEntryNum(entry);
			Assert("PreCondition: Entry Status is Empty", entryNumber.CE_EntryStatus.IsEmpty);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();
			entryNumber.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("message owner is updated", CustomsEntryStatusTypeList.Codes.DMS, incomingMessage.EM_MessageOwner);
			AssertNullOrEmpty("entry status is not updated", entryNumber.CE_EntryStatus);
			AssertEquals(incomingMessage.EM_ApplicationReference, entry.Messages[0].EM_MessageNum);

			AssertContains("수입신고 수리전 반출 신고서", incomingMessage.EM_MessageInterpretation);
			AssertContains("E 기각", incomingMessage.EM_MessageInterpretation);
			AssertContains("01", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("E 기각", email.Body);
			AssertContains("01", email.Body);
		}

		public void Test5BEForCCL()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR5BE_CCL.xml");
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			var entryNumber = LoadCusEntryNum(entry);
			Assert("PreCondition: Entry Status is Empty", entryNumber.CE_EntryStatus.IsEmpty);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();
			entryNumber.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("message owner is updated", CustomsEntryStatusTypeList.Codes.DMS, incomingMessage.EM_MessageOwner);
			AssertNullOrEmpty("entry status is not updated", entryNumber.CE_EntryStatus);
			AssertEquals(incomingMessage.EM_ApplicationReference, entry.Messages[0].EM_MessageNum);

			AssertContains("수입신고 수리전 반출 신고서", incomingMessage.EM_MessageInterpretation);
			AssertContains("G 승인취소", incomingMessage.EM_MessageInterpretation);
			AssertContains("01 징수형태 수정 오류", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("G 승인취소", email.Body);
			AssertContains("01 징수형태 수정 오류", email.Body);
		}

		public void TestImport5BE_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				CreateMessageForTest("GOVCBR5BE_0.xml");
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [수입신고 수리전 반출 처리결과 통보서]6N00220000076M 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		public void TestImport5BE_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBR5BE_0.xml");
				CreateEntryForImport(false);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수입신고 수리전 반출 처리결과 통보서] Response for Declaration Number: B00001000 / 제출번호: 6N00220000076M", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [수입수리전반출신청서]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);
			}
		}

		public void TestImport5BE_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBR5BE_0.xml");
				CreateEntryForImport(true);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수입신고 수리전 반출 처리결과 통보서] Response for Declaration Number: B00001000 / 제출번호: 6N00220000076M", email.Subject);
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
			entryNumber.CE_EntryNum = "6N00220000076M";
			entryNumber.CE_EntryType = "IMP";
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNumber.CE_ParentID = importEntry.PK;
			entryNumber.CE_ParentTable = CusEntryHeader.Schema.TableName;

			var cusEntryNum = importEntry.EntryNumbers.AddNew();
			cusEntryNum.CE_EntryNum = "6N00220000076M";
			cusEntryNum.CE_EntryType = ElectronicDocumentTypeList.Codes._5BD;
			cusEntryNum.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			cusEntryNum.CE_ParentID = importEntry.PK;
			cusEntryNum.CE_ParentTable = CusEntryHeader.Schema.TableName;
		}
		CusEntryHeader importEntry;

		CusEntryHeader CreateEntryWithOutgoingMessageForImport()
		{
			if (importEntry == null)
			{
				CreateEntryForImport(true);
			}
			var outgoingMessage = importEntry.Messages.AddNew();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5BD;
			outgoingMessage.EM_SystemCreateUser = "ORG";

			return importEntry;
		}

		CusEntryNumber LoadCusEntryNum(CusEntryHeader entry)
		{
			var numFilter = new ZQuery(CusEntryNumSchema.CE_ParentID, entry.PK);
			numFilter.AddToFilter(CusEntryNumSchema.CE_EntryType, ElectronicDocumentTypeList.Codes._5BD);
			var cusEntryNumber = Factory.LoadTop1<CusEntryNumber>(numFilter);

			return cusEntryNumber;
		}

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBR5BEMessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5BE;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			return incomingMessage;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
	}
}
