using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common.Credentials;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.Accounting.ElectronicMessaging.Testing.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Malaysia.Testing
{
	[TestedType(typeof(ElectronicMessagingProcessingServiceTaskForMalaysia))]
	public class ElectronicMessagingProcessingServiceTaskForMalaysiaTest : GlobalElectronicMessagingProcessingServiceTaskTest<ElectronicMessagingProcessingServiceTaskForMalaysia>
	{
		protected override void AddCountrySpecificCredentialsForCompanyOrBranch(GlbBranch branchWithCompany)
		{
			var newFactory = new BusinessObjectFactory();

			var credential = newFactory.New<GlbExternalPassword>();
			credential.GP_GB = ZGuid.Empty;
			credential.GP_GC = branchWithCompany.Company.PK;
			credential.GP_PasswordType = PasswordTypesList.Codes.EIM;
			credential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			credential.GP_IssueDate = ZDateTime.Today.AddDays(-30);
			credential.GP_ExpiryDate = ZDateTime.Today.AddDays(30);

			const string certificateRootName = "CN = Some Testing Root Certificate;O = KSeF;C = PL";
			const string certificateChildName = "CN = Wisetech Global Limited Test Certificate;OU = Wisetech Global Limited;O = Wisetech Global Limited;STREET = 72 O'Riodran St Post Code 2015;L = Alexendria;S = NSW;C = AU";
			var certPassword = new NetworkCredential("user", "password12345678");
			using (var root = SampleCertificateHelper.CreateSampleRootCertificate(new X500DistinguishedName(certificateRootName, X500DistinguishedNameFlags.UseSemicolons)))
			using (var cert = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(certificateChildName, X500DistinguishedNameFlags.UseSemicolons)))
			{
				credential.GP_Certificate = cert.Export(X509ContentType.Pfx, certPassword.SecurePassword);
				credential.CurrentDecryptedCertificatePassphrase = certPassword.Password;
			}
			newFactory.Save();

			var tokenCredential = AccountingElectronicMessagingRegistry.Instance.MalaysiaEInvoicingCredentials.Value;
			tokenCredential.ClientId = ClientID;
			tokenCredential.ClientSecret = ClientSecret;
			AccountingElectronicMessagingRegistry.Instance.MalaysiaEInvoicingCredentials.SetTemporaryValue(branchWithCompany.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, tokenCredential);
		}

		protected override void AssertCountrySpecificGEIMessageContent(GlobalElectronicInvoicing geiMessage)
		{
			AssertEquals(ExpectedMessageTypeForGenerateInvoiceRequest, geiMessage.Header.ElectronicInvoiceBatchRequest.MessageType);
			AssertNotNull(nameof(geiMessage.Transaction), geiMessage.Transaction);
			AssertEquals(4, geiMessage.Header.ElectronicInvoiceBatchRequest.Credentials.Count);

			var credentials = geiMessage.Header.ElectronicInvoiceBatchRequest.Credentials.Cast<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential>();
			AssertEquals(ClientID, credentials.Single(x => x.Key == CredentialKeys.Username).Value.Value);

			var token = credentials.Single(x => x.Key == CredentialKeys.Password);
			AssertEquals(nameof(CredentialKeys.Password) + ".Encrypted", true, token.Value.Encrypted);
			AssertNotNullOrEmpty(nameof(CredentialKeys.Password), token.Value.Value);
			AssertNoExceptionThrown(nameof(CredentialKeys.Password), () => Convert.FromBase64String(token.Value.Value));
		}

		public override void TestSuccessfulCreatedEDIInterchangeBodyTextForCancellationMessageFromReversal()
			=> Assert("Not applicable; Malaysia does not handle cancellations.", true);

		protected override ZString CountryCode => CountryCodes.Malaysia;

		protected override ElectronicMessagingProcessingServiceTaskForMalaysia GetCountrySpecificServiceTask() => new ElectronicMessagingProcessingServiceTaskForMalaysia();

		protected override string ExpectedMessageTypeForGenerateInvoiceRequest =>
			MalaysiaEInvoiceAPICommandList.Codes.SubmitTransaction;

		protected override IReadOnlyCollection<int> ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions => new int[] { 1 };

		protected override void AddCountrySpecificCustomsCodesForBranchOrgProxy(OrgHeader orgProxy)
		{
			orgProxy.CustomsCodes.AddNew(MalaysiaOrgCusCodeInfo.OrgCusCodes.TaxIdentificationNumber, "1", CountryCodes.Malaysia);
			orgProxy.CustomsCodes.AddNew(MalaysiaOrgCusCodeInfo.OrgCusCodes.RegistrarOfCompany, "1", CountryCodes.Malaysia);
			orgProxy.CustomsCodes.AddNew(MalaysiaOrgCusCodeInfo.OrgCusCodes.SER, "1", CountryCodes.Malaysia);
			orgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.StandardIndustrialClassification, MYMSICCodeDescriptionPairList.Codes.AirportAndAirTrafficControl, CountryCodes.Malaysia);
		}

		protected override void AddCountrySpecificCustomsCodesForDebtor(OrgHeader arOrg)
		{
			arOrg.CustomsCodes.AddNew(MalaysiaOrgCusCodeInfo.OrgCusCodes.TaxIdentificationNumber, "1", CountryCodes.Malaysia);

			if (arOrg.OH_RL_NKClosestPort.IsEmpty)
			{
				arOrg.OH_RL_NKClosestPort = "AUSYD";
			}

			var contact = TestObjectCreator.CreateContact(arOrg, "first contact");
			contact.OC_Phone = "123";
			var orgDocument = contact.Documents.AddNew();
			orgDocument.OD_DocumentGroup = ContactType.Receivables.Code;
		}

		protected override void AddCountrySpecificData()
		{
			var query = new ZQuery(GlbStaffSchema.GS_Code, StaticCurrentFetcher.Instance.CurrentUserCode);
			var staff = Factory.LoadTop1<GlbStaff>(query);
			staff.GS_WorkPhone = "123";
		}

		string ClientID => "Client ID";
		string ClientSecret => "Client Secret";
	}
}
