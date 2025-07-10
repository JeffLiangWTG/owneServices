using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Bi.Common;
using CargoWise.Bi.Registration.PowerBi;
using CargoWise.Data;
using CargoWise.Data.Testing;
using NUnit.Framework;

namespace CargoWise.Bi.Registration.Testing
{
	public class PowerBiReportCollectionTest : TestCase
	{
		const string logisticModelLogicalName = "LogisticsModel";

		[UseSnapshotProtection]
		public void TestGetAllPowerBiReports()
		{
			var tabularModelsToDeploy = new List<string>
			{
				logisticModelLogicalName
			};
			SetTabularModelDeployFlagAndAssertPowerBiCollection(tabularModelsToDeploy);
		}

		[UseSnapshotProtection]
		public void TestGetPowerBiReports_OnlyLogisticsModelIsDeployed()
		{
			var tabularModelsToDeploy = new List<string> { logisticModelLogicalName };
			SetTabularModelDeployFlagAndAssertPowerBiCollection(tabularModelsToDeploy);
		}

		void SetTabularModelDeployFlagAndAssertPowerBiCollection(IEnumerable<string> tabularModelsToDeploy)
		{
			using (var connection = Db.NewAdminConnection())
			using (SnapshotCreator.CreateSnapshot(connection, () => Db.Connection.CloseConnection(), Db.EdwDatabaseName, Db.EdwDatabaseName + "-SS"))
			{
				SetTabularModelDeployToServerFlag(connection, tabularModelsToDeploy);
				AssertPowerBiReportCollectionIsComplete(tabularModelsToDeploy);
			}
		}

		void SetTabularModelDeployToServerFlag(DbConnection connection, IEnumerable<string> deployedCubes)
		{
			using (((ICurrentDbControl)connection).UseDatabase(Db.EdwDatabaseName))
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture,
@"UPDATE [{0}].[SsasCube] SET DeployToServer = 0 WHERE [SsasModelLogicalName] NOT IN ('{1}')
 UPDATE [{0}].[SsasCube] SET DeployToServer = 1 WHERE [SsasModelLogicalName] IN ('{1}')",
					BiConstants.BiAdminSchemaName,
					string.Join("', '", deployedCubes));

				connection.ExecuteNonQuery(sqlText);
			}
		}

		void AssertPowerBiReportCollectionIsComplete(IEnumerable<string> modelFilterList)
		{
			var powerBiReportsFromCollection = new PowerBiReportCollection().GetFilteredPowerBiReports(BiReportCategory.All).Select(r => r.Name);
			var powerBiReportItemsFromAssembly = GetPowerBiReportsFromAssemblyWithDataSource(modelFilterList);
			AssertContainsExactElementsInAnyOrder("Power BI report list", powerBiReportItemsFromAssembly, powerBiReportsFromCollection);
		}

		IEnumerable<string> GetPowerBiReportsFromAssemblyWithDataSource(IEnumerable<string> modelFilterList)
		{
			return new DeploymentFileLoader().GetAllPowerBiReports()
				.Where(r => modelFilterList.Contains(r.DataSourceModel) && !r.IsDataset)
				.Select(r => r.Name);
		}
	}
}
