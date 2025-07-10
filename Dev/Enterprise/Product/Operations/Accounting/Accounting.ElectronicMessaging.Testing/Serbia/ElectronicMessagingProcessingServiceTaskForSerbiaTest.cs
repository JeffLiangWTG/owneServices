using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Serbia;
using Enterprise.Accounting.ElectronicMessaging.Testing.GlobalEInvoicing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Testing.Serbia
{
	[TestedType(typeof(ElectronicMessagingProcessingServiceTaskForSerbia))]
	public class ElectronicMessagingProcessingServiceTaskForSerbiaTest : GlobalElectronicMessagingProcessingServiceTaskTest<ElectronicMessagingProcessingServiceTaskForSerbia>
	{
		protected override ZString CountryCode => CountryCodes.Serbia;

		protected override int ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions => 3;
		protected override IReadOnlyCollection<int> ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions => new int[] { 1, 1, 1 };

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
			=> new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						AccEInvoicingTransactionPivotSchema.Constants.TableName,
						null,
						$"{AccEInvoicingTransactionPivotSchema.Constants.AIP_Status} = {EInvoicingPivotState.Queued}",
						$"{AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode} = {CountryCodes.Serbia}")
				};

		protected override string ExpectedMessageTypeForGenerateInvoiceRequest => SerbiaEInvoiceAPICommandList.Codes.GenerateInvoiceSubmission;

		protected override string ExpectedServicePoint => "XHUB_RS_EINVOICING";

		protected override (int period, RunsEvery runningEvery) ExpectedServiceTaskRunFrequency
			=> (30, RunsEvery.Minute);

		protected override ElectronicMessagingProcessingServiceTaskForSerbia GetCountrySpecificServiceTask() => new ElectronicMessagingProcessingServiceTaskForSerbia();

		public override void TestSuccessfulCreatedEDIInterchangeBodyTextForCancellationMessageFromReversal()
			=> Assert("Not applicable; Does not handle cancellations.", true);
	}
}
