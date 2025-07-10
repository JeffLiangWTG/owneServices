using System;
using System.Text;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;
using DIWTemplateUpdateHelperTest = Enterprise.DbUpgrader.Transformation.Common.Testing.DIWTemplateUpdateHelperTest;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Customs.TW.Testing
{
	[TestedType(typeof(FixTWInvoiceLineDIWTemplate))]
	public class FixTWInvoiceLineDIWTemplateTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new FixTWInvoiceLineDIWTemplate();

		protected override void PrepareTestData()
		{
			var nowDate = DateTime.Now;
			var testDataCreator = new TransformationTestDataCreator();
			var twGC = testDataCreator.CreateCompany("GC1", "TW");
			var twGB = testDataCreator.CreateGlbBranch("GB1", twGC);
			var auGC = testDataCreator.CreateCompany("GC2", "AU");
			var auGB = testDataCreator.CreateGlbBranch("GB2", auGC);

			var filterData = Encoding.ASCII.GetBytes(OriginalXMLData);
			twExportPK = testDataCreator.CreateModuleFilter(FixTWInvoiceLineDIWTemplate.ExportInvoiceLineDIW, "Test TW Export DIW", filterData, twGC);
			twImportPK = testDataCreator.CreateModuleFilter(FixTWInvoiceLineDIWTemplate.ImportInvoiceLineDIW, "Test TW Import DIW", filterData, twGC);
			auExportPK = testDataCreator.CreateModuleFilter(FixTWInvoiceLineDIWTemplate.ExportInvoiceLineDIW, "Test AU Export DIW", filterData, auGC);
			auImportPK = testDataCreator.CreateModuleFilter(FixTWInvoiceLineDIWTemplate.ImportInvoiceLineDIW, "Test AU Import DIW", filterData, auGC);
		}

		Guid twExportPK;
		Guid twImportPK;
		Guid auExportPK;
		Guid auImportPK;

		const string OriginalXMLData = @"
<DataImportWizardSettings>
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
    <ColumnIndexOrder>0</ColumnIndexOrder>
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>TW_AlcoholPercentage</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <ColumnIndexOrder>1</ColumnIndexOrder>
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>TW_AlcoholAge</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <ColumnIndexOrder>2</ColumnIndexOrder>
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Delimiter>,</Delimiter>
  <TextQualifier>""</TextQualifier>
  <FilterIndex>2</FilterIndex>
</DataImportWizardSettings>";

		const string UpdatedXMLData = @"
<DataImportWizardSettings>
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
    <ColumnIndexOrder>0</ColumnIndexOrder>
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_AlcoholPercentage</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <ColumnIndexOrder>1</ColumnIndexOrder>
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Mapping>
    <ColumnIndex>-1</ColumnIndex>
    <Name>JI_AlcoholAge</Name>
    <DefaultValue />
    <Expression />
    <MapAs />
    <ProperCase>False</ProperCase>
    <UpdateExisting>False</UpdateExisting>
    <CustomMapList />
    <Delimiter />
    <ColumnIndexOrder>2</ColumnIndexOrder>
    <WesternCharactersOnly>False</WesternCharactersOnly>
  </Mapping>
  <Delimiter>,</Delimiter>
  <TextQualifier>""</TextQualifier>
  <FilterIndex>2</FilterIndex>
</DataImportWizardSettings>";

		protected override void AssertPreConditions()
		{
			DIWTemplateUpdateHelperTest.AssertStmModuleFilterData(twExportPK, OriginalXMLData);
			DIWTemplateUpdateHelperTest.AssertStmModuleFilterData(twImportPK, OriginalXMLData);
			DIWTemplateUpdateHelperTest.AssertStmModuleFilterData(auExportPK, OriginalXMLData);
			DIWTemplateUpdateHelperTest.AssertStmModuleFilterData(auImportPK, OriginalXMLData);
		}

		protected override void AssertTransformationResults()
		{
			DIWTemplateUpdateHelperTest.AssertStmModuleFilterData(twExportPK, UpdatedXMLData);
			DIWTemplateUpdateHelperTest.AssertStmModuleFilterData(twImportPK, UpdatedXMLData);
			DIWTemplateUpdateHelperTest.AssertStmModuleFilterData(auExportPK, OriginalXMLData);
			DIWTemplateUpdateHelperTest.AssertStmModuleFilterData(auImportPK, OriginalXMLData);
		}
	}
}
