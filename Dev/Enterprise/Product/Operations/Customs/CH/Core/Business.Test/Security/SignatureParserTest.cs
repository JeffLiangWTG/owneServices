using System;
using System.Security.Cryptography.X509Certificates;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

public class SignatureParserTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("securityXml is null", () => new SignatureParser(null));

			var parser = new SignatureParser(TestingData.InputEvvResponseVATValidSignature);

			AssertNotNull("Certificate not null", parser.Certificate);
			AssertEquals("type of Certificate", "X509", parser.Certificate.GetFormat());
			AssertNotNull("SignedXml not null", parser.SignedXml);
		});
	}

	public void TestIsValidSwissCustomsIssuer()
	{
		CombineAssertions(() =>
		{
			var chCertificate = new SignatureParser(TestingData.InputEvvResponseVATValidSignature);
			Assert("Valid Swiss Customs Issuer", chCertificate.IsValidSwissCustomsIssuer);

			chCertificate = new SignatureParser(TestingData.InputEvvResponseVATInvalidSwissCustomsCertificate);
			AssertEquals("Valid Swiss Customs Issuer", false, chCertificate.IsValidSwissCustomsIssuer);
		});
	}

	[TestDate(2024, 1, 24)]
	public void TestCertificateDateValidationResult()
	{
		CombineAssertions(() =>
		{
			var chCertificate = new SignatureParser(TestingData.InputEvvResponseVATValidSignature);
			Assert("Certificate Date Validation", chCertificate.CertificateDateValidationResult);

			chCertificate = new SignatureParser(TestingData.InputEvvResponseVATExpiredCertificate);
			AssertEquals("Certificate Date Validation", false, chCertificate.CertificateDateValidationResult);
		});
	}

	public void TestCertificateRevocationListValidationResult() => CombineAssertions(() =>
	{
		using (CHCustomsDataRegistry.Instance.RevokedCertificateCAIssuers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<string>()))
		{
			var signatureParser = new SignatureParser(TestingData.InputEvvResponseVATValidSignature);
			AssertEquals($"InputEvvResponseVATValidSignature\n{GetCertificateInfo(signatureParser.Certificate)}", true, signatureParser.CertificateRevocationListValidationResult);

			signatureParser = new SignatureParser(TestingData.InputEvvResponseVATInvalidSwissCustomsCertificate);
			AssertEquals($"InputEvvResponseVATInvalidSwissCustomsCertificate\n{GetCertificateInfo(signatureParser.Certificate)}", false, signatureParser.CertificateRevocationListValidationResult);
		}

		using (CHCustomsDataRegistry.Instance.RevokedCertificateCAIssuers.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { SwissCAII }))
		{
			var signatureParser = new SignatureParser(TestingData.InputEvvResponseVATValidSignature);
			AssertEquals($"InputEvvResponseVATValidSignature, CA01 revoked\n{GetCertificateInfo(signatureParser.Certificate)}", true, signatureParser.CertificateRevocationListValidationResult);
		}
	});

	public void TestCertificateChainValidationResult()
	{
		CombineAssertions(() =>
		{
			var chCertificate = new SignatureParser(TestingData.InputEvvResponseVATValidSignature);
			Assert("Certificate Chain Validation", chCertificate.CertificateChainValidationResult);

			chCertificate = new SignatureParser(TestingData.InputEvvResponseVATInvalidSwissCustomsCertificate);
			AssertEquals("Certificate Chain Validation", false, chCertificate.CertificateChainValidationResult);
		});
	}

	public void TestSignatureValidationResult()
	{
		CombineAssertions(() =>
		{
			var chCertificate = new SignatureParser(TestingData.InputEvvResponseVATValidSignature);
			Assert("Certificate Chain Validation", chCertificate.SignatureValidationResult);

			chCertificate = new SignatureParser(TestingData.InputEvvResponseVATInvalidSignature);
			AssertEquals("Certificate Chain Validation", false, chCertificate.SignatureValidationResult);
		});
	}

	static string GetCertificateInfo(X509Certificate2 certificate)
	{
		return string.Format("Subject: {0}\nIssuer: {1}\nNotBefore: {2} NotAfter: {3}",
			certificate.Subject,
			certificate.Issuer,
			certificate.NotBefore.ToString("yyyy-MM-dd"),
			certificate.NotAfter.ToString("yyyy-MM-dd"));
	}

	const string SwissCAII = "CN=Swiss Government Root CA II, OU=Certification Authorities, OU=Services, O=The Federal Authorities of the Swiss Confederation, C=CH";
}
