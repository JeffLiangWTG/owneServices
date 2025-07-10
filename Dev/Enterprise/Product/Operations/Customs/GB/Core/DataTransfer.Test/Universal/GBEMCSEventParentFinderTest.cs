using System;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.CDS;
using Enterprise.Customs.GB.EMCS.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;

namespace Enterprise.Customs.GB.DataTransfer.Universal.Testing
{
	sealed class GBEMCSEventParentFinderTest : TestCaseWithFactory
	{
		public void TestUpdateFromXmlRejectionMessage()
		{
			AssertDeclarationAndNewRejectionMessage(XmlFailureResponseTextDecoded);
		}

		public void TestUpdateFromJsonRejectionMessage()
		{
			AssertDeclarationAndNewRejectionMessage(JsonFailureResponseTextDecoded);
		}

		public void TestUpdateFromJsonRejectionMessageWithoutErrorDetails()
		{
			AssertDeclarationAndNewRejectionMessage(JsonFailureResponseTextWithoutErrorDetailsDecoded);
		}

		void AssertDeclarationAndNewRejectionMessage(string responseTextDecoded)
		{
			var sessionGuid = new ZGuid("1DEDA5DE-495A-4874-A824-B118F03C2C44");
			_ = SetupOutgoingMessage(Factory, sessionGuid);
			var logParents = ProcessMessage(Factory, string.Format(XmlFailureResponseEvent, Convert.ToBase64String(Encoding.UTF8.GetBytes(responseTextDecoded))));
			var declaration = logParents[0];
			CombineAssertions(() =>
			{
				AssertEquals(EDIMessage.Status.Failed, declaration.Messages[0].EM_Status);
				AssertEquals("New rejection message created", 2, declaration.Messages.Count);

				var newRejectionMessage = declaration.Messages[1];
				AssertEquals(EDIMessage.Direction.Receive, newRejectionMessage.EM_ReceiveTransmit);
				AssertEquals(CDSEDIMessageTypeList.Codes.EHubErrorResponse, newRejectionMessage.EM_MessageType);
				AssertEquals(EDIMessage.Status.ProcessedOK, newRejectionMessage.EM_Status);
				AssertEquals(responseTextDecoded, newRejectionMessage.EM_MessageText);
				AssertNotNullOrEmpty(newRejectionMessage.EM_MessageInterpretation);
				AssertEquals("Declaration MessageStatus", EDIMessageStatusList.Codes.Rejected, declaration.JE_MessageStatus);
			});
		}

		#region Reject response messages

		static readonly ZString XmlFailureResponseEvent = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
  <Event>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>EMCSJobDeclaration</Type>
          <Key>E00000944</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <EventTime>2023-05-22T12:59:28+10:00</EventTime>
    <EventType>MRJ</EventType>
    <DataContext>
      <DataSource>
        <DataProvider>EMCS</DataProvider>
      </DataSource>
    </DataContext>
    <ContextCollection>
      <Context>
        <Type>eHubTrackingID</Type>
        <Value>1DEDA5DE-495A-4874-A824-B118F03C2C44</Value>
      </Context>
      <Context>
        <Type>Error</Type>
        <Value>schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema; schema</Value>
      </Context>
      <Context>
        <Type>ErrorSummary</Type>
        <Value>Message not accepted</Value>
      </Context>
      <Context>
        <Type>ResponseText</Type>
        <Value>{0}</Value>
      </Context>
    </ContextCollection>
  </Event>
</UniversalEvent>";
		static readonly string XmlFailureResponseTextDecoded = @"<soapenv:Envelope xmlns:soapenv=""http://www.w3.org/2003/05/soap-envelope"">
  <soapenv:Body>
    <HMRCSOAPResponse xmlns=""http://www.inlandrevenue.gov.uk/SOAP/Response/2"">
      <ErrorResponse xmlns=""http://www.govtalk.gov.uk/CM/errorresponse"" SchemaVersion=""2.0"">
        <Application>
          <MessageCount>15</MessageCount> 
        </Application>
        <Error>
          <RaisedBy>ChRIS</RaisedBy> 
          <Number>4052</Number> 
          <Type>schema</Type> 
          <Text>Element 'q1:SealInformation' must only have valid text as its content</Text> 
          <Location>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:BodyEadEsad[1]/ie:Package[1]/ie:SealInformation[1]</Location> 
          <Application>
            <Messages>
              <DeveloperMessage>cvc-complex-type.2.2: Element 'q1:SealInformation' must have no element [children], and the value must be valid.</DeveloperMessage> 
            </Messages>
          </Application>
        </Error>
        <Error>
          <RaisedBy>ChRIS</RaisedBy> 
          <Number>4001</Number> 
          <Type>schema</Type> 
          <Text>Missing attribute on element 'q1:SealInformation'</Text> 
          <Location>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:BodyEadEsad[1]/ie:Package[1]/ie:SealInformation[1]/@language</Location> 
          <Application>
            <Messages>
              <DeveloperMessage>cvc-complex-type.4: Attribute 'language' must appear on element 'q1:SealInformation'.</DeveloperMessage> 
            </Messages>
          </Application>
        </Error>
        <Error>
          <RaisedBy>ChRIS</RaisedBy> 
          <Number>4085</Number> 
          <Type>schema</Type> 
          <Text>Value '' doesn't have the correct format</Text> 
          <Location>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:BodyEadEsad[1]/ie:Package[1]/ie:SealInformation[1]</Location> 
          <Application>
            <Messages>
              <DeveloperMessage>cvc-pattern-valid: Value '' is not facet-valid with respect to pattern '.{1,350}' for type 'SealInformationType'.</DeveloperMessage> 
            </Messages>
          </Application>
        </Error>
        <Error>
          <RaisedBy>ChRIS</RaisedBy> 
          <Number>4085</Number> 
          <Type>schema</Type> 
          <Text>Value '' doesn't have the correct format</Text> 
          <Location>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:BodyEadEsad[1]/ie:Package[1]/ie:CommercialSealIdentification[1]</Location> 
          <Application>
            <Messages>
              <DeveloperMessage>cvc-pattern-valid: Value '' is not facet-valid with respect to pattern '.{1,35}' for type 'CommercialSealIdentificationType'.</DeveloperMessage> 
            </Messages>
          </Application>
        </Error>
        <Error>
          <RaisedBy>ChRIS</RaisedBy> 
          <Number>4065</Number> 
          <Type>schema</Type> 
          <Text>Invalid content found at element 'q1:CommercialSealIdentification'</Text> 
          <Location>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:BodyEadEsad[1]/ie:Package[1]/ie:CommercialSealIdentification[1]</Location> 
          <Application>
            <Messages>
              <DeveloperMessage>cvc-type.3.1.3: The value '' of element 'q1:CommercialSealIdentification' is not valid.</DeveloperMessage> 
            </Messages>
          </Application>
        </Error>
        <Error>
          <RaisedBy>ChRIS</RaisedBy> 
          <Number>4065</Number> 
          <Type>schema</Type> 
          <Text>Invalid content found at element 'q1:Quantity'</Text> 
          <Location>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:BodyEadEsad[1]/ie:Quantity[1]</Location> 
          <Application>
            <Messages>
              <DeveloperMessage>cvc-type.3.1.3: The value '0' of element 'q1:Quantity' is not valid.</DeveloperMessage> 
            </Messages>
          </Application>
        </Error>
        <Error>
          <RaisedBy>ChRIS</RaisedBy> 
          <Number>4085</Number> 
          <Type>schema</Type> 
          <Text>Value '0' doesn't have the correct format</Text> 
          <Location>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:BodyEadEsad[1]/ie:Quantity[1]</Location> 
          <Application>
            <Messages>
              <DeveloperMessage>cvc-pattern-valid: Value '0' is not facet-valid with respect to pattern '[1-9]\d{0,14}|([1-9]\d{0,13}|0)\.[0-9]|([1-9]\d{0,12}|0)\.\d[0-9]|([1-9]\d{0,11}|0)\.\d\d[0-9]' for type 'QuantityType'.</DeveloperMessage> 
            </Messages>
          </Application>
        </Error>
        <Error>
          <RaisedBy>ChRIS</RaisedBy> 
          <Number>4065</Number> 
          <Type>schema</Type> 
          <Text>Invalid content found at element 'q1:FirstTransporterTrader'</Text> 
          <Location>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:FirstTransporterTrader[1]</Location> 
          <Application>
            <Messages>
              <DeveloperMessage>cvc-complex-type.2.4.a: Invalid content was found starting with element 'q1:FirstTransporterTrader'. One of '{""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE815:V3.13"":DeliveryPlaceCustomsOffice, ""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE815:V3.13"":CompetentAuthorityDispatchOffice}' is expected.</DeveloperMessage> 
            </Messages>
          </Application>
        </Error>
        <Error>
          <RaisedBy>ChRIS</RaisedBy> 
          <Number>4065</Number> 
          <Type>schema</Type> 
          <Text>Invalid content found at element 'q1:ReferenceOfTaxWarehouse'</Text> 
          <Location>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:PlaceOfDispatchTrader[1]/ie:ReferenceOfTaxWarehouse[1]</Location> 
          <Application>
            <Messages>
              <DeveloperMessage>cvc-type.3.1.3: The value '69' of element 'q1:ReferenceOfTaxWarehouse' is not valid.</DeveloperMessage> 
            </Messages>
          </Application>
        </Error>
        <Error>
          <RaisedBy>ChRIS</RaisedBy> 
          <Number>4085</Number> 
          <Type>schema</Type> 
          <Text>Value '69' doesn't have the correct format</Text> 
          <Location>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:PlaceOfDispatchTrader[1]/ie:ReferenceOfTaxWarehouse[1]</Location> 
          <Application>
            <Messages>
              <DeveloperMessage>cvc-pattern-valid: Value '69' is not facet-valid with respect to pattern '[A-Z]{2}[a-zA-Z0-9]{11}' for type 'ExciseNumberType'.</DeveloperMessage> 
            </Messages>
          </Application>
        </Error>
        <Error>
          <RaisedBy>ChRIS</RaisedBy> 
          <Number>4065</Number> 
          <Type>schema</Type> 
          <Text>Invalid content found at element 'q1:TraderExciseNumber'</Text> 
          <Location>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:ConsignorTrader[1]/ie:TraderExciseNumber[1]</Location> 
          <Application>
            <Messages>
              <DeveloperMessage>cvc-type.3.1.3: The value '1234' of element 'q1:TraderExciseNumber' is not valid.</DeveloperMessage> 
            </Messages>
          </Application>
        </Error>
        <Error>
          <RaisedBy>ChRIS</RaisedBy> 
          <Number>4085</Number> 
          <Type>schema</Type> 
          <Text>Value '1234' doesn't have the correct format</Text> 
          <Location>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Body[1]/ie:SubmittedDraftOfEADESAD[1]/ie:ConsignorTrader[1]/ie:TraderExciseNumber[1]</Location> 
          <Application>
            <Messages>
              <DeveloperMessage>cvc-pattern-valid: Value '1234' is not facet-valid with respect to pattern '[A-Z]{2}[a-zA-Z0-9]{11}' for type 'ExciseNumberType'.</DeveloperMessage> 
            </Messages>
          </Application>
        </Error>
        <Error>
          <RaisedBy>ChRIS</RaisedBy> 
          <Number>4065</Number> 
          <Type>schema</Type> 
          <Text>Invalid content found at element 'MessageIdentifier'</Text> 
          <Location>/tns:Envelope[1]/tns:Body[1]/ie:IE815[1]/ie:Header[1]/tms:MessageIdentifier[1]</Location> 
          <Application>
            <Messages>
              <DeveloperMessage>cvc-complex-type.2.4.a: Invalid content was found starting with element 'MessageIdentifier'. One of '{""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13"":TimeOfPreparation}' is expected.</DeveloperMessage> 
            </Messages>
          </Application>
        </Error>
        <Error>
          <RaisedBy>ChRIS</RaisedBy> 
          <Number>4065</Number> 
          <Type>schema</Type> 
          <Text>Invalid content found at element 'h:ConsignorId'</Text> 
          <Location>/tns:Envelope[1]/tns:Header[1]/emcs-info-header:EMCSInfo[1]/emcs-info-header:ConsignorId[1]</Location> 
          <Application>
            <Messages>
              <DeveloperMessage>cvc-type.3.1.3: The value '123' of element 'h:ConsignorId' is not valid.</DeveloperMessage> 
            </Messages>
          </Application>
        </Error>
        <Error>
          <RaisedBy>ChRIS</RaisedBy> 
          <Number>4085</Number> 
          <Type>schema</Type> 
          <Text>Value '123' doesn't have the correct format</Text> 
          <Location>/tns:Envelope[1]/tns:Header[1]/emcs-info-header:EMCSInfo[1]/emcs-info-header:ConsignorId[1]</Location> 
          <Application>
            <Messages>
              <DeveloperMessage>cvc-pattern-valid: Value '123' is not facet-valid with respect to pattern '[A-Z]{2}[a-zA-Z0-9]{11}' for type '#AnonType_ConsignorIdEMCSInfo'.</DeveloperMessage> 
            </Messages>
          </Application>
        </Error>
      </ErrorResponse>
    </HMRCSOAPResponse>
  </soapenv:Body>
</soapenv:Envelope>";
		static readonly string JsonFailureResponseTextDecoded = @"{
    ""dateTime"": ""2023-12-06T11:34:13.191992"",
    ""debugMessage"": ""Error while parsing &lt;q1:Header xmlns:q1=\""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE815:V3.13\""&gt;&lt;MessageSender xmlns=\""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13\""&gt;NDEA.GB&lt;/MessageSender&gt;&lt;MessageRecipient xmlns=\""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13\""&gt;NDEA.GB&lt;/MessageRecipient&gt;&lt;DateOfPreparation xmlns=\""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13\""&gt;2023-12-05&lt;/DateOfPreparation&gt;&lt;MessageIdentifier xmlns=\""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13\""&gt;48&lt;/MessageIdentifier&gt;&lt;CorrelationIdentifier xmlns=\""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13\""&gt;48&lt;/CorrelationIdentifier&gt;&lt;TimeOfPreparation xmlns=\""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13\""&gt;10:59:21&lt;/TimeOfPreparation&gt;&lt;/q1:Header&gt;: parser error \""'{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}TimeOfPreparation' expected but {urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}MessageIdentifier found\"" while parsing /{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE815:V3.13}IE815/{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE815:V3.13}Header/{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}MessageSender{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}MessageRecipient{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}DateOfPreparation{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}MessageIdentifier{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}CorrelationIdentifier{urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13}TimeOfPreparation\n   ^"",
    ""message"": ""Not valid IE815 message"",
	""errors"": [
        {
            ""errorCode"": 8081,
            ""errorMessage"": ""The Identity of Transport Units must be entered if Transport Unit Code is not fixed transport installation. Please amend your entry and resubmit."",
            ""location"": ""/q1:IE815[1]/q1:Body[1]/q1:SubmittedDraftOfEADESAD[1]/q1:TransportDetails[1]"",
            ""value"": null
        },
        {
            ""errorCode"": 8082,
            ""errorMessage"": ""The Identity of Transport Units must not be entered if Transport Unit Code is not fixed transport installation. Please amend your entry and resubmit."",
            ""location"": ""/q1:IE815[1]/q1:Body[1]/q1:SubmittedDraftOfEADESAD[1]/q1:TransportDetails[1]"",
            ""value"": ""Unknown unit""
        }
    ]
}";
		static readonly string JsonFailureResponseTextWithoutErrorDetailsDecoded = @"{
    ""dateTime"": ""2024-07-12T09:15:16.216Z"",
    ""message"": ""Message cannot be sent"",
    ""debugMessage"": ""The Consignor is not authorised to submit this message for the movement"",
    ""errors"": null
}";

		#endregion

		public void TestGetLogParentsForEvent()
		{
			var sessionGuid = new ZGuid("1DEDA5DE-495A-4874-A824-B118F03C2C44");
			var declaration = SetupOutgoingMessage(Factory, sessionGuid);
			var logParents = ProcessMessage(Factory, CreateSuccessfulResponseXml(sessionGuid));
			AssertEquals(1, logParents.Length);
			var dec = logParents[0];
			AssertEquals(declaration, dec);

			logParents = ProcessMessage(Factory, CreateSuccessfulResponseXmlWithoutDataTarget(sessionGuid));
			AssertEquals(1, logParents.Length);
			dec = logParents[0];
			AssertEquals(declaration, dec);

			logParents = ProcessMessage(Factory, CreateSuccessfulResponseXml(ZGuid.NewZGuid()));
			AssertNull("No matching eHubTrackingID", logParents);

			logParents = ProcessMessage(Factory, CreateSuccessfulResponseXmlWithoutDataTarget(ZGuid.NewZGuid()));
			AssertNull("No matching eHubTrackingID", logParents);
		}

		public void TestUpdateEntryWithAcknowledgeStatus()
		{
			var sessionGuid = new ZGuid("1DEDA5DE-495A-4874-A824-B118F03C2C44");
			_ = SetupOutgoingMessage(Factory, sessionGuid);
			var logParents = ProcessMessage(Factory, CreateSuccessfulResponseXml(sessionGuid));
			var dec = logParents[0];
			AssertEquals(EDIMessage.Status.Acknowledged, dec.Messages[0].EM_Status);
			AssertEquals("Message acknowledged - No new message created", 1, dec.Messages.Count);
			AssertEquals(EDIMessage.Status.Acknowledged, dec.JE_MessageStatus);
		}

		public void TestUpdateEntryWithRejectStatus()
		{
			var sessionGuid = new ZGuid("1DEDA5DE-495A-4874-A824-B118F03C2C44");
			_ = SetupOutgoingMessage(Factory, sessionGuid);
			var logParents = ProcessMessage(Factory, CreateRejectedResponseXml(sessionGuid));
			var dec = logParents[0];
			AssertEquals(EDIMessage.Status.Failed, dec.Messages[0].EM_Status);
			AssertEquals("Message rejected and new message created", 2, dec.Messages.Count);
		}

		public void TestEventNotProcessedByJobDeclarationEventParentFinder()
		{
			var sessionGuid = new ZGuid("1DEDA5DE-495A-4874-A824-B118F03C2C44");
			var declaration = SetupOutgoingMessage(Factory, sessionGuid);
			var logParents = ProcessCDSMessage(CreateSuccessfulResponseXml(sessionGuid));
			AssertNull("JobDeclarationEventParentFinder should not process EMCS messages", logParents);
			AssertEquals(EDIMessage.Status.Sent, declaration.Messages[0].EM_Status);
		}

		public void TestUpdateFromPVTResponseValid()
		{
			var sessionGuid = new ZGuid("1DEDA5DE-495A-4874-A824-B118F03C2C44");
			var declaration = SetupOutgoingMessage(Factory, sessionGuid, EDIMessage.Status.Sent, outgoingPVTRequestMessage);
			var (payload, xmlMessage) = CreatePVTResponseMessage(sessionGuid, "123456789", validTrader: true);
			ProcessMessage(Factory, xmlMessage);

			AssertEquals("New message created", 2, declaration.Messages.Count);
			AssertEquals("MessageText", payload, declaration.Messages[1].EM_MessageText);
			AssertContains("MessageInterpretation", "valid", declaration.Messages[1].EM_MessageInterpretation);
			AssertNotContains("MessageInterpretation", "invalid", declaration.Messages[1].EM_MessageInterpretation);
		}

		public void TestUpdateFromPVTResponseInvalid()
		{
			var sessionGuid = new ZGuid("1DEDA5DE-495A-4874-A824-B118F03C2C44");
			var declaration = SetupOutgoingMessage(Factory, sessionGuid, EDIMessage.Status.Sent, outgoingPVTRequestMessage);
			var (payload, xmlMessage) = CreatePVTResponseMessage(sessionGuid, "123456789", validTrader: false);
			ProcessMessage(Factory, xmlMessage);

			AssertEquals("New message created", 2, declaration.Messages.Count);
			AssertEquals("MessageText", payload, declaration.Messages[1].EM_MessageText);
			AssertContains("MessageInterpretation", "invalid", declaration.Messages[1].EM_MessageInterpretation);
		}

		public void TestUpdateApplicationReferenceOnMessageRequestedToBeSent()
		{
			var sessionGuid = new ZGuid("1DEDA5DE-495A-4874-A824-B118F03C2C44");
			var correlationId = "6358120B-7084-4D74-BDB8-C3C771CA6FD9";
			var declaration = SetupOutgoingMessage(Factory, sessionGuid);
			_ = ProcessMessage(Factory, CreateSuccessfulResponseWithCorrelationIDXml(sessionGuid, correlationId));
			AssertEquals(correlationId, declaration.Messages[0].EM_ApplicationReference);
		}

		public void TestUpdateMessageStatusUnderConcurrencyConflict()
		{
			var sessionGuid = new ZGuid("1DEDA5DE-495A-4874-A824-B118F03C2C44");
			var declaration = SetupOutgoingMessage(Factory, sessionGuid);

			var newFactory = NewFactory();
			newFactory.RefreshEnabled = false;
			newFactory.Load<EMCSJobDeclaration>(declaration.PK).JE_MessageStatus = EDIMessage.Status.Received;
			newFactory.Save();

			var logParents = ProcessMessage(Factory, CreateSuccessfulResponseXml(sessionGuid));
			var dec = logParents[0];
			dec.Reload();
			AssertEquals("Message acknowledged - No new message created", 1, dec.Messages.Count);
			AssertEquals(EDIMessage.Status.Acknowledged, dec.Messages[0].EM_Status);
			AssertEquals("JE_MessageStatus should remain unchanged", EDIMessage.Status.Received, dec.JE_MessageStatus);
		}

		static IXmlEventValueObject CreateEventDataObject(ZString xmlMessage)
		{
			return new XmlEventDeserializer().Parse(xmlMessage);
		}

		static public EMCSJobDeclaration[] ProcessMessage(BusinessObjectFactory factory, ZString xmlMessage)
		{
			var eventDataObject = CreateEventDataObject(xmlMessage);
			var subscriber = new GBEMCSEventParentFinder(factory, new GBEMCSDataContextManager(), new TestErrorLogger());
			return subscriber.GetLogParentsForEvent(eventDataObject)?.Cast<EMCSJobDeclaration>().ToArray();
		}

		BusinessObject[] ProcessCDSMessage(ZString xmlMessage)
		{
			var eventDataObject = CreateEventDataObject(xmlMessage);
			var subscriber = new JobDeclarationEventParentFinder(Factory, new JobDeclarationDataContextManager(), new TestErrorLogger());
			return subscriber.GetLogParentsForEvent(eventDataObject);
		}

		static public EMCSJobDeclaration SetupOutgoingMessage(BusinessObjectFactory factory, ZGuid sessionId, string outgoingMessageStatus = EDIMessage.Status.Sent, string outgoingMessageText = "")
		{
			var declaration = factory.New<EMCSJobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_DeclarationReference = "B0001000";
			declaration.JE_HouseBill = "TESTHOUSE";
			declaration.JE_MessageStatus = EDIMessage.Status.Sent;

			var outgoingInterchange = factory.New<EDIInterchange>();
			outgoingInterchange.EI_SessionGUID = sessionId;
			outgoingInterchange.EI_HeaderText = "";
			var outgoingSentMessage = outgoingInterchange.ContainedMessages.AddNew(typeof(EMCSOutboundEDIMessage));
			declaration.Messages.Add(outgoingSentMessage);
			outgoingSentMessage.EM_EI = outgoingInterchange.PK;
			outgoingSentMessage.EM_MessageText = outgoingMessageText;
			outgoingSentMessage.MessageNumberStrategy = new GbMessageNumberStrategy(factory, EDIMessage.ApplicationCodes.GbCustomsEMCS);
			outgoingSentMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingSentMessage.EM_Status = outgoingMessageStatus;
			outgoingSentMessage.EM_MessageNum = "999";
			outgoingInterchange.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingInterchange.EI_Status = EDIMessage.Status.Sent;
			outgoingInterchange.EI_From = "Sender";
			outgoingInterchange.EI_To = "AAW";
			outgoingInterchange.EI_BodyText = "";
			factory.Save();
			return declaration;
		}

		internal static ZString CreateSuccessfulResponseXml(ZGuid trackingId) => $@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Key>B0001000</Key>
					<Type>EMCSJobDeclaration</Type>
				</DataTarget>
			</DataTargetCollection>
			<DataSource>
				<DataProvider>EMCS</DataProvider>
			</DataSource>
		</DataContext>
		<EventTime>2022-09-23T08:30:23</EventTime>
		<EventType>MRS</EventType>
		<ContextCollection>
			<Context>
				<Type>eHubTrackingID</Type>
				<Value>{trackingId}</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		static ZString CreateSuccessfulResponseXmlWithoutDataTarget(ZGuid trackingId) => $@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataSource>
				<DataProvider>EMCS</DataProvider>
			</DataSource>
		</DataContext>
		<EventTime>2022-09-23T08:30:23</EventTime>
		<EventType>MRS</EventType>
		<ContextCollection>
			<Context>
				<Type>eHubTrackingID</Type>
				<Value>{trackingId}</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		static ZString CreateRejectedResponseXml(ZGuid trackingId) => $@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Key>B0001000</Key>
					<Type>EMCSJobDeclaration</Type>
				</DataTarget>
			</DataTargetCollection>
			<DataSource>
				<DataProvider>EMCS</DataProvider>
			</DataSource>
		</DataContext>
		<EventTime>2022-09-23T08:30:23</EventTime>
		<EventType>MRJ</EventType>
		<ContextCollection>
			<Context>
				<Type>eHubTrackingID</Type>
				<Value>{trackingId}</Value>
			</Context>
			<Context>
				<Type>ErrorSummary</Type>
				<Value>Could not upload to EMCS service</Value>
			</Context>
			<Context>
				<Type>ResponseText</Type>
				<Value>PHhtbD48ZXJyb3I+VEVTVCBFUlJPUiBNRVNTQUdFPC9lcnJvcj48L3htbD4=</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		readonly static ZString outgoingPVTRequestMessage = @"<UniversalEvent>
	<Event>
		<ContextCollection>
			<Context>
				<Type>PreValidateTraderBody</Type>
				<Value>eyJleGNpc2VUcmFkZXJWYWxpZGF0aW9uUmVxdWVzdCI6eyJleGNpc2VUcmFkZXJSZXF1ZXN0Ijp7ImV4Y2lzZVJlZ2lzdHJhdGlvbk51bWJlciI6IjEyMzQ1Njc4OSIsImVudGl0eUdyb3VwIjoiVUsgcmVjb3JkIiwidmFsaWRhdGVQcm9kdWN0QXV0aG9yaXNhdGlvblJlcXVlc3QiOlt7InByb2R1Y3QiOnsiZXhjaXNlUHJvZHVjdENvZGUiOiIyMjAzMDAwMTAwIn19XX19fQ==</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";

		static (ZString rawPayload, ZString xmlMessage) CreatePVTResponseMessage(ZGuid trackingId, string traderId, bool validTrader)
		{
			string payload;
			if (validTrader)
			{
				payload = $@"{{
  ""exciseTraderValidationResponse"": {{
    ""validationTimestamp"": ""2024-05-31T12:34:56+01:00"",
    ""exciseTraderResponse"": [{{
        ""exciseRegistrationNumber"": ""{traderId}"",
        ""entityGroup"": ""UK Record"",
        ""validTrader"": true,
        ""traderType"": ""1"",
        ""validateProductAuthorisationResponse"": {{
          ""valid"": true
        }}
      }}
    ]
  }}
}}";
			}
			else
			{
				payload = $@"{{
  ""validationTimestamp"": ""2024-05-31T12:34:56+01:00"",
  ""exciseTraderResponse"": [{{
      ""exciseRegistrationNumber"": ""{traderId}"",
      ""entityGroup"": ""UK Record"",
      ""validTrader"": false,
      ""errorCode"": ""6"",
      ""errorText"": ""Not Found""
    }}
  ]
}}";
			}
			var payloadBytes = Encoding.UTF8.GetBytes(payload);
			var base64payload = Convert.ToBase64String(payloadBytes);

			return (payload, $@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Key>B0001000</Key>
					<Type>EMCSJobDeclaration</Type>
				</DataTarget>
			</DataTargetCollection>
			<DataSource>
				<DataProvider>EMCS</DataProvider>
			</DataSource>
		</DataContext>
		<EventTime>2023-11-10T10:41:20</EventTime>
		<EventType>EVP</EventType>
		<ContextCollection>
			<Context>
				<Type>ResponseType</Type>
				<Value>Pre-Validate Trader Response</Value>
			</Context>
			<Context>
				<Type>eHubTrackingID</Type>
				<Value>{trackingId}</Value>
			</Context>
			<Context>
				<Type>ResponseText</Type>
				<Value>{base64payload}</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>");
		}

		static ZString CreateSuccessfulResponseWithCorrelationIDXml(ZGuid trackingId, string correlationId) => $@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	<Event>
		<DataContext>
			<DataTargetCollection>
				<DataTarget>
					<Key>B0001000</Key>
					<Type>EMCSJobDeclaration</Type>
				</DataTarget>
			</DataTargetCollection>
			<DataSource>
				<DataProvider>EMCS</DataProvider>
			</DataSource>
		</DataContext>
		<EventTime>2024-06-05T20:30:23</EventTime>
		<EventType>MRS</EventType>
		<ContextCollection>
			<Context>
				<Type>eHubTrackingID</Type>
				<Value>{trackingId}</Value>
			</Context>
			<Context>
				<Type>CorrelationID</Type>
				<Value>{correlationId}</Value>
			</Context>
		</ContextCollection>
	</Event>
</UniversalEvent>";
	}
}
