using Enterprise.MasterFiles.Business.VersionReport;

namespace Enterprise.Licensing.ServiceTasks.Testing
{
	sealed class SystemLicenceServiceForSendCurrentVersionReportTesting : SystemLicenceServiceForTesting
	{
		protected override IVersionReportSender CreateVersionReportSender()
		{
			return new VersionReportBuilderFactory();
		}
	}
}
