using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.ElectronicMessaging.Testing.GlobalEInvoicing;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Argentina.Testing
{
	[TestedType(typeof(ElectronicMessagingProcessingServiceTaskForArgentina))]
	public class ElectronicMessagingProcessingServiceTaskForArgentinaTest : GlobalElectronicMessagingProcessingServiceTaskTest<ElectronicMessagingProcessingServiceTaskForArgentina>
	{
		protected override ZString CountryCode => CountryCodes.Argentina;
		protected override string ExpectedMessageTypeForGenerateInvoiceRequest => "GEL";
		protected override string ExpectedMessageTypeForGenerateCancellationRequest => "GEL";
		protected override int ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions => 2;
		protected override IReadOnlyCollection<int> ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions => new int[] { 1, 1 };
		protected override ElectronicMessagingProcessingServiceTaskForArgentina GetCountrySpecificServiceTask() => new ElectronicMessagingProcessingServiceTaskForArgentina();

		protected override void BeforeSaveOfARAPINVCRDADJTransactions(ARInvoice arInvoice, ARCreditNote arCreditNote, ARAdjustmentNote arAdjustmentNote, APInvoice apInvoice, APCreditNote apCreditNote, APAdjustmentNote apAdjustmentNote)
		{
			if (arInvoice != null)
			{
				arInvoice.AH_ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TXA;
			}
			if (arCreditNote != null)
			{
				arCreditNote.AH_ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TCA;
			}
			if (arAdjustmentNote != null)
			{
				arAdjustmentNote.AH_ComplianceSubType = ArgentinaComplianceInfo.ComplianceSubTypeCodes.TDA;
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
				};
			}
		}
	}
}
