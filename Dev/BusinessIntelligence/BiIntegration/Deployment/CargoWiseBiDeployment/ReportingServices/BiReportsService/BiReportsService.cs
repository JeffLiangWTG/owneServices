using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.Bi.Deployment.AnalysisServices;
using CargoWise.Bi.Deployment.ReportingServices.API.OldAuditApi;
using CargoWise.Bi.Registration;
using CargoWise.Bi.Registration.Common;
using CargoWise.Bi.Registration.PowerBi;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Billing.Business;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Newtonsoft.Json;

namespace CargoWise.Bi.Deployment.ReportingServices
{
	public class BiReportsService : IBiReportsService
	{
		DeploymentFileLoader FileLoader
		{
			get
			{
				return fileLoader = fileLoader ?? new DeploymentFileLoader();
			}
		}
		DeploymentFileLoader fileLoader;

		virtual public Uri PowerBiWebPortalUrl
		{
			get
			{
				if (powerBiWebPortalUrl == null)
				{
					powerBiWebPortalUrl = new Uri(SystemDataRegistry.Instance.BiPowerBiWebPortalUrl.Value);
				}
				return powerBiWebPortalUrl;
			}
		}
		Uri powerBiWebPortalUrl;

		virtual public Uri GlowServicelUrl
		{
			get
			{
				if (glowServiceUrl == null)
				{
					glowServiceUrl = new Uri(GlowRegistry.Instance.GlowServiceUri);
				}
				return glowServiceUrl;
			}
		}
		Uri glowServiceUrl;

		public string PowerBiPortalName
		{
			get
			{
				if (powerBiPortalName == null)
				{
					if (!string.IsNullOrEmpty(PowerBiWebPortalUrl.ToString()))
					{
						powerBiPortalName = PowerBiWebPortalUrl.Segments.LastOrDefault();
					}
				}
				return powerBiPortalName;
			}
		}
		string powerBiPortalName;

		public string PowerBiServerName
		{
			get
			{
				if (powerBiServerName == null)
				{
					if (!string.IsNullOrEmpty(PowerBiWebPortalUrl.ToString()))
					{
						powerBiServerName = PowerBiWebPortalUrl.ToString().Replace(PowerBiPortalName, string.Empty);
					}
				}
				return powerBiServerName;
			}
		}
		string powerBiServerName;

		public string DataWarehouseServer
		{
			get
			{
				if (dataWarehouseServer == null)
				{
					using (Db.DisposableActionForDbConnection())
					{
						dataWarehouseServer = BiServers.LoadDataWarehouseServerUsingCacheIfPossible(Db.Connection);
					}
				}
				return dataWarehouseServer;
			}
		}
		string dataWarehouseServer;

		IProductRegistration Registration
		{
			get
			{
				return registration ?? (registration = ObjectFactory.Get<IProductRegistration>());
			}
		}
		IProductRegistration registration;

		public PowerBiItem GetPowerBiItemByName(string name)
		{
			return FileLoader.GetPowerBiReport(name);
		}

		public PowerBiReport[] GetPowerBiReportsList(IPowerBiReportLinkBuilderFactory reportLinkBuilderFactory, BiReportCategory reportCategory)
		{
			var reports = GetPowerBiReportsList(reportLinkBuilderFactory, reportCategory, Env.Security.AnalyticsReports);
			return reports.ToArray();
		}

		public List<PowerBiReport> GetPowerBiReportsList(IPowerBiReportLinkBuilderFactory reportLinkBuilderFactory,
			BiReportCategory reportCategory,
			SecurityCheckpoint businessAreaCheckPoints)
		{
			var powerBiReportsList = new List<PowerBiReport>();
			var reportLinkBuilder = reportLinkBuilderFactory.NewLinkBuilder(Registration, PowerBiWebPortalUrl);

			foreach (var report in new DeploymentFileLoader().GetPowerBiReports(reportCategory))
			{
				var path = reportLinkBuilder.GetReportPath(report);
				var checkPoint = businessAreaCheckPoints.ChildCheckPoints.SingleOrDefault(c => string.Equals(c.HumanReadableName, report.BusinessArea, StringComparison.OrdinalIgnoreCase));

				if (checkPoint != null && (checkPoint.ChildCheckPoints.Any(r => r.IsAllowed && string.Equals(r.HumanReadableName, report.Name, StringComparison.OrdinalIgnoreCase))))
				{
					powerBiReportsList.Add(new PowerBiReport() { ReportName = report.Name, ReportPath = path, BusinessArea = report.BusinessArea, UrlEncodedReportName = WebUtility.UrlEncode(report.Name), ResourceType = report.ResourceType });
				}
			}
			return powerBiReportsList;
		}

		public string GetPowerBiBaseReportPath(string reportType)
		{
			var enterpriseCode = Registration.Key.EnterpriseCode;
			var serverCode = Registration.Key.ServerCode;
			return $"cw1api/analytics/loadReport/{PowerBiPortalName}/{reportType}/{enterpriseCode}/{serverCode}/Analytics";  // url
		}

		public string GetQueryString(string path)
		{
			var queryparams = path.Split(new[] { '?' });
			return queryparams.Length > 1 ? "?" + queryparams[1] : string.Empty;
		}

		public HttpRequestMessage CloneRequest(HttpRequestMessage req, Uri newUri)
		{
			var clone = new HttpRequestMessage(req.Method, newUri);

			if (req.Method != HttpMethod.Get)
			{
				clone.Content = req.Content;
			}
			clone.Version = req.Version;

#if NETFRAMEWORK
			foreach (KeyValuePair<string, object> prop in req.Properties)
			{
				clone.Properties.Add(prop);
			}
#endif
			// HttpRequestMessage.Properties has been deprecated in .net8.0. Therefore we need to use Options instead.
			// We cannot enumerate all the options like we could with Properties above. Options needs to be added by keys
			// We need to revisit this if we need to add custom options.

			foreach (KeyValuePair<string, IEnumerable<string>> header in req.Headers)
			{
				clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
			}

			const string rsClientSessionIdString = "X-SSRS-ClientSessionId"; // A header key string
			const string userAgentString = "User-Agent"; // A header key string
			const string rsClientSessionIdValueString = "1"; // A header value string
			const string defaultUserAgentValueString = "Mozilla/5.0"; // A header value string

			clone.Headers.TryAddWithoutValidation(rsClientSessionIdString, rsClientSessionIdValueString);
			clone.Headers.TryAddWithoutValidation(userAgentString, defaultUserAgentValueString);
			clone.Headers.Host = newUri.Authority;
			return clone;
		}

		public SecurityCheckpoint GetCheckPointForReportItem(PowerBiItem reportItem)
		{
			SecurityCheckpoint checkPoint = null;
			var analyticsCheckPoint = Env.Security.AnalyticsReports;
			var businessAreaCheckpoint = analyticsCheckPoint.ChildCheckPoints.SingleOrDefault(c => string.Equals(c.HumanReadableName, reportItem.BusinessArea, StringComparison.OrdinalIgnoreCase));

			if (businessAreaCheckpoint != null)
			{
				checkPoint = businessAreaCheckpoint.ChildCheckPoints.SingleOrDefault(c => string.Equals(c.HumanReadableName, reportItem.Name, StringComparison.OrdinalIgnoreCase));
			}

			return checkPoint;
		}

		const string JavascriptMediaType = "javascript";
		const string htmlMediaType = "text/html";
		public async Task<HttpResponseMessage> ModifyProxyResponse(PowerBiItem reportItem, string fileName, HttpResponseMessage serverResponse, string countryCode, string companyCode = "", string branchCode = "")
		{
			if (serverResponse.Content == null)
			{
				return GetHttpResponseMessage(HttpStatusCode.NotFound, Res.GetString("1571C1D5-D0DA-41DE-B738-FB5C5C387FC5", "No content found"), "text/html");
			}

			if (serverResponse.Content.Headers?.ContentType == null)
			{
				return serverResponse;
			}

			try
			{
				var contentMediaType = serverResponse.Content.Headers.ContentType.MediaType;
				var modifiedContent = string.Empty;

				if (serverResponse.Content.Headers.ContentType.MediaType == htmlMediaType)
				{
					var htmlProcessor = new PowerBiHtmlProcessor(reportItem, PowerBiPortalName, companyCode, countryCode, branchCode);
					using (var byteStream = await serverResponse.Content.ReadAsStreamAsync())
					{
						using (var reader = new StreamReader(byteStream))
						{
							modifiedContent = htmlProcessor.ProcessHtml(fileName, reader.ReadToEnd());
							serverResponse.Content = new StringContent(modifiedContent);
						}
					}
				}
				else if (serverResponse.Content.Headers.ContentType.MediaType.Contains(JavascriptMediaType))
				{
					var javascriptProcessor = new PowerBiJavascriptProcessor(PowerBiPortalName);

					using (var byteStream = await serverResponse.Content.ReadAsStreamAsync())
					{
						using (var reader = new StreamReader(byteStream))
						{
							modifiedContent = javascriptProcessor.ProcessJavascript(fileName, reader.ReadToEnd());
							serverResponse.Content = new StringContent(modifiedContent);
						}
					}
				}
				else if (serverResponse.Content.Headers.ContentType.MediaType.Contains("application/json"))
				{
					var jsonProcessor = new PowerBiJsonProcessor(PowerBiPortalName);

					using (var byteStream = await serverResponse.Content.ReadAsStreamAsync())
					{
						using (var reader = new StreamReader(byteStream))
						{
							modifiedContent = jsonProcessor.ProcessJSon(fileName, reader.ReadToEnd());
							serverResponse.Content = new StringContent(modifiedContent);
						}
					}
				}
				serverResponse.Content.Headers.ContentType.MediaType = contentMediaType;
				return serverResponse;
			}
			catch (Exception ex)
			{
				// Log all exceptions at the moment
				var devMessage = $"Module:Report Service, Exception: {ex.Message}, File Name: {fileName}"; // exception message
				ErrorReporter.ReportOnce(devMessage, ex);
				throw;
			}
		}

		public async Task<HttpResponseMessage> ShowUnauthorisedUserMessage()
		{
			var unAthorisedMessage = new HttpResponseMessage();
			unAthorisedMessage.Content = new StringContent(Res.GetString("54132a12-3d0a-4cdc-a667-1ff947446f27", "{0} Report User Credentials are incorrect or does not have enough permission to access the report server.{1}", "<html><body>", "</body></html>"));
			unAthorisedMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
			using (var byteStream = await unAthorisedMessage.Content.ReadAsStreamAsync())
			{ }
			return unAthorisedMessage;
		}

		static HttpResponseMessage GetHttpResponseMessage(HttpStatusCode httpStatusCode, string content, string contentType)
		{
			var httpResponseMessage = new HttpResponseMessage
			{
				StatusCode = httpStatusCode,
				Content = new StringContent(content)
			};
			httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
			return httpResponseMessage;
		}

		public string GetCompanyLogoFromRegistry()
		{
			string imageStr = string.Empty;

			var companyPk = GetHomeCompanyPkFromCurrentUser();
			if (companyPk != Guid.Empty)
			{
				using (var image = SystemDataRegistry.Instance.BiReportCompanyLogo.GetValueWithoutFallback(companyPk, Guid.Empty, Guid.Empty))
				{
					imageStr = ConvertImageToBase64String(image);
				}
			}

			return imageStr;
		}

		Guid GetHomeCompanyPkFromCurrentUser()
		{
			var homeCompany = (EnvProxy.Instance.CurrentUser as Enterprise.MasterFiles.Business.GlbStaff)?.HomeBranch?.Company;
			if (homeCompany != null)
			{
				return homeCompany.PK.ToGuid();
			}
			else
			{
				return Guid.Empty;
			}
		}

		string ConvertImageToBase64String(Image image)
		{
			if (image != null)
			{
				var imageBytes = new ImageRegistryDataType().Serialise(image);
				return Convert.ToBase64String(imageBytes);
			}
			else
			{
				return string.Empty;
			}
		}

		public string GetReportConfigurations(string companyCode, string reportName = "Shipment Profile Report", string visualName = "VisualC") // default parameters
		{
			var configurations = GetReportConfigurationsFromEdw(companyCode, reportName, visualName);
			return $"[{string.Join(",", configurations)}]"; // String formatting
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		IEnumerable<string> GetReportConfigurationsFromEdw(string companyCode, string reportName, string visualName)
		{
			if (!string.IsNullOrEmpty(DataWarehouseServer))
			{
				using (var connection = Db.NewExtraConnectionWithMainDbCredentials(DataWarehouseServer, Db.SqlMasterDb))
				{
					if (connection.DatabaseExists(Db.EdwDatabaseName))
					{
						using (((ICurrentDbControl)connection).UseDatabase(Db.EdwDatabaseName))
						{
							var sqlText = $"select ColumnConfiguration from [{BiConstants.BiAdminSchemaName}].[ReportConfiguration] where CompanyCode = @CompanyCode and ReportName = @ReportName and VisualName = @VisualName";  // SQL Query
							using (var command = connection.Command(sqlText))
							{
								command.AddParameter("@CompanyCode", SqlDbType.VarChar, 3, companyCode);
								command.AddParameter("@ReportName", SqlDbType.VarChar, 100, reportName);
								command.AddParameter("@VisualName", SqlDbType.VarChar, 100, visualName);

								return DataUtils.GetListOfValuesFromCommand(command);
							}
						}
					}
				}
			}
			return new List<string>();
		}

		public DataTable RetrieveCdcHistorySummaryDetails(ZDateTime utcFromDate, ZDateTime utcToDate, int batchSize)
		{
			return OldAuditAPI.Instance.GetCdcHistorySummaryDetails(utcFromDate, utcToDate, batchSize);
		}

		public DataTable RetrieveCdcHistorySummaryDetailsAfterLsn(string lsn, int batchSize)
		{
			return OldAuditAPI.Instance.GetCdcHistorySummaryDetailsAfterLsn(lsn, batchSize);
		}

		public void DeleteReportConfigurations(string reportConfigurationID) // default parameters
		{
			DeleteReportConfigurationsFromEdw(reportConfigurationID);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void DeleteReportConfigurationsFromEdw(string reportConfigurationID)
		{
			using (var connection = GetDataWarehouseConnection())
			{
				var sqlText = $"delete from [{BiConstants.BiAdminSchemaName}].[ReportConfiguration] where ReportConfigurationID = @ReportConfigurationID";  // SQL Query
				using (var command = connection.Command(sqlText))
				{
					command.AddParameter("@ReportConfigurationID", SqlDbType.VarChar, 128, reportConfigurationID);
					command.ExecuteNonQuery();
				}
			}
		}

		DbConnection GetDataWarehouseConnection()
		{
			DbConnection connection = null;
			if (!string.IsNullOrEmpty(DataWarehouseServer))
			{
				connection = Db.NewExtraConnectionWithMainDbCredentials(DataWarehouseServer, Db.EdwDatabaseName);
			}
			else
			{
				throw new PowerBiException("Data warehouse server could not be found.");
			}
			return connection;
		}

		public void AddReportConfigurations(string reportConfigurationID, string companyCode, string configurationName, string columnConfiguration, bool isDefault, string lastModifiedDateUTC, string reportName = "Shipment Profile Report", string visualName = "VisualC") // default parameters
		{
			AddReportConfigurationsFromEdw(reportConfigurationID, companyCode, configurationName, columnConfiguration, isDefault, lastModifiedDateUTC, reportName, visualName);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1122:DoNotUseDateTimeParse", Justification = "Reading from Web Request")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void AddReportConfigurationsFromEdw(string reportConfigurationID, string companyCode, string configurationName, string columnConfiguration, bool isDefault, string lastModifiedDateUTC, string reportName, string visualName)
		{
			using (var connection = GetDataWarehouseConnection())
			{
				var lastModifiedDate = DateTime.Parse(lastModifiedDateUTC, CultureInfo.InvariantCulture); // Reading from Web Request
				var sqlText = $"insert into [{BiConstants.BiAdminSchemaName}].[ReportConfiguration] (ReportConfigurationID, CompanyCode, ConfigurationName, ColumnConfiguration, IsDefault, LastModifiedDateUTC, ReportName, VisualName) values (@ReportConfigurationID, @CompanyCode, @ConfigurationName, @ColumnConfiguration, @IsDefault, @LastModifiedDateUTC, @ReportName, @VisualName)";  // SQL Query
				using (var command = connection.Command(sqlText))
				{
					command.AddParameter("@ReportConfigurationID", SqlDbType.VarChar, 128, reportConfigurationID);
					command.AddParameter("@ColumnConfiguration", SqlDbType.VarChar, -1, columnConfiguration);
					command.AddParameter("@ConfigurationName", SqlDbType.VarChar, 128, configurationName);
					command.AddParameter("@IsDefault", SqlDbType.Bit, isDefault);
					command.AddParameter("@LastModifiedDateUTC", SqlDbType.DateTime, lastModifiedDate);
					command.AddParameter("@CompanyCode", SqlDbType.VarChar, 128, companyCode);
					command.AddParameter("@ReportName", SqlDbType.VarChar, 128, reportName);
					command.AddParameter("@VisualName", SqlDbType.VarChar, 128, visualName);
					command.ExecuteNonQuery();
				}
			}
		}

		public void UpdateReportConfigurations(string reportConfigurationID, string companyCode, string configurationName, string columnConfiguration, bool isDefault, string lastModifiedDateUTC, string newModifiedDateUTC, string reportName = "Shipment Profile Report", string visualName = "VisualC") // default parameters
		{
			UpdateReportConfigurationsFromEdw(reportConfigurationID, companyCode, configurationName, columnConfiguration, isDefault, lastModifiedDateUTC, newModifiedDateUTC, reportName, visualName);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1122:DoNotUseDateTimeParse", Justification = "Reading from Web Request")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void UpdateReportConfigurationsFromEdw(string reportConfigurationID, string companyCode, string configurationName, string columnConfiguration, bool isDefault, string lastModifiedDateUTC, string newModifiedDateUTC, string reportName, string visualName)
		{
			using (var connection = GetDataWarehouseConnection())
			{
				var lastModifiedDate = DateTime.Parse(lastModifiedDateUTC, CultureInfo.InvariantCulture); // Reading from Web Request
				var newDate = DateTime.Parse(newModifiedDateUTC, CultureInfo.InvariantCulture); // Reading from Web Request
				var modifiedColumnConfiguration = columnConfiguration.Replace(string.Format(CultureInfo.InvariantCulture, "\"LastModifiedDateUTC\":\"{0}\"", lastModifiedDateUTC), string.Format(CultureInfo.InvariantCulture, "\"LastModifiedDateUTC\":\"{0}\"", newModifiedDateUTC));
				var sqlText = string.Format(CultureInfo.InvariantCulture,
					@"update [{0}].[ReportConfiguration]
					set ColumnConfiguration = @ColumnConfiguration, IsDefault = @IsDefault, LastModifiedDateUTC = @NewLastModifiedDateUTC, ConfigurationName = @ConfigurationName
					where ReportConfigurationID = @ReportConfigurationID and LastModifiedDateUTC =  @OldLastModifiedDateUTC", BiConstants.BiAdminSchemaName);   // SQL Query

				using (var command = connection.Command(sqlText))
				{
					command.AddParameter("@ReportConfigurationID", SqlDbType.VarChar, 128, reportConfigurationID);
					command.AddParameter("@ColumnConfiguration", SqlDbType.VarChar, -1, modifiedColumnConfiguration);
					command.AddParameter("@ConfigurationName", SqlDbType.VarChar, 128, configurationName);
					command.AddParameter("@IsDefault", SqlDbType.Bit, isDefault);
					command.AddParameter("@NewLastModifiedDateUTC", SqlDbType.DateTime, newDate);
					command.AddParameter("@OldLastModifiedDateUTC", SqlDbType.DateTime, lastModifiedDate);
					command.AddParameter("@CompanyCode", SqlDbType.VarChar, 128, companyCode);
					command.AddParameter("@ReportName", SqlDbType.VarChar, 128, reportName);
					command.AddParameter("@VisualName", SqlDbType.VarChar, 128, visualName);
					var rows = command.ExecuteNonQuery();
					if (rows == 0)
					{
						throw new NewReportConfigurationConcurrencyException();
					}
				}
			}
		}

		public string GetUserContextInformationFromEnvProxy()
		{
			var userContextInformation = new UserContextInformation();
			var serialisedUserContextInformation = JsonConvert.SerializeObject(userContextInformation);
			return serialisedUserContextInformation;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "DAX Query")]
		public string GetLastModifiedDateFromDB(string modelName)
		{
			string analysisServerName;
			using (Db.DisposableActionForDbConnection())
			{
				analysisServerName = BiServers.LoadAnalysisServerUsingCacheIfPossible(Db.Connection);
			}

			if (!string.IsNullOrEmpty(analysisServerName))
			{
				using (var ssasServer = SsasServer.New(analysisServerName))
				using (new BiReportUser().Impersonate())
				{
					var dataTable = ssasServer.GetDataTableFromQuery(modelName, "EVALUATE 'Cube Info'");
					var lastRefreshUtcDate = dataTable.Rows[0]?["Cube Info[__Last Data Refresh Date Time UTC]"];

					if (lastRefreshUtcDate is DateTime)
					{
						return ((DateTime)lastRefreshUtcDate).ToString("O", CultureInfo.InvariantCulture);
					}
					else
					{
						return null;
					}
				}
			}
			else
			{
				throw new AnalyticsAPIException("Analysis server is not set in the registry.");
			}
		}

		public DataTable GetCdcChanges(string lsn, int batch, string schema, string tableName, int batchSize)
		{
			return OldAuditAPI.Instance.GetCdcChanges(lsn, batch, schema, tableName, batchSize);
		}

		public DataTable GetShipmentProfileReportData<T>(string shipmentNo, string companyCode, string countryCode) where T : AnalyticsModelHelper, new()
		{
			return AnalyticsAPI<T>.Instance.GetShipmentProfileReportData(shipmentNo, companyCode, countryCode);
		}

		public DataTable GetTrialBalanceReportData<T>(string companyCode) where T : AnalyticsModelHelper, new()
		{
			return AnalyticsAPI<T>.Instance.GetTrialBalanceReportData(companyCode);
		}

		public DataTable GetTrialBalanceReportFilteredData<T>(string companyCode) where T : AnalyticsModelHelper, new()
		{
			return AnalyticsAPI<T>.Instance.GetTrialBalanceReportFilteredData(companyCode);
		}

		public void ReportUsage(string endpoint, string parameters, string statusCode, string databaseName, string count, string timestamp)
		{
			var properties = new List<(string name, object value)>
			{
				(UsageProperties.RequestUrl, endpoint),
				(UsageProperties.Parameters, parameters),
				(UsageProperties.StatusCode, statusCode),
				(UsageProperties.DatabaseName, databaseName),
				(UsageProperties.Count, count),
				(UsageProperties.Timestamp, timestamp),
				(UsageProperties.UserLoginName, (EnvProxy.Instance.CurrentUser as Enterprise.MasterFiles.Business.GlbStaff)?.GS_LoginName ?? string.Empty)
			};
			try
			{
				UsageReporter(properties);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var message = $"Could not report usage of Replication API. Error: {ex.Message}";
				ErrorReporter.ReportOnce(message, ex);
			}
		}

		internal Action<List<(string name, object value)>> UsageReporter = properties => UsageCollector.Report(UsageFeatures.Codes.BiReplicationAPI, properties.ToArray());

		public class UserContextInformation
		{
			public readonly string UserFullName;
			public readonly string UserCode;
			public readonly string HomeCompanyName;
			public readonly string HomeCompanyCode;
			public readonly string HomeCompanyPK;
			public readonly string HomeBranchName;
			public readonly string HomeBranchCode;
			public readonly string HomeBranchPK;
			public readonly string CurrentCompanyName;
			public readonly string CurrentCompanyCode;
			public readonly string CurrentCompanyPK;
			public readonly string CurrentBranchName;
			public readonly string CurrentBranchCode;
			public readonly string CurrentBranchPK;

			public UserContextInformation()
			{
				var currentUser = (EnvProxy.Instance.CurrentUser as Enterprise.MasterFiles.Business.GlbStaff);
				if (currentUser != null)
				{
					this.UserFullName = currentUser.GS_FullName;
					this.UserCode = currentUser.GS_Code;

					var homeBranch = currentUser.HomeBranch;
					if (homeBranch != null)
					{
						this.HomeBranchName = homeBranch.GB_BranchName;
						this.HomeBranchCode = homeBranch.GB_Code;
						this.HomeBranchPK = homeBranch.PK.ToString();

						var homeCompany = homeBranch.Company;
						if (homeCompany != null)
						{
							this.HomeCompanyName = homeCompany.CompanyName;
							this.HomeCompanyCode = homeCompany.GC_Code;
							this.HomeCompanyPK = homeCompany.PK.ToString();
						}
					}
				}

				var currentCompany = EnvProxy.Instance.CurrentCompany;
				if (currentCompany != null)
				{
					this.CurrentCompanyName = currentCompany.Name;
					this.CurrentCompanyCode = currentCompany.Code;
					this.CurrentCompanyPK = currentCompany.PK.ToString();
				}

				var currentBranch = EnvProxy.Instance.CurrentBranch;
				if (currentBranch != null)
				{
					this.CurrentBranchName = currentBranch.Name;
					this.CurrentBranchCode = currentBranch.Code;
					this.CurrentBranchPK = currentBranch.PK.ToString();
				}
			}
		}
	}
}
