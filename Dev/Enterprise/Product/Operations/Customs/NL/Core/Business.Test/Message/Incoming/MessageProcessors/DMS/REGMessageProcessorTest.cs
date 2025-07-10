using System.Text;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class REGMessageProcessorTest : DMSMessageProcessorAbstractTest
{
	public new void TestProcessMessage()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = BGMReference;

		var message = CreateNewTestMessage();
		message.EM_Status = NLEDIMessage.Status.PreProcessedOK;
		message.EM_LinkedObject = entryHeader;
		message.EM_MessageText = TestMessageText;

		Factory.Save();

		MessageProcessor.ProcessMessage(message);

		var mrnEntryNumber = CusEntryNumber.Load(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbCompany.CurrentCompany.Country.Code);

		CombineAssertions(() =>
		{
			AssertEquals("EDI Message - Message Status", NLEDIMessage.Status.ProcessedOK, message.EM_Status);
			AssertEquals("Entry Header - Message Status", NLConstants.Status.Received, entryHeader.CH_Status);
			AssertEquals("Entry Header - Entry Status", NLConstants.EntryStatus.AdvanceDeclarationReceived, entryHeader.CH_EntryStatus);
			AssertEquals("EDI Message - Message Interpretation", ExpectedMessageInterpretation, message.EM_MessageInterpretation);
			AssertEquals("Entry Number - CE_ExpiryDate", new ZDateTime(2020, 07, 12), mrnEntryNumber.CE_ExpiryDate);
		});
	}

	protected override string ExpectedMessageInterpretation
	{
		get
		{
			var sb = new StringBuilder();
			sb.Append("<table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>Registration date:</b></td><td><i>" + RegistrationDateTimeZoneDependent + "</i></td></tr></table></font>");
			sb.Append("<BR><H1>Errors reported by customs</H1><BR/><table style='margin-left: 10pt'>");
			sb.Append("<tr><td><b>Error Validation:</b></td><td><i>ValidationCode</i></td></tr>");
			sb.Append("<tr><td><b>Error Description:</b></td><td><i>Description</i></td></tr>");
			sb.Append("<tr><td><b>Value that is rejected:</b></td><td><i>OriginalAttributeValue</i></td></tr>");
			sb.Append("<tr><td><b>Applies To:</b></td><td><i>Location</i></td></tr>");
			sb.Append("</table></font>");

			return sb.ToString();
		}
	}

	protected override string MessageSubType => NLIncomingMessageSubTypeList.Codes.CCREGA;

	protected override string TestMessageText => "<MetaData xsi:schemaLocation='urn:wco:datamodel:WCO:DMS.Response:1 DMS.Response_1p30.xsd' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns='urn:wco:datamodel:WCO:DMS.Response:1'><WCOTypeCode>CCREGA</WCOTypeCode><CommunicationMetaData><ApplicationReferenceID>TestReferenceREG</ApplicationReferenceID><CommunicationsAgreementID>325656</CommunicationsAgreementID><Recipient><ID>00000001</ID></Recipient><Sender><ID>DMS.NL</ID></Sender></CommunicationMetaData><Response><Status><EffectiveDateTime formatCode='304'>20200723143432Z</EffectiveDateTime></Status><Declaration><AcceptanceDateTime>20200612</AcceptanceDateTime><IssueDateTime>20220112143432Z</IssueDateTime><FunctionalReferenceID>TestReferenceREG</FunctionalReferenceID><ID>22NL13215444</ID></Declaration><Error><Description>Description</Description><ValidationCode>ValidationCode</ValidationCode><OriginalAttributeValue>OriginalAttributeValue</OriginalAttributeValue><Pointer><Location>Location</Location></Pointer></Error></Response></MetaData>";

	protected override string BGMReference => "TestReferenceREG";

	protected override DMSResponseMessageProcessor MessageProcessor => new REGMessageProcessor(logger);

	ZDateTime RegistrationDateTimeZoneDependent
	{
		get
		{
			ZDateTime registrationDate;
			ZDateTime.TryParseExact("20200723143432Z", out registrationDate, "yyyyMMddHHmmssZ");
			return registrationDate;
		}
	}
}
