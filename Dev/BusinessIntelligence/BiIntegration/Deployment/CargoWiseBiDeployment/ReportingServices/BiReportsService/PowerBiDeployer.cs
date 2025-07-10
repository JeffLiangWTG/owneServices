using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Bi.Registration;
using CargoWise.Bi.Registration.Common;
using CargoWise.Bi.Registration.PowerBi;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CargoWise.Bi.Deployment.ReportingServices
{
	#region SuppressResourceStringsCheckRegion

	abstract public class PowerBiDeployer : Deployer
	{
		protected const string DirectorySeparator = @"\";
		readonly char AltDirectorySeparatorChar = Path.AltDirectorySeparatorChar;

		protected PowerBiDeployer(ILogger logger)
		{
			this.logger = logger;
			this.biReportUser = new BiReportUser();
		}
		readonly ILogger logger;
		readonly BiReportUser biReportUser;

		protected void Log(LogType logType, string message)
		{
			logger?.Log(logType, message);
		}

#if DEBUG
		public ILogger Logger
		{
			get
			{
				return logger;
			}
		}
#endif
		#region API

		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		public virtual string PowerBiApiUrl
		{
			get
			{
				if (string.IsNullOrEmpty(powerBiApiUrl))
				{
					powerBiApiUrl = BuildURL(SystemDataRegistry.Instance.BiPowerBiWebPortalUrl.Value, @"/api/v2.0/");
				}
				return powerBiApiUrl;
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		protected string powerBiApiUrl;

#if DEBUG
		public IDisposable SetPowerBiApiUrl(string newValue)
		{
			if (!Globals.IsTest)
			{
				throw new InvalidOperationException("SetPowerBiApiUrl only allowed in Unit Tests");
			}

			string previousValue = powerBiApiUrl;
			powerBiApiUrl = newValue;
			return new DisposableAction(() => powerBiApiUrl = previousValue);
		}
#endif

		#region Power BI Queries

		string Cookie
		{
			get
			{
				if (cookie == null)
				{
					SetCookies();
				}
				return cookie;
			}
		}
		string cookie;

		string Token
		{
			get
			{
				if (token == null)
				{
					SetCookies();
				}
				return token;
			}
		}
		string token;

		void SetCookies()
		{
			var cookies = GetCookie(PowerBiApiUrl);
			if (cookies.IsNullOrEmpty())
			{
				return;
			}
			var nonce = Extract(cookies, "XSRF-NONCE=");
			token = ExtractToken(cookies, "XSRF-TOKEN=");
			cookie = string.Format(CultureInfo.InvariantCulture, "XSRF-NONCE={0}; XSRF-TOKEN={1};", nonce, token);
		}

		protected virtual void SendPowerBiRequest(string resourcePath, string method, string jsonContent = null)
		{
			var url = BuildURL(PowerBiApiUrl, resourcePath);
			using (var requestHandler = new ImpersonatedWebRequestHandler(biReportUser))
			using (var requestMessage = new HttpRequestMessage(new HttpMethod(method.ToUpperInvariant()), new Uri(url)))
			{
				if (!string.IsNullOrEmpty(jsonContent))
				{
					requestMessage.Headers.Add("Cookie", Cookie);
					requestMessage.Headers.Add("X-XSRF-TOKEN", Token);
					requestMessage.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
				}

				SendPowerBiRequest(requestHandler, requestMessage);
			}
		}

		void SendPowerBiRequest(ImpersonatedWebRequestHandler requestHandler, HttpRequestMessage powerBiRequestMessage, Action<HttpResponseMessage> successCallback = null)
		{
			using (var response = requestHandler.SendAsync(powerBiRequestMessage).Result)
			{
				Thread.Sleep(3000);
				if (!response.IsSuccessStatusCode)
				{
					throw new PowerBiException(response.ReasonPhrase);
				}
				successCallback?.Invoke(response);
			}
		}

		protected T GetPowerBiItem<T>(string resourcePath, string method)
		{
			for (int retryCount = 0; retryCount < 5; retryCount++)
			{
				try
				{
					return GetPowerBiItemUnsafe<T>(resourcePath, method);
				}
				catch (PowerBiException ex) when (ex.Message.Equals("Internal Server Error", StringComparison.OrdinalIgnoreCase))
				{
					if (retryCount < 5)
					{
						Thread.Sleep(TimeSpan.FromSeconds(1));
					}
					else
					{
						throw;
					}
				}
			}
			throw new PowerBiException("Internal Server Error");
		}

		T GetPowerBiItemUnsafe<T>(string resourcePath, string method)
		{
			var url = BuildURL(PowerBiApiUrl, resourcePath);
			T powerBiItem = default(T);
			using (var requestHandler = new ImpersonatedWebRequestHandler(biReportUser))
			using (var client = new HttpClient(requestHandler))
			{
				using (HttpRequestMessage requestMessage = new HttpRequestMessage(new HttpMethod(method.ToUpperInvariant()), new Uri(url)))
				{
					requestHandler.UseDefaultCredentials = false;
					SendPowerBiRequest(requestHandler, requestMessage, response =>
					{
						using (var stream = response.Content.ReadAsStreamAsync())
						{
							using (StreamReader sr = new StreamReader(stream.Result))
							{
								var streamValue = sr.ReadToEnd();
								powerBiItem = JsonConvert.DeserializeObject<T>(streamValue);
							}
						}
					});
					return powerBiItem;
				}
			}
		}

		public static string Extract(string cookie, string tag)
		{
			int start = cookie.IndexOf(tag, StringComparison.OrdinalIgnoreCase);
			string value = cookie.Substring(start + tag.Length);
			int end = value.IndexOf(";", StringComparison.OrdinalIgnoreCase);
			return value.Substring(0, end);
		}

		public static string ExtractToken(string cookie, string tag)
		{
			return WebUtility.UrlDecode(Extract(cookie, tag));
		}

		public string GetCookie(string apiString)
		{
			string cookieString = string.Empty;
			var url = apiString + "me";
			using (var requestHandler = new ImpersonatedWebRequestHandler(biReportUser))
			using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(url)))
			{
				SendPowerBiRequest(requestHandler, requestMessage, response =>
				{
					IEnumerable<string> cookies;
					response.Headers.TryGetValues("Set-Cookie", out cookies);
					cookieString = cookies.IsNullOrEmpty() ? string.Empty : string.Join(",", cookies);
				});
				return cookieString;
			}
		}

		#endregion

		#endregion

		#region CatalogItem Functions

		public virtual PowerBiCatalogItem[] ListChildCatalogItems(string path, bool recursive)
		{
			var childItems = new List<PowerBiCatalogItem>();

			var resourcePath = recursive ? "CatalogItems" : string.Format(CultureInfo.InvariantCulture, "Folders(path='{0}')/CatalogItems", path);
			var expectedPath = path == AltDirectorySeparatorChar.ToString() ? path : path + AltDirectorySeparatorChar.ToString();
			foreach (var item in GetPowerBiItem<PowerBiCatalogItems>(resourcePath, "GET")?.value)
			{
				if (item.Path.StartsWith(expectedPath, StringComparison.OrdinalIgnoreCase))
				{
					childItems.Add(item);
				}
			}
			return childItems.ToArray();
		}

		public void DeleteCatalogItem(string path)
		{
			if (GetPowerBiItem<PowerBiCatalogItems>("CatalogItems", "GET").value.Any(i => i.Path == path))
			{
				SendPowerBiRequest(string.Format(CultureInfo.InvariantCulture, "CatalogItems(path='{0}')", path), "DELETE", null);
			}
		}

		#endregion

		#region Folders

		public void CreateClientFolders()
		{
			var currentFolder = new StringBuilder();
			foreach (var folderName in ClientFolderPath.Split(new[] { AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries))
			{
				currentFolder.Append(AltDirectorySeparatorChar + folderName);

				if (!FolderExists(currentFolder.ToString()))
				{
					CreateFolder(folderName, currentFolder.ToString());
				}
			}
		}

		public void CleanClientFolders()
		{
			var clientRootFolderPath = ClientFolderPath;
			if (FolderExists(clientRootFolderPath))
			{
				foreach (var content in ListChildCatalogItems(clientRootFolderPath, false).Where(i => !i.Path.Equals(ClientFolderPath, StringComparison.OrdinalIgnoreCase) && !i.Path.Equals(clientRootFolderPath + @"/SSRS", StringComparison.OrdinalIgnoreCase)))
				{
					DeleteCatalogItem(content.Path);
				}
			}

			if (FolderExists(ClientFolderPath))
			{
				foreach (var content in ListChildCatalogItems(ClientFolderPath, false))
				{
					DeleteCatalogItem(content.Path);
				}
			}

			if (FolderExists(AltDirectorySeparatorChar + Registration.Key.EnterpriseCode) && FolderIsEmpty(AltDirectorySeparatorChar + Registration.Key.EnterpriseCode))
			{
				DeleteCatalogItem(AltDirectorySeparatorChar + Registration.Key.EnterpriseCode);
			}

			RefreshCatalogItems();
		}

		public void RefreshCatalogItems()
		{
			catalogItems = GetPowerBiItem<PowerBiCatalogItems>("Folders", "GET");
		}

		PowerBiCatalogItems CatalogItems
		{
			get
			{
				return catalogItems ?? (catalogItems = GetPowerBiItem<PowerBiCatalogItems>("Folders", "GET"));
			}
		}
		PowerBiCatalogItems catalogItems;

		public bool FolderExists(string path)
		{
			return CatalogItems.value.Any(i => i.Path == path);
		}

		bool FolderIsEmpty(string path)
		{
			return !CatalogItems.value.Any(i => i.Path.StartsWith(path + AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase));
		}

		public void CreateFolder(string folderName, string folderPath)
		{
			try
			{
				DeleteCatalogItem(folderPath);
				CreateFolderUnsafe(folderName, folderPath);
				RefreshCatalogItems();
			}
			catch (PowerBiException e) when (e.Message.Equals("Forbidden", StringComparison.OrdinalIgnoreCase) || e.Message.Equals("Conflict", StringComparison.OrdinalIgnoreCase))
			{
				throw new PowerBiException(string.Format(CultureInfo.InvariantCulture, "Cannot create folder '{0}' because of a conflict. Verify that the folder doesn't exist or that '{1}' has rights, and try again.", folderPath, biReportUser.DomainAndUsername), e);
			}
		}

		void CreateFolderUnsafe(string folderName, string folderPath)
		{
			string jsonContent = string.Format(CultureInfo.InvariantCulture,
							@"{{
				""Id"": ""00000000-0000-0000-0000-000000000000"",
				 ""Name"": ""{0}"",
				 ""Description"": null,
				 ""Path"": ""{1}"",
				 ""Type"": ""Folder"",
				 ""Hidden"": false,
				 ""Size"": 0,
				 ""ModifiedBy"": null,
				 ""ModifiedDate"": ""0001-01-01T00:00:00Z"",
				 ""CreatedBy"": null,
				 ""CreatedDate"": ""0001-01-01T00:00:00Z"",
				 ""ParentFolderId"": null,
				 ""ContentType"": null,
				 ""Content"": null,
				 ""IsFavorite"": false
				}}", folderName, folderPath);

			SendPowerBiRequest("Folders", "POST", jsonContent);
		}

		#endregion

		public void DeployReportFiles()
		{
			CleanClientFolders();
			CreateClientFolders();
			CheckPowerBiHasCorrectServerVersion();
			DeployPowerBiReports();
			if (SystemDataRegistry.Instance.BiReportAPI.Value)
			{
				DeploySharedDatasets();
			}

			RefreshCatalogItems();
		}

		public string ClientFolderPath
		{
			get
			{
				if (clientFolderPath == null)
				{
#if DEBUG
					if (Registration.LocalVerify() == ProductRegistrationVerifyResult.OK)
					{
						clientFolderPath = AltDirectorySeparatorChar + Registration.Key.EnterpriseCode + AltDirectorySeparatorChar + Registration.Key.ServerCode + AltDirectorySeparatorChar + BaseFolder;
					}
					else
					{
						clientFolderPath = AltDirectorySeparatorChar + Registration.Key.EnterpriseCode + AltDirectorySeparatorChar + Registration.Key.ServerCode + @"_" + Db.DatabaseName + AltDirectorySeparatorChar + BaseFolder;
					}
#else
					clientFolderPath = AltDirectorySeparatorChar + Registration.Key.EnterpriseCode + AltDirectorySeparatorChar + Registration.Key.ServerCode + AltDirectorySeparatorChar + BaseFolder;
#endif
				}
				return clientFolderPath;
			}
		}
		string clientFolderPath;

		protected abstract string BaseFolder { get; }

		public string BuildURL(string url, string resourcePath)
		{
			url = url.TrimEnd(new[] { System.IO.Path.AltDirectorySeparatorChar });
			resourcePath = resourcePath.TrimStart(new[] { System.IO.Path.AltDirectorySeparatorChar });
			return url + System.IO.Path.AltDirectorySeparatorChar + resourcePath;
		}

		#region Reports

		public bool CheckPowerBiReportList()
		{
			return PowerBiReportList.Any();
		}

		public void CheckPowerBiHasCorrectServerVersion()
		{
			var majorVersion = Convert.ToInt32(PowerBiServerVersion.Split('.').ElementAtOrDefault(0), CultureInfo.InvariantCulture);
			var minorVersion = Convert.ToInt32(PowerBiServerVersion.Split('.').ElementAtOrDefault(1), CultureInfo.InvariantCulture);

			if (majorVersion < 1 || (majorVersion == 1 && minorVersion < 5))
			{
				CleanClientFolders();
			}
		}

		protected IEnumerable<PowerBiItem> PowerBiReportList
		{
			get
			{
				if (powerBiReportList == null)
				{
					powerBiReportList = new DeploymentFileLoader().GetPowerBiReports(ReportCategory);
				}
				return powerBiReportList;
			}
		}
		IEnumerable<PowerBiItem> powerBiReportList;

		protected IEnumerable<PowerBiItem> PowerBiAPIDatasetList
		{
			get
			{
				if (powerBiAPIDatasetList == null)
				{
					powerBiAPIDatasetList = new DeploymentFileLoader().GetWebAPIDatasets(ReportCategory);
				}
				return powerBiAPIDatasetList;
			}
		}
		IEnumerable<PowerBiItem> powerBiAPIDatasetList;

		protected abstract BiReportCategory ReportCategory { get; }

		void DeployPowerBiReports()
		{
			CreatePowerBiReports();
			Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "The following reports have been deployed:\r\n{0}", string.Join("\r\n", PowerBiReportList.Select(r => $"{r.BusinessArea}/{r.Name}"))));
		}

		public void DeploySharedDatasets()
		{
			var businessAreas = PowerBiAPIDatasetList.Select(r => r.BusinessArea).Distinct();
			Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "Deploying Shared Datasets:", string.Empty));
			foreach (var businessArea in businessAreas)
			{
				var businessAreaPath = string.Join(System.IO.Path.AltDirectorySeparatorChar.ToString(), ClientFolderPath, businessArea);
				if (!FolderExists(businessAreaPath))
				{
					CreateFolder(businessArea, businessAreaPath);
				}
				CreateDatasetDataSource(businessArea + "Model", businessAreaPath);
				var businessAreaDatasetList = PowerBiAPIDatasetList.Where(r => string.Equals(r.BusinessArea, businessArea));
				foreach (var dataset in businessAreaDatasetList)
				{
					Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, $"Deploying {dataset.Name}:", string.Empty));
					CreateDataset(dataset, businessAreaPath);
					SetDatasetDataSource(dataset, businessAreaPath);
				}
			}
			Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "Deploying Shared Datasets complete", string.Empty));
		}

		void CreatePowerBiReports()
		{
			var businessAreas = PowerBiReportList.Select(r => r.BusinessArea).Distinct();

			foreach (var businessArea in businessAreas)
			{
				var businessAreaPath = string.Join(System.IO.Path.AltDirectorySeparatorChar.ToString(), ClientFolderPath, businessArea);
				CreateFolder(businessArea, businessAreaPath);

				var businessAreaReportList = PowerBiReportList.Where(r => string.Equals(r.BusinessArea, businessArea));
				foreach (var report in businessAreaReportList)
				{
					CreateReport(report, businessAreaPath);
				}
				SetDataSources(businessAreaReportList);
			}
		}

		protected abstract void SetDatasetDataSource(PowerBiItem item, string parentFolder);

		protected abstract void SetDataSources(IEnumerable<PowerBiItem> items);

		protected abstract void CreateDatasetDataSource(string model, string parentFolder);

		protected void CreateReport(PowerBiItem report, string parentFolder)
		{
			CreateReport(report.Name, report.ResourceName, report.ReportType, parentFolder);
		}

		public void CreateReport(string reportName, string resourceName, BiReportType reportType, string parentFolder)
		{
			string reportDefinition = Convert.ToBase64String(new DeploymentFileLoader().LoadPowerBiEmbeddedResource(resourceName));
			string resourcePath = reportType == BiReportType.NonPaginated ? "PowerBIReports" : "Reports";
			string jsonContent = string.Format(CultureInfo.InvariantCulture,
@"{{
	""Id"": ""00000000-0000-0000-0000-000000000000"",
   ""Name"": ""{0}"",
   ""Description"": null,
   ""Path"": ""{1}"",
   ""Type"": ""{3}"",
   ""Hidden"": false,
   ""Size"": 0,
   ""ModifiedBy"": null,
   ""ModifiedDate"": ""0001-01-01T00:00:00Z"",
   ""CreatedBy"": null,
   ""CreatedDate"": ""0001-01-01T00:00:00Z"",
   ""ParentFolderId"": null,
   ""ContentType"": null,
   ""Content"": ""{2}"",
   ""IsFavorite"": true,
   ""HasDataSources"": true
	}}
	", reportName, parentFolder + AltDirectorySeparatorChar + reportName, reportDefinition, resourcePath.TrimEnd('s'));
			SendPowerBiRequest(resourcePath, "POST", jsonContent);
		}

		protected void CreateDataset(PowerBiItem dataset, string parentFolder)
		{
			CreateDataset(dataset.Name, dataset.ResourceName, parentFolder);
		}

		public void CreateDataset(string datasetName, string resourceName, string parentFolder)
		{
			var fileContent = new DeploymentFileLoader().LoadPowerBiEmbeddedResource(resourceName);
			string datasetDefinition = Convert.ToBase64String(fileContent);
			string jsonContent = string.Format(CultureInfo.InvariantCulture,
				@"{{
   ""@odata.type"": ""#Model.DataSet"",
   ""Name"": ""{0}"",
   ""Content"": ""{2}"",
   ""ContentType"": """",
   ""Path"": ""{1}""
   	}}
	", datasetName, parentFolder + AltDirectorySeparatorChar + datasetName, datasetDefinition);
			SendPowerBiRequest("catalogitems", "POST", jsonContent);
		}

		#endregion

		#region PowerBi Server

		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "0#")]
		public bool IsPowerBiConfigured(out string errorMessage)
		{
			errorMessage = null;
			try
			{
				ListChildCatalogItems(System.IO.Path.AltDirectorySeparatorChar.ToString(), false);
			}
			catch (PowerBiException ex)
			{
				if (ex.Message.Contains("Forbidden") || ex.Message.Contains("Not Found"))
				{
					errorMessage = $"Catalog items not found in the report server. Make sure that the Power BI Report Server User in the registry is set as Content Manager in the root folder settings.";
				}
				else if (ex.Message.Contains("Unauthorized"))
				{
					Log(LogType.Warning, UnauthorizedExceptionMessage);
				}
				else
				{
					errorMessage = ex.Message;
					throw new PowerBiException(errorMessage);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				string biWebPortalUrl = SystemDataRegistry.Instance.BiPowerBiWebPortalUrl.Value;
				string exceptionMessage = string.Format(CultureInfo.InvariantCulture, "Could not connect to Power BI Report URL.\r\n Power BI Report URL is  - {0}\r\n{1}", biWebPortalUrl, ex.Message).Trim();
				if (ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalSystem())
				{
					Log(LogType.Warning, exceptionMessage);
				}
				else
				{
					throw new PowerBiException(exceptionMessage, ex);
				}
			}
			return (errorMessage == null);
		}

		public string UnauthorizedExceptionMessage = "The report user is unauthorized. Please check the System > BI > Report User Credentials registry item";

		public virtual string PowerBiServerVersion
		{
			get
			{
				if (powerBiServerVersion == null)
				{
					string errorMessage;
					if (IsPowerBiConfigured(out errorMessage))
					{
						powerBiServerVersion = GetPowerBiItem<JObject>("System", "GET")["ProductVersion"].ToString();
					}
					else
					{
						powerBiServerVersion = errorMessage;
					}
				}
				return powerBiServerVersion;
			}
		}
		string powerBiServerVersion { get; set; }

		public bool IsLegacyPowerBiServer
		{
			get
			{
				if (!isLegacyPowerBiServer.HasValue)
				{
					isLegacyPowerBiServer = false;

					try
					{
						var majorVersion = Convert.ToInt32(PowerBiServerVersion.Split('.').ElementAtOrDefault(0), CultureInfo.InvariantCulture);
						var minorVersion = Convert.ToInt32(PowerBiServerVersion.Split('.').ElementAtOrDefault(1), CultureInfo.InvariantCulture);
						if (majorVersion < 1 || (majorVersion == 1 && minorVersion <= 8))
						{
							isLegacyPowerBiServer = true;
						}
					}
					catch (Exception)
					{
						isLegacyPowerBiServer = false;
					}
				}
				return isLegacyPowerBiServer.Value;
			}
		}

		[ThreadStatic]
		static bool? isLegacyPowerBiServer;
		#endregion

		#region Power BI Health Check

		public bool CheckPowerBiServerHealthStatus()
		{
			var healthStatus = true;

			RefreshCatalogItems();
			if (FolderExists(ClientFolderPath))
			{
				healthStatus = CheckReports();
			}
			else
			{
				Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "Cannot find folder path '{0}'.", ClientFolderPath));
				healthStatus = false;
			}

			return healthStatus;
		}

		#region Reports

		bool CheckReports()
		{
			var healthStatus = true;

			var reportList = ListChildCatalogItems(ClientFolderPath, true).Where(c => c.Type == "PowerBIReport" || c.Type == "Report");
			healthStatus &= CheckMissingReports(reportList);
			healthStatus &= CheckReportDataSources(reportList);

			return healthStatus;
		}

		bool CheckMissingReports(IEnumerable<PowerBiCatalogItem> reportList)
		{
			var missingReports = new List<string>();

			var reports = reportList.Where(r => r.Path.StartsWith(ClientFolderPath, StringComparison.OrdinalIgnoreCase));
			foreach (var missingReport in PowerBiReportList.Where(c => !reports.Any(r => r.Name == c.Name)))
			{
				missingReports.Add(missingReport.BusinessArea + AltDirectorySeparatorChar + missingReport.Name);
			}

			if (missingReports.Any())
			{
				Log(LogType.Warning, string.Format(CultureInfo.InvariantCulture, "Missing reports from Power BI Report server:\r\n{0}", string.Join("\r\n", missingReports)));
				return false;
			}
			else
			{
				return true;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1079:DoNotCompareOnExceptionMessage", Justification = "Baseline")]
		bool CheckReportDataSources(IEnumerable<PowerBiCatalogItem> reportList)
		{
			var reportsWithIncorrectDataSource = new List<string>();

			var reports = reportList.Where(r => r.Path.StartsWith(ClientFolderPath, StringComparison.OrdinalIgnoreCase) && r.Type == "PowerBIReport");
			foreach (var report in reports)
			{
				var dataSourceObj = GetPowerBiItem<JObject>(string.Format(CultureInfo.InvariantCulture, @"PowerBIReports(path='{0}')/DataSources", report.Path), "GET");
				if (!IsDataSourceConfigured(dataSourceObj))
				{
					try
					{
						var businessArea = PowerBiReportList.First(r => r.Name == report.Name).BusinessArea;
						reportsWithIncorrectDataSource.Add(businessArea + AltDirectorySeparatorChar.ToString() + report.Name);
					}
					catch (Exception ex)
					{
						if (!CheckPowerBiReportList())
						{
							throw new Exception("No PowerBI reports have been deployed");
						}
						else if (ex.Message == "Sequence contains no matching element")
						{
							throw new Exception($"PowerBi Report List does not contain a report named {report.Name} in its {PowerBiReportList.Count()} elements");
						}
						throw;
					}
				}
			}

			if (reportsWithIncorrectDataSource.Any())
			{
				Log(LogType.Warning, string.Format(CultureInfo.InvariantCulture, "Incorrect data source configuration for reports:\r\n{0}", string.Join("\r\n", reportsWithIncorrectDataSource)));
				return false;
			}
			else
			{
				return true;
			}
		}

		protected abstract bool IsDataSourceConfigured(JObject dataSourceObj);

		#endregion

		#endregion
	}

	#endregion

	#region Classes

	public class PowerBiCatalogItems
	{
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public PowerBiCatalogItem[] value { get; set; }
	}

	public class PowerBiCatalogItem
	{
		public string Type { get; set; }
		public string Path { get; set; }
		public string Name { get; set; }
		public string Id { get; set; }
	}

	#endregion
}
