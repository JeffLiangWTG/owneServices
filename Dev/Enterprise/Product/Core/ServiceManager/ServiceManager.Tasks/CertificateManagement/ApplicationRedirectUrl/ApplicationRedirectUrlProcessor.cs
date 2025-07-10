using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Newtonsoft.Json;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ServiceManager.Tasks.CertificateManagement
{
	class ApplicationRedirectUrlProcessor : IApplicationRedirectUrlProcessor
	{
		public async Task ProcessAsync(ISystemToSystemTrustApiHelper systemTrustApiHelper, IAuthenticationService authenticationService, ILogger logger, CancellationToken cancellationToken)
		{
			logger.Information("Start to send the redirect urls.");

			var redirectUrlItems = ApplicationRedirectUrlItems
				.Where(url => !string.IsNullOrEmpty(url.RedirectUrl));

			var identityRedirectUrls = ConvertToIdentityRedirectUrlRequests(redirectUrlItems);

			var oidcConfigRegistryItemValue = SystemDataRegistry.Instance.OIDCConfig.Value;
			var clientId = SystemDataRegistry.Instance.OIDCClientIDForWebApplications.Value;
			var oidcRedirectUrlRequest = new IdentityOidcRedirectUrlRequest
			{
				AuthorityUrl = oidcConfigRegistryItemValue.AuthorityURL.ToString(),
				ClientId = clientId,
				RedirectUrls = identityRedirectUrls
			};
			var jsonContent = JsonConvert.SerializeObject(oidcRedirectUrlRequest);
			var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
			var accessToken = await authenticationService.GetAccessTokenAsync(cancellationToken);
			var response = systemTrustApiHelper.SystemToSystemTrustApiPost("application/oidcredirecturls", stringContent, accessToken);

			if (response.Success)
			{
				logger.Information("Redirect urls have been sent.");
			}
			else
			{
				logger.Error("Error when sending redirect urls: " + response.Content);
			}
		}

		internal IEnumerable<ApplicationRedirectUrlItem> ApplicationRedirectUrlItems
		{
			get
			{
				var redirectUrls = new List<ApplicationRedirectUrlItem>()
				{
					new ApplicationRedirectUrlItem("Glow Service", GlowRegistry.Instance.GlowServiceUriRegistryItem, "signin-oidc"),
					new ApplicationRedirectUrlItem("Glow Service External", GlowRegistry.Instance.GlowServiceExternalUriRegistryItem, "signin-oidc"),
				};

				if (WebDataRegistry.Instance.IsEdiProd)
				{
					redirectUrls.Add(new ApplicationRedirectUrlItem("MyAccount", WebDataRegistry.Instance.CargoWiseUserPortalUrl, "api/oidc/callback"));
				}

				return redirectUrls;
			}
		}

		static List<IdentityRedirectUrl> ConvertToIdentityRedirectUrlRequests(
			IEnumerable<ApplicationRedirectUrlItem> applicationRedirectUrlItems)
		{
			return applicationRedirectUrlItems
				.Select(item => new IdentityRedirectUrl
				{
					ApplicationName = item.ApplicationName,
					RedirectUrl = item.RedirectUrl,
					RedirectUrlType = item.RedirectUrlType
				})
				.ToList();
		}
	}
}
