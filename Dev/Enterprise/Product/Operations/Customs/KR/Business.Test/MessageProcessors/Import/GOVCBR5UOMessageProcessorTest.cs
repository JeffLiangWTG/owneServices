using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5UOMessageProcessorTest : XMLMessageTestHelper<GOVCBR5UOMessageProcessorTest>
	{
		public void Test5UO()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR5UO_CUS.xml");
			var cusEntryNumber = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5UL);
			var cusEntryNumber5UO = entry.EntryNumbers.GetOrCreateCusEntryNum(ElectronicDocumentTypeList.Codes._5UO);
			Factory.Save();

			Assert("PreCondition: No entry is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);

			Assert("PreCondition: Approval Date is empty", cusEntryNumber5UO.CE_IssueDate.IsEmpty);
			Assert("PreCondition: Approval Number is empty", cusEntryNumber5UO.CE_EntryNum.IsEmpty);
			Assert("PreCondition: Request Number is empty", cusEntryNumber5UO.CE_EntryLineReference.IsEmpty);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			entry.Reload();
			incomingMessage.Reload();
			cusEntryNumber.Reload();
			cusEntryNumber5UO.Reload();

			AssertEquals("entry is located", entry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to ANT", CustomsEntryStatusTypeList.Codes.ANT, incomingMessage.EM_MessageOwner);

			AssertEquals("Approval Date is updated correctly", new ZDateTime(2020, 09, 24), cusEntryNumber5UO.CE_IssueDate);
			AssertEquals("Approval Number is updated correctly", "030752019608", cusEntryNumber5UO.CE_EntryNum);
			AssertEquals("Request Number is updated correctly", "6N00220000085M", cusEntryNumber5UO.CE_EntryLineReference);

			var outgoingMessage = entry.Messages.LastOutgoingMessage;
			AssertEquals(outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);

			AssertContains("과오납 환급 신청서", incomingMessage.EM_MessageInterpretation);
			AssertContains("6N00220000085M", incomingMessage.EM_MessageInterpretation);
			AssertContains("켐트리 코퍼레이션", incomingMessage.EM_MessageInterpretation);
			AssertContains("이재천", incomingMessage.EM_MessageInterpretation);
			AssertContains("5405동 504호 (창곡동,위례센트럴푸르지오)", incomingMessage.EM_MessageInterpretation);
			AssertContains("경기 성남시 수정구 위례순환로 211,", incomingMessage.EM_MessageInterpretation);
			AssertContains("부산세관", incomingMessage.EM_MessageInterpretation);
			AssertContains("심사정보과", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-08-27", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-09-24", incomingMessage.EM_MessageInterpretation);
			AssertContains("030752019608", incomingMessage.EM_MessageInterpretation);
			AssertContains("한국은행", incomingMessage.EM_MessageInterpretation);
			AssertContains("기업은행", incomingMessage.EM_MessageInterpretation);
			AssertContains("98600471901016", incomingMessage.EM_MessageInterpretation);
			AssertContains("2014-05-06", incomingMessage.EM_MessageInterpretation);

			AssertContains("<tr><th align=\"center\" width=\"550\" colspan=\"3\">세&nbsp;&nbsp;&nbsp;&nbsp;액&nbsp;&nbsp;&nbsp;&nbsp;내&nbsp;&nbsp;&nbsp;&nbsp;용</th></tr>", incomingMessage.EM_MessageInterpretation);
			AssertContains("<tr><td align=\"center\" width=\"33%\">관&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;세</td><td align=\"center\" width=\"33%\">교통에너지환경세</td><td align=\"center\" width=\"33%\">개&nbsp;&nbsp;별&nbsp;&nbsp;소&nbsp;&nbsp;비&nbsp;&nbsp;세</td></tr>", incomingMessage.EM_MessageInterpretation);
			AssertContains("<tr><td align=\"center\" width=\"33%\">주&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;세</td><td align=\"center\" width=\"33%\">농&nbsp;&nbsp;&nbsp;&nbsp;특&nbsp;&nbsp;&nbsp;&nbsp;세</td><td align=\"center\" width=\"33%\">부&nbsp;&nbsp;가&nbsp;&nbsp;가&nbsp;&nbsp;치&nbsp;&nbsp;세</td></tr>", incomingMessage.EM_MessageInterpretation);
			AssertContains("<tr><td align=\"center\" width=\"33%\">교&nbsp;&nbsp;&nbsp;&nbsp;육&nbsp;&nbsp;&nbsp;&nbsp;세</td><td align=\"center\" width=\"33%\">가&nbsp;&nbsp;&nbsp;&nbsp;산&nbsp;&nbsp;&nbsp;&nbsp;금</td><td align=\"center\" width=\"33%\">세&nbsp;&nbsp;&nbsp;&nbsp;외&nbsp;&nbsp;&nbsp;&nbsp;수&nbsp;&nbsp;&nbsp;&nbsp;입</td></tr>", incomingMessage.EM_MessageInterpretation);
			AssertContains("<tr><td align=\"right\" width=\"33%\">5,368,150</td><td align=\"right\" width=\"33%\">200</td><td align=\"right\" width=\"33%\">100</td></tr>", incomingMessage.EM_MessageInterpretation);
			AssertContains("<tr><td align=\"right\" width=\"33%\">300</td><td align=\"right\" width=\"33%\">500</td><td align=\"right\" width=\"33%\">536,820</td></tr>", incomingMessage.EM_MessageInterpretation);
			AssertContains("<tr><td align=\"right\" width=\"33%\">400</td><td align=\"right\" width=\"33%\">6,597</td><td align=\"right\" width=\"33%\">600</td></tr>", incomingMessage.EM_MessageInterpretation);
			AssertContains("<tr><td align=\"center\" colspan=\"2\">합&nbsp;계&nbsp;금&nbsp;액</td><td align=\"right\" width=\"33%\">5,913,667</td></tr></table>", incomingMessage.EM_MessageInterpretation);

			var stmAlogFilter = new ZQuery(StmALogSchema.SL_Parent, entry.PK);
			stmAlogFilter.AddToFilter(StmALogSchema.SL_Table, CusEntryHeader.Schema.TableName);
			var alog = Factory.LoadTop1<StmALog>(stmAlogFilter);
			AssertEquals("StmAlog SL_EventTime is updated", "20200924", alog.SL_EventTime.ToString("yyyyMMdd"));
			AssertEquals("StmAlog SL_SE_NKEvent is updated", "ATH", alog.SL_SE_NKEvent);
			AssertEquals("StmAlog SL_Parent is updated", entry.PK, alog.SL_Parent);
			AssertEquals("StmAlog SL_Reference is updated", "TYP=Refund|REF=6N00220000085M", alog.SL_Reference);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("과오납 환급 신청서", email.Body);
			AssertContains("6N00220000085M", email.Body);
			AssertContains("켐트리 코퍼레이션", email.Body);
			AssertContains("이재천", email.Body);
			AssertContains("5405동 504호 (창곡동,위례센트럴푸르지오)", email.Body);
			AssertContains("경기 성남시 수정구 위례순환로 211,", email.Body);
			AssertContains("부산세관", email.Body);
			AssertContains("심사정보과", email.Body);
			AssertContains("2020-08-27", email.Body);
			AssertContains("2020-09-24", email.Body);
			AssertContains("030752019608", email.Body);
			AssertContains("한국은행", email.Body);
			AssertContains("기업은행", email.Body);
			AssertContains("98600471901016", email.Body);
			AssertContains("2014-05-06", email.Body);

			AssertContains("<tr><th align=\"center\" width=\"550\" colspan=\"3\">세&nbsp;&nbsp;&nbsp;&nbsp;액&nbsp;&nbsp;&nbsp;&nbsp;내&nbsp;&nbsp;&nbsp;&nbsp;용</th></tr>", email.Body);
			AssertContains("<tr><td align=\"center\" width=\"33%\">관&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;세</td><td align=\"center\" width=\"33%\">교통에너지환경세</td><td align=\"center\" width=\"33%\">개&nbsp;&nbsp;별&nbsp;&nbsp;소&nbsp;&nbsp;비&nbsp;&nbsp;세</td></tr>", email.Body);
			AssertContains("<tr><td align=\"center\" width=\"33%\">주&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;세</td><td align=\"center\" width=\"33%\">농&nbsp;&nbsp;&nbsp;&nbsp;특&nbsp;&nbsp;&nbsp;&nbsp;세</td><td align=\"center\" width=\"33%\">부&nbsp;&nbsp;가&nbsp;&nbsp;가&nbsp;&nbsp;치&nbsp;&nbsp;세</td></tr>", email.Body);
			AssertContains("<tr><td align=\"center\" width=\"33%\">교&nbsp;&nbsp;&nbsp;&nbsp;육&nbsp;&nbsp;&nbsp;&nbsp;세</td><td align=\"center\" width=\"33%\">가&nbsp;&nbsp;&nbsp;&nbsp;산&nbsp;&nbsp;&nbsp;&nbsp;금</td><td align=\"center\" width=\"33%\">세&nbsp;&nbsp;&nbsp;&nbsp;외&nbsp;&nbsp;&nbsp;&nbsp;수&nbsp;&nbsp;&nbsp;&nbsp;입</td></tr>", email.Body);
			AssertContains("<tr><td align=\"right\" width=\"33%\">5,368,150</td><td align=\"right\" width=\"33%\">200</td><td align=\"right\" width=\"33%\">100</td></tr>", email.Body);
			AssertContains("<tr><td align=\"right\" width=\"33%\">300</td><td align=\"right\" width=\"33%\">500</td><td align=\"right\" width=\"33%\">536,820</td></tr>", email.Body);
			AssertContains("<tr><td align=\"right\" width=\"33%\">400</td><td align=\"right\" width=\"33%\">6,597</td><td align=\"right\" width=\"33%\">600</td></tr>", email.Body);
			AssertContains("<tr><td align=\"center\" colspan=\"2\">합&nbsp;계&nbsp;금&nbsp;액</td><td align=\"right\" width=\"33%\">5,913,667</td></tr></table>", email.Body);
		}

		public void Test5UO_RefundDeclaration()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var refundDeclaration = new TestDataSetupHelper(Factory).CreateRefundDeclarationWithOutgoingMessage("6N00220000085M");
			var incomingMessage = CreateMessageForTest("GOVCBR5UO_CUS.xml");
			var filter5UL = new ZQuery(CusEntryNumSchema.CE_ParentID, refundDeclaration.PK);
			filter5UL.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusReconDeclaration.Schema.TableName);
			filter5UL.AddToFilter(CusEntryNumSchema.CE_EntryType, ElectronicDocumentTypeList.Codes._5UL);
			filter5UL.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.KoreaSouth);
			var cusEntryNumber = Factory.LoadTop1<CusEntryNumber>(filter5UL);

			var filter5UO = new ZQuery(CusEntryNumSchema.CE_ParentID, refundDeclaration.PK);
			filter5UO.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusReconDeclaration.Schema.TableName);
			filter5UO.AddToFilter(CusEntryNumSchema.CE_EntryType, ElectronicDocumentTypeList.Codes._5UO);
			filter5UO.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.KoreaSouth);
			var cusEntryNumber5UO = Factory.LoadTop1<CusEntryNumber>(filter5UO);
			Factory.Save();

			Assert("PreCondition: No Declaration is linked", incomingMessage.EM_LinkUniqueID.IsEmpty);

			AssertNull("PreCondition: 5UO Entry Number is null", cusEntryNumber5UO);

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			refundDeclaration.Reload();
			incomingMessage.Reload();
			cusEntryNumber.Reload();
			cusEntryNumber5UO = Factory.LoadTop1<CusEntryNumber>(filter5UO);

			AssertEquals("Declaration is located", refundDeclaration.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to ANT", CustomsEntryStatusTypeList.Codes.ANT, incomingMessage.EM_MessageOwner);

			AssertEquals("Approval Date is updated correctly", new ZDateTime(2020, 09, 24), cusEntryNumber5UO.CE_IssueDate);
			AssertEquals("Approval Number is updated correctly", "030752019608", cusEntryNumber5UO.CE_EntryNum);
			AssertEquals("Request Number is updated correctly", "6N00220000085M", cusEntryNumber5UO.CE_EntryLineReference);

			var outgoingMessage = refundDeclaration.Messages.LastOutgoingMessage;
			AssertEquals(outgoingMessage.EM_MessageNum, incomingMessage.EM_ApplicationReference);

			AssertContains("과오납 환급 신청서", incomingMessage.EM_MessageInterpretation);
			AssertContains("6N00220000085M", incomingMessage.EM_MessageInterpretation);
			AssertContains("켐트리 코퍼레이션", incomingMessage.EM_MessageInterpretation);
			AssertContains("이재천", incomingMessage.EM_MessageInterpretation);
			AssertContains("5405동 504호 (창곡동,위례센트럴푸르지오)", incomingMessage.EM_MessageInterpretation);
			AssertContains("경기 성남시 수정구 위례순환로 211,", incomingMessage.EM_MessageInterpretation);
			AssertContains("부산세관", incomingMessage.EM_MessageInterpretation);
			AssertContains("심사정보과", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-08-27", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-09-24", incomingMessage.EM_MessageInterpretation);
			AssertContains("030752019608", incomingMessage.EM_MessageInterpretation);
			AssertContains("한국은행", incomingMessage.EM_MessageInterpretation);
			AssertContains("기업은행", incomingMessage.EM_MessageInterpretation);
			AssertContains("98600471901016", incomingMessage.EM_MessageInterpretation);
			AssertContains("2014-05-06", incomingMessage.EM_MessageInterpretation);

			AssertContains("<tr><th align=\"center\" width=\"550\" colspan=\"3\">세&nbsp;&nbsp;&nbsp;&nbsp;액&nbsp;&nbsp;&nbsp;&nbsp;내&nbsp;&nbsp;&nbsp;&nbsp;용</th></tr>", incomingMessage.EM_MessageInterpretation);
			AssertContains("<tr><td align=\"center\" width=\"33%\">관&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;세</td><td align=\"center\" width=\"33%\">교통에너지환경세</td><td align=\"center\" width=\"33%\">개&nbsp;&nbsp;별&nbsp;&nbsp;소&nbsp;&nbsp;비&nbsp;&nbsp;세</td></tr>", incomingMessage.EM_MessageInterpretation);
			AssertContains("<tr><td align=\"center\" width=\"33%\">주&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;세</td><td align=\"center\" width=\"33%\">농&nbsp;&nbsp;&nbsp;&nbsp;특&nbsp;&nbsp;&nbsp;&nbsp;세</td><td align=\"center\" width=\"33%\">부&nbsp;&nbsp;가&nbsp;&nbsp;가&nbsp;&nbsp;치&nbsp;&nbsp;세</td></tr>", incomingMessage.EM_MessageInterpretation);
			AssertContains("<tr><td align=\"center\" width=\"33%\">교&nbsp;&nbsp;&nbsp;&nbsp;육&nbsp;&nbsp;&nbsp;&nbsp;세</td><td align=\"center\" width=\"33%\">가&nbsp;&nbsp;&nbsp;&nbsp;산&nbsp;&nbsp;&nbsp;&nbsp;금</td><td align=\"center\" width=\"33%\">세&nbsp;&nbsp;&nbsp;&nbsp;외&nbsp;&nbsp;&nbsp;&nbsp;수&nbsp;&nbsp;&nbsp;&nbsp;입</td></tr>", incomingMessage.EM_MessageInterpretation);
			AssertContains("<tr><td align=\"right\" width=\"33%\">5,368,150</td><td align=\"right\" width=\"33%\">200</td><td align=\"right\" width=\"33%\">100</td></tr>", incomingMessage.EM_MessageInterpretation);
			AssertContains("<tr><td align=\"right\" width=\"33%\">300</td><td align=\"right\" width=\"33%\">500</td><td align=\"right\" width=\"33%\">536,820</td></tr>", incomingMessage.EM_MessageInterpretation);
			AssertContains("<tr><td align=\"right\" width=\"33%\">400</td><td align=\"right\" width=\"33%\">6,597</td><td align=\"right\" width=\"33%\">600</td></tr>", incomingMessage.EM_MessageInterpretation);
			AssertContains("<tr><td align=\"center\" colspan=\"2\">합&nbsp;계&nbsp;금&nbsp;액</td><td align=\"right\" width=\"33%\">5,913,667</td></tr></table>", incomingMessage.EM_MessageInterpretation);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("과오납 환급 신청서", email.Body);
			AssertContains("6N00220000085M", email.Body);
		}

		public void Test5UO_EntryNumExistsOrNot()
		{
			var refundDeclaration = new TestDataSetupHelper(Factory).CreateRefundDeclarationWithOutgoingMessage("6N00220000085M");
			var incomingMessage = CreateMessageForTest("GOVCBR5UO_CUS.xml");
			Factory.Save();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			refundDeclaration.Reload();
			incomingMessage.Reload();
			var filter5UO = new ZQuery(CusEntryNumSchema.CE_ParentID, refundDeclaration.PK);
			filter5UO.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusReconDeclaration.Schema.TableName);
			filter5UO.AddToFilter(CusEntryNumSchema.CE_EntryType, ElectronicDocumentTypeList.Codes._5UO);
			filter5UO.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.KoreaSouth);
			var cusEntryNumber5UO = Factory.LoadTop1<CusEntryNumber>(filter5UO);

			AssertEquals("Approval Date is updated correctly", new ZDateTime(2020, 09, 24), cusEntryNumber5UO.CE_IssueDate);
			AssertEquals("Approval Number is updated correctly", "030752019608", cusEntryNumber5UO.CE_EntryNum);
			AssertEquals("Request Number is updated correctly", "6N00220000085M", cusEntryNumber5UO.CE_EntryLineReference);

			cusEntryNumber5UO.CE_IssueDate = ZDateTime.Empty;
			cusEntryNumber5UO.CE_EntryNum = ZString.Empty;
			cusEntryNumber5UO.CE_EntryLineReference = ZString.Empty;

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
			refundDeclaration.Reload();
			incomingMessage.Reload();
			cusEntryNumber5UO.Reload();

			AssertEquals("Approval Date is updated correctly", new ZDateTime(2020, 09, 24), cusEntryNumber5UO.CE_IssueDate);
			AssertEquals("Approval Number is updated correctly", "030752019608", cusEntryNumber5UO.CE_EntryNum);
			AssertEquals("Request Number is updated correctly", "6N00220000085M", cusEntryNumber5UO.CE_EntryLineReference);
		}

		public void Test5UO_IssueDateTimeIsEmpty()
		{
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var entry = CreateEntryWithOutgoingMessageForImport();
			var incomingMessage = CreateMessageForTest("GOVCBR5UO_Empty.xml");
			Factory.Save();

			AssertNoExceptionThrown("When Xml Element Values is Empty, system should still proceed successfully", () =>
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch());
			incomingMessage.Reload();

			AssertContains("incomingMessage EM_MessageInterpretation is updated", "과오납 환급 신청서", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("2014-05-06", incomingMessage.EM_MessageInterpretation);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertNotContains("2014-05-06", email.Body);
		}

		public void TestImport5UO_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBR5UO_CUS.xml");
				CreateEntryForImport(false);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[환급금통지서] Response for Declaration Number: B00001000 / 제출번호: 6N00220000085M", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [과오납 환급신청서]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);
			}
		}

		public void TestImport5UO_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.ExportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				CreateMessageForTest("GOVCBR5UO_CUS.xml");
				CreateEntryForImport(true);
				Factory.Save();

				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[환급금통지서] Response for Declaration Number: B00001000 / 제출번호: 6N00220000085M", email.Subject);
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
			entryNumber.CE_EntryNum = "6N00220000085M";
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
			outgoingMessage.EM_MessageOwner = "6N00220000085M";

			return importEntry;
		}

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBR5UOMessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._5UO;
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = messageText;
			return incomingMessage;
		}
		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
	}
}
