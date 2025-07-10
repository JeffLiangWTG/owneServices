using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.Credentials;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.Common.Universal.Testing;
using Enterprise.Accounting.ElectronicMessaging.Testing.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Newtonsoft.Json;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using UniversalTransaction = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Brazil.Testing
{
	[TestedType(typeof(ElectronicMessagingProcessingServiceTaskForBrazil))]
	public class ElectronicMessagingProcessingServiceTaskForBrazilTest : GlobalElectronicMessagingProcessingServiceTaskTest<ElectronicMessagingProcessingServiceTaskForBrazil>
	{
		protected override ZString CountryCode => CountryCodes.Brazil;

		protected override DateTime TestDate => new DateTime(2021, 02, 12);

		protected override string ExpectedMessageTypeForGenerateInvoiceRequest => BrazilEInvoiceAPICommandList.Codes.GenerateInvoiceRequest;

		protected override ElectronicMessagingProcessingServiceTaskForBrazil GetCountrySpecificServiceTask() => new ElectronicMessagingProcessingServiceTaskForBrazil();

		protected override int ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions => 1;

		protected override IReadOnlyCollection<int> ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions => new int[] { 1 };

		protected override void AddCountrySpecificData()
		{
			ComplianceBook = TestObjectCreator.SetupComplianceSequence(ZGuid.Empty, BrazilComplianceInfo.ComplianceSubTypeCodes.NFS, "ThePrefix", 1, 100, 1, allocationLevel: "BRN");
		}

		protected override void AddAdditionalInformationForCompany(GlbCompany company)
		{
			AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetValue(
					company.PK.ToGuid(), Guid.Empty, Guid.Empty,
					AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.GovermentNumberAllocate);
		}

		protected override void AddCountrySpecificCustomsCodesForBranchOrgProxy(OrgHeader orgProxy)
		{
			Helper.AddCustomsCodeForCountryIfMissing(orgProxy, CountryCode, BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ);
		}
		protected override void AddCountrySpecificCustomsCodesForDebtor(OrgHeader arOrg)
		{
			Helper.AddCustomsCodeForCountryIfMissing(arOrg, CountryCode, BrazilOrgCusCodeInfo.OrgCusCodes.MunicipalTaxPayerRegistration);
		}

		protected override void BeforeSaveOfARAPINVCRDADJTransactions(ARInvoice arInvoice, ARCreditNote arCreditNote, ARAdjustmentNote arAdjustmentNote, APInvoice apInvoice, APCreditNote apCreditNote, APAdjustmentNote apAdjustmentNote)
		{
			// Brazil uses compliance sub types and compliance numbers.
			if (arInvoice != null)
			{
				arInvoice.AH_XD_ComplianceBook = ComplianceBook.PK;
				arInvoice.AH_ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.NFS;
				arInvoice.AH_TransactionReference = "AB1234";
			}
			if (arCreditNote != null)
			{
				// Stand alone credit note (not reversal)
				arCreditNote.AH_XD_ComplianceBook = ComplianceBook.PK;
				arCreditNote.AH_ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.CNS;
				arCreditNote.AH_TransactionReference = "AB1234";
			}
			if (arAdjustmentNote != null)
			{
				arAdjustmentNote.AH_XD_ComplianceBook = ComplianceBook.PK;
				arAdjustmentNote.AH_ComplianceSubType = BrazilComplianceInfo.ComplianceSubTypeCodes.CNS;
				arAdjustmentNote.AH_TransactionReference = "AB1234";
			}
		}

		string CertificateThumbprint;
		protected override void AddCountrySpecificCredentialsForCompanyOrBranch(GlbBranch branchWithCompany)
		{
			var newFactory = new BusinessObjectFactory();
			var credential = newFactory.New<GlbExternalPassword>();
			credential.GP_GB = branchWithCompany.PK;
			credential.GP_GC = ZGuid.Empty;
			credential.GP_PasswordType = PasswordTypesList.Codes.EIM;
			credential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			credential.GP_IssueDate = ZDateTime.Today.AddDays(-30);
			credential.GP_ExpiryDate = ZDateTime.Today.AddDays(30);

			const string certificateRootName = "CN = VMS ICA1 Staging;O = FRCS;C = FJ";
			const string certificateChildName = "CN = PTY7 Wisetech Global Limited;OU = Wisetech Global Limited;O = Wisetech Global Limited;STREET = 72 O'Riodran St Post Code 2015;L = Alexendria NSW;S = UNKNOWN;C = AU";
			var certPassword = new NetworkCredential("user", "password12345678");
			using (var root = SampleCertificateHelper.CreateSampleRootCertificate(new X500DistinguishedName(certificateRootName, X500DistinguishedNameFlags.UseSemicolons)))
			using (var cert = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(certificateChildName, X500DistinguishedNameFlags.UseSemicolons)))
			{
				credential.GP_Certificate = cert.Export(X509ContentType.Pfx, certPassword.SecurePassword);
				credential.CurrentDecryptedCertificatePassphrase = certPassword.Password;
				CertificateThumbprint = cert.Thumbprint;
			}
			newFactory.Save();
		}

		protected override void AssertCountrySpecificGEIMessageContent(GlobalElectronicInvoicing geiMessage)
		{
			AssertNotNullOrEmpty(nameof(geiMessage.Payload), geiMessage.Payload);
			var jsonPayload = Encoding.UTF8.GetString(Convert.FromBase64String(geiMessage.Payload));
			var payloadObject = JsonConvert.DeserializeObject<dynamic>(jsonPayload);
			AssertEquals("Payload JSON ComplianceBookPrefix", "ThePrefix", payloadObject.ComplianceBookPrefix.ToString());

			AssertCommonGEIMessageContent(geiMessage);
		}

		protected override void AssertCountrySpecificGEIMessageContentForReversal(GlobalElectronicInvoicing geiMessage)
		{
			AssertNotNullOrEmpty(nameof(geiMessage.Payload), geiMessage.Payload);
			var jsonPayload = Encoding.UTF8.GetString(Convert.FromBase64String(geiMessage.Payload));
			var payloadObject = JsonConvert.DeserializeObject<dynamic>(jsonPayload);
			AssertEquals("Payload JSON GovernmentAllocatedNumber", GetGovernmentAllocatedNumberForCancellation, payloadObject.GovernmentAllocatedNumber.ToString());

			AssertCommonGEIMessageContent(geiMessage);
		}

		void AssertCommonGEIMessageContent(GlobalElectronicInvoicing geiMessage)
		{
			AssertNotNullOrEmpty(nameof(geiMessage.Transaction), geiMessage.Transaction);
			UniversalDataBussExtensionsTestHelper.AssertBase64UniversalXMLIsParsableWithoutErrors<UniversalTransaction>(geiMessage.Transaction, nameof(geiMessage.Transaction));

			AssertEquals(2, geiMessage.Header.ElectronicInvoiceBatchRequest.Credentials.Count);
			var encodedCertificate = geiMessage.Header.ElectronicInvoiceBatchRequest.Credentials.Cast<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential>().Single(x => x.Key == CredentialKeys.Certificate);
			using (var actualCertificate = new X509Certificate2(Convert.FromBase64String(encodedCertificate.Value.Value), "password12345678"))
			{
				AssertEquals(nameof(CredentialKeys.Certificate), CertificateThumbprint, actualCertificate.Thumbprint);
			}

			var certificatePassword = geiMessage.Header.ElectronicInvoiceBatchRequest.Credentials.Cast<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential>().Single(x => x.Key == CredentialKeys.CertificatePassword);
			AssertEquals(nameof(CredentialKeys.CertificatePassword) + ".Encrypted", true, certificatePassword.Value.Encrypted);
			AssertNotNullOrEmpty(nameof(CredentialKeys.CertificatePassword), certificatePassword.Value.Value);
			AssertNoExceptionThrown(nameof(CredentialKeys.CertificatePassword), () => Convert.FromBase64String(certificatePassword.Value.Value));   // eHub encryption is not deterministic, so just assert we can decode.
		}

		AccComplianceSequence ComplianceBook;
	}
}
