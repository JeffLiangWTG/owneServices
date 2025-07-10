using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.JP.AFR;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal.Testing
{
	class AFRMessageGeneratorTest : TestCaseWithFactory
	{
		public void TestSendCMVToCustoms()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "VESSEL";
			vessel.RV_RadioCallSign = "12345";
			vessel.RV_RN_NKCountryOfReg = "CN";

			var portOfLoading = Factory.NewWithValidTestData<RefUNLOCO>();
			portOfLoading.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			portOfLoading.RL_PortName = "LN123";
			portOfLoading.Code = "L123";

			var portOfDischarge = Factory.NewWithValidTestData<RefUNLOCO>();
			portOfDischarge.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			portOfDischarge.RL_PortName = "DN123";
			portOfDischarge.Code = "D123";

			Factory.Save();

			var header = Factory.New<JPAFRHeader>();
			header.JPH_IsShippingLineEntry = true;
			header.JPH_CarrierCode = "C123";
			header.JPH_VesselName = vessel.RV_Code;
			header.JPH_Voyage = "V123";
			header.JPH_OperationalCarrierVoyageNo = "O123";
			header.JPH_RL_NKLoading = portOfLoading.Code;
			header.JPH_LoadingPortSuffix = "1";
			header.JPH_RelaxedAppId = ZBool.True;
			header.JPH_RL_NKDischarge = portOfDischarge.Code;
			header.JPH_DischargePortSuffix = "1";
			header.JPH_ETD = new ZDateTime(2017, 5, 3);
			header.JPH_ETA = new ZDateTime(2017, 5, 3);

			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			AssertEquals(2, header.Bills.Count);

			var blanketVesselChange = new BlanketVesselChange(header);
			blanketVesselChange.Factory.Save();

			var generator = new AFRMessageGenerator(header, DefaultDataObjectWriterStrategy.Instance);
			generator.SendCMVToCustoms(MessagingTypeList.Codes.BlanketVesselChange, (ZGuid billPK, ref string functionType) =>
			{
				functionType = FunctionTypeList.Codes.BlanketVesselChange;
				return true;
			});
			AssertEquals("header.Messages.Count", 1, header.Messages.Count);
			var message = header.Messages[0];
			AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
			AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
			AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);
			AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
			AssertEquals("message.EM_MessageOwner", MessagingTypeList.Codes.BlanketVesselChange, message.EM_MessageOwner);
			AssertEquals("message.IsInDatabase", true, message.IsInDatabase);
			AssertMultilineASCIIEquals("message.EM_MessageText",
				@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>AFRHeader</Type>
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
    <LloydsIMO></LloydsIMO>
    <PortOfDischarge>
      <Code>D123</Code>
      <Name>DN123</Name>
    </PortOfDischarge>
    <PortOfLoading>
      <Code>L123</Code>
      <Name>LN123</Name>
    </PortOfLoading>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <VesselCountryOfRegistration>
      <Code>CN</Code>
      <Name>China</Name>
    </VesselCountryOfRegistration>
    <VesselName>VESSEL</VesselName>
    <VoyageFlightNo>V123</VoyageFlightNo>
    <WayBillNumber></WayBillNumber>
    <WayBillType>
      <Code>MWB</Code>
      <Description>Master Waybill</Description>
    </WayBillType>

    <AddInfoCollection>
      <AddInfo>
        <Key>JPCarrierCode</Key>
        <Value>C123</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPPortOfLoadingSuffix</Key>
        <Value>1</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPPortOfDischargeSuffix</Key>
        <Value>1</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPIsDepartureFromRelaxedArea</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPVesselCallSign</Key>
        <Value>12345</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPOperationalCarrierVoyageNo</Key>
        <Value>O123</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPCarrierCodeNew</Key>
        <Value>C123</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPVesselNameNew</Key>
        <Value>VESSEL</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPVesselCallSignNew</Key>
        <Value>12345</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPVesselCountryNew</Key>
        <Value>CN</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPVoyageNumberNew</Key>
        <Value>V123</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPOperationalCarrierVoyageNoNew</Key>
        <Value>O123</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPPortOfLoadingSuffixNew</Key>
        <Value>1</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPPortOfLoadingCodeNew</Key>
        <Value>L123</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPPortOfLoadingNameNew</Key>
        <Value>LN123</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPIsDepartureFromRelaxedAreaNew</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPEstimatedDateTimeOfDepartureNew</Key>
        <Value>2017-05-03T00:00:00</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPBlanketChange</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPInternalTransactionNumber</Key>
        <Value>JP00000001</Value>
      </AddInfo>
    </AddInfoCollection>

    <DateCollection>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2017-05-03T00:00:00</Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2017-05-03T00:00:00</Value>
      </Date>
    </DateCollection>

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
              <Amount>0</Amount>
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
        <WayBillNumber></WayBillNumber>
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
            <Key>JPMasterBillIdentifier</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPContainerOperatorCode</Key>
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
          <AddInfo>
            <Key>JPGeneralCustomsTransitApprovalNumber</Key>
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
            <DetailedDescription></DetailedDescription>
            <HarmonisedCode></HarmonisedCode>
            <MarksAndNos></MarksAndNos>
            <PackQty>0</PackQty>
            <PackType>
              <Code></Code>
            </PackType>
            <Volume>0</Volume>
            <VolumeUnit>
              <Code>M3</Code>
              <Description>Cubic Meter</Description>
            </VolumeUnit>
            <Weight>0</Weight>
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
                <Description>International Freight</Description>
              </ChargeType>
              <Amount>0</Amount>
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
        <WayBillNumber></WayBillNumber>
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
            <Key>JPMasterBillIdentifier</Key>
            <Value></Value>
          </AddInfo>
          <AddInfo>
            <Key>JPContainerOperatorCode</Key>
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
          <AddInfo>
            <Key>JPGeneralCustomsTransitApprovalNumber</Key>
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
            <DetailedDescription></DetailedDescription>
            <HarmonisedCode></HarmonisedCode>
            <MarksAndNos></MarksAndNos>
            <PackQty>0</PackQty>
            <PackType>
              <Code></Code>
            </PackType>
            <Volume>0</Volume>
            <VolumeUnit>
              <Code>M3</Code>
              <Description>Cubic Meter</Description>
            </VolumeUnit>
            <Weight>0</Weight>
            <WeightUnit>
              <Code>KG</Code>
              <Description>Kilogram</Description>
            </WeightUnit>
          </PackingLine>
        </PackingLineCollection>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>", message.EM_MessageText);
			var interchange = message.Interchange;
			AssertEquals("Interchange message should be the same as Header message", message.PK, interchange.ContainedMessages[0].PK);
			AssertEquals("interchange.EI_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, interchange.EI_ApplicationCode);
			AssertEquals("interchange.EI_InterchangeType", EDIInterchangeTypeList.Codes.XDC, interchange.EI_InterchangeType);
			AssertEquals("interchange.EI_From", "ENT", interchange.EI_From);
			AssertEquals("interchange.EI_To", "JPCustoms", interchange.EI_To);
			AssertEquals("interchange.EI_Status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
			AssertEquals("interchange.EI_TransportType", EDIInterchangeTransportTypeList.Codes.eHub, interchange.EI_TransportType);
			AssertEquals("interchange.EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
			AssertEquals("interchange.IsInDatabase", true, interchange.IsInDatabase);
			AssertMultilineASCIIEquals("MessageText should be same as BodyText", message.EM_MessageText, interchange.EI_BodyText);
		}

		public void TestMessageStatus_CMV()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_CarrierCode = "OTT1";
			header.JPH_MasterBillNumber = "OTT1MB32342";
			header.JPH_ETD = new ZDateTime(2013, 4, 19, 22, 11, 12);
			header.JPH_LoadingPortSuffix = "8";
			header.JPH_ETA = new ZDateTime(2014, 5, 20, 23, 12, 13);
			header.JPH_DischargePortSuffix = "9";
			header.JPH_IsShippingLineEntry = true;
			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "SDSDHB323422";
			var bill2 = header.Bills.AddNew();
			bill2.JPB_BillNumber = "SDAB99546552";
			Factory.Save();
			var generator = new AFRMessageGenerator(header, DefaultDataObjectWriterStrategy.Instance);
			generator.SendCMVToCustoms(
				MessagingTypeList.Codes.BlanketVesselChange,
				(ZGuid billPK, ref string functionType) =>
				{
					functionType = FunctionTypeList.Codes.BlanketVesselChange;
					return true;
				});

			CombineAssertions(() =>
			{
				AssertEquals(MessageStatusList.Codes.AwaitingBlanketVesselChangeCompletion, bill1.JPB_MessageStatus);
				AssertEquals(MessageStatusList.Codes.AwaitingBlanketVesselChangeCompletion, bill2.JPB_MessageStatus);
			});
		}

		public void TestDeleteMessageFromFactory_WhenMeetsSaveException()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_CarrierCode = "OTT1";
			header.JPH_MasterBillNumber = "OTT1MB32342";
			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "SDSDHB323422";
			var bill2 = header.Bills.AddNew();
			bill2.JPB_BillNumber = "SDAB99546552";
			var bill3 = header.Bills.AddNew();
			bill3.JPB_BillNumber = "SDSGHB895654";
			Factory.Save();

			var generator = new AFRMessageGeneratorForTest(header);

			CombineAssertions("we should delete failing message from Factory when message fails to save", () =>
			{
				AssertExceptionThrown<RethrownByExceptionHandlerException>(() =>
				{
					generator.SendCompletionMessageToCustoms(ActionCode.RegisterCompletionByRegistration);
				});

				AssertEquals(0, header.Messages.Count);
				AssertEquals(0, header.AFRMessages.Count);

				var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals(0, ediMessages.Length);

				var ediInterchanges = Factory.Load<EDIInterchange>(new ZQuery());
				AssertEquals(0, ediInterchanges.Length);
			});
		}

		public void TestRestoreToOriginal_WhenMeetsSaveException()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_CarrierCode = "OTT1";
			header.JPH_MasterBillNumber = "OTT1MB32342";
			header.JPH_OverrideFreightDefaults = false;
			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "SDSDHB323422";
			var bill2 = header.Bills.AddNew();
			bill2.JPB_BillNumber = "SDAB99546552";
			var bill3 = header.Bills.AddNew();
			bill3.JPB_BillNumber = "SDSGHB895654";
			Factory.Save();

			header.JPH_MessageStatus = "XXX";
			bill1.JPB_MessageStatus = "ZZZ";
			bill3.JPB_MessageStatus = "111";
			var generator = new AFRMessageGeneratorForTest(header);

			CombineAssertions("we should delete failing message from Factory when message fails to save", () =>
			{
				AssertExceptionThrown<RethrownByExceptionHandlerException>(() =>
				{
					generator.SendCompletionMessageToCustoms(ActionCode.RegisterCompletionByRegistration);
				});
				AssertEquals("", header.JPH_MessageStatus);
				AssertEquals(false, header.JPH_OverrideFreightDefaults);
				AssertEquals("", bill1.JPB_MessageStatus);
				AssertEquals("", bill3.JPB_MessageStatus);

				AssertEquals(0, header.Messages.Count);
				AssertEquals(0, header.AFRMessages.Count);

				var ediMessages = Factory.Load<EDIMessage>(new ZQuery());
				AssertEquals(0, ediMessages.Length);

				var ediInterchanges = Factory.Load<EDIInterchange>(new ZQuery());
				AssertEquals(0, ediInterchanges.Length);
			});
		}

		public void TestSendDataToCustoms_MeetingSaveConcurrencyException()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			header.JPH_CarrierCode = "OTT1";
			header.JPH_MasterBillNumber = "OTT1MB32342";
			header.JPH_OverrideFreightDefaults = false;
			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "SDSDHB323422";

			Factory.Save();

			var factory1 = new BusinessObjectFactory { RefreshEnabled = false };
			var headerFactory1 = factory1.Load<JPAFRHeader>(header.PK);
			var bill1Factory1 = factory1.Load<JPAFRBills>(bill1.PK);

			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };
			var headerFactory2 = factory2.Load<JPAFRHeader>(header.PK);
			headerFactory2.JPH_MessageStatus = "EHR";
			var bill1Factory2 = factory2.Load<JPAFRBills>(bill1.PK);
			bill1Factory2.JPB_MessageStatus = "EHR";

			var generator = new AFRMessageGeneratorForTest2(headerFactory1);
			generator.AddtionalActionForTest += () =>
			{
				factory2.Save();
			};

			generator.SendDataToCustoms(MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouse, (ZGuid billPK, ref string functionType) =>
			{
				functionType = FunctionTypeList.Codes.Registration;
				return true;
			});

			AssertEquals("JPH_MessageStatus", "EHR", headerFactory1.JPH_MessageStatus);
			AssertEquals("JPH_OverrideFreightDefaults", false, headerFactory1.JPH_OverrideFreightDefaults);
			AssertEquals("JPB_MessageStatus", "EHR", bill1Factory1.JPB_MessageStatus);

			AssertEquals(0, headerFactory1.Messages.Count);
			AssertEquals(0, headerFactory1.AFRMessages.Count);

			var newFactory = new BusinessObjectFactory();
			var headerNewFactory = newFactory.Load<JPAFRHeader>(header.PK);
			var bill1NewFactory = newFactory.Load<JPAFRBills>(bill1.PK);
			AssertEquals("JPH_MessageStatus", "EHR", headerNewFactory.JPH_MessageStatus);
			AssertEquals("JPH_OverrideFreightDefaults", false, headerNewFactory.JPH_OverrideFreightDefaults);
			AssertEquals("JPB_MessageStatus", "EHR", bill1NewFactory.JPB_MessageStatus);
			var ediMessages = newFactory.Load<EDIMessage>(new ZQuery());
			AssertEquals(0, ediMessages.Length);

			var ediInterchanges = newFactory.Load<EDIInterchange>(new ZQuery());
			AssertEquals(0, ediInterchanges.Length);
		}

		[TestDate(2017, 10, 10)]
		public void TestSendDataToCustoms()
		{
			var factory = new BusinessObjectFactory();
			var header = factory.New<JPAFRHeader>();
			header.JPH_CarrierCode = "OTT1";
			header.JPH_MasterBillNumber = "OTT1MB32342";
			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "SDSDHB323422";
			var bill2 = header.Bills.AddNew();
			bill2.JPB_BillNumber = "SDAB99546552";
			var bill3 = header.Bills.AddNew();
			bill3.JPB_BillNumber = "SDSGHB895654";
			factory.Save();

			header = Factory.Load<JPAFRHeader>(header.PK);
			var generator = new AFRMessageGenerator(header, DefaultDataObjectWriterStrategy.Instance);
			generator.SendDataToCustoms(MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouse, (ZGuid billPK, ref string functionType) =>
			{
				functionType = FunctionTypeList.Codes.Registration;
				return true;
			});
			AssertEquals("header.Messages.Count", 1, header.Messages.Count);
			var message = header.Messages[0];
			AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
			AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
			AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);
			AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
			AssertEquals("message.EM_MessageOwner", MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouse, message.EM_MessageOwner);
			AssertEquals("message.IsInDatabase", true, message.IsInDatabase);
			AssertMultilineASCIIEquals("message.EM_MessageText", MessageWithAllBills, message.EM_MessageText);
			var interchange = message.Interchange;
			AssertEquals("Interchange message should be the same as Header message", message.PK, interchange.ContainedMessages[0].PK);
			AssertEquals("interchange.EI_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, interchange.EI_ApplicationCode);
			AssertEquals("interchange.EI_InterchangeType", EDIInterchangeTypeList.Codes.XDC, interchange.EI_InterchangeType);
			AssertEquals("interchange.EI_From", "ENT", interchange.EI_From);
			AssertEquals("interchange.EI_To", "JPCustoms", interchange.EI_To);
			AssertEquals("interchange.EI_Status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
			AssertEquals("interchange.EI_TransportType", EDIInterchangeTransportTypeList.Codes.eHub, interchange.EI_TransportType);
			AssertEquals("interchange.EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
			AssertEquals("interchange.IsInDatabase", true, interchange.IsInDatabase);
			AssertMultilineASCIIEquals("MessageText should be same as BodyText", message.EM_MessageText, interchange.EI_BodyText);

			header.Messages.RemoveAndDeleteAllFromTest();
			interchange.Delete();

			generator.SendDataToCustoms(MessagingTypeList.Codes.UpdateRegisteredAdvanceCargoInformationMaster, (ZGuid billPK, ref string functionType) =>
			{
				functionType = bill1.PK == billPK ? FunctionTypeList.Codes.Add : FunctionTypeList.Codes.Update;
				return bill2.PK != billPK;
			});
			AssertEquals("header.Messages.Count", 1, header.Messages.Count);
			message = header.Messages[0];
			AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
			AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
			AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);
			AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
			AssertEquals("message.EM_MessageOwner", MessagingTypeList.Codes.UpdateRegisteredAdvanceCargoInformationMaster, message.EM_MessageOwner);
			AssertMultilineASCIIEquals("message.EM_MessageText", MessageWithoutOneBill, message.EM_MessageText);
			interchange = message.Interchange;
			AssertEquals("Interchange message should be the same as Header message", message.PK, interchange.ContainedMessages[0].PK);
			AssertEquals("interchange.EI_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, interchange.EI_ApplicationCode);
			AssertEquals("interchange.EI_InterchangeType", EDIInterchangeTypeList.Codes.XDC, interchange.EI_InterchangeType);
			AssertEquals("interchange.EI_From", "ENT", interchange.EI_From);
			AssertEquals("interchange.EI_To", "JPCustoms", interchange.EI_To);
			AssertEquals("interchange.EI_Status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
			AssertEquals("interchange.EI_TransportType", EDIInterchangeTransportTypeList.Codes.eHub, interchange.EI_TransportType);
			AssertEquals("interchange.EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
			AssertMultilineASCIIEquals("MessageText should be same as BodyText", message.EM_MessageText, interchange.EI_BodyText);

			header.Messages.RemoveAndDeleteAllFromTest();

			generator = new AFRMessageGenerator(header, DefaultDataObjectWriterStrategy.Instance, true);
			generator.SendDataToCustoms(MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouse, (ZGuid billPK, ref string functionType) =>
			{
				functionType = FunctionTypeList.Codes.Registration;
				return true;
			});
			AssertEquals("header.Messages.Count", 1, header.Messages.Count);
			message = header.Messages[0];
			AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
			AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
			AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);
			AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
			AssertEquals("message.EM_MessageOwner", MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouse, message.EM_MessageOwner);
			AssertEquals("message.IsInDatabase", false, message.IsInDatabase);
			AssertMultilineASCIIEquals("message.EM_MessageText", MessageWithAllBillsWithoutMessageNumber, message.EM_MessageText);
			interchange = message.Interchange;
			AssertEquals("Interchange message should be the same as Header message", message.PK, interchange.ContainedMessages[0].PK);
			AssertEquals("interchange.EI_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, interchange.EI_ApplicationCode);
			AssertEquals("interchange.EI_InterchangeType", EDIInterchangeTypeList.Codes.XDC, interchange.EI_InterchangeType);
			AssertEquals("interchange.EI_From", "ENT", interchange.EI_From);
			AssertEquals("interchange.EI_To", "JPCustoms", interchange.EI_To);
			AssertEquals("interchange.EI_Status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
			AssertEquals("interchange.EI_TransportType", EDIInterchangeTransportTypeList.Codes.eHub, interchange.EI_TransportType);
			AssertEquals("interchange.EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
			AssertEquals("interchange.IsInDatabase", false, interchange.IsInDatabase);
			AssertMultilineASCIIEquals("MessageText should be same as BodyText", message.EM_MessageText, interchange.EI_BodyText);
		}

		[TestDate(2017, 10, 10)]
		public void TestSendDataToCustomsForCAWithCorrectMessageNumber()
		{
			var factory = new BusinessObjectFactory();
			var cpmpany = factory.New<GlbCompany>();
			cpmpany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			cpmpany.GC_Code = "TC1";

			var branch = factory.New<GlbBranch>();
			branch.GB_Code = "GB1";
			branch.GB_GC = cpmpany.PK;

			var header = factory.New<JPAFRHeader>();
			header.JPH_CarrierCode = "OTT1";
			header.JPH_GB_Branch = branch.PK;
			header.JPH_MasterBillNumber = "OTT1MB32342";
			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "SDSDHB323422";
			var bill2 = header.Bills.AddNew();
			bill2.JPB_BillNumber = "SDAB99546552";
			var bill3 = header.Bills.AddNew();
			bill3.JPB_BillNumber = "SDSGHB895654";

			factory.Save();

			header = Factory.Load<JPAFRHeader>(header.PK);
			var generator = new AFRMessageGenerator(header, DefaultDataObjectWriterStrategy.Instance);
			generator.SendDataToCustoms(MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouse, (ZGuid billPK, ref string functionType) =>
			{
				functionType = FunctionTypeList.Codes.Registration;
				return true;
			});
			AssertEquals("header.Messages.Count", 1, header.Messages.Count);
			var message = header.Messages[0];
			AssertEquals("message.EM_MessageNum", "JP00000001", message.EM_MessageNum);
		}

		public void TestSendDataToCustoms_ShouldSetOverrideBillDetails_WhenSendSuccecefully()
		{
			var factory = new BusinessObjectFactory();
			var header = factory.New<JPAFRHeader>();
			var consol = Factory.New<ForwardingConsol>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			header.JPH_CarrierCode = "OTT1";
			header.JPH_MasterBillNumber = "OTT1MB32342";
			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "SDSDHB323422";

			header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;

			factory.Save();

			header = Factory.Load<JPAFRHeader>(header.PK);
			var generator = new AFRMessageGenerator(header, DefaultDataObjectWriterStrategy.Instance);
			var sendOk = generator.SendDataToCustoms(MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouse, (ZGuid billPK, ref string functionType) =>
			{
				functionType = FunctionTypeList.Codes.Registration;
				return true;
			});

			Assert("Message was sent OK", sendOk);

			header.Refresh();

			AssertEquals("header.Messages.Count", 1, header.Messages.Count);

			AssertEquals("JPH_OverrideFreightDefaults", true, header.JPH_OverrideFreightDefaults);
		}

		[TestDate(2017, 10, 10)]
		public void TestSendCompletionMessageToCustoms_REG()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_CarrierCode = "OTT1";
			header.JPH_MasterBillNumber = "OTT1MB32342";
			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "SDSDHB323422";
			var bill2 = header.Bills.AddNew();
			bill2.JPB_BillNumber = "SDAB99546552";
			var bill3 = header.Bills.AddNew();
			bill3.JPB_BillNumber = "SDSGHB895654";
			Factory.Save();
			var generator = new AFRMessageGenerator(header, DefaultDataObjectWriterStrategy.Instance);
			generator.SendCompletionMessageToCustoms(ActionCode.RegisterCompletionByRegistration);
			var message = header.Messages[0];
			AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
			AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
			AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);
			AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
			AssertEquals("message.EM_MessageOwner", MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouseCompletion, message.EM_MessageOwner);
			AssertMultilineASCIIEquals("message.EM_MessageText", MessageWithBillCompletion_REG, message.EM_MessageText);
			var interchange = message.Interchange;
			AssertEquals("Interchange message should be the same as Header message", message.PK, interchange.ContainedMessages[0].PK);
			AssertEquals("interchange.EI_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, interchange.EI_ApplicationCode);
			AssertEquals("interchange.EI_InterchangeType", EDIInterchangeTypeList.Codes.XDC, interchange.EI_InterchangeType);
			AssertEquals("interchange.EI_From", "ENT", interchange.EI_From);
			AssertEquals("interchange.EI_To", "JPCustoms", interchange.EI_To);
			AssertEquals("interchange.EI_Status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
			AssertEquals("interchange.EI_TransportType", EDIInterchangeTransportTypeList.Codes.eHub, interchange.EI_TransportType);
			AssertEquals("interchange.EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
			AssertMultilineASCIIEquals("MessageText should be same as BodyText", message.EM_MessageText, interchange.EI_BodyText);
		}

		[TestDate(2017, 10, 10)]
		public void TestSendCompletionMessageToCustoms_ADD()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_CarrierCode = "OTT1";
			header.JPH_MasterBillNumber = "OTT1MB32342";
			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "SDSDHB323422";
			var bill2 = header.Bills.AddNew();
			bill2.JPB_BillNumber = "SDAB99546552";
			var bill3 = header.Bills.AddNew();
			bill3.JPB_BillNumber = "SDSGHB895654";
			Factory.Save();
			var generator = new AFRMessageGenerator(header, DefaultDataObjectWriterStrategy.Instance);
			generator.SendCompletionMessageToCustoms(ActionCode.RegisterCompletionByAmendment);
			var message = header.Messages[0];
			AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
			AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
			AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);
			AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
			AssertEquals("message.EM_MessageOwner", MessagingTypeList.Codes.AdvanceCargoInformationRegistrationHouseCompletion, message.EM_MessageOwner);
			AssertMultilineASCIIEquals("message.EM_MessageText", MessageWithBillCompletion_ADD, message.EM_MessageText);
			var interchange = message.Interchange;
			AssertEquals("Interchange message should be the same as Header message", message.PK, interchange.ContainedMessages[0].PK);
			AssertEquals("interchange.EI_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, interchange.EI_ApplicationCode);
			AssertEquals("interchange.EI_InterchangeType", EDIInterchangeTypeList.Codes.XDC, interchange.EI_InterchangeType);
			AssertEquals("interchange.EI_From", "ENT", interchange.EI_From);
			AssertEquals("interchange.EI_To", "JPCustoms", interchange.EI_To);
			AssertEquals("interchange.EI_Status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
			AssertEquals("interchange.EI_TransportType", EDIInterchangeTransportTypeList.Codes.eHub, interchange.EI_TransportType);
			AssertEquals("interchange.EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
			AssertMultilineASCIIEquals("MessageText should be same as BodyText", message.EM_MessageText, interchange.EI_BodyText);
		}

		public void TestSendDepartureTimeRegistrationToCustoms_WithNoBill()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_CarrierCode = "OTT1";
			header.JPH_MasterBillNumber = "OTT1MB32342";
			header.JPH_ETD = new ZDateTime(2013, 4, 19, 22, 11, 12);
			header.JPH_LoadingPortSuffix = "8";
			header.JPH_ETA = new ZDateTime(2014, 5, 20, 23, 12, 13);
			header.JPH_DischargePortSuffix = "9";
			header.JPH_IsShippingLineEntry = true;
			Factory.Save();
			var generator = new AFRMessageGenerator(header, DefaultDataObjectWriterStrategy.Instance);
			generator.SendDepartureTimeRegistrationToCustoms(false);
			var message = header.Messages[0];
			CombineAssertions(() =>
			{
				AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
				AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
				AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);
				AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("message.EM_MessageOwner", MessagingTypeList.Codes.DepartureTimeRegistration, message.EM_MessageOwner);
				AssertMultilineASCIIEquals("message.EM_MessageText", MessageWithDepartureTimeRegistration, message.EM_MessageText);
			});
			CombineAssertions(() =>
			{
				var interchange = message.Interchange;
				AssertEquals("Interchange message should be the same as Header message", message.PK, interchange.ContainedMessages[0].PK);
				AssertEquals("interchange.EI_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, interchange.EI_ApplicationCode);
				AssertEquals("interchange.EI_InterchangeType", EDIInterchangeTypeList.Codes.XDC, interchange.EI_InterchangeType);
				AssertEquals("interchange.EI_From", "ENT", interchange.EI_From);
				AssertEquals("interchange.EI_To", "JPCustoms", interchange.EI_To);
				AssertEquals("interchange.EI_Status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
				AssertEquals("interchange.EI_TransportType", EDIInterchangeTransportTypeList.Codes.eHub, interchange.EI_TransportType);
				AssertEquals("interchange.EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
				AssertMultilineASCIIEquals("MessageText should be same as BodyText", message.EM_MessageText, interchange.EI_BodyText);
			});
		}

		public void TestSendDepartureTimeRegistrationToCustoms_WithBills()
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_CarrierCode = "OTT1";
			header.JPH_MasterBillNumber = "OTT1MB32342";
			header.JPH_ETD = new ZDateTime(2013, 4, 19, 22, 11, 12);
			header.JPH_LoadingPortSuffix = "8";
			header.JPH_ETA = new ZDateTime(2014, 5, 20, 23, 12, 13);
			header.JPH_DischargePortSuffix = "9";
			header.JPH_IsShippingLineEntry = true;
			var bill1 = header.Bills.AddNew();
			bill1.JPB_BillNumber = "SDSDHB323422";
			var bill2 = header.Bills.AddNew();
			bill2.JPB_BillNumber = "SDAB99546552";
			var bill3 = header.Bills.AddNew();
			bill3.JPB_BillNumber = "SDSGHB895654";
			Factory.Save();
			var generator = new AFRMessageGenerator(header, DefaultDataObjectWriterStrategy.Instance);
			generator.SendDepartureTimeRegistrationToCustoms(false);
			var message = header.Messages[0];
			CombineAssertions(() =>
			{
				AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
				AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
				AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);
				AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("message.EM_MessageOwner", MessagingTypeList.Codes.DepartureTimeRegistration, message.EM_MessageOwner);
				AssertMultilineASCIIEquals("message.EM_MessageText", MessageWithDepartureTimeRegistration, message.EM_MessageText);
			});
			CombineAssertions(() =>
			{
				var interchange = message.Interchange;
				AssertEquals("Interchange message should be the same as Header message", message.PK, interchange.ContainedMessages[0].PK);
				AssertEquals("interchange.EI_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, interchange.EI_ApplicationCode);
				AssertEquals("interchange.EI_InterchangeType", EDIInterchangeTypeList.Codes.XDC, interchange.EI_InterchangeType);
				AssertEquals("interchange.EI_From", "ENT", interchange.EI_From);
				AssertEquals("interchange.EI_To", "JPCustoms", interchange.EI_To);
				AssertEquals("interchange.EI_Status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
				AssertEquals("interchange.EI_TransportType", EDIInterchangeTransportTypeList.Codes.eHub, interchange.EI_TransportType);
				AssertEquals("interchange.EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
				AssertMultilineASCIIEquals("MessageText should be same as BodyText", message.EM_MessageText, interchange.EI_BodyText);
			});
		}

		public void TestSendBLLMessageToCustoms_RegisterSplit()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill3 = header.Bills.AddNew("bill3");
			bill3.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;

			var bllFunction = BLLFunction.New(header, BLLFunctionCode.RegisterSplit, bill1);
			bllFunction.JPM_ChangeReasonCode = AFRBLLChangeReasonCodeList.Codes.ChangeInCargoOperation;
			bllFunction.SelectedBills.AddRange(new BLLFunctionBill(bill2), new BLLFunctionBill(bill3));
			Factory.Save();

			var generator = new AFRMessageGenerator(header, DefaultDataObjectWriterStrategy.Instance);
			generator.SendBLLMessageToCustoms(bllFunction);

			CombineAssertions(() =>
			{
				AssertEquals(MessageStatusList.Codes.AwaitingBLLRegistration, bill1.JPB_MessageStatus);
				AssertEquals(MessageStatusList.Codes.AwaitingBLLRegistration, bill2.JPB_MessageStatus);
				AssertEquals(MessageStatusList.Codes.AwaitingBLLRegistration, bill3.JPB_MessageStatus);
				var message = header.Messages[0];
				AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
				AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
				AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);
				AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("message.EM_MessageOwner", MessagingTypeList.Codes.RegisterBLL, message.EM_MessageOwner);
				AssertMultilineASCIIEquals("message.EM_MessageText", @"<?xml version=""1.0"" encoding=""utf-8""?>
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
          <Key>bill1</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <WayBillNumber>bill1</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>

    <AddInfoCollection>
      <AddInfo>
        <Key>JPBLLFunctionCode</Key>
        <Value>1</Value>
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
            <Value>bill1</Value>
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
            <Value>bill2</Value>
          </AddInfo>
          <AddInfo>
            <Key>BillNumber-2</Key>
            <Value>bill3</Value>
          </AddInfo>
        </AddInfoCollection>
      </AddInfoGroup>
    </AddInfoGroupCollection>
  </Shipment>
</UniversalShipment>", message.EM_MessageText);
				var interchange = message.Interchange;
				AssertEquals("Interchange message should be the same as Header message", message.PK, interchange.ContainedMessages[0].PK);
				AssertEquals("interchange.EI_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, interchange.EI_ApplicationCode);
				AssertEquals("interchange.EI_InterchangeType", EDIInterchangeTypeList.Codes.XDC, interchange.EI_InterchangeType);
				AssertEquals("interchange.EI_From", "ENT", interchange.EI_From);
				AssertEquals("interchange.EI_To", "JPCustoms", interchange.EI_To);
				AssertEquals("interchange.EI_Status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
				AssertEquals("interchange.EI_TransportType", EDIInterchangeTransportTypeList.Codes.eHub, interchange.EI_TransportType);
				AssertEquals("interchange.EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
				AssertMultilineASCIIEquals("MessageText should be same as BodyText", message.EM_MessageText, interchange.EI_BodyText);
			});
		}

		public void TestSendBLLMessageToCustoms_RegisterSwitch()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;

			var bllFunction = BLLFunction.New(header, BLLFunctionCode.RegisterSwitch, bill1);
			bllFunction.JPM_ChangeReasonCode = AFRBLLChangeReasonCodeList.Codes.ChangeInCargoOperation;
			bllFunction.SelectedBills.AddRange(new BLLFunctionBill(bill2));
			Factory.Save();

			var generator = new AFRMessageGenerator(header, DefaultDataObjectWriterStrategy.Instance);
			generator.SendBLLMessageToCustoms(bllFunction);

			CombineAssertions(() =>
			{
				AssertEquals(MessageStatusList.Codes.AwaitingBLLRegistration, bill1.JPB_MessageStatus);
				AssertEquals(MessageStatusList.Codes.AwaitingBLLRegistration, bill2.JPB_MessageStatus);
				var message = header.Messages[0];
				AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
				AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
				AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);
				AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("message.EM_MessageOwner", MessagingTypeList.Codes.RegisterBLL, message.EM_MessageOwner);
				AssertMultilineASCIIEquals("message.EM_MessageText", @"<?xml version=""1.0"" encoding=""utf-8""?>
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
          <Key>bill1</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <WayBillNumber>bill1</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>

    <AddInfoCollection>
      <AddInfo>
        <Key>JPBLLFunctionCode</Key>
        <Value>2</Value>
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
            <Value>bill1</Value>
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
            <Value>bill2</Value>
          </AddInfo>
        </AddInfoCollection>
      </AddInfoGroup>
    </AddInfoGroupCollection>
  </Shipment>
</UniversalShipment>", message.EM_MessageText);
				var interchange = message.Interchange;
				AssertEquals("Interchange message should be the same as Header message", message.PK, interchange.ContainedMessages[0].PK);
				AssertEquals("interchange.EI_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, interchange.EI_ApplicationCode);
				AssertEquals("interchange.EI_InterchangeType", EDIInterchangeTypeList.Codes.XDC, interchange.EI_InterchangeType);
				AssertEquals("interchange.EI_From", "ENT", interchange.EI_From);
				AssertEquals("interchange.EI_To", "JPCustoms", interchange.EI_To);
				AssertEquals("interchange.EI_Status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
				AssertEquals("interchange.EI_TransportType", EDIInterchangeTransportTypeList.Codes.eHub, interchange.EI_TransportType);
				AssertEquals("interchange.EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
				AssertMultilineASCIIEquals("MessageText should be same as BodyText", message.EM_MessageText, interchange.EI_BodyText);
			});
		}

		public void TestSendBLLMessageToCustoms_RegisterMerge()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill3 = header.Bills.AddNew("bill3");
			bill3.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;

			var bllFunction = BLLFunction.New(header, BLLFunctionCode.RegisterMerge, bill1);
			bllFunction.JPM_ChangeReasonCode = AFRBLLChangeReasonCodeList.Codes.ChangeInCargoOperation;
			bllFunction.SelectedBills.AddRange(new BLLFunctionBill(bill2), new BLLFunctionBill(bill3));
			Factory.Save();

			var generator = new AFRMessageGenerator(header, DefaultDataObjectWriterStrategy.Instance);
			generator.SendBLLMessageToCustoms(bllFunction);

			CombineAssertions(() =>
			{
				AssertEquals(MessageStatusList.Codes.AwaitingBLLRegistration, bill1.JPB_MessageStatus);
				AssertEquals(MessageStatusList.Codes.AwaitingBLLRegistration, bill2.JPB_MessageStatus);
				AssertEquals(MessageStatusList.Codes.AwaitingBLLRegistration, bill3.JPB_MessageStatus);
				var message = header.Messages[0];
				AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
				AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
				AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);
				AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("message.EM_MessageOwner", MessagingTypeList.Codes.RegisterBLL, message.EM_MessageOwner);
				AssertMultilineASCIIEquals("message.EM_MessageText", @"<?xml version=""1.0"" encoding=""utf-8""?>
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
          <Key>bill1</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <WayBillNumber>bill1</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>

    <AddInfoCollection>
      <AddInfo>
        <Key>JPBLLFunctionCode</Key>
        <Value>3</Value>
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
            <Key>BillNumber-1</Key>
            <Value>bill2</Value>
          </AddInfo>
          <AddInfo>
            <Key>BillNumber-2</Key>
            <Value>bill3</Value>
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
            <Value>bill1</Value>
          </AddInfo>
        </AddInfoCollection>
      </AddInfoGroup>
    </AddInfoGroupCollection>
  </Shipment>
</UniversalShipment>", message.EM_MessageText);
				var interchange = message.Interchange;
				AssertEquals("Interchange message should be the same as Header message", message.PK, interchange.ContainedMessages[0].PK);
				AssertEquals("interchange.EI_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, interchange.EI_ApplicationCode);
				AssertEquals("interchange.EI_InterchangeType", EDIInterchangeTypeList.Codes.XDC, interchange.EI_InterchangeType);
				AssertEquals("interchange.EI_From", "ENT", interchange.EI_From);
				AssertEquals("interchange.EI_To", "JPCustoms", interchange.EI_To);
				AssertEquals("interchange.EI_Status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
				AssertEquals("interchange.EI_TransportType", EDIInterchangeTransportTypeList.Codes.eHub, interchange.EI_TransportType);
				AssertEquals("interchange.EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
				AssertMultilineASCIIEquals("MessageText should be same as BodyText", message.EM_MessageText, interchange.EI_BodyText);
			});
		}

		public void TestSendBLLMessageToCustoms_CancelSplit()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill3 = header.Bills.AddNew("bill3");
			bill3.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;

			var bllFunction = BLLFunction.New(header, BLLFunctionCode.CancelSplit, bill1);
			bllFunction.JPM_ChangeReasonCode = AFRBLLChangeReasonCodeList.Codes.ChangeInCargoOperation;
			bllFunction.SelectedBills.AddRange(new BLLFunctionBill(bill2), new BLLFunctionBill(bill3));
			Factory.Save();

			var generator = new AFRMessageGenerator(header, DefaultDataObjectWriterStrategy.Instance);
			generator.SendBLLMessageToCustoms(bllFunction);

			CombineAssertions(() =>
			{
				AssertEquals(MessageStatusList.Codes.AwaitingBLLCancellation, bill1.JPB_MessageStatus);
				AssertEquals(MessageStatusList.Codes.AwaitingBLLCancellation, bill2.JPB_MessageStatus);
				AssertEquals(MessageStatusList.Codes.AwaitingBLLCancellation, bill3.JPB_MessageStatus);
				var message = header.Messages[0];
				AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
				AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
				AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);
				AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("message.EM_MessageOwner", MessagingTypeList.Codes.CancelBLL, message.EM_MessageOwner);
				AssertMultilineASCIIEquals("message.EM_MessageText", @"<?xml version=""1.0"" encoding=""utf-8""?>
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
          <Key>bill1</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <WayBillNumber>bill1</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
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
            <Value>bill1</Value>
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
            <Value>bill2</Value>
          </AddInfo>
          <AddInfo>
            <Key>BillNumber-2</Key>
            <Value>bill3</Value>
          </AddInfo>
        </AddInfoCollection>
      </AddInfoGroup>
    </AddInfoGroupCollection>
  </Shipment>
</UniversalShipment>", message.EM_MessageText);
				var interchange = message.Interchange;
				AssertEquals("Interchange message should be the same as Header message", message.PK, interchange.ContainedMessages[0].PK);
				AssertEquals("interchange.EI_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, interchange.EI_ApplicationCode);
				AssertEquals("interchange.EI_InterchangeType", EDIInterchangeTypeList.Codes.XDC, interchange.EI_InterchangeType);
				AssertEquals("interchange.EI_From", "ENT", interchange.EI_From);
				AssertEquals("interchange.EI_To", "JPCustoms", interchange.EI_To);
				AssertEquals("interchange.EI_Status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
				AssertEquals("interchange.EI_TransportType", EDIInterchangeTransportTypeList.Codes.eHub, interchange.EI_TransportType);
				AssertEquals("interchange.EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
				AssertMultilineASCIIEquals("MessageText should be same as BodyText", message.EM_MessageText, interchange.EI_BodyText);
			});
		}

		public void TestSendBLLMessageToCustoms_CancelSwitch()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;

			var bllFunction = BLLFunction.New(header, BLLFunctionCode.CancelSwitch, bill1);
			bllFunction.JPM_ChangeReasonCode = AFRBLLChangeReasonCodeList.Codes.ChangeInCargoOperation;
			bllFunction.SelectedBills.AddRange(new BLLFunctionBill(bill2));
			Factory.Save();

			var generator = new AFRMessageGenerator(header, DefaultDataObjectWriterStrategy.Instance);
			generator.SendBLLMessageToCustoms(bllFunction);

			CombineAssertions(() =>
			{
				AssertEquals(MessageStatusList.Codes.AwaitingBLLCancellation, bill1.JPB_MessageStatus);
				AssertEquals(MessageStatusList.Codes.AwaitingBLLCancellation, bill2.JPB_MessageStatus);
				var message = header.Messages[0];
				AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
				AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
				AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);
				AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("message.EM_MessageOwner", MessagingTypeList.Codes.CancelBLL, message.EM_MessageOwner);
				AssertMultilineASCIIEquals("message.EM_MessageText", @"<?xml version=""1.0"" encoding=""utf-8""?>
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
          <Key>bill1</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <WayBillNumber>bill1</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>

    <AddInfoCollection>
      <AddInfo>
        <Key>JPBLLFunctionCode</Key>
        <Value>5</Value>
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
            <Value>bill1</Value>
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
            <Value>bill2</Value>
          </AddInfo>
        </AddInfoCollection>
      </AddInfoGroup>
    </AddInfoGroupCollection>
  </Shipment>
</UniversalShipment>", message.EM_MessageText);
				var interchange = message.Interchange;
				AssertEquals("Interchange message should be the same as Header message", message.PK, interchange.ContainedMessages[0].PK);
				AssertEquals("interchange.EI_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, interchange.EI_ApplicationCode);
				AssertEquals("interchange.EI_InterchangeType", EDIInterchangeTypeList.Codes.XDC, interchange.EI_InterchangeType);
				AssertEquals("interchange.EI_From", "ENT", interchange.EI_From);
				AssertEquals("interchange.EI_To", "JPCustoms", interchange.EI_To);
				AssertEquals("interchange.EI_Status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
				AssertEquals("interchange.EI_TransportType", EDIInterchangeTransportTypeList.Codes.eHub, interchange.EI_TransportType);
				AssertEquals("interchange.EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
				AssertMultilineASCIIEquals("MessageText should be same as BodyText", message.EM_MessageText, interchange.EI_BodyText);
			});
		}

		public void TestSendBLLMessageToCustoms_CancelMerge()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill3 = header.Bills.AddNew("bill3");
			bill3.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;

			var bllFunction = BLLFunction.New(header, BLLFunctionCode.CancelMerge, bill1);
			bllFunction.JPM_ChangeReasonCode = AFRBLLChangeReasonCodeList.Codes.ChangeInCargoOperation;
			bllFunction.SelectedBills.AddRange(new BLLFunctionBill(bill2), new BLLFunctionBill(bill3));
			Factory.Save();

			var generator = new AFRMessageGenerator(header, DefaultDataObjectWriterStrategy.Instance);
			generator.SendBLLMessageToCustoms(bllFunction);

			CombineAssertions(() =>
			{
				AssertEquals(MessageStatusList.Codes.AwaitingBLLCancellation, bill1.JPB_MessageStatus);
				AssertEquals(MessageStatusList.Codes.AwaitingBLLCancellation, bill2.JPB_MessageStatus);
				AssertEquals(MessageStatusList.Codes.AwaitingBLLCancellation, bill3.JPB_MessageStatus);
				var message = header.Messages[0];
				AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, message.EM_ApplicationCode);
				AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
				AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
				AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);
				AssertEquals("message.EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
				AssertEquals("message.EM_MessageOwner", MessagingTypeList.Codes.CancelBLL, message.EM_MessageOwner);
				AssertMultilineASCIIEquals("message.EM_MessageText", @"<?xml version=""1.0"" encoding=""utf-8""?>
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
          <Key>bill1</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>

    <WayBillNumber>bill1</WayBillNumber>
    <WayBillType>
      <Code>HWB</Code>
      <Description>House Waybill</Description>
    </WayBillType>

    <AddInfoCollection>
      <AddInfo>
        <Key>JPBLLFunctionCode</Key>
        <Value>6</Value>
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
            <Key>BillNumber-1</Key>
            <Value>bill2</Value>
          </AddInfo>
          <AddInfo>
            <Key>BillNumber-2</Key>
            <Value>bill3</Value>
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
            <Value>bill1</Value>
          </AddInfo>
        </AddInfoCollection>
      </AddInfoGroup>
    </AddInfoGroupCollection>
  </Shipment>
</UniversalShipment>", message.EM_MessageText);
				var interchange = message.Interchange;
				AssertEquals("Interchange message should be the same as Header message", message.PK, interchange.ContainedMessages[0].PK);
				AssertEquals("interchange.EI_ApplicationCode", ApplicationCodeList.Codes.UniversalDataMessaging, interchange.EI_ApplicationCode);
				AssertEquals("interchange.EI_InterchangeType", EDIInterchangeTypeList.Codes.XDC, interchange.EI_InterchangeType);
				AssertEquals("interchange.EI_From", "ENT", interchange.EI_From);
				AssertEquals("interchange.EI_To", "JPCustoms", interchange.EI_To);
				AssertEquals("interchange.EI_Status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
				AssertEquals("interchange.EI_TransportType", EDIInterchangeTransportTypeList.Codes.eHub, interchange.EI_TransportType);
				AssertEquals("interchange.EI_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
				AssertMultilineASCIIEquals("MessageText should be same as BodyText", message.EM_MessageText, interchange.EI_BodyText);
			});
		}

		#region Message For Assertion

		static string MessageWithBillCompletion_REG => @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>AFRHeader</Type>
          <Key>AFR00000001</Key>
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
      <Code></Code>
    </PortOfDischarge>
    <PortOfLoading>
      <Code></Code>
    </PortOfLoading>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <VesselName></VesselName>
    <VoyageFlightNo></VoyageFlightNo>
    <WayBillNumber>OTT1MB32342</WayBillNumber>
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
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>JPIsDepartureFromRelaxedArea</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPVesselCallSign</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>JPVesselDetailsChanged</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPInternalTransactionNumber</Key>
        <Value>JP00000001</Value>
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

		static string MessageWithBillCompletion_ADD => @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>AFRHeader</Type>
          <Key>AFR00000001</Key>
        </DataSource>
      </DataSourceCollection>

      <ActionPurpose>
        <Code>CHR</Code>
        <Description>Update Advance Cargo Information Registration House</Description>
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
      <Code></Code>
    </PortOfDischarge>
    <PortOfLoading>
      <Code></Code>
    </PortOfLoading>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <VesselName></VesselName>
    <VoyageFlightNo></VoyageFlightNo>
    <WayBillNumber>OTT1MB32342</WayBillNumber>
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
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>JPIsDepartureFromRelaxedArea</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPVesselCallSign</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>JPVesselDetailsChanged</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPInternalTransactionNumber</Key>
        <Value>JP00000001</Value>
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

    <SubShipmentCollection>
      <SubShipment>
        <DataContext>

          <ActionPurpose>
            <Code>ADD</Code>
            <Description>Add</Description>
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

		static string MessageWithAllBillsWithoutMessageNumber => @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>AFRHeader</Type>
          <Key>AFR00000001</Key>
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
      <Code></Code>
    </PortOfDischarge>
    <PortOfLoading>
      <Code></Code>
    </PortOfLoading>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <VesselName></VesselName>
    <VoyageFlightNo></VoyageFlightNo>
    <WayBillNumber>OTT1MB32342</WayBillNumber>
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
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>JPIsDepartureFromRelaxedArea</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPVesselCallSign</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>JPVesselDetailsChanged</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPInternalTransactionNumber</Key>
        <Value>__(AFR MESSAGE NO)__</Value>
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
          <Code></Code>
        </PortOfDestination>
        <PortOfOrigin>
          <Code></Code>
        </PortOfOrigin>
        <WayBillNumber>SDSDHB323422</WayBillNumber>
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
            <DetailedDescription></DetailedDescription>
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
        <WayBillNumber>SDAB99546552</WayBillNumber>
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
            <DetailedDescription></DetailedDescription>
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
        <WayBillNumber>SDSGHB895654</WayBillNumber>
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
            <DetailedDescription></DetailedDescription>
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

		static string MessageWithAllBills => @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>AFRHeader</Type>
          <Key>AFR00000001</Key>
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
      <Code></Code>
    </PortOfDischarge>
    <PortOfLoading>
      <Code></Code>
    </PortOfLoading>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <VesselName></VesselName>
    <VoyageFlightNo></VoyageFlightNo>
    <WayBillNumber>OTT1MB32342</WayBillNumber>
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
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>JPIsDepartureFromRelaxedArea</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPVesselCallSign</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>JPVesselDetailsChanged</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPInternalTransactionNumber</Key>
        <Value>JP00000001</Value>
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
          <Code></Code>
        </PortOfDestination>
        <PortOfOrigin>
          <Code></Code>
        </PortOfOrigin>
        <WayBillNumber>SDSDHB323422</WayBillNumber>
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
            <DetailedDescription></DetailedDescription>
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
        <WayBillNumber>SDAB99546552</WayBillNumber>
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
            <DetailedDescription></DetailedDescription>
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
        <WayBillNumber>SDSGHB895654</WayBillNumber>
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
            <DetailedDescription></DetailedDescription>
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

		static string MessageWithoutOneBill
		{
			get
			{
				return @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>AFRHeader</Type>
          <Key>AFR00000001</Key>
        </DataSource>
      </DataSourceCollection>

      <ActionPurpose>
        <Code>CMR</Code>
        <Description>Update Registered Advance Cargo Information Master</Description>
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
      <Code></Code>
    </PortOfDischarge>
    <PortOfLoading>
      <Code></Code>
    </PortOfLoading>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <VesselName></VesselName>
    <VoyageFlightNo></VoyageFlightNo>
    <WayBillNumber>OTT1MB32342</WayBillNumber>
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
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>JPIsDepartureFromRelaxedArea</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPVesselCallSign</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>JPVesselDetailsChanged</Key>
        <Value>Y</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPInternalTransactionNumber</Key>
        <Value>JP00000002</Value>
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
            <Code>ADD</Code>
            <Description>Add</Description>
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
        <WayBillNumber>SDSDHB323422</WayBillNumber>
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
            <DetailedDescription></DetailedDescription>
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
            <Code>UPD</Code>
            <Description>Update</Description>
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
        <WayBillNumber>SDSGHB895654</WayBillNumber>
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
            <DetailedDescription></DetailedDescription>
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
			}
		}

		static string MessageWithDepartureTimeRegistration => @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>AFRHeader</Type>
          <Key>AFR00000001</Key>
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
      <Name>BN - AUBNE</Name>
    </Branch>
    <PortOfDischarge>
      <Code></Code>
    </PortOfDischarge>
    <PortOfLoading>
      <Code></Code>
    </PortOfLoading>
    <TransportMode>
      <Code>SEA</Code>
      <Description>Sea Freight</Description>
    </TransportMode>
    <VesselName></VesselName>
    <VoyageFlightNo></VoyageFlightNo>
    <WayBillNumber>OTT1MB32342</WayBillNumber>
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
        <Value>8</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPPortOfDischargeSuffix</Key>
        <Value>9</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPIsDepartureFromRelaxedArea</Key>
        <Value>N</Value>
      </AddInfo>
      <AddInfo>
        <Key>JPVesselCallSign</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>JPOperationalCarrierVoyageNo</Key>
        <Value></Value>
      </AddInfo>
      <AddInfo>
        <Key>JPInternalTransactionNumber</Key>
        <Value>JP00000001</Value>
      </AddInfo>
    </AddInfoCollection>

    <DateCollection>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2013-04-19T22:11:12</Value>
      </Date>
      <Date>
        <Type>Arrival</Type>
        <IsEstimate>false</IsEstimate>
        <Value>2014-05-20T23:12:13</Value>
      </Date>
    </DateCollection>

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
	}

	class AFRMessageGeneratorForTest : AFRMessageGenerator
	{
		public AFRMessageGeneratorForTest(JPAFRHeader header, bool savingIsDoneExternally = false) : base(header, DefaultDataObjectWriterStrategy.Instance, savingIsDoneExternally)
		{
		}

		protected override void Message_Saving(EDIMessage message)
		{
			base.Message_Saving(message);
			var table = new System.Data.DataTable("BlahBlah");
			var column = new System.Data.DataColumn("PK", typeof(Guid));
			table.Columns.Add(column);
			table.PrimaryKey = new[] { column };
			var dataRow = table.NewRow();
			var innerException = new Exception();
			var saveException = new ZSaveException(new ZDataException(innerException, dataRow, CargoWise.Data.Db.Connection), new BusinessObjectFactory());
			throw saveException;
		}
	}

	class AFRMessageGeneratorForTest2 : AFRMessageGenerator
	{
		public AFRMessageGeneratorForTest2(JPAFRHeader header, bool savingIsDoneExternally = false) : base(header, DefaultDataObjectWriterStrategy.Instance, savingIsDoneExternally)
		{
		}

		protected override void SaveInternally()
		{
			AddtionalActionForTest?.Invoke();
		}

		public Action AddtionalActionForTest;
	}
}
