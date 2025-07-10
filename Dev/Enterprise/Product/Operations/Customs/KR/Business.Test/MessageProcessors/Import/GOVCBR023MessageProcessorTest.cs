using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR023MessageProcessorTest : XMLMessageTestHelper<GOVCBR023MessageProcessorTest>
	{
		public void TestStatusIsCLR()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR023_CLR.xml");
			var entryNum = LoadEntryNumber(entry, ElectronicDocumentTypeList.Codes._934);
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			AssertEquals("PreCondition: Entry Status is Empty", ZString.Empty, entry.CH_EntryStatus);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();
			entryNum.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to CLR", "CLR", entry.CH_EntryStatus);
			AssertEquals("entry status is CLR when entry CH_EntryReleaseDate is updated", "202008211343", entry.CH_EntryReleaseDate.ToString("yyyyMMddHHmm"));
			AssertEquals("CusEntryHeader CH_CustomsMessafeRemarks is updated", "2020-08-21 13:43:31" + "\r\n" + "전자서류(원본) 제출 대상입니다. 첨부서류는 원본을 변환한 이미지 파일을 전자제출 하여야 합니다. 문의사항은 통관지 세관 수입담당자에게 문의 바랍니다.\r\n", entry.CH_CustomsMessageRemarks);
			AssertEquals("Current EDIMessage.EM_ApplicationReference is updated", "1", incomingMessage.EM_ApplicationReference);
			AssertEquals("Current EDIMessage.EM_ApplicationReference is updated", "1235489520125487592", entry.CH_BGMReference);

			var stmAlogFilter = new ZQuery(StmALogSchema.SL_Parent, entry.PK);
			stmAlogFilter.AddToFilter(StmALogSchema.SL_Table, CusEntryHeader.Schema.TableName);
			var alog = Factory.LoadTop1<StmALog>(stmAlogFilter);
			AssertEquals("StmAlog SL_EventTime is updated", "202008211343", alog.SL_EventTime.ToString("yyyyMMddHHmm"));
			AssertEquals("CusEntryNum CE_ExiryDate is updated", "20200901", entryNum.CE_ExpiryDate.ToString("yyyyMMdd"));

			AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-08-21 13:43", incomingMessage.EM_MessageInterpretation);
			AssertContains("12345-20-000045M", incomingMessage.EM_MessageInterpretation);
			AssertContains("1235-489-52-01-2-548759-2", incomingMessage.EM_MessageInterpretation);
			AssertContains("이정주", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-09-01", incomingMessage.EM_MessageInterpretation);
			AssertContains("전자서류(원본) 제출 대상입니다. 첨부서류는 원본을 변환한 이미지 파일을 전자제출 하여야 합니다. 문의사항은 통관지 세관 수입담당자에게 문의 바랍니다.", incomingMessage.EM_MessageInterpretation);
			AssertContains("선별결과 내역", incomingMessage.EM_MessageInterpretation);
			AssertContains("The second html table is formatted to have at most four rows and all entry line numbers should be displayed", "검사대상 선별", incomingMessage.EM_MessageInterpretation);
			AssertContains("1", incomingMessage.EM_MessageInterpretation);
			AssertContains("2", incomingMessage.EM_MessageInterpretation);
			AssertContains("검사생략으로 선별", incomingMessage.EM_MessageInterpretation);
			AssertContains("전자서류(원본) 제출 대상입니다. 첨부서류는 원본을 변환한 이미지 파일을 전자제출 하여야 합니다. 문의사항은 통관지 세관 수입담당자에게 문의 바랍니다.", incomingMessage.EM_MessageInterpretation);
			AssertContains("The second html table is formatted to have at most four rows and all entry line numbers should be displayed", "검사대상 선별", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("수입신고서", email.Body);
			AssertContains("2020-08-21 13:43", email.Body);
			AssertContains("12345-20-000045M", email.Body);
			AssertContains("1235-489-52-01-2-548759-2", email.Body);
			AssertContains("이정주", email.Body);
			AssertContains("2020-09-01", email.Body);
			AssertContains("전자서류(원본) 제출 대상입니다. 첨부서류는 원본을 변환한 이미지 파일을 전자제출 하여야 합니다. 문의사항은 통관지 세관 수입담당자에게 문의 바랍니다.", email.Body);
			AssertContains("선별결과 내역", email.Body);
			AssertContains("The second html table is formatted to have at most four rows and all entry line numbers should be displayed", "검사대상 선별", email.Body);
			AssertContains("1", email.Body);
			AssertContains("2", email.Body);
			AssertContains("검사생략으로 선별", email.Body);
			AssertContains("전자서류(원본) 제출 대상입니다. 첨부서류는 원본을 변환한 이미지 파일을 전자제출 하여야 합니다. 문의사항은 통관지 세관 수입담당자에게 문의 바랍니다.", incomingMessage.EM_MessageInterpretation);
			AssertContains("The second html table is formatted to have at most four rows and all entry line numbers should be displayed", "검사대상 선별", incomingMessage.EM_MessageInterpretation);
		}

		public void TestStatusIsOPD()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR023_OPD.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to OPD", "OPD", entry.CH_EntryStatus);
			AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);
		}

		public void TestStatusIsSGN()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			entry.CH_CustomsMessageRemarks = "Test ADD, messageData.CustomsOfficeContent + CH_CustomsMessageRemarks";
			var incomingMessage = CreateMessageForTest("GOVCBR023_SGN.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to SGN", "SGN", entry.CH_EntryStatus);
			var entryLoaded = new BusinessObjectFactory().Load<CusEntryHeader>(entry.PK);
			AssertEquals("CusEntryHeader CH_CustomsMessafeRemarks is updated", "2020-08-21 13:43:31" + "\r\n" + "전자서류(원본) 제출 대상입니다. 첨부서류는 원본을 변환한 이미지 파일을 전자제출 하여야 합니다. 문의사항은 통관지 세관 수입담당자에게 문의 바랍니다.\r\nTest ADD, messageData.CustomsOfficeContent + CH_CustomsMessageRemarks", entryLoaded.CH_CustomsMessageRemarks);

			AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);
		}

		public void TestStatusIsRDY()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR023_RDY.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to RDY", "RDY", entry.CH_EntryStatus);

			AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);
		}

		public void TestStatusIsACL()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR023_ACL.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to ACL", "ACL", entry.CH_EntryStatus);
			AssertEquals("entry status is ACL when entry CH_EntryReleaseDate is updated", "202008211343", entry.CH_EntryReleaseDate.ToString("yyyyMMddHHmm"));

			AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);
		}

		public void TestStatusIsBAC()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR023_BAC.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to BAC", "BAC", entry.CH_EntryStatus);
			AssertEquals("entry status is BAC when entry CH_EntryReleaseDate is updated", "202008211343", entry.CH_EntryReleaseDate.ToString("yyyyMMddHHmm"));

			AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);
		}

		public void TestStatusIsICG()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR023_ICG.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to ICG", "ICG", entry.CH_EntryStatus);

			AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);
		}

		public void TestStatusIsCCG()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR023_CCG.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to CCG", "CCG", entry.CH_EntryStatus);

			AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);
		}

		public void TestStatusIsADT()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR023_ADT.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to ADT", "ADT", entry.CH_EntryStatus);

			AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);
		}

		public void TestStatusIsINS()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR023_INS.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to INS", "INS", entry.CH_EntryStatus);

			AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);
		}

		public void TestStatusIsNED()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR023_NED.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to NED", "NED", entry.CH_EntryStatus);

			AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);
		}

		public void TestStatusIsNDC()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR023_NDC.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to NDC", "NDC", entry.CH_EntryStatus);

			AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);
		}

		public void TestStatusIsCCL()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR023_CCL.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to CCL", "CCL", entry.CH_EntryStatus);
			AssertEquals("entry message status is updated correctly to CAB", CustomsMessageStatusTypeList.Codes.CancellationByCustoms, entry.CH_Status);

			AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);
		}

		public void TestStatusIsEDC()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR023_EDC.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to EDC", "EDC", entry.CH_EntryStatus);

			AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);
		}

		public void TestStatusIsSRN()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR023_SRN.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to SRN", "SRN", entry.CH_EntryStatus);

			AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);
		}

		public void TestImport023_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				CreateMessageForTest("GOVCBR023_CLR.xml");
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [수입 처리결과통보]1234520000045M 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		public void TestImport023_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBR023_CLR.xml");
				CreateEntryForImport(false);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수입 처리결과통보] Response for Declaration Number: B00001000 / 제출번호: 1234520000045M", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [수입신고서]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);
			}
		}

		public void TestImport023_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBR023_CLR.xml");
				CreateEntryForImport(true);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수입 처리결과통보] Response for Declaration Number: B00001000 / 제출번호: 1234520000045M", email.Subject);
				AssertEquals("CusAgent@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		public void TestCusStatementHeaderNotUpdate()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR023_EmptyNoticeNumber.xml");
			Factory.Save();

			ZQuery query = new ZQuery(CusStatementHeaderSchema.B2_GC, entry.Declaration.JE_GC);
			query.AddToFilter(CusStatementHeaderSchema.B2_StatementType, StatementHeaderTypeList.Codes.CustomsDisbursementBill);
			var statementCollection = Factory.Load<CusStatementHeader>(query);
			var beforeCount = statementCollection.Length;

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			statementCollection = Factory.Load<CusStatementHeader>(query);
			var afterCount = statementCollection.Length;

			AssertEquals(beforeCount, afterCount);

			incomingMessage = CreateMessageForTest("GOVCBR023_NotReleased.xml"); // NameCode Not In ('13', '14', '15')
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			statementCollection = Factory.Load<CusStatementHeader>(query);
			afterCount = statementCollection.Length;

			AssertEquals(beforeCount, afterCount);

			incomingMessage = CreateMessageForTest("GOVCBR023_ACL.xml");
			entry.Declaration.JE_DeclarationPlan = ImportCustomsClearancePlanCodeList.Codes.B;
			entry.Declaration.JE_PaymentMethod = PaymentMethodCodeList.Codes._11;
			var import929 = new ImportEntryHeaderCreator().Create(entry);
			import929.RoundDecimalValueRoundedWithDecimalPlaces();
			entry.Snapshots.RemoveAndDeleteAll();
			using (var stream = KRXmlObjectSerializer.Serialize(import929))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._929, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._929);
				Factory.Save();
			}

			statementCollection = Factory.Load<CusStatementHeader>(query);
			afterCount = statementCollection.Length;

			AssertEquals(beforeCount, afterCount);
		}

		public void TestCusStatementHeaderUpdate()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR023_ACL.xml");

			entry.Declaration.JE_DeclarationPlan = ImportCustomsClearancePlanCodeList.Codes.G;

			var statement = Factory.New<CusStatementHeader>();
			statement.B2_StatementNumber = "1235489520125487592";
			statement.B2_GC = entry.Declaration.JE_GC;
			statement.B2_StatementType = StatementHeaderTypeList.Codes.CustomsDisbursementBill;

			var import929 = new ImportEntryHeaderCreator().Create(entry);
			import929.RoundDecimalValueRoundedWithDecimalPlaces();
			entry.Snapshots.RemoveAndDeleteAll();
			using (var stream = KRXmlObjectSerializer.Serialize(import929))
			{
				AccumulativeAmendmentManager.CreateNewSnapshot(entry, ElectronicDocumentTypeList.Codes._929, stream);
				AccumulativeAmendmentManager.AcceptCurrentSnapshot(entry, ElectronicDocumentTypeList.Codes._929);
				Factory.Save();
			}

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			statement.Reload();

			AssertEquals(new ZDateTime(2020, 08, 21), statement.B2_PrintDate);
			AssertEquals(new ZDateTime(2020, 09, 05), statement.B2_DueDate);
		}

		CusEntryNumber LoadEntryNumber(CusEntryHeader entry, ZString typeCode)
		{
			var cusEntryNumber = entry.EntryNumbers.GetOrCreateCusEntryNum(typeCode);

			return cusEntryNumber;
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
			Factory.Save();
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
			outgoingMessage.EM_LinkedObject = importEntry;

			return importEntry;
		}

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBR023MessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._023;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			Factory.Save();

			return incomingMessage;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
	}
}
