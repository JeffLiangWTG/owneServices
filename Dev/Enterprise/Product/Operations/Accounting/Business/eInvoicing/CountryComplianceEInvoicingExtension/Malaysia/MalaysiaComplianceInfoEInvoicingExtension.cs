using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.EInvoicing
{
	public class MalaysiaComplianceInfoEInvoicingExtension : MalaysiaComplianceInfo,
		IEInvoicingTransactionValidation,
		IExistPivotCheckProvider
	{
		#region IEInvoicingTransactionValidation

		public ZString GetCantAmendErrorMessage(InvoicingBase originalTransaction, string transactionType) => ZString.Empty;

		public ZString GetCantReverseErrorMessage(IReversing originalTransaction)
		{
			if (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.Value
				&& originalTransaction is InvoicingBase invoicingBase && !invoicingBase.IsReversed
				&& invoicingBase.IsARAPInvoiceOrCreditNote
				&& !invoicingBase.EInvoicingStatus.IsEmpty
				&& invoicingBase.EInvoicingStatus != EInvoicingPivotState.Discarded
				&& !ExistDocumentDetailsPivot(invoicingBase.Factory, invoicingBase.PK))
			{
				return Res.GetString("3a3ce9bc-3243-4e52-886b-952197483f0c", "Reversing is not allowed if the transaction is not taken as Invalid. If applicable, please Amend the Original Transaction in Job Billing module.");
			}

			return ZString.Empty;
		}

		public ZString GetValidationMessageForAfterPostAction(IEnumerable<AccEInvoicingTransactionPivot> pivots, string actionType)
		{
			switch (actionType)
			{
				case EInvoicingPivotActionType.DocumentAction:
					return GetValidationMessageForDocumentAction(pivots);
				case EInvoicingPivotActionType.StatusCheck:
					return GetValidationMessageForStatusCheckAction(pivots);
				default:
					return string.Empty;
			}
		}

		ZString GetValidationMessageForStatusCheckAction(IEnumerable<AccEInvoicingTransactionPivot> pivots)
		{
			var allowedStatuses = new ZString[] { EInvoicingPivotState.Delivered, EInvoicingPivotState.Failed, EInvoicingPivotState.InProcessing };
			var submissionPivot = pivots.FirstOrDefault(x => x.AIP_ActionType == EInvoicingPivotActionType.Submit);

			var parentTransactionHeader = pivots.FirstOrDefault().ParentTransactionHeader;
			if (parentTransactionHeader.IsReversed)
			{
				return ReversedMessage;
			}
			else if (!allowedStatuses.Contains(submissionPivot.AIP_Status) || (submissionPivot.AIP_Status == EInvoicingPivotState.Failed && parentTransactionHeader.EInvoicingGovernmentAllocatedNumber.IsEmpty))
			{
				return Res.GetString("4D4F4873-155B-41BB-BC5E-EDF3E32BBA13", "The transaction is not eligible for requests because the E-Reporting Status of the transaction is not 'DLV - Delivered' or 'IMP - In Processing' or 'FAL - Failed' with E-Reporting Govt #.");
			}

			return ZString.Empty;
		}

		ZString GetValidationMessageForDocumentAction(IEnumerable<AccEInvoicingTransactionPivot> pivots)
		{
			var message = ZString.Empty;

			if (pivots.FirstOrDefault().ParentTransactionHeader.IsReversed)
			{
				message = ReversedMessage;
			}
			else if (!pivots.Any(x => x.AIP_ActionType == EInvoicingPivotActionType.Submit && x.AIP_Status == EInvoicingPivotState.Succeed))
			{
				message = NotSucceedMessage;
			}

			return message;
		}

		readonly ZString NotSucceedMessage = Res.GetString("5941744-ACF9-4542-BA51-25190FB0612C", "The transaction is not eligible for requests as the E-Reporting Status of the transaction is not equal to SUC.");

		readonly ZString ReversedMessage = Res.GetString("ECB44974-1DAC-498B-B8C6-779108390755", "The transaction is not eligible for requests as the transaction has been reversed.");

		bool ExistDocumentDetailsPivot(BusinessObjectFactory factory, ZGuid transactionPk)
		{
			var zQuery = new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, transactionPk)
				.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ActionType, EInvoicingPivotActionType.DocumentDetail)
				.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ParentTableCode, AccTransactionHeaderSchema.Constants.Prefix)
				.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_Status, SQLComparisonOperator.NotEqual, EInvoicingPivotState.Discarded);
			return factory.Exists(typeof(AccEInvoicingTransactionPivot), zQuery);
		}

		#endregion

		#region IExistPivotCheckProvider

		public bool CheckExistActivePivot(AccEInvoicingTransactionPivot pivot)
		{
			return pivot.AIP_LastResponseReceivedUtc.IsEmpty && (pivot.AIP_LastSentTimeUtc.IsEmpty || pivot.AIP_LastSentTimeUtc >= ZDateTime.UtcNow.AddMinutes(-30));
		}

		public (bool, ZString) CanExistSucceedPivot()
		{
			return (true, ZString.Empty);
		}

		#endregion
	}
}
