using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CertificateManagerTest : TestCaseWithFactory
	{
		[TestDate(2023, 08, 16, 0, 29, 59)]
		public void TestGetCustomsCertificate()
		{
			var helper = new CertificateManagerHelper(Factory);
			var cryptCrtData = helper.GetEmbeddedFileData(CertificateManagerHelper.AUCryptCrt2023);
			helper.CreateCertificate(AUConstants.RefSysConfigCodes.AUCryptCrt, cryptCrtData, new ZDateTime(2021, 10, 20), new ZDateTime(2023, 08, 16, 0, 29, 59));
			var newCryptCrtData = helper.GetEmbeddedFileData(CertificateManagerHelper.AUCryptCrt2025);
			helper.CreateCertificate(AUConstants.RefSysConfigCodes.AUCryptCrtNew, newCryptCrtData, new ZDateTime(2023, 08, 16, 0, 30, 00), ZDateTime.MaxSmallDateTime);
			Factory.Save();

			using (var certificateManager = new CertificateManagerForTest(Factory))
			{
				AssertEquals("CurrentValue", cryptCrtData, certificateManager.GetCustomsCertificateDataExposed());

				TestDateAttribute.AddSeconds(1);
				AssertEquals("New Value", newCryptCrtData, certificateManager.GetCustomsCertificateDataExposed());

				var certificate = certificateManager.CustomsCertificate;
				AssertEquals("CCF E-mail Gateway", certificate.Name);
			}
		}

		[TestDate(2021, 09, 01)]
		public void TestGetCustomsCertificate_BadData()
		{
			var helper = new CertificateManagerHelper(Factory);
			var badTrustCrtData = helper.GetEmbeddedFileData(CertificateManagerHelper.AUCompanyCrt);
			helper.CreateCertificate(AUConstants.RefSysConfigCodes.AUCryptCrt, badTrustCrtData, ZDateTime.Today, ZDateTime.Today.AddDays(1));
			Factory.Save();

			using (var certificateManager = new CertificateManagerForTest(Factory))
			{
				var badCertificate = certificateManager.CustomsCertificate;
				AssertNull(badCertificate);
				AssertContains("There was an error loading the Customs Certificate", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2021, 09, 01)]
		public void TestGetTrustPointCertificate()
		{
			var helper = new CertificateManagerHelper(Factory);
			var trustCrtData = helper.GetEmbeddedFileData(CertificateManagerHelper.AUTrustCrt2021);
			helper.CreateCertificate(AUConstants.RefSysConfigCodes.AUTrustCrt, trustCrtData, ZDateTime.Today.AddDays(-3), ZDateTime.Today);
			var newTrustCrtData = helper.GetEmbeddedFileData(CertificateManagerHelper.AUTrustCrt2023);
			helper.CreateCertificate(AUConstants.RefSysConfigCodes.AUTrustCrtNew, newTrustCrtData, ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(5));
			Factory.Save();

			using (var certificateManager = new CertificateManagerForTest(Factory))
			{
				AssertEquals("Current Value", trustCrtData, certificateManager.GetTrustPointCertificateDataExposed());

				TestDateAttribute.AddDays(1);
				AssertEquals("New Value", newTrustCrtData, certificateManager.GetTrustPointCertificateDataExposed());

				var certificate = certificateManager.TrustPointCertificate;
				AssertEquals("DigiCert Gatekeeper Device Issuing CA", certificate.Name);
			}
		}

		[TestDate(2021, 09, 01)]
		public void TestGetTrustPointCertificate_BadData()
		{
			var helper = new CertificateManagerHelper(Factory);
			var badTrustCrtData = helper.GetEmbeddedFileData(CertificateManagerHelper.AUCompanyCrt);
			helper.CreateCertificate(AUConstants.RefSysConfigCodes.AUTrustCrt, badTrustCrtData, ZDateTime.Today, ZDateTime.Today.AddDays(1));
			Factory.Save();

			using (var certificateManager = new CertificateManagerForTest(Factory))
			{
				var badCertificate = certificateManager.TrustPointCertificate;
				AssertNull(badCertificate);
				AssertContains("There was an error loading the Trust Point Certificate", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		sealed class CertificateManagerForTest : CertificateManager
		{
			public CertificateManagerForTest(BusinessObjectFactory factory) : base(factory)
			{
			}

			public byte[] GetCustomsCertificateDataExposed() => GetCustomsCertificateData();

			public byte[] GetTrustPointCertificateDataExposed() => GetTrustPointCertificateData();
		}
	}
}
