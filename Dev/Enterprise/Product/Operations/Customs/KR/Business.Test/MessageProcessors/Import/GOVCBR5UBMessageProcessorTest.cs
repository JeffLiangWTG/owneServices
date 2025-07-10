using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5UBMessageProcessorTest : XMLMessageTestHelper<GOVCBR5UBMessageProcessorTest>
	{
		public void Test5UB()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			CreateEntryForImport(true);
			var oldOutgoingMessage = importEntry.Messages.AddNew();
			oldOutgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5UA;
			oldOutgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			oldOutgoingMessage.EM_SystemCreateUser = "ORG";
			oldOutgoingMessage.EM_ApplicationReference = "1";

			var outgoingMessage = Create5UAMessageWithEntry();
			outgoingMessage.EM_ApplicationReference = "2";

			var outgoingMessage5FE = importEntry.Messages.AddNew();
			outgoingMessage5FE.EM_MessageType = ElectronicDocumentTypeList.Codes._5FE;
			outgoingMessage5FE.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage5FE.EM_SystemCreateUser = "ORG";
			outgoingMessage5FE.EM_ApplicationReference = "2";
			outgoingMessage5FE.EM_MessageOwner = "";

			var instruction = importEntry.Declaration.CustomsEntryInstructions.AddNew();
			importEntry.CH_CEI_Instruction = instruction.PK;
			var amendmentSessionalData = importEntry.EntryInstruction.AmendmentSessionalDataCollection.AddNew();
			amendmentSessionalData.CSI_Code = DutyTaxCorrectionCodeList.Codes.A;
			var penaltyExemptionSessionalData1 = amendmentSessionalData.PenaltyExemptionSessionalData;
			penaltyExemptionSessionalData1.CSI_LineNo = 1;
			penaltyExemptionSessionalData1.CSI_Code = DutyPenaltyExemptionCodeList.Codes.Y;
			amendmentSessionalData = importEntry.EntryInstruction.AmendmentSessionalDataCollection.AddNew();
			amendmentSessionalData.CSI_Code = DutyTaxCorrectionCodeList.Codes.A;
			var penaltyExemptionSessionalData2 = amendmentSessionalData.PenaltyExemptionSessionalData;
			penaltyExemptionSessionalData2.CSI_LineNo = 2;
			penaltyExemptionSessionalData2.CSI_Code = DutyPenaltyExemptionCodeList.Codes.Y;

			var incomingMessage = CreateMessageForTest("GOVCBR5UB_0.xml");
			SampleCodeType();
			Factory.Save();

			AssertEquals("1", oldOutgoingMessage.EM_MessageNum);
			AssertEquals("2", outgoingMessage.EM_MessageNum);

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: ApplicationReference is Empty", incomingMessage.EM_ApplicationReference.IsEmpty);
			Assert("PreCondition: MessageSubType is Empty", incomingMessage.EM_MessageSubType.IsEmpty);
			Assert("PreCondition: MessageOwner is Empty", incomingMessage.EM_MessageOwner.IsEmpty);
			Assert("PreCondition: CSI_Status is Empty", penaltyExemptionSessionalData1.CSI_Status.IsEmpty);
			Assert("PreCondition: CSI_Status is Empty", penaltyExemptionSessionalData2.CSI_Status.IsEmpty);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			importEntry.Reload();
			incomingMessage.Reload();
			penaltyExemptionSessionalData1.Reload();
			penaltyExemptionSessionalData2.Reload();

			AssertEquals("entry is located", importEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("5UB Message.EM_ApplicationReference is update correctly to 5UA Message.EM_MessageNum", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals("5UB Message.EM_MessageSubType is update correctly to 5UA", ElectronicDocumentTypeList.Codes._5UA, incomingMessage.EM_MessageSubType);
			AssertEquals("5UB Message.EM_MessageOwner is update correctly to ANT", CustomsEntryStatusTypeList.Codes.ANT, incomingMessage.EM_MessageOwner);
			Assert("PreCondition: CSI_Status is Empty", penaltyExemptionSessionalData1.CSI_Status.IsEmpty);
			AssertEquals("5UB Message.CSI_Status is update correctly to ANT", CustomsEntryStatusTypeList.Codes.ANT, penaltyExemptionSessionalData2.CSI_Status);

			AssertContains("가산세(보정이자) 감면 신청서", incomingMessage.EM_MessageInterpretation);
			AssertContains("6N002-20-000084M", incomingMessage.EM_MessageInterpretation);
			AssertContains("이순애", incomingMessage.EM_MessageInterpretation);
			AssertContains("[02011] 인천세관 수입(1)과", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-06-01", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-06-01 17:03:26", incomingMessage.EM_MessageInterpretation);
			AssertContains("B 가산세", incomingMessage.EM_MessageInterpretation);
			AssertContains("C 승인", incomingMessage.EM_MessageInterpretation);
			AssertContains("광주세관 납세심사과-1914 (가산세 면제 통지)", incomingMessage.EM_MessageInterpretation);
			AssertContains("0000-000-00-00-0-000000-0", incomingMessage.EM_MessageInterpretation);
			AssertContains("1", incomingMessage.EM_MessageInterpretation);
			AssertContains("0", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("가산세(보정이자) 감면 신청서", email.Body);
			AssertContains("6N002-20-000084M", email.Body);
			AssertContains("이순애", email.Body);
			AssertContains("[02011] 인천세관 수입(1)과", email.Body);
			AssertContains("2020-06-01", email.Body);
			AssertContains("2020-06-01 17:03:26", email.Body);
			AssertContains("B 가산세", email.Body);
			AssertContains("C 승인", email.Body);
			AssertContains("광주세관 납세심사과-1914 (가산세 면제 통지)", email.Body);
			AssertContains("0000-000-00-00-0-000000-0", email.Body);
			AssertContains("1", email.Body);
			AssertContains("0", email.Body);
		}

		public void TestWhetherLastMessageIs5FEor5UA()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			CreateEntryForImport(true);
			var outgoingMessage5FE = importEntry.Messages.AddNew();
			outgoingMessage5FE.EM_MessageType = ElectronicDocumentTypeList.Codes._5FE;
			outgoingMessage5FE.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage5FE.EM_SystemCreateUser = "ORG";
			outgoingMessage5FE.EM_ApplicationReference = "2";
			outgoingMessage5FE.EM_MessageOwner = "2";

			var outgoingMessage5UA = importEntry.Messages.AddNew();
			outgoingMessage5UA.EM_MessageType = ElectronicDocumentTypeList.Codes._5UA;
			outgoingMessage5UA.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage5UA.EM_SystemCreateUser = "ORG";
			outgoingMessage5UA.EM_ApplicationReference = "2";

			var instruction = importEntry.Declaration.CustomsEntryInstructions.AddNew();
			importEntry.CH_CEI_Instruction = instruction.PK;
			var amendmentSessionalData = importEntry.EntryInstruction.AmendmentSessionalDataCollection.AddNew();
			amendmentSessionalData.CSI_LineNo = 3;
			amendmentSessionalData.CSI_Code = DutyTaxCorrectionCodeList.Codes.A;
			var penaltyExemptionSessionalData = amendmentSessionalData.PenaltyExemptionSessionalData;
			penaltyExemptionSessionalData.CSI_LineNo = 2;
			penaltyExemptionSessionalData.CSI_Code = DutyPenaltyExemptionCodeList.Codes.Y;

			var incomingMessage = CreateMessageForTest("GOVCBR5UB_0.xml");
			SampleCodeType();
			Factory.Save();

			amendmentSessionalData = importEntry.EntryInstruction.AmendmentSessionalDataCollection.Cast<AmendmentSessionalData>().SingleOrDefault(x => x.CSI_LineNo == 3);
			var exemptionRequestVersionNo = amendmentSessionalData.PenaltyExemptionSessionalData;

			Assert("PreCondition: ApplicationReference is Empty", incomingMessage.EM_ApplicationReference.IsEmpty);
			Assert("PreCondition: MessageSubType is Empty", incomingMessage.EM_MessageSubType.IsEmpty);
			Assert("PreCondition: CSI_Status is Empty", exemptionRequestVersionNo.CSI_Status.IsEmpty);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			importEntry.Reload();
			incomingMessage.Reload();
			amendmentSessionalData.Reload();
			exemptionRequestVersionNo.Reload();

			AssertEquals("entry is located", importEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("5UB Message.EM_ApplicationReference is update correctly to 5FE Message.EM_MessageNum", outgoingMessage5FE.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals("5UB Message.EM_MessageSubType is update correctly to 5FE", ElectronicDocumentTypeList.Codes._5FE, incomingMessage.EM_MessageSubType);
			AssertEquals("5UB Message.CSI_Status is update correctly to ANT", CustomsEntryStatusTypeList.Codes.ANT, exemptionRequestVersionNo.CSI_Status);

			outgoingMessage5FE.EM_MessageOwner = "";
			var incomingMessagefrom5UA = CreateMessageForTest("GOVCBR5UB_0.xml");
			exemptionRequestVersionNo.CSI_Status = ZString.Empty;
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			importEntry.Reload();
			incomingMessagefrom5UA.Reload();
			amendmentSessionalData.Reload();
			exemptionRequestVersionNo.Reload();

			AssertEquals("5UB Message.EM_ApplicationReference is update correctly to 5UA Message.EM_MessageNum", outgoingMessage5UA.EM_MessageNum, incomingMessagefrom5UA.EM_ApplicationReference);
			AssertEquals("5UB Message.EM_MessageSubType is update correctly to 5UA", ElectronicDocumentTypeList.Codes._5UA, incomingMessagefrom5UA.EM_MessageSubType);
			AssertEquals("5UB Message.CSI_Status is update correctly to ANT", CustomsEntryStatusTypeList.Codes.ANT, exemptionRequestVersionNo.CSI_Status);
		}

		public void TestWrongStructure()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			Create5UAMessageWithEntry();
			var incomingMessage = CreateMessageForTest("GOVCBR5UB_WrongDeclarationOfficeID.xml");
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);

			AssertNoExceptionThrown(() => new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch());
			incomingMessage.Reload();

			AssertEquals("entry is located", importEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertContains("[020110]", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("[020110]", email.Body);
		}

		public void TestEmptyReferenceID()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			Create5UAMessageWithEntry();
			var incomingMessage = CreateMessageForTest("GOVCBR5UB_WithoutDutyTaxFee.xml");
			SampleCodeType();
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);

			AssertNoExceptionThrown(() => new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch());
			incomingMessage.Reload();

			AssertContains("incomingMessage EM_MessageInterpretation is updated", "가산세(보정이자) 감면 신청서", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("0000-000-00-00-0-000000-0", incomingMessage.EM_MessageInterpretation);
			AssertContains("10", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("10.5", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertNotContains("0000-000-00-00-0-000000-0", email.Body);
			AssertContains("10", email.Body);
			AssertNotContains("10.5", email.Body);
		}

		public void TestImport5UB_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				CreateMessageForTest("GOVCBR5UB_0.xml");
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [가산세(보정이자) 감면 승인통보]6N00220000084M 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		public void TestImport5UB_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBR5UB_0.xml");
				CreateEntryForImport(false);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[가산세(보정이자) 감면 승인통보] Response for Declaration Number: B00001000 / 제출번호: 6N00220000084M", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [가산세(보정이자)면제 신청서]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);
			}
		}

		public void TestImport5UB_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBR5UB_0.xml");
				CreateEntryForImport(true);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[가산세(보정이자) 감면 승인통보] Response for Declaration Number: B00001000 / 제출번호: 6N00220000084M", email.Subject);
				AssertEquals("CusAgent@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		public void Test5UBisDeclined()
		{
			CreateEntryForImport(true);
			SampleCodeType();
			var payer = TestOrgDataSetUpHelper.CreateOrgHeader(Factory, "BUS", "READYKOREA", "한국아이비엠(주)");
			TestOrgDataSetUpHelper.AddOrgContact(payer, "송기홍", true);
			TestOrgDataSetUpHelper.AddOrgAddress(payer.MainAddress, "서울특별시 영등포구 국제금융로 10", "(여의도동, 서울 국제금융 센터)");
			var payerCodes = new IDNumberAndType[]
			{
				new IDNumberAndType() { Type = IdentificationType.BusinessRegNo, Number = "1168103897", CountryOfIssue = Core.Constants.CountryCodes.KoreaSouth },
			};
			TestOrgDataSetUpHelper.AddCustomsCode(payer, payerCodes);
			importEntry.Declaration.JE_OH_DutyPayer = payer.PK;
			importEntry.Declaration.JE_CustomsOffice = "020";

			var outgoingMessage5FE = importEntry.Messages.AddNew();
			outgoingMessage5FE.EM_MessageType = ElectronicDocumentTypeList.Codes._5FE;
			outgoingMessage5FE.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage5FE.EM_SystemCreateUser = "ORG";
			outgoingMessage5FE.EM_ApplicationReference = "2";
			outgoingMessage5FE.EM_MessageOwner = "2";

			var outgoingMessage5UA = importEntry.Messages.AddNew();
			outgoingMessage5UA.EM_MessageType = ElectronicDocumentTypeList.Codes._5UA;
			outgoingMessage5UA.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage5UA.EM_SystemCreateUser = "ORG";
			outgoingMessage5UA.EM_ApplicationReference = "2";

			var incomingMessage = CreateMessageForTest("GOVCBR5UB_DMS.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			importEntry.Reload();
			incomingMessage.Reload();
			AssertEquals(1, importEntry.CustomsDisbursementBills.Count);
			var statementHeader = importEntry.CustomsDisbursementBills[0];

			AssertEquals("0127030012000018260", statementHeader.B2_StatementNumber);
			AssertEquals(StatementHeaderStatusList.Codes.U, statementHeader.B2_Status);
			AssertEquals(StatementHeaderTypeList.Codes.CustomsDisbursementBill, statementHeader.B2_StatementType);
			AssertEquals(StatementHeaderPaymentStatusList.Codes.PYI, statementHeader.B2_PaymentStatus);
			AssertEquals(new ZDateTime(2020, 06, 01, 17, 03, 00), statementHeader.B2_ProcessDate);
			AssertEquals(new ZDateTime(2020, 06, 02), statementHeader.B2_PrintDate);
			AssertEquals(new ZDateTime(2020, 06, 03), statementHeader.B2_DueDate);
			AssertEquals("020", statementHeader.B2_ProcessPort);
			AssertEquals(incomingMessage.Branch.Company.PK, statementHeader.B2_GC);
			AssertEquals(payer.PK, statementHeader.B2_OH_Importer);
			AssertEquals(incomingMessage.EM_MessageNum, statementHeader.B2_CheckNo);
			AssertEquals(1000000m, statementHeader.B2_StatementAmount);

			AssertEquals(1, statementHeader.StatementLines.Count);
			var statementLine = statementHeader.FirstLine;
			AssertEquals("6N00220000084M", statementLine.B3_EntryNum);
			AssertEquals(KRJobMessageTypeList.Codes.Import, statementLine.B3_EntryType);
			AssertEquals(1u, statementLine.B3_SequenceNumber);
			AssertEquals(1000000m, statementLine.B3_CustomsFeesTotal);

			AssertEquals(1, statementLine.Charges.Count);
			var lineCharge = statementLine.Charges[0];
			AssertEquals(ChargeTypeList.Codes.PenaltyAndInterest, lineCharge.B4_ChargeType);
			AssertEquals(1000000m, lineCharge.B4_ChargeAmount);

			AssertEquals(1, statementHeader.Messages.Count);
			var clonedMessage = statementHeader.Messages[0];
			AssertEquals(ElectronicDocumentTypeList.Codes._5UB, clonedMessage.EM_MessageType);
			AssertEquals(incomingMessage.EM_MessageNum, clonedMessage.EM_MessageNum);

			AssertContains("가산세(보정이자) 감면 신청서", incomingMessage.EM_MessageInterpretation);
			AssertContains("6N002-20-000084M", incomingMessage.EM_MessageInterpretation);
			AssertContains("이순애", incomingMessage.EM_MessageInterpretation);
			AssertContains("[02011] 인천세관 수입(1)과", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-06-02", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-06-01 17:03:26", incomingMessage.EM_MessageInterpretation);
			AssertContains("B 가산세", incomingMessage.EM_MessageInterpretation);
			AssertContains("E 불승인", incomingMessage.EM_MessageInterpretation);
			AssertContains("광주세관 납세심사과-1914 (가산세 면제 통지)", incomingMessage.EM_MessageInterpretation);
			AssertContains("0127-030-01-20-0-001826-0", incomingMessage.EM_MessageInterpretation);
			AssertContains("2", incomingMessage.EM_MessageInterpretation);
			AssertContains("1000000", incomingMessage.EM_MessageInterpretation);

			AssertContains("가산세(보정이자) 감면 신청서", clonedMessage.EM_MessageInterpretation);
			AssertContains("6N002-20-000084M", clonedMessage.EM_MessageInterpretation);
			AssertContains("이순애", clonedMessage.EM_MessageInterpretation);
			AssertContains("[02011] 인천세관 수입(1)과", clonedMessage.EM_MessageInterpretation);
			AssertContains("2020-06-02", clonedMessage.EM_MessageInterpretation);
			AssertContains("2020-06-01 17:03:26", clonedMessage.EM_MessageInterpretation);
			AssertContains("B 가산세", clonedMessage.EM_MessageInterpretation);
			AssertContains("E 불승인", clonedMessage.EM_MessageInterpretation);
			AssertContains("광주세관 납세심사과-1914 (가산세 면제 통지)", clonedMessage.EM_MessageInterpretation);
			AssertContains("0127-030-01-20-0-001826-0", clonedMessage.EM_MessageInterpretation);
			AssertContains("2", clonedMessage.EM_MessageInterpretation);
			AssertContains("1000000", clonedMessage.EM_MessageInterpretation);

			incomingMessage = CreateMessageForTest("GOVCBR5UB_DMS.xml");
			Factory.Save();

			var existingStatementHeaderPK = statementHeader.PK;
			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			importEntry.Reload();
			incomingMessage.Reload();
			AssertEquals("If the statement header exists, new statement header won't be created.", 1, importEntry.CustomsDisbursementBills.Count);

			statementHeader = new BusinessObjectFactory().Load<CusStatementHeader>(existingStatementHeaderPK);
			AssertEquals(1, statementHeader.StatementLines.Count);
			AssertEquals(1, statementHeader.StatementLines[0].Charges.Count);
		}

		void SampleCodeType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "020", "인천세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "11", "수입(1)과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
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
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
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

		EDIMessage Create5UAMessageWithEntry()
		{
			if (importEntry == null)
			{
				CreateEntryForImport(true);
			}
			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5UA;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_LinkTable = CusEntryHeader.Schema.TableName;
			outgoingMessage.EM_LinkedObject = importEntry;

			return outgoingMessage;
		}

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBR5UBMessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5UB;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			return incomingMessage;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
	}
}
