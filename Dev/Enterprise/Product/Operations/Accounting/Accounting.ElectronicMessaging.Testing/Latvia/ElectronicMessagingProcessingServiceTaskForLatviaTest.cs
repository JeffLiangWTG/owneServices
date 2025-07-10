using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.Common.Universal.Testing;
using Enterprise.Accounting.ElectronicMessaging.Testing.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using UniversalTransaction = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.ElectronicMessaging.Latvia.Testing
{
	[TestedType(typeof(ElectronicMessagingProcessingServiceTaskForLatvia))]
	public class ElectronicMessagingProcessingServiceTaskForLatviaTest : GlobalElectronicMessagingProcessingServiceTaskTest<ElectronicMessagingProcessingServiceTaskForLatvia>
	{
		protected override ZString CountryCode => CountryCodes.Latvia;

		protected override DateTime TestDate => new DateTime(2024, 10, 01);

		protected override string ExpectedMessageTypeForGenerateInvoiceRequest => LatviaEInvoiceAPICommandList.Codes.GenerateInvoiceRequest;

		protected override ElectronicMessagingProcessingServiceTaskForLatvia GetCountrySpecificServiceTask()
			=> new ElectronicMessagingProcessingServiceTaskForLatvia();

		protected override int ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions => 3;

		protected override IReadOnlyCollection<int> ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions => new int[] { 1, 1, 1 };

		protected override void AddCountrySpecificCredentialsForCompanyOrBranch(GlbBranch branchWithCompany)
		{
			// TODO: credentials
		}

		protected override void AssertCountrySpecificGEIMessageContent(GlobalElectronicInvoicing geiMessage)
		{
			AssertNullOrEmpty(nameof(geiMessage.Payload), geiMessage.Payload);
			AssertNotNull(nameof(geiMessage.Transaction), geiMessage.Transaction);
			AssertNull(nameof(geiMessage.TransactionBatch), geiMessage.TransactionBatch.Transactions);
			UniversalDataBussExtensionsTestHelper.AssertBase64UniversalXMLIsParsableWithoutErrors<UniversalTransaction>(geiMessage.Transaction, "Transaction");

			// TODO: credentials
		}

		public override void TestSuccessfulCreatedEDIInterchangeBodyTextForCancellationMessageFromReversal()
		{
			Assert("Not applicable; Latvia does not support cancellation messages.", true);
		}
	}
}
