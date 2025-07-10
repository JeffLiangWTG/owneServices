using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.DataTransfer.Phase4.Testing
{
	class NctsHeaderUniversalMessagingHelperTests : TestCaseWithFactory
	{
		[TestDate(2017, 07, 24)]
		public void TestSendInterchangeViaEHub()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			void SetupOfficeCode(UniversalReferenceTestDataHelper universalHelper, ZString officeCode, string officePurposeCode)
			{
				var dataGroupingCode = officeCode.Left(2);
				var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
				helper.CreateNewOrGetExistingDataGrouping(dataGroupingCode, parent: eunZZZ);
				universalHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office Code");
				universalHelper.CreateNewOrGetExistingCusCodeList(officeCode, dataGroupingCode, "Office Description" + officeCode, new ZString[] { officePurposeCode });

				universalHelper.CreateNewOrGetExistingRefCusCodeListAttributeName(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, dataGroupingCode);
			}

			helper.CreateNctsDeclarationTypeList();
			SetupOfficeCode(helper, "FR000060", EU.Business.OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
			SetupOfficeCode(helper, "NL025100", EU.Business.OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			Factory.Save();

			var nctsMovement = CreateNctsDepartureForTest(Factory);

			new NctsHeaderUniversalMessagingHelper().SendViaEHub(nctsMovement, new NctsMessageFunctionSet.DeclarationDataMessage());
			var queuedMessage = nctsMovement.Messages[0];
			AssertEquals("UDM", queuedMessage.EM_ApplicationCode);
			AssertEquals("XDC", queuedMessage.EM_MessageType);
			AssertEquals("XUS", queuedMessage.EM_MessageSubType);
			AssertEquals("TRX", queuedMessage.EM_ReceiveTransmit);
			AssertEquals("SNT", queuedMessage.EM_Status);
			AssertEquals("HQU", queuedMessage.Interchange.EI_Status);
			AssertEquals(EDIInterchangeTransportTypeList.Codes.eHub, queuedMessage.Interchange.EI_TransportType);
			AssertEquals("NCTS.FR", queuedMessage.Interchange.EI_To);
			AssertNotEquals("", queuedMessage.Interchange.eHubID);
			AssertMultilineASCIIEquals("queuedMessage.EM_MessageText", SampleUxml, queuedMessage.EM_MessageText);
			var interchange = queuedMessage.Interchange;
			var interchangeText = interchange.EI_BodyText;
			AssertContains("</UniversalShipment>", interchangeText);
		}

		public void TestSendDepartureViaEHub()
		{
			var nctsMovement = CreateNctsDepartureForTest(Factory);
			new NctsHeaderUniversalMessagingHelper().SendViaEHub(nctsMovement, new NctsMessageFunctionSet.DeclarationDataMessage());
			AssertNotNullOrEmpty(nctsMovement.Messages[0].EM_MessageText);
		}

		public void TestCaseMessageSendCanceled()
		{
			var nctsMovement = CreateNctsDepartureForTest(Factory);
			var errorMessage = new NctsHeaderUniversalMessagingHelperForTest().SendViaEHub(nctsMovement, new NctsMessageFunctionSet.DeclarationDataMessage());
			AssertEquals(nctsMovement.Messages.Count, 0);
			AssertEquals(errorMessage, "CanSendMessage method returns false");
		}
		public static NctsHeader CreateNctsDepartureForTest(BusinessObjectFactory factory)
		{
			var nctsMovement = factory.New<NctsHeader>();
			nctsMovement.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsMovement.SetMovementType(NctsMovementType.Codes.Departure);
			nctsMovement.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
			nctsMovement.BH_JobReference = "NCT12345678";
			NCTSTestHelper.CreateCustomsOfficeForTest(nctsMovement, EU.Business.OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "FR000060", ZDateTime.Empty, true);
			NCTSTestHelper.CreateCustomsOfficeForTest(nctsMovement, EU.Business.OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination, "NL025100", ZDateTime.Empty, false);
			CreateGoodsItemForTest(nctsMovement.MovementHeader.GoodsItems.AddNew());
			CreateGoodsItemForTest(nctsMovement.MovementHeader.GoodsItems.AddNew());
			CreateGoodsItemForTest(nctsMovement.MovementHeader.GoodsItems.AddNew());
			return nctsMovement;
		}

		public static NctsHeader CreateNctsArrivalForTest(BusinessObjectFactory factory)
		{
			var nctsMovement = factory.New<NctsHeader>();
			nctsMovement.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsMovement.SetMovementType(NctsMovementType.Codes.Arrival);
			var mrn = CusEntryNumber.LoadOrCreate(nctsMovement, CusEntryNumberTypes.Standard.MovementReferenceNumber, GlbBranch.CurrentBranch.Company.GC_RN_NKCountryCode);
			mrn.CE_EntryNum = "MRN123345678";
			NCTSTestHelper.CreateCustomsOfficeForTest(nctsMovement, EU.Business.OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "NL000060", ZDateTime.Empty, true);
			NCTSTestHelper.CreateCustomsOfficeForTest(nctsMovement, EU.Business.OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination, "FR025100", ZDateTime.Empty, false);
			return nctsMovement;
		}

		public static NctsCommonCargoDesc CreateGoodsItemForTest(NctsCommonCargoDesc goodsItem)
		{
			goodsItem.BY_Description = "Decription" + goodsItem.BY_LineNo;
			return goodsItem;
		}

		public static ZString SampleUxml
		{
			get
			{
				return
@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
    <DataContext>
      <DataSourceCollection>
        <DataSource>
          <Type>NctsHeader</Type>
          <Key>NCT12345678</Key>
        </DataSource>
      </DataSourceCollection>

      <ActionPurpose>
        <Code>015</Code>
        <Description></Description>
      </ActionPurpose>
      <TriggerCount>0</TriggerCount>
      <TriggerDescription></TriggerDescription>
      <TriggerType>Trigger</TriggerType>
    </DataContext>

    <AdditionalTerms></AdditionalTerms>
    <Branch>
      <Code>BNE</Code>
      <Name>BN - AUBNE</Name>
    </Branch>
    <CommercialInfo>
      <CommercialInvoiceCollection>
        <CommercialInvoice>
          <RelatedIndicator>
            <Code>D</Code>
            <Description>Departure</Description>
          </RelatedIndicator>

          <CommercialInvoiceLineCollection>
            <CommercialInvoiceLine>
              <LineNo>1</LineNo>
              <CountryOfExport>
                <Code></Code>
                <Name></Name>
              </CountryOfExport>
              <CountryOfOrigin>
                <Code></Code>
                <Name></Name>
              </CountryOfOrigin>
              <CustomsSecondQuantity>0</CustomsSecondQuantity>
              <CustomsSecondQuantityUnit>
                <Code></Code>
                <Description></Description>
              </CustomsSecondQuantityUnit>
              <CustomsValue>0</CustomsValue>
              <Description>Decription1</Description>
              <HarmonisedCode></HarmonisedCode>
              <NetWeight>0</NetWeight>
              <NetWeightUnit>
                <Code>KG</Code>
                <Description>Kilograms</Description>
              </NetWeightUnit>
              <Procedure></Procedure>
              <StateOfOrigin>
                <Code></Code>
              </StateOfOrigin>
              <TaxType>
                <Code></Code>
                <Description></Description>
              </TaxType>
              <Weight>0</Weight>
              <WeightUnit>
                <Code>KG</Code>
                <Description>Kilograms</Description>
              </WeightUnit>
            </CommercialInvoiceLine>
            <CommercialInvoiceLine>
              <LineNo>2</LineNo>
              <CountryOfExport>
                <Code></Code>
                <Name></Name>
              </CountryOfExport>
              <CountryOfOrigin>
                <Code></Code>
                <Name></Name>
              </CountryOfOrigin>
              <CustomsSecondQuantity>0</CustomsSecondQuantity>
              <CustomsSecondQuantityUnit>
                <Code></Code>
                <Description></Description>
              </CustomsSecondQuantityUnit>
              <CustomsValue>0</CustomsValue>
              <Description>Decription2</Description>
              <HarmonisedCode></HarmonisedCode>
              <NetWeight>0</NetWeight>
              <NetWeightUnit>
                <Code>KG</Code>
                <Description>Kilograms</Description>
              </NetWeightUnit>
              <Procedure></Procedure>
              <StateOfOrigin>
                <Code></Code>
              </StateOfOrigin>
              <TaxType>
                <Code></Code>
                <Description></Description>
              </TaxType>
              <Weight>0</Weight>
              <WeightUnit>
                <Code>KG</Code>
                <Description>Kilograms</Description>
              </WeightUnit>
            </CommercialInvoiceLine>
            <CommercialInvoiceLine>
              <LineNo>3</LineNo>
              <CountryOfExport>
                <Code></Code>
                <Name></Name>
              </CountryOfExport>
              <CountryOfOrigin>
                <Code></Code>
                <Name></Name>
              </CountryOfOrigin>
              <CustomsSecondQuantity>0</CustomsSecondQuantity>
              <CustomsSecondQuantityUnit>
                <Code></Code>
                <Description></Description>
              </CustomsSecondQuantityUnit>
              <CustomsValue>0</CustomsValue>
              <Description>Decription3</Description>
              <HarmonisedCode></HarmonisedCode>
              <NetWeight>0</NetWeight>
              <NetWeightUnit>
                <Code>KG</Code>
                <Description>Kilograms</Description>
              </NetWeightUnit>
              <Procedure></Procedure>
              <StateOfOrigin>
                <Code></Code>
              </StateOfOrigin>
              <TaxType>
                <Code></Code>
                <Description></Description>
              </TaxType>
              <Weight>0</Weight>
              <WeightUnit>
                <Code>KG</Code>
                <Description>Kilograms</Description>
              </WeightUnit>
            </CommercialInvoiceLine>
          </CommercialInvoiceLineCollection>
        </CommercialInvoice>
      </CommercialInvoiceCollection>
    </CommercialInfo>
    <CustomsBroker>
      <Code></Code>
      <Name>CargoWise Support</Name>
    </CustomsBroker>
    <CustomsOffice>
      <Code>Brisbane</Code>
      <Description>Brisbane</Description>
    </CustomsOffice>
    <CustomsProfileIdentifier>
      <Type>UserName</Type>
      <Value></Value>
    </CustomsProfileIdentifier>
    <DeliveryMode>
      <Code></Code>
    </DeliveryMode>
    <LocationAtClearance>
      <Code></Code>
      <Description>Customs Sub Place</Description>
    </LocationAtClearance>
    <MessageSubType>
      <Code>D</Code>
    </MessageSubType>
    <MessageType>
      <Code>T1</Code>
      <Description>Goods moving under external Community transit procedure</Description>
    </MessageType>
    <MessagingApplicationCode>
      <Code>NCT</Code>
      <Description>NCTS Phase 4</Description>
    </MessagingApplicationCode>
    <OwnerRef>NCT12345678</OwnerRef>
    <PaymentMethod>
      <Code></Code>
    </PaymentMethod>
    <PortOfDestination>
      <Code></Code>
    </PortOfDestination>
    <PortOfDischarge>
      <Code></Code>
    </PortOfDischarge>
    <PortOfLoading>
      <Code></Code>
    </PortOfLoading>
    <PortOfOrigin>
      <Code></Code>
    </PortOfOrigin>
    <SealInfo>
      <Quantity>0</Quantity>
      <Type>
        <Code></Code>
      </Type>
    </SealInfo>
    <TotalNoOfPacks>0</TotalNoOfPacks>
    <TotalWeight>0</TotalWeight>
    <TotalWeightUnit>
      <Code>KG</Code>
      <Description>Kilograms</Description>
    </TotalWeightUnit>
    <TransportMode>
      <Code></Code>
    </TransportMode>
    <VesselCountryOfRegistration>
      <Code></Code>
    </VesselCountryOfRegistration>
    <VesselName></VesselName>
    <VoyageFlightNo></VoyageFlightNo>

    <AddInfoCollection>
      <AddInfo>
        <Key>SecurityIndicator</Key>
        <Value>N</Value>
      </AddInfo>
    </AddInfoCollection>

    <ContainerCollection>
    </ContainerCollection>

    <CustomsReferenceCollection>
      <CustomsReference>
        <Type>
          <Code>COM</Code>
          <Description>Commercial Reference Number</Description>
        </Type>
        <Reference></Reference>
      </CustomsReference>
      <CustomsReference>
        <Type>
          <Code>EUO</Code>
          <Description>Office Code</Description>
        </Type>
        <IsOverridden>false</IsOverridden>
        <Order>1</Order>
        <Reference>FR000060</Reference>
        <ReferencedEntityDescription>Office DescriptionFR000060</ReferencedEntityDescription>
        <SubType>
          <Code>DEP</Code>
          <Description>NCTS Office of departure</Description>
        </SubType>

        <DateCollection>
          <Date>
            <Type>DateAtOffice</Type>
            <Value></Value>
          </Date>
        </DateCollection>
      </CustomsReference>
      <CustomsReference>
        <Type>
          <Code>EUO</Code>
          <Description>Office Code</Description>
        </Type>
        <IsOverridden>false</IsOverridden>
        <Order>1</Order>
        <Reference>NL025100</Reference>
        <ReferencedEntityDescription>Office DescriptionNL025100</ReferencedEntityDescription>
        <SubType>
          <Code>DES</Code>
          <Description>NCTS Office of destination</Description>
        </SubType>

        <DateCollection>
          <Date>
            <Type>DateAtOffice</Type>
            <Value></Value>
          </Date>
        </DateCollection>
      </CustomsReference>
    </CustomsReferenceCollection>

    <DateCollection>
      <Date>
        <Type>Departure</Type>
        <IsEstimate>false</IsEstimate>
        <Value></Value>
      </Date>
    </DateCollection>

    <EntryNumberCollection>
    </EntryNumberCollection>

    <PackingLineCollection Content=""Complete"">
    </PackingLineCollection>
  </Shipment>
</UniversalShipment>";
			}
		}

		class NctsHeaderUniversalMessagingHelperForTest : NctsHeaderUniversalMessagingHelper
		{
			protected override bool CanSendMessage(XmlEDIMessage message, StringBuilder errMsg)
			{
				errMsg.Append("CanSendMessage method returns false");
				return false;
			}
		}
	}
}
