using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBRRCJMessageProcessorTest : XMLMessageTestHelper<GOVCBRRCJMessageProcessorTest>
	{
		public void TestRCJ()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBRRCJ_0.xml");

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "Customs Department");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "030", "부산세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "11", "수입(1)과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", entry.CH_EntryStatus.IsEmpty);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to SUP", CustomsEntryStatusTypeList.Codes.SUP, entry.CH_EntryStatus);

			AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);
			AssertContains("2019-12-24 17:37:47", incomingMessage.EM_MessageInterpretation);
			AssertContains("030-81-보류-19-00097", incomingMessage.EM_MessageInterpretation);
			AssertContains("12345-20-000045M", incomingMessage.EM_MessageInterpretation);
			AssertContains("41634-19-508005M", incomingMessage.EM_MessageInterpretation);
			AssertContains("[3] 일부통관보류", incomingMessage.EM_MessageInterpretation);
			AssertContains("성분미달로 인한 요건 미비", incomingMessage.EM_MessageInterpretation);
			AssertContains("2019-12-24", incomingMessage.EM_MessageInterpretation);
			AssertContains("2019-12-24", incomingMessage.EM_MessageInterpretation);
			AssertContains("[02] 세관장확인대상물품 요건확인 미비", incomingMessage.EM_MessageInterpretation);
			AssertContains("[99] 기타", incomingMessage.EM_MessageInterpretation);
			AssertContains("관세법인 에이원 부산총괄본부/이성욱이흥대", incomingMessage.EM_MessageInterpretation);
			AssertContains("한국마즈(유)", incomingMessage.EM_MessageInterpretation);
			AssertContains("정선우", incomingMessage.EM_MessageInterpretation);
			AssertContains("[03011] 부산세관 수입(1)과", incomingMessage.EM_MessageInterpretation);
			AssertContains("유나리", incomingMessage.EM_MessageInterpretation);
			AssertContains("01012345678", incomingMessage.EM_MessageInterpretation);
			AssertContains("04299999999", incomingMessage.EM_MessageInterpretation);
			AssertContains("<td>1</td>", incomingMessage.EM_MessageInterpretation);
			AssertContains("2309101000", incomingMessage.EM_MessageInterpretation);
			AssertContains("DRY PETFOOD", incomingMessage.EM_MessageInterpretation);
			AssertContains("GREENIES", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("2019-12-24 17:37:47", email.Body);
			AssertContains("030-81-보류-19-00097", email.Body);
			AssertContains("12345-20-000045M", email.Body);
			AssertContains("41634-19-508005M", email.Body);
			AssertContains("[3] 일부통관보류", email.Body);
			AssertContains("성분미달로 인한 요건 미비", email.Body);
			AssertContains("2019-12-24", email.Body);
			AssertContains("2019-12-24", email.Body);
			AssertContains("[02] 세관장확인대상물품 요건확인 미비", email.Body);
			AssertContains("[99] 기타", email.Body);
			AssertContains("관세법인 에이원 부산총괄본부/이성욱이흥대", email.Body);
			AssertContains("한국마즈(유)", email.Body);
			AssertContains("정선우", email.Body);
			AssertContains("[03011] 부산세관 수입(1)과", email.Body);
			AssertContains("유나리", email.Body);
			AssertContains("01012345678", email.Body);
			AssertContains("04299999999", email.Body);
			AssertContains("001", email.Body);
			AssertContains("2309101000", email.Body);
			AssertContains("DRY PETFOOD", email.Body);
			AssertContains("GREENIES", email.Body);
		}

		public void TestEmptyValueAnd10Data()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBRRCJ_EmptyValueAnd10Data.xml");
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);
			Assert("PreCondition: Entry Status is Empty", entry.CH_EntryStatus.IsEmpty);

			AssertNoExceptionThrown(() => new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch());
			entry.Reload();
			incomingMessage.Reload();

			AssertEquals("entry status is updated correctly to CCL", CustomsEntryStatusTypeList.Codes.CCL, entry.CH_EntryStatus);

			AssertContains("수입신고서", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("41634-19-508005M", incomingMessage.EM_MessageInterpretation);
			AssertContains("[2] 전체통관취소", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("성분미달로 인한 요건 미비", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("[99] 기타", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("01012345678", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("04299999999", incomingMessage.EM_MessageInterpretation);

			AssertContains("<td>1</td>", incomingMessage.EM_MessageInterpretation);
			AssertContains("2309101000", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("DRY PETFOOD", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("GREENIES", incomingMessage.EM_MessageInterpretation);
			AssertContains("<td>2</td>", incomingMessage.EM_MessageInterpretation);
			AssertContains("2309101002", incomingMessage.EM_MessageInterpretation);
			AssertContains("<td>3</td>", incomingMessage.EM_MessageInterpretation);
			AssertContains("2309101003", incomingMessage.EM_MessageInterpretation);
			AssertContains("<td>4</td>", incomingMessage.EM_MessageInterpretation);
			AssertContains("2309101004", incomingMessage.EM_MessageInterpretation);
			AssertContains("<td>5</td>", incomingMessage.EM_MessageInterpretation);
			AssertContains("2309101005", incomingMessage.EM_MessageInterpretation);
			AssertContains("<td>6</td>", incomingMessage.EM_MessageInterpretation);
			AssertContains("2309101006", incomingMessage.EM_MessageInterpretation);
			AssertContains("<td>7</td>", incomingMessage.EM_MessageInterpretation);
			AssertContains("2309101007", incomingMessage.EM_MessageInterpretation);
			AssertContains("<td>8</td>", incomingMessage.EM_MessageInterpretation);
			AssertContains("2309101008", incomingMessage.EM_MessageInterpretation);
			AssertContains("<td>9</td>", incomingMessage.EM_MessageInterpretation);
			AssertContains("2309101009", incomingMessage.EM_MessageInterpretation);
			AssertContains("<td>10</td>", incomingMessage.EM_MessageInterpretation);
			AssertContains("2309101010", incomingMessage.EM_MessageInterpretation);
			AssertContains("<td>11</td>", incomingMessage.EM_MessageInterpretation);
			AssertContains("2309101011", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertNotContains("41634-19-508005M", email.Body);
			AssertContains("[2] 전체통관취소", email.Body);
			AssertNotContains("성분미달로 인한 요건 미비", email.Body);
			AssertNotContains("[99] 기타", email.Body);
			AssertNotContains("01012345678", email.Body);
			AssertNotContains("04299999999", email.Body);

			AssertContains("001", email.Body);
			AssertContains("2309101000", email.Body);
			AssertNotContains("DRY PETFOOD", email.Body);
			AssertNotContains("GREENIES", email.Body);
			AssertContains("002", email.Body);
			AssertContains("2309101002", email.Body);
			AssertContains("003", email.Body);
			AssertContains("2309101003", email.Body);
			AssertContains("004", email.Body);
			AssertContains("2309101004", email.Body);
			AssertContains("005", email.Body);
			AssertContains("2309101005", email.Body);
			AssertContains("006", email.Body);
			AssertContains("2309101006", email.Body);
			AssertContains("007", email.Body);
			AssertContains("2309101007", email.Body);
			AssertContains("008", email.Body);
			AssertContains("2309101008", email.Body);
			AssertContains("009", email.Body);
			AssertContains("2309101009", email.Body);
			AssertContains("010", email.Body);
			AssertContains("2309101010", email.Body);
			AssertContains("011", email.Body);
			AssertContains("2309101011", email.Body);
		}

		public void TestImportRCJ_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				CreateMessageForTest("GOVCBRRCJ_0.xml");
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [수입 통관보류통보]1234520000045M 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		public void TestImportRCJ_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBRRCJ_0.xml");
				CreateEntryForImport(false);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수입 통관보류통보] Response for Declaration Number: B00001000 / 제출번호: 1234520000045M", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [수입신고서]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);
			}
		}

		public void TestImportRCJ_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBRRCJ_0.xml");
				CreateEntryForImport(true);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[수입 통관보류통보] Response for Declaration Number: B00001000 / 제출번호: 1234520000045M", email.Subject);
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
			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._929;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_LinkTable = CusEntryHeader.Schema.TableName;
			outgoingMessage.EM_LinkedObject = importEntry;

			return importEntry;
		}

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBRRCJMessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._RCJ;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			Factory.Save();
			return incomingMessage;
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
	}
}
