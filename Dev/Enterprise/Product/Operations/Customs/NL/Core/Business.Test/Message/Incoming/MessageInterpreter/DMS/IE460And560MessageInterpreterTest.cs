using System;
using System.Text;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using static Enterprise.Customs.NL.Business.Common.NLConstants;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(IE460And560MessageInterpreter))]
sealed class IE460And560MessageInterpreterTest : MessageInterpreterTestCase<IE460And560MessageInterpreter, IDMSIncomingDataProvider>
{
	public override string ExpectedMessageInterpretation
	{
		get
		{
			var sb = new StringBuilder();
			sb.Append("<font size='2' face='Courier New' ><H1>Control Statement</H1><BR/>");
			sb.Append("<table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>MRN:</b></td><td><i>MRN460</i></td></tr>");
			sb.Append("<tr><td><b>Functional Reference ID:</b></td><td><i>TestReference460</i></td></tr>");
			sb.Append("<tr><td><b>Statement Type:</b></td><td><i>Examination result comment</i></td></tr>");
			sb.Append("<tr><td><b>Statement Description:</b></td><td><i>Control Additional Info Statement Description</i></td></tr></table>");
			sb.Append("<BR><H1>Control Details</H1><BR/><table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>Control Date:</b></td><td><i>20220217</i></td></tr>");
			sb.Append("<tr><td><b>Control Type:</b></td><td><i>Physical Inspection</i></td></tr>");
			sb.Append("<tr><td><b>Control Remarks:</b></td><td><i>Control Additional Info Statement Description</i></td></tr></table>");
			sb.Append("<BR/><table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>Control Date:</b></td><td><i>20220228</i></td></tr>");
			sb.Append("<tr><td><b>Control Type:</b></td><td><i>Other</i></td></tr>");
			sb.Append("<tr><td><b>Control Remarks:</b></td><td><i>Control Additional Info Statement Description2</i></td></tr></table>");
			sb.Append("<BR><H1>Requested Documents</H1><BR/><table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>Requested Document:</b></td><td><i>DOCDESC1</i></td></tr>");
			sb.Append("<tr><td><b>Applies To:</b></td><td><i>Reference, Reference2</i></td></tr></table>");
			sb.Append("<BR/><table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>Requested Document:</b></td><td><i>DOCDESC2</i></td></tr>");
			sb.Append("<tr><td><b>Applies To:</b></td><td><i>Reference3</i></td></tr></table>");
			sb.Append("<BR/><table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>Requested Document:</b></td><td><i>DOCDESC3</i></td></tr>");
			sb.Append("<tr><td><b>Applies To:</b></td><td></td></tr>");
			sb.Append("</table>");

			return sb.ToString();
		}
	}

	protected override void SetUp()
	{
		DataProviderMock.Setup(x => x.WCOTypeCode).Returns(WCoTypeCodes.ControlNotification);
		DataProviderMock.Setup(x => x.Declaration.Id).Returns("MRN460");
		DataProviderMock.Setup(x => x.Declaration.FunctionalReference).Returns("TestReference460");

		var additionalInformation = DMSResponseMessageTestHelper.MockResponseAdditionalInformation(ResponseStatementTypes.Codes.ExaminationResultComment, "Statement Description").Object;
		DataProviderMock.Setup(x => x.AdditionalInformations).Returns(new IDMSAdditionalInformation[] { additionalInformation });

		var control = DMSResponseMessageTestHelper.MockResponseControl(null, "40", new DateTime(2022, 02, 17), "Control Additional Info Statement Description").Object;
		var control2 = DMSResponseMessageTestHelper.MockResponseControl(null, "50", new DateTime(2022, 02, 28), "Control Additional Info Statement Description2").Object;
		DataProviderMock.Setup(x => x.Controls).Returns(new IDMSControl[] { control, control2 });

		var document = DMSResponseMessageTestHelper.MockResponseRequestedDocument("TRA", "DOCDESC1").Object;
		var document2 = DMSResponseMessageTestHelper.MockResponseRequestedDocument("REF", "DOCDESC2").Object;
		var document3 = DMSResponseMessageTestHelper.MockResponseRequestedDocument("XXX", "DOCDESC3").Object;
		DataProviderMock.Setup(x => x.RequestedDocuments).Returns(new IDMSRequestedDocument[] { document, document2, document3 });

		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = instruction.PK;
		CreateSupportingDocument(instruction.SupportingDocuments, 1, "Reference", "TRA");
		CreateSupportingDocument(instruction.SupportingDocuments, 2, "Reference2", "TRA");
		CreateSupportingDocument(instruction.SupportingDocuments, 3, "Reference3", "REF");
		CreateSupportingDocument(instruction.SupportingDocuments, 4, "Reference4", "XXX");
		MessageMock.Object.EM_LinkedObject = entryHeader;
		MessageMock.Object.EM_MessageType = "DMS";
		MessageMock.Object.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		MessageMock.Object.EM_MessageText = "<MetaData xsi:schemaLocation='urn:wco:datamodel:WCO:DMS.Response:1 DMS.Response_1p30.xsd' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns='urn:wco:datamodel:WCO:DMS.Response:1'><WCOTypeCode>CC460A</WCOTypeCode><CommunicationMetaData><ApplicationReferenceID>TestReferenceABC</ApplicationReferenceID><CommunicationsAgreementID>325656</CommunicationsAgreementID><Recipient><ID>00000001</ID></Recipient><Sender><ID>DMS.NL</ID></Sender></CommunicationMetaData><Response><AdditionalInformation><StatementTypeCode>BLF</StatementTypeCode></AdditionalInformation><Control><TypeCode>40</TypeCode><InspectionStartDateTime formatCode=\"102\">20220217</InspectionStartDateTime><AdditionalInformation><StatementDescription>STATEMENTDESC</StatementDescription></AdditionalInformation></Control><Control><TypeCode>40</TypeCode><InspectionStartDateTime formatCode=\"102\">20220217</InspectionStartDateTime><AdditionalInformation><StatementDescription>STATEMENTDESC2</StatementDescription></AdditionalInformation></Control><Status><EffectiveDateTime formatCode=\"304\">20220216172832Z</EffectiveDateTime></Status><RequestedDocument><Description>DOCDESC1</Description><TypeCode>TRA</TypeCode></RequestedDocument><RequestedDocument><Description>DOCDESC2</Description><TypeCode>REF</TypeCode></RequestedDocument><RequestedDocument><Description>DOCDESC3</Description><TypeCode>XXX</TypeCode></RequestedDocument><Declaration><FunctionalReferenceID>TestReference460</FunctionalReferenceID><ID>MRN460</ID></Declaration></Response></MetaData>";
	}

	static void CreateSupportingDocument(EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection supportingDocuments, ZShort itemNumber, ZString referenceNumber, ZString type)
	{
		var supportingDoc = supportingDocuments.AddNew();
		supportingDoc.CSI_ItemNumber = itemNumber;
		supportingDoc.CSI_ReferenceNumber = referenceNumber;
		supportingDoc.CSI_Type = type;
	}
}
