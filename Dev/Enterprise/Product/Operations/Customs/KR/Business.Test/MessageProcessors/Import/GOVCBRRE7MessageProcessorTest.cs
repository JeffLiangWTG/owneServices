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
	sealed class GOVCBRRE7MessageProcessorTest : XMLMessageTestHelper<GOVCBRRE7MessageProcessorTest>
	{
		public void TestRE7()
		{
			var incomingMessage = CreateMessageForTest("GOVCBRRE7_0.xml");
			CreateEntryWithOutgoingMessageForImport();
			SampleCodeType();
			Factory.Save();

			var numFilter = new ZQuery(CusEntryNumSchema.CE_ParentID, importEntry.PK);
			numFilter.AddToFilter(CusEntryNumSchema.CE_EntryType, ElectronicDocumentTypeList.Codes._5UL);
			var cusEntryNumber = Factory.LoadTop1<CusEntryNumber>(numFilter);

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);

			Assert("PreCondition: Entry Status is Empty", cusEntryNumber.CE_EntryStatus.IsEmpty);
			Assert("PreCondition: Accepted Date is empty", cusEntryNumber.CE_IssueDate.IsEmpty);

			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);

			AssertEquals("entry is located", importEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to OAC", CustomsMessageStatusTypeList.Codes.OriginalAccepted, cusEntryNumber.CE_EntryStatus);
			AssertEquals("Accepted Date is updated correctly", new ZDateTime(2020, 09, 15, 16, 32, 56), cusEntryNumber.CE_IssueDate);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);

			var outgoingMessage = importEntry.Messages.LastOutgoingMessage;
			AssertEquals(outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("2020-09-15 16:32:56", email.Body);
			AssertContains("과오납 및 계약상이 환급신청서", email.Body);
			AssertContains("6N002-20-000076M", email.Body);
			AssertContains("2020-09-15 16:32:56", email.Body);
			AssertContains("[04064] 인천세관 조사(납세)심사과", email.Body);
			AssertContains("특이사항입니다.", email.Body);

			AssertContains("과오납 및 계약상이 환급신청서", incomingMessage.EM_MessageInterpretation);
			AssertContains("<td>통보일시</td><td>2020-09-15 16:32:56</td>", incomingMessage.EM_MessageInterpretation);
			AssertContains("6N002-20-000076M", incomingMessage.EM_MessageInterpretation);
			AssertContains("<td>신청문서 수신일시</td><td>2020-09-15 16:32:56</td>", incomingMessage.EM_MessageInterpretation);
			AssertContains("[04064] 인천세관 조사(납세)심사과", incomingMessage.EM_MessageInterpretation);
			AssertContains("특이사항입니다.", incomingMessage.EM_MessageInterpretation);
		}

		public void TestRE7_CusReconDeclaration()
		{
			var incomingMessage = CreateMessageForTest("GOVCBRRE7_0.xml");
			CreateRefundDeclarationWithOutgoingMessage();
			SampleCodeType();
			Factory.Save();

			var numFilter = new ZQuery(CusEntryNumSchema.CE_ParentID, refundDeclaration.PK);
			numFilter.AddToFilter(CusEntryNumSchema.CE_EntryType, ElectronicDocumentTypeList.Codes._5UL);
			var cusEntryNumber = Factory.LoadTop1<CusEntryNumber>(numFilter);

			Assert("PreCondition: No Declaration is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);

			Assert("PreCondition: Entry Status is Empty", cusEntryNumber.CE_EntryStatus.IsEmpty);
			Assert("PreCondition: Accepted Date is empty", cusEntryNumber.CE_IssueDate.IsEmpty);

			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);

			AssertEquals("Declaration is located", refundDeclaration.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals(CustomsMessageStatusTypeList.Codes.OriginalAccepted, refundDeclaration.CRD_MessageStatus);
			AssertEquals("entry status is updated correctly to OAC", CustomsMessageStatusTypeList.Codes.OriginalAccepted, cusEntryNumber.CE_EntryStatus);
			AssertEquals("Accepted Date is updated correctly", new ZDateTime(2020, 09, 15, 16, 32, 56), cusEntryNumber.CE_IssueDate);

			var outgoingMessage = refundDeclaration.Messages.LastOutgoingMessage;
			AssertEquals(outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);

			AssertContains("과오납 및 계약상이 환급신청서", incomingMessage.EM_MessageInterpretation);
			AssertContains("<td>통보일시</td><td>2020-09-15 16:32:56</td>", incomingMessage.EM_MessageInterpretation);
			AssertContains("6N002-20-000076M", incomingMessage.EM_MessageInterpretation);
			AssertContains("<td>신청문서 수신일시</td><td>2020-09-15 16:32:56</td>", incomingMessage.EM_MessageInterpretation);
			AssertContains("[04064] 인천세관 조사(납세)심사과", incomingMessage.EM_MessageInterpretation);
			AssertContains("특이사항입니다.", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("2020-09-15 16:32:56", email.Body);
			AssertContains("과오납 및 계약상이 환급신청서", email.Body);
		}

		public void TestEmptyData()
		{
			var incomingMessage = CreateMessageForTest("GOVCBRRE7_EmptyData.xml");
			CreateEntryWithOutgoingMessageForImport();
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);

			var numFilter = new ZQuery(CusEntryNumSchema.CE_ParentID, importEntry.PK);
			numFilter.AddToFilter(CusEntryNumSchema.CE_EntryType, ElectronicDocumentTypeList.Codes._5UL);
			var cusEntryNumber = Factory.LoadTop1<CusEntryNumber>(numFilter);
			Assert("PreCondition: Entry Status is Empty", cusEntryNumber.CE_EntryStatus.IsEmpty);

			AssertNoExceptionThrown(() => new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage));

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertNotContains("[]", email.Body);
			AssertNotContains("특이사항입니다.", email.Body);

			AssertContains("과오납 및 계약상이 환급신청서", incomingMessage.EM_MessageInterpretation);
			AssertContains("<td>통보일시</td><td>2020-09-15 16:32:56</td>", incomingMessage.EM_MessageInterpretation);
			AssertContains("6N002-20-000076M", incomingMessage.EM_MessageInterpretation);
			AssertContains("<td>신청문서 수신일시</td><td>2020-09-15 16:32:56</td>", incomingMessage.EM_MessageInterpretation);
			AssertContains("<td>접수 세관(과)</td><td>&nbsp;</td>", incomingMessage.EM_MessageInterpretation);
			AssertContains("<td>특이사항</td><td>&nbsp;</td>", incomingMessage.EM_MessageInterpretation);
		}

		public void TestWrongCusOffice()
		{
			var incomingMessage = CreateMessageForTest("GOVCBRRE7_WrongCusOffice.xml");
			CreateEntryWithOutgoingMessageForImport();
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);

			var numFilter = new ZQuery(CusEntryNumSchema.CE_ParentID, importEntry.PK);
			numFilter.AddToFilter(CusEntryNumSchema.CE_EntryType, ElectronicDocumentTypeList.Codes._5UL);
			var cusEntryNumber = Factory.LoadTop1<CusEntryNumber>(numFilter);
			Assert("PreCondition: Entry Status is Empty", cusEntryNumber.CE_EntryStatus.IsEmpty);

			new MessageProcessorFactory(new BatchProcessor.LoggingInformation()).ProcessMessage(incomingMessage);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("[040640]", email.Body);

			AssertContains("<td>접수 세관(과)</td><td>[040640] </td>", incomingMessage.EM_MessageInterpretation);
		}

		public void TestImportRE7_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ImportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				var incomingMessage = CreateMessageForTest("GOVCBRRE7_0.xml");
				SampleCodeType();
				Factory.Save();
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [징수 접수통보]6N00220000076M 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);

				AssertContains("과오납 및 계약상이 환급신청서", incomingMessage.EM_MessageInterpretation);
				AssertContains("<td>통보일시</td><td>2020-09-15 16:32:56</td>", incomingMessage.EM_MessageInterpretation);
				AssertContains("6N002-20-000076M", incomingMessage.EM_MessageInterpretation);
				AssertContains("<td>신청문서 수신일시</td><td>2020-09-15 16:32:56</td>", incomingMessage.EM_MessageInterpretation);
				AssertContains("[04064] 인천세관 조사(납세)심사과", incomingMessage.EM_MessageInterpretation);
				AssertContains("특이사항입니다.", incomingMessage.EM_MessageInterpretation);
			}
		}

		public void TestImportRE7_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.ImportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				var incomingMessage = CreateMessageForTest("GOVCBRRE7_0.xml");
				SampleCodeType();
				CreateEntryForImport(false);
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[징수 접수통보] Response for Declaration Number: B00001000 / 제출번호: 6N00220000076M", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [과오납 환급신청서]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);

				AssertContains("과오납 및 계약상이 환급신청서", incomingMessage.EM_MessageInterpretation);
				AssertContains("<td>통보일시</td><td>2020-09-15 16:32:56</td>", incomingMessage.EM_MessageInterpretation);
				AssertContains("6N002-20-000076M", incomingMessage.EM_MessageInterpretation);
				AssertContains("<td>신청문서 수신일시</td><td>2020-09-15 16:32:56</td>", incomingMessage.EM_MessageInterpretation);
				AssertContains("[04064] 인천세관 조사(납세)심사과", incomingMessage.EM_MessageInterpretation);
				AssertContains("특이사항입니다.", incomingMessage.EM_MessageInterpretation);
			}
		}

		public void TestImportRE7_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.ImportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				var incomingMessage = CreateMessageForTest("GOVCBRRE7_0.xml");
				SampleCodeType();
				CreateEntryForImport(true);
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[징수 접수통보] Response for Declaration Number: B00001000 / 제출번호: 6N00220000076M", email.Subject);
				AssertEquals("CusAgent@wisetechglobal.com", email.Recipients[0].Email);

				AssertContains("과오납 및 계약상이 환급신청서", incomingMessage.EM_MessageInterpretation);
				AssertContains("<td>통보일시</td><td>2020-09-15 16:32:56</td>", incomingMessage.EM_MessageInterpretation);
				AssertContains("6N002-20-000076M", incomingMessage.EM_MessageInterpretation);
				AssertContains("<td>신청문서 수신일시</td><td>2020-09-15 16:32:56</td>", incomingMessage.EM_MessageInterpretation);
				AssertContains("[04064] 인천세관 조사(납세)심사과", incomingMessage.EM_MessageInterpretation);
				AssertContains("특이사항입니다.", incomingMessage.EM_MessageInterpretation);
			}
		}

		public void TestUpdateRefundSessionalData_CSI_DateOfIssue()
		{
			CreateMessageForTest("GOVCBRRE7_0.xml");
			CreateEntryWithOutgoingMessageForImport();
			var instruction = importEntry.Declaration.CustomsEntryInstructions.AddNew();
			var oldRefundSessionalData = CreateRefundSessionalData("6N00220000075M", new ZDateTime(2020, 08, 15, 16, 32, 56));
			var refundSessionalData = CreateRefundSessionalData("6N00220000076M", ZDateTime.Empty);
			importEntry.CH_CEI_Instruction = instruction.PK;
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			oldRefundSessionalData.Reload();
			refundSessionalData.Reload();

			AssertEquals(new ZDateTime(2020, 08, 15, 16, 32, 00), oldRefundSessionalData.CSI_DateOfIssue);
			AssertEquals(new ZDateTime(2020, 09, 15, 16, 32, 00), refundSessionalData.CSI_DateOfIssue);

			RefundSessionalData CreateRefundSessionalData(string refundApplicationNumber, ZDateTime dateOfIssue)
			{
				var amendmentSessionalData = instruction.AmendmentSessionalDataCollection.AddNew();
				amendmentSessionalData.CSI_Code = DutyTaxCorrectionCodeList.Codes.C;

				var refundSessionalData = instruction.RefundSessionalDataCollection.AddNew();
				refundSessionalData.CSI_ReferenceNumber = refundApplicationNumber;
				refundSessionalData.CSI_DateOfIssue = dateOfIssue;
				refundSessionalData.CSI_CSI_SupportingInfo = amendmentSessionalData.PK;

				return refundSessionalData;
			}
		}

		public void TestHasNotRefundSessionalData()
		{
			var incomingMessage = CreateMessageForTest("GOVCBRRE7_0.xml");
			CreateEntryWithOutgoingMessageForImport();
			var instruction = importEntry.Declaration.CustomsEntryInstructions.AddNew();
			instruction.AmendmentSessionalDataCollection.AddNew();
			importEntry.CH_CEI_Instruction = instruction.PK;
			Factory.Save();

			var processor = new MessageProcessorProvider().GetProcessor(ElectronicDocumentTypeList.Codes._RE7);
			AssertNoExceptionThrown(() => processor.Process(incomingMessage));
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
			staff2.GS_EmailAddress = "ImportGroupTest@wisetechglobal.com";
			importGroup = Factory.New<GlbGroup>();
			var link1 = Factory.New<GlbGroupLink>();
			link1.GK_GG = importGroup.PK;
			link1.GK_GS = staff2.PK;

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
			entryNumber.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNumber.CE_ParentID = importEntry.PK;
			entryNumber.CE_ParentTable = CusEntryHeader.Schema.TableName;
			entryNumber.CE_EntryStatus = ZString.Empty;
			Factory.Save();
		}
		CusEntryHeader importEntry;

		void CreateEntryWithOutgoingMessageForImport()
		{
			if (importEntry == null)
			{
				CreateEntryForImport(true);
			}
			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5UL;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_LinkTable = CusEntryHeader.Schema.TableName;
			outgoingMessage.EM_LinkUniqueID = importEntry.PK;
			outgoingMessage.EM_LinkedObject = importEntry;
			outgoingMessage.EM_MessageOwner = "6N00220000076M";
		}

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBRRE7MessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._RE7;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			return incomingMessage;
		}

		void CreateRefundDeclaration(bool setCusAgent)
		{
			refundDeclaration = Factory.New<CusReconDeclaration>();
			if (setCusAgent)
			{
				refundDeclaration.CRD_GS_NKCustomsAgent = "AG";
			}
			refundDeclaration.CRD_ApplicationCode = "KRC";
			refundDeclaration.CRD_JobReferenceNumber = "5UL001";
			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_EntryNum = "6N00220000076M";
			entryNumber.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNumber.CE_ParentID = refundDeclaration.PK;
			entryNumber.CE_ParentTable = CusReconDeclaration.Schema.TableName;
			entryNumber.CE_EntryStatus = ZString.Empty;
		}
		CusReconDeclaration refundDeclaration;

		void CreateRefundDeclarationWithOutgoingMessage()
		{
			if (refundDeclaration == null)
			{
				CreateRefundDeclaration(true);
			}
			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5UL;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_LinkTable = CusReconDeclaration.Schema.TableName;
			outgoingMessage.EM_LinkUniqueID = refundDeclaration.PK;
			outgoingMessage.EM_LinkedObject = refundDeclaration;
		}

		void SampleCodeType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "040", "인천세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "64", "조사(납세)심사과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
	}
}
