using System;
using System.Data;
using System.Text;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformations;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.Common.Testing
{
	public sealed class DIWTemplateUpdateHelperTest : TransactionedTestCase
	{
		public void TestUpdateTemplateMappingPrefix()
		{
			var testDataCreator = new TransformationTestDataCreator();
			var twGC = testDataCreator.CreateCompany("GC1", "TW");
			var twGB = testDataCreator.CreateGlbBranch("GB1", twGC);
			var auGC = testDataCreator.CreateCompany("GC2", "AU");
			var auGB = testDataCreator.CreateGlbBranch("GB2", auGC);

			var moduleIDMaxLength = StmModuleFilterSchema.S9_ModuleID.MaxLength;
			AssertEquals("Currently we use dbo.TVP_varchar_200; please ensure that it match S9_ModuleID length", 200, moduleIDMaxLength);
			var filterData = Encoding.ASCII.GetBytes(OriginalXMLData);
			var moduleID1 = DIWModuleID1.PadRight(moduleIDMaxLength, '1');
			var twExportPK = testDataCreator.CreateModuleFilter(moduleID1, "Test TW Export DIW", filterData, twGC);
			var twImportPK = testDataCreator.CreateModuleFilter(DIWModuleID2, "Test TW Import DIW", filterData, twGC);
			var auExportPK = testDataCreator.CreateModuleFilter(moduleID1, "Test AU Export DIW", filterData, auGC);
			var auImportPK = testDataCreator.CreateModuleFilter(DIWModuleID2, "Test AU Import DIW", filterData, auGC);
			DIWTemplateUpdateHelper.UpdateTemplateMappingPrefix("TW", "JI", "TW", moduleID1, DIWModuleID2);
			AssertStmModuleFilterData(twExportPK, UpdatedXMLData);
			AssertStmModuleFilterData(twImportPK, UpdatedXMLData);
			AssertStmModuleFilterData(auExportPK, OriginalXMLData);
			AssertStmModuleFilterData(auImportPK, OriginalXMLData);
		}

		public static void AssertStmModuleFilterData(Guid pk, string expectedValue)
		{
			var schemeSql = $@"
SELECT dbo.CLRUncompressAsString(S9_FilterData) AS FilterData
FROM dbo.StmModuleFilter
WHERE S9_PK = @pk";

			using (var cmd = Db.Connection.Command(schemeSql))
			{
				cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				AssertEquals(expectedValue, (string)cmd.ExecuteScalar());
			}
		}

		const string DIWModuleID1 = "DIW:ABC1234567890";
		const string DIWModuleID2 = "DIW:DEF1234567890";

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
	}
}
