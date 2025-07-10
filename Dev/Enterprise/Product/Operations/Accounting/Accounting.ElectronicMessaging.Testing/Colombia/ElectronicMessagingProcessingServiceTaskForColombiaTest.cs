using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Testing.GlobalEInvoicing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Colombia.Testing
{
	[TestedType(typeof(ElectronicMessagingProcessingServiceTaskForColombia))]
	public class ElectronicMessagingProcessingServiceTaskForColombiaTest : GlobalElectronicMessagingProcessingServiceTaskTest<ElectronicMessagingProcessingServiceTaskForColombia>
	{
		protected override ZString CountryCode => CountryCodes.Colombia;
		protected override string ExpectedMessageTypeForGenerateInvoiceRequest => ColombiaEInvoiceAPICommandList.Codes.GenerateInvoiceRequest;
		protected override string ExpectedMessageTypeForGenerateCancellationRequest => ColombiaEInvoiceAPICommandList.Codes.GenerateInvoiceRequest;
		protected override int ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions => 2;
		protected override IReadOnlyCollection<int> ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions => new int[] { 1, 1 };

		protected override ElectronicMessagingProcessingServiceTaskForColombia GetCountrySpecificServiceTask() => new ElectronicMessagingProcessingServiceTaskForColombia();
	}
}
