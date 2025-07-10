using System.IO;
using CargoWise.Cryptoki.Common.ClientServerApi;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	public abstract class CryptokiCertificateProviderTest : TestCase
	{
		public abstract void TestGetCertificateList();

		public abstract void TestReadCertificate();

		public void TestParseCertificate()
		{
			var cryptokiCertificateProvider = new CryptokiCertificateProviderForTest();
			var cert = cryptokiCertificateProvider.ParseCertificateExposed("SomeChipset", ReadSampleCertificate());

			CombineAssertions(() =>
			{
				AssertEquals("Owner", "CN=İLKER PAKTEN, L=İSTANBUL, C=TR, SERIALNUMBER=20201224104", cert.Owner);
				AssertEquals("Issuer", "CN=TÜRKTRUST Nitelikli Elektronik Sertifika Hizmetleri H4, O=TÜRKTRUST Bilgi İletişim ve Bilişim Güvenliği Hizmetleri A.Ş., OU=Dayanak: T.C. 5070 sayılı Elektronik İmza Kanunu, C=TR", cert.Issuer);
				AssertEquals("Thumbprint", "87F0C1636306F5B274E2A3A129D43E2C1072EB6D", cert.Thumbprint);
				AssertEquals("SerialNumber", "02B9572B9CAD7250A906B3", cert.SerialNumber);
				AssertEquals("TokenManufacturerId", ZString.Empty, cert.TokenManufacturerId);
				AssertEquals("TokenModel", ZString.Empty, cert.TokenModel);
				AssertEquals("TokenChipset", "SomeChipset", cert.TokenChipset);

				// In TR time zone certificate is valid from 2020-02-20T15:43:33 to 2023-02-19T15:43:33
				// Asserting only Year/Month to cater for the test running in computers on different time zones.
				AssertEquals("NotBefore Year", 2020, cert.NotBefore.Year);
				AssertEquals("NotBefore Month", 2, cert.NotBefore.Month);
				AssertEquals("NotAfter Year", 2023, cert.NotAfter.Year);
				AssertEquals("NotAfter Month", 2, cert.NotAfter.Month);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			cryptokiCertificateProvider = CertificateHelper.GetNewCryptokiCertificateProvider(CerificateSourceToTest);
		}

		protected ICryptokiCertificateProvider cryptokiCertificateProvider;
		protected abstract string CerificateSourceToTest { get; }

		CertificateInfo ReadSampleCertificate()
		{
			var thisType = GetType();
			var fullResourcePath = thisType.Namespace + ".MasterFiles.Certificates.sample-certificate.crt";

			using (var stream = thisType.Assembly.GetManifestResourceStream(fullResourcePath))
			{
				var mem = new MemoryStream();
				stream.CopyTo(mem);
				return new CertificateInfo { Content = mem.ToArray() };
			}
		}

		class CryptokiCertificateProviderForTest : CryptokiCertificateProvider
		{
			public CryptokiCertificate ParseCertificateExposed(string chipset, CertificateInfo certificateInfo) => ParseCertificate(chipset, certificateInfo);

			protected override CertificateInfo GetCertificate(string chipset, byte[] serialNumber)
			{
				throw new System.NotImplementedException();
			}

			protected override CertificateInfo[] GetCertificates(string chipset)
			{
				throw new System.NotImplementedException();
			}
		}
	}
}
