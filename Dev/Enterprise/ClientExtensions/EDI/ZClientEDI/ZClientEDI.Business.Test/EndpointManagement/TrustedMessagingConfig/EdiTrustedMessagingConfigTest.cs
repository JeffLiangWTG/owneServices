namespace Enterprise.Client.EDI.TrustedMessaging.Business.Testing
{
	using System;
	using System.Data;
	using System.Net;
	using System.Security.Cryptography;
	using System.Security.Cryptography.X509Certificates;
	using CargoWise.EntityFramework;
	using CargoWise.IO;
	using Enterprise.Client.EDI.TrustedMessaging.Business;
	using Enterprise.ZArchitecture.Business.Testing;
	using NUnit.Framework;

	[TestedType(typeof(EdiTrustedMessagingConfig))]
	public class EdiTrustedMessagingConfigTest : EnterpriseBusinessObjectTestCase
	{
		public void TestUniqueIndex_ETM_CertificateType()
		{
			var c1 = Factory.New<EdiTrustedMessagingConfig>();
			c1.ETM_Product = "CW1";
			c1.ETM_CertificateType = "CSC";
			Factory.Save();

			var c2 = Factory.New<EdiTrustedMessagingConfig>();
			c2.ETM_Product = "CW1";
			c2.ETM_CertificateType = "TSC";
			Factory.Save();

			var c2a = Factory.New<EdiTrustedMessagingConfig>();
			c2a.ETM_Product = "CW1";
			c2a.ETM_CertificateType = "TSC";
			Factory.Save();

			try
			{
				var c3 = Factory.New<EdiTrustedMessagingConfig>();
				c3.ETM_Product = "CW1";
				c3.ETM_CertificateType = "CSC";
				Factory.Save();
				Fail("Should have ZSaveException");
			}
			catch (ZSaveException ex)
			{
				AssertContains($"Cannot insert duplicate key row in object 'dbo.EdiTrustedMessagingConfig' with unique index 'NR_UX__ETM_Product_ETM_CertificateType'", ex.Message);
			}
		}

		public void TestGetCertificate()
		{
			var c1 = Factory.New<EdiTrustedMessagingConfig>();
			c1.ETM_Product = "CW1";
			AssertNull(c1.GetCertificate());
			c1.ETM_CertificateData = LoadLocalCertAsBytes("Server.pfx");
			AssertNotNull(c1.GetCertificate());
			c1.ETM_CertificateData = LoadLocalCertAsBytes("Client.cer");
			AssertNotNull(c1.GetCertificate());
		}

		public void TestTryGenerateCertificate()
		{
			EDIDataRegistry.Instance.MyAccountCertificateAuthorityUserAccountPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "pwd");

			var c1 = Factory.New<EdiTrustedMessagingConfigForTest>();
			AssertEquals(true, c1.ETM_CertificateData.IsEmpty);
			AssertEquals(true, c1.ETM_CertificatePassword.IsEmpty);
			AssertNull(c1.GetCertificate());

			AssertEquals(true, c1.TryGenerateCertificate("subject1", out var output));
			AssertEquals(true, string.IsNullOrWhiteSpace(output));
			AssertEquals(false, c1.ETM_CertificateData.IsEmpty);
			AssertEquals(false, c1.ETM_CertificatePassword.IsEmpty);

			var cert = c1.GetCertificate();
			AssertNotNull(cert.GetRSAPrivateKey());
			AssertNotNull(cert.GetRSAPublicKey());
			AssertEquals("CN=subject1", cert.Subject);
		}

		public class EdiTrustedMessagingConfigForTest : EdiTrustedMessagingConfig
		{
			readonly RSACryptoServiceProvider RSAProvider = new RSACryptoServiceProvider(4096);

			public EdiTrustedMessagingConfigForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override IEDICertRequest GetCertRequest() => new EDICertRequestForTest(RSAProvider);

			protected override RSACryptoServiceProvider GetRASProvider() => RSAProvider;
		}

		class EDICertRequestForTest : IEDICertRequest
		{
			public EDICertRequestForTest(RSA rsa)
			{
				var req = new CertificateRequest($"CN=subject1", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
				using (var cert = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(5)))
				{
					Output = Convert.ToBase64String(cert.Export(X509ContentType.Cert));
				}
			}

			string Output { get; set; }

			public bool TrySubmitSafe(string soapRequestTemplate, string subjectName, byte[] keyBlob, ICredentials credentials, EdiCertRequestContext context, out string output)
			{
				output = Output;
				return true;
			}
		}

		public static byte[] LoadLocalCertAsBytes(string certFileName)
		{
			var resourceRetriever = new EmbeddedResourceRetriever(System.Reflection.Assembly.GetExecutingAssembly());
			return resourceRetriever.GetBytes(@"ZClientEDI.Business.Test." + certFileName);
		}
	}
}
