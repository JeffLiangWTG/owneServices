using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5SHMessageProcessorTest : XMLMessageTestHelper<GOVCBR5SHMessageProcessorTest>
	{
		public void Test5SH_DMS()
		{
			var incomingMessage = SetDataAndCheck("GOVCBR5SH_E.xml");

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			Factory.Save();
			miscHeader.Reload();
			incomingMessage.Reload();

			AssertEquals(miscHeader.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals(CustomsEntryStatusTypeList.Codes.DMS, incomingMessage.EM_MessageOwner);
			AssertContains("incomingMessage EM_MessageInterpretation is updated", "확정가격신고 기간 연장 신청서", incomingMessage.EM_MessageInterpretation);
			AssertEquals(outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);

			var entryNum_5SG = miscHeader.CusEntryNumber;
			entryNum_5SG.Reload();
			AssertEquals(new ZDateTime("2021-07-08"), entryNum_5SG.CE_IssueDate);

			var entryNum_934 = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._934);
			entryNum_934.Reload();
			AssertEquals(new ZDateTime("2021-07-01"), entryNum_934.CE_ExpiryDate);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("확정가격신고 기간 연장 신청서", email.Body);
			AssertContains("5SG123452015X000001", email.Body);
			AssertContains("2021-07-08", email.Body);
			AssertContains("[E] 기각", email.Body);
			AssertContains("확정가격신고 기각 사유", email.Body);
			AssertContains("김유중", email.Body);
			AssertContains("[030] 부산세관", email.Body);
			AssertContains("12345-20-000045M", email.Body);
			AssertContains("2021-07-07", email.Body);
		}

		public void Test5SG_ANT()
		{
			var incomingMessage = SetDataAndCheck("GOVCBR5SH_C.xml");

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			Factory.Save();
			miscHeader.Reload();
			incomingMessage.Reload();

			AssertEquals(miscHeader.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals(CustomsEntryStatusTypeList.Codes.ANT, incomingMessage.EM_MessageOwner);
			AssertContains("incomingMessage EM_MessageInterpretation is updated", "확정가격신고 기간 연장 신청서", incomingMessage.EM_MessageInterpretation);
			AssertEquals(outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);

			var entryNum_5SG = miscHeader.CusEntryNumber;
			entryNum_5SG.Reload();
			AssertEquals(new ZDateTime("2021-07-08"), entryNum_5SG.CE_IssueDate);

			var entryNum_934 = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._934);
			entryNum_934.Reload();
			AssertEquals(new ZDateTime("2021-07-07"), entryNum_934.CE_ExpiryDate);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("확정가격신고 기간 연장 신청서", email.Body);
			AssertContains("5SG123452015X000001", email.Body);
			AssertContains("2021-07-08", email.Body);
			AssertContains("[C] 승인", email.Body);
			AssertContains("확정가격신고 승인 사유", email.Body);
			AssertContains("김유중", email.Body);
			AssertContains("[030] 부산세관", email.Body);
			AssertContains("12345-20-000045M", email.Body);
			AssertContains("2021-07-07", email.Body);
		}

		EDIMessage SetDataAndCheck(string file)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "030", "부산세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest(file);
			Factory.Save();

			Assert(incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert(incomingMessage.EM_MessageOwner.IsEmpty);
			Assert(incomingMessage.EM_ApplicationReference.IsEmpty);

			var entryNum_5SG = miscHeader.CusEntryNumber;
			Assert(entryNum_5SG.CE_IssueDate.IsEmpty);
			var entryNum_934 = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._934);
			AssertEquals(new ZDateTime("2021-07-01"), entryNum_934.CE_ExpiryDate);

			return incomingMessage;
		}

		public void Test11Data()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR5SH_11Data.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();
			miscHeader.Reload();

			AssertEquals(CustomsEntryStatusTypeList.Codes.ANT, incomingMessage.EM_MessageOwner);
			AssertContains("incomingMessage EM_MessageInterpretation is updated", "확정가격신고 기간 연장 신청서", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("나머지 내역은 프로그램에서 확인하십시오.", incomingMessage.EM_MessageInterpretation);
			AssertContains("12345-20-000011M", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("나머지 내역은 프로그램에서 확인 하십시오.", email.Body);
			AssertNotContains("12345-20-000011M", email.Body);
		}

		public void TestEntryReleaseDate()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			CreateEntryWithOutgoingMessageForImport();
			var line = miscHeader.RequestLines.AddNew();
			line.CML_EntryType = ReferenceNumberTypeList.Codes.IMP;
			line.CML_EntryNumber = "1234520000001M";
			line.CML_Remarks = "(2021-08-02) 연장신청사유1";

			line = miscHeader.RequestLines.AddNew();
			line.CML_EntryType = ReferenceNumberTypeList.Codes.IMP;
			line.CML_EntryNumber = "1234520000002M";
			line.CML_Remarks = "(2021-08-02) 연장신청사유2\r\n연장신청사유상세";

			line = miscHeader.RequestLines.AddNew();
			line.CML_EntryType = ReferenceNumberTypeList.Codes.IMP;
			line.CML_EntryNumber = "1234520000003M";
			line.CML_Remarks = ZString.Empty;

			line = miscHeader.RequestLines.AddNew();
			line.CML_EntryType = ReferenceNumberTypeList.Codes.IMP;
			line.CML_EntryNumber = "1234520000004M";
			line.CML_Remarks = "(2021-08-02) 연장신청사유4";
			Factory.Save();

			var incomingMessage = CreateMessageForTest("GOVCBR5SH_Remark.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();
			miscHeader.Reload();

			var lines = miscHeader.RequestLines;
			lines[0].Reload();
			AssertEquals("1234520000001M", lines[0].CML_EntryNumber);
			AssertEquals(new ZString("연장수리일자 : 2021-07-07\r\n(2021-08-02) 연장신청사유1"), lines[0].CML_Remarks);

			lines[1].Reload();
			AssertEquals("1234520000002M", lines[1].CML_EntryNumber);
			AssertEquals(new ZString("연장수리일자 : 2021-07-08\r\n(2021-08-02) 연장신청사유2\r\n연장신청사유상세"), lines[1].CML_Remarks);

			lines[2].Reload();
			AssertEquals("1234520000003M", lines[2].CML_EntryNumber);
			AssertEquals(new ZString("연장수리일자 : 2021-07-09"), lines[2].CML_Remarks);

			lines[3].Reload();
			AssertEquals("1234520000004M", lines[3].CML_EntryNumber);
			AssertEquals(new ZString("(2021-08-02) 연장신청사유4"), lines[3].CML_Remarks);
		}

		public void TestEmptyData()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR5SH_Empty.xml");
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);

			AssertNoExceptionThrown(() => new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch());
			miscHeader.Reload();
			incomingMessage.Reload();

			AssertContains("incomingMessage EM_MessageInterpretation is updated", "확정가격신고 기간 연장 신청서", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("[0300]", email.Body);
		}

		public void TestImport5SH_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				var incomingMessage = CreateMessageForTest("GOVCBR5SH_E.xml");
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				incomingMessage.Reload();

				AssertContains("incomingMessage EM_MessageInterpretation is updated", "확정가격신고 기각 사유", incomingMessage.EM_MessageInterpretation);

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [확정가격신고기간 연장 신청 결과통보]5SG123452015X000001 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		public void TestImport5SH_NotificationSendertWithEntryButNoOutgoingMessage()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				var incomingMessage = CreateMessageForTest("GOVCBR5SH_E.xml");
				CreateEntryForImport(false);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				incomingMessage.Reload();

				AssertContains("incomingMessage EM_MessageInterpretation is updated", "확정가격신고 기각 사유", incomingMessage.EM_MessageInterpretation);

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[확정가격신고기간 연장 신청 결과통보] Response for Declaration Number: MSC00000001 / 제출번호: 5SG123452015X000001", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [확정가격신고 기간연장신청]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);
			}
		}

		public void TestImport5SH_NotificationSendertWithEntryButNoOutgoingMessageWithBroker()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				var incomingMessage = CreateMessageForTest("GOVCBR5SH_E.xml");
				CreateEntryForImport(true);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				incomingMessage.Reload();

				AssertContains("incomingMessage EM_MessageInterpretation is updated", "확정가격신고 기각 사유", incomingMessage.EM_MessageInterpretation);

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[확정가격신고기간 연장 신청 결과통보] Response for Declaration Number: MSC00000001 / 제출번호: 5SG123452015X000001", email.Subject);
				AssertEquals("CusAgent@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		public void TestLastOutgoingMessage()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			CreateEntryWithOutgoingMessageForImport();

			var outgoingMessage1 = Factory.New<EDIMessage>();
			outgoingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage1.EM_MessageType = ElectronicDocumentTypeList.Codes._5BC;
			outgoingMessage1.EM_SystemCreateUser = "ORG";
			outgoingMessage1.EM_LinkTable = CusEntryHeader.Schema.TableName;
			outgoingMessage1.EM_LinkedObject = miscHeader;

			var incomingMessage = CreateMessageForTest("GOVCBR5SH_C.xml");
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "030", "부산세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			Factory.Save();
			miscHeader.Reload();
			incomingMessage.Reload();

			AssertEquals(outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
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

		void CreateEntryForImport(bool hasBroker)
		{
			entry = Factory.NewWithValidTestData<CusEntryHeader>();
			entry.CH_MessageType = KRJobMessageTypeList.Codes.Import;

			var entryNumber = entry.EntryNumbers.GetOrCreateCusEntryNum(KRJobMessageTypeList.Codes.Import);
			entryNumber.CE_EntryNum = "1234520000045M";
			entryNumber.CE_ExpiryDate = ZDateTime.Empty;

			var entryNumber934 = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._934);
			entryNumber934.CE_ExpiryDate = new ZDateTime("2021-07-01");

			miscHeader = Factory.New<CusMiscRequestHeader>();
			miscHeader.CMR_RequestDate = ZDateTime.UtcNow;
			miscHeader.CMR_MessageType = ElectronicDocumentTypeList.Codes._5SG;
			miscHeader.CMR_CustomsOffice = "01020";
			miscHeader.CMR_GB = GlbBranch.CurrentBranch.PK;
			if (hasBroker)
			{
				miscHeader.CMR_GS_NKBroker = "AG";
			}

			var entryNumber5SG = miscHeader.CreateCusEntryNumber();
			entryNumber5SG.CE_EntryNum = "5SG123452015X000001";
			entryNumber5SG.CE_EntryType = ElectronicDocumentTypeList.Codes._5SG;
			entryNumber5SG.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNumber5SG.CE_ParentID = miscHeader.PK;
			entryNumber5SG.CE_ParentTable = CusMiscRequestHeader.Schema.TableName;
		}
		CusMiscRequestHeader miscHeader;
		CusEntryHeader entry;

		void CreateEntryWithOutgoingMessageForImport()
		{
			if (miscHeader == null)
			{
				CreateEntryForImport(false);
			}
			outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5SG;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_LinkTable = CusEntryHeader.Schema.TableName;
			outgoingMessage.EM_LinkedObject = miscHeader;
		}
		EDIMessage outgoingMessage;

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBR5SHMessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5SH;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			return incomingMessage;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
	}
}
