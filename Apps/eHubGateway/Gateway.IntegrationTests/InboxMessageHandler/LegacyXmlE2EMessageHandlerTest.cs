using System;
using System.IO;
using System.Text;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using CargoWise.eHub.Integration;
using NUnit.Framework;

namespace CargoWise.eHub.Gateway.IntegrationTests.InboxMessageHandler
{
	[TestFixture]
	public class LegacyXmlE2EMessageHandlerTest : GatewayIntegrationTestBase
	{
		[Test]
		public void TestLegacyXmlE2EMessageSuccess()
		{
			var adapter = CreateAdapter(SenderID, TestAuthenticatedClientPassword);
			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(message)))
			{
				var messagePK = Guid.NewGuid();
				var message = new eHubMessage(messagePK, SenderID, ClientID, MessageSchemaType.Xml, ApplicationCode.XMS, "http://www.edi.com.au/EnterpriseService", messageStream);
				adapter.Outbox.AddMessage(message);
				adapter.SendMessages();

				var content = StreamExtensions.CompressAndEncode(messageStream).ReadToEnd();
				AssertInboxMessage(messagePK, SenderIDPK, ApplicationCode.XMS, ClientIDPK, "http://www.edi.com.au/EnterpriseService", "", "", content, 0, 0);
			}
		}

		[Test]
		public void TestLegacyXmlE2EMessageFail_SenderIsNotLegacyXmlAllowed()
		{
			var adapter = CreateAdapter(TestAuthenticatedClientID, TestAuthenticatedClientPassword);
			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(message)))
			{
				var messagePK = Guid.NewGuid();
				var message = new eHubMessage(messagePK, TestAuthenticatedClientID, ClientID, MessageSchemaType.Xml, ApplicationCode.XMS, "http://www.edi.com.au/EnterpriseService", messageStream);
				adapter.Outbox.AddMessage(message);

				try
				{
					adapter.SendMessages();
					Assert.Fail("Sender is not allowed legacy xml messages.");
				}
				catch (eHubAdapterException ex)
				{
					Assert.IsTrue(ex.Message.Contains("1 errors occured during processing send request"));
					var dict = ex.GetMessageExceptionDictionary();
					Assert.AreEqual(dict.Count, 1);
					Assert.IsTrue(ex.Message.Contains("You are using E2E with the superseded legacy application Type, XMS that is no longer supported."));
				}
			}
		}

		[Test]
		public void TestLegacyXmlE2EMessageFail_ReceiverIsNotLegacyXmlAllowed()
		{
			var adapter = CreateAdapter(TestAuthenticatedClientID, TestAuthenticatedClientPassword);
			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(message)))
			{
				var messagePK = Guid.NewGuid();
				var message = new eHubMessage(messagePK, SenderID, TestAuthenticatedClientID, MessageSchemaType.Xml, ApplicationCode.XMS, "http://www.edi.com.au/EnterpriseService", messageStream);
				adapter.Outbox.AddMessage(message);

				try
				{
					adapter.SendMessages();
					Assert.Fail("Receiver is not allowed legacy xml messages.");
				}
				catch (eHubAdapterException ex)
				{
					Assert.IsTrue(ex.Message.Contains("1 errors occured during processing send request"));
					var dict = ex.GetMessageExceptionDictionary();
					Assert.AreEqual(dict.Count, 1);
					Assert.IsTrue(ex.Message.Contains("You are using E2E with the superseded legacy application Type, XMS that is no longer supported."));
				}
			}
		}

		[Test]
		public void TestLegacyXmlE2EMessageFail()
		{
			var adapter = CreateAdapter(TestAuthenticatedClientID, TestAuthenticatedClientPassword);
			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(message)))
			{
				var messagePK = Guid.NewGuid();
				var message = new eHubMessage(messagePK, TestAuthenticatedClientID, TestClientID, MessageSchemaType.Xml, ApplicationCode.XMS, "http://www.edi.com.au/EnterpriseService", messageStream);
				adapter.Outbox.AddMessage(message);

				try
				{
					adapter.SendMessages();
					Assert.Fail("Sender and Receiver is not allowed legacy xml messages.");
				}
				catch (eHubAdapterException ex)
				{
					Assert.IsTrue(ex.Message.Contains("1 errors occured during processing send request"));
					var dict = ex.GetMessageExceptionDictionary();
					Assert.AreEqual(dict.Count, 1);
					Assert.IsTrue(ex.Message.Contains("You are using E2E with the superseded legacy application Type, XMS that is no longer supported."));
				}
			}
		}

		#region constants

		const string SenderID = "ENTXMSSVR";
		const string ClientID = "ENTDFGSVR";
		static readonly Guid SenderIDPK = new Guid("56BEBD86-50AE-403F-B61D-3EBE41D8BF2C");
		static readonly Guid ClientIDPK = new Guid("85F6F61F-50C8-49E3-9DE8-57DD502D84C7");
		
		string message = @"
<XmlInterchange xmlns=""http://www.edi.com.au/EnterpriseService"">
  <InterchangeInfo>
    <Date>2010-10-01T14:03:49.4500000+10:00</Date>
    <XmlType>Verbose</XmlType>
    <Source>
      <EnterpriseCode>EDI</EnterpriseCode>
      <CompanyCode>EDI</CompanyCode>
      <OriginServer>DAT</OriginServer>
      <LoginName>EDISupport</LoginName>
    </Source>
    <Target />
    <EDIOrganisation EDICode=""EDICUS"" OwnerCode=""EDICUS"">
      <OrganisationDetails>
        <Name>EDI CUSTOMS BROKERS</Name>
        <Location Country=""Australia"" City=""Brisbane"">AUBNE</Location>
        <Addresses>
          <Address AddressType=""MAIN"">
            <AddressLine1>10 HUTCHESON STREET</AddressLine1>
            <AddressLine2>ALBION  QLD</AddressLine2>
            <AddressCode>PST: 10 HUTCHESON STREET</AddressCode>
            <PostCode>4010</PostCode>
            <Language>ENG</Language>
            <Location>AUBNE</Location>
            <Sequence>1</Sequence>
            <AddressCapabilities>
              <AddressCapability AddressType=""MAIN"" />
              <AddressCapability IsMainAddress=""true"" AddressType=""OFC"" />
            </AddressCapabilities>
          </Address>
          <Address>
            <AddressLine1>10 HUTCHESON STREET</AddressLine1>
            <AddressCode>Pick Up Address</AddressCode>
            <CityOrSuburb>ALBION</CityOrSuburb>
            <StateOrProvince>QLD</StateOrProvince>
            <Sequence>2</Sequence>
            <AddressCapabilities>
              <AddressCapability IsMainAddress=""false"" AddressType=""PIC"" />
            </AddressCapabilities>
          </Address>
        </Addresses>
      </OrganisationDetails>
    </EDIOrganisation>
  </InterchangeInfo>
  <Payload>
    <Consols>
      <Consol>
        <Events>
          <Event>
            <Source>JobConsolTransport</Source>
            <Code>DEP</Code>
            <CodeDescription>Departure</CodeDescription>
            <DateTime>2010-10-01T14:02:00.0000000+10:00</DateTime>
            <PostedDateTime>2010-10-01T04:03:31.1830000+10:00</PostedDateTime>
            <Information>AUSYD-&gt;NZABY To: 01-Oct-10</Information>
            <User>E</User>
            <UserName>CargoWise Support (E)</UserName>
            <IsEstimatedDate>true</IsEstimatedDate>
          </Event>
          <Event>
            <Source>JobConsol</Source>
            <Code>ADD</Code>
            <CodeDescription>Added a record to the system</CodeDescription>
            <DateTime>2010-10-01T14:02:27.4830000+10:00</DateTime>
            <PostedDateTime>2010-10-01T04:03:31.1830000+10:00</PostedDateTime>
            <User>E</User>
            <UserName>CargoWise Support (E)</UserName>
            <IsEstimatedDate>false</IsEstimatedDate>
          </Event>
          <Event>
            <Source>JobVoyage</Source>
            <Code>ADD</Code>
            <CodeDescription>Added a record to the system</CodeDescription>
            <DateTime>2010-10-01T14:02:52.9530000+10:00</DateTime>
            <PostedDateTime>2010-10-01T04:03:31.1830000+10:00</PostedDateTime>
            <User>E</User>
            <UserName>CargoWise Support (E)</UserName>
            <IsEstimatedDate>false</IsEstimatedDate>
          </Event>
        </Events>
        <ConsolIdentifier ConsolIdentifierType=""MasterWaybill"">081</ConsolIdentifier>
        <ConsolDetail>
          <DateCreated>2010-10-01T04:03:31.1830000+10:00</DateCreated>
          <ConsolType>Agent</ConsolType>
          <ContainerMode>LSE</ContainerMode>
          <TransportMode>AIR</TransportMode>
          <PortOfLoading>
            <Port Country=""Australia"" City=""Sydney"">AUSYD</Port>
            <EstimatedDateTime>2010-10-01T14:02:00.0000000+10:00</EstimatedDateTime>
          </PortOfLoading>
          <PortOfDischarge>
            <Port Country=""New Zealand"" City=""Albany"">NZABY</Port>
          </PortOfDischarge>
          <RoadRailFlight>
            <ETD>2010-10-01T14:02:00.0000000+10:00</ETD>
            <FlightNoJourneyNoTruckRegNo>QF122</FlightNoJourneyNoTruckRegNo>
          </RoadRailFlight>
          <PaymentType>PPD</PaymentType>
          <PlannedLegs>
            <PlannedLeg>
              <TransportMode>AIR</TransportMode>
              <PortOfLoading>
                <Port Country=""Australia"" City=""Sydney"">AUSYD</Port>
                <EstimatedDateTime>2010-10-01T14:02:00.0000000+10:00</EstimatedDateTime>
              </PortOfLoading>
              <PortOfDischarge>
                <Port Country=""New Zealand"" City=""Albany"">NZABY</Port>
              </PortOfDischarge>
              <TransportType>Flight1</TransportType>
              <RoadRailFlight>
                <ETD>2010-10-01T14:02:00.0000000+10:00</ETD>
                <FlightNoJourneyNoTruckRegNo>QF122</FlightNoJourneyNoTruckRegNo>
              </RoadRailFlight>
            </PlannedLeg>
          </PlannedLegs>
          <AgentReference>C00001000</AgentReference>
          <NumberOfOriginalBills>3</NumberOfOriginalBills>
          <NumberOfCopyBills>3</NumberOfCopyBills>
        </ConsolDetail>
      </Consol>
      <Consol>
        <Events>
          <Event>
            <Source>JobConsolTransport</Source>
            <Code>DEP</Code>
            <CodeDescription>Departure</CodeDescription>
            <DateTime>2010-10-01T14:02:00.0000000+10:00</DateTime>
            <PostedDateTime>2010-10-01T04:03:31.1830000+10:00</PostedDateTime>
            <Information>AUSYD-&gt;NZABY To: 01-Oct-10</Information>
            <User>E</User>
            <UserName>CargoWise Support (E)</UserName>
            <IsEstimatedDate>true</IsEstimatedDate>
          </Event>
          <Event>
            <Source>JobConsol</Source>
            <Code>ADD</Code>
            <CodeDescription>Added a record to the system</CodeDescription>
            <DateTime>2010-10-01T14:02:27.4830000+10:00</DateTime>
            <PostedDateTime>2010-10-01T04:03:31.1830000+10:00</PostedDateTime>
            <User>E</User>
            <UserName>CargoWise Support (E)</UserName>
            <IsEstimatedDate>false</IsEstimatedDate>
          </Event>
          <Event>
            <Source>JobVoyage</Source>
            <Code>ADD</Code>
            <CodeDescription>Added a record to the system</CodeDescription>
            <DateTime>2010-10-01T14:02:52.9530000+10:00</DateTime>
            <PostedDateTime>2010-10-01T04:03:31.1830000+10:00</PostedDateTime>
            <User>E</User>
            <UserName>CargoWise Support (E)</UserName>
            <IsEstimatedDate>false</IsEstimatedDate>
          </Event>
        </Events>
        <ConsolIdentifier ConsolIdentifierType=""MasterWaybill"">081</ConsolIdentifier>
        <ConsolDetail>
          <DateCreated>2010-10-01T04:03:31.1830000+10:00</DateCreated>
          <ConsolType>Agent</ConsolType>
          <ContainerMode>LSE</ContainerMode>
          <TransportMode>AIR</TransportMode>
          <PortOfLoading>
            <Port Country=""Australia"" City=""Sydney"">AUSYD</Port>
            <EstimatedDateTime>2010-10-01T14:02:00.0000000+10:00</EstimatedDateTime>
          </PortOfLoading>
          <PortOfDischarge>
            <Port Country=""New Zealand"" City=""Albany"">NZABY</Port>
          </PortOfDischarge>
          <RoadRailFlight>
            <ETD>2010-10-01T14:02:00.0000000+10:00</ETD>
            <FlightNoJourneyNoTruckRegNo>QF122</FlightNoJourneyNoTruckRegNo>
          </RoadRailFlight>
          <PaymentType>PPD</PaymentType>
          <PlannedLegs>
            <PlannedLeg>
              <TransportMode>AIR</TransportMode>
              <PortOfLoading>
                <Port Country=""Australia"" City=""Sydney"">AUSYD</Port>
                <EstimatedDateTime>2010-10-01T14:02:00.0000000+10:00</EstimatedDateTime>
              </PortOfLoading>
              <PortOfDischarge>
                <Port Country=""New Zealand"" City=""Albany"">NZABY</Port>
              </PortOfDischarge>
              <TransportType>Flight1</TransportType>
              <RoadRailFlight>
                <ETD>2010-10-01T14:02:00.0000000+10:00</ETD>
                <FlightNoJourneyNoTruckRegNo>QF122</FlightNoJourneyNoTruckRegNo>
              </RoadRailFlight>
            </PlannedLeg>
          </PlannedLegs>
          <AgentReference>C00001000</AgentReference>
          <NumberOfOriginalBills>3</NumberOfOriginalBills>
          <NumberOfCopyBills>3</NumberOfCopyBills>
        </ConsolDetail>
      </Consol>
    </Consols>
  </Payload>
</XmlInterchange>";

		#endregion
	}
}
