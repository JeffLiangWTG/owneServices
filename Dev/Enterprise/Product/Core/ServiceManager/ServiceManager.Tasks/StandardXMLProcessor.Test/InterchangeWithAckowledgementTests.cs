using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.eHubMessaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor.Testing
{
	class InterchangeWithAckowledgementTests : StandardXmlServiceTaskTestBase
	{
		public void TestAckWithLongNameDoesntBlowUp()
		{
			var interchangeWithACK = CreateInterchangeWithAcknowledgement(InterchangeAcknowledgement.InterchangeAcknowledgementRequiredList.OnSuccess, InterchangeAcknowledgement.InterchangeAcknowledgementChannelList.eAdaptor);

			interchangeWithACK.EI_HeaderText = string.Format(@"<InterchangeInfo xmlns=""http://www.edi.com.au/EnterpriseService/"">
	<Date>2013-11-28T15:37:56.893+11:00</Date>
	<XmlType>Verbose</XmlType>
	<Source/>
	<Target />
	<Acknowledgement>
      <Required>OnAll</Required>
      <Channel>eAdaptor</Channel>
      <RecipientID>L77CANTRNXXXSCYSDVSDCVSDVBSBVSUSDCUCI8RE8T8T54GHRT8HV34G834YT85Y8G5T45HU85U38HU43HU38YH38YH33XXXX13</RecipientID>
      <ContextCollection>
        <Context>
          <Type>ORG</Type>
          <Value>INBORG</Value>
        </Context>
      </ContextCollection>
    </Acknowledgement>
  </InterchangeInfo>");

			EDIInterchange acknowlegementInterchange;
			EDIMessage acknowlegementMessage;
			ProcessMessage(interchangeWithACK, out acknowlegementMessage, out acknowlegementInterchange);

			var newFactory = new BusinessObjectFactory();
			var messages = newFactory.Load<EDIMessage>(new ZQuery());
			AssertEquals("messages.Count()", 1, messages.Length);
			var message = messages[0];
			AssertEquals(EDIMessageStatusList.Codes.Received, message.EM_Status);
			var notes = ((IStmNoteParent)message).Notes;
			var successNote = (StmNote)notes.GetAllNotes().Single();
			AssertContains("Import finished", successNote.ST_NoteDataAsText);
		}

		public void TestProcessShipmentWithAcknowledgement_WrongRequiredChannelRecipientID()
		{
			var interchangeWithACK = CreateInterchangeWithAcknowledgement(InterchangeAcknowledgement.InterchangeAcknowledgementRequiredList.OnSuccess, InterchangeAcknowledgement.InterchangeAcknowledgementChannelList.eAdaptor);

			interchangeWithACK.EI_HeaderText = string.Format(@"<InterchangeInfo xmlns=""http://www.edi.com.au/EnterpriseService/"">
	<Date>2013-11-28T15:37:56.893+11:00</Date>
	<XmlType>Verbose</XmlType>
	<Source/>
	<Target />
	  <Acknowledgement>
		<Required>WrongRequired</Required>
		<Channel>WrongChannel</Channel>
		<RecipientID></RecipientID>
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
	  </Acknowledgement>
  </InterchangeInfo>");

			EDIInterchange acknowlegementInterchange;
			EDIMessage acknowlegementMessage;
			ProcessMessage(interchangeWithACK, out acknowlegementMessage, out acknowlegementInterchange);

			var newFactory = new BusinessObjectFactory();
			var messages = newFactory.Load<EDIMessage>(new ZQuery());
			AssertEquals("messages.Count()", 1, messages.Length);
			var message = messages[0];
			AssertEquals(EDIMessageStatusList.Codes.Received, message.EM_Status);
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

		public void TestProcessShipmentWithAcknowledgement_Successfuly_AcknowledgeEAdaptorOnSuccess()
		{
			var interchangeWithACK = CreateInterchangeWithAcknowledgement(InterchangeAcknowledgement.InterchangeAcknowledgementRequiredList.OnSuccess, InterchangeAcknowledgement.InterchangeAcknowledgementChannelList.eAdaptor);

			EDIInterchange acknowlegementInterchange;
			EDIMessage acknowlegementMessage;
			ProcessMessage(interchangeWithACK, out acknowlegementMessage, out acknowlegementInterchange);

			AssertEquals(EDIMessageStatusList.Codes.Received, interchangeWithACK.ContainedMessages[0].EM_Status);
			AssertNotNull("Acknowledgement message was created", acknowlegementMessage);
			AssertNotNull("Acknowledgement interchange was created", acknowlegementInterchange);

			ValidateAknowledgementInterchangeAndMessage(acknowlegementMessage, acknowlegementInterchange, EDIMessageStatusList.Codes.Sent, EDIInterchangeStatusList.Codes.eAdaptorQueued, EDIInterchange.TransportType.eAdaptor);
		}

		public void TestProcessShipmentWith_Successfuly_AcknowledgeEHubOnSuccess()
		{
			using (Factory.AddDisposableService())
			{
				var interchangeWithACK = CreateInterchangeWithAcknowledgement(InterchangeAcknowledgement.InterchangeAcknowledgementRequiredList.OnSuccess, InterchangeAcknowledgement.InterchangeAcknowledgementChannelList.eHub);

				EDIInterchange acknowlegementInterchange;
				EDIMessage acknowlegementMessage;
				ProcessMessage(interchangeWithACK, out acknowlegementMessage, out acknowlegementInterchange);

				AssertEquals(EDIMessageStatusList.Codes.Received, interchangeWithACK.ContainedMessages[0].EM_Status);
				AssertNotNull("Acknowledgement message was created", acknowlegementMessage);
				AssertNotNull("Acknowledgement interchange was created", acknowlegementInterchange);

				ValidateAknowledgementInterchangeAndMessage(acknowlegementMessage, acknowlegementInterchange, EDIMessageStatusList.Codes.Sent, EDIInterchangeStatusList.Codes.eHubQueued, EDIInterchange.TransportType.eHub);
			}
		}

		public void TestProcessShipmentWith_Successfuly_AcknowledgeEHubOnAll()
		{
			var interchangeWithACK = CreateInterchangeWithAcknowledgement(InterchangeAcknowledgement.InterchangeAcknowledgementRequiredList.OnAll, InterchangeAcknowledgement.InterchangeAcknowledgementChannelList.eHub);

			EDIInterchange acknowlegementInterchange;
			EDIMessage acknowlegementMessage;
			ProcessMessage(interchangeWithACK, out acknowlegementMessage, out acknowlegementInterchange);

			AssertEquals(EDIMessageStatusList.Codes.Received, interchangeWithACK.ContainedMessages[0].EM_Status);
			AssertNotNull("Acknowledgement message was created", acknowlegementMessage);
			AssertNotNull("Acknowledgement interchange was created", acknowlegementInterchange);

			ValidateAknowledgementInterchangeAndMessage(acknowlegementMessage, acknowlegementInterchange, EDIMessageStatusList.Codes.Sent, EDIInterchangeStatusList.Codes.eHubQueued, EDIInterchange.TransportType.eHub);
		}

		public void TestProcessShipmentWith_Successfuly_AcknowledgeEHubOnError()
		{
			var interchangeWithACK = CreateInterchangeWithAcknowledgement(InterchangeAcknowledgement.InterchangeAcknowledgementRequiredList.OnError, InterchangeAcknowledgement.InterchangeAcknowledgementChannelList.eHub);

			EDIInterchange acknowlegementInterchange;
			EDIMessage acknowlegementMessage;
			ProcessMessage(interchangeWithACK, out acknowlegementMessage, out acknowlegementInterchange);

			AssertEquals(EDIMessageStatusList.Codes.Received, interchangeWithACK.ContainedMessages[0].EM_Status);
			AssertNull("Acknowledgement message should not be created", acknowlegementMessage);
			AssertNull("Acknowledgement interchange should not be created", acknowlegementInterchange);
		}

		public void TestProcessShipmentWith_Failed_AcknowledgeEHubOnError_ActionNotFound()
		{
			var interchangeWithACK = CreateInterchangeWithAcknowledgement(InterchangeAcknowledgement.InterchangeAcknowledgementRequiredList.OnError, InterchangeAcknowledgement.InterchangeAcknowledgementChannelList.eHub);

			EDIInterchange acknowlegementInterchange;
			EDIMessage acknowlegementMessage;
			ProcessMessage(interchangeWithACK, out acknowlegementMessage, out acknowlegementInterchange, returnNullMessageAction: true);

			AssertEquals(EDIMessageStatusList.Codes.Error, interchangeWithACK.ContainedMessages[0].EM_Status);
			AssertNotNull("Acknowledgement message was created", acknowlegementMessage);
			AssertNotNull("Acknowledgement interchange was created", acknowlegementInterchange);
		}

		public void TestProcessShipmentWith_Failed_AcknowledgeEHubOnAll_ActionNotFound()
		{
			var interchangeWithACK = CreateInterchangeWithAcknowledgement(InterchangeAcknowledgement.InterchangeAcknowledgementRequiredList.OnAll, InterchangeAcknowledgement.InterchangeAcknowledgementChannelList.eHub);

			EDIInterchange acknowlegementInterchange;
			EDIMessage acknowlegementMessage;
			ProcessMessage(interchangeWithACK, out acknowlegementMessage, out acknowlegementInterchange, returnNullMessageAction: true);

			AssertEquals(EDIMessageStatusList.Codes.Error, interchangeWithACK.ContainedMessages[0].EM_Status);
			AssertNotNull("Acknowledgement message was created", acknowlegementMessage);
			AssertNotNull("Acknowledgement interchange was created", acknowlegementInterchange);
		}

		public void TestProcessShipmentWith_Failed_AcknowledgeEHubOnSuccess_ActionNotFound()
		{
			var interchangeWithACK = CreateInterchangeWithAcknowledgement(InterchangeAcknowledgement.InterchangeAcknowledgementRequiredList.OnSuccess, InterchangeAcknowledgement.InterchangeAcknowledgementChannelList.eHub);

			EDIInterchange acknowlegementInterchange;
			EDIMessage acknowlegementMessage;
			ProcessMessage(interchangeWithACK, out acknowlegementMessage, out acknowlegementInterchange, returnNullMessageAction: true);

			AssertEquals(EDIMessageStatusList.Codes.Error, interchangeWithACK.ContainedMessages[0].EM_Status);
			AssertNull("Acknowledgement message should not be created", acknowlegementMessage);
			AssertNull("Acknowledgement interchange should not be created", acknowlegementInterchange);
		}

		public void TestProcessShipmentWith_Failed_AcknowledgeEHubOnError_ActionThrowException()
		{
			using (Factory.AddDisposableService())
			{
				var interchangeWithACK = CreateInterchangeWithAcknowledgement(InterchangeAcknowledgement.InterchangeAcknowledgementRequiredList.OnError, InterchangeAcknowledgement.InterchangeAcknowledgementChannelList.eHub);

				EDIInterchange acknowlegementInterchange;
				EDIMessage acknowlegementMessage;
				ProcessMessage(interchangeWithACK, out acknowlegementMessage, out acknowlegementInterchange, returnNullMessageAction: false, messageActionTrowException: new Exception("Some error"));

				ErrorReporter.Clear();
				AssertEquals(EDIMessageStatusList.Codes.Error, interchangeWithACK.ContainedMessages[0].EM_Status);
				AssertNotNull("Acknowledgement message was created", acknowlegementMessage);
				AssertNotNull("Acknowledgement interchange was created", acknowlegementInterchange);
			}
		}

		public void TestProcessShipmentWith_Successfuly_AcknowledgeEHubOnAll_WithNamespaces()
		{
			var interchangeWithACK = Factory.New<EDIInterchange>();
			interchangeWithACK.EI_InterchangeType = EDIInterchangeTypeList.Codes.XMS;
			interchangeWithACK.EI_From = "Sender";
			interchangeWithACK.EI_To = "Recipient";
			interchangeWithACK.EI_ApplicationCode = "XMS";

			interchangeWithACK.EI_HeaderText = @"<ns0:InterchangeInfo xmlns:ns0=""http://www.edi.com.au/EnterpriseService/""><ns0:Date>2013-12-23T17:29:41+11:00</ns0:Date><ns0:EDIOrganisation OwnerCode=""MATLCELLO""></ns0:EDIOrganisation><ns0:Acknowledgement><ns0:Required>OnAll</ns0:Required><ns0:Channel>eHub</ns0:Channel><ns0:RecipientID>TPODELHST_PKG</ns0:RecipientID><ns0:ContextCollection><ns0:Context><ns0:Type>MessageSenderIdentifier</ns0:Type><ns0:Value>MATLCELLO</ns0:Value></ns0:Context><ns0:Context><ns0:Type>MessageReceiverIdentifier</ns0:Type><ns0:Value>MATLFWDR</ns0:Value></ns0:Context><ns0:Context><ns0:Type>MessageTypeIdentifier</ns0:Type><ns0:Value>PKGLST</ns0:Value></ns0:Context><ns0:Context><ns0:Type>MessageNumber</ns0:Type><ns0:Value>OUBT80120120619015MA</ns0:Value></ns0:Context><ns0:Context><ns0:Type>MessageFunctionCode</ns0:Type><ns0:Value>9</ns0:Value></ns0:Context><ns0:Context><ns0:Type>DocumentDateTime</ns0:Type><ns0:Value>20120619104020</ns0:Value></ns0:Context></ns0:ContextCollection></ns0:Acknowledgement></ns0:InterchangeInfo>";

			var message = GetQueuedXMSOrderMessage("Order.xml");
			interchangeWithACK.ContainedMessages.Add((EDIMessage)message);
			Factory.Save();

			var interchangesBegore = Factory.Load<EDIInterchange>(new ZQuery());
			var messagesBegore = Factory.Load<EDIMessage>(new ZQuery());

			var notifications = ProcessMessages(false);

			var interchangesAfter = Factory.Load<EDIInterchange>(new ZQuery());
			var messagesAfter = Factory.Load<EDIMessage>(new ZQuery());

			var acknowlegementMessage = messagesAfter.Except(messagesBegore).SingleOrDefault();
			var acknowlegementInterchange = interchangesAfter.Except(interchangesBegore).SingleOrDefault();

			AssertNotNull(acknowlegementInterchange);
			AssertNotNull(acknowlegementMessage);
		}

		#region Implementation

		EDIInterchange CreateInterchangeWithAcknowledgement(InterchangeAcknowledgement.InterchangeAcknowledgementRequiredList required, InterchangeAcknowledgement.InterchangeAcknowledgementChannelList channel)
		{
			var interchangeWithACK = Factory.New<EDIInterchange>();
			interchangeWithACK.EI_InterchangeType = EDIInterchangeTypeList.Codes.XMS;
			interchangeWithACK.EI_From = "Sender";
			interchangeWithACK.EI_To = "Recipient";
			interchangeWithACK.EI_ApplicationCode = "XMS";

			interchangeWithACK.EI_HeaderText = string.Format(@"<InterchangeInfo xmlns=""http://www.edi.com.au/EnterpriseService/"">
	<Date>2013-11-28T15:37:56.893+11:00</Date>
	<XmlType>Verbose</XmlType>
	<Source/>
	<Target />
	  <Acknowledgement>
		<Required>{0}</Required>
		<Channel>{1}</Channel>
		<RecipientID>HYEDAUIKB</RecipientID>
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
	  </Acknowledgement>
  </InterchangeInfo>", required, channel);

			return interchangeWithACK;
		}

		void ProcessMessage(EDIInterchange interchangeWithACK, out EDIMessage acknowlegementMessage, out EDIInterchange acknowlegementInterchange, bool returnNullMessageAction = false, Exception messageActionTrowException = null)
		{
			var messageBRIBranch = GetQueuedXMSShipmentMessage("ShipmentBRE.xml");
			interchangeWithACK.ContainedMessages.Add((EDIMessage)messageBRIBranch);
			Factory.Save();

			var interchangesBegore = Factory.Load<EDIInterchange>(new ZQuery());
			var messagesBegore = Factory.Load<EDIMessage>(new ZQuery());

			var notifications = ProcessMessages(false, false, returnNullMessageAction, messageActionTrowException);

			var interchangesAfter = Factory.Load<EDIInterchange>(new ZQuery());
			var messagesAfter = Factory.Load<EDIMessage>(new ZQuery());

			acknowlegementMessage = messagesAfter.Except(messagesBegore).SingleOrDefault();
			acknowlegementInterchange = interchangesAfter.Except(interchangesBegore).SingleOrDefault();
			Factory.ReloadAll<EDIInterchange>();
			Factory.ReloadAll<EDIMessage>();
		}

		void ValidateAknowledgementInterchangeAndMessage(EDIMessage message, EDIInterchange interchange, string messageStatus, string interchangeStatus, string transportType)
		{
			AssertEquals(true, message.EM_IsActive);
			AssertEquals(ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
			AssertEquals(EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
			AssertEquals(EDIMessageSubTypeList.Codes.XmlUniversalEvent, message.EM_MessageSubType);
			AssertEquals(EDICommunicationsModeCommsDirectionList.Codes.Transmit, message.EM_ReceiveTransmit);
			AssertEquals(messageStatus, message.EM_Status);
			AssertEquals(
				"<UniversalEvent xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\" version=\"1.1\">\r\n  <Event>\r\n    <DataContext>\r\n\r\n      <Company>\r\n        <Code>EDI</Code>\r\n        <Country>\r\n          <Code>AU</Code>\r\n          <Name>Australia</Name>\r\n        </Country>\r\n        <Name>Eagle Datamation International</Name>\r\n      </Company>\r\n      <DataProvider>EDIDATEDI</DataProvider>\r\n      <EnterpriseID>EDI</EnterpriseID>\r\n      <ServerID>DAT</ServerID>\r\n    </DataContext>\r\n\r\n    <EventTime>2013-12-05T13:03:28.927</EventTime>\r\n    <EventType>DIM</EventType>\r\n    <ContextCollection>\r\n      <Context>\r\n        <Type>Foo</Type>\r\n        <Value>CS908217349087123</Value>\r\n      </Context>\r\n      <Context>\r\n        <Type>OriginalMessage</Type>\r\n        <Value>Blalalalallalalalal</Value>\r\n      </Context>\r\n      <Context>\r\n        <Type>DataImportLog</Type>\r\n        <Value>Combining message text\r\nRunning import\r\nImporting shipment with House Bill 'TESTBNESHIPMENT'\r\nSuccessfully matched organization with code 'ABIGAS', Mapping Organization: EDICUS, Matching by Foreign code: ABIGASBNE, Using: Similarity Matcher, Found match: True\r\nSuccessfully matched organization with code 'ADEBOX', Mapping Organization: EDICUS, Matching by Foreign code: ADEBOXAKL, Using: Similarity Matcher, Found match: True\r\nShipment (House Bill='TESTBNESHIPMENT') created\r\nLink message to Job\r\nImport finished\r\n</Value>\r\n      </Context>\r\n      <Context>\r\n        <Type>ProcessingResultStatus</Type>\r\n        <Value>RCV</Value>\r\n      </Context>\r\n    </ContextCollection>\r\n" +
				$@"    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{message.Interchange.EI_SessionGUID}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{message.Interchange.EI_InterchangeNum}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{message.EM_MessageNum}</MessageNumber>
    </MessageNumberCollection>
" +
				"  </Event>\r\n</UniversalEvent>\r\n",
				ReplaceEventTime(message.EM_MessageText, "2013-12-05T13:03:28.927"));

			AssertEquals(true, interchange.EI_IsActive);
			AssertEquals(ApplicationCodeList.Codes.UniversalDataMessaging, interchange.EI_ApplicationCode);
			AssertEquals(EDIInterchangeTypeList.Codes.XDC, interchange.EI_InterchangeType);
			AssertEquals(EDICommunicationsModeCommsDirectionList.Codes.Transmit, interchange.EI_ReceiveTransmit);
			AssertEquals(interchangeStatus, interchange.EI_Status);
			AssertEquals(transportType, interchange.EI_TransportType);
			AssertEquals("EDIEDIDAT", interchange.EI_From);
			AssertEquals("HYEDAUIKB", interchange.EI_To);
			AssertEquals("<EDIDelivery><FileName></FileName><EmailSubject></EmailSubject></EDIDelivery>",
				interchange.EI_HeaderNText);
			AssertEquals(
				"<UniversalInterchange xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\" version=\"1.1\">\r\n  <Header>\r\n    <SenderID>EDIEDIDAT</SenderID>\r\n    <RecipientID>HYEDAUIKB</RecipientID>\r\n  </Header>\r\n  <Body>\r\n    <UniversalEvent xmlns=\"http://www.cargowise.com/Schemas/Universal/2011/11\" version=\"1.1\">\r\n  <Event>\r\n    <DataContext>\r\n\r\n      <Company>\r\n        <Code>EDI</Code>\r\n        <Country>\r\n          <Code>AU</Code>\r\n          <Name>Australia</Name>\r\n        </Country>\r\n        <Name>Eagle Datamation International</Name>\r\n      </Company>\r\n      <DataProvider>EDIDATEDI</DataProvider>\r\n      <EnterpriseID>EDI</EnterpriseID>\r\n      <ServerID>DAT</ServerID>\r\n    </DataContext>\r\n\r\n    <EventTime>2013-12-05T13:20:44.283</EventTime>\r\n    <EventType>DIM</EventType>\r\n    <ContextCollection>\r\n      <Context>\r\n        <Type>Foo</Type>\r\n        <Value>CS908217349087123</Value>\r\n      </Context>\r\n      <Context>\r\n        <Type>OriginalMessage</Type>\r\n        <Value>Blalalalallalalalal</Value>\r\n      </Context>\r\n      <Context>\r\n        <Type>DataImportLog</Type>\r\n        <Value>Combining message text\r\nRunning import\r\nImporting shipment with House Bill 'TESTBNESHIPMENT'\r\nSuccessfully matched organization with code 'ABIGAS', Mapping Organization: EDICUS, Matching by Foreign code: ABIGASBNE, Using: Similarity Matcher, Found match: True\r\nSuccessfully matched organization with code 'ADEBOX', Mapping Organization: EDICUS, Matching by Foreign code: ADEBOXAKL, Using: Similarity Matcher, Found match: True\r\nShipment (House Bill='TESTBNESHIPMENT') created\r\nLink message to Job\r\nImport finished\r\n</Value>\r\n      </Context>\r\n      <Context>\r\n        <Type>ProcessingResultStatus</Type>\r\n        <Value>RCV</Value>\r\n      </Context>\r\n    </ContextCollection>\r\n" +
				$@"    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{message.Interchange.EI_SessionGUID}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{message.Interchange.EI_InterchangeNum}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{message.EM_MessageNum}</MessageNumber>
    </MessageNumberCollection>
" +
				"  </Event>\r\n</UniversalEvent>\r\n  </Body>\r\n</UniversalInterchange>",
				ReplaceEventTime(interchange.EI_BodyText, "2013-12-05T13:20:44.283"));
			AssertEquals("", interchange.EI_FooterText);
		}

		string ReplaceEventTime(string text, string dateString)
		{
			string startTag = "<EventTime>";
			string endTag = "</EventTime>";
			int startPos = text.IndexOf(startTag);
			int endPos = text.IndexOf(endTag);

			return text.Substring(0, startPos + startTag.Length) + dateString + text.Substring(endPos, text.Length - endPos);
		}

		#endregion
	}
}
