using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.Credentials;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.Common.Universal.Testing;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.Accounting.ElectronicMessaging.Testing.GlobalEInvoicing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using UniversalTransaction = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Poland.Testing
{
	[TestedType(typeof(ElectronicMessagingProcessingServiceTaskForPoland))]
	public class ElectronicMessagingProcessingServiceTaskForPolandTest : GlobalElectronicMessagingProcessingServiceTaskTest<ElectronicMessagingProcessingServiceTaskForPoland>
	{
		protected override ZString CountryCode => CountryCodes.Poland;

		protected override DateTime TestDate => new DateTime(2023, 12, 15);

		protected override string ExpectedMessageTypeForGenerateInvoiceRequest => PolandEInvoiceAPICommandList.Codes.SendInvoiceBatch;

		protected override ElectronicMessagingProcessingServiceTaskForPoland GetCountrySpecificServiceTask()
			=> new ElectronicMessagingProcessingServiceTaskForPoland();

		protected override int ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions => 3;

		protected override IReadOnlyCollection<int> ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions => new int[] { 3 };

		protected override void AddCountrySpecificCustomsCodesForBranchOrgProxy(OrgHeader orgProxy)
		{
			Helper.AddCustomsCodeForCountryIfMissing(orgProxy, CountryCode, OrgCusCode.PolandCodeTypes.PTU);
		}

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
				CertificateThumbprint = cert.Thumbprint;
			}
			newFactory.Save();

			var tokenCredential = AccountingElectronicMessagingRegistry.Instance.PolandEInvoicingCredentials.Value;
			tokenCredential.ClientId = "KSeF Token Name";
			tokenCredential.ClientSecret = "KSeF Token";
			AccountingElectronicMessagingRegistry.Instance.PolandEInvoicingCredentials.SetTemporaryValue(branchWithCompany.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, tokenCredential);
		}

		string CertificateThumbprint;

		protected override void BeforeSaveOfARAPINVCRDADJTransactions(ARInvoice arInvoice, ARCreditNote arCreditNote, ARAdjustmentNote arAdjustmentNote, APInvoice apInvoice, APCreditNote apCreditNote, APAdjustmentNote apAdjustmentNote)
		{
			if (arInvoice != null)
			{
				// Link a job to the AR Invoice, to see additional data items in GEI message
				var objectCreator = new TestObjectCreator(Factory);
				var job = objectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
				var shipment = (ForwardingShipment)job.PlugInData;
				shipment.JS_GoodsDescription = "The Goods Description";
				shipment.DocsAndCartage.JP_OrderItemsAsString = "OrderNumber";
				var chargeCode = objectCreator.CreateChargeCode(arInvoice.Company.GC_Code + "CC");
				var charge = TestObjectCreator.CreateCharge(job, chargeCode: chargeCode, desc: "The Charge",
					costCurrency: TestObjectCreator.EUR, osCostAmt: 450m, creditor: TestObjectCreator.Creditor1,
					sellCurrency: TestObjectCreator.EUR, osSellAmt: 545m, debtor: TestObjectCreator.Debtor1
				);
				arInvoice.Lines.Add(TestObjectCreator.CreateRevenueLine(charge, arInvoice.PK));
			}
		}

		protected override void AssertCountrySpecificGEIMessageContent(GlobalElectronicInvoicing geiMessage)
		{
			// Assert payload sections
			AssertNullOrEmpty(nameof(geiMessage.Payload), geiMessage.Payload);
			AssertNullOrEmpty(nameof(geiMessage.Transaction), geiMessage.Transaction);
			AssertNotNull(nameof(geiMessage.TransactionBatch), geiMessage.TransactionBatch);
			AssertEquals(1, geiMessage.TransactionBatch.Transactions.Length);
			UniversalDataBussExtensionsTestHelper.AssertBase64UniversalXMLIsParsableWithoutErrors<UniversalTransaction>(geiMessage.TransactionBatch.Transactions[0], "TransactionBatch[0]");

			// Assert credentials
			AssertEquals(4, geiMessage.Header.ElectronicInvoiceBatchRequest.Credentials.Count);
			var encodedCertificate = geiMessage.Header.ElectronicInvoiceBatchRequest.Credentials.Cast<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential>().Single(x => x.Key == CredentialKeys.Certificate);
			using (var actualCertificate = new X509Certificate2(Convert.FromBase64String(encodedCertificate.Value.Value), "password12345678"))
			{
				AssertEquals(nameof(CredentialKeys.Certificate), CertificateThumbprint, actualCertificate.Thumbprint);
			}

			var certificatePassword = geiMessage.Header.ElectronicInvoiceBatchRequest.Credentials.Cast<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential>().Single(x => x.Key == CredentialKeys.CertificatePassword);
			AssertEquals(nameof(CredentialKeys.CertificatePassword) + ".Encrypted", true, certificatePassword.Value.Encrypted);
			AssertNotNullOrEmpty(nameof(CredentialKeys.CertificatePassword), certificatePassword.Value.Value);
			AssertNoExceptionThrown(nameof(CredentialKeys.CertificatePassword), () => Convert.FromBase64String(certificatePassword.Value.Value));   // eHub encryption is not deterministic, so just assert we can decode.

			var tokenName = geiMessage.Header.ElectronicInvoiceBatchRequest.Credentials.Cast<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential>().Single(x => x.Key == CredentialKeys.Username);

			var token = geiMessage.Header.ElectronicInvoiceBatchRequest.Credentials.Cast<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential>().Single(x => x.Key == CredentialKeys.Password);
			AssertEquals(nameof(CredentialKeys.Password) + ".Encrypted", true, token.Value.Encrypted);
			AssertNotNullOrEmpty(nameof(CredentialKeys.Password), token.Value.Value);
			AssertNoExceptionThrown(nameof(CredentialKeys.Password), () => Convert.FromBase64String(token.Value.Value));   // eHub encryption is not deterministic, so just assert we can decode.

			// Assert additional data
			AssertNotNull(nameof(geiMessage.Header.ElectronicInvoiceBatchRequest.AdditionalDataItems), geiMessage.Header.ElectronicInvoiceBatchRequest.AdditionalDataItems);
			AssertEquals(nameof(geiMessage.Header.ElectronicInvoiceBatchRequest.AdditionalDataItems) + ".Count", 1, geiMessage.Header.ElectronicInvoiceBatchRequest.AdditionalDataItems.Count);
			AssertEquals(nameof(geiMessage.Header.ElectronicInvoiceBatchRequest.AdditionalDataItems) + "[0].Key", "AR:INV:ZDebtor1:00001000║ShipmentDetails", geiMessage.Header.ElectronicInvoiceBatchRequest.AdditionalDataItems[0].Key);
			AssertEquals(nameof(geiMessage.Header.ElectronicInvoiceBatchRequest.AdditionalDataItems) + "[0].Value", @"{""GoodsDescriptions"":[""The Goods Description""],""OrderNumbers"":[""OrderNumber""]}", geiMessage.Header.ElectronicInvoiceBatchRequest.AdditionalDataItems[0].Value);
		}

		public override void TestSuccessfulCreatedEDIInterchangeBodyTextForCancellationMessageFromReversal()
		{
			Assert("Not applicable; Poland does not support cancellation messages.", true);
		}
	}
}
