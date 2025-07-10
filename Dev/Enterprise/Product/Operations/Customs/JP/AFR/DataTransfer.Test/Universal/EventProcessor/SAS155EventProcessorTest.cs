using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.EventProcessing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal.Testing
{
	[TestedType(typeof(SAS155EventProcessor))]
	class SAS155EventProcessorTest : AFREventProcessorAbstractTest<SAS155EventProcessor>
	{
		protected override SAS155EventProcessor GetNewProcessor(IXmlEventValueObject eventDataObject, IXmlImportLogger logger, BusinessObjectFactory factory)
		{
			return new SAS155EventProcessor(eventDataObject, logger, factory);
		}

		protected override bool ExpectEmailToPostmasterOnError => false;

		public void TestSAS155StatusUpdate()
		{
			TestSAS155StatusUpdate(true, true, AFRBillCustomsStatusList.Codes.Registered, AFRBillCustomsStatusList.Codes.Registered, AFRBillCustomsStatusList.Codes.Registered);
			TestSAS155StatusUpdate(true, true, AFRBillCustomsStatusList.Codes.NL1, AFRBillCustomsStatusList.Codes.NL1, AFRBillCustomsStatusList.Codes.NL1);
			TestSAS155StatusUpdate(true, true, AFRBillCustomsStatusList.Codes.NL2, AFRBillCustomsStatusList.Codes.NL5, AFRBillCustomsStatusList.Codes.NL5);
			TestSAS155StatusUpdate(true, true, AFRBillCustomsStatusList.Codes.NL3, AFRBillCustomsStatusList.Codes.Registered, AFRBillCustomsStatusList.Codes.Registered);
			TestSAS155StatusUpdate(true, true, AFRBillCustomsStatusList.Codes.NL4, AFRBillCustomsStatusList.Codes.NL1, AFRBillCustomsStatusList.Codes.NL1);
			TestSAS155StatusUpdate(true, true, AFRBillCustomsStatusList.Codes.NL5, AFRBillCustomsStatusList.Codes.NL2, AFRBillCustomsStatusList.Codes.NL2);

			TestSAS155StatusUpdate(true, false, AFRBillCustomsStatusList.Codes.Registered, AFRBillCustomsStatusList.Codes.NL3, AFRBillCustomsStatusList.Codes.Registered);
			TestSAS155StatusUpdate(true, false, AFRBillCustomsStatusList.Codes.NL1, AFRBillCustomsStatusList.Codes.NL4, AFRBillCustomsStatusList.Codes.NL1);
			TestSAS155StatusUpdate(true, false, AFRBillCustomsStatusList.Codes.NL2, AFRBillCustomsStatusList.Codes.NL5, AFRBillCustomsStatusList.Codes.NL5);
			TestSAS155StatusUpdate(true, false, AFRBillCustomsStatusList.Codes.NL3, AFRBillCustomsStatusList.Codes.NL3, AFRBillCustomsStatusList.Codes.Registered);
			TestSAS155StatusUpdate(true, false, AFRBillCustomsStatusList.Codes.NL4, AFRBillCustomsStatusList.Codes.NL4, AFRBillCustomsStatusList.Codes.NL1);
			TestSAS155StatusUpdate(true, false, AFRBillCustomsStatusList.Codes.NL5, AFRBillCustomsStatusList.Codes.NL5, AFRBillCustomsStatusList.Codes.NL2);

			TestSAS155StatusUpdate(false, true, AFRBillCustomsStatusList.Codes.Registered, AFRBillCustomsStatusList.Codes.Registered, AFRBillCustomsStatusList.Codes.NL3);
			TestSAS155StatusUpdate(false, true, AFRBillCustomsStatusList.Codes.NL1, AFRBillCustomsStatusList.Codes.NL1, AFRBillCustomsStatusList.Codes.NL4);
			TestSAS155StatusUpdate(false, true, AFRBillCustomsStatusList.Codes.NL2, AFRBillCustomsStatusList.Codes.NL5, AFRBillCustomsStatusList.Codes.NL5);
			TestSAS155StatusUpdate(false, true, AFRBillCustomsStatusList.Codes.NL3, AFRBillCustomsStatusList.Codes.Registered, AFRBillCustomsStatusList.Codes.NL3);
			TestSAS155StatusUpdate(false, true, AFRBillCustomsStatusList.Codes.NL4, AFRBillCustomsStatusList.Codes.NL1, AFRBillCustomsStatusList.Codes.NL4);
			TestSAS155StatusUpdate(false, true, AFRBillCustomsStatusList.Codes.NL5, AFRBillCustomsStatusList.Codes.NL2, AFRBillCustomsStatusList.Codes.NL5);

			TestSAS155StatusUpdate(false, false, AFRBillCustomsStatusList.Codes.Registered, AFRBillCustomsStatusList.Codes.NL3, AFRBillCustomsStatusList.Codes.NL3);
			TestSAS155StatusUpdate(false, false, AFRBillCustomsStatusList.Codes.NL1, AFRBillCustomsStatusList.Codes.NL4, AFRBillCustomsStatusList.Codes.NL4);
			TestSAS155StatusUpdate(false, false, AFRBillCustomsStatusList.Codes.NL2, AFRBillCustomsStatusList.Codes.NL5, AFRBillCustomsStatusList.Codes.NL5);
			TestSAS155StatusUpdate(false, false, AFRBillCustomsStatusList.Codes.NL3, AFRBillCustomsStatusList.Codes.NL3, AFRBillCustomsStatusList.Codes.NL3);
			TestSAS155StatusUpdate(false, false, AFRBillCustomsStatusList.Codes.NL4, AFRBillCustomsStatusList.Codes.NL4, AFRBillCustomsStatusList.Codes.NL4);
			TestSAS155StatusUpdate(false, false, AFRBillCustomsStatusList.Codes.NL5, AFRBillCustomsStatusList.Codes.NL5, AFRBillCustomsStatusList.Codes.NL5);
		}

		void TestSAS155StatusUpdate(ZBool blanketChange, ZBool isNON, ZString oldReleaseStatus, ZString expectedBill1ReleaseStatus, ZString expectedBill2ReleaseStatus)
		{
			var testVessel = Factory.NewWithValidTestData<RefVessel>();
			testVessel.RV_RadioCallSign = "OVYQX";
			testVessel.RV_Code = "X P MOLLER";

			var header = Factory.New<JPAFRHeader>();
			header.JPH_JobReference = "novcc";
			header.JPH_MasterBillNumber = "MB20170306";
			header.JPH_Voyage = "1234567X";
			header.JPH_CarrierCode = "VIC";
			header.JPH_RL_NKLoading = "ADALV";
			header.JPH_LoadingPortSuffix = "X";
			header.JPH_VesselName = testVessel.RV_Code;
			header.JPH_MessageStatus = MessageStatusList.Codes.AwaitingBlanketVesselChangeCompletion;
			var bill1 = header.Bills.AddNew("NACC1234567891");
			var bill2 = header.Bills.AddNew("NACC1234567892");
			Factory.SaveForTesting();

			SetupOriginalMessageAndSave(header, MessagingTypeList.Codes.BlanketVesselChange);
			bill1.JPB_ReleaseStatus = oldReleaseStatus;
			bill2.JPB_ReleaseStatus = oldReleaseStatus;
			Factory.SaveForTesting();

			var cMVResponseXML = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<ActionPurpose>
				<Code>VCR</Code>
				<Description>SAS155</Description>
			</ActionPurpose>
			<DataProvider>AFR</DataProvider>
			<DataTargetCollection>
				<DataTarget>
					<Key>AFRHeader</Key>
					<Type>AFRHeader</Type>
				</DataTarget>
			</DataTargetCollection>
		</DataContext>
		<EventTime>2013-12-04T04:37:00.0000000Z</EventTime>
		<EventType>MSC</EventType>
		<EventReference>TEST</EventReference>
		<ContextCollection>
			<Context>
				<Type>InternalTransactionNumber</Type>
				<Value>{1}</Value>
			</Context>
			<Context>
				<Type>VesselCallSign</Type>
				<Value>OVYQ2</Value>
			</Context>
			<Context>
				<Type>VesselName</Type>
				<Value>A P MOLLER</Value>
			</Context>
			<Context>
				<Type>VoyageNumber</Type>
				<Value>12345678</Value>
			</Context>
			<Context>
				<Type>CarrierCode</Type>
				<Value>JEFF</Value>
			</Context>
			<Context>
				<Type>PortOfLoadingUNLOCO</Type>
				<Value>AUSYD</Value>
			</Context>
			<Context>
				<Type>PortOfLoadingSuffix</Type>
				<Value>1</Value>
			</Context>
			<Context>
				<Type>VesselCallSignNew</Type>
				<Value>OVYQ2</Value>
			</Context>
			<Context>
				<Type>VoyageNumberNew</Type>
				<Value>12345678</Value>
			</Context>
			<Context>
				<Type>CarrierCodeNew</Type>
				<Value>JEFF</Value>
			</Context>
			<Context>
				<Type>PortOfLoadingUNLOCONew</Type>
				<Value>AUSYD</Value>
			</Context>
			<Context>
				<Type>PortOfLoadingSuffixNew</Type>
				<Value>1</Value>
			</Context>
			<Context>
				<Type>NotificationDetails</Type>
				<Value>{0}</Value>
			</Context>
			{2}
		</ContextCollection>
	</Event>
</UniversalEvent>
";
			Assert(header.Messages.Any());
			var cmvMsg = header.Messages[0];
			if (blanketChange)
			{
				cmvMsg.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
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
			}
			else
			{
				cmvMsg.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
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
			}
			Factory.SaveForTesting();

			var notificationDetails = @"&lt;table width=""100%"" border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""&gt;&lt;tr&gt;&lt;td&gt;Procedure Code&lt;/td&gt;&lt;td&gt;CMV&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Internal Procedure Code&lt;/td&gt;&lt;td&gt;1CM&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Vessel Call Sign (Original)&lt;/td&gt;&lt;td&gt;VREE9&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Voyage Number (Original)&lt;/td&gt;&lt;td&gt;200517&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Carrier Code (Original)&lt;/td&gt;&lt;td&gt;SAPC&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Port of Loading (Original)&lt;/td&gt;&lt;td&gt;AUMEL&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Vessel Call Sign (New)&lt;/td&gt;&lt;td&gt;VREE9&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Voyage Number (New)&lt;/td&gt;&lt;td&gt;200519&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Carrier Code (New)&lt;/td&gt;&lt;td&gt;SAPC&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Port of Loading (New)&lt;/td&gt;&lt;td&gt;AUMEL&lt;/td&gt;&lt;/tr&gt;&lt;/table&gt;&lt;BR/&gt;&lt;BR/&gt;&lt;table width=""100%"" border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""&gt;&lt;tr class=""tableheadings""&gt;&lt;th colspan=""5""&gt;Bill of Lading Details&lt;/th&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;House B/L Number&lt;/td&gt;&lt;td&gt;Process Result Code&lt;/td&gt;&lt;td&gt;Process Result Field&lt;/td&gt;&lt;td&gt;Process Result Decritption&lt;/td&gt;&lt;td&gt;Process Result Suggestion&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;NACC1234567890&lt;/td&gt;&lt;td&gt;U0001&lt;/td&gt;&lt;td&gt;Error occured on Field:""0000 - -"" on 3rd occurance&lt;/td&gt;&lt;td&gt;Login failure: 1. This User ID is invalid. 2. This User ID is invalid because the temporary password has not been changed after password initialization.&lt;/td&gt;&lt;td&gt;1. Check if User ID is valid. 2. Change the temporary password in URY/URY0W procedure.&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;NACC1234567891&lt;/td&gt;&lt;td&gt;E0016&lt;/td&gt;&lt;td&gt;Error occured on Field:""0000 - -"" on 1st occurance&lt;/td&gt;&lt;td&gt;Even though Departure Time Registration (ATD) has been done, this procedure is not available because Advance Cargo Information Registration has not been done for Master B/L related to the entered House B/L.&lt;/td&gt;&lt;td&gt;1. Check the entered House B/L Number and correct it if it is wrong. 2. Implement this procedure after AMR or CMR for Master B/L.&lt;/td&gt;&lt;/tr&gt;&lt;/table&gt;";
			var xmlEvent = new XmlEventDeserializer().Parse(string.Format(cMVResponseXML, notificationDetails, header.Messages[0].EM_MessageNum,
				!isNON ? @"<Context>
				<Type>BillInformation</Type>
				<Value />
				<SubContextCollection>
					<SubContext>
						<Type>BillNumber</Type>
						<Value>NACC1234567891</Value>
					</SubContext>
					<SubContext>
						<Type>ProcessResultCode</Type>
						<Value>E0006</Value>
					</SubContext>
				</SubContextCollection>
			</Context>" : @"<Context>
				<Type>BillInformation</Type>
				<Value />
				<SubContextCollection>
					<SubContext>
						<Type>BillNumber</Type>
						<Value>NON</Value>
					</SubContext>
				</SubContextCollection>
			</Context>"));

			var logger = new TestErrorLogger();
			var subscriber = GetNewEventParentFinder(logger);
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
			var logParents = subscriber.GetLogParentsForEvent(xmlEvent);

			var sb = new ZStringBuilder();

			sb.Append($"Information - The Carrier Code of AFR Job '{header.JPH_JobReference}' changes from 'VIC' to 'JEFF'.");
			sb.Append("Warning - Vessel can not be found via Vessel Call Sign 'OVYQ2'.");
			sb.Append($"Information - The Vessel Name of AFR Job '{header.JPH_JobReference}' changes from 'X P MOLLER' to ''.");
			sb.Append($"Information - The JPAFRHeader.JPH_RadioCallSign of AFR Job '{header.JPH_JobReference}' changes from 'OVYQX' to 'OVYQ2'.");
			sb.Append($"Information - The Voyage Number of AFR Job '{header.JPH_JobReference}' changes from '1234567X' to '12345678'.");
			sb.Append($"Information - The Port Of Loading of AFR Job '{header.JPH_JobReference}' changes from 'ADALV' to 'AUSYD'.");
			sb.Append($"Information - The Port Of Loading Suffix of AFR Job '{header.JPH_JobReference}' changes from 'X' to '1'.");

			if (oldReleaseStatus != expectedBill1ReleaseStatus && oldReleaseStatus != expectedBill2ReleaseStatus && blanketChange && !isNON)
			{
				sb.Append($"Information - The Bill '{bill2.JPB_BillNumber}' Customs Status of AFR Job '{header.JPH_JobReference}' changes from '{oldReleaseStatus}' to '{expectedBill2ReleaseStatus}'.");
				sb.Append($"Information - The Bill '{bill1.JPB_BillNumber}' Customs Status of AFR Job '{header.JPH_JobReference}' changes from '{oldReleaseStatus}' to '{expectedBill1ReleaseStatus}'.");
			}
			else
			{
				if (oldReleaseStatus != expectedBill1ReleaseStatus)
				{
					sb.Append($"Information - The Bill '{bill1.JPB_BillNumber}' Customs Status of AFR Job '{header.JPH_JobReference}' changes from '{oldReleaseStatus}' to '{expectedBill1ReleaseStatus}'.");
				}
				if (oldReleaseStatus != expectedBill2ReleaseStatus)
				{
					sb.Append($"Information - The Bill '{bill2.JPB_BillNumber}' Customs Status of AFR Job '{header.JPH_JobReference}' changes from '{oldReleaseStatus}' to '{expectedBill2ReleaseStatus}'.");
				}
			}

			CombineAssertions($"{header.JPH_JobReference}-{blanketChange}-{isNON}-{oldReleaseStatus}-{expectedBill1ReleaseStatus}-{expectedBill2ReleaseStatus}", () =>
			{
				AssertEquals("Release status of Bill1", expectedBill1ReleaseStatus, bill1.JPB_ReleaseStatus);
				AssertEquals("Release status of Bill2", expectedBill2ReleaseStatus, bill2.JPB_ReleaseStatus);
				AssertEquals("LogParent", 1, logParents.Length);
				AssertEquals("LogParentPK", header.PK, logParents[0].PK);
				AssertEquals(sb.ToStringWithNewLineBetweenAppends(), logger.Logs);
				AssertHasEmail(MessagingTypeList.Descriptions.VesselChangeResult + " Response for " + header.JPH_JobReference, "<table width=\"100%\" border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><tr><td>Procedure Code</td><td>CMV</td></tr><tr><td>Internal Procedure Code</td><td>1CM</td></tr><tr><td>Vessel Call Sign (Original)</td><td>VREE9</td></tr><tr><td>Voyage Number (Original)</td><td>200517</td></tr><tr><td>Carrier Code (Original)</td><td>SAPC</td></tr><tr><td>Port of Loading (Original)</td><td>AUMEL</td></tr><tr><td>Vessel Call Sign (New)</td><td>VREE9</td></tr><tr><td>Voyage Number (New)</td><td>200519</td></tr><tr><td>Carrier Code (New)</td><td>SAPC</td></tr><tr><td>Port of Loading (New)</td><td>AUMEL</td></tr></table><BR/><BR/><table width=\"100%\" border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><tr class=\"tableheadings\"><th colspan=\"5\">Bill of Lading Details</th></tr><tr><td>House B/L Number</td><td>Process Result Code</td><td>Process Result Field</td><td>Process Result Decritption</td><td>Process Result Suggestion</td></tr><tr><td>NACC1234567890</td><td>U0001</td><td>Error occured on Field:\"0000 - -\" on 3rd occurance</td><td>Login failure: 1. This User ID is invalid. 2. This User ID is invalid because the temporary password has not been changed after password initialization.</td><td>1. Check if User ID is valid. 2. Change the temporary password in URY/URY0W procedure.</td></tr><tr><td>NACC1234567891</td><td>E0016</td><td>Error occured on Field:\"0000 - -\" on 1st occurance</td><td>Even though Departure Time Registration (ATD) has been done, this procedure is not available because Advance Cargo Information Registration has not been done for Master B/L related to the entered House B/L.</td><td>1. Check the entered House B/L Number and correct it if it is wrong. 2. Implement this procedure after AMR or CMR for Master B/L.</td></tr></table>", Staff2.GS_EmailAddress);
			});

			header.Messages.RemoveAndDeleteAllFromTest();
			header.Delete();
			testVessel.Delete();
			Factory.SaveForTesting();
		}
	}
}
