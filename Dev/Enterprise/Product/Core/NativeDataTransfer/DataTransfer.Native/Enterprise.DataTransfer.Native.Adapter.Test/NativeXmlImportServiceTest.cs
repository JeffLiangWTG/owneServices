using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Adapter.ImportServices;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions;
using Enterprise.DataTransfer.Native.Common.Logging;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DataTransfer.Native.Adapter
{
	class NativeXmlImportServiceTest : TestCaseWithFactory
	{
		public void TestGeneratingXSDOfTableThatDoesNotExistThrowsError()
		{
			DropDummyBizo();
			TestUtil.AddDummyBizoToGlobalDefinitions();
			var nativeXmlImportService = new DummyImportService();

			using (var tempFileDirectory = new TempDirectory())
			{
				nativeXmlImportService.DirectoryName = tempFileDirectory.DirectoryName;
				AssertExceptionThrown("All XSDs creation threw an exception", typeof(InvalidOperationException), "Column names for object DummyDependentBizo could not be obtained.", () => nativeXmlImportService.GenerateAndSaveXSD(null));
				AssertEquals(1,ErrorReporter.TotalErrorCount);
				ErrorReporter.Clear();
			}
		}

		public void TestShipment_Order_Declaration_XSDsAreNotGeneratedWhenGeneratingAllXSDs()
		{
			DropDummyBizo();
			var nativeXmlImportService = new DummyImportService();

			using (var tempFileDirectory = new TempDirectory())
			{
				nativeXmlImportService.DirectoryName = tempFileDirectory.DirectoryName;
				AssertNoExceptionThrown("All XSDs creation threw an exception", () => nativeXmlImportService.GenerateAndSaveXSD(null));
				CombineAssertions(delegate
				{
					Assert("Shipment XSD should not be created", !File.Exists(tempFileDirectory.DirectoryName + "\\NativeShipment.xsd"));
					Assert("Order XSD should not be created", !File.Exists(tempFileDirectory.DirectoryName + "\\NativeOrder.xsd"));
					Assert("Declaration XSD should not be created", !File.Exists(tempFileDirectory.DirectoryName + "\\NativeDeclaration.xsd"));
				});
			}
		}

		public void TestShipment_Order_Declaration_XSDsAreStillNamedNative()
		{
			DropDummyBizo();
			var nativeXmlImportService = new DummyImportService();

			using (var tempFileDirectory = new TempDirectory())
			{
				nativeXmlImportService.DirectoryName = tempFileDirectory.DirectoryName;
				AssertNoExceptionThrown("JobOrderHeader XSD creation threw an exception", () => nativeXmlImportService.GenerateAndSaveXSD("JobOrderHeader"));
				AssertNoExceptionThrown("JobDeclaration XSD creation threw an exception", () => nativeXmlImportService.GenerateAndSaveXSD("JobDeclaration"));
				AssertNoExceptionThrown("JobShipment XSD creation threw an exception", () => nativeXmlImportService.GenerateAndSaveXSD("JobShipment"));
				CombineAssertions(delegate
				{
					Assert("Shipment XSD should be created", File.Exists(Path.Combine(tempFileDirectory.DirectoryName, $"{"NativeShipment"}.zip")));
					Assert("Order XSD should be created", File.Exists(Path.Combine(tempFileDirectory.DirectoryName,$"{"NativeOrder"}.zip")));
					Assert("Declaration XSD should be created", File.Exists(Path.Combine(tempFileDirectory.DirectoryName, $"{"NativeDeclaration"}.zip")));
				});
			}
		}

		public void TestDummyXSDIsNotGeneratedWhenGeneratingAllXSDs()
		{
			DropDummyBizo();
			var nativeXmlImportService = new DummyImportService();

			using (var tempFileDirectory = new TempDirectory())
			{
				nativeXmlImportService.DirectoryName = tempFileDirectory.DirectoryName;
				AssertNoExceptionThrown("All XSDs creation threw an exception", () => nativeXmlImportService.GenerateAndSaveXSD(null));
				Assert("Dummy XSD should not be created", !File.Exists(tempFileDirectory.DirectoryName + "\\NativeDummy.xsd"));
			}
		}

		void DropDummyBizo()
		{
			TestUtil.Connection.ExecuteNonQuery(@"DROP VIEW ZZDummyBizo;
DROP VIEW ZZDummyBizo_Idx;
DROP TABLE DummyDependentBizo;
DROP TABLE DummyBizo;");
		}

		public void TestNativeXMLImportService_DisableData()
		{
			DataRegistry.Instance.NativeXMLSupportTillDate = ZDateTime.UtcNow.AddMonths(1).ToDateTime();
			var nativeXmlImportService = new NativeXmlImportService();
			AssertEquals(true, nativeXmlImportService.CanBeImported("JobDeclaration"));
			AssertEquals(true, nativeXmlImportService.CanBeImported("JobShipment"));
			AssertEquals(true, nativeXmlImportService.CanBeImported("JobOrderHeader"));
			AssertEquals(true, nativeXmlImportService.CanBeImported("OrgSupplierPart"));

			DataRegistry.Instance.NativeXMLSupportTillDate = ZDateTime.UtcNow.AddMonths(-1).ToDateTime();
			AssertEquals(false, nativeXmlImportService.CanBeImported("JobDeclaration"));
			AssertEquals(false, nativeXmlImportService.CanBeImported("JobShipment"));
			AssertEquals(false, nativeXmlImportService.CanBeImported("JobOrderHeader"));
			AssertEquals(true, nativeXmlImportService.CanBeImported("OrgSupplierPart"));
		}

		public void TestNativeXMLImportService()
		{
			TestUtil.RecreateDummyBizoTablesWithStandardSchema();

			var nativeXmlImportService = new DummyImportService();
			var globalDefinitions = GlobalDefinition.Instance;
			TestUtil.AddDummyBizoToGlobalDefinitions();
			var definitions = globalDefinitions.TableMapping;
			var realTable = "DummyBizo";

			Assert("nativeXmlImportService.CanBeImported should return false on invalid table", !nativeXmlImportService.CanBeImported("I am not a table"));
			Assert("nativeXmlImportService.CanBeImported should return true on valid table", nativeXmlImportService.CanBeImported(realTable));

			using (var tempFileDirectory = new TempDirectory())
			{
				nativeXmlImportService.DirectoryName = tempFileDirectory.DirectoryName;
				AssertExceptionThrown(typeof(NativeXMLUserVisibleException)
					, "Could not load EntitySet using Table Name: I am not a table - Could not load EntitySet with Entity Set Name:  - Entity Set Name is Empty"
					, () => nativeXmlImportService.GenerateAndSaveXSD("I am not a table"));
			}

			nativeXmlImportService.DirectoryName = null;
			nativeXmlImportService.GenerateAndSaveXSD(realTable);
			Assert("File \\NativeDummy.xsd should not be created when user clicks cancel", !File.Exists("\\NativeDummy.xsd"));

			var unitTestNotification = UnitTestUserNotification.Instance;
			nativeXmlImportService.DirectoryName = @"~!@#$%^&*()_+-={}[]/|?\><,.:;'";
			AssertExceptionThrown<ArgumentException>(() => nativeXmlImportService.GenerateAndSaveXSD(realTable));

			using (var tempFileDirectory = new TempDirectory())
			{
				unitTestNotification.ClearMessagesAndAnswers();
				nativeXmlImportService.DirectoryName = tempFileDirectory.DirectoryName;
				AssertNoExceptionThrown("Single XSD creation threw an exception", () => nativeXmlImportService.GenerateAndSaveXSD(realTable));
				AssertEquals("unitTestNotification.LastMessage.Text", "NativeDummy.xsd plus the Common Schema file and Native Schema files exported to [" + tempFileDirectory.DirectoryName + "].", unitTestNotification.LastMessage.Text);
				var zipPath = Path.Combine(tempFileDirectory.DirectoryName, "NativeDummy.zip");
				AssertEquals("File NativeDummy.zip Exists", true, File.Exists(zipPath));
				ZipCompression.Unzip(zipPath, tempFileDirectory.DirectoryName);
				AssertEquals("File UniversalCommon.xsd Exists", true, File.Exists(tempFileDirectory.DirectoryName + "\\UniversalCommon.xsd"));
				AssertEquals("File Native.xsd Exists", true, File.Exists(tempFileDirectory.DirectoryName + "\\Native.xsd"));
				AssertEquals("File NativeDummy.xsd Exists", true, File.Exists(tempFileDirectory.DirectoryName + "\\NativeDummy.xsd"));
			}

			using (var tempFileDirectory = new TempDirectory())
			{
				unitTestNotification.ClearMessagesAndAnswers();
				nativeXmlImportService.DirectoryName = tempFileDirectory.DirectoryName;
				AssertNoExceptionThrown("All XSDs creation threw an exception", () => nativeXmlImportService.GenerateAndSaveXSD(null));
				var zipPath = Path.Combine(tempFileDirectory.DirectoryName, $"{NativeXmlInfo.ZipFileName}.zip");
				AssertEquals("File Native XML.zip Exists", true, File.Exists(zipPath));
				ZipCompression.Unzip(zipPath, tempFileDirectory.DirectoryName);

				CombineAssertions(delegate
				{
					foreach (var definition in definitions)
					{
						if (definition.Value == "Shipment" || definition.Value == "Order" || definition.Value == "Declaration")
						{
							Assert("Generating all XSDs failed as Native" + definition.Value + ".xsd was included", !File.Exists(tempFileDirectory.DirectoryName + "\\" + "Native" + definition.Value + ".xsd"));
						}
						else
						{
							Assert("Generating all XSDs failed as Native" + definition.Value + ".xsd was not created", File.Exists(tempFileDirectory.DirectoryName + "\\" + "Native" + definition.Value + ".xsd"));
						}
					}

					AssertEquals("Information Dialog did not display the correct message", (definitions.Count - 3).ToString() + " Individual Schema files plus the Common Schema file and Reference Data Schema file exported to [" + tempFileDirectory.DirectoryName + "].", unitTestNotification.LastMessage.Text);
					AssertEquals("File UniversalCommon.xsd Exists", true, File.Exists(tempFileDirectory.DirectoryName + "\\UniversalCommon.xsd"));
					AssertEquals("File Native.xsd Exists", true, File.Exists(tempFileDirectory.DirectoryName + "\\Native.xsd"));
				});

				CombineAssertions(delegate
				{
					AssertMultilineASCIIEquals("Check xsd contents are correct", string.Join("\r\n", VersionHeader, string.Format(ExpectedDummyXSD, NativeXmlInfo.Version_2011_11)), File.ReadAllText(tempFileDirectory.DirectoryName + "\\NativeDummy.xsd"));
					AssertContains("Check UniversalCommonSchema has correct contents", ExpectedUniversalCommonXSD, File.ReadAllText(tempFileDirectory.DirectoryName + "\\UniversalCommon.xsd"));
					AssertMultilineASCIIEquals("Check Native.xsd has correct contents", string.Join("\r\n", VersionHeader, string.Format(ExpectedNativeXSD, NativeXmlInfo.Version_2011_11)), File.ReadAllText(tempFileDirectory.DirectoryName + "\\Native.xsd"));
				});
			}
		}

		public void TestUpdateOrgWithExcludeScreeningStatus()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "DUMMY CORP";
			orgHeader.OH_IsActive = true;
			orgHeader.OH_RL_NKClosestPort = "AUSYD";
			Factory.Save();

			orgHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			Factory.Save();
			AssertEquals(ScreeningStatusesList.Codes.Matched, orgHeader.OH_ScreeningStatus);

			#region import xml

			var importXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>YJP</OwnerCode>
  </Header>
  <Body>
    <Organization>
      <OrgHeader Action=""UPDATE"">
        <FullName>DUMMY CORP</FullName>
        <ScreeningStatus>UNK</ScreeningStatus>
      </OrgHeader>
    </Organization>
  </Body>
</Native>";

			#endregion

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(importXML)))
			{
				var importService = new ImportHandler(new AncillaryImportServices());
				importService.Import(stream);

				orgHeader.Reload();
				AssertEquals(ScreeningStatusesList.Codes.Matched, orgHeader.OH_ScreeningStatus);
			}
		}

		public void TestImportSelfReference()
		{
			#region Imported Xml

			var importedXml = @"
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Declaration version=""2.0"">
      <JobDeclaration Action=""MERGE"">
        <PK>7e960ec4-4b95-4bf1-b085-518a3e040d3d</PK>
        <ClusterKey>1</ClusterKey>
        <MessageType>EXP</MessageType>
        <MessageSubType>NCF</MessageSubType>
        <DeclarationReference>B00001000</DeclarationReference>
        <EFTMode></EFTMode>
        <MergeBy>TRF</MergeBy>
        <OperationalStatus></OperationalStatus>
        <MessageStatus></MessageStatus>
        <EntryStatus></EntryStatus>
        <EntrySubmittedDate></EntrySubmittedDate>
        <EntryAuthorisationDate></EntryAuthorisationDate>
        <TotalNoOfPacks>0</TotalNoOfPacks>
        <TotalNoOfPacksPackType></TotalNoOfPacksPackType>
        <TotalNoOfPieces>0</TotalNoOfPieces>
        <PermitMessageGuid></PermitMessageGuid>
        <PermitGSTAmount>0.0000</PermitGSTAmount>
        <PermitFeeAmount>0.0000</PermitFeeAmount>
        <PermitTotalDuty>0.0000</PermitTotalDuty>
        <PermitTotalExcise>0.0000</PermitTotalExcise>
        <PermitTotalAmount>0.0000</PermitTotalAmount>
        <DateAtOrigin></DateAtOrigin>
        <ExportDate></ExportDate>
        <DateOfFirstArrival></DateOfFirstArrival>
        <DateOfArrival></DateOfArrival>
        <DateAtFinalDestination></DateAtFinalDestination>
        <LloydsIMO></LloydsIMO>
        <VoyageFlightNo></VoyageFlightNo>
        <MasterBill></MasterBill>
        <HouseBill></HouseBill>
        <TransportMode></TransportMode>
        <AddInfo></AddInfo>
        <ContainerMode>CNT</ContainerMode>
        <GoodsDescription></GoodsDescription>
        <ContainerCount>0</ContainerCount>
        <ExportGoodsType>OT</ExportGoodsType>
        <AgentsReference></AgentsReference>
        <OwnerRef></OwnerRef>
        <Folio></Folio>
        <PaymentMethod>DEF</PaymentMethod>
        <TotalWeight>0.000</TotalWeight>
        <TotalVolume>0.000</TotalVolume>
        <TotalWeightUnit>KG</TotalWeightUnit>
        <TotalVolumeUnit>M3</TotalVolumeUnit>
        <ShipmentIncoTerm></ShipmentIncoTerm>
        <LandedCostByWeight>100</LandedCostByWeight>
        <LandedCostByVolume>0</LandedCostByVolume>
        <LandedCostByUnits>0</LandedCostByUnits>
        <LandedCostByCost>0</LandedCostByCost>
        <ApplicationCode></ApplicationCode>
        <ConsolidatedCargoStatus></ConsolidatedCargoStatus>
        <WarehouseReleaseDate></WarehouseReleaseDate>
        <WarehouseTransactionStatus></WarehouseTransactionStatus>
        <TotalNoOfPacksDecimal>0.0000</TotalNoOfPacksDecimal>
        <ScreeningStatus>UNK</ScreeningStatus>
        <IsCancelled>false</IsCancelled>
        <OverrideFreightDefaults>false</OverrideFreightDefaults>
        <IsPersonalEffects>false</IsPersonalEffects>
        <UseOwnerRefAsQuarantineRef>false</UseOwnerRefAsQuarantineRef>
        <AutoWeightApportion>false</AutoWeightApportion>
        <JobComInvoiceHeaderCollection>
          <JobComInvoiceHeader Action=""MERGE"">
            <PK>e59bd403-0fdf-4abe-8913-5a9a122255e4</PK>
            <ClusterKey>1</ClusterKey>
            <InvoiceDate>2014-09-18T00:00:00</InvoiceDate>
            <ValuationDateOverride></ValuationDateOverride>
            <InvoiceNumber>All Invoices</InvoiceNumber>
            <InvoiceAmount>0.0000</InvoiceAmount>
            <IncoTerm></IncoTerm>
            <Volume>100.000</Volume>
            <VolumeUQ></VolumeUQ>
            <Weight>0.000</Weight>
            <WeightUQ></WeightUQ>
            <InvoiceCurrExRate>0.000000000</InvoiceCurrExRate>
            <InvoiceCurrLandedCostExRate>0.000000000</InvoiceCurrLandedCostExRate>
            <FOBValue>0.0000</FOBValue>
            <AddInfo></AddInfo>
            <PaymentNo></PaymentNo>
            <PaymentAmount>0.0000</PaymentAmount>
            <PaymentExRate>0.000000000</PaymentExRate>
            <PaymentDate></PaymentDate>
            <StandAloneInvoiceDirection></StandAloneInvoiceDirection>
            <MessageStatus></MessageStatus>
            <InvoiceCurrExRateType></InvoiceCurrExRateType>
            <InvoiceDisplaySequence>0</InvoiceDisplaySequence>
            <NoOfPacks>0.000</NoOfPacks>
            <OverrideFOB>false</OverrideFOB>
            <GroupInvoice>true</GroupInvoice>
            <Buyer TableName=""OrgHeader"" />
            <Supplier TableName=""OrgHeader"" />
            <DefaultOrigin TableName=""RefCountry"" />
            <IncoTermNamedPort TableName=""RefUNLOCO"" />
            <GroupInvoiceFK TableName=""JobComInvoiceHeader"" />
            <RelatedHouseBill TableName=""CusDecHouseBill"" />
            <GlbBranch />
            <Invoice_Currency TableName=""RefCurrency"" />
          </JobComInvoiceHeader>
          <JobComInvoiceHeader Action=""MERGE"">
            <PK>ff4ad5f9-8ece-44f6-8db6-5b91f7d57b81</PK>
            <ClusterKey>1</ClusterKey>
            <InvoiceDate>2014-09-18T00:00:00</InvoiceDate>
            <ValuationDateOverride></ValuationDateOverride>
            <InvoiceNumber>1</InvoiceNumber>
            <InvoiceAmount>0.0000</InvoiceAmount>
            <IncoTerm></IncoTerm>
            <Volume>10.000</Volume>
            <VolumeUQ></VolumeUQ>
            <Weight>0.000</Weight>
            <WeightUQ>KG</WeightUQ>
            <InvoiceCurrExRate>0.000000000</InvoiceCurrExRate>
            <InvoiceCurrLandedCostExRate>0.000000000</InvoiceCurrLandedCostExRate>
            <FOBValue>0.0000</FOBValue>
            <AddInfo>HeaderREL_Hidden=N*VALB_Hidden=TV</AddInfo>
            <PaymentNo></PaymentNo>
            <PaymentAmount>0.0000</PaymentAmount>
            <PaymentExRate>0.000000000</PaymentExRate>
            <PaymentDate></PaymentDate>
            <StandAloneInvoiceDirection></StandAloneInvoiceDirection>
            <MessageStatus></MessageStatus>
            <InvoiceCurrExRateType></InvoiceCurrExRateType>
            <InvoiceDisplaySequence>1</InvoiceDisplaySequence>
            <NoOfPacks>0.000</NoOfPacks>
            <OverrideFOB>false</OverrideFOB>
            <GroupInvoice>false</GroupInvoice>
            <Buyer TableName=""OrgHeader"" />
            <Supplier TableName=""OrgHeader"" />
            <DefaultOrigin TableName=""RefCountry"" />
            <IncoTermNamedPort TableName=""RefUNLOCO"" />
            <GroupInvoiceFK TableName=""JobComInvoiceHeader"">
              <PK>e59bd403-0fdf-4abe-8913-5a9a122255e4</PK>
            </GroupInvoiceFK>
            <RelatedHouseBill TableName=""CusDecHouseBill"" />
            <GlbBranch />
            <Invoice_Currency TableName=""RefCurrency"" />
          </JobComInvoiceHeader>
          <JobComInvoiceHeader Action=""MERGE"">
            <PK>459eea88-e9ec-4eb6-be68-7a0dd9efe510</PK>
            <ClusterKey>1</ClusterKey>
            <InvoiceDate>2014-09-18T00:00:00</InvoiceDate>
            <ValuationDateOverride></ValuationDateOverride>
            <InvoiceNumber>2</InvoiceNumber>
            <InvoiceAmount>0.0000</InvoiceAmount>
            <IncoTerm></IncoTerm>
            <Volume>20.000</Volume>
            <VolumeUQ></VolumeUQ>
            <Weight>0.000</Weight>
            <WeightUQ>KG</WeightUQ>
            <InvoiceCurrExRate>0.000000000</InvoiceCurrExRate>
            <InvoiceCurrLandedCostExRate>0.000000000</InvoiceCurrLandedCostExRate>
            <FOBValue>0.0000</FOBValue>
            <AddInfo>HeaderREL_Hidden=N*VALB_Hidden=TV</AddInfo>
            <PaymentNo></PaymentNo>
            <PaymentAmount>0.0000</PaymentAmount>
            <PaymentExRate>0.000000000</PaymentExRate>
            <PaymentDate></PaymentDate>
            <StandAloneInvoiceDirection></StandAloneInvoiceDirection>
            <MessageStatus></MessageStatus>
            <InvoiceCurrExRateType></InvoiceCurrExRateType>
            <InvoiceDisplaySequence>2</InvoiceDisplaySequence>
            <NoOfPacks>0.000</NoOfPacks>
            <OverrideFOB>false</OverrideFOB>
            <GroupInvoice>false</GroupInvoice>
            <Buyer TableName=""OrgHeader"" />
            <Supplier TableName=""OrgHeader"" />
            <DefaultOrigin TableName=""RefCountry"" />
            <IncoTermNamedPort TableName=""RefUNLOCO"" />
            <GroupInvoiceFK TableName=""JobComInvoiceHeader"">
              <PK>e59bd403-0fdf-4abe-8913-5a9a122255e4</PK>
            </GroupInvoiceFK>
            <RelatedHouseBill TableName=""CusDecHouseBill"" />
            <GlbBranch />
            <Invoice_Currency TableName=""RefCurrency"" />
          </JobComInvoiceHeader>
        </JobComInvoiceHeaderCollection>
        <JobDocsAndCartageCollection>
          <JobDocsAndCartage Action=""MERGE"">
            <PK>6824d21d-7ca7-4899-bcac-9a3b55f32d04</PK>
            <FCLPickupEquipmentNeeded></FCLPickupEquipmentNeeded>
            <EstimatedPickup></EstimatedPickup>
            <PickupRequiredBy></PickupRequiredBy>
            <PickupCartageAdvised></PickupCartageAdvised>
            <ArrivalCartageRef></ArrivalCartageRef>
            <PickupCartageCompleted></PickupCartageCompleted>
            <PickupLabourTime></PickupLabourTime>
            <PickupLabourCharge>0.0000</PickupLabourCharge>
            <DemurrageOnPickupTime></DemurrageOnPickupTime>
            <DemurrageOnPickupCharge>0.0000</DemurrageOnPickupCharge>
            <PrintOptionForPackagesOnAWB>DEF</PrintOptionForPackagesOnAWB>
            <FCLDeliveryEquipmentNeeded></FCLDeliveryEquipmentNeeded>
            <FCLAvailable></FCLAvailable>
            <FCLStorageCommences></FCLStorageCommences>
            <LCLAvailable></LCLAvailable>
            <LCLStorageCommences></LCLStorageCommences>
            <LCLAirStorageDaysOrHours>0</LCLAirStorageDaysOrHours>
            <LCLAirStorageCharge>0.0000</LCLAirStorageCharge>
            <EstimatedDelivery></EstimatedDelivery>
            <DeliveryRequiredBy></DeliveryRequiredBy>
            <DeliveryCartageAdvised></DeliveryCartageAdvised>
            <DeliveryCartageCompleted></DeliveryCartageCompleted>
            <DeliveryLabourTime></DeliveryLabourTime>
            <DeliveryLabourCharge>0.0000</DeliveryLabourCharge>
            <DemurrageOnDeliveryTime></DemurrageOnDeliveryTime>
            <DemurrageOnDeliveryCharge>0.0000</DemurrageOnDeliveryCharge>
            <CustomAttrib1></CustomAttrib1>
            <CustomAttrib2></CustomAttrib2>
            <CustomDate1></CustomDate1>
            <CustomDate2></CustomDate2>
            <CustomDecimal1>0.000</CustomDecimal1>
            <CustomDecimal2>0.000</CustomDecimal2>
            <ExportStatement></ExportStatement>
            <HasProhibitedPackaging>false</HasProhibitedPackaging>
            <InsuranceRequired>false</InsuranceRequired>
            <IsContingencyRelease>false</IsContingencyRelease>
            <CustomFlag1>false</CustomFlag1>
            <CustomFlag2>false</CustomFlag2>
            <LCLDatesOverrideConsol>false</LCLDatesOverrideConsol>
            <PickupCartageCoAddr TableName=""OrgAddress"" />
            <DeliveryCartageCoAddr TableName=""OrgAddress"" />
          </JobDocsAndCartage>
        </JobDocsAndCartageCollection>
        <Importer TableName=""OrgHeader"" />
        <Supplier TableName=""OrgHeader"" />
        <ShippingLine TableName=""OrgHeader"" />
        <Forwarder TableName=""OrgHeader"" />
        <ServiceLevel TableName=""RefServiceLevel"">
          <Code>STD</Code>
          <PK>de20e49f-224f-4ea0-9523-e07b26cb4841</PK>
        </ServiceLevel>
        <Origin TableName=""RefUNLOCO"" />
        <PortOfLoading TableName=""RefUNLOCO"" />
        <PortOfFirstArrival TableName=""RefUNLOCO"" />
        <PortOfArrival TableName=""RefUNLOCO"" />
        <FinalDestination TableName=""RefUNLOCO"" />
        <Vessel TableName=""RefVessel"" />
        <CusAgent TableName=""GlbStaff"" />
        <JobShipment />
        <GlbBranch>
          <Code>BNE</Code>
          <PK>27a55065-ac88-4ec3-8bed-e575e79172cb</PK>
        </GlbBranch>
        <GlbCompany>
          <Code>EDI</Code>
          <PK>878D7ACA-FFC3-49FC-9710-969CA0C0F2AC</PK>
        </GlbCompany>
      </JobDeclaration>
    </Declaration>
  </Body>
</Native>";

			#endregion

			DataRegistry.Instance.NativeXMLSupportTillDate = ZDateTime.UtcNow.AddMonths(1).ToDateTime();
			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(importedXml)))
			{
				var businessObjectFactory = new BusinessObjectFactory();
				var groupHeader = businessObjectFactory.LoadTop1<Enterprise.Integration.Customs.AU.IJobComInvoiceGroupHeader>(new ZQuery());
				AssertNull(groupHeader);
				var declaration = businessObjectFactory.LoadTop1<Enterprise.Integration.Customs.AU.IJobDeclaration>(new ZQuery());
				AssertNull(declaration);

				var importService = new ImportHandler(new AncillaryImportServices());
				importService.Import(stream);
				groupHeader = businessObjectFactory.LoadTop1<Enterprise.Integration.Customs.AU.IJobComInvoiceGroupHeader>(new ZQuery());

				AssertNotNull(groupHeader);
				AssertEquals(ZGuid.Empty, groupHeader.JZ_JZ_GroupInvoiceFK);

				declaration = businessObjectFactory.LoadTop1<Enterprise.Integration.Customs.AU.IJobDeclaration>(new ZQuery());
				AssertEquals("27a55065-ac88-4ec3-8bed-e575e79172cb", declaration.JE_GB.ToString());
				AssertEquals("878d7aca-ffc3-49fc-9710-969ca0c0f2ac", declaration.CompanyPK.ToString());
			}
		}

		public void TestImportOrgWithContainingNewLineFeedElement()
		{
			var factory = new BusinessObjectFactory();
			var orgHeader = factory.New<OrgHeader>();
			orgHeader.OH_FullName = "DUMMY CORP";
			orgHeader.OH_IsActive = true;
			orgHeader.OH_RL_NKClosestPort = "AUSYD";

			factory.Save();

			AssertEquals("DUMCORSYD", orgHeader.OH_Code);

			#region import xml

			var importXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>YJP</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Organization>
      <OrgHeader Action=""UPDATE"">
        <Code>DUMCORSYD</Code>
        <StmNoteCollection>
          <StmNote Action=""insert"">
            <Description>Goods Handling Instructions</Description>
            <NoteText  xml:space=""preserve"">
TEST1
TEST2
TEST3
            </NoteText>
            <NoteType>PRV</NoteType>
            <NoteContext>AAA</NoteContext>
            <IsCustomDescription>false</IsCustomDescription>
            <ForceRead>false</ForceRead>
            <RelatedCompany TableName=""GlbCompany"" />
          </StmNote>
        </StmNoteCollection>
      </OrgHeader>
    </Organization>
  </Body>
</Native>";

			#endregion

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(importXML)))
			{
				var importService = new ImportHandler(new AncillaryImportServices());
				importService.Import(stream);

				factory.ReloadAll<OrgHeader>();
				orgHeader = factory.Load<OrgHeader>(orgHeader.PK);
				AssertEquals(1, orgHeader.Notes.DatabaseCount);

				AssertEquals("\\r\\nTEST1\\r\\nTEST2\\r\\nTEST3\\r\\n", ((StmNote)orgHeader.Notes.GetAllNotes().First()).ST_NoteText.Replace("\n", "\\n").Replace("\r", "\\r"));
			}
		}

		public void TestImportDoesNotUpdateActivityTrackingStatus()
		{
			var glbStaff = Factory.New<GlbStaff>();
			glbStaff.GS_FullName = "Test";
			Factory.Save();

			glbStaff.Reload();
			AssertEquals("Test", glbStaff.GS_FullName);
			AssertEquals("CMP", glbStaff.GS_ActivityTrackingStatus); //CMP is the default value

			#region import xml

			var importXML = @$"
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>YOUAUS_AU</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Staff version=""2.0"">
      <GlbStaff Action=""MERGE"">
        <PK>{glbStaff.PK}</PK>
        <FullName>Test 1</FullName>
        <ActivityTrackingStatus>No</ActivityTrackingStatus>
      </GlbStaff>
    </Staff>
  </Body>
</Native>";

			#endregion

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(importXML)))
			{
				var session = new AncillaryImportServices();
				var importService = new ImportHandler(session);
				importService.Import(stream);

				var expectedLog = @"GlbStaff - 0 inserts, 1 updates, 0 deletes".Trim();
				AssertEquals(expectedLog, string.Join("\r\n", ((MemoryLogger)session.Logger).Buffer.Logs().Select(log => log.Message).ToArray()));

				glbStaff.Reload();
				AssertEquals("Test 1", glbStaff.GS_FullName);
				AssertEquals("CMP", glbStaff.GS_ActivityTrackingStatus);
			}
		}

		public void TestImportDoesNotUpdateIsSystemAccountAndIsDeveloper()
		{
			var glbStaff = Factory.New<GlbStaff>();
			glbStaff.GS_FullName = "Test";
			Factory.Save();

			glbStaff.Reload();
			AssertEquals(false, glbStaff.GS_IsSystemAccount);
			AssertEquals(false, glbStaff.GS_IsDeveloper);

			#region import xml

			var importXML = @$"
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
	<Header>
		<OwnerCode>YOUAUS_AU</OwnerCode>
		<EnableCodeMapping>true</EnableCodeMapping>
	</Header>
	<Body>
		<Staff version=""2.0"">
			<GlbStaff Action=""MERGE"">
				<PK>{glbStaff.PK}</PK>
				<FullName>Test 1</FullName>
				<IsSystemAccount>true</IsSystemAccount>
				<IsDeveloper>true</IsDeveloper>
			</GlbStaff>
		</Staff>
	</Body>
</Native>";

			#endregion

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(importXML)))
			{
				var session = new AncillaryImportServices();
				var importService = new ImportHandler(session);
				importService.Import(stream);

				var expectedLog = @"GlbStaff - 0 inserts, 1 updates, 0 deletes".Trim();
				AssertEquals(expectedLog, string.Join("\r\n", ((MemoryLogger)session.Logger).Buffer.Logs().Select(log => log.Message).ToArray()));

				glbStaff.Reload();
				AssertEquals("Test 1", glbStaff.GS_FullName);
				AssertEquals(false, glbStaff.GS_IsSystemAccount);
				AssertEquals(false, glbStaff.GS_IsDeveloper);
			}
		}

		public void TestImportCantUpdateSystemRecordAtAll()
		{
			var glbStaff = Factory.New<GlbStaff>();
			glbStaff.GS_FullName = "Test";
			glbStaff.GS_IsSystemAccount = true;
			Factory.Save();

			glbStaff.Reload();
			AssertEquals(true, glbStaff.GS_IsSystemAccount);

			#region import xml

			var importXML = @$"
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
	<Header>
		<OwnerCode>YOUAUS_AU</OwnerCode>
		<EnableCodeMapping>true</EnableCodeMapping>
	</Header>
	<Body>
		<Staff version=""2.0"">
			<GlbStaff Action=""MERGE"">
				<PK>{glbStaff.PK}</PK>
				<FullName>Test 1</FullName>
			</GlbStaff>
		</Staff>
	</Body>
</Native>";

			#endregion

			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(importXML)))
			{
				var session = new AncillaryImportServices();
				var importService = new ImportHandler(session);
				importService.Import(stream);

				var expectedLog = @"No insert/update action performed.";
				AssertContains(expectedLog, string.Join("\r\n", ((MemoryLogger)session.Logger).Buffer.Logs().Select(log => log.Message).ToArray()));

				glbStaff.Reload();
				AssertEquals("Test", glbStaff.GS_FullName);
				AssertEquals(true, glbStaff.GS_IsSystemAccount);
			}
		}

		readonly string VersionHeader = string.Format(@"<!-- CW1 Version : {0} Release : {1}-->", new EnterpriseInformationRetriever().VersionNumber, new EnterpriseInformationRetriever().Release);

		#region Expected Dummy XSD

		const string ExpectedDummyXSD = @"<xs:schema targetNamespace=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""{0}"" elementFormDefault=""qualified"" xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  <xs:include schemaLocation=""Native.xsd"" />
  <xs:element name=""Dummy"" type=""DummyData"" />
  <xs:complexType name=""DummyData"">
    <xs:all>
      <xs:element name=""DummyBizo"" type=""NativeDummy"" />
    </xs:all>
    <xs:attribute name=""version"" type=""xs:token"" />
  </xs:complexType>
  <xs:complexType name=""NativeDummy"">
    <xs:all>
      <xs:element name=""Byte"" minOccurs=""0"" type=""xs:short"" />
      <xs:element name=""Code"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""5"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""Date"" minOccurs=""0"" type=""emptiableDateTime"" />
      <xs:element name=""DateTimeOffset"" minOccurs=""0"" type=""emptiableDateTime"" />
      <xs:element name=""Decimal"" minOccurs=""0"" type=""xs:decimal"" />
      <xs:element name=""Description"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""100"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""Geography"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"" />
        </xs:simpleType>
      </xs:element>
      <xs:element name=""Money"" minOccurs=""0"" type=""xs:decimal"" />
      <xs:element name=""Number"" minOccurs=""0"" type=""xs:int"" />
      <xs:element name=""NVarChar"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""20"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""NVarCharMax"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"" />
        </xs:simpleType>
      </xs:element>
      <xs:element name=""PK"" minOccurs=""0"" type=""xs:string"" />
      <xs:element name=""Short"" minOccurs=""0"" type=""xs:int"" />
      <xs:element name=""SmallDateTime"" minOccurs=""0"" type=""emptiableDateTime"" />
      <xs:element name=""Time"" minOccurs=""0"" type=""emptiableDateTime"" />
      <xs:element name=""VarBinaryMax"" minOccurs=""0"" type=""xs:base64Binary"" />
      <xs:element name=""VarCharMax"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"" />
        </xs:simpleType>
      </xs:element>
      <xs:element name=""DummyDependentBizoCollection"" minOccurs=""0"">
        <xs:complexType>
          <xs:sequence>
            <xs:element name=""DummyDependentBizo"" minOccurs=""0"" maxOccurs=""unbounded"">
              <xs:complexType>
                <xs:all>
                  <xs:element name=""Code"" minOccurs=""0"">
                    <xs:simpleType>
                      <xs:restriction base=""xs:string"">
                        <xs:maxLength value=""5"" />
                      </xs:restriction>
                    </xs:simpleType>
                  </xs:element>
                  <xs:element name=""Number"" minOccurs=""0"" type=""xs:int"" />
                  <xs:element name=""PK"" minOccurs=""0"" type=""xs:string"" />
                </xs:all>
                <xs:attribute name=""Action"" type=""Action"" />
              </xs:complexType>
            </xs:element>
          </xs:sequence>
        </xs:complexType>
      </xs:element>
    </xs:all>
    <xs:attribute name=""Action"" type=""Action"" />
  </xs:complexType>
</xs:schema>";

		#endregion

		#region Expected Universal Common Schema

		const string ExpectedUniversalCommonXSD = @"<xs:complexType name=""DataContext"">";

		#endregion

		#region Expected Reference Data Schema

		const string ExpectedNativeXSD = @"<xs:schema targetNamespace=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""{0}"" elementFormDefault=""qualified"" xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" xmlns:uv=""http://www.cargowise.com/Schemas/Universal/2011/11"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  <xs:import schemaLocation=""UniversalCommon.xsd"" namespace=""http://www.cargowise.com/Schemas/Universal/2011/11"" />
  <xs:element name=""Native"">
    <xs:complexType>
      <xs:sequence>
        <xs:element name=""Header"">
          <xs:complexType>
            <xs:sequence>
              <xs:element name=""OwnerCode"" type=""xs:string"" minOccurs=""0"" />
              <xs:element name=""EnableCodeMapping"" type=""xs:boolean"" minOccurs=""0"" />
              <xs:element name=""DataContext"" type=""uv:DataContext"" minOccurs=""0"" />
              <xs:element name=""MessageNumberCollection"" minOccurs=""0"">
                <xs:complexType>
                  <xs:sequence>
                    <xs:element name=""MessageNumber"" minOccurs=""0"" maxOccurs=""unbounded"" type=""uv:MessageNumber"" />
                  </xs:sequence>
                </xs:complexType>
              </xs:element>
            </xs:sequence>
          </xs:complexType>
        </xs:element>
        <xs:element name=""Body"">
          <xs:complexType>
            <xs:sequence>
              <xs:any minOccurs=""1"" maxOccurs=""unbounded"" processContents=""skip"" />
            </xs:sequence>
          </xs:complexType>
        </xs:element>
      </xs:sequence>
      <xs:attribute name=""version"" type=""xs:token"" />
    </xs:complexType>
  </xs:element>
  <xs:simpleType name=""Action"">
    <xs:restriction base=""xs:string"">
      <xs:enumeration value=""INSERT"" />
      <xs:enumeration value=""UPDATE"" />
      <xs:enumeration value=""MERGE"" />
      <xs:enumeration value=""DELETE"" />
    </xs:restriction>
  </xs:simpleType>
  <!-- Organisation address, simplified-->
  <xs:complexType name=""NativeOrganizationAddress"">
    <xs:all>
      <xs:element name=""OrganizationCode"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""12"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""Address1"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""50"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""Address2"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""50"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""City"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""25"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""CompanyName"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""100"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""AddressShortCode"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""25"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""Country"" minOccurs=""0"">
        <xs:complexType>
          <xs:all>
            <xs:element name=""Code"">
              <xs:simpleType>
                <xs:restriction base=""xs:string"">
                  <xs:maxLength value=""2"" />
                </xs:restriction>
              </xs:simpleType>
            </xs:element>
            <xs:element type=""xs:string"" name=""Name"" minOccurs=""0"" />
          </xs:all>
        </xs:complexType>
      </xs:element>
      <xs:element name=""Email"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""60"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""Fax"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""20"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""Phone"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""20"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""Port"" minOccurs=""0"">
        <xs:complexType>
          <xs:all>
            <xs:element name=""Code"">
              <xs:simpleType>
                <xs:restriction base=""xs:string"">
                  <xs:maxLength value=""5"" />
                </xs:restriction>
              </xs:simpleType>
            </xs:element>
            <xs:element type=""xs:string"" name=""Name"" minOccurs=""0"" />
          </xs:all>
        </xs:complexType>
      </xs:element>
      <xs:element name=""Postcode"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""10"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""State"" minOccurs=""0"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""25"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""AddressType"" minOccurs=""1"">
        <xs:simpleType>
          <xs:restriction base=""xs:string"">
            <xs:maxLength value=""40"" />
          </xs:restriction>
        </xs:simpleType>
      </xs:element>
      <xs:element name=""RegistrationNumberCollection"" minOccurs=""0"">
        <xs:complexType>
          <xs:sequence>
            <xs:element name=""RegistrationNumber"" minOccurs=""0"" maxOccurs=""unbounded"">
              <xs:complexType>
                <xs:all>
                  <xs:element name=""Type"" minOccurs=""1"">
                    <xs:complexType>
                      <xs:all>
                        <xs:element name=""Code"" minOccurs=""1"">
                          <xs:simpleType>
                            <xs:restriction base=""xs:string"">
                              <xs:maxLength value=""3"" />
                            </xs:restriction>
                          </xs:simpleType>
                        </xs:element>
                        <xs:element name=""Description"" minOccurs=""0"">
                          <xs:simpleType>
                            <xs:restriction base=""xs:string"">
                              <xs:maxLength value=""35"" />
                            </xs:restriction>
                          </xs:simpleType>
                        </xs:element>
                      </xs:all>
                    </xs:complexType>
                  </xs:element>
                  <xs:element name=""CountryOfIssue"" minOccurs=""0"">
                    <xs:complexType>
                      <xs:all>
                        <xs:element name=""Code"" minOccurs=""1"">
                          <xs:simpleType>
                            <xs:restriction base=""xs:string"">
                              <xs:maxLength value=""2"" />
                            </xs:restriction>
                          </xs:simpleType>
                        </xs:element>
                        <xs:element name=""Name"" minOccurs=""0"">
                          <xs:simpleType>
                            <xs:restriction base=""xs:string"">
                              <xs:maxLength value=""35"" />
                            </xs:restriction>
                          </xs:simpleType>
                        </xs:element>
                      </xs:all>
                    </xs:complexType>
                  </xs:element>
                  <xs:element name=""Value"" minOccurs=""1"">
                    <xs:simpleType>
                      <xs:restriction base=""xs:string"">
                        <xs:maxLength value=""35"" />
                      </xs:restriction>
                    </xs:simpleType>
                  </xs:element>
                </xs:all>
              </xs:complexType>
            </xs:element>
          </xs:sequence>
        </xs:complexType>
      </xs:element>
    </xs:all>
  </xs:complexType>
  <!-- end address -->
  <xs:complexType name=""CustomValues"">
    <xs:sequence maxOccurs=""unbounded"">
      <xs:choice>
        <xs:element name=""Boolean"">
          <xs:complexType>
            <xs:simpleContent>
              <xs:extension base=""xs:boolean"">
                <xs:attribute name=""Name"" type=""xs:string"" use=""required"" />
              </xs:extension>
            </xs:simpleContent>
          </xs:complexType>
        </xs:element>
        <xs:element name=""DateTime"">
          <xs:complexType>
            <xs:simpleContent>
              <xs:extension base=""xs:dateTime"">
                <xs:attribute name=""Name"" type=""xs:string"" use=""required"" />
              </xs:extension>
            </xs:simpleContent>
          </xs:complexType>
        </xs:element>
        <xs:element name=""Integer"">
          <xs:complexType>
            <xs:simpleContent>
              <xs:extension base=""xs:integer"">
                <xs:attribute name=""Name"" type=""xs:string"" use=""required"" />
              </xs:extension>
            </xs:simpleContent>
          </xs:complexType>
        </xs:element>
        <xs:element name=""String"">
          <xs:complexType>
            <xs:simpleContent>
              <xs:extension base=""xs:string"">
                <xs:attribute name=""Name"" type=""xs:string"" use=""required"" />
              </xs:extension>
            </xs:simpleContent>
          </xs:complexType>
        </xs:element>
        <xs:element name=""Time"">
          <xs:complexType>
            <xs:simpleContent>
              <xs:extension base=""xs:time"">
                <xs:attribute name=""Name"" type=""xs:string"" use=""required"" />
              </xs:extension>
            </xs:simpleContent>
          </xs:complexType>
        </xs:element>
      </xs:choice>
    </xs:sequence>
  </xs:complexType>
  <xs:simpleType name=""emptiableDateTime"">
    <xs:union memberTypes=""xs:dateTime emptyString"" />
  </xs:simpleType>
  <xs:simpleType name=""emptyString"">
    <xs:restriction base=""xs:string"">
      <xs:length value=""0"" />
    </xs:restriction>
  </xs:simpleType>
  <xs:simpleType name=""emptiableTime"">
    <xs:union memberTypes=""xs:time emptyString"" />
  </xs:simpleType>
</xs:schema>";

		#endregion

		#region Implementation

		class DummyImportService : NativeXmlImportService
		{
			public string DirectoryName { get { return directoryName; } set { directoryName = value; } }
			string directoryName;

			protected override string GetDirectory()
			{
				return directoryName;
			}
		}
		#endregion
	}
}
