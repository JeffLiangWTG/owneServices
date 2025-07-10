using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Newtonsoft.Json;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ServiceManager.Tasks.CertificateManagement
{
	public class CertificateManager : ICertificateManager
	{
		public CertificateManager() : this(new SystemToSystemTrustApiHelper(), new AuthenticationService())
		{
		}

		internal CertificateManager(ISystemToSystemTrustApiHelper apiHelper, IAuthenticationService authenticationService)
		{
			this.apiHelper = apiHelper;
			this.authenticationService = authenticationService;
		}

		readonly ISystemToSystemTrustApiHelper apiHelper;
		readonly IAuthenticationService authenticationService;

		public bool IsAboutToExpire(byte[] certificate) => new X509Certificate2(certificate).NotAfter <= ZDateTime.Today.AddMonths(2);

		public bool IsExpired(byte[] certificate) => new X509Certificate2(certificate).NotAfter <= ZDateTime.Today;

		public string RequestCertificate(string csr)
		{
			if (string.IsNullOrEmpty(csr))
			{
				throw new ArgumentNullException(nameof(csr));
			}

			var registrationKey = ObjectFactory.Get<IProductRegistration>()?.Key;
			if (!IsRegistrationKeyValid(registrationKey))
			{
				throw new InvalidOperationException("Product registration is not valid");
			}

			var requestInfo = new IdentityCertificateInitialRequest
			{
				DatabaseNumber = registrationKey.DatabaseNumber.ToString(CultureInfo.InvariantCulture),
				Password = registrationKey.Password,
				CertificateSignRequest = csr
			};

			var secureQueryString = new SecureQueryString
			{
				[nameof(IdentityCertificateInitialRequest)] = JsonConvert.SerializeObject(requestInfo)
			};
			var stringContent = new StringContent(secureQueryString.ToString());

			try
			{
				var response = apiHelper.SystemToSystemTrustApiPost("certificate/request", stringContent);
				if (response == null || !response.Success)
				{
					throw new CertificateManagementException(response?.Content);
				}

				return response.Content;
			}
			catch (SystemToSystemTrustCertificateManagementException e)
			{
				throw new CertificateManagementException($"Failed to request certificate", e);
			}
		}

		public string RegisterCertificate(string csr, string module, string applicationDescription, string caRoot)
		{
			if (string.IsNullOrEmpty(csr))
			{
				throw new ArgumentNullException(nameof(csr));
			}

			if (string.IsNullOrEmpty(module))
			{
				throw new ArgumentNullException(nameof(module));
			}

			if (string.IsNullOrEmpty(applicationDescription))
			{
				throw new ArgumentNullException(nameof(applicationDescription));
			}

			if (string.IsNullOrEmpty(caRoot))
			{
				throw new ArgumentNullException(nameof(caRoot));
			}

			var registrationKey = ObjectFactory.Get<IProductRegistration>()?.Key;
			if (!IsRegistrationKeyValid(registrationKey))
			{
				throw new InvalidOperationException("Product registration is not valid");
			}

			var token = GetAccessToken();

			var certificateRequest = new IdentityCertificateRegisterRequest
			{
				CertificateSignRequest = csr,
				Module = module,
				ApplicationDescription = applicationDescription,
				CARoot = caRoot
			};

			var stringContent = new StringContent(JsonConvert.SerializeObject(certificateRequest), Encoding.UTF8, "application/json");

			try
			{
				var response = apiHelper.SystemToSystemTrustApiPost("certificate/register", stringContent, token);
				if (response == null || !response.Success)
				{
					throw new CertificateManagementException(response?.Content);
				}

				return response.Content;
			}
			catch (SystemToSystemTrustCertificateManagementException e)
			{
				throw new CertificateManagementException($"Failed to register certificate", e);
			}
		}

		bool IsRegistrationKeyValid(IProductRegistrationKey registrationKey)
		{
			return registrationKey != null
				&& registrationKey.DatabaseNumber > 0
				&& !registrationKey.Password.IsNullOrEmpty();
		}

		public string RolloverCertificate(string clientId, string csr, string caRoot = null)
		{
			if (string.IsNullOrEmpty(csr))
			{
				throw new ArgumentNullException(nameof(csr));
			}

			if (string.IsNullOrEmpty(clientId))
			{
				throw new ArgumentNullException(nameof(clientId));
			}

			var token = GetAccessToken();

			var certificateRequest = new IdentityCertificateRolloverRequest
			{
				ClientId = clientId,
				CertificateSignRequest = csr
			};

			if (!string.IsNullOrEmpty(caRoot))
			{
				certificateRequest.CaRootType = caRoot;
			}

			var stringContent = new StringContent(JsonConvert.SerializeObject(certificateRequest), Encoding.UTF8, "application/json");

			try
			{
				var response = apiHelper.SystemToSystemTrustApiPost("certificate/rollover", stringContent, token);
				if (response == null || !response.Success)
				{
					throw new CertificateManagementException(response?.Content);
				}

				return response.Content;
			}
			catch (SystemToSystemTrustCertificateManagementException e)
			{
				throw new CertificateManagementException($"Failed to rollover certificate", e);
			}
		}

		public (string TenantId, string ClientId, byte[] Certificate, string StatusCode) DownloadCertificate(string operationId)
		{
			if (string.IsNullOrEmpty(operationId))
			{
				throw new ArgumentNullException(nameof(operationId));
			}

			try
			{
				var response = apiHelper.SystemToSystemTrustApiGet("certificate/" + operationId);
				if (response == null || !response.Success)
				{
					throw new CertificateManagementException(response?.Content);
				}

				var certificateResponse = JsonConvert.DeserializeObject<IdentityCertificateResponse>(response.Content);

				return (certificateResponse.TenantId, certificateResponse.ClientId, certificateResponse.CertificateData, certificateResponse.StatusCode);
			}
			catch (SystemToSystemTrustCertificateManagementException e)
			{
				throw new CertificateManagementException($"Failed to download certificate", e);
			}
		}

		public IEnumerable<byte[]> DownloadCertificates(string clientId)
		{
			if (string.IsNullOrEmpty(clientId))
			{
				throw new ArgumentNullException(nameof(clientId));
			}

			var token = GetAccessToken();

			try
			{
				var response = apiHelper.SystemToSystemTrustApiGet($"certificates/{clientId}", token);
				if (!response.Success)
				{
					throw new CertificateManagementException($"Failed to process request with content: \n{response.Content}");
				}

				var certificateResponse = JsonConvert.DeserializeObject<IdentityCertificateResponse>(response.Content);
				var certificates = new List<byte[]>();
				foreach (var certData in certificateResponse.CertificateDataArray)
				{
					certificates.Add(certData.CertificateData);
				}

				return certificates;
			}
			catch (SystemToSystemTrustCertificateManagementException e)
			{
				throw new CertificateManagementException($"Failed to download certificates", e);
			}
		}

		public async Task<string> LoadDatabaseNumberByClientIdAsync(CancellationToken cancellationToken)
		{
			var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

			var stringContent = new StringContent(string.Empty, Encoding.UTF8, "application/json");

			try
			{
				var response = apiHelper.SystemToSystemTrustApiPost("load/databasenumber", stringContent, token);
				if (response == null || !response.Success)
				{
					throw new CertificateManagementException(response?.Content);
				}

				return response.Content;
			}
			catch (SystemToSystemTrustCertificateManagementException e)
			{
				throw new CertificateManagementException($"Failed to load database number", e);
			}
		}

		string GetAccessToken()
		{
			var token = authenticationService.GetAccessToken();
			return token;
		}

		Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
			=> authenticationService.GetAccessTokenAsync(cancellationToken);
	}
}
