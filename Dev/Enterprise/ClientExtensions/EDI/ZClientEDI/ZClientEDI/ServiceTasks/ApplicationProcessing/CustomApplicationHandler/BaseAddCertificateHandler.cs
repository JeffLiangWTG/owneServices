using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.Client.EDI.IdentityCertificate.Business;

namespace Enterprise.Client.EDI.ServiceTasks.ApplicationProcessing.ApplicationHandler
{
	class BaseAddCertificateHandler : BaseApplicationHandler
	{
		protected override void HandleCore(EdiIdentityApplication application)
		{
			var certificates = application.Certificates
				.Where(c => c.ICE_ProcessingStatus == EdiIdentityCertificateProcessingStatus.Codes.PRC)
				.ToArray();

			var certificateData = certificates
				.Select(x => new X509Certificate2(x.ICE_CertificateData))
				.ToArray();

			AddCertificates(application, certificateData);

			foreach (var certificate in certificates)
			{
				certificate.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.COM;
			}
			application.Factory.Save();
		}

		public override bool Applicable(EdiIdentityApplication application)
		{
			return application.NeedAddCertificates();
		}

		protected virtual void AddCertificates(EdiIdentityApplication application, X509Certificate2[] certificateData)
		{
		}
	}
}
