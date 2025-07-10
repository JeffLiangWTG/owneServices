using System;
using Enterprise.Integration;

namespace CargoWise.Bi.Deployment.ReportingServices.Testing
{
	public class AnalyticsReportDeployerForTest : AnalyticsReportDeployer, IExposedDeployer
	{
		public AnalyticsReportDeployerForTest(ILogger logger) : base(logger)
		{
			SetPowerBiApiUrl(PowerBiDeployerTest<AnalyticsReportDeployer>.PowerBiWebPortalUrl + "/api/v2.0/");
			ClientSystemExposed = developmentGroup;
		}

		public string PowerBiServerVersion_Exposed { get; set; }
		public string GetAnalysisServerForDataSource_Exposed(string analysisServer)
		{
			return GetAnalysisServerForDataSource(analysisServer);
		}

		public override string PowerBiServerVersion
		{
			get
			{
				return !string.IsNullOrEmpty(PowerBiServerVersion_Exposed) ?
					PowerBiServerVersion_Exposed :
					base.PowerBiServerVersion;
			}
		}

		protected override string ClientSystemGroup => ClientSystemExposed;

		public string ClientSystemExposed { get; set; }

		public string JsonDataSourceContent { get; set; }

		protected override void SendPowerBiRequest(string resourcePath, string method, string jsonContent = null)
		{
			JsonDataSourceContent = jsonContent;
			base.SendPowerBiRequest(resourcePath, method, jsonContent);
		}

		public void SetInheritParentPolicy(string path, bool inheritParentPolicy)
		{
			throw new NotImplementedException();
		}
	}
}
