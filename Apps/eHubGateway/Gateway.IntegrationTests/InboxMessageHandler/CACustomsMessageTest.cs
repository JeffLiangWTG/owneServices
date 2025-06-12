using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.eHub.Common.Extensions;
using NUnit.Framework;

namespace CargoWise.eHub.Gateway.IntegrationTests.InboxMessageHandler
{
	[TestFixture]
	public class CACustomsMessageTest : GatewayIntegrationTestBase
	{
		[Test]
		public void TestCACustomsMessageHandlerTest_SenderHasTestLicense_MessageInsertedIntoInbox()
		{
			using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(SampleMessage)))
			{
				var message = new eHubMessage(Guid.NewGuid(), SenderIDWithTestLicence, RecipientIDForProduction, MessageSchemaType.Xml, ApplicationCode, SchemaName, stream);

				var adapter = CreateAdapter(message.SenderID, TestClientPassword);
				adapter.Outbox.AddMessage(message);
				adapter.SendMessages();

				var content = stream.CompressAndEncode().ReadToEnd();

				Assert.IsTrue(FindInboxMessage(message.TrackingID, SenderPKWithTestLicence, ApplicationCode, RecipientPKForTest, SchemaName, "", "", content, 0));
				Assert.IsFalse(FindInboxMessage(message.TrackingID, SenderPKWithTestLicence, ApplicationCode, RecipientPKForProduction, SchemaName, "", "", content, 0));
			}
		}

		[Test]
		public void TestCACustomsMessageHandlerTest_SenderHasProductionLicense_MessageInsertedIntoInbox()
		{
			using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(SampleMessage)))
			{
				var message = new eHubMessage(Guid.NewGuid(), SenderIDWithProductionLicence, RecipientIDForProduction, MessageSchemaType.Xml, ApplicationCode, SchemaName, stream);

				var adapter = CreateAdapter(message.SenderID, TestAuthenticatedClientPassword);
				adapter.Outbox.AddMessage(message);
				adapter.SendMessages();

				var content = stream.CompressAndEncode().ReadToEnd();

				Assert.IsTrue(FindInboxMessage(message.TrackingID, SenderPKWithProductionLicense, ApplicationCode, RecipientPKForProduction, SchemaName, "", "", content, 0));
				Assert.IsFalse(FindInboxMessage(message.TrackingID, SenderPKWithProductionLicense, ApplicationCode, RecipientPKForTest, SchemaName, "", "", content, 0));
			}
		}

		[Test]
		public void TestCACustomsMessageHandlerTest_SenderHasInvalidLicense_MessageNotInsertedIntoInbox()
		{
			using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(SampleMessage)))
			{
				var message = new eHubMessage(Guid.NewGuid(), SenderIDWithInvalidLicence, RecipientIDForProduction, MessageSchemaType.Xml, ApplicationCode, SchemaName, stream);

				var adapter = CreateAdapter(message.SenderID, TestClientPassword);
				adapter.Outbox.AddMessage(message);
				var exception = Assert.Throws<eHubAdapterException>(() => adapter.SendMessages());
				Assert.That(exception.Message.Contains("Error retrieving licence details for sender"));

				var content = stream.CompressAndEncode().ReadToEnd();

				Assert.IsFalse(FindInboxMessage(message.TrackingID, SenderPKWithInvalidLicence, ApplicationCode, RecipientPKForTest, SchemaName, "", "", content, 0));
				Assert.IsFalse(FindInboxMessage(message.TrackingID, SenderPKWithInvalidLicence, ApplicationCode, RecipientPKForProduction, SchemaName, "", "", content, 0));
			}
		}

		[Test]
		public void TestCACustomsMessageHandlerTest_MonitoringSender_MessageInsertedIntoInbox()
		{
			using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(SampleMessage)))
			{
				var message = new eHubMessage(Guid.NewGuid(), CACustomsMonitoringClientID, RecipientIDForProduction, MessageSchemaType.Xml, ApplicationCode, SchemaName, stream);

				var adapter = CreateAdapter(message.SenderID, TestClientPassword);
				adapter.Outbox.AddMessage(message);
				adapter.SendMessages();

				var content = stream.CompressAndEncode().ReadToEnd();

				Assert.IsTrue(FindInboxMessage(message.TrackingID, CACustomsMonitoringClientPK, ApplicationCode, RecipientPKForProduction, SchemaName, "", "", content, 0));
				Assert.IsFalse(FindInboxMessage(message.TrackingID, CACustomsMonitoringClientPK, ApplicationCode, RecipientPKForTest, SchemaName, "", "", content, 0));
			}
		}

		private const string ApplicationCode = "UDM";
		private readonly Guid RecipientPKForTest = Guid.Parse("ABB9A931-370C-4B2E-A159-BD62117F45CF"); // ID = CACustomsTest

		private const string RecipientIDForProduction = "CACustoms";
		private readonly Guid RecipientPKForProduction = Guid.Parse("2337B2F2-CE59-40A6-BC56-D18779E21DD0");

		private const string SenderIDWithTestLicence = "ENTTSTSVZ";
		private readonly Guid SenderPKWithTestLicence = Guid.Parse("E0F40A8A-FA6B-43FE-A96E-1D731D34D1DA");

		private const string SenderIDWithProductionLicence = "ENTTSTSVR";
		private readonly Guid SenderPKWithProductionLicense = Guid.Parse("C3C7C44E-2BF3-43EB-97D7-04F2C12C70E1");

		private const string SenderIDWithInvalidLicence = "ENTTSTSVI";
		private readonly Guid SenderPKWithInvalidLicence = Guid.Parse("2C627C46-AE1F-4523-A4EA-DE75CB2EE326");

		private const string SchemaName = "http://www.cargowise.com/Schemas/Universal";
		private const string SampleMessage = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http:=""//www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
    <Shipment>
      <DataContext>
        <DataSourceCollection>
          <DataSource>
            <Type></Type>
            <Key></Key>
          </DataSource>
        </DataSourceCollection>
        <ActionPurpose>
          <Code></Code>
          <Description></Description>
        </ActionPurpose>
        <TriggerCount></TriggerCount>
        <TriggerDescription></TriggerDescription>
        <TriggerType></TriggerType>
        <RecipientRoleCollection>
          <RecipientRole>
            <Code></Code>
            <Description></Description>
          </RecipientRole>
        </RecipientRoleCollection>
      </DataContext>
      <Branch>
        <Code></Code>
        <Name></Name>
      </Branch>
      <LloydsIMO></LloydsIMO>
      <PortOfDischarge>
        <Code></Code>
        <Name></Name>
      </PortOfDischarge>
      <PortOfLoading>
        <Code></Code>
        <Name></Name>
      </PortOfLoading>
      <TransportMode>
        <Code></Code>
        <Description></Description>
      </TransportMode>
      <VesselCountryOfRegistration>
        <Code></Code>
        <Name></Name>
      </VesselCountryOfRegistration>
      <VesselName></VesselName>
      <VoyageFlightNo></VoyageFlightNo>
      <WayBillNumber></WayBillNumber>
      <WayBillType>
        <Code></Code>
        <Description></Description>
      </WayBillType>
      <AddInfoCollection>
        <AddInfo>
          <Key></Key>
          <Value></Value>
        </AddInfo>
      </AddInfoCollection>
      <DateCollection>
        <Date>
          <Type></Type>
          <IsEstimate></IsEstimate>
          <Value></Value>
        </Date>
      </DateCollection>
      <OrganizationAddressCollection>
        <OrganizationAddress>
          <AddressType></AddressType>
          <AddressShortCode></AddressShortCode>
          <OrganizationCode></OrganizationCode>
          <Address1></Address1>
          <Address2></Address2>
          <AddressOverride></AddressOverride>
          <City></City>
          <CompanyName></CompanyName>
          <Country>
            <Code></Code>
            <Name></Name>
          </Country>
          <Email></Email>
          <Fax></Fax>
          <GovRegNum></GovRegNum>
          <GovRegNumType>
            <Code></Code>
            <Description></Description>
          </GovRegNumType>
          <Phone></Phone>
          <Port>
            <Code></Code>
            <Name></Name>
          </Port>
          <Postcode></Postcode>
          <ScreeningStatus>
            <Code></Code>
            <Description></Description>
          </ScreeningStatus>
          <State></State>
          <RegistrationNumberCollection>
            <RegistrationNumber>
              <Type>
                <Code></Code>
                <Description></Description>
              </Type>
              <CountryOfIssue>
                <Code></Code>
                <Name></Name>
              </CountryOfIssue>
              <Value></Value>
            </RegistrationNumber>
          </RegistrationNumberCollection>
        </OrganizationAddress>
      </OrganizationAddressCollection>
      <SubShipmentCollection>
        <SubShipment>
          <DataContext>
            <DataSourceCollection>
              <DataSource>
                <Type></Type>
                <Key></Key>
              </DataSource>
            </DataSourceCollection>
            <ActionPurpose>
              <Code></Code>
              <Description></Description>
            </ActionPurpose>
            <TriggerCount></TriggerCount>
            <TriggerDescription></TriggerDescription>
            <TriggerType></TriggerType>
          </DataContext>
          <CommercialInfo>
            <CommercialChargeCollection>
              <CommercialCharge>
                <ChargeType>
                  <Code></Code>
                  <Description></Description>
                </ChargeType>
                <Amount></Amount>
              </CommercialCharge>
            </CommercialChargeCollection>
          </CommercialInfo>
          <GoodsValue></GoodsValue>
          <PortOfDestination>
            <Code></Code>
            <Name></Name>
          </PortOfDestination>
          <PortOfOrigin>
            <Code></Code>
            <Name></Name>
          </PortOfOrigin>
          <WayBillNumber></WayBillNumber>
          <WayBillType>
            <Code></Code>
            <Description></Description>
          </WayBillType>
          <AddInfoCollection>
            <AddInfo>
              <Key></Key>
              <Value></Value>
            </AddInfo>
          </AddInfoCollection>
          <ContainerCollection Content=""Complete="""">
            <Container>
              <ContainerNumber></ContainerNumber>
              <ContainerType>
                <Code></Code>
                <Category>
                  <Code></Code>
                  <Description></Description>
                </Category>
                <Description></Description>
                <ISOCode></ISOCode>
              </ContainerType>
              <IsEmptyContainer></IsEmptyContainer>
              <Seal></Seal>
              <SecondSeal></SecondSeal>
              <TotalHeight></TotalHeight>
              <TotalLength></TotalLength>
              <TotalWidth></TotalWidth>
              <AddInfoCollection>
                <AddInfo>
                  <Key></Key>
                  <Value></Value>
                </AddInfo>
              </AddInfoCollection>
            </Container>
          </ContainerCollection>
          <NoteCollection>
            <Note>
              <Description></Description>
              <IsCustomDescription></IsCustomDescription>
              <NoteText></NoteText>
            </Note>
          </NoteCollection>
          <OrganizationAddressCollection>
            <OrganizationAddress>
              <AddressType></AddressType>
              <AddressShortCode></AddressShortCode>
              <OrganizationCode></OrganizationCode>
              <Address1></Address1>
              <Address2></Address2>
              <AddressOverride></AddressOverride>
              <City></City>
              <CompanyName></CompanyName>
              <Country>
                <Code></Code>
                <Name></Name>
              </Country>
              <Email></Email>
              <Fax></Fax>
              <GovRegNum></GovRegNum>
              <GovRegNumType>
                <Code></Code>
                <Description></Description>
              </GovRegNumType>
              <Phone></Phone>
              <Port>
                <Code></Code>
                <Name></Name>
              </Port>
              <Postcode></Postcode>
              <ScreeningStatus>
                <Code></Code>
                <Description></Description>
              </ScreeningStatus>
              <State></State>
            </OrganizationAddress>
          </OrganizationAddressCollection>
          <PackingLineCollection>
            <PackingLine>
              <GoodsDescription></GoodsDescription>
              <HarmonisedCode></HarmonisedCode>
              <MarksAndNos></MarksAndNos>
              <PackQty></PackQty>
              <PackType>
                <Code></Code>
                <Description></Description>
              </PackType>
              <Volume></Volume>
              <VolumeUnit>
                <Code></Code>
                <Description></Description>
              </VolumeUnit>
              <Weight></Weight>
              <WeightUnit>
                <Code></Code>
                <Description></Description>
              </WeightUnit>
            </PackingLine>
          </PackingLineCollection>
        </SubShipment>
      </SubShipmentCollection>
    </Shipment>
</UniversalShipment>";
	}
}
