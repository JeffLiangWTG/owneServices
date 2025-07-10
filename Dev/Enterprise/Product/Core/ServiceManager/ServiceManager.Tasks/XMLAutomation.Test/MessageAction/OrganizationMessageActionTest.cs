using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.eHubMessaging.Tests;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	class OrganizationMessageActionTest : ImportMessageActionTest
	{
		public void TestExecuteAction_ImportOrg()
		{
			var factoryProvider = new BusinessObjectFactoryProvider(Factory);

			int noOfOrganizations = Factory.GetDatabaseCount(typeof(OrgHeader));

			var buffer = new NotificationBuffer();
			var message = CreateMessage(Factory);
			message.EM_MessageText = messageBody;
			var action = new OrganizationMessageAction(factoryProvider);

			message.Interchange.EI_HeaderText = interchangeHeader;
			message.Interchange.EI_FooterText = interchangeFooter;
			message.Interchange.EI_To = "blah";
			message.Interchange.EI_From = "blah blah";

			List<ITransactionParticipant> participants;
			Assert(action.ExecuteAction(message, buffer, out participants));
			Factory.Save();
			AssertEquals(noOfOrganizations + 1, Factory.GetDatabaseCount(typeof(OrgHeader)));
		}

		#region XML

		const string interchangeHeader = @"<?xml version=""1.0"" encoding=""utf-8""?><XmlInterchange xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Version=""1"" xmlns=""http://www.edi.com.au/EnterpriseService/""><InterchangeInfo><Date>2011-02-03T12:15:11</Date></InterchangeInfo><Payload><Organisations>";
		const string interchangeFooter = @"</Organisations></Payload></XmlInterchange>";
		const string messageBody = @"<Organisation EDICode=""TestOrg"" OwnerCode=""TestOrg"">
		<OrganisationDetails>
		  <Name>New Org</Name>
		  <Location Country=""Australia"" City=""Brisbane"">AUBNE</Location>
		  <Addresses>
			<Address AddressType=""MAIN"">
			  <AddressLine1>1024 ABBOTSFORD ROAD</AddressLine1>
			  <AddressCode>PST: 171 ABBOTSFORD ROAD</AddressCode>
			  <CityOrSuburb>BOWEN HILLS</CityOrSuburb>
			  <StateOrProvince>QLD</StateOrProvince>
			  <PostCode>4006</PostCode>
			  <TelephoneNumbers>
				<TelephoneNumber NumberType=""Business"">+61245781244</TelephoneNumber>
				<TelephoneNumber NumberType=""Fax"">+61388881244</TelephoneNumber>
			  </TelephoneNumbers>
			  <Language>EN</Language>
			  <Location>AUBNE</Location>
			  <Sequence>1</Sequence>
			  <AddressCapabilities>
				<AddressCapability AddressType=""MAIN"" />
				<AddressCapability IsMainAddress=""false"" AddressType=""DLV"" />
				<AddressCapability IsMainAddress=""true"" AddressType=""OFC"" />
				<AddressCapability IsMainAddress=""false"" AddressType=""PAD"" />
				<AddressCapability IsMainAddress=""false"" AddressType=""PIC"" />
				<AddressCapability IsMainAddress=""false"" AddressType=""PST"" />
			  </AddressCapabilities>
			</Address>
			<Address>
			  <AddressLine1>110 SYMONDS ST</AddressLine1>
			  <AddressLine2>TOWER 2 LVL 7</AddressLine2>
			  <AddressCode>110 SYMONDS ST</AddressCode>
			  <CityOrSuburb>AUCKLAND</CityOrSuburb>
			  <PostCode>1001</PostCode>
			  <Language>EN</Language>
			  <CompanyName>SYNGENTA CROP PROTECTION LTD</CompanyName>
			  <Sequence>2</Sequence>
			  <AddressCapabilities>
				<AddressCapability IsMainAddress=""true"" AddressType=""APM"" />
				<AddressCapability IsMainAddress=""false"" AddressType=""MSC"" />
				<AddressCapability IsMainAddress=""true"" AddressType=""PAD"" />
			  </AddressCapabilities>
			</Address>
			<Address>
			  <AddressLine1>1 MONIER PLACE</AddressLine1>
			  <AddressLine2>PENROSE</AddressLine2>
			  <AddressCode>M06</AddressCode>
			  <CityOrSuburb>PENROSE</CityOrSuburb>
			  <PostCode>2000</PostCode>
			  <Language>EN</Language>
			  <CompanyName>CSR BUILDING PRODUCTS NZ LTD</CompanyName>
			  <Sequence>3</Sequence>
			  <AddressCapabilities>
				<AddressCapability IsMainAddress=""false"" AddressType=""PAD"" />
				<AddressCapability IsMainAddress=""true"" AddressType=""PIC"" />
				<AddressCapability IsMainAddress=""true"" AddressType=""PST"" />
			  </AddressCapabilities>
			</Address>
		  </Addresses>
		  <Contacts>
			<Contact>
			  <Name>ADELE</Name>
			  <Language>EN</Language>
			  <NotifyMode>EML</NotifyMode>
			  <Phone>+61 (7) 38-3783</Phone>
			  <AttachmentType>PDF</AttachmentType>
			  <EmailAddress>asdfasdf@asdfsa.com</EmailAddress>
			  <Sequence>1</Sequence>
			</Contact>
			<Contact>
			  <Name>BridgetKramer</Name>
			  <Language>EN</Language>
			  <NotifyMode>PRN</NotifyMode>
			  <Phone>+61292760950</Phone>
			  <EmailAddress>adsfasdf@asdfasdf.com</EmailAddress>
			  <Sequence>2</Sequence>
			</Contact>
		  </Contacts>
		  <RegistrationNumbers>
			<RegistrationNumber>
			  <CountryOfRegistration>AU</CountryOfRegistration>
			  <NumberType>CCP</NumberType>
			  <Number>1234A</Number>
			  <AddressCode>PST: 171 ABBOTSFORD ROAD</AddressCode>
			</RegistrationNumber>
			<RegistrationNumber>
			  <CountryOfRegistration>AU</CountryOfRegistration>
			  <NumberType>UNC</NumberType>
			  <Number>dasfasdf</Number>
			</RegistrationNumber>
			<RegistrationNumber>
			  <CountryOfRegistration>AU</CountryOfRegistration>
			  <NumberType>CID</NumberType>
			  <Number>789456123</Number>
			</RegistrationNumber>
			<RegistrationNumber>
			  <CountryOfRegistration>AU</CountryOfRegistration>
			  <NumberType>LSC</NumberType>
			  <Number>Externalsystemcode</Number>
			</RegistrationNumber>
			<RegistrationNumber>
			  <CountryOfRegistration>AU</CountryOfRegistration>
			  <NumberType>GTN</NumberType>
			  <Number>121212</Number>
			</RegistrationNumber>
			<RegistrationNumber>
			  <CountryOfRegistration>GB</CountryOfRegistration>
			  <NumberType>VAT</NumberType>
			  <Number>testvatnum</Number>
			</RegistrationNumber>
			<RegistrationNumber>
			  <CountryOfRegistration>AU</CountryOfRegistration>
			  <NumberType>UOC</NumberType>
			  <Number>3333</Number>
			</RegistrationNumber>
			<RegistrationNumber>
			  <CountryOfRegistration>US</CountryOfRegistration>
			  <NumberType>MID</NumberType>
			  <Number>CNCHINAT5BEI</Number>
			  <AddressCode>M06</AddressCode>
			</RegistrationNumber>
		  </RegistrationNumbers>
		  <EDICodeMappings>
			<EDICodeMapping>
			  <Relationship>ORG</Relationship>
			  <EDICode>ABIMOT</EDICode>
			  <ForeignCode>1232300002</ForeignCode>
			</EDICodeMapping>
			<EDICodeMapping>
			  <Relationship>ORG</Relationship>
			  <EDICode>ABIGASBNE1</EDICode>
			  <ForeignCode>NUIPHAVNHAN</ForeignCode>
			</EDICodeMapping>
			<EDICodeMapping>
			  <Relationship>INC</Relationship>
			  <EDICode>CPT</EDICode>
			  <ForeignCode>FO</ForeignCode>
			</EDICodeMapping>
		  </EDICodeMappings>
		</OrganisationDetails>
	  </Organisation>";

		#endregion
	}
}
