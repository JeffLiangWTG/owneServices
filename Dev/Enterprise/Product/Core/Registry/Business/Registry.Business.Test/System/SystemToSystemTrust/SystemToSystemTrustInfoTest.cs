using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.SystemToSystemTrust.Extensions;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SystemToSystemTrustInfo))]
	sealed class SystemToSystemTrustInfoTest : RegistryBusinessObjectTemplateTestCase
	{
		#region TestData

		static readonly string testRsaKey = FormatOneLine(@"-----BEGIN RSA PRIVATE KEY-----
MIIEpAIBAAKCAQEAvTQ1eKPDlt7Akq2q89lv+TPjL+r/Oxlgs+Lugu/7WyFlQPAf
eevry+q7F9gAjblSEDbeKToWTtDuGkPYZOtU3hK5Kk6i2C7tvOeMf7tqYT9Iq76P
NNhW0FHm90W0hWzdl4WzSFi+/RqD50UaXv8sMHJrpEdCcV8PVw35+E3t2AYNdx5a
nXKiXumBbak7q+Aq4eOadmSDTkZOvrEiqdiy6sBBwKe64XymDv6+cbwIt4OPIvmn
b4oxXyCNpKutCCNdGVYNWW00M9prCsCeEkdEw8GBx+cCu41U5Exb15CJ1cuL/kmK
BsjOfQUN/Q81pDnh6DSvyYYbBa62Evt+94MN7wIDAQABAoIBAQCaGGt0Vex2c/Vj
gQ46jF0mGZxu6nX1LDYWTDz1Z37QOeK9gNQh+IOTERpP0RyiUM0FZ6qI6UZB6ugB
fEBB8WitdZJZYrI+X/pAYFWNspZxnZkSR6Fa6NfL0ujJe3wLLx9KyRm7Uow6l93V
/fH8bNcQniANK/xxWXqk7D0qfk24Ic66/zWXskbw6FegrhZ6LmiT/wsf0G0D7he8
ywcsqv5bfOax25SMhQ5MZUG0vignfH+9177hvfID0817f11rBM2Of5cG3tCcPLMm
LtK1heBgBT4DPI2ssr7lpQoKGZchUl8jKEvsaPmc1Q1Yzcfeq5OMvuH3z3z8dnNg
ibuSDZQBAoGBAOlPwzmqcD9H+rSK4heAnZTHAXzYYkHeRan/4TZaeBrte/VkyIy5
oZyde2KQ0ozFWvn0MIXXuFzcK0h9PrGgwJAXzioHY2/sKo3+EORd1kkm/5NGogNe
9SPGvNEkTXQMT3maT5GFOXxj5UzX642neB9WdaUbFBmknhHRDFKQs7b1AoGBAM+a
ZaOaG7ej57MO87EsxX067GcjZuAJCvKBj8AmATtCDB16P9cYYpUGI4V/6kQ05heq
YCsjzlvetaXFchStg643zHc90+/kK34IiHgmNBnRriR8/4txKI7tMk5Po082QSJn
AqWHcfpIBIpJiWiKoUn5Gfuvb5p9+p5QrpQE2PrTAoGAOsLmDo4Iu0drszEPhI/W
IHSGwWTWSnSq4wgZNtFOUqnhgIqjoB3YwFNBki+bd+z03uNLnUoZmvmwxQ8WTTKJ
jUERobA+sR+wEBcfNgUURVRXJkax41t1Lk+Nmrcj8shu89eTlrkRI20dgV8YapL1
RB9Ifg713weA8EfQbLMZMEkCgYEAx9HskpP6qb9xfL+ff6GAVREqCfvhQTJy/wgF
vQTXmpnv30+Tsw0dliLafdypOA1SiuTKu7szHOo4HN7290ArxryeaQdxvyz3T9AW
ys66xz8zRpupvCWmzCsyiH3OyqeF+f78ZScBZc170T8Gq0EEDZdekwpBeYpYd5lg
78fasVECgYAMKazsFA5W1i/RQQuwPLtRWL9KUrUVxApb/KPHQnYfCngC1yfh9DzI
uXO3wlO8rHBR1PodcbGi/JZMJTAmnVrJO5MFCQ6fixl7Da8C66D2baKhpz22btOO
bl7N795AsX2VYpkKWv9TXd+vJ+n2V2XYlSz92Cg4YtX42SSJRVd09g==
-----END RSA PRIVATE KEY----");

		static readonly string testCsr = FormatOneLine(@"-----BEGIN CERTIFICATE REQUEST-----
MIICvzCCAacCAQAwejELMAkGA1UEBhMCQVUxDDAKBgNVBAgMA05TVzEPMA0GA1UE
BwwGU3lkbmV5MRgwFgYDVQQKDA9XaXNlVGVjaCBnbG9iYWwxHDAaBgNVBAsME0lk
ZW50aXR5QW5kU2VjdXJpdHkxFDASBgNVBAMMC2V4YW1wbGUuY29tMIIBIjANBgkq
hkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAvVIkZbr64CddxSFwXVoRMbHihIBoL+9z
ztwm+/9FwFbg6RppDyGp6BkSrxbn5siU2HcobxNI9ZjunbfFi/2bI/+qp5CyfFHr
8qlOxROoGJyGZYUefCLUVLXI8/6CqgQJim5n7Jmxtb8w9MpFd4jlgqZIeQDnc9nK
17M10MVGvr8FnY3twoXTkHDvzhj4cUcvhk9ulDuaU5Y72GfGKqKowP5lL8+Agbi4
tYTFm/0vQBZaWeCHQ33KsYZtCIekrtpkvkaGrGEHKF4twgl4z9GVyarx/+ZC97zn
qF3qatUxDtM50zp0nyG2xdY2h9DfzjkHlGfW9Ts2c6aj0JZD5RyLjwIDAQABoAAw
DQYJKoZIhvcNAQELBQADggEBAI3cZiCqaWVk76AbK8ZxAaj/QOG6McDY3cU4xuDc
co3V2svM0rF7KaV4Ras3IYR2AK10jMIWDODWZyD4rGNkbTtc5f7/eBewukhSWlvN
Tng55EuTH5rJxAsKdLf8Q0VTESbPVzQdwtV5hyrMARDNga3O8E5LiCusKmX/F6Rc
LnQDiVR74BM+YWvceQzN4kAupzvp6enw3iGSjFW9ONR+jtxxedkPQYXYTAYS1mcm
sQfLgkpisxYeE0TO3h0UBx6IaUZI0QdZO1H+BH1nmSR1ewKgvhxrJ6NZT60GzK1J
3n7bECqg5X2rfuSb7dO6VyiTywmuxGksfa2AvBiuN2o0QlA=
-----END CERTIFICATE REQUEST-----");

		static readonly ZString testStringCertificate = "testscertificate";
		readonly byte[] testByteCertificate = Convert.FromBase64String(testStringCertificate);
		static readonly ZString testOperationId = ZGuid.NewZGuid().ToString();
		static readonly ZString testClientId = ZGuid.NewZGuid().ToString();
		static readonly ZString testTenantId = ZGuid.NewZGuid().ToString();
		Mock<IDataProtectorServiceFactory> mockDataProtectorServiceFactory;
		Mock<IDataProtectorService> mockDataProtectorService;
		List<string> capturedKeys;
		List<string> capturedEncryptedKeys;

		const string Certificate = @"-----BEGIN CERTIFICATE-----
MIID6DCCAtCgAwIBAgIQROOIIrCQ7LRi0hKk4rLEdDANBgkqhkiG9w0BAQsFADBx
MQswCQYDVQQGEwJDTjERMA8GA1UECgwIamF5d3RnQ0ExHDAaBgNVBAsME0lkZW50
aXR5QW5kU2VjdXJpdHkxEDAOBgNVBAgMB05hbmppbmcxDTALBgNVBAMMBE5KRzEx
EDAOBgNVBAcMB1dURyBOSkcwHhcNMjMwMjI0MDIzMTU5WhcNMjQwMjI0MDMzMTU5
WjBxMQswCQYDVQQGEwJDTjERMA8GA1UECgwIamF5d3RnQ0ExHDAaBgNVBAsME0lk
ZW50aXR5QW5kU2VjdXJpdHkxEDAOBgNVBAgMB05hbmppbmcxDTALBgNVBAMMBE5K
RzExEDAOBgNVBAcMB1dURyBOSkcwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEK
AoIBAQCVJt0UDpMaAOMxUiBtstzgTVtBC541t5+mGCS8wOmOdMCspwU1jkC6w0VB
sh0ZwFkGJyu51aEOkqES2oTR/G7/ISj7iZO2Hx4C7dlShm31gDlp2sgNkWls+Acg
HsAvu1GpHEXQjMv1gtPZr5uC3ys9uY5zm5XyirCno4+AEzntZ9Bxvupu1cmuS9Z7
xFdFXuBq4pmb7s6vGp3bMMabEimvlRkg1EiaIJLlLVxbb4til3jmh+wkf6o3R2KS
V65+f4F6XZvA5vsB7rUv7HDvuSNNGVdtFSQ9RxnzKOSyemypSM+CPQyDNcWJZSjd
51o58wEEv7jQrvu5NmeggXMiFDR3AgMBAAGjfDB6MAkGA1UdEwQCMAAwHwYDVR0j
BBgwFoAUxRkU5BydYIvZdbTEsI6oOFnvAuIwHQYDVR0OBBYEFMUZFOQcnWCL2XW0
xLCOqDhZ7wLiMA4GA1UdDwEB/wQEAwIFoDAdBgNVHSUEFjAUBggrBgEFBQcDAQYI
KwYBBQUHAwIwDQYJKoZIhvcNAQELBQADggEBAFd9ujf8VlE2UxAqjTmaAddX/FKU
NHSWILGSjOZm6Lb2nz0267Al8G71NiLdwDAPEH7sBwKUIXZBKoLJU9pBxSshxkf0
lX+4bKUvHkiDf/GDg9X1RqX7sVgglPaR3FmQNxEvbs6lD+rWar6ZtHOX2Kaqrb/7
szao5Hnoo7U8CL4Lm0ZDZ+nxP9gwq3W83KDLsGHHNld0i9zl55Lzt1vCoUsqJlsJ
XPyDaHxT4m7lrI+fv8HAC2KclNkEf9xYTqDfwHpbWKQtV54gYG568D+kyptHDsgv
7qiJtBmvKf+O4g7p9NPjcQdfoHq2IrKzZO9kOANlcVE0UCXBk4Msi4VguJ0=
-----END CERTIFICATE-----";

		#endregion

		#region TestSerialization

		static string FormatOneLine(string s) => s.Replace("\r", "").Replace("\n", "").Replace("\t", "");

		string FakeEncrypt(string s)
		{
			return string.IsNullOrEmpty(s) ? s : $"{s}Encrypted";
		}

		string FakeDecrypt(string s)
		{
			AssertEndsWith($"Invalid encrypted message {s}", "Encrypted", s);
			return s.Substring(0, s.Length - 9);
		}

		string GeneratePrivateKey()
		{
			using var rsa = new RSACryptoServiceProvider(2048);
			var privateKeyBytes = rsa.ExportCspBlob(true);
			var base64PrivateKey = Convert.ToBase64String(privateKeyBytes);
			var sb = new StringBuilder();
			sb.AppendLine("-----BEGIN RSA PRIVATE KEY-----");
			for (var i = 0; i < base64PrivateKey.Length; i += 64)
			{
				sb.AppendLine(base64PrivateKey.Substring(i, Math.Min(64, base64PrivateKey.Length - i)));
			}
			sb.AppendLine("-----END RSA PRIVATE KEY-----");
			return FormatOneLine(sb.ToString());
		}

		public void TestReadElements_BackwardCompatibility1()
		{
			var legacyPrivateKey = GeneratePrivateKey();
			var rolloverPrivateKey = GeneratePrivateKey();
			var testXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
<SystemToSystemTrustInfo>
	<Certificate>{testStringCertificate}</Certificate>
	<PrivateKey>{testRsaKey}</PrivateKey>
	<LegacyPrivateKey>{legacyPrivateKey}</LegacyPrivateKey>
	<RolloverPrivateKey>{rolloverPrivateKey}</RolloverPrivateKey>
	<LegacyCertificate />
	<CSR>{testCsr}</CSR>
	<ClientId>{testClientId}</ClientId>
	<TenantId>{testTenantId}</TenantId>
	<OperationId>{testOperationId}</OperationId>
</SystemToSystemTrustInfo>";

#pragma warning disable CS0618 // Type or member is obsolete: bypass for unit test
			var expectedSystemToSystemTrustInfo = new SystemToSystemTrustInfo
			{
				Certificate = testByteCertificate,
				PrivateKey = testRsaKey,
				LegacyPrivateKey = legacyPrivateKey,
				RolloverPrivateKey = rolloverPrivateKey,
				LegacyCertificate = ZBlob.Empty,
				CertificateSigningRequest = testCsr,
				ClientId = testClientId,
				TenantId = testTenantId,
				OperationId = testOperationId
			};
#pragma warning restore CS0618 // Type or member is obsolete: bypass for unit test
			var expectedEncryptedKeys = Array.Empty<string>();
			TestReadElements(testXml, expectedSystemToSystemTrustInfo, expectedEncryptedKeys);
		}

		public void TestReadElements_SimpleValues()
		{
			var certificate = ZBlob.FromAscii("Certificate");
			var legacyCertificate = ZBlob.FromAscii("Legacy Certificate");
			const string certificateSigningRequest = "Certificate Signing Request";
			const string privateKey = "Private Key";
			const string legacyPrivateKey = "Legacy Private Key";
			const string rolloverPrivateKey = "Rollover Private Key";
			var testXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
<SystemToSystemTrustInfo>
	<Certificate>{Convert.ToBase64String(certificate)}</Certificate>
	<PrivateKey>{privateKey}</PrivateKey>
	<PrivateKeyEncrypted>{FakeEncrypt(privateKey)}</PrivateKeyEncrypted>
	<LegacyPrivateKey>{legacyPrivateKey}</LegacyPrivateKey>
	<LegacyPrivateKeyEncrypted>{FakeEncrypt(legacyPrivateKey)}</LegacyPrivateKeyEncrypted>
	<RolloverPrivateKey>{rolloverPrivateKey}</RolloverPrivateKey>
	<RolloverPrivateKeyEncrypted>{FakeEncrypt(rolloverPrivateKey)}</RolloverPrivateKeyEncrypted>
	<LegacyCertificate>{Convert.ToBase64String(legacyCertificate)}</LegacyCertificate>
	<CSR>{certificateSigningRequest}</CSR>
	<ClientId>{testClientId}</ClientId>
	<TenantId>{testTenantId}</TenantId>
	<OperationId>{testOperationId}</OperationId>
</SystemToSystemTrustInfo>";
#pragma warning disable CS0618 // Type or member is obsolete: bypass for unit test
			var expectedSystemToSystemTrustInfo = new SystemToSystemTrustInfo
			{
				Certificate = certificate,
				PrivateKey = privateKey,
				LegacyPrivateKey = legacyPrivateKey,
				RolloverPrivateKey = rolloverPrivateKey,
				LegacyCertificate = legacyCertificate,
				CertificateSigningRequest = certificateSigningRequest,
				ClientId = testClientId,
				TenantId = testTenantId,
				OperationId = testOperationId
			};
#pragma warning restore CS0618 // Type or member is obsolete: bypass for unit test
			var expectedEncryptedKeys = new[] { privateKey, legacyPrivateKey, rolloverPrivateKey };
			TestReadElements(testXml, expectedSystemToSystemTrustInfo, expectedEncryptedKeys);
		}

		public void TestWriteElements_SimpleValues()
		{
			var certificate = ZBlob.FromAscii("Certificate");
			var legacyCertificate = ZBlob.FromAscii("Legacy Certificate");
			const string certificateSigningRequest = "Certificate Signing Request";
			const string privateKey = "Private Key";
			const string legacyPrivateKey = "Legacy Private Key";
			const string rolloverPrivateKey = "Rollover Private Key";
#pragma warning disable CS0618 // Type or member is obsolete: bypass for unit test
			var info = new SystemToSystemTrustInfo(mockDataProtectorServiceFactory.Object)
			{
				Certificate = certificate,
				PrivateKey = privateKey,
				LegacyPrivateKey = legacyPrivateKey,
				RolloverPrivateKey = rolloverPrivateKey,
				LegacyCertificate = legacyCertificate,
				CertificateSigningRequest = certificateSigningRequest,
				ClientId = testClientId,
				TenantId = testTenantId,
				OperationId = testOperationId
			};
#pragma warning restore CS0618 // Type or member is obsolete: bypass for unit test
			var expectedXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
<SystemToSystemTrustInfo>
	<Certificate>{Convert.ToBase64String(certificate)}</Certificate>
	<PrivateKey>{privateKey}</PrivateKey>
	<PrivateKeyEncrypted>{FakeEncrypt(privateKey)}</PrivateKeyEncrypted>
	<LegacyPrivateKey>{legacyPrivateKey}</LegacyPrivateKey>
	<LegacyPrivateKeyEncrypted>{FakeEncrypt(legacyPrivateKey)}</LegacyPrivateKeyEncrypted>
	<RolloverPrivateKey>{rolloverPrivateKey}</RolloverPrivateKey>
	<RolloverPrivateKeyEncrypted>{FakeEncrypt(rolloverPrivateKey)}</RolloverPrivateKeyEncrypted>
	<LegacyCertificate>{Convert.ToBase64String(legacyCertificate)}</LegacyCertificate>
	<CSR>{certificateSigningRequest}</CSR>
	<ClientId>{testClientId}</ClientId>
	<TenantId>{testTenantId}</TenantId>
	<OperationId>{testOperationId}</OperationId>
</SystemToSystemTrustInfo>";
			var expectedKeysToEncrypt = new[] { privateKey, legacyPrivateKey, rolloverPrivateKey };
			TestWriteElements(info, expectedXml, expectedKeysToEncrypt);
		}

		public void TestReadElements_WithOnlyPrivateKey()
		{
			const string privateKey = "Private Key";
			var testXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
<SystemToSystemTrustInfo>
	<Certificate>{testStringCertificate}</Certificate>
	<PrivateKey>{privateKey}</PrivateKey>
	<PrivateKeyEncrypted>{FakeEncrypt(privateKey)}</PrivateKeyEncrypted>
	<LegacyPrivateKey />
	<LegacyPrivateKeyEncrypted />
	<RolloverPrivateKey />
	<RolloverPrivateKeyEncrypted />
	<LegacyCertificate />
	<CSR>{testCsr}</CSR>
	<ClientId>{testClientId}</ClientId>
	<TenantId>{testTenantId}</TenantId>
	<OperationId>{testOperationId}</OperationId>
</SystemToSystemTrustInfo>";
#pragma warning disable CS0618 // Type or member is obsolete: bypass for unit test
			var expectedSystemToSystemTrustInfo = new SystemToSystemTrustInfo
			{
				Certificate = testByteCertificate,
				PrivateKey = privateKey,
				LegacyPrivateKey = string.Empty,
				RolloverPrivateKey = string.Empty,
				LegacyCertificate = ZBlob.Empty,
				CertificateSigningRequest = testCsr,
				ClientId = testClientId,
				TenantId = testTenantId,
				OperationId = testOperationId
			};
#pragma warning restore CS0618 // Type or member is obsolete: bypass for unit test
			var expectedEncryptedKeys = new[] { privateKey };
			TestReadElements(testXml, expectedSystemToSystemTrustInfo, expectedEncryptedKeys);
		}

		public void TestWriteElements_WithOnlyPrivateKey()
		{
			const string privateKey = "Private Key";
#pragma warning disable CS0618 // Type or member is obsolete: bypass for unit test
			var info = new SystemToSystemTrustInfo(mockDataProtectorServiceFactory.Object)
			{
				Certificate = testByteCertificate,
				PrivateKey = privateKey,
				LegacyPrivateKey = string.Empty,
				RolloverPrivateKey = string.Empty,
				LegacyCertificate = ZBlob.Empty,
				CertificateSigningRequest = testCsr,
				ClientId = testClientId,
				TenantId = testTenantId,
				OperationId = testOperationId
			};
#pragma warning restore CS0618 // Type or member is obsolete: bypass for unit test
			var expectedXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
<SystemToSystemTrustInfo>
	<Certificate>{testStringCertificate}</Certificate>
	<PrivateKey>{privateKey}</PrivateKey>
	<PrivateKeyEncrypted>{FakeEncrypt(privateKey)}</PrivateKeyEncrypted>
	<LegacyPrivateKey />
	<LegacyPrivateKeyEncrypted />
	<RolloverPrivateKey />
	<RolloverPrivateKeyEncrypted />
	<LegacyCertificate />
	<CSR>{testCsr}</CSR>
	<ClientId>{testClientId}</ClientId>
	<TenantId>{testTenantId}</TenantId>
	<OperationId>{testOperationId}</OperationId>
</SystemToSystemTrustInfo>";
			var expectedKeysToEncrypt = new[] { privateKey };
			TestWriteElements(info, expectedXml, expectedKeysToEncrypt);
		}

		void TestReadElements(string testXml, SystemToSystemTrustInfo expectedSystemToSystemTrustInfo, string[] expectedEncryptedKeys)
		{
			using (var stringReader = new StringReader(testXml))
			using (var xmlReader = XmlReader.Create(stringReader))
			{
				var info = new SystemToSystemTrustInfo(mockDataProtectorServiceFactory.Object);
				xmlReader.ReadStartElement();
				((IXmlSerializable)info).ReadXml(xmlReader);
				mockDataProtectorServiceFactory.Verify(x => x.Create(), Times.Never);
#pragma warning disable CS0618 // Type or member is obsolete: bypass for unit test
				AssertEquals(expectedSystemToSystemTrustInfo.Certificate, info.Certificate);
				AssertEquals(expectedSystemToSystemTrustInfo.LegacyCertificate, info.LegacyCertificate);
				AssertEquals(expectedSystemToSystemTrustInfo.PrivateKey, info.PrivateKey);
				AssertEquals(expectedSystemToSystemTrustInfo.RolloverPrivateKey, info.RolloverPrivateKey);
				AssertEquals(expectedSystemToSystemTrustInfo.LegacyPrivateKey, info.LegacyPrivateKey);
				AssertEquals(expectedSystemToSystemTrustInfo.CertificateSigningRequest, info.CertificateSigningRequest);
				AssertEquals(expectedSystemToSystemTrustInfo.ClientId, info.ClientId);
				AssertEquals(expectedSystemToSystemTrustInfo.TenantId, info.TenantId);
				AssertEquals(expectedSystemToSystemTrustInfo.OperationId, info.OperationId);
#pragma warning restore CS0618 // Type or member is obsolete: bypass for unit test
			}

			AssertContainsExactElementsInAnyOrder(expectedEncryptedKeys.Select(FakeEncrypt), capturedEncryptedKeys);
			mockDataProtectorService.Verify(m => m.Decrypt(It.IsAny<string>()), Times.Exactly(expectedEncryptedKeys.Length));
			mockDataProtectorServiceFactory.Verify(x => x.Create(), Times.Exactly(expectedEncryptedKeys.Length == 0 ? 0 : 1));
		}

		void TestWriteElements(SystemToSystemTrustInfo info, string expectedXml, string[] expectedKeysToEncrypt)
		{
			using (var stream = new StringWriter())
			using (var writer = new XmlTextWriter(stream))
			{
				writer.WriteStartDocument();
				writer.WriteStartElement("SystemToSystemTrustInfo");
				((IXmlSerializable)info).WriteXml(writer);
				writer.WriteEndElement();
				writer.Flush();

				var result = stream.ToString();
				var actualDoc = XDocument.Parse(result);
				var expectedDoc = XDocument.Parse(expectedXml);
				CombineAssertions(() =>
				{
					AssertContainsExactLinesInExactOrder(expectedDoc.ToString(), actualDoc.ToString());
					AssertNotContains("\r", result);
					AssertNotContains("\n", result);
					AssertNotContains("\t", result);
				});
			}

			AssertContainsExactElementsInAnyOrder(expectedKeysToEncrypt, capturedKeys);
			mockDataProtectorService.Verify(m => m.Encrypt(It.IsAny<string>()), Times.Exactly(expectedKeysToEncrypt.Length));
			mockDataProtectorServiceFactory.Verify(x => x.Create(), Times.Once);
		}

		#endregion

		#region TestEncryptionAndDecryption

#pragma warning disable CS0618 // Type or member is obsolete: bypass for unit test
		public void TestEncryptionDecryption_PrivateKey()
		{
			TestEncryptionDecryption(info => info.PrivateKey, (info, value) => info.PrivateKey = value);
		}

		public void TestEncryptionDecryption_LegacyPrivateKey()
		{
			TestEncryptionDecryption(info => info.LegacyPrivateKey, (info, value) => info.LegacyPrivateKey = value);
		}

		public void TestEncryptionDecryption_RolloverPrivateKey()
		{
			TestEncryptionDecryption(info => info.RolloverPrivateKey, (info, value) => info.RolloverPrivateKey = value);
		}

		public void TestEncryptionDecryption_ResetValue_PrivateKey()
		{
			TestEncryptionDecryption_ResetValue(info => info.PrivateKey, (info, value) => info.PrivateKey = value);
		}

		public void TestEncryptionDecryption_ResetValue_LegacyPrivateKey()
		{
			TestEncryptionDecryption_ResetValue(info => info.LegacyPrivateKey, (info, value) => info.LegacyPrivateKey = value);
		}

		public void TestEncryptionDecryption_ResetValue_RolloverPrivateKey()
		{
			TestEncryptionDecryption_ResetValue(info => info.RolloverPrivateKey, (info, value) => info.RolloverPrivateKey = value);
		}

		public void TestEncryptionDecryption_EncryptionFailure_PrivateKey()
		{
			var error = TestEncryption_EncryptionFailure(info => info.PrivateKey, (info, value) => info.PrivateKey = value);
			AssertEquals("Encryption failed for PrivateKey value.", error);
		}

		public void TestEncryptionDecryption_EncryptionFailure_LegacyPrivateKey()
		{
			var error = TestEncryption_EncryptionFailure(info => info.LegacyPrivateKey, (info, value) => info.LegacyPrivateKey = value);
			AssertEquals("Encryption failed for LegacyPrivateKey value.", error);
		}

		public void TestEncryptionDecryption_EncryptionFailure_RolloverPrivateKey()
		{
			var error = TestEncryption_EncryptionFailure(info => info.RolloverPrivateKey, (info, value) => info.RolloverPrivateKey = value);
			AssertEquals("Encryption failed for RolloverPrivateKey value.", error);
		}

		public void TestEncryptionDecryption_DecryptionFailure_PrivateKey()
		{
			var error = TestEncryptionDecryption_DecryptionFailure(info => info.PrivateKey, (info, value) => info.PrivateKey = value);
			AssertEquals("Decryption failed for PrivateKey value.", error);
		}

		public void TestEncryptionDecryption_DecryptionFailure_LegacyPrivateKey()
		{
			var error = TestEncryptionDecryption_DecryptionFailure(info => info.LegacyPrivateKey, (info, value) => info.LegacyPrivateKey = value);
			AssertEquals("Decryption failed for LegacyPrivateKey value.", error);
		}

		public void TestEncryptionDecryption_DecryptionFailure_RolloverPrivateKey()
		{
			var error = TestEncryptionDecryption_DecryptionFailure(info => info.RolloverPrivateKey, (info, value) => info.RolloverPrivateKey = value);
			AssertEquals("Decryption failed for RolloverPrivateKey value.", error);
		}
#pragma warning restore CS0618 // Type or member is obsolete: bypass for unit test

		void TestEncryptionDecryption(Func<SystemToSystemTrustInfo, string> getter, Action<SystemToSystemTrustInfo, string> setter)
		{
			var info = new SystemToSystemTrustInfo(mockDataProtectorServiceFactory.Object);
			var testData = Guid.NewGuid().ToString();
			setter(info, testData);
			mockDataProtectorService.Verify(x => x.Encrypt(testData), Times.Once);
			AssertEquals(testData, getter(info));
			mockDataProtectorService.Verify(x => x.Decrypt(FakeEncrypt(testData)), Times.Once);
		}

		void TestEncryptionDecryption_ResetValue(Func<SystemToSystemTrustInfo, string> getter, Action<SystemToSystemTrustInfo, string> setter)
		{
			var info = GetBusinessObjectToSerialise() as SystemToSystemTrustInfo;
			AssertNotNullOrEmpty(getter(info));
			mockDataProtectorService.Invocations.Clear();
			setter(info, string.Empty);
			AssertEquals(string.Empty, getter(info));
			setter(info, null);
			AssertNullOrEmpty(getter(info));
			mockDataProtectorServiceFactory.Verify(x => x.Create(), Times.Once);
		}

		string TestEncryption_EncryptionFailure(Func<SystemToSystemTrustInfo, string> getter, Action<SystemToSystemTrustInfo, string> setter)
		{
			mockDataProtectorService
				.Setup(x => x.Encrypt(Capture.In(capturedKeys)))
				.Throws(new Exception("Encryption failed"));
			var info = new SystemToSystemTrustInfo(mockDataProtectorServiceFactory.Object);
			var testData = Guid.NewGuid().ToString();
			try
			{
				setter(info, testData);
				AssertEquals(testData, getter(info));
				mockDataProtectorService.Verify(x => x.Encrypt(testData), Times.Once);
				var error = ErrorReporter.LastMessageReported;
				var exception = ErrorReporter.LastExceptionReported;
				AssertEquals("Encryption failed", exception?.Message);
				return error;
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		string TestEncryptionDecryption_DecryptionFailure(Func<SystemToSystemTrustInfo, string> getter, Action<SystemToSystemTrustInfo, string> setter)
		{
			mockDataProtectorService
				.Setup(x => x.Decrypt(Capture.In(capturedEncryptedKeys)))
				.Throws(new Exception("Decryption failed"));
			var info = new SystemToSystemTrustInfo(mockDataProtectorServiceFactory.Object);
			var testData = Guid.NewGuid().ToString();
			try
			{
				setter(info, testData);
				AssertNullOrEmpty(ErrorReporter.LastMessageReported);
				mockDataProtectorService.Verify(x => x.Encrypt(testData), Times.Once);
				AssertEquals(testData, getter(info));
				mockDataProtectorService.Verify(x => x.Decrypt(FakeEncrypt(testData)), Times.Once);
				var error = ErrorReporter.LastMessageReported;
				var exception = ErrorReporter.LastExceptionReported;
				AssertEquals("Decryption failed", exception?.Message);
				return error;
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}
		#endregion

		public void TestConstructor_DoNotCallFactory()
		{
			var info = new SystemToSystemTrustInfo(mockDataProtectorServiceFactory.Object);
			AssertNotNull(info);
			mockDataProtectorServiceFactory.Verify(x => x.Create(), Times.Never);
		}

		public void TestGetCertificateProperties_InvalidCertificate()
		{
			var info = new SystemToSystemTrustInfo(mockDataProtectorServiceFactory.Object)
			{
				Certificate = testByteCertificate
			};

			CombineAssertions(() =>
			{
				AssertEquals(null, info.CertificateThumbprint);
				AssertEquals(ZDateTime.Empty, info.CertificateValidFrom);
				AssertEquals(ZDateTime.Empty, info.CertificateValidTo);

				AssertEquals("SystemToSystemTrustInfo_X509Certificate", ErrorReporter.LastKeyReported);
			});

			ErrorReporter.Clear();
		}

		public void TestGetCertificateProperties_EmptyCertificate()
		{
			var info = new SystemToSystemTrustInfo(mockDataProtectorServiceFactory.Object) { };

			CombineAssertions(() =>
			{
				AssertEquals(null, info.CertificateThumbprint);
				AssertEquals(ZDateTime.Empty, info.CertificateValidFrom);
				AssertEquals(ZDateTime.Empty, info.CertificateValidTo);
			});
		}

		public void TestGetCertificateProperties_ValidCertificate()
		{
			var x509Certificate = new X509Certificate2(Encoding.UTF8.GetBytes(Certificate));
			var info = new SystemToSystemTrustInfo(mockDataProtectorServiceFactory.Object)
			{
				Certificate = Encoding.UTF8.GetBytes(Certificate)
			};

			CombineAssertions(() =>
			{
				AssertEquals("0153A7C4B112DE00D8E50756C5249D5789C64B03", info.CertificateThumbprint);
				AssertEquals(new ZDateTime(x509Certificate.NotBefore), info.CertificateValidFrom);
				AssertEquals(new ZDateTime(x509Certificate.NotAfter), info.CertificateValidTo);
			});
		}

		#region TestIsSetUp

		public void TestIsSetUp_AfterDefaultConstructor_ReturnsFalse()
		{
			var info = new SystemToSystemTrustInfo(mockDataProtectorServiceFactory.Object);
			AssertEquals(false, info.IsSetUp());
		}

		public void TestIsSetUp_WhenPropertiesSet_ReturnsTrue()
		{
#pragma warning disable CS0618 // Type or member is obsolete: bypass for unit test
			var info = new SystemToSystemTrustInfo
			{
				Certificate = testByteCertificate,
				ClientId = testClientId,
				PrivateKey = "Private Key",
				TenantId = testTenantId,
				LegacyPrivateKey = string.Empty,
				RolloverPrivateKey = string.Empty,
				LegacyCertificate = ZBlob.Empty,
				CertificateSigningRequest = string.Empty,
				OperationId = string.Empty,
			};
#pragma warning restore CS0618 // Type or member is obsolete: bypass for unit test
			AssertEquals(true, info.IsSetUp());
		}

		public void TestIsSetUp_WhenCertificateNotSet_ReturnsFalse()
		{
#pragma warning disable CS0618 // Type or member is obsolete: bypass for unit test
			var info = new SystemToSystemTrustInfo
			{
				Certificate = ZBlob.Empty,
				ClientId = testClientId,
				PrivateKey = "Private Key",
				TenantId = testTenantId,
				LegacyPrivateKey = "Legacy Private Key",
				RolloverPrivateKey = "Rollover Private Key",
				LegacyCertificate = ZBlob.FromAscii("Legacy Certificate"),
				CertificateSigningRequest = "Certificate Signing Request",
				OperationId = testOperationId
			};
#pragma warning restore CS0618 // Type or member is obsolete: bypass for unit test
			AssertEquals(false, info.IsSetUp());
		}

		public void TestIsSetUp_WhenClientIdNotSet_ReturnsFalse()
		{
#pragma warning disable CS0618 // Type or member is obsolete: bypass for unit test
			var info = new SystemToSystemTrustInfo
			{
				Certificate = testByteCertificate,
				ClientId = string.Empty,
				PrivateKey = "Private Key",
				TenantId = testTenantId,
				LegacyPrivateKey = "Legacy Private Key",
				RolloverPrivateKey = "Rollover Private Key",
				LegacyCertificate = ZBlob.FromAscii("Legacy Certificate"),
				CertificateSigningRequest = "Certificate Signing Request",
				OperationId = testOperationId
			};
#pragma warning restore CS0618 // Type or member is obsolete: bypass for unit test
			AssertEquals(false, info.IsSetUp());
		}

		public void TestIsSetUp_WhenPrivateKeyNotSet_ReturnsFalse()
		{
#pragma warning disable CS0618 // Type or member is obsolete: bypass for unit test
			var info = new SystemToSystemTrustInfo
			{
				Certificate = testByteCertificate,
				ClientId = testClientId,
				PrivateKey = string.Empty,
				TenantId = testTenantId,
				LegacyPrivateKey = "Legacy Private Key",
				RolloverPrivateKey = "Rollover Private Key",
				LegacyCertificate = ZBlob.FromAscii("Legacy Certificate"),
				CertificateSigningRequest = "Certificate Signing Request",
				OperationId = testOperationId
			};
#pragma warning restore CS0618 // Type or member is obsolete: bypass for unit test
			AssertEquals(false, info.IsSetUp());
		}

		public void TestIsSetUp_WhenTenantIdNotSet_ReturnsFalse()
		{
#pragma warning disable CS0618 // Type or member is obsolete: bypass for unit test
			var info = new SystemToSystemTrustInfo
			{
				Certificate = testByteCertificate,
				ClientId = testClientId,
				PrivateKey = "Private Key",
				TenantId = string.Empty,
				LegacyPrivateKey = "Legacy Private Key",
				RolloverPrivateKey = "Rollover Private Key",
				LegacyCertificate = ZBlob.FromAscii("Legacy Certificate"),
				CertificateSigningRequest = "Certificate Signing Request",
				OperationId = testOperationId
			};
#pragma warning restore CS0618 // Type or member is obsolete: bypass for unit test
			AssertEquals(false, info.IsSetUp());
		}

		public void TestIsSetUp_WhenPropertiesSetToNull_ReturnsFalse()
		{
#pragma warning disable CS0618 // Type or member is obsolete: bypass for unit test
			var info = new SystemToSystemTrustInfo
			{
				Certificate = null,
				PrivateKey = null,
				LegacyPrivateKey = null,
				RolloverPrivateKey = null,
				LegacyCertificate = null,
				CertificateSigningRequest = null,
				ClientId = null,
				TenantId = null,
				OperationId = null,
			};
#pragma warning restore CS0618 // Type or member is obsolete: bypass for unit test
			AssertEquals(false, info.IsSetUp());
		}

		#endregion

		#region Overrides

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
#pragma warning disable CS0618 // Type or member is obsolete: bypass for unit test
			var obj = new SystemToSystemTrustInfo(mockDataProtectorServiceFactory.Object)
			{
				Certificate = ZBlob.FromAscii("Certificate"),
				PrivateKey = testRsaKey,
				LegacyPrivateKey = GeneratePrivateKey(),
				RolloverPrivateKey = GeneratePrivateKey(),
				LegacyCertificate = ZBlob.FromAscii("Legacy Certificate"),
				CertificateSigningRequest = testCsr,
				ClientId = testClientId,
				TenantId = testTenantId,
				OperationId = testOperationId
			};
#pragma warning restore CS0618 // Type or member is obsolete: bypass for unit test
			mockDataProtectorService.Invocations.Clear();
			return obj;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override void SetUp()
		{
			base.SetUp();
			mockDataProtectorService = new Mock<IDataProtectorService>(MockBehavior.Strict);
			capturedEncryptedKeys = new List<string>();
			mockDataProtectorService
				.Setup(x => x.Decrypt(Capture.In(capturedEncryptedKeys)))
				.Returns(FakeDecrypt)
				.Verifiable();

			capturedKeys = new List<string>();
			mockDataProtectorService
				.Setup(x => x.Encrypt(Capture.In(capturedKeys)))
				.Returns(FakeEncrypt)
				.Verifiable();

			mockDataProtectorServiceFactory = new Mock<IDataProtectorServiceFactory>(MockBehavior.Strict);
			mockDataProtectorServiceFactory
				.Setup(x => x.Create())
				.Returns(mockDataProtectorService.Object)
				.Verifiable();
		}

		protected override void TearDown()
		{
			mockDataProtectorServiceFactory?.Verify(x => x.Create(), Times.AtMostOnce);
			mockDataProtectorServiceFactory?.VerifyNoOtherCalls();
			mockDataProtectorService?.VerifyNoOtherCalls();
			base.TearDown();
		}

		#endregion
	}
}
