using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.UniversalDataBuss.Management.Testing
{
	class AcknowledgementTest : TestCaseWithFactoryAndMessagingHelpers
	{
		#region TestSendSuccessAcknowledgement

		public void TestSendSuccessAcknowledgement_WrongRequiredChannelRecipientID()
		{
			SendSuccessAcknowledgement("WrongRequired", "WrongChannel", "", false);

			var messages = boFactory.Load<EDIMessage>(new ZQuery());
			AssertEquals("messages.Count()", 1, messages.Length);
			var message = messages[0];
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			var notes = ((IStmNoteParent)message).Notes;
			AssertEquals("notes.GetAllNotes().Count()", 2, notes.GetAllNotes().Count);
			var sendAcknowledgementFailureLogNote = notes.FindByDescription("Send Acknowledge Failure Log")[0];
			AssertEquals(true, sendAcknowledgementFailureLogNote.ST_IsCustomDescription);
			var expectedNoteText =
@"Acknowledgement Required element has invalid value 'WrongRequired'. Valid values: 'OnAll', 'OnError', 'OnSuccess'.
Acknowledgement Channel element has invalid value 'WrongChannel'. Valid values: 'eHub', 'eAdaptor'.
Acknowledgement 'RecipientID' element can not be empty.
";
			AssertEquals("sendAcknowledgementFailureLogNote.ST_NoteDataAsText", expectedNoteText, sendAcknowledgementFailureLogNote.ST_NoteDataAsText);
		}

		public void TestSendSuccessAcknowledgement_eApapterOnAll()
		{
			SendSuccessAcknowledgement("OnAll", "eAdaptor", "HYEDAUIKB", true, EDIMessageStatusList.Codes.Sent, EDIInterchangeStatusList.Codes.eAdaptorQueued, EDIInterchangeTransportTypeList.Codes.eAdaptor);
		}

		public void TestSendSuccessAcknowledgement_eHubOnAll()
		{
			SendSuccessAcknowledgement("OnAll", "eHub", "HYEDAUIKB", true, EDIMessageStatusList.Codes.Sent, EDIInterchangeStatusList.Codes.eHubQueued, EDIInterchangeTransportTypeList.Codes.eHub);
		}

		public void TestSendSuccessAcknowledgement_eApapterOnError()
		{
			SendSuccessAcknowledgement("OnError", "eAdaptor", "HYEDAUIKB", false);
		}

		public void TestSendSuccessAcknowledgement_eHubOnError()
		{
			SendSuccessAcknowledgement("OnError", "eHub", "HYEDAUIKB", false);
		}

		public void TestSendSuccessAcknowledgement_eAdaptorOnSuccess()
		{
			SendSuccessAcknowledgement("OnSuccess", "eAdaptor", "HYEDAUIKB", true, EDIMessageStatusList.Codes.Sent, EDIInterchangeStatusList.Codes.eAdaptorQueued, EDIInterchangeTransportTypeList.Codes.eAdaptor);
		}

		public void TestSendSuccessAcknowledgement_eHubOnSuccess()
		{
			SendSuccessAcknowledgement("OnSuccess", "eHub", "HYEDAUIKB", true, EDIMessageStatusList.Codes.Sent, EDIInterchangeStatusList.Codes.eHubQueued, EDIInterchangeTransportTypeList.Codes.eHub);
		}

		public void TestSendSuccessAcknowledgement_eHubOnSuccess_MessageProcessedResultIsWarning()
		{
			SendSuccessAcknowledgementWhenMessageProcessedResultIsWarning("OnSuccess", "eHub", "HYEDAUIKB", true, EDIMessageStatusList.Codes.Sent, EDIInterchangeStatusList.Codes.eHubQueued, EDIInterchangeTransportTypeList.Codes.eHub);
		}

		const string SampleUniversalShipmentTemplate =
@"<UniversalShipment>
  <Shipment>
	<DataContext>
	  <DataTargetCollection>
		<DataTarget>
		  <Type>ForwardingConsol</Type>
		</DataTarget>
	  </DataTargetCollection>
	  <Company>
		<Code>DNZ</Code>
		<Name>NZ Demo Company</Name>
	  </Company>
	</DataContext>
	<ContainerMode>
	  <Code>FCL</Code>
	  <Description>Full Container Load</Description>
	</ContainerMode>
	<PortOfDischarge>
	  <Code>AUSYD</Code>
	  <Name>Sydney</Name>
	</PortOfDischarge>
	<PortOfLoading>
	  <Code>USLAX</Code>
	  <Name>Los Angeles</Name>
	</PortOfLoading>
	<ShipmentType>
	  <Code>AGT</Code>
	  <Description>Agent</Description>
	</ShipmentType>
	<TransportMode>
	  <Code>SEA</Code>
	  <Description>Sea Freight</Description>
	</TransportMode>
	<VesselName>BUNGA DELIMA</VesselName>
	<VoyageFlightNo>822</VoyageFlightNo>
	<WayBillNumber>FAT_TONY</WayBillNumber>
	<WayBillType>
	  <Code>MWB</Code>
	  <Description>Master Waybill</Description>
	</WayBillType>
	<SubShipmentCollection>
	  <SubShipment>
		<ContainerMode>
		  <Code>LCL</Code>
		  <Description>Less Container Load</Description>
		</ContainerMode>
		<PortOfDestination>
		  <Code>AUMEL</Code>
		  <Name>Melbourne</Name>
		</PortOfDestination>
		<PortOfOrigin>
		  <Code>USSFO</Code>
		  <Name>San Francisco</Name>
		</PortOfOrigin>
		<ShipmentType>
		  <Code>STD</Code>
		  <Description>Standard House</Description>
		</ShipmentType>
		<TransportMode>
		  <Code>SEA</Code>
		  <Description>Sea Freight</Description>
		</TransportMode>
		<WayBillNumber>JIMMY_THE_SNITCH</WayBillNumber>
		<WayBillType>
		  <Code>HWB</Code>
		  <Description>House Waybill</Description>
		</WayBillType>
	  
	</SubShipment>
	</SubShipmentCollection>

	{0}

  </Shipment>
</UniversalShipment>";

		public void TestSendSuccessAcknowledgement_WithNoContextCollection()
		{
			string messageText = string.Format(SampleUniversalShipmentTemplate, string.Empty);

			var expectedServiceTaskLog = @"
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim();
			var expectedMessageLogNote = @"No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.";

			var expectedMessageStatus = EDIMessageStatusList.Codes.Sent;
			var expectedInterchangeStatus = EDIInterchangeStatusList.Codes.eHubQueued;
			var expectedTransportType = EDIInterchangeTransportTypeList.Codes.eHub;
			var expectedMessageProcessedResult = EDIMessageStatusList.Codes.ProcessedOK;

			AssertEquals("Precondition", 0, boFactory.GetDatabaseCount(typeof(EDIInterchange)));
			AssertEquals("Precondition", 0, boFactory.GetDatabaseCount(typeof(EDIMessage)));

			#region interchange text
			var interchangeHeaderText = @"<Header>
						<SenderID>A</SenderID>
						<RecipientID>B</RecipientID>
						<Acknowledgement>
							<Required>OnSuccess</Required>
							<Channel>eHub</Channel>
							<RecipientID>HYEDAUIKB</RecipientID>
						</Acknowledgement>
					</Header>";
			var interchangeBodyText = string.Format(
					@"<Body>
						{0}
					</Body>", messageText);
			#endregion

			var message = GetQueuedUniversalShipmentMessage(messageText, true, true, interchangeHeaderText, interchangeBodyText, boFactory);
			var messagesBefore = boFactory.Load<IEDIMessage>(new ZQuery());
			var interchangesBefore = boFactory.Load<IEDIInterchange>(new ZQuery());
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			using (boFactory.AddDisposableService())
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				manager.Process(message);
				boFactory.Save();
			}

			AssertEquals(expectedMessageProcessedResult, message.EM_Status);
			AssertMultilineASCIIEquals("Service Task Log", expectedServiceTaskLog, serviceTaskLog.ToString());
			AssertMultilineASCIIEquals("Message Log Note", expectedMessageLogNote, message.GetLogNoteText());

			var messagesAfter = boFactory.Load<IEDIMessage>(new ZQuery());
			var interchangesAfter = boFactory.Load<IEDIInterchange>(new ZQuery());
			var acknowledgementMessage = messagesAfter.Except(messagesBefore).SingleOrDefault();
			var acknowledgementInterchange = interchangesAfter.Except(interchangesBefore).SingleOrDefault();
			AssertEquals(true, acknowledgementMessage.EM_IsActive);
			AssertEquals(ApplicationCodeList.Codes.UniversalDataMessaging, acknowledgementMessage.EM_ApplicationCode);
			AssertEquals(EDIMessageTypeList.Codes.XDC, acknowledgementMessage.EM_MessageType);
			AssertEquals(EDIMessageSubTypeList.Codes.XmlUniversalEvent, acknowledgementMessage.EM_MessageSubType);
			AssertEquals("TRX", acknowledgementMessage.EM_ReceiveTransmit);
			AssertEquals(expectedMessageStatus, acknowledgementMessage.EM_Status);
			var expectedMessageText = string.Format("<UniversalEvent xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\" version=\"1.1\">\r\n  <Event>\r\n    <DataContext>\r\n\r\n      <Company>\r\n        <Code>EDI</Code>\r\n        <Country>\r\n          <Code>AU</Code>\r\n          <Name>Australia</Name>\r\n        </Country>\r\n        <Name>Eagle Datamation International</Name>\r\n      </Company>\r\n      <DataProvider>EDIDATEDI</DataProvider>\r\n      <EnterpriseID>EDI</EnterpriseID>\r\n      <ServerID>DAT</ServerID>\r\n    </DataContext>\r\n\r\n    <EventTime>2013-12-05T13:03:28.927</EventTime>\r\n    <EventType>DIM</EventType>\r\n    <ContextCollection>\r\n      <Context>\r\n        <Type>DataImportLog</Type>\r\n        <Value>{0}</Value>\r\n      </Context>\r\n      <Context>\r\n        <Type>ProcessingResultStatus</Type>\r\n        <Value>{1}</Value>\r\n      </Context>\r\n    </ContextCollection>\r\n" +
				@"    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{2}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{3}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{4}</MessageNumber>
    </MessageNumberCollection>
" +
				"  </Event>\r\n</UniversalEvent>\r\n", ReplaceAngleBracketsWithXmlEscapeCharacters(expectedMessageLogNote), expectedMessageProcessedResult, acknowledgementInterchange.EI_SessionGUID, acknowledgementInterchange.EI_InterchangeNum, acknowledgementMessage.EM_MessageNum);
			AssertEquals(expectedMessageText, ReplaceEventTime(acknowledgementMessage.EM_MessageText, "2013-12-05T13:03:28.927"));
			AssertEquals(true, acknowledgementInterchange.EI_IsActive);
			AssertEquals(ApplicationCodeList.Codes.UniversalDataMessaging, acknowledgementInterchange.EI_ApplicationCode);
			AssertEquals(EDIInterchangeTypeList.Codes.XDC, acknowledgementInterchange.EI_InterchangeType);
			AssertEquals("TRX", acknowledgementInterchange.EI_ReceiveTransmit);
			AssertEquals(expectedInterchangeStatus, acknowledgementInterchange.EI_Status);
			AssertEquals(expectedTransportType, acknowledgementInterchange.EI_TransportType);
			AssertEquals("EDIEDIDAT", acknowledgementInterchange.EI_From);
			AssertEquals("HYEDAUIKB", acknowledgementInterchange.EI_To);
			AssertEquals("<EDIDelivery><FileName></FileName><EmailSubject></EmailSubject></EDIDelivery>", acknowledgementInterchange.EI_HeaderText);
			var expectedInterchangeText = string.Format("<UniversalInterchange xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\" version=\"1.1\">\r\n  <Header>\r\n    <SenderID>EDIEDIDAT</SenderID>\r\n    <RecipientID>HYEDAUIKB</RecipientID>\r\n  </Header>\r\n  <Body>\r\n    <UniversalEvent xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\" version=\"1.1\">\r\n  <Event>\r\n    <DataContext>\r\n\r\n      <Company>\r\n        <Code>EDI</Code>\r\n        <Country>\r\n          <Code>AU</Code>\r\n          <Name>Australia</Name>\r\n        </Country>\r\n        <Name>Eagle Datamation International</Name>\r\n      </Company>\r\n      <DataProvider>EDIDATEDI</DataProvider>\r\n      <EnterpriseID>EDI</EnterpriseID>\r\n      <ServerID>DAT</ServerID>\r\n    </DataContext>\r\n\r\n    <EventTime>2013-12-05T13:20:44.283</EventTime>\r\n    <EventType>DIM</EventType>\r\n    <ContextCollection>\r\n      <Context>\r\n        <Type>DataImportLog</Type>\r\n        <Value>{0}</Value>\r\n      </Context>\r\n      <Context>\r\n        <Type>ProcessingResultStatus</Type>\r\n        <Value>{1}</Value>\r\n      </Context>\r\n    </ContextCollection>\r\n" +
				@"    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{2}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{3}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{4}</MessageNumber>
    </MessageNumberCollection>
" +
				"  </Event>\r\n</UniversalEvent>\r\n  </Body>\r\n</UniversalInterchange>", ReplaceAngleBracketsWithXmlEscapeCharacters(expectedMessageLogNote), expectedMessageProcessedResult, acknowledgementInterchange.EI_SessionGUID, acknowledgementInterchange.EI_InterchangeNum, acknowledgementMessage.EM_MessageNum);
			AssertEquals(expectedInterchangeText, ReplaceEventTime(acknowledgementInterchange.EI_BodyText, "2013-12-05T13:20:44.283"));
			AssertEquals("", acknowledgementInterchange.EI_FooterText);
		}

		public void TestSendSuccessAcknowledgement_WithValidationRules()
		{
			string messageText = string.Format(SampleUniversalShipmentTemplate, @"
	<ValidationRuleCollection>
		<ValidationRule>
			<Code>R001</Code>
		</ValidationRule>
	</ValidationRuleCollection>
");

			var expectedMessageLogNote = @"No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Warning - Validation Rule R001, Sequence 1 is not met: Macro: 1 == 2
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.";
			var expectedMessageProcessedResult = EDIMessageStatusList.Codes.Warning;

			#region interchange text
			var interchangeHeaderText = @"<Header>
						<SenderID>A</SenderID>
						<RecipientID>B</RecipientID>
						<Acknowledgement>
							<Required>OnSuccess</Required>
							<Channel>eHub</Channel>
							<RecipientID>HYEDAUIKB</RecipientID>
						</Acknowledgement>
					</Header>";
			var interchangeBodyText = string.Format(
					@"<Body>
						{0}
					</Body>", messageText);
			#endregion

			var workflowTestHelper = ObjectFactory.Get<IWorkflowTestHelper>();
			var ruleSet1 = workflowTestHelper.CreateUniversalValidationRuleSet(boFactory, DataContextType.ForwardingConsol, "R001", "");
			var rule1 = workflowTestHelper.AddUniversalValidationRule(ruleSet1, "1 == 2", "WRN", "Oops I failed");
			boFactory.Save();

			var message = GetQueuedUniversalShipmentMessage(messageText, true, true, interchangeHeaderText, interchangeBodyText, boFactory);
			var messagesBefore = boFactory.Load<IEDIMessage>(new ZQuery());
			var interchangesBefore = boFactory.Load<IEDIInterchange>(new ZQuery());
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);

			using (boFactory.AddDisposableService())
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				manager.Process(message);
				boFactory.Save();
			}

			AssertEquals(expectedMessageProcessedResult, message.EM_Status);
			AssertMultilineASCIIEquals("Message Log Note", expectedMessageLogNote, message.GetLogNoteText());

			var messagesAfter = boFactory.Load<IEDIMessage>(new ZQuery());
			var interchangesAfter = boFactory.Load<IEDIInterchange>(new ZQuery());
			var acknowledgementMessage = messagesAfter.Except(messagesBefore).SingleOrDefault();
			var acknowledgementInterchange = interchangesAfter.Except(interchangesBefore).SingleOrDefault();
			var expectedMessageText = string.Format("<UniversalEvent xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\" version=\"1.1\">\r\n  <Event>\r\n    <DataContext>\r\n\r\n      <Company>\r\n        <Code>EDI</Code>\r\n        <Country>\r\n          <Code>AU</Code>\r\n          <Name>Australia</Name>\r\n        </Country>\r\n        <Name>Eagle Datamation International</Name>\r\n      </Company>\r\n      <DataProvider>EDIDATEDI</DataProvider>\r\n      <EnterpriseID>EDI</EnterpriseID>\r\n      <ServerID>DAT</ServerID>\r\n    </DataContext>\r\n\r\n    <EventTime>2013-12-05T13:03:28.927</EventTime>\r\n    <EventType>DIM</EventType>\r\n    <ContextCollection>\r\n      <Context>\r\n        <Type>DataImportLog</Type>\r\n        <Value>{0}</Value>\r\n      </Context>\r\n      <Context>\r\n        <Type>ProcessingResultStatus</Type>\r\n        <Value>{1}</Value>\r\n      </Context>\r\n    </ContextCollection>\r\n" +
				@"    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{2}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{3}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{4}</MessageNumber>
    </MessageNumberCollection>
" +
				@"    <ValidationRuleCollection>
      <ValidationRule>
        <Code>R001</Code>
        <MessageLog>Oops I failed</MessageLog>
        <Result>WARNING</Result>
        <Sequence>1</Sequence>
      </ValidationRule>
    </ValidationRuleCollection>
" +
				"  </Event>\r\n</UniversalEvent>\r\n", ReplaceAngleBracketsWithXmlEscapeCharacters(expectedMessageLogNote), expectedMessageProcessedResult, acknowledgementInterchange.EI_SessionGUID, acknowledgementInterchange.EI_InterchangeNum, acknowledgementMessage.EM_MessageNum);
			AssertEquals(expectedMessageText, ReplaceEventTime(acknowledgementMessage.EM_MessageText, "2013-12-05T13:03:28.927"));
			var expectedInterchangeText = string.Format("<UniversalInterchange xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\" version=\"1.1\">\r\n  <Header>\r\n    <SenderID>EDIEDIDAT</SenderID>\r\n    <RecipientID>HYEDAUIKB</RecipientID>\r\n  </Header>\r\n  <Body>\r\n    <UniversalEvent xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\" version=\"1.1\">\r\n  <Event>\r\n    <DataContext>\r\n\r\n      <Company>\r\n        <Code>EDI</Code>\r\n        <Country>\r\n          <Code>AU</Code>\r\n          <Name>Australia</Name>\r\n        </Country>\r\n        <Name>Eagle Datamation International</Name>\r\n      </Company>\r\n      <DataProvider>EDIDATEDI</DataProvider>\r\n      <EnterpriseID>EDI</EnterpriseID>\r\n      <ServerID>DAT</ServerID>\r\n    </DataContext>\r\n\r\n    <EventTime>2013-12-05T13:20:44.283</EventTime>\r\n    <EventType>DIM</EventType>\r\n    <ContextCollection>\r\n      <Context>\r\n        <Type>DataImportLog</Type>\r\n        <Value>{0}</Value>\r\n      </Context>\r\n      <Context>\r\n        <Type>ProcessingResultStatus</Type>\r\n        <Value>{1}</Value>\r\n      </Context>\r\n    </ContextCollection>\r\n" +
				@"    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{2}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{3}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{4}</MessageNumber>
    </MessageNumberCollection>
" +
				@"    <ValidationRuleCollection>
      <ValidationRule>
        <Code>R001</Code>
        <MessageLog>Oops I failed</MessageLog>
        <Result>WARNING</Result>
        <Sequence>1</Sequence>
      </ValidationRule>
    </ValidationRuleCollection>
" +
				"  </Event>\r\n</UniversalEvent>\r\n  </Body>\r\n</UniversalInterchange>", ReplaceAngleBracketsWithXmlEscapeCharacters(expectedMessageLogNote), expectedMessageProcessedResult, acknowledgementInterchange.EI_SessionGUID, acknowledgementInterchange.EI_InterchangeNum, acknowledgementMessage.EM_MessageNum);
			AssertEquals(expectedInterchangeText, ReplaceEventTime(acknowledgementInterchange.EI_BodyText, "2013-12-05T13:20:44.283"));
		}

		#endregion

		#region TestSendFailureAcknowledgement

		public void TestSendFailureAcknowledgement_eApapterOnAll()
		{
			SendFailureAcknowledgement("OnAll", "eAdaptor", "HYEDAUIKB", true, EDIMessageStatusList.Codes.Sent, EDIInterchangeStatusList.Codes.eAdaptorQueued, EDIInterchangeTransportTypeList.Codes.eAdaptor);
		}

		public void TestSendFailureAcknowledgement_eHubOnAll()
		{
			SendFailureAcknowledgement("OnAll", "eHub", "HYEDAUIKB", true, EDIMessageStatusList.Codes.Sent, EDIInterchangeStatusList.Codes.eHubQueued, EDIInterchangeTransportTypeList.Codes.eHub);
		}

		public void TestSendFailureAcknowledgement_eApapterOnError()
		{
			SendFailureAcknowledgement("OnError", "eAdaptor", "HYEDAUIKB", true, EDIMessageStatusList.Codes.Sent, EDIInterchangeStatusList.Codes.eAdaptorQueued, EDIInterchangeTransportTypeList.Codes.eAdaptor);
		}

		public void TestSendFailureAcknowledgement_eHubOnError()
		{
			SendFailureAcknowledgement("OnError", "eHub", "HYEDAUIKB", true, EDIMessageStatusList.Codes.Sent, EDIInterchangeStatusList.Codes.eHubQueued, EDIInterchangeTransportTypeList.Codes.eHub);
		}

		public void TestSendFailureAcknowledgement_eAdaptorOnSuccess()
		{
			SendFailureAcknowledgement("OnSuccess", "eAdaptor", "HYEDAUIKB", false);
		}

		public void TestSendFailureAcknowledgement_eHubOnSuccess()
		{
			SendFailureAcknowledgement("OnSuccess", "eHub", "HYEDAUIKB", false);
		}

		#endregion

		#region Implementation

		string GetAcknowledgementText(string required, string channel, string recipientID)
		{
			return string.Format(@"<Acknowledgement>
						<Required>{0}</Required>
						<Channel>{1}</Channel>
						<RecipientID>{2}</RecipientID>
						<ContextCollection>
						<Context>
							<Type>Foo</Type>
							<Value>CS908217349087123</Value>
						</Context>
						<Context>
							<Type>OriginalMessage</Type>
							<Value>Blalalalallalalalal</Value>
						</Context>
						</ContextCollection>
					</Acknowledgement>", required, channel, recipientID);
		}

		void SendSuccessAcknowledgement(string acknowledgementRequired, string acknowledgementChannel, string recepientID, bool expectedAcknowledgementMessageShouldBeCreated, string expectedMessageStatus = "", string expectedInterchangeStatus = "", string expectedTransportType = "")
		{
			#region message text

			string messageText =
@"<UniversalShipment>
  <Shipment>
	<DataContext>
	  <DataTargetCollection>
		<DataTarget>
		  <Type>ForwardingConsol</Type>
		</DataTarget>
	  </DataTargetCollection>

	  <Company>
		<Code>DNZ</Code>
		<Name>NZ Demo Company</Name>
	  </Company>
	</DataContext>
	<ContainerMode>
	  <Code>FCL</Code>
	  <Description>Full Container Load</Description>
	</ContainerMode>
	<PortOfDischarge>
	  <Code>AUSYD</Code>
	  <Name>Sydney</Name>
	</PortOfDischarge>
	<PortOfLoading>
	  <Code>USLAX</Code>
	  <Name>Los Angeles</Name>
	</PortOfLoading>
	<ShipmentType>
	  <Code>AGT</Code>
	  <Description>Agent</Description>
	</ShipmentType>
	<TransportMode>
	  <Code>SEA</Code>
	  <Description>Sea Freight</Description>
	</TransportMode>
	<VesselName>BUNGA DELIMA</VesselName>
	<VoyageFlightNo>822</VoyageFlightNo>
	<WayBillNumber>FAT_TONY</WayBillNumber>
	<WayBillType>
	  <Code>MWB</Code>
	  <Description>Master Waybill</Description>
	</WayBillType>
	<SubShipmentCollection>
	  <SubShipment>
		<ContainerMode>
		  <Code>LCL</Code>
		  <Description>Less Container Load</Description>
		</ContainerMode>
		<PortOfDestination>
		  <Code>AUMEL</Code>
		  <Name>Melbourne</Name>
		</PortOfDestination>
		<PortOfOrigin>
		  <Code>USSFO</Code>
		  <Name>San Francisco</Name>
		</PortOfOrigin>
		<ShipmentType>
		  <Code>STD</Code>
		  <Description>Standard House</Description>
		</ShipmentType>
		<TransportMode>
		  <Code>SEA</Code>
		  <Description>Sea Freight</Description>
		</TransportMode>
		<WayBillNumber>JIMMY_THE_SNITCH</WayBillNumber>
		<WayBillType>
		  <Code>HWB</Code>
		  <Description>House Waybill</Description>
		</WayBillType>
	</SubShipment>
	</SubShipmentCollection>
  </Shipment>
</UniversalShipment>";

			#endregion

			string expectedServiceTaskLog = @"
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.
".Trim();

			string expectedMessageLogNote = @"No matching ForwardingConsol found, creating new ForwardingConsol.
Populating ForwardingConsol...
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Added Shipment (House Bill='JIMMY_THE_SNITCH') from UniversalShipment.
Added Consol (Master Bill='FAT_TONY') from UniversalShipment.
Successfully saved Consol C00001000 (Master Bill='FAT_TONY') with 1 x ForwardingShipment.";

			SendSuccessAcknowledgement(acknowledgementRequired, acknowledgementChannel, recepientID, messageText, EDIMessageStatusList.Codes.ProcessedOK, expectedServiceTaskLog, expectedMessageLogNote, expectedAcknowledgementMessageShouldBeCreated, expectedMessageStatus, expectedInterchangeStatus, expectedTransportType);
		}

		void SendSuccessAcknowledgementWhenMessageProcessedResultIsWarning(string acknowledgementRequired, string acknowledgementChannel, string recepientID, bool expectedAcknowledgementMessageShouldBeCreated, string expectedMessageStatus = "", string expectedInterchangeStatus = "", string expectedTransportType = "")
		{
			#region message text

			string messageText =
@"<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
	<DataContext>
	  <DataTargetCollection>
		<DataTarget>
		  <Type>ForwardingShipment</Type>
		</DataTarget>
	  </DataTargetCollection>
	  <CodesMappedToTarget>true</CodesMappedToTarget>
	</DataContext>

	<ContainerMode>
	  <Code>LSE</Code>
	  <Description>Loose</Description>
	</ContainerMode>
	<GoodsDescription>BIG FAT FISH</GoodsDescription>
	<PortOfDestination>
	  <Code>AUBNE</Code>
	  <Name>Brisbane</Name>
	</PortOfDestination>
	<PortOfOrigin>
	  <Code>NZABY</Code>
	  <Name>Albany</Name>
	</PortOfOrigin>
	<ShipmentIncoTerm>
	  <Code>FOB</Code>
	  <Description>Free On Board</Description>
	</ShipmentIncoTerm>
	<TotalNoOfPacks>0</TotalNoOfPacks>
	<TotalNoOfPacksPackageType>
	  <Code>CTN</Code>
	  <Description>Carton</Description>
	</TotalNoOfPacksPackageType>
	<TotalVolume>0.300</TotalVolume>
	<TotalVolumeUnit>
	  <Code>M3</Code>
	  <Description>Cubic Meters</Description>
	</TotalVolumeUnit>
	<TotalWeight>234.000</TotalWeight>
	<TotalWeightUnit>
	  <Code>KG</Code>
	  <Description>Kilograms</Description>
	</TotalWeightUnit>
	<TransportMode>
	  <Code>AIR</Code>
	  <Description>Air Freight</Description>
	</TransportMode>
	<WayBillNumber>TEST BAD DATE</WayBillNumber>
	<WayBillType>
	  <Code>HWB</Code>
	  <Description>House Waybill</Description>
	</WayBillType>
	<LocalProcessing>
	  <ArrivalCartageRef></ArrivalCartageRef>
	  <DeliveryCartageAdvised>1788-01-26T00:00:00</DeliveryCartageAdvised>
	  <DeliveryCartageCompleted>2188-01-26T00:00:00</DeliveryCartageCompleted>
	  <EstimatedDelivery>2012-01-01T00:00:00</EstimatedDelivery>
	</LocalProcessing>
	<CustomizedFieldCollection>
	  <CustomizedField>
		<Key>CustomBlaString</Key>
		<DataType>String</DataType>
		<Value>TEST</Value>
	  </CustomizedField>
	</CustomizedFieldCollection>
	<DateCollection>
	  <Date>
		<Type>Departure</Type>
		<IsEstimate>true</IsEstimate>
		<Value>1788-01-26T00:00:00</Value>
	  </Date>
	  <Date>
		<Type>Arrival</Type>
		<IsEstimate>true</IsEstimate>
		<Value>2188-01-26T00:00:00</Value>
	  </Date>
	</DateCollection>
	<OrganizationAddressCollection>
	  <OrganizationAddress>
		<AddressType>ConsigneeDocumentaryAddress</AddressType>
		<AddressShortCode>Pick Up Address</AddressShortCode>
		<OrganizationCode>BAROPT</OrganizationCode>
		<Address1>12 COOLIBAH DRIVE</Address1>
		<Address2></Address2>
		<AddressOverride>false</AddressOverride>
		<City>PALM BEACH</City>
		<CompanyName>BARZ OPTICS</CompanyName>
		<Country>
		  <Code>AU</Code>
		  <Name>Australia</Name>
		</Country>
		<Port>
		  <Code>AUBNE</Code>
		  <Name>Brisbane</Name>
		</Port>
		<Postcode>4221</Postcode>
		<State>QLD</State>
	  </OrganizationAddress>
	  <OrganizationAddress>
		<AddressType>ConsignorDocumentaryAddress</AddressType>
		<AddressShortCode>PST: DIESLSTR 11</AddressShortCode>
		<OrganizationCode>ABABEU</OrganizationCode>
		<Address1>DIESLSTR 11</Address1>
		<Address2>57439 ATTENDORN, GERMANY</Address2>
		<AddressOverride>false</AddressOverride>
		<City>MOSCOW</City>
		<CompanyName>ABA BEUL</CompanyName>
		<Country>
		  <Code>DE</Code>
		  <Name>Germany</Name>
		</Country>
		<Port>
		  <Code>DEFRA</Code>
		  <Name>Frankfurt am Main</Name>
		</Port>
		<Postcode>113186</Postcode>
		<State>BE</State>
	  </OrganizationAddress>
	</OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

			#endregion

			string expectedServiceTaskLog = @"
Added Shipment (House Bill='TEST BAD DATE') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='TEST BAD DATE').
".Trim();

			string expectedMessageLogNote = @"
Warning - Line 55: <Shipment>.<LocalProcessing>.<DeliveryCartageAdvised> - Invalid value [1788-01-26T00:00:00]. Value must be a valid date between 2-Jan-1900 and 6-Jun-2079.
Warning - Line 56: <Shipment>.<LocalProcessing>.<DeliveryCartageCompleted> - Invalid value [2188-01-26T00:00:00]. Value must be a valid date between 2-Jan-1900 and 6-Jun-2079.
Warning - Line 70: <Shipment>.<DateCollection>.<Date>.<Value> - Invalid value [1788-01-26T00:00:00]. Value must be a valid date between 2-Jan-1900 and 6-Jun-2079.
Warning - Line 71: Element <Shipment>.<DateCollection>.<Date> opened at line 67 was excluded as it was missing mandatory elements. Missing: Value.
Warning - Line 75: <Shipment>.<DateCollection>.<Date>.<Value> - Invalid value [2188-01-26T00:00:00]. Value must be a valid date between 2-Jan-1900 and 6-Jun-2079.
Warning - Line 76: Element <Shipment>.<DateCollection>.<Date> opened at line 72 was excluded as it was missing mandatory elements. Missing: Value.
No matching ForwardingShipment found, creating new ForwardingShipment.
Populating ForwardingShipment...
Matching 'ConsignorDocumentaryAddress':- Matched to 'ABABEU' by code, address 'PST: DIESLSTR 11' by short code.
Matching 'ConsigneeDocumentaryAddress':- Matched to 'BAROPT' by code, address 'Pick Up Address' by short code.
Added Shipment (House Bill='TEST BAD DATE') from UniversalShipment.
Successfully saved Shipment S00001000 (House Bill='TEST BAD DATE').
".Trim();

			SendSuccessAcknowledgement(acknowledgementRequired, acknowledgementChannel, recepientID, messageText, EDIMessageStatusList.Codes.Warning, expectedServiceTaskLog, expectedMessageLogNote, expectedAcknowledgementMessageShouldBeCreated, expectedMessageStatus, expectedInterchangeStatus, expectedTransportType);
		}

		void SendSuccessAcknowledgement(string acknowledgementRequired, string acknowledgementChannel, string recipientID, string messageText, string expectedMessageProcessedResult, string expectedServiceTaskLog, string expectedMessageLogNote, bool expectedAcknowledgementShouldBeCreated, string expectedMessageStatus = "", string expectedInterchangeStatus = "", string expectedTransportType = "")
		{
			AssertEquals("Precondition", 0, boFactory.GetDatabaseCount(typeof(EDIInterchange)));
			AssertEquals("Precondition", 0, boFactory.GetDatabaseCount(typeof(EDIMessage)));

			#region interchange text

			var acknowledgementText = GetAcknowledgementText(acknowledgementRequired, acknowledgementChannel, recipientID);
			var interchangeHeaderText = string.Format(
					@"<Header>
						<SenderID>A</SenderID>
						<RecipientID>B</RecipientID>
						{0}
					</Header>", acknowledgementText);
			var interchangeBodyText = string.Format(
					@"<Body>
						{0}
					</Body>", messageText);

			#endregion

			var message = GetQueuedUniversalShipmentMessage(messageText, true, true, interchangeHeaderText, interchangeBodyText, boFactory);

			var messagesBefore = boFactory.Load<IEDIMessage>(new ZQuery());
			var interchangesBefore = boFactory.Load<IEDIInterchange>(new ZQuery());

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			using (boFactory.AddDisposableService())
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				manager.Process(message);
				boFactory.Save();
			}
			CombineAssertions(delegate
			{
				AssertEquals(expectedMessageProcessedResult, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", expectedServiceTaskLog, serviceTaskLog.ToString());
				AssertMultilineASCIIEquals("Message Log Note", expectedMessageLogNote, message.GetLogNoteText());

				var messagesAfter = boFactory.Load<IEDIMessage>(new ZQuery());
				var interchangesAfter = boFactory.Load<IEDIInterchange>(new ZQuery());

				var acknowlegementMessage = messagesAfter.Except(messagesBefore).SingleOrDefault();
				var acknowlegementInterchange = interchangesAfter.Except(interchangesBefore).SingleOrDefault();

				if (expectedAcknowledgementShouldBeCreated)
				{
					ValidateAknowledgementInterchangeAndMessage(acknowlegementMessage, acknowlegementInterchange, expectedMessageStatus, expectedInterchangeStatus, expectedTransportType, expectedMessageLogNote, expectedMessageProcessedResult);
				}
				else
				{
					AssertNull(acknowlegementMessage);
					AssertNull(acknowlegementInterchange);
				}
			});
		}

		void SendFailureAcknowledgement(string acknowledgementRequired, string acknowledgementChannel, string recipientID, bool expectedAcknowledgementMessageShouldBeCreated, string expectedMessageStatus = "", string expectedInterchangeStatus = "", string expectedTransportType = "")
		{
			#region message text and interchange text

			var messageText = @"<UniversalShipment></UniversalShipment>";
			var acknowledgementText = GetAcknowledgementText(acknowledgementRequired, acknowledgementChannel, recipientID);
			var interchangeHeaderText = string.Format(
					@"<Header>
						<SenderID>A</SenderID>
						<RecipientID>B</RecipientID>
						{0}
					</Header>", acknowledgementText);
			var interchangeBodyText = string.Format(
					@"<Body>
						{0}
					</Body>", messageText);

			#endregion

			AssertEquals("Precondition", 0, boFactory.GetDatabaseCount(typeof(EDIInterchange)));
			AssertEquals("Precondition", 0, boFactory.GetDatabaseCount(typeof(EDIMessage)));

			var message = GetQueuedUniversalShipmentMessage(messageText, true, true, interchangeHeaderText, interchangeBodyText, boFactory);

			var messagesBefore = boFactory.Load<IEDIMessage>(new ZQuery());
			var interchangesBefore = boFactory.Load<IEDIInterchange>(new ZQuery());

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			using (boFactory.AddDisposableService())
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				manager.Process(message);
				boFactory.Save();
			}
			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.Rejected, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
ERROR - Failed to parse XML. Errors found:-
Error - Line 1: Root element <UniversalShipment> must contain one <Shipment> element. </UniversalShipment> is not valid in this scope.
".Trim(), serviceTaskLog.ToString());

				var messageLogNote = @"Error - Line 1: Root element <UniversalShipment> must contain one <Shipment> element. </UniversalShipment> is not valid in this scope.
Message Rejected.";

				AssertMultilineASCIIEquals("Message Log Note", messageLogNote, message.GetLogNoteText());

				var messagesAfter = boFactory.Load<IEDIMessage>(new ZQuery());
				var interchangesAfter = boFactory.Load<IEDIInterchange>(new ZQuery());

				var acknowlegementMessage = messagesAfter.Except(messagesBefore).SingleOrDefault();
				var acknowlegementInterchange = interchangesAfter.Except(interchangesBefore).SingleOrDefault();

				if (expectedAcknowledgementMessageShouldBeCreated)
				{
					ValidateAknowledgementInterchangeAndMessage(acknowlegementMessage, acknowlegementInterchange, expectedMessageStatus, expectedInterchangeStatus, expectedTransportType, messageLogNote, EDIMessageStatusList.Codes.Rejected);
				}
				else
				{
					AssertNull(acknowlegementMessage);
					AssertNull(acknowlegementInterchange);
				}
			});
		}

		void ValidateAknowledgementInterchangeAndMessage(IEDIMessage acknowledgementMessage, IEDIInterchange acknowledgementInterchange, string expectedMessageStatus, string expectedInterchangeStatus, string expectedTransportType, string dataImportLog, string processingResultStatus)
		{
			AssertEquals(true, acknowledgementMessage.EM_IsActive);
			AssertEquals(ApplicationCodeList.Codes.UniversalDataMessaging, acknowledgementMessage.EM_ApplicationCode);
			AssertEquals(EDIMessageTypeList.Codes.XDC, acknowledgementMessage.EM_MessageType);
			AssertEquals(EDIMessageSubTypeList.Codes.XmlUniversalEvent, acknowledgementMessage.EM_MessageSubType);
			AssertEquals("TRX", acknowledgementMessage.EM_ReceiveTransmit);
			AssertEquals(expectedMessageStatus, acknowledgementMessage.EM_Status);
			var expectedMessageText = string.Format("<UniversalEvent xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\" version=\"1.1\">\r\n  <Event>\r\n    <DataContext>\r\n\r\n      <Company>\r\n        <Code>EDI</Code>\r\n        <Country>\r\n          <Code>AU</Code>\r\n          <Name>Australia</Name>\r\n        </Country>\r\n        <Name>Eagle Datamation International</Name>\r\n      </Company>\r\n      <DataProvider>EDIDATEDI</DataProvider>\r\n      <EnterpriseID>EDI</EnterpriseID>\r\n      <ServerID>DAT</ServerID>\r\n    </DataContext>\r\n\r\n    <EventTime>2013-12-05T13:03:28.927</EventTime>\r\n    <EventType>DIM</EventType>\r\n    <ContextCollection>\r\n      <Context>\r\n        <Type>Foo</Type>\r\n        <Value>CS908217349087123</Value>\r\n      </Context>\r\n      <Context>\r\n        <Type>OriginalMessage</Type>\r\n        <Value>Blalalalallalalalal</Value>\r\n      </Context>\r\n      <Context>\r\n        <Type>DataImportLog</Type>\r\n        <Value>{0}</Value>\r\n      </Context>\r\n      <Context>\r\n        <Type>ProcessingResultStatus</Type>\r\n        <Value>{1}</Value>\r\n      </Context>\r\n    </ContextCollection>\r\n" +
				$@"    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{acknowledgementInterchange.EI_SessionGUID}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{acknowledgementInterchange.EI_InterchangeNum}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{acknowledgementMessage.EM_MessageNum}</MessageNumber>
    </MessageNumberCollection>
" +
				"  </Event>\r\n</UniversalEvent>", ReplaceAngleBracketsWithXmlEscapeCharacters(dataImportLog), processingResultStatus);
			AssertEquals(expectedMessageText, ReplaceEventTime(acknowledgementMessage.EM_MessageText, "2013-12-05T13:03:28.927").Trim());

			AssertEquals(true, acknowledgementInterchange.EI_IsActive);
			AssertEquals(ApplicationCodeList.Codes.UniversalDataMessaging, acknowledgementInterchange.EI_ApplicationCode);
			AssertEquals(EDIInterchangeTypeList.Codes.XDC, acknowledgementInterchange.EI_InterchangeType);
			AssertEquals("TRX", acknowledgementInterchange.EI_ReceiveTransmit);
			AssertEquals(expectedInterchangeStatus, acknowledgementInterchange.EI_Status);
			AssertEquals(expectedTransportType, acknowledgementInterchange.EI_TransportType);
			AssertEquals("EDIEDIDAT", acknowledgementInterchange.EI_From);
			AssertEquals("HYEDAUIKB", acknowledgementInterchange.EI_To);
			AssertEquals("<EDIDelivery><FileName></FileName><EmailSubject></EmailSubject></EDIDelivery>", acknowledgementInterchange.EI_HeaderText);
			var expectedInterchangeText = string.Format("<UniversalInterchange xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\" version=\"1.1\">\r\n  <Header>\r\n    <SenderID>EDIEDIDAT</SenderID>\r\n    <RecipientID>HYEDAUIKB</RecipientID>\r\n  </Header>\r\n  <Body>\r\n    <UniversalEvent xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\" version=\"1.1\">\r\n  <Event>\r\n    <DataContext>\r\n\r\n      <Company>\r\n        <Code>EDI</Code>\r\n        <Country>\r\n          <Code>AU</Code>\r\n          <Name>Australia</Name>\r\n        </Country>\r\n        <Name>Eagle Datamation International</Name>\r\n      </Company>\r\n      <DataProvider>EDIDATEDI</DataProvider>\r\n      <EnterpriseID>EDI</EnterpriseID>\r\n      <ServerID>DAT</ServerID>\r\n    </DataContext>\r\n\r\n    <EventTime>2013-12-05T13:20:44.283</EventTime>\r\n    <EventType>DIM</EventType>\r\n    <ContextCollection>\r\n      <Context>\r\n        <Type>Foo</Type>\r\n        <Value>CS908217349087123</Value>\r\n      </Context>\r\n      <Context>\r\n        <Type>OriginalMessage</Type>\r\n        <Value>Blalalalallalalalal</Value>\r\n      </Context>\r\n      <Context>\r\n        <Type>DataImportLog</Type>\r\n        <Value>{0}</Value>\r\n      </Context>\r\n      <Context>\r\n        <Type>ProcessingResultStatus</Type>\r\n        <Value>{1}</Value>\r\n      </Context>\r\n    </ContextCollection>\r\n" +
				$@"    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{acknowledgementInterchange.EI_SessionGUID}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{acknowledgementInterchange.EI_InterchangeNum}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{acknowledgementMessage.EM_MessageNum}</MessageNumber>
    </MessageNumberCollection>
" +
				"  </Event>\r\n</UniversalEvent>\r\n  </Body>\r\n</UniversalInterchange>", ReplaceAngleBracketsWithXmlEscapeCharacters(dataImportLog), processingResultStatus);
			AssertEquals(expectedInterchangeText, ReplaceEventTime(acknowledgementInterchange.EI_BodyText, "2013-12-05T13:20:44.283"));
			AssertEquals("", acknowledgementInterchange.EI_FooterText);
		}

		string ReplaceEventTime(string text, string dateString)
		{
			string startTag = "<EventTime>";
			string endTag = "</EventTime>";
			int startPos = text.IndexOf(startTag);
			int endPos = text.IndexOf(endTag);

			return text.Substring(0, startPos + startTag.Length) + dateString + text.Substring(endPos, text.Length - endPos);
		}

		string ReplaceAngleBracketsWithXmlEscapeCharacters(string input)
		{
			return input.Replace(">", "&gt;").Replace("<", "&lt;");
		}

		readonly BusinessObjectFactory boFactory = new BusinessObjectFactory();

		#endregion
	}
}
