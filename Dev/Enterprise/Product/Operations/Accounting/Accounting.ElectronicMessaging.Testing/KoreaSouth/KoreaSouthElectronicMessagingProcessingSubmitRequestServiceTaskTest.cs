using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.KoreaSouth.Testing
{
	[TestedType(typeof(KoreaSouthElectronicMessagingProcessingSubmitRequestServiceTask))]
	public class KoreaSouthElectronicMessagingProcessingSubmitRequestServiceTaskTest : KoreaSouthElectronicMessagingProcessingServiceTaskTest<KoreaSouthElectronicMessagingProcessingSubmitRequestServiceTask>
	{
		public override void TestHostedServiceTimeProperties()
		{
			var attributes = typeof(KoreaSouthElectronicMessagingProcessingSubmitRequestServiceTask).Assembly.GetCustomAttributes<HostedServiceAttribute>();
			var attribute = attributes.First(x => x.Code == "EKR");
			AssertEquals("1hour", attribute.DefaultScheduleRunEvery);
			AssertEquals("15minutes", attribute.MinimumPeriod);
			AssertEquals("12hours", attribute.MaximumPeriod);
		}

		protected override KoreaSouthElectronicMessagingProcessingSubmitRequestServiceTask GetCountrySpecificServiceTask() => new KoreaSouthElectronicMessagingProcessingSubmitRequestServiceTask();

		protected override void AssertProcessMessageSubType(params ARInvoice[] invoiceList)
		{
			AssertEReportingStatus("GEN E-Reporting status will be Sent", EInvoicingPivotState.Sent, invoiceList[0], EInvoicingPivotActionType.Submit);
			AssertEReportingStatus("GEQ E-Reporting status will still be Queued", EInvoicingPivotState.Queued, invoiceList[1], EInvoicingPivotActionType.StatusCheck);
		}

		protected override string ExpectedMessageTypeForGenerateInvoiceRequest => KoreaSouthEInvoiceAPICommandList.Codes.GenerateInvoiceRequest;

		protected override string ExpectedMessageTypeForGenerateCancellationRequest => KoreaSouthEInvoiceAPICommandList.Codes.GenerateInvoiceRequest;

		protected override string ExpectedPivotActionType => EInvoicingPivotActionType.Submit;

		protected override IReadOnlyCollection<int> ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions => new int[] { 1 };
	}
}
