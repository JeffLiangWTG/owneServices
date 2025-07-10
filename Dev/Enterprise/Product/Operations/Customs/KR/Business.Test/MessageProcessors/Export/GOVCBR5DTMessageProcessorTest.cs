using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5DTMessageProcessorTest : XMLMessageTestHelper<GOVCBR5DTMessageProcessorTest>
	{
		public void TestEntryStatus_ANT()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5DT_Status_ANT.xml");
			var outgoingMessage = CreateEntryWithOutgoingMessageForExport("6N00220000051X");
			outgoingMessage.EM_ApplicationReference = "2";
			SampleCodeType();
			Factory.Save();

			AssertEquals("PreCondition: Message ApplicationReference is Empty", ZString.Empty, incomingMessage.EM_ApplicationReference);
			AssertEquals("PreCondition: Message Message Owner is Empty", ZString.Empty, incomingMessage.EM_MessageOwner);
			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			exportEntry.Reload();
			incomingMessage.Reload();
			outgoingMessage.Reload();

			AssertEquals("entry is located", exportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to ANT", "ANT", exportEntry.CH_EntryStatus);

			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals("Message Owner is updated", exportEntry.CH_EntryStatus, incomingMessage.EM_MessageOwner);
			AssertEquals(incomingMessage.EM_MessageOwner, outgoingMessage.MessageOrEntryStatus);

			TestEmailIsSent();
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("승인", email.Body);
			AssertContains("관세사 등", email.Body);
		}
		public void TestEntryStatus_CCL()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5DT_Status_CCL.xml");
			var outgoingMessage = CreateEntryWithOutgoingMessageForExport("6N00220000051X");
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._DKJ;
			outgoingMessage.EM_ApplicationReference = "2";
			SampleCodeType();
			outgoingMessage.EM_MessageSubType = "B";
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			SampleCodeType();

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);
			AssertEquals("entry is located", exportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to CCL", "CCL", exportEntry.CH_EntryStatus);
			AssertEquals("entry message status is updated correctly to CAP", "CAP", exportEntry.CH_Status);

			TestEmailIsSent();
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("승인", email.Body);
			AssertContains("관세사 등", email.Body);
		}
		public void TestEntryStatus_CGD()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5DT_Status_CGD.xml");
			CreateEntryWithOutgoingMessageForExport("6N00220000051X");
			SampleCodeType();
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);
			AssertEquals("entry is located", exportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to CGD", "CGD", exportEntry.CH_EntryStatus);

			TestEmailIsSent();
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("서류변경", email.Body);
			AssertContains("담당자 등", email.Body);
		}
		public void TestEntryStatus_DMS()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5DT_Status_DMS.xml");
			CreateEntryWithOutgoingMessageForExport("6N00220000051X");
			SampleCodeType();
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);
			AssertEquals("entry is located", exportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to DMS", "DMS", exportEntry.CH_EntryStatus);

			TestEmailIsSent();
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("기각", email.Body);
			AssertContains("수출화주/수출대행자", email.Body);
		}

		public void TestEntryStatus_DMSAmendment()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5DT_Status_DMS.xml");
			CreateEntryWithOutgoingMessageForExport("6N00220000051X", _5ASAmendmentType.Codes.Amendment);
			exportEntry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;
			SampleCodeType();
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);
			AssertEquals("entry is located", exportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to DMS", "DMS", exportEntry.CH_EntryStatus);
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, exportEntry.CH_Status);

			TestEmailIsSent();
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("기각", email.Body);
			AssertContains("수출화주/수출대행자", email.Body);
		}

		public void TestEntryStatus_DMSCancellation()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5DT_Status_DMS.xml");
			var outgoingMessage = CreateEntryWithOutgoingMessageForExport("6N00220000051X", "");
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._DKJ;
			outgoingMessage.EM_ApplicationReference = "2";
			SampleCodeType();
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);
			AssertEquals("entry is located", exportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to DMS", "DMS", exportEntry.CH_EntryStatus);
			AssertEquals(CustomsMessageStatusTypeList.Codes.CancellationDeclined, exportEntry.CH_Status);
		}

		void SetUpSnapshot(string electronicDocumentType)
		{
			exportEntry.CH_MessageType = electronicDocumentType;

			var snapshot = exportEntry.Snapshots.AddNew();
			snapshot.CES_MessageType = electronicDocumentType;
			snapshot.CES_Status = EntrySnapshotStatus.Lodged;
			snapshot.CES_SystemCreateTimeUtc = ZDateTime.Today;

			snapshot = exportEntry.Snapshots.AddNew();
			snapshot.CES_MessageType = electronicDocumentType;
			snapshot.CES_Status = EntrySnapshotStatus.Lodged;
			snapshot.CES_SystemCreateTimeUtc = ZDateTime.Today.AddDays(1);
			Factory.Save();
		}
		public void TestShapshotWhenStatusIsDMS()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5DT_Status_DMS.xml");
			var outgoingMessage = CreateEntryWithOutgoingMessageForExport("6N00220000051X");
			outgoingMessage.EM_ApplicationReference = "2";
			SetUpSnapshot(ElectronicDocumentTypeList.Codes._830);

			AssertEquals("Entry has 2 Snapshots", 2, exportEntry.Snapshots.Count);
			var latestSnapshot = exportEntry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._830, EntrySnapshotStatus.Lodged);
			AssertNotNull("Entry has latestSnapshot", latestSnapshot);
			AssertEquals("SystemCreateTimeUtc of latestSnapshot is today+1", ZDateTime.Today.AddDays(1), latestSnapshot.CES_SystemCreateTimeUtc);

			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);
			Factory.Save();
			exportEntry = new BusinessObjectFactory().Load<CusEntryHeader>(exportEntry.PK);

			AssertEquals("EntryHeader update is DMS", "DMS", exportEntry.CH_EntryStatus);
			AssertEquals("Successfully updated CES_Status to 'DEL'.", 1, exportEntry.Snapshots.Count);

			latestSnapshot = exportEntry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._830, EntrySnapshotStatus.Lodged);
			AssertEquals("SystemCreateTimeUtc of latestSnapshot is today", ZDateTime.Today, latestSnapshot.CES_SystemCreateTimeUtc);
		}

		public void TestShapshotWhenStatusIsDMSCancellation()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5DT_Status_DMS.xml");
			var outgoingMessage = CreateEntryWithOutgoingMessageForExport("6N00220000051X", "");
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._DKJ;
			outgoingMessage.EM_ApplicationReference = "2";
			SetUpSnapshot(ElectronicDocumentTypeList.Codes._830);

			AssertEquals("Entry has 2 Snapshots", 2, exportEntry.Snapshots.Count);
			var latestSnapshot = exportEntry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._830, EntrySnapshotStatus.Lodged);
			AssertNotNull("Entry has latestSnapshot", latestSnapshot);
			AssertEquals("SystemCreateTimeUtc of latestSnapshot is today+1", ZDateTime.Today.AddDays(1), latestSnapshot.CES_SystemCreateTimeUtc);

			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);
			Factory.Save();
			exportEntry = new BusinessObjectFactory().Load<CusEntryHeader>(exportEntry.PK);

			AssertEquals("EntryHeader update is DMS", "DMS", exportEntry.CH_EntryStatus);
			AssertEquals(CustomsMessageStatusTypeList.Codes.CancellationDeclined, exportEntry.CH_Status);

			AssertEquals("Successfully, When message type is DKJ, snapshot was not updated'.", 2, exportEntry.Snapshots.Count);
			latestSnapshot = exportEntry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._830, EntrySnapshotStatus.Lodged);
			AssertEquals("SystemCreateTimeUtc of latestSnapshot is today+1", ZDateTime.Today.AddDays(1), latestSnapshot.CES_SystemCreateTimeUtc);
		}

		public void TestShapshotWhenStatusIsNotDMS()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5DT_Status_CGD.xml");
			CreateEntryWithOutgoingMessageForExport("6N00220000051X");
			SetUpSnapshot(ElectronicDocumentTypeList.Codes._830);

			AssertEquals("Entry has 2 Snapshots", 2, exportEntry.Snapshots.Count);
			var latestSnapshot = exportEntry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._830, EntrySnapshotStatus.Lodged);
			AssertNotNull("Entry has latestSnapshot", latestSnapshot);
			AssertEquals("SystemCreateTimeUtc of latestSnapshot is today+1", ZDateTime.Today.AddDays(1), latestSnapshot.CES_SystemCreateTimeUtc);

			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);
			Factory.Save();
			exportEntry = new BusinessObjectFactory().Load<CusEntryHeader>(exportEntry.PK);

			AssertEquals("EntryHeader update is CGD", "CGD", exportEntry.CH_EntryStatus);
			AssertEquals("Entry has 2 Snapshots", 2, exportEntry.Snapshots.Count);
			latestSnapshot = exportEntry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._830, EntrySnapshotStatus.Lodged);
			AssertEquals("SystemCreateTimeUtc of latestSnapshot is today+1", ZDateTime.Today.AddDays(1), latestSnapshot.CES_SystemCreateTimeUtc);

			var deleteSnapshot = exportEntry.Snapshots.Cast<CusEntrySnapshot>().FirstOrDefault(x => x.CES_Status == EntrySnapshotStatus.Deleted);
			AssertNull("Snapshots are not deleted in this process case.", deleteSnapshot);
		}

		void TestEmailIsSent()
		{
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("수출신고수리 정정/취하 신청서", email.Body);
			AssertContains("2020-08-18 08:15:44", email.Body);
			AssertContains("6N002-20-000051X", email.Body);
			AssertContains("2020-08-18", email.Body);
			AssertContains("02015200056345", email.Body);
			AssertContains("[01020]서울세관 내륙기지통관과", email.Body);
		}
		public void TestExport5DT_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				CreateMessageForTest("GOVCBR5DT_0.xml");
				SampleCodeType();
				Factory.Save();
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [수출 정정취하 결과통보]6N00220000052X 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ExportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		public void TestExport5DT_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBR5DT_0.xml");
				CreateEntryForExport(false, "6N00220000052X");
				SampleCodeType();
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수출 정정취하 결과통보] Response for Declaration Number: B00001000 / 제출번호: 6N00220000052X", email.Subject);
				AssertEquals("ExportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [수출정정신고서, 수출취하신청서]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);
			}
		}

		public void TestExport5DT_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, exportGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBR5DT_0.xml");
				CreateEntryForExport(true, "6N00220000052X");
				SampleCodeType();
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수출 정정취하 결과통보] Response for Declaration Number: B00001000 / 제출번호: 6N00220000052X", email.Subject);
				AssertEquals("CusAgent@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		public void TestEM_MessageSubTypeIsC()
		{
			var outgoingMessage = CreateEntryWithOutgoingMessageForExport("6N00220000051X");
			outgoingMessage.EM_MessageSubType = "C";
			outgoingMessage.EM_ApplicationReference = "2";
			var fileReader = new TestFileReader(typeof(GOVCBR5DTMessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText($"{TestFilesPath}.Outgoing", "GOVCBR5AS_Extend.xml");
			outgoingMessage.EM_MessageText = messageText;

			var incomingMessage = CreateMessageForTest("GOVCBR5DT_Status_CCL.xml");
			incomingMessage.EM_ApplicationReference = outgoingMessage.EM_MessageNum;

			Factory.Save();

			var entrynum = exportEntry.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == "EXP");
			AssertEquals(ZDate.Empty, entrynum.CE_ExpiryDate);
			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);
			AssertEquals(new ZDateTime("2020-12-31"), entrynum.CE_ExpiryDate);
		}
		public void TestAmendReasonDescription()
		{
			var outgoingMessage1 = CreateEntryWithOutgoingMessageForExport("6N00220000051X");
			var fileReader = new TestFileReader(typeof(GOVCBR5DTMessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText($"{TestFilesPath}.Outgoing", "GOVCBRDKJ_Test.xml");
			outgoingMessage1.EM_MessageText = messageText;
			exportEntry.CH_VersionID = 1;
			outgoingMessage1.EM_ApplicationReference = "2";

			var outgoingMessage2 = CreateEntryWithOutgoingMessageForExport("6N00220000051X");
			messageText = fileReader.GetEmbeddedFileText($"{TestFilesPath}.Outgoing", "GOVCBR5AS_Extend.xml");
			outgoingMessage2.EM_MessageText = messageText;
			outgoingMessage2.EM_ApplicationReference = "2";

			Factory.Save();

			AssertEquals(1, exportEntry.ExportAmendmentDetailsCollection.Count);
			var amendmentDetails = exportEntry.ExportAmendmentDetailsCollection[0];
			AssertEquals("23", amendmentDetails.ReasonCode);
			AssertEquals("선적지연", amendmentDetails.AmendReasonDescription);
		}

		public void TestAmendmentDetailsWith5DT()
		{
			var fileReader = new TestFileReader(typeof(GOVCBR5DTMessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText("Enterprise.Customs.KR.Business.Testing.TestFiles.Export.Outgoing", "GOVCBR5AS_Extend.xml");
			var outgoingMessage = CreateEntryWithOutgoingMessageForExport("6N00220000051X");
			outgoingMessage.EM_MessageText = messageText;
			exportEntry.CH_VersionID = 1;
			outgoingMessage.EM_ApplicationReference = "2";

			var incomingMessage5AF = CreateMessageForTest("GOVCBR5AF_5AS.xml");
			incomingMessage5AF.EM_MessageType = "5AF";
			incomingMessage5AF.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			incomingMessage5AF.EM_ApplicationReference = outgoingMessage.EM_MessageNum;

			var incomingMessage = CreateMessageForTest("GOVCBR5DT_Status_CCL.xml");
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.KRCustoms;
			incomingMessage.EM_ApplicationReference = outgoingMessage.EM_MessageNum;
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			exportEntry.Reload();
			incomingMessage5AF.Reload();
			incomingMessage.Reload();

			AssertEquals(1, exportEntry.ExportAmendmentDetailsCollection.Count);

			var amendmentDetails = exportEntry.ExportAmendmentDetailsCollection[0];
			AssertEquals(1, amendmentDetails.AmendSequenceNo);
			AssertEquals(CustomsMessageStatusTypeList.Codes.AmendmentAccepted, amendmentDetails.MessageStatus);
			AssertEquals(new ZDateTime(2021, 2, 2), amendmentDetails.SubmissionDate);
			AssertEquals("C", amendmentDetails.AmendmentType);
			AssertEquals("기간연장", amendmentDetails.AmendmentTypeDescription);
			AssertEquals("C", amendmentDetails.FaultParty);
			AssertEquals("해외거래처/구매자", amendmentDetails.FaultPartyOtherDescription);
			AssertEquals("23", amendmentDetails.ReasonCode);
			AssertEquals("선적지연", amendmentDetails.AmendReasonDescription);
			AssertEquals("05", amendmentDetails.NoticeType);
			AssertEquals("승인", amendmentDetails.NoticeDescription);
			AssertEquals("02015200056345", amendmentDetails.ApprovalNo);
			AssertEquals(new ZDateTime(2020, 8, 18), amendmentDetails.DecisionDate);
		}

		public void TestOriginalMessageGetLatestDate()
		{
			var incomingMessage = CreateMessageForTest("GOVCBR5DT_Status_ANT.xml");
			var outgoingMessage5AS = CreateEntryWithOutgoingMessageForExport("6N00220000051X");
			outgoingMessage5AS.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(-10);
			outgoingMessage5AS.EM_ApplicationReference = "2";

			var outgoingMessageDKJ = CreateEntryWithOutgoingMessageForExport("6N00220000051X");
			outgoingMessageDKJ.EM_MessageType = ElectronicDocumentTypeList.Codes._DKJ;
			outgoingMessageDKJ.EM_ApplicationReference = "2";

			SampleCodeType();
			Factory.Save();

			AssertEquals("PreCondition: Message ApplicationReference is Empty", ZString.Empty, incomingMessage.EM_ApplicationReference);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();

			AssertEquals("Message ApplicationReference is updated", outgoingMessageDKJ.EM_MessageNum, incomingMessage.EM_ApplicationReference);
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

		void CreateEntryForExport(bool setCusAgent, string ce_EntryNum)
		{
			var declaration = Factory.New<JobDeclaration>();
			if (setCusAgent)
			{
				declaration.JE_GS_NKCusAgent = "AG";
			}
			exportEntry = declaration.CustomsEntryHeaders.AddNew();
			var entryNumber = exportEntry.EntryNumbers.AddNew();
			entryNumber.CE_EntryNum = ce_EntryNum;
			entryNumber.CE_EntryType = "EXP";
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNumber.CE_ParentID = exportEntry.PK;
			entryNumber.CE_ParentTable = CusEntryHeader.Schema.TableName;
			entryNumber.CE_EntryStatus = ZString.Empty;
			Factory.Save();
		}
		CusEntryHeader exportEntry;

		EDIMessage CreateEntryWithOutgoingMessageForExport(string ce_EntryNum, string em_subType = "")
		{
			if (exportEntry == null)
			{
				CreateEntryForExport(true, ce_EntryNum);
			}
			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5AS;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_LinkTable = CusEntryHeader.Schema.TableName;
			outgoingMessage.EM_LinkUniqueID = exportEntry.PK;
			outgoingMessage.EM_LinkedObject = exportEntry;
			outgoingMessage.EM_MessageNum = "62252";
			outgoingMessage.EM_MessageSubType = em_subType;
			return outgoingMessage;
		}

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBR5DTMessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText($"{TestFilesPath}.Incoming", fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5DT;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			return incomingMessage;
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

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Export";
	}
}
