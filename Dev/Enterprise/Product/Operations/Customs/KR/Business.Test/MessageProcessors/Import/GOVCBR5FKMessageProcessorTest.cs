using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5FKMessageProcessorTest : XMLMessageTestHelper<GOVCBR5FKMessageProcessorTest>
	{
		public void Test5FK_ANT()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR5FK_ANT.xml");

			var messageR99 = entry.Messages.AddNew();
			messageR99.EM_SystemCreateTimeUtc = new ZDateTime(2020, 08, 26);
			messageR99.EM_SystemCreateUser = "ORG";
			messageR99.EM_MessageType = ElectronicDocumentTypeList.Codes._R99;
			messageR99.EM_MessageSubType = ElectronicDocumentTypeList.Codes._5FE;
			Factory.Save();

			var amendmentSessionalData = entry.EntryInstruction.AmendmentSessionalDataCollection.Cast<AmendmentSessionalData>().SingleOrDefault(x => x.CSI_LineNo == 2);
			var penaltyExemptionSessionalData = amendmentSessionalData.PenaltyExemptionSessionalData;

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			AssertEquals("PreCondition: CSI_Status Empty", ZString.Empty, amendmentSessionalData.CSI_Status);
			AssertEquals("PreCondition: CSI_Value Empty", ZDecimal.Zero, penaltyExemptionSessionalData.CSI_Value);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();
			amendmentSessionalData.Reload();
			penaltyExemptionSessionalData.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("EntryHeader CH_EntryStatus update is ANT", "ANT", entry.CH_EntryStatus);
			AssertEquals("ANT", incomingMessage.EM_MessageOwner);
			AssertEquals("CSI_Status is ANT", "ANT", amendmentSessionalData.CSI_Status);
			AssertEquals("CSI_Value is 3", 3m, amendmentSessionalData.PenaltyExemptionSessionalData.CSI_Value);
			AssertEquals(entry.Messages[0].EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals("5FE", incomingMessage.EM_MessageSubType);

			AssertContains("수입신고 정정 신청서", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-08-26", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-08-27", incomingMessage.EM_MessageInterpretation);
			AssertContains("12345-20-000045M", incomingMessage.EM_MessageInterpretation);
			AssertContains("승인통보", incomingMessage.EM_MessageInterpretation);
			AssertContains("0127-020-11-20-0-004951-3", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("2020-08-26", email.Body);
			AssertContains("2020-08-27", email.Body);
			AssertContains("12345-20-000045M", email.Body);
			AssertContains("승인통보", email.Body);
			AssertContains("0127-020-11-20-0-004951-3", email.Body);

			var stmAlogFilter = new ZQuery(StmALogSchema.SL_Table, CusEntryHeader.Schema.TableName);
			stmAlogFilter.AddToFilter(StmALogSchema.SL_Parent, entry.PK);
			stmAlogFilter.AddToFilter(StmALogSchema.SL_SE_NKEvent, "CES");
			stmAlogFilter.AddToFilter(StmALogSchema.SL_Reference, entry.CH_EntryStatus);

			var alog = Factory.LoadTop1<StmALog>(stmAlogFilter);
			AssertEquals("StmAlog SL_EventTime is updated", "2020-08-27", alog.SL_EventTime.ToString(DateFormatType.DateKorean));

			var dutyTaxTBD = entry.Charges.OfType<CusEntryHeaderCharges>().FirstOrDefault(x => x.C1_ChargeType == TaxType.TBD);
			AssertEquals("CusEntryHeaderCharges C1_ChargeAmount is updated when C1_ChargeType is TBD", 1m, dutyTaxTBD.C1_ChargeAmount);

			var dutyTaxTAD = entry.Charges.OfType<CusEntryHeaderCharges>().FirstOrDefault(x => x.C1_ChargeType == TaxType.TAD);
			AssertEquals("CusEntryHeaderCharges C1_ChargeAmount is updated when C1_ChargeType is TAD", 2m, dutyTaxTAD.C1_ChargeAmount);

			var dutyTaxTPA = entry.Charges.OfType<CusEntryHeaderCharges>().FirstOrDefault(x => x.C1_ChargeType == TaxType.TPA);
			AssertEquals("CusEntryHeaderCharges C1_ChargeAmount is updated when C1_ChargeType is TPA", 3m, dutyTaxTPA.C1_ChargeAmount);
		}

		public void Test5FK_CCL()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR5FK_CCL.xml");
			Factory.Save();

			var amendmentSessionalData = entry.EntryInstruction.AmendmentSessionalDataCollection.Cast<AmendmentSessionalData>().SingleOrDefault(x => x.CSI_LineNo == 2);
			var penaltyExemptionSessionalData = amendmentSessionalData.PenaltyExemptionSessionalData;

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();
			amendmentSessionalData.Reload();
			penaltyExemptionSessionalData.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("EntryHeader update is CCL", "CCL", entry.CH_EntryStatus);
			AssertEquals("CCL", incomingMessage.EM_MessageOwner);
			AssertEquals("CSI_Status is CCL", "CCL", amendmentSessionalData.CSI_Status);
			AssertEquals("CSI_Value is 3", 3m, amendmentSessionalData.PenaltyExemptionSessionalData.CSI_Value);

			AssertContains("수입신고 정정 신청서", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-08-26", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-08-27", incomingMessage.EM_MessageInterpretation);
			AssertContains("12345-20-000045M", incomingMessage.EM_MessageInterpretation);
			AssertContains("승인취소", incomingMessage.EM_MessageInterpretation);
			AssertContains("0127-020-11-20-0-004951-3", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("2020-08-26", email.Body);
			AssertContains("2020-08-27", email.Body);
			AssertContains("12345-20-000045M", email.Body);
			AssertContains("승인취소", email.Body);
			AssertContains("0127-020-11-20-0-004951-3", email.Body);

			var stmAlogFilter = new ZQuery(StmALogSchema.SL_Table, CusEntryHeader.Schema.TableName);
			stmAlogFilter.AddToFilter(StmALogSchema.SL_Parent, entry.PK);
			stmAlogFilter.AddToFilter(StmALogSchema.SL_SE_NKEvent, "CES");
			stmAlogFilter.AddToFilter(StmALogSchema.SL_Reference, entry.CH_EntryStatus);

			var alog = Factory.LoadTop1<StmALog>(stmAlogFilter);
			AssertEquals("StmAlog SL_EventTime is updated", "2020-08-27", alog.SL_EventTime.ToString(DateFormatType.DateKorean));

			var dutyTaxTBD = entry.Charges.OfType<CusEntryHeaderCharges>().FirstOrDefault(x => x.C1_ChargeType == TaxType.TBD);
			AssertEquals("CusEntryHeaderCharges C1_ChargeAmount is updated when C1_ChargeType is TBD", 1m, dutyTaxTBD.C1_ChargeAmount);

			var dutyTaxTAD = entry.Charges.OfType<CusEntryHeaderCharges>().FirstOrDefault(x => x.C1_ChargeType == TaxType.TAD);
			AssertEquals("CusEntryHeaderCharges C1_ChargeAmount is updated when C1_ChargeType is TAD", 2m, dutyTaxTAD.C1_ChargeAmount);

			var dutyTaxTPA = entry.Charges.OfType<CusEntryHeaderCharges>().FirstOrDefault(x => x.C1_ChargeType == TaxType.TPA);
			AssertEquals("CusEntryHeaderCharges C1_ChargeAmount is updated when C1_ChargeType is TPA", 3m, dutyTaxTPA.C1_ChargeAmount);
		}

		public void Test5FK_RJC()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR5FK_RJC.xml");
			Factory.Save();

			var amendmentSessionalData = entry.EntryInstruction.AmendmentSessionalDataCollection.Cast<AmendmentSessionalData>().SingleOrDefault(x => x.CSI_LineNo == 2);
			var penaltyExemptionSessionalData = amendmentSessionalData.PenaltyExemptionSessionalData;

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();
			amendmentSessionalData.Reload();
			penaltyExemptionSessionalData.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("EntryHeader update is RJC", "RJC", entry.CH_EntryStatus);
			AssertEquals("RJC", incomingMessage.EM_MessageOwner);
			AssertEquals("CSI_Status is RJC", "RJC", amendmentSessionalData.CSI_Status);
			AssertEquals("CSI_Value is 3", 3m, amendmentSessionalData.PenaltyExemptionSessionalData.CSI_Value);

			AssertContains("수입신고 정정 신청서", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-08-26", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-08-27", incomingMessage.EM_MessageInterpretation);
			AssertContains("12345-20-000045M", incomingMessage.EM_MessageInterpretation);
			AssertContains("정정신청 각하", incomingMessage.EM_MessageInterpretation);
			AssertContains("0127-020-11-20-0-004951-3", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("2020-08-26", email.Body);
			AssertContains("2020-08-27", email.Body);
			AssertContains("12345-20-000045M", email.Body);
			AssertContains("정정신청 각하", email.Body);
			AssertContains("0127-020-11-20-0-004951-3", email.Body);

			var stmAlogFilter = new ZQuery(StmALogSchema.SL_Table, CusEntryHeader.Schema.TableName);
			stmAlogFilter.AddToFilter(StmALogSchema.SL_Parent, entry.PK);
			stmAlogFilter.AddToFilter(StmALogSchema.SL_SE_NKEvent, "CES");
			stmAlogFilter.AddToFilter(StmALogSchema.SL_Reference, entry.CH_EntryStatus);

			var alog = Factory.LoadTop1<StmALog>(stmAlogFilter);
			AssertEquals("StmAlog SL_EventTime is updated", "2020-08-27", alog.SL_EventTime.ToString(DateFormatType.DateKorean));

			var dutyTaxTBD = entry.Charges.OfType<CusEntryHeaderCharges>().FirstOrDefault(x => x.C1_ChargeType == TaxType.TBD);
			AssertEquals("CusEntryHeaderCharges C1_ChargeAmount is updated when C1_ChargeType is TBD", 1m, dutyTaxTBD.C1_ChargeAmount);

			var dutyTaxTAD = entry.Charges.OfType<CusEntryHeaderCharges>().FirstOrDefault(x => x.C1_ChargeType == TaxType.TAD);
			AssertEquals("CusEntryHeaderCharges C1_ChargeAmount is updated when C1_ChargeType is TAD", 2m, dutyTaxTAD.C1_ChargeAmount);

			var dutyTaxTPA = entry.Charges.OfType<CusEntryHeaderCharges>().FirstOrDefault(x => x.C1_ChargeType == TaxType.TPA);
			AssertEquals("CusEntryHeaderCharges C1_ChargeAmount is updated when C1_ChargeType is TPA", 3m, dutyTaxTPA.C1_ChargeAmount);
		}

		public void Test5FK_DMS()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR5FK_DMS.xml");
			Factory.Save();

			var amendmentSessionalData = entry.EntryInstruction.AmendmentSessionalDataCollection.Cast<AmendmentSessionalData>().SingleOrDefault(x => x.CSI_LineNo == 2);
			var penaltyExemptionSessionalData = amendmentSessionalData.PenaltyExemptionSessionalData;

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();
			amendmentSessionalData.Reload();
			penaltyExemptionSessionalData.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("EntryHeader update is DMS", "DMS", entry.CH_EntryStatus);
			AssertEquals("DMS", incomingMessage.EM_MessageOwner);
			AssertEquals("CSI_Status is DMS", "DMS", amendmentSessionalData.CSI_Status);
			AssertEquals("CSI_Value is 3", 3m, amendmentSessionalData.PenaltyExemptionSessionalData.CSI_Value);

			AssertContains("수입신고 정정 신청서", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-08-26", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-08-27", incomingMessage.EM_MessageInterpretation);
			AssertContains("12345-20-000045M", incomingMessage.EM_MessageInterpretation);
			AssertContains("기각", incomingMessage.EM_MessageInterpretation);
			AssertContains("0127-020-11-20-0-004951-3", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("2020-08-26", email.Body);
			AssertContains("2020-08-27", email.Body);
			AssertContains("12345-20-000045M", email.Body);
			AssertContains("기각", email.Body);
			AssertContains("0127-020-11-20-0-004951-3", email.Body);

			var stmAlogFilter = new ZQuery(StmALogSchema.SL_Table, CusEntryHeader.Schema.TableName);
			stmAlogFilter.AddToFilter(StmALogSchema.SL_Parent, entry.PK);
			stmAlogFilter.AddToFilter(StmALogSchema.SL_SE_NKEvent, "CES");
			stmAlogFilter.AddToFilter(StmALogSchema.SL_Reference, entry.CH_EntryStatus);

			var alog = Factory.LoadTop1<StmALog>(stmAlogFilter);
			AssertEquals("StmAlog SL_EventTime is updated", "2020-08-27", alog.SL_EventTime.ToString(DateFormatType.DateKorean));

			var dutyTaxTBD = entry.Charges.OfType<CusEntryHeaderCharges>().FirstOrDefault(x => x.C1_ChargeType == TaxType.TBD);
			AssertEquals("CusEntryHeaderCharges C1_ChargeAmount is updated when C1_ChargeType is TBD", 1m, dutyTaxTBD.C1_ChargeAmount);

			var dutyTaxTAD = entry.Charges.OfType<CusEntryHeaderCharges>().FirstOrDefault(x => x.C1_ChargeType == TaxType.TAD);
			AssertEquals("CusEntryHeaderCharges C1_ChargeAmount is updated when C1_ChargeType is TAD", 2m, dutyTaxTAD.C1_ChargeAmount);

			var dutyTaxTPA = entry.Charges.OfType<CusEntryHeaderCharges>().FirstOrDefault(x => x.C1_ChargeType == TaxType.TPA);
			AssertEquals("CusEntryHeaderCharges C1_ChargeAmount is updated when C1_ChargeType is TPA", 3m, dutyTaxTPA.C1_ChargeAmount);
		}

		public void TestEmpty()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR5FK_Empty.xml");
			Factory.Save();

			AssertNoExceptionThrown("When no xml Elements is there, system should still proceed successfully", () =>
						new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch());
			entry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);

			AssertContains("수입신고 정정 신청서", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("2020-08-27", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("승인통보", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("0127-020-11-20-0-004951-3", incomingMessage.EM_MessageInterpretation);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertNotContains("2020-08-27", email.Body);
			AssertNotContains("승인통보", email.Body);
			AssertNotContains("0127-020-11-20-0-004951-3", email.Body);

			var dutyTaxTBD = entry.Charges.OfType<CusEntryHeaderCharges>().FirstOrDefault(x => x.C1_ChargeType == TaxType.TBD);
			AssertNull("CusEntryHeaderCharges C1_ChargeAmount is Not updated when chargeAmount value is 0", dutyTaxTBD);

			var dutyTaxTAD = entry.Charges.OfType<CusEntryHeaderCharges>().FirstOrDefault(x => x.C1_ChargeType == TaxType.TAD);
			AssertNull("CusEntryHeaderCharges C1_ChargeAmount is Not updated when chargeAmount value is 0", dutyTaxTAD);

			var dutyTaxTPA = entry.Charges.OfType<CusEntryHeaderCharges>().FirstOrDefault(x => x.C1_ChargeType == TaxType.TPA);
			AssertNull("CusEntryHeaderCharges C1_ChargeAmount is Not updated when chargeAmount value is 0", dutyTaxTPA);
		}

		public void TestImport5FK_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				CreateMessageForTest("GOVCBR5FK_ANT.xml");
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [수입(납세)신고 정정 처리결과 통보서]1234520000045M 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		public void TestImport5FK_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBR5FK_ANT.xml");
				CreateEntryForImport(false);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수입(납세)신고 정정 처리결과 통보서] Response for Declaration Number: B00001000 / 제출번호: 1234520000045M", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [수입정정신고서]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);
			}
		}

		public void TestImport5FK_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBR5FK_ANT.xml");
				CreateEntryForImport(true);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수입(납세)신고 정정 처리결과 통보서] Response for Declaration Number: B00001000 / 제출번호: 1234520000045M", email.Subject);
				AssertEquals("CusAgent@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		void SetUpSnapshot(CusEntryHeader entry, string electronicDocumentType)
		{
			entry.CH_MessageType = electronicDocumentType;

			var snapshot = entry.Snapshots.AddNew();
			snapshot.CES_MessageType = electronicDocumentType;
			snapshot.CES_Status = EntrySnapshotStatus.Lodged;
			snapshot.CES_SystemCreateTimeUtc = ZDateTime.Today;

			snapshot = entry.Snapshots.AddNew();
			snapshot.CES_MessageType = electronicDocumentType;
			snapshot.CES_Status = EntrySnapshotStatus.Lodged;
			snapshot.CES_SystemCreateTimeUtc = ZDateTime.Today.AddDays(1);
		}
		public void TestShapshotWhenStatusIsANT()
		{
			var entry = CreateEntryWithOutgoingMessageForImport();
			SetUpSnapshot(entry, ElectronicDocumentTypeList.Codes._929);

			CreateMessageForTest("GOVCBR5FK_ANT.xml");
			Factory.Save();

			AssertEquals("Entry has 2 Snapshots", 2, entry.Snapshots.Count);
			var latestSnapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._929, EntrySnapshotStatus.Lodged);
			AssertEquals("SystemCreateTimeUtc of latestSnapshot is today+1", ZDateTime.Today.AddDays(1), latestSnapshot.CES_SystemCreateTimeUtc);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			Factory.Save();
			entry = new BusinessObjectFactory().Load<CusEntryHeader>(entry.PK);
			AssertEquals("EntryHeader update is ANT", "ANT", entry.CH_EntryStatus);

			latestSnapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._929, EntrySnapshotStatus.Lodged);
			AssertEquals("Entry has 2 Snapshots", 2, entry.Snapshots.Count);
			AssertEquals("SystemCreateTimeUtc of latestSnapshot is today+1", ZDateTime.Today.AddDays(1), latestSnapshot.CES_SystemCreateTimeUtc);

			var deleteSnapshot = entry.Snapshots.Cast<CusEntrySnapshot>().FirstOrDefault(x => x.CES_Status == EntrySnapshotStatus.Deleted);
			AssertNull("Snapshots are not deleted in this process case.", deleteSnapshot);
		}
		public void TestShapshotWhenStatusIsCCL()
		{
			var entry = CreateEntryWithOutgoingMessageForImport();
			SetUpSnapshot(entry, ElectronicDocumentTypeList.Codes._929);

			CreateMessageForTest("GOVCBR5FK_CCL.xml");
			Factory.Save();

			AssertEquals("Entry has 2 Snapshots", 2, entry.Snapshots.Count);
			var latestSnapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._929, EntrySnapshotStatus.Lodged);
			AssertNotNull("Entry has latestSnapshot", latestSnapshot);
			AssertEquals("SystemCreateTimeUtc of latestSnapshot is today+1", ZDateTime.Today.AddDays(1), latestSnapshot.CES_SystemCreateTimeUtc);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			Factory.Save();
			entry = new BusinessObjectFactory().Load<CusEntryHeader>(entry.PK);

			AssertEquals("EntryHeader update is CCL", "CCL", entry.CH_EntryStatus);
			AssertEquals("Successfully updated CES_Status to 'DEL'.", 1, entry.Snapshots.Count);

			latestSnapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._929, EntrySnapshotStatus.Lodged);
			AssertEquals("SystemCreateTimeUtc of latestSnapshot is today", ZDateTime.Today, latestSnapshot.CES_SystemCreateTimeUtc);
		}
		public void TestShapshotWhenStatusIsRJC()
		{
			var entry = CreateEntryWithOutgoingMessageForImport();
			SetUpSnapshot(entry, ElectronicDocumentTypeList.Codes._929);

			CreateMessageForTest("GOVCBR5FK_RJC.xml");
			Factory.Save();

			AssertEquals("Entry has 2 Snapshots", 2, entry.Snapshots.Count);
			var latestSnapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._929, EntrySnapshotStatus.Lodged);
			AssertNotNull("Entry has latestSnapshot", latestSnapshot);
			AssertEquals("SystemCreateTimeUtc of latestSnapshot is today+1", ZDateTime.Today.AddDays(1), latestSnapshot.CES_SystemCreateTimeUtc);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			Factory.Save();
			entry = new BusinessObjectFactory().Load<CusEntryHeader>(entry.PK);

			AssertEquals("EntryHeader update is RJC", "RJC", entry.CH_EntryStatus);
			AssertEquals("Successfully updated CES_Status to 'DEL'.", 1, entry.Snapshots.Count);

			latestSnapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._929, EntrySnapshotStatus.Lodged);
			AssertEquals("SystemCreateTimeUtc of latestSnapshot is today", ZDateTime.Today, latestSnapshot.CES_SystemCreateTimeUtc);
		}
		public void TestShapshotWhenStatusIsDMS()
		{
			var entry = CreateEntryWithOutgoingMessageForImport();
			SetUpSnapshot(entry, ElectronicDocumentTypeList.Codes._929);

			CreateMessageForTest("GOVCBR5FK_DMS.xml");
			Factory.Save();

			AssertEquals("Entry has 2 Snapshots", 2, entry.Snapshots.Count);
			var latestSnapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._929, EntrySnapshotStatus.Lodged);
			AssertNotNull("Entry has latestSnapshot", latestSnapshot);
			AssertEquals("SystemCreateTimeUtc of latestSnapshot is today+1", ZDateTime.Today.AddDays(1), latestSnapshot.CES_SystemCreateTimeUtc);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			Factory.Save();
			entry = new BusinessObjectFactory().Load<CusEntryHeader>(entry.PK);

			AssertEquals("EntryHeader update is DMS", "DMS", entry.CH_EntryStatus);
			AssertEquals("Successfully updated CES_Status to 'DEL'.", 1, entry.Snapshots.Count);

			latestSnapshot = entry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._929, EntrySnapshotStatus.Lodged);
			AssertEquals("SystemCreateTimeUtc of latestSnapshot is today", ZDateTime.Today, latestSnapshot.CES_SystemCreateTimeUtc);
		}

		public void TestNonCreateStatementHeader()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR5FK_EmptyNoticeNumber.xml");
			Factory.Save();

			ZQuery query = new ZQuery(CusStatementHeaderSchema.B2_GC, entry.Declaration.JE_GC);
			query.AddToFilter(CusStatementHeaderSchema.B2_StatementType, StatementHeaderTypeList.Codes.CustomsDisbursementBill);
			var statementCollection = Factory.Load<CusStatementHeader>(query);
			var beforeCount = statementCollection.Length;

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			statementCollection = Factory.Load<CusStatementHeader>(query);
			var afterCount = statementCollection.Length;

			AssertEquals(beforeCount, afterCount);

			incomingMessage = CreateMessageForTest("GOVCBR5FK_ANT.xml"); // TransactionNatureCode Not In ('O', 'A', 'B')
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			statementCollection = Factory.Load<CusStatementHeader>(query);
			afterCount = statementCollection.Length;

			AssertEquals(beforeCount, afterCount);

			incomingMessage = CreateMessageForTest("GOVCBR5FK_NameCodeIsNot11.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			statementCollection = Factory.Load<CusStatementHeader>(query);
			afterCount = statementCollection.Length;

			AssertEquals(beforeCount, afterCount);
		}

		public void TestCreateStatementHeaderTransactionNatureCodeIsA()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			entry.Declaration.JE_CustomsOffice = "010";
			entry.Declaration.JE_OH_DutyPayer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "레디코리아").PK;
			var incomingMessage = CreateMessageForTest("GOVCBR5FK_TransactionNatureCodeIsA.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			var newFactory = new BusinessObjectFactory();
			var statement = new CusStatementHeader.Loader(newFactory).Load("0127020112000049513", entry.Declaration.JE_GC, StatementHeaderTypeList.Codes.CustomsDisbursementBill);

			AssertNotNull(statement);
			AssertEquals(1, statement.Messages.Count);
			AssertEquals(incomingMessage.EM_MessageNum, statement.B2_IncomingMessageNo);
			AssertEquals(new ZDateTime(2020, 08, 27, 00, 02, 00), statement.B2_ProcessDate);
			AssertEquals(new ZDateTime(2020, 08, 26), statement.B2_PrintDate);
			AssertEquals(new ZDateTime(2020, 08, 27), statement.B2_DueDate);
			AssertEquals(1m, statement.B2_StatementAmount);
			AssertEquals("A", statement.B2_Status);
			AssertEquals(StatementHeaderPaymentStatusList.Codes.PYI, statement.B2_PaymentStatus);
			AssertEquals(entry.Declaration.JE_CustomsOffice, statement.B2_ProcessPort);
			AssertEquals(entry.Declaration.JE_OH_DutyPayer, statement.B2_OH_Importer);
			AssertContains(statement.Messages[0].EM_MessageInterpretation, incomingMessage.EM_MessageInterpretation);

			var statementLine = statement.StatementLines[0];
			AssertEquals(KRJobMessageTypeList.Codes.Import, statementLine.B3_EntryType);
			AssertEquals("1234520000045M", statementLine.B3_EntryNum);
			AssertEquals(1u, statementLine.B3_SequenceNumber);
			AssertEquals(-8313807m, statementLine.B3_CustomsFeesTotal);

			assertStatementLineCharge(statement.StatementLines[0], ChargeTypeList.Codes.Duty, -7557760);
			assertStatementLineCharge(statement.StatementLines[0], ChargeTypeList.Codes.VAT, -755780);
			assertStatementLineCharge(statement.StatementLines[0], ChargeTypeList.Codes.SpecialConsumptionTax, -10);
			assertStatementLineCharge(statement.StatementLines[0], ChargeTypeList.Codes.TransportationTax, -20);
			assertStatementLineCharge(statement.StatementLines[0], ChargeTypeList.Codes.LiquorTax, -30);
			assertStatementLineCharge(statement.StatementLines[0], ChargeTypeList.Codes.EducationTax, -40);
			assertStatementLineCharge(statement.StatementLines[0], ChargeTypeList.Codes.AgricultureTax, -50);
			assertStatementLineCharge(statement.StatementLines[0], ChargeTypeList.Codes.PenaltyForLateOrMissedDeclaration, -120);
			assertStatementLineCharge(statement.StatementLines[0], ChargeTypeList.Codes.PenaltyAndInterest, 3);
		}

		public void TestCreateStatementHeaderTransactionNatureCodeIsO()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			entry.Declaration.JE_CustomsOffice = "010";
			entry.Declaration.JE_OH_DutyPayer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "레디코리아").PK;
			var incomingMessage = CreateMessageForTest("GOVCBR5FK_TransactionNatureCodeIsO.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			var newFactory = new BusinessObjectFactory();
			var statement = new CusStatementHeader.Loader(newFactory).Load("0127020112000049513", entry.Declaration.JE_GC, StatementHeaderTypeList.Codes.CustomsDisbursementBill);

			AssertNotNull(statement);
			AssertEquals(1, statement.Messages.Count);
			AssertEquals(incomingMessage.EM_MessageNum, statement.B2_IncomingMessageNo);
			AssertEquals(new ZDateTime(2020, 08, 27, 00, 02, 00), statement.B2_ProcessDate);
			AssertEquals(ZDateTime.Empty, statement.B2_PrintDate);
			AssertEquals(ZDateTime.Empty, statement.B2_DueDate);
			AssertEquals(10m, statement.B2_StatementAmount);
			AssertEquals("O", statement.B2_Status);
			AssertEquals(StatementHeaderPaymentStatusList.Codes.PYI, statement.B2_PaymentStatus);
			AssertEquals(entry.Declaration.JE_CustomsOffice, statement.B2_ProcessPort);
			AssertEquals(entry.Declaration.JE_OH_DutyPayer, statement.B2_OH_Importer);

			var statementLine = statement.StatementLines[0];
			AssertEquals(KRJobMessageTypeList.Codes.Import, statementLine.B3_EntryType);
			AssertEquals("1234520000045M", statementLine.B3_EntryNum);
			AssertEquals(1u, statementLine.B3_SequenceNumber);
			AssertEquals(21288710m, statementLine.B3_CustomsFeesTotal);

			assertStatementLineCharge(statementLine, ChargeTypeList.Codes.Duty, 5038500);
			assertStatementLineCharge(statementLine, ChargeTypeList.Codes.VAT, 16249180);
			assertStatementLineCharge(statementLine, ChargeTypeList.Codes.PenaltyForLateOrMissedDeclaration, 1000);
			assertStatementLineCharge(statementLine, ChargeTypeList.Codes.PenaltyAndInterest, 30);
		}

		public void TestCreateStatementHeaderTransactionNatureCodeIsOPaymentTypeIs33()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			entry.Declaration.JE_CustomsOffice = "010";
			entry.Declaration.JE_OH_DutyPayer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "레디코리아").PK;
			outgoingMessage.EM_ApplicationReference = "2";

			entry.Declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._33;
			var incomingMessage = CreateMessageForTest("GOVCBR5FK_TransactionNatureCodeIsO.xml");

			var statementSetup = Factory.New<CusStatementHeader>();
			statementSetup.B2_StatementNumber = "0127020112000049513";
			statementSetup.B2_GC = entry.Declaration.JE_GC;
			statementSetup.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;

			var import929 = new ImportEntryHeaderCreator().Create(entry);
			import929.RoundDecimalValueRoundedWithDecimalPlaces();
			entry.Snapshots.RemoveAndDeleteAll();
			using (var stream = KRXmlObjectSerializer.Serialize(import929))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._929, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._929);
				entry.Snapshots[0].CES_VersionNumber = 2;
				Factory.Save();
			}

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			var newFactory = new BusinessObjectFactory();
			var statement = new CusStatementHeader.Loader(newFactory).Load("0127020112000049513", entry.Declaration.JE_GC, StatementHeaderTypeList.Codes.CustomsDisbursementBill);

			AssertNotNull(statement);
			AssertEquals(1, statement.Messages.Count);
			AssertEquals(incomingMessage.EM_MessageNum, statement.B2_IncomingMessageNo);
			AssertEquals(new ZDateTime(2020, 08, 27, 00, 02, 00), statement.B2_ProcessDate);
			AssertEquals(ZDateTime.Empty, statement.B2_PrintDate);
			AssertEquals(ZDateTime.Empty, statement.B2_DueDate);
			AssertEquals(16249180m, statement.B2_StatementAmount);
			AssertEquals("O", statement.B2_Status);
			AssertEquals(StatementHeaderPaymentStatusList.Codes.PYI, statement.B2_PaymentStatus);
			AssertEquals(entry.Declaration.JE_CustomsOffice, statement.B2_ProcessPort);
			AssertEquals(entry.Declaration.JE_OH_DutyPayer, statement.B2_OH_Importer);

			var statementLine = statement.StatementLines[0];
			AssertEquals(KRJobMessageTypeList.Codes.Import, statementLine.B3_EntryType);
			AssertEquals("1234520000045M", statementLine.B3_EntryNum);
			AssertEquals(1u, statementLine.B3_SequenceNumber);
			AssertEquals(21288710m, statementLine.B3_CustomsFeesTotal);

			assertStatementLineCharge(statementLine, ChargeTypeList.Codes.Duty, 5038500);
			assertStatementLineCharge(statementLine, ChargeTypeList.Codes.VAT, 16249180);
			assertStatementLineCharge(statementLine, ChargeTypeList.Codes.PenaltyForLateOrMissedDeclaration, 1000);
			assertStatementLineCharge(statementLine, ChargeTypeList.Codes.PenaltyAndInterest, 30);
		}

		void assertStatementLineCharge(CusStatementLine statementLine, string chargeType, decimal amount)
		{
			AssertEquals(amount, statementLine.Charges.Cast<CusStatementLineCharge>()?.FirstOrDefault(x => x.B4_ChargeType == chargeType).B4_ChargeAmount);
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
			declaration.JE_DeclarationReference = "B00001000";
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
			outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5FE;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_LinkTable = CusEntryHeader.Schema.TableName;
			outgoingMessage.EM_LinkedObject = importEntry;
			outgoingMessage.EM_ApplicationReference = "2";

			CreateAmendmentSessionalData(importEntry);

			return importEntry;
		}
		EDIMessage outgoingMessage;

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBR5FKMessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5FK;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			return incomingMessage;
		}

		void CreateAmendmentSessionalData(CusEntryHeader entry)
		{
			var instruction = entry.Declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var amendmentSessionalData = entry.EntryInstruction.AmendmentSessionalDataCollection.AddNew();
			amendmentSessionalData.CSI_LineNo = 2;
			amendmentSessionalData.CSI_Code = DutyTaxCorrectionCodeList.Codes.A;

			var penaltyExemptionSessionalData = amendmentSessionalData.PenaltyExemptionSessionalData;
			penaltyExemptionSessionalData.CSI_LineNo = 1;
			penaltyExemptionSessionalData.CSI_Code = DutyPenaltyExemptionCodeList.Codes.Y;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
	}
}
