using System;
using System.IO;
using System.Linq;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.eHubMessaging.Business.DownloadHandler;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.DownloadHandler
{
	class UniversalInterchangeHeaderHandlerTests : TestCaseWithFactory
	{
		public void TestUniversalActivityHandler()
		{
			using (eAdaptorRegistry.Instance.FileNameAttachToNotesEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var dataStream = ResourceManager.GetFileResource("TestFiles.UniversalActivity.xml"))
			{
				var message = new Mock<IeHubMessage>();
				var trackingID = new Guid("728e874f-8f09-4860-a2a0-082eb62743a5");
				message.Setup(m => m.TrackingID).Returns(trackingID);
				message.Setup(m => m.SenderID).Returns("SenderID");
				message.Setup(m => m.RecipientID).Returns("RecipientID");
				message.Setup(m => m.SchemaName).Returns("Schema");
				message.Setup(m => m.MessageStream).Returns(dataStream);
				message.Setup(m => m.Filename).Returns("FileName");

				var handler = new Mock<UniversalInterchangeHeaderHandler>() { CallBase = true };
				var notification = new NotificationBuffer();
				handler.Object.SaveMessage(message.Object, TestHelpers.ValidCompanyForTest(handler.Object.FactoryProvider.Current), notification);
				AssertNativeEDIInterchange(trackingID, "UniversalActivityHeader.xml", "UniversalActivityBody.xml", @"<UniversalActivity xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Activity>
		<DataContext>
			<DataSourceCollection>
				<DataSource>
					<Type>CustomerServiceTicket</Type>
				</DataSource>
			</DataSourceCollection>
		</DataContext>

		<Summary>Celebrity Baby Plastic Surgery Disasters</Summary>
		<OrganizationAddressCollection>
			<OrganizationAddress>
				<AddressType>Client</AddressType>
				<Address1>#1ASFSDF</Address1>
				<Address2></Address2>
				<AddressOverride>false</AddressOverride>
				<AddressShortCode>#1</AddressShortCode>
				<City>1</City>
				<CompanyName>111</CompanyName>
				<Contact>Dave</Contact>
				<Country>
					<Code>AU</Code>
					<Name>Australia</Name>
				</Country>
				<Email>dave@alkj.com</Email>
				<Fax></Fax>
				<Mobile></Mobile>
				<OrganizationCode>111SYD</OrganizationCode>
				<Phone>+61795555555</Phone>
				<Port>
					<Code>AUSYD</Code>
					<Name>Sydney</Name>
				</Port>
				<Postcode>1</Postcode>
				<ScreeningStatus>
					<Code>UNK</Code>
					<Description>Unknown</Description>
				</ScreeningStatus>
				<State>NSW</State>
			</OrganizationAddress>
		</OrganizationAddressCollection>
	</Activity>
</UniversalActivity>", true);
				AssertEquals(notification.AsString, "");

				handler.VerifyAll();
			}
		}

		public void TestUniversalActivityHandlerWithFileNameAttachToNotesDisabledByDefault()
		{
			using (var dataStream = ResourceManager.GetFileResource("TestFiles.UniversalActivity.xml"))
			{
				var message = new Mock<IeHubMessage>();
				var trackingID = new Guid("728e874f-8f09-4860-a2a0-082eb62743a5");
				message.Setup(m => m.TrackingID).Returns(trackingID);
				message.Setup(m => m.SenderID).Returns("SenderID");
				message.Setup(m => m.RecipientID).Returns("RecipientID");
				message.Setup(m => m.SchemaName).Returns("Schema");
				message.Setup(m => m.MessageStream).Returns(dataStream);
				message.Setup(m => m.Filename).Returns("FileName");

				var handler = new Mock<UniversalInterchangeHeaderHandler>() { CallBase = true };
				var notification = new NotificationBuffer();
				handler.Object.SaveMessage(message.Object, TestHelpers.ValidCompanyForTest(handler.Object.FactoryProvider.Current), notification);
				AssertNativeEDIInterchange(trackingID, "UniversalActivityHeader.xml", "UniversalActivityBody.xml", @"<UniversalActivity xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Activity>
		<DataContext>
			<DataSourceCollection>
				<DataSource>
					<Type>CustomerServiceTicket</Type>
				</DataSource>
			</DataSourceCollection>
		</DataContext>

		<Summary>Celebrity Baby Plastic Surgery Disasters</Summary>
		<OrganizationAddressCollection>
			<OrganizationAddress>
				<AddressType>Client</AddressType>
				<Address1>#1ASFSDF</Address1>
				<Address2></Address2>
				<AddressOverride>false</AddressOverride>
				<AddressShortCode>#1</AddressShortCode>
				<City>1</City>
				<CompanyName>111</CompanyName>
				<Contact>Dave</Contact>
				<Country>
					<Code>AU</Code>
					<Name>Australia</Name>
				</Country>
				<Email>dave@alkj.com</Email>
				<Fax></Fax>
				<Mobile></Mobile>
				<OrganizationCode>111SYD</OrganizationCode>
				<Phone>+61795555555</Phone>
				<Port>
					<Code>AUSYD</Code>
					<Name>Sydney</Name>
				</Port>
				<Postcode>1</Postcode>
				<ScreeningStatus>
					<Code>UNK</Code>
					<Description>Unknown</Description>
				</ScreeningStatus>
				<State>NSW</State>
			</OrganizationAddress>
		</OrganizationAddressCollection>
	</Activity>
</UniversalActivity>", false);
				AssertEquals(notification.AsString, "");

				handler.VerifyAll();
			}
		}

		public void TestUniversalScheduleHandler()
		{
			using (eAdaptorRegistry.Instance.FileNameAttachToNotesEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var dataStream = ResourceManager.GetFileResource("TestFiles.UniversalSchedule.xml"))
			{
				var message = new Mock<IeHubMessage>();
				var trackingID = new Guid("728e874f-8f09-4860-a2a0-082eb62743a5");
				message.Setup(m => m.TrackingID).Returns(trackingID);
				message.Setup(m => m.SenderID).Returns("SenderID");
				message.Setup(m => m.RecipientID).Returns("RecipientID");
				message.Setup(m => m.SchemaName).Returns("Schema");
				message.Setup(m => m.MessageStream).Returns(dataStream);
				message.Setup(m => m.Filename).Returns("FileName");

				var handler = new Mock<UniversalInterchangeHeaderHandler>() { CallBase = true };
				var notification = new NotificationBuffer();
				handler.Object.SaveMessage(message.Object, TestHelpers.ValidCompanyForTest(handler.Object.FactoryProvider.Current), notification);
				AssertNativeEDIInterchange(trackingID, "UniversalScheduleHeader.xml", "UniversalScheduleBody.xml", @"<UniversalSchedule xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	  <Schedule>
		<Carrier>
		  <AddressType>Carrier</AddressType>
		  <OrganizationCode>OFR</OrganizationCode>
		  <CompanyName>CMA-CGM</CompanyName>
		</Carrier>
		<DataProvider>DAK</DataProvider>
		<IsCancellation>false</IsCancellation>
		<Transport>
		  <Sea>
			<Vessel>
			  <VesselName>GRANDE NIGERIA</VesselName>
			  <LloydsNumber>9130937</LloydsNumber>
			</Vessel>
			<VoyageNumber>OFR1125</VoyageNumber>
		  </Sea>
		</Transport>
		<DischargeCollection>
		  <Discharge>
			<Port>
			  <Code>SNDKR</Code>
			</Port>
			<EstimatedArrival>2012-08-02T00:00:00</EstimatedArrival>
		  </Discharge>
		  <Discharge>
			<Port>
			  <Code>NGLOS</Code>
			</Port>
			<EstimatedArrival>2012-08-08T00:00:00</EstimatedArrival>
		  </Discharge>
		  <Discharge>
			<Port>
			  <Code>NGTIN</Code>
			</Port>
			<EstimatedArrival>2012-08-08T00:00:00</EstimatedArrival>
		  </Discharge>
		  <Discharge>
			<Port>
			  <Code>NGAPP</Code>
			</Port>
			<EstimatedArrival>2012-08-08T00:00:00</EstimatedArrival>
		  </Discharge>
		  <Discharge>
			<Port>
			  <Code>GHTEM</Code>
			</Port>
			<EstimatedArrival>2012-08-11T00:00:00</EstimatedArrival>
		  </Discharge>
		  <Discharge>
			<Port>
			  <Code>CIABJ</Code>
			</Port>
			<EstimatedArrival>2012-08-14T00:00:00</EstimatedArrival>
		  </Discharge>
		  <Discharge>
			<Port>
			  <Code>GHTKD</Code>
			</Port>
			<EstimatedArrival>2012-08-18T00:00:00</EstimatedArrival>
		  </Discharge>
		</DischargeCollection>
		<LoadingCollection>
		  <Loading>
			<Port>
			  <Code>NLRTM</Code>
			</Port>
			<EstimatedArrival>2012-07-01T00:00:00</EstimatedArrival>
			<EstimatedDeparture>2012-07-02T00:00:00</EstimatedDeparture>
		  </Loading>
		  <Loading>
			<Port>
			  <Code>DEBRV</Code>
			</Port>
			<EstimatedDeparture>2012-07-16T00:00:00</EstimatedDeparture>
			<TerminalCode>XXX</TerminalCode>
		  </Loading>
		  <Loading>
			<Port>
			  <Code>DEHAM</Code>
			</Port>
			<EstimatedDeparture>2012-07-23T00:00:00</EstimatedDeparture>
			<TerminalCode>EUR</TerminalCode>
			<TerminalName>Eurogate Container Terminal Hamburg</TerminalName>
		  </Loading>
		</LoadingCollection>
	  </Schedule>
	</UniversalSchedule>", true);
				AssertEquals(notification.AsString, "");

				handler.VerifyAll();
			}
		}

		public void TestUniversalEventHeaderHandler()
		{
			using (eAdaptorRegistry.Instance.FileNameAttachToNotesEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (Stream nativeEventHeaderMessage = ResourceManager.GetFileResource("TestFiles.UniversalEvent.xml"))
			{
				var message = new Mock<IeHubMessage>();
				Guid trackingID = new Guid("728e874f-8f09-4860-a2a0-082eb62743a5");
				message.Setup(m => m.TrackingID).Returns(trackingID);
				message.Setup(m => m.SenderID).Returns("SenderID");
				message.Setup(m => m.RecipientID).Returns("RecipientID");
				message.Setup(m => m.SchemaName).Returns("Schema");
				message.Setup(m => m.MessageStream).Returns(nativeEventHeaderMessage);
				message.Setup(m => m.Filename).Returns("FileName");

				var handler = new Mock<UniversalInterchangeHeaderHandler>() { CallBase = true };

				var notification = new NotificationBuffer();
				handler.Object.SaveMessage(message.Object, TestHelpers.ValidCompanyForTest(handler.Object.FactoryProvider.Current), notification);
				AssertNativeEDIInterchange(trackingID, "UniversalEventHeader.xml", "UniversalEventBody.xml", @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.0"">
  <Event>
	<DataContext>
	  <DataSourceCollection>
		<DataSource>
		  <Type>ForwardingConsol</Type>
		  <Key>C00001010</Key>
		</DataSource>
	  </DataSourceCollection>

	  <ActionPurpose>
		<Code>EVT</Code>
		<Description>Event</Description>
	  </ActionPurpose>
	  <Company>
		<Code>EDI</Code>
		<Name>Eagle Datamation International</Name>
	  </Company>
	  <EnterpriseID>EDI</EnterpriseID>
	  <EventType>
		<Code>RCV</Code>
		<Description>Received Into Store</Description>
	  </EventType>
	  <ServerID>DAT</ServerID>
	  <TriggerDescription>Received Goods</TriggerDescription>
	  <TriggerType>Trigger</TriggerType>
	</DataContext>

	<EventTime>2010-12-25T00:00:00</EventTime>
	<EventType>RCV</EventType>
	<DataProvider>HYEAYADAU</DataProvider>
	<IsEstimate>false</IsEstimate>

	<ContextCollection>
	  <Context>
		<Type>MBOLNumber</Type>
		<Value>MB123456</Value>
	  </Context>
	  <Context>
		<Type>MBOLOriginUNLOCO</Type>
		<Value>AUSYD</Value>
	  </Context>
	  <Context>
		<Type>MBOLDestinationUNLOCO</Type>
		<Value>NZAKL</Value>
	  </Context>
	</ContextCollection>
  </Event>
  </UniversalEvent>", true);
				AssertEquals(notification.AsString, "");

				handler.VerifyAll();
			}
		}

		void AssertNativeEDIInterchange(Guid trackingID, string headerFileName, string bodyFileName, string expectedMessage, bool fileNameAttachToNotesEnabled)
		{
			EDIInterchange interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_SessionGUID, trackingID));
			Assert(interchange != null);
			AssertEquals("interchange.EI_ApplicationCode", "UDM", interchange.EI_ApplicationCode);
			AssertEquals("interchange.EI_InterchangeType", "XDC", interchange.EI_InterchangeType);
			AssertEquals("interchange.EI_ReceiveTransmit", "RCV", interchange.EI_ReceiveTransmit);
			AssertEquals("interchange.EI_From", "SenderID", interchange.EI_From);
			AssertEquals("interchange.EI_To", "RecipientID", interchange.EI_To);
			AssertEquals("interchange.EI_Status", "RCV", interchange.EI_Status);

			AssertEquals("header is different", true, MessageHandlerTestHelper.CompareXmlString(ResourceManager.GetFileResourceString("TestFiles." + headerFileName).Trim(), interchange.EI_HeaderNText.ToString()));
			AssertEquals("body is different", true, MessageHandlerTestHelper.CompareXmlString(ResourceManager.GetFileResourceString("TestFiles." + bodyFileName).Trim(), interchange.EI_BodyText.ToString()));
			AssertEquals("interchange.EI_FooterNText", "", interchange.EI_FooterNText.ToString().Trim());
			var expectedInterchangeNote = interchange.Notes.FindByDescription("File Name");
			if (fileNameAttachToNotesEnabled)
			{
				AssertEquals("ediMessage should have a note of description 'File Name'", 1, expectedInterchangeNote.Length);
				AssertEquals("The Note should contain file name", "FileName", expectedInterchangeNote[0].ST_NoteDataAsText);
			} else
			{
				AssertEquals("ediMessage should have a note of description 'File Name'", 0, expectedInterchangeNote.Length);
			}

			EDIMessage[] ediMessages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchange.PK));
			AssertEquals("ediMessages.Length", 1, ediMessages.Length);
			var ediMessage = ediMessages[0];
			AssertEquals("ediMessage.EM_ApplicationCode", "UDM", ediMessage.EM_ApplicationCode);
			AssertEquals("ediMessage.EM_MessageType", "XDC", ediMessage.EM_MessageType);
			AssertEquals("ediMessage.EM_ReceiveTransmit", "RCV", ediMessage.EM_ReceiveTransmit);
			AssertEquals("ediMessage.EM_Status", "QUE", ediMessage.EM_Status);
			var expectedNote = ediMessage.Notes.FindByDescription("File Name");
			if (fileNameAttachToNotesEnabled)
			{
				AssertEquals("ediMessage should have a note of description 'File Name'", 1, expectedNote.Length);
				AssertEquals("The Note should contain file name", "FileName", expectedNote[0].ST_NoteDataAsText);
			} else
			{
				AssertEquals("ediMessage should have a note of description 'File Name'", 0, expectedNote.Length);
			}

			AssertEquals("ediMessage.EM_MessageText is different from expectedMessage", true, MessageHandlerTestHelper.CompareXmlString(expectedMessage.Trim(), ediMessage.EM_MessageText.ToString().Trim()));
		}

		[TestDate(2014, 1, 29, 15, 16, 17)]
		public void TestSendAcknowledgement()
		{
			using (Factory.AddDisposableService())
			{
				AssertEquals("Precondition", 0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

				var fileName = "UniversalEventsWithAcknowledgementRequestAndInvalidNodesInBody.xml";
				var eHubMessage = CreateeHubUniversalXMLMesage(EDIInterchangeTypeList.Descriptions.XDC, ResourceManager.GetFileResource("TestFiles." + fileName));
				var notification = new NotificationBuffer();

				var handler = HandlerFactory.GetHandler(eHubMessage.SchemaName) as UniversalInterchangeHeaderHandler;
				AssertNotNull(handler);
				using (handler.FactoryProvider.Current.AddDisposableService())
				{
					var company = TestHelpers.ValidCompanyForTest(handler.FactoryProvider.Current);
					handler.SaveMessage(eHubMessage, company, notification);

					CombineAssertions(delegate
					{
						AssertEquals(notification.AsString, "");
						AssertEquals(2, Factory.GetDatabaseCount(typeof(EDIInterchange)));
						var interchanges = Factory.Load<EDIInterchange>(new ZQuery()).OrderByDescending(e => e.EI_InterchangeNum).ToArray();

						var interchange = interchanges[0];

						AssertEquals("interchange.EI_Status", EDIInterchange.Status.Failed, interchange.EI_Status);
						var expectedInterchangeHeader =
		@"<Header xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<SenderID>test</SenderID>
	<RecipientID>HYEDAUAYA</RecipientID>
	<Acknowledgement>
	  <Required>OnAll</Required>
	  <!-- Options: OnAll, OnError, OnSuccess -->
	  <Channel>eAdaptor</Channel>
	  <!-- Options: eHub, eAdaptor -->
	  <RecipientID>EDIEDIDAT</RecipientID>
	  <ContextCollection>
		<Context>
		  <Type>Good Fake President</Type>
		  <Value>Harrison Ford</Value>
		</Context>
	  </ContextCollection>
	</Acknowledgement>
  </Header>";
						Assert("interchange header", MessageHandlerTestHelper.CompareXmlString(expectedInterchangeHeader, interchange.EI_HeaderNText));
						AssertEquals("interchange footer should be empty", string.Empty, interchange.EI_FooterNText);
						using (var bodyTextReader = interchange.GetEI_BodyTextReader())
						{
							AssertEquals("interchange body text should be same as file content", true, MessageHandlerTestHelper.CompareXmlString(ResourceManager.GetFileResourceString("TestFiles." + fileName).Trim(), bodyTextReader.ReadToEnd()));
						}
						AssertEquals(EDIInterchange.ApplicationCodes.UniversalDataMessaging, interchange.EI_ApplicationCode);
						AssertEquals(EDIInterchangeTypeList.Codes.XDC, interchange.EI_InterchangeType);
						AssertEquals(EDIInterchange.Direction.Receive, interchange.EI_ReceiveTransmit);
						AssertEquals(eHubMessage.SenderID, interchange.EI_From);
						AssertEquals(eHubMessage.RecipientID, interchange.EI_To);
						AssertEquals(company.Branches.FirstOrDefault().PK, interchange.EI_GB);
						AssertEquals(eHubMessage.TrackingID, interchange.EI_SessionGUID);
						AssertEquals(true, interchange.EI_IsActive);

						var acknowledgementInterchange = interchanges[1];

						AssertEquals("acknowledgementInterchange.EI_Status", EDIInterchange.Status.eAdaptorQueued, acknowledgementInterchange.EI_Status);
						AssertEquals("acknowledgementInterchange.EI_TransportType", EDIInterchange.TransportType.eAdaptor, acknowledgementInterchange.EI_TransportType);
						AssertEquals("<EDIDelivery><FileName></FileName><EmailSubject></EmailSubject></EDIDelivery>", acknowledgementInterchange.EI_HeaderNText);
						AssertEquals("acknowledgementInterchange footer should be empty", string.Empty, acknowledgementInterchange.EI_FooterNText);
						#region expectedAcknowledgementInterchangeBodyText

						var expectedAcknowledgementInterchangeBodyText =
		$@"<UniversalInterchange xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Header>
	<SenderID>EDIEDIDAT</SenderID>
	<RecipientID>EDIEDIDAT</RecipientID>
  </Header>
  <Body>
	<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
	<DataContext>

	  <Company>
		<Code>EDI</Code>
		<Country>
		  <Code>AU</Code>
		  <Name>Australia</Name>
		</Country>
		<Name>Eagle Datamation International</Name>
	  </Company>
	  <DataProvider>EDIDATEDI</DataProvider>
	  <EnterpriseID>EDI</EnterpriseID>
	  <ServerID>DAT</ServerID>
	</DataContext>

	<EventTime>2014-01-29T15:16:17.000+00:00</EventTime>
	<EventType>DIM</EventType>

	<ContextCollection>
	  <Context>
		<Type>Good Fake President</Type>
		<Value>Harrison Ford</Value>
	  </Context>
	  <Context>
		<Type>DataImportLog</Type>
		<Value>Length of the message content is 1401 before Get Payload Sub Type
An error occurred: The 'DataTargetCollection' start tag on line 23 position 12 does not match the end tag of 'DataTargetCollectio'. Line 28, position 15.</Value>
	  </Context>
	  <Context>
		<Type>ProcessingResultStatus</Type>
		<Value>FAL</Value>
	  </Context>
	</ContextCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{acknowledgementInterchange.EI_SessionGUID}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{acknowledgementInterchange.EI_InterchangeNum}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{acknowledgementInterchange.ContainedMessages[0].EM_MessageNum}</MessageNumber>
    </MessageNumberCollection>
  </Event>
</UniversalEvent>
  </Body>
</UniversalInterchange>";

						#endregion
						using (var bodyTextReader = acknowledgementInterchange.GetEI_BodyTextReader())
						{
							Assert("acknowledgementInterchange body text", MessageHandlerTestHelper.CompareXmlString(expectedAcknowledgementInterchangeBodyText, bodyTextReader.ReadToEnd()));
						}
						AssertEquals(EDIInterchange.ApplicationCodes.UniversalDataMessaging, acknowledgementInterchange.EI_ApplicationCode);
						AssertEquals(EDIInterchangeTypeList.Codes.XDC, acknowledgementInterchange.EI_InterchangeType);
						AssertEquals(EDIInterchange.Direction.Transmit, acknowledgementInterchange.EI_ReceiveTransmit);
						AssertEquals("EDIEDIDAT", acknowledgementInterchange.EI_From);
						AssertEquals("EDIEDIDAT", acknowledgementInterchange.EI_To);
						AssertEquals(GlbCompany.CurrentCompany.Branches.FirstOrDefault().PK, acknowledgementInterchange.EI_GB);
						Assert("acknowledgementInterchange sessionGUID should be set with a GUID", acknowledgementInterchange.EI_SessionGUID.IsValid);
						AssertEquals(true, acknowledgementInterchange.EI_IsActive);

						AssertEquals("acknowledgementInterchange.ContainedMessages", 1, acknowledgementInterchange.ContainedMessages.Count);
						var acknowledgementMessage = acknowledgementInterchange.ContainedMessages[0];
						var failedMessage = $@"Failed to create message - Interchange Session GUID - {interchange.EI_SessionGUID}, Sender - {interchange.EI_From}, Recipient - {interchange.EI_To}, Schema - http://www.cargowise.com/Schemas/Universal#UniversalInter".Trim();
						AssertEquals("acknowledgementMessage.EM_Status", EDIMessage.Status.Sent, acknowledgementMessage.EM_Status);
						#region expectedAcknowledgementMessageText

						var expectedAcknowledgementMessageText =
		$@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
	<DataContext>

	  <Company>
		<Code>EDI</Code>
		<Country>
		  <Code>AU</Code>
		  <Name>Australia</Name>
		</Country>
		<Name>Eagle Datamation International</Name>
	  </Company>
	  <DataProvider>EDIDATEDI</DataProvider>
	  <EnterpriseID>EDI</EnterpriseID>
	  <ServerID>DAT</ServerID>
	</DataContext>

	<EventTime>2014-01-29T15:16:17.000+00:00</EventTime>
	<EventType>DIM</EventType>

	<ContextCollection>
	  <Context>
		<Type>Good Fake President</Type>
		<Value>Harrison Ford</Value>
	  </Context>
	  <Context>
		<Type>DataImportLog</Type>
		<Value>Length of the message content is 1401 before Get Payload Sub Type
An error occurred: The 'DataTargetCollection' start tag on line 23 position 12 does not match the end tag of 'DataTargetCollectio'. Line 28, position 15.</Value>
	  </Context>
	  <Context>
		<Type>ProcessingResultStatus</Type>
		<Value>FAL</Value>
	  </Context>
	</ContextCollection>
    <MessageNumberCollection>
      <MessageNumber Type=""TrackingID"">{acknowledgementInterchange.EI_SessionGUID}</MessageNumber>
      <MessageNumber Type=""InterchangeNumber"">{acknowledgementInterchange.EI_InterchangeNum}</MessageNumber>
      <MessageNumber Type=""MessageNumber"">{acknowledgementMessage.EM_MessageNum}</MessageNumber>
    </MessageNumberCollection>
  </Event>
</UniversalEvent>";

						#endregion
						using (var messageTextReader = acknowledgementMessage.GetEM_MessageTextReader())
						{
							Assert("acknowledgementMessage message text", MessageHandlerTestHelper.CompareXmlString(expectedAcknowledgementMessageText, messageTextReader.ReadToEnd()));
						}
						AssertEquals(ApplicationCodeList.Codes.UniversalDataMessaging, acknowledgementMessage.EM_ApplicationCode);
						AssertEquals(EDIMessageTypeList.Codes.XDC, acknowledgementMessage.EM_MessageType);
						AssertEquals(EDIMessageSubTypeList.Codes.XmlUniversalEvent, acknowledgementMessage.EM_MessageSubType);
						AssertEquals(EDIMessage.Direction.Transmit, acknowledgementMessage.EM_ReceiveTransmit);
						AssertEquals(acknowledgementInterchange.EI_TransportType, acknowledgementMessage.EM_TransportType);
						AssertEquals(true, acknowledgementMessage.EM_IsActive);
					});
				}
			}
		}

		public void TestSyntexRejectWithoutUniversalInterchange()
		{
			using (Stream nativeEventHeaderMessage = ResourceManager.GetFileResource("TestFiles.UniversalEventBody.xml"))
			{
				var message = new Mock<IeHubMessage>();
				Guid trackingID = new Guid("728e874f-8f09-4860-a2a0-082eb62743a5");
				message.Setup(m => m.TrackingID).Returns(trackingID);
				message.Setup(m => m.SenderID).Returns("SenderID");
				message.Setup(m => m.RecipientID).Returns("RecipientID");
				message.Setup(m => m.SchemaName).Returns("Schema");
				message.Setup(m => m.MessageStream).Returns(nativeEventHeaderMessage);
				var handler = new Mock<UniversalInterchangeHeaderHandler>() { CallBase = true };

				var notification = new NotificationBuffer();
				handler.Object.SaveMessage(message.Object, TestHelpers.ValidCompanyForTest(handler.Object.FactoryProvider.Current), notification);
				CombineAssertions(delegate
				{
					var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());

					AssertEquals("Total Notes if processing failed.", 1, interchange.Notes.DatabaseCount);
					var note = (StmNote)interchange.Notes.GetAllNotes().ToList()[0];
					AssertEquals("Note description if having syntax error.", "Failure Log", note.ST_Description.ToString());
					AssertEquals("interchange.EI_Status", EDIInterchange.Status.SyntaxRejected, interchange.EI_Status);
					AssertContains("Invalid SOAP Request: Universal Interchange is missing", note.ST_NoteDataAsText.ToString());
				});
			}

			using (Stream nativeEventHeaderMessage = ResourceManager.GetFileResource("TestFiles.UniversalEventHeader.xml"))
			{
				var message = new Mock<IeHubMessage>();
				Guid trackingID = new Guid("728e874f-8f09-4860-a2a0-082eb62743a5");
				message.Setup(m => m.TrackingID).Returns(trackingID);
				message.Setup(m => m.SenderID).Returns("SenderID");
				message.Setup(m => m.RecipientID).Returns("RecipientID");
				message.Setup(m => m.SchemaName).Returns("Schema");
				message.Setup(m => m.MessageStream).Returns(nativeEventHeaderMessage);
				var handler = new Mock<UniversalInterchangeHeaderHandler>() { CallBase = true };

				var notification = new NotificationBuffer();
				handler.Object.SaveMessage(message.Object, TestHelpers.ValidCompanyForTest(handler.Object.FactoryProvider.Current), notification);
				CombineAssertions(delegate
				{
					var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());

					AssertEquals("Total Notes if processing failed.", 1, interchange.Notes.DatabaseCount);
					var note = (StmNote)interchange.Notes.GetAllNotes().ToList()[0];
					AssertEquals("Note description if having syntax error.", "Failure Log", note.ST_Description.ToString());
					AssertEquals("interchange.EI_Status", EDIInterchange.Status.SyntaxRejected, interchange.EI_Status);
					AssertContains("Invalid SOAP Request: Universal Interchange is missing", note.ST_NoteDataAsText.ToString());
				});
			}
		}

		public static IeHubMessage CreateeHubUniversalXMLMesage(string schemaName, Stream stream)
		{
			return new eHubMessage(Guid.NewGuid(), "Sender1", "Recipient1", MessageSchemaType.Xml, ApplicationCodeList.Codes.UniversalDataMessaging, schemaName, stream);
		}
	}
}
