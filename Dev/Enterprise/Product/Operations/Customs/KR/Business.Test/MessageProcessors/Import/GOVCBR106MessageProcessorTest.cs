using System;
using Enterprise.Customs.Business.AccumulativeAmendment;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR106MessageProcessorTest : XMLMessageTestHelper<GOVCBR106MessageProcessorTest>
	{
		public void Test_StatusIsANT_OutgoingMessageTypeIs105()
		{
			SetupAndAssertRequiredData("GOVCBR106_ANT.xml", ElectronicDocumentTypeList.Codes._5SC, ElectronicDocumentTypeList.Codes._105, CustomsEntryStatusTypeList.Codes.ANT, "승인");
			AssertEquals(1, importEntry.Snapshots.Count);
			AssertEquals(EntrySnapshotStatus.Lodged, importEntry.Snapshots[0].CES_Status);
		}

		public void Test_StatusIsDMS_OutgoingMessageTypeIs105()
		{
			SetupAndAssertRequiredData("GOVCBR106_DMS.xml", ElectronicDocumentTypeList.Codes._5SC, ElectronicDocumentTypeList.Codes._105, CustomsEntryStatusTypeList.Codes.DMS, "기각");
			AssertEquals(1, importEntry.Snapshots.Count);
			AssertEquals(EntrySnapshotStatus.Deleted, importEntry.Snapshots[0].CES_Status);
		}

		public void Test_StatusIsANT_OutgoingMessageTypeIsDHS()
		{
			SetupAndAssertRequiredData("GOVCBR106_ANT.xml", ElectronicDocumentTypeList.Codes._DHR, ElectronicDocumentTypeList.Codes._DHS, CustomsEntryStatusTypeList.Codes.ANT, "승인", Core.Constants.CountryCodes.China);
			AssertEquals(1, importEntry.Snapshots.Count);
			AssertEquals(EntrySnapshotStatus.Lodged, importEntry.Snapshots[0].CES_Status);
		}

		public void Test_StatusIsDMS_OutgoingMessageTypeIsDHS()
		{
			SetupAndAssertRequiredData("GOVCBR106_DMS.xml", ElectronicDocumentTypeList.Codes._DHR, ElectronicDocumentTypeList.Codes._DHS, CustomsEntryStatusTypeList.Codes.DMS, "기각", Core.Constants.CountryCodes.China);
			AssertEquals(1, importEntry.Snapshots.Count);
			AssertEquals(EntrySnapshotStatus.Deleted, importEntry.Snapshots[0].CES_Status);
		}

		void SetupAndAssertRequiredData(string xml, string snapshotType, string messageType, string messageOwner, string resultType, string countryOfOrigin = Core.Constants.CountryCodes.Australia)
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			CreateEntryWithOutgoingMessageForImport(snapshotType, messageType);

			var invoiceLine = importEntry.Declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CL = importEntry.MergedLines.AddNew().PK;
			invoiceLine.JI_CountryOfOrigin = countryOfOrigin;

			var incomingMessage = CreateMessageForTest(xml);
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			AssertEquals(1, importEntry.Snapshots.Count);
			AssertEquals(EntrySnapshotStatus.Lodged, importEntry.Snapshots[0].CES_Status);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			importEntry.Reload();
			incomingMessage.Reload();
			importEntry.Snapshots[0].Reload();

			AssertEquals("entry is located", importEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals(messageOwner, incomingMessage.EM_MessageOwner);

			AssertContains("협정정정 신고서", incomingMessage.EM_MessageInterpretation);
			AssertContains("12345-20-000045M", incomingMessage.EM_MessageInterpretation);
			AssertContains(resultType, incomingMessage.EM_MessageInterpretation);
			AssertContains("담당부서입니다.", incomingMessage.EM_MessageInterpretation);
			AssertContains("김담당", incomingMessage.EM_MessageInterpretation);
			AssertContains("0215428563", incomingMessage.EM_MessageInterpretation);
			AssertContains("2014-05-06", incomingMessage.EM_MessageInterpretation);
			AssertContains("2013-01-29", incomingMessage.EM_MessageInterpretation);
			AssertContains("처리내역입니다.", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("협정정정 신고서", email.Body);
			AssertContains("12345-20-000045M", email.Body);
			AssertContains(resultType, email.Body);
			AssertContains("담당부서입니다.", email.Body);
			AssertContains("김담당", email.Body);
			AssertContains("0215428563", email.Body);
			AssertContains("2014-05-06", email.Body);
			AssertContains("2013-01-29", email.Body);
			AssertContains("처리내역입니다.", email.Body);

			var outgoingMessage = importEntry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, messageType);
			AssertEquals(outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);
		}

		public void Test_Empty()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport("DHR", "DHS");
			var incomingMessage = CreateMessageForTest("GOVCBR106_Empty.xml");
			Factory.Save();

			AssertNoExceptionThrown("When Xml Element Values is Empty, system should still proceed successfully", () =>
			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch());
			incomingMessage.Reload();

			AssertContains("협정정정 신고서", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("담당부서입니다.", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("김담당", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("0215428563", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("처리내역입니다.", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertNotContains("담당부서입니다.", email.Body);
			AssertNotContains("김담당", email.Body);
			AssertNotContains("0215428563", email.Body);
			AssertNotContains("처리내역입니다.", email.Body);
		}

		public void TestImport106_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				CreateMessageForTest("GOVCBR106_ANT.xml");
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [협정관세적용신청 정정신청 결과통보]1234520000045M 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		public void TestImport106_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBR106_ANT.xml");
				CreateEntryForImport(false);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[협정관세적용신청 정정신청 결과통보] Response for Declaration Number: B00001000 / 제출번호: 1234520000045M", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [협정관세 정정신청서, 협정관세적용신청 정정신청서(자료교환용)]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);
			}
		}

		public void TestImport106_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBR106_ANT.xml");
				CreateEntryForImport(true);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[협정관세적용신청 정정신청 결과통보] Response for Declaration Number: B00001000 / 제출번호: 1234520000045M", email.Subject);
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
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNumber.CE_ParentID = importEntry.PK;
			entryNumber.CE_ParentTable = CusEntryHeader.Schema.TableName;
		}
		CusEntryHeader importEntry;

		CusEntryHeader CreateEntryWithOutgoingMessageForImport(string snapshotType, string messageType)
		{
			if (importEntry == null)
			{
				CreateEntryForImport(true);
			}
			var snapshot = importEntry.Snapshots.AddNew();
			snapshot.CES_MessageType = snapshotType;
			snapshot.CES_Status = EntrySnapshotStatus.Lodged;
			snapshot.CES_SystemCreateTimeUtc = DateTime.Now;

			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = messageType;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_LinkTable = CusEntryHeader.Schema.TableName;
			outgoingMessage.EM_LinkedObject = importEntry;

			return importEntry;
		}

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBR106MessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._106;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			return incomingMessage;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
	}
}
