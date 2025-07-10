using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using CargoWise.Bi.Common;
using CargoWise.Bi.Registration.Common;
using CargoWise.Bi.Registration.PowerBi;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Integration;
using Newtonsoft.Json.Linq;

namespace CargoWise.Bi.Deployment.ReportingServices
{
	#region SuppressResourceStringsCheckRegion

	public class AnalyticsReportDeployer : PowerBiDeployer
	{
		public AnalyticsReportDeployer(ILogger logger) : base(logger)
		{ }

		protected override string BaseFolder => "Analytics";
		protected override BiReportCategory ReportCategory => BiReportCategory.Analytics;

		protected override void SetDataSources(IEnumerable<PowerBiItem> items)
		{
			foreach (var report in items)
			{
				if (report.ReportType == BiReportType.NonPaginated)
				{
					var path = string.Join(System.IO.Path.AltDirectorySeparatorChar.ToString(), ClientFolderPath, report.BusinessArea);
					SetDataSource(report, path);
				}
			}
		}

		public void SetDataSource(PowerBiItem item, string parentFolder)
		{
			var reportPath = string.Join(System.IO.Path.AltDirectorySeparatorChar.ToString(), parentFolder, item.Name);
			var itemType = item.IsDataset ? "Datasets" : "PowerBIReports";
			var dataSourceObj = GetPowerBiItem<JObject>(string.Format(CultureInfo.InvariantCulture, @"{0}(path='{1}')/DataSources", itemType, reportPath), "GET")["value"];
			var biReportUser = new BiReportUser();
			if (dataSourceObj != null)
			{
				var dataSourceToken = dataSourceObj.FirstOrDefault();
				if (dataSourceToken != null)
				{
					dataSourceToken["ModifiedDate"] = ZDateTime.Now.ToString("o", CultureInfo.InvariantCulture);
					dataSourceToken["Name"] = item.DataSourceModel;
					dataSourceToken["ConnectionString"] = GetAnalysisServerDataSourceConnectionString(item.DataSourceModel);
					var credentialsInServer = new JObject
					{
						{ "UserName", biReportUser.DomainAndUsername },
						{ "Password", biReportUser.Password },
						{ "UseAsWindowsCredentials", true },
						{ "ImpersonateAuthenticatedUser", false },
					};

					dataSourceToken["CredentialsInServer"] = credentialsInServer;

					if (item.IsDataset)
					{
						dataSourceToken["IsConnectionStringOverridden"] = true;
						dataSourceToken["CredentialRetrieval"] = "store";
						dataSourceToken["DataSourceType"] = "OLEDB-MD";
						dataSourceToken["Path"] = string.Join(System.IO.Path.AltDirectorySeparatorChar.ToString(), parentFolder, item.DataSourceModel);
					}
					else
					{
						var dataModelDataSource = dataSourceToken["DataModelDataSource"];
						dataModelDataSource["AuthType"] = "Windows";
						dataModelDataSource["Username"] = biReportUser.DomainAndUsername;
						dataModelDataSource["Secret"] = biReportUser.Password;
						dataSourceToken["DataModelDataSource"] = dataModelDataSource;
					}

					var dataSourceJsonContent = "[\r\n" + dataSourceToken.ToString() + "\r\n]";

					try
					{
						SendPowerBiRequest(string.Format(CultureInfo.InvariantCulture, @"{0}(path='{1}')/DataSources", itemType, reportPath), "PATCH", dataSourceJsonContent);
					}
					catch (PowerBiException ex)
					{
						throw new PowerBiException(string.Format(CultureInfo.InvariantCulture, "Failed to set data source for {0}.", item.Name), ex);
					}
				}
			}
			else
			{
				Log(LogType.Warning, string.Format(CultureInfo.InvariantCulture, "Failed to get data source for {0}. Check that the report exists on the server.", item.Name));
			}
		}

		protected override void SetDatasetDataSource(PowerBiItem item, string parentFolder)
		{
			var reportPath = string.Join(System.IO.Path.AltDirectorySeparatorChar.ToString(), parentFolder, item.Name);
			var dataSetToken = new JObject
			{
				{ "Name", "DataSetDataSource" },
				{ "Path", string.Join(System.IO.Path.AltDirectorySeparatorChar.ToString(), parentFolder, item.DataSourceModel) },
				{ "IsReference", true },
				{ "@odata.type", "#Model.DataSource" }
			};
			var payload = "[\r\n" + dataSetToken.ToString() + "\r\n]";
			try
			{
				SendPowerBiRequest(string.Format(CultureInfo.InvariantCulture, @"{0}(path='{1}')/DataSources", "Datasets", reportPath), "PUT", payload);
			}
			catch (PowerBiException ex)
			{
				throw new PowerBiException(string.Format(CultureInfo.InvariantCulture, "Failed to set data set for {0}.", item.Name), ex);
			}
		}

		string DataSourcePayload(string model, string parentFolder)
		{
			var biReportUser = new BiReportUser();
			var credentialsInServer = new JObject
					{
						{ "UserName", biReportUser.DomainAndUsername },
						{ "Password", biReportUser.Password },
						{ "UseAsWindowsCredentials", true },
						{ "ImpersonateAuthenticatedUser", false },
					};

			var dataSourceJsonContent = new JObject
					{
						{ "Name", model },
						{ "Description", string.Empty },
						{ "Hidden", false },
						{ "Path",  string.Join(System.IO.Path.AltDirectorySeparatorChar.ToString(), parentFolder, model) },
						{ "IsEnabled", true },
						{ "IsReference", false },
						{ "DataSourceType", "OLEDB-MD" },
						{ "ConnectionString", GetAnalysisServerDataSourceConnectionString(model) },
						{ "IsConnectionStringOverridden", true },
						{ "CredentialRetrieval", "store" },
						{ "CredentialsInServer", credentialsInServer },
						{ "CredentialsByUser", null }
					};
			return dataSourceJsonContent.ToString();
		}

		protected override void CreateDatasetDataSource(string model, string parentFolder)
		{
			var reportPath = string.Join(System.IO.Path.AltDirectorySeparatorChar.ToString(), parentFolder, model);
			try
			{
				SendPowerBiRequest(string.Format(CultureInfo.InvariantCulture, @"{0}", "DataSources", reportPath), "POST", DataSourcePayload(model, parentFolder));
			}
			catch (PowerBiException ex)
			{
				throw new PowerBiException(string.Format(CultureInfo.InvariantCulture, "Failed to create data set.", model), ex);
			}
		}

		protected override bool IsDataSourceConfigured(JObject dataSourceObj)
		{
			var type = dataSourceObj["value"].FirstOrDefault()?["DataModelDataSource"]?["Type"]?.ToString();
			var kind = dataSourceObj["value"].FirstOrDefault()?["DataModelDataSource"]?["Kind"]?.ToString();
			var connectionString = dataSourceObj["value"].FirstOrDefault()?["ConnectionString"]?.ToString();
			var authType = dataSourceObj["value"].FirstOrDefault()?["DataModelDataSource"]?["AuthType"]?.ToString();
			var userName = dataSourceObj["value"].FirstOrDefault()?["DataModelDataSource"]?["Username"]?.ToString();

			var result = false;
			var biReportUser = new BiReportUser();

			if (string.Equals(kind, "AnalysisServices", StringComparison.OrdinalIgnoreCase) && // This is a datasource connection kind
				string.Equals(type, "Live", StringComparison.OrdinalIgnoreCase) && // This is a datasource authentication type
				string.Equals(authType, "Windows") &&
				string.Equals(userName, biReportUser.DomainAndUsername)) // This is a datasource connection type
			{
				result = IsAnalysisServerConnectionStringConfigured(connectionString);
			}

			return result;
		}

		bool IsAnalysisServerConnectionStringConfigured(string connectionString)
		{
			var result = false;
			if (!string.IsNullOrEmpty(connectionString))
			{
				var regex = new Regex(@"Initial\s+Catalog\s*=\s*(.+?)_(?<modelLogicalName>.+?)\s*(;|$)", RegexOptions.IgnoreCase);
				var modelLogicalName = regex.Match(connectionString).Groups["modelLogicalName"].ToString();

				result = connectionString == GetAnalysisServerDataSourceConnectionString(modelLogicalName);
			}
			return result;
		}

		string GetAnalysisServerDataSourceConnectionString(string modelLogicalName)
		{
			return string.Format(CultureInfo.InvariantCulture,
				@"Data Source={0};Initial Catalog={1}_{2};Cube=Model;",
				AnalysisServerDataSource,
				Db.DatabaseName,
				modelLogicalName);
		}

		string AnalysisServerDataSource
		{
			get
			{
				return analysisServerDataSource ?? (analysisServerDataSource = GetAnalysisServerForDataSource(AnalysisServer));
			}
		}
		string analysisServerDataSource;

		protected string AnalysisServer
		{
			get
			{
				return analysisServer ?? (analysisServer = BiServers.LoadAnalysisServerUsingCacheIfPossible(Db.Connection));
			}
		}
		string analysisServer;

		protected string GetAnalysisServerForDataSource(string serverName)
		{
			if (!string.IsNullOrEmpty(serverName))
			{
				return AnalysisServerIsInSameBoxAsReportServer(serverName) ?
					serverName.Replace(GetAnalysisServerDomain(serverName), ".") :
					serverName;
			}
			else
			{
				return null;
			}
		}

		bool AnalysisServerIsInSameBoxAsReportServer(string serverName)
		{
			var analysisServerAddressList = Dns.GetHostEntry(GetAnalysisServerDomain(serverName)).AddressList.Select(a => a.ToString());
			var reportingServerAddressList = Dns.GetHostEntry(GetReportServerDomain()).AddressList.Select(a => a.ToString());

			return analysisServerAddressList.Intersect(reportingServerAddressList).Any();
		}

		string GetAnalysisServerDomain(string serverName)
		{
			string domainName;

			if (serverName.Contains(DirectorySeparator))
			{
				domainName = serverName.Substring(0, AnalysisServer.IndexOf(DirectorySeparator, StringComparison.OrdinalIgnoreCase));
			}
			else
			{
				domainName = serverName;
			}

			return domainName;
		}

		string GetReportServerDomain()
		{
			var domainNameRegex = new Regex(@"(http(s*):(\/\/|\\\\))*(?<domainName>.+?)(:\d+)*(((\/|\\).*)|($))", RegexOptions.IgnoreCase);
			var match = domainNameRegex.Match(PowerBiApiUrl);
			string domainName;
			if (match != null)
			{
				domainName = match.Groups["domainName"].ToString();
			}
			else
			{
				domainName = PowerBiApiUrl;
			}

			return domainName;
		}
	}

	#endregion
}
