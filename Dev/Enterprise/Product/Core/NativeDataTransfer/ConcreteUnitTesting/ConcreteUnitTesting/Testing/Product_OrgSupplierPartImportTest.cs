using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting
{
	class Product_OrgSupplierPartImportTest : TestCaseWithFactory
	{
		public void TestDescNotStrippedAfterImport()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "MAXSERSYD";
			org.OH_Code = "MAXSERSYD";
			var address = Factory.NewWithValidTestData<OrgAddress>();
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse = (IWhsWarehouse)helper.CreateWarehouse("WH1 Name", "WH1", "A");
			warehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
			warehouse.WW_OA_WarehouseAddress = address.PK;
			Factory.Save();

			var actualLog = ImportNativeXmlReturningLog(NativeProductXml);
			var expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Importing Product: FAN
Processed: Product
--- Import Process Finished -----------------------------------------------------------
OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes
WhsProductParamsByWhsAndClient - 1 inserts, 0 updates, 0 deletes
OrgSupplierPartBarcode - 2 inserts, 0 updates, 0 deletes
OrgPartUnit - 3 inserts, 0 updates, 0 deletes
StmNote - 1 inserts, 0 updates, 0 deletes
			".Trim();

			var supplyPart = Factory.LoadTop1<OrgSupplierPart>(new ZQuery());
			AssertEquals(@"FANTALES
FANTALES", supplyPart.OP_Desc);
			AssertMultilineASCIIEquals("Precondition: Log Text", expectedLog, actualLog);
		}

		public void TestImportWhsProductWithPickFaceCorrectSchemaName()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "MAXSERSYD";
			org.OH_Code = "MAXSERSYD";

			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = org.PK;
			orgAddress.OA_City = "DummyCity";
			orgAddress.OA_Address1 = "3489 BLAU KASE STRASSE";

			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse = (IWhsWarehouse)helper.CreateWarehouse("WH1 Name", "WH1", "A");
			warehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
			warehouse.WW_OA_WarehouseAddress = orgAddress.PK;

			var area = Factory.New<IWhsArea>();
			area.WA_Name = "A";
			area.WA_WW_Whs = warehouse.PK;

			Factory.Save(); //Generate Locations with Unique Location String

			var actualLog = ImportNativeXmlReturningLog(NativeProductXml);

			var expectedLog = @"
--- Start Import Process --------------------------------------------------------------
Importing Product: FAN
Processed: Product
--- Import Process Finished -----------------------------------------------------------
OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes
WhsProductParamsByWhsAndClient - 1 inserts, 0 updates, 0 deletes
OrgSupplierPartBarcode - 2 inserts, 0 updates, 0 deletes
OrgPartUnit - 3 inserts, 0 updates, 0 deletes
StmNote - 1 inserts, 0 updates, 0 deletes
			".Trim();

			AssertMultilineASCIIEquals("Precondition: Log Text", expectedLog, actualLog);
		}

		public void TestImportWhsProductWithUNDangerousGoods()
		{
			var query = new ZQuery(UNDGSubstanceSchema.DG_UniqueRecordId, "51");
			query.AddToFilter(UNDGSubstanceSchema.DG_Standard, UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA);
			var substance = Factory.LoadTop1<UNDGSubstance>(query) ?? Factory.New<UNDGSubstance>();
			if (substance.DG_UniqueRecordId.IsEmpty)
			{
				substance.DG_UNNO = "9999";
				substance.DG_UniqueRecordId = "51";
				substance.DG_Standard = "IAT";
				Factory.Save();
			}

			var onlyCodeXml = string.Format(NativeProductXmlForUNDG, @"
<UNDGSubstance>
    <Code>0007</Code>
    <Standard>IMO</Standard>
 </UNDGSubstance>");
			var actualLogOnlyCode = ImportNativeXmlReturningLog(onlyCodeXml);
			var expectedLogOnlyCode = @"
--- Start Import Process --------------------------------------------------------------
Importing Product: FAN
Processed: Product
--- Import Process Finished -----------------------------------------------------------
OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes
UNDGDataItem - 1 inserts, 0 updates, 0 deletes
UNDGSubstancePivot - 1 inserts, 0 updates, 0 deletes
			".Trim();

			AssertMultilineASCIIEquals("Precondition: Log Text", expectedLogOnlyCode, actualLogOnlyCode);

			var iataUniqueRecordIDXml = string.Format(NativeProductXmlForUNDG, @"
 <UNDGSubstance>
    <Standard>IAT</Standard>
    <UniqueRecordId>51</UniqueRecordId>
 </UNDGSubstance>");
			var actualLogIATAUniqueRecordId = ImportNativeXmlReturningLog(iataUniqueRecordIDXml);
			var expectedLogIATAUniqueRecordId = @"
--- Start Import Process --------------------------------------------------------------
Importing Product: FAN
Processed: Product
--- Import Process Finished -----------------------------------------------------------
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes
UNDGDataItem - 1 inserts, 0 updates, 0 deletes
UNDGSubstancePivot - 1 inserts, 0 updates, 0 deletes";

			AssertMultilineASCIIEquals("Precondition: Log Text", expectedLogIATAUniqueRecordId, actualLogIATAUniqueRecordId);

			var actualLogOnlyCodeSecondTime = ImportNativeXmlReturningLog(onlyCodeXml);
			var expectedLogOnlyCodeSecondTime = @"
--- Start Import Process --------------------------------------------------------------
Importing Product: FAN
Processed: Product
--- Import Process Finished -----------------------------------------------------------
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes
UNDGDataItem - 0 inserts, 0 updates, 0 deletes
UNDGSubstancePivot - 0 inserts, 1 updates, 0 deletes
			".Trim();

			AssertMultilineASCIIEquals("Precondition: Log Text", expectedLogOnlyCodeSecondTime, actualLogOnlyCodeSecondTime);

			var unnoAndVariantXml = string.Format(NativeProductXmlForUNDG, @"
<UNDGSubstance>
    <UNNO>0004</UNNO>
    <Variant>a</Variant>
    <Standard>IMO</Standard>
 </UNDGSubstance>");
			var actualLogUnnoAndVariant = ImportNativeXmlReturningLog(unnoAndVariantXml);
			var expectedLogUnnoAndVariant = @"
--- Start Import Process --------------------------------------------------------------
Importing Product: FAN
Processed: Product
--- Import Process Finished -----------------------------------------------------------
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes
UNDGDataItem - 1 inserts, 0 updates, 0 deletes
UNDGSubstancePivot - 1 inserts, 0 updates, 0 deletes
			".Trim();

			AssertMultilineASCIIEquals("Precondition: Log Text", expectedLogUnnoAndVariant, actualLogUnnoAndVariant);

			var codeOverUnnoAndVariantXml = string.Format(NativeProductXmlForUNDG, @"
<UNDGSubstance>
    <Code>1100</Code>
    <UNNO>0004</UNNO>
    <Variant>a</Variant>
    <Standard>IMO</Standard>
    <UniqueRecordId></UniqueRecordId>
</UNDGSubstance>");
			var actualLogCodeOverUnnoAndVariant = ImportNativeXmlReturningLog(codeOverUnnoAndVariantXml);
			var expectedLogCodeOverUnnoAndVariant = @"
--- Start Import Process --------------------------------------------------------------
Importing Product: FAN
Processed: Product
--- Import Process Finished -----------------------------------------------------------
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes
UNDGDataItem - 1 inserts, 0 updates, 0 deletes
UNDGSubstancePivot - 1 inserts, 0 updates, 0 deletes
			".Trim();

			AssertMultilineASCIIEquals("Precondition: Log Text", expectedLogCodeOverUnnoAndVariant, actualLogCodeOverUnnoAndVariant);

			var existingPK = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			var existingPKXml = string.Format(NativeProductXmlForUNDG, $@"
<UNDGSubstance>
    <PK>{existingPK}</PK>
</UNDGSubstance>");
			var actualLogExistingPK = ImportNativeXmlReturningLog(existingPKXml);
			var expectedLogExistingPK = @"
--- Start Import Process --------------------------------------------------------------
Importing Product: FAN
Processed: Product
--- Import Process Finished -----------------------------------------------------------
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes
UNDGDataItem - 0 inserts, 0 updates, 0 deletes
UNDGSubstancePivot - 0 inserts, 1 updates, 0 deletes
			".Trim();

			AssertMultilineASCIIEquals("Precondition: Log Text", expectedLogExistingPK, actualLogExistingPK);
			var newSubsPK = UNDGSubstanceLoader.LoadSubstances(Factory, "3005", "a", "IMO").First().PK;
			var newSubsPKXml = string.Format(NativeProductXmlForUNDG, $@"
<UNDGSubstance>
    <PK>{newSubsPK}</PK>
</UNDGSubstance>");
			var actualLogNewSubsPK = ImportNativeXmlReturningLog(newSubsPKXml);
			var expectedLogNewSubsPK = @"
--- Start Import Process --------------------------------------------------------------
Importing Product: FAN
Processed: Product
--- Import Process Finished -----------------------------------------------------------
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes
UNDGDataItem - 1 inserts, 0 updates, 0 deletes
UNDGSubstancePivot - 1 inserts, 0 updates, 0 deletes
			".Trim();

			AssertMultilineASCIIEquals("Precondition: Log Text", expectedLogNewSubsPK, actualLogNewSubsPK);

			var uniqueCodeXml = string.Format(NativeProductXmlForUNDG, $@"
<UNDGSubstance>
    <Code>2005</Code>
</UNDGSubstance>");
			var actualLogUniqueCode = ImportNativeXmlReturningLog(uniqueCodeXml);
			var expectedLogUniqueCode = @"
--- Start Import Process --------------------------------------------------------------
Importing Product: FAN
Processed: Product
--- Import Process Finished -----------------------------------------------------------
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes
UNDGDataItem - 1 inserts, 0 updates, 0 deletes
UNDGSubstancePivot - 1 inserts, 0 updates, 0 deletes
			".Trim();

			AssertMultilineASCIIEquals("Precondition: Log Text", expectedLogUniqueCode, actualLogUniqueCode);
		}

		public void TestImportWhsProductWithUNDangerousGoodsSubstancePivot()
		{
			var subs = Factory.New<UNDGSubstance>();
			subs.DG_UNNO = "9999";
			subs.DG_Variant = "a";
			subs.DG_Code = "9999a";
			subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
			Factory.Save();

			var codeXML = string.Format(NativeProductXmlForUNDG, @"
<UNDGSubstance>
    <Code>9999a</Code>
    <Standard>IMO</Standard>
 </UNDGSubstance>
<UNDGSubstancePivotCollection>
    <UNDGSubstancePivot Action=""MERGE"">
		<ParentTableCode>DI</ParentTableCode>
		<UNNO>9999</UNNO>
		<Variant>a</Variant>
		<IsDefault>true</IsDefault>
		<Standard>IMO</Standard>
    </UNDGSubstancePivot>
</UNDGSubstancePivotCollection>
");
			var actualLogOnlyCode = ImportNativeXmlReturningLog(codeXML);
			var expectedLogOnlyCode = @"
--- Start Import Process --------------------------------------------------------------
Importing Product: FAN
Processed: Product
--- Import Process Finished -----------------------------------------------------------
OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes
UNDGDataItem - 1 inserts, 0 updates, 0 deletes
UNDGSubstancePivot - 1 inserts, 0 updates, 0 deletes
			".Trim();

			AssertMultilineASCIIEquals("Precondition: Log Text", expectedLogOnlyCode, actualLogOnlyCode);

			var actualLogOnlyCodeSecondTime = ImportNativeXmlReturningLog(codeXML);
			var expectedLogOnlyCodeSecondTime = @"
--- Start Import Process --------------------------------------------------------------
Importing Product: FAN
Processed: Product
--- Import Process Finished -----------------------------------------------------------
OrgSupplierPart - 0 inserts, 0 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes
UNDGDataItem - 0 inserts, 0 updates, 0 deletes
UNDGSubstancePivot - 0 inserts, 0 updates, 0 deletes
			".Trim();

			AssertMultilineASCIIEquals("Precondition: Log Text", expectedLogOnlyCodeSecondTime, actualLogOnlyCodeSecondTime);
		}

		public void TestImportWhsProduct_WhsProductParamsByWhsAndClient()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "MAXSERSYD";
			org.OH_Code = "MAXSERSYD";

			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = org.PK;
			orgAddress.OA_City = "DummyCity";
			orgAddress.OA_Address1 = "3489 BLAU KASE STRASSE";

			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var warehouse = (IWhsWarehouse)helper.CreateWarehouse("WH1 Name", "WH1", "A");
			warehouse.WW_GB_RelatedCompanyBranch = GlbBranch.CurrentBranch.PK;
			warehouse.WW_OA_WarehouseAddress = orgAddress.PK;

			var area = Factory.New<IWhsArea>();
			area.WA_Name = "A";
			area.WA_WW_Whs = warehouse.PK;

			Factory.Save(); //Generate Locations with Unique Location String

			var actualLog1 = ImportNativeXmlReturningLog(NativeProductXml);

			var expectedLog1 = @"
--- Start Import Process --------------------------------------------------------------
Importing Product: FAN
Processed: Product
--- Import Process Finished -----------------------------------------------------------
OrgSupplierPart - 1 inserts, 0 updates, 0 deletes
OrgPartRelation - 1 inserts, 0 updates, 0 deletes
WhsProductParamsByWhsAndClient - 1 inserts, 0 updates, 0 deletes
OrgSupplierPartBarcode - 2 inserts, 0 updates, 0 deletes
OrgPartUnit - 3 inserts, 0 updates, 0 deletes
StmNote - 1 inserts, 0 updates, 0 deletes
			".Trim();

			AssertMultilineASCIIEquals("Precondition: Log Text", expectedLog1, actualLog1);

			var actualLog2 = ImportNativeXmlReturningLog(NativeProductXml2);

			var expectedLog2 = @"
--- Start Import Process --------------------------------------------------------------
Importing Product: FAN
Processed: Product
--- Import Process Finished -----------------------------------------------------------
OrgSupplierPart - 0 inserts, 1 updates, 0 deletes
OrgPartRelation - 0 inserts, 0 updates, 0 deletes
WhsProductParamsByWhsAndClient - 0 inserts, 1 updates, 0 deletes
OrgSupplierPartBarcode - 0 inserts, 0 updates, 0 deletes
OrgPartUnit - 0 inserts, 0 updates, 0 deletes
StmNote - 0 inserts, 0 updates, 0 deletes
			".Trim();

			AssertMultilineASCIIEquals("Precondition: Log Text", expectedLog2, actualLog2);

			var productParams = Factory.Load(ObjectFactory.GetType<IWhsProductParamsByWhsAndClient>(), new ZQuery());
			AssertEquals(1, productParams.Length);
		}

		const string NativeProductXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns = ""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Product version = ""2.0"">
	  <OrgSupplierPart Action=""MERGE"">
        <PK>4c1e57cf-fb89-42c1-ab90-4a877c4e1d4e</PK>
        <PartNum>FAN</PartNum>
        <StockKeepingUnit>CTN</StockKeepingUnit>
        <CountDecimalPlaces>2</CountDecimalPlaces>
        <Weight>2.000</Weight>
        <WeightUQ>KG</WeightUQ>
        <Cubic>0.006</Cubic>
        <CubicUQ>M3</CubicUQ>
        <Depth>10.000</Depth>
        <Height>30.000</Height>
        <Width>20.000</Width>
        <MeasureUQ>CM</MeasureUQ>
        <LastCost>1.1000</LastCost>
        <WeightedCost>1.0000</WeightedCost>
        <QtyInStock>333.00</QtyInStock>
        <VendorPackQty>1.000</VendorPackQty>
        <OrderMultipleQty>1.000</OrderMultipleQty>
        <OrderMultipleUnit>CTN</OrderMultipleUnit>
        <Division>DIVISION</Division>
        <Department>DEPAREMENT</Department>
        <CustomDecimal1>0.000</CustomDecimal1>
        <CustomDecimal2>0.000</CustomDecimal2>
        <CustomDecimal3>0.000</CustomDecimal3>
        <CustomDecimal4>0.000</CustomDecimal4>
        <CustomDecimal5>0.000</CustomDecimal5>
        <CustomAttrib1></CustomAttrib1>
        <CustomAttrib2></CustomAttrib2>
        <CustomAttrib3></CustomAttrib3>
        <CustomAttrib4></CustomAttrib4>
        <CustomAttrib5></CustomAttrib5>
        <CustomDate1></CustomDate1>
        <CustomDate2></CustomDate2>
        <CustomDate3></CustomDate3>
        <CustomDate4></CustomDate4>
        <CustomDate5></CustomDate5>
        <NetWeight>1.900</NetWeight>
        <Brand>THE BRAND IS FAN</Brand>
        <Model>THE MODEL IS FAN</Model>
        <SystemLastEditTimeUtc>2015-12-23T23:33:00</SystemLastEditTimeUtc>
        <SystemCreateTimeUtc>2015-12-23T23:07:00</SystemCreateTimeUtc>
        <IsActive>true</IsActive>
        <CustomFlag1>false</CustomFlag1>
        <CustomFlag2>false</CustomFlag2>
        <CustomFlag3>false</CustomFlag3>
        <CustomFlag4>false</CustomFlag4>
        <CustomFlag5>false</CustomFlag5>
        <AutoPrintAssemblyInstructions>false</AutoPrintAssemblyInstructions>
        <CanDisassembleKit>true</CanDisassembleKit>
        <CanResell>true</CanResell>
        <Desc>FANTALES
FANTALES</Desc>
        <IsComponentPickedOnSalesOrder>false</IsComponentPickedOnSalesOrder>
        <IsBarcoded>true</IsBarcoded>
        <OrgPartRelationCollection>
          <OrgPartRelation Action = ""MERGE"">
			<PK>0b9da9ea-e1cb-4e0f-b792-a0069998dc80</PK>
            <Relationship>OWN</Relationship>
            <LocalPartNumber>MY FAN</LocalPartNumber>
            <Ti>1</Ti>
            <Hi>3</Hi>
            <LandedCostMarginPercent1>0.00</LandedCostMarginPercent1>
            <LandedCostMarginPercent2>0.00</LandedCostMarginPercent2>
            <LandedCostMarginPercent3>0.00</LandedCostMarginPercent3>
            <ClientUQ>CTN</ClientUQ>
            <RoyaltyPercent>0.000</RoyaltyPercent>
            <RoyaltyFlatAmount>0.0000</RoyaltyFlatAmount>
            <RFAttributeConfirm>NON</RFAttributeConfirm>
            <ExpiryDateFormatString></ExpiryDateFormatString>
            <PackingDateFormatString></PackingDateFormatString>
            <PickMode>ASP</PickMode>
            <RollUpAttributesOnDocuments>false</RollUpAttributesOnDocuments>
            <JulianBatchNoFormat></JulianBatchNoFormat>
            <IsPartAttrib1ReleaseCaptured>false</IsPartAttrib1ReleaseCaptured>
            <IsPartAttrib2ReleaseCaptured>false</IsPartAttrib2ReleaseCaptured>
            <IsPartAttrib3ReleaseCaptured>false</IsPartAttrib3ReleaseCaptured>
            <FormLayoutController>true</FormLayoutController>
            <UseExpiryDate>false</UseExpiryDate>
            <UsePackingDate>false</UsePackingDate>
            <UsePartAttrib1>false</UsePartAttrib1>
            <UsePartAttrib2>false</UsePartAttrib2>
            <UsePartAttrib3>false</UsePartAttrib3>
            <CompletePalletPicking>false</CompletePalletPicking>
            <LocalPartDescription>MY FANTALES</LocalPartDescription>
            <OrgHeader>
              <Code>MAXSERSYD</Code>
              <PK>b8b5bc24-1700-42e6-a95f-063c860fbeaf</PK>
            </OrgHeader>
            <RoyaltyCurrency TableName = ""RefCurrency""/>
			<Category TableName= ""OrgPartCategory""/>
		  </OrgPartRelation>
		</OrgPartRelationCollection>
		<WhsProductParamsByWhsAndClientCollection>
		  <WhsProductParamsByWhsAndClient Action=""MERGE"">
			<PK> 0f8ae1a8-4239-46cd-bf2b-0355313693ea</PK>
            <StockTakeCycle></StockTakeCycle>
            <ExpiryNotificationPeriod>100</ExpiryNotificationPeriod>
            <ReplenishmentMinimum>10.000</ReplenishmentMinimum>
            <EconomicQuantity>48.000</EconomicQuantity>
            <PickGroup>0</PickGroup>
            <MaximumShelfLife>0</MaximumShelfLife>
            <WhsWarehouse>
              <WarehouseCode>WH1</WarehouseCode>
              <PK>187b0bbc-8992-49c7-91b3-21eb75296498</PK>
            </WhsWarehouse>
            <OrgHeader>
              <Code>MAXSERSYD</Code>
              <PK>b8b5bc24-1700-42e6-a95f-063c860fbeaf</PK>
            </OrgHeader>
            <Area TableName = ""WhsArea"">
			  <Name>DEFAULT</Name>
			  <PK>12f10692-b18a-445e-9842-807195fb996f</PK>
              <Whs TableName = ""WhsWarehouse"">
				<WarehouseCode> WH1 </WarehouseCode>
				<PK>187b0bbc-8992-49c7-91b3-21eb75296498</PK>
              </Whs>
            </Area>
            <StagingAreaBOM TableName = ""WhsArea""/>
			<ReceivedPackType TableName= ""RefPackType"">
			  <Code>PLT</Code>
			  <PK>aa289f33-8e5f-4eff-8ade-10f80af88de0</PK>
            </ReceivedPackType>
            <ReleasedPackType TableName = ""RefPackType"">
			  <Code>CTN</Code>
			  <PK>b15c82f5-178d-4bb1-a2d7-caa11ed69683</PK>
            </ReleasedPackType>
          </WhsProductParamsByWhsAndClient>
        </WhsProductParamsByWhsAndClientCollection>
        <OrgSupplierPartBarcodeCollection>
          <OrgSupplierPartBarcode Action= ""MERGE"">
			<PK> a3181d36-2308-49d4-8d4b-6b36bdec2c8b</PK>
            <Barcode>55556</Barcode>
            <PackType TableName = ""RefPackType"">
			  <Code>CTN</Code>
			  <PK>b15c82f5-178d-4bb1-a2d7-caa11ed69683</PK>
            </PackType>
          </OrgSupplierPartBarcode>
          <OrgSupplierPartBarcode Action = ""MERGE"">
			<PK>ae0a1863-af0f-4aee-a1a8-e851c4f61b69</PK>
            <Barcode>6666667</Barcode>
            <PackType TableName = ""RefPackType"">
			  <Code>PLT</Code>
			  <PK>aa289f33-8e5f-4eff-8ade-10f80af88de0</PK>
            </PackType>
          </OrgSupplierPartBarcode>
        </OrgSupplierPartBarcodeCollection>
        <WhsPickfaceCollection>
          <WhsPickface Action = ""MERGE"">
			<PK>9896f41e-7d42-478c-ace3-8dacdadfc133</PK>
            <ReplenishMinimum>3.000</ReplenishMinimum>
            <ReplenishMaximum>20.000</ReplenishMaximum>
            <ReplenishmentMultiple>1.000</ReplenishmentMultiple>
            <WhsLocation>
              <PK>7b867a1c-0e00-41fd-addc-1a7f60159d99</PK>
            </WhsLocation>
            <Client TableName = ""OrgHeader"">
			  <Code>MAXSERSYD</Code>
			  <PK>b8b5bc24-1700-42e6-a95f-063c860fbeaf</PK>
            </Client>
          </WhsPickface>
        </WhsPickfaceCollection>
        <OrgPartUnitCollection>
          <OrgPartUnit Action = ""MERGE"">
			<PK>24ff3d99-e78e-4143-bd74-05abefa95eb5</PK>
            <Weight>96.000</Weight>
            <Height>100.000</Height>
            <Width>101.000</Width>
            <Depth>99.000</Depth>
            <Cubic>0.000</Cubic>
            <QuantityInParent>48.000000</QuantityInParent>
            <NoOfSKUsInThisPack>0.000</NoOfSKUsInThisPack>
            <PackType>CTN</PackType>
            <ParentPackType>PLT</ParentPackType>
          </OrgPartUnit>
          <OrgPartUnit Action = ""MERGE"">
			<PK>941647fb-a8a6-4eb4-82b2-06c89243b5d2</PK>
            <Weight>0.000</Weight>
            <Height>0.000</Height>
            <Width>0.000</Width>
            <Depth>0.000</Depth>
            <Cubic>0.000</Cubic>
            <QuantityInParent>2.000000</QuantityInParent>
            <NoOfSKUsInThisPack>0.000</NoOfSKUsInThisPack>
            <PackType>KG</PackType>
            <ParentPackType>CTN</ParentPackType>
          </OrgPartUnit>
          <OrgPartUnit Action = ""MERGE"">
			<PK>f22565cc-0a7e-4da9-bc8d-50f61f10a009</PK>
            <Weight>0.000</Weight>
            <Height>0.000</Height>
            <Width>0.000</Width>
            <Depth>0.000</Depth>
            <Cubic>0.000</Cubic>
            <QuantityInParent>0.006000</QuantityInParent>
            <NoOfSKUsInThisPack>0.000</NoOfSKUsInThisPack>
            <PackType>M3</PackType>
            <ParentPackType>CTN</ParentPackType>
          </OrgPartUnit>
        </OrgPartUnitCollection>
        <StmNoteCollection>
          <StmNote Action = ""MERGE"">
			<PK>22153a3c-d0ff-4b31-8d62-a82aae238999</PK>
            <Description>Extended Commercial Description</Description>
            <NoteData></NoteData>
            <NoteText>This is extended commerical description</NoteText>
            <NoteType>PUB</NoteType>
            <NoteContext>AAA</NoteContext>
            <IsCustomDescription>false</IsCustomDescription>
            <ForceRead>true</ForceRead>
            <RelatedCompany TableName = ""GlbCompany"" />
		  </StmNote>
		</StmNoteCollection>
		<CommodityCode TableName= ""RefCommodityCode"">
		  <Code>AFAT</Code>
		  <PK>5aab92a9-443c-41d3-9077-c7489da00688</PK>
        </CommodityCode>
        <LastWeightedCostCurr TableName = ""RefCurrency"">
		  <Code>AUD</Code>
		  <PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
        </LastWeightedCostCurr>
        <PackType TableName = ""RefPackType"">
		  <Code>PLT</Code>
		  <PK>aa289f33-8e5f-4eff-8ade-10f80af88de0</PK>
        </PackType>
      </OrgSupplierPart>
    </Product>
  </Body>
</Native>";

		const string NativeProductXml2 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns = ""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Product version = ""2.0"">
	  <OrgSupplierPart Action=""MERGE"">
        <PK>4c1e57cf-fb89-42c1-ab90-4a877c4e1d4e</PK>
        <PartNum>FAN</PartNum>
        <StockKeepingUnit>CTN</StockKeepingUnit>
        <CountDecimalPlaces>2</CountDecimalPlaces>
        <Weight>2.000</Weight>
        <WeightUQ>KG</WeightUQ>
        <Cubic>0.006</Cubic>
        <CubicUQ>M3</CubicUQ>
        <Depth>10.000</Depth>
        <Height>30.000</Height>
        <Width>20.000</Width>
        <MeasureUQ>CM</MeasureUQ>
        <LastCost>1.1000</LastCost>
        <WeightedCost>1.0000</WeightedCost>
        <QtyInStock>333.00</QtyInStock>
        <VendorPackQty>1.000</VendorPackQty>
        <OrderMultipleQty>1.000</OrderMultipleQty>
        <OrderMultipleUnit>CTN</OrderMultipleUnit>
        <Division>DIVISION</Division>
        <Department>DEPAREMENT</Department>
        <CustomDecimal1>0.000</CustomDecimal1>
        <CustomDecimal2>0.000</CustomDecimal2>
        <CustomDecimal3>0.000</CustomDecimal3>
        <CustomDecimal4>0.000</CustomDecimal4>
        <CustomDecimal5>0.000</CustomDecimal5>
        <CustomAttrib1></CustomAttrib1>
        <CustomAttrib2></CustomAttrib2>
        <CustomAttrib3></CustomAttrib3>
        <CustomAttrib4></CustomAttrib4>
        <CustomAttrib5></CustomAttrib5>
        <CustomDate1></CustomDate1>
        <CustomDate2></CustomDate2>
        <CustomDate3></CustomDate3>
        <CustomDate4></CustomDate4>
        <CustomDate5></CustomDate5>
        <NetWeight>1.900</NetWeight>
        <Brand>THE BRAND IS FAN</Brand>
        <Model>THE MODEL IS FAN</Model>
        <SystemLastEditTimeUtc>2015-12-23T23:33:00</SystemLastEditTimeUtc>
        <SystemCreateTimeUtc>2015-12-23T23:07:00</SystemCreateTimeUtc>
        <IsActive>true</IsActive>
        <CustomFlag1>false</CustomFlag1>
        <CustomFlag2>false</CustomFlag2>
        <CustomFlag3>false</CustomFlag3>
        <CustomFlag4>false</CustomFlag4>
        <CustomFlag5>false</CustomFlag5>
        <AutoPrintAssemblyInstructions>false</AutoPrintAssemblyInstructions>
        <CanDisassembleKit>true</CanDisassembleKit>
        <CanResell>true</CanResell>
        <Desc>FANTALES</Desc>
        <IsComponentPickedOnSalesOrder>false</IsComponentPickedOnSalesOrder>
        <IsBarcoded>true</IsBarcoded>
        <OrgPartRelationCollection>
          <OrgPartRelation Action = ""MERGE"">
			<PK>0b9da9ea-e1cb-4e0f-b792-a0069998dc80</PK>
            <Relationship>OWN</Relationship>
            <LocalPartNumber>MY FAN</LocalPartNumber>
            <Ti>1</Ti>
            <Hi>3</Hi>
            <LandedCostMarginPercent1>0.00</LandedCostMarginPercent1>
            <LandedCostMarginPercent2>0.00</LandedCostMarginPercent2>
            <LandedCostMarginPercent3>0.00</LandedCostMarginPercent3>
            <ClientUQ>CTN</ClientUQ>
            <RoyaltyPercent>0.000</RoyaltyPercent>
            <RoyaltyFlatAmount>0.0000</RoyaltyFlatAmount>
            <RFAttributeConfirm>NON</RFAttributeConfirm>
            <ExpiryDateFormatString></ExpiryDateFormatString>
            <PackingDateFormatString></PackingDateFormatString>
            <PickMode>ASP</PickMode>
            <RollUpAttributesOnDocuments>false</RollUpAttributesOnDocuments>
            <JulianBatchNoFormat></JulianBatchNoFormat>
            <IsPartAttrib1ReleaseCaptured>false</IsPartAttrib1ReleaseCaptured>
            <IsPartAttrib2ReleaseCaptured>false</IsPartAttrib2ReleaseCaptured>
            <IsPartAttrib3ReleaseCaptured>false</IsPartAttrib3ReleaseCaptured>
            <FormLayoutController>true</FormLayoutController>
            <UseExpiryDate>false</UseExpiryDate>
            <UsePackingDate>false</UsePackingDate>
            <UsePartAttrib1>false</UsePartAttrib1>
            <UsePartAttrib2>false</UsePartAttrib2>
            <UsePartAttrib3>false</UsePartAttrib3>
            <CompletePalletPicking>false</CompletePalletPicking>
            <LocalPartDescription>MY FANTALES</LocalPartDescription>
            <OrgHeader>
              <Code>MAXSERSYD</Code>
              <PK>b8b5bc24-1700-42e6-a95f-063c860fbeaf</PK>
            </OrgHeader>
            <RoyaltyCurrency TableName = ""RefCurrency""/>
			<Category TableName= ""OrgPartCategory""/>
		  </OrgPartRelation>
		</OrgPartRelationCollection>
		<WhsProductParamsByWhsAndClientCollection>
		  <WhsProductParamsByWhsAndClient Action=""MERGE"">
			<PK> 0f8ae1a8-4239-46cd-bf2b-0355313693ea</PK>
            <StockTakeCycle></StockTakeCycle>
            <ExpiryNotificationPeriod>931</ExpiryNotificationPeriod>
            <ReplenishmentMinimum>10.000</ReplenishmentMinimum>
            <EconomicQuantity>48.000</EconomicQuantity>
            <PickGroup>0</PickGroup>
            <MaximumShelfLife>931</MaximumShelfLife>
            <WhsWarehouse>
              <WarehouseCode>WH1</WarehouseCode>
              <PK>187b0bbc-8992-49c7-91b3-21eb75296498</PK>
            </WhsWarehouse>
            <OrgHeader>
              <Code>MAXSERSYD</Code>
              <PK>b8b5bc24-1700-42e6-a95f-063c860fbeaf</PK>
            </OrgHeader>
            <Area TableName = ""WhsArea"">
			  <Name>DEFAULT</Name>
			  <PK>12f10692-b18a-445e-9842-807195fb996f</PK>
              <Whs TableName = ""WhsWarehouse"">
				<WarehouseCode> WH1 </WarehouseCode>
				<PK>187b0bbc-8992-49c7-91b3-21eb75296498</PK>
              </Whs>
            </Area>
            <StagingAreaBOM TableName = ""WhsArea""/>
			<ReceivedPackType TableName= ""RefPackType"">
			  <Code>PLT</Code>
			  <PK>aa289f33-8e5f-4eff-8ade-10f80af88de0</PK>
            </ReceivedPackType>
            <ReleasedPackType TableName = ""RefPackType"">
			  <Code>CTN</Code>
			  <PK>b15c82f5-178d-4bb1-a2d7-caa11ed69683</PK>
            </ReleasedPackType>
          </WhsProductParamsByWhsAndClient>
        </WhsProductParamsByWhsAndClientCollection>
        <OrgSupplierPartBarcodeCollection>
          <OrgSupplierPartBarcode Action= ""MERGE"">
			<PK> a3181d36-2308-49d4-8d4b-6b36bdec2c8b</PK>
            <Barcode>55556</Barcode>
            <PackType TableName = ""RefPackType"">
			  <Code>CTN</Code>
			  <PK>b15c82f5-178d-4bb1-a2d7-caa11ed69683</PK>
            </PackType>
          </OrgSupplierPartBarcode>
          <OrgSupplierPartBarcode Action = ""MERGE"">
			<PK>ae0a1863-af0f-4aee-a1a8-e851c4f61b69</PK>
            <Barcode>6666667</Barcode>
            <PackType TableName = ""RefPackType"">
			  <Code>PLT</Code>
			  <PK>aa289f33-8e5f-4eff-8ade-10f80af88de0</PK>
            </PackType>
          </OrgSupplierPartBarcode>
        </OrgSupplierPartBarcodeCollection>
        <WhsPickfaceCollection>
          <WhsPickface Action = ""MERGE"">
			<PK>9896f41e-7d42-478c-ace3-8dacdadfc133</PK>
            <ReplenishMinimum>3.000</ReplenishMinimum>
            <ReplenishMaximum>20.000</ReplenishMaximum>
            <ReplenishmentMultiple>1.000</ReplenishmentMultiple>
            <WhsLocation>
              <PK>7b867a1c-0e00-41fd-addc-1a7f60159d99</PK>
            </WhsLocation>
            <Client TableName = ""OrgHeader"">
			  <Code>MAXSERSYD</Code>
			  <PK>b8b5bc24-1700-42e6-a95f-063c860fbeaf</PK>
            </Client>
          </WhsPickface>
        </WhsPickfaceCollection>
        <OrgPartUnitCollection>
          <OrgPartUnit Action = ""MERGE"">
			<PK>24ff3d99-e78e-4143-bd74-05abefa95eb5</PK>
            <Weight>96.000</Weight>
            <Height>100.000</Height>
            <Width>101.000</Width>
            <Depth>99.000</Depth>
            <Cubic>0.000</Cubic>
            <QuantityInParent>48.000000</QuantityInParent>
            <NoOfSKUsInThisPack>0.000</NoOfSKUsInThisPack>
            <PackType>CTN</PackType>
            <ParentPackType>PLT</ParentPackType>
          </OrgPartUnit>
          <OrgPartUnit Action = ""MERGE"">
			<PK>941647fb-a8a6-4eb4-82b2-06c89243b5d2</PK>
            <Weight>0.000</Weight>
            <Height>0.000</Height>
            <Width>0.000</Width>
            <Depth>0.000</Depth>
            <Cubic>0.000</Cubic>
            <QuantityInParent>2.000000</QuantityInParent>
            <NoOfSKUsInThisPack>0.000</NoOfSKUsInThisPack>
            <PackType>KG</PackType>
            <ParentPackType>CTN</ParentPackType>
          </OrgPartUnit>
          <OrgPartUnit Action = ""MERGE"">
			<PK>f22565cc-0a7e-4da9-bc8d-50f61f10a009</PK>
            <Weight>0.000</Weight>
            <Height>0.000</Height>
            <Width>0.000</Width>
            <Depth>0.000</Depth>
            <Cubic>0.000</Cubic>
            <QuantityInParent>0.006000</QuantityInParent>
            <NoOfSKUsInThisPack>0.000</NoOfSKUsInThisPack>
            <PackType>M3</PackType>
            <ParentPackType>CTN</ParentPackType>
          </OrgPartUnit>
        </OrgPartUnitCollection>
        <StmNoteCollection>
          <StmNote Action = ""MERGE"">
			<PK>22153a3c-d0ff-4b31-8d62-a82aae238999</PK>
            <Description>Extended Commercial Description</Description>
            <NoteData></NoteData>
            <NoteText>This is extended commerical description</NoteText>
            <NoteType>PUB</NoteType>
            <NoteContext>AAA</NoteContext>
            <IsCustomDescription>false</IsCustomDescription>
            <ForceRead>true</ForceRead>
            <RelatedCompany TableName = ""GlbCompany"" />
		  </StmNote>
		</StmNoteCollection>
		<CommodityCode TableName= ""RefCommodityCode"">
		  <Code>AFAT</Code>
		  <PK>5aab92a9-443c-41d3-9077-c7489da00688</PK>
        </CommodityCode>
        <LastWeightedCostCurr TableName = ""RefCurrency"">
		  <Code>AUD</Code>
		  <PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
        </LastWeightedCostCurr>
        <PackType TableName = ""RefPackType"">
		  <Code>PLT</Code>
		  <PK>aa289f33-8e5f-4eff-8ade-10f80af88de0</PK>
        </PackType>
      </OrgSupplierPart>
    </Product>
  </Body>
</Native>";

		const string NativeProductXmlForUNDG = @"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDIDNLAMS</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <Product version=""2.0"">
      <OrgSupplierPart Action= ""MERGE"">
        <PK>3f815c74-1c87-4c47-a77b-61785e1f6c2c</PK>
        <IsActive>true</IsActive>
        <PartNum>FAN</PartNum>
        <StockKeepingUnit>UNT</StockKeepingUnit>
        <CountDecimalPlaces>0</CountDecimalPlaces>
        <Weight>0.000</Weight>
        <NetWeight>0.000</NetWeight>
        <WeightUQ>KG</WeightUQ>
        <Cubic>0.000</Cubic>
        <CubicUQ>M3</CubicUQ>
        <Depth>0.000</Depth>
        <Height>0.000</Height>
        <Width>0.000</Width>
        <MeasureUQ></MeasureUQ>
        <LastCost>0.0000</LastCost>
        <WeightedCost>0.0000</WeightedCost>
        <QtyInStock>0.00</QtyInStock>
        <VendorPackQty>0.000</VendorPackQty>
        <OrderMultipleQty>0.000</OrderMultipleQty>
        <OrderMultipleUnit></OrderMultipleUnit>
        <AutoPrintAssemblyInstructions>false</AutoPrintAssemblyInstructions>
        <CanDisassembleKit>true</CanDisassembleKit>
        <CanResell>true</CanResell>
        <Division></Division>
        <Department></Department>
        <CustomDecimal1>0.000</CustomDecimal1>
        <CustomDecimal2>0.000</CustomDecimal2>
        <CustomDecimal3>0.000</CustomDecimal3>
        <CustomDecimal4>0.000</CustomDecimal4>
        <CustomDecimal5>0.000</CustomDecimal5>
        <CustomAttrib1></CustomAttrib1>
        <CustomAttrib2></CustomAttrib2>
        <CustomAttrib3></CustomAttrib3>
        <CustomAttrib4></CustomAttrib4>
        <CustomAttrib5></CustomAttrib5>
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
        <SystemCreateTimeUtc>2020-03-19T03:52:00</SystemCreateTimeUtc>
        <SystemLastEditTimeUtc>2020-03-19T23:59:00</SystemLastEditTimeUtc>
        <Desc>TEST PRODUCT</Desc>
        <IsComponentPickedOnSalesOrder>false</IsComponentPickedOnSalesOrder>
        <IsBarcoded>true</IsBarcoded>
        <KitIsAutoReplenished>false</KitIsAutoReplenished>
        <Brand></Brand>
        <Model></Model>
        <KeepUpright>false</KeepUpright>
        <UnitsPerPallet>0.000000</UnitsPerPallet>
        <OrgPartRelationCollection>
          <OrgPartRelation Action=""MERGE"">
            <PK>c41495a4-3d8f-4fda-b3b2-fa6f03c1177d</PK>
            <Relationship>SUP</Relationship>
            <LocalPartNumber></LocalPartNumber>
            <Ti>0</Ti>
            <Hi>0</Hi>
            <ClientUQ></ClientUQ>
            <RoyaltyPercent>0.000</RoyaltyPercent>
            <RoyaltyFlatAmount>0.0000</RoyaltyFlatAmount>
            <LandedCostMarginPercent1>0.00</LandedCostMarginPercent1>
            <LandedCostMarginPercent2>0.00</LandedCostMarginPercent2>
            <LandedCostMarginPercent3>0.00</LandedCostMarginPercent3>
            <FormLayoutController>false</FormLayoutController>
            <JulianBatchNoFormat></JulianBatchNoFormat>
            <PickMode>ASP</PickMode>
            <UseExpiryDate>false</UseExpiryDate>
            <UsePackingDate>false</UsePackingDate>
            <UsePartAttrib1>false</UsePartAttrib1>
            <UsePartAttrib2>false</UsePartAttrib2>
            <UsePartAttrib3>false</UsePartAttrib3>
            <RFAttributeConfirm>NON</RFAttributeConfirm>
            <ExpiryDateFormatString></ExpiryDateFormatString>
            <PackingDateFormatString></PackingDateFormatString>
            <CompletePalletPicking>false</CompletePalletPicking>
            <RollUpAttributesOnDocuments>false</RollUpAttributesOnDocuments>
            <IsPartAttrib1ReleaseCaptured>false</IsPartAttrib1ReleaseCaptured>
            <IsPartAttrib2ReleaseCaptured>false</IsPartAttrib2ReleaseCaptured>
            <IsPartAttrib3ReleaseCaptured>false</IsPartAttrib3ReleaseCaptured>
            <LocalPartDescription></LocalPartDescription>
            <ConsigneeMinShelfLifeAccepted>0</ConsigneeMinShelfLifeAccepted>
            <OrgHeader>
              <Code>4BELEV</Code>
              <PK>24181eea-d3e5-4afe-8892-8684ab555879</PK>
            </OrgHeader>
            <RoyaltyCurrency TableName=""RefCurrency"" />
            <Category TableName=""OrgPartCategory"" />
            <CartonGroup TableName=""WhsCartonGroup"" />
          </OrgPartRelation>
        </OrgPartRelationCollection>
        <UNDGDataItemCollection>
          <UNDGDataItem Action=""MERGE"">
            <PK>393120e9-3a52-4372-8c00-aa1d038eff42</PK>
            <DGFlashPoint>0.0</DGFlashPoint>
            <TechnicalName></TechnicalName>
            <IMOClass>1.1D</IMOClass>
            <MPMarinePollutant></MPMarinePollutant>
            <DGVolume>0.000</DGVolume>
            <UnitOfVolume></UnitOfVolume>
            <DGWeight>0.000</DGWeight>
            <UnitOfWeight></UnitOfWeight>
            <PackageCount>0</PackageCount>
            <IsLimitedQuantity>false</IsLimitedQuantity>
            <HasOverpack>false</HasOverpack>
            <OverpackID></OverpackID>
            {0}
            <DGContact TableName=""OrgContact"" />
            <PackType TableName=""RefPackType"" />
          </UNDGDataItem>
        </UNDGDataItemCollection>
        <ProductStyleColour Action=""MERGE"" TableName=""WhsProductStyleColour"" />
        <ProductStyleSize Action=""MERGE"" TableName=""WhsProductStyleSize"" />
        <CommodityCode TableName=""RefCommodityCode"" />
        <LastWeightedCostCurr TableName=""RefCurrency"" />
        <PackType TableName=""RefPackType"" />
      </OrgSupplierPart>
    </Product>
  </Body>
</Native>";

		#region Implementation

		static string ImportNativeXmlReturningLog(string incomingXmlText)
		{
			string actualLog;
			var encoding = new UTF8Encoding();
			using (var stream = new MemoryStream(encoding.GetBytes(incomingXmlText)))
			{
				var manager = new ImportServiceManagerForTesting();
				manager.ImportService.Import(stream);
				actualLog = manager.GetLogs();
			}
			return actualLog;
		}

		#endregion
	}
}
