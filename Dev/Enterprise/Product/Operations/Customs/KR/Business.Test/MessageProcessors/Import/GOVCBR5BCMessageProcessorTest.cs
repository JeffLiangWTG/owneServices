using System;
using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5BCMessageProcessorTest : XMLMessageTestHelper<GOVCBR5BCMessageProcessorTest>
	{
		EDIMessage SetupAndAssertRequiredData(string xml, string statusType)
		{
			importEntry = CreateEntryWithOutgoingMessageForImport();
			var entryNum5BA = importEntry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5BA);
			var incomingMessage = CreateMessageForTest(xml);
			SampleCodeType();
			Factory.Save();
			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			importEntry.Reload();
			incomingMessage.Reload();
			entryNum5BA.Reload();

			AssertEquals("entry is located", importEntry.PK, incomingMessage.EM_LinkUniqueID);

			AssertEquals("message owner is updated", statusType, incomingMessage.EM_MessageOwner);
			AssertNullOrEmpty("entry status is not updated", entryNum5BA.CE_EntryStatus);
			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertContains("합의세율 정정신청서", incomingMessage.EM_MessageInterpretation);
			AssertContains("12345-20-000045M", incomingMessage.EM_MessageInterpretation);
			AssertContains("2021-05-06", incomingMessage.EM_MessageInterpretation);
			AssertContains("[01020] 서울세관 내륙기지통관과", incomingMessage.EM_MessageInterpretation);
			AssertContains("<td>처리담당자명</td><td>담당자</td>", incomingMessage.EM_MessageInterpretation);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("합의세율 정정신청서", email.Body);
			AssertContains("2021-05-06", email.Body);
			AssertContains("[01020] 서울세관 내륙기지통관과", email.Body);
			AssertContains("<td>처리담당자명</td><td>담당자</td>", email.Body);

			return incomingMessage;
		}

		public void TestEmpty()
		{
			var incomingMessage = SetupAndAssertRequiredData("GOVCBR5BC_Empty.xml", CustomsEntryStatusTypeList.Codes.ANT);
			AssertContains("<td>정정기각사유</td><td>&nbsp;</td>", incomingMessage.EM_MessageInterpretation);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("<td>정정기각사유</td><td>&nbsp;</td>", email.Body);
		}

		public void TestResultTypeIsC()
		{
			var incomingMessage = SetupAndAssertRequiredData("GOVCBR5BC_ResultTypeIsC.xml", CustomsEntryStatusTypeList.Codes.ANT);
			AssertContains("승인", incomingMessage.EM_MessageInterpretation);

			var outgoingMessage = importEntry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5BB);
			AssertEquals("message ApplicationReference is updated correctly to EM_MessageNum", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals("message subType is DeclarationType", ElectronicDocumentTypeList.Codes._5BB, incomingMessage.EM_MessageSubType);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("승인", email.Body);
		}

		public void TestResultTypeIsE()
		{
			var incomingMessage = SetupAndAssertRequiredData("GOVCBR5BC_ResultTypeIsE.xml", CustomsEntryStatusTypeList.Codes.DMS);
			AssertContains("기각", incomingMessage.EM_MessageInterpretation);

			var snapshot = importEntry.Snapshots.GetLatestSnapshotIn(ElectronicDocumentTypeList.Codes._5BA);
			snapshot.Reload();
			AssertEquals("snapshot is updated 'DEL'", EntrySnapshotStatus.Deleted, snapshot.CES_Status);

			var outgoingMessage = importEntry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5BB);
			AssertEquals("message ApplicationReference is updated correctly to EM_MessageNum", outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
			AssertEquals("message subType is DeclarationType", ElectronicDocumentTypeList.Codes._5BB, incomingMessage.EM_MessageSubType);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("기각", email.Body);
		}

		public void TestAmendTypeIsU()
		{
			var incomingMessage = SetupAndAssertRequiredData("GOVCBR5BC_AmendTypeIsU.xml", CustomsEntryStatusTypeList.Codes.DMS);
			AssertContains("합의세율 항목 정정", incomingMessage.EM_MessageInterpretation);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("합의세율 항목 정정", email.Body);
		}

		public void TestAmendTypeIsC()
		{
			var incomingMessage = SetupAndAssertRequiredData("GOVCBR5BC_AmendTypeIsC.xml", CustomsEntryStatusTypeList.Codes.DMS);
			AssertContains("합의세율 내역 삭제", incomingMessage.EM_MessageInterpretation);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("합의세율 내역 삭제", email.Body);
		}

		public void TestAmendTypeIsA()
		{
			var incomingMessage = SetupAndAssertRequiredData("GOVCBR5BC_AmendTypeIsA.xml", CustomsEntryStatusTypeList.Codes.DMS);
			AssertContains("합의세율 란 추가", incomingMessage.EM_MessageInterpretation);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("합의세율 란 추가", email.Body);
		}

		public void TestAmendTypeIsD()
		{
			var incomingMessage = SetupAndAssertRequiredData("GOVCBR5BC_AmendTypeIsD.xml", CustomsEntryStatusTypeList.Codes.DMS);
			AssertContains("합의세율 란 삭제", incomingMessage.EM_MessageInterpretation);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertContains("합의세율 란 삭제", email.Body);
		}

		public void TestImport5BC_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ImportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				CreateMessageForTest("GOVCBR5BC.xml");
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [합의세율 정정신청서 처리결과 통보서]1234520000045M 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		public void TestImport5BC_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.ImportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBR5BC.xml");
				CreateEntryForImport(false);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[합의세율 정정신청서 처리결과 통보서] Response for Declaration Number: B00001000 / 제출번호: 1234520000045M", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [합의세율 정정신청서]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);
			}
		}

		public void TestImport5BC_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.ImportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBR5BC.xml");
				CreateEntryForImport(true);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[합의세율 정정신청서 처리결과 통보서] Response for Declaration Number: B00001000 / 제출번호: 1234520000045M", email.Subject);
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
			entryNumber.CE_EntryNum = "1234520000045M";
			entryNumber.CE_EntryType = "IMP";

			var snapshot = importEntry.Snapshots.AddNew();
			snapshot.CES_MessageType = ElectronicDocumentTypeList.Codes._5BA;
			snapshot.CES_Status = EntrySnapshotStatus.Lodged;
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
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5BB;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_LinkTable = CusEntryHeader.Schema.TableName;
			outgoingMessage.EM_LinkedObject = importEntry;

			return importEntry;
		}

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBR5BCMessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5BC;
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

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
	}
}
