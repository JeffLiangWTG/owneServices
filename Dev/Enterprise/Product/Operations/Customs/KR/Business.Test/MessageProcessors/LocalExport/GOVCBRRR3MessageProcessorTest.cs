using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBRRR3MessageProcessorTest : XMLMessageTestHelper<GOVCBRRR3MessageProcessorTest>
	{
		public void Test5DP_StatusIsANT()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DP", "4271120006350", "");
			incomingMessage = CreateMessageForTest("GOVCBRRR3_5DP_StatusIsANT.xml");
			SampleCodeType();
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			AssertEquals("PreCondition: Entry Status is Empty", ZString.Empty, localExportEntry.CH_EntryStatus);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			localExportEntry.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to ANT", "ANT", localExportEntry.CH_EntryStatus);
			AssertEquals("ANT", incomingMessage.EM_MessageOwner);

			var stmAlogFilter = new ZQuery(StmALogSchema.SL_Parent, localExportEntry.PK);
			stmAlogFilter.AddToFilter(StmALogSchema.SL_Table, CusEntryHeader.Schema.TableName);
			var alog = Factory.LoadTop1<StmALog>(stmAlogFilter);
			AssertEquals("StmAlog SL_EventTime is updated", "20201120172629", alog.SL_EventTime.ToString("yyyyMMddHHmmss"));

			var customsOfficer = localExportEntry.CustomsOfficers.Cast<CustomsOfficer>().Where(x => x.CY_Code == CustomsOfficerTypeList.Codes.ResponsibleCustomsOfficer)?.LastOrDefault();
			AssertEquals("조성은 181796", customsOfficer.CY_Data);
			AssertEquals(new ZDateTime(2020, 11, 20, 17, 26, 00), customsOfficer.CY_Date);

			AssertEquals("20201120172629\r\nremark_5DP", localExportEntry.CH_CustomsMessageRemarks);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email1.Recipients[0];

			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("GOVCBR5DP", email1.Body);
			AssertContains("환급대상수출물품 반입확인 제출", email1.Body);
			AssertContains("승인/허가", email1.Body);
			AssertContains("[01010] 서울세관 통관지원(1)과", email1.Body);
			AssertContains("조성은 181796", email1.Body);
			AssertContains("2020-11-20 17:26:29", email1.Body);
			AssertContains("01010200006731", email1.Body);

			EM_MessageInterpretation_Check(incomingMessage);
		}
		public void Test5DP_StatusIsCCL()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DP", "4271120006350", "");
			incomingMessage = CreateMessageForTest("GOVCBRRR3_5DP_StatusIsCCL.xml");
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			localExportEntry.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to CCL", "CCL", localExportEntry.CH_EntryStatus);
			AssertEquals("CCL", incomingMessage.EM_MessageOwner);
			AssertEquals("entry message status is updated correctly to CAB", CustomsMessageStatusTypeList.Codes.CancellationByCustoms, localExportEntry.CH_Status);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("승인취소", email1.Body);
		}
		public void Test5DP_StatusIsCNR()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DP", "4271120006350", "");
			incomingMessage = CreateMessageForTest("GOVCBRRR3_5DP_StatusIsCNR.xml");
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			localExportEntry.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to CNR", "CNR", localExportEntry.CH_EntryStatus);
			AssertEquals("CNR", incomingMessage.EM_MessageOwner);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("확인등록", email1.Body);
		}
		public void Test5DP_StatusIsDMS()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DP", "4271120006350", "");
			incomingMessage = CreateMessageForTest("GOVCBRRR3_5DP_StatusIsDMS.xml");
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			localExportEntry.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to DMS", "DMS", localExportEntry.CH_EntryStatus);
			AssertEquals("DMS", incomingMessage.EM_MessageOwner);

			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("기각", email1.Body);
		}
		public void Test5DP_StatusIsMFI()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DP", "4271120006350", "");
			incomingMessage = CreateMessageForTest("GOVCBRRR3_5DP_StatusIsMFI.xml");
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			localExportEntry.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to MFI", "MFI", localExportEntry.CH_EntryStatus);
			AssertEquals("MFI", incomingMessage.EM_MessageOwner);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("보완요청", email1.Body);
		}
		public void Test5DP_StatusIsOGJ()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DP", "4271120006350", "");
			incomingMessage = CreateMessageForTest("GOVCBRRR3_5DP_StatusIsOGJ.xml");
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			localExportEntry.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to OGJ", "OGJ", localExportEntry.CH_EntryStatus);
			AssertEquals("OGJ", incomingMessage.EM_MessageOwner);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("출무대상", email1.Body);
		}

		public void Test5DQ_StatusIsANT()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DQ", "4271120006120", "");
			incomingMessage = CreateMessageForTest("GOVCBRRR3_5DQ_StatusIsANT.xml");
			SampleCodeType();
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			AssertEquals("PreCondition: Entry Status is Empty", ZString.Empty, localExportEntry.CH_EntryStatus);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			localExportEntry.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to ANT", "ANT", localExportEntry.CH_EntryStatus);
			AssertEquals("ANT", incomingMessage.EM_MessageOwner);

			var customsOfficer = localExportEntry.CustomsOfficers.Cast<CustomsOfficer>().Where(x => x.CY_Code == CustomsOfficerTypeList.Codes.ResponsibleCustomsOfficer)?.LastOrDefault();
			AssertEquals("정상근 182096", customsOfficer.CY_Data);
			AssertEquals(new ZDateTime(2020, 11, 20, 17, 26, 00), customsOfficer.CY_Date);

			var stmAlogFilter = new ZQuery(StmALogSchema.SL_Parent, localExportEntry.PK);
			stmAlogFilter.AddToFilter(StmALogSchema.SL_Table, CusEntryHeader.Schema.TableName);
			var alog = Factory.LoadTop1<StmALog>(stmAlogFilter);
			AssertEquals("StmAlog SL_EventTime is updated", "20201210232629", alog.SL_EventTime.ToString("yyyyMMddHHmmss"));

			AssertEquals("20201120172629\r\nremark_5DQ", localExportEntry.CH_CustomsMessageRemarks);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email1.Recipients[0];

			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("GOVCBR5DQ", email1.Body);
			AssertContains("환급대상수출물품 적재신청 제출", email1.Body);
			AssertContains("승인/허가", email1.Body);
			AssertContains("[01010] 서울세관 통관지원(1)과", email1.Body);
			AssertContains("정상근 182096", email1.Body);
			AssertContains("2020-11-20 17:26:29", email1.Body);
			AssertContains("01010210008732", email1.Body);
		}
		public void Test5DQ_StatusIsCCL()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DQ", "4271120006120", "");
			incomingMessage = CreateMessageForTest("GOVCBRRR3_5DQ_StatusIsCCL.xml");
			SampleCodeType();
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			localExportEntry.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to CCL", "CCL", localExportEntry.CH_EntryStatus);
			AssertEquals("CCL", incomingMessage.EM_MessageOwner);
			AssertEquals("entry message status is updated correctly to CAB", CustomsMessageStatusTypeList.Codes.CancellationByCustoms, localExportEntry.CH_Status);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("승인취소", email1.Body);
		}
		public void Test5DQ_StatusIsCNR()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DQ", "4271120006120", "");
			incomingMessage = CreateMessageForTest("GOVCBRRR3_5DQ_StatusIsCNR.xml");
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			localExportEntry.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to CNR", "CNR", localExportEntry.CH_EntryStatus);
			AssertEquals("CNR", incomingMessage.EM_MessageOwner);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("확인등록", email1.Body);
		}
		public void Test5DQ_StatusIsDMS()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DQ", "4271120006120", "");
			incomingMessage = CreateMessageForTest("GOVCBRRR3_5DQ_StatusIsDMS.xml");
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			localExportEntry.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to DMS", "DMS", localExportEntry.CH_EntryStatus);
			AssertEquals("DMS", incomingMessage.EM_MessageOwner);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("기각", email1.Body);
		}
		public void Test5DQ_StatusIsMFI()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DQ", "4271120006120", "");
			incomingMessage = CreateMessageForTest("GOVCBRRR3_5DQ_StatusIsMFI.xml");
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			localExportEntry.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to MFI", "MFI", localExportEntry.CH_EntryStatus);
			AssertEquals("MFI", incomingMessage.EM_MessageOwner);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("보완요청", email1.Body);
		}
		public void Test5DQ_StatusIsOGJ()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DQ", "4271120006120", "");
			incomingMessage = CreateMessageForTest("GOVCBRRR3_5DQ_StatusIsOGJ.xml");
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			localExportEntry.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to OGJ", "OGJ", localExportEntry.CH_EntryStatus);
			AssertEquals("OGJ", incomingMessage.EM_MessageOwner);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("출무대상", email1.Body);
		}

		public void Test5DR_StatusIsAntWhenMessageSubTypeIs1()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DR", "4271120006680", LocalExportAmendmentTypeList.Codes.Amendment);
			incomingMessage = CreateMessageForTest("GOVCBRRR3_5DR_StatusIsANTOrCCL.xml");
			SampleCodeType();
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			AssertEquals("PreCondition: Entry Status is Empty", ZString.Empty, localExportEntry.CH_EntryStatus);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			localExportEntry.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to ANT", "ANT", localExportEntry.CH_EntryStatus);
			AssertEquals("ANT", incomingMessage.EM_MessageOwner);

			var customsOfficer = localExportEntry.CustomsOfficers.Cast<CustomsOfficer>().Where(x => x.CY_Code == CustomsOfficerTypeList.Codes.ResponsibleCustomsOfficer)?.LastOrDefault();
			AssertNull(customsOfficer);

			var stmAlogFilter = new ZQuery(StmALogSchema.SL_Parent, localExportEntry.PK);
			stmAlogFilter.AddToFilter(StmALogSchema.SL_Table, CusEntryHeader.Schema.TableName);
			var alog = Factory.LoadTop1<StmALog>(stmAlogFilter);
			AssertEquals("StmAlog SL_EventTime is updated", "20201117160540", alog.SL_EventTime.ToString("yyyyMMddHHmmss"));

			AssertEquals("20201117160541\r\nremark_5DR", localExportEntry.CH_CustomsMessageRemarks);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email1.Recipients[0];

			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("GOVCBR5DR", email1.Body);
			AssertContains("환급대상수출물품 반입확인 정정/취하 제출", email1.Body);
			AssertContains("승인/허가", email1.Body);
			AssertContains("[01010] 서울세관 통관지원(1)과", email1.Body);
			AssertContains("강민주", email1.Body);
			AssertContains("2020-11-17 16:05:40", email1.Body);
			AssertContains("01010200011291", email1.Body);
		}
		public void Test5DR_StatusIsCCLWhenMessageSubTypeIs2()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DR", "4271120006680", LocalExportAmendmentTypeList.Codes.Cancellation);
			incomingMessage = CreateMessageForTest("GOVCBRRR3_5DR_StatusIsANTOrCCL.xml");
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			AssertEquals("PreCondition: Entry Status is Empty", ZString.Empty, localExportEntry.CH_EntryStatus);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			localExportEntry.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to CCL", "CCL", localExportEntry.CH_EntryStatus);
			AssertEquals("CAP", localExportEntry.CH_Status);
			AssertEquals("CCL", incomingMessage.EM_MessageOwner);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
		}
		public void Test5DR_StatusIsCCL()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DR", "4271120006680", "");
			incomingMessage = CreateMessageForTest("GOVCBRRR3_5DR_StatusIsCCL.xml");
			SetUpSnapshot(ElectronicDocumentTypeList.Codes._5DP);
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			AssertEquals("Entry has 2 Snapshots", 2, localExportEntry.Snapshots.Count);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			localExportEntry.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to CCL", "CCL", localExportEntry.CH_EntryStatus);
			AssertEquals("CCL", incomingMessage.EM_MessageOwner);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("승인취소", email1.Body);

			var entry = new BusinessObjectFactory().Load<CusEntryHeader>(localExportEntry.PK);
			AssertEquals("Entry has 1 Snapshots", 1, entry.Snapshots.Count);
		}
		public void Test5DR_StatusIsCCLCancellation()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DR", "4271120006680", LocalExportAmendmentTypeList.Codes.Cancellation);
			incomingMessage = CreateMessageForTest("GOVCBRRR3_5DR_StatusIsCCL.xml");
			SetUpSnapshot(ElectronicDocumentTypeList.Codes._5DP);
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			AssertEquals("Entry has 2 Snapshots", 2, localExportEntry.Snapshots.Count);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			localExportEntry.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to CCL", "CCL", localExportEntry.CH_EntryStatus);
			AssertEquals("CCL", incomingMessage.EM_MessageOwner);
			AssertEquals(CustomsMessageStatusTypeList.Codes.CancellationDeclined, localExportEntry.CH_Status);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("승인취소", email1.Body);

			var entry = new BusinessObjectFactory().Load<CusEntryHeader>(localExportEntry.PK);
			AssertEquals("Entry has 2 Snapshots", 2, entry.Snapshots.Count);
		}
		public void Test5DR_StatusIsCCLCancellationNotOriginalMessage()
		{
			CreateEntryForLocalExport(true, "4271120006680");
			incomingMessage = CreateMessageForTest("GOVCBRRR3_5DR_StatusIsCCL.xml");
			SetUpSnapshot(ElectronicDocumentTypeList.Codes._5DP);
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			AssertEquals("Entry has 2 Snapshots", 2, localExportEntry.Snapshots.Count);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			localExportEntry.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to CCL", "CCL", localExportEntry.CH_EntryStatus);
			AssertEquals("CCL", incomingMessage.EM_MessageOwner);
			AssertNotEquals(CustomsMessageStatusTypeList.Codes.CancellationDeclined, localExportEntry.CH_Status);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("승인취소", email1.Body);

			var entry = new BusinessObjectFactory().Load<CusEntryHeader>(localExportEntry.PK);
			AssertEquals("Entry has 2 Snapshots", 2, entry.Snapshots.Count);
		}

		public void Test5DR_StatusIsCNR()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DR", "4271120006680", "");
			incomingMessage = CreateMessageForTest("GOVCBRRR3_5DR_StatusIsCNR.xml");
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			localExportEntry.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to CNR", "CNR", localExportEntry.CH_EntryStatus);
			AssertEquals("CNR", incomingMessage.EM_MessageOwner);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("확인등록", email1.Body);
		}
		public void Test5DR_StatusIsDMS()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DR", "4271120006681", "");
			incomingMessage = CreateMessageForTest("GOVCBRRR3_5DR_StatusIsDMS.xml");
			SetUpSnapshot(ElectronicDocumentTypeList.Codes._5DP);
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			AssertEquals("Entry has 2 Snapshots", 2, localExportEntry.Snapshots.Count);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			localExportEntry.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to DMS", "DMS", localExportEntry.CH_EntryStatus);
			AssertEquals("DMS", incomingMessage.EM_MessageOwner);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("기각", email1.Body);

			var entry = new BusinessObjectFactory().Load<CusEntryHeader>(localExportEntry.PK);
			AssertEquals("Entry has 1 Snapshots", 1, entry.Snapshots.Count);
		}

		public void Test5DR_StatusIsDMSCancellation()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DR", "4271120006681", LocalExportAmendmentTypeList.Codes.Cancellation);
			incomingMessage = CreateMessageForTest("GOVCBRRR3_5DR_StatusIsDMS.xml");
			SetUpSnapshot(ElectronicDocumentTypeList.Codes._5DP);
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			AssertEquals("Entry has 2 Snapshots", 2, localExportEntry.Snapshots.Count);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			localExportEntry.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to DMS", "DMS", localExportEntry.CH_EntryStatus);
			AssertEquals("DMS", incomingMessage.EM_MessageOwner);
			AssertEquals(CustomsMessageStatusTypeList.Codes.CancellationDeclined, localExportEntry.CH_Status);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("기각", email1.Body);

			var entry = new BusinessObjectFactory().Load<CusEntryHeader>(localExportEntry.PK);
			AssertEquals("Entry has 2 Snapshots", 2, entry.Snapshots.Count);
		}

		public void Test5DR_StatusIsDMSCancellationNotOriginalMessage()
		{
			CreateEntryForLocalExport(true, "4271120006681");
			incomingMessage = CreateMessageForTest("GOVCBRRR3_5DR_StatusIsDMS.xml");
			SetUpSnapshot(ElectronicDocumentTypeList.Codes._5DP);
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			AssertEquals("Entry has 2 Snapshots", 2, localExportEntry.Snapshots.Count);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			localExportEntry.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to DMS", "DMS", localExportEntry.CH_EntryStatus);
			AssertEquals("DMS", incomingMessage.EM_MessageOwner);
			AssertNotEquals(CustomsMessageStatusTypeList.Codes.CancellationDeclined, localExportEntry.CH_Status);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("기각", email1.Body);

			var entry = new BusinessObjectFactory().Load<CusEntryHeader>(localExportEntry.PK);
			AssertEquals("Entry has 2 Snapshots", 2, entry.Snapshots.Count);
		}

		public void Test5DR_StatusIsMFI()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DR", "4271120006680", "");
			incomingMessage = CreateMessageForTest("GOVCBRRR3_5DR_StatusIsMFI.xml");
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			localExportEntry.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to MFI", "MFI", localExportEntry.CH_EntryStatus);
			AssertEquals("MFI", incomingMessage.EM_MessageOwner);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("보완요청", email1.Body);
		}
		public void Test5DR_StatusIsOGJ()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DR", "4271120006680", "");
			incomingMessage = CreateMessageForTest("GOVCBRRR3_5DR_StatusIsOGJ.xml");
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			localExportEntry.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to OGJ", "OGJ", localExportEntry.CH_EntryStatus);
			AssertEquals("OGJ", incomingMessage.EM_MessageOwner);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("출무대상", email1.Body);
		}

		public void Test5DS_StatusIsAntWhenMessageSubTypeIs1()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DS", "3271420001710", LocalExportAmendmentTypeList.Codes.Amendment);
			incomingMessage = CreateMessageForTest("GOVCBRRR3_5DS_StatusIsANTOrCCL.xml");
			SampleCodeType();
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			AssertEquals("PreCondition: Entry Status is Empty", ZString.Empty, localExportEntry.CH_EntryStatus);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			localExportEntry.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to ANT", "ANT", localExportEntry.CH_EntryStatus);
			AssertEquals("ANT", incomingMessage.EM_MessageOwner);

			var customsOfficer = localExportEntry.CustomsOfficers.Cast<CustomsOfficer>().Where(x => x.CY_Code == CustomsOfficerTypeList.Codes.ResponsibleCustomsOfficer)?.LastOrDefault();
			AssertNull(customsOfficer);

			var stmAlogFilter = new ZQuery(StmALogSchema.SL_Parent, localExportEntry.PK);
			stmAlogFilter.AddToFilter(StmALogSchema.SL_Table, CusEntryHeader.Schema.TableName);
			var alog = Factory.LoadTop1<StmALog>(stmAlogFilter);
			AssertEquals("StmAlog SL_EventTime is updated", "20201127180510", alog.SL_EventTime.ToString("yyyyMMddHHmmss"));

			AssertEquals("20201117160541\r\nremark_5DS", localExportEntry.CH_CustomsMessageRemarks);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email1.Recipients[0];

			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("GOVCBR5DS", email1.Body);
			AssertContains("환급대상수출물품 적재신청 정정/취하 제출", email1.Body);
			AssertContains("승인/허가", email1.Body);
			AssertContains("[03010] 부산세관 통관지원(1)과", email1.Body);
			AssertContains("김세정", email1.Body);
			AssertContains("2020-11-27 18:05:10", email1.Body);
			AssertContains("03010200066291", email1.Body);
		}
		public void Test5DS_StatusIsCCLWhenMessageSubTypeIs2()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DS", "3271420001710", LocalExportAmendmentTypeList.Codes.Cancellation);
			incomingMessage = CreateMessageForTest("GOVCBRRR3_5DS_StatusIsANTOrCCL.xml");
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			AssertEquals("PreCondition: Entry Status is Empty", ZString.Empty, localExportEntry.CH_EntryStatus);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			localExportEntry.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to CCL", "CCL", localExportEntry.CH_EntryStatus);
			AssertEquals("CAP", localExportEntry.CH_Status);
			AssertEquals("CCL", incomingMessage.EM_MessageOwner);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
		}
		public void Test5DS_StatusIsCCL()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DS", "3271420001710", "");
			incomingMessage = CreateMessageForTest("GOVCBRRR3_5DS_StatusIsCCL.xml");
			SetUpSnapshot(ElectronicDocumentTypeList.Codes._5DQ);
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			AssertEquals("Entry has 2 Snapshots", 2, localExportEntry.Snapshots.Count);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			localExportEntry.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to CCL", "CCL", localExportEntry.CH_EntryStatus);
			AssertEquals("CCL", incomingMessage.EM_MessageOwner);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("승인취소", email1.Body);

			var entry = new BusinessObjectFactory().Load<CusEntryHeader>(localExportEntry.PK);
			AssertEquals("Entry has 1 Snapshots", 1, entry.Snapshots.Count);
		}
		public void Test5DS_StatusIsCCLCancellation()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DS", "3271420001710", LocalExportAmendmentTypeList.Codes.Cancellation);
			incomingMessage = CreateMessageForTest("GOVCBRRR3_5DS_StatusIsCCL.xml");
			SetUpSnapshot(ElectronicDocumentTypeList.Codes._5DQ);
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			AssertEquals("Entry has 2 Snapshots", 2, localExportEntry.Snapshots.Count);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			localExportEntry.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to CCL", "CCL", localExportEntry.CH_EntryStatus);
			AssertEquals("CCL", incomingMessage.EM_MessageOwner);
			AssertEquals(CustomsMessageStatusTypeList.Codes.CancellationDeclined, localExportEntry.CH_Status);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("승인취소", email1.Body);

			var entry = new BusinessObjectFactory().Load<CusEntryHeader>(localExportEntry.PK);
			AssertEquals("Entry has 2 Snapshots", 2, entry.Snapshots.Count);
		}
		public void Test5DS_StatusIsCCLCancellationNotOriginalMessage()
		{
			CreateEntryForLocalExport(true, "3271420001710");
			incomingMessage = CreateMessageForTest("GOVCBRRR3_5DS_StatusIsCCL.xml");
			SetUpSnapshot(ElectronicDocumentTypeList.Codes._5DQ);
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			AssertEquals("Entry has 2 Snapshots", 2, localExportEntry.Snapshots.Count);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			localExportEntry.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to CCL", "CCL", localExportEntry.CH_EntryStatus);
			AssertEquals("CCL", incomingMessage.EM_MessageOwner);
			AssertNotEquals(CustomsMessageStatusTypeList.Codes.CancellationDeclined, localExportEntry.CH_Status);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("승인취소", email1.Body);

			var entry = new BusinessObjectFactory().Load<CusEntryHeader>(localExportEntry.PK);
			AssertEquals("Entry has 2 Snapshots", 2, entry.Snapshots.Count);
		}
		public void Test5DS_StatusIsCNR()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DS", "3271420001710", "");
			incomingMessage = CreateMessageForTest("GOVCBRRR3_5DS_StatusIsCNR.xml");
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			localExportEntry.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to CNR", "CNR", localExportEntry.CH_EntryStatus);
			AssertEquals("CNR", incomingMessage.EM_MessageOwner);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("확인등록", email1.Body);
		}
		public void Test5DS_StatusIsDMS()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DS", "3271420001710", "");
			incomingMessage = CreateMessageForTest("GOVCBRRR3_5DS_StatusIsDMS.xml");
			SetUpSnapshot(ElectronicDocumentTypeList.Codes._5DQ);
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			AssertEquals("Entry has 2 Snapshots", 2, localExportEntry.Snapshots.Count);
			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			localExportEntry.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to DMS", "DMS", localExportEntry.CH_EntryStatus);
			AssertEquals("DMS", incomingMessage.EM_MessageOwner);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("기각", email1.Body);

			var entry = new BusinessObjectFactory().Load<CusEntryHeader>(localExportEntry.PK);
			AssertEquals("Entry has 1 Snapshots", 1, entry.Snapshots.Count);
		}

		public void Test5DS_StatusIsDMSCancellation()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DS", "3271420001710", LocalExportAmendmentTypeList.Codes.Cancellation);
			incomingMessage = CreateMessageForTest("GOVCBRRR3_5DS_StatusIsDMS.xml");
			SetUpSnapshot(ElectronicDocumentTypeList.Codes._5DQ);
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			AssertEquals("Entry has 2 Snapshots", 2, localExportEntry.Snapshots.Count);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			localExportEntry.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to DMS", "DMS", localExportEntry.CH_EntryStatus);
			AssertEquals("DMS", incomingMessage.EM_MessageOwner);
			AssertEquals(CustomsMessageStatusTypeList.Codes.CancellationDeclined, localExportEntry.CH_Status);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("기각", email1.Body);

			var entry = new BusinessObjectFactory().Load<CusEntryHeader>(localExportEntry.PK);
			AssertEquals("Entry has 2 Snapshots", 2, entry.Snapshots.Count);
		}

		public void Test5DS_StatusIsDMSCancellationNotOriginalMessage()
		{
			CreateEntryForLocalExport(true, "3271420001710");
			incomingMessage = CreateMessageForTest("GOVCBRRR3_5DS_StatusIsDMS.xml");
			SetUpSnapshot(ElectronicDocumentTypeList.Codes._5DQ);
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			AssertEquals("Entry has 2 Snapshots", 2, localExportEntry.Snapshots.Count);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			localExportEntry.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to DMS", "DMS", localExportEntry.CH_EntryStatus);
			AssertEquals("DMS", incomingMessage.EM_MessageOwner);
			AssertNotEquals(CustomsMessageStatusTypeList.Codes.CancellationDeclined, localExportEntry.CH_Status);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("기각", email1.Body);

			var entry = new BusinessObjectFactory().Load<CusEntryHeader>(localExportEntry.PK);
			AssertEquals("Entry has 2 Snapshots", 2, entry.Snapshots.Count);
		}
		public void Test5DS_StatusIsMFI()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DS", "3271420001710", "");
			incomingMessage = CreateMessageForTest("GOVCBRRR3_5DS_StatusIsMFI.xml");
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			localExportEntry.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to MFI", "MFI", localExportEntry.CH_EntryStatus);
			AssertEquals("MFI", incomingMessage.EM_MessageOwner);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("보완요청", email1.Body);
		}
		public void Test5DS_StatusIsOGJ()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DS", "3271420001710", "");
			incomingMessage = CreateMessageForTest("GOVCBRRR3_5DS_StatusIsOGJ.xml");
			Factory.Save();
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			incomingMessage.Reload();
			localExportEntry.Reload();

			AssertEquals("entry is located", localExportEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to OGJ", "OGJ", localExportEntry.CH_EntryStatus);
			AssertEquals("OGJ", incomingMessage.EM_MessageOwner);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email1 = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("출무대상", email1.Body);
		}
		void SetUpSnapshot(string electronicDocumentType)
		{
			localExportEntry.CH_MessageType = electronicDocumentType;
			localExportEntry.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentAccepted;

			var snapshot = localExportEntry.Snapshots.AddNew();
			snapshot.CES_MessageType = electronicDocumentType;
			snapshot.CES_Status = EntrySnapshotStatus.Lodged;
			snapshot.CES_SystemCreateTimeUtc = ZDateTime.Today;

			snapshot = localExportEntry.Snapshots.AddNew();
			snapshot.CES_MessageType = electronicDocumentType;
			snapshot.CES_Status = EntrySnapshotStatus.Lodged;
			snapshot.CES_SystemCreateTimeUtc = ZDateTime.Today.AddDays(1);
			Factory.Save();
		}
		public void Test5DRShapshotWhenStatusIsNotDeclined()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DR", "4271120006680", "");
			CreateMessageForTest("GOVCBRRR3_5DR_StatusIsOGJ.xml");
			SetUpSnapshot(ElectronicDocumentTypeList.Codes._5DP);

			AssertEquals("Entry has 2 Snapshots", 2, localExportEntry.Snapshots.Count);
			var latestSnapshot = localExportEntry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._5DP, EntrySnapshotStatus.Lodged);
			AssertNotNull("Entry has latestSnapshot", latestSnapshot);
			AssertEquals("SystemCreateTimeUtc of latestSnapshot is today+1", ZDateTime.Today.AddDays(1), latestSnapshot.CES_SystemCreateTimeUtc);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();
			Factory.Save();
			localExportEntry = new BusinessObjectFactory().Load<CusEntryHeader>(localExportEntry.PK);

			AssertEquals("EntryHeader update is OGJ", "OGJ", localExportEntry.CH_EntryStatus);
			AssertEquals("OGJ", incomingMessage.EM_MessageOwner);
			AssertEquals("Entry has 2 Snapshots", 2, localExportEntry.Snapshots.Count);
			latestSnapshot = localExportEntry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._5DP, EntrySnapshotStatus.Lodged);
			AssertEquals("SystemCreateTimeUtc of latestSnapshot is today+1", ZDateTime.Today.AddDays(1), latestSnapshot.CES_SystemCreateTimeUtc);

			var deleteSnapshot = localExportEntry.Snapshots.Cast<CusEntrySnapshot>().FirstOrDefault(x => x.CES_Status == EntrySnapshotStatus.Deleted);
			AssertNull("Snapshots are not deleted in this process case.", deleteSnapshot);
		}
		public void Test5DRShapshotWhenStatusIsDMS()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DR", "4271120006681", "");
			CreateMessageForTest("GOVCBRRR3_5DR_StatusIsDMS.xml");
			SetUpSnapshot(ElectronicDocumentTypeList.Codes._5DP);

			AssertEquals("Entry has 2 Snapshots", 2, localExportEntry.Snapshots.Count);
			var latestSnapshot = localExportEntry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._5DP, EntrySnapshotStatus.Lodged);
			AssertNotNull("Entry has latestSnapshot", latestSnapshot);
			AssertEquals("SystemCreateTimeUtc of latestSnapshot is today+1", ZDateTime.Today.AddDays(1), latestSnapshot.CES_SystemCreateTimeUtc);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();
			Factory.Save();
			localExportEntry = new BusinessObjectFactory().Load<CusEntryHeader>(localExportEntry.PK);

			AssertEquals("EntryHeader update is DMS", "DMS", localExportEntry.CH_EntryStatus);
			AssertEquals("DMS", incomingMessage.EM_MessageOwner);
			AssertEquals("Successfully updated CES_Status to 'DEL'.", 1, localExportEntry.Snapshots.Count);

			latestSnapshot = localExportEntry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._5DP, EntrySnapshotStatus.Lodged);
			AssertEquals("SystemCreateTimeUtc of latestSnapshot is today", ZDateTime.Today, latestSnapshot.CES_SystemCreateTimeUtc);
		}
		public void Test5DRShapshotWhenStatusIsCCL()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DR", "4271120006680", "");
			CreateMessageForTest("GOVCBRRR3_5DR_StatusIsCCL.xml");
			SetUpSnapshot(ElectronicDocumentTypeList.Codes._5DP);

			AssertEquals("Entry has 2 Snapshots", 2, localExportEntry.Snapshots.Count);
			var latestSnapshot = localExportEntry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._5DP, EntrySnapshotStatus.Lodged);
			AssertNotNull("Entry has latestSnapshot", latestSnapshot);
			AssertEquals("SystemCreateTimeUtc of latestSnapshot is today+1", ZDateTime.Today.AddDays(1), latestSnapshot.CES_SystemCreateTimeUtc);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();
			Factory.Save();
			localExportEntry = new BusinessObjectFactory().Load<CusEntryHeader>(localExportEntry.PK);

			AssertEquals("EntryHeader update is CCL", "CCL", localExportEntry.CH_EntryStatus);
			AssertEquals("CCL", incomingMessage.EM_MessageOwner);
			AssertEquals("Successfully updated CES_Status to 'DEL'.", 1, localExportEntry.Snapshots.Count);

			latestSnapshot = localExportEntry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._5DP, EntrySnapshotStatus.Lodged);
			AssertEquals("SystemCreateTimeUtc of latestSnapshot is today", ZDateTime.Today, latestSnapshot.CES_SystemCreateTimeUtc);
		}
		public void Test5DSShapshotWhenStatusIsNotDeclined()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DS", "3271420001710", "");
			CreateMessageForTest("GOVCBRRR3_5DS_StatusIsOGJ.xml");
			SetUpSnapshot(ElectronicDocumentTypeList.Codes._5DQ);

			AssertEquals("Entry has 2 Snapshots", 2, localExportEntry.Snapshots.Count);
			var latestSnapshot = localExportEntry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._5DQ, EntrySnapshotStatus.Lodged);
			AssertNotNull("Entry has latestSnapshot", latestSnapshot);
			AssertEquals("SystemCreateTimeUtc of latestSnapshot is today+1", ZDateTime.Today.AddDays(1), latestSnapshot.CES_SystemCreateTimeUtc);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();
			Factory.Save();
			localExportEntry = new BusinessObjectFactory().Load<CusEntryHeader>(localExportEntry.PK);

			AssertEquals("EntryHeader update is OGJ", "OGJ", localExportEntry.CH_EntryStatus);
			AssertEquals("OGJ", incomingMessage.EM_MessageOwner);
			AssertEquals("Entry has 2 Snapshots", 2, localExportEntry.Snapshots.Count);
			latestSnapshot = localExportEntry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._5DQ, EntrySnapshotStatus.Lodged);
			AssertEquals("SystemCreateTimeUtc of latestSnapshot is today+1", ZDateTime.Today.AddDays(1), latestSnapshot.CES_SystemCreateTimeUtc);

			var deleteSnapshot = localExportEntry.Snapshots.Cast<CusEntrySnapshot>().FirstOrDefault(x => x.CES_Status == EntrySnapshotStatus.Deleted);
			AssertNull("Snapshots are not deleted in this process case.", deleteSnapshot);
		}
		public void Test5DSShapshotWhenStatusIsDMS()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DS", "3271420001710", "");
			CreateMessageForTest("GOVCBRRR3_5DS_StatusIsDMS.xml");
			SetUpSnapshot(ElectronicDocumentTypeList.Codes._5DQ);

			AssertEquals("Entry has 2 Snapshots", 2, localExportEntry.Snapshots.Count);
			var latestSnapshot = localExportEntry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._5DQ, EntrySnapshotStatus.Lodged);
			AssertNotNull("Entry has latestSnapshot", latestSnapshot);
			AssertEquals("SystemCreateTimeUtc of latestSnapshot is today+1", ZDateTime.Today.AddDays(1), latestSnapshot.CES_SystemCreateTimeUtc);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();
			Factory.Save();
			localExportEntry = new BusinessObjectFactory().Load<CusEntryHeader>(localExportEntry.PK);

			AssertEquals("EntryHeader update is DMS", "DMS", localExportEntry.CH_EntryStatus);
			AssertEquals("DMS", incomingMessage.EM_MessageOwner);
			AssertEquals("Successfully updated CES_Status to 'DEL'.", 1, localExportEntry.Snapshots.Count);

			latestSnapshot = localExportEntry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._5DQ, EntrySnapshotStatus.Lodged);
			AssertEquals("SystemCreateTimeUtc of latestSnapshot is today", ZDateTime.Today, latestSnapshot.CES_SystemCreateTimeUtc);
		}
		public void Test5DSShapshotWhenStatusIsCCL()
		{
			CreateEntryWithOutgoingMessageForLocalExport("5DS", "3271420001710", "");
			CreateMessageForTest("GOVCBRRR3_5DS_StatusIsCCL.xml");
			SetUpSnapshot(ElectronicDocumentTypeList.Codes._5DQ);

			AssertEquals("Entry has 2 Snapshots", 2, localExportEntry.Snapshots.Count);
			var latestSnapshot = localExportEntry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._5DQ, EntrySnapshotStatus.Lodged);
			AssertNotNull("Entry has latestSnapshot", latestSnapshot);
			AssertEquals("SystemCreateTimeUtc of latestSnapshot is today+1", ZDateTime.Today.AddDays(1), latestSnapshot.CES_SystemCreateTimeUtc);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();
			Factory.Save();
			localExportEntry = new BusinessObjectFactory().Load<CusEntryHeader>(localExportEntry.PK);

			AssertEquals("EntryHeader update is CCL", "CCL", localExportEntry.CH_EntryStatus);
			AssertEquals("CCL", incomingMessage.EM_MessageOwner);
			AssertEquals("Successfully updated CES_Status to 'DEL'.", 1, localExportEntry.Snapshots.Count);

			latestSnapshot = localExportEntry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._5DQ, EntrySnapshotStatus.Lodged);
			AssertEquals("SystemCreateTimeUtc of latestSnapshot is today", ZDateTime.Today, latestSnapshot.CES_SystemCreateTimeUtc);
		}
		public void TestRR3_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.LocalExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, localExportGroup.PK.ToGuid()))
			{
				incomingMessage = CreateMessageForTest("GOVCBRRR3_5DP_StatusIsANT.xml");
				SampleCodeType();
				Factory.Save();
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [공항만감시 결과통보서]4271120006350 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("LocalExportGroupTest@wisetechglobal.com", email.Recipients[0].Email);

				EM_MessageInterpretation_Check(incomingMessage);
			}
		}

		public void TestRR3_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.LocalExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, localExportGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				SampleCodeType();
				incomingMessage = CreateMessageForTest("GOVCBRRR3_5DP_StatusIsANT.xml");
				CreateEntryForLocalExport(false, "4271120006350");
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[공항만감시 결과통보서] Response for Declaration Number: B00001000 / 제출번호: 4271120006350", email.Subject);
				AssertEquals("LocalExportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [환급대상수출물품 반입확인 제출]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);

				EM_MessageInterpretation_Check(incomingMessage);
			}
		}

		public void TestRR3_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.LocalExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, localExportGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				SampleCodeType();
				incomingMessage = CreateMessageForTest("GOVCBRRR3_5DP_StatusIsANT.xml");
				CreateEntryForLocalExport(true, "4271120006350");
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[공항만감시 결과통보서] Response for Declaration Number: B00001000 / 제출번호: 4271120006350", email.Subject);
				AssertEquals("CusAgent@wisetechglobal.com", email.Recipients[0].Email);

				EM_MessageInterpretation_Check(incomingMessage);
			}
		}

		public void TestUpdateEM_ApplicationReference()
		{
			CreateEntryForLocalExport(true, "4271120006350");
			var firstMessage = localExportEntry.Messages.AddNew();
			firstMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			firstMessage.EM_MessageType = "5DP";
			firstMessage.EM_SystemCreateUser = "ORG";
			firstMessage.EM_MessageNum = "1";
			firstMessage.EM_SystemCreateTimeUtc = new ZDateTime("1999-01-01");

			var secondMessage = localExportEntry.Messages.AddNew();
			secondMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			secondMessage.EM_MessageType = "5DP";
			secondMessage.EM_SystemCreateUser = "ORG";
			secondMessage.EM_MessageNum = "2";
			secondMessage.EM_SystemCreateTimeUtc = DateTime.Now;

			incomingMessage = CreateMessageForTest("GOVCBRRR3_5DP_StatusIsANT.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			incomingMessage.Reload();

			AssertEquals("2", incomingMessage.EM_ApplicationReference);
		}

		public void TestRR3_ReturnMessageSubTypeAsOutGoingMessageType_5DP()
		{
			AssertMessageSubType("5DP", "4271120006350", "GOVCBRRR3_5DP_StatusIsCCL.xml");
		}
		public void TestRR3_ReturnMessageSubTypeAsOutGoingMessageType_5DQ()
		{
			AssertMessageSubType("5DQ", "4271120006120", "GOVCBRRR3_5DQ_StatusIsCCL.xml");
		}
		public void TestRR3_ReturnMessageSubTypeAsOutGoingMessageType_5DR()
		{
			AssertMessageSubType("5DR", "4271120006680", "GOVCBRRR3_5DR_StatusIsCCL.xml");
		}
		public void TestRR3_ReturnMessageSubTypeAsOutGoingMessageType_5DS()
		{
			AssertMessageSubType("5DS", "3271420001710", "GOVCBRRR3_5DS_StatusIsCCL.xml");
		}
		void AssertMessageSubType(string outgoingMessageType, string entryNum, string incomingMessageFileName)
		{
			CreateEntryWithOutgoingMessageForLocalExport(outgoingMessageType, entryNum, "");
			incomingMessage = CreateMessageForTest(incomingMessageFileName);
			Factory.Save();

			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);

			AssertEquals(outgoingMessageType, incomingMessage.EM_MessageSubType);
		}

		void EM_MessageInterpretation_Check(EDIMessage message)
		{
			AssertContains("GOVCBR5DP", message.EM_MessageInterpretation);
			AssertContains("환급대상수출물품 반입확인 제출", message.EM_MessageInterpretation);
			AssertContains("승인/허가", message.EM_MessageInterpretation);
			AssertContains("<td>심사결과내역</td><td>remark_5DP</td>", message.EM_MessageInterpretation);
			AssertContains("[01010] 서울세관 통관지원(1)과", message.EM_MessageInterpretation);
			AssertContains("조성은 181796", message.EM_MessageInterpretation);
			AssertContains("<td>신청문서 심사일시</td><td>2020-11-20 17:26:29</td>", message.EM_MessageInterpretation);
			AssertContains("<td>심사결과 통보일시</td><td>2020-11-20 17:26:29</td>", message.EM_MessageInterpretation);
			AssertContains("01010200006731", message.EM_MessageInterpretation);
			AssertContains("<td>정정차수</td><td>0</td>", message.EM_MessageInterpretation);
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

		void CreateEntryForLocalExport(bool setCusAgent, string ce_EntryNum)
		{
			var declaration = Factory.New<JobDeclaration>();
			if (setCusAgent)
			{
				declaration.JE_GS_NKCusAgent = "AG";
			}
			localExportEntry = declaration.CustomsEntryHeaders.AddNew();
			var entryNumber = localExportEntry.EntryNumbers.AddNew();
			entryNumber.CE_EntryNum = ce_EntryNum;
			entryNumber.CE_EntryType = "LEX";
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNumber.CE_ParentID = localExportEntry.PK;
			entryNumber.CE_ParentTable = CusEntryHeader.Schema.TableName;
			entryNumber.CE_EntryStatus = ZString.Empty;
			Factory.Save();
		}
		CusEntryHeader localExportEntry;

		void CreateEntryWithOutgoingMessageForLocalExport(string em_messgeType, string ce_EntryNum, string em_subType)
		{
			if (localExportEntry == null)
			{
				CreateEntryForLocalExport(true, ce_EntryNum);
			}
			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = em_messgeType;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_LinkTable = CusEntryHeader.Schema.TableName;
			outgoingMessage.EM_LinkUniqueID = localExportEntry.PK;
			outgoingMessage.EM_LinkedObject = localExportEntry;
			outgoingMessage.EM_MessageSubType = em_subType;
		}

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBRRR3MessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._RR3;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			return incomingMessage;
		}
		EDIMessage incomingMessage;

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.LocalExport.Incoming";

		void SampleCodeType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "010", "서울세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "030", "부산세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "10", "통관지원(1)과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
		}
	}
}
