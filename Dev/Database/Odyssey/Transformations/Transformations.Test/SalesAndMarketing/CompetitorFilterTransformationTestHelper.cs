using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.SalesAndMarketing.Testing
{
	public class CompetitorFilterTransformationTestHelper
	{
		public void AssertTransformedFilterOutput(List<Guid> filterPks, string expectedFilterData, string expectedFilterDataValue, string assertionMessage)
		{
			var schemeTable = GetTableResult(filterPks);
			var expectedFilterDataOutput = XDocument.Parse(expectedFilterData).ToString();
			var expectedFilterDataValuesOutput = XDocument.Parse(expectedFilterDataValue).ToString();

			foreach (DataRow row in schemeTable.Rows)
			{
				var filterDataOutput = XDocument.Parse(row["FilterData"] as string ?? string.Empty).ToString();
				var filterDataValuesOutput = XDocument.Parse(row["FilterDataValues"] as string ?? string.Empty).ToString();

				AssertionWithHtml.CombineAssertions(assertionMessage, () =>
				{
					Assertion.AssertEquals(expectedFilterDataOutput, filterDataOutput);
					Assertion.AssertEquals(expectedFilterDataValuesOutput, filterDataValuesOutput);
				});
			}
		}

		public DataTable GetTableResult(List<Guid> pkList)
		{
			var pkListForQuery = string.Join(",", pkList.Select(x => "'" + x + "'"));
			var schemeSql = $@"
							SELECT
							S9_PK AS PK,
							dbo.CLRUncompressAsString(S9_FilterData) AS FilterData,
							dbo.CLRUncompressAsString(S0_FilterDataValues) AS FilterDataValues
							from dbo.StmModuleFilter join dbo.StmModuleFilterUserData on (StmModuleFilter.S9_PK = StmModuleFilterUserData.S0_S9)
							Where s9_pk in ({pkListForQuery})";

			var schemeTable = new DataTable();
			using (var schemeCmd = Db.Connection.Command(schemeSql))
			using (var schemeAdapter = schemeCmd.NewDataAdapter())
			{
				schemeAdapter.Fill(schemeTable);
			}
			schemeTable.PrimaryKey = new DataColumn[] { schemeTable.Columns[0] };

			return schemeTable;
		}

		public string GetStmModuleFilterXml(string[] filterDescription)
		{
			var text = filterDescription.Aggregate(@"
<FilterLayoutSerializer>
	<FilterStrips>", (current, filter) => current + $@"
		<FilterStrip>
			<FilterDescription>{filter}</FilterDescription>
			<OrCategory>None</OrCategory>
			<GroupOrCategory>None</GroupOrCategory>
			<GroupName />
			<AdditionalColourName />
			<AdditionalGroupColourName />
			<FilterPropertyLockStatus>False</FilterPropertyLockStatus>
		</FilterStrip>");

			text += @"
	</FilterStrips>
</FilterLayoutSerializer>";

			return text;
		}

		public string GetStmModuleFilterUserDataXml(string[] properties)
		{
			var text = properties.Aggregate(@"
<FilterLayoutValuesSerializer>
	<ModuleFilters>", (current, property) => current + $@"
		<ModuleFilter>
			{property}
		</ModuleFilter>");

			text += @"
	</ModuleFilters>
</FilterLayoutValuesSerializer>";
			return text;
		}

		public string GetExpectedStmModuleFilterXml(string newFilterDescription, int iteration)
		{
			var expectedFilterDescription = new string[iteration];

			for (var i = 0; i < iteration; i++)
			{
				expectedFilterDescription[i] = newFilterDescription;
			}

			return GetStmModuleFilterXml(expectedFilterDescription);
		}

		public string GetExpectedStmModuleFilterUserDataXml(string[] competitorTypes, string[] properties, string propertyName)
		{
			var expectedProperties = new string[properties.Length];
			for (var i = 0; i < properties.Length; i++)
			{
				expectedProperties[i] =
$@"<CompetitorType>{competitorTypes[i]}</CompetitorType>
<{propertyName}>{properties[i]}</{propertyName}>";
			}
			return GetStmModuleFilterUserDataXml(expectedProperties);
		}
	}
}
