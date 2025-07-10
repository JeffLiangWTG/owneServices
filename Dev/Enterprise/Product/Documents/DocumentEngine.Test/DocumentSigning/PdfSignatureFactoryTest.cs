using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.DigitalSignature.DigitalSign;
using Enterprise.DocumentEngine.DigitalSignature.EMudhra.V1;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using FlexCel.Pdf;
using static Enterprise.DocumentEngineCore.Registry.DocumentSigningRegistryConstants;

namespace Enterprise.DocumentEngine.DigitalSignature.Testing
{
	sealed class PdfSignatureFactoryTest : TestCaseWithFactory
	{
		public void CheckSignedByLabel(ZString signinOption)
		{
			var systemLabel = "Jason the Signer";
			var companyLabel = "Brandon the Signer";

			var factory = new BusinessObjectFactory();

			var testCompany = factory.NewWithValidTestData<GlbCompany>();
			var testBranch = factory.NewWithValidTestData<GlbBranch>();
			testBranch.GB_GC = testCompany.PK;
			var testStaff = factory.NewWithValidTestData<GlbStaff>();
			testStaff.GS_GB_HomeBranch = testBranch.PK;

			var otherCompany = factory.NewWithValidTestData<GlbCompany>();
			var otherBranch = factory.NewWithValidTestData<GlbBranch>();
			otherBranch.GB_GC = otherCompany.PK;
			var otherStaff = factory.NewWithValidTestData<GlbStaff>();
			otherStaff.GS_GB_HomeBranch = otherBranch.PK;

			factory.Save();

			DocumentsDataRegistry.Instance.SigningServiceSignedByLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, systemLabel); // system default
			DocumentsDataRegistry.Instance.SigningServiceSignedByLabel.SetValue(testCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, companyLabel); // company specific

			var result = PdfSignatureFactory.NewPdfSignature(signinOption);

			AssertNotNull(result);

			AssertEquals($@"{Core.Constants.CompanyBrandingName} (WTG) has provided the feature to generate and sign this PDF document using the data verified by the application users and available in {Core.Constants.ProductName} at the time of signing. WTG or any of its employees shall not be held responsible for any discrepancies observed in the data contained in this document.", result.Reason);
			AssertEquals(systemLabel, result.Name);

			result = PdfSignatureFactory.NewPdfSignature(signinOption, testBranch.PK.ToGuid());
			AssertNotNull(result);
			AssertEquals($@"{Core.Constants.CompanyBrandingName} (WTG) has provided the feature to generate and sign this PDF document using the data verified by the application users and available in {Core.Constants.ProductName} at the time of signing. WTG or any of its employees shall not be held responsible for any discrepancies observed in the data contained in this document.", result.Reason);
			AssertEquals(companyLabel, result.Name);

			result = PdfSignatureFactory.NewPdfSignature(signinOption, otherBranch.PK.ToGuid());
			AssertNotNull(result);
			AssertEquals($@"{Core.Constants.CompanyBrandingName} (WTG) has provided the feature to generate and sign this PDF document using the data verified by the application users and available in {Core.Constants.ProductName} at the time of signing. WTG or any of its employees shall not be held responsible for any discrepancies observed in the data contained in this document.", result.Reason);
			AssertEquals(systemLabel, result.Name);
		}

		public void TestPlaceholder()
		{
			CheckSignedByLabel(PdfSigningOptionCodes.Placeholder);
		}

		public void TestEmudhra()
		{
			CheckSignedByLabel(PdfSigningOptionCodes.EmudhraV1);
		}

		public void TestCredentials_Signer()
		{
			DocumentsDataRegistry.Instance.DocumentSigningServicePartnerCredentials.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				new DocumentSigningServicePartnerCredentials() { PartnerID = "id", PartnerAccessKey = "key" });

			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var company2 = Factory.NewWithValidTestData<GlbCompany>();

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var branch3 = Factory.NewWithValidTestData<GlbBranch>();

			branch1.GB_GC = company1.PK;
			branch2.GB_GC = company1.PK;
			branch3.GB_GC = company2.PK;

			Factory.Save();

			var config1branch1 = new DocumentSigningServiceCredentialsWithProviderConfiguration() { ProviderCode = PdfSigningOptionCodes.EmudhraV1, AccessKey = "1", ClientID = "1", KeyID = "1" };
			var config2branch2 = new DocumentSigningServiceCredentialsWithProviderConfiguration() { ProviderCode = PdfSigningOptionCodes.EmudhraV1, AccessKey = "2", ClientID = "2", KeyID = "2" };
			var config3company1 = new DocumentSigningServiceCredentialsWithProviderConfiguration() { ProviderCode = PdfSigningOptionCodes.EmudhraV1, AccessKey = "3", ClientID = "3", KeyID = "3" };
			var config4company2 = new DocumentSigningServiceCredentialsWithProviderConfiguration() { ProviderCode = PdfSigningOptionCodes.EmudhraV1, AccessKey = "4", ClientID = "4", KeyID = "4" };

			DocumentsDataRegistry.Instance.DocumentSigningServiceCredentials.SetValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, config1branch1);
			DocumentsDataRegistry.Instance.DocumentSigningServiceCredentials.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, config3company1);
			DocumentsDataRegistry.Instance.DocumentSigningServiceCredentials.SetValue(Guid.Empty, branch2.PK.ToGuid(), Guid.Empty, config2branch2);
			DocumentsDataRegistry.Instance.DocumentSigningServiceCredentials.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, config4company2);

			var result1 = PdfSignatureFactory.NewPdfSignature(PdfSigningOptionCodes.EmudhraV1);
			var result2 = PdfSignatureFactory.NewPdfSignature(PdfSigningOptionCodes.EmudhraV1, company1.PK);
			var result3 = PdfSignatureFactory.NewPdfSignature(PdfSigningOptionCodes.EmudhraV1, branch1.PK);
			var result4 = PdfSignatureFactory.NewPdfSignature(PdfSigningOptionCodes.EmudhraV1, branch2.PK);
			var result5 = PdfSignatureFactory.NewPdfSignature(PdfSigningOptionCodes.EmudhraV1, company2.PK);
			var result6 = PdfSignatureFactory.NewPdfSignature(PdfSigningOptionCodes.EmudhraV1, branch3.PK);
			var result7 = PdfSignatureFactory.NewPdfSignature(PdfSigningOptionCodes.EmudhraV1, ZGuid.NewZGuid());

			AssertSigner(result1, null);
			AssertSigner(result2, config3company1);
			AssertSigner(result3, config1branch1);
			AssertSigner(result4, config2branch2);
			AssertSigner(result5, config4company2);
			AssertSigner(result6, config4company2);
			AssertSigner(result7, null);
		}

		void AssertSigner(TPdfSignature signature, DocumentSigningServiceCredentialsWithProviderConfiguration config)
		{
			var pdfFactory = signature.SignerFactory as PdfSignatureFactory.PdfSignerFactory;
			AssertNotNull(pdfFactory);

			var signer = pdfFactory.Signer as EMudhraPdfSigner;
			AssertNotNull(signer);

			if (config != null)
			{
				AssertEquals(config.AccessKey, signer.Client.AccessKey);
				AssertEquals(config.ClientID, signer.Client.ClientID);
				AssertEquals(config.KeyID, signer.Client.KeyID);
			}
			else
			{
				AssertEquals("", signer.Client.AccessKey);
				AssertEquals("", signer.Client.ClientID);
				AssertEquals("", signer.Client.KeyID);
			}

			var partnerCredentials = DocumentsDataRegistry.Instance.DocumentSigningServicePartnerCredentials.Value;
			AssertEquals(partnerCredentials.PartnerID, signer.Client.PartnerID);
			AssertEquals(partnerCredentials.PartnerAccessKey, signer.Client.PartnerAccessKey);
		}

		public void TestCredentials_BatchSigner()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var company2 = Factory.NewWithValidTestData<GlbCompany>();

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var branch3 = Factory.NewWithValidTestData<GlbBranch>();

			branch1.GB_GC = company1.PK;
			branch2.GB_GC = company1.PK;
			branch3.GB_GC = company2.PK;

			Factory.Save();

			var config1branch1 = new DocumentSigningServiceCredentialsWithProviderConfiguration() { ProviderCode = PdfSigningOptionCodes.EmudhraV1, AccessKey = "1", ClientID = "1", KeyID = "1" };
			var config2branch2 = new DocumentSigningServiceCredentialsWithProviderConfiguration() { ProviderCode = PdfSigningOptionCodes.EmudhraV1, AccessKey = "2", ClientID = "2", KeyID = "2" };
			var config3company1 = new DocumentSigningServiceCredentialsWithProviderConfiguration() { ProviderCode = PdfSigningOptionCodes.EmudhraV1, AccessKey = "3", ClientID = "3", KeyID = "3" };
			var config4company2 = new DocumentSigningServiceCredentialsWithProviderConfiguration() { ProviderCode = PdfSigningOptionCodes.EmudhraV1, AccessKey = "4", ClientID = "4", KeyID = "4" };
			var extraConfig = new DocumentSigningServiceCredentialsConfiguration() { AccessKey = "partnerKey", ClientID = "partnerName", KeyID = "accessToken" };

			DocumentsDataRegistry.Instance.DocumentSigningServiceCredentials.SetValue(Guid.Empty, branch1.PK.ToGuid(), Guid.Empty, config1branch1);
			DocumentsDataRegistry.Instance.DocumentSigningServiceCredentials.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, config3company1);
			DocumentsDataRegistry.Instance.DocumentSigningServiceCredentials.SetValue(Guid.Empty, branch2.PK.ToGuid(), Guid.Empty, config2branch2);
			DocumentsDataRegistry.Instance.DocumentSigningServiceCredentials.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, config4company2);

			DocumentsDataRegistry.Instance.DocumentSigningServicePartnerCredentialsAccessToken.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, extraConfig);

			var signingOptions = new List<string> { PdfSigningOptionCodes.EmudhraV1 , PdfSigningOptionCodes.DigitalSign };
			foreach (var signingOption in signingOptions)
			{
				AssertBatchSigner(signingOption, ZGuid.NewZGuid(), null, extraConfig);
				AssertBatchSigner(signingOption, company1.PK, config3company1, extraConfig);
				AssertBatchSigner(signingOption, branch1.PK, config1branch1, extraConfig);
				AssertBatchSigner(signingOption, branch2.PK, config2branch2, extraConfig);
				AssertBatchSigner(signingOption, company2.PK, config4company2, extraConfig);
				AssertBatchSigner(signingOption, branch3.PK, config4company2, extraConfig);
			}
		}

		void AssertBatchSigner(string signingOption, ZGuid branch, DocumentSigningServiceCredentialsWithProviderConfiguration config, DocumentSigningServiceCredentialsConfiguration extraConfig)
		{
			var batchSigner = PdfSignatureFactory.NewPdfBatchSigner(signingOption, branch);
			AssertNotNull(batchSigner);

			if (batchSigner is EMudhraBatchSigner eMudhraBatchSigner)
			{
				if (config != null)
				{
					AssertEquals(config.AccessKey, eMudhraBatchSigner.Client.AccessKey);
					AssertEquals(config.ClientID, eMudhraBatchSigner.Client.ClientID);
					AssertEquals(config.KeyID, eMudhraBatchSigner.Client.KeyID);
				}
				else
				{
					AssertEquals("", eMudhraBatchSigner.Client.AccessKey);
					AssertEquals("", eMudhraBatchSigner.Client.ClientID);
					AssertEquals("", eMudhraBatchSigner.Client.KeyID);
				}

				AssertEquals("Documents -> Document Signing Service -> Cloud Signing Service Provider API Endpoint", eMudhraBatchSigner.Client.WebApiUrlSource);
			}

			if (batchSigner is DigitalSignBatchSigner digitalSignBatchSigner)
			{
				if (config != null)
				{
					AssertEquals(config.AccessKey, digitalSignBatchSigner.Client.AuthorizerTotpID);
					AssertEquals(config.KeyID, digitalSignBatchSigner.Client.AuthorizerTotpSecretKey);
				}
				else
				{
					AssertEquals("", digitalSignBatchSigner.Client.AuthorizerTotpID);
					AssertEquals("", digitalSignBatchSigner.Client.AuthorizerTotpSecretKey);
				}

				AssertEquals(extraConfig.KeyID, digitalSignBatchSigner.Client.AccessToken);
			}
		}
	}
}
