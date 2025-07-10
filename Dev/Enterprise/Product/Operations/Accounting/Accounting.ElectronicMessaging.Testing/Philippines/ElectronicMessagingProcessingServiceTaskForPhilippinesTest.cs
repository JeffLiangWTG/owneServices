using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Testing.GlobalEInvoicing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Philippines.Testing
{
	[TestedType(typeof(ElectronicMessagingProcessingServiceTaskForPhilippines))]
	public class ElectronicMessagingProcessingServiceTaskForPhilippinesTest : GlobalElectronicMessagingProcessingServiceTaskTest<ElectronicMessagingProcessingServiceTaskForPhilippines>
	{
		protected override ZString CountryCode => CountryCodes.Philippines;

		protected override ElectronicMessagingProcessingServiceTaskForPhilippines GetCountrySpecificServiceTask()
			=> new ElectronicMessagingProcessingServiceTaskForPhilippines();

		protected override int ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions => 3;

		protected override IReadOnlyCollection<int> ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions => new int[] { 1, 1, 1 };

		protected override string ExpectedMessageTypeForGenerateInvoiceRequest => PhilippinesEInvoiceAPICommandList.Codes.GenerateSubmitTaxInvoice;

		public override void TestSuccessfulCreatedEDIInterchangeBodyTextForCancellationMessageFromReversal()
			=> Assert("Not applicable; Philippines does not handle cancellations.", true);
	}
}
