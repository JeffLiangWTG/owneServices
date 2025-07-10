using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business.Testing
{
	public abstract class AESCommonResponseMessageProcessorTest<TResponse, TResponseProcessor> : XMLResponseMessageProcessorTest<TResponse, IMessagePrettyFormatter, TResponseProcessor>
		where TResponse : AESCommonResponseMessageProcessor<TResponseProcessor>
		where TResponseProcessor : class, ICommonServiceSegment, IResponseCode, ICommonErrors, IMRNField
	{
		public void TestProcessRejectedMessage()
		{
			AddMessageProcessAndAssertResult_RejectedMessageBase(RejectedEntryStatus);
		}

		public void TestProcessErrorMessage()
		{
			var responseMessage = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetErrorTestFile(), InterchangeID);
			ProcessMessageForTest(responseMessage);
			GenericCommonAssertProcessEntryData(responseMessage, entryHeader, expectedMessageInterpretation: ErrorMessageInterpretation, messageNum: MessageNum, messageSubType: RejectedMessageStatus, entryStatusCode: OriginalEntryStatus, movementReferenceNumber: RejectedMRN, acceptanceDate: RejectedAcceptanceDate);
		}

		protected void AddMessageProcessAndAssertResult_RejectedMessage(bool isEntryStatusEmpty = false)
		{
			var entryStatus = isEntryStatusEmpty ? ZString.Empty : RejectedEntryStatus;

			AddMessageProcessAndAssertResult_RejectedMessageBase(entryStatus);
		}

		protected void AddMessageProcessAndAssertResult_RejectedMessage()
		{
			AddMessageProcessAndAssertResult_RejectedMessageBase(RejectedEntryStatus);
		}

		void AddMessageProcessAndAssertResult_RejectedMessageBase(ZString entryStatus)
		{
			var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetRejectedTestFile(), InterchangeID);
			ProcessMessageForTest(message);
			GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: RejectedMessageInterpretation, messageNum: MessageNum, messageSubType: RejectedMessageStatus, entryStatusCode: entryStatus, movementReferenceNumber: RejectedMRN, acceptanceDate: RejectedAcceptanceDate);
		}

		protected override void SetUp()
		{
			base.SetUp();

			newDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			newDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
			var staffWithCertificateHelperTest = new StaffWithCertificateTestHelper(Factory);
			var staff = staffWithCertificateHelperTest.Staff;
			newDeclaration.JE_GS_NKCusAgent = staff.GS_Code;

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_FullName = DeclarantName;
			declarant.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, DeclarantId);
			newDeclaration.Declarant.OA_OH = declarant.PK;

			invoice = newDeclaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			instruction = newDeclaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.A;
			invoiceLine.JI_CEI = instruction.PK;

			entryHeader = (CusEntryHeader)newDeclaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = ApplicationReference;
			entryHeader.CH_CEI_Instruction = instruction.PK;
			entryHeader.CH_EntryStatus = OriginalEntryStatus;
			entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;
			invoiceLine.JI_CL = entryLine.PK;

			SetSentInterchange(entryHeader, InterchangeID);

			helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status");

			grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(EsCode, parent: grouping);
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, OriginalEntryStatus, "Original Entry Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CLR", "Customs Declaration Accepted and Cleared", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CAN", "Cancelled Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CDA", "Customs Declaration accepted", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "PCO", "Customs PCO Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "PDA", "Customs PDA Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "EFD", "Customs EFD Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "CLP", "Customs CLP Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "STD", "Customs STD Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "INV", "Customs INV Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			Factory.Save();
		}
		protected UniversalReferenceTestDataHelper helper;
		protected RefDataGrouping grouping;

		protected JobDeclaration newDeclaration;
		protected JobComInvoiceHeader invoice;
		protected JobComInvoiceLine invoiceLine;
		protected CusEntryLine entryLine;
		protected CusEntryInstruction instruction;

		protected CodeDescriptionPairList entryStatusList;
		protected TestEdiMessage sentMessage;

		protected abstract string GetRejectedTestFile();
		protected abstract string GetErrorTestFile();

		protected abstract ZString RejectedMRN { get; }
		protected abstract ZString ErrorMRN { get; }

		protected abstract ZDateTime RejectedAcceptanceDate { get; }
		protected abstract ZDateTime ErrorAcceptanceDate { get; }

		protected virtual ZString RejectedMessageStatus => EDIMessage.Status.Received;
		ZString RejectedEntryStatus => OriginalEntryStatus;

		protected virtual ZString RejectedMessageInterpretation => "<H3>Rejected Declaration</H3>" +
				"<H4>List of Errors:</H4>" +
				"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td><strong>Code</strong></td><td><strong>Place</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
				"<tr><td>14</td><td>/CC514C/ExportOperation/MRN</td><td>(1162) No existe declaración para el valor del MRN indicado.</td><td>20ES00999930006184</td></tr>" +
				"</table>";

		protected abstract ZString ErrorMessageInterpretation { get; }

		protected const string MRNCode = "20ES00999930006184";
		protected const string MessageNum = "20221011162539689003";
		protected readonly ZDateTime AcceptanceDate = new ZDateTime(2022, 06, 10);
		protected const string CsvClearance = "TEST444444444444";
		protected const string CsvElectronicDeclaration = "HHAXSXPQA6NT963Y";
		protected const string MRNCodeExisting = "21ES009999L0000001";
		protected readonly ZDateTime AcceptanceDateExisting = new ZDateTime(2021, 01, 15, 05, 40, 55);
		protected readonly ZDateTime MovementReferenceNumberIssueDate = new ZDateTime(2021, 01, 15, 05, 40, 55);

		protected override EDIInterchange CreateTestResponseInterchange(ZGuid interchangeID, ZString interchangeTransportType)
		{
			var interchange = base.CreateTestResponseInterchange(interchangeID, interchangeTransportType);
			if (interchangeTransportType != EDIInterchange.TransportType.xT)
			{
				interchange.EI_HeaderText = ZString.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Headers>
  <BrokerCode>AZ</BrokerCode>
  <CertificateName>CertName</CertificateName>
  <CertificateThumbPrint>CertThumbPrint</CertificateThumbPrint>
  <EntryReferenceNumber>1234123444</EntryReferenceNumber>
  <TestMessage>Y</TestMessage>
  <Service>Service</Service>
  <Operation>Operation</Operation>
  <SentEDIMessageNumber>1</SentEDIMessageNumber>
</Headers>");
			}
			return interchange;
		}
	}
}
