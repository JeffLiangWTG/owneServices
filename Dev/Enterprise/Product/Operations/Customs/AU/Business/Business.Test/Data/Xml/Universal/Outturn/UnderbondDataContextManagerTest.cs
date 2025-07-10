using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.IO;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using NUnit.Framework;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(UnderbondDataContextManager))]
	sealed class UnderbondDataContextManagerTest : ShipmentDataContextManagerTestCase<UnderbondDataContextManager, CusUnderbond>
	{
		public void TestProperties()
		{
			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_SendersMessageReference = "123";
			var manager = new UnderbondDataContextManager();
			AssertEquals(DataContextType.UnderBond, manager.DataContextType);
			AssertType<UnderbondDataContextManager>(underbond.GetUniversalDataContextManager());
			AssertEquals("123", underbond.GetUniversalDataContextManager().DataContextKey);
			AssertEquals("", manager.DefaultOutputDirectory);
			AssertEquals(true, manager.ManagesShipments);
			AssertEquals(false, manager.ManagesEvents);
		}

		public void TestGetShipmentDataObjectReader()
		{
			const string dataTarget = "Underbond";
			UnderbondDataContextManagerForTest manager = new UnderbondDataContextManagerForTest();
			var logger = new DummyLogger();
			SetupDataForDataContextManagerTestCase();

			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			var testXML = CreateTestXMLForGetShipmentDataObjectReader(transportMode: Core.Constants.TransportModes.Air, recipientRole: ("COA", "Customs Outturn Agent", string.Empty), dataTarget: dataTarget);
			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(testXML)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
			}
			var reader = manager.GetShipmentDataObjectReaderForTest(shipment, logger, Factory);
			AssertNotNull("Reader should not be null when there is a RecipientRole of 'COA' and transport context = 'Air'", reader);

			shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			testXML = CreateTestXMLForGetShipmentDataObjectReader(transportMode: Core.Constants.TransportModes.Air, recipientRole: ("COA", "Customs Outturn Agent", Core.Constants.TransportModes.Sea), dataTarget: dataTarget);
			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(testXML)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
			}
			reader = manager.GetShipmentDataObjectReaderForTest(shipment, logger, Factory);
			AssertNull("Reader should be null when there is a RecipientRole of 'COA' and transport context = 'Air', but RecipientRole's ServiceCode override transport context to 'Sea'", reader);

			shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			testXML = CreateTestXMLForGetShipmentDataObjectReader(transportMode: Core.Constants.TransportModes.Sea, recipientRole: ("COA", "Customs Outturn Agent", string.Empty), dataTarget: dataTarget);
			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(testXML)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
			}
			reader = manager.GetShipmentDataObjectReaderForTest(shipment, logger, Factory);
			AssertNull("Reader should be null when there is a RecipientRole of 'COA', but transport context is not 'Air'", reader);

			shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			testXML = CreateTestXMLForGetShipmentDataObjectReader(transportMode: Core.Constants.TransportModes.Sea, recipientRole: ("COA", "Customs Outturn Agent", Core.Constants.TransportModes.Air), dataTarget: dataTarget);
			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(testXML)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
			}
			reader = manager.GetShipmentDataObjectReaderForTest(shipment, logger, Factory);
			AssertNotNull("Reader should not be null when there is a RecipientRole of 'COA', but transport context is not 'Air', but RecipientRole's ServiceCode override transport context to 'Air'", reader);

			shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			testXML = CreateTestXMLForGetShipmentDataObjectReader(transportMode: Core.Constants.TransportModes.Sea, recipientRole: null, dataTarget: dataTarget);
			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(testXML)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
			}
			reader = manager.GetShipmentDataObjectReaderForTest(shipment, logger, Factory);
			AssertNotNull("Reader should not be null when there is not a RecipientRole of 'COA'", reader);
		}

		string CreateTestXMLForGetShipmentDataObjectReader(string transportMode, (string code, string description, string serviceCode)? recipientRole = null, string dataTarget = null)
		{
			var template = new StringBuilder(@"
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingConsol</Type>
          <Key>C001</Key>
        </DataSource>
      </DataSourceCollection>");

			if (!string.IsNullOrEmpty(dataTarget))
			{
				template.Append(@$"
      <DataTargetCollection>
        <DataTarget>
          <Type>{dataTarget}</Type>
        </DataTarget>
      </DataTargetCollection>");
			}

			if (recipientRole != null)
			{
				template.Append(@$"
      <RecipientRoleCollection>
        <RecipientRole>
          <Code>{recipientRole?.code}</Code>
          <Description>{recipientRole?.description}</Description>");
				if (!string.IsNullOrEmpty(recipientRole?.serviceCode)) {
					template.Append(@$"
          <ServiceCode>{recipientRole?.serviceCode}</ServiceCode>");
				}
				template.Append(@$"
        </RecipientRole>
      </RecipientRoleCollection>");
			}

			template.Append(@$"
    </DataContext>
    <TransportMode>
      <Code>{transportMode}</Code>
      <Description>{transportMode}</Description>
    </TransportMode>
  </Shipment>
</UniversalShipment>
");
			return template.ToString();
		}

		public void TestExportData_SubShipment()
		{
			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_MAWB = "OB1";
			var outturn1 = Factory.New<CusOutturn>();
			outturn1.C5_HouseBill = "HB1";
			var outturn2 = Factory.New<CusOutturn>();
			outturn2.C5_HouseBill = "HB2";
			underbond.Outturns.AddRange(outturn1, outturn2);
			IShipmentDataContextManager manager = new UnderbondDataContextManager();
			var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.COA, underbond)));
			var underbondData = (UniversalShipment)writer.GetDataObject(underbond);
			AssertEquals("underbondData.DataContext.DataSourceCollection.Count", 1, underbondData.DataContext.DataSourceCollection.Count());
			AssertNotNull("Should have UnderBond", underbondData.GetMatchingDataSource(DataContextType.UnderBond));
			AssertEquals("underbondData.WayBillNumber", "OB1", underbondData.WayBillNumber);
			AssertEquals("underbondData.SubShipmentCollection.Count", 2, underbondData.SubShipmentCollection.Count);
			var outturn1Data = underbondData.SubShipmentCollection[0];
			var outturn2Data = underbondData.SubShipmentCollection[1];
			if (outturn2Data.WayBillNumber.GetValueOrDefault() == "HB1")
			{
				outturn1Data = underbondData.SubShipmentCollection[1];
				outturn2Data = underbondData.SubShipmentCollection[0];
			}
			AssertEquals("outturn1Data.WayBillNumber", "HB1", outturn1Data.WayBillNumber);
			AssertNull("outturn1Data.SubShipmentCollection", outturn1Data.SubShipmentCollection);
			AssertEquals("outturn2Data.WayBillNumber", "HB2", outturn2Data.WayBillNumber);
			AssertNull("outturn2Data.SubShipmentCollection", outturn2Data.SubShipmentCollection);
		}

		public void TestEtailReaderIsCalled()
		{
			UnderbondDataContextManagerForTest manager = new UnderbondDataContextManagerForTest();
			var logger = new DummyLogger();
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			SetupDataForDataContextManagerTestCase();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(ValidPopulatedUniversalShipmentXML)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
			}

			var reader = manager.GetShipmentDataObjectReaderForTest(shipment, logger, Factory);
			Assert("Etail Reader should be called", reader.GetType() == typeof(ETailCusUnderbondDataObjectReader));
		}

		public void TestNonEtailReaderIsCalled()
		{
			UnderbondDataContextManagerForTest manager = new UnderbondDataContextManagerForTest();
			var logger = new DummyLogger();
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			SetupDataForDataContextManagerTestCase();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(NonEtailUniversalShipmentXML)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
			}

			var reader = manager.GetShipmentDataObjectReaderForTest(shipment, logger, Factory);
			Assert("Etail Reader should be called", reader.GetType() == typeof(CusUnderbondDataObjectReader));
		}

		public void TestTransitReaderIsCalled()
		{
			UnderbondDataContextManagerForTest manager = new UnderbondDataContextManagerForTest();
			var logger = new DummyLogger();
			var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);

			SetupDataForDataContextManagerTestCase();

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(TransitUniversalShipmentXML)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
			}

			var reader = manager.GetShipmentDataObjectReaderForTest(shipment, logger, Factory);
			Assert("Transit Reader should be called", reader.GetType() == typeof(CusUnderbondDataObjectReaderForTransitWarehouse));
		}

		protected override RecipientRoleType[] SupportedRecipientRoleTypes => new[] { RecipientRoleType.COA };

		protected override void SetupDataForDataContextManagerTestCase()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C001";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S001";
			shipment.JS_HouseBill = "02";

			var cusMAWB = Factory.New<CusMAWB>();
			cusMAWB.CM_MAWB = "123456";
			cusMAWB.CM_MasterHouseBill = "02";
			cusMAWB.Underbonds.AddNew();

			Factory.SaveForTesting();
		}

		protected override string ValidPopulatedUniversalShipmentXML =>
	@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingConsol</Type>
          <Key>C001</Key>
        </DataSource>
        <DataSource>
          <Type>ForwardingShipment</Type>
          <Key>S001</Key>
        </DataSource>
      </DataSourceCollection>

      <ActionPurpose>
        <Code>APP</Code>
        <Description>As Per Payload</Description>
      </ActionPurpose>
      <Company>
        <Code>DAU</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Your Australia Demo Company</Name>
      </Company>
      <DataProvider>HYECMTDAU</DataProvider>
      <EnterpriseID>HYE</EnterpriseID>
      <EventBranch>
        <Code>SYD</Code>
        <Name>Sydney, Australia</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code>ADD</Code>
        <Description>Added a record to the system</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>CMT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2017-08-31T18:09:02.823</TriggerDate>
      <TriggerDescription>1234</TriggerDescription>
      <TriggerType>Trigger</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>COA</Code>
          <Description>Customs Outturn Agent</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <EntryStatus>
      <Code></Code>
      <Description></Description>
    </EntryStatus>
    <ShipmentType>
      <Code>DCL</Code>
      <Description>DCL</Description>
    </ShipmentType>
    <TotalNoOfPieces>23</TotalNoOfPieces>
    <TransportMode>
      <Code>AIR</Code>
      <Description>AIR</Description>
    </TransportMode>
    <VoyageFlightNo>111324</VoyageFlightNo>
    <WayBillNumber>123456</WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>

    <AddInfoCollection>
      <AddInfo>
        <Key>UnderbondBySeaVoyage</Key>
        <Value>13454555</Value>
      </AddInfo>
      <AddInfo>
        <Key>IsMoveFromDischarge</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>UnderbondBySeaVessel</Key>
        <Value>23</Value>
      </AddInfo>
    </AddInfoCollection>

    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type>
          <Code>DCP</Code>
          <Description>Discharge Premise ID</Description>
        </Type>
        <ReferenceNumber></ReferenceNumber>
        <ContextInformation>AU</ContextInformation>
      </AdditionalReference>
      <AdditionalReference>
        <Type>
          <Code>OGP</Code>
          <Description>Origin Premise ID</Description>
        </Type>
        <ReferenceNumber>J8978</ReferenceNumber>
        <ContextInformation>AU</ContextInformation>
      </AdditionalReference>
      <AdditionalReference>
        <Type>
          <Code>DSP</Code>
          <Description>Destination Premise ID</Description>
        </Type>
        <ReferenceNumber>9914N</ReferenceNumber>
        <ContextInformation>AU</ContextInformation>
      </AdditionalReference>
      <AdditionalReference>
        <Type>
          <Code>RPI</Code>
          <Description>Responsible Party ID</Description>
        </Type>
        <ReferenceNumber>41065894724/001</ReferenceNumber>
        <ContextInformation>AU</ContextInformation>
      </AdditionalReference>
    </AdditionalReferenceCollection>

    <DateCollection>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2017-08-31T00:00:00</Value>
      </Date>
      <Date>
        <Type>Pack</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2017-08-31T18:10:00</Value>
      </Date>
    </DateCollection>

    <EntryNumberCollection>
      <EntryNumber>
        <Type>
          <Code>UBM</Code>
          <Description>Underbond Status</Description>
        </Type>
        <CountryOfIssue>
          <Code>AU</Code>
        </CountryOfIssue>
        <EntryStatus>
          <Code>NOT</Code>
          <Description>Not Sent</Description>
        </EntryStatus>
      </EntryNumber>
    </EntryNumberCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>OriginAddress</AddressType>
        <Address1>RSD 85</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>PST: RSD 85</AddressShortCode>
        <City>STRATHALBYN</City>
        <CompanyName>ASET SERVICES</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email></Email>
        <Fax>+61885375016</Fax>
        <OrganizationCode>ASETADL</OrganizationCode>
        <Phone>+61885375020</Phone>
        <Port>
          <Code>AUADL</Code>
          <Name>Adelaide</Name>
        </Port>
        <Postcode>5255</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>SA</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>CCP</Code>
              <Description>Customs Controlled Premises Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>AU</Code>
              <Name>Australia</Name>
            </CountryOfIssue>
            <Value>J8978</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>CID</Code>
              <Description>CCID Customs Client Identifier</Description>
            </Type>
            <CountryOfIssue>
              <Code>AU</Code>
              <Name>Australia</Name>
            </CountryOfIssue>
            <Value>AAA3366797R</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <SubShipmentCollection>
      <SubShipment>
        <DataContext>
          <DataSourceCollection>
            <DataSource>
              <Type>ForwardingShipment</Type>
              <Key>0123456</Key>
            </DataSource>
          </DataSourceCollection>
        </DataContext>

        <EntryStatus>
          <Code></Code>
          <Description></Description>
        </EntryStatus>
        <OuterPacks>0</OuterPacks>
        <OuterPacksPackageType>
          <Code></Code>
          <Description></Description>
        </OuterPacksPackageType>
        <ShipmentType>
          <Code>HVL</Code>
          <Description>HVLV</Description>
        </ShipmentType>
        <TotalNoOfPacks>1</TotalNoOfPacks>
        <WayBillNumber>123</WayBillNumber>
        <WayBillType>
          <Code>HWB</Code>
          <Description>House Waybill</Description>
        </WayBillType>

        <AddInfoCollection>
          <AddInfo>
            <Key>IsDamage</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>IsPillage</Key>
            <Value>N</Value>
          </AddInfo>
        </AddInfoCollection>

        <AdditionalReferenceCollection>
          <AdditionalReference>
            <Type>
              <Code>RPI</Code>
              <Description>RPI</Description>
            </Type>
            <ReferenceNumber>41065894724/001</ReferenceNumber>
            <ContextInformation>AU</ContextInformation>
          </AdditionalReference>
        </AdditionalReferenceCollection>

        <NoteCollection>
          <Note>
            <Description>GoodsDescription</Description>
            <IsCustomDescription>true</IsCustomDescription>
            <NoteText>11</NoteText>
          </Note>
        </NoteCollection>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>
";

		protected override void ConfigureDataContext(IDataContextDataObject dataContext)
		{
			base.ConfigureDataContext(dataContext);
			if (dataContext.RecipientRoleCollection == null)
			{
				dataContext.RecipientRoleCollection = new[]
				{
					new RecipientRole() { Code = RecipientRoleType.COA }
				};
			}
			else if (!dataContext.RecipientRoleCollection.Any(rr => rr.Code == RecipientRoleType.COA))
			{
				dataContext.RecipientRoleCollection.Append(new RecipientRole() { Code = RecipientRoleType.COA });
			}
		}

		const string NonEtailUniversalShipmentXML =
			@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>ForwardingConsol</Type>
          <Key>C001</Key>
        </DataSource>
        <DataSource>
          <Type>ForwardingShipment</Type>
          <Key>S001</Key>
        </DataSource>
      </DataSourceCollection>

      <ActionPurpose>
        <Code>APP</Code>
        <Description>As Per Payload</Description>
      </ActionPurpose>
      <Company>
        <Code>DAU</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Your Australia Demo Company</Name>
      </Company>
      <DataProvider>HYECMTDAU</DataProvider>
      <EnterpriseID>HYE</EnterpriseID>
      <EventBranch>
        <Code>SYD</Code>
        <Name>Sydney, Australia</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code>ADD</Code>
        <Description>Added a record to the system</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>CMT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2017-08-31T18:09:02.823</TriggerDate>
      <TriggerDescription>1234</TriggerDescription>
      <TriggerType>Trigger</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>COA</Code>
          <Description>Customs Outturn Agent</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <EntryStatus>
      <Code></Code>
      <Description></Description>
    </EntryStatus>
    <ShipmentType>
      <Code>DCL</Code>
      <Description>DCL</Description>
    </ShipmentType>
    <TotalNoOfPieces>23</TotalNoOfPieces>
    <TransportMode>
      <Code>AIR</Code>
      <Description>AIR</Description>
    </TransportMode>
    <VoyageFlightNo>111324</VoyageFlightNo>
    <WayBillNumber>123456</WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>

    <AddInfoCollection>
      <AddInfo>
        <Key>UnderbondBySeaVoyage</Key>
        <Value>13454555</Value>
      </AddInfo>
      <AddInfo>
        <Key>IsMoveFromDischarge</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>UnderbondBySeaVessel</Key>
        <Value>23</Value>
      </AddInfo>
    </AddInfoCollection>

    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type>
          <Code>DCP</Code>
          <Description>Discharge Premise ID</Description>
        </Type>
        <ReferenceNumber></ReferenceNumber>
        <ContextInformation>AU</ContextInformation>
      </AdditionalReference>
      <AdditionalReference>
        <Type>
          <Code>OGP</Code>
          <Description>Origin Premise ID</Description>
        </Type>
        <ReferenceNumber>J8978</ReferenceNumber>
        <ContextInformation>AU</ContextInformation>
      </AdditionalReference>
      <AdditionalReference>
        <Type>
          <Code>DSP</Code>
          <Description>Destination Premise ID</Description>
        </Type>
        <ReferenceNumber>9914N</ReferenceNumber>
        <ContextInformation>AU</ContextInformation>
      </AdditionalReference>
      <AdditionalReference>
        <Type>
          <Code>RPI</Code>
          <Description>Responsible Party ID</Description>
        </Type>
        <ReferenceNumber>41065894724/001</ReferenceNumber>
        <ContextInformation>AU</ContextInformation>
      </AdditionalReference>
    </AdditionalReferenceCollection>

    <DateCollection>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2017-08-31T00:00:00</Value>
      </Date>
      <Date>
        <Type>Pack</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2017-08-31T18:10:00</Value>
      </Date>
    </DateCollection>

    <EntryNumberCollection>
      <EntryNumber>
        <Type>
          <Code>UBM</Code>
          <Description>Underbond Status</Description>
        </Type>
        <CountryOfIssue>
          <Code>AU</Code>
        </CountryOfIssue>
        <EntryStatus>
          <Code>NOT</Code>
          <Description>Not Sent</Description>
        </EntryStatus>
      </EntryNumber>
    </EntryNumberCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>OriginAddress</AddressType>
        <Address1>RSD 85</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>PST: RSD 85</AddressShortCode>
        <City>STRATHALBYN</City>
        <CompanyName>ASET SERVICES</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email></Email>
        <Fax>+61885375016</Fax>
        <OrganizationCode>ASETADL</OrganizationCode>
        <Phone>+61885375020</Phone>
        <Port>
          <Code>AUADL</Code>
          <Name>Adelaide</Name>
        </Port>
        <Postcode>5255</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>SA</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>CCP</Code>
              <Description>Customs Controlled Premises Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>AU</Code>
              <Name>Australia</Name>
            </CountryOfIssue>
            <Value>J8978</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>CID</Code>
              <Description>CCID Customs Client Identifier</Description>
            </Type>
            <CountryOfIssue>
              <Code>AU</Code>
              <Name>Australia</Name>
            </CountryOfIssue>
            <Value>AAA3366797R</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <SubShipmentCollection>
      <SubShipment>
        <DataContext>
          <DataSourceCollection>
            <DataSource>
              <Type>ForwardingShipment</Type>
              <Key>0123456</Key>
            </DataSource>
          </DataSourceCollection>
        </DataContext>

        <EntryStatus>
          <Code></Code>
          <Description></Description>
        </EntryStatus>
        <OuterPacks>0</OuterPacks>
        <OuterPacksPackageType>
          <Code></Code>
          <Description></Description>
        </OuterPacksPackageType>
        <ShipmentType>
          <Code>TTT</Code>
          <Description>Test</Description>
        </ShipmentType>
        <TotalNoOfPacks>1</TotalNoOfPacks>
        <WayBillNumber>123</WayBillNumber>
        <WayBillType>
          <Code>HWB</Code>
          <Description>House Waybill</Description>
        </WayBillType>

        <AddInfoCollection>
          <AddInfo>
            <Key>IsDamage</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>IsPillage</Key>
            <Value>N</Value>
          </AddInfo>
        </AddInfoCollection>

        <AdditionalReferenceCollection>
          <AdditionalReference>
            <Type>
              <Code>RPI</Code>
              <Description>RPI</Description>
            </Type>
            <ReferenceNumber>41065894724/001</ReferenceNumber>
            <ContextInformation>AU</ContextInformation>
          </AdditionalReference>
        </AdditionalReferenceCollection>

        <NoteCollection>
          <Note>
            <Description>GoodsDescription</Description>
            <IsCustomDescription>true</IsCustomDescription>
            <NoteText>11</NoteText>
          </Note>
        </NoteCollection>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>
";

		const string TransitUniversalShipmentXML =
			@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>TransitReceive</Type>
          <Key>S001</Key>
        </DataSource>
      </DataSourceCollection>

      <ActionPurpose>
        <Code>APP</Code>
        <Description>As Per Payload</Description>
      </ActionPurpose>
      <Company>
        <Code>DAU</Code>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Name>Your Australia Demo Company</Name>
      </Company>
      <DataProvider>HYECMTDAU</DataProvider>
      <EnterpriseID>HYE</EnterpriseID>
      <EventBranch>
        <Code>SYD</Code>
        <Name>Sydney, Australia</Name>
      </EventBranch>
      <EventDepartment>
        <Code>BRN</Code>
        <Name>Branch</Name>
      </EventDepartment>
      <EventType>
        <Code>ADD</Code>
        <Description>Added a record to the system</Description>
      </EventType>
      <EventUser>
        <Code>E</Code>
        <Name>CargoWise Support</Name>
      </EventUser>
      <ServerID>CMT</ServerID>
      <TriggerCount>1</TriggerCount>
      <TriggerDate>2017-08-31T18:09:02.823</TriggerDate>
      <TriggerDescription>1234</TriggerDescription>
      <TriggerType>Trigger</TriggerType>

      <RecipientRoleCollection>
        <RecipientRole>
          <Code>COA</Code>
          <Description>Customs Outturn Agent</Description>
        </RecipientRole>
      </RecipientRoleCollection>
    </DataContext>

    <EntryStatus>
      <Code></Code>
      <Description></Description>
    </EntryStatus>
    <ShipmentType>
      <Code>DCL</Code>
      <Description>DCL</Description>
    </ShipmentType>
    <TotalNoOfPieces>23</TotalNoOfPieces>
    <TransportMode>
      <Code>AIR</Code>
      <Description>AIR</Description>
    </TransportMode>
    <VoyageFlightNo>111324</VoyageFlightNo>
    <WayBillNumber>123456</WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>

    <AddInfoCollection>
      <AddInfo>
        <Key>UnderbondBySeaVoyage</Key>
        <Value>13454555</Value>
      </AddInfo>
      <AddInfo>
        <Key>IsMoveFromDischarge</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>UnderbondBySeaVessel</Key>
        <Value>23</Value>
      </AddInfo>
    </AddInfoCollection>

    <AdditionalReferenceCollection>
      <AdditionalReference>
        <Type>
          <Code>DCP</Code>
          <Description>Discharge Premise ID</Description>
        </Type>
        <ReferenceNumber></ReferenceNumber>
        <ContextInformation>AU</ContextInformation>
      </AdditionalReference>
      <AdditionalReference>
        <Type>
          <Code>OGP</Code>
          <Description>Origin Premise ID</Description>
        </Type>
        <ReferenceNumber>J8978</ReferenceNumber>
        <ContextInformation>AU</ContextInformation>
      </AdditionalReference>
      <AdditionalReference>
        <Type>
          <Code>DSP</Code>
          <Description>Destination Premise ID</Description>
        </Type>
        <ReferenceNumber>9914N</ReferenceNumber>
        <ContextInformation>AU</ContextInformation>
      </AdditionalReference>
      <AdditionalReference>
        <Type>
          <Code>RPI</Code>
          <Description>Responsible Party ID</Description>
        </Type>
        <ReferenceNumber>41065894724/001</ReferenceNumber>
        <ContextInformation>AU</ContextInformation>
      </AdditionalReference>
    </AdditionalReferenceCollection>

    <DateCollection>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2017-08-31T00:00:00</Value>
      </Date>
      <Date>
        <Type>Pack</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2017-08-31T18:10:00</Value>
      </Date>
    </DateCollection>

    <EntryNumberCollection>
      <EntryNumber>
        <Type>
          <Code>UBM</Code>
          <Description>Underbond Status</Description>
        </Type>
        <CountryOfIssue>
          <Code>AU</Code>
        </CountryOfIssue>
        <EntryStatus>
          <Code>NOT</Code>
          <Description>Not Sent</Description>
        </EntryStatus>
      </EntryNumber>
    </EntryNumberCollection>

    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>OriginAddress</AddressType>
        <Address1>RSD 85</Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode>PST: RSD 85</AddressShortCode>
        <City>STRATHALBYN</City>
        <CompanyName>ASET SERVICES</CompanyName>
        <Country>
          <Code>AU</Code>
          <Name>Australia</Name>
        </Country>
        <Email></Email>
        <Fax>+61885375016</Fax>
        <OrganizationCode>ASETADL</OrganizationCode>
        <Phone>+61885375020</Phone>
        <Port>
          <Code>AUADL</Code>
          <Name>Adelaide</Name>
        </Port>
        <Postcode>5255</Postcode>
        <ScreeningStatus>
          <Code>UNK</Code>
          <Description>Unknown</Description>
        </ScreeningStatus>
        <State>SA</State>

        <RegistrationNumberCollection>
          <RegistrationNumber>
            <Type>
              <Code>CCP</Code>
              <Description>Customs Controlled Premises Code</Description>
            </Type>
            <CountryOfIssue>
              <Code>AU</Code>
              <Name>Australia</Name>
            </CountryOfIssue>
            <Value>J8978</Value>
          </RegistrationNumber>
          <RegistrationNumber>
            <Type>
              <Code>CID</Code>
              <Description>CCID Customs Client Identifier</Description>
            </Type>
            <CountryOfIssue>
              <Code>AU</Code>
              <Name>Australia</Name>
            </CountryOfIssue>
            <Value>AAA3366797R</Value>
          </RegistrationNumber>
        </RegistrationNumberCollection>
      </OrganizationAddress>
    </OrganizationAddressCollection>

    <SubShipmentCollection>
      <SubShipment>
        <DataContext>
          <DataSourceCollection>
            <DataSource>
              <Type>ForwardingShipment</Type>
              <Key>0123456</Key>
            </DataSource>
          </DataSourceCollection>
        </DataContext>

        <EntryStatus>
          <Code></Code>
          <Description></Description>
        </EntryStatus>
        <OuterPacks>0</OuterPacks>
        <OuterPacksPackageType>
          <Code></Code>
          <Description></Description>
        </OuterPacksPackageType>
        <ShipmentType>
          <Code>HVL</Code>
          <Description>HVLV</Description>
        </ShipmentType>
        <TotalNoOfPacks>1</TotalNoOfPacks>
        <WayBillNumber>123</WayBillNumber>
        <WayBillType>
          <Code>HWB</Code>
          <Description>House Waybill</Description>
        </WayBillType>

        <AddInfoCollection>
          <AddInfo>
            <Key>IsDamage</Key>
            <Value>N</Value>
          </AddInfo>
          <AddInfo>
            <Key>IsPillage</Key>
            <Value>N</Value>
          </AddInfo>
        </AddInfoCollection>

        <AdditionalReferenceCollection>
          <AdditionalReference>
            <Type>
              <Code>RPI</Code>
              <Description>RPI</Description>
            </Type>
            <ReferenceNumber>41065894724/001</ReferenceNumber>
            <ContextInformation>AU</ContextInformation>
          </AdditionalReference>
        </AdditionalReferenceCollection>

        <NoteCollection>
          <Note>
            <Description>GoodsDescription</Description>
            <IsCustomDescription>true</IsCustomDescription>
            <NoteText>11</NoteText>
          </Note>
        </NoteCollection>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>
";

		sealed class UnderbondDataContextManagerForTest : UnderbondDataContextManager
		{
			public ITopLevelDataObjectReader GetShipmentDataObjectReaderForTest(UniversalShipment universalShipment, IXmlImportLogger logger, UniversalObjectFactory factory)
				=> GetShipmentDataObjectReader(universalShipment, logger, factory);
		}
	}
}
