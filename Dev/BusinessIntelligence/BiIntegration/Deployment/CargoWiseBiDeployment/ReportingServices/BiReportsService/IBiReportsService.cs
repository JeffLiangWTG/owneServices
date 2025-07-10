using System;
using System.Data;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.Bi.Registration.PowerBi;
using CargoWise.Types;

namespace CargoWise.Bi.Deployment.ReportingServices
{
	public interface IBiReportsService
	{
		Uri PowerBiWebPortalUrl { get; }
		string PowerBiPortalName { get; }
		string PowerBiServerName { get; }
		PowerBiReport[] GetPowerBiReportsList(IPowerBiReportLinkBuilderFactory reportLinkBuilderFactory, BiReportCategory reportCategory);
		string GetPowerBiBaseReportPath(string reportType);
		PowerBiItem GetPowerBiItemByName(string name);
		DataTable RetrieveCdcHistorySummaryDetails(ZDateTime utcFromDate, ZDateTime utcToDate, int batchSize);
		[Obsolete("Replication API Pilot")]
		DataTable RetrieveCdcHistorySummaryDetailsAfterLsn(string lsn, int batchSize);
		[Obsolete("Replication API Pilot")]
		DataTable GetCdcChanges(string lsn, int batch, string schema, string tableName, int batchSize);
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "We may use Additional Model Helpers.")]
		DataTable GetShipmentProfileReportData<T>(string shipmentNo, string companyCode, string countryCode) where T : AnalyticsModelHelper, new();
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "We may use Additional Model Helpers.")]
		DataTable GetTrialBalanceReportData<T>(string companyCode) where T : AnalyticsModelHelper, new();
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "We may use Additional Model Helpers.")]
		DataTable GetTrialBalanceReportFilteredData<T>(string companyCode) where T : AnalyticsModelHelper, new();
		string GetQueryString(string path);
		string GetCompanyLogoFromRegistry();
		string GetReportConfigurations(string companyCode, string reportName = "Shipment Profile Report", string visualName = "VisualC"); // default parameters
		void AddReportConfigurations(string reportConfigurationID, string companyCode, string configurationName, string columnConfiguration, bool isDefault, string lastModifiedDateUTC, string reportName = "Shipment Profile Report", string visualName = "VisualC"); // default parameters
		void UpdateReportConfigurations(string reportConfigurationID, string companyCode, string configurationName, string columnConfiguration, bool isDefault, string lastModifiedDateUTC, string newModifiedDateUTC, string reportName = "Shipment Profile Report", string visualName = "VisualC"); // default parameters
		void DeleteReportConfigurations(string reportConfigurationID); // default parameters
		HttpRequestMessage CloneRequest(HttpRequestMessage req, Uri newUri);
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		Task<HttpResponseMessage> ModifyProxyResponse(PowerBiItem reportItem, string fileName, HttpResponseMessage serverResponse, string countryCode, string companyCode, string branchCode);
		Task<HttpResponseMessage> ShowUnauthorisedUserMessage();
		string GetUserContextInformationFromEnvProxy();
		string GetLastModifiedDateFromDB(string reportName);
		void ReportUsage(string endpoint, string parameters, string statusCode, string databaseName, string count, string timestamp);
	}
}
