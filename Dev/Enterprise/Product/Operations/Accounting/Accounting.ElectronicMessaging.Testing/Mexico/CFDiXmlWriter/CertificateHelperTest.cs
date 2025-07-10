using System.IO;
using System.Xml;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico.Testing
{
	sealed class CertificateHelperTest : TestCaseWithFactory
	{
		public void TestItUsedAsDependency()
		{
			AssertType<CertificateHelper>(new CFDiXmlWriter().SignXmlDocument_ExposedForTestOnly);
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCadenaOriginalXSLT_Exists()
		{
			var xsltResource = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.ElectronicMessaging\Mexico\CFDiXmlWriter\Transformation\cadenaoriginal_4_0.xslt";
			Assert("XSLT File must be exists for EInvoicing process for Mexico", File.Exists(xsltResource));
		}

		public void TestCredentials_WithNullCertificate()
		{
			var doc = new XmlDocument();
			doc.LoadXml(CfdiXml_ForTestOnly);

			var sello = doc?.SelectSingleNode($"/*[local-name()='Comprobante']/@Sello");
			var certificado = doc?.SelectSingleNode($"/*[local-name()='Comprobante']/@Cerificado");
			var noCertificado = doc?.SelectSingleNode($"/*[local-name()='Comprobante']/@NoCerificado");

			AssertNull("Sello attribute must be be null", sello);
			AssertNull("Certificado attribute must be be null", certificado);
			AssertNull("NoCertificado attribute must be be null", noCertificado);
			(new CertificateHelper() as ICertificateHelper).SignCFDiXmlDocument(doc, null);

			sello = doc?.SelectSingleNode($"/*[local-name()='Comprobante']/@Sello");
			certificado = doc?.SelectSingleNode($"/*[local-name()='Comprobante']/@Cerificado");
			noCertificado = doc?.SelectSingleNode($"/*[local-name()='Comprobante']/@NoCerificado");

			AssertNull("Sello attribute must be be null", sello);
			AssertNull("Certificado attribute must be be null", certificado);
			AssertNull("NoCertificado attribute must be be null", noCertificado);
		}

		public void TestCredentials_WithValidCertificate()
		{
			var credential_ForTestOnly = GetCompanyEInvoicingCredential_ForTestOnly(GlbCompany.CurrentCompany.PK);

			var doc = new XmlDocument();
			doc.LoadXml(CfdiXml_ForTestOnly);

			var sello = doc?.SelectSingleNode($"/*[local-name()='Comprobante']/@Sello");
			var certificado = doc?.SelectSingleNode($"/*[local-name()='Comprobante']/@Certificado");
			var noCertificado = doc?.SelectSingleNode($"/*[local-name()='Comprobante']/@NoCertificado");

			AssertNull("Sello attribute must be be null", sello);
			AssertNull("Certificado attribute must be be null", certificado);
			AssertNull("NoCertificado attribute must be be null", noCertificado);
			(new CertificateHelper() as ICertificateHelper).SignCFDiXmlDocument(doc, credential_ForTestOnly);

			sello = doc?.SelectSingleNode($"/*[local-name()='Comprobante']/@Sello");
			certificado = doc?.SelectSingleNode($"/*[local-name()='Comprobante']/@Certificado");
			noCertificado = doc?.SelectSingleNode($"/*[local-name()='Comprobante']/@NoCertificado");

			AssertNotNull("Sello attribute should have proper value.", sello);
			AssertEquals("Xh6k+tgMUNU9BzQ8L/HSa3cLF/yLQ8HO97XSwmYe4KqIqivGkkFKD+8NWDtymnPUDEGkqsre6EOmhH9euWAoKeU96vfiQpqEDyI1PWbJbW/pUTZ56YYiFbus3PN8fcwmUJWAttg45nUJZoaZYtU5N1ITiMIkW9tRa7lcMDU/ITk=", sello.InnerText);

			AssertNotNull("Certificado attribute should have proper value.", certificado);
			AssertEquals("MIID5zCCAs+gAwIBAgIISC6QKk2pE8kwDQYJKoZIhvcNAQELBQAwbDELMAkGA1UEBhMCSVQxHTAbBgNVBAoTFEFnZW56aWEgZGVsbGUgRG9nYW5lMRwwGgYDVQQLExNTZXJ2aXppbyBUZWxlbWF0aWNvMSAwHgYDVQQDExdDQSBBZ2VuemlhIGRlbGxlIERvZ2FuZTAeFw0xNzA3MjEwODE0MTVaFw0yMDA3MjEwODI0MDBaMGcxCzAJBgNVBAYTAklUMR0wGwYDVQQKDBRBZ2VuemlhIGRlbGxlIERvZ2FuZTEfMB0GA1UECwwWU2Vydml6aSBBdXRlbnRpY2F6aW9uZTEYMBYGA1UEAwwPMTMxNDk2MDAxNTAtMDAzMIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCDc98IEYWAhDgTuzEV1dMAo2RMQMTEoP8P/IGumnrEOd8hA0IuYANVkQUnQbXiBR0SHDQsLrcC92sJ+UX2IWD22ea9vRhY8TBq4MEBdPoadEICPP1IOAr4DioTKkUz3soY/d5ySKgmmJqhu/v+I37aoDrJrQmDTUhl+oItgRUkLwIDAQABo4IBFDCCARAwHwYDVR0jBBgwFoAUKdSjQ3uS8RNyA93kg8fUVOrBFXcwEwYDVR0lBAwwCgYIKwYBBQUHAwIwgagGA1UdHwSBoDCBnTCBmqCBl6CBlIaBkWxkYXA6Ly9jYWRzLmRvZ2FuZS5maW5hbnplLml0L0NOPUNBJTIwQWdlbnppYSUyMGRlbGxlJTIwRG9nYW5lLE9VPVNlcnZpemlvJTIwVGVsZW1hdGljbyxPPUFnZW56aWElMjBkZWxsZSUyMERvZ2FuZSxDPWl0P2NlcnRpZmljYXRlUmV2b2NhdGlvbkxpc3QwHQYDVR0OBBYEFFcCfVI3YXOW9zK3ZTvxWSmxyUTWMA4GA1UdDwEB/wQEAwIFoDANBgkqhkiG9w0BAQsFAAOCAQEAMmSp00P3r0CFjv+cSJ/fj2p/l8XoFoj7oQZlygHIOSy50/dEFEQ5B+0P+FuIHo4zp8tAlJS0z5y9NQzSqlJpms4bw9L9v2v429N23LSXC1Gxh8pd+nFTs+pw+Lh7SLputtsdUAWnJ8Sdp7R47wvG2IDsTFEeIkRD4IRfwgUyGdaTuOaKj4TR+x6lBCnjFeNbxS05KwQ7s7RaJP5MYBvPjEMEs9SuA7gmkBE7v0JK4PvXN4N8+EsJqt4z2VHInsDQ3b1ZksfOzqCRqiDfuoDObb8EH14NSlyFs9kBaH6uFG/ego2lp8LeiwEz/8EqX7NHr9km8sJ06CbvalGv6JFmVQ==", certificado.InnerText);

			AssertNotNull("NoCertificado attribute should have proper value.", noCertificado);
			AssertEquals("20001000000300022823", noCertificado.InnerText);
		}

		public void TestTransformSerialNumberFromHexStringForMexico()
		{
			var credentialForTestOnly = GetCompanyEInvoicingCredential_ForTestOnly(GlbCompany.CurrentCompany.PK);
			var expectedSerialNumber = "20001000000300022823";

			var doc = new XmlDocument();
			doc.LoadXml(CfdiXml_ForTestOnly);

			(new CertificateHelper() as ICertificateHelper).SignCFDiXmlDocument(doc, credentialForTestOnly);
			var noCertificado = doc?.SelectSingleNode($"/*[local-name()='Comprobante']/@NoCertificado");

			AssertNotNull("NoCertificado attribute should have proper value.", noCertificado);
			AssertEquals("3230303031303030303030333030303232383233", credentialForTestOnly.GP_CertificateSerialNumber);
			AssertEquals(expectedSerialNumber, noCertificado.InnerText);
		}

		public void TestCredentials_GPCertificateSerialNumberIsEmpty()
		{
			var credentialForTestOnly = GetCompanyEInvoicingCredential_ForTestOnly(GlbCompany.CurrentCompany.PK);

			credentialForTestOnly.GP_CertificateSerialNumber = ZString.Empty;
			var expectedSerialNumber = "482E902A4DA913C9";

			var doc = new XmlDocument();
			doc.LoadXml(CfdiXml_ForTestOnly);

			(new CertificateHelper() as ICertificateHelper).SignCFDiXmlDocument(doc, credentialForTestOnly);
			var noCertificado = doc?.SelectSingleNode($"/*[local-name()='Comprobante']/@NoCertificado");

			AssertNotNull("NoCertificado attribute should have proper value.", noCertificado);
			AssertEquals("482E902A4DA913C9", credentialForTestOnly.SerialNumber);
			AssertEquals(expectedSerialNumber, noCertificado.InnerText);
		}

		public void TestGetCredentialData()
		{
			var (certificate, password) = (new CertificateHelper() as ICertificateHelper).GetCredentialDataForCancellation(null);
			AssertEquals(string.Empty, certificate);
			AssertEquals(string.Empty, password);

			var credentialForTestOnly = GetCompanyEInvoicingCredential_ForTestOnly(GlbCompany.CurrentCompany.PK);
			(certificate, password) = (new CertificateHelper() as ICertificateHelper).GetCredentialDataForCancellation(credentialForTestOnly);
			AssertNotNull("Certificate should have proper value.", certificate);
			AssertNotNull("Private key should have proper value.", password);
			AssertEquals(CertificateEncoded_ForTestOnly, certificate);
			AssertEquals(CertificatePrivateKey_ForTestOnly, password);
		}

		#region Implementation

		GlbCompanyEInvoicingCertificateCredential GetCompanyEInvoicingCredential_ForTestOnly(ZGuid companyPK)
		{
			var credentialForTestOnly = Factory.New<GlbCompanyEInvoicingCertificateCredential>();

			credentialForTestOnly.GP_GC = companyPK;
			credentialForTestOnly.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			credentialForTestOnly.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			credentialForTestOnly.GP_CertificateSerialNumber = "3230303031303030303030333030303232383233";
			credentialForTestOnly.GP_IssueDate = ZDateTime.Now.AddDays(-1);
			credentialForTestOnly.GP_ExpiryDate = ZDateTime.Now.AddDays(1);

			return credentialForTestOnly;
		}

		internal static string CfdiXml_ForTestOnly => @"<?xml version=""1.0"" encoding=""utf-8""?><cfdi:Comprobante xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" Version=""4.0"" Fecha=""0001-01-01T00:00:00"" SubTotal=""0.00"" Moneda=""AED"" Total=""0.00"" TipoDeComprobante=""I""  Exportación=""01"" xmlns:cfdi=""http://www.sat.gob.mx/cfd/4"" />";
		internal static string CertificateEncoded_ForTestOnly => "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";
		internal static string CertificatePrivateKey_ForTestOnly => "CaRgOw1sE";

		#endregion
	}
}
