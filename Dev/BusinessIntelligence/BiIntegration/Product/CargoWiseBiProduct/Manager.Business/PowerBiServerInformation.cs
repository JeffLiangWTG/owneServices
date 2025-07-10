using System;
using CargoWise.Bi.Deployment.ReportingServices;
using CargoWise.Common;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.Environment;

namespace CargoWise.Bi.Product.Manager.Business
{
	#region SuppressResourceStringsCheckRegion

	public class PowerBiServerInformation
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings")]
		public PowerBiServerInformation(string powerBiWebPortalUrl)
		{
			if (!string.IsNullOrEmpty(powerBiWebPortalUrl))
			{
				PowerBiWebPortalUrl = powerBiWebPortalUrl;
				try
				{
					var deployer = new AnalyticsReportDeployer(null);
					PowerBiServerVersion = deployer.PowerBiServerVersion;

					PowerBiReportsVersion = new VersionLabel(
						Env.Registry.DatabaseMajorAnalyticsReportProjectVersion,
						Env.Registry.DatabaseMinorAnalyticsReportProjectVersion).ToString();

					PowerBiReportsStatus = (PowerBiReportsVersion == AnalyticsReportProjectVersion.Application.ToString()) ? "Updated" : "Waiting for BI Deployment";
				}
				catch (Exception ex) when (!ex.IsCriticalException() && ex.Message.Contains("Unauthorized"))
				{
					PowerBiServerVersion = adminRightsRequired;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					PowerBiServerVersion = ex.Message;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		public string PowerBiWebPortalUrl { get; private set; }

		public string PowerBiServerVersion { get; private set; }

		public string PowerBiReportsVersion { get; private set; }

		public string PowerBiReportsStatus { get; private set; }

		const string adminRightsRequired = "(Requires administrator rights to retrieve this information)"; // message when admin rights required. Might be for CWSupport only.
	}

	#endregion
}
