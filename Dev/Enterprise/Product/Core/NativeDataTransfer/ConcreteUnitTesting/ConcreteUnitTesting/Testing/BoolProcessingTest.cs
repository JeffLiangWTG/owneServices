using System;
using System.IO;
using System.Text;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Native.Adapter;
using Enterprise.DataTransfer.Native.Adapter.Utils;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.DataTransfer.Native.Utils;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting.Testing
{
	public class BoolProcessingTest : TestCaseWithFactory
	{
		public void TestExportImportOrgHeaderNativeXml()
		{
			if (!EntitySetDefinitionCache.GetInstance().Definitions.IsEmpty())
			{
				StringBuilder definitionBuilder = new StringBuilder();
				StringBuilder assemblyBuilder = new StringBuilder();
				foreach (string assembly in EntitySetDefinitionCache.GetInstance().DefinitionLocator.assemblyNames)
				{
					assemblyBuilder.Append(assembly).Append(" ");
				}
				definitionBuilder.Append("\r\n");
				foreach (EntitySetDefinition definition in EntitySetDefinitionCache.GetInstance().Definitions)
				{
					definitionBuilder.Append(definition.Name).Append(". ")
						.Append(definition.Root).Append(". ")
						.Append(definition.GetXSDFileName()).Append(". ")
						.Append("\r\n");
				}
				throw new Exception(string.Format("EntitySetDefinitionCache had these assemblies defined before test :- {0} \r\n and these definitions defined before test :- {1}", assemblyBuilder.ToString(), definitionBuilder.ToString()));
			}

			var orgHeader = Factory.New<OrgHeader>();

			orgHeader.OH_FullName = "FROSTY ICE CREAM";
			orgHeader.OH_RL_NKClosestPort = "USHBO";
			orgHeader.OH_Code = "FROICEHBO";
			orgHeader.OH_IsLocalTransport = true;
			orgHeader.OH_IsConsignee = true;
			Factory.Save();

			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };

			using (var dataStream = xmlSerializer.SerializeToStream(orgHeader))
			{
				var dataText = new StreamReader(dataStream).ReadToEnd();
				AssertNotEquals("Check that bool exists", -1, dataText.IndexOf(">true<"));
				dataStream.Position = 0;

				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(dataStream);
				var insertLog = manager.GetLogs();
				AssertEquals("Import should be successful", -1, insertLog.IndexOf("Failed to Import:"));
			}
		}

		public void TestExportImportOrderNativeXml()
		{
			var order = Factory.NewWithValidTestData<Order>();
			Factory.Save();

			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var xmlSerializer = new NativeXmlSerializer() { Converter = converter };

			using (var dataStream = xmlSerializer.SerializeToStream(order))
			{
				var dataText = new StreamReader(dataStream).ReadToEnd();
				AssertNotEquals("Check that bool exists", -1, dataText.IndexOf(">true<"));
				dataStream.Position = 0;

				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(dataStream);
				var insertLog = manager.GetLogs();
				AssertEquals("Import should be successful", -1, insertLog.IndexOf("Failed to Import:"));
			}
		}

		public void TestOrderImportFromNativeXml()
		{
			using (var dataStream = new MemoryStream(Encoding.UTF8.GetBytes(XmlOrderNativeXml)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(dataStream);
				var insertLog = manager.GetLogs();
				AssertEquals("Import should be successful", -1, insertLog.IndexOf("Failed to Import:"));
			}
		}

		#region XmlOrderNativeXml

		const string XmlOrderNativeXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""1.0"">
  <Header>
    <OwnerCode>AUDEMOSDX</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Order version=""1.0"">
      <JobOrderHeader Action=""MERGE"">
        <IsCancelled>false</IsCancelled>
        <FirstBuyerContact>AMM1</FirstBuyerContact>
        <SecondBuyerContact></SecondBuyerContact>
        <OrderNumber>11111</OrderNumber>
        <OrderNumberSplit>1</OrderNumberSplit>
        <OrderDate>2010-01-19T00:00:00</OrderDate>
        <OrderStatus>CNF</OrderStatus>
        <OrderGoodsDescription></OrderGoodsDescription>
        <TransportMode>SEA</TransportMode>
        <ContainerMode>LCL</ContainerMode>
        <BookingConfRef></BookingConfRef>
        <BookingConfDate></BookingConfDate>
        <ExWorksRequiredBy></ExWorksRequiredBy>
        <DeliveryRequiredBy>2010-03-18T00:00:00</DeliveryRequiredBy>
        <QuoteNumber></QuoteNumber>
        <InvoiceNumber></InvoiceNumber>
        <InvoiceDate></InvoiceDate>
        <EstimatedExchangeRate>1.000000000</EstimatedExchangeRate>
        <IncoTerm>FOB</IncoTerm>
        <AdditionalTerms></AdditionalTerms>
        <DepartureVoyage></DepartureVoyage>
        <DepartureVesselCutoffDate></DepartureVesselCutoffDate>
        <IntermediateVoyage></IntermediateVoyage>
        <ArrivalVoyage></ArrivalVoyage>
        <ActualVolume>0.000</ActualVolume>
        <UnitOfVolume>M3</UnitOfVolume>
        <ActualWeight>0.000</ActualWeight>
        <UnitOfWeight>KG</UnitOfWeight>
        <Waybill></Waybill>
        <MasterWaybill></MasterWaybill>
        <Packs>0</Packs>
        <E_ARV_2ndIntermediate></E_ARV_2ndIntermediate>
        <E_ARV_1stIntermediate></E_ARV_1stIntermediate>
        <E_DEP_2></E_DEP_2>
        <E_DEP_3></E_DEP_3>
        <EstimateUserDate1></EstimateUserDate1>
        <EstimateUserDate2></EstimateUserDate2>
        <EstimateUserDate3></EstimateUserDate3>
        <EstimateUserDate4></EstimateUserDate4>
        <ActualUserDate1></ActualUserDate1>
        <ActualUserDate2></ActualUserDate2>
        <ActualUserDate3></ActualUserDate3>
        <ActualUserDate4></ActualUserDate4>
        <FollowUpDate></FollowUpDate>
        <CustomAttrib1></CustomAttrib1>
        <CustomAttrib2></CustomAttrib2>
        <CustomAttrib3></CustomAttrib3>
        <CustomAttrib4></CustomAttrib4>
        <CustomAttrib5></CustomAttrib5>
        <CustomDate1></CustomDate1>
        <CustomDate2></CustomDate2>
        <CustomDecimal1>0.000</CustomDecimal1>
        <CustomDecimal2>0.000</CustomDecimal2>
        <CustomDecimal3>0.000</CustomDecimal3>
        <CustomDecimal4>0.000</CustomDecimal4>
        <CustomDecimal5>0.000</CustomDecimal5>
        <CustomFlag1>true</CustomFlag1>
        <CustomFlag2>false</CustomFlag2>
        <CustomFlag3>false</CustomFlag3>
        <CustomFlag4>false</CustomFlag4>
        <CustomFlag5>false</CustomFlag5>
        <OrderType>STD</OrderType>
        <JobOrderLineCollection>
          <JobOrderLine Action=""MERGE"">
            <LineNo>200</LineNo>
            <SubLineNo>1</SubLineNo>
            <Partno>07111287E</Partno>
            <PartAttrib1></PartAttrib1>
            <PartAttrib2></PartAttrib2>
            <PartAttrib3></PartAttrib3>
            <Description>LUSTALENE        DK NAVY</Description>
            <Quantity>1100.00000</Quantity>
            <OrderUnitOfQty></OrderUnitOfQty>
            <InnerPacks>0.000</InnerPacks>
            <OuterPacks>0.000</OuterPacks>
            <ActualVolume>0.000</ActualVolume>
            <UnitOfVolume></UnitOfVolume>
            <ActualWeight>0.000</ActualWeight>
            <UnitOfWeight></UnitOfWeight>
            <ItemPrice>2.3000</ItemPrice>
            <LinePrice>2530.0000</LinePrice>
            <LineDropDate>2010-03-18T00:00:00</LineDropDate>
            <QtyInvoiced>0.00000</QtyInvoiced>
            <QtyReceived>0.00000</QtyReceived>
            <LineStatus>CNF</LineStatus>
            <SpecialInstructions></SpecialInstructions>
            <AdditionalInformation></AdditionalInformation>
            <CustomAttrib1>ACP</CustomAttrib1>
            <CustomAttrib2></CustomAttrib2>
            <CustomAttrib3></CustomAttrib3>
            <CustomAttrib4></CustomAttrib4>
            <CustomAttrib5></CustomAttrib5>
            <CustomAttrib6></CustomAttrib6>
            <CustomTextBlob1></CustomTextBlob1>
            <CustomFlag1>false</CustomFlag1>
            <CustomFlag2>false</CustomFlag2>
            <CustomFlag3>false</CustomFlag3>
            <CustomFlag4>false</CustomFlag4>
            <CustomFlag5>false</CustomFlag5>
            <CustomDate1></CustomDate1>
            <CustomDate2></CustomDate2>
            <CustomDate3></CustomDate3>
            <CustomDate4></CustomDate4>
            <CustomDate5></CustomDate5>
            <CustomDecimal1>0.000</CustomDecimal1>
            <CustomDecimal2>0.000</CustomDecimal2>
            <CustomDecimal3>0.000</CustomDecimal3>
            <CustomDecimal4>0.000</CustomDecimal4>
            <CustomDecimal5>0.000</CustomDecimal5>
            <ContainerNumber></ContainerNumber>
            <ContainerPackingOrder>0</ContainerPackingOrder>
            <CommercialInvoiceNo></CommercialInvoiceNo>
            <LineSplitNumber>0</LineSplitNumber>
            <ItemPriceFactor>1</ItemPriceFactor>
            <INCO></INCO>
            <AdditionalTerms></AdditionalTerms>
            <ConfirmationNum></ConfirmationNum>
            <HSCode></HSCode>
            <EANUPC></EANUPC>
            <ConfirmationDate></ConfirmationDate>
            <InnerPacksUQ></InnerPacksUQ>
            <OuterPacksUQ></OuterPacksUQ>
            <ExWorksDate></ExWorksDate>
            <JobOrderLineDeliveryCollection>
              <JobOrderLineDelivery Action=""MERGE"">
                <Allocated>0.00000</Allocated>
                <CustomAttribute1></CustomAttribute1>
                <CustomAttribute2></CustomAttribute2>
                <CustomAttribute3></CustomAttribute3>
                <CustomAttribute4></CustomAttribute4>
                <CustomAttribute5></CustomAttribute5>
                <CustomFlag1>false</CustomFlag1>
                <CustomFlag2>false</CustomFlag2>
                <CustomFlag3>false</CustomFlag3>
                <CustomFlag4>false</CustomFlag4>
                <CustomFlag5>false</CustomFlag5>
                <CustomDate1></CustomDate1>
                <CustomDate2></CustomDate2>
                <CustomDate3></CustomDate3>
                <CustomDate4></CustomDate4>
                <CustomDate5></CustomDate5>
                <CustomDecimal1>0.000</CustomDecimal1>
                <CustomDecimal2>0.000</CustomDecimal2>
                <CustomDecimal3>0.000</CustomDecimal3>
                <CustomDecimal4>0.000</CustomDecimal4>
                <CustomDecimal5>0.000</CustomDecimal5>
                <DestinationPort TableName=""RefUNLOCO"">
                  <Code>AUMEL</Code>
                </DestinationPort>
              </JobOrderLineDelivery>
            </JobOrderLineDeliveryCollection>
          </JobOrderLine>
        </JobOrderLineCollection>
        <Supplier TableName=""OrgHeader"">
          <Code>ABABEU</Code>
          <IsActive>true</IsActive>
          <FullName>ABA BEUL</FullName>
          <IsConsignee>1</IsConsignee>
          <IsConsignor>true</IsConsignor>
          <IsTransportClient>false</IsTransportClient>
          <IsWarehouseClient>false</IsWarehouseClient>
          <IsForwarder>false</IsForwarder>
          <IsShippingProvider>false</IsShippingProvider>
          <IsAirWholesaler>false</IsAirWholesaler>
          <IsSeaWholesaler>false</IsSeaWholesaler>
          <IsRailProvider>false</IsRailProvider>
          <IsLineHaulProvider>false</IsLineHaulProvider>
          <IsMiscFreightServices>false</IsMiscFreightServices>
          <IsAirCTO>false</IsAirCTO>
          <IsAirLine>false</IsAirLine>
          <IsBroker>false</IsBroker>
          <IsContainerPark>false</IsContainerPark>
          <IsLocalTransport>false</IsLocalTransport>
          <IsPackDepot>false</IsPackDepot>
          <IsSeaCTO>false</IsSeaCTO>
          <IsShippingLine>false</IsShippingLine>
          <IsUnpackDepot>false</IsUnpackDepot>
          <IsRailHead>false</IsRailHead>
          <IsRoadFreightDepot>false</IsRoadFreightDepot>
          <IsShippingConsortium>false</IsShippingConsortium>
          <IsFumigationContractor>false</IsFumigationContractor>
          <IsNationalAccount>false</IsNationalAccount>
          <IsSalesLead>false</IsSalesLead>
          <IsMailOut>false</IsMailOut>
          <IsFaxUpdate>false</IsFaxUpdate>
          <IsEmailUpdate>false</IsEmailUpdate>
          <IsNewsLetter>false</IsNewsLetter>
          <IsCompetitor>false</IsCompetitor>
          <IsTempAccount>false</IsTempAccount>
          <IsPersonalEffectsAccount>false</IsPersonalEffectsAccount>
          <IsUserFlag1>false</IsUserFlag1>
          <IsUserFlag2>false</IsUserFlag2>
          <IsUserFlag3>false</IsUserFlag3>
          <IsUserFlag4>false</IsUserFlag4>
          <IsUserFlag5>false</IsUserFlag5>
          <IsUserFlag6>false</IsUserFlag6>
          <IsUserFlag7>false</IsUserFlag7>
          <IsUserFlag8>false</IsUserFlag8>
          <Prospect>true</Prospect>
          <PartialBusiness>false</PartialBusiness>
          <Language>EN</Language>
          <IsGlobalAccount>false</IsGlobalAccount>
          <ScreeningStatus>CLR</ScreeningStatus>
          <SupplierAddressCollection TableName=""OrgAddress"">
            <SupplierAddress>
              <IsActive>true</IsActive>
              <Code>123 FIRST ST</Code>
              <Language>EN</Language>
              <CompanyNameOverride></CompanyNameOverride>
              <Address1>123 FIRST ST</Address1>
              <Address2></Address2>
              <City>WELLINGTON</City>
              <State>WGN</State>
              <PostCode>2658</PostCode>
              <Phone></Phone>
              <Fax></Fax>
              <Mobile></Mobile>
              <Email></Email>
              <PickupFromTimeOnly></PickupFromTimeOnly>
              <PickupToTimeOnly></PickupToTimeOnly>
              <DeliverFromTimeOnly></DeliverFromTimeOnly>
              <DeliverToTimeOnly></DeliverToTimeOnly>
              <DoNotAttendFrom></DoNotAttendFrom>
              <DoNotAttendTo></DoNotAttendTo>
              <DockLeveler>false</DockLeveler>
              <ForkLift>false</ForkLift>
              <PalletJack>false</PalletJack>
              <ContainerHandling></ContainerHandling>
              <AccessPoint></AccessPoint>
              <LabourRequired></LabourRequired>
              <CommunicationRequired></CommunicationRequired>
              <Dock_Height></Dock_Height>
              <OtherWarehouseFacilities></OtherWarehouseFacilities>
              <LoadingUnloadingConstraints></LoadingUnloadingConstraints>
              <FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
              <LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
              <AIREquipmentNeeded>PSL</AIREquipmentNeeded>
              <Latitude>0.00000</Latitude>
              <Longitude>0.00000</Longitude>
              <RelatedPortCode TableName=""RefUNLOCO"">
                <Code>NZWLG</Code>
              </RelatedPortCode>
            </SupplierAddress>
            <SupplierAddress>
              <IsActive>true</IsActive>
              <Code>Pick Up Address</Code>
              <Language>EN</Language>
              <CompanyNameOverride></CompanyNameOverride>
              <Address1>DIESLSTR 11</Address1>
              <Address2></Address2>
              <City>ATTENDORN?, GERMANY</City>
              <State>BE</State>
              <PostCode>57439</PostCode>
              <Phone></Phone>
              <Fax></Fax>
              <Mobile></Mobile>
              <Email></Email>
              <PickupFromTimeOnly></PickupFromTimeOnly>
              <PickupToTimeOnly></PickupToTimeOnly>
              <DeliverFromTimeOnly></DeliverFromTimeOnly>
              <DeliverToTimeOnly></DeliverToTimeOnly>
              <DoNotAttendFrom></DoNotAttendFrom>
              <DoNotAttendTo></DoNotAttendTo>
              <DockLeveler>false</DockLeveler>
              <ForkLift>false</ForkLift>
              <PalletJack>false</PalletJack>
              <ContainerHandling></ContainerHandling>
              <AccessPoint></AccessPoint>
              <LabourRequired></LabourRequired>
              <CommunicationRequired></CommunicationRequired>
              <Dock_Height></Dock_Height>
              <OtherWarehouseFacilities></OtherWarehouseFacilities>
              <LoadingUnloadingConstraints></LoadingUnloadingConstraints>
              <FCLEquipmentNeeded>LOF</FCLEquipmentNeeded>
              <LCLEquipmentNeeded>HSL</LCLEquipmentNeeded>
              <AIREquipmentNeeded>HSL</AIREquipmentNeeded>
              <Latitude>0.00000</Latitude>
              <Longitude>0.00000</Longitude>
            </SupplierAddress>
            <SupplierAddress>
              <IsActive>true</IsActive>
              <Code>PST: DIESLSTR 11</Code>
              <Language>EN</Language>
              <CompanyNameOverride></CompanyNameOverride>
              <Address1>DIESLSTR 11</Address1>
              <Address2>57439 ATTENDORN, GERMANY</Address2>
              <City>57439 ATTENDORN, GERMANY</City>
              <State>BE</State>
              <PostCode>2222</PostCode>
              <Phone></Phone>
              <Fax></Fax>
              <Mobile></Mobile>
              <Email></Email>
              <PickupFromTimeOnly></PickupFromTimeOnly>
              <PickupToTimeOnly></PickupToTimeOnly>
              <DeliverFromTimeOnly></DeliverFromTimeOnly>
              <DeliverToTimeOnly></DeliverToTimeOnly>
              <DoNotAttendFrom></DoNotAttendFrom>
              <DoNotAttendTo></DoNotAttendTo>
              <DockLeveler>false</DockLeveler>
              <ForkLift>false</ForkLift>
              <PalletJack>false</PalletJack>
              <ContainerHandling></ContainerHandling>
              <AccessPoint></AccessPoint>
              <LabourRequired></LabourRequired>
              <CommunicationRequired></CommunicationRequired>
              <Dock_Height></Dock_Height>
              <OtherWarehouseFacilities></OtherWarehouseFacilities>
              <LoadingUnloadingConstraints></LoadingUnloadingConstraints>
              <FCLEquipmentNeeded>LOF</FCLEquipmentNeeded>
              <LCLEquipmentNeeded>HSL</LCLEquipmentNeeded>
              <AIREquipmentNeeded>HSL</AIREquipmentNeeded>
              <Latitude>0.00000</Latitude>
              <Longitude>0.00000</Longitude>
              <RelatedPortCode TableName=""RefUNLOCO"">
                <Code>DEFRA</Code>
              </RelatedPortCode>
            </SupplierAddress>
          </SupplierAddressCollection>
          <ClosestPort TableName=""RefUNLOCO"">
            <Code>DEFRA</Code>
          </ClosestPort>
        </Supplier>
        <Buyer TableName=""OrgHeader"">
          <Code>ABIGASNCA1</Code>
          <IsActive>true</IsActive>
          <FullName>ABI GAS &amp; TOOLS &amp; UNIONPOWER LOGISTICS (CHINA) LTD</FullName>
          <IsConsignee>true</IsConsignee>
          <IsConsignor>true</IsConsignor>
          <IsTransportClient>true</IsTransportClient>
          <IsWarehouseClient>true</IsWarehouseClient>
          <IsForwarder>true</IsForwarder>
          <IsShippingProvider>false</IsShippingProvider>
          <IsAirWholesaler>false</IsAirWholesaler>
          <IsSeaWholesaler>false</IsSeaWholesaler>
          <IsRailProvider>false</IsRailProvider>
          <IsLineHaulProvider>false</IsLineHaulProvider>
          <IsMiscFreightServices>false</IsMiscFreightServices>
          <IsAirCTO>false</IsAirCTO>
          <IsAirLine>false</IsAirLine>
          <IsBroker>false</IsBroker>
          <IsContainerPark>false</IsContainerPark>
          <IsLocalTransport>false</IsLocalTransport>
          <IsPackDepot>false</IsPackDepot>
          <IsSeaCTO>false</IsSeaCTO>
          <IsShippingLine>false</IsShippingLine>
          <IsUnpackDepot>false</IsUnpackDepot>
          <IsRailHead>false</IsRailHead>
          <IsRoadFreightDepot>false</IsRoadFreightDepot>
          <IsShippingConsortium>false</IsShippingConsortium>
          <IsFumigationContractor>false</IsFumigationContractor>
          <IsNationalAccount>false</IsNationalAccount>
          <IsSalesLead>false</IsSalesLead>
          <IsMailOut>false</IsMailOut>
          <IsFaxUpdate>false</IsFaxUpdate>
          <IsEmailUpdate>false</IsEmailUpdate>
          <IsNewsLetter>false</IsNewsLetter>
          <IsCompetitor>false</IsCompetitor>
          <IsTempAccount>false</IsTempAccount>
          <IsPersonalEffectsAccount>false</IsPersonalEffectsAccount>
          <IsUserFlag1>false</IsUserFlag1>
          <IsUserFlag2>false</IsUserFlag2>
          <IsUserFlag3>false</IsUserFlag3>
          <IsUserFlag4>false</IsUserFlag4>
          <IsUserFlag5>false</IsUserFlag5>
          <IsUserFlag6>false</IsUserFlag6>
          <IsUserFlag7>false</IsUserFlag7>
          <IsUserFlag8>false</IsUserFlag8>
          <Prospect>true</Prospect>
          <PartialBusiness>false</PartialBusiness>
          <Language>EN</Language>
          <IsGlobalAccount>false</IsGlobalAccount>
          <ScreeningStatus>UNK</ScreeningStatus>
          <BuyerAddressCollection TableName=""OrgAddress"">
            <BuyerAddress>
              <IsActive>true</IsActive>
              <Code>1128</Code>
              <Language>EN</Language>
              <CompanyNameOverride></CompanyNameOverride>
              <Address1>171 ABBOTSFORD ROAD</Address1>
              <Address2></Address2>
              <City>MAYNE</City>
              <State>QLD</State>
              <PostCode></PostCode>
              <Phone></Phone>
              <Fax></Fax>
              <Mobile></Mobile>
              <Email></Email>
              <PickupFromTimeOnly></PickupFromTimeOnly>
              <PickupToTimeOnly></PickupToTimeOnly>
              <DeliverFromTimeOnly></DeliverFromTimeOnly>
              <DeliverToTimeOnly></DeliverToTimeOnly>
              <DoNotAttendFrom></DoNotAttendFrom>
              <DoNotAttendTo></DoNotAttendTo>
              <DockLeveler>false</DockLeveler>
              <ForkLift>false</ForkLift>
              <PalletJack>false</PalletJack>
              <ContainerHandling></ContainerHandling>
              <AccessPoint></AccessPoint>
              <LabourRequired></LabourRequired>
              <CommunicationRequired></CommunicationRequired>
              <Dock_Height></Dock_Height>
              <OtherWarehouseFacilities></OtherWarehouseFacilities>
              <LoadingUnloadingConstraints></LoadingUnloadingConstraints>
              <FCLEquipmentNeeded>LOF</FCLEquipmentNeeded>
              <LCLEquipmentNeeded>HSL</LCLEquipmentNeeded>
              <AIREquipmentNeeded>HSL</AIREquipmentNeeded>
              <Latitude>0.00000</Latitude>
              <Longitude>0.00000</Longitude>
              <RelatedPortCode TableName=""RefUNLOCO"">
                <Code>AUSYD</Code>
              </RelatedPortCode>
            </BuyerAddress>
            <BuyerAddress>
              <IsActive>true</IsActive>
              <Code>20111</Code>
              <Language>EN</Language>
              <CompanyNameOverride></CompanyNameOverride>
              <Address1>ADDRESS1</Address1>
              <Address2></Address2>
              <City>AAAA</City>
              <State>NSW</State>
              <PostCode>2222</PostCode>
              <Phone></Phone>
              <Fax></Fax>
              <Mobile></Mobile>
              <Email></Email>
              <PickupFromTimeOnly></PickupFromTimeOnly>
              <PickupToTimeOnly></PickupToTimeOnly>
              <DeliverFromTimeOnly></DeliverFromTimeOnly>
              <DeliverToTimeOnly></DeliverToTimeOnly>
              <DoNotAttendFrom></DoNotAttendFrom>
              <DoNotAttendTo></DoNotAttendTo>
              <DockLeveler>false</DockLeveler>
              <ForkLift>false</ForkLift>
              <PalletJack>false</PalletJack>
              <ContainerHandling></ContainerHandling>
              <AccessPoint></AccessPoint>
              <LabourRequired></LabourRequired>
              <CommunicationRequired></CommunicationRequired>
              <Dock_Height></Dock_Height>
              <OtherWarehouseFacilities></OtherWarehouseFacilities>
              <LoadingUnloadingConstraints></LoadingUnloadingConstraints>
              <FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
              <LCLEquipmentNeeded>HSL</LCLEquipmentNeeded>
              <AIREquipmentNeeded>PSL</AIREquipmentNeeded>
              <Latitude>0.00000</Latitude>
              <Longitude>0.00000</Longitude>
              <RelatedPortCode TableName=""RefUNLOCO"">
                <Code>AUSYD</Code>
              </RelatedPortCode>
            </BuyerAddress>
            <BuyerAddress>
              <IsActive>true</IsActive>
              <Code>40BNE</Code>
              <Language>EN</Language>
              <CompanyNameOverride></CompanyNameOverride>
              <Address1>171 ABBOTSFORD ROAD</Address1>
              <Address2>171 ABBOTSFORD ROAD</Address2>
              <City>MAYNE</City>
              <State>11</State>
              <PostCode>4006</PostCode>
              <Phone>11115555</Phone>
              <Fax></Fax>
              <Mobile></Mobile>
              <Email></Email>
              <PickupFromTimeOnly></PickupFromTimeOnly>
              <PickupToTimeOnly></PickupToTimeOnly>
              <DeliverFromTimeOnly></DeliverFromTimeOnly>
              <DeliverToTimeOnly></DeliverToTimeOnly>
              <DoNotAttendFrom></DoNotAttendFrom>
              <DoNotAttendTo></DoNotAttendTo>
              <DockLeveler>false</DockLeveler>
              <ForkLift>false</ForkLift>
              <PalletJack>false</PalletJack>
              <ContainerHandling></ContainerHandling>
              <AccessPoint></AccessPoint>
              <LabourRequired></LabourRequired>
              <CommunicationRequired></CommunicationRequired>
              <Dock_Height></Dock_Height>
              <OtherWarehouseFacilities></OtherWarehouseFacilities>
              <LoadingUnloadingConstraints></LoadingUnloadingConstraints>
              <FCLEquipmentNeeded>LOF</FCLEquipmentNeeded>
              <LCLEquipmentNeeded>HSL</LCLEquipmentNeeded>
              <AIREquipmentNeeded>HSL</AIREquipmentNeeded>
              <Latitude>0.00000</Latitude>
              <Longitude>0.00000</Longitude>
              <RelatedPortCode TableName=""RefUNLOCO"">
                <Code>CNNCA</Code>
              </RelatedPortCode>
            </BuyerAddress>
          </BuyerAddressCollection>
          <BuyerContactCollection TableName=""OrgContact"">
            <BuyerContact>
              <IsActive>true</IsActive>
              <ContactName>LYNN MCVIE</ContactName>
              <Salutation></Salutation>
              <Language>EN</Language>
              <NotifyMode>PRN</NotifyMode>
              <Title></Title>
              <JobCategory></JobCategory>
              <Phone></Phone>
              <PhoneExtension></PhoneExtension>
              <Fax></Fax>
              <Mobile></Mobile>
              <HomePhone></HomePhone>
              <Pager></Pager>
              <OtherPhone></OtherPhone>
              <Email></Email>
              <AttachmentType></AttachmentType>
              <WebAccessEnabled>false</WebAccessEnabled>
              <Password></Password>
              <Birthday></Birthday>
              <YearJoinedIndustry></YearJoinedIndustry>
              <YearJoinedCompany></YearJoinedCompany>
              <ContactSource></ContactSource>
              <DetailsVerified></DetailsVerified>
              <PersonalInfo></PersonalInfo>
              <WebContractSignedDate></WebContractSignedDate>
              <Gender></Gender>
              <ProfilePhoto></ProfilePhoto>
            </BuyerContact>
            <BuyerContact>
              <IsActive>true</IsActive>
              <ContactName>AMM1</ContactName>
              <Salutation></Salutation>
              <Language>EN</Language>
              <NotifyMode>EML</NotifyMode>
              <Title></Title>
              <JobCategory></JobCategory>
              <Phone></Phone>
              <PhoneExtension></PhoneExtension>
              <Fax></Fax>
              <Mobile></Mobile>
              <HomePhone></HomePhone>
              <Pager></Pager>
              <OtherPhone></OtherPhone>
              <Email>test@edi.com.au</Email>
              <AttachmentType>PDF</AttachmentType>
              <WebAccessEnabled>false</WebAccessEnabled>
              <Password></Password>
              <Birthday></Birthday>
              <YearJoinedIndustry></YearJoinedIndustry>
              <YearJoinedCompany></YearJoinedCompany>
              <ContactSource></ContactSource>
              <DetailsVerified></DetailsVerified>
              <PersonalInfo></PersonalInfo>
              <WebContractSignedDate></WebContractSignedDate>
              <Gender></Gender>
              <ProfilePhoto></ProfilePhoto>
            </BuyerContact>
            <BuyerContact>
              <IsActive>true</IsActive>
              <ContactName>asdfas</ContactName>
              <Salutation></Salutation>
              <Language>EN</Language>
              <NotifyMode>EML</NotifyMode>
              <Title></Title>
              <JobCategory></JobCategory>
              <Phone></Phone>
              <PhoneExtension></PhoneExtension>
              <Fax></Fax>
              <Mobile></Mobile>
              <HomePhone></HomePhone>
              <Pager></Pager>
              <OtherPhone></OtherPhone>
              <Email></Email>
              <AttachmentType>PDF</AttachmentType>
              <WebAccessEnabled>false</WebAccessEnabled>
              <Password></Password>
              <Birthday></Birthday>
              <YearJoinedIndustry></YearJoinedIndustry>
              <YearJoinedCompany></YearJoinedCompany>
              <ContactSource></ContactSource>
              <DetailsVerified></DetailsVerified>
              <PersonalInfo></PersonalInfo>
              <WebContractSignedDate></WebContractSignedDate>
              <Gender></Gender>
              <ProfilePhoto></ProfilePhoto>
            </BuyerContact>
          </BuyerContactCollection>
          <ClosestPort TableName=""RefUNLOCO"">
            <Code>CNNCA</Code>
          </ClosestPort>
        </Buyer>
        <GoodsAvailableAt TableName=""RefUNLOCO"">
          <Code>CNSHA</Code>
        </GoodsAvailableAt>
        <PortOfLoading TableName=""RefUNLOCO"">
          <Code>CNSHA</Code>
        </PortOfLoading>
        <PortOfDischarge TableName=""RefUNLOCO"">
          <Code>AUMEL</Code>
        </PortOfDischarge>
        <GoodsDeliveredTo TableName=""RefUNLOCO"">
          <Code>AUMEL</Code>
        </GoodsDeliveredTo>
        <SendingAgent TableName=""OrgHeader"">
          <Code>UTISHA</Code>
        </SendingAgent>
        <JobShipment>
          <UniqueConsignRef>S00001034</UniqueConsignRef>
        </JobShipment>
        <CountryOfSupply TableName=""RefCountry"">
          <Code>CN</Code>
        </CountryOfSupply>
        <OrderCurrency TableName=""RefCurrency"">
          <Code>USD</Code>
        </OrderCurrency>
        <PackTypeExternal TableName=""RefPackType"">
          <Code>PLT</Code>
        </PackTypeExternal>
      </JobOrderHeader>
    </Order>
  </Body>
</Native>";
		#endregion
	}
}
