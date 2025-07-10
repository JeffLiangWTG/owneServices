using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal.Testing
{
	class CMVUniversalShipmentHelperTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_JobReference = "novcc";
			header.JPH_MasterBillNumber = "MB20170306";
			header.JPH_Voyage = "1234567X";
			header.JPH_CarrierCode = "VIC";
			header.JPH_RL_NKLoading = "ADALV";
			header.JPH_LoadingPortSuffix = "X";

			var bill1 = header.Bills.AddNew("NACC1234567891");
			var bill2 = header.Bills.AddNew("NACC1234567892");

			var cmvEdiMessage = Factory.New<XmlEDIMessage>();
			cmvEdiMessage.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			cmvEdiMessage.EM_MessageType = EDIMessageTypeList.Codes.XDC;
			cmvEdiMessage.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			cmvEdiMessage.EM_Status = EDIMessageStatusList.Codes.Sent;
			cmvEdiMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			cmvEdiMessage.EM_MessageOwner = MessagingTypeList.Codes.BlanketVesselChange;
			cmvEdiMessage.EM_GB = header.JPH_GB_Branch;
			cmvEdiMessage.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
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
    <WayBillNumber>MB20170306</WayBillNumber>
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
  </Shipment>
</UniversalShipment>
";
			var helper = new CMVUniversalShipmentHelper(new TestErrorLogger(), header, cmvEdiMessage);
			AssertEquals(2, helper.BillsSent.Count);
			AssertContainsExactElementsInAnyOrder(new[] { bill1, bill2 }, helper.BillsSent);
			AssertEquals(0, helper.BillsUnsent.Count);

			cmvEdiMessage.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
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
    <WayBillNumber>MB20170306</WayBillNumber>
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
        <Value>N</Value>
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
        <WayBillNumber>NACC1234567891</WayBillNumber>
        <WayBillType>
          <Code>HWB</Code>
          <Description>House Waybill</Description>
        </WayBillType>

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
			helper = new CMVUniversalShipmentHelper(new TestErrorLogger(), header, cmvEdiMessage);
			AssertEquals(1, helper.BillsSent.Count);
			AssertContainsExactElementsInAnyOrder(new[] { bill1 }, helper.BillsSent);
			AssertEquals(1, helper.BillsUnsent.Count);
			AssertContainsExactElementsInAnyOrder(new[] { bill2 }, helper.BillsUnsent);

			cmvEdiMessage.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
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
    <WayBillNumber>MB20170306</WayBillNumber>
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
        <WayBillNumber>NACC1234567891</WayBillNumber>
        <WayBillType>
          <Code>HWB</Code>
          <Description>House Waybill</Description>
        </WayBillType>

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
			helper = new CMVUniversalShipmentHelper(new TestErrorLogger(), header, cmvEdiMessage);
			AssertEquals(1, helper.BillsSent.Count);
			AssertContainsExactElementsInAnyOrder(new[] { bill1 }, helper.BillsSent);
			AssertEquals(1, helper.BillsUnsent.Count);
			AssertContainsExactElementsInAnyOrder(new[] { bill2 }, helper.BillsUnsent);
		}
	}
}
