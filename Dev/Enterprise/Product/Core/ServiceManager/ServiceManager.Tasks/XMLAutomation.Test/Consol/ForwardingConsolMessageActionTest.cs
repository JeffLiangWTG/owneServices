using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.eHubMessaging.Tests;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	class ForwardingConsolMessageActionTest : ImportMessageActionTest
	{
		public void TestExecuteAction_ImportConsol()
		{
			var factoryProvider = new BusinessObjectFactoryProvider(Factory);

			int noOfConsols = Factory.GetDatabaseCount(typeof(ForwardingConsol));

			var buffer = new NotificationBuffer();
			var message = CreateMessage(Factory);
			message.EM_MessageText = messageBody;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.Consols;
			var action = new ForwardingConsolMessageAction(factoryProvider);
			List<ITransactionParticipant> participants;
			Assert(!action.ExecuteAction(message, buffer, out participants));
			Factory.Save();
			AssertEquals(noOfConsols, Factory.GetDatabaseCount(typeof(ForwardingConsol)));

			message.Interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.XMS;
			message.Interchange.EI_HeaderText = "";
			message.Interchange.EI_FooterText = "";
			message.Interchange.EI_BodyText = interchangeBody;
			message.Interchange.EI_From = "blah";
			message.Interchange.EI_From = "blah blah";

			Assert(action.ExecuteAction(message, buffer, out participants));
			Factory.Save();
			AssertEquals(noOfConsols + 1, Factory.GetDatabaseCount(typeof(ForwardingConsol)));
		}

		#region XML

		const string interchangeBody = @"<?xml version=""1.0"" encoding=""utf-8""?><XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Version=""1"" xmlns=""http://www.edi.com.au/EnterpriseService/"">	<InterchangeInfo>		<Date>2010-10-01T14:03:49.45+10:00</Date>		<XmlType>Verbose</XmlType>		<Source>			<EnterpriseCode>EDI</EnterpriseCode>			<CompanyCode>EDI</CompanyCode>			<OriginServer>DAT</OriginServer>			<LoginName>EDISupport</LoginName>		</Source>		<Target />		<EDIOrganisation EDICode=""EDICUS"" OwnerCode=""EDICUS"">			<OrganisationDetails>				<Name>EDI CUSTOMS BROKERS</Name>				<Location Country=""Australia"" City=""Brisbane"">AUBNE</Location>				<Addresses>					<Address AddressType=""MAIN"">						<AddressLine1>10 HUTCHESON STREET</AddressLine1>						<AddressLine2>ALBION  QLD</AddressLine2>						<AddressCode>PST: 10 HUTCHESON STREET</AddressCode>						<PostCode>4010</PostCode>						<Language>EN</Language>						<Location>AUBNE</Location>						<Sequence>1</Sequence>						<AddressCapabilities>							<AddressCapability AddressType=""MAIN"" />							<AddressCapability IsMainAddress=""true"" AddressType=""OFC"" />						</AddressCapabilities>					</Address>					<Address>						<AddressLine1>10 HUTCHESON STREET</AddressLine1>						<AddressCode>Pick Up Address</AddressCode>						<CityOrSuburb>ALBION</CityOrSuburb>						<StateOrProvince>QLD</StateOrProvince>						<Sequence>2</Sequence>						<AddressCapabilities>							<AddressCapability IsMainAddress=""false"" AddressType=""PIC"" />						</AddressCapabilities>					</Address>				</Addresses>			</OrganisationDetails>		</EDIOrganisation>	</InterchangeInfo>	<Payload>		<Consols>			<Consol>				<Events>					<Event>						<Source>JobConsolTransport</Source>						<Code>DEP</Code>						<CodeDescription>Departure</CodeDescription>						<DateTime>2010-10-01T14:02:00+10:00</DateTime>						<PostedDateTime>2010-10-01T04:03:31.183+10:00</PostedDateTime>						<Information>AUSYD-&gt;NZABY To: 01-Oct-10</Information>						<User>E</User>						<UserName>CargoWise Support (E)</UserName>						<IsEstimatedDate>true</IsEstimatedDate>					</Event>					<Event>						<Source>JobConsol</Source>						<Code>ADD</Code>						<CodeDescription>Added a record to the system</CodeDescription>						<DateTime>2010-10-01T14:02:27.483+10:00</DateTime>						<PostedDateTime>2010-10-01T04:03:31.183+10:00</PostedDateTime>						<User>E</User>						<UserName>CargoWise Support (E)</UserName>						<IsEstimatedDate>false</IsEstimatedDate>					</Event>					<Event>						<Source>JobVoyage</Source>						<Code>ADD</Code>						<CodeDescription>Added a record to the system</CodeDescription>						<DateTime>2010-10-01T14:02:52.953+10:00</DateTime>						<PostedDateTime>2010-10-01T04:03:31.183+10:00</PostedDateTime>						<User>E</User>						<UserName>CargoWise Support (E)</UserName>						<IsEstimatedDate>false</IsEstimatedDate>					</Event>				</Events>				<ConsolIdentifier ConsolIdentifierType=""MasterWaybill"">081</ConsolIdentifier>				<ConsolDetail>					<DateCreated>2010-10-01T04:03:31.183+10:00</DateCreated>					<ConsolType>Agent</ConsolType>					<ContainerMode>LSE</ContainerMode>					<TransportMode>AIR</TransportMode>					<PortOfLoading>						<Port Country=""Australia"" City=""Sydney"">AUSYD</Port>						<EstimatedDateTime>2010-10-01T14:02:00+10:00</EstimatedDateTime>					</PortOfLoading>					<PortOfDischarge>						<Port Country=""New Zealand"" City=""Albany"">NZABY</Port>					</PortOfDischarge>					<RoadRailFlight>						<ETD>2010-10-01T14:02:00+10:00</ETD>						<FlightNoJourneyNoTruckRegNo>QF122</FlightNoJourneyNoTruckRegNo>					</RoadRailFlight>					<PaymentType>PPD</PaymentType>					<PlannedLegs>						<PlannedLeg>							<TransportMode>AIR</TransportMode>							<PortOfLoading>								<Port Country=""Australia"" City=""Sydney"">AUSYD</Port>								<EstimatedDateTime>2010-10-01T14:02:00+10:00</EstimatedDateTime>							</PortOfLoading>							<PortOfDischarge>								<Port Country=""New Zealand"" City=""Albany"">NZABY</Port>							</PortOfDischarge>							<TransportType>Flight1</TransportType>							<RoadRailFlight>								<ETD>2010-10-01T14:02:00+10:00</ETD>								<FlightNoJourneyNoTruckRegNo>QF122</FlightNoJourneyNoTruckRegNo>							</RoadRailFlight>						</PlannedLeg>					</PlannedLegs>					<AgentReference>C00001000</AgentReference>					<NumberOfOriginalBills>3</NumberOfOriginalBills>					<NumberOfCopyBills>3</NumberOfCopyBills>				</ConsolDetail>			</Consol>			<Consol>				<Events>					<Event>						<Source>JobConsolTransport</Source>						<Code>DEP</Code>						<CodeDescription>Departure</CodeDescription>						<DateTime>2010-10-01T14:02:00+10:00</DateTime>						<PostedDateTime>2010-10-01T04:03:31.183+10:00</PostedDateTime>						<Information>AUSYD-&gt;NZABY To: 01-Oct-10</Information>						<User>E</User>						<UserName>CargoWise Support (E)</UserName>						<IsEstimatedDate>true</IsEstimatedDate>					</Event>					<Event>						<Source>JobConsol</Source>						<Code>ADD</Code>						<CodeDescription>Added a record to the system</CodeDescription>						<DateTime>2010-10-01T14:02:27.483+10:00</DateTime>						<PostedDateTime>2010-10-01T04:03:31.183+10:00</PostedDateTime>						<User>E</User>						<UserName>CargoWise Support (E)</UserName>						<IsEstimatedDate>false</IsEstimatedDate>					</Event>					<Event>						<Source>JobVoyage</Source>						<Code>ADD</Code>						<CodeDescription>Added a record to the system</CodeDescription>						<DateTime>2010-10-01T14:02:52.953+10:00</DateTime>						<PostedDateTime>2010-10-01T04:03:31.183+10:00</PostedDateTime>						<User>E</User>						<UserName>CargoWise Support (E)</UserName>						<IsEstimatedDate>false</IsEstimatedDate>					</Event>				</Events>				<ConsolIdentifier ConsolIdentifierType=""MasterWaybill"">081</ConsolIdentifier>				<ConsolDetail>					<DateCreated>2010-10-01T04:03:31.183+10:00</DateCreated>					<ConsolType>Agent</ConsolType>					<ContainerMode>LSE</ContainerMode>					<TransportMode>AIR</TransportMode>					<PortOfLoading>						<Port Country=""Australia"" City=""Sydney"">AUSYD</Port>						<EstimatedDateTime>2010-10-01T14:02:00+10:00</EstimatedDateTime>					</PortOfLoading>					<PortOfDischarge>						<Port Country=""New Zealand"" City=""Albany"">NZABY</Port>					</PortOfDischarge>					<RoadRailFlight>						<ETD>2010-10-01T14:02:00+10:00</ETD>						<FlightNoJourneyNoTruckRegNo>QF122</FlightNoJourneyNoTruckRegNo>					</RoadRailFlight>					<PaymentType>PPD</PaymentType>					<PlannedLegs>						<PlannedLeg>							<TransportMode>AIR</TransportMode>							<PortOfLoading>								<Port Country=""Australia"" City=""Sydney"">AUSYD</Port>								<EstimatedDateTime>2010-10-01T14:02:00+10:00</EstimatedDateTime>							</PortOfLoading>							<PortOfDischarge>								<Port Country=""New Zealand"" City=""Albany"">NZABY</Port>							</PortOfDischarge>							<TransportType>Flight1</TransportType>							<RoadRailFlight>								<ETD>2010-10-01T14:02:00+10:00</ETD>								<FlightNoJourneyNoTruckRegNo>QF122</FlightNoJourneyNoTruckRegNo>							</RoadRailFlight>						</PlannedLeg>					</PlannedLegs>					<AgentReference>C00001000</AgentReference>					<NumberOfOriginalBills>3</NumberOfOriginalBills>					<NumberOfCopyBills>3</NumberOfCopyBills>				</ConsolDetail>			</Consol>		</Consols>	</Payload></XmlInterchange>";
		const string messageBody = @"<ns0:Consol xmlns:ns0=""http://www.edi.com.au/EnterpriseService/""><ns0:ConsolIdentifier ConsolIdentifierType=""MasterWaybill"" /><ns0:ConsolDetail><ns0:ConsolType>Agent</ns0:ConsolType><ns0:ContainerMode>FCL</ns0:ContainerMode><ns0:TransportMode>SEA</ns0:TransportMode><ns0:PaymentType>PPD</ns0:PaymentType><ns0:PlannedLegs><ns0:PlannedLeg><ns0:TransportMode>SEA</ns0:TransportMode><ns0:TransportType>MainVessel</ns0:TransportType><ns0:Vessel /></ns0:PlannedLeg></ns0:PlannedLegs><ns0:NumberOfOriginalBills>3</ns0:NumberOfOriginalBills><ns0:NumberOfCopyBills>3</ns0:NumberOfCopyBills></ns0:ConsolDetail></ns0:Consol>";

		#endregion
	}
}
