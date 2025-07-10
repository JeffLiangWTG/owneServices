using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public class CDSErrorResponseMessageProcessorTests : TestCaseWithFactory
	{
		CDSErrorResponseEDIMessage ediMessage;

		void SetupForTest(bool amendmentTest = false)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_DeclarationReference = "B00000001";
			declaration.JE_HouseBill = "TESTHOUSE";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "123456";
			entry.CH_EntryStatus = "CES";

			var outgoingInterchange = Factory.New<EDIInterchange>();
			var sessionGuid = ZGuid.NewZGuid();
			outgoingInterchange.EI_SessionGUID = sessionGuid;

			if (amendmentTest)
			{
				var outgoingSentMessageNAM = Factory.New<CDSAmendmentComparisonEDIMessage>();
				outgoingSentMessageNAM.EM_MessageNum = "1";
				outgoingSentMessageNAM.EM_EI = outgoingInterchange.PK;
				outgoingSentMessageNAM.EM_Status = EDIMessage.Status.Sent;
				outgoingSentMessageNAM.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				entry.Messages.Add(outgoingSentMessageNAM);
			}

			outgoingInterchange.EI_From = "FROM";
			outgoingInterchange.EI_To = "TO";
			outgoingInterchange.EI_Status = EDIMessage.Status.Sent;
			outgoingInterchange.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
			var outgoingSentMessage = amendmentTest ? Factory.New<CDSAmendDeclarationEDIMessage>() : Factory.New<CDSEDIMessage>();
			outgoingSentMessage.EM_MessageNum = "2";
			outgoingSentMessage.EM_EI = outgoingInterchange.PK;
			outgoingSentMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingSentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			ediMessage = Factory.New<CDSErrorResponseEDIMessage>();
			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
			ediMessage.EM_EI = outgoingInterchange.PK;
			ediMessage.EM_ApplicationReference = outgoingSentMessage.PK.ToString().Replace("-", string.Empty);
			entry.Messages.Add(outgoingSentMessage);
			entry.Messages.Add(ediMessage);
			Factory.Save();
		}

		public void TestCredentialsFailure()
		{
			SetupForTest();
			ediMessage.EM_MessageText = @"<errorResponse><code>INVALID_CREDENTIALS</code><message>Invalid Authentication information provided</message></errorResponse>";

			var processor = new CDSErrorResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(ediMessage);
			CombineAssertions(() =>
			{
				AssertEquals("Incoming Message.EM_Status", EDIMessageStatusList.Codes.Received, ediMessage.EM_Status);
				AssertEquals("Message.EM_MessageInterpretation", @"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style>"
								+ @"<h3>Message 1 was rejected by an upstream system for the following reasons</h3>"
								+ @"<li>Invalid Authentication information provided</li>"
								+ new CDSErrorResponseMessageProcessor(new LoggingInformation()).htmlToExplainInvalidCredentials
								, ediMessage.EM_MessageInterpretation);
			});
		}

		public void TestProcessFirstLineFailureFromGateway()
		{
			SetupForTest();

			ediMessage.EM_MessageText = @"<errorResponse>
      <code>BAD_REQUEST</code>
      <message>Payload is not valid according to schema</message>
      <errors>
        <error>
          <code>xml_validation_error</code>
          <message>cvc-pattern-valid: Value '' is not facet-valid with respect to pattern '.*[^\s].*' for type '#AnonType_SupervisingOfficeIdentificationIDType'.</message>
        </error><error>
          <code>xml_validation_error</code>
          <message>cvc-complex-type.2.2: Element 'ID' must have no element [children], and the value must be valid.</message>
        </error>
		<error>
         <code>xml_validation_error</code>
         <message>cvc-pattern-valid: Value '' is not facet-valid with respect to pattern '[A-Z]{3}' for type 'ISO3AlphaCurrencyCodeContentType'.</message>
      </error>
      <error>
         <code>xml_validation_error</code>
         <message>cvc-attribute.3: The value '' of attribute 'currencyID' on element 'StatisticalValueAmount' is not valid with respect to its type, 'ISO3AlphaCurrencyCodeContentType'.</message>
      </error>
      <error>
         <code>xml_validation_error</code>
         <message>cvc-pattern-valid: We recognise the category but cannot parse the rest of it - do not explode.</message>
      </error>
      <error>
         <code>xml_validation_error</code>
         <message>whatever-category: We do not even recognise the category so have no idea what to expect here - but still do not explode.</message>
      </error>
      </errors>
    </errorResponse>";

			var processor = new CDSErrorResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(ediMessage);
			CombineAssertions(() =>
			{
				AssertEquals("Message.EM_Status", EDIMessageStatusList.Codes.Received, ediMessage.EM_Status);
				AssertEquals("Message.EM_MessageInterpretation", @"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style>"
								+ @"<h3>Message 1 was rejected by an upstream system for the following reasons</h3>"
								+ @"<li>Supervising office, coded - Value '' is not facet-valid with respect to pattern '.*[^\s].*'</li>"
								+ @"<li>Element 'ID' must have no element [children], and the value must be valid.</li>"
								+ @"<li>Value '' is not facet-valid with respect to pattern '[A-Z]{3}'</li>"
								+ @"<li>The value '' of attribute 'currencyID' on element 'StatisticalValueAmount' is not valid with respect to its type, 'ISO3AlphaCurrencyCodeContentType'.</li>"
								+ @"<li>We recognise the category but cannot parse the rest of it - do not explode.</li>"
								+ @"<li>We do not even recognise the category so have no idea what to expect here - but still do not explode.</li>", ediMessage.EM_MessageInterpretation);
			});
		}

		public void TestProcessForDeclarationTypeCodeType()
		{
			SetupForTest();

			ediMessage.EM_MessageText = @"<errorResponse>
      <code>BAD_REQUEST</code>
      <message>Payload is not valid according to schema</message>
      <errors>
        <error>
          <code>xml_validation_error</code>
          <message>cvc-pattern-valid: Value 'IM' is not facet-valid with respect to pattern '..A|..B|..C|..D|..E|..F|..Z|..X|..Y|GPR|COR' for type '#AnonType_DeclarationTypeCodeType'.</message>
        </error><error>
          <code>xml_validation_error</code>
          <message>cvc-complex-type.2.2: Element 'TypeCode' must have no element [children], and the value must be valid.</message>
        </error><error>
          <code>xml_validation_error</code>
          <message>cvc-enumeration-valid: Value 'CNT' is not facet-valid with respect to enumeration '[0, 1]'. It must be a value from the enumeration.</message>
        </error><error>
          <code>xml_validation_error</code>
          <message>cvc-complex-type.2.2: Element 'ContainerCode' must have no element [children], and the value must be valid.</message>
        </error>
      </errors>
    </errorResponse>
";

			var processor = new CDSErrorResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(ediMessage);
			CombineAssertions(() =>
			{
				AssertEquals("Message.EM_Status", EDIMessageStatusList.Codes.Received, ediMessage.EM_Status);
				AssertEquals("Message.EM_MessageInterpretation", @"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style>" +
@"<h3>Message 1 was rejected by an upstream system for the following reasons</h3>" +
@"<li>Document name, coded - Value 'IM' is not facet-valid with respect to pattern '..A|..B|..C|..D|..E|..F|..Z|..X|..Y|GPR|COR'</li>" +
@"<li>Element 'TypeCode' must have no element [children], and the value must be valid.</li>" +
@"<li>Value 'CNT' is not facet-valid with respect to enumeration '[0, 1]'. It must be a value from the enumeration.</li>" +
@"<li>Element 'ContainerCode' must have no element [children], and the value must be valid.</li>", ediMessage.EM_MessageInterpretation);
			});
		}

		public void TestProcessingOfInvalidXml()
		{
			SetupForTest();
			ediMessage.EM_MessageText = @"Invalid XML to process";

			var processor = new CDSErrorResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(ediMessage);
			AssertEquals("Message.EM_Status", EDIMessageStatusList.Codes.Failed, ediMessage.EM_Status);
			AssertContains("Exception processing should display friendly interpretation heading", @"Message 1 was rejected by an upstream system for the following reasons", ediMessage.EM_MessageInterpretation);
			AssertContains("Exception processing should display friendly interpretation error heading", @"Error reading xml", ediMessage.EM_MessageInterpretation);
			AssertContains("Exception processing should display friendly interpretation exception datails", @"Data at the root level is invalid. Line 1, position 1.", ediMessage.EM_MessageInterpretation);
			AssertContains("Exception processing should display friendly interpretation help text", @"Please refer to the Message Text tab for the original content.", ediMessage.EM_MessageInterpretation);
		}

		public void TestProcessingUsingMessageNum()
		{
			CDSErrorResponseEDIMessage cdsEDIMessage2;
			CDSErrorResponseEDIMessage cdsEDIMessage3;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_DeclarationReference = "B00000001";
			declaration.JE_HouseBill = "TESTHOUSE";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.EntryNumber = "123456";
			entry.CH_EntryStatus = "CES";

			var outgoingInterchange2 = Factory.New<EDIInterchange>();
			var sessionGuid2 = ZGuid.NewZGuid();
			outgoingInterchange2.EI_SessionGUID = sessionGuid2;

			outgoingInterchange2.EI_From = "FROM";
			outgoingInterchange2.EI_To = "TO";
			outgoingInterchange2.EI_Status = EDIMessage.Status.Sent;
			outgoingInterchange2.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;

			var outgoingSentMessage2 = Factory.New<CDSEDIMessage>();
			outgoingSentMessage2.EM_EI = outgoingInterchange2.PK;
			outgoingSentMessage2.EM_Status = EDIMessage.Status.Sent;
			outgoingSentMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			cdsEDIMessage2 = Factory.New<CDSErrorResponseEDIMessage>();
			cdsEDIMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			cdsEDIMessage2.EM_Status = EDIMessageStatusList.Codes.Queued;
			cdsEDIMessage2.EM_EI = outgoingInterchange2.PK;
			cdsEDIMessage2.EM_SystemCreateTimeUtc = new ZDateTime(2022, 06, 09, 15, 00, 00);

			entry.Messages.Add(outgoingSentMessage2);
			entry.Messages.Add(cdsEDIMessage2);

			var outgoingInterchange3 = Factory.New<EDIInterchange>();
			var sessionGuid3 = ZGuid.NewZGuid();
			outgoingInterchange3.EI_SessionGUID = sessionGuid3;

			outgoingInterchange3.EI_From = "FROM";
			outgoingInterchange3.EI_To = "TO";
			outgoingInterchange3.EI_Status = EDIMessage.Status.Sent;
			outgoingInterchange3.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;

			var outgoingSentMessage3 = Factory.New<CDSEDIMessage>();
			outgoingSentMessage3.EM_EI = outgoingInterchange3.PK;
			outgoingSentMessage3.EM_Status = EDIMessage.Status.Sent;
			outgoingSentMessage3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			cdsEDIMessage3 = Factory.New<CDSErrorResponseEDIMessage>();
			cdsEDIMessage3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			cdsEDIMessage3.EM_Status = EDIMessageStatusList.Codes.Queued;
			cdsEDIMessage3.EM_EI = outgoingInterchange3.PK;
			cdsEDIMessage3.EM_SystemCreateTimeUtc = new ZDateTime(2022, 06, 08, 15, 00, 00);

			entry.Messages.Add(outgoingSentMessage3);
			entry.Messages.Add(cdsEDIMessage3);

			Factory.Save();
			outgoingSentMessage2.EM_MessageNum = "222";
			outgoingSentMessage3.EM_MessageNum = "333";
			cdsEDIMessage2.EM_MessageNum = "222E";
			cdsEDIMessage3.EM_MessageNum = "333E";
			cdsEDIMessage2.EM_ApplicationReference = outgoingSentMessage2.PK.ToString().Replace("-", string.Empty);
			cdsEDIMessage3.EM_ApplicationReference = outgoingSentMessage3.PK.ToString().Replace("-", string.Empty);

			cdsEDIMessage2.EM_MessageText = @"<errorResponse><code>INVALID_CREDENTIALS</code><message>Invalid Authentication information provided</message></errorResponse>";
			cdsEDIMessage3.EM_MessageText = @"<errorResponse><code>INVALID_CREDENTIALS</code><message>Invalid Authentication information provided</message></errorResponse>";

			var processor = new CDSErrorResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(cdsEDIMessage2);
			processor.ProcessMessage(cdsEDIMessage3);
			CombineAssertions(() =>
			{
				AssertContains("Message 222 was rejected", cdsEDIMessage2.EM_MessageInterpretation);
				AssertContains("Message 333 was rejected", cdsEDIMessage3.EM_MessageInterpretation);
			});
		}

		public void TestProcessingDuringAmendment()
		{
			SetupForTest(true);
			ediMessage.EM_MessageText = @"<errorResponse><code>INVALID_CREDENTIALS</code><message>Invalid Authentication information provided</message></errorResponse>";

			var processor = new CDSErrorResponseMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(ediMessage);
			CombineAssertions(() =>
			{
				AssertEquals("Incoming Message.EM_Status", EDIMessageStatusList.Codes.Received, ediMessage.EM_Status);
				AssertEquals("Message.EM_MessageInterpretation", @"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style>"
								+ @"<h3>Message 2 was rejected by an upstream system for the following reasons</h3>"
								+ @"<li>Invalid Authentication information provided</li>"
								+ new CDSErrorResponseMessageProcessor(new LoggingInformation()).htmlToExplainInvalidCredentials
								, ediMessage.EM_MessageInterpretation);
			});
		}
	}
}
