using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.Common.Universal.Testing;
using Enterprise.Accounting.ElectronicMessaging.D365;
using Enterprise.Accounting.ElectronicMessaging.Testing.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using UniversalTransaction = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Testing.D365
{
	[TestedType(typeof(ElectronicMessagingProcessingServiceTaskForD365))]
	public class ElectronicMessagingProcessingServiceTaskForD365Test : GlobalElectronicMessagingProcessingServiceTaskTest<ElectronicMessagingProcessingServiceTaskForD365>
	{
		protected override ZString CountryCode => CountryCodes.UnitedKingdom;

		protected override DateTime TestDate => new DateTime(2025, 05, 16);

		protected override string ExpectedMessageTypeForGenerateInvoiceRequest => D365APICommandList.Codes.GenerateInvoiceRequest;

		protected override ElectronicMessagingProcessingServiceTaskForD365 GetCountrySpecificServiceTask()
		=> new ElectronicMessagingProcessingServiceTaskForD365();

		protected override int ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions => 3;

		protected override string ExpectedMessagingSystem => "Electronic Accounting Integration";

		protected override string ExpectedServicePoint => "XHUB_D365_EACCOUNTING";

		protected override IReadOnlyCollection<int> ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions => new int[] { 1, 1, 1 };

		public override void TestSuccessfulCreatedEDIInterchangeBodyTextForCancellationMessageFromReversal()
		{
			Assert("Not applicable; D365 does not support cancellation messages.", true);
		}

		public override void TestSuccessfulCreationOfEDIInterchange_ServicePointWithSuffix()
		{
			Assert("Not applicable; EDI Interchange creation is not applicable for Service Points with suffix", true);
		}

		protected override void AssertCountrySpecificGEIMessageContent(GlobalElectronicInvoicing geiMessage)
		{
			AssertNullOrEmpty(nameof(geiMessage.Payload), geiMessage.Payload);
			AssertNullOrEmpty(nameof(geiMessage.Transaction), geiMessage.Transaction);
			AssertNotNull(nameof(geiMessage.TransactionBatch), geiMessage.TransactionBatch);
			UniversalDataBussExtensionsTestHelper.AssertBase64UniversalXMLIsParsableWithoutErrors<UniversalTransaction>(geiMessage.TransactionBatch.Transactions[0], "TransactionBatch[0]");

			AssertEquals(4, geiMessage.Header.ElectronicInvoiceBatchRequest.Credentials.Count);

			var serviceEndpoint = geiMessage.Header.ElectronicInvoiceBatchRequest.Credentials.Cast<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential>().Single(x => x.Key == "ServiceEndpoint");
			AssertEquals("ServiceEndpoint", "serviceEndpointUrl", serviceEndpoint.Value.Value);

			var clientID = geiMessage.Header.ElectronicInvoiceBatchRequest.Credentials.Cast<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential>().Single(x => x.Key == "ClientId");
			AssertEquals("clientID", clientID.Value.Value);

			var tenantID = geiMessage.Header.ElectronicInvoiceBatchRequest.Credentials.Cast<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential>().Single(x => x.Key == "TenantId");
			AssertEquals("tenantID", tenantID.Value.Value);

			var clientSecret = geiMessage.Header.ElectronicInvoiceBatchRequest.Credentials.Cast<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential>().Single(x => x.Key == "ClientSecret");
			AssertEquals(true, clientSecret.Value.Encrypted);
			AssertNotNullOrEmpty(clientSecret.Value.Value);
			AssertNoExceptionThrown("Encrypted credential is base64 encoded", () => Convert.FromBase64String(clientSecret.Value.Value));
		}

		protected override void AddCountrySpecificCredentialsForCompanyOrBranch(GlbBranch branchWithCompany)
		{
			var credential = AccountingMasterFilesRegistry.Instance.D365Credentials.Value;
			credential.ClientID = "clientID";
			credential.ClientSecret = "clientSecret";
			credential.TenantID = "tenantID";

			AccountingMasterFilesRegistry.Instance.D365Credentials.SetTemporaryValue(branchWithCompany.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, credential);
			AccountingMasterFilesRegistry.Instance.D365WebserviceURL.SetTemporaryValue(branchWithCompany.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, "serviceEndpointUrl");
		}
	}
}
