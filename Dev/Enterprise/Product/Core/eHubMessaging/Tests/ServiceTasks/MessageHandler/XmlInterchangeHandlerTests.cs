using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.eHubMessaging.Business.DownloadHandler;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks.DownloadHandler
{
	class XmlInterchangeHandlerTests : TestCaseWithFactory
	{
		public const string anyTextKeyWord = "****CanBeAnyText****";
		GlbCompany Company { get; set; }

		public XmlInterchangeHandlerTests()
		{
			Company = TestHelpers.ValidCompanyForTest(Factory);
		}

		public void TestBillsOfLadingMessage()
		{
			string fileName = "BillsOfLading.xml";
			string expectedHeaderFileName = "BillsOfLadingHeader.xml";
			string expectedBodyFileName = "BillsOfLadingBody.xml";
			string expectedSubType = "AGB";
			string expectedMessage = @"<AgencyBillOfLading xmlns=""http://www.edi.com.au/EnterpriseService/""><BillNumber>V00001000</BillNumber><Principal EDICode=""ABIMOTBNE"" OwnerCode=""ABIMOTBNE""><OrganisationDetails><Name>ABINGDON MOTORS</Name><Location Country=""Australia"" City=""Brisbane"">AUBNE</Location><Addresses><Address AddressType=""MAIN""><AddressLine1>192 ANNERLY ROAD</AddressLine1><AddressLine2>DUTTON</AddressLine2><AddressCode>Pickup and Delivery Addre</AddressCode><CityOrSuburb>PARK</CityOrSuburb><StateOrProvince>QLD</StateOrProvince><PostCode>1234</PostCode><Language>EN</Language><Location>AUBNE</Location><Sequence>1</Sequence><AddressCapabilities><AddressCapability AddressType=""MAIN"" /><AddressCapability IsMainAddress=""true"" AddressType=""OFC"" /></AddressCapabilities></Address><Address><AddressLine1>192 ANNERLY ROAD</AddressLine1><AddressLine2>DUTTON PARK QLD</AddressLine2><AddressCode>PST: 192 ANNERLY ROAD</AddressCode><CityOrSuburb>NEWCITY</CityOrSuburb><StateOrProvince>QLD</StateOrProvince><PostCode>4102</PostCode><Language>EN</Language><Sequence>2</Sequence><AddressCapabilities><AddressCapability IsMainAddress=""false"" AddressType=""PST"" /></AddressCapabilities></Address></Addresses></OrganisationDetails></Principal><Description>something heavy</Description><ReleaseType>OBR</ReleaseType><ChargesApply>SHW</ChargesApply><CustomsEntryNumber><Country>AU</Country></CustomsEntryNumber><PackageSummary><PackType>PLT</PackType><NumberOfPacks>1</NumberOfPacks><Weight DimensionType=""KG"">0</Weight><Length>0</Length><Width>0</Width><Height>0</Height><Volume DimensionType=""M3"">0</Volume></PackageSummary><ShippedOnBoard><Code>SHP</Code></ShippedOnBoard><OriginalBills>3</OriginalBills><CopyBills>3</CopyBills><CargoType>FCL</CargoType><Load Country=""Australia"" City=""Sydney"">AUSYD</Load><Origin Country=""Australia"" City=""Sydney"">AUSYD</Origin><PortOfOrigin><Port Country=""Australia"" City=""Sydney"">AUSYD</Port></PortOfOrigin><Discharge Country=""Australia"" City=""Brisbane"">AUBNE</Discharge><Destination Country=""Australia"" City=""Brisbane"">AUBNE</Destination><PortOfDestination><Port Country=""Australia"" City=""Brisbane"">AUBNE</Port></PortOfDestination><Packages><Package><PackType>PLT</PackType><NumberOfPacks>1</NumberOfPacks><Weight DimensionType=""KG"">0</Weight><Length DimensionType=""M"">0</Length><Width DimensionType=""M"">0</Width><Height DimensionType=""M"">0</Height><Volume DimensionType=""M3"">0</Volume><CommodityCode>GEN</CommodityCode></Package></Packages><Addresses><DocAddress AddressType=""CED""><AddressReference><AddressSequenceRef>1</AddressSequenceRef><Organisation EDICode=""AUSTOR"" OwnerCode=""AUSTOR""><OrganisationDetails><Name>AUSTORIENT FREIGHT SERVICES</Name><Location Country=""Australia"" City=""Sydney"">AUSYD</Location><Addresses><Address AddressType=""MAIN""><AddressLine1>1/19-21 BOURKE ROAD</AddressLine1><AddressLine2>ALEXANDRIA, NSW</AddressLine2><AddressCode>PST: 1/19-21 BOURKE ROAD</AddressCode><PostCode>2015</PostCode><Language>EN</Language><Location>AUSYD</Location><Sequence>1</Sequence><AddressCapabilities><AddressCapability AddressType=""MAIN"" /><AddressCapability IsMainAddress=""true"" AddressType=""OFC"" /></AddressCapabilities></Address><Address><AddressLine1>1/19-21 BOURKE ROAD</AddressLine1><AddressCode>Pickup and Delivery Addre</AddressCode><CityOrSuburb>ALEXANDRIA</CityOrSuburb><StateOrProvince>NSW</StateOrProvince><Sequence>2</Sequence><AddressCapabilities><AddressCapability IsMainAddress=""false"" AddressType=""PAD"" /></AddressCapabilities></Address></Addresses><Contacts><Contact><Name>ROSS FEHLBERG</Name><NotifyMode>PRN</NotifyMode><Sequence>1</Sequence></Contact></Contacts></OrganisationDetails></Organisation></AddressReference></DocAddress><DocAddress AddressType=""CEG""><AddressReference><AddressSequenceRef>2</AddressSequenceRef><Organisation EDICode=""AUSTOR"" OwnerCode=""AUSTOR""><OrganisationDetails><Name>AUSTORIENT FREIGHT SERVICES</Name><Location Country=""Australia"" City=""Sydney"">AUSYD</Location><Addresses><Address AddressType=""MAIN""><AddressLine1>1/19-21 BOURKE ROAD</AddressLine1><AddressLine2>ALEXANDRIA, NSW</AddressLine2><AddressCode>PST: 1/19-21 BOURKE ROAD</AddressCode><PostCode>2015</PostCode><Language>EN</Language><Location>AUSYD</Location><Sequence>1</Sequence><AddressCapabilities><AddressCapability AddressType=""MAIN"" /><AddressCapability IsMainAddress=""true"" AddressType=""OFC"" /></AddressCapabilities></Address><Address><AddressLine1>1/19-21 BOURKE ROAD</AddressLine1><AddressCode>Pickup and Delivery Addre</AddressCode><CityOrSuburb>ALEXANDRIA</CityOrSuburb><StateOrProvince>NSW</StateOrProvince><Sequence>2</Sequence><AddressCapabilities><AddressCapability IsMainAddress=""false"" AddressType=""PAD"" /></AddressCapabilities></Address></Addresses><Contacts><Contact><Name>ROSS FEHLBERG</Name><NotifyMode>PRN</NotifyMode><Sequence>1</Sequence></Contact></Contacts></OrganisationDetails></Organisation></AddressReference></DocAddress><DocAddress AddressType=""CRD""><AddressReference><AddressSequenceRef>1</AddressSequenceRef><Organisation EDICode=""AUSNEW"" OwnerCode=""AUSNEW""><OrganisationDetails><Name>AUSTRALIA NEW ZEALAND DIRECT LINE</Name><Location Country=""Australia"" City=""Sydney"">AUSYD</Location><Addresses><Address AddressType=""MAIN""><AddressLine1>GPO BOX 4181</AddressLine1><AddressLine2>SYDNEY, NSW</AddressLine2><AddressCode>PST: GPO BOX 4181</AddressCode><PostCode>2001</PostCode><Language>EN</Language><Location>AUSYD</Location><Sequence>1</Sequence><AddressCapabilities><AddressCapability AddressType=""MAIN"" /><AddressCapability IsMainAddress=""true"" AddressType=""OFC"" /></AddressCapabilities></Address></Addresses></OrganisationDetails></Organisation></AddressReference></DocAddress><DocAddress AddressType=""CRG""><AddressReference><AddressSequenceRef>1</AddressSequenceRef><Organisation EDICode=""AUSNEW"" OwnerCode=""AUSNEW""><OrganisationDetails><Name>AUSTRALIA NEW ZEALAND DIRECT LINE</Name><Location Country=""Australia"" City=""Sydney"">AUSYD</Location><Addresses><Address AddressType=""MAIN""><AddressLine1>GPO BOX 4181</AddressLine1><AddressLine2>SYDNEY, NSW</AddressLine2><AddressCode>PST: GPO BOX 4181</AddressCode><PostCode>2001</PostCode><Language>EN</Language><Location>AUSYD</Location><Sequence>1</Sequence><AddressCapabilities><AddressCapability AddressType=""MAIN"" /><AddressCapability IsMainAddress=""true"" AddressType=""OFC"" /></AddressCapabilities></Address></Addresses></OrganisationDetails></Organisation></AddressReference></DocAddress></Addresses></AgencyBillOfLading>";
			CreateAndValidateInterchange(fileName, expectedHeaderFileName, expectedBodyFileName, expectedSubType, expectedMessage);
		}

		public void TestConsolsMessage()
		{
			string fileName = "Consols.xml";
			string expectedHeaderFileName = "ConsolsHeader.xml";
			string expectedBodyFileName = "ConsolsBody.xml";
			string expectedSubType = "CON";
			string expectedMessage = @"<Consol xmlns=""http://www.edi.com.au/EnterpriseService/""><Events>					<Event>						<Source>JobConsolTransport</Source>						<Code>DEP</Code>						<CodeDescription>Departure</CodeDescription>						<DateTime>2010-10-01T14:02:00+10:00</DateTime>						<PostedDateTime>2010-10-01T04:03:31.183+10:00</PostedDateTime>						<Information>AUSYD-&gt;NZABY To: 01-Oct-10</Information>						<User>E</User>						<UserName>CargoWise Support (E)</UserName>						<IsEstimatedDate>true</IsEstimatedDate>					</Event>					<Event>						<Source>JobConsol</Source>						<Code>ADD</Code>						<CodeDescription>Added a record to the system</CodeDescription>						<DateTime>2010-10-01T14:02:27.483+10:00</DateTime>						<PostedDateTime>2010-10-01T04:03:31.183+10:00</PostedDateTime>						<User>E</User>						<UserName>CargoWise Support (E)</UserName>						<IsEstimatedDate>false</IsEstimatedDate>					</Event>					<Event>						<Source>JobVoyage</Source>						<Code>ADD</Code>						<CodeDescription>Added a record to the system</CodeDescription>						<DateTime>2010-10-01T14:02:52.953+10:00</DateTime>						<PostedDateTime>2010-10-01T04:03:31.183+10:00</PostedDateTime>						<User>E</User>						<UserName>CargoWise Support (E)</UserName>						<IsEstimatedDate>false</IsEstimatedDate>					</Event>				</Events>				<ConsolIdentifier ConsolIdentifierType=""MasterWaybill"">081</ConsolIdentifier>				<ConsolDetail>					<DateCreated>2010-10-01T04:03:31.183+10:00</DateCreated>					<ConsolType>Agent</ConsolType>					<ContainerMode>LSE</ContainerMode>					<TransportMode>AIR</TransportMode>					<PortOfLoading>						<Port Country=""Australia"" City=""Sydney"">AUSYD</Port>						<EstimatedDateTime>2010-10-01T14:02:00+10:00</EstimatedDateTime>					</PortOfLoading>					<PortOfDischarge>						<Port Country=""New Zealand"" City=""Albany"">NZABY</Port>					</PortOfDischarge>					<RoadRailFlight>						<ETD>2010-10-01T14:02:00+10:00</ETD>						<FlightNoJourneyNoTruckRegNo>QF122</FlightNoJourneyNoTruckRegNo>					</RoadRailFlight>					<PaymentType>PPD</PaymentType>					<PlannedLegs>						<PlannedLeg>							<TransportMode>AIR</TransportMode>							<PortOfLoading>								<Port Country=""Australia"" City=""Sydney"">AUSYD</Port>								<EstimatedDateTime>2010-10-01T14:02:00+10:00</EstimatedDateTime>							</PortOfLoading>							<PortOfDischarge>								<Port Country=""New Zealand"" City=""Albany"">NZABY</Port>							</PortOfDischarge>							<TransportType>Flight1</TransportType>							<RoadRailFlight>								<ETD>2010-10-01T14:02:00+10:00</ETD>								<FlightNoJourneyNoTruckRegNo>QF122</FlightNoJourneyNoTruckRegNo>							</RoadRailFlight>						</PlannedLeg>					</PlannedLegs>					<AgentReference>C00001000</AgentReference>					<NumberOfOriginalBills>3</NumberOfOriginalBills>					<NumberOfCopyBills>3</NumberOfCopyBills>				</ConsolDetail></Consol>";
			CreateAndValidateInterchange(fileName, expectedHeaderFileName, expectedBodyFileName, expectedSubType, expectedMessage, 2);
		}

		public void TestDeclarationMessage()
		{
			string fileName = "Declaration.xml";
			string expectedHeaderFileName = "DeclarationHeader.xml";
			string expectedBodyFileName = "DeclarationBody.xml";
			string expectedSubType = "BRK";
			string expectedMessage = @"<Consol xmlns=""http://www.edi.com.au/EnterpriseService/""><Events>					<Event>						<Source>JobConsolTransport</Source>						<Code>DEP</Code>						<CodeDescription>Departure</CodeDescription>						<DateTime>2010-10-01T14:02:00+10:00</DateTime>						<PostedDateTime>2010-10-01T04:03:31.183+10:00</PostedDateTime>						<Information>AUSYD-&gt;NZABY To: 01-Oct-10</Information>						<User>E</User>						<UserName>CargoWise Support (E)</UserName>						<IsEstimatedDate>true</IsEstimatedDate>					</Event>					<Event>						<Source>JobConsol</Source>						<Code>ADD</Code>						<CodeDescription>Added a record to the system</CodeDescription>						<DateTime>2010-10-01T14:02:27.483+10:00</DateTime>						<PostedDateTime>2010-10-01T04:03:31.183+10:00</PostedDateTime>						<User>E</User>						<UserName>CargoWise Support (E)</UserName>						<IsEstimatedDate>false</IsEstimatedDate>					</Event>					<Event>						<Source>JobVoyage</Source>						<Code>ADD</Code>						<CodeDescription>Added a record to the system</CodeDescription>						<DateTime>2010-10-01T14:02:52.953+10:00</DateTime>						<PostedDateTime>2010-10-01T04:03:31.183+10:00</PostedDateTime>						<User>E</User>						<UserName>CargoWise Support (E)</UserName>						<IsEstimatedDate>false</IsEstimatedDate>					</Event>				</Events>				<ConsolIdentifier ConsolIdentifierType=""MasterWaybill"">081</ConsolIdentifier>				<ConsolDetail>					<DateCreated>2010-10-01T04:03:31.183+10:00</DateCreated>					<ConsolType>Agent</ConsolType>					<ContainerMode>LSE</ContainerMode>					<TransportMode>AIR</TransportMode>					<PortOfLoading>						<Port Country=""Australia"" City=""Sydney"">AUSYD</Port>						<EstimatedDateTime>2010-10-01T14:02:00+10:00</EstimatedDateTime>					</PortOfLoading>					<PortOfDischarge>						<Port Country=""New Zealand"" City=""Albany"">NZABY</Port>					</PortOfDischarge>					<RoadRailFlight>						<ETD>2010-10-01T14:02:00+10:00</ETD>						<FlightNoJourneyNoTruckRegNo>QF122</FlightNoJourneyNoTruckRegNo>					</RoadRailFlight>					<PaymentType>PPD</PaymentType>					<PlannedLegs>						<PlannedLeg>							<TransportMode>AIR</TransportMode>							<PortOfLoading>								<Port Country=""Australia"" City=""Sydney"">AUSYD</Port>								<EstimatedDateTime>2010-10-01T14:02:00+10:00</EstimatedDateTime>							</PortOfLoading>							<PortOfDischarge>								<Port Country=""New Zealand"" City=""Albany"">NZABY</Port>							</PortOfDischarge>							<TransportType>Flight1</TransportType>							<RoadRailFlight>								<ETD>2010-10-01T14:02:00+10:00</ETD>								<FlightNoJourneyNoTruckRegNo>QF122</FlightNoJourneyNoTruckRegNo>							</RoadRailFlight>						</PlannedLeg>					</PlannedLegs>					<AgentReference>C00001000</AgentReference>					<NumberOfOriginalBills>3</NumberOfOriginalBills>					<NumberOfCopyBills>3</NumberOfCopyBills>				</ConsolDetail></Consol>";
			CreateAndValidateInterchange(fileName, expectedHeaderFileName, expectedBodyFileName, expectedSubType, expectedMessage, 2);
		}

		public void TestCFSLoadListMessage()
		{
			string fileName = "CFSLoadList.xml";
			string expectedHeaderFileName = "CFSLoadListHeader.xml";
			string expectedBodyFileName = "CFSLoadListBody.xml";
			string expectedSubType = "CLL";
			string expectedMessage = @"<Consol xmlns=""http://www.edi.com.au/EnterpriseService/""><Events>					<Event>						<Source>JobConsolTransport</Source>						<Code>DEP</Code>						<CodeDescription>Departure</CodeDescription>						<DateTime>2010-10-01T14:02:00+10:00</DateTime>						<PostedDateTime>2010-10-01T04:03:31.183+10:00</PostedDateTime>						<Information>AUSYD-&gt;NZABY To: 01-Oct-10</Information>						<User>E</User>						<UserName>CargoWise Support (E)</UserName>						<IsEstimatedDate>true</IsEstimatedDate>					</Event>					<Event>						<Source>JobConsol</Source>						<Code>ADD</Code>						<CodeDescription>Added a record to the system</CodeDescription>						<DateTime>2010-10-01T14:02:27.483+10:00</DateTime>						<PostedDateTime>2010-10-01T04:03:31.183+10:00</PostedDateTime>						<User>E</User>						<UserName>CargoWise Support (E)</UserName>						<IsEstimatedDate>false</IsEstimatedDate>					</Event>					<Event>						<Source>JobVoyage</Source>						<Code>ADD</Code>						<CodeDescription>Added a record to the system</CodeDescription>						<DateTime>2010-10-01T14:02:52.953+10:00</DateTime>						<PostedDateTime>2010-10-01T04:03:31.183+10:00</PostedDateTime>						<User>E</User>						<UserName>CargoWise Support (E)</UserName>						<IsEstimatedDate>false</IsEstimatedDate>					</Event>				</Events>				<ConsolIdentifier ConsolIdentifierType=""MasterWaybill"">081</ConsolIdentifier>				<ConsolDetail>					<DateCreated>2010-10-01T04:03:31.183+10:00</DateCreated>					<ConsolType>Agent</ConsolType>					<ContainerMode>LSE</ContainerMode>					<TransportMode>AIR</TransportMode>					<PortOfLoading>						<Port Country=""Australia"" City=""Sydney"">AUSYD</Port>						<EstimatedDateTime>2010-10-01T14:02:00+10:00</EstimatedDateTime>					</PortOfLoading>					<PortOfDischarge>						<Port Country=""New Zealand"" City=""Albany"">NZABY</Port>					</PortOfDischarge>					<RoadRailFlight>						<ETD>2010-10-01T14:02:00+10:00</ETD>						<FlightNoJourneyNoTruckRegNo>QF122</FlightNoJourneyNoTruckRegNo>					</RoadRailFlight>					<PaymentType>PPD</PaymentType>					<PlannedLegs>						<PlannedLeg>							<TransportMode>AIR</TransportMode>							<PortOfLoading>								<Port Country=""Australia"" City=""Sydney"">AUSYD</Port>								<EstimatedDateTime>2010-10-01T14:02:00+10:00</EstimatedDateTime>							</PortOfLoading>							<PortOfDischarge>								<Port Country=""New Zealand"" City=""Albany"">NZABY</Port>							</PortOfDischarge>							<TransportType>Flight1</TransportType>							<RoadRailFlight>								<ETD>2010-10-01T14:02:00+10:00</ETD>								<FlightNoJourneyNoTruckRegNo>QF122</FlightNoJourneyNoTruckRegNo>							</RoadRailFlight>						</PlannedLeg>					</PlannedLegs>					<AgentReference>C00001000</AgentReference>					<NumberOfOriginalBills>3</NumberOfOriginalBills>					<NumberOfCopyBills>3</NumberOfCopyBills>				</ConsolDetail></Consol>";
			CreateAndValidateInterchange(fileName, expectedHeaderFileName, expectedBodyFileName, expectedSubType, expectedMessage, 2);
		}

		public void TestContainerMovementsMessage()
		{
			string fileName = "ContainerMovements.xml";
			string expectedHeaderFileName = "ContainerMovementsHeader.xml";
			string expectedBodyFileName = "ContainerMovementsBody.xml";
			string expectedSubType = "CMV";
			string expectedMessage = @"<ContainerMovement xmlns=""http://www.edi.com.au/EnterpriseService/""><ContainerNum>ContainerNum_0</ContainerNum>" + MessageHandlerTestHelper.AnyTextKeyWord + "</ContainerMovement>";
			CreateAndValidateInterchange(fileName, expectedHeaderFileName, expectedBodyFileName, expectedSubType, expectedMessage);
		}

		public void TestEventsMessage()
		{
			string fileName = "Events.xml";
			string expectedHeaderFileName = "EventsHeader.xml";
			string expectedBodyFileName = "EventsBody.xml";
			string expectedSubType = "EVT";
			string expectedMessage = @"<ns0:Event xmlns:ns0=""http://www.edi.com.au/EnterpriseService/""><ns0:Source>NACCS</ns0:Source>				<ns0:Code>IES</ns0:Code>				<ns0:DateTime>2011-01-02T09:31:00+11:00</ns0:DateTime>				<ns0:Information>Remark 1</ns0:Information>				<ns0:ReferenceKeys>					<ns0:ReferenceKey ReferenceKeyName=""ShipmentJobNumber"">S00001223</ns0:ReferenceKey>				</ns0:ReferenceKeys>			</ns0:Event>";
			CreateAndValidateInterchange(fileName, expectedHeaderFileName, expectedBodyFileName, expectedSubType, expectedMessage, 2);
		}

		public void TestFinancialTransactionsMessage()
		{
			string fileName = "FinancialTransactions.xml";
			string expectedHeaderFileName = "FinancialTransactionsHeader.xml";
			string expectedBodyFileName = "FinancialTransactionsBody.xml";
			string expectedSubType = "FTR";
			string expectedMessage = @"<FinancialInvoice xmlns=""http://www.edi.com.au/EnterpriseService/""><Ledger>AR</Ledger><DebtorOrCreditor EDICode=""KIRSTESYD"" OwnerCode=""KIRSTESYD""><OrganisationDetails><Name>KIRSTEN COMPANY PTY LTD</Name><Location Country=""Australia"" City=""Sydney"">AUSYD</Location><Addresses><Address AddressType=""MAIN""><AddressLine1>124 MARINE PARADE</AddressLine1><AddressCode>MARINE PARADE</AddressCode><CityOrSuburb>MAROUBRA</CityOrSuburb><StateOrProvince>NSW</StateOrProvince><PostCode>2035</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">02 9003 2456</TelephoneNumber><TelephoneNumber NumberType=""Fax"">02 9000 1002</TelephoneNumber></TelephoneNumbers><Language>EN</Language><Location>AUSYD</Location><Sequence>1</Sequence><AddressCapabilities><AddressCapability AddressType=""MAIN"" /><AddressCapability IsMainAddress=""true"" AddressType=""OFC"" /></AddressCapabilities></Address></Addresses><Contacts><Contact><Name>BNB0</Name><Salutation>Mr Contact Person</Salutation><Language>EN</Language><NotifyMode>EML</NotifyMode><AttachmentType>PDF</AttachmentType><EmailAddress>jsmith@charlesparsons.com</EmailAddress><Sequence>1</Sequence></Contact><Contact><Name>TFXF0</Name><Language>EN</Language><NotifyMode>EML</NotifyMode><AttachmentType>PDF</AttachmentType><Sequence>2</Sequence></Contact></Contacts><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>LSC</NumberType><Number>NON-FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>NZ</CountryOfRegistration><NumberType>GST</NumberType><Number>FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>DK</CountryOfRegistration><NumberType>GBR</NumberType><Number>FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>DK</CountryOfRegistration><NumberType>GCR</NumberType><Number>FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>DK</CountryOfRegistration><NumberType>CVR</NumberType><Number>FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>DK</CountryOfRegistration><NumberType>APC</NumberType><Number>NON-FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>DK</CountryOfRegistration><NumberType>CMM</NumberType><Number>NON-FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>US</CountryOfRegistration><NumberType>LSC</NumberType><Number>EMIN25</Number></RegistrationNumber></RegistrationNumbers><AccountsReceivables><AccountsReceivable><AccountGroup>ASC</AccountGroup><SettlementDetails><SettlementGroup EDICode=""DEACARSYD"" OwnerCode=""DEACARSYD""><Name>DEANO CARRIER SYDNEY</Name><Location Country=""Australia"" City=""Sydney"">AUSYD</Location><Addresses><Address AddressType=""MAIN""><AddressLine1>1 PITT STREET</AddressLine1><AddressCode>1 PITT STREET</AddressCode><CityOrSuburb>SYDNEY</CityOrSuburb><StateOrProvince>NSW</StateOrProvince><PostCode>2000</PostCode><Language>EN</Language><Location>AUSYD</Location><Sequence>1</Sequence><AddressCapabilities><AddressCapability AddressType=""MAIN"" /><AddressCapability IsMainAddress=""true"" AddressType=""OFC"" /></AddressCapabilities></Address></Addresses></SettlementGroup></SettlementDetails><AllowMultiCurrencyPayment>false</AllowMultiCurrencyPayment></AccountsReceivable></AccountsReceivables><AccountsPayables><AccountsPayable><AccountGroup>ASC</AccountGroup><SettlementDetails><SettlementGroup EDICode=""DEACARSYD"" OwnerCode=""DEACARSYD""><Name>DEANO CARRIER SYDNEY</Name><Location Country=""Australia"" City=""Sydney"">AUSYD</Location><Addresses><Address AddressType=""MAIN""><AddressLine1>1 PITT STREET</AddressLine1><AddressCode>1 PITT STREET</AddressCode><CityOrSuburb>SYDNEY</CityOrSuburb><StateOrProvince>NSW</StateOrProvince><PostCode>2000</PostCode><Language>EN</Language><Location>AUSYD</Location><Sequence>1</Sequence><AddressCapabilities><AddressCapability AddressType=""MAIN"" /><AddressCapability IsMainAddress=""true"" AddressType=""OFC"" /></AddressCapabilities></Address></Addresses></SettlementGroup></SettlementDetails></AccountsPayable></AccountsPayables></OrganisationDetails><Notes><Note><NoteType>Custom</NoteType><CustomNoteTypeName>VIP Detention No.</CustomNoteTypeName><NoteData>Port=DESTR, vip=VIP TESThallo</NoteData><NoteCreatedDateTime>2010-11-22T20:55:33.353+11:00</NoteCreatedDateTime></Note><Note><NoteType>DeliveryInstructionsNote</NoteType><NoteData>Kirsten Test Dataasdflasldf</NoteData><NoteCreatedDateTime>2008-08-07T01:23:58.773+10:00</NoteCreatedDateTime></Note></Notes></DebtorOrCreditor><TxnType>INV</TxnType><TxnCount>1</TxnCount><TxnCategory>FIN</TxnCategory><TxnNumber>00001003</TxnNumber><Description>AR INVOICE</Description><InvoiceDate>2011-02-08T13:21:00+11:00</InvoiceDate><InvTerm>COD</InvTerm><InvTermDays>0</InvTermDays><DueDate>2011-02-08T13:21:00+11:00</DueDate><PostDate>2011-02-08T13:21:00+11:00</PostDate><GLPeriod>201108</GLPeriod><Branch>BNE</Branch><Department>BRN</Department><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">100.00</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">110.00</LocalInvoiceAmtInclTax><LocalTaxAmount CurrencyCode=""AUD"">10.00</LocalTaxAmount><LocalWHTAmount CurrencyCode=""AUD"">0.00</LocalWHTAmount><OsInvoiceAmtExclTax CurrencyCode=""AUD"">100.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">110.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">10.00</OsTaxAmount><OsWHTAmount CurrencyCode=""AUD"">0</OsWHTAmount><CashBasisTaxIndicator>N</CashBasisTaxIndicator><CreatedUserId>CWSupport</CreatedUserId><TxnHeaderGUID>headerGUID</TxnHeaderGUID><TxnLines><TxnLine><LineType>REV</LineType><Sequence>1</Sequence><GLAccount>1210.20.10</GLAccount><Description>WAREHOUSE HANDLING COSTS ACTUAL</Description><Branch>BNE</Branch><Department>BRN</Department><LocalInvoiceAmtExclTax CurrencyCode=""AUD"">100.00</LocalInvoiceAmtExclTax><LocalInvoiceAmtInclTax CurrencyCode=""AUD"">110.00</LocalInvoiceAmtInclTax><LocalTaxAmount CurrencyCode=""AUD"">10.00</LocalTaxAmount><LocalWHTAmount CurrencyCode=""AUD"">0.00</LocalWHTAmount><OsInvoiceAmtExclTax CurrencyCode=""AUD"">100.00</OsInvoiceAmtExclTax><OsInvoiceAmtInclTax CurrencyCode=""AUD"">110.00</OsInvoiceAmtInclTax><OsTaxAmount CurrencyCode=""AUD"">10.00</OsTaxAmount><OsWHTAmount CurrencyCode=""AUD"">0</OsWHTAmount><TaxCode>CAPGST</TaxCode><DepartmentActivity>Miscellaneous</DepartmentActivity><Weight>0</Weight><Volume>0</Volume><TxnLineGUID>lineGUID</TxnLineGUID><RevenueRecognitionDate>2011-02-08T13:21:00+11:00</RevenueRecognitionDate></TxnLine></TxnLines></FinancialInvoice>";
			CreateAndValidateInterchange(fileName, expectedHeaderFileName, expectedBodyFileName, expectedSubType, expectedMessage);
		}

		public void TestOrdersMessage()
		{
			string fileName = "Orders.xml";
			string expectedHeaderFileName = "OrdersHeader.xml";
			string expectedBodyFileName = "OrdersBody.xml";
			string expectedSubType = "ORD";
			string expectedMessage = @"<Order xmlns=""http://www.edi.com.au/EnterpriseService/""><Events><Event><Source>JobOrderHeader</Source><Code>OCF</Code><CodeDescription>Order Confirmed</CodeDescription><DateTime>2010-02-18T00:00:00+11:00</DateTime><PostedDateTime>2010-02-17T23:37:29.823+11:00</PostedDateTime><User>E</User><UserName>CargoWise Support (E)</UserName><IsEstimatedDate>false</IsEstimatedDate></Event><Event><Source>JobOrderHeader</Source><Code>ADD</Code><CodeDescription>Added a record to the system</CodeDescription><DateTime>2010-02-18T08:36:49.003+11:00</DateTime><PostedDateTime>2010-02-17T23:37:29.823+11:00</PostedDateTime><User>E</User><UserName>CargoWise Support (E)</UserName><IsEstimatedDate>false</IsEstimatedDate></Event><Event><Source>JobOrderLine</Source><Code>ADD</Code><CodeDescription>Added a record to the system</CodeDescription><DateTime>2010-02-18T08:58:35.957+11:00</DateTime><PostedDateTime>2010-02-17T23:59:22.92+11:00</PostedDateTime><User>E</User><UserName>CargoWise Support (E)</UserName><IsEstimatedDate>false</IsEstimatedDate></Event><Event><Source>JobOrderLine</Source><Code>ADD</Code><CodeDescription>Added a record to the system</CodeDescription><DateTime>2010-02-18T08:59:24.77+11:00</DateTime><PostedDateTime>2010-02-18T00:00:30.043+11:00</PostedDateTime><User>E</User><UserName>CargoWise Support (E)</UserName><IsEstimatedDate>false</IsEstimatedDate></Event></Events><OrderIdentifier><OrderNumber>P000001</OrderNumber><OrderNumberSplit>0</OrderNumberSplit></OrderIdentifier><OrderDetail><Buyer EDICode=""DAIMAC"" OwnerCode=""DAIMAC""><OrganisationDetails><Name>DAIWA MACHINERY TRADING</Name><Location Country=""Japan"" City=""Osaka"">JPOSA</Location><Addresses><Address AddressType=""MAIN""><AddressLine1>C.P.O. BOX 940</AddressLine1><AddressLine2>OSAKA  JAPAN  530-91</AddressLine2><AddressCode>PST: C.P.O. BOX 940</AddressCode><Language>EN</Language><Location>JPOSA</Location><Sequence>1</Sequence><AddressCapabilities><AddressCapability AddressType=""MAIN"" /><AddressCapability IsMainAddress=""true"" AddressType=""OFC"" /></AddressCapabilities></Address></Addresses></OrganisationDetails></Buyer><Supplier EDICode=""SADCON"" OwnerCode=""SADCON""><OrganisationDetails><Name>SADA CONCERIA SPA</Name><Location Country=""Italy"" City=""Milano"">ITMIL</Location><Addresses><Address AddressType=""MAIN""><AddressLine1>VIA VALCHIAMPO 5</AddressLine1><AddressCode>Pickup and Delivery Addre</AddressCode><CityOrSuburb>ZERMEGHEDO</CityOrSuburb><Location>ITMIL</Location><Sequence>1</Sequence><AddressCapabilities><AddressCapability AddressType=""MAIN"" /><AddressCapability IsMainAddress=""true"" AddressType=""OFC"" /></AddressCapabilities></Address><Address><AddressLine1>VIA VALCHIAMPO 5</AddressLine1><AddressLine2>ZERMEGHEDO</AddressLine2><AddressCode>PST: VIA VALCHIAMPO 5</AddressCode><Language>EN</Language><Sequence>2</Sequence><AddressCapabilities><AddressCapability IsMainAddress=""false"" AddressType=""PST"" /></AddressCapabilities></Address></Addresses></OrganisationDetails></Supplier><ConfirmNumber>6346236</ConfirmNumber><ConfirmDate>2010-02-18T00:00:00+11:00</ConfirmDate><InvoiceNumber>262356236</InvoiceNumber><OrderStatus>INC</OrderStatus><Description>sdGSDAG</Description><OrderDateTime>2010-02-18T08:36:00+11:00</OrderDateTime><OrderTotal CurrencyCode=""USD"">16000.0000</OrderTotal><ExchRateBasis>F</ExchRateBasis><Incoterm>FOB</Incoterm><TransportMode>AIR</TransportMode><ContainerMode>LSE</ContainerMode><Milestones /><ShipmentPlanning><GoodsOrigin Country=""Italy"" City=""Milano"">ITMIL</GoodsOrigin><GoodsDestination Country=""Japan"" City=""Osaka"">JPOSA</GoodsDestination><LoadPort Country=""Italy"" City=""Milano"">ITMIL</LoadPort><DischargePort Country=""Japan"" City=""Osaka"">JPOSA</DischargePort><Packs DimensionType=""PLT"">0</Packs><Weight DimensionType=""KG"">0.000</Weight><Volume DimensionType=""M3"">0.000</Volume></ShipmentPlanning><Custom /></OrderDetail><OrderLines><OrderLine><OrderLineNo>1</OrderLineNo><OrderSubLineNo>1</OrderSubLineNo><OrderLineDetail><Product>2363463</Product><Description>iPod</Description><QtyOrdered DimensionType=""UNT"">50.00000</QtyOrdered><QtyInvoiced DimensionType=""UNT"">1.00000</QtyInvoiced><QtyReceived DimensionType=""UNT"">1.00000</QtyReceived><QtyReceivedToDate DimensionType=""UNT"">1.00000</QtyReceivedToDate><InnerPacks>50.000</InnerPacks><OuterPacks>0.000</OuterPacks><ItemPrice>200.0000</ItemPrice><LinePrice>10000.0000</LinePrice><LineStatus>PLC</LineStatus><Custom /><Weight>0.000</Weight><Volume>0.000</Volume></OrderLineDetail></OrderLine><OrderLine><OrderLineNo>2</OrderLineNo><OrderSubLineNo>1</OrderSubLineNo><OrderLineDetail><Product>63464366</Product><Description>iPad</Description><QtyOrdered DimensionType=""UNT"">10.00000</QtyOrdered><QtyInvoiced DimensionType=""UNT"">1.00000</QtyInvoiced><QtyReceived DimensionType=""UNT"">1.00000</QtyReceived><QtyReceivedToDate DimensionType=""UNT"">1.00000</QtyReceivedToDate><InnerPacks>10.000</InnerPacks><OuterPacks>0.000</OuterPacks><ItemPrice>600.0000</ItemPrice><LinePrice>6000.0000</LinePrice><LineStatus>PLC</LineStatus><Custom /><Weight>0.000</Weight><Volume>0.000</Volume></OrderLineDetail></OrderLine></OrderLines></Order>";
			CreateAndValidateInterchange(fileName, expectedHeaderFileName, expectedBodyFileName, expectedSubType, expectedMessage);
		}

		public void TestProductsMessage()
		{
			string fileName = "Products.xml";
			string expectedHeaderFileName = "ProductsHeader.xml";
			string expectedBodyFileName = "ProductsBody.xml";
			string expectedSubType = "PRD";
			string expectedMessage = @"<Product xmlns=""http://www.edi.com.au/EnterpriseService/""><ProductCode>PRODUCT1</ProductCode><ProductDescription>PRODUCT DESCRIPTION 1</ProductDescription><StockUnit>UNT</StockUnit><RelatedOrganisations><RelatedOrganisation><Organisation EDICode=""KIRSTESYD"" OwnerCode=""KIRSTESYD""><OrganisationDetails><Name>KIRSTEN COMPANY PTY LTD</Name><Location Country=""Australia"" City=""Sydney"">AUSYD</Location><Addresses><Address AddressType=""MAIN""><AddressLine1>124 MARINE PARADE</AddressLine1><AddressCode>MARINE PARADE</AddressCode><CityOrSuburb>MAROUBRA</CityOrSuburb><StateOrProvince>NSW</StateOrProvince><PostCode>2035</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">02 9003 2456</TelephoneNumber><TelephoneNumber NumberType=""Fax"">02 9000 1002</TelephoneNumber></TelephoneNumbers><Language>EN</Language><Location>AUSYD</Location><Sequence>1</Sequence><AddressCapabilities><AddressCapability AddressType=""MAIN"" /><AddressCapability IsMainAddress=""true"" AddressType=""OFC"" /></AddressCapabilities></Address><Address><AddressLine1>222 BRISBANE CRESCENT</AddressLine1><AddressCode>BRISBANE CRESCENT</AddressCode><CityOrSuburb>BRISBANE</CityOrSuburb><StateOrProvince>QLD</StateOrProvince><PostCode>4000</PostCode><Language>EN</Language><Sequence>2</Sequence><AddressCapabilities><AddressCapability IsMainAddress=""false"" AddressType=""PAD"" /></AddressCapabilities></Address><Address><AddressLine1>111 WAREHOUSE STREET</AddressLine1><AddressCode>C34</AddressCode><CityOrSuburb>SYDNEY</CityOrSuburb><StateOrProvince>NSW</StateOrProvince><PostCode>2000</PostCode><Language>EN</Language><Sequence>3</Sequence><AddressCapabilities><AddressCapability IsMainAddress=""true"" AddressType=""DLV"" /><AddressCapability IsMainAddress=""true"" AddressType=""PAD"" /><AddressCapability IsMainAddress=""true"" AddressType=""PIC"" /></AddressCapabilities></Address></Addresses><Contacts><Contact><Name>BNB0</Name><Salutation>Mr Contact Person</Salutation><Language>EN</Language><NotifyMode>EML</NotifyMode><AttachmentType>PDF</AttachmentType><EmailAddress>jsmith@charlesparsons.com</EmailAddress><Sequence>1</Sequence></Contact><Contact><Name>TFXF0</Name><Language>EN</Language><NotifyMode>EML</NotifyMode><AttachmentType>PDF</AttachmentType><Sequence>2</Sequence></Contact></Contacts><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>LSC</NumberType><Number>NON-FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>NZ</CountryOfRegistration><NumberType>GST</NumberType><Number>FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>DK</CountryOfRegistration><NumberType>GBR</NumberType><Number>FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>DK</CountryOfRegistration><NumberType>GCR</NumberType><Number>FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>DK</CountryOfRegistration><NumberType>CVR</NumberType><Number>FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>DK</CountryOfRegistration><NumberType>APC</NumberType><Number>NON-FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>DK</CountryOfRegistration><NumberType>CMM</NumberType><Number>NON-FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>US</CountryOfRegistration><NumberType>LSC</NumberType><Number>EMIN25</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails><Notes><Note><NoteType>Custom</NoteType><CustomNoteTypeName>VIP Detention No.</CustomNoteTypeName><NoteData>Port=DESTR, vip=VIP TESThallo</NoteData><NoteCreatedDateTime>2010-11-22T20:55:33.353+11:00</NoteCreatedDateTime></Note><Note><NoteType>DeliveryInstructionsNote</NoteType><NoteData>Kirsten Test Dataasdflasldf</NoteData><NoteCreatedDateTime>2008-08-07T01:23:58.773+10:00</NoteCreatedDateTime></Note></Notes></Organisation><RelationshipType>OWN</RelationshipType><RFAttributeConfirm>NON</RFAttributeConfirm></RelatedOrganisation></RelatedOrganisations><DimensionDetails><DimensionUnit>CM</DimensionUnit><GrossWeight DimensionType=""KG"">1.100</GrossWeight><NetWeight>1.000</NetWeight><Volume DimensionType=""M3"">0.100</Volume></DimensionDetails><UnitConversions><UnitConversion><Package DimensionType=""PLT"">10.000000</Package><ParentUQ>UNT</ParentUQ></UnitConversion><UnitConversion><Package DimensionType=""UNT"">0.100000</Package><ParentUQ>M3</ParentUQ></UnitConversion><UnitConversion><Package DimensionType=""KG"">1.100000</Package><ParentUQ>UNT</ParentUQ></UnitConversion><UnitConversion><Package DimensionType=""M3"">0.100000</Package><ParentUQ>UNT</ParentUQ></UnitConversion></UnitConversions><BillOfMaterials><BillOfMaterial><AllowResale>true</AllowResale><AllowDisassemblyOfKit>true</AllowDisassemblyOfKit></BillOfMaterial></BillOfMaterials><ClientDefinedDetails /><BasicStockControl /><ClientWarehouseDetails><ClientWarehouseDetail><Client EDICode=""KIRSTESYD"" OwnerCode=""KIRSTESYD""><OrganisationDetails><Name>KIRSTEN COMPANY PTY LTD</Name><Location Country=""Australia"" City=""Sydney"">AUSYD</Location><Addresses><Address AddressType=""MAIN""><AddressLine1>124 MARINE PARADE</AddressLine1><AddressCode>MARINE PARADE</AddressCode><CityOrSuburb>MAROUBRA</CityOrSuburb><StateOrProvince>NSW</StateOrProvince><PostCode>2035</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">02 9003 2456</TelephoneNumber><TelephoneNumber NumberType=""Fax"">02 9000 1002</TelephoneNumber></TelephoneNumbers><Language>EN</Language><Location>AUSYD</Location><Sequence>1</Sequence><AddressCapabilities><AddressCapability AddressType=""MAIN"" /><AddressCapability IsMainAddress=""true"" AddressType=""OFC"" /></AddressCapabilities></Address><Address><AddressLine1>222 BRISBANE CRESCENT</AddressLine1><AddressCode>BRISBANE CRESCENT</AddressCode><CityOrSuburb>BRISBANE</CityOrSuburb><StateOrProvince>QLD</StateOrProvince><PostCode>4000</PostCode><Language>EN</Language><Sequence>2</Sequence><AddressCapabilities><AddressCapability IsMainAddress=""false"" AddressType=""PAD"" /></AddressCapabilities></Address><Address><AddressLine1>111 WAREHOUSE STREET</AddressLine1><AddressCode>C34</AddressCode><CityOrSuburb>SYDNEY</CityOrSuburb><StateOrProvince>NSW</StateOrProvince><PostCode>2000</PostCode><Language>EN</Language><Sequence>3</Sequence><AddressCapabilities><AddressCapability IsMainAddress=""true"" AddressType=""DLV"" /><AddressCapability IsMainAddress=""true"" AddressType=""PAD"" /><AddressCapability IsMainAddress=""true"" AddressType=""PIC"" /></AddressCapabilities></Address></Addresses><Contacts><Contact><Name>BNB0</Name><Salutation>Mr Contact Person</Salutation><Language>EN</Language><NotifyMode>EML</NotifyMode><AttachmentType>PDF</AttachmentType><EmailAddress>jsmith@charlesparsons.com</EmailAddress><Sequence>1</Sequence></Contact><Contact><Name>TFXF0</Name><Language>EN</Language><NotifyMode>EML</NotifyMode><AttachmentType>PDF</AttachmentType><Sequence>2</Sequence></Contact></Contacts><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>LSC</NumberType><Number>NON-FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>NZ</CountryOfRegistration><NumberType>GST</NumberType><Number>FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>DK</CountryOfRegistration><NumberType>GBR</NumberType><Number>FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>DK</CountryOfRegistration><NumberType>GCR</NumberType><Number>FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>DK</CountryOfRegistration><NumberType>CVR</NumberType><Number>FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>DK</CountryOfRegistration><NumberType>APC</NumberType><Number>NON-FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>DK</CountryOfRegistration><NumberType>CMM</NumberType><Number>NON-FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>US</CountryOfRegistration><NumberType>LSC</NumberType><Number>EMIN25</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails><Notes><Note><NoteType>Custom</NoteType><CustomNoteTypeName>VIP Detention No.</CustomNoteTypeName><NoteData>Port=DESTR, vip=VIP TESThallo</NoteData><NoteCreatedDateTime>2010-11-22T20:55:33.353+11:00</NoteCreatedDateTime></Note><Note><NoteType>DeliveryInstructionsNote</NoteType><NoteData>Kirsten Test Dataasdflasldf</NoteData><NoteCreatedDateTime>2008-08-07T01:23:58.773+10:00</NoteCreatedDateTime></Note></Notes></Client><WarehouseCode>e9555d7f-7bfe-40bf-99d2-79eb369a2579</WarehouseCode></ClientWarehouseDetail></ClientWarehouseDetails></Product>";
			CreateAndValidateInterchange(fileName, expectedHeaderFileName, expectedBodyFileName, expectedSubType, expectedMessage);
		}

		public void TestProductsGeneratedMessage()
		{
			string fileName = "ProductGenerated.xml";
			string expectedHeaderFileName = "ProductGeneratedHeader.xml";
			string expectedBodyFileName = "ProductGeneratedBody.xml";
			string expectedSubType = "PRD";
			string expectedMessage = MessageHandlerTestHelper.AnyTextKeyWord;
			CreateAndValidateInterchange(fileName, expectedHeaderFileName, expectedBodyFileName, expectedSubType, expectedMessage);
		}

		public void TestShipmentsMessage()
		{
			string fileName = "Shipments.xml";
			string expectedHeaderFileName = "ShipmentsHeader.xml";
			string expectedBodyFileName = "ShipmentsBody.xml";
			string expectedSubType = "SHP";
			string expectedMessage = @"<Shipment xmlns=""http://www.edi.com.au/EnterpriseService/"">" + MessageHandlerTestHelper.AnyTextKeyWord + "</Shipment>";
			CreateAndValidateInterchange(fileName, expectedHeaderFileName, expectedBodyFileName, expectedSubType, expectedMessage);
		}

		public void TestWhsDocketsMessage()
		{
			string fileName = "WhsDockets.xml";
			string expectedHeaderFileName = "WhsDocketsHeader.xml";
			string expectedBodyFileName = "WhsDocketsBody.xml";
			string expectedSubType = "WHD";
			string expectedMessage = @"<WhsDocket xmlns=""http://www.edi.com.au/EnterpriseService/""><Identifier><Client EDICode=""KIRSTESYD"" OwnerCode=""KIRSTESYD""><OrganisationDetails><Name>KIRSTEN COMPANY PTY LTD</Name><Location Country=""Australia"" City=""Sydney"">AUSYD</Location><Addresses><Address AddressType=""MAIN""><AddressLine1>124 MARINE PARADE</AddressLine1><AddressCode>MARINE PARADE</AddressCode><CityOrSuburb>MAROUBRA</CityOrSuburb><StateOrProvince>NSW</StateOrProvince><PostCode>2035</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">02 9003 2456</TelephoneNumber><TelephoneNumber NumberType=""Fax"">02 9000 1002</TelephoneNumber></TelephoneNumbers><Language>EN</Language><Location>AUSYD</Location><Sequence>1</Sequence><AddressCapabilities><AddressCapability AddressType=""MAIN"" /><AddressCapability IsMainAddress=""true"" AddressType=""OFC"" /></AddressCapabilities></Address></Addresses><Contacts><Contact><Name>BNB0</Name><Salutation>Mr Contact Person</Salutation><Language>EN</Language><NotifyMode>EML</NotifyMode><AttachmentType>PDF</AttachmentType><EmailAddress>jsmith@charlesparsons.com</EmailAddress><Sequence>1</Sequence></Contact><Contact><Name>TFXF0</Name><Language>EN</Language><NotifyMode>EML</NotifyMode><AttachmentType>PDF</AttachmentType><Sequence>2</Sequence></Contact></Contacts><RegistrationNumbers><RegistrationNumber><CountryOfRegistration>AU</CountryOfRegistration><NumberType>LSC</NumberType><Number>NON-FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>NZ</CountryOfRegistration><NumberType>GST</NumberType><Number>FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>DK</CountryOfRegistration><NumberType>GBR</NumberType><Number>FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>DK</CountryOfRegistration><NumberType>GCR</NumberType><Number>FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>DK</CountryOfRegistration><NumberType>CVR</NumberType><Number>FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>DK</CountryOfRegistration><NumberType>APC</NumberType><Number>NON-FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>DK</CountryOfRegistration><NumberType>CMM</NumberType><Number>NON-FINANCIAL</Number></RegistrationNumber><RegistrationNumber><CountryOfRegistration>US</CountryOfRegistration><NumberType>LSC</NumberType><Number>EMIN25</Number></RegistrationNumber></RegistrationNumbers></OrganisationDetails><Notes><Note><NoteType>Custom</NoteType><CustomNoteTypeName>VIP Detention No.</CustomNoteTypeName><NoteData>Port=DESTR, vip=VIP TEST</NoteData><NoteCreatedDateTime>2010-11-22T20:55:33.353+11:00</NoteCreatedDateTime></Note><Note><NoteType>DeliveryInstructionsNote</NoteType><NoteData>Kirsten Test Dataasdflasldf</NoteData><NoteCreatedDateTime>2008-08-07T01:23:58.773+10:00</NoteCreatedDateTime></Note></Notes></Client><Reference>ORDER NO1</Reference><DocketID>W00000004</DocketID><DocketType>WOH</DocketType><ActionType>CON</ActionType></Identifier><DocketDetail><WarehouseCode>DEM</WarehouseCode><CustomerReference>CUSTOMER REF</CustomerReference><Units>60.000</Units><Packages DimensionType=""UNT"">60</Packages><Pallets>0</Pallets><Weight DimensionType=""KG"">66.000</Weight><Cubic DimensionType=""M3"">6.000</Cubic><TransportCompany AddressType=""TRA""><AddressReference><AddressSequenceRef>1</AddressSequenceRef><Organisation EDICode=""DEACARSYD"" OwnerCode=""DEACARSYD""><OrganisationDetails><Name>DEANO CARRIER SYDNEY</Name><Location Country=""Australia"" City=""Sydney"">AUSYD</Location><Addresses><Address AddressType=""MAIN""><AddressLine1>1 PITT STREET</AddressLine1><AddressCode>1 PITT STREET</AddressCode><CityOrSuburb>SYDNEY</CityOrSuburb><StateOrProvince>NSW</StateOrProvince><PostCode>2000</PostCode><Language>EN</Language><Location>AUSYD</Location><Sequence>1</Sequence><AddressCapabilities><AddressCapability AddressType=""MAIN"" /><AddressCapability IsMainAddress=""true"" AddressType=""OFC"" /></AddressCapabilities></Address></Addresses></OrganisationDetails></Organisation></AddressReference></TransportCompany><TransportReference>TRANSPORT REF</TransportReference><TransportInsurance>0.0000</TransportInsurance><ShipperCODAmount>0.0000</ShipperCODAmount><CustomerOrderDetail><OrderType>ORD</OrderType><DateRequired>2011-01-10T00:00:00+11:00</DateRequired><Consignee AddressType=""CEA""><TelephoneNumbers><TelephoneNumber NumberType=""Business"">+61 40 1234 5678</TelephoneNumber><TelephoneNumber NumberType=""Fax"">+61 40 1234 1000</TelephoneNumber></TelephoneNumbers><AddressReference><AddressSequenceRef>1</AddressSequenceRef><Organisation EDICode=""HAMBURHAM"" OwnerCode=""HAMBURHAM""><OrganisationDetails><Name>HAMBURG COMPANY</Name><Location Country=""Germany"" City=""Hamburg"">DEHAM</Location><Addresses><Address AddressType=""MAIN""><AddressLine1>HAUPTSTR 111</AddressLine1><AddressCode>HAUPTSTR 111</AddressCode><CityOrSuburb>HAMBURG-BERGEDORF</CityOrSuburb><StateOrProvince>HH</StateOrProvince><PostCode>20345</PostCode><TelephoneNumbers><TelephoneNumber NumberType=""Business"">+61 40 1234 5678</TelephoneNumber><TelephoneNumber NumberType=""Fax"">+61 40 1234 1000</TelephoneNumber></TelephoneNumbers><Language>EN</Language><Location>DEHAM</Location><Sequence>1</Sequence><AddressCapabilities><AddressCapability AddressType=""MAIN"" /><AddressCapability IsMainAddress=""true"" AddressType=""OFC"" /></AddressCapabilities></Address></Addresses></OrganisationDetails></Organisation></AddressReference></Consignee><GoodsBilledTo AddressType=""GBA""><AddressReference><AddressSequenceRef>1</AddressSequenceRef><Organisation EDICode=""BILTONSYD"" OwnerCode=""BILTONSYD""><OrganisationDetails><Name>BILL TO NAME</Name><Location Country=""Australia"" City=""Sydney"">AUSYD</Location><Addresses><Address AddressType=""MAIN""><AddressLine1>BILL TO ADDRESS 1</AddressLine1><AddressLine2>BILL TO ADDRESS 2</AddressLine2><AddressCode>BILL TO ADDRESS 1</AddressCode><CityOrSuburb>BILL TO CITY</CityOrSuburb><StateOrProvince>NSW</StateOrProvince><PostCode>2000</PostCode><Language>EN</Language><Location>AUSYD</Location><Sequence>1</Sequence><AddressCapabilities><AddressCapability AddressType=""MAIN"" /><AddressCapability IsMainAddress=""true"" AddressType=""OFC"" /></AddressCapabilities></Address></Addresses><Contacts><Contact><Name>MARK  COKES</Name><Language>EN</Language><NotifyMode>EML</NotifyMode><AttachmentType>PDF</AttachmentType><Sequence>1</Sequence></Contact></Contacts></OrganisationDetails></Organisation></AddressReference></GoodsBilledTo></CustomerOrderDetail><Status>FIN</Status><CustomAttributes><CustomAttrib1>NETWORK PRINTER</CustomAttrib1><CustomDate1>2011-11-11T00:00:00+11:00</CustomDate1><CustomFlag1>true</CustomFlag1></CustomAttributes></DocketDetail><DocketLines><DocketLine><Product>PRODUCT1</Product><Description>PRODUCT DESCRIPTION 1</Description><QuantityFromClientOrder>20.00</QuantityFromClientOrder><QuantityActuallyOrdered>20.00</QuantityActuallyOrdered><ProductUQ>UNT</ProductUQ><LineAttributes /><CustomerOrderLineDetail><Pricing><RecommendedUnitPrice>0.0000</RecommendedUnitPrice><UnitDiscount>0.00</UnitDiscount><UnitDiscountAmount>0.0000</UnitDiscountAmount><UnitPriceAfterDiscount>0.0000</UnitPriceAfterDiscount><ExtendedPrice>0.0000</ExtendedPrice></Pricing><ProjectedShortfallQuantity>0</ProjectedShortfallQuantity></CustomerOrderLineDetail><LineNumber>2</LineNumber><SubLineNumber>1</SubLineNumber><CustomsData><EntryLineNumber>0</EntryLineNumber><CustomsQuantity>0</CustomsQuantity><BondedWhsQuantity>0</BondedWhsQuantity><ValueForDuty>0</ValueForDuty><TILVAmount>0</TILVAmount></CustomsData><Confirmation><Lines><Line><Quantity>20.000</Quantity><QuantityUQ>UNT</QuantityUQ></Line></Lines><Quantity>20.000</Quantity></Confirmation></DocketLine><DocketLine><Product>PRODUCT1</Product><Description>PRODUCT DESCRIPTION 1</Description><QuantityFromClientOrder>10.00</QuantityFromClientOrder><QuantityActuallyOrdered>10.00</QuantityActuallyOrdered><ProductUQ>UNT</ProductUQ><LineAttributes /><CustomerOrderLineDetail><Pricing><RecommendedUnitPrice>0.0000</RecommendedUnitPrice><UnitDiscount>0.00</UnitDiscount><UnitDiscountAmount>0.0000</UnitDiscountAmount><UnitPriceAfterDiscount>0.0000</UnitPriceAfterDiscount><ExtendedPrice>0.0000</ExtendedPrice></Pricing><ProjectedShortfallQuantity>0</ProjectedShortfallQuantity></CustomerOrderLineDetail><LineNumber>1</LineNumber><SubLineNumber>1</SubLineNumber><CustomsData><EntryLineNumber>0</EntryLineNumber><CustomsQuantity>0</CustomsQuantity><BondedWhsQuantity>0</BondedWhsQuantity><ValueForDuty>0</ValueForDuty><TILVAmount>0</TILVAmount></CustomsData><Confirmation><Lines><Line><Quantity>10.000</Quantity><QuantityUQ>UNT</QuantityUQ></Line></Lines><Quantity>10.000</Quantity></Confirmation></DocketLine><DocketLine><Product>PRODUCT2</Product><Description>PRODUCT DESCRIPTION 2</Description><QuantityFromClientOrder>30.00</QuantityFromClientOrder><QuantityActuallyOrdered>30.00</QuantityActuallyOrdered><ProductUQ>UNT</ProductUQ><LineAttributes /><CustomerOrderLineDetail><Pricing><RecommendedUnitPrice>0.0000</RecommendedUnitPrice><UnitDiscount>0.00</UnitDiscount><UnitDiscountAmount>0.0000</UnitDiscountAmount><UnitPriceAfterDiscount>0.0000</UnitPriceAfterDiscount><ExtendedPrice>0.0000</ExtendedPrice></Pricing><ProjectedShortfallQuantity>0</ProjectedShortfallQuantity></CustomerOrderLineDetail><LineNumber>3</LineNumber><SubLineNumber>1</SubLineNumber><CustomsData><EntryLineNumber>0</EntryLineNumber><CustomsQuantity>0</CustomsQuantity><BondedWhsQuantity>0</BondedWhsQuantity><ValueForDuty>0</ValueForDuty><TILVAmount>0</TILVAmount></CustomsData><Confirmation><Lines><Line><Quantity>30.000</Quantity><QuantityUQ>UNT</QuantityUQ></Line></Lines><Quantity>30.000</Quantity></Confirmation></DocketLine></DocketLines><Events InitialDataExported=""2011-01-14T12:19:05.367+11:00""><Event><Source>WhsDocket</Source><Code>ADD</Code><CodeDescription>Added a record to the system</CodeDescription><DateTime>2011-01-14T12:14:07.717+11:00</DateTime><PostedDateTime>2011-01-14T02:14:58.493+11:00</PostedDateTime><User>E</User><UserName>CargoWise Support (E)</UserName><IsEstimatedDate>false</IsEstimatedDate></Event><Event><Source>JobHeader</Source><Code>ADD</Code><CodeDescription>Added a record to the system</CodeDescription><DateTime>2011-01-14T12:14:55.557+11:00</DateTime><PostedDateTime>2011-01-14T02:14:58.493+11:00</PostedDateTime><User>E</User><UserName>CargoWise Support (E)</UserName><IsEstimatedDate>false</IsEstimatedDate></Event><Event><Source>WhsDocket</Source><Code>WHE</Code><CodeDescription>Warehouse Job Entered</CodeDescription><DateTime>2011-01-14T12:14:58.423+11:00</DateTime><PostedDateTime>2011-01-14T02:14:58.493+11:00</PostedDateTime><User>E</User><UserName>CargoWise Support (E)</UserName><IsEstimatedDate>false</IsEstimatedDate></Event><Event><Source>JobHeader</Source><Code>JOP</Code><CodeDescription>Job Open</CodeDescription><DateTime>2011-01-14T12:14:58.477+11:00</DateTime><PostedDateTime>2011-01-14T02:14:58.493+11:00</PostedDateTime><User>E</User><UserName>CargoWise Support (E)</UserName><IsEstimatedDate>false</IsEstimatedDate></Event><Event><Source>WhsDocket</Source><Code>WHI</Code><CodeDescription>Warehouse Order Picking</CodeDescription><DateTime>2011-01-14T12:15:58.423+11:00</DateTime><PostedDateTime>2011-01-14T02:15:06.727+11:00</PostedDateTime><User>E</User><UserName>CargoWise Support (E)</UserName><IsEstimatedDate>false</IsEstimatedDate></Event><Event><Source>WhsDocket</Source><Code>FIN</Code><CodeDescription>Item/Document/Job Finalised</CodeDescription><DateTime>2011-01-14T12:18:19.113+11:00</DateTime><PostedDateTime>2011-01-14T02:18:39.903+11:00</PostedDateTime><User>E</User><UserName>CargoWise Support (E)</UserName><IsEstimatedDate>false</IsEstimatedDate></Event></Events></WhsDocket>";
			CreateAndValidateInterchange(fileName, expectedHeaderFileName, expectedBodyFileName, expectedSubType, expectedMessage);
		}

		public void TestUnknownMessage()
		{
			string fileName = "Unknown.xml";
			string expectedHeaderFileName = null;
			string expectedBodyFileName = fileName;
			string expectedSubType = null;
			CreateAndValidateInterchange(fileName, expectedHeaderFileName, expectedBodyFileName, expectedSubType, null, 0);
		}

		public void TestCreateInterchangeHandlesXmlException()
		{
			string fileName = "InvalidXmlMessage.xml";
			EDIInterchange interchange = CreateAndValidateBaseInterchange(fileName);
			AssertNotNull("interchange", interchange);
			CombineAssertions(delegate
			{
				AssertEquals("interchange.EI_ApplicationCode", XmlEDIInterchange.ApplicationCodes.XMS, interchange.EI_ApplicationCode);
				AssertEquals("interchange.EI_Status", XmlEDIInterchange.Status.Failed, interchange.EI_Status);
			});

			var failNotes = interchange.Notes.FindByDescription("Failure Log");
			AssertNotNull("failNotes", failNotes);
			AssertEquals("failNotes.Length", 1, failNotes.Length);
			var failNote = failNotes[0];
			var failedMessage = @"Message has invalid XML exception.
System.Xml.XmlException: Root element is missing.
   at System.Xml.XmlTextReaderImpl.Throw(Exception e)
			".Trim();
			AssertStartsWith("failNote.ST_NoteDataAsText", failedMessage, failNote.ST_NoteDataAsText);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1043:EAdaptorNamingRule", Justification = "Testing")]
		[TestDate(2014, 1, 29, 15, 16, 17)]
		public void TestSendAcknowledgement()
		{
			AssertEquals("Precondition", 0, Factory.GetDatabaseCount(typeof(EDIInterchange)));

			#region EventsWithAcknowledgementRequestAndInvalidNodesInPayload.xml Moved Here to Suppress "eAdaptor" Code Analysis Rule. Testing legacy functionality that still needs to work.

			const string eventsWithAcknowledgementRequestAndInvalidNodesInPayload = @"<ns0:XmlInterchange xmlns:ns0=""http://www.edi.com.au/EnterpriseService/"">
  <ns0:InterchangeInfo>
    <ns0:Acknowledgement>
      <ns0:Required>OnAll</ns0:Required>
      <!-- Options: OnAll, OnError, OnSuccess -->
      <ns0:Channel>eAdapter</ns0:Channel>
      <!-- Options: eHub, eAdapter -->
      <ns0:RecipientID>EDIEDIDAT</ns0:RecipientID>
      <ns0:ContextCollection>
        <ns0:Context>
          <ns0:Type>Good Fake President</ns0:Type>
          <ns0:Value>Harrison Ford</ns0:Value>
        </ns0:Context>
      </ns0:ContextCollection>
    </ns0:Acknowledgement>
  </ns0:InterchangeInfo>
  <ns0:Payload>
    <ns0:Events>
      <ns0:Event>
        <ns0:Source>NACCS</ns0:Source>
        <ns0:Code>IES</ns0:Code>
        <ns0:DateTime>2011-01-02T09:31:00+11:00</ns0:DateTime>
        <ns0:Information>Remark 1</ns0:Information>
        <ns0:ReferenceKeys>
          <ns0:ReferenceKey ReferenceKeyName=""ShipmentJobNumber"">
            S00001223</ns0:ReferenceKe>
          </ns0:ReferenceKeys>
      </ns0:Event>
    </ns0:Events>
  </ns0:Payload>
</ns0:XmlInterchange>";

			#endregion

			using (Factory.AddDisposableService())
			using (var messageText = new MemoryStream(Encoding.UTF8.GetBytes(eventsWithAcknowledgementRequestAndInvalidNodesInPayload)))
			{
				var eHubMessage = CreateeHubXMSXMLMesage(EDIInterchangeTypeList.Descriptions.XMS, messageText);
				var notification = new NotificationBuffer();

				CallHandler(eHubMessage, notification);

				CombineAssertions(delegate
				{
					AssertEquals(notification.AsString, "");
					AssertEquals(2, Factory.GetDatabaseCount(typeof(EDIInterchange)));
					var interchanges = Factory.Load<EDIInterchange>(new ZQuery()).OrderByDescending(e => e.EI_InterchangeNum).ToArray();

					var interchange = interchanges[0];

					AssertEquals("interchange.EI_Status", EDIInterchange.Status.Failed, interchange.EI_Status);
					var expectedInterchangeHeader =
				@"<ns0:InterchangeInfo xmlns:ns0=""http://www.edi.com.au/EnterpriseService/"">
    <ns0:Acknowledgement>
      <ns0:Required>OnAll</ns0:Required>
      <!-- Options: OnAll, OnError, OnSuccess -->
      <ns0:Channel>eAdapter</ns0:Channel>
      <!-- Options: eHub, eAdapter -->
       <ns0:RecipientID>EDIEDIDAT</ns0:RecipientID>
      <ns0:ContextCollection>
        <ns0:Context>
          <ns0:Type>Good Fake President</ns0:Type>
          <ns0:Value>Harrison Ford</ns0:Value>
        </ns0:Context>
      </ns0:ContextCollection>
    </ns0:Acknowledgement>
</ns0:InterchangeInfo>";
					Assert("interchange header", MessageHandlerTestHelper.CompareXmlString(expectedInterchangeHeader, interchange.EI_HeaderNText));
					AssertEquals("interchange footer should be empty", string.Empty, interchange.EI_FooterNText);
					using (var bodyTextReader = interchange.GetEI_BodyTextReader())
					{
						AssertEquals("interchange body text should be same as file content", eventsWithAcknowledgementRequestAndInvalidNodesInPayload, bodyTextReader.ReadToEnd());
					}
					AssertEquals(EDIInterchange.ApplicationCodes.XMS, interchange.EI_ApplicationCode);
					AssertEquals(EDIInterchangeTypeList.Codes.XMS, interchange.EI_InterchangeType);
					AssertEquals(EDIInterchange.Direction.Receive, interchange.EI_ReceiveTransmit);
					AssertEquals(eHubMessage.SenderID, interchange.EI_From);
					AssertEquals(eHubMessage.RecipientID, interchange.EI_To);
					AssertEquals(Company.Branches.FirstOrDefault().PK, interchange.EI_GB);
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
        <Value>Length of the message content is 1111 before Get Payload Sub Type
An error occurred: The 'ns0:ReferenceKey' start tag on line 25 position 12 does not match the end tag of 'ns0:ReferenceKe'. Line 26, position 24.</Value>
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
					var failedMessage = string.Format("Failed to create message - Interchange Session GUID - {0}, Sender - {1}, Recipient - {2}, Schema - http://www.edi.com.au/EnterpriseService/#XmlInterchange, Me".Trim(), interchange.EI_SessionGUID,
						interchange.EI_From, interchange.EI_To);
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
        <Value>Length of the message content is 1111 before Get Payload Sub Type
An error occurred: The 'ns0:ReferenceKey' start tag on line 25 position 12 does not match the end tag of 'ns0:ReferenceKe'. Line 26, position 24.</Value>
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

		void CreateAndValidateInterchange(string fileName, string expectedHeaderFileName, string expectedBodyFileName, string expectedSubType, string expectedMessage, int expectedMessageCount = 1)
		{
			CreateAndValidateInterchange(fileName, expectedHeaderFileName, expectedBodyFileName, expectedSubType, expectedMessage, EDIMessage.Status.Queued, expectedMessageCount);
		}

		void CreateAndValidateInterchange(string fileName, string expectedHeaderFileName, string expectedBodyFileName, string expectedSubType, string expectedMessage, string expectedMessageStatus, int expectedMessageCount = 1)
		{
			EDIInterchange interchange = CreateAndValidateBaseInterchange(fileName);
			if (expectedSubType != null)
			{
				AssertEquals(EDIInterchange.Status.Received, interchange.EI_Status);
			}
			else
			{
				AssertEquals(EDIInterchange.Status.Failed, interchange.EI_Status);
			}

			TextReader headerReader = interchange.GetEI_HeaderTextReader();
			AssertEquals("header is different", true, MessageHandlerTestHelper.CompareXmlString(headerReader.ReadToEnd(), expectedHeaderFileName == null ? @"" : GetFileResourceString(expectedHeaderFileName)));
			headerReader.Close();

			TextReader bodyReader = interchange.GetEI_BodyTextReader();
			AssertEquals("body is different", true, MessageHandlerTestHelper.CompareXmlString(bodyReader.ReadToEnd(), GetFileResourceString(expectedBodyFileName)));
			bodyReader.Close();

			TextReader footerReader = interchange.GetEI_FooterTextReader();
			AssertEquals("footer is different", true, MessageHandlerTestHelper.CompareXmlString(footerReader.ReadToEnd(), @""));
			footerReader.Close();

			AssertEquals("Message count", expectedMessageCount, interchange.ContainedMessages.Count);

			if (expectedMessage != null)
			{
				foreach (EDIMessage msg in interchange.ContainedMessages)
				{
					AssertEquals(expectedMessageStatus, msg.EM_Status);
					AssertEquals(false, msg.EM_IsTestMessage);
					AssertEquals(EDIMessageTypeList.Codes.XMS, msg.EM_MessageType);
					AssertEquals(expectedSubType, msg.EM_MessageSubType);
					AssertEquals(interchange.PK, msg.EM_EI);
					AssertEquals(interchange.EI_GB, msg.EM_GB);
					AssertEquals(interchange.EI_TransportType, msg.EM_TransportType);
					TextReader messageReader = msg.GetEM_MessageTextReader();
					AssertEquals("Message is different", true, MessageHandlerTestHelper.CompareXmlString(messageReader.ReadToEnd(), expectedMessage));
					messageReader.Close();
				}
			}
		}

		EDIInterchange CreateAndValidateBaseInterchange(string fileName)
		{
			AssertEquals("Precondition", 0, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			var message = CreateeHubXMSXMLMesage(EDIInterchangeTypeList.Descriptions.XMS, GetFileResource(fileName));
			var notification = new NotificationBuffer();
			CallHandler(message, notification);
			AssertEquals(notification.AsString, "");
			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			EDIInterchange interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());

			AssertEquals(EDIInterchange.Status.Received, interchange.EI_ReceiveTransmit);
			AssertEquals(message.SenderID, interchange.EI_From);
			AssertEquals(message.RecipientID, interchange.EI_To);
			AssertEquals(Company.Branches.FirstOrDefault().PK, interchange.EI_GB);
			AssertEquals(message.TrackingID, interchange.EI_SessionGUID);
			return interchange;
		}

		public void CallHandler(IeHubMessage message, INotifications notifier)
		{
			HandlerFactory.GetHandler(message.SchemaName).SaveMessage(message, Company, notifier);
		}

		public static IeHubMessage CreateeHubXMSXMLMesage(string schemaName, Stream stream)
		{
			return new eHubMessage(Guid.NewGuid(), "Sender1", "Recipient1", MessageSchemaType.Xml, ApplicationCodeList.Codes.XMS, schemaName, stream);
		}

		public static IeHubMessage CreateeHubCIMFLATMesage(string schemaName, Stream stream)
		{
			return new eHubMessage(Guid.NewGuid(), "Sender1", "Recipient1", MessageSchemaType.FlatFile, ApplicationCodeList.Codes.CIM, schemaName, stream);
		}

		public static IeHubMessage CreateeHubCAEXPFLATMesage(string schemaName, Stream stream)
		{
			return new eHubMessage(Guid.NewGuid(), "Sender1", "Recipient1", MessageSchemaType.FlatFile, ApplicationCodeList.Codes.CAEXP, schemaName, stream);
		}

		public static Stream GetFileResource(string resourceName)
		{
			var assembly = Assembly.GetExecutingAssembly();
			return assembly.GetManifestResourceStream(assembly.GetName().Name + ".TestFiles." + resourceName);
		}

		public static string GetFileResourceString(string resourceName)
		{
			return new StreamReader(GetFileResource(resourceName)).ReadToEnd();
		}
	}
}
