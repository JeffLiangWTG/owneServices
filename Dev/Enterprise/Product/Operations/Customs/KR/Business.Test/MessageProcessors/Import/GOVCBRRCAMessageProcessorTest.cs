using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBRRCAMessageProcessorTest : XMLMessageTestHelper<GOVCBRRCAMessageProcessorTest>
	{
		void SetDataAndRunProcessor(string xml, string messageOwner, string noticeTypeAndDescription)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "020", "인천세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "75", "심사정보과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));

			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest(xml);

			var snapShot = importEntry.Snapshots.AddNew();
			snapShot.CES_MessageType = "5UL";
			snapShot.CES_Status = "LDG";
			Factory.Save();

			Assert(incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert(incomingMessage.EM_MessageOwner.IsEmpty);
			AssertEquals(1, importEntry.Snapshots.Count);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			importEntry.Reload();
			incomingMessage.Reload();
			snapShot.Reload();

			var outgoingMessage = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5UL);

			AssertEquals(importEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals(messageOwner, incomingMessage.EM_MessageOwner);
			AssertEquals(outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertContains("과오납 및 계약상이 환급신청서", incomingMessage.EM_MessageInterpretation);
			AssertContains("6N00220000076M", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-09-15 11:55:05", incomingMessage.EM_MessageInterpretation);
			AssertContains(noticeTypeAndDescription, incomingMessage.EM_MessageInterpretation);
			AssertContains("오유리", incomingMessage.EM_MessageInterpretation);
			AssertContains("[02075] 인천세관 심사정보과", incomingMessage.EM_MessageInterpretation);
			AssertContains("(6N00220000076M) 결재 승인되었습니다.", incomingMessage.EM_MessageInterpretation);
			AssertContains("특이사항1 특이사항2", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("과오납 및 계약상이 환급신청서", email.Body);
			AssertContains("6N00220000076M", email.Body);
			AssertContains("2020-09-15 11:55:05", email.Body);
			AssertContains(noticeTypeAndDescription, email.Body);
			AssertContains("오유리", email.Body);
			AssertContains("[02075] 인천세관 심사정보과", email.Body);
			AssertContains("(6N00220000076M) 결재 승인되었습니다.", email.Body);
			AssertContains("특이사항1 특이사항2", email.Body);
		}

		public void TestStatusIsANT()
		{
			SetDataAndRunProcessor("GOVCBRRCA_0.xml", CustomsEntryStatusTypeList.Codes.ANT, "[D] 승인통보");
			AssertEquals("LDG", importEntry.Snapshots[0].CES_Status);
		}

		public void TestStatusDMS()
		{
			SetDataAndRunProcessor("GOVCBRRCA_DMS.xml", CustomsEntryStatusTypeList.Codes.DMS, "[C] 기각통보");
			AssertEquals("DEL", importEntry.Snapshots[0].CES_Status);
		}

		public void TestStatusPNR()
		{
			SetDataAndRunProcessor("GOVCBRRCA_PNR.xml", CustomsEntryStatusTypeList.Codes.PNR, "[E] 지급정상");
			AssertEquals("LDG", importEntry.Snapshots[0].CES_Status);
		}

		public void TestStatusPFL()
		{
			SetDataAndRunProcessor("GOVCBRRCA_PFL.xml", CustomsEntryStatusTypeList.Codes.PFL, "[F] 지급오류");
			AssertEquals("LDG", importEntry.Snapshots[0].CES_Status);
		}

		public void TestWrongstructure()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBRRCA_WithoutData.xml");
			Factory.Save();

			Assert(incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert(incomingMessage.EM_MessageOwner.IsEmpty);

			AssertNoExceptionThrown(() => new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch());
			entry.Reload();
			incomingMessage.Reload();

			AssertEquals(entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals(CustomsEntryStatusTypeList.Codes.NDC, incomingMessage.EM_MessageOwner);
			AssertContains("[B] 서류제출통보", incomingMessage.EM_MessageInterpretation);
			AssertContains("[020750]", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("(6N00220000076M) 결재 승인되었습니다.", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("특이사항1 특이사항2", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("[B] 서류제출통보", email.Body);
			AssertContains("[020750]", email.Body);
			AssertNotContains("(6N00220000076M) 결재 승인되었습니다.", email.Body);
			AssertNotContains("특이사항1 특이사항2", email.Body);
		}

		public void TestImportRCA_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				CreateMessageForTest("GOVCBRRCA_0.xml");
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [환급신청서 처리결과통보]6N00220000076M 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		public void TestImportRCA_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBRRCA_0.xml");
				CreateEntryForImport(false);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[환급신청서 처리결과통보] Response for Declaration Number: B00001000 / 제출번호: 6N00220000076M", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [과오납 환급신청서]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);
			}
		}

		public void TestImportRCA_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBRRCA_0.xml");
				CreateEntryForImport(true);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[환급신청서 처리결과통보] Response for Declaration Number: B00001000 / 제출번호: 6N00220000076M", email.Subject);
				AssertEquals("CusAgent@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		public void TestUpdateRefundSessionalData_CSI_Status()
		{
			CreateEntryWithOutgoingMessageForImport();
			var instruction = importEntry.Declaration.CustomsEntryInstructions.AddNew();
			var oldRefundSessionalData = CreateRefundSessionalData("6N00220000075M", CustomsEntryStatusTypeList.Codes.PFL);
			var refundSessionalData = CreateRefundSessionalData("6N00220000076M", ZString.Empty);
			importEntry.CH_CEI_Instruction = instruction.PK;
			CreateMessageForTest("GOVCBRRCA_0.xml");

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			oldRefundSessionalData.Reload();
			refundSessionalData.Reload();

			AssertEquals(CustomsEntryStatusTypeList.Codes.PFL, oldRefundSessionalData.CSI_Status);
			AssertEquals(CustomsEntryStatusTypeList.Codes.ANT, refundSessionalData.CSI_Status);

			RefundSessionalData CreateRefundSessionalData(string refundApplicationNumber, string status)
			{
				var refundSessionalData = instruction.RefundSessionalDataCollection.AddNew();
				refundSessionalData.CSI_ReferenceNumber = refundApplicationNumber;
				refundSessionalData.CSI_Status = status;

				return refundSessionalData;
			}
		}

		public void TestHasNotRefundSessionalData()
		{
			CreateEntryWithOutgoingMessageForImport();
			var instruction = importEntry.Declaration.CustomsEntryInstructions.AddNew();
			instruction.AmendmentSessionalDataCollection.AddNew();
			importEntry.CH_CEI_Instruction = instruction.PK;
			var incomingMessage = CreateMessageForTest("GOVCBRRCA_0.xml");

			var processor = new MessageProcessorProvider().GetProcessor(ElectronicDocumentTypeList.Codes._RCA);
			AssertNoExceptionThrown(() => processor.Process(incomingMessage));
		}

		void SetRefundDeclarationDataAndRunProcessor(string xml, string messageOwner, string messageStatus)
		{
			var refundDeclaration = new TestDataSetupHelper(Factory).CreateRefundDeclarationWithOutgoingMessage("6N00220000076M");
			var incomingMessage = CreateMessageForTest(xml);
			Factory.Save();

			Assert(incomingMessage.EM_LinkUniqueID.IsEmpty);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			refundDeclaration.Reload();
			incomingMessage.Reload();

			AssertEquals(refundDeclaration.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals(messageOwner, incomingMessage.EM_MessageOwner);
			AssertEquals(messageStatus, refundDeclaration.CRD_MessageStatus);
			AssertEquals(messageOwner, refundDeclaration.CRD_CustomsStatus);

			var outgoingMessage = (EDIMessage)refundDeclaration.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5UL);
			AssertEquals("Message ApplicationReference is updated", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("과오납 및 계약상이 환급신청서", email.Body);
			AssertContains("6N00220000076M", email.Body);
		}

		public void TestRefundDeclarationStatusIsANT()
		{
			SetRefundDeclarationDataAndRunProcessor("GOVCBRRCA_0.xml", CustomsEntryStatusTypeList.Codes.ANT, "");
		}

		public void TestRefundDeclarationStatusDMS()
		{
			SetRefundDeclarationDataAndRunProcessor("GOVCBRRCA_DMS.xml", CustomsEntryStatusTypeList.Codes.DMS, CustomsMessageStatusTypeList.Codes.CancellationByCustoms);
		}

		public void TestRefundDeclarationStatusPNR()
		{
			SetRefundDeclarationDataAndRunProcessor("GOVCBRRCA_PNR.xml", CustomsEntryStatusTypeList.Codes.PNR, "");
		}

		public void TestRefundDeclarationStatusPFL()
		{
			SetRefundDeclarationDataAndRunProcessor("GOVCBRRCA_PFL.xml", CustomsEntryStatusTypeList.Codes.PFL, "");
		}

		public void TestRefundDeclarationStatusNDC()
		{
			SetRefundDeclarationDataAndRunProcessor("GOVCBRRCA_WithoutData.xml", CustomsEntryStatusTypeList.Codes.NDC, "");
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
			entryNumber.CE_EntryType = ElectronicDocumentTypeList.Codes._5UL;
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
			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5UL;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_LinkTable = CusEntryHeader.Schema.TableName;
			outgoingMessage.EM_LinkedObject = importEntry;
			outgoingMessage.EM_MessageOwner = "6N00220000076M";

			return importEntry;
		}

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBRRCAMessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._RCA;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			Factory.Save();
			return incomingMessage;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
	}
}
