using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.Client.EDI.IdentityCertificate.Business;

namespace Enterprise.Client.EDI.ServiceTasks.ApplicationProcessing
{
	static class ApplicationHandlerApplicableCheckExtension
	{
		public static bool NeedCreateAzureApplication(this EdiIdentityApplication application)
		{
			return !application.IDA_IsRollback && application.IDA_ClientID.IsEmpty;
		}

		public static bool NeedAddCertificates(this EdiIdentityApplication application)
		{
			return !application.IDA_IsRollback &&
					!application.IDA_ClientID.IsEmpty &&
					application.Certificates.Any(c => c.ICE_ProcessingStatus == EdiIdentityCertificateProcessingStatus.Codes.PRC);
		}

		public static bool NeedRemoveCertificates(this EdiIdentityApplication application)
		{
			return !application.IDA_ClientID.IsEmpty &&
					application.Certificates.Any(c =>
						c.ICE_IsActive &&
						(application.NeedRollbackAzureApplication() ||
						c.ICE_IsCertificateRevoked ||
						c.ICE_CertificateExpiryDate < ZDateTime.UtcNow));
		}

		public static bool NeedRollbackAzureApplication(this EdiIdentityApplication application)
		{
			return application.IDA_IsRollback || (application.LicenceDatabase != null && !application.LicenceDatabase.LD_IsActive);
		}

		public static bool NeedSyncRedirectUrls(this EdiIdentityApplication application)
		{
			return !application.IDA_ClientID.IsEmpty &&
					(application.NeedRollbackAzureApplication() ||
					application.IDA_RedirectUrlStatus == EdiIdentityApplicationRedirectUrlStatus.Codes.Nudged ||
					application.IDA_RedirectUrlStatus == EdiIdentityApplicationRedirectUrlStatus.Codes.Scheduled &&
					application.IDA_RedirectUrlLastSyncTimeUtc < ZDateTime.UtcNow.AddDays(0 - EDIDataRegistry.Instance.AzureApplicationRedirectUrlSyncInterval.Value));
		}

		public static bool NeedReactivateApplication(this EdiIdentityApplication application)
		{
			return application.LicenceDatabase != null && application.LicenceDatabase.LD_IsActive && !application.IDA_IsActive;
		}
	}
}
