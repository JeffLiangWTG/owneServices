using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Testing.GlobalEInvoicing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.CostaRica.Testing
{
	[TestedType(typeof(ElectronicMessagingProcessingServiceTaskForCostaRica))]
	public class ElectronicMessagingProcessingServiceTaskForCostaRicaTest : GlobalElectronicMessagingProcessingServiceTaskTest<ElectronicMessagingProcessingServiceTaskForCostaRica>
	{
		protected override ZString CountryCode => CountryCodes.CostaRica;
		protected override string ExpectedMessageTypeForGenerateInvoiceRequest => CostaRicaEInvoiceAPICommandList.Codes.GenerateInvoiceRequest;
		protected override string ExpectedMessageTypeForGenerateCancellationRequest => CostaRicaEInvoiceAPICommandList.Codes.GenerateInvoiceRequest;
		protected override int ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions => 1;
		protected override IReadOnlyCollection<int> ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions => new int[] { 1 };

		protected override ElectronicMessagingProcessingServiceTaskForCostaRica GetCountrySpecificServiceTask() => new ElectronicMessagingProcessingServiceTaskForCostaRica();

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new(
						AccEInvoicingTransactionPivotSchema.Constants.TableName,
						CountryCode + " Newly created transactions",
						AccEInvoicingTransactionPivotSchema.Constants.AIP_Status + "=" + EInvoicingPivotState.Queued,
						AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode + "=" + CountryCode),
				};
			}
		}
	}
}
