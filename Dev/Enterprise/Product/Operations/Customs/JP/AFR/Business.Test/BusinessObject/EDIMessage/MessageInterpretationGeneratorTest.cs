using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using ReceiveTransmitList = Enterprise.Messaging.Integration.ReceiveTransmitList.Codes;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class MessageInterpretationGeneratorTest : TestCaseWithFactory
	{
		#region Preperation

		JPAFRMessage TestTRXMessage
		{
			get
			{
				if (testTRXMessage == null)
				{
					testTRXMessage = Factory.New<JPAFRMessage>();
					testTRXMessage.EM_ReceiveTransmit = ReceiveTransmitList.Transmit;
					testTRXMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
				}
				return testTRXMessage;
			}
		}
		JPAFRMessage testTRXMessage;

		JPAFRMessage TestRCVMessage
		{
			get
			{
				if (testRCVMessage == null)
				{
					testRCVMessage = Factory.New<JPAFRMessage>();
					testRCVMessage.EM_ReceiveTransmit = ReceiveTransmitList.Receive;
					testRCVMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
				}
				return testRCVMessage;
			}
		}
		JPAFRMessage testRCVMessage;

		#region InputMessages
		#region TestXUSCompletionBody
		const string TestXUSCompletionBody = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>AFRHeader</Type>
          <Key>C00001079</Key>
        </DataSource>
      </DataSourceCollection>

      <ActionPurpose>
        <Code>AHR</Code>
        <Description>Advance Cargo Information Registration House</Description>
      </ActionPurpose>
      <TriggerCount>0</TriggerCount>
      <TriggerDescription></TriggerDescription>
      <TriggerType>Manual</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>AFR</Code>
          <Description>Japan Customs Advance Filing Rules</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <Branch>
      <Code>BNE</Code>
      <Name>BN - AUBNE</Name>
    </Branch>
    <PortOfDischarge>
      <Code>JPTKO</Code>
      <Name>Tokonami</Name>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>CNNKG</Code>
      <Name>Nanjing</Name>
    </PortOfLoading>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <VesselName>TESTVESSEL</VesselName>
    <VoyageFlightNo>VOYAGE</VoyageFlightNo>
    <WayBillNumber>SPQAVICTM001</WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>

    <AddInfoCollection>
      <AddInfo>
        <Key>JPVesselDetailsChanged</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPCarrierCode</Key>
        <Value>SPQA</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPPortOfLoadingSuffix</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>JPIsDepartureFromRelaxedArea</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPInternalTransactionNumber</Key>
        <Value>JP00000041</Value>
      </AddInfo>
    </AddInfoCollection>

    <DateCollection>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2014-01-08T13:33:00</Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2014-01-09T12:03:00</Value>
      </Date>
    </DateCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>Carrier</AddressType>
        <AddressShortCode>3 SHIP ST</AddressShortCode>
        <OrganizationCode>ANRSHIBAL</OrganizationCode>
        <Address1>3 SHIP ST</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>BALTIMORE</City>
        <CompanyName>ANRO SHIPPING LINE</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <Phone></Phone>
        <Port>
          <Code>USBAL</Code>
          <Name>Baltimore</Name>
        </Port>
        <Postcode>123456</Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State>MD</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <CountryOfIssue>
              <Code>AU</Code>
              <Name>Australia</Name>
            </CountryOfIssue>
            <Type>
              <Code>CAR</Code>
              <Description>Shipping Company Carrier/Principal </Description>
            </Type>
            <Value>car123</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <CountryOfIssue>
              <Code>JP</Code>
              <Name>Japan</Name>
            </CountryOfIssue>
            <Type>
              <Code>CCC</Code>
              <Description>Customs Carrier Code</Description>
            </Type>
            <Value>SPQA</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <SubShipmentCollection>
      <SubShipment>
        <DataContext>

          <ActionPurpose>
            <Code>REG</Code>
            <Description>Registration</Description>
          </ActionPurpose>
          <TriggerCount>0</TriggerCount>
          <TriggerDescription></TriggerDescription>
          <TriggerType>Manual</TriggerType>

          <DataTargetCollection>
            <DataTarget>
              <Type>AFRBill</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>


        <AddInfoCollection>
          <AddInfo>
            <Key>JPHouseBillRegisterCompletion</Key>
            <Value>Y</Value>
          </AddInfo>
        </AddInfoCollection>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>";
		#endregion
		#region TestXUSAHRBody
		const string TestXUSAHRBody = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>AFRHeader</Type>
          <Key>C00001079</Key>
        </DataSource>
      </DataSourceCollection>

      <ActionPurpose>
        <Code>AHR</Code>
        <Description>Advance Cargo Information Registration House</Description>
      </ActionPurpose>
      <TriggerCount>0</TriggerCount>
      <TriggerDescription></TriggerDescription>
      <TriggerType>Manual</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>AFR</Code>
          <Description>Japan Customs Advance Filing Rules</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <Branch>
      <Code>BNE</Code>
      <Name>BN - AUBNE</Name>
    </Branch>
    <PortOfDischarge>
      <Code>JPTKO</Code>
      <Name>Tokonami</Name>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>CNNKG</Code>
      <Name>Nanjing</Name>
    </PortOfLoading>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <VesselName>TESTVESSEL</VesselName>
    <VoyageFlightNo>VOYAGE</VoyageFlightNo>
    <WayBillNumber>SPQAVICTM001</WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>

    <AddInfoCollection>
      <AddInfo>
        <Key>JPVesselDetailsChanged</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPCarrierCode</Key>
        <Value>SPQA</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPPortOfLoadingSuffix</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>JPIsDepartureFromRelaxedArea</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPInternalTransactionNumber</Key>
        <Value>JP00000040</Value>
      </AddInfo>
    </AddInfoCollection>

    <DateCollection>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2014-01-08T13:33:00</Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2014-01-09T12:03:00</Value>
      </Date>
    </DateCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>Carrier</AddressType>
        <AddressShortCode>3 SHIP ST</AddressShortCode>
        <OrganizationCode>ANRSHIBAL</OrganizationCode>
        <Address1>3 SHIP ST</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>BALTIMORE</City>
        <CompanyName>ANRO SHIPPING LINE</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <Phone></Phone>
        <Port>
          <Code>USBAL</Code>
          <Name>Baltimore</Name>
        </Port>
        <Postcode>123456</Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State>MD</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <CountryOfIssue>
              <Code>AU</Code>
              <Name>Australia</Name>
            </CountryOfIssue>
            <Type>
              <Code>CAR</Code>
              <Description>Shipping Company Carrier/Principal </Description>
            </Type>
            <Value>car123</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <CountryOfIssue>
              <Code>JP</Code>
              <Name>Japan</Name>
            </CountryOfIssue>
            <Type>
              <Code>CCC</Code>
              <Description>Customs Carrier Code</Description>
            </Type>
            <Value>SPQA</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <SubShipmentCollection>
      <SubShipment>
        <DataContext>
          <DataSourceCollection>
            <DataSource>
              <Type>AFRBill</Type>
              <Key></Key>
            </DataSource>
          </DataSourceCollection>

          <ActionPurpose>
            <Code>REG</Code>
            <Description>Registration</Description>
          </ActionPurpose>
          <TriggerCount>0</TriggerCount>
          <TriggerDescription></TriggerDescription>
          <TriggerType>Manual</TriggerType>
        </DataContext>

        <CommercialInfo>
          <CommercialChargeCollection>
            <CommercialCharge>
              <ChargeType>
                <Code>OFT</Code>
                <Description>International Freight</Description>
              </ChargeType>
              <Amount>0.0000</Amount>
            </CommercialCharge>
          </CommercialChargeCollection>
        </CommercialInfo>
        <GoodsValue>0</GoodsValue>
        <PortOfDestination>
          <Code>JPYOK</Code>
          <Name>Yokohama</Name>
        </PortOfDestination>
        <PortOfOrigin>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </PortOfOrigin>
        <WayBillNumber>J07-SYYOSRE08018</WayBillNumber>
        <WayBillType>
          <Code>HWB</Code>
          <Description>House Waybill</Description>
        </WayBillType>

        <AddInfoCollection>
          <AddInfo>
            <Key>JPSPCCode</Key>
            <Value>PLQ</Value>
          </AddInfo>
          <AddInfo>
            <Key>JPPlaceOfDeliveryCode</Key>
            <Value>JPYOK</Value>
          </AddInfo>
          <AddInfo>
            <Key>JPPlaceOfDeliveryName</Key>
            <Value>Yokohama</Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentArrivalPlaceCode</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentArrivalPlaceName</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentEstimatedStartDate</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentEstimatedFinishDate</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentDuration</Key>
            <Value>0</Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentReasonCode</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentTransportMode</Key>
            <Value></Value>
          </AddInfo>
        </AddInfoCollection>

        <NoteCollection>
          <Note>
            <Description>Remarks</Description>
            <IsCustomDescription>true</IsCustomDescription>
            <NoteText></NoteText>
          </Note>
        </NoteCollection>

        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ConsigneeAddress</AddressType>
            <AddressShortCode>PST: ASPLEY HOMEBASE,</AddressShortCode>
            <OrganizationCode>BARGALBNE</OrganizationCode>
            <Address1>ASPLEY HOMEBASE,</Address1>
            <Address2>825 ZILLMERE ROAD</Address2>
            <AddressOverride>false</AddressOverride>
            <City>ASPLEY</City>
            <CompanyName>BARBEQUES GALORE</CompanyName>
            <Country>
              <Code>AU</Code>
              <Name>Australia</Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <Phone></Phone>
            <Port>
              <Code>AUBNE</Code>
              <Name>Brisbane</Name>
            </Port>
            <Postcode>4034</Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State></State>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <AddressShortCode>OFC: NO ADDRESS SPECIFIED</AddressShortCode>
            <OrganizationCode>UNMATCHED</OrganizationCode>
            <Address1>NO ADDRESS SPECIFIED</Address1>
            <Address2>PLEASE SEE ATTACHED NOTE</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NA</City>
            <CompanyName>UNMATCHED ORGANISATION</CompanyName>
            <Contact>PATRICK MCNAB</Contact>
            <Country>
              <Code>AU</Code>
              <Name>Australia</Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <Mobile></Mobile>
            <Phone>073500260</Phone>
            <Port>
              <Code>AUSYD</Code>
              <Name>Sydney</Name>
            </Port>
            <Postcode></Postcode>
            <ScreeningStatus>
              <Code>CLR</Code>
              <Description>Clear</Description>
            </ScreeningStatus>
            <State>NSW</State>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty</AddressType>
            <AddressShortCode>PST: ASPLEY HOMEBASE,</AddressShortCode>
            <OrganizationCode>BARGALBNE</OrganizationCode>
            <Address1>ASPLEY HOMEBASE,</Address1>
            <Address2>825 ZILLMERE ROAD</Address2>
            <AddressOverride>false</AddressOverride>
            <City>ASPLEY</City>
            <CompanyName>BARBEQUES GALORE</CompanyName>
            <Country>
              <Code>AU</Code>
              <Name>Australia</Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <Phone></Phone>
            <Port>
              <Code>AUBNE</Code>
              <Name>Brisbane</Name>
            </Port>
            <Postcode>4034</Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State></State>
          </OrganizationAddress>
        </OrganizationAddressCollection>

        <PackingLineCollection>
          <PackingLine>
            <GoodsDescription>HOUSEHOLD GOODS &amp; PERSONAL EFFECTS</GoodsDescription>
            <HarmonisedCode></HarmonisedCode>
            <MarksAndNos></MarksAndNos>
            <PackQty>1</PackQty>
            <PackType>
              <Code>PK</Code>
              <Description>Package</Description>
            </PackType>
            <Volume>0.400</Volume>
            <VolumeUnit>
              <Code>M3</Code>
              <Description>Cubic Meter</Description>
            </VolumeUnit>
            <Weight>6.718837</Weight>
            <WeightUnit>
              <Code>KG</Code>
              <Description>Kilogram</Description>
            </WeightUnit>

            <UNDGCollection>
              <UNDG>
                <UNDGCode>0020</UNDGCode>
                <FlashPoint>0.0</FlashPoint>
                <IMOClass>1.2K</IMOClass>
                <MarinePollutant>
                  <Code></Code>
                  <Description></Description>
                </MarinePollutant>
                <PackedInLimitedQuantity>false</PackedInLimitedQuantity>
                <PackingGroup></PackingGroup>
                <ProperShippingName>AMMUNITION, TOXIC</ProperShippingName>
                <TechicalName></TechicalName>
                <Volume>0.000</Volume>
                <VolumeUQ>
                  <Code></Code>
                </VolumeUQ>
                <Weight>0.000</Weight>
                <WeightUQ>
                  <Code></Code>
                </WeightUQ>
              </UNDG>
            </UNDGCollection>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
      <SubShipment>
        <DataContext>
          <DataSourceCollection>
            <DataSource>
              <Type>AFRBill</Type>
              <Key></Key>
            </DataSource>
          </DataSourceCollection>

          <ActionPurpose>
            <Code>DEL</Code>
            <Description>Delete</Description>
          </ActionPurpose>
          <TriggerCount>0</TriggerCount>
          <TriggerDescription></TriggerDescription>
          <TriggerType>Manual</TriggerType>
        </DataContext>

        <CommercialInfo>
          <CommercialChargeCollection>
            <CommercialCharge>
              <ChargeType>
                <Code>OFT</Code>
                <Description>International Freight</Description>
              </ChargeType>
              <Amount>0.0000</Amount>
            </CommercialCharge>
          </CommercialChargeCollection>
        </CommercialInfo>
        <GoodsValue>0</GoodsValue>
        <PortOfDestination>
          <Code>JPYOK</Code>
          <Name>Yokohama</Name>
        </PortOfDestination>
        <PortOfOrigin>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </PortOfOrigin>
        <WayBillNumber>J07-SYYOSRE08016</WayBillNumber>
        <WayBillType>
          <Code>HWB</Code>
          <Description>House Waybill</Description>
        </WayBillType>

        <AddInfoCollection>
          <AddInfo>
            <Key>JPSPCCode</Key>
            <Value>PLQ</Value>
          </AddInfo>
          <AddInfo>
            <Key>JPDeleteReasonCode</Key>
            <Value>5</Value>
          </AddInfo>
          <AddInfo>
            <Key>JPDeleteReasonText</Key>
            <Value>this is test</Value>
          </AddInfo>
          <AddInfo>
            <Key>JPPlaceOfDeliveryCode</Key>
            <Value>JPYOK</Value>
          </AddInfo>
          <AddInfo>
            <Key>JPPlaceOfDeliveryName</Key>
            <Value>Yokohama</Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentArrivalPlaceCode</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentArrivalPlaceName</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentEstimatedStartDate</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentEstimatedFinishDate</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentDuration</Key>
            <Value>0</Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentReasonCode</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentTransportMode</Key>
            <Value></Value>
          </AddInfo>
        </AddInfoCollection>

        <ContainerCollection>
          <Container>
            <ContainerNumber>TEST6543216</ContainerNumber>
            <ContainerType>
              <Code>20GP</Code>
              <Category>
                <Code>TNK</Code>
                <Description>Tank</Description>
              </Category>
              <Description>Twenty foot general purpose</Description>
              <ISOCode>22G0</ISOCode>
            </ContainerType>
            <IsEmptyContainer>false</IsEmptyContainer>
            <Seal>SEALA</Seal>
            <SecondSeal>SEALB</SecondSeal>
            <TotalHeight>8.500</TotalHeight>
            <TotalLength>10.000</TotalLength>
            <TotalWidth>8.000</TotalWidth>

            <AddInfoCollection>
              <AddInfo>
                <Key>JPContainerOwnershipCode</Key>
                <Value></Value>
              </AddInfo>
            </AddInfoCollection>
          </Container>
        </ContainerCollection>

        <NoteCollection>
          <Note>
            <Description>Remarks</Description>
            <IsCustomDescription>true</IsCustomDescription>
            <NoteText></NoteText>
          </Note>
        </NoteCollection>

        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ConsigneeAddress</AddressType>
            <AddressShortCode>OFC: NO ADDRESS SPECIFIED</AddressShortCode>
            <OrganizationCode>UNMATCHED</OrganizationCode>
            <Address1>NO ADDRESS SPECIFIED</Address1>
            <Address2>PLEASE SEE ATTACHED NOTE</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NA</City>
            <CompanyName>UNMATCHED ORGANISATION</CompanyName>
            <Contact>B PEELEN</Contact>
            <Country>
              <Code>AU</Code>
              <Name>Australia</Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <Mobile></Mobile>
            <Phone>JH</Phone>
            <Port>
              <Code>AUSYD</Code>
              <Name>Sydney</Name>
            </Port>
            <Postcode></Postcode>
            <ScreeningStatus>
              <Code>CLR</Code>
              <Description>Clear</Description>
            </ScreeningStatus>
            <State>NSW</State>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <AddressShortCode>OFC: NO ADDRESS SPECIFIED</AddressShortCode>
            <OrganizationCode>UNMATCHED</OrganizationCode>
            <Address1>NO ADDRESS SPECIFIED</Address1>
            <Address2>PLEASE SEE ATTACHED NOTE</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NA</City>
            <CompanyName>UNMATCHED ORGANISATION</CompanyName>
            <Contact>PATRICK MCNAB</Contact>
            <Country>
              <Code>AU</Code>
              <Name>Australia</Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <Mobile></Mobile>
            <Phone>073500260</Phone>
            <Port>
              <Code>AUSYD</Code>
              <Name>Sydney</Name>
            </Port>
            <Postcode></Postcode>
            <ScreeningStatus>
              <Code>CLR</Code>
              <Description>Clear</Description>
            </ScreeningStatus>
            <State>NSW</State>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty</AddressType>
            <AddressShortCode>OFC: NO ADDRESS SPECIFIED</AddressShortCode>
            <OrganizationCode>UNMATCHED</OrganizationCode>
            <Address1>NO ADDRESS SPECIFIED</Address1>
            <Address2>PLEASE SEE ATTACHED NOTE</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NA</City>
            <CompanyName>UNMATCHED ORGANISATION</CompanyName>
            <Country>
              <Code>AU</Code>
              <Name>Australia</Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <Phone></Phone>
            <Port>
              <Code>AUSYD</Code>
              <Name>Sydney</Name>
            </Port>
            <Postcode></Postcode>
            <ScreeningStatus>
              <Code>CLR</Code>
              <Description>Clear</Description>
            </ScreeningStatus>
            <State>NSW</State>
          </OrganizationAddress>
        </OrganizationAddressCollection>

        <PackingLineCollection>
          <PackingLine>
            <GoodsDescription>HOUSEHOLD GOODS &amp; PERSONAL EFFECTS</GoodsDescription>
            <HarmonisedCode></HarmonisedCode>
            <MarksAndNos></MarksAndNos>
            <PackQty>2</PackQty>
            <PackType>
              <Code>PK</Code>
              <Description>Package</Description>
            </PackType>
            <Volume>10.120</Volume>
            <VolumeUnit>
              <Code>M3</Code>
              <Description>Cubic Meter</Description>
            </VolumeUnit>
            <Weight>968.000</Weight>
            <WeightUnit>
              <Code>KG</Code>
              <Description>Kilogram</Description>
            </WeightUnit>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
      <SubShipment>
        <DataContext>
          <DataSourceCollection>
            <DataSource>
              <Type>AFRBill</Type>
              <Key></Key>
            </DataSource>
          </DataSourceCollection>

          <ActionPurpose>
            <Code>REG</Code>
            <Description>Registration</Description>
          </ActionPurpose>
          <TriggerCount>0</TriggerCount>
          <TriggerDescription></TriggerDescription>
          <TriggerType>Manual</TriggerType>
        </DataContext>

        <CommercialInfo>
          <CommercialChargeCollection>
            <CommercialCharge>
              <ChargeType>
                <Code>OFT</Code>
                <Description>International Freight</Description>
              </ChargeType>
              <Amount>0.0000</Amount>
            </CommercialCharge>
          </CommercialChargeCollection>
        </CommercialInfo>
        <GoodsValue>0</GoodsValue>
        <PortOfDestination>
          <Code>JPYOK</Code>
          <Name>Yokohama</Name>
        </PortOfDestination>
        <PortOfOrigin>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </PortOfOrigin>
        <WayBillNumber>J07-VICTH001001</WayBillNumber>
        <WayBillType>
          <Code>HWB</Code>
          <Description>House Waybill</Description>
        </WayBillType>

        <AddInfoCollection>
          <AddInfo>
            <Key>JPPlaceOfDeliveryCode</Key>
            <Value>JPYOK</Value>
          </AddInfo>
          <AddInfo>
            <Key>JPPlaceOfDeliveryName</Key>
            <Value>Yokohama</Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentArrivalPlaceCode</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentArrivalPlaceName</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentEstimatedStartDate</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentEstimatedFinishDate</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentDuration</Key>
            <Value>0</Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentReasonCode</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentTransportMode</Key>
            <Value></Value>
          </AddInfo>
        </AddInfoCollection>

        <ContainerCollection>
          <Container>
            <ContainerNumber>TEST6543221</ContainerNumber>
            <ContainerType>
              <Code>40RE</Code>
              <Category>
                <Code>RFG</Code>
                <Description>Refrigerated</Description>
              </Category>
              <Description>Forty foot reefer</Description>
              <ISOCode>42R0</ISOCode>
            </ContainerType>
            <IsEmptyContainer>false</IsEmptyContainer>
            <Seal></Seal>
            <SecondSeal></SecondSeal>
            <TotalHeight>8.500</TotalHeight>
            <TotalLength>40.000</TotalLength>
            <TotalWidth>8.000</TotalWidth>

            <AddInfoCollection>
              <AddInfo>
                <Key>JPContainerOwnershipCode</Key>
                <Value>1</Value>
              </AddInfo>
            </AddInfoCollection>
          </Container>
          <Container>
            <ContainerNumber>TEST6543216</ContainerNumber>
            <ContainerType>
              <Code>20GP</Code>
              <Category>
                <Code>TNK</Code>
                <Description>Tank</Description>
              </Category>
              <Description>Twenty foot general purpose</Description>
              <ISOCode>22G0</ISOCode>
            </ContainerType>
            <IsEmptyContainer>false</IsEmptyContainer>
            <Seal>SEALA</Seal>
            <SecondSeal>SEALB</SecondSeal>
            <TotalHeight>8.500</TotalHeight>
            <TotalLength>10.000</TotalLength>
            <TotalWidth>8.000</TotalWidth>

            <AddInfoCollection>
              <AddInfo>
                <Key>JPContainerOwnershipCode</Key>
                <Value></Value>
              </AddInfo>
            </AddInfoCollection>
          </Container>
        </ContainerCollection>

        <NoteCollection>
          <Note>
            <Description>Remarks</Description>
            <IsCustomDescription>true</IsCustomDescription>
            <NoteText></NoteText>
          </Note>
        </NoteCollection>

        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ConsigneeAddress</AddressType>
            <AddressShortCode>40BNE</AddressShortCode>
            <OrganizationCode>ABIGAS_WW</OrganizationCode>
            <Address1>171 ABBOTSFORD ROAD</Address1>
            <Address2>171 ABBOTSFORD ROAD</Address2>
            <AddressOverride>false</AddressOverride>
            <City>MAYNE</City>
            <CompanyName>ABI GAS &amp; TOOLS &amp; UNIONPOWER LOGISTICS (CHINA) LTD</CompanyName>
            <Country>
              <Code>CN</Code>
              <Name>China</Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <Phone>11115555</Phone>
            <Port>
              <Code>CNSHG</Code>
              <Name>Sanshan</Name>
            </Port>
            <Postcode>4006</Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State>11</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Type>
                  <Code>LSC</Code>
                  <Description>Legacy System Code</Description>
                </Type>
                <Value>LEG3</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Type>
                  <Code>SSN</Code>
                  <Description>Social Security Number</Description>
                </Type>
                <Value>624-56-7168</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <CountryOfIssue>
                  <Code>TW</Code>
                  <Name>TAIWAN</Name>
                </CountryOfIssue>
                <Type>
                  <Code>VAT</Code>
                  <Description>Government VAT Code</Description>
                </Type>
                <Value>86308990</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <AddressShortCode>PST: DIESLSTR 11</AddressShortCode>
            <OrganizationCode>ABABEUSHA</OrganizationCode>
            <Address1>DIESLSTR 11</Address1>
            <Address2>57439 ATTENDORN, GERMANY</Address2>
            <AddressOverride>false</AddressOverride>
            <City>57439 ATTENDORN, GERMANY</City>
            <CompanyName>ABA BEUL</CompanyName>
            <Country>
              <Code>CN</Code>
              <Name>China</Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <Phone></Phone>
            <Port>
              <Code>CNSHA</Code>
              <Name>Shanghai</Name>
            </Port>
            <Postcode>2222</Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State>12</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <CountryOfIssue>
                  <Code>AU</Code>
                  <Name>Australia</Name>
                </CountryOfIssue>
                <Type>
                  <Code>CID</Code>
                  <Description>CCID Customs Client Identifier</Description>
                </Type>
                <Value>33333</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Type>
                  <Code>LSC</Code>
                  <Description>Legacy System Code</Description>
                </Type>
                <Value>LEG2</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Type>
                  <Code>EIN</Code>
                  <Description>Employer Identification Number</Description>
                </Type>
                <Value>38-203957350</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty2</AddressType>
            <Address1>980 LYTTON ROAD</Address1>
            <Address2></Address2>
            <AddressOverride>true</AddressOverride>
            <City>MURARRIE</City>
            <CompanyName>SOMECOMPANY2</CompanyName>
            <Contact></Contact>
            <Country>
              <Code>AU</Code>
              <Name>Australia</Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <GovRegNum></GovRegNum>
            <GovRegNumType>
              <Code>DEF</Code>
              <Description>Default</Description>
            </GovRegNumType>
            <Mobile></Mobile>
            <Phone></Phone>
            <Postcode>4172</Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State>QLD</State>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty</AddressType>
            <AddressShortCode>OFC: NO ADDRESS SPECIFIED</AddressShortCode>
            <OrganizationCode>UNMATCHED</OrganizationCode>
            <Address1>NO ADDRESS SPECIFIED</Address1>
            <Address2>PLEASE SEE ATTACHED NOTE</Address2>
            <AddressOverride>false</AddressOverride>
            <City>NA</City>
            <CompanyName>UNMATCHED ORGANISATION</CompanyName>
            <Country>
              <Code>AU</Code>
              <Name>Australia</Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <Phone></Phone>
            <Port>
              <Code>AUSYD</Code>
              <Name>Sydney</Name>
            </Port>
            <Postcode></Postcode>
            <ScreeningStatus>
              <Code>CLR</Code>
              <Description>Clear</Description>
            </ScreeningStatus>
            <State>NSW</State>
          </OrganizationAddress>
        </OrganizationAddressCollection>

        <PackingLineCollection>
          <PackingLine>
            <GoodsDescription>HOUSEHOLD GOODS &amp; PERSONAL EFFECTS</GoodsDescription>
            <HarmonisedCode></HarmonisedCode>
            <MarksAndNos></MarksAndNos>
            <PackQty>1</PackQty>
            <PackType>
              <Code>PK</Code>
              <Description>Package</Description>
            </PackType>
            <Volume>0.800</Volume>
            <VolumeUnit>
              <Code>M3</Code>
              <Description>Cubic Meter</Description>
            </VolumeUnit>
            <Weight>250.000</Weight>
            <WeightUnit>
              <Code>KG</Code>
              <Description>Kilogram</Description>
            </WeightUnit>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>
";
		#endregion
		#region TestXUSNoSubShipmentBody
		internal const string TestXUSNoSubShipmentBody = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>AFRHeader</Type>
          <Key>C00001079</Key>
        </DataSource>
      </DataSourceCollection>

      <ActionPurpose>
        <Code>AHR</Code>
        <Description>Advance Cargo Information Registration House</Description>
      </ActionPurpose>
      <TriggerCount>0</TriggerCount>
      <TriggerDescription></TriggerDescription>
      <TriggerType>Manual</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>AFR</Code>
          <Description>Japan Customs Advance Filing Rules</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <Branch>
      <Code>BNE</Code>
      <Name>BN - AUBNE</Name>
    </Branch>
    <PortOfDischarge>
      <Code>JPTKO</Code>
      <Name>Tokonami</Name>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>CNNKG</Code>
      <Name>Nanjing</Name>
    </PortOfLoading>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <VesselName>TESTVESSEL</VesselName>
    <VoyageFlightNo>VOYAGE</VoyageFlightNo>
    <WayBillNumber>SPQAVICTM001</WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>

    <AddInfoCollection>
      <AddInfo>
        <Key>JPVesselDetailsChanged</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPCarrierCode</Key>
        <Value>SPQA</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPPortOfLoadingSuffix</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>JPIsDepartureFromRelaxedArea</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPInternalTransactionNumber</Key>
        <Value>JP00000040</Value>
      </AddInfo>
    </AddInfoCollection>

    <DateCollection>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2014-01-08T13:33:00</Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2014-01-09T12:03:00</Value>
      </Date>
    </DateCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>Carrier</AddressType>
        <AddressShortCode>3 SHIP ST</AddressShortCode>
        <OrganizationCode>ANRSHIBAL</OrganizationCode>
        <Address1>3 SHIP ST</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>BALTIMORE</City>
        <CompanyName>ANRO SHIPPING LINE</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <Phone></Phone>
        <Port>
          <Code>USBAL</Code>
          <Name>Baltimore</Name>
        </Port>
        <Postcode>123456</Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State>MD</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <CountryOfIssue>
              <Code>AU</Code>
              <Name>Australia</Name>
            </CountryOfIssue>
            <Type>
              <Code>CAR</Code>
              <Description>Shipping Company Carrier/Principal </Description>
            </Type>
            <Value>car123</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <CountryOfIssue>
              <Code>JP</Code>
              <Name>Japan</Name>
            </CountryOfIssue>
            <Type>
              <Code>CCC</Code>
              <Description>Customs Carrier Code</Description>
            </Type>
            <Value>SPQA</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>
    <SubShipmentCollection/>
  </Shipment>
</UniversalShipment>
";
		#endregion
		#region TestXUSAMRBody
		const string TestXUSAMRBody = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>AFRHeader</Type>
          <Key>AFR00000047</Key>
        </DataSource>
      </DataSourceCollection>

      <ActionPurpose>
        <Code>AMR</Code>
        <Description>Advance Cargo Information Registration Master</Description>
      </ActionPurpose>
      <TriggerCount>0</TriggerCount>
      <TriggerDescription></TriggerDescription>
      <TriggerType>Manual</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>AFR</Code>
          <Description>Japan Customs Advance Filing Rules</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <Branch>
      <Code>BNE</Code>
      <Name>BN - AUBNe</Name>
    </Branch>
    <LloydsIMO>9060297</LloydsIMO>
    <PortOfDischarge>
      <Code>JPABA</Code>
      <Name>Abashiri</Name>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>AUSYD</Code>
      <Name>Sydney</Name>
    </PortOfLoading>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <VesselCountryOfRegistration>
      <Code>CN</Code>
      <Name>China</Name>
    </VesselCountryOfRegistration>
    <VesselName>P&amp;O NEDLLOYD LAGOS</VesselName>
    <VoyageFlightNo>009N</VoyageFlightNo>
    <WayBillNumber></WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>

    <AddInfoCollection>
      <AddInfo>
        <Key>JPOperationalCarrierVoyageNo</Key>
        <Value>XXX</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPCarrierCode</Key>
        <Value>SPQA</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPPortOfLoadingSuffix</Key>
        <Value>1</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPPortOfDischargeSuffix</Key>
        <Value>2</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPIsDepartureFromRelaxedArea</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPVesselCallSign</Key>
        <Value>CALLME</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPInternalTransactionNumber</Key>
        <Value>JP00000105</Value>
      </AddInfo>
    </AddInfoCollection>

    <DateCollection>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2014-06-08T12:35:00</Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2014-06-13T12:35:00</Value>
      </Date>
    </DateCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>Carrier</AddressType>
        <AddressShortCode>3 SHIP ST</AddressShortCode>
        <OrganizationCode>ANRSHIBAL</OrganizationCode>
        <Address1>3 SHIP ST</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>BALTIMORE</City>
        <CompanyName>ANRO SHIPPING LINE</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <Phone></Phone>
        <Port>
          <Code>USBAL</Code>
          <Name>Baltimore</Name>
        </Port>
        <Postcode>123456</Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State>MD</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <CountryOfIssue>
              <Code>AU</Code>
              <Name>Australia</Name>
            </CountryOfIssue>
            <Type>
              <Code>CAR</Code>
              <Description>Shipping Company Carrier/Principal </Description>
            </Type>
            <Value>car123</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <CountryOfIssue>
              <Code>JP</Code>
              <Name>Japan</Name>
            </CountryOfIssue>
            <Type>
              <Code>CCC</Code>
              <Description>Customs Carrier Code</Description>
            </Type>
            <Value>SPQA</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <SubShipmentCollection>
      <SubShipment>
        <DataContext>
          <DataSourceCollection>
            <DataSource>
              <Type>AFRBill</Type>
              <Key></Key>
            </DataSource>
          </DataSourceCollection>

          <ActionPurpose>
            <Code>DEL</Code>
            <Description>Delete</Description>
          </ActionPurpose>
          <TriggerCount>0</TriggerCount>
          <TriggerDescription></TriggerDescription>
          <TriggerType>Manual</TriggerType>
        </DataContext>

        <CommercialInfo>
          <CommercialChargeCollection>
            <CommercialCharge>
              <ChargeType>
                <Code>OFT</Code>
                <Description>International Freight</Description>
              </ChargeType>
              <Amount>4.0000</Amount>
              <Currency>
                <Code>AUD</Code>
                <Description>Australian Dollar</Description>
              </Currency>
            </CommercialCharge>
          </CommercialChargeCollection>
        </CommercialInfo>
        <GoodsValue>0</GoodsValue>
        <PortOfDestination>
          <Code>JPTKA</Code>
          <Name>Tokai</Name>
        </PortOfDestination>
        <PortOfOrigin>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </PortOfOrigin>
        <WayBillNumber>J07JHB001</WayBillNumber>
        <WayBillType>
          <Code>HWB</Code>
          <Description>House Waybill</Description>
        </WayBillType>

        <AddInfoCollection>
          <AddInfo>
            <Key>JPSPCCode</Key>
            <Value>PLQ</Value>
          </AddInfo>
          <AddInfo>
            <Key>JPDeleteReasonCode</Key>
            <Value>5</Value>
          </AddInfo>
          <AddInfo>
            <Key>JPDeleteReasonText</Key>
            <Value>this is test</Value>
          </AddInfo>
          <AddInfo>
            <Key>JPNotificationForwardingPartyCode1</Key>
            <Value>TEST1</Value>
          </AddInfo>
          <AddInfo>
            <Key>JPOtherRelevantLawCode1</Key>
            <Value>AM</Value>
          </AddInfo>
          <AddInfo>
            <Key>JPPlaceOfDeliveryCode</Key>
            <Value>JPTKY</Value>
          </AddInfo>
          <AddInfo>
            <Key>JPPlaceOfDeliveryName</Key>
            <Value>Tokuyama</Value>
          </AddInfo>
          <AddInfo>
            <Key>JPMasterBillIdentifier</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPContainerOperatorCode</Key>
            <Value>TCOC1</Value>
          </AddInfo>
          <AddInfo>
            <Key>JPGeneralCustomsTransitApprovalNumber</Key>
            <Value>TESTGTN0012</Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentArrivalPlaceCode</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentArrivalPlaceName</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentEstimatedStartDate</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentEstimatedFinishDate</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentDuration</Key>
            <Value>0</Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentReasonCode</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentTransportMode</Key>
            <Value></Value>
          </AddInfo>
        </AddInfoCollection>

        <ContainerCollection Content=""Complete"">
          <Container>
            <ContainerNumber>SPQA1111118</ContainerNumber>
            <ContainerType>
              <Code>40GP</Code>
              <Category>
                <Code>DRY</Code>
                <Description>Dry Storage</Description>
              </Category>
              <Description>Forty foot general purpose</Description>
              <ISOCode>42G0</ISOCode>
            </ContainerType>
            <IsEmptyContainer>true</IsEmptyContainer>
            <Seal>SEAL1</Seal>
            <SecondSeal></SecondSeal>
            <TotalHeight>8.500</TotalHeight>
            <TotalLength>40.000</TotalLength>
            <TotalWidth>8.000</TotalWidth>

            <AddInfoCollection>
              <AddInfo>
                <Key>JPContainerOwnershipCode</Key>
                <Value>2</Value>
              </AddInfo>
              <AddInfo>
                <Key>JPContainerTypeOfService</Key>
                <Value>53</Value>
              </AddInfo>
              <AddInfo>
                <Key>JPContainerVanningType</Key>
                <Value>1</Value>
              </AddInfo>
              <AddInfo>
                <Key>JPContainerCCCApplicationId</Key>
                <Value>t</Value>
              </AddInfo>
              <AddInfo>
                <Key>JPContainerSearchExclusionId</Key>
                <Value>A</Value>
              </AddInfo>
            </AddInfoCollection>
          </Container>
        </ContainerCollection>

        <NoteCollection>
          <Note>
            <Description>Remarks</Description>
            <IsCustomDescription>true</IsCustomDescription>
            <NoteText>REMARKS</NoteText>
          </Note>
        </NoteCollection>

        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ConsigneeAddress</AddressType>
            <AddressShortCode>PST: ASPLEY HOMEBASE,</AddressShortCode>
            <OrganizationCode>BARGALBNE</OrganizationCode>
            <Address1>ASPLEY HOMEBASE,</Address1>
            <Address2>825 ZILLMERE ROAD</Address2>
            <AddressOverride>false</AddressOverride>
            <City>ASPLEY</City>
            <CompanyName>BARBEQUES GALORE</CompanyName>
            <Country>
              <Code>AU</Code>
              <Name>Australia</Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <Phone></Phone>
            <Port>
              <Code>AUBNE</Code>
              <Name>Brisbane</Name>
            </Port>
            <Postcode>4034</Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State></State>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <AddressShortCode>PST: DIESLSTR 11</AddressShortCode>
            <OrganizationCode>ABABEU</OrganizationCode>
            <Address1>DIESLSTR 12</Address1>
            <Address2>57439 ATTENDORN, GERMAN</Address2>
            <AddressOverride>false</AddressOverride>
            <City>57439 ATTENDORN, GERMANY</City>
            <CompanyName>ABA BEUL</CompanyName>
            <Country>
              <Code>DE</Code>
              <Name>Germany</Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <Phone></Phone>
            <Port>
              <Code>DEFRA</Code>
              <Name>Frankfurt am Main</Name>
            </Port>
            <Postcode>2222</Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State>BE</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <CountryOfIssue>
                  <Code>AU</Code>
                  <Name>Australia</Name>
                </CountryOfIssue>
                <Type>
                  <Code>CID</Code>
                  <Description>CCID Customs Client Identifier</Description>
                </Type>
                <Value>33333</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Type>
                  <Code>LSC</Code>
                  <Description>Legacy System Code</Description>
                </Type>
                <Value>LEG2</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Type>
                  <Code>EIN</Code>
                  <Description>Employer Identification Number</Description>
                </Type>
                <Value>38-203957350</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty</AddressType>
            <AddressShortCode>PST: 226 COMMONWEALTH STR</AddressShortCode>
            <OrganizationCode>CABINT</OrganizationCode>
            <Address1>226 COMMONWEALTH STREET</Address1>
            <Address2>SURRY HILLS  QLD</Address2>
            <AddressOverride>false</AddressOverride>
            <City>A</City>
            <CompanyName>CABLE INTERNATIONAL PTY LTD</CompanyName>
            <Country>
              <Code>AU</Code>
              <Name>Australia</Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <Phone></Phone>
            <Port>
              <Code>AUSYD</Code>
              <Name>Sydney</Name>
            </Port>
            <Postcode>2010</Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State></State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <CountryOfIssue>
                  <Code>JP</Code>
                  <Name>Japan</Name>
                </CountryOfIssue>
                <Type>
                  <Code>CCD</Code>
                  <Description>Customs Client Code</Description>
                </Type>
                <Value>CCD2</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>

        <PackingLineCollection>
          <PackingLine>
            <CountryOfOrigin>
              <Code>AU</Code>
              <Name>Australia</Name>
            </CountryOfOrigin>
            <GoodsDescription>GOODS</GoodsDescription>
            <HarmonisedCode>010101</HarmonisedCode>
            <MarksAndNos>MARKS</MarksAndNos>
            <PackQty>1</PackQty>
            <PackType>
              <Code>PP</Code>
              <Description>Pallet And Package</Description>
            </PackType>
            <Volume>3.000</Volume>
            <VolumeUnit>
              <Code>M3</Code>
              <Description>Cubic Meter</Description>
            </VolumeUnit>
            <Weight>2.000</Weight>
            <WeightUnit>
              <Code>KG</Code>
              <Description>Kilogram</Description>
            </WeightUnit>

            <UNDGCollection>
              <UNDG>
                <UNDGCode>0004a</UNDGCode>
                <FlashPoint>0.0</FlashPoint>
                <IMOClass>1.1D</IMOClass>
                <MarinePollutant>
                  <Code></Code>
                  <Description></Description>
                </MarinePollutant>
                <PackedInLimitedQuantity>false</PackedInLimitedQuantity>
                <PackingGroup></PackingGroup>
                <ProperShippingName>AMMONIUM PICRATE</ProperShippingName>
                <TechicalName></TechicalName>
                <Volume>0.000</Volume>
                <VolumeUQ>
                  <Code></Code>
                </VolumeUQ>
                <Weight>0.000</Weight>
                <WeightUQ>
                  <Code></Code>
                </WeightUQ>
              </UNDG>
            </UNDGCollection>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
      <SubShipment>
        <DataContext>
          <DataSourceCollection>
            <DataSource>
              <Type>AFRBill</Type>
              <Key></Key>
            </DataSource>
          </DataSourceCollection>

          <ActionPurpose>
            <Code>REG</Code>
            <Description>Registration</Description>
          </ActionPurpose>
          <TriggerCount>0</TriggerCount>
          <TriggerDescription></TriggerDescription>
          <TriggerType>Manual</TriggerType>
        </DataContext>

        <CommercialInfo>
          <CommercialChargeCollection>
            <CommercialCharge>
              <ChargeType>
                <Code>OFT</Code>
                <Description>International Freight</Description>
              </ChargeType>
              <Amount>4.0000</Amount>
              <Currency>
                <Code>AUD</Code>
                <Description>Australian Dollar</Description>
              </Currency>
            </CommercialCharge>
          </CommercialChargeCollection>
        </CommercialInfo>
        <GoodsValue>0</GoodsValue>
        <PortOfDestination>
          <Code>JPTKA</Code>
          <Name>Tokai</Name>
        </PortOfDestination>
        <PortOfOrigin>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </PortOfOrigin>
        <WayBillNumber>J07JHB002</WayBillNumber>
        <WayBillType>
          <Code>HWB</Code>
          <Description>House Waybill</Description>
        </WayBillType>

        <AddInfoCollection>
          <AddInfo>
            <Key>JPSPCCode</Key>
            <Value>PLQ</Value>
          </AddInfo>
          <AddInfo>
            <Key>JPPlaceOfDeliveryCode</Key>
            <Value>JPTKY</Value>
          </AddInfo>
          <AddInfo>
            <Key>JPPlaceOfDeliveryName</Key>
            <Value>Tokuyama</Value>
          </AddInfo>
          <AddInfo>
            <Key>JPMasterBillIdentifier</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPContainerOperatorCode</Key>
            <Value>RTTES</Value>
          </AddInfo>
          <AddInfo>
            <Key>JPGeneralCustomsTransitApprovalNumber</Key>
            <Value>WHATEVER</Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentArrivalPlaceCode</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentArrivalPlaceName</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentEstimatedStartDate</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentEstimatedFinishDate</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentDuration</Key>
            <Value>0</Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentReasonCode</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentTransportMode</Key>
            <Value></Value>
          </AddInfo>
        </AddInfoCollection>

        <ContainerCollection Content=""Complete"">
          <Container>
            <ContainerNumber>SPQA1111119</ContainerNumber>
            <ContainerType>
              <Code>40GP</Code>
              <Category>
                <Code>DRY</Code>
                <Description>Dry Storage</Description>
              </Category>
              <Description>Forty foot general purpose</Description>
              <ISOCode>42G0</ISOCode>
            </ContainerType>
            <IsEmptyContainer>true</IsEmptyContainer>
            <Seal>SEAL1</Seal>
            <SecondSeal></SecondSeal>
            <TotalHeight>8.500</TotalHeight>
            <TotalLength>40.000</TotalLength>
            <TotalWidth>8.000</TotalWidth>

            <AddInfoCollection>
              <AddInfo>
                <Key>JPContainerOwnershipCode</Key>
                <Value></Value>
              </AddInfo>
              <AddInfo>
                <Key>JPContainerTypeOfService</Key>
                <Value></Value>
              </AddInfo>
              <AddInfo>
                <Key>JPContainerVanningType</Key>
                <Value></Value>
              </AddInfo>
              <AddInfo>
                <Key>JPContainerCCCApplicationId</Key>
                <Value></Value>
              </AddInfo>
              <AddInfo>
                <Key>JPContainerSearchExclusionId</Key>
                <Value>A</Value>
              </AddInfo>
            </AddInfoCollection>
          </Container>
        </ContainerCollection>

        <NoteCollection>
          <Note>
            <Description>Remarks</Description>
            <IsCustomDescription>true</IsCustomDescription>
            <NoteText>REMARKS</NoteText>
          </Note>
        </NoteCollection>

        <OrganizationAddressCollection>
          <OrganizationAddress>
            <AddressType>ConsigneeAddress</AddressType>
            <AddressShortCode>PST: ASPLEY HOMEBASE,</AddressShortCode>
            <OrganizationCode>BARGALBNE</OrganizationCode>
            <Address1>ASPLEY HOMEBASE,</Address1>
            <Address2>825 ZILLMERE ROAD</Address2>
            <AddressOverride>false</AddressOverride>
            <City>ASPLEY</City>
            <CompanyName>BARBEQUES GALORE</CompanyName>
            <Country>
              <Code>AU</Code>
              <Name>Australia</Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <Phone></Phone>
            <Port>
              <Code>AUBNE</Code>
              <Name>Brisbane</Name>
            </Port>
            <Postcode>4034</Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State></State>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <AddressShortCode>PST: DIESLSTR 11</AddressShortCode>
            <OrganizationCode>ABABEU</OrganizationCode>
            <Address1>DIESLSTR 12</Address1>
            <Address2>57439 ATTENDORN, GERMAN</Address2>
            <AddressOverride>false</AddressOverride>
            <City>57439 ATTENDORN, GERMANY</City>
            <CompanyName>ABA BEUL</CompanyName>
            <Country>
              <Code>DE</Code>
              <Name>Germany</Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <Phone></Phone>
            <Port>
              <Code>DEFRA</Code>
              <Name>Frankfurt am Main</Name>
            </Port>
            <Postcode>2222</Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State>BE</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <CountryOfIssue>
                  <Code>AU</Code>
                  <Name>Australia</Name>
                </CountryOfIssue>
                <Type>
                  <Code>CID</Code>
                  <Description>CCID Customs Client Identifier</Description>
                </Type>
                <Value>33333</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Type>
                  <Code>LSC</Code>
                  <Description>Legacy System Code</Description>
                </Type>
                <Value>LEG2</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Type>
                  <Code>EIN</Code>
                  <Description>Employer Identification Number</Description>
                </Type>
                <Value>38-203957350</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty</AddressType>
            <AddressShortCode>PST: 226 COMMONWEALTH STR</AddressShortCode>
            <OrganizationCode>CABINT</OrganizationCode>
            <Address1>226 COMMONWEALTH STREET</Address1>
            <Address2>SURRY HILLS  QLD</Address2>
            <AddressOverride>false</AddressOverride>
            <City>A</City>
            <CompanyName>CABLE INTERNATIONAL PTY LTD</CompanyName>
            <Country>
              <Code>AU</Code>
              <Name>Australia</Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <Phone></Phone>
            <Port>
              <Code>AUSYD</Code>
              <Name>Sydney</Name>
            </Port>
            <Postcode>2010</Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State></State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <CountryOfIssue>
                  <Code>JP</Code>
                  <Name>Japan</Name>
                </CountryOfIssue>
                <Type>
                  <Code>CCD</Code>
                  <Description>Customs Client Code</Description>
                </Type>
                <Value>CCD2</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
        </OrganizationAddressCollection>

        <PackingLineCollection>
          <PackingLine>
            <CountryOfOrigin>
              <Code>AU</Code>
              <Name>Australia</Name>
            </CountryOfOrigin>
            <GoodsDescription>GOODS</GoodsDescription>
            <HarmonisedCode>010101</HarmonisedCode>
            <MarksAndNos>MARKS</MarksAndNos>
            <PackQty>1</PackQty>
            <PackType>
              <Code>PP</Code>
              <Description>Pallet And Package</Description>
            </PackType>
            <Volume>3.000</Volume>
            <VolumeUnit>
              <Code>M3</Code>
              <Description>Cubic Meter</Description>
            </VolumeUnit>
            <Weight>2.000</Weight>
            <WeightUnit>
              <Code>KG</Code>
              <Description>Kilogram</Description>
            </WeightUnit>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
      <SubShipment>
        <DataContext>
          <DataSourceCollection>
            <DataSource>
              <Type>AFRBill</Type>
              <Key></Key>
            </DataSource>
          </DataSourceCollection>

          <ActionPurpose>
            <Code>REG</Code>
            <Description>Registration</Description>
          </ActionPurpose>
          <TriggerCount>0</TriggerCount>
          <TriggerDescription></TriggerDescription>
          <TriggerType>Manual</TriggerType>
        </DataContext>

        <CommercialInfo>
          <CommercialChargeCollection>
            <CommercialCharge>
              <ChargeType>
                <Code>OFT</Code>
                <Description>International Freight</Description>
              </ChargeType>
              <Amount>0.0000</Amount>
            </CommercialCharge>
          </CommercialChargeCollection>
        </CommercialInfo>
        <GoodsValue>0</GoodsValue>
        <PortOfDestination>
          <Code></Code>
        </PortOfDestination>
        <PortOfOrigin>
          <Code></Code>
        </PortOfOrigin>
        <WayBillNumber>TESTSDF</WayBillNumber>
        <WayBillType>
          <Code>HWB</Code>
          <Description>House Waybill</Description>
        </WayBillType>

        <AddInfoCollection>
          <AddInfo>
            <Key>JPSPCCode</Key>
            <Value>PLQ</Value>
          </AddInfo>
          <AddInfo>
            <Key>JPPlaceOfDeliveryCode</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPPlaceOfDeliveryName</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPMasterBillIdentifier</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPContainerOperatorCode</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPGeneralCustomsTransitApprovalNumber</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentArrivalPlaceCode</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentArrivalPlaceName</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentEstimatedStartDate</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentEstimatedFinishDate</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentDuration</Key>
            <Value>0</Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentReasonCode</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentTransportMode</Key>
            <Value></Value>
          </AddInfo>
        </AddInfoCollection>

        <NoteCollection>
          <Note>
            <Description>Remarks</Description>
            <IsCustomDescription>true</IsCustomDescription>
            <NoteText></NoteText>
          </Note>
        </NoteCollection>

        <PackingLineCollection>
          <PackingLine>
            <GoodsDescription></GoodsDescription>
            <HarmonisedCode></HarmonisedCode>
            <MarksAndNos></MarksAndNos>
            <PackQty>0</PackQty>
            <PackType>
              <Code></Code>
            </PackType>
            <Volume>0.000</Volume>
            <VolumeUnit>
              <Code>M3</Code>
              <Description>Cubic Meter</Description>
            </VolumeUnit>
            <Weight>0.000</Weight>
            <WeightUnit>
              <Code>KG</Code>
              <Description>Kilogram</Description>
            </WeightUnit>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>";
		#endregion
		#region TestXUSATDBody
		const string TestXUSATDBody = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>AFRHeader</Type>
          <Key>AFR00000047</Key>
        </DataSource>
      </DataSourceCollection>

      <ActionPurpose>
        <Code>DTR</Code>
        <Description>Departure Time Registration</Description>
      </ActionPurpose>
      <TriggerCount>0</TriggerCount>
      <TriggerDescription></TriggerDescription>
      <TriggerType>Manual</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>AFR</Code>
          <Description>Japan Customs Advance Filing Rules</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <Branch>
      <Code>BNE</Code>
      <Name>BN - AUBNe</Name>
    </Branch>
    <LloydsIMO>9060297</LloydsIMO>
    <PortOfDischarge>
      <Code>JPABA</Code>
      <Name>Abashiri</Name>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>AUSYD</Code>
      <Name>Sydney</Name>
    </PortOfLoading>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <VesselCountryOfRegistration>
      <Code>CN</Code>
      <Name>China</Name>
    </VesselCountryOfRegistration>
    <VesselName>P&amp;O NEDLLOYD LAGOS</VesselName>
    <VoyageFlightNo>009N</VoyageFlightNo>
    <WayBillNumber></WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>

    <AddInfoCollection>
      <AddInfo>
        <Key>JPCarrierCode</Key>
        <Value>SPQA</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPPortOfLoadingSuffix</Key>
        <Value>1</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPPortOfDischargeSuffix</Key>
        <Value>2</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPIsDepartureFromRelaxedArea</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPVesselCallSign</Key>
        <Value>CALLME</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPInternalTransactionNumber</Key>
        <Value>JP00000106</Value>
      </AddInfo>
    </AddInfoCollection>

    <DateCollection>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2014-06-08T12:35:00</Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2014-06-13T12:35:00</Value>
      </Date>
    </DateCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>Carrier</AddressType>
        <AddressShortCode>3 SHIP ST</AddressShortCode>
        <OrganizationCode>ANRSHIBAL</OrganizationCode>
        <Address1>3 SHIP ST</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <City>BALTIMORE</City>
        <CompanyName>ANRO SHIPPING LINE</CompanyName>
        <Country>
          <Code>US</Code>
          <Name>United States</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <Phone></Phone>
        <Port>
          <Code>USBAL</Code>
          <Name>Baltimore</Name>
        </Port>
        <Postcode>123456</Postcode>
        <ScreeningStatus>
          <Code>CLR</Code>
          <Description>Clear</Description>
        </ScreeningStatus>
        <State>MD</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <CountryOfIssue>
              <Code>AU</Code>
              <Name>Australia</Name>
            </CountryOfIssue>
            <Type>
              <Code>CAR</Code>
              <Description>Shipping Company Carrier/Principal </Description>
            </Type>
            <Value>car123</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <CountryOfIssue>
              <Code>JP</Code>
              <Name>Japan</Name>
            </CountryOfIssue>
            <Type>
              <Code>CCC</Code>
              <Description>Customs Carrier Code</Description>
            </Type>
            <Value>SPQA</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <SubShipmentCollection>
      <SubShipment>
        <DataContext>

          <ActionPurpose>
            <Code>REG</Code>
            <Description>Registration</Description>
          </ActionPurpose>
          <TriggerCount>0</TriggerCount>
          <TriggerDescription></TriggerDescription>
          <TriggerType>Manual</TriggerType>

          <DataTargetCollection>
            <DataTarget>
              <Type>AFRBill</Type>
            </DataTarget>
          </DataTargetCollection>
        </DataContext>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>";
		#endregion
		#region TestXUEAHRBody
		internal const string TestXUEAHRBody = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11""><Event><DataContext><ActionPurpose><Code>AHR</Code><Description>AHR</Description></ActionPurpose><DataProvider>AFR</DataProvider><DataTargetCollection><DataTarget><Key>AFRHeader</Key><Type>AFRHeader</Type></DataTarget></DataTargetCollection></DataContext><EventTime>2014-01-21T04:01:00.0000000Z</EventTime><EventType>MSC</EventType><EventReference>AHR-REJECTED</EventReference><ContextCollection><Context><Type>InternalTransactionNumber</Type><Value>JP00000096</Value></Context><Context><Type>MBOLNumber</Type><Value>SPQBBOL09818349</Value></Context><Context><Type>HBOLNumber</Type><Value>J07J827382738</Value></Context><Context><Type>NotificationDetails</Type><Value>&lt;table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""&gt;&lt;tr&gt;&lt;td&gt;Master Bill Of Lading Number&lt;/td&gt;&lt;td&gt;SPQBBOL09818349&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;House Bill Of Lading Number&lt;/td&gt;&lt;td&gt;J07J827382738&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Process Result Code - 1&lt;/td&gt;&lt;td&gt;R0095&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Process Result Field - 1&lt;/td&gt;&lt;td&gt;Error occured on Field:""SDT - Estimated Start Date of Transportation""&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Process Result Description - 1&lt;/td&gt;&lt;td&gt;Arrival Place Code entry is required because Estimated Start Date of Transportation has been entered.&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Process Result Suggestion - 1&lt;/td&gt;&lt;td&gt;1. Enter Arrival Place Code when customs transit of temporary landing cargo is intended.&lt;BR/&gt;2. Delete Estimated Start Date of Transportation when customs transit of temporary landing cargo is not intended.&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Process Result Code - 2&lt;/td&gt;&lt;td&gt;R0095&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Process Result Field - 2&lt;/td&gt;&lt;td&gt;Error occured on Field:""ARR - Arrival Place Code""&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Process Result Description - 2&lt;/td&gt;&lt;td&gt;Arrival Place Code entry is required because Estimated Start Date of Transportation has been entered.&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Process Result Suggestion - 2&lt;/td&gt;&lt;td&gt;1. Enter Arrival Place Code when customs transit of temporary landing cargo is intended.&lt;BR/&gt;2. Delete Estimated Start Date of Transportation when customs transit of temporary landing cargo is not intended.&lt;/td&gt;&lt;/tr&gt;&lt;/table&gt;</Value></Context></ContextCollection></Event> </UniversalEvent>";
		#endregion
		#region TestXUERARBody
		const string TestXUERARBody = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11""><Event><DataContext><ActionPurpose><Code>RAR</Code><Description>SAS1110</Description></ActionPurpose><DataProvider>AFR</DataProvider><DataTargetCollection><DataTarget><Key>AFRHeader</Key><Type>AFRHeader</Type></DataTarget></DataTargetCollection></DataContext><EventTime>2014-01-22T04:38:00.0000000Z</EventTime><EventType>MSC</EventType><EventReference>HLD</EventReference><ContextCollection><Context><Type>InternalTransactionNumber</Type><Value></Value></Context><Context><Type>MBOLNumber</Type><Value>SPQBBOL09818349</Value></Context><Context><Type>HBOLNumber</Type><Value>J07J827382738</Value></Context><Context><Type>LloydsNumber</Type><Value>9044748</Value></Context><Context><Type>VesselName</Type><Value>AALSMEERGRACHT</Value></Context><Context><Type>VoyageNumber</Type><Value>098123</Value></Context><Context><Type>CarrierCode</Type><Value>SPQB</Value></Context><Context><Type>NotificationDetails</Type><Value>&lt;table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""&gt;&lt;tr&gt;&lt;td&gt;Master Bill of Lading Number&lt;/td&gt;&lt;td&gt;SPQBBOL09818349&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;House Bill of Lading Number&lt;/td&gt;&lt;td&gt;J07J827382738&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Prior Notification Code&lt;/td&gt;&lt;td&gt;HLD&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Prior Notification Subject&lt;/td&gt;&lt;td&gt;*TEST*REQUIRE CORRECTING AND/OR ADDING CARGO INFORMATION&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Jurisdictional Customs Office Code&lt;/td&gt;&lt;td&gt;1A&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Jurisdictional Customs Office Name&lt;/td&gt;&lt;td&gt;TOKYO&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Vessel Call Sign&lt;/td&gt;&lt;td&gt;9044748&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Vessel Name&lt;/td&gt;&lt;td&gt;AALSMEERGRACHT&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Voyage Number&lt;/td&gt;&lt;td&gt;098123&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Carrier Code&lt;/td&gt;&lt;td&gt;SPQB&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Date Time Of Advance Cargo Information Registration&lt;/td&gt;&lt;td&gt;2014-01-21T13:30:00.0000000&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Details Of Notifications Directions&lt;/td&gt;&lt;td&gt;THE FILER IS REQUIRED TO SUBMIT DETAILED DESCRIPTION OF GODS.&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Contact Name&lt;/td&gt;&lt;td&gt;XXXXXXXXXXXXXXXXXXXXXXXXX&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Contact Telephone Number&lt;/td&gt;&lt;td&gt;XXXXXXXXXX&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Contact Fax Number&lt;/td&gt;&lt;td&gt;XXXXXXXXXX&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Contact EMail Address&lt;/td&gt;&lt;td&gt;XXXXXXXXX@XXXXXXXXXXXXXXXX&lt;/td&gt;&lt;/tr&gt;&lt;/table&gt;</Value></Context></ContextCollection></Event></UniversalEvent>";
		#endregion
		#region TestXUERACBody
		const string TestXUERACBody = @"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11""><Event><DataContext><ActionPurpose><Code>RAC</Code><Description>SAS1120</Description></ActionPurpose><DataProvider>AFR</DataProvider><DataTargetCollection><DataTarget><Key>AFRHeader</Key><Type>AFRHeader</Type></DataTarget></DataTargetCollection></DataContext><EventTime>2014-01-22T08:00:00.0000000Z</EventTime><EventType>MSC</EventType><EventReference>HLD</EventReference><ContextCollection><Context><Type>InternalTransactionNumber</Type><Value></Value></Context><Context><Type>MBOLNumber</Type><Value>SPQBBOL09818349</Value></Context><Context><Type>HBOLNumber</Type><Value>J07J827382738</Value></Context><Context><Type>LloydsNumber</Type><Value>9044748</Value></Context><Context><Type>VesselName</Type><Value>AALSMEERGRACHT</Value></Context><Context><Type>VoyageNumber</Type><Value>098123</Value></Context><Context><Type>CarrierCode</Type><Value>SPQB</Value></Context><Context><Type>NotificationDetails</Type><Value>&lt;table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""&gt;&lt;tr&gt;&lt;td&gt;Master Bill of Lading Number&lt;/td&gt;&lt;td&gt;SPQBBOL09818349&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;House Bill of Lading Number&lt;/td&gt;&lt;td&gt;J07J827382738&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Prior Notification Code&lt;/td&gt;&lt;td&gt;HLD&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Prior Notification Subject&lt;/td&gt;&lt;td&gt;*TEST*CANCELLATION OF HLD&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Jurisdictional Customs Office Code&lt;/td&gt;&lt;td&gt;1A&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Jurisdictional Customs Office Name&lt;/td&gt;&lt;td&gt;TOKYO&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Vessel Call Sign&lt;/td&gt;&lt;td&gt;9044748&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Vessel Name&lt;/td&gt;&lt;td&gt;AALSMEERGRACHT&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Voyage Number&lt;/td&gt;&lt;td&gt;098123&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Carrier Code&lt;/td&gt;&lt;td&gt;SPQB&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Date Time Of Advance Cargo Information Registration&lt;/td&gt;&lt;td&gt;2014-01-21T13:30:00.0000000&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Contact Name&lt;/td&gt;&lt;td&gt;XXXXXXXXXXXXXXXXXXXXXXXXX&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Contact Telephone Number&lt;/td&gt;&lt;td&gt;XXXXXXXXXX&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Contact Fax Number&lt;/td&gt;&lt;td&gt;XXXXXXXXXX&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Contact EMail Address&lt;/td&gt;&lt;td&gt;XXXXXXXXX@XXXXXXXXXXXXXXXX&lt;/td&gt;&lt;/tr&gt;&lt;/table&gt;</Value></Context></ContextCollection></Event></UniversalEvent>";
		#endregion
		#region TestCMVBody
		const string TestCMVBody = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>AFRHeader</Type>
          <Key>AFR00000036</Key>
        </DataSource>
      </DataSourceCollection>

      <ActionPurpose>
        <Code>CMV</Code>
        <Description>Blanket Vessel Change</Description>
      </ActionPurpose>
      <TriggerCount>0</TriggerCount>
      <TriggerDescription></TriggerDescription>
      <TriggerType>Manual</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>AFR</Code>
          <Description>Japan Customs Advance Filing Rules</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <Branch>
      <Code>TYO</Code>
      <Name>Cargowise EDI - Tokyo</Name>
    </Branch>
    <LloydsIMO></LloydsIMO>
    <PortOfDischarge>
      <Code></Code>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>ADALV</Code>
      <Name>Andorra la Vella</Name>
    </PortOfLoading>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <VesselCountryOfRegistration>
      <Code>AD</Code>
      <Name>Andorra</Name>
    </VesselCountryOfRegistration>
    <VesselName>A P MOLLER</VesselName>
    <VoyageFlightNo>12345678</VoyageFlightNo>
    <WayBillNumber>NVOCC170424</WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>

    <AddInfoCollection>
      <AddInfo>
        <Key>JPCarrierCode</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>JPPortOfLoadingSuffix</Key>
        <Value>1</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPIsDepartureFromRelaxedArea</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPVesselCallSign</Key>
        <Value>OVYQ2</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPVesselDetailsChanged</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPCarrierCodeNew</Key>
        <Value>1222</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPVesselNameNew</Key>
        <Value>A P MOLLER</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPVesselCallSignNew</Key>
        <Value>OVYQ2</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPVesselCountryNew</Key>
        <Value>AD</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPVoyageNumberNew</Key>
        <Value>12345678</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPPortOfLoadingSuffixNew</Key>
        <Value>1</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPPortOfLoadingCodeNew</Key>
        <Value>ADALV</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPPortOfLoadingNameNew</Key>
        <Value>Andorra la Vella</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPIsDepartureFromRelaxedAreaNew</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPPortOfDischargeCodeNew</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>JPPortOfDischargeNameNew</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>JPEstimatedDateTimeOfDepartureNew</Key>
        <Value>2017-05-03T10:34:00</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPEstimatedDateTimeOfArrivalNew</Key>
        <Value>2017-05-03T10:35:00</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPBlanketChange</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPInternalTransactionNumber</Key>
        <Value>JP00000200</Value>
      </AddInfo>
    </AddInfoCollection>

    <DateCollection>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
    </DateCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>Carrier</AddressType>
        <Address1>COLLINS STREET</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>COLLINS STREET</AddressShortCode>
        <City>MELBOURNE</City>
        <CompanyName>ANL CONTAINER LINE PTY LTD</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <OrganizationCode>ANLCON_WW</OrganizationCode>
        <Phone></Phone>
        <Port>
          <Code>AUSYD</Code>
          <Name>Sydney</Name>
        </Port>
        <Postcode>3000</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>NSW</State>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <SubShipmentCollection>
      <SubShipment>
        <DataContext>
          <DataSourceCollection>
            <DataSource>
              <Type>AFRBill</Type>
              <Key></Key>
            </DataSource>
          </DataSourceCollection>

          <ActionPurpose>
            <Code>CMV</Code>
            <Description>Blanket Vessel Change</Description>
          </ActionPurpose>
          <TriggerCount>0</TriggerCount>
          <TriggerDescription></TriggerDescription>
          <TriggerType>Manual</TriggerType>
        </DataContext>

        <CommercialInfo>
          <CommercialChargeCollection>
            <CommercialCharge>
              <ChargeType>
                <Code>OFT</Code>
                <Description>International Freight</Description>
              </ChargeType>
              <Amount>0.0000</Amount>
            </CommercialCharge>
          </CommercialChargeCollection>
        </CommercialInfo>
        <GoodsValue>0</GoodsValue>
        <PortOfDestination>
          <Code></Code>
        </PortOfDestination>
        <PortOfOrigin>
          <Code></Code>
        </PortOfOrigin>
        <WayBillNumber>NVOCC170424</WayBillNumber>
        <WayBillType>
          <Code>HWB</Code>
          <Description>House Waybill</Description>
        </WayBillType>

        <AddInfoCollection>
          <AddInfo>
            <Key>JPPlaceOfDeliveryCode</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPPlaceOfDeliveryName</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentArrivalPlaceCode</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentArrivalPlaceName</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentEstimatedStartDate</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentEstimatedFinishDate</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentDuration</Key>
            <Value>0</Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentReasonCode</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentTransportMode</Key>
            <Value></Value>
          </AddInfo>
        </AddInfoCollection>

        <NoteCollection>
          <Note>
            <Description>Remarks</Description>
            <IsCustomDescription>true</IsCustomDescription>
            <NoteText></NoteText>
          </Note>
        </NoteCollection>

        <PackingLineCollection Content=""Complete"">
          <PackingLine>
            <GoodsDescription></GoodsDescription>
            <HarmonisedCode></HarmonisedCode>
            <MarksAndNos></MarksAndNos>
            <PackQty>0</PackQty>
            <PackType>
              <Code></Code>
            </PackType>
            <Volume>0.000</Volume>
            <VolumeUnit>
              <Code>M3</Code>
              <Description>Cubic Meter</Description>
            </VolumeUnit>
            <Weight>0.000</Weight>
            <WeightUnit>
              <Code>KG</Code>
              <Description>Kilogram</Description>
            </WeightUnit>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
      <SubShipment>
        <DataContext>
          <DataSourceCollection>
            <DataSource>
              <Type>AFRBill</Type>
              <Key></Key>
            </DataSource>
          </DataSourceCollection>

          <ActionPurpose>
            <Code>CMV</Code>
            <Description>Blanket Vessel Change</Description>
          </ActionPurpose>
          <TriggerCount>0</TriggerCount>
          <TriggerDescription></TriggerDescription>
          <TriggerType>Manual</TriggerType>
        </DataContext>

        <CommercialInfo>
          <CommercialChargeCollection>
            <CommercialCharge>
              <ChargeType>
                <Code>OFT</Code>
                <Description>International Freight</Description>
              </ChargeType>
              <Amount>0.0000</Amount>
            </CommercialCharge>
          </CommercialChargeCollection>
        </CommercialInfo>
        <GoodsValue>0</GoodsValue>
        <PortOfDestination>
          <Code></Code>
        </PortOfDestination>
        <PortOfOrigin>
          <Code></Code>
        </PortOfOrigin>
        <WayBillNumber>NVOCC170425</WayBillNumber>
        <WayBillType>
          <Code>HWB</Code>
          <Description>House Waybill</Description>
        </WayBillType>

        <AddInfoCollection>
          <AddInfo>
            <Key>JPPlaceOfDeliveryCode</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPPlaceOfDeliveryName</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentArrivalPlaceCode</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentArrivalPlaceName</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentEstimatedStartDate</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentEstimatedFinishDate</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentDuration</Key>
            <Value>0</Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentReasonCode</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPTranshipmentTransportMode</Key>
            <Value></Value>
          </AddInfo>
        </AddInfoCollection>

        <NoteCollection>
          <Note>
            <Description>Remarks</Description>
            <IsCustomDescription>true</IsCustomDescription>
            <NoteText></NoteText>
          </Note>
        </NoteCollection>

        <PackingLineCollection Content=""Complete"">
          <PackingLine>
            <GoodsDescription></GoodsDescription>
            <HarmonisedCode></HarmonisedCode>
            <MarksAndNos></MarksAndNos>
            <PackQty>0</PackQty>
            <PackType>
              <Code></Code>
            </PackType>
            <Volume>0.000</Volume>
            <VolumeUnit>
              <Code>M3</Code>
              <Description>Cubic Meter</Description>
            </VolumeUnit>
            <Weight>0.000</Weight>
            <WeightUnit>
              <Code>KG</Code>
              <Description>Kilogram</Description>
            </WeightUnit>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>
";
		#endregion
		#region TestBLLBody
		const string TestBLLBody = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <ActionPurpose>
        <Code>BLL</Code>
        <Description>Register BLL</Description>
      </ActionPurpose>
      <TriggerCount>0</TriggerCount>
      <TriggerDescription></TriggerDescription>
      <TriggerType>Manual</TriggerType>
      <DataTargetCollection>
        <DataTarget>
          <Type>AFRBill</Type>
          <Key>VICJEFFABC123-6</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <WayBillNumber>VICJEFFABC123-6</WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>
    <AddInfoCollection>
      <AddInfo>
        <Key>JPBLLFunctionCode</Key>
        <Value>3</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPBLLChangeReasonCode</Key>
        <Value>3</Value>
      </AddInfo>
    </AddInfoCollection>
    <AddInfoGroupCollection>
      <AddInfoGroup>
        <Type>
          <Code>Org</Code>
          <Description>Original Bill Numbers</Description>
        </Type>
        <AddInfoCollection>
          <AddInfo>
            <Key>BillNumber-1</Key>
            <Value>VICJEFFABC123-4</Value>
          </AddInfo>
          <AddInfo>
            <Key>BillNumber-2</Key>
            <Value>VICJEFFABC123-5</Value>
          </AddInfo>
          <AddInfo>
            <Key>BillNumber-3</Key>
            <Value>VICJEFFABC123-6</Value>
          </AddInfo>
          <AddInfo>
            <Key>BillNumber-4</Key>
            <Value>VICJEFFABC123-7</Value>
          </AddInfo>
        </AddInfoCollection>
      </AddInfoGroup>
      <AddInfoGroup>
        <Type>
          <Code>New</Code>
          <Description>New Bill Numbers</Description>
        </Type>
        <AddInfoCollection>
          <AddInfo>
            <Key>BillNumber</Key>
            <Value>VICJEFFABC123-6</Value>
          </AddInfo>
        </AddInfoCollection>
      </AddInfoGroup>
    </AddInfoGroupCollection>
  </Shipment>
</UniversalShipment>";
		#endregion
		#region TestBLCBody
		const string TestBLCBody = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <ActionPurpose>
        <Code>BLC</Code>
        <Description>Cancel BLL</Description>
      </ActionPurpose>
      <TriggerCount>0</TriggerCount>
      <TriggerDescription></TriggerDescription>
      <TriggerType>Manual</TriggerType>
      <DataTargetCollection>
        <DataTarget>
          <Type>AFRBill</Type>
          <Key>VICJEFFABC123-8</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <WayBillNumber>VICJEFFABC123-8</WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>
    <AddInfoCollection>
      <AddInfo>
        <Key>JPBLLFunctionCode</Key>
        <Value>4</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPBLLChangeReasonCode</Key>
        <Value>2</Value>
      </AddInfo>
    </AddInfoCollection>
    <AddInfoGroupCollection>
      <AddInfoGroup>
        <Type>
          <Code>Org</Code>
          <Description>Original Bill Numbers</Description>
        </Type>
        <AddInfoCollection>
          <AddInfo>
            <Key>BillNumber</Key>
            <Value>VICJEFFABC123-8</Value>
          </AddInfo>
        </AddInfoCollection>
      </AddInfoGroup>
      <AddInfoGroup>
        <Type>
          <Code>New</Code>
          <Description>New Bill Numbers</Description>
        </Type>
        <AddInfoCollection>
          <AddInfo>
            <Key>BillNumber-1</Key>
            <Value>VICJEFFABC123-12</Value>
          </AddInfo>
          <AddInfo>
            <Key>BillNumber-2</Key>
            <Value>VICJEFFABC123-13</Value>
          </AddInfo>
          <AddInfo>
            <Key>BillNumber-3</Key>
            <Value>VICJEFFABC123-3</Value>
          </AddInfo>
        </AddInfoCollection>
      </AddInfoGroup>
    </AddInfoGroupCollection>
  </Shipment>
</UniversalShipment>";
		#endregion
		#region TestBLLBodyWithoutFunctionCode
		const string TestBLLBodyWithoutFunctionCode = @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <ActionPurpose>
        <Code>BLL</Code>
        <Description>Register BLL</Description>
      </ActionPurpose>
      <TriggerCount>0</TriggerCount>
      <TriggerDescription></TriggerDescription>
      <TriggerType>Manual</TriggerType>
      <DataTargetCollection>
        <DataTarget>
          <Type>AFRBill</Type>
          <Key>VICJEFFABC123-6</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <WayBillNumber>VICJEFFABC123-6</WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>
    <AddInfoCollection>
      <AddInfo>
        <Key>JPBLLFunctionCode</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>JPBLLChangeReasonCode</Key>
        <Value>3</Value>
      </AddInfo>
    </AddInfoCollection>
    <AddInfoGroupCollection>
      <AddInfoGroup>
        <Type>
          <Code>Org</Code>
          <Description>Original Bill Numbers</Description>
        </Type>
        <AddInfoCollection>
          <AddInfo>
            <Key>BillNumber-1</Key>
            <Value>VICJEFFABC123-4</Value>
          </AddInfo>
          <AddInfo>
            <Key>BillNumber-2</Key>
            <Value>VICJEFFABC123-5</Value>
          </AddInfo>
          <AddInfo>
            <Key>BillNumber-3</Key>
            <Value>VICJEFFABC123-6</Value>
          </AddInfo>
          <AddInfo>
            <Key>BillNumber-4</Key>
            <Value>VICJEFFABC123-7</Value>
          </AddInfo>
        </AddInfoCollection>
      </AddInfoGroup>
      <AddInfoGroup>
        <Type>
          <Code>New</Code>
          <Description>New Bill Numbers</Description>
        </Type>
        <AddInfoCollection>
          <AddInfo>
            <Key>BillNumber</Key>
            <Value>VICJEFFABC123-6</Value>
          </AddInfo>
        </AddInfoCollection>
      </AddInfoGroup>
    </AddInfoGroupCollection>
  </Shipment>
</UniversalShipment>";
		#endregion

		#endregion

		#region ExpectedOutputs
		const string DefaultTableStart = "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><tr><td colspan=\"2\">";
		const string DefaultTableEnd = "</td></tr></table>";
		const string ExpectedContainers1 = "No Container was included in this bill";
		const string ExpectedContainers2 = "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\"><thead><tr class=\"tableheadings\"><th>Manifested Container Number</th><th>Seal 1</th><th>Seal2</th></tr></thead><tr><td width=\"150px\">TEST6543216</td><td>SEALA</td><td>SEALB</td></tr></table>";
		const string ExpectedContainers3 = "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\"><thead><tr class=\"tableheadings\"><th>Manifested Container Number</th><th>Seal 1</th><th>Seal2</th></tr></thead><tr><td width=\"150px\">TEST6543221</td><td>&nbsp;</td><td>&nbsp;</td></tr><tr><td width=\"150px\">TEST6543216</td><td>SEALA</td><td>SEALB</td></tr></table>";
		const string ExpectedSubshipment1 = "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\"><tr><th colspan=\"2\">AFR Bill Information</th></tr><tr><td>Action Type</td><td>REG - Registration</td></tr><tr><td>Special Cargo Code</td><td>PLQ</td></tr><tr><td>House Waybill</td><td>J07-SYYOSRE08018</td></tr><tr><td colspan=\"2\">" + ExpectedContainers1 + "</td></tr></table>";
		const string ExpectedSubshipment2 = "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\"><tr><th colspan=\"2\">AFR Bill Information</th></tr><tr><td>Action Type</td><td>DEL - Delete</td></tr><tr><td>Delete Reason Code</td><td>5</td></tr><tr><td>Delete Reason Text</td><td>this is test</td></tr><tr><td>Special Cargo Code</td><td>PLQ</td></tr><tr><td>House Waybill</td><td>J07-SYYOSRE08016</td></tr><tr><td colspan=\"2\"><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\"><thead><tr class=\"tableheadings\"><th>Manifested Container Number</th><th>Seal 1</th><th>Seal2</th></tr></thead><tr><td width=\"150px\">TEST6543216</td><td>SEALA</td><td>SEALB</td></tr></table></td></tr></table>";
		const string ExpectedSubshipment3 = "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\"><tr><th colspan=\"2\">AFR Bill Information</th></tr><tr><td>Action Type</td><td>REG - Registration</td></tr><tr><td>House Waybill</td><td>J07-VICTH001001</td></tr><tr><td colspan=\"2\">" + ExpectedContainers3 + "</td></tr></table>";
		internal const string ExpectedMastershipmentHeader = "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><th colspan=\"2\">AFR Header Information</th></tr><tr><td>Master Waybill</td><td>SPQAVICTM001</td></tr><tr><td width=\"150px\">Port of Loading</td><td>CNNKG</td></tr><tr><td width=\"150px\">Port of Discharge</td><td>JPTKO</td></tr><tr><td width=\"150px\">Carrier Code</td><td>SPQA</td></tr><tr><td width=\"150px\">Vessel Name</td><td>TESTVESSEL</td></tr><tr><td width=\"150px\">Vessel Call Sign</td><td>&nbsp;</td></tr><tr><td width=\"150px\">Voyage Number</td><td>VOYAGE</td></tr><tr><td width=\"150px\">Vessel Details Changed</td><td>Y</td></tr>";
		internal const string ExpectedMastershipmentFooter = "</table><br/>For more detailed information, please refer to the 'Message Text' tab.<br/>";
		const string ExpectedMasterDepartureTimeHeader = "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><th colspan=\"2\">Departure Time Information</th></tr><tr><td width=\"150px\">Port of Loading</td><td>AUSYD</td></tr><tr><td width=\"150px\">Port of Discharge</td><td>JPABA</td></tr><tr><td width=\"150px\">Carrier Code</td><td>SPQA</td></tr><tr><td width=\"150px\">Vessel Name</td><td>P&O NEDLLOYD LAGOS</td></tr><tr><td width=\"150px\">Vessel Call Sign</td><td>CALLME</td></tr><tr><td width=\"150px\">Voyage Number</td><td>009N</td></tr><tr><td width=\"150px\">Departure Time</td><td>08-Jun-14 12:35:00</td></tr>";
		const string ExpectedMasterAMRHeader = "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><th colspan=\"2\">AFR Header Information</th></tr><tr><td width=\"150px\">Port of Loading</td><td>AUSYD</td></tr><tr><td width=\"150px\">Port of Discharge</td><td>JPABA</td></tr><tr><td width=\"150px\">Carrier Code</td><td>SPQA</td></tr><tr><td width=\"150px\">Vessel Name</td><td>P&O NEDLLOYD LAGOS</td></tr><tr><td width=\"150px\">Vessel Call Sign</td><td>CALLME</td></tr><tr><td width=\"150px\">Voyage Number</td><td>009N</td></tr><tr><td width=\"150px\">Operational Carrier Voyage No</td><td>XXX</td></tr><tr><td width=\"150px\">Included Bill</td><td><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\"><tr><th colspan=\"2\">AFR Bill Information</th></tr><tr><td>Action Type</td><td>DEL - Delete</td></tr><tr><td>Delete Reason Code</td><td>5</td></tr><tr><td>Delete Reason Text</td><td>this is test</td></tr><tr><td>Special Cargo Code</td><td>PLQ</td></tr><tr><td>House Waybill</td><td>J07JHB001</td></tr><tr><td colspan=\"2\"><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\"><thead><tr class=\"tableheadings\"><th>Manifested Container Number</th><th>Seal 1</th><th>Seal2</th></tr></thead><tr><td width=\"150px\">SPQA1111118</td><td>SEAL1</td><td>&nbsp;</td></tr></table></td></tr></table></td></tr><tr><td width=\"150px\">Included Bill</td><td><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\"><tr><th colspan=\"2\">AFR Bill Information</th></tr><tr><td>Action Type</td><td>REG - Registration</td></tr><tr><td>Special Cargo Code</td><td>PLQ</td></tr><tr><td>House Waybill</td><td>J07JHB002</td></tr><tr><td colspan=\"2\"><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\"><thead><tr class=\"tableheadings\"><th>Manifested Container Number</th><th>Seal 1</th><th>Seal2</th></tr></thead><tr><td width=\"150px\">SPQA1111119</td><td>SEAL1</td><td>&nbsp;</td></tr></table></td></tr></table></td></tr><tr><td width=\"150px\">Included Bill</td><td><table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\"><tr><th colspan=\"2\">AFR Bill Information</th></tr><tr><td>Action Type</td><td>REG - Registration</td></tr><tr><td>Special Cargo Code</td><td>PLQ</td></tr><tr><td>House Waybill</td><td>TESTSDF</td></tr><tr><td colspan=\"2\">No Container was included in this bill</td></tr></table></td></tr>";
		const string ExpectedSubshipmentWrapperHeader = "<tr><td width=\"150px\">Included Bill</td><td>";
		const string ExpectedSubshipmentWrapperFooter = "</td></tr>";
		const string ExpectedCompletionMessage = "<tr><td width=\"150px\">Register Completion</td><td><b>The Master Bill has been marked as Registration Completed through 'Registration' message.</b></td></tr>";
		internal const string ExpectedXUEOutput1 = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><th>Response Details</th></tr><tr><td>A response message has been received from Customs.<br />The message type is : 'AHR - Advance Cargo Information Registration House'.<br/>The response indicates your House Bill Registration has been REJECTED<br/><br/>Shown below is a summary of relevant information received in the message.<br /></td></tr><tr><td><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Master Bill Of Lading Number</td><td>SPQBBOL09818349</td></tr><tr><td>House Bill Of Lading Number</td><td>J07J827382738</td></tr><tr><td>Process Result Code - 1</td><td>R0095</td></tr><tr><td>Process Result Field - 1</td><td>Error occured on Field:""SDT - Estimated Start Date of Transportation""</td></tr><tr><td>Process Result Description - 1</td><td>Arrival Place Code entry is required because Estimated Start Date of Transportation has been entered.</td></tr><tr><td>Process Result Suggestion - 1</td><td>1. Enter Arrival Place Code when customs transit of temporary landing cargo is intended.<BR/>2. Delete Estimated Start Date of Transportation when customs transit of temporary landing cargo is not intended.</td></tr><tr><td>Process Result Code - 2</td><td>R0095</td></tr><tr><td>Process Result Field - 2</td><td>Error occured on Field:""ARR - Arrival Place Code""</td></tr><tr><td>Process Result Description - 2</td><td>Arrival Place Code entry is required because Estimated Start Date of Transportation has been entered.</td></tr><tr><td>Process Result Suggestion - 2</td><td>1. Enter Arrival Place Code when customs transit of temporary landing cargo is intended.<BR/>2. Delete Estimated Start Date of Transportation when customs transit of temporary landing cargo is not intended.</td></tr></table></td></tr></table>";
		const string ExpectedXUEOutput2 = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><th>Response Details</th></tr><tr><td>A response message has been received from Customs.<br />The message type is : 'RAR - Risk Assessment Result'.<br/>The response indicates you have received a Risk Assessment Result 'HLD'.<br/><br/>Shown below is a summary of relevant information received in the message.<br /></td></tr><tr><td><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Master Bill of Lading Number</td><td>SPQBBOL09818349</td></tr><tr><td>House Bill of Lading Number</td><td>J07J827382738</td></tr><tr><td>Prior Notification Code</td><td>HLD</td></tr><tr><td>Prior Notification Subject</td><td>*TEST*REQUIRE CORRECTING AND/OR ADDING CARGO INFORMATION</td></tr><tr><td>Jurisdictional Customs Office Code</td><td>1A</td></tr><tr><td>Jurisdictional Customs Office Name</td><td>TOKYO</td></tr><tr><td>Vessel Call Sign</td><td>9044748</td></tr><tr><td>Vessel Name</td><td>AALSMEERGRACHT</td></tr><tr><td>Voyage Number</td><td>098123</td></tr><tr><td>Carrier Code</td><td>SPQB</td></tr><tr><td>Date Time Of Advance Cargo Information Registration</td><td>2014-01-21T13:30:00.0000000</td></tr><tr><td>Details Of Notifications Directions</td><td>THE FILER IS REQUIRED TO SUBMIT DETAILED DESCRIPTION OF GODS.</td></tr><tr><td>Contact Name</td><td>XXXXXXXXXXXXXXXXXXXXXXXXX</td></tr><tr><td>Contact Telephone Number</td><td>XXXXXXXXXX</td></tr><tr><td>Contact Fax Number</td><td>XXXXXXXXXX</td></tr><tr><td>Contact EMail Address</td><td>XXXXXXXXX@XXXXXXXXXXXXXXXX</td></tr></table></td></tr></table>";
		const string ExpectedXUEOutput3 = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><th>Response Details</th></tr><tr><td>A response message has been received from Customs.<br />The message type is : 'RAC - Cancellation of Risk Assessment Result'.<br/>The response indicates your previous Risk Assessment Result 'HLD' has been canceled.<br/><br/>Shown below is a summary of relevant information received in the message.<br /></td></tr><tr><td><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Master Bill of Lading Number</td><td>SPQBBOL09818349</td></tr><tr><td>House Bill of Lading Number</td><td>J07J827382738</td></tr><tr><td>Prior Notification Code</td><td>HLD</td></tr><tr><td>Prior Notification Subject</td><td>*TEST*CANCELLATION OF HLD</td></tr><tr><td>Jurisdictional Customs Office Code</td><td>1A</td></tr><tr><td>Jurisdictional Customs Office Name</td><td>TOKYO</td></tr><tr><td>Vessel Call Sign</td><td>9044748</td></tr><tr><td>Vessel Name</td><td>AALSMEERGRACHT</td></tr><tr><td>Voyage Number</td><td>098123</td></tr><tr><td>Carrier Code</td><td>SPQB</td></tr><tr><td>Date Time Of Advance Cargo Information Registration</td><td>2014-01-21T13:30:00.0000000</td></tr><tr><td>Contact Name</td><td>XXXXXXXXXXXXXXXXXXXXXXXXX</td></tr><tr><td>Contact Telephone Number</td><td>XXXXXXXXXX</td></tr><tr><td>Contact Fax Number</td><td>XXXXXXXXXX</td></tr><tr><td>Contact EMail Address</td><td>XXXXXXXXX@XXXXXXXXXXXXXXXX</td></tr></table></td></tr></table>";
		const string ExpectedNewVesselDetails = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><th colspan=""2"">Original Vessel Details</th></tr><tr><td>Master Waybill</td><td>NVOCC170424</td></tr><tr><td width=""150px"">Port of Loading</td><td>ADALV</td></tr><tr><td width=""150px"">Port of Discharge</td><td>&nbsp;</td></tr><tr><td width=""150px"">Carrier Code</td><td>&nbsp;</td></tr><tr><td width=""150px"">Vessel Name</td><td>A P MOLLER</td></tr><tr><td width=""150px"">Vessel Call Sign</td><td>OVYQ2</td></tr><tr><td width=""150px"">Voyage Number</td><td>12345678</td></tr><tr><td width=""150px"">Vessel Details Changed</td><td>Y</td></tr><tr><th colspan=""2"">New Vessel Details</th></tr><tr><td width=""150px"">New Carrier Code</td><td>1222</td></tr><tr><td width=""150px"">New Vessel Name</td><td>A P MOLLER</td></tr><tr><td width=""150px"">New Vessel Call Sign</td><td>OVYQ2</td></tr><tr><td width=""150px"">New Port of Loading Code</td><td>ADALV</td></tr><tr><td width=""150px"">New ETD</td><td>2017-05-03T10:34:00</td></tr><tr><td width=""150px"">Included Bill</td><td><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%""><tr><th colspan=""2"">AFR Bill Information</th></tr><tr><td>Action Type</td><td>CMV - Blanket Vessel Change</td></tr><tr><td>House Waybill</td><td>NVOCC170424</td></tr></table></td></tr><tr><td width=""150px"">Included Bill</td><td><table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%""><tr><th colspan=""2"">AFR Bill Information</th></tr><tr><td>Action Type</td><td>CMV - Blanket Vessel Change</td></tr><tr><td>House Waybill</td><td>NVOCC170425</td></tr></table></td></tr></table><br/>For more detailed information, please refer to the 'Message Text' tab.<br/>";
		#endregion

		#endregion

		public void TestGetInterpretatedHTML()
		{
			AssertEquals(MessageInterpretationGenerator.ParsingFailureResult, MessageInterpretationGenerator.GetInterpretatedHTML(null));
			CombineAssertions(() =>
			{
				TestTRXMessage.EM_MessageText = TestXUSAHRBody;
				AssertContains(ExpectedMastershipmentHeader + ExpectedSubshipmentWrapperHeader + ExpectedSubshipment1 + ExpectedSubshipmentWrapperFooter
					+ ExpectedSubshipmentWrapperHeader + ExpectedSubshipment2 + ExpectedSubshipmentWrapperFooter
					+ ExpectedSubshipmentWrapperHeader + ExpectedSubshipment3 + ExpectedSubshipmentWrapperFooter + ExpectedMastershipmentFooter
					, MessageInterpretationGenerator.GetInterpretatedHTML(TestTRXMessage));

				TestTRXMessage.EM_MessageText = TestXUSCompletionBody;
				AssertContains(ExpectedMastershipmentHeader + ExpectedCompletionMessage + ExpectedMastershipmentFooter
							, MessageInterpretationGenerator.GetInterpretatedHTML(TestTRXMessage));

				TestTRXMessage.EM_MessageText = TestXUSNoSubShipmentBody;
				AssertContains(ExpectedMastershipmentHeader + ExpectedMastershipmentFooter
					, MessageInterpretationGenerator.GetInterpretatedHTML(TestTRXMessage));

				TestTRXMessage.EM_MessageText = TestCMVBody;
				AssertContains(ExpectedNewVesselDetails
					, MessageInterpretationGenerator.GetInterpretatedHTML(TestTRXMessage));

				TestTRXMessage.EM_MessageText = TestXUEAHRBody;
				AssertEquals(MessageInterpretationGenerator.ParsingFailureResult, MessageInterpretationGenerator.GetInterpretatedHTML(TestTRXMessage));
			});
			CombineAssertions(() =>
			{
				TestRCVMessage.EM_MessageText = TestXUEAHRBody;
				AssertContains(ExpectedXUEOutput1
					, MessageInterpretationGenerator.GetInterpretatedHTML(TestRCVMessage));

				TestRCVMessage.EM_MessageText = TestXUERARBody;
				AssertContains(ExpectedXUEOutput2
					, MessageInterpretationGenerator.GetInterpretatedHTML(TestRCVMessage));

				TestRCVMessage.EM_MessageText = TestXUERACBody;
				AssertContains(ExpectedXUEOutput3
					, MessageInterpretationGenerator.GetInterpretatedHTML(TestRCVMessage));

				TestRCVMessage.EM_MessageText = TestXUSAHRBody;
				AssertEquals(MessageInterpretationGenerator.ParsingFailureResult, MessageInterpretationGenerator.GetInterpretatedHTML(TestRCVMessage));
			});
		}

		public void TestGetActionPurposeMeaning()
		{
			AssertEquals("The response indicates your House Bill Registration has been REJECTED<br/>", MessageInterpretationGenerator.GetActionPurposeMeaning("AHR-REJECTED", "AHR"));
			AssertEquals("The response indicates your House Bill Registration has been ACCEPTED<br/>", MessageInterpretationGenerator.GetActionPurposeMeaning("AHR-ACCEPTED", "AHR"));
			AssertEquals("The response indicates your House Bill Amendment has been REJECTED<br/>", MessageInterpretationGenerator.GetActionPurposeMeaning("CHR-REJECTED", "CHR"));
			AssertEquals("The response indicates your House Bill Amendment has been ACCEPTED<br/>", MessageInterpretationGenerator.GetActionPurposeMeaning("CHR-ACCEPTED", "CHR"));
			AssertEquals("The response indicates your Master Bill Registration has been REJECTED<br/>", MessageInterpretationGenerator.GetActionPurposeMeaning("AMR-REJECTED", "AMR"));
			AssertEquals("The response indicates your Master Bill Registration has been ACCEPTED<br/>", MessageInterpretationGenerator.GetActionPurposeMeaning("AMR-ACCEPTED", "AMR"));
			AssertEquals("The response indicates your Master Bill Amendment has been REJECTED<br/>", MessageInterpretationGenerator.GetActionPurposeMeaning("CMR-REJECTED", "CMR"));
			AssertEquals("The response indicates your Master Bill Amendment has been ACCEPTED<br/>", MessageInterpretationGenerator.GetActionPurposeMeaning("CMR-ACCEPTED", "CMR"));
			AssertEquals("The response indicates your Departure Time Registration has been REJECTED<br/>", MessageInterpretationGenerator.GetActionPurposeMeaning("DTR-REJECTED", "DTR"));
			AssertEquals("The response indicates your Departure Time Registration has been ACCEPTED<br/>", MessageInterpretationGenerator.GetActionPurposeMeaning("DTR-ACCEPTED", "DTR"));
			AssertEquals("The response indicates you have received a Risk Assessment Result 'DNL'.<br/>", MessageInterpretationGenerator.GetActionPurposeMeaning("DNL", "RAR"));
			AssertEquals("The response indicates you have received a Risk Assessment Result 'DNU'.<br/>", MessageInterpretationGenerator.GetActionPurposeMeaning("DNU", "RAR"));
			AssertEquals("The response indicates you have received a Risk Assessment Result 'HLD'.<br/>", MessageInterpretationGenerator.GetActionPurposeMeaning("HLD", "RAR"));
			AssertEquals("The response indicates your previous Risk Assessment Result 'DNL' has been canceled.<br/>", MessageInterpretationGenerator.GetActionPurposeMeaning("DNL", "RAC"));
			AssertEquals("The response indicates your previous Risk Assessment Result 'DNU' has been canceled.<br/>", MessageInterpretationGenerator.GetActionPurposeMeaning("DNU", "RAC"));
			AssertEquals("The response indicates your previous Risk Assessment Result 'HLD' has been canceled.<br/>", MessageInterpretationGenerator.GetActionPurposeMeaning("HLD", "RAC"));

			AssertEquals(string.Empty, MessageInterpretationGenerator.GetActionPurposeMeaning("AHR", "AHR"));
			AssertEquals(string.Empty, MessageInterpretationGenerator.GetActionPurposeMeaning("CHR", "CHR"));
			AssertEquals(string.Empty, MessageInterpretationGenerator.GetActionPurposeMeaning("AHR-", "AHR"));
			AssertEquals(string.Empty, MessageInterpretationGenerator.GetActionPurposeMeaning("-CHR", "CHR"));
			AssertEquals(string.Empty, MessageInterpretationGenerator.GetActionPurposeMeaning("AMR", "AMR"));
			AssertEquals(string.Empty, MessageInterpretationGenerator.GetActionPurposeMeaning("CMR", "CMR"));
			AssertEquals(string.Empty, MessageInterpretationGenerator.GetActionPurposeMeaning("AMR-", "AMR"));
			AssertEquals(string.Empty, MessageInterpretationGenerator.GetActionPurposeMeaning("-CMR", "CMR"));
			AssertEquals(string.Empty, MessageInterpretationGenerator.GetActionPurposeMeaning("", "RAR"));
			AssertEquals(string.Empty, MessageInterpretationGenerator.GetActionPurposeMeaning("", "RAC"));

			AssertEquals("The response indicates your House Bill Registration has been tested<br/>", MessageInterpretationGenerator.GetActionPurposeMeaning("AHR-tested-rejected", "AHR"));
			AssertEquals("The response indicates your House Bill Amendment has been accepted<br/>", MessageInterpretationGenerator.GetActionPurposeMeaning("CHR-accepted-tested", "CHR"));

			AssertEquals(string.Empty, MessageInterpretationGenerator.GetActionPurposeMeaning("AHR-tested-rejected", ""));
			AssertEquals(string.Empty, MessageInterpretationGenerator.GetActionPurposeMeaning("CHR-accepted-tested", ""));
			AssertEquals(string.Empty, MessageInterpretationGenerator.GetActionPurposeMeaning("DNL", ""));
			AssertEquals(string.Empty, MessageInterpretationGenerator.GetActionPurposeMeaning("DNU", ""));
			AssertEquals(string.Empty, MessageInterpretationGenerator.GetActionPurposeMeaning("HLD", ""));
		}

		public void TestGenerateOutputForSubshipments()
		{
			CombineAssertions(() =>
			{
				var basicTableHeader = "<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\">";
				var basicTableFooter = "</table>";
				using (var stringReader = new StringReader(TestXUSAHRBody))
				{
					var htmlCreator = new HtmlTableCreator();
					htmlCreator.EnableHTMLEncoding = false;
					var shipment = stringReader.Parse<UniversalShipment>();
					var subshipments = shipment.SubShipmentCollection;
					MessageInterpretationGenerator.GenerateOutputForSubshipments(htmlCreator, subshipments, Factory);
					AssertEquals(basicTableHeader + ExpectedSubshipmentWrapperHeader + ExpectedSubshipment1 + ExpectedSubshipmentWrapperFooter
						+ ExpectedSubshipmentWrapperHeader + ExpectedSubshipment2 + ExpectedSubshipmentWrapperFooter
						+ ExpectedSubshipmentWrapperHeader + ExpectedSubshipment3 + ExpectedSubshipmentWrapperFooter + basicTableFooter
						, htmlCreator.ToHtml());
				}
				using (var stringReader = new StringReader(TestXUSCompletionBody))
				{
					var htmlCreator = new HtmlTableCreator();
					htmlCreator.EnableHTMLEncoding = false;
					var shipment = stringReader.Parse<UniversalShipment>();
					var subshipments = shipment.SubShipmentCollection;
					MessageInterpretationGenerator.GenerateOutputForSubshipments(htmlCreator, subshipments, Factory);
					AssertEquals(basicTableHeader + ExpectedCompletionMessage + basicTableFooter, htmlCreator.ToHtml());
				}
				using (var stringReader = new StringReader(TestXUSNoSubShipmentBody))
				{
					var htmlCreator = new HtmlTableCreator();
					htmlCreator.EnableHTMLEncoding = false;
					var shipment = stringReader.Parse<UniversalShipment>();
					var subshipments = shipment.SubShipmentCollection;
					MessageInterpretationGenerator.GenerateOutputForSubshipments(htmlCreator, subshipments, Factory);
					AssertEquals(basicTableHeader + basicTableFooter, htmlCreator.ToHtml());
				}
			});
		}

		public void TestGenerateOutputForContainers()
		{
			CombineAssertions(() =>
			{
				using (var stringReader = new StringReader(TestXUSAHRBody))
				{
					var shipment = stringReader.Parse<UniversalShipment>();
					var subshipments = shipment.SubShipmentCollection;

					var htmlCreator = new HtmlTableCreator() { EnableHTMLEncoding = false };
					MessageInterpretationGenerator.GenerateOutputForContainers(htmlCreator, subshipments[0].ContainerCollection);
					AssertEquals("Expect 0 container", DefaultTableStart + ExpectedContainers1 + DefaultTableEnd, htmlCreator.ToHtml());
					htmlCreator = new HtmlTableCreator() { EnableHTMLEncoding = false };
					MessageInterpretationGenerator.GenerateOutputForContainers(htmlCreator, subshipments[1].ContainerCollection);
					AssertEquals("Expect 1 container", DefaultTableStart + ExpectedContainers2 + DefaultTableEnd, htmlCreator.ToHtml());
					htmlCreator = new HtmlTableCreator() { EnableHTMLEncoding = false };
					MessageInterpretationGenerator.GenerateOutputForContainers(htmlCreator, subshipments[2].ContainerCollection);
					AssertEquals("Expect multiple container", DefaultTableStart + ExpectedContainers3 + DefaultTableEnd, htmlCreator.ToHtml());
				}
			});
		}

		public void TestGenerateOutputForUniversalShipment_NVOCC()
		{
			CombineAssertions(() =>
			{
				using (var stringReader = new StringReader(TestXUSAHRBody))
				{
					var shipment = stringReader.Parse<UniversalShipment>();
					var subshipments = shipment.SubShipmentCollection;
					AssertEquals("Normal Test - 1", ExpectedSubshipment1, MessageInterpretationGenerator.GenerateOutputForUniversalShipment(subshipments[0], false, Factory));
					AssertEquals("Normal Test - 2", ExpectedSubshipment2, MessageInterpretationGenerator.GenerateOutputForUniversalShipment(subshipments[1], false, Factory));
					AssertEquals("Normal Test - 3", ExpectedSubshipment3, MessageInterpretationGenerator.GenerateOutputForUniversalShipment(subshipments[2], false, Factory));
					AssertEquals("Normal Test - 4", ExpectedMastershipmentHeader
						+ ExpectedSubshipmentWrapperHeader + ExpectedSubshipment1 + ExpectedSubshipmentWrapperFooter
						+ ExpectedSubshipmentWrapperHeader + ExpectedSubshipment2 + ExpectedSubshipmentWrapperFooter
						+ ExpectedSubshipmentWrapperHeader + ExpectedSubshipment3 + ExpectedSubshipmentWrapperFooter + ExpectedMastershipmentFooter
						, MessageInterpretationGenerator.GenerateOutputForUniversalShipment(shipment, true, Factory));
				}
				using (var stringReader = new StringReader(TestXUSCompletionBody))
				{
					var shipment = stringReader.Parse<UniversalShipment>();
					var subshipments = shipment.SubShipmentCollection;
					AssertEquals("Completion Test - 1", ExpectedMastershipmentHeader + ExpectedCompletionMessage + ExpectedMastershipmentFooter, MessageInterpretationGenerator.GenerateOutputForUniversalShipment(shipment, true, Factory));
				}
				using (var stringReader = new StringReader(TestXUSNoSubShipmentBody))
				{
					var shipment = stringReader.Parse<UniversalShipment>();
					var subshipments = shipment.SubShipmentCollection;
					AssertEquals("Empty Test - 1", ExpectedMastershipmentHeader + ExpectedMastershipmentFooter, MessageInterpretationGenerator.GenerateOutputForUniversalShipment(shipment, true, Factory));
				}
			});
		}

		public void TestGenerateOutputForUniversalShipment_VOCC()
		{
			CombineAssertions(() =>
			{
				using (var stringReader = new StringReader(TestXUSAMRBody))
				{
					var shipment = stringReader.Parse<UniversalShipment>();
					AssertEquals(ExpectedMasterAMRHeader + ExpectedMastershipmentFooter, MessageInterpretationGenerator.GenerateOutputForUniversalShipment(shipment, true, Factory));
				}
			});
		}

		public void TestGenerateOutputForUniversalShipment_ATD()
		{
			CombineAssertions(() =>
			{
				using (var stringReader = new StringReader(TestXUSATDBody))
				{
					var shipment = stringReader.Parse<UniversalShipment>();
					AssertEquals(ExpectedMasterDepartureTimeHeader + ExpectedMastershipmentFooter, MessageInterpretationGenerator.GenerateOutputForUniversalShipment(shipment, true, Factory));
				}
			});
		}

		public void TestGenerateOutputForUniversalEvent()
		{
			CombineAssertions(() =>
			{
				var eventDeserializer = new XmlEventDeserializer();
				var xmlEvent = eventDeserializer.Parse(TestXUEAHRBody);
				var output = MessageInterpretationGenerator.GenerateOutputForUniversalEvent(xmlEvent, Factory);
				AssertEquals(ExpectedXUEOutput1, output);

				xmlEvent = eventDeserializer.Parse(TestXUERARBody);
				output = MessageInterpretationGenerator.GenerateOutputForUniversalEvent(xmlEvent, Factory);
				AssertEquals(ExpectedXUEOutput2, output);

				xmlEvent = eventDeserializer.Parse(TestXUERACBody);
				output = MessageInterpretationGenerator.GenerateOutputForUniversalEvent(xmlEvent, Factory);
				AssertEquals(ExpectedXUEOutput3, output);
			});
		}

		public void TestFormatOutputInTemplate()
		{
			CombineAssertions(() =>
			{
				AssertContains(SystemDataRegistry.Instance.HtmlEmailStyleSheet.Value, MessageInterpretationGenerator.FormatOutputInTemplate(string.Empty));
				AssertContains("   <B>Body</B>", MessageInterpretationGenerator.FormatOutputInTemplate("<B>Body</B>"));
				AssertContains("<table ", MessageInterpretationGenerator.FormatOutputInTemplate("Something"));
			});
		}

		public void TestGenerateOutputForUniversalShipmentForBLLFunction()
		{
			var testBLLMessage = Factory.New<JPAFRMessage>();
			testBLLMessage.EM_ReceiveTransmit = ReceiveTransmitList.Transmit;
			testBLLMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			testBLLMessage.EM_MessageOwner = MessagingTypeList.Codes.RegisterBLL;
			testBLLMessage.EM_MessageText = TestBLLBody;

			var testBLCMessage = Factory.New<JPAFRMessage>();
			testBLCMessage.EM_ReceiveTransmit = ReceiveTransmitList.Transmit;
			testBLCMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			testBLCMessage.EM_MessageOwner = MessagingTypeList.Codes.RegisterBLL;
			testBLCMessage.EM_MessageText = TestBLCBody;

			var testBLLMessageWithoutFunctionCode = Factory.New<JPAFRMessage>();
			testBLLMessageWithoutFunctionCode.EM_ReceiveTransmit = ReceiveTransmitList.Transmit;
			testBLLMessageWithoutFunctionCode.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			testBLLMessageWithoutFunctionCode.EM_MessageOwner = MessagingTypeList.Codes.RegisterBLL;
			testBLLMessageWithoutFunctionCode.EM_MessageText = TestBLLBodyWithoutFunctionCode;
			CombineAssertions(() =>
			{
				AssertContains("BLL", @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><th colspan=""2"">BLL Function Information</th></tr><tr><td>Action Type</td><td>BLL - Register BLL</td></tr><tr><td>Function Code</td><td>3 - Register Merge</td></tr><tr><td>Change Reason</td><td>3 - Error in reported contents</td></tr><tr><th colspan=""2"">Original Bill Numbers</th></tr><tr><td>Original Bill 1</td><td>VICJEFFABC123-4</td></tr><tr><td>Original Bill 2</td><td>VICJEFFABC123-5</td></tr><tr><td>Original Bill 3</td><td>VICJEFFABC123-6</td></tr><tr><td>Original Bill 4</td><td>VICJEFFABC123-7</td></tr><tr><th colspan=""2"">New Bill Number</th></tr><tr><td>New Bill</td><td>VICJEFFABC123-6</td></tr></table><br/>For more detailed information, please refer to the 'Message Text' tab.<br/>", MessageInterpretationGenerator.GetInterpretatedHTML(testBLLMessage));
				AssertContains("BLC", @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><th colspan=""2"">BLL Function Information</th></tr><tr><td>Action Type</td><td>BLC - Cancel BLL</td></tr><tr><td>Function Code</td><td>4 - Cancel Split</td></tr><tr><td>Change Reason</td><td>2 - Change in cargo operation etc.</td></tr><tr><th colspan=""2"">Original Bill Number</th></tr><tr><td>Original Bill</td><td>VICJEFFABC123-8</td></tr><tr><th colspan=""2"">New Bill Numbers</th></tr><tr><td>New Bill 1</td><td>VICJEFFABC123-12</td></tr><tr><td>New Bill 2</td><td>VICJEFFABC123-13</td></tr><tr><td>New Bill 3</td><td>VICJEFFABC123-3</td></tr></table><br/>For more detailed information, please refer to the 'Message Text' tab.<br/>", MessageInterpretationGenerator.GetInterpretatedHTML(testBLCMessage));
				AssertContains("BLL Without Function Code", @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><th colspan=""2"">BLL Function Information</th></tr><tr><td>Action Type</td><td>BLL - Register BLL</td></tr><tr><td>Function Code</td><td>&nbsp;</td></tr><tr><td>Change Reason</td><td>3 - Error in reported contents</td></tr><tr><th colspan=""2"">Original Bill Numbers</th></tr><tr><td>Original Bill 1</td><td>VICJEFFABC123-4</td></tr><tr><td>Original Bill 2</td><td>VICJEFFABC123-5</td></tr><tr><td>Original Bill 3</td><td>VICJEFFABC123-6</td></tr><tr><td>Original Bill 4</td><td>VICJEFFABC123-7</td></tr><tr><th colspan=""2"">New Bill Number</th></tr><tr><td>New Bill</td><td>VICJEFFABC123-6</td></tr></table><br/>For more detailed information, please refer to the 'Message Text' tab.<br/>", MessageInterpretationGenerator.GetInterpretatedHTML(testBLLMessageWithoutFunctionCode));
			});
		}
	}
}
