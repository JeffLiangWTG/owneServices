using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.GB.DataTransfer.Universal.Testing
{
	public sealed class GBCTCNctsHeaderEventParentFinderTest : TestCaseWithFactory
	{
		public void TestDataProviderFiltering()
		{
			AssertDataProviderFiltering();

			originalHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			AssertDataProviderFiltering();
		}

		public void AssertDataProviderFiltering()
		{
			var nextMessage = SetupOutgoingMessage(originalHeader);
			var finder = GBCTCNctsHeaderEventParentFinderTestHelper.New(Factory);
			var uEvent = GetUniversalEventFromXml(PopulateXml(ValidXMLEvent, nextMessage.Interchange.EI_SessionGUID, context: PopulateContext(), dataProvider: "CTCGB"));

			var header = finder.FindHeaderAndProcess_Exposed(uEvent);
			AssertNotNull("Found header", header);
			AssertSame(originalHeader, header);

			uEvent = GetUniversalEventFromXml(PopulateXml(ValidXMLEvent, nextMessage.Interchange.EI_SessionGUID, context: PopulateContext(), dataProvider: "CTCNI"));
			header = finder.FindHeaderAndProcess_Exposed(uEvent);
			AssertNotNull("Found header", header);
			AssertSame(originalHeader, header);

			uEvent = GetUniversalEventFromXml(PopulateXml(ValidXMLEvent, nextMessage.Interchange.EI_SessionGUID, context: PopulateContext(), dataProvider: "QWERTY"));
			header = finder.FindHeaderAndProcess_Exposed(uEvent);
			AssertNull("No header", header);
		}

		public void TestProcessWithInvalidEHubTrackingId()
		{
			AssertProcessWithInvalidEHubTrackingId();

			originalHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			AssertProcessWithInvalidEHubTrackingId();
		}

		public void AssertProcessWithInvalidEHubTrackingId()
		{
			SetupOutgoingMessage(originalHeader);

			var finder = GBCTCNctsHeaderEventParentFinderTestHelper.New(Factory);
			var uEvent = GetUniversalEventFromXml(PopulateXml(ValidXMLEvent, ZGuid.NewZGuid(), context: PopulateContext()));
			var header = finder.FindHeaderAndProcess_Exposed(uEvent);
			AssertNull(header);

			uEvent = GetUniversalEventFromXml(PopulateXml(ValidXMLEvent, ZGuid.Empty, context: PopulateContext()));
			header = finder.FindHeaderAndProcess_Exposed(uEvent);
			AssertNull(header);
		}

		public void TestProcessWithNoContextCollection()
		{
			AssertProcessWithNoContextCollection();

			originalHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			AssertProcessWithNoContextCollection();
		}

		public void AssertProcessWithNoContextCollection()
		{
			var nextMessage = SetupOutgoingMessage(originalHeader);
			var finder = GBCTCNctsHeaderEventParentFinderTestHelper.New(Factory);
			var uEvent = GetUniversalEventFromXml(PopulateXml(ErrorXMLEventNoContextCollection, nextMessage.Interchange.EI_SessionGUID, context: PopulateContext(), eventType: "MRJ"));
			AssertNoExceptionThrown(() => finder.FindHeaderAndProcess_Exposed(uEvent));
		}

		public void TestProcessWithNoDepartureIdOrArrivalId()
		{
			AssertProcessWithNoDepartureIdOrArrivalId();

			originalHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			AssertProcessWithNoDepartureIdOrArrivalId();
		}

		public void AssertProcessWithNoDepartureIdOrArrivalId()
		{
			var nextMessage = SetupOutgoingMessage(originalHeader);
			var finder = GBCTCNctsHeaderEventParentFinderTestHelper.New(Factory);
			var uEvent = GetUniversalEventFromXml(PopulateXml(ErrorXMLEventNoDepartureIdOrArrivalId, nextMessage.Interchange.EI_SessionGUID, context: PopulateContext(), eventType: "MRJ"));

			var header = finder.FindHeaderAndProcess_Exposed(uEvent);
			AssertNotNull("Found header", header);
			AssertEquals("Rej Status", "FAL", nextMessage.EM_Status);

			var recievedMessage = header.Messages[1];
			AssertEquals("Expected MessageNum", "123R", recievedMessage.EM_MessageNum);
		}

		public void TestGetLogParentsForEvent()
		{
			AssertGetLogParentsForEvent();
			originalHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			AssertGetLogParentsForEvent();
		}

		public void AssertGetLogParentsForEvent()
		{
			var nextMessage = SetupOutgoingMessage(originalHeader);
			var finder = GBCTCNctsHeaderEventParentFinderTestHelper.New(Factory);
			var uEvent = GetUniversalEventFromXml(PopulateXml(ValidXMLEvent, nextMessage.Interchange.EI_SessionGUID, context: PopulateContext()));

			var bizObjs = finder.GetLogParentsForEvent(uEvent);
			AssertNotNull("Got BizObj array", bizObjs);
			AssertEquals("Should have 1", 1, bizObjs.Length);
			AssertSame(originalHeader, bizObjs[0]);
			AssertEquals("Check Status", EU.NCTS.Business.NctsMessageStatusList.Codes.Ok, (bizObjs[0] as NctsHeader).BH_MessageStatus);
		}

		public void TestGetLogParentsForEventUsingContext()
		{
			AssertGetLogParentsForEventUsingContext();

			originalHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			AssertGetLogParentsForEventUsingContext();
		}

		public void AssertGetLogParentsForEventUsingContext()
		{
			var nextMessage = SetupOutgoingMessage(originalHeader);
			var finder = GBCTCNctsHeaderEventParentFinderTestHelper.New(Factory);
			var uEvent = GetUniversalEventFromXml(PopulateXml(ValidXMLEvent, nextMessage.Interchange.EI_SessionGUID, context: PopulateContext()));

			var bizObjs = finder.GetLogParentsForEventUsingContext_Exposed(uEvent);
			AssertNotNull("Got BizObj array", bizObjs);
			AssertEquals("Should have 1", 1, bizObjs.Length);
			AssertSame(originalHeader, bizObjs[0]);
			AssertEquals("Check Status", EU.NCTS.Business.NctsMessageStatusList.Codes.Ok, (bizObjs[0] as NctsHeader).BH_MessageStatus);
		}

		public void TestProcessUpdatesDeparture()
		{
			AssertStatuses("DepartureId", EU.NCTS.Business.NctsMessageStatusList.Codes.Rejected);
		}

		public void TestProcessUpdatesArrival()
		{
			AssertStatuses("ArrivalId", EU.NCTS.Business.NctsMessageStatusList.Codes.ArrivalNotificationRejected);
		}

		void AssertStatuses(string contextType, string expectedRejectionStatus)
		{
			var nextMessage = SetupOutgoingMessage(originalHeader);

			var finder = GBCTCNctsHeaderEventParentFinderTestHelper.New(Factory);
			var uEvent = GetUniversalEventFromXml(PopulateXml(ValidXMLEvent, nextMessage.Interchange.EI_SessionGUID, context: PopulateContext(contextType), eventType: "MSN"));

			var header = finder.GetLogParentsForEvent(uEvent).FirstOrDefault() as NctsHeader;
			AssertNotNull("Found header", header);

			AssertEquals("Header Ack Status", EU.NCTS.Business.NctsMessageStatusList.Codes.Ok, header.BH_MessageStatus);
			AssertEquals("Ack App Ref", "12345", nextMessage.EM_ApplicationReference);
			AssertEquals("Ack Status", "ACK", nextMessage.EM_Status);

			nextMessage = SetupOutgoingMessage(originalHeader);
			uEvent = GetUniversalEventFromXml(PopulateXml(ValidXMLEvent, nextMessage.Interchange.EI_SessionGUID, context: PopulateContext(contextType), eventType: "MRJ"));

			header = finder.GetLogParentsForEvent(uEvent).FirstOrDefault() as NctsHeader;
			AssertNotNull("Found header", header);

			AssertEquals("Header Rej Status", expectedRejectionStatus, header.BH_MessageStatus);
			AssertEquals("Rej Status", "REJ", nextMessage.EM_Status);
		}

		public void TestOriginalMessageLinkedObject()
		{
			var message = SetupOutgoingMessage(originalHeader);
			AssertType<NctsHeader>(message.EM_LinkedObject);

			originalHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			message = SetupOutgoingMessage(originalHeader);
			AssertType<NctsDepartureMovementHeader>(message.EM_LinkedObject);
		}

		UniversalEvent GetUniversalEventFromXml(string xml)
		{
			var eventDeserializer = new XmlEventDeserializer();
			return eventDeserializer.Parse(xml) as UniversalEvent;
		}

		string PopulateXml(string xmlWithPlaceholders, ZGuid ehubTrackingId, string jobNumber = "NCT00000123", string eventType = "MSN", string dataProvider = "CTCGB", string context = "")
		{
			return string.Format(xmlWithPlaceholders, jobNumber, eventType, dataProvider, ehubTrackingId.IsEmpty ? "" : ehubTrackingId.ToString(), context);
		}
		string PopulateContext(string contextType = "DepartureId", string contextValue = "12345")
		{
			return string.Format(@"<Context>
			<Type>{0}</Type>
			<Value>{1}</Value>
		  </Context>", contextType, contextValue);
		}

		const string ValidXMLEvent = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	  <Event>
		<DataContext>
		  <DataTargetCollection>
			<DataTarget>
			  <Type>NctsHeader</Type>
			  <Key>{0}</Key>
			</DataTarget>
		  </DataTargetCollection>
		</DataContext>
		<EventTime>2021-10-08T01:19:23+11:00</EventTime>
		<EventType>{1}</EventType>
		<DataContext>
		  <DataSource>
			<DataProvider>{2}</DataProvider>
		  </DataSource>
		</DataContext>
		<ContextCollection>
		  <Context>
			<Type>eHubTrackingID</Type>
			<Value>{3}</Value>
		  </Context>
		  {4}
		</ContextCollection>
	  </Event>
	</UniversalEvent>";

		const string ErrorXMLEventNoDepartureIdOrArrivalId = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
	  <Event>
        <DataContext>
          <DataTargetCollection>
            <DataTarget>
              <Type>NctsHeader</Type>
              <Key>{0}</Key>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
        <EventTime>2021-11-11T01:49:19+11:00</EventTime>
        <EventType>{1}</EventType>
        <DataContext>
          <DataSource>
            <DataProvider>{2}</DataProvider>
          </DataSource>
        </DataContext>
        <ContextCollection>
          <Context>
            <Type>eHubTrackingID</Type>
            <Value>{3}</Value>
          </Context>
          <Context>
            <Type>Error</Type>
            <Value>
            </Value>
          </Context>
          <Context>
            <Type>ErrorSummary</Type>
            <Value>
                                            HTTP Status (400): BadRequest
                                        </Value>
          </Context>
          <Context>
            <Type>ResponseText</Type>
            <Value>CiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgVGhlIHJlcXVlc3QgaGFzIGZhaWxlZCBzY2hlbWEgdmFsaWRhdGlvbi4gUGxlYXNlIHJldmlldyB0aGUgcmVxdWlyZWQgbWVzc2FnZSBzdHJ1Y3R1cmUgYXMgc3BlY2lmaWVkIGJ5IHRoZSBYU0QgZmlsZSAnY2MwMTViLnhzZCcuIERldGFpbGVkIGVycm9yIGJlbG93OgpjdmMtcGF0dGVybi12YWxpZDogVmFsdWUgJ05DVDAwMDAwMDAwMDE4My81JyBpcyBub3QgZmFjZXQtdmFsaWQgd2l0aCByZXNwZWN0IHRvIHBhdHRlcm4gJy57MSwxNH0nIGZvciB0eXBlICdBbHBoYW51bWVyaWNfTWF4MTQnLgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAg</Value>
          </Context>
        </ContextCollection>
      </Event>
    </UniversalEvent>";

		const string ErrorXMLEventNoContextCollection = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"">
  <Event>
    <DataContext>
      <Company>
        <Code>XXX</Code>
      </Company>
      <DataProvider>XXXYYYBHX</DataProvider>
      <EnterpriseID>XXX</EnterpriseID>
      <EventType>
        <Code>Z47</Code>
      </EventType>
      <ServerID>XXX</ServerID>
      <DataTargetCollection>
        <DataTarget>
          <Type>NCTSHeader</Type>
          <Key>{0}</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <EventTime>2022-03-02T08:00:00</EventTime>
    <EventType>Z47</EventType>
    <IsEstimate>true</IsEstimate>
  </Event>
</UniversalEvent>";

		protected override void SetUp()
		{
			base.SetUp();

			originalHeader = Factory.New<NctsHeader>();
			originalHeader.BH_JobReference = "NCT00000123";
		}

		EDIMessage SetupOutgoingMessage(NctsHeader header)
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_SessionGUID = ZGuid.NewZGuid();
			interchange.EI_HeaderText = "";
			interchange.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
			interchange.EI_Status = EDIMessage.Status.Sent;
			interchange.EI_From = "Sender";
			interchange.EI_To = "Receiver";
			interchange.EI_BodyText = "";
			var outgoingMessage = interchange.ContainedMessages.AddNew();
			header.LinkedMessages.Add(outgoingMessage);
			outgoingMessage.EM_MessageText = "";
			outgoingMessage.MessageNumberStrategy = new GbMessageNumberStrategy(Factory, "X");
			Factory.Save();
			outgoingMessage.EM_MessageNum = "123";
			Factory.Save();

			return outgoingMessage;
		}

		NctsHeader originalHeader;

		class GBCTCNctsHeaderEventParentFinderTestHelper : GBCTCNctsHeaderEventParentFinder
		{
			public static GBCTCNctsHeaderEventParentFinderTestHelper New(BusinessObjectFactory factory)
			{
				return new GBCTCNctsHeaderEventParentFinderTestHelper(factory, new EU.NCTS.DataTransfer.NctsHeaderDataContextManager(), new TestErrorLogger());
			}

			protected GBCTCNctsHeaderEventParentFinderTestHelper(BusinessObjectFactory factory, EU.NCTS.DataTransfer.NctsHeaderDataContextManager manager, TestErrorLogger logger) : base(factory, manager, logger)
			{
				Logger = logger;
				ContextManager = manager;
			}

			public TestErrorLogger Logger { get; }
			public EU.NCTS.DataTransfer.NctsHeaderDataContextManager ContextManager { get; }

			public BusinessObject[] GetLogParentsForEventUsingContext_Exposed(UniversalEvent xmlEvent) => base.GetLogParentsForEventUsingContext(xmlEvent);
			public NctsHeader FindHeaderAndProcess_Exposed(UniversalEvent xmlEvent) => base.FindHeaderAndProcess(xmlEvent);
		}
	}
}
