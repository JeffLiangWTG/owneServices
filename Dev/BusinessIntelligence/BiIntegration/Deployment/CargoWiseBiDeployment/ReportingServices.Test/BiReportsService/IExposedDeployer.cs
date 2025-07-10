namespace CargoWise.Bi.Deployment.ReportingServices.Testing
{
	public interface IExposedDeployer
	{
		string PowerBiServerVersion_Exposed { get; set; }
		void SetInheritParentPolicy(string path, bool inheritParentPolicy);
		string GetAnalysisServerForDataSource_Exposed(string analysisServer);

		string ClientSystemExposed { get; set; }
	}
}
