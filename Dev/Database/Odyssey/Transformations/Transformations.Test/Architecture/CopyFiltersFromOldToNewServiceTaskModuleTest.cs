using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.IO;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Architecture;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Architecture
{
	[TestedType(typeof(CopyFiltersFromOldToNewServiceTaskModule))]
	public class CopyFiltersFromOldToNewServiceTaskModuleTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new CopyFiltersFromOldToNewServiceTaskModule();
		}

		protected override void PrepareTestData()
		{
			var testDataCreator = new TransformationTestDataCreator();
			var companyPK = testDataCreator.CreateGlbCompany("TKR", "KR");
			staff1Pk = testDataCreator.CreateStaff("asd", "asd");
			staff2Pk = testDataCreator.CreateStaff("dsa", "dsa");
			var filterData = Compressor.Compress(Encoding.ASCII.GetBytes(InitialFilter));
			var userData = Compressor.Compress(Encoding.ASCII.GetBytes(InitialUserData));
			var customFilterData = Compressor.Compress(Encoding.ASCII.GetBytes(ExpectedLayoutResults.First()));
			TestConnection.ExecuteNonQuery(
				@"
INSERT INTO StmModuleFilter ([S9_PK],[S9_GC],[S9_ModuleID],[S9_RelatedEntityID],[S9_IsPublished],[S9_FilterData],[S9_ColumnLayoutData],[S9_SaveColumnLayout],[S9_IsSystem],[S9_FilterName],[S9_FilterType],[S9_ParentTableCode],[S9_SaveGridColourLayout],[S9_SystemCreateTimeUtc],[S9_SystemCreateUser],[S9_SystemLastEditTimeUtc],[S9_SystemLastEditUser],[S9_IsIndexSearch])
VALUES (@pk, @gc, 'ServiceTask', @reid, 0, @filterData, @customLayoutData,1,0,@filterName,'','',0,GetUtcDate(),'~BP',GetUtcDate(),'~BP',0)

INSERT INTO StmModuleFilterUserData ([S0_PK],[S0_RelatedEntityTableCode],[S0_RelatedEntityID],[S0_FilterDataValues],[S0_S9],[S0_SystemCreateTimeUtc],[S0_SystemCreateUser],[S0_SystemLastEditTimeUtc],[S0_SystemLastEditUser])
VALUES
	(NEWID(), 'GS', @staff1, @userData, @pk, GetUtcDate(),'~BP',GetUtcDate(),'~BP'),
	(NEWID(), 'GS', @staff2, @userData, @pk, GetUtcDate(),'~BP',GetUtcDate(),'~BP')
",
			cmd =>
			{
				cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, filterPk);
				cmd.AddParameter("@gc", SqlDbType.UniqueIdentifier, companyPK);
				cmd.AddParameter("@reid", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				cmd.AddParameter("@filterData", SqlDbType.Binary, filterData);
				cmd.AddParameter("@customLayoutData", SqlDbType.Binary, customFilterData);
				cmd.AddParameter("@filterName", SqlDbType.NVarChar, 3, "ASD");
				cmd.AddParameter("@staff1", SqlDbType.UniqueIdentifier, staff1Pk);
				cmd.AddParameter("@staff2", SqlDbType.UniqueIdentifier, staff2Pk);
				cmd.AddParameter("@userData", SqlDbType.Binary, userData);
			});
		}

		protected override void AssertTransformationResults()
		{
			var filterDataResults = new List<string>();
			var customLayoutResults = new List<string>();
			Db.Connection.ExecuteReader("SELECT * FROM StmModuleFilter WHERE S9_ModuleID = 'StmServiceTask'",
				reader =>
				{
					var filterData = (byte[])reader["S9_FilterData"];
					var customLayoutData = (byte[])reader["S9_ColumnLayoutData"];
					filterDataResults.Add(Compressor.UncompressAsString(filterData));
					customLayoutResults.Add(Compressor.UncompressAsString(customLayoutData));
				});

			var userDataResults = new List<(Guid userGuid, string filterData)>();
			Db.Connection.ExecuteReader("SELECT * FROM [StmModuleFilterUserData] WHERE S0_S9 = @pk",
				cmd => { cmd.AddParameter("@pk", SqlDbType.UniqueIdentifier, filterPk); },
				reader =>
				{
					userDataResults.Add(((Guid)reader["S0_RelatedEntityID"], Compressor.UncompressAsString((byte[])reader["S0_FilterDataValues"])));
				});

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Did not match expected StmModuleFilter", ExpectedFilterResults, filterDataResults);
				AssertContainsExactElementsInAnyOrder("Did not match expected StmModuleFilter column layout", ExpectedLayoutResults, customLayoutResults);
				AssertContainsExactElementsInAnyOrder("Users not matched", new[] { staff1Pk, staff2Pk }, userDataResults.Select(r => r.userGuid));
				AssertContainsExactElementsInAnyOrder("user data does not match", new[] { InitialUserData, InitialUserData }, userDataResults.Select(r => r.filterData));
			});
		}

		static Guid filterPk = Guid.NewGuid();
		Guid staff1Pk;
		Guid staff2Pk;

		static IEnumerable<string> ExpectedFilterResults => new[] { @"<?xml version=""1.0""?>
<?xml version=""1.0""?>
<FilterLayoutSerializer>
  <FilterStrips>
    <FilterStrip>
      <FilterDescription>Status</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>Creating User</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>Code</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>Running Count</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>BindingsCount</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>ErrorCountLast24Hours</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>Active Status</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>Mutually Exclusive Group</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>Branch</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>BindingTypes</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>Description</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>PlaceInQueue</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>ProcessHost</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>ProcessID</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>ProductArea</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>RegisteredOnHosts</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>SecondsInQueue</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>SecondsRunning</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
  </FilterStrips>
</FilterLayoutSerializer>" };
		static IEnumerable<string> ExpectedLayoutResults => new[] { @"<NewDataSet>
  <OGridColumnSettings>
    <MappingName>SST_ServiceTaskCode</MappingName>
    <Width>60</Width>
    <IsVisible>true</IsVisible>
  </OGridColumnSettings>
  <OGridColumnSettings>
    <MappingName>Description</MappingName>
    <Width>150</Width>
    <IsVisible>true</IsVisible>
  </OGridColumnSettings>
  <OGridColumnSettings>
    <MappingName>Category</MappingName>
    <Width>60</Width>
    <IsVisible>true</IsVisible>
  </OGridColumnSettings>
  <OGridColumnSettings>
    <MappingName>SST_Active</MappingName>
    <Width>60</Width>
    <IsVisible>true</IsVisible>
  </OGridColumnSettings>
  <OGridColumnSettings>
    <MappingName>StatusString</MappingName>
    <Width>60</Width>
    <IsVisible>true</IsVisible>
  </OGridColumnSettings>
  <OGridColumnSettings>
    <MappingName>ErrorCountLast24Hours</MappingName>
    <Width>80</Width>
    <IsVisible>true</IsVisible>
  </OGridColumnSettings>
  <OGridColumnSettings>
    <MappingName>RunningCount</MappingName>
    <Width>100</Width>
    <IsVisible>true</IsVisible>
  </OGridColumnSettings>
  <OGridColumnSettings>
    <MappingName>ProcessIDsString</MappingName>
    <Width>100</Width>
    <IsVisible>true</IsVisible>
  </OGridColumnSettings>
  <OGridColumnSettings>
    <MappingName>PlaceInQueueString</MappingName>
    <Width>100</Width>
    <IsVisible>true</IsVisible>
  </OGridColumnSettings>
  <OGridColumnSettings>
    <MappingName>SecondsInQueueString</MappingName>
    <Width>100</Width>
    <IsVisible>true</IsVisible>
  </OGridColumnSettings>
  <OGridColumnSettings>
    <MappingName>SecondsRunningString</MappingName>
    <Width>100</Width>
    <IsVisible>true</IsVisible>
  </OGridColumnSettings>
  <OGridColumnSettings>
    <MappingName>SchedulePeriodDuration</MappingName>
    <Width>140</Width>
    <IsVisible>true</IsVisible>
  </OGridColumnSettings>
  <OGridColumnSettings>
    <MappingName>ServiceTaskBindingsCount</MappingName>
    <Width>80</Width>
    <IsVisible>true</IsVisible>
  </OGridColumnSettings>
  <OGridColumnSettings>
    <MappingName>ServiceTaskBindingTypesString</MappingName>
    <Width>140</Width>
    <IsVisible>true</IsVisible>
  </OGridColumnSettings>
  <OGridColumnSettings>
    <MappingName>RegisteredOnHosts</MappingName>
    <Width>140</Width>
    <IsVisible>true</IsVisible>
  </OGridColumnSettings>
  <OGridColumnSettings>
    <MappingName>MutuallyExclusiveGroup</MappingName>
    <Width>140</Width>
    <IsVisible>true</IsVisible>
  </OGridColumnSettings>
  <OGridColumnSettings>
    <MappingName>SST_SystemCreateUser</MappingName>
    <Width>66</Width>
    <IsVisible>false</IsVisible>
  </OGridColumnSettings>
  <OGridColumnSettings>
    <MappingName>SST_SystemCreateTimeUtc</MappingName>
    <Width>100</Width>
    <IsVisible>false</IsVisible>
  </OGridColumnSettings>
  <OGridColumnSettings>
    <MappingName>SST_SystemLastEditUser</MappingName>
    <Width>66</Width>
    <IsVisible>false</IsVisible>
  </OGridColumnSettings>
  <OGridColumnSettings>
    <MappingName>SST_SystemLastEditTimeUtc</MappingName>
    <Width>100</Width>
    <IsVisible>false</IsVisible>
  </OGridColumnSettings>
  <OGridColumnSettings>
    <MappingName>SST_GB_Branch</MappingName>
    <Width>60</Width>
    <IsVisible>false</IsVisible>
  </OGridColumnSettings>
  <OGridColumnSettings>
    <MappingName>CalcLastErrorTimeLocal</MappingName>
    <Width>80</Width>
    <IsVisible>false</IsVisible>
  </OGridColumnSettings>
  <OGridColumnSettings>
    <MappingName>CalcNextRunTimeLocal</MappingName>
    <Width>80</Width>
    <IsVisible>false</IsVisible>
  </OGridColumnSettings>
  <OGridColumnSettings>
    <MappingName>CalcLastRunTimeLocal</MappingName>
    <Width>80</Width>
    <IsVisible>false</IsVisible>
  </OGridColumnSettings>
  <OGridColumnSettings>
    <MappingName>LastErrorTime</MappingName>
    <Width>80</Width>
    <IsVisible>false</IsVisible>
  </OGridColumnSettings>
  <OGridColumnSettings>
    <MappingName>NextRunTime</MappingName>
    <Width>80</Width>
    <IsVisible>false</IsVisible>
  </OGridColumnSettings>
  <OGridColumnSettings>
    <MappingName>LastRunTime</MappingName>
    <Width>80</Width>
    <IsVisible>false</IsVisible>
  </OGridColumnSettings>
</NewDataSet>" };

		static string InitialFilter = @"<?xml version=""1.0""?>
<?xml version=""1.0""?>
<FilterLayoutSerializer>
  <FilterStrips>
    <FilterStrip>
      <FilterDescription>Status</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>Creating User</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>Code</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>Running Count</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>BindingsCount</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>ErrorCountLast24Hours</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>Active Status</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>Mutually Exclusive Group</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>Branch</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>BindingTypes</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>Description</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>PlaceInQueue</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>ProcessHost</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>ProcessID</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>ProductArea</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>RegisteredOnHosts</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>SecondsInQueue</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>SecondsRunning</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
  </FilterStrips>
</FilterLayoutSerializer>";

		static string InitialUserData = @"<?xml version=""1.0""?>
<FilterLayoutValuesSerializer>
  <ModuleFilters>
    <ModuleFilter>
      <Comparer>starts with</Comparer>
      <Property>Idle</Property>
    </ModuleFilter>
    <ModuleFilter>
      <Comparer>current user</Comparer>
      <Property>E</Property>
    </ModuleFilter>
    <ModuleFilter>
      <Comparer>starts with</Comparer>
      <Property>DSA</Property>
    </ModuleFilter>
  </ModuleFilters>
</FilterLayoutValuesSerializer>";
	}
}
