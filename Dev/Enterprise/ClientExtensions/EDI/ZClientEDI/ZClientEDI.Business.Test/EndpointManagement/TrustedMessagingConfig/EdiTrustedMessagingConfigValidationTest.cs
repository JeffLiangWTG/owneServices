namespace Enterprise.Client.EDI.TrustedMessaging.Business.Testing
{
	using CargoWise.EntityFramework.Testing;
	using CargoWise.IO;

	internal class EdiTrustedMessagingConfigValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckETM_Product()
		{
			var config = Factory.New<EdiTrustedMessagingConfig>();
			config.ETM_Product = "XXX";
			AssertHasError(config.ETM_ProductInfo, "Enter a valid selection.");
			config.ETM_Product = "";
			AssertHasError(config.ETM_ProductInfo, "Please enter a value.");
			config.ETM_Product = "CW1";
			AssertNoErrors(config.ETM_ProductInfo);
		}

		public void TestCheckETM_CertificateType()
		{
			var config = Factory.New<EdiTrustedMessagingConfig>();
			config.ETM_Product = "CW1";
			config.ETM_CertificateType = "CSC";
			Factory.Save();

			var config2 = Factory.New<EdiTrustedMessagingConfig>();
			config2.ETM_Product = "CW1";
			config2.ETM_CertificateType = "CSC";
			AssertHasError(config2.ETM_CertificateTypeInfo, "Another configuration with the same Product/System exists. There can only be one configuration valid at a time.");
			config2.ETM_CertificateType = "TSC";
			AssertNoErrors(config2.ETM_CertificateTypeInfo);
			Factory.Save();
		}

		public void TestValidateCertificateData()
		{
			var config = Factory.New<EdiTrustedMessagingConfig>();
			config.ETM_Product = "CW1";
			config.ETM_CertificateType = "CSC";

			config.RunPreSaveValidation();
			AssertHasRowError(config, "Please enter a Certificate.");

			config.ETM_CertificateData = new byte[] { 1, 2, 3 };
			config.RunPreSaveValidation();
			AssertHasRowError(config, "Invalid Certificate / Invalid Certificate Password.");

			config.ETM_CertificateData = LoadLocalCertAsBytes("Client.cer");
			config.RunPreSaveValidation();
			AssertHasRowError(config, "Certificate does not contain a private key.");

			config.ETM_CertificateData = LoadLocalCertAsBytes("Server.pfx");
			config.RunPreSaveValidation();
			AssertNoRowErrors(config);
		}

		byte[] LoadLocalCertAsBytes(string certFileName)
		{
			var assembly = System.Reflection.Assembly.GetExecutingAssembly();
			var resourceRetriever = new EmbeddedResourceRetriever(assembly);
			return resourceRetriever.GetBytes(@"ZClientEDI.Business.Test." + certFileName);
		}
	}
}
