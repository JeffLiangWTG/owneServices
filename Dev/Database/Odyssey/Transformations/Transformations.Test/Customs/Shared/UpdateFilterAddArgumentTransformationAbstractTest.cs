using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.Shared;

[TestedType(typeof(UpdateFilterAddArgumentTransformation))]
public abstract class UpdateFilterAddArgumentTransformationAbstractTest : DataTransformationTestCase
{
	protected abstract string ModuleToUpdate { get; }

	protected abstract string OtherModule { get; }

	protected abstract string FilterDescriptionToUpdate { get; }

	protected abstract string OtherFilterDescription { get; }

	protected abstract XElement ParameterToBeAdded { get; }

	protected override void PrepareTestData()
	{
		var helper = new TransformationTestDataCreator();

		var mainFilter1 = helper.CreateModuleFilter(ModuleToUpdate, FilterDescriptionToUpdate, Encoding.ASCII.GetBytes(filterXml1));
		filterData1Pk = helper.CreateModuleFilterUserData("GS", new Guid(), mainFilter1, Encoding.ASCII.GetBytes(moduleFilterXmlBeforeTransform1));

		var mainFilter2 = helper.CreateModuleFilter(ModuleToUpdate, $"{FilterDescriptionToUpdate} 2", Encoding.ASCII.GetBytes(filterXml2));
		filterData2Pk = helper.CreateModuleFilterUserData("GS", new Guid(), mainFilter2, Encoding.ASCII.GetBytes(moduleFilterXmlBeforeTransform2));

		var mainFilter3 = helper.CreateModuleFilter(ModuleToUpdate, "Multiple filters", Encoding.ASCII.GetBytes(filterXml3));
		filterData3Pk = helper.CreateModuleFilterUserData("GS", new Guid(), mainFilter3, Encoding.ASCII.GetBytes(moduleFilterXmlBeforeTransform3));

		var mainFilter4 = helper.CreateModuleFilter($"{ModuleToUpdate}_CT", "Module Color Theme", Encoding.ASCII.GetBytes(filterXml4));
		filterData4Pk = helper.CreateModuleFilterUserData("GS", new Guid(), mainFilter4, Encoding.ASCII.GetBytes(moduleFilterXmlBeforeTransform4));

		var mainFilter5 = helper.CreateModuleFilter(OtherModule, $"Other Module with ({FilterDescriptionToUpdate})", Encoding.ASCII.GetBytes(filterXml1));
		filterData5Pk = helper.CreateModuleFilterUserData("GS", new Guid(), mainFilter5, Encoding.ASCII.GetBytes(moduleFilterXmlBeforeTransform1));

		var mainFilter6 = helper.CreateModuleFilter(ModuleToUpdate, "Filter has malformed XML", Encoding.ASCII.GetBytes(malformedFilterXML));
		filterData6Pk = helper.CreateModuleFilterUserData("GS", new Guid(), mainFilter6, Encoding.ASCII.GetBytes(moduleFilterXmlBeforeTransform1));

		var mainFilter7 = helper.CreateModuleFilter($"{ModuleToUpdate}_CS", FilterDescriptionToUpdate, Encoding.ASCII.GetBytes(filterXml1));
		filterData7Pk = helper.CreateModuleFilterUserData("GS", new Guid(), mainFilter7, Encoding.ASCII.GetBytes(moduleFilterXmlBeforeTransform1));

		var mainFilter8 = helper.CreateModuleFilter($"{ModuleToUpdate}_CT", "Module Color Theme 2", Encoding.ASCII.GetBytes(unrelatedFilter));
		filterData8Pk = helper.CreateModuleFilterUserData("GS", new Guid(), mainFilter8, Encoding.ASCII.GetBytes(moduleFilterXmlBeforeTransform4));

		var mainFilter9 = helper.CreateModuleFilter(ModuleToUpdate, "Filter has malformed module XML", Encoding.ASCII.GetBytes(filterXml1));
		filterData9Pk = helper.CreateModuleFilterUserData("GS", new Guid(), mainFilter9, Encoding.ASCII.GetBytes(malformedModuleFilterXML));

		var pkList = new List<Guid>
		{
			filterData1Pk,
			filterData2Pk,
			filterData3Pk,
			filterData4Pk,
			filterData5Pk,
			filterData6Pk,
			filterData7Pk,
			filterData8Pk,
			filterData9Pk,
		};

		var pkListForQuery = string.Join(",", pkList.Select(x => $"'{x}'"));
		var query = $"UPDATE dbo.StmModuleFilterUserData SET S0_SystemLastEditTimeUtc = '2023-03-03', S0_SystemLastEditUser ='~BP' WHERE S0_PK IN ({pkListForQuery})";
		Db.Connection.ExecuteNonQuery(query);
	}

	protected override void AssertTransformationResults()
	{
		var resultTable = GetTableResult(new List<Guid> { filterData1Pk, filterData2Pk, filterData3Pk, filterData4Pk, filterData5Pk, filterData6Pk, filterData7Pk, filterData8Pk, filterData9Pk });

		CombineAssertions(() =>
		{
			AssertTransformationResult($"'{FilterDescriptionToUpdate}' filter for {ModuleToUpdate} module should be updated", filterData1Pk, expectedModuleFilterXml1, resultTable);
			AssertTransformationResult("FilterType element already present, no update", filterData2Pk, moduleFilterXmlBeforeTransform2, resultTable, updated: false);
			AssertTransformationResult($"Multiple filters present, only the {FilterDescriptionToUpdate} filter parameters are updated", filterData3Pk, expectedModuleFilterXml3, resultTable);
			AssertTransformationResult($"Multiple {FilterDescriptionToUpdate} Filters are present for Module Color theme (_CT suffix), all are updated to have the additional parameter", filterData4Pk, expectedModuleFilterXml4, resultTable);
			AssertTransformationResult($"Module does not match that which is provided, no update", filterData5Pk, moduleFilterXmlBeforeTransform1, resultTable, updated: false);
			AssertTransformationResult($"Filter strip XML is malformed, will not be updated even though Module and Filter Description match", filterData6Pk, moduleFilterXmlBeforeTransform1, resultTable, updated: false);
			AssertTransformationResult($"'{FilterDescriptionToUpdate}' filter for {ModuleToUpdate} module should not be updated due to _CS suffix", filterData7Pk, moduleFilterXmlBeforeTransform1, resultTable, updated: false);
			AssertTransformationResult($"Should not be updated as '{FilterDescriptionToUpdate}' is not in FilterDescription", filterData8Pk, moduleFilterXmlBeforeTransform4, resultTable, updated: false);
			AssertTransformationResult($"Filter module XML is malformed, will not be updated even though Module and Filter Description match", filterData6Pk, moduleFilterXmlBeforeTransform1, resultTable, updated: false);
		});
	}

	void AssertTransformationResult(string message, Guid filterDataPk, string expectedXml, DataTable resultTable, bool updated = true)
	{
		var result = resultTable.Rows.Find(filterDataPk);
		var resultFilterData = XDocument.Parse(result["ModuleData"] as string ?? string.Empty).ToString();
		var resultLastEditUtc = result["S0_SystemLastEditTimeUtc"] as DateTime?;
		var resultLastEditUser = result["S0_SystemLastEditUser"] as string;
		AssertEquals(message, expectedXml, resultFilterData);

		if (updated)
		{
			AssertGreaterThan(DateTime.Now.Subtract(TimeSpan.FromMinutes(1)), resultLastEditUtc.Value);
			AssertEquals("E", resultLastEditUser);
		}
		else
		{
			AssertEquals(new DateTime(2023, 03, 03), resultLastEditUtc.Value);
			AssertEquals("~BP", resultLastEditUser);
		}
	}

	DataTable GetTableResult(List<Guid> pkList)
	{
		var pkListForQuery = string.Join(",", pkList.Select(x => $"'{x}'"));
		var sql = $@"
				SELECT
					S0_PK AS PK,
					dbo.CLRUncompressAsString(S9_FilterData) AS FilterData,
					dbo.CLRUncompressAsString(S0_FilterDataValues) AS ModuleData,
					S0_SystemLastEditTimeUtc,
					S0_SystemLastEditUser
				FROM dbo.StmModuleFilter JOIN dbo.StmModuleFilterUserData ON (StmModuleFilter.S9_PK = StmModuleFilterUserData.S0_S9)
				WHERE S0_PK in ({pkListForQuery})";

		var table = new DataTable();
		using (var cmd = Db.Connection.Command(sql))
		using (var adapter = cmd.NewDataAdapter())
		{
			adapter.Fill(table);
		}

		table.PrimaryKey = new[] { table.Columns[0] };

		return table;
	}

	#region first test case
	const string moduleFilterXmlBeforeTransform1 = @"<FilterLayoutValuesSerializer>
  <ModuleFilters>
    <ModuleFilter>
      <Comparer>exact</Comparer>
      <Property>RL1</Property>
    </ModuleFilter>
  </ModuleFilters>
</FilterLayoutValuesSerializer>";

	string expectedModuleFilterXml1 => $@"<FilterLayoutValuesSerializer>
  <ModuleFilters>
    <ModuleFilter>
      <Comparer>exact</Comparer>
      <Property>RL1</Property>
      {ParameterToBeAdded}
    </ModuleFilter>
  </ModuleFilters>
</FilterLayoutValuesSerializer>";

	string filterXml1 => $@"<FilterLayoutSerializer>
  <FilterStrips>
    <FilterStrip>
      <FilterDescription>{FilterDescriptionToUpdate}</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
  </FilterStrips>
</FilterLayoutSerializer>";

	#endregion

	#region second test case
	const string moduleFilterXmlBeforeTransform2 = @"<FilterLayoutValuesSerializer>
  <ModuleFilters>
    <ModuleFilter>
      <Comparer>exact</Comparer>
      <Property>RL3</Property>
      <FilterType>Any</FilterType>
    </ModuleFilter>
  </ModuleFilters>
</FilterLayoutValuesSerializer>";

	string filterXml2 => $@"<FilterLayoutSerializer>
  <FilterStrips>
    <FilterStrip>
      <FilterDescription>{FilterDescriptionToUpdate}</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
  </FilterStrips>
</FilterLayoutSerializer>";

	#endregion

	#region third test case
	const string moduleFilterXmlBeforeTransform3 = @"<FilterLayoutValuesSerializer>
  <ModuleFilters>
    <ModuleFilter>
      <SearchProperty>Last 12 Mths.</SearchProperty>
      <Property1 />
      <Property2 />
      <FilterOption>Past</FilterOption>
      <PropertyDecimal1>0.00</PropertyDecimal1>
      <PropertyDecimal2>0.00</PropertyDecimal2>
    </ModuleFilter>
    <ModuleFilter>
      <Comparer>starts with</Comparer>
      <Property>1234</Property>
    </ModuleFilter>
    <ModuleFilter>
      <Comparer>exact</Comparer>
      <Property>RL1</Property>
    </ModuleFilter>
    <ModuleFilter>
      <Comparer>starts with</Comparer>
      <Property>23</Property>
    </ModuleFilter>
  </ModuleFilters>
</FilterLayoutValuesSerializer>";

	string expectedModuleFilterXml3 => $@"<FilterLayoutValuesSerializer>
  <ModuleFilters>
    <ModuleFilter>
      <SearchProperty>Last 12 Mths.</SearchProperty>
      <Property1 />
      <Property2 />
      <FilterOption>Past</FilterOption>
      <PropertyDecimal1>0.00</PropertyDecimal1>
      <PropertyDecimal2>0.00</PropertyDecimal2>
    </ModuleFilter>
    <ModuleFilter>
      <Comparer>starts with</Comparer>
      <Property>1234</Property>
    </ModuleFilter>
    <ModuleFilter>
      <Comparer>exact</Comparer>
      <Property>RL1</Property>
      {ParameterToBeAdded}
    </ModuleFilter>
    <ModuleFilter>
      <Comparer>starts with</Comparer>
      <Property>23</Property>
    </ModuleFilter>
  </ModuleFilters>
</FilterLayoutValuesSerializer>";

	string filterXml3 => $@"<FilterLayoutSerializer>
  <FilterStrips>
    <FilterStrip>
      <FilterDescription>{OtherFilterDescription} 1</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>{OtherFilterDescription} 2</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>{FilterDescriptionToUpdate}</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>{OtherFilterDescription} 3</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
  </FilterStrips>
</FilterLayoutSerializer>";

	#endregion

	#region fourth test case
	const string moduleFilterXmlBeforeTransform4 = @"<FilterLayoutValuesSerializer>
  <ModuleFilters>
    <ModuleFilter>
      <SearchProperty>Last 12 Mths.</SearchProperty>
      <Property1 />
      <Property2 />
      <FilterOption>Past</FilterOption>
      <PropertyDecimal1>0.00</PropertyDecimal1>
      <PropertyDecimal2>0.00</PropertyDecimal2>
    </ModuleFilter>
    <ModuleFilter>
      <Comparer>exact</Comparer>
      <Property>RL1</Property>
    </ModuleFilter>
    <ModuleFilter>
      <Comparer>exact</Comparer>
      <Property>130</Property>
    </ModuleFilter>
  </ModuleFilters>
</FilterLayoutValuesSerializer>";

	string expectedModuleFilterXml4 => $@"<FilterLayoutValuesSerializer>
  <ModuleFilters>
    <ModuleFilter>
      <SearchProperty>Last 12 Mths.</SearchProperty>
      <Property1 />
      <Property2 />
      <FilterOption>Past</FilterOption>
      <PropertyDecimal1>0.00</PropertyDecimal1>
      <PropertyDecimal2>0.00</PropertyDecimal2>
    </ModuleFilter>
    <ModuleFilter>
      <Comparer>exact</Comparer>
      <Property>RL1</Property>
      {ParameterToBeAdded}
    </ModuleFilter>
    <ModuleFilter>
      <Comparer>exact</Comparer>
      <Property>130</Property>
      {ParameterToBeAdded}
    </ModuleFilter>
  </ModuleFilters>
</FilterLayoutValuesSerializer>";

	string filterXml4 => $@"<FilterLayoutSerializer>
  <FilterStrips>
    <FilterStrip>
      <FilterDescription>{OtherFilterDescription}</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>{FilterDescriptionToUpdate}</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>{FilterDescriptionToUpdate}</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
  </FilterStrips>
</FilterLayoutSerializer>";
	#endregion

	string unrelatedFilter => $@"<FilterLayoutSerializer>
  <FilterStrips>
    <FilterStrip>
      <FilterDescription>{OtherFilterDescription}</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName>{FilterDescriptionToUpdate}</GroupName>
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>{OtherFilterDescription}</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>{OtherFilterDescription}</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
  </FilterStrips>
</FilterLayoutSerializer>";

	#region malformed XML test case
	string malformedFilterXML => $@"<FilterLayoutSerializer>
  <FilterStrips>
    <FilterStrip>
      <FilterDescription>{FilterDescriptionToUpdate}</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip
  </FilterStrips>
</FilterLayoutSerializer>";

	string malformedModuleFilterXML => @"<FilterLayoutValuesSerializer>
  <ModuleFilters>
    <ModuleFilter
      <Comparer>exact</Comparer>
      <Property>RL1</Property>
    </ModuleFilter>
</FilterLayoutValuesSerializer>";
	#endregion

	Guid filterData1Pk;
	Guid filterData2Pk;
	Guid filterData3Pk;
	Guid filterData4Pk;
	Guid filterData5Pk;
	Guid filterData6Pk;
	Guid filterData7Pk;
	Guid filterData8Pk;
	Guid filterData9Pk;
}
