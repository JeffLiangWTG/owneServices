using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
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
	[TestedType(typeof(CMVEventProcessor))]
	class CMVEventProcessorTest : AFREventProcessorAbstractTest<CMVEventProcessor>
	{
		protected override CMVEventProcessor GetNewProcessor(IXmlEventValueObject eventDataObject, IXmlImportLogger logger, BusinessObjectFactory factory)
		{
			return new CMVEventProcessor(eventDataObject, logger, factory);
		}

		public void TestCMVEventProcessor()
		{
			var statuses = new AFRBillCustomsStatusList();
			statuses.AddPair("", "");
			foreach (ICodeDescription pair in statuses)
			{
				TestCMVEventProcessorCore(true, pair.Code, CMVEventProcessor.GetNewReleaseStatus(pair.Code));
				TestCMVEventProcessorCore(false, pair.Code, CMVEventProcessor.GetNewReleaseStatus(pair.Code));
			}
		}

		public void TestCMVEventProcessorCore(ZBool blanketChange, ZString oldReleaseStatus, ZString expectedReleaseStatus)
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

			const string CMVResponseXML = @"
<UniversalEvent>
	<Event>
		<DataContext>
			<ActionPurpose>
				<Code>SMV</Code>
				<Description>SMV</Description>
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
		<EventReference>SMV-ACCEPTED</EventReference>
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
				<Type>NotificationDetails</Type>
				<Value>{0}</Value>
			</Context>
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

			const string notificationDetails = @"&lt;table width=""100%"" border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""&gt;&lt;tr&gt;&lt;td&gt;Process Result&lt;/td&gt;&lt;td&gt;W1000-0000-0000&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Vessel Call Sign&lt;/td&gt;&lt;td&gt;PCAM&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Voyage Number&lt;/td&gt;&lt;td&gt;1&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Carrier Code&lt;/td&gt;&lt;td&gt;SAPC&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Port of Loading&lt;/td&gt;&lt;td&gt;USLAX&lt;/td&gt;&lt;/tr&gt;&lt;tr&gt;&lt;td&gt;Port of Loading Suffix&lt;/td&gt;&lt;td&gt;1&lt;/td&gt;&lt;/tr&gt;&lt;/table&gt;";
			var xmlEvent = new XmlEventDeserializer().Parse(string.Format(CMVResponseXML, notificationDetails, cmvMsg.EM_MessageNum));

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
			if (oldReleaseStatus != expectedReleaseStatus)
			{
				sb.Append($"Information - The Bill '{bill1.JPB_BillNumber}' Customs Status of AFR Job '{header.JPH_JobReference}' changes from '{oldReleaseStatus}' to '{expectedReleaseStatus}'.");
				sb.Append($"Information - The Bill '{bill2.JPB_BillNumber}' Customs Status of AFR Job '{header.JPH_JobReference}' changes from '{oldReleaseStatus}' to '{expectedReleaseStatus}'.");
			}

			CombineAssertions($"{header.JPH_JobReference}-{blanketChange}-{oldReleaseStatus}-{expectedReleaseStatus}", () =>
			{
				AssertEquals(expectedReleaseStatus, bill1.JPB_ReleaseStatus);
				AssertEquals(expectedReleaseStatus, bill2.JPB_ReleaseStatus);
				AssertEquals("LogParent", 1, logParents.Length);
				AssertEquals("LogParentPK", header.PK, logParents[0].PK);
				AssertEquals(sb.ToStringWithNewLineBetweenAppends(), logger.Logs);
				AssertHasEmail(MessagingTypeList.Descriptions.BlanketVesselChangeResponseReceived + " Response for " + header.JPH_JobReference, "<table width=\"100%\" border=\"1\" cellpadding=\"1\" cellspacing=\"0\" class=\"table\"><tr><td>Process Result</td><td>W1000-0000-0000</td></tr><tr><td>Vessel Call Sign</td><td>PCAM</td></tr><tr><td>Voyage Number</td><td>1</td></tr><tr><td>Carrier Code</td><td>SAPC</td></tr><tr><td>Port of Loading</td><td>USLAX</td></tr><tr><td>Port of Loading Suffix</td><td>1</td></tr></table", Staff2.GS_EmailAddress);
			});

			header.Messages.RemoveAndDeleteAllFromTest();
			header.Delete();
			testVessel.Delete();
			Factory.SaveForTesting();
		}
	}
}
