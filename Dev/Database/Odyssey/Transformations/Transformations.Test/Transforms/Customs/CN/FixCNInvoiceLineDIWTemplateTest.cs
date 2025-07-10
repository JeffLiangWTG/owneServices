using System;
using System.Text;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;
using DIWTemplateUpdateHelperTest = Enterprise.DbUpgrader.Transformation.Common.Testing.DIWTemplateUpdateHelperTest;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.CN.Testing
{
	[TestedType(typeof(FixCNInvoiceLineDIWTemplate))]
	public class FixCNInvoiceLineDIWTemplateTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new FixCNInvoiceLineDIWTemplate();

		protected override void PrepareTestData()
		{
			var nowDate = DateTime.Now;
			var testDataCreator = new TransformationTestDataCreator();
			var cnGC = testDataCreator.CreateCompany("GC1", "CN");
			var auGC = testDataCreator.CreateCompany("GC2", "AU");

			var filterData = Encoding.ASCII.GetBytes(OriginalXMLData);
			cnExportPK = testDataCreator.CreateModuleFilter(FixCNInvoiceLineDIWTemplate.ExportInvoiceLineDIW, "Test CN Export DIW", filterData, cnGC);
			auExportPK = testDataCreator.CreateModuleFilter(FixCNInvoiceLineDIWTemplate.ExportInvoiceLineDIW, "Test AU Export DIW", filterData, auGC);
		}

		Guid cnExportPK;
		Guid auExportPK;

		const string OriginalXMLData = @"
<DataImportWizardSettings>
  <StartingRow>1</StartingRow>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>CusEntryLine+EntryLRNAndEntryLineNo</Name>
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
    <Name>XC_ProductManualNo</Name>
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
    <Name>XC_NameOfGoods</Name>
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
    <Name>XC_TradeQuantity</Name>
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
    <Name>XC_TradeUnitQty</Name>
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
    <Name>XC_ProductVersion</Name>
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
    <Name>XC_DutyMode</Name>
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
  <Delimiter>,</Delimiter>
  <TextQualifier>""""</TextQualifier>
  <FilterIndex>0</FilterIndex>
</DataImportWizardSettings>";

		const string UpdatedXMLData = @"
<DataImportWizardSettings>
  <StartingRow>1</StartingRow>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>CusEntryLine+EntryLRNAndEntryLineNo</Name>
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
    <Name>JI_ProductManualNo</Name>
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
    <Name>JI_NameOfGoods</Name>
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
    <Name>JI_TradeQuantity</Name>
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
    <Name>JI_TradeUnitQty</Name>
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
    <Name>JI_ProductVersion</Name>
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
    <Name>JI_DutyMode</Name>
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
  <Delimiter>,</Delimiter>
  <TextQualifier>""""</TextQualifier>
  <FilterIndex>0</FilterIndex>
</DataImportWizardSettings>";

		protected override void AssertPreConditions()
		{
			DIWTemplateUpdateHelperTest.AssertStmModuleFilterData(cnExportPK, OriginalXMLData);
			DIWTemplateUpdateHelperTest.AssertStmModuleFilterData(auExportPK, OriginalXMLData);
		}

		protected override void AssertTransformationResults()
		{
			DIWTemplateUpdateHelperTest.AssertStmModuleFilterData(cnExportPK, UpdatedXMLData);
			DIWTemplateUpdateHelperTest.AssertStmModuleFilterData(auExportPK, OriginalXMLData);
		}
	}
}
