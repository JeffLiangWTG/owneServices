using System;
using System.Text;
using Enterprise.DbUpgrader.Transformation.Common.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.BR;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.BR
{
	[TestedType(typeof(FixBRInvoiceLineDIWTemplate))]
	public class FixBRInvoiceLineDIWTemplateTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new FixBRInvoiceLineDIWTemplate();

		protected override void PrepareTestData()
		{
			var nowDate = DateTime.Now;
			var testDataCreator = new TransformationTestDataCreator();
			var brGC = testDataCreator.CreateCompany("GC1", "BR");
			var brGB = testDataCreator.CreateGlbBranch("GB1", brGC);
			var auGC = testDataCreator.CreateCompany("GC2", "AU");
			var auGB = testDataCreator.CreateGlbBranch("GB2", auGC);

			var filterData = Encoding.ASCII.GetBytes(OriginalXMLData);
			brExportPK = testDataCreator.CreateModuleFilter(FixBRInvoiceLineDIWTemplate.ExportInvoiceLineDIW, "Test BR Export DIW", filterData, brGC);
			brImportPK = testDataCreator.CreateModuleFilter(FixBRInvoiceLineDIWTemplate.ImportInvoiceLineDIW, "Test BR Import DIW", filterData, brGC);
			brLicensePK = testDataCreator.CreateModuleFilter(FixBRInvoiceLineDIWTemplate.ImportLicenseInvoiceLineDIW, "Test BR Import License DIW", filterData, brGC);
			brSiscomexPK = testDataCreator.CreateModuleFilter(FixBRInvoiceLineDIWTemplate.ImportSiscomexInvoiceLineDIW, "Test BR Import Siscomex DIW", filterData, brGC);
			auExportPK = testDataCreator.CreateModuleFilter(FixBRInvoiceLineDIWTemplate.ExportInvoiceLineDIW, "Test AU Export DIW", filterData, auGC);
			auImportPK = testDataCreator.CreateModuleFilter(FixBRInvoiceLineDIWTemplate.ImportInvoiceLineDIW, "Test AU Import DIW", filterData, auGC);
			auLicensePK = testDataCreator.CreateModuleFilter(FixBRInvoiceLineDIWTemplate.ImportLicenseInvoiceLineDIW, "Test AU Import License DIW", filterData, auGC);
			auSiscomexPK = testDataCreator.CreateModuleFilter(FixBRInvoiceLineDIWTemplate.ImportSiscomexInvoiceLineDIW, "Test AU Import Siscomex DIW", filterData, auGC);
		}

		Guid brExportPK;
		Guid brImportPK;
		Guid brLicensePK;
		Guid brSiscomexPK;
		Guid auExportPK;
		Guid auImportPK;
		Guid auLicensePK;
		Guid auSiscomexPK;

		const string OriginalXMLData = @"<DataImportWizardSettings>
  <StartingRow>1</StartingRow>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_LineNo</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_Calc_Invoice</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>ManufacturerOrgPK</Name>
    <DefaultValue />
    <Expression />
    <MapAs>Code</MapAs>
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_OA_ManufacturerAddress</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>BR_ManufacturerAuthorityVersion</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>BR_ManufacturerAuthorityIdentifier</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>BR_ManufacturerIndicator</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_PartNo</Name>
    <DefaultValue />
    <Expression />
    <MapAs>Code</MapAs>
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_CGC_Catalog</Name>
    <DefaultValue />
    <Expression />
    <MapAs>Code</MapAs>
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>BR_CatalogAuthorityIdentifier</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>BR_CatalogAuthorityVersion</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_Tariff</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_InvoiceQuantity</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_InvoiceUQ</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_CustomsQuantity</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_CustomsUnitQty</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_LinePrice</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>FullGoodsDescription</Name>
    <DefaultValue>goods</DefaultValue>
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_CountryOfOrigin</Name>
    <DefaultValue />
    <Expression />
    <MapAs>Code</MapAs>
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_RH_NKCommodity_Code</Name>
    <DefaultValue />
    <Expression />
    <MapAs>Code</MapAs>
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_Weight</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_WeightUQ</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_NetWeight</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_NetWeightUQ</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_Volume</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_VolumeUQ</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_OrderNumber</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_Calc_OrderLineNumberAndSubLine</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>UnitPrice</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_SerialNumber</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>FMMBenefit</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>FMMBenefitDescription</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_CEI</Name>
    <DefaultValue />
    <Expression />
    <MapAs>Code</MapAs>
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_Calc_MergedLineNumber</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>ICMSTaxRegime</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>ICMSLegalBase</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>BR_ICMSRate</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>BR_ICMSBaseValueReductionPercentage</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>BR_ICMSFormula</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>ICMSFCPRateValue</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>BR_ICMSTotalAmountReductionPercentage</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Delimiter>,</Delimiter>
  <TextQualifier>""</TextQualifier>
  <FilterIndex>1</FilterIndex>
</DataImportWizardSettings>";

		const string UpdatedXMLData = @"<DataImportWizardSettings>
  <StartingRow>1</StartingRow>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_LineNo</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_Calc_Invoice</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>ManufacturerOrgPK</Name>
    <DefaultValue />
    <Expression />
    <MapAs>Code</MapAs>
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_OA_ManufacturerAddress</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_ManufacturerAuthorityVersion</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_ManufacturerAuthorityIdentifier</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_ManufacturerIndicator</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_PartNo</Name>
    <DefaultValue />
    <Expression />
    <MapAs>Code</MapAs>
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_CGC_Catalog</Name>
    <DefaultValue />
    <Expression />
    <MapAs>Code</MapAs>
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_CatalogAuthorityIdentifier</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_CatalogAuthorityVersion</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_Tariff</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_InvoiceQuantity</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_InvoiceUQ</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_CustomsQuantity</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_CustomsUnitQty</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_LinePrice</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>FullGoodsDescription</Name>
    <DefaultValue>goods</DefaultValue>
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_CountryOfOrigin</Name>
    <DefaultValue />
    <Expression />
    <MapAs>Code</MapAs>
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_RH_NKCommodity_Code</Name>
    <DefaultValue />
    <Expression />
    <MapAs>Code</MapAs>
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_Weight</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_WeightUQ</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_NetWeight</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_NetWeightUQ</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_Volume</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_VolumeUQ</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_OrderNumber</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_Calc_OrderLineNumberAndSubLine</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>UnitPrice</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_SerialNumber</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>FMMBenefit</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>FMMBenefitDescription</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_CEI</Name>
    <DefaultValue />
    <Expression />
    <MapAs>Code</MapAs>
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_Calc_MergedLineNumber</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>ICMSTaxRegime</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>ICMSLegalBase</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_ICMSRate</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_ICMSBaseValueReductionPercentage</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_ICMSFormula</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>ICMSFCPRateValue</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_ICMSTotalAmountReductionPercentage</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Delimiter>,</Delimiter>
  <TextQualifier>""</TextQualifier>
  <FilterIndex>1</FilterIndex>
</DataImportWizardSettings>";

		protected override void AssertPreConditions()
		{
			DIWTemplateUpdateHelperTest.AssertStmModuleFilterData(brExportPK, OriginalXMLData);
			DIWTemplateUpdateHelperTest.AssertStmModuleFilterData(brImportPK, OriginalXMLData);
			DIWTemplateUpdateHelperTest.AssertStmModuleFilterData(brLicensePK, OriginalXMLData);
			DIWTemplateUpdateHelperTest.AssertStmModuleFilterData(brSiscomexPK, OriginalXMLData);
			DIWTemplateUpdateHelperTest.AssertStmModuleFilterData(auExportPK, OriginalXMLData);
			DIWTemplateUpdateHelperTest.AssertStmModuleFilterData(auImportPK, OriginalXMLData);
			DIWTemplateUpdateHelperTest.AssertStmModuleFilterData(auLicensePK, OriginalXMLData);
			DIWTemplateUpdateHelperTest.AssertStmModuleFilterData(auSiscomexPK, OriginalXMLData);
		}

		protected override void AssertTransformationResults()
		{
			DIWTemplateUpdateHelperTest.AssertStmModuleFilterData(brExportPK, UpdatedXMLData);
			DIWTemplateUpdateHelperTest.AssertStmModuleFilterData(brImportPK, UpdatedXMLData);
			DIWTemplateUpdateHelperTest.AssertStmModuleFilterData(brLicensePK, UpdatedXMLData);
			DIWTemplateUpdateHelperTest.AssertStmModuleFilterData(brSiscomexPK, UpdatedXMLData);
			DIWTemplateUpdateHelperTest.AssertStmModuleFilterData(auExportPK, OriginalXMLData);
			DIWTemplateUpdateHelperTest.AssertStmModuleFilterData(auImportPK, OriginalXMLData);
			DIWTemplateUpdateHelperTest.AssertStmModuleFilterData(auLicensePK, OriginalXMLData);
			DIWTemplateUpdateHelperTest.AssertStmModuleFilterData(auSiscomexPK, OriginalXMLData);
		}
	}
}
