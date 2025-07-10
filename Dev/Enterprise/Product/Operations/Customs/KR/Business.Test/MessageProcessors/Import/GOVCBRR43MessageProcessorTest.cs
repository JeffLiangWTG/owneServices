using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBRR43MessageProcessorTest : XMLMessageTestHelper<GOVCBRR43MessageProcessorTest>
	{
		public void Test_Status_Name_CodeIsC()
		{
			SampleCodeType();
			CreateEntryWithOutgoingMessageForImport();
			var outgoingMessage1 = CreateOutgoingMessage();
			outgoingMessage1.EM_ApplicationReference = "2";
			var outgoingMessage2 = CreateOutgoingMessage();
			outgoingMessage2.EM_ApplicationReference = "1";

			var incomingMessage = CreateMessageForTest("GOVCBRR43_Result_C.xml");
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			AssertEquals("PreCondition: Entry Status is Empty", ZString.Empty, incomingMessage.EM_MessageOwner);
			AssertEquals("PreCondition: Entry Status is Empty", ZDateTime.Empty, invoiceLine1.JI_ScheduledReExportDate);
			AssertEquals("PreCondition: Entry Status is Empty", ZDateTime.Empty, invoiceLine2.JI_ScheduledReExportDate);
			AssertEquals("PreCondition: Entry Status is Empty", ZDateTime.Empty, invoiceLine3.JI_ScheduledReExportDate);
			AssertEquals("PreCondition: Entry Status is Empty", ZDateTime.Empty, invoiceLine4.JI_ScheduledReExportDate);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			importEntry.Reload();
			invoiceLine1.Reload();
			invoiceLine2.Reload();
			invoiceLine3.Reload();
			invoiceLine4.Reload();
			incomingMessage.Reload();

			AssertEquals("entry status is updated correctly to ANT", "ANT", incomingMessage.EM_MessageOwner);
			AssertEquals("PreCondition: Entry Status is Empty", ZDateTime.Empty, invoiceLine1.JI_ScheduledReExportDate);
			AssertEquals("PreCondition: Entry Status is Empty", ZDateTime.Empty, invoiceLine2.JI_ScheduledReExportDate);
			AssertEquals("CusEntryNum.CE_ExpiryDate is updated correctly to Response/Declaration/AuthenticationDateTime", "20220112", invoiceLine3.JI_ScheduledReExportDate.ToString("yyyyMMdd"));
			AssertEquals("PreCondition: Entry Status is Empty", ZDateTime.Empty, invoiceLine4.JI_ScheduledReExportDate);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("재수출조건부 면세승인 이행기간연장 신청서", email.Body);
			AssertContains("12345-20-000045M", email.Body);
			AssertContains("2020-12-28", email.Body);
			AssertContains("2020-12-28 16:14:01", email.Body);
			AssertContains("2022-01-12", email.Body);
			AssertContains("승인", email.Body);
			AssertContains("박지영", email.Body);
			AssertContains("[03310] 양산세관 통관지원(1)과", email.Body);
			AssertContains(" - 사후심사결과에 따라 적용세율 변경될 수 있음<br>        - 재수출이행 이행기간 :  2021년01월13일 , 재수출 불이행시 면제된 관세 등과 가산세를 즉시 징수하게 됨. 용도외 사용, 양도(임대)&#183;폐기하고자 하는 때에는 미리 세관장 승인을 얻어야 함.<br>        - 확정가격 신고기한 : 2021-12-31   까지(불이행시 100만원 이하의 과태료 부과)", email.Body);
			AssertContains("의무이행요구사항", email.Body);
			AssertContains("감면/분납/용도세율적용대상물품의 양도 및 용도외사용금지", email.Body);
			AssertContains("재수출조건부 수입물품 재수출이행", email.Body);

			AssertContains("재수출조건부 면세승인 이행기간연장 신청서", incomingMessage.EM_MessageInterpretation);
			AssertContains("12345-20-000045M", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-12-28", incomingMessage.EM_MessageInterpretation);
			AssertContains("2020-12-28 16:14:01", incomingMessage.EM_MessageInterpretation);
			AssertContains("2022-01-12", incomingMessage.EM_MessageInterpretation);
			AssertContains("승인", incomingMessage.EM_MessageInterpretation);
			AssertContains("박지영", incomingMessage.EM_MessageInterpretation);
			AssertContains("[03310] 양산세관 통관지원(1)과", incomingMessage.EM_MessageInterpretation);
			AssertContains("의무이행요구사항", incomingMessage.EM_MessageInterpretation);
			AssertContains("감면/분납/용도세율적용대상물품의 양도 및 용도외사용금지", incomingMessage.EM_MessageInterpretation);
			AssertContains("재수출조건부 수입물품 재수출이행", incomingMessage.EM_MessageInterpretation);
		}
		public void Test_Status_Name_CodeIsE()
		{
			SampleCodeType();
			CreateEntryWithOutgoingMessageForImport();
			var outgoingMessage1 = CreateOutgoingMessage();
			outgoingMessage1.EM_ApplicationReference = "2";
			var outgoingMessage2 = CreateOutgoingMessage();
			outgoingMessage2.EM_ApplicationReference = "1";
			var incomingMessage = CreateMessageForTest("GOVCBRR43_Result_E.xml");
			Factory.Save();

			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			AssertEquals("PreCondition: Entry Status is Empty", ZString.Empty, incomingMessage.EM_MessageOwner);
			AssertEquals("PreCondition: Entry Status is Empty", ZDateTime.Empty, invoiceLine1.JI_ScheduledReExportDate);
			AssertEquals("PreCondition: Entry Status is Empty", ZDateTime.Empty, invoiceLine2.JI_ScheduledReExportDate);
			AssertEquals("PreCondition: Entry Status is Empty", ZDateTime.Empty, invoiceLine3.JI_ScheduledReExportDate);
			AssertEquals("PreCondition: Entry Status is Empty", ZDateTime.Empty, invoiceLine4.JI_ScheduledReExportDate);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			importEntry.Reload();
			invoiceLine1.Reload();
			invoiceLine2.Reload();
			invoiceLine3.Reload();
			invoiceLine4.Reload();
			incomingMessage.Reload();

			AssertEquals("entry status is updated correctly to DMS", "DMS", incomingMessage.EM_MessageOwner);
			AssertEquals("PreCondition: Entry Status is Empty", ZDateTime.Empty, invoiceLine1.JI_ScheduledReExportDate);
			AssertEquals("PreCondition: Entry Status is Empty", ZDateTime.Empty, invoiceLine2.JI_ScheduledReExportDate);
			AssertEquals("PreCondition: Entry Status is Empty", ZDateTime.Empty, invoiceLine3.JI_ScheduledReExportDate);
			AssertEquals("PreCondition: Entry Status is Empty", ZDateTime.Empty, invoiceLine4.JI_ScheduledReExportDate);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var recipient = email.Recipients[0];
			AssertEquals("OriginalSender@wisetechglobal.com", recipient.Email);
			AssertContains("재수출조건부 면세승인 이행기간연장 신청서", email.Body);
			AssertContains("12345-20-000045M", email.Body);
			AssertContains("2020-09-25", email.Body);
			AssertContains("2020-09-28 09:57:31", email.Body);
			AssertContains("2020-09-28", email.Body);
			AssertContains("기각", email.Body);
			AssertContains("연장신청기간 재검토", email.Body);
			AssertContains("홍다은", email.Body);
			AssertContains("[04011] 인천세관 수입(1)과", email.Body);
			AssertContains("        - 사후심사결과에 따라 적용세율 변경될 수 있음<br>        - 관세법 제157조의 2에 따라 화주 또는 반입자는 수입신고 수리 후 15일 이내 수입화물을 반출하여야 함.(위반시 과태료 부과)<br>        - 재수출이행 이행기간 :  2020년10월07일 , 재수출 불이행시 면제된 관세 등과 가산세를 즉시 징수하게 됨. 용도외 사용, 양도(임대)&#183;폐기하고자 하는 때에는 미리 세관장 승인을 얻어야 함.", email.Body);
			AssertContains("의무이행요구사항", email.Body);
			AssertContains("재수출조건부 수입물품 재수출이행", email.Body);

			AssertContains("기각", incomingMessage.EM_MessageInterpretation);
		}

		public void TestExportR43_NotificationSenderWithNoEntry()
		{
			using (KRCustomsRegistry.Instance.ImportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				SampleCodeType();
				var incomingMessage = CreateMessageForTest("GOVCBRR43_Result_C.xml");
				Factory.Save();
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("이메일 전송실패: [재수출이행기간 연장 신청서 처리결과 통보서]1234520000045M 사유: 신고내역을 찾을 수 없습니다.", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);

				AssertContains("재수출조건부 면세승인 이행기간연장 신청서", incomingMessage.EM_MessageInterpretation);
				AssertContains("12345-20-000045M", incomingMessage.EM_MessageInterpretation);
				AssertContains("2020-12-28", incomingMessage.EM_MessageInterpretation);
				AssertContains("2020-12-28 16:14:01", incomingMessage.EM_MessageInterpretation);
				AssertContains("2022-01-12", incomingMessage.EM_MessageInterpretation);
				AssertContains("승인", incomingMessage.EM_MessageInterpretation);
				AssertContains("박지영", incomingMessage.EM_MessageInterpretation);
				AssertContains("[03310] 양산세관 통관지원(1)과", incomingMessage.EM_MessageInterpretation);
				AssertContains("의무이행요구사항", incomingMessage.EM_MessageInterpretation);
				AssertContains("감면/분납/용도세율적용대상물품의 양도 및 용도외사용금지", incomingMessage.EM_MessageInterpretation);
				AssertContains("재수출조건부 수입물품 재수출이행", incomingMessage.EM_MessageInterpretation);
			}
		}
		public void TestExportR43_NotificationSendertWithEntryButNoOutgoingMessageWithoutCusAgent()
		{
			using (KRCustomsRegistry.Instance.ImportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				SampleCodeType();
				var incomingMessage = CreateMessageForTest("GOVCBRR43_Result_C.xml");
				CreateEntryForImport(false);
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[재수출이행기간 연장 신청서 처리결과 통보서] Response for Declaration Number: B00001000 / 제출번호: 1234520000045M", email.Subject);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains("요청 메시지 [재수출면세 이행기간연장 신청서]를 찾을 수 없어 해당 메시지 송신자가 아닌 레지스트리에 설정된 이메일 그룹으로 보내집니다.", email.Body);

				AssertContains("재수출조건부 면세승인 이행기간연장 신청서", incomingMessage.EM_MessageInterpretation);
				AssertContains("12345-20-000045M", incomingMessage.EM_MessageInterpretation);
				AssertContains("2020-12-28", incomingMessage.EM_MessageInterpretation);
				AssertContains("2020-12-28 16:14:01", incomingMessage.EM_MessageInterpretation);
				AssertContains("2022-01-12", incomingMessage.EM_MessageInterpretation);
				AssertContains("승인", incomingMessage.EM_MessageInterpretation);
				AssertContains("박지영", incomingMessage.EM_MessageInterpretation);
				AssertContains("[03310] 양산세관 통관지원(1)과", incomingMessage.EM_MessageInterpretation);
				AssertContains("의무이행요구사항", incomingMessage.EM_MessageInterpretation);
				AssertContains("감면/분납/용도세율적용대상물품의 양도 및 용도외사용금지", incomingMessage.EM_MessageInterpretation);
				AssertContains("재수출조건부 수입물품 재수출이행", incomingMessage.EM_MessageInterpretation);
			}
		}
		public void TestExportR43_NotificationSendertWithEntryButNoOutgoingMessageWithCusAgent()
		{
			using (KRCustomsRegistry.Instance.ImportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
				SampleCodeType();
				var incomingMessage = CreateMessageForTest("GOVCBRR43_Result_C.xml");
				CreateEntryForImport(true);
				new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();
				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals("[재수출이행기간 연장 신청서 처리결과 통보서] Response for Declaration Number: B00001000 / 제출번호: 1234520000045M", email.Subject);
				AssertEquals("CusAgent@wisetechglobal.com", email.Recipients[0].Email);

				AssertContains("재수출조건부 면세승인 이행기간연장 신청서", incomingMessage.EM_MessageInterpretation);
				AssertContains("12345-20-000045M", incomingMessage.EM_MessageInterpretation);
				AssertContains("2020-12-28", incomingMessage.EM_MessageInterpretation);
				AssertContains("2020-12-28 16:14:01", incomingMessage.EM_MessageInterpretation);
				AssertContains("2022-01-12", incomingMessage.EM_MessageInterpretation);
				AssertContains("승인", incomingMessage.EM_MessageInterpretation);
				AssertContains("박지영", incomingMessage.EM_MessageInterpretation);
				AssertContains("[03310] 양산세관 통관지원(1)과", incomingMessage.EM_MessageInterpretation);
				AssertContains("의무이행요구사항", incomingMessage.EM_MessageInterpretation);
				AssertContains("감면/분납/용도세율적용대상물품의 양도 및 용도외사용금지", incomingMessage.EM_MessageInterpretation);
				AssertContains("재수출조건부 수입물품 재수출이행", incomingMessage.EM_MessageInterpretation);
			}
		}
		public void Test_ExceptionCase()
		{
			SampleCodeType();
			CreateEntryWithOutgoingMessageForImport();
			var outgoingMessage = CreateOutgoingMessage();
			outgoingMessage.EM_ApplicationReference = "1";
			var incomingMessage = CreateMessageForTest("GOVCBRR43_ExceptionCase.xml");
			Factory.Save();
			AssertEquals("PreCondition: Message Linked Object is Empty", ZGuid.Empty, incomingMessage.EM_LinkUniqueID);
			AssertEquals("PreCondition: Entry Status is Empty", ZString.Empty, incomingMessage.EM_MessageOwner);
			AssertEquals("PreCondition: Entry Status is Empty", ZDateTime.Empty, invoiceLine1.JI_ScheduledReExportDate);
			AssertEquals("PreCondition: Entry Status is Empty", ZDateTime.Empty, invoiceLine2.JI_ScheduledReExportDate);
			AssertEquals("PreCondition: Entry Status is Empty", ZDateTime.Empty, invoiceLine3.JI_ScheduledReExportDate);
			AssertEquals("PreCondition: Entry Status is Empty", ZDateTime.Empty, invoiceLine4.JI_ScheduledReExportDate);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			new KRCIncomingMessageProcessor(new BatchProcessor.LoggingInformation()).ExecuteBatch();

			importEntry.Reload();
			invoiceLine1.Reload();
			invoiceLine2.Reload();
			invoiceLine3.Reload();
			invoiceLine4.Reload();
			incomingMessage.Reload();

			AssertEquals("entry is located", importEntry.PK, incomingMessage.EM_LinkUniqueID);
			AssertEquals("entry status is updated correctly to ''", ZString.Empty, incomingMessage.EM_MessageOwner);
			AssertEquals("PreCondition: Entry Status is Empty", ZDateTime.Empty, invoiceLine1.JI_ScheduledReExportDate);
			AssertEquals("PreCondition: Entry Status is Empty", ZDateTime.Empty, invoiceLine2.JI_ScheduledReExportDate);
			AssertEquals("PreCondition: Entry Status is Empty", ZDateTime.Empty, invoiceLine3.JI_ScheduledReExportDate);
			AssertEquals("PreCondition: Entry Status is Empty", ZDateTime.Empty, invoiceLine4.JI_ScheduledReExportDate);

			AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
			AssertNotContains("[]", email.Body);
			AssertNotContains("의무이행요구사항", email.Body);

			AssertContains("<td>처리세관(과)</td><td>&nbsp;</td>", incomingMessage.EM_MessageInterpretation);
			AssertNotContains("의무이행요구사항", incomingMessage.EM_MessageInterpretation);
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
			entryNumber.CE_EntryNum = "1234520000045M";
			entryNumber.CE_EntryType = "IMP";
			entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNumber.CE_ParentID = importEntry.PK;
			entryNumber.CE_ParentTable = CusEntryHeader.Schema.TableName;

			var entryNumberD72 = importEntry.EntryNumbers.AddNew();
			entryNumberD72.CE_EntryNum = "1234520000045M";
			entryNumberD72.CE_EntryType = "D72";
			entryNumberD72.CE_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			entryNumberD72.CE_ParentID = importEntry.PK;
			entryNumberD72.CE_ParentTable = CusEntryHeader.Schema.TableName;

			#region D72Line
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "EUR";

			invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Description = "02JAC106I603C087";
			invoiceLine2.JI_InvoiceUQ = "PC";
			invoiceLine2.JI_InvoiceQuantity = 1m;
			invoiceLine2.JI_LinePrice = 68m;
			invoiceLine2.JI_CountryOfOrigin = declaration.CountryCode;
			invoiceLine2.JI_Tariff = "01234";
			invoiceLine2.JI_SequenceNumber = 2;

			invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Description = "9E1J29A3802 X0849";
			invoiceLine1.JI_InvoiceUQ = "PC";
			invoiceLine1.JI_InvoiceQuantity = 1m;
			invoiceLine1.JI_LinePrice = 610m;
			invoiceLine1.JI_CountryOfOrigin = declaration.CountryCode;
			invoiceLine1.JI_Tariff = "01234";
			invoiceLine1.JI_SequenceNumber = 1;

			invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Description = "9E1J29A3802 X0849";
			invoiceLine3.JI_InvoiceUQ = "PC";
			invoiceLine3.JI_InvoiceQuantity = 1m;
			invoiceLine3.JI_LinePrice = 610m;
			invoiceLine3.JI_CountryOfOrigin = declaration.CountryCode;
			invoiceLine3.JI_Tariff = "56789";
			invoiceLine3.JI_SequenceNumber = 1;

			invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_Description = "9E1J29A3802 X0849";
			invoiceLine4.JI_InvoiceUQ = "PC";
			invoiceLine4.JI_InvoiceQuantity = 1m;
			invoiceLine4.JI_LinePrice = 610m;
			invoiceLine4.JI_CountryOfOrigin = declaration.CountryCode;
			invoiceLine4.JI_Tariff = "56789";
			invoiceLine4.JI_SequenceNumber = 2;

			var entryLine = importEntry.MergedLines.AddNew();
			entryLine.CL_CH = importEntry.PK;
			entryLine.CL_LineNumber = 001;
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;
			entryLine.CL_AdValoremTariff = "01234";

			var entryLine2 = importEntry.MergedLines.AddNew();
			entryLine2.CL_CH = importEntry.PK;
			entryLine2.CL_LineNumber = 002;
			invoiceLine3.JI_CL = entryLine2.PK;
			invoiceLine3.JI_CL = entryLine2.PK;
			entryLine2.CL_AdValoremTariff = "56789";
			#endregion

			Factory.Save();
		}
		CusEntryHeader importEntry;
		JobComInvoiceLine invoiceLine1;
		JobComInvoiceLine invoiceLine2;
		JobComInvoiceLine invoiceLine3;
		JobComInvoiceLine invoiceLine4;

		void CreateEntryWithOutgoingMessageForImport()
		{
			if (importEntry == null)
			{
				CreateEntryForImport(true);
			}
		}
		
		EDIMessage CreateOutgoingMessage()
		{
			var outgoingMessage = Factory.New<EDIMessage>();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._D72;
			outgoingMessage.EM_SystemCreateUser = "ORG";
			outgoingMessage.EM_LinkTable = CusEntryHeader.Schema.TableName;
			outgoingMessage.EM_LinkUniqueID = importEntry.PK;
			outgoingMessage.EM_LinkedObject = importEntry;
			outgoingMessage.EM_MessageSubType = "";
			return outgoingMessage;
		}

		EDIMessage CreateMessageForTest(string fileName)
		{
			var fileReader = new TestFileReader(typeof(GOVCBRR43MessageProcessorTest));
			var messageText = fileReader.GetEmbeddedFileText(TestFilesPath, fileName);
			var incomingMessage = Factory.New<EDIMessage>();
			incomingMessage.EM_MessageType = ElectronicDocumentTypeList.Codes._R43;
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
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "033", "양산세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "10", "통관지원(1)과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "040", "인천세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsDepartment, "11", "수입(1)과", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Incoming";
	}
}
