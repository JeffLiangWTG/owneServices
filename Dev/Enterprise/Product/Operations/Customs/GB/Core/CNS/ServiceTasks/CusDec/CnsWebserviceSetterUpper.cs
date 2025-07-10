using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Enterprise.Customs.GB.Chief;
using Enterprise.Customs.GB.Registry;

namespace Enterprise.Customs.GB.CNS.WebServices
{
	namespace CnsPrints
	{
		public partial class MailBox : ICspPrintsMailBoxProvider
		{
			public MailBox()
			{
				UserAgent = Chief.GenericMessagingHarness.CredentialsAndBadgeChecker.GetUserAgentForSoapRequests("");
				CnsWebserviceSetterUpper.SetUrlCredentialsAndAcceptSslCertificate(this);
			}

			ICspResultOfAcknowledgement ICspPrintsMailBoxProvider.acknowledgeEdifactPrints(string companyCode, string printer, string batchId)
			{
				UserAgent = Chief.GenericMessagingHarness.CredentialsAndBadgeChecker.GetUserAgentForSoapRequests(companyCode);
				AcknowledgeEdifactPrints acknowledgeEdifactPrints = new AcknowledgeEdifactPrints();
				acknowledgeEdifactPrints.batchId = decimal.Parse(batchId);
				acknowledgeEdifactPrints.device = printer;
				return this.AcknowledgeEdifactPrints(acknowledgeEdifactPrints);
			}

			ICspDownloadResult ICspPrintsMailBoxProvider.getAvailableEdifactPrints(string company, string printer)
			{
				UserAgent = Chief.GenericMessagingHarness.CredentialsAndBadgeChecker.GetUserAgentForSoapRequests(company);
				GetAvailableEdifactPrints getter = new GetAvailableEdifactPrints();
				getter.device = printer;
				return this.GetAvailableEdifactPrints(getter);
			}

			ICspDownloadResult ICspPrintsMailBoxProvider.checkCdsCredentials(string company, string printer)
			{
				UserAgent = Chief.GenericMessagingHarness.CredentialsAndBadgeChecker.GetUserAgentForSoapRequests(company);
				return new CdsCredentialChecker().checkCdsCredentials(Credentials, printer, GBCustomsDataRegistry.Instance.CnsCdsCheckCredentialsUrl, UserAgent);
			}
		}
	}

	namespace CnsChiefEDI
	{
		public partial class ChiefEDIPortQSService : IGbCspUploaderInterface
		{
			public ChiefEDIPortQSService()
			{
				UserAgent = Chief.GenericMessagingHarness.CredentialsAndBadgeChecker.GetUserAgentForSoapRequests("");
				CnsWebserviceSetterUpper.SetUrlCredentialsAndAcceptSslCertificate(this);
			}

			CredentialsSetting credentialsSetting;
			public CredentialsSetting CredentialsSetting
			{
				get { return credentialsSetting; }
				set
				{
					credentialsSetting = value;
					this.Credentials = credentialsSetting.GetCredential(null, null);
				}
			}
		}
	}

	public static class CnsWebserviceSetterUpper
	{
		public static void SetUrlCredentialsAndAcceptSslCertificate(System.Web.Services.Protocols.SoapHttpClientProtocol ws)
		{
			ServicePointManager.ServerCertificateValidationCallback += delegate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
			{
				return true;  // otherwise we barf at CNS's invalid SSL certificate
			};
		}
	}
}
