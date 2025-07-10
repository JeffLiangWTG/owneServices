using System.Threading.Tasks;
using Enterprise.TrustedMessaging.Business.SecuredHttp;
using WTG.TrustedMessaging.Models;
using WTG.TrustedMessaging.MyAccount;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.TrustedMessaging.Business
{
	public interface ICertificateAuthority
	{
		Task<TrustedResponse<CertificateResponse>> GetCertificate(CertificateInfo request);
	}

	public class CertificateAuthorityClient : ICertificateAuthority
	{
		public Task<TrustedResponse<CertificateResponse>> GetCertificate(CertificateInfo request)
		{
			var client = new MyAccountClient(new UserPortalClientConfiguration());
			return client.GetCertificate(request);
		}
	}

	public class SecuredHttpTrustedClient : MyAccountClient
	{
		public SecuredHttpTrustedClient(ISecuredHttpTrustedClientConfiguration configuration) : base(configuration)
		{
		}
	}
}
