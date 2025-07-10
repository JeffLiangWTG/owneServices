using System.Globalization;
using CargoWise.Application;
using Enterprise.Integration.Licensing;
using WTG.TrustedMessaging;

namespace Enterprise.TrustedMessaging.Business.SecuredHttp
{
	public class SecuredHttpTrustedClientConfiguration : ISecuredHttpTrustedClientConfiguration
	{
		public SecuredHttpTrustedClientConfiguration(string endpointRoot, ICertificatePairProvider certificatePairProvider, ISecretKeyStorage secretKeyStorage)
		{
			RemoteEndpointRootUrl = endpointRoot;
			this.certificatePairProvider = certificatePairProvider;
			this.secretKeyStorage = secretKeyStorage;
		}

		readonly ISecretKeyStorage secretKeyStorage;
		readonly ICertificatePairProvider certificatePairProvider;

		public ICertificatePairProvider CertificatePairProvider => certificatePairProvider;

		public ISecretKeyStorage SecretKeyStorage => secretKeyStorage;

		public string RemoteEndpointRootUrl { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public virtual int RetryIntervalInMilliseconds => 5000;

		public int MaxRetryCount => 3;

		public string ProductCode => "CW1";

		public string SystemId => ObjectFactory.Get<IProductRegistration>().Key.DatabaseNumber.ToString(CultureInfo.CurrentCulture);
	}
}
