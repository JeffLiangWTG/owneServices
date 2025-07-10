using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.BufferManagement.GUI.Test
{
	class AcceptabilityBandPerformanceTest : BMSTestCaseWithFactory
	{
		public void TestInactiveWorkflowsBand_ShouldUseIndexSeek_AndNotScan()
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			var activeWorkflows = BMSTestHelper.CreateWorkflows(config.Buffer, 100, 1);
			var inactiveWorkflows = BMSTestHelper.CreateWorkflows(config.Buffer, 4, 1);
			inactiveWorkflows.ForEach(x => x.FH_IsActive = false);

			Factory.Save();

			var band = BMSTestHelper.CreateAcceptabilityBand(Factory, 0, 0, 0, 0, 0, 0, "Inactive Workflows", type: AcceptabilityBandTypes.Codes.Count);
			band.BAB_FiltersBySection = false;
			FilterStripsTestHelper.AddFilterStrip<ModuleTextFilter>(band.FilterRule, ProcessHeader.ModuleFilterConstants.ActiveStatus, f => f.Property = "Inactive");

			var planalyzer = GetPlanalyzerForAcceptabilityBandQuery(band);

			CombineAssertions(() =>
			{
				AssertEquals("The query should use an index seek. SAD!", 1, planalyzer.IndexSeeks.Count());
				AssertEquals("The query should not use index scans. SAD!", 0, planalyzer.IndexScans.Count());
				AssertEquals("The query should not use table scans. SAD!", 0, planalyzer.TableScans.Count());
			});

			var command = GetAcceptabilityBandCommand(band);
			var result = command.ExecuteScalar();

			AssertEquals("The band should return the correct result, which proves that it is configured correctly for this test. SAD!", 4m, result);
		}

		public void TestQueryHeader()
		{
			var band = BMSTestHelper.CreateAcceptabilityBand(Factory, 0, 0, 0, 0, 0, 0, "Carole Baskin", type: AcceptabilityBandTypes.Codes.Count);
			var command = GetAcceptabilityBandCommand(band);

			AssertStartsWith("The query should include info about the database name and band name, so we can track down bad queries that show up in Kibana. SAD!", $@"
-- Database: {TestConnection.CurrentDatabase}
-- Acceptability Band name: [Carole Baskin], Type: [NUM]", command.CommandText);
		}

		#region Implementation

		QueryPlanalyzer GetPlanalyzerForAcceptabilityBandQuery(BMComponentAcceptabilityBand band)
		{
			var command = GetAcceptabilityBandCommand(band);
			var commandText = command.CommandText;

			if (command.ParameterCount > 0)
			{
				var filterQuery = RelatedModuleFiltersHelper.GetFilterQuery(band.FilterRule);
				var parameterString = new ZStringBuilder();

				foreach (var parameter in filterQuery.Params)
				{
					parameterString.AppendLine($"DECLARE {parameter.ParameterName} {parameter.SchemaColumn.SqlDbTypeDeclaration} = {parameter.ParameterValueTextSql};");
				}

				commandText = parameterString + System.Environment.NewLine + commandText;
			}

			return new QueryPlanalyzer(commandText, TestConnection);
		}

		DbCommand GetAcceptabilityBandCommand(BMComponentAcceptabilityBand band)
		{
			var parameters = new AcceptabilityBandSqlBuilderParameters(band);
			return band.GetAcceptabilityBandSqlCommand(parameters, TestConnection);
		}

		#endregion
	}
}
