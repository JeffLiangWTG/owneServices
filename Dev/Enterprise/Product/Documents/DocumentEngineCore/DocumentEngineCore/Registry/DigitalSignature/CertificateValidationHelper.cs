using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using CargoWise.Common;

namespace Enterprise.DocumentEngineCore.Registry
{
	public static class CertificateValidationHelper
	{
		public static string GetInvalidCertificateChainErrors(byte[] certificate, string certificatePassword, Action<X509ChainPolicy> configureChainPolicy = null)
		{
			Argument.NotNull(certificate, nameof(certificate));

			if (certificate.Length <= 0)
			{
				return string.Empty;
			}

			using var cert = new X509Certificate2(certificate, certificatePassword);
			using var chain = new X509Chain();
			chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
			configureChainPolicy?.Invoke(chain.ChainPolicy);
			return chain.Build(cert) ? string.Empty : GetErrorsFromChain(chain);
		}

		static string GetErrorsFromChain(X509Chain chain)
			=> string.Join(System.Environment.NewLine, chain.ChainStatus
				.Where(status => status is not { Status: X509ChainStatusFlags.NoError })
				.Select((status, i) => $"{i + 1}. {GetErrorInformation(status)}"));

		static string GetErrorInformation(X509ChainStatus status)
			=> status.Status switch
			{
				X509ChainStatusFlags.NoError => string.Empty,
				X509ChainStatusFlags.NotTimeValid or X509ChainStatusFlags.CtlNotTimeValid => ResString.GetMultilingualString("CHAINSTATUS|CERT_E_EXPIRED", "The certificate has expired. Please create a new certificate."),
				X509ChainStatusFlags.NotSignatureValid or X509ChainStatusFlags.CtlNotSignatureValid => ResString.GetMultilingualString("CHAINSTATUS|TRUST_E_CERT_SIGNATURE", "The certificate signature is not valid."),
				X509ChainStatusFlags.InvalidNameConstraints or X509ChainStatusFlags.InvalidPolicyConstraints or X509ChainStatusFlags.InvalidExtension or X509ChainStatusFlags.InvalidBasicConstraints => ResString.GetMultilingualString("CHAINSTATUS|CERT_E_INVALID_NAME", "The certificate is invalid due to name, policy and/or extension constraints."),
				X509ChainStatusFlags.UntrustedRoot => ResString.GetMultilingualString("CHAINSTATUS|CERT_E_UNTRUSTEDROOT", "The certificate is not valid due to an untrusted root certification."),
				_ => status.StatusInformation.TrimEnd('\n', '\r')
			};
	}
}
