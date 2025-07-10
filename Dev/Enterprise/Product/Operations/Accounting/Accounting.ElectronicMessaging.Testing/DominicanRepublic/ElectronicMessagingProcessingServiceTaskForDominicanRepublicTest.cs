using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.ElectronicMessaging.Testing.GlobalEInvoicing;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.DominicanRepublic.Testing
{
	[TestedType(typeof(ElectronicMessagingProcessingServiceTaskForDominicanRepublic))]
	public class ElectronicMessagingProcessingServiceTaskForDominicanRepublicTest : GlobalElectronicMessagingProcessingServiceTaskTest<ElectronicMessagingProcessingServiceTaskForDominicanRepublic>
	{
		protected override ZString CountryCode => CountryCodes.DominicanRepublic;
		protected override string ExpectedMessageTypeForGenerateInvoiceRequest => DominicanRepublicEInvoiceAPICommandList.Codes.GenerateInvoiceRequest;
		protected override string ExpectedMessageTypeForGenerateCancellationRequest => DominicanRepublicEInvoiceAPICommandList.Codes.GenerateInvoiceRequest;
		protected override int ExpectedEligibleTransactionCountFromHelperARAPINVCRDADJTransactions => 2;
		protected override IReadOnlyCollection<int> ExpectedPivotsPerBatchFromHelperARAPINVCRDADJTransactions => new int[] { 1, 1 };
		protected override ElectronicMessagingProcessingServiceTaskForDominicanRepublic GetCountrySpecificServiceTask() => new ElectronicMessagingProcessingServiceTaskForDominicanRepublic();

		protected override void BeforeSaveOfARAPINVCRDADJTransactions(ARInvoice arInvoice, ARCreditNote arCreditNote, ARAdjustmentNote arAdjustmentNote, APInvoice apInvoice, APCreditNote apCreditNote, APAdjustmentNote apAdjustmentNote)
		{
			if (arInvoice != null)
			{
				arInvoice.AH_ComplianceSubType = DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.TEI;
			}
			if (arCreditNote != null)
			{
				arCreditNote.AH_ComplianceSubType = DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.TEC;
			}
			if (apInvoice != null)
			{
				apInvoice.AH_ComplianceSubType = DominicanRepublicComplianceInfo.ComplianceSubTypeCodes.XCL;
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
