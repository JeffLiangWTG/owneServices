using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Bi.Configuration;
using CargoWise.Bi.Configuration.DataSets;
using CargoWise.Resource.Shared;
using static CargoWise.Bi.Configuration.DataSets.BiAutomationConfigDataSet;

namespace CargoWise.Bi.ConfigLoader
{
	public class BiConfigurationData
	{
		public BiConfigurationData()
		{
			isCdcConfigurationLoaded = false;
			isEdwConfigurationLoaded = false;
			isSsasConfigurationLoaded = false;
			isXmlDataSourcesConfigurationLoaded = false;
			isTabularModelConfigLoaded = false;
			isReportMappingConfigurationLoaded = false;
		}

		bool isCdcConfigurationLoaded;
		bool isEdwConfigurationLoaded;
		bool isSsasConfigurationLoaded;
		bool isXmlDataSourcesConfigurationLoaded;
		bool isTabularModelConfigLoaded;
		bool isReportMappingConfigurationLoaded;

		BiAutomationConfigDataSet ConfigurationDataSet
		{
			get
			{
				return configurationDataSet ?? (configurationDataSet = new BiAutomationConfigDataSet());
			}
		}
		BiAutomationConfigDataSet configurationDataSet;

		#region Data Tables

		public CdcTableConfigDataTable CdcTableConfig
		{
			get
			{
				LoadCdcConfigurationIfNotLoaded();
				return ConfigurationDataSet.CdcTableConfig;
			}
		}

		public CdcColumnConfigDataTable CdcColumnConfig
		{
			get
			{
				LoadCdcConfigurationIfNotLoaded();
				return ConfigurationDataSet.CdcColumnConfig;
			}
		}

		public EdwTableConfigDataTable EdwTableConfig
		{
			get
			{
				LoadEdwConfigurationIfNotLoaded();
				return ConfigurationDataSet.EdwTableConfig;
			}
		}

		public EdwColumnConfigDataTable EdwColumnConfig
		{
			get
			{
				LoadEdwConfigurationIfNotLoaded();
				return ConfigurationDataSet.EdwColumnConfig;
			}
		}

		public EdwDenormalizedTableConfigDataTable EdwDenormalizedTableConfig
		{
			get
			{
				LoadEdwConfigurationIfNotLoaded();
				return ConfigurationDataSet.EdwDenormalizedTableConfig;
			}
		}

		public EdwDenormalizedColumnConfigDataTable EdwDenormalizedColumnConfig
		{
			get
			{
				LoadEdwConfigurationIfNotLoaded();
				return ConfigurationDataSet.EdwDenormalizedColumnConfig;
			}
		}

		public EdwCustomTableConfigDataTable EdwCustomTableConfig
		{
			get
			{
				LoadEdwConfigurationIfNotLoaded();
				return ConfigurationDataSet.EdwCustomTableConfig;
			}
		}

		public EdwCustomColumnConfigDataTable EdwCustomColumnConfig
		{
			get
			{
				LoadEdwConfigurationIfNotLoaded();
				return ConfigurationDataSet.EdwCustomColumnConfig;
			}
		}

		public EdwModelViewTableConfigDataTable EdwModelViewTableConfig
		{
			get
			{
				LoadEdwConfigurationIfNotLoaded();
				return ConfigurationDataSet.EdwModelViewTableConfig;
			}
		}

		public EdwModelViewColumnConfigDataTable EdwModelViewColumnConfig
		{
			get
			{
				LoadEdwConfigurationIfNotLoaded();
				return ConfigurationDataSet.EdwModelViewColumnConfig;
			}
		}

		public SsasCubesDataTable SsasCubes
		{
			get
			{
				LoadSsasConfigurationIfNotLoaded();
				return ConfigurationDataSet.SsasCubes;
			}
		}

		public SsasTablesDataTable SsasTables
		{
			get
			{
				LoadSsasConfigurationIfNotLoaded();
				return ConfigurationDataSet.SsasTables;
			}
		}

		public XmlDataSourcesDataTable XmlDataSources
		{
			get
			{
				LoadXmlDataSourcesConfigurationIfNotLoaded();
				return ConfigurationDataSet.XmlDataSources;
			}
		}

		public TabularModelDataTable TabularModel
		{
			get
			{
				LoadTabularModelConfigIfNotLoaded();
				return ConfigurationDataSet.TabularModel;
			}
		}

		public LinkedTableDataTable LinkedTable
		{
			get
			{
				LoadTabularModelConfigIfNotLoaded();
				return ConfigurationDataSet.LinkedTable;
			}
		}

		public ReportMappingConfigDataTable ReportMappings
		{
			get
			{
				LoadReportMappingIfNotLoaded();
				return ConfigurationDataSet.ReportMappingConfig;
			}
		}

		#endregion

		#region Loading BI config files

		readonly object mutex = new object();

		void LoadCdcConfigurationIfNotLoaded()
		{
			lock (mutex)
			{
				if (!isCdcConfigurationLoaded)
				{
					var embeddedResources = new[]
					{
						BiConfigurationFileType.CdcTableConfig
					};

					LoadBiConfigurationFromEmbeddedResources(embeddedResources);
					LoadCdcConfigurationFromSharedAssemblies();
					isCdcConfigurationLoaded = true;
				}
			}
		}

		void LoadEdwConfigurationIfNotLoaded()
		{
			lock (mutex)
			{
				LoadCdcConfigurationIfNotLoaded();
				if (!isEdwConfigurationLoaded)
				{
					var embeddedResources = new[]
					{
						BiConfigurationFileType.EdwTableConfig,
						BiConfigurationFileType.EdwDenormalizedTableConfig,
						BiConfigurationFileType.EdwCustomTableConfig,
						BiConfigurationFileType.EdwModelViewTableConfig
					};

					LoadBiConfigurationFromEmbeddedResources(embeddedResources);
					configurationDataSet.SortModelDependencyOrder();
					isEdwConfigurationLoaded = true;
				}
			}
		}

		void LoadSsasConfigurationIfNotLoaded()
		{
			lock (mutex)
			{
				LoadEdwConfigurationIfNotLoaded();
				if (!isSsasConfigurationLoaded)
				{
					var embeddedResources = new[]
					{
						BiConfigurationFileType.SsasCubes
					};

					LoadBiConfigurationFromEmbeddedResources(embeddedResources);
					isSsasConfigurationLoaded = true;
				}
			}
		}

		void LoadXmlDataSourcesConfigurationIfNotLoaded()
		{
			lock (mutex)
			{
				if (!isXmlDataSourcesConfigurationLoaded)
				{
					var embeddedResources = new[]
					{
						BiConfigurationFileType.XmlDataSources
					};

					LoadBiConfigurationFromEmbeddedResources(embeddedResources);
					isXmlDataSourcesConfigurationLoaded = true;
				}
			}
		}

		void LoadTabularModelConfigIfNotLoaded()
		{
			lock (mutex)
			{
				if (!isTabularModelConfigLoaded)
				{
					var embeddedResources = new[]
					{
						BiConfigurationFileType.TabularModel
					};

					LoadBiConfigurationFromEmbeddedResources(embeddedResources);
					isTabularModelConfigLoaded = true;
				}
			}
		}

		void LoadReportMappingIfNotLoaded()
		{
			lock (mutex)
			{
				if (!isReportMappingConfigurationLoaded)
				{
					var embeddedResources = new[]
					{
						BiConfigurationFileType.ReportMapping
					};

					LoadBiConfigurationFromEmbeddedResources(embeddedResources);
					isReportMappingConfigurationLoaded = true;
				}
			}
		}

		enum BiConfigurationFileType
		{ 
			CdcTableConfig = 0,
			EdwTableConfig,
			EdwDenormalizedTableConfig,
			EdwCustomTableConfig,
			EdwModelViewTableConfig,
			SsasCubes,
			TabularModel,
			XmlDataSources,
			ReportMapping,
		}

		protected virtual IEnumerable<CdcRequiredTable> LoadCdcConfigurationFromShared()
		{
			return CdcRequiredTableHelper.Load(Assembly.Load("Enterprise.DbUpgrader.Resource"));
		}

		protected virtual void LoadCdcConfigurationFromSharedAssemblies()
		{
			ConfigurationDataSet.EnforceConstraints = false;

			var tables = LoadCdcConfigurationFromShared();
			var cdcTableConfig = ConfigurationDataSet.CdcTableConfig.ToDictionary(x => TableKey(x.SourceSchema, x.SourceTable), StringComparer.OrdinalIgnoreCase);
			var cdcColumnConfig = ConfigurationDataSet.CdcColumnConfig
				.GroupBy(x => x.CdcTableConfigRow)
				.ToDictionary(
					x => x.Key,
					x => x.ToDictionary(column => column.SourceColumn, StringComparer.OrdinalIgnoreCase));
			foreach (var table in tables)
			{
				if (!cdcTableConfig.TryGetValue(TableKey(table.SchemaName, table.TableName), out var cdcTable))
				{
					throw new BiConfigurationException($"Failed to load [CDC] configuration from shared assemblies, couldn't find {table.SchemaName}.{table.TableName} in config.");
				}

				cdcTable.TableInAudit = true;

				if (!cdcColumnConfig.TryGetValue(cdcTable, out var cdcColumns))
				{
					cdcColumns = new Dictionary<string, CdcColumnConfigRow>();
				}

				foreach (var column in table.ColumnNames)
				{
					if (!cdcColumns.TryGetValue(column, out var cdcColumn))
					{
						throw new BiConfigurationException($"Failed to load [CDC] configuration from shared assemblies, couldn't find {table.SchemaName}.{table.TableName}.{column} in config.");
					}
					cdcColumn.CdcEnabled = true;
					cdcColumn.ColumnInAudit = true;
				}
			}

			ConfigurationDataSet.EnforceConstraints = true;

			string TableKey(string schemaName, string tableName) => $"{schemaName}.{tableName}";
		}

		void LoadBiConfigurationFromEmbeddedResources(BiConfigurationFileType[] biConfigFileTypes)
		{
			ConfigurationDataSet.EnforceConstraints = false;

			foreach (var biConfigFileType in biConfigFileTypes)
			{
				try
				{
					var biFileContents = GetBiFileContents(biConfigFileType);
					byte[] byteArray = Encoding.UTF8.GetBytes(biFileContents);
					using (Stream scriptContentsStream = new MemoryStream(byteArray))
					{
						using (var tempDataSet = new DataSet()) // Temporary storage
						{
							tempDataSet.EnforceConstraints = false;
							tempDataSet.Locale = CultureInfo.InvariantCulture;
							tempDataSet.ReadXml(scriptContentsStream, XmlReadMode.ReadSchema);
							tempDataSet.AcceptChanges();
							ConfigurationDataSet.Merge(tempDataSet, true);
						}
					}
				}
				catch (Exception ex)
				{
					throw new BiConfigurationException($"Failed to load [{biConfigFileType}] configuration from embedded files.", ex);
				}
			}

			ConfigurationDataSet.EnforceConstraints = true;
		}

		string GetBiFileContents(BiConfigurationFileType biConfigFileType)
		{
			var thisType = GetType();
			var fileContents = new StringBuilder();
			var regex = new Regex($"{thisType.Namespace}\\.Config\\.{biConfigFileType}\\.(.*)xml", RegexOptions.IgnoreCase);
			foreach (var embeddedResourceFile in thisType.Assembly.GetManifestResourceNames().Where(r => regex.Match(r).Success).OrderBy(f => f))
			{
				var resourceStream = thisType.Assembly.GetManifestResourceStream(embeddedResourceFile);
				if (resourceStream != null)
				{
					using (var reader = new StreamReader(resourceStream))
					{
						fileContents.AppendLine(reader.ReadToEnd());
					}
				}
			}
			if (biConfigFileType == BiConfigurationFileType.XmlDataSources)
			{
				return fileContents.ToString();
			}
			else
			{
				string configTags = @"<?xml version=""1.0"" encoding=""utf-8""?>
<{0}>
{1}
</{0}>";
				return string.Format(CultureInfo.InvariantCulture, configTags, biConfigFileType.ToString(), fileContents.ToString());
			}
		}

		#endregion

		#region Methods

		public string GetInitialLoadQueryForEdwTable(EdwTableConfigRow edwTable)
		{
			return ConfigurationDataSet.GetInitialLoadQueryForEdwTable(edwTable);
		}

		public string GetIncrementalInsertQueryForEdwTable(EdwTableConfigRow edwTable)
		{
			return ConfigurationDataSet.GetIncrementalInsertQueryForEdwTable(edwTable);
		}

		public string GetIncrementalDeleteQueryForEdwTable(EdwTableConfigRow edwTable)
		{
			return ConfigurationDataSet.GetIncrementalDeleteQueryForEdwTable(edwTable);
		}

		public string GetCustomIndexScriptQueryForEdwTable(EdwTableConfigRow edwTable)
		{
			return ConfigurationDataSet.GetCustomIndexScriptQueryForEdwTable(edwTable);
		}

		public string GetEditableCustomIndexScriptQueryForEdwTable(EdwTableConfigRow edwTable)
		{
			return ConfigurationDataSet.GetEditableCustomIndexScriptQueryForEdwTable(edwTable);
		}

		public string GetCreateDenormalizedTableViewQuery(EdwDenormalizedTableConfigRow denormTable)
		{
			return ConfigurationDataSet.GetCreateDenormalizedTableViewQuery(denormTable);
		}

		public string GetCustomIndexScriptQueryForDenormalizedTable(EdwDenormalizedTableConfigRow edwTable)
		{
			return ConfigurationDataSet.GetCustomIndexScriptQueryForDenormalizedTable(edwTable);
		}

		public string GetEditableCustomIndexScriptQueryForDenormalizedTable(EdwDenormalizedTableConfigRow edwTable)
		{
			return ConfigurationDataSet.GetEditableCustomIndexScriptQueryForDenormalizedTable(edwTable);
		}

		public string GetEditableCustomIndexScriptQueryForCustomTable(EdwCustomTableConfigRow edwTable)
		{
			return ConfigurationDataSet.GetEditableCustomIndexScriptQueryForCustomTable(edwTable);
		}
#if DEBUG

		public void Validate()
		{
			LoadCdcConfigurationIfNotLoaded();
			LoadEdwConfigurationIfNotLoaded();
			LoadSsasConfigurationIfNotLoaded();
			LoadReportMappingIfNotLoaded();
			ConfigurationDataSet.Validate();
		}

		public void SortModelDependencyOrder()
		{
			LoadCdcConfigurationIfNotLoaded();
			LoadEdwConfigurationIfNotLoaded();
			LoadSsasConfigurationIfNotLoaded();
			ConfigurationDataSet.SortModelDependencyOrder();
		}

		public bool EnforceConstraints
		{
			get => ConfigurationDataSet.EnforceConstraints;
			set
			{
				ConfigurationDataSet.EnforceConstraints = value;
			}
		}

		public string GetInitialLoadQueryForDenormalizedTable(EdwDenormalizedTableConfigRow denormTable)
		{
			return ConfigurationDataSet.GetInitialLoadQueryForDenormalizedTable(denormTable);
		}

		public string GetIncrementalLoadQueryForDenormalizedTable(EdwDenormalizedTableConfigRow denormTable)
		{
			return ConfigurationDataSet.GetIncrementalLoadQueryForDenormalizedTable(denormTable);
		}

#endif

		#endregion
	}
}
