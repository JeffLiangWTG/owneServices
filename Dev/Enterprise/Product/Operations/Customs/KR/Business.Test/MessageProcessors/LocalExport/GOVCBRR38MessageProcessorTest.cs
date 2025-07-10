using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBRR38MessageProcessorTest : XMLMessageTestHelper<GOVCBRR38MessageProcessorTest>
	{
		public void Test5DP()
		{
			CreateEntryWithOutgoingMessageForLocalExport(KRJobMessageTypeList.Codes.LocalExport, ElectronicDocumentTypeList.Codes._5DP);
			var incomingMessage = CreateMessageForTest("GOVCBRR38_5DP.xml");
			SampleCodeType("016", "평택세관", "10", "통관지원(1)과");
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", localExportEntry.CH_Status.IsEmpty);
			Assert("PreCondition: Entry CustomsMessageRemarks is Empty", localExportEntry.CH_CustomsMessageRemarks.IsEmpty);

			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("message subType is DeclarationType", ElectronicDocumentTypeList.Codes._5DP, incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to OAC", CustomsMessageStatusTypeList.Codes.OriginalAccepted, localExportEntry.CH_Status);
			AssertEquals("entry BGMReference is updated correctly to ConfirmNumber", "01610200093751", localExportEntry.CH_BGMReference);
			AssertEquals("entry CustomsMessageRemarks is updated correctly to ContentDescription", "20201202133905\r\n세관담당자:윤동화, 서류제출생략", localExportEntry.CH_CustomsMessageRemarks);

			var outgoingMessage = localExportEntry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5DP);
			AssertEquals("message ApplicationReference is updated correctly to EM_MessageNum", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);

			var entryNum = localExportEntry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == KRJobMessageTypeList.Codes.LocalExport);
			AssertEquals("2020-12-02 13:38:46", entryNum.CE_IssueDate.ToString(DateFormatType.DateTimeKorean));

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("환급대상수출물품 반입확인 제출", email.Body);
			AssertContains("2020-12-02 13:39:05", email.Body);
			AssertContains("[01610] 평택세관 통관지원(1)과", email.Body);
			AssertContains("6N002-20-000052X", email.Body);
			AssertContains("01610200093751", email.Body);
			AssertContains("세관담당자:윤동화, 서류제출생략", email.Body);

			AssertContains("환급대상수출물품 반입확인 제출", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-12-02 13:39:05", incomingMessage.EM_MessageInterpretation);
			AssertContains("[01610] 평택세관 통관지원(1)과", incomingMessage.EM_MessageInterpretation);
			AssertContains("6N002-20-000052X", incomingMessage.EM_MessageInterpretation);
			AssertContains("01610200093751", incomingMessage.EM_MessageInterpretation);
			AssertContains("세관담당자:윤동화, 서류제출생략", incomingMessage.EM_MessageInterpretation);
		}

		public void TestEmptyData()
		{
			CreateEntryWithOutgoingMessageForLocalExport(KRJobMessageTypeList.Codes.LocalExport, ElectronicDocumentTypeList.Codes._5DR);
			var incomingMessage = CreateMessageForTest("GOVCBRR38_EmptyData.xml");

			var outgoingMessage = localExportEntry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5DR);
			outgoingMessage.EM_MessageSubType = "2";

			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", localExportEntry.CH_Status.IsEmpty);

			AssertNoExceptionThrown(() => new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage));

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("message subType is DeclarationType", ElectronicDocumentTypeList.Codes._5DR, incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to CAC", CustomsMessageStatusTypeList.Codes.CancellationAccepted, localExportEntry.CH_Status);
			Assert("Even if ConfirmNumber is empty, it works fine.", localExportEntry.CH_BGMReference.IsEmpty);
			Assert("Even if ContentDescription is empty, it works fine.", localExportEntry.CH_CustomsMessageRemarks.IsEmpty);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
		}

		public void Test5DR()
		{
			CreateEntryWithOutgoingMessageForLocalExport(KRJobMessageTypeList.Codes.LocalExport, ElectronicDocumentTypeList.Codes._5DR);
			var incomingMessage = CreateMessageForTest("GOVCBRR38_5DR.xml");
			SampleCodeType("040", "인천세관", "10", "통관지원(1)과");

			var outgoingMessage = localExportEntry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5DR);
			outgoingMessage.EM_MessageSubType = "1";
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", localExportEntry.CH_Status.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", localExportEntry.CH_EntryStatus.IsEmpty);
			Assert("PreCondition: Entry CustomsMessageRemarks is Empty", localExportEntry.CH_CustomsMessageRemarks.IsEmpty);

			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("message subType is DeclarationType", ElectronicDocumentTypeList.Codes._5DR, incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to AAC", CustomsMessageStatusTypeList.Codes.AmendmentAccepted, localExportEntry.CH_Status);
			AssertEquals("entry status is updated correctly to NDC", CustomsEntryStatusTypeList.Codes.NDC, localExportEntry.CH_EntryStatus);
			AssertNotEquals("entry BGMReference isn't updated correctly to ConfirmNumber", "04010200048611", localExportEntry.CH_BGMReference);
			AssertEquals(outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertNullOrEmpty("entry CustomsMessageRemarks isn't updated", localExportEntry.CH_CustomsMessageRemarks);

			var entryNum = localExportEntry.EntryNumbers.Cast<CusEntryNumber>().LastOrDefault(x => x.CE_EntryType == KRJobMessageTypeList.Codes.LocalExport);
			AssertEquals(ZDateTime.Empty, entryNum.CE_IssueDate);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("환급대상수출물품 반입확인 정정/취하 제출", email.Body);
			AssertContains("2020-12-02 10:46:33", email.Body);
			AssertContains("[04010] 인천세관 통관지원(1)과", email.Body);
			AssertContains("6N002-20-000052X", email.Body);
			AssertContains("04010200048611", email.Body);
			AssertContains("세관담당자:김분이, 서류제출대상", email.Body);
		}

		public void Test5DQ()
		{
			CreateEntryWithOutgoingMessageForLocalExport(KRJobMessageTypeList.Codes.LocalExport, ElectronicDocumentTypeList.Codes._5DQ);
			var incomingMessage = CreateMessageForTest("GOVCBRR38_5DQ.xml");
			SampleCodeType("010", "서울세관", "10", "통관지원(1)과");
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", localExportEntry.CH_Status.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", localExportEntry.CH_EntryStatus.IsEmpty);
			Assert("PreCondition: Entry CustomsMessageRemarks is Empty", localExportEntry.CH_CustomsMessageRemarks.IsEmpty);

			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("message subType is DeclarationType", ElectronicDocumentTypeList.Codes._5DQ, incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to OAC", CustomsMessageStatusTypeList.Codes.OriginalAccepted, localExportEntry.CH_Status);
			AssertEquals("entry status is updated correctly to NDC", CustomsEntryStatusTypeList.Codes.NDC, localExportEntry.CH_EntryStatus);
			AssertEquals("entry BGMReference is updated correctly to ConfirmNumber", "01610753524861", localExportEntry.CH_BGMReference);
			AssertEquals("entry CustomsMessageRemarks is updated correctly to ContentDescription", "20201202133905\r\n세관담당자:김세희, 서류제출대상", localExportEntry.CH_CustomsMessageRemarks);

			var outgoingMessage = localExportEntry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5DQ);
			AssertEquals("message ApplicationReference is updated correctly to EM_MessageNum", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);

			var entryNum = localExportEntry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == KRJobMessageTypeList.Codes.LocalExport);
			AssertEquals("2020-12-02 13:38:46", entryNum.CE_IssueDate.ToString(DateFormatType.DateTimeKorean));

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("환급대상수출물품 적재신청 제출", email.Body);
			AssertContains("2020-12-02 13:39:05", email.Body);
			AssertContains("[01010] 서울세관 통관지원(1)과", email.Body);
			AssertContains("6N002-20-000052X", email.Body);
			AssertContains("01610753524861", email.Body);
			AssertContains("세관담당자:김세희, 서류제출대상", email.Body);
		}

		public void Test5DS()
		{
			CreateEntryWithOutgoingMessageForLocalExport(KRJobMessageTypeList.Codes.LocalExport, ElectronicDocumentTypeList.Codes._5DS);
			var incomingMessage = CreateMessageForTest("GOVCBRR38_5DS.xml");
			SampleCodeType("030", "부산세관", "01", "경인항지소");

			var outgoingMessage = localExportEntry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5DS);
			outgoingMessage.EM_MessageSubType = "2";
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", localExportEntry.CH_Status.IsEmpty);
			Assert("PreCondition: Entry CustomsMessageRemarks is Empty", localExportEntry.CH_CustomsMessageRemarks.IsEmpty);

			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("message subType is DeclarationType", ElectronicDocumentTypeList.Codes._5DS, incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to CAC", CustomsMessageStatusTypeList.Codes.CancellationAccepted, localExportEntry.CH_Status);
			AssertNotEquals("entry BGMReference isn't updated correctly to ConfirmNumber", "03001201536925", localExportEntry.CH_BGMReference);
			AssertEquals(outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertNullOrEmpty("entry CustomsMessageRemarks isn't updated", localExportEntry.CH_CustomsMessageRemarks);

			var entryNum = localExportEntry.EntryNumbers.Cast<CusEntryNumber>().LastOrDefault(x => x.CE_EntryType == KRJobMessageTypeList.Codes.LocalExport);
			AssertEquals(ZDateTime.Empty, entryNum.CE_IssueDate);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("환급대상수출물품 적재신청 정정/취하 제출", email.Body);
			AssertContains("2020-12-02 10:46:33", email.Body);
			AssertContains("[03001] 부산세관 경인항지소", email.Body);
			AssertContains("6N002-20-000052X", email.Body);
			AssertContains("03001201536925", email.Body);
			AssertContains("세관담당자:김보라", email.Body);
		}

		public void TestDF3()
		{
			CreateEntryWithOutgoingMessageForLocalExport("DF3", ElectronicDocumentTypeList.Codes._DF3);
			var incomingMessage = CreateMessageForTest("GOVCBRR38_DF3.xml");
			SampleCodeType("010", "서울세관", "20", "내륙기지통관과");
			var outgoingMessage = localExportEntry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._DF3);

			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", localExportEntry.CH_Status.IsEmpty);
			Assert("PreCondition: Entry CustomsMessageRemarks is Empty", localExportEntry.CH_CustomsMessageRemarks.IsEmpty);

			var numFilter = new ZQuery(CusEntryNumSchema.CE_ParentID, localExportEntry.PK);
			numFilter.AddToFilter(CusEntryNumSchema.CE_EntryType, "DF3");
			var cusEntryNumber = localExportEntry.Factory.LoadTop1<CusEntryNumber>(numFilter);

			Assert("PreCondition: Entry Status is Empty", cusEntryNumber.CE_EntryStatus.IsEmpty);

			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("message subType is DeclarationType", ElectronicDocumentTypeList.Codes._DF3, incomingMessage.EM_MessageSubType);
			AssertEquals("entry status is updated correctly to OAC", CustomsMessageStatusTypeList.Codes.OriginalAccepted, cusEntryNumber.CE_EntryStatus);
			AssertNotEquals("entry BGMReference isn't updated correctly to ConfirmNumber", "01020352186391", localExportEntry.CH_BGMReference);
			AssertEquals(outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertNullOrEmpty("entry CustomsMessageRemarks isn't updated", localExportEntry.CH_CustomsMessageRemarks);

			var entryNum = localExportEntry.EntryNumbers.Cast<CusEntryNumber>().LastOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._DF3);
			AssertEquals(ZDateTime.Empty, entryNum.CE_IssueDate);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("환급대상수출물품 적재 완료보고서", email.Body);
			AssertContains("2020-12-08 11:35:15", email.Body);
			AssertContains("[01020] 서울세관 내륙기지통관과", email.Body);
			AssertContains("6N002-20-000052X", email.Body);
			AssertContains("01020352186391", email.Body);
			AssertContains("세관담당자:이화직", email.Body);
		}

		public void TestR38_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.LocalExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, localExportGroup.PK.ToGuid()))
			{
				SampleCodeType("016", "평택세관", "10", "통관지원(1)과");
				var incomingMessage = CreateMessageForTest("GOVCBRR38_5DP.xml");
				Factory.Save();
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [갈음 접수통보]6N00220000052X 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("LocalExportGroupTest@wisetechglobal.com", email.Recipients[0].Email);

				AssertContains("환급대상수출물품 반입확인 제출", incomingMessage.EM_MessageInterpretation);
				AssertContains("2020-12-02 13:39:05", incomingMessage.EM_MessageInterpretation);
				AssertContains("[01610] 평택세관 통관지원(1)과", incomingMessage.EM_MessageInterpretation);
				AssertContains("6N002-20-000052X", incomingMessage.EM_MessageInterpretation);
				AssertContains("01610200093751", incomingMessage.EM_MessageInterpretation);
				AssertContains("세관담당자:윤동화, 서류제출생략", incomingMessage.EM_MessageInterpretation);
			}
		}

		public void TestR38_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.LocalExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, localExportGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				SampleCodeType("016", "평택세관", "10", "통관지원(1)과");
				var incomingMessage = CreateMessageForTest("GOVCBRR38_5DP.xml");
				CreateEntryForLocalExport(false, KRJobMessageTypeList.Codes.LocalExport);
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[갈음 접수통보] Response for Declaration Number: B00001000 / 제출번호: 6N00220000052X", email.Subject);
				AssertEquals("LocalExportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [환급대상수출물품 반입확인 제출]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);

				AssertContains("환급대상수출물품 반입확인 제출", incomingMessage.EM_MessageInterpretation);
				AssertContains("2020-12-02 13:39:05", incomingMessage.EM_MessageInterpretation);
				AssertContains("[01610] 평택세관 통관지원(1)과", incomingMessage.EM_MessageInterpretation);
				AssertContains("6N002-20-000052X", incomingMessage.EM_MessageInterpretation);
				AssertContains("01610200093751", incomingMessage.EM_MessageInterpretation);
				AssertContains("세관담당자:윤동화, 서류제출생략", incomingMessage.EM_MessageInterpretation);
			}
		}

		public void TestR38_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.LocalExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, localExportGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				SampleCodeType("016", "평택세관", "10", "통관지원(1)과");
				var incomingMessage = CreateMessageForTest("GOVCBRR38_5DP.xml");
				CreateEntryForLocalExport(true, KRJobMessageTypeList.Codes.LocalExport);
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[갈음 접수통보] Response for Declaration Number: B00001000 / 제출번호: 6N00220000052X", email.Subject);
				AssertEquals("CusAgent@wisetechglobal.com", email.Recipients[0].Email);

				AssertContains("환급대상수출물품 반입확인 제출", incomingMessage.EM_MessageInterpretation);
				AssertContains("2020-12-02 13:39:05", incomingMessage.EM_MessageInterpretation);
				AssertContains("[01610] 평택세관 통관지원(1)과", incomingMessage.EM_MessageInterpretation);
				AssertContains("6N002-20-000052X", incomingMessage.EM_MessageInterpretation);
				AssertContains("01610200093751", incomingMessage.EM_MessageInterpretation);
				AssertContains("세관담당자:윤동화, 서류제출생략", incomingMessage.EM_MessageInterpretation);
			}
		}

		public void TestOriginalCH_VersionID()
		{
			var outgoingMessage = CreateEntryWithOutgoingMessageForLocalExport(KRJobMessageTypeList.Codes.LocalExport, ElectronicDocumentTypeList.Codes._5DP);
			outgoingMessage.EM_ApplicationReference = "1";

			localExportEntryNum.CE_EntryNum = "1083699012345";

			var incomingMessage = CreateMessageForTest("Wrapper_GOVCBRR38_5DP.xml");
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			AssertEquals("0 until R38 is received.", 0u, localExportEntry.CH_VersionID);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			localExportEntry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals(1u, localExportEntry.CH_VersionID);
		}

		public void TestAmendmentCH_VersionID()
		{
			var outgoingMessage = CreateEntryWithOutgoingMessageForLocalExport(KRJobMessageTypeList.Codes.LocalExport, ElectronicDocumentTypeList.Codes._5DP);
			outgoingMessage.EM_ApplicationReference = "1";

			localExportEntryNum.CE_EntryNum = "1083699012345";

			localExportEntryNum = localExportEntry.EntryNumbers.GetOrCreateCusEntryNum(KRJobMessageTypeList.Codes.LocalExport);
			localExportEntryNum.CE_EntryNum = "6N00220000052X";

			outgoingMessage = CreateEntryWithOutgoingMessageForLocalExport(KRJobMessageTypeList.Codes.LocalExport, ElectronicDocumentTypeList.Codes._5DR);
			outgoingMessage.EM_ApplicationReference = "3";
			localExportEntry.CH_VersionID = 2;

			var incomingMessage = CreateMessageForTest("GOVCBRR38_5DR.xml");
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			AssertEquals("2 until R38 is received.", 2u, localExportEntry.CH_VersionID);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			localExportEntry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals(3u, localExportEntry.CH_VersionID);
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
		void SampleCodeType(ZString cusCode, ZString cusDescription, ZString secCode, ZString secDescription)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CUSOF", "Customs Office");
			helper.CreateNewOrGetExistingCusCodeType("CUSDP", "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, "CUSOF", cusCode, cusDescription, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, "CUSDP", secCode, secDescription, ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
		}

		void CreateEntryForLocalExport(bool setCusAgent, ZString entryType)
		{
			var declaration = Factory.New<JobDeclaration>();
			if (setCusAgent)
			{
				declaration.JE_GS_NKCusAgent = "AG";
			}
			localExportEntry = declaration.CustomsEntryHeaders.AddNew();
			localExportEntryNum = localExportEntry.EntryNumbers.GetOrCreateCusEntryNum(entryType);
			localExportEntryNum.CE_EntryNum = "6N00220000052X";
			Factory.Save();
		}
		CusEntryHeader localExportEntry;
		CusEntryNumber localExportEntryNum;

		EDIMessage CreateEntryWithOutgoingMessageForLocalExport(string entryType, string em_MessageType)
		{
			if (localExportEntry == null)
			{
				CreateEntryForLocalExport(true, entryType);
			}
			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = em_MessageType;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_LinkedObject = localExportEntry;

			return outgoingMessage;
		}

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBRR38MessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._R38;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			return incomingMessage;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.LocalExport.Incoming";
	}
}
