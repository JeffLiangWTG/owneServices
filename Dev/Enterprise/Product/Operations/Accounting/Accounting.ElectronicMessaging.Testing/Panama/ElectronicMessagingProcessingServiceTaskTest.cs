using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.ElectronicMessaging.Testing.GlobalEInvoicing;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Panama.Testing
{
	[TestedType(typeof(ElectronicMessagingProcessingServiceTask))]
	public class ElectronicMessagingProcessingServiceTaskTest : GlobalElectronicMessagingProcessingServiceTaskTest<ElectronicMessagingProcessingServiceTask>
	{
		protected override ZString CountryCode => CountryCodes.Panama;

		protected override string ExpectedMessageTypeForGenerateInvoiceRequest => EInvoiceAPICommandList.Codes.GenerateInvoiceRequest;

		protected override string ExpectedMessageTypeForGenerateCancellationRequest => EInvoiceAPICommandList.Codes.GenerateInvoiceRequest;

		protected override int ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions => 2;

		protected override IReadOnlyCollection<int> ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions => new int[] { 1, 1 };

		protected override ElectronicMessagingProcessingServiceTask GetCountrySpecificServiceTask() => new ElectronicMessagingProcessingServiceTask();

		protected override void BeforeSaveOfARAPINVCRDADJTransactions(ARInvoice arInvoice, ARCreditNote arCreditNote, ARAdjustmentNote arAdjustmentNote, APInvoice apInvoice, APCreditNote apCreditNote, APAdjustmentNote apAdjustmentNote)
		{
			if (arInvoice != null)
			{
				arInvoice.AH_ComplianceSubType = PanamaComplianceInfo.ComplianceSubTypeCodes.TXI;
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						AccEInvoicingTransactionPivotSchema.Constants.TableName,
						CountryCode + " Newly created transactions",
						AccEInvoicingTransactionPivotSchema.Constants.AIP_Status            + "=" + EInvoicingPivotState.Queued,
						AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode  + "=" + CountryCode),

					new TaskNudgeInformationForTest(
						AccEInvoicingTransactionPivotSchema.Constants.TableName,
						CountryCode + " Transactions that depend on their original transactions",
						AccEInvoicingTransactionPivotSchema.Constants.AIP_Status            + "=" + EInvoicingPivotState.Succeed,
						AccEInvoicingTransactionPivotSchema.Constants.AIP_RN_NKCountryCode  + "=" + CountryCode),
				};
			}
		}
	}
}
