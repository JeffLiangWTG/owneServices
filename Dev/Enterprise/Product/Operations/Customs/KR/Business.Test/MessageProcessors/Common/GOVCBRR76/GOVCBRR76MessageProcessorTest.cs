using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBRR76MessageProcessorTest : XMLMessageTestHelper<GOVCBRR76MessageProcessorTest>
	{
		public void TestR76_OutgoingMessageIs5GW()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var requestHeader = CreateEntryWithOutgoingMessage(ElectronicDocumentTypeList.Codes._5GW, "41634200000072U", ElectronicDocumentTypeList.Codes._5GW);
			var incomingMessage = CreateMessageForTest("GOVCBRR76_5GW.xml");
			SampleCodeType();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			requestHeader.Reload();
			incomingMessage.Reload();
			var outgoingMessage = requestHeader.Messages.Cast<EDIMessage>().FirstOrDefault(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5GW);

			AssertEquals("requestHeader is located", requestHeader.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("requestHeader.CMR_Status is Updated", requestHeader.CMR_Status, CustomsMessageStatusTypeList.Codes.OriginalAccepted);

			AssertContains("수입 임시개청 신청서", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-09-25 17:55:17", incomingMessage.EM_MessageInterpretation);
			AssertContains("41634200000072U", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-09-25 17:55:17", incomingMessage.EM_MessageInterpretation);
			AssertContains("[03017] 부산세관 부두통관2과", incomingMessage.EM_MessageInterpretation);

			AssertEquals("outgoing message.EM_MessageNum is saved to incoming message.EM_ApplicationReference.", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("수입 임시개청 신청서", email.Body);
			AssertContains("2020-09-25 17:55:17", email.Body);
			AssertContains("41634200000072U", email.Body);
			AssertContains("2020-09-25 17:55:17", email.Body);
			AssertContains("[03017] 부산세관 부두통관2과", email.Body);
		}
		public void TestR76_OutgoingMessageIs5AC()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var requestHeader = CreateEntryWithOutgoingMessage(ElectronicDocumentTypeList.Codes._5AC, "23625200000056U", ElectronicDocumentTypeList.Codes._5AC);
			var incomingMessage = CreateMessageForTest("GOVCBRR76_5AC.xml");
			SampleCodeType();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			requestHeader.Reload();
			incomingMessage.Reload();
			var outgoingMessage = requestHeader.Messages.Cast<EDIMessage>().FirstOrDefault(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5AC);

			AssertEquals("requestHeader is located", requestHeader.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("requestHeader.CMR_Status is Updated", requestHeader.CMR_Status, CustomsMessageStatusTypeList.Codes.OriginalAccepted);

			AssertContains("수출 임시개청 신청서", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-09-25 17:54:17", incomingMessage.EM_MessageInterpretation);
			AssertContains("23625200000056U", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-09-25 17:54:17", incomingMessage.EM_MessageInterpretation);
			AssertContains("[02015] 인천세관 수출(자유지역)과", incomingMessage.EM_MessageInterpretation);

			AssertEquals("outgoing message.EM_MessageNum is saved to incoming message.EM_ApplicationReference", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("수출 임시개청 신청서", email.Body);
			AssertContains("2020-09-25 17:54:17", email.Body);
			AssertContains("23625200000056U", email.Body);
			AssertContains("2020-09-25 17:54:17", email.Body);
			AssertContains("[02015] 인천세관 수출(자유지역)과", email.Body);
		}

		public void TestR76_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				CreateMessageForTest("GOVCBRR76_5AC.xml");
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [임시개청신청서 접수통보]23625200000056U 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ExportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		public void TestR76_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.ImportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBRR76_5GW.xml");
				CreateEntry(false, "41634200000072U", ElectronicDocumentTypeList.Codes._5GW);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[임시개청신청서 접수통보] Response for Declaration Number: B00001000 / 제출번호: 41634200000072U", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [수입 임시개청 신청서]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);
			}
		}

		public void TestR76_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.ImportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBRR76_5GW.xml");
				CreateEntry(true, "41634200000072U", ElectronicDocumentTypeList.Codes._5GW);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[임시개청신청서 접수통보] Response for Declaration Number: B00001000 / 제출번호: 41634200000072U", email.Subject);
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
			staff2.GS_LoginName = "Test1";
			staff2.GS_EmailAddress = "ImportGroupTest@wisetechglobal.com";
			importGroup = Factory.New<GlbGroup>();
			var link1 = Factory.New<GlbGroupLink>();
			link1.GK_GG = importGroup.PK;
			link1.GK_GS = staff2.PK;

			var staff3 = Factory.New<GlbStaff>();
			staff3.GS_Code = "T2";
			staff3.GS_LoginName = "Test2";
			staff3.GS_EmailAddress = "ExportGroupTest@wisetechglobal.com";
			exportGroup = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			var link2 = Factory.New<GlbGroupLink>();
			link2.GK_GG = exportGroup.PK;
			link2.GK_GS = staff3.PK;

			var staff4 = Factory.New<GlbStaff>();
			staff4.GS_Code = "AG";
			staff4.GS_LoginName = "Agent";
			staff4.GS_EmailAddress = "CusAgent@wisetechglobal.com";
			Factory.Save();
		}
		GlbGroup importGroup;
		GlbGroup exportGroup;
		void CreateEntry(bool broker, string entryNumber, string entryType)
		{
			requestHeader = Factory.New<CusMiscRequestHeader>();
			requestHeader.CMR_JobNumber = "B00001000";
			requestHeader.CMR_MessageType = entryType;
			requestHeader.CMR_RequestDate = new ZDateTime(2020, 09, 25, 17, 54, 17);
			requestHeader.CMR_CustomsOffice = "020";
			requestHeader.CMR_GB = Env.CurrentBranch.PK;
			if (broker)
			{
				requestHeader.CMR_GS_NKBroker = "AG";
			}
			var entryNum = Factory.New<CusEntryNumber>();
			entryNum.CE_EntryNum = entryNumber;
			entryNum.CE_EntryType = entryType;
			entryNum.CE_ParentID = requestHeader.PK;
			entryNum.CE_ParentTable = requestHeader.TableName;
			Factory.Save();
		}
		CusMiscRequestHeader requestHeader;
		CusMiscRequestHeader CreateEntryWithOutgoingMessage(string em_MessageType, string entryNumber, string entryType)
		{
			if (requestHeader == null)
			{
				CreateEntry(true, entryNumber, entryType);
			}
			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = em_MessageType;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_LinkTable = CusMiscRequestHeader.Schema.TableName;
			outgoingMessage.EM_LinkedObject = requestHeader;

			return requestHeader;
		}
		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBRR76MessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._R76;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			Factory.Save();

			return incomingMessage;
		}
		void SampleCodeType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "020", "인천세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "15", "수출(자유지역)과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "030", "부산세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "17", "부두통관2과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Common.Incoming";
	}
}
