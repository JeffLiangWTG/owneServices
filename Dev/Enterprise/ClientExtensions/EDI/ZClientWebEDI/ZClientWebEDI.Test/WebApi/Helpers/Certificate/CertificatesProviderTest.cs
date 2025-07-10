using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using WTG.TrustedMessaging;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class CertificatesProviderTest : TestCaseWithFactory
	{
		public void TestCertificates()
		{
			var licHeader1 = BillingTestHelper.CreateLicence(Factory, "ENT");
			licHeader1.Database.LD_Product = MultiTenantDatabaseProductTypeList.Codes.CSP;
			licHeader1.Database.LD_ETS_TrustedSystem = ZGuid.Empty;
			Factory.Save();
			var provider = ObjectFactory.New<ICertificatesProvider>("xxx");
			AssertNull(provider.LocalCertificate);
			AssertNull(provider.RemoteCertificate);
			provider = ObjectFactory.New<ICertificatesProvider>(MultiTenantDatabaseProductTypeList.Codes.CSP);
			AssertNull(provider.LocalCertificate);
			AssertNull(provider.RemoteCertificate);
			var config1 = Factory.New<EdiTrustedMessagingConfig>();
			config1.ETM_CertificateType = "CSC";
			config1.ETM_Product = MultiTenantDatabaseProductTypeList.Codes.CSP;
			config1.ETM_CertificateData = LoadLocalCertAsBytes("Server.pfx");
			var config2 = Factory.New<EdiTrustedMessagingConfig>();
			config2.ETM_CertificateType = "PDC";
			config2.ETM_Product = MultiTenantDatabaseProductTypeList.Codes.CSP;
			config2.ETM_CertificateData = LoadLocalCertAsBytes("Client.cer");
			Factory.Save();
			provider = ObjectFactory.New<ICertificatesProvider>(MultiTenantDatabaseProductTypeList.Codes.CSP);
			AssertNotNull(provider.LocalCertificate);
			AssertNotNull(provider.RemoteCertificate);
			var system = licHeader1.Database.GetOrCreateTrustedSystem();
			provider = ObjectFactory.New<ICertificatesProvider>(system);
			AssertNotNull(provider.LocalCertificate);
			AssertNull(provider.RemoteCertificate);
			licHeader1.Database.GetOrCreateTrustedSystem().GetOrCreateCertificateConfig().ETM_CertificateData = LoadLocalCertAsBytes("Client.cer");
			Factory.Save();
			provider = ObjectFactory.New<ICertificatesProvider>(system);
			AssertNotNull(provider.LocalCertificate);
			AssertNotNull(provider.RemoteCertificate);
			//Create productivity database
			//Create CW1 trusted system
			//Create CW1 central system certificate and remote certificate
			//Assert both certs available when loading from trusted system
			AssertExceptionThrown<ArgumentOutOfRangeException>(() => ObjectFactory.New<ICertificatesProvider>(new[] { 1 }));
		}

		public static byte[] LoadLocalCertAsBytes(string certFileName)
		{
			var resourceRetriever = new EmbeddedResourceRetriever(System.Reflection.Assembly.GetExecutingAssembly());
			return resourceRetriever.GetBytes(@"ZClientWebEDI.Test.TestFiles.Certificates." + certFileName);
		}
	}
}
