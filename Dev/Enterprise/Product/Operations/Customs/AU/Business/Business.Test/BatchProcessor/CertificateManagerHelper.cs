using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CertificateManagerHelper : Integration.Customs.AU.ICertificateManagerHelper
	{
		public CertificateManagerHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		public void RemoveCertificates()
		{
			factory.LoadFromNaturalKey<RefSysConfig>(RefSysConfigSchema.ZRC_ZRT_NKConfigCode, AUConstants.RefSysConfigCodes.AUCryptCrt)?.Delete();
			factory.LoadFromNaturalKey<RefSysConfig>(RefSysConfigSchema.ZRC_ZRT_NKConfigCode, AUConstants.RefSysConfigCodes.AUTrustCrt)?.Delete();
		}

		public void CreateCustomsCertificates()
		{
			CreateCustomsCertificates(ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		}

		public void CreateCustomsCertificates(ZDateTime startDate, ZDateTime endDate)
		{
			CreateCertificate(AUConstants.RefSysConfigCodes.AUTrustCrt, GetEmbeddedFileData(AUTrustCrt2023), startDate, endDate);
			CreateCertificate(AUConstants.RefSysConfigCodes.AUCryptCrt, GetEmbeddedFileData(AUCryptCrt2025), startDate, endDate);
		}

		public void CreateCustomsCertificates2004()
		{
			CreateCertificate(AUConstants.RefSysConfigCodes.AUTrustCrt, AUTrustCrt2004);
			CreateCertificate(AUConstants.RefSysConfigCodes.AUCryptCrt, AUCryptCrt2004);
		}

		public void CreateCustomsCertificates2005()
		{
			CreateCertificate(AUConstants.RefSysConfigCodes.AUTrustCrt, AUTrustCrt2005);
			CreateCertificate(AUConstants.RefSysConfigCodes.AUCryptCrt, AUCryptCrt2005);
		}

		public void CreateCustomsCertificates2012()
		{
			CreateCertificate(AUConstants.RefSysConfigCodes.AUTrustCrt, AUTrustCrt2012);
			CreateCertificate(AUConstants.RefSysConfigCodes.AUCryptCrt, AUCryptCrt2012);
		}

		/// <summary>
		/// Valid to Sep. 2021.
		/// </summary>
		public void CreateCustomsCertificates2021()
		{
			CreateCertificate(AUConstants.RefSysConfigCodes.AUTrustCrt, AUTrustCrt2021);
			CreateCertificate(AUConstants.RefSysConfigCodes.AUCryptCrt, AUCryptCrt2021);
		}

		public void CreateCustomsCertificates2023()
		{
			CreateCertificate(AUConstants.RefSysConfigCodes.AUTrustCrt, AUTrustCrt2023);
			CreateCertificate(AUConstants.RefSysConfigCodes.AUCryptCrt, AUCryptCrt2023);
		}

		public void CreateCustomsCertificates2025()
		{
			CreateCertificate(AUConstants.RefSysConfigCodes.AUTrustCrt, AUTrustCrt2023);
			CreateCertificate(AUConstants.RefSysConfigCodes.AUCryptCrt, AUCryptCrt2025);
		}

		[TestDate]
		public void SetupValidCompanyCertificatesForTest()
		{
			SetupValidCompanyCertificatesForTest(out _, out _, EnvProxy.Instance.CurrentCompany.PK);
		}

		[TestDate]
		public void SetupValidCompanyCertificatesForTest(out DateTime startDate, out DateTime endDate)
		{
			SetupValidCompanyCertificatesForTest(out startDate, out endDate, EnvProxy.Instance.CurrentCompany.PK);
		}

		public void SetupValidCompanyCertificatesForTest(out DateTime startDate, out DateTime endDate, Guid companyPK)
		{
			TestDateAttribute.Date = AUCCompanyCertificateStartDateForTest.AddDays(1);

			var rawRegistry = ZArchitecture.Environment.DataRegistry.Instance.RawRegistry;
			rawRegistry.AUCCompanyCertificateData.SetValue(companyPK, Guid.Empty, Guid.Empty, AUCCompanyCertificateDataForTest);
			rawRegistry.AUCCompanyCertificatePassword.SetValue(companyPK, Guid.Empty, Guid.Empty, AUCCompanyCertificatePasswordForTest);

			startDate = AUCCompanyCertificateStartDateForTest;
			endDate = AUCCompanyCertificateEndDateForTest;
		}

		void CreateCertificate(string code, string fileName)
		{
			var data = GetEmbeddedFileData(fileName);
			CreateCertificate(code, data, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		}

		public void CreateCertificate(string code, byte[] data, ZDateTime startDate, ZDateTime endDate)
		{
			if (!factory.Exists(typeof(RefSysConfigType), new ZQuery(RefSysConfigTypeSchema.ZRT_ConfigCode, code)))
			{
				DataHelper.CreateRefSysConfigType(code, "Certificate", "Certificate");
			}

			var certificateEntry = DataHelper.CreateRefSysConfig<string>(code, null, startDate, endDate);
			certificateEntry.ZRC_BinaryValue = data;
		}

		public byte[] GetEmbeddedFileData(string fileName) => FileReader.GetEmbeddedFileData(TestFilesPath, fileName);

		UniversalReferenceTestDataHelper DataHelper => dataHelper ?? (dataHelper = new UniversalReferenceTestDataHelper(factory));
		UniversalReferenceTestDataHelper dataHelper;

		TestFileReader FileReader => fileReader ?? (fileReader = new TestFileReader(typeof(CertificateManagerHelper)));
		TestFileReader fileReader;

		public const string AUCryptCrt2004 = "Conf.cer";
		public const string AUTrustCrt2004 = "acme_ca.crt";
		public const string AUCryptCrt2005 = "CCFEmailGateway20050212.cer";
		public const string AUTrustCrt2005 = "GatekeeperType3CA20130410.cer";
		public const string AUCryptCrt2012 = "CCFEmailGateway20120726.cer";
		public const string AUTrustCrt2012 = "GatekeeperType3CA20130410.cer";
		public const string AUCryptCrt2021 = "CCFEmailGateway20220125.cer";
		public const string AUTrustCrt2021 = "CACert.cer";
		public const string AUCryptCrt2023 = "CCFEmailGateway20230824.cer";
		public const string AUCryptCrt2025 = "CCFEmailGateway20250824.cer";
		public const string AUTrustCrt2023 = "DigiCertGatekeeperDeviceIssuingCA20341123.cer";

		public const string TestFilesPath = "Enterprise.Customs.AU.Declaration.Business.Testing.BatchProcessor.TestFiles.Certificates";

		/// <summary>
		/// Adjust these if a new company certificate(cmr_device_2023.pfx for now) is used for test
		/// </summary>
		public DateTime AUCCompanyCertificateStartDateForTest => new DateTime(2023, 6, 1, 8, 0, 0);
		public DateTime AUCCompanyCertificateEndDateForTest => new DateTime(2025, 7, 12, 7, 59, 59);
		public byte[] AUCCompanyCertificateDataForTest => GetEmbeddedFileData(AUCompanyCrt);
		public string AUCCompanyCertificatePasswordForTest => "C4rg0W1s3";

		public const string AUCompanyCrt = "cmr_device_2023.pfx";
	}
}
