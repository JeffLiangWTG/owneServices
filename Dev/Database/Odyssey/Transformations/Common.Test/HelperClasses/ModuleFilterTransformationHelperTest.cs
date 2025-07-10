using System.Xml.Linq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.Common.Test.HelperClasses;

[TestedType(typeof(ModuleFilterTransformationHelper))]
public class ModuleFilterTransformationHelperTest : TestCase
{
	public void TestUpdateFilterAddParameter()
	{
		var filterData = XDocument.Parse(singleFilterStripData);
		var filterParameterData = XDocument.Parse(singleFilterParameterData);

		var elementToAdd = new XElement("NewParameter", "ABC");

		var updated = ModuleFilterTransformationHelper.UpdateFilterParameter(filterData, filterParameterData, "Entry Status", elementToAdd);

		CombineAssertions(() =>
		{
			AssertEquals(true, updated);

			var updatedFilterParameters = filterParameterData.ToString();

			AssertMultilineASCIIEquals("Parameter is added to the XML", updatedSingleFilterParameterData, updatedFilterParameters);
		});
	}

	public void TestUpdateFilterAddParameterWhenMultipleMatchingFilters()
	{
		var filterData = XDocument.Parse(multipleFilterStripData);
		var filterParameterData = XDocument.Parse(multipleFilterParameterData);

		var elementToAdd = new XElement("Number", "5678");

		var updated = ModuleFilterTransformationHelper.UpdateFilterParameter(filterData, filterParameterData, "Job Number", elementToAdd);

		CombineAssertions(() =>
		{
			AssertEquals(true, updated);

			var updatedFilterParameters = filterParameterData.ToString();

			AssertMultilineASCIIEquals(
				"Parameter element is added to the Module Filter elements for the multiple matching filter strips",
				updatedMultipleFilterParameterData,
				updatedFilterParameters);
		});
	}

	public void TestUpdateFilterAddParameterWhenParameterAlreadyExists()
	{
		var filterData = XDocument.Parse(singleFilterParameterData);
		var filterParameterData = XDocument.Parse(updatedSingleFilterParameterData);

		var elementToAdd = new XElement("NewParameter", "ABC");

		var updated = ModuleFilterTransformationHelper.UpdateFilterParameter(filterData, filterParameterData, "Entry Status", elementToAdd);

		CombineAssertions(() =>
		{
			AssertEquals("Not updated as the parameters already contain the parameter we try to add", false, updated);

			var filterParameters = filterParameterData.ToString();

			AssertMultilineASCIIEquals("XML should not be changed", updatedSingleFilterParameterData, filterParameters);
		});
	}

	#region single filter strip test case data
	const string singleFilterStripData = $@"<FilterLayoutSerializer>
  <FilterStrips>
    <FilterStrip>
      <FilterDescription>Entry Status</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
  </FilterStrips>
</FilterLayoutSerializer>";

	const string singleFilterParameterData = @"<FilterLayoutValuesSerializer>
  <ModuleFilters>
    <ModuleFilter>
      <Comparer>exact</Comparer>
      <Property>RL1</Property>
    </ModuleFilter>
  </ModuleFilters>
</FilterLayoutValuesSerializer>";

	const string updatedSingleFilterParameterData = @"<FilterLayoutValuesSerializer>
  <ModuleFilters>
    <ModuleFilter>
      <Comparer>exact</Comparer>
      <Property>RL1</Property>
      <NewParameter>ABC</NewParameter>
    </ModuleFilter>
  </ModuleFilters>
</FilterLayoutValuesSerializer>";
	#endregion

	#region multiple filter strips test case data

	const string multipleFilterStripData = $@"<FilterLayoutSerializer>
  <FilterStrips>
    <FilterStrip>
      <FilterDescription>Created Time</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>Job Number</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>Job Number</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
    <FilterStrip>
      <FilterDescription>Created Date</FilterDescription>
      <OrCategory>None</OrCategory>
      <GroupOrCategory>None</GroupOrCategory>
      <GroupName />
      <AdditionalColourName />
      <AdditionalGroupColourName />
      <FilterPropertyLockStatus>False</FilterPropertyLockStatus>
    </FilterStrip>
  </FilterStrips>
</FilterLayoutSerializer>";

	const string multipleFilterParameterData = @"<FilterLayoutValuesSerializer>
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

	const string updatedMultipleFilterParameterData = @"<FilterLayoutValuesSerializer>
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
      <Number>5678</Number>
    </ModuleFilter>
    <ModuleFilter>
      <Comparer>exact</Comparer>
      <Property>RL1</Property>
      <Number>5678</Number>
    </ModuleFilter>
    <ModuleFilter>
      <Comparer>starts with</Comparer>
      <Property>23</Property>
    </ModuleFilter>
  </ModuleFilters>
</FilterLayoutValuesSerializer>";

	#endregion
}
