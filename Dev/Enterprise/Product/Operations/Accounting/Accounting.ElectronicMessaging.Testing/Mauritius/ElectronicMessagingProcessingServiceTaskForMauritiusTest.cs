using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common.Credentials;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.Common.Universal.Testing;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.Accounting.ElectronicMessaging.Testing.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using UniversalTransaction = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Mauritius.Testing
{
	[TestedType(typeof(ElectronicMessagingProcessingServiceTaskForMauritius))]
	public class ElectronicMessagingProcessingServiceTaskForMauritiusTest : GlobalElectronicMessagingProcessingServiceTaskTest<ElectronicMessagingProcessingServiceTaskForMauritius>
	{
		protected override ZString CountryCode => CountryCodes.Mauritius;

		protected override DateTime TestDate => new DateTime(2024, 04, 26);

		protected override string ExpectedMessageTypeForGenerateInvoiceRequest => MauritiusEInvoiceAPICommandList.Codes.GenerateInvoiceRequest;

		protected override ElectronicMessagingProcessingServiceTaskForMauritius GetCountrySpecificServiceTask()
			=> new ElectronicMessagingProcessingServiceTaskForMauritius();

		protected override int ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions => 3;

		protected override IReadOnlyCollection<int> ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions => new int[] { 1, 1, 1 };

		protected override void AddCountrySpecificCredentialsForCompanyOrBranch(GlbBranch branchWithCompany)
		{
			AccountingElectronicMessagingRegistry.Instance.MauritiusEInvoicingUsername.SetTemporaryValue(branchWithCompany.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, "MRA Username for test");
			AccountingElectronicMessagingRegistry.Instance.MauritiusEInvoicingPassword.SetTemporaryValue(branchWithCompany.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, "MRA Password for test");
			AccountingElectronicMessagingRegistry.Instance.MauritiusEInvoicingEbsMraID.SetTemporaryValue(branchWithCompany.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, "EBS MRA ID for test");
		}

		protected override void AssertCountrySpecificGEIMessageContent(GlobalElectronicInvoicing geiMessage)
		{
			AssertNullOrEmpty(nameof(geiMessage.Payload), geiMessage.Payload);
			AssertNotNull(nameof(geiMessage.Transaction), geiMessage.Transaction);
			AssertNull(nameof(geiMessage.TransactionBatch), geiMessage.TransactionBatch.Transactions);
			UniversalDataBussExtensionsTestHelper.AssertBase64UniversalXMLIsParsableWithoutErrors<UniversalTransaction>(geiMessage.Transaction, "Transaction");

			// Assert credentials
			AssertEquals(3, geiMessage.Header.ElectronicInvoiceBatchRequest.Credentials.Count);

			var userName = geiMessage.Header.ElectronicInvoiceBatchRequest.Credentials.Cast<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential>().Single(x => x.Key == CredentialKeys.Username);
			AssertEquals(nameof(CredentialKeys.Username), "MRA Username for test", userName.Value.Value);
			var ebsMraId = geiMessage.Header.ElectronicInvoiceBatchRequest.Credentials.Cast<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential>().Single(x => x.Key == "EbsMraId");
			AssertEquals("EbsMraId", "EBS MRA ID for test", ebsMraId.Value.Value);

			var password = geiMessage.Header.ElectronicInvoiceBatchRequest.Credentials.Cast<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential>().Single(x => x.Key == CredentialKeys.Password);
			AssertEquals(nameof(CredentialKeys.Password) + ".Encrypted", true, password.Value.Encrypted);
			AssertNotNullOrEmpty(nameof(CredentialKeys.Password), password.Value.Value);
			AssertNoExceptionThrown(nameof(CredentialKeys.Password), () => Convert.FromBase64String(password.Value.Value));   // eHub encryption is not deterministic, so just assert we can decode.
		}

		public override void TestSuccessfulCreatedEDIInterchangeBodyTextForCancellationMessageFromReversal()
		{
			Assert("Not applicable; Mauritius does not support cancellation messages.", true);
		}
	}
}
