using System.Globalization;
using System.Linq;
using CargoWise.Bi.Common;
using CargoWise.Bi.ConfigLoader;
using CargoWise.Data;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Manager.Business.Testing
{
	class EdwInformationTest : TransactionedTestCase
	{
		public void TestSourceTablesAreUnique()
		{
			var dwServer = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(TestConnection);
			var edwInfo = new EdwInformation(dwServer, Db.EdwDatabaseName);
			edwInfo.RefreshInfo();

			var edwSourceTablesFromConfiguration = BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.Count(t => t.TableInEdw && !t.IsEdiClient);

			var sourceTableNames = edwInfo.EdwSourceTables.Select(t => t.Name);
			var duplicatedTables = sourceTableNames.Distinct().Where(t1 => sourceTableNames.Count(t2 => t1 == t2) > 1);

			CombineAssertions(() =>
			{
				AssertEquals("EDW Source Table count:", edwSourceTablesFromConfiguration, edwInfo.EdwSourceTables.Count);
				Assert(string.Format(CultureInfo.InvariantCulture, "Duplicated entries:\r\n{0}", string.Join("\r\n", duplicatedTables)), !duplicatedTables.Any());
			});
		}

		public void TestBaseTablesAreUnique()
		{
			var dwServer = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(TestConnection);
			var edwInfo = new EdwInformation(dwServer, Db.EdwDatabaseName);
			edwInfo.RefreshInfo();

			var stagingTables = BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.Where(t => t.TableInEdw && !t.IsEdiClient).Select(t => t.SourceTable);
			var edwBaseTablesFromConfiguration = BiAutomationConfigLoader.Instance.ConfigData.EdwTableConfig.Count(t => stagingTables.Contains(t.StagingTable));

			var baseTableNames = edwInfo.EdwBaseTables.Select(t => string.Format(CultureInfo.InvariantCulture, "Name: {0} Transform ID: {1}", t.Name, t.TransformId));
			var duplicatedTables = baseTableNames.Distinct().Where(t1 => baseTableNames.Count(t2 => t1 == t2) > 1);

			CombineAssertions(() =>
			{
				AssertEquals("EDW Base Table count:", edwBaseTablesFromConfiguration, edwInfo.EdwBaseTables.Count);
				Assert(string.Format(CultureInfo.InvariantCulture, "Duplicated entries:\r\n{0}", string.Join("\r\n", duplicatedTables)), !duplicatedTables.Any());
			});
		}

		public void TestAggregateTablesAreUnique()
		{
			var dwServer = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(TestConnection);
			var edwInfo = new EdwInformation(dwServer, Db.EdwDatabaseName);
			edwInfo.RefreshInfo();

			var edwAggregateTablesFromConfiguration = BiAutomationConfigLoader.Instance.ConfigData.EdwDenormalizedTableConfig.Count;

			var aggTableNames = edwInfo.EdwAggregateTables.Select(t => t.Name);
			var duplicatedTables = aggTableNames.Distinct().Where(t1 => aggTableNames.Count(t2 => t1 == t2) > 1);

			CombineAssertions(() =>
			{
				AssertEquals("EDW Aggregate Table count:", edwAggregateTablesFromConfiguration, edwInfo.EdwAggregateTables.Count);
				Assert(string.Format(CultureInfo.InvariantCulture, "Duplicated entries:\r\n{0}", string.Join("\r\n", duplicatedTables)), !duplicatedTables.Any());
			});
		}

		public void TestCustomTablesAreUnique()
		{
			var dwServer = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(TestConnection);
			var edwInfo = new EdwInformation(dwServer, Db.EdwDatabaseName);
			edwInfo.RefreshInfo();

			var edwCustomTablesFromConfiguration = BiAutomationConfigLoader.Instance.ConfigData.EdwCustomTableConfig.Count;

			var customTableNames = edwInfo.EdwCustomTables.Select(t => t.Name);
			var duplicatedTables = customTableNames.Distinct().Where(t1 => customTableNames.Count(t2 => t1 == t2) > 1);

			CombineAssertions(() =>
			{
				AssertEquals("EDW Custom Table count:", edwCustomTablesFromConfiguration, edwInfo.EdwCustomTables.Count);
				Assert(string.Format(CultureInfo.InvariantCulture, "Duplicated entries:\r\n{0}", string.Join("\r\n", duplicatedTables)), !duplicatedTables.Any());
			});
		}
	}
}
