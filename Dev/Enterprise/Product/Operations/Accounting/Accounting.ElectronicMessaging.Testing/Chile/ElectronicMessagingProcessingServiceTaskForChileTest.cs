using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Testing.GlobalEInvoicing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Chile.Testing
{
	[TestedType(typeof(ElectronicMessagingProcessingServiceTaskForChile))]
	public class ElectronicMessagingProcessingServiceTaskForChileTest : GlobalElectronicMessagingProcessingServiceTaskTest<ElectronicMessagingProcessingServiceTaskForChile>
	{
		protected override ZString CountryCode => CountryCodes.Chile;

		protected override string ExpectedMessageTypeForGenerateInvoiceRequest => ChileEInvoiceAPICommandList.Codes.GenerateInvoiceRequest;

		protected override string ExpectedMessageTypeForGenerateCancellationRequest => ChileEInvoiceAPICommandList.Codes.GenerateInvoiceRequest;

		protected override int ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions => 2;

		protected override IReadOnlyCollection<int> ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions => new int[] { 1, 1 };

		protected override ElectronicMessagingProcessingServiceTaskForChile GetCountrySpecificServiceTask() => new ElectronicMessagingProcessingServiceTaskForChile();
	}
}
