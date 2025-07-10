using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
namespace Enterprise.Customs.ES.Business.Testing;

public abstract class ImportH1CommonResponseMessageProcessorTest<TResponse, TResponseProcessor> : XMLResponseMessageProcessorTest<TResponse, IMessagePrettyFormatter, TResponseProcessor>
	where TResponse : ImportH1CommonResponseMessageProcessor<TResponseProcessor>
	where TResponseProcessor : class, ICommonServiceSegment, IResponseCode, ICommonErrors, IMRNField
{
	public void TestProcessRejectedMessage()
	{
		var expectedMessageInterpretation = "<H3>Rejected Declaration</H3>" +
		"<H4>List of Errors:</H4>" +
		"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
		"<tr><td><strong>Code</strong></td><td><strong>Place</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
		"<tr><td>99</td><td>/PDI400/ImportOperation/LRN</td><td>(80111) LRN no es modificable</td><td>LSVPDIC016</td></tr>" +
		"<tr><td>98</td><td>/PDI400/ImportOperation/MRN</td><td>(80110) MRN no es modificable</td><td>&nbsp;</td></tr></table>";

		var message = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetRejectedTestFile(), InterchangeID);
		ProcessMessageForTest(message);
		GenericCommonAssertProcessEntryData(message, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageNum: MessageNum, messageSubType: EDIMessage.Status.Rejected, entryStatusCode: OriginalEntryStatus);
	}

	public void TestProcessErrorMessage()
	{
		var expectedMessageInterpretation = "<H3>Rejected Declaration</H3>" +
		"<H4>List of Errors:</H4>" +
		"<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
		"<tr><td><strong>Line / Column</strong></td><td><strong>Location</strong></td><td><strong>Code</strong></td><td><strong>Reason</strong></td><td><strong>Original Value</strong></td></tr>" +
		"<tr><td>1 / 0</td><td>/PDI400/CustomsOfficeOfImport/referenceNumber</td><td>51</td><td>El elemento no comple con el formato exigido. Patrón: [A-Z]{2}[A-Z0-9]{6}</td><td>ES0/09999</td></tr>" +
		"<tr><td>2 / 1</td><td>&nbsp;</td><td>52</td><td>El elemento no comple con el formato exigido. Patrón: [A-Z]{2}[A-Z0-9]{7}</td><td>&nbsp;</td></tr></table>";

		var responseMessage = CreateNewEDIMessage(entryHeader.CH_BGMReference, GetErrorTestFile(), InterchangeID);
		ProcessMessageForTest(responseMessage);
		GenericCommonAssertProcessEntryData(responseMessage, entryHeader, expectedMessageInterpretation: expectedMessageInterpretation, messageNum: MessageNum, messageSubType: EDIMessage.Status.Rejected, entryStatusCode: OriginalEntryStatus);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var newDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
		newDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
		var staffWithCertificateHelperTest = new StaffWithCertificateTestHelper(Factory);
		var staff = staffWithCertificateHelperTest.Staff;
		newDeclaration.JE_GS_NKCusAgent = staff.GS_Code;

		var declarant = Factory.NewWithValidTestData<OrgHeader>();
		declarant.OH_FullName = DeclarantName;
		declarant.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, DeclarantId);
		newDeclaration.Declarant.OA_OH = declarant.PK;

		var invoice = newDeclaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		var instruction = newDeclaration.CustomsEntryInstructions.AddNew();
		instruction.CEI_SubStyle = Declaration.EntrySubStyleList.Codes.A;
		invoiceLine.JI_CEI = instruction.PK;

		entryHeader = (CusEntryHeader)newDeclaration.ActiveEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = ApplicationReference;
		entryHeader.CH_CEI_Instruction = instruction.PK;
		entryHeader.CH_EntryStatus = OriginalEntryStatus;
		var entryLine = entryHeader.AllEntryLines.AddNew();
		entryLine.CL_LineNumber = 1;
		invoiceLine.JI_CL = entryLine.PK;
		SetSentInterchange(entryHeader, InterchangeID);

		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status");

		var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
		helper.CreateNewOrGetExistingDataGrouping(EsCode, parent: grouping);
		helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, OriginalEntryStatus, "Original Entry Status", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		helper.CreateNewOrGetExistingCusCodeList(EsCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "PDI", "Incomplete Pre-Declaration accepted", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
		Factory.Save();
	}

	protected const string MessageNum = "20241028154045238010";
	protected const string MRNCode = "24ES009998I0004SR7";
	protected const string ExportMRNCode = "25ES00999912345678";

	protected abstract string GetRejectedTestFile();
	protected abstract string GetErrorTestFile();
}
