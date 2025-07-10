using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal.Testing
{
	[TestedType(typeof(AFRHeaderDataContextManager))]
	sealed class AFRHeaderDataContextManagerTest : ShipmentDataContextManagerTestCase<AFRHeaderDataContextManager, JPAFRHeader>
	{
		public void TestEventContextValues()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_MasterBillNumber = "MB001";
			header.JPH_RL_NKLoading = "AUSYD";
			header.JPH_RL_NKDischarge = "JPUKB";
			header.JPH_VesselName = "VES005";
			header.JPH_RadioCallSign = "TEST01234";
			header.JPH_Voyage = "NS001";

			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "HB1";
			bill1.JPB_RL_NKOrigin = "AUBNE";
			bill1.JPB_RL_NKFinalDestination = "AFBEP";

			var bill2 = header.Bills.AddNew();
			bill2.JPB_BillNumber = "HB2";
			bill2.JPB_RL_NKOrigin = "SGSIN";
			bill2.JPB_RL_NKFinalDestination = "AFCLN";

			var manager = new AFRHeaderDataContextManager();
			((IDataContextManager)manager).Init(header);

			var values = manager.EventContextValues.ToDictionary(c => c.Key.Type, c => c.Value);

			CombineAssertions(() =>
			{
				AssertEquals("Count", 9, values.Count);

				AssertEquals("MBOLNumber", values[nameof(UniversalEvent.ContextTypes.MBOLNumber)], header.JPH_MasterBillNumber);
				AssertEquals("MBOLOriginUNLOCO", values[nameof(UniversalEvent.ContextTypes.MBOLOriginUNLOCO)], header.JPH_RL_NKLoading);
				AssertEquals("MBOLDestinationUNLOCO", values[nameof(UniversalEvent.ContextTypes.MBOLDestinationUNLOCO)], header.JPH_RL_NKDischarge);

				AssertEquals("VesselName", values[nameof(UniversalEvent.ContextTypes.VesselName)], header.JPH_VesselName);
				AssertEquals("VesselCallSign", values[nameof(UniversalEvent.ContextTypes.VesselCallSign)], header.JPH_RadioCallSign);
				AssertEquals("VoyageNumber", values[nameof(UniversalEvent.ContextTypes.VoyageNumber)], header.JPH_Voyage);

				AssertEquals("HBOLNumber", values[nameof(UniversalEvent.ContextTypes.HBOLNumber)], bill1.JPB_BillNumber);
				AssertEquals("HBOLOriginUNLOCO", values[nameof(UniversalEvent.ContextTypes.HBOLOriginUNLOCO)], bill1.JPB_RL_NKOrigin);
				AssertEquals("HBOLDestinationUNLOCO", values[nameof(UniversalEvent.ContextTypes.HBOLDestinationUNLOCO)], bill1.JPB_RL_NKFinalDestination);
			});
		}

		public void TestExportData_SubShipment()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_MasterBillNumber = "OB1";
			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "HB1";
			var bill2 = header.Bills.AddNew();
			bill2.JPB_BillNumber = "HB2";
			IShipmentDataContextManager manager = new AFRHeaderDataContextManager();
			var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, header)));
			var headerData = (UniversalShipment)writer.GetDataObject(header);
			AssertEquals("headerData.DataContext.DataSourceCollection.Count", 1, headerData.DataContext.DataSourceCollection.Count());
			AssertNotNull("Should have AFRHeader", headerData.GetMatchingDataSource(DataContextType.AFRHeader));
			AssertEquals("headerData.WayBillNumber", "OB1", headerData.WayBillNumber);
			AssertEquals("headerData.SubShipmentCollection.Count", 2, headerData.SubShipmentCollection.Count);
			var bill1Data = headerData.SubShipmentCollection[0];
			var bill2Data = headerData.SubShipmentCollection[1];
			if (bill2Data.WayBillNumber.GetValueOrDefault() == "HB1")
			{
				bill1Data = headerData.SubShipmentCollection[1];
				bill2Data = headerData.SubShipmentCollection[0];
			}
			AssertEquals("bill1Data.WayBillNumber", "HB1", bill1Data.WayBillNumber);
			AssertNull("bill1Data.SubShipmentCollection", bill1Data.SubShipmentCollection);
			AssertEquals("bill2Data.WayBillNumber", "HB2", bill2Data.WayBillNumber);
			AssertNull("bill2Data.SubShipmentCollection", bill2Data.SubShipmentCollection);
		}

		public void TestOnlyProcessSeaShipment()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			IShipmentDataContextManager manager = new AFRHeaderDataContextManager();
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.AFRHeader, null);

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = "MB1234",
				WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
				TransportMode = new CodeDescriptionPair() { Code = Core.Constants.TransportModes.Air }
			};
			var logger = new TestErrorLogger();
			AssertEquals("Should not be used as it's not Sea", false, manager.UseIncomingShipmentData(shipment, logger, new UniversalObjectFactory()));
			shipment.TransportMode.Code = Core.Constants.TransportModes.Sea;
			AssertEquals("Should be used as it's Sea", true, manager.UseIncomingShipmentData(shipment, logger, new UniversalObjectFactory()));
		}

		public void TestMatchingDataTargetByKey()
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.AFRHeader, null);
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = "MB2345",
				WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
				TransportMode = new CodeDescriptionPair() { Code = Core.Constants.TransportModes.Sea }
			};
			IShipmentDataContextManager manager = new AFRHeaderDataContextManager();

			CombineAssertions(() =>
			{
				JPAFRHeader[] reloadedHeaders = null;
				AssertEquals(0, Factory.Load<JPAFRHeader>(new ZQuery()).Length);
				var logger = new TestErrorLogger();
				AssertEquals(true, manager.UseIncomingShipmentData(shipment, logger, Factory));
				Factory.SaveForTesting();
				reloadedHeaders = Factory.Load<JPAFRHeader>(new ZQuery());
				AssertEquals("Should Have 1 By Now", 1, reloadedHeaders.Length);
				AssertEquals("MB2345", reloadedHeaders.FirstOrDefault().JPH_MasterBillNumber);
				Assert(logger.Logs.Contains("Information - No matching JPAFRHeader found, creating new JPAFRHeader."));
				logger.ClearLogs();

				var targetJobRef = reloadedHeaders.FirstOrDefault().JPH_JobReference;
				shipment.DataContext.DataTargetCollection.FirstOrDefault().Key = targetJobRef;
				shipment.WayBillNumber = "MB3456";
				AssertEquals(true, manager.UseIncomingShipmentData(shipment, logger, Factory));
				Factory.SaveForTesting();
				reloadedHeaders = Factory.Load<JPAFRHeader>(new ZQuery());
				AssertEquals("Should Have 1 By Now", 1, reloadedHeaders.Length);
				AssertEquals("MB3456", reloadedHeaders.FirstOrDefault().JPH_MasterBillNumber);
				Assert(logger.Logs.Contains("Information - Successfully loaded matching JPAFRHeader."));
				logger.ClearLogs();

				shipment.DataContext.DataTargetCollection.FirstOrDefault().Key = "xxxxxxxxx";
				shipment.WayBillNumber = "MB4567";
				AssertExceptionThrown<DataObjectReadFailureException>("Match couldn't be found for AFRHeader with Key xxxxxxxxx", () => manager.UseIncomingShipmentData(shipment, logger, Factory));
				Factory.SaveForTesting();
				reloadedHeaders = Factory.Load<JPAFRHeader>(new ZQuery());
				AssertEquals("Should Have 1 By Now", 1, reloadedHeaders.Length);
				AssertEquals("MB3456", reloadedHeaders.FirstOrDefault().JPH_MasterBillNumber);
				logger.ClearLogs();
			});
		}

		protected override string ValidPopulatedUniversalShipmentXML
		{
			get
			{
				return @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>AFRHeader</Type>
          <Key>C00001201</Key>
        </DataSource>
      </DataSourceCollection>
    </DataContext>

    <Branch>
      <Code>CHI</Code>
      <Name>Chicago</Name>
    </Branch>
    <GoodsValue>100.0000</GoodsValue>
    <GoodsValueCurrency>
      <Code>AUD</Code>
      <Description>Australian Dollar</Description>
    </GoodsValueCurrency>
    <LloydsIMO>8811924</LloydsIMO>
    <PortOfDischarge>
      <Code>JPTKA</Code>
      <Name>Tokai</Name>
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
      <Code>AU</Code>
      <Name>Australia</Name>
    </VesselCountryOfRegistration>
    <VesselName>ADMIRALENGRACHT</VesselName>
    <VoyageFlightNo>V234</VoyageFlightNo>
    <WayBillNumber>OTT1MB3234232323</WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>

    <AddInfoCollection>
      <AddInfo>
        <Key>JPCarrierCode</Key>
        <Value>OTT1</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPPortOfLoadingSuffix</Key>
        <Value>1</Value>
      </AddInfo>
      <AddInfo>
        <Key>IsDepartureFromRelaxedArea</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPNotificationForwardingPartyCode1</Key>
        <Value>XXXW</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPNotificationForwardingPartyCode2</Key>
        <Value>XXXA</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPNotificationForwardingPartyCode3</Key>
        <Value>APLD</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPOtherRelevantLawCode1</Key>
        <Value>3S</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPOtherRelevantLawCode2</Key>
        <Value>SA</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPOtherRelevantLawCode3</Key>
        <Value>DH</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPOtherRelevantLawCode4</Key>
        <Value>AH</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPOtherRelevantLawCode5</Key>
        <Value>KS</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPTranshipmentArrivalPlaceCode</Key>
        <Value>ABSAD</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPTranshipmentArrivalPlaceName</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>JPTranshipmentEstimatedStartDate</Key>
        <Value>2013-09-29T00:00:00</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPTranshipmentEstimatedFinishDate</Key>
        <Value>2013-10-02T00:00:00</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPTranshipmentDuration</Key>
        <Value>3</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPTranshipmentReasonCode</Key>
        <Value>POS</Value>
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
        <Key>JPTranshipmentTransportMode</Key>
        <Value>16</Value>
      </AddInfo>
    </AddInfoCollection>

    <DateCollection>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2013-09-24T03:00:00</Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2013-09-29T03:00:00</Value>
      </Date>
    </DateCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>Carrier</AddressType>
        <AddressShortCode>PIC: 32 PETERKIN STREET</AddressShortCode>
        <OrganizationCode>ABSCOUBNE</OrganizationCode>
        <Address1>32 PETERKIN STREET</Address1>
        <Address2>ACACIA RIDGE QLD</Address2>
        <AddressOverride>false</AddressOverride>
        <City>AUSTRALIA</City>
        <CompanyName>ABS COURIER SERVICES</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum>234-46-4567</GovRegNum>
        <GovRegNumType>
          <Code>SSN</Code>
          <Description>Social Security Number</Description>
        </GovRegNumType>
        <Phone></Phone>
        <Port>
          <Code></Code>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State></State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <CountryOfIssue>
              <Code>AU</Code>
              <Name>Australia</Name>
            </CountryOfIssue>
            <Type>
              <Code>ABN</Code>
              <Description>Australian Business Number ( Regist</Description>
            </Type>
            <Value>27010649377</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <CountryOfIssue>
              <Code>AU</Code>
              <Name>Australia</Name>
            </CountryOfIssue>
            <Type>
              <Code>DLV</Code>
              <Description>Deliverance System Code</Description>
            </Type>
            <Value>BECMEL_abscou</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <CountryOfIssue>
              <Code>US</Code>
              <Name>United States</Name>
            </CountryOfIssue>
            <Type>
              <Code>CCC</Code>
              <Description>Standard Carrier Alpha Code</Description>
            </Type>
            <Value>UNKN</Value>
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
        </DataContext>

        <CommercialInfo>
          <CommercialChargeCollection>
            <CommercialCharge>
              <ChargeType>
                <Code>OFT</Code>
                <Description>Overseas Freight</Description>
              </ChargeType>
              <Amount>3000.0000</Amount>
              <Currency>
                <Code>AUD</Code>
                <Description>Australian Dollar</Description>
              </Currency>
            </CommercialCharge>
          </CommercialChargeCollection>
        </CommercialInfo>
        <PortOfDestination>
          <Code>SGSIN</Code>
          <Name>Singapore</Name>
        </PortOfDestination>
        <PortOfOrigin>
          <Code>AUMEL</Code>
          <Name>Melbourne</Name>
        </PortOfOrigin>
        <TotalWeight>1.000</TotalWeight>
        <TotalWeightUnit>
          <Code>TN</Code>
          <Description>Metric Ton</Description>
        </TotalWeightUnit>
        <WayBillNumber>XXXAHB3234232</WayBillNumber>
        <WayBillType>
          <Code>HWB</Code>
          <Description>House Waybill</Description>
        </WayBillType>

        <ContainerCollection>
          <Container>
            <ContainerNumber>STUE0924182</ContainerNumber>
            <ContainerType>
              <Code>20FR</Code>
              <Description>Twenty foot flatrack</Description>
              <ISOCode>22P1</ISOCode>
            </ContainerType>
            <IsEmptyContainer>true</IsEmptyContainer>
            <Seal>S32</Seal>
            <SecondSeal>D112</SecondSeal>
            <TotalHeight>8.500</TotalHeight>
            <TotalLength>20.000</TotalLength>
            <TotalWidth>8.000</TotalWidth>

            <AddInfoCollection>
              <AddInfo>
                <Key>JPContainerOwnershipCode</Key>
                <Value>3</Value>
              </AddInfo>
            </AddInfoCollection>
          </Container>
          <Container>
            <ContainerNumber>STUE0924195</ContainerNumber>
            <ContainerType>
              <Code>40FR</Code>
              <Description>Forty foot flatrack</Description>
              <ISOCode>42P1</ISOCode>
            </ContainerType>
            <IsEmptyContainer>false</IsEmptyContainer>
            <Seal>F434</Seal>
            <SecondSeal>D2323</SecondSeal>
            <TotalHeight>8.500</TotalHeight>
            <TotalLength>40.000</TotalLength>
            <TotalWidth>8.000</TotalWidth>

            <AddInfoCollection>
              <AddInfo>
                <Key>JPContainerOwnershipCode</Key>
                <Value>T</Value>
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
            <AddressShortCode>OFC: 6 KNOCKLFTY TERRACE</AddressShortCode>
            <OrganizationCode>BABINAHBA</OrganizationCode>
            <Address1>6 KNOCKLFTY TERRACE</Address1>
            <Address2></Address2>
            <AddressOverride>false</AddressOverride>
            <City>WEST HOBART</City>
            <CompanyName>BABES IN ARMS</CompanyName>
            <Country>
              <Code>AU</Code>
              <Name>Australia</Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <GovRegNum>323423432</GovRegNum>
            <GovRegNumType>
              <Code>DUN</Code>
              <Description>Data Universal Numbering System</Description>
            </GovRegNumType>
            <Phone>+61 (3) 6234-4500</Phone>
            <Port>
              <Code>AUHBA</Code>
              <Name>Hobart</Name>
            </Port>
            <Postcode>7000</Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State>TAS</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <CountryOfIssue>
                  <Code>AU</Code>
                  <Name>Australia</Name>
                </CountryOfIssue>
                <Type>
                  <Code>CCP</Code>
                  <Description>Customs Controlled Premises Code</Description>
                </Type>
                <Value>DG35M</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>ConsignorDocumentaryAddress</AddressType>
            <AddressShortCode>OFC: 27 WEDGE WOOD ROAD</AddressShortCode>
            <OrganizationCode>AAAWHOMEL</OrganizationCode>
            <Address1>27 WEDGE WOOD ROAD</Address1>
            <Address2></Address2>
            <AddressOverride>false</AddressOverride>
            <City>HALLAM</City>
            <CompanyName>AAA WHOLESALE *FABRICS &amp; RUMORTEX PTY LTD</CompanyName>
            <Country>
              <Code>AU</Code>
              <Name>Australia</Name>
            </Country>
            <Email></Email>
            <Fax>+61 (3) 8-7951-2442</Fax>
            <GovRegNum>103970-12345</GovRegNum>
            <GovRegNumType>
              <Code>CBN</Code>
              <Description>CBP Assigned Number</Description>
            </GovRegNumType>
            <Phone>+61 (3) 8-7951-2223</Phone>
            <Port>
              <Code>AUMEL</Code>
              <Name>Melbourne</Name>
            </Port>
            <Postcode>3803</Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State>VIC</State>

            <RegistrationNumberCollection>
              <RegistrationNumber>
                <CountryOfIssue>
                  <Code>AU</Code>
                  <Name>Australia</Name>
                </CountryOfIssue>
                <Type>
                  <Code>ABN</Code>
                  <Description>Australian Business Number ( Regist</Description>
                </Type>
                <Value>36105207501</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <CountryOfIssue>
                  <Code>AU</Code>
                  <Name>Australia</Name>
                </CountryOfIssue>
                <Type>
                  <Code>DLV</Code>
                  <Description>Deliverance System Code</Description>
                </Type>
                <Value>BECMEL_aaawho</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Type>
                  <Code>FEI</Code>
                  <Description>FDA Establishment Identifier</Description>
                </Type>
                <Value>17153568490</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <CountryOfIssue>
                  <Code>CA</Code>
                  <Name>Canada</Name>
                </CountryOfIssue>
                <Type>
                  <Code>COC</Code>
                  <Description>Customs Office Code</Description>
                </Type>
                <Value>923</Value>
              </RegistrationNumber>
              <RegistrationNumber>
                <CountryOfIssue>
                  <Code>US</Code>
                  <Name>United States</Name>
                </CountryOfIssue>
                <Type>
                  <Code>CCC</Code>
                  <Description>Standard Carrier Alpha Code</Description>
                </Type>
                <Value>jahd</Value>
              </RegistrationNumber>
            </RegistrationNumberCollection>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty2</AddressType>
            <AddressShortCode>PST: 226 COMMONWEALTH STR</AddressShortCode>
            <OrganizationCode>CABINTSYD</OrganizationCode>
            <Address1>226 COMMONWEALTH STREET</Address1>
            <Address2>SURRY HILLS  QLD</Address2>
            <AddressOverride>false</AddressOverride>
            <City>SYDNEY</City>
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
            <State>NSW</State>
          </OrganizationAddress>
          <OrganizationAddress>
            <AddressType>NotifyParty</AddressType>
            <Address1>234 WHERE ST</Address1>
            <Address2></Address2>
            <AddressOverride>true</AddressOverride>
            <City>MELBOURNE</City>
            <CompanyName>DA BOOK PTY LTD</CompanyName>
            <Contact></Contact>
            <Country>
              <Code>AU</Code>
              <Name>Australia</Name>
            </Country>
            <Email></Email>
            <Fax></Fax>
            <GovRegNum>NK32342</GovRegNum>
            <GovRegNumType>
              <Code>PAS</Code>
              <Description>Passport Number</Description>
            </GovRegNumType>
            <Mobile></Mobile>
            <Phone></Phone>
            <Postcode>3020</Postcode>
            <ScreeningStatus>
              <Code>UNK</Code>
              <Description>Unknown</Description>
            </ScreeningStatus>
            <State>VIC</State>
          </OrganizationAddress>
        </OrganizationAddressCollection>

        <PackingLineCollection>
          <PackingLine>
            <CountryOfOrigin>
              <Code>AU</Code>
              <Name>Australia</Name>
            </CountryOfOrigin>
            <GoodsDescription>GOODS AND MORE GOODS</GoodsDescription>
            <HarmonisedCode>100000</HarmonisedCode>
            <MarksAndNos>MARKS AND MORE MARKS</MarksAndNos>
            <PackQty>100</PackQty>
            <PackType>
              <Code>KG</Code>
              <Description>Keg</Description>
            </PackType>
            <Volume>10.000</Volume>
            <VolumeUnit>
              <Code>MT</Code>
              <Description>Cubic Meter</Description>
            </VolumeUnit>
            <Weight>1000.500</Weight>
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
        </DataContext>

        <CommercialInfo>
          <CommercialChargeCollection>
            <CommercialCharge>
              <ChargeType>
                <Code>OFT</Code>
                <Description>Overseas Freight</Description>
              </ChargeType>
              <Amount>0.0000</Amount>
            </CommercialCharge>
          </CommercialChargeCollection>
        </CommercialInfo>
        <PortOfDestination>
          <Code>JPTKY</Code>
          <Name>Tokuyama</Name>
        </PortOfDestination>
        <PortOfOrigin>
          <Code>AUADL</Code>
          <Name>Adelaide</Name>
        </PortOfOrigin>
        <TotalWeight>0.000</TotalWeight>
        <TotalWeightUnit>
          <Code></Code>
        </TotalWeightUnit>
        <WayBillNumber>XXXG323222323</WayBillNumber>
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

        <PackingLineCollection>
          <PackingLine>
            <CountryOfOrigin>
              <Code>NZ</Code>
              <Name>New Zealand</Name>
            </CountryOfOrigin>
            <GoodsDescription>GOODS</GoodsDescription>
            <HarmonisedCode>320230</HarmonisedCode>
            <MarksAndNos>GOOS</MarksAndNos>
            <PackQty>3000</PackQty>
            <PackType>
              <Code>DJ</Code>
              <Description>DemiJohn, non-protected</Description>
            </PackType>
            <Volume>0.000</Volume>
            <VolumeUnit>
              <Code></Code>
            </VolumeUnit>
            <Weight>0.000</Weight>
            <WeightUnit>
              <Code></Code>
            </WeightUnit>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>
";
			}
		}

		protected override RecipientRoleType[] SupportedRecipientRoleTypes
		{
			get { return System.Array.Empty<RecipientRoleType>(); }
		}
	}
}
