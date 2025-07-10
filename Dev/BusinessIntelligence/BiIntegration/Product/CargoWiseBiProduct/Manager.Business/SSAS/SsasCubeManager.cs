using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Bi.Common;
using CargoWise.Bi.ConfigLoader;
using CargoWise.Data;
using CargoWise.Types;
using static CargoWise.Bi.Configuration.DataSets.BiAutomationConfigDataSet;

namespace CargoWise.Bi.Product.Manager.Business
{
	public class SsasModelManager : IDisposable
	{
		public SsasModelManager()
		{
			var dwServer = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(Db.Connection);
			if (!string.IsNullOrEmpty(dwServer))
			{
				BiConnection = Db.NewExtraConnectionWithMainDbCredentials(dwServer, Db.EdwDatabaseName);
			}
			else
			{
				BiConnection = null;
			}
		}
		DbConnection BiConnection;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (BiConnection != null)
				{
					BiConnection.Dispose();
					BiConnection = null;
				}
			}
		}

		#region Tabular Model Menu Options

		public void ActivateModels(IEnumerable<ZString> tabularModels)
		{
			ExecuteQuery(activateModelQuery, tabularModels);
		}

		public void DeactivateModels(IEnumerable<ZString> tabularModels)
		{
			ExecuteQuery(deactivateModelQuery, tabularModels);
		}

		public void ReprocessModels(IEnumerable<ZString> tabularModels)
		{
			ExecuteQuery(reprocessModelQuery, tabularModels);
		}

		public void RedeployModels(IEnumerable<ZString> tabularModels)
		{
			ExecuteQuery(redeployModelQuery, tabularModels);
		}

		public void DisableEtlOnModel(IEnumerable<ZString> tabularModels)
		{
			UpdateEtlOnSelectedModels(tabularModels, enable: false);
			EnsureEnableEtlTrueOnStagingTableStateOnlyForEnabledTabularModels();
		}

		public void EnableEtlOnModel(IEnumerable<ZString> tabularModels)
		{
			UpdateEtlOnSelectedModels(tabularModels, enable: true);
			EnableEtlOnAllDependentTables(tabularModels);
		}

		public void UpdateEtlOnSelectedModels(IEnumerable<ZString> tabularModelFileNames, bool enable)
		{
			if (enable)
			{
				ExecuteQuery(enableEtlOnModelQuery, tabularModelFileNames);
			}
			else
			{
				ExecuteQuery(disableEtlOnModelQuery, tabularModelFileNames);
			}
		}

		public void EnsureEnableEtlTrueOnStagingTableStateOnlyForEnabledTabularModels()
		{
			DisableEtlForAllStagingTableStatesQuery();
			var enabledTabularModels = GetAllEnabledTabularModels();
			if (enabledTabularModels.Any())
			{
				EnableEtlOnAllDependentTables(enabledTabularModels);
			}
		}

		void EnableEtlOnAllDependentTables(IEnumerable<ZString> tabularModels)
		{
			var linkedTableConfig = ConfigData.LinkedTable;
			var dependentTables = GetDependentTablesFor(tabularModels, linkedTableConfig);
			ExecuteEnableEtlForDependentTableStateQuery(dependentTables);
		}

		public BiConfigurationData ConfigData
		{
			get
			{
				return configurationData ?? (configurationData = BiAutomationConfigLoader.Instance.ConfigData);
			}
		}
		BiConfigurationData configurationData;

		public IEnumerable<ZString> GetDependentTablesFor(IEnumerable<ZString> tabularModelNames, LinkedTableDataTable linkedTableConfig)
		{
			var tabularModelsQuery = string.Join<string>("','", tabularModelNames.Select(x => x.ToString().Replace(" ", string.Empty)));
			var linkedTableNames = linkedTableConfig.Select($"TabularModelLogicName IN ('{tabularModelsQuery}')").AsEnumerable().Select(row => new ZString($"{row["Schema"]}.{row["Name"]}")).Distinct();
			if (linkedTableNames.Any())
			{
				return linkedTableNames;
			}
			else
			{
				throw new InvalidOperationException($"The Linked Table config contains no dependent tables for any of the following tabular models:\r\n '{tabularModelsQuery}'");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "SQL query")]
		IEnumerable<ZString> GetAllEnabledTabularModels()
		{
			var query = "SELECT SsasModelLogicalName FROM biadmin.SsasCube WHERE EnableEtl = 1";
			var queryResult = DataUtils.GetListOfValuesFromQuery(BiConnection, query);
			var enabledTabularModels = queryResult.Select(logicalName => new ZString(logicalName)).ToList();
			return enabledTabularModels;
		}

		public void EnableEtlForAllStagingTableStatesQuery()
		{
			var query = $@"UPDATE biadmin.StagingTableState SET EnableEtl = 1";
			BiConnection.ExecuteNonQuery(query);
		}

		public void DisableEtlForAllStagingTableStatesQuery()
		{
			var query = $@"UPDATE biadmin.StagingTableState SET EnableEtl = 0";
			BiConnection.ExecuteNonQuery(query);
		}

		public void DisableEtlForAllTabularModelsQuery()
		{
			var query = $@"UPDATE biadmin.SsasCube SET EnableEtl = 0";
			BiConnection.ExecuteNonQuery(query);
		}

		public void EnableEtlForAllTabularModels()
		{
			var query = $@"UPDATE biadmin.SsasCube SET EnableEtl = 1";
			BiConnection.ExecuteNonQuery(query);
		}

		public int GetCountOfEnabledTables()
		{
			var query = $@"SELECT SUM(CAST(EnableEtl AS INT)) FROM biadmin.StagingTableState";
			return BiConnection.ExecuteScalar<int>(query);
		}

		public bool IsStagingTableEtlEnabled(string stagingTableName)
		{
			var query = $@"SELECT EnableEtl FROM biadmin.StagingTableState WHERE SourceTableName = '{stagingTableName}'";
			return BiConnection.ExecuteScalar<bool>(query);
		}

		void ExecuteEnableEtlForDependentTableStateQuery(IEnumerable<ZString> dependentTables)
		{
			var dependentTablesSql = string.Join("','", dependentTables);
			var query = $@"
				UPDATE biadmin.StagingTableState
					SET EnableEtl = 1
					WHERE SourceTableName IN ('{dependentTablesSql}')
				";
			BiConnection.ExecuteNonQuery(query);
		}

		public void cancelRedeployModels(IEnumerable<ZString> tabularModels)
		{
			ExecuteQuery(cancelRedeployModelQuery, tabularModels);
		}
		#endregion // Tabular Model Menu Options

		#region Query

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL Query constructor")]
		const string activateModelQuery =
@"UPDATE [{0}].SsasCube
	SET DeployToServer = 1
{1}";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL Query constructor")]
		const string deactivateModelQuery =
@"UPDATE [{0}].SsasCube
	SET DeployToServer = 0
{1}";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL Query constructor")]
		const string reprocessModelQuery =
@"UPDATE [{0}].SsasCube
	SET IsCubeProcessing = 0,
			LastSourceLsnDateTimeUTC = NULL,
			LastProcessingStartDateTimeUTC = NULL,
			LastProcessingFinishDateTimeUTC = NULL
{1}";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL Query constructor")]
		const string redeployModelQuery =
@"UPDATE [{0}].SsasCube
	SET RedeployOnNextBID = 1
{1}";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL Query constructor")]
		const string disableEtlOnModelQuery =
@"UPDATE [{0}].SsasCube
	SET EnableEtl = 0
{1}";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL Query constructor")]
		const string enableEtlOnModelQuery =
@"UPDATE [{0}].SsasCube
	SET EnableEtl = 1
{1}";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL Query constructor")]
		const string cancelRedeployModelQuery =
@"UPDATE [{0}].SsasCube
	SET RedeployOnNextBID = 0
{1}";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL Query constructor")]
		void ExecuteQuery(string query, IEnumerable<ZString> tabularModels)
		{
			if (BiConnection != null && tabularModels.Any())
			{
				var whereCondition = tabularModels.Any() ? string.Format(CultureInfo.InvariantCulture, @"WHERE SsasModelFileName IN ('{0}')", string.Join("', '", tabularModels)) : "";
				var sqlText = string.Format(CultureInfo.InvariantCulture, query, BiConstants.BiAdminSchemaName, whereCondition);
				BiConnection.ExecuteNonQuery(sqlText);
			}
		}

		#endregion // Query
	}
}
