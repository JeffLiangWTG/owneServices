using Enterprise.MasterFiles.Business.VersionReport;

namespace Enterprise.Licensing.ServiceTasks.Testing
{
	sealed class TestVersionReportSender : IVersionReportSender
	{
		public int SendCurrentCalls;
		public VersionReport SendCurrent(string licenceUsage, bool isLicenceUsageRequest)
		{
			++SendCurrentCalls;
			return null;
		}
	}
}
