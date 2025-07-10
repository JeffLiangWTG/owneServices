using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.Data;
using Enterprise.Integration.Licensing;

namespace CargoWise.Bi.Registration.PowerBi
{
	public class PowerBiReportCollection
	{
		/// <summary>
		/// Public parameterless constructor
		/// </summary>
		public PowerBiReportCollection()
		{
		}

		/// <summary>
		/// Returns a set of powerBI report items of the specified enum.
		/// It dynamically creates the powerBI reports according to the chosen category.
		/// If "All" is chosen, it returns all PowerBiItem objects currently in the assembly
		/// </summary>
		/// <param name="filteredCategory"></param>
		/// <param name="getAllReports"></param>
		/// <returns>A list of PowerBiItem to be displayed</returns>
		public IEnumerable<PowerBiItem> GetFilteredPowerBiReports(BiReportCategory categoryFilter)
		{
			var allReports = GetAllPowerBiReportsFromAssembly().Where(r => !r.IsDataset);
			var availableModels = GetAvailableModelList();

			var registration = ObjectFactory.Get<IProductRegistration>();
			if (!registration.IsWiseTechGlobalInternalSystem())
			{
				allReports = allReports.Where(r => r.DeployToClients);
			}

			if (categoryFilter.Equals(BiReportCategory.Analytics))
			{
				return allReports;
			}
			else
			{
				return FilterReportListByCategory(allReports.Where(r => availableModels.Contains(r.DataSourceModel)), categoryFilter);
			}
		}

		public IEnumerable<PowerBiItem> GetFilteredAPIDatasetItems(BiReportCategory categoryFilter)
		{
			var allDatasets = GetAllPowerBiReportsFromAssembly().Where(r => r.IsDataset);
			var availableModels = GetAvailableModelList();

			var registration = ObjectFactory.Get<IProductRegistration>();
			if (!registration.IsWiseTechGlobalInternalSystem())
			{
				allDatasets = allDatasets.Where(r => r.DeployToClients);
			}

			if (categoryFilter.Equals(BiReportCategory.Analytics))
			{
				return allDatasets;
			}
			else
			{
				return FilterReportListByCategory(allDatasets.Where(r => availableModels.Contains(r.DataSourceModel)), categoryFilter);
			}
		}

		IEnumerable<PowerBiItem> FilterReportListByCategory(IEnumerable<PowerBiItem> reportItems, BiReportCategory categoryFilter)
		{
			if (reportItems != null)
			{
				foreach (var report in reportItems)
				{
					if (categoryFilter == BiReportCategory.All || report.Category == categoryFilter)
					{
						yield return report;
					}
				}
			}
		}

		public virtual IEnumerable<string> GetAvailableModelList()
		{
			var modelList = new List<string>();

			var dwServer = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(Db.Connection);
			if (!string.IsNullOrEmpty(dwServer))
			{
				using (var biConnection = (dwServer.Equals(Db.ServerName))
					? Db.Connection
					: Db.NewExtraConnectionWithMainDbCredentials(dwServer, Db.SqlMasterDb))
				{
					if (biConnection.DatabaseExists(Db.EdwDatabaseName))
					{
						using (((ICurrentDbControl)biConnection).UseDatabase(Db.EdwDatabaseName))
						{
							var sqlText = $"SELECT SsasModelLogicalName FROM [{BiConstants.BiAdminSchemaName}].[SsasCube] WHERE DeployToServer = 1";
							modelList = DataUtils.GetListOfValuesFromQuery(biConnection, sqlText).ToList();
						}
					}
				}
			}
			return modelList;
		}

		public static IEnumerable<PowerBiItem> GetAllPowerBiReportsFromAssembly()
		{
			return new List<PowerBiItem>();
		}
	}
}
