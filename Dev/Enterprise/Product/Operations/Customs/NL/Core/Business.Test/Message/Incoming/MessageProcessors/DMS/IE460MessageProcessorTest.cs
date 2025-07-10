using System.Text;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business.Testing;

class IE460MessageProcessorTest : DMSMessageProcessorAbstractTest
{
	protected override string MessageSubType => NLIncomingMessageSubTypeList.Codes.CC460A;

	protected override string ExpectedMessageInterpretation
	{
		get
		{
			var sb = new StringBuilder();
			sb.Append("<font size='2' face='Courier New' ><H1>");
			sb.Append("Control Statement");
			sb.Append("</H1><BR/>");
			sb.Append("<table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>MRN:</b></td><td><i>MRN460</i></td></tr>");
			sb.Append("<tr><td><b>Functional Reference ID:</b></td><td><i>TestReference460</i></td></tr>");
			sb.Append("<tr><td><b>Statement Type:</b></td><td><i>Examination result comment</i></td></tr>");
			sb.Append("<tr><td><b>Statement Description:</b></td><td><i>STATEMENTDESC</i></td></tr>");
			sb.Append("</table>");
			sb.Append("<BR><H1>");
			sb.Append("Control Details");
			sb.Append("</H1>");
			sb.Append("<BR/><table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>Control Date:</b></td><td><i>20220217</i></td></tr>");
			sb.Append("<tr><td><b>Control Type:</b></td><td><i>Physical Inspection</i></td></tr>");
			sb.Append("<tr><td><b>Control Remarks:</b></td><td><i>STATEMENTDESC</i></td></tr>");
			sb.Append("</table>");
			sb.Append("<BR/><table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>Control Date:</b></td><td><i>20220217</i></td></tr>");
			sb.Append("<tr><td><b>Control Type:</b></td><td><i>Physical Inspection</i></td></tr>");
			sb.Append("<tr><td><b>Control Remarks:</b></td><td><i>STATEMENTDESC2</i></td></tr>");
			sb.Append("</table>");
			sb.Append("<BR><H1>");
			sb.Append("Requested Documents");
			sb.Append("</H1>");
			sb.Append("<BR/><table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>Requested Document:</b></td><td><i>DOCDESC1</i></td></tr>");
			sb.Append("<tr><td><b>Applies To:</b></td><td><i>Reference, Reference2</i></td></tr>");
			sb.Append("</table>");
			sb.Append("<BR/><table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>Requested Document:</b></td><td><i>DOCDESC2</i></td></tr>");
			sb.Append("<tr><td><b>Applies To:</b></td><td><i>Reference3</i></td></tr>");
			sb.Append("</table>");
			sb.Append("<BR/><table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>Requested Document:</b></td><td><i>DOCDESC3</i></td></tr>");
			sb.Append("<tr><td><b>Applies To:</b></td><td></td></tr>");
			sb.Append("</table>");

			return sb.ToString();
		}
	}

	protected override string TestMessageText => "<MetaData xsi:schemaLocation='urn:wco:datamodel:WCO:DMS.Response:1 DMS.Response_1p30.xsd' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns='urn:wco:datamodel:WCO:DMS.Response:1'><WCOTypeCode>CC460A</WCOTypeCode><CommunicationMetaData><ApplicationReferenceID>TestReferenceABC</ApplicationReferenceID><CommunicationsAgreementId>325656</CommunicationsAgreementId><Recipient><Id>00000001</Id></Recipient><Sender><Id>DMS.NL</Id></Sender></CommunicationMetaData><Response><AdditionalInformation><StatementTypeCode>BLF</StatementTypeCode></AdditionalInformation><Control><TypeCode>40</TypeCode><InspectionStartDateTime formatCode=\"102\">20220217</InspectionStartDateTime><AdditionalInformation><StatementDescription>STATEMENTDESC</StatementDescription></AdditionalInformation></Control><Control><TypeCode>40</TypeCode><InspectionStartDateTime formatCode=\"102\">20220217</InspectionStartDateTime><AdditionalInformation><StatementDescription>STATEMENTDESC2</StatementDescription></AdditionalInformation></Control><Status><EffectiveDateTime formatCode=\"304\">20220216172832Z</EffectiveDateTime></Status><RequestedDocument><Description>DOCDESC1</Description><TypeCode>TRA</TypeCode></RequestedDocument><RequestedDocument><Description>DOCDESC2</Description><TypeCode>REF</TypeCode></RequestedDocument><RequestedDocument><Description>DOCDESC3</Description><TypeCode>XXX</TypeCode></RequestedDocument><Declaration><FunctionalReferenceID>TestReference460</FunctionalReferenceID><ID>MRN460</ID></Declaration></Response></MetaData>";

	protected override DMSResponseMessageProcessor MessageProcessor => new IE460And560MessageProcessor(logger);

	ZDateTime EffectiveDateTimeZoneDependent
	{
		get
		{
			ZDateTime effectiveDateTime;
			ZDateTime.TryParseExact("20220216172832Z", out effectiveDateTime, "yyyyMMddHHmmssZ");
			return effectiveDateTime;
		}
	}

	public new void TestProcessMessage()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "TestReference460";
		var mrnEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbCompany.CurrentCompany.Country.Code);
		mrnEntryNumber.CE_EntryNum = "MRN460";

		var message = CreateNewTestMessage();
		message.EM_Status = NLEDIMessage.Status.PreProcessedOK;
		message.EM_LinkedObject = entryHeader;
		message.EM_MessageText = TestMessageText;

		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "Customs Status");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Netherlands, "Netherlands");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Netherlands, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, "340", "Controlled - Physical inspection", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));

		var instruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = instruction.PK;
		CreateSupportingDocument(instruction.SupportingDocuments, 1, "Reference", "TRA");
		CreateSupportingDocument(instruction.SupportingDocuments, 2, "Reference2", "TRA");
		CreateSupportingDocument(instruction.SupportingDocuments, 3, "Reference3", "REF");
		CreateSupportingDocument(instruction.SupportingDocuments, 4, "Reference4", "XXX");

		Factory.Save();

		MessageProcessor.ProcessMessage(message);

		CombineAssertions(() =>
		{
			AssertEquals("EDI Message - Message Status", NLEDIMessage.Status.ProcessedOK, message.EM_Status);
			AssertEquals("EntryHeadder - Message Status", ExpectedEntryHeaderStatus, entryHeader.CH_Status);
			AssertEquals("Entry Header - Entry Status", ExpectedEntryHeaderEntryStatus, entryHeader.CH_EntryStatus);
			AssertEquals("EDI Message - Message Interpretation", ExpectedMessageInterpretation, message.EM_MessageInterpretation);
			AssertEquals("Entry Header - Issue date", EffectiveDateTimeZoneDependent, entryHeader.CH_EntrySubmittedDate);
		});
	}

	static void CreateSupportingDocument(EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection supportingDocuments, ZShort itemNumber, ZString referenceNumber, ZString type)
	{
		var supportingDoc = supportingDocuments.AddNew();
		supportingDoc.CSI_ItemNumber = itemNumber;
		supportingDoc.CSI_ReferenceNumber = referenceNumber;
		supportingDoc.CSI_Type = type;
	}

	protected virtual string ExpectedEntryHeaderStatus => MessageStatuses.FYC;

	protected virtual string ExpectedEntryHeaderEntryStatus => EntryStatus.PhysicalInspection;
}
