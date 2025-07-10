using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class SignatureParser
{
	public SignatureParser(string securityXml)
	{
		Argument.NotNullOrEmpty(securityXml, nameof(securityXml));

		var xmlDocument = new XmlDocument();
		xmlDocument.PreserveWhitespace = true;
		xmlDocument.LoadXml(securityXml);

		SignedXml = GetSignedXml(xmlDocument);
		Certificate = GetX509Certificate(xmlDocument);
	}

	public readonly SignedXmlWithId SignedXml;
	public readonly X509Certificate2 Certificate;

	SignedXmlWithId GetSignedXml(XmlDocument xmlDocument)
	{
		SignedXmlWithId signedXml = null;

		try
		{
			signedXml = new SignedXmlWithId(xmlDocument);

			if (xmlDocument.GetElementsByTagName((NoResString)"Signature", "http://www.w3.org/2000/09/xmldsig#").Item(0) is XmlElement signElement)
			{
				signedXml.LoadXml(signElement);
			}
		}
		catch (Exception)
		{
		}

		return signedXml;
	}

	X509Certificate2 GetX509Certificate(XmlDocument xmlDocument)
	{
		X509Certificate2 certificate = null;

		try
		{
			if (xmlDocument.GetElementsByTagName("BinarySecurityToken", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd").Item(0) is XmlElement tokenElement)
			{
				certificate = new X509Certificate2(Convert.FromBase64String(tokenElement.InnerText));
			}
		}
		catch (Exception)
		{
		}

		return certificate;
	}

	public bool IsValidSwissCustomsIssuer => (fIsValidSwissCustomsIssuer ?? (fIsValidSwissCustomsIssuer = ValidateSwissCustomsIssuer())).Value;
	bool? fIsValidSwissCustomsIssuer;

	public bool CertificateDateValidationResult => (fCertificateDateValidationResult ?? (fCertificateDateValidationResult = ValidateCertificateDate())).Value;
	bool? fCertificateDateValidationResult;

	public bool CertificateRevocationListValidationResult => (fCertificateRevocationListValidationResult ?? (fCertificateRevocationListValidationResult = ValidateCertificateRevocationList())).Value;
	bool? fCertificateRevocationListValidationResult;

	public bool CertificateChainValidationResult => (fCertificateChainValidationResult ?? (fCertificateChainValidationResult = ValidateCertificateChain())).Value;
	bool? fCertificateChainValidationResult;

	public bool SignatureValidationResult => (fSignatureValidationResult ?? (fSignatureValidationResult = ValidateSignature())).Value;
	bool? fSignatureValidationResult;

	public bool ValidationSummaryResult => SignatureValidationResult && CertificateDateValidationResult && CertificateRevocationListValidationResult && CertificateChainValidationResult;

	IEnumerable<X509ChainStatusFlags> InvalidCertificateChainErrors => fInvalidCertificateChainErrors ?? (fInvalidCertificateChainErrors = GetInvalidCertificateChainErrors().ToList());
	IEnumerable<X509ChainStatusFlags> fInvalidCertificateChainErrors;

	protected virtual IEnumerable<X509ChainStatusFlags> GetInvalidCertificateChainErrors()
	{
		var revokedCertificateCAIssuers = CHCustomsDataRegistry.Instance.RevokedCertificateCAIssuers.Value;

		using (var chain = new X509Chain())
		{
			chain.ChainPolicy.VerificationTime = ZDateTime.Now.ToDateTime();
			chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;

			if (!chain.Build(Certificate))
			{
				foreach (var chainElement in chain.ChainElements)
				{
					if (!revokedCertificateCAIssuers.Contains(chainElement.Certificate.Issuer))
					{
						foreach (var status in chainElement.ChainElementStatus)
						{
							switch (status.Status)
							{
								case X509ChainStatusFlags.NoError:
									break;
								case X509ChainStatusFlags.Revoked:
									if (chainElement.ChainElementStatus.Any(x => x.Status == X509ChainStatusFlags.UntrustedRoot) && !IsTrustedRoot(chainElement.Certificate))
									{
										yield return status.Status;
									}
									break;
								default:
									yield return status.Status;
									break;
							}
						}
					}
				}
			}
		}

		static bool IsTrustedRoot(X509Certificate2 cert)
		{
			try
			{
				using var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
				store.Open(OpenFlags.ReadOnly);
				return store.Certificates.Contains(cert);
			}
			catch (CryptographicException)
			{
				return false;
			}
		}
	}

	bool ValidateSwissCustomsIssuer() => Certificate != null && !Certificate.Issuer.IsNullOrEmpty() && CHCustomsDataRegistry.Instance.AllowedCertificateCAIssuers.Value.Contains(Certificate.Issuer);

	bool ValidateCertificateDate() => Certificate?.NotAfter > ZDateTime.Now.ToDateTime();

	bool ValidateCertificateRevocationList() => IsValidSwissCustomsIssuer && !InvalidCertificateChainErrors.Contains(X509ChainStatusFlags.Revoked);

	bool ValidateCertificateChain() => IsValidSwissCustomsIssuer && !InvalidCertificateChainErrors.Contains(X509ChainStatusFlags.PartialChain);

	bool ValidateSignature()
	{
		try
		{
			return SignedXml?.CheckSignature(Certificate?.GetRSAPublicKey()) ?? false;
		}
		catch (Exception)
		{
			return false;
		}
	}
}
