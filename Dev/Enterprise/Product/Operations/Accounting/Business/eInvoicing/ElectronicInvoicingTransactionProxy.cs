using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.Accounting.Business.EInvoicing
{
	public sealed class ElectronicInvoicingTransactionProxy : IEInvoicingTransaction
	{
		public ElectronicInvoicingTransactionProxy(AccTransactionHeader transaction)
		{
			Transaction = Argument.NotNull(transaction, nameof(transaction));
			var countryCode = transaction.Company?.GC_RN_NKCountryCode ?? ZString.Empty;

			var countryComplianceFactory = ObjectFactory.Get<ICountryComplianceFactory>();
			EInvoicingComplianceInfo = countryComplianceFactory.GetIComplianceInfoElectronicInvoicing(countryCode);

			var countryFactory = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(countryCode);
			EInvoicingRejectionFunctionalityProvider = (countryFactory as IInstanceProvider<ITransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProvider>)?.Get();
			EInvoicingPivotActionTypeProvider = (countryFactory as IInstanceProvider<IEInvoicingPivotActionTypeProvider>)?.Get();
			EInvoicingActionProvider = (countryFactory as IInstanceProvider<IEInvoicingActionProvider>)?.Get();
			EInvoicingPreEligibilityProvider = (countryFactory as IInstanceProvider<IEInvoicingPreEligibilityProvider>)?.Get() ?? new EInvoicingPreEligibilityProvider();
			EInvoicingPivotStatusProvider = (countryFactory as IInstanceProvider<IEInvoicingPivotStatusProvider>)?.Get();

			var eInvoicingObjectFactory = ObjectFactory.Get<IGlobalEInvoicingObjectFactory>();
			SupportsWaitForOriginalTransactionForAmending = eInvoicingObjectFactory.GetCountryEInvoicingBatchCreatorStrategy(countryCode)?.SupportsWaitForOriginalTransactionForAmending ?? false;

			var extensionFactory = ObjectFactory.Get<ICountryComplianceEInvoicingExtensionFactory>();
			MostRecentPivotProvider = extensionFactory.GetIMostRecentPivotProvider(countryCode);
		}

		readonly AccTransactionHeader Transaction;

		readonly IComplianceInfoElectronicInvoicing EInvoicingComplianceInfo;
		readonly IEInvoicingActionProvider EInvoicingActionProvider;
		readonly IEInvoicingPreEligibilityProvider EInvoicingPreEligibilityProvider;
		readonly ITransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProvider EInvoicingRejectionFunctionalityProvider;
		readonly IEInvoicingPivotActionTypeProvider EInvoicingPivotActionTypeProvider;
		readonly IMostRecentPivotProvider MostRecentPivotProvider;
		readonly IEInvoicingPivotStatusProvider EInvoicingPivotStatusProvider;

		readonly bool SupportsWaitForOriginalTransactionForAmending;

		#region Properties

		public ZGuid PK => Transaction.PK;

		public ZString UniqueIdentifier
			=> FormattableString.Invariant($"{Transaction.AH_Ledger} {Transaction.AH_TransactionType} {Transaction.AH_TransactionNum}");

		public ZString CurrentStatus => MostRecentPivot?.AIP_Status ?? ZString.Empty;

		public ZString CurrentError => MostRecentPivot?.AIP_ErrorDescription ?? ZString.Empty;

		public ZDateTime LastResponseReceivedUtc => MostRecentPivot?.AIP_LastResponseReceivedUtc ?? ZDateTime.Empty;

		public ZDateTime LastSentTimeUtc => MostRecentPivot?.AIP_LastSentTimeUtc ?? ZDateTime.Empty;

		public ZString BatchNumber => EInvoicingBatch?.AIB_BatchNumber.ToString() ?? ZString.Empty;

		public ZString eHubAllocatedNumber => EInvoicingBatch?.AIB_EHubAllocatedNumber ?? ZString.Empty;

		public ZString GovernmentAllocatedNumber
		{
			get
			{
				switch (EInvoicingComplianceInfo?.GetGovernmentAllocatedNumberColumnName())
				{
					case AccEInvoicingBatchSchema.Constants.AIB_GovernmentAllocatedNumber:
						return EInvoicingBatch?.AIB_GovernmentAllocatedNumber ?? ZString.Empty;
					case AccTransactionHeaderSchema.Constants.AH_GovernmentAllocatedID:
						return Transaction.AH_GovernmentAllocatedID;
					case AccTransactionHeaderReferenceSchema.Constants.AH1_Reference:
						return GetTransactionHeaderReferenceRed();
					default:
						return GetGovernmentAllocatedNumberFromAuthorisationRecord();
				}
			}
		}

		ZString GetTransactionHeaderReferenceRed()
		{
			var query = new ZQuery(AccTransactionHeaderReferenceSchema.AH1_Type, AccTransactionHeaderReferenceTypes.RED);
			query.AddToFilter(AccTransactionHeaderReferenceSchema.AH1_AH, Transaction.PK);
			var result = Transaction.Factory.LoadTop1<AccTransactionHeaderReference>(query);

			return result?.AH1_Reference ?? ZString.Empty;
		}

		ZString GetGovernmentAllocatedNumberFromAuthorisationRecord()
		{
			var governmentAllocatedNumber = ZString.Empty;
			var governmentAllocatedNumberColumnName = EInvoicingComplianceInfo?.GetGovernmentAllocatedNumberColumnName() ?? ZString.Empty;

			var authRecordType = EInvoicingComplianceInfo?.GetAccTransactionHeaderAuthorisationRecordType();
			if (!authRecordType.IsNullOrEmpty() && MostRecentPivot != null)
			{
				switch (governmentAllocatedNumberColumnName)
				{
					case AccTransactionHeaderAuthorisationRecordSchema.Constants.AHF_Number:
						var isAHF_NumberNullOrEmpty = AuthorisationRecord?.IsAHF_NumberNullOrEmpty() ?? true;
						if (!isAHF_NumberNullOrEmpty)
						{
							governmentAllocatedNumber = AuthorisationRecord?.AHF_Number ?? ZString.Empty;
						}
						break;
					default:
						break;
				}
			}

			return governmentAllocatedNumber;
		}

		public ZString AuthorisationNumber => (AuthorisationRecord?.IsAHF_NumberNullOrEmpty() ?? true) ? ZString.Empty : AuthorisationRecord.AHF_Number;

		public ZDateTimeOffset AuthorisationDateTime => (AuthorisationRecord?.IsAHF_DateTimeNullOrEmpty() ?? true) ? ZDateTimeOffset.Empty : AuthorisationRecord.AHF_DateTime;

		public AccEInvoicingTransactionPivot MostRecentPivot
		{
			get
			{
				if (mostRecentPivotValue == null || mostRecentPivotValue.AIP_Status == EInvoicingPivotState.Discarded)
				{
					if (MostRecentPivotProvider != null)
					{
						return mostRecentPivotValue = MostRecentPivotProvider.GetMostRecentPivot(this, Transaction);
					}

					var query = new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ActionType, SQLComparisonOperator.Contains, EInvoicingPivotActionType.CommandActionTypes)
									.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ParentID, PK)
									.AddToFilter(AccEInvoicingTransactionPivotSchema.AIP_ParentTableCode, AccTransactionHeaderSchema.Constants.Prefix);
					var pivots = Transaction.Factory.Load<AccEInvoicingTransactionPivot>(query);
					var sortedPivots = pivots.OrderByDescending(p => p.AIP_LastSentTimeUtc.IsEmpty ? p.AIP_SystemCreateTimeUtc : p.AIP_LastSentTimeUtc);
					mostRecentPivotValue = sortedPivots.FirstOrDefault(x => x.AIP_Status != EInvoicingPivotState.Discarded)
											?? sortedPivots.FirstOrDefault();
				}

				return mostRecentPivotValue;
			}
		}

		AccEInvoicingTransactionPivot mostRecentPivotValue;

		#endregion

		public void EvaluateEligibilityAndQueue()
		{
			if (IsEligibleForTransactionPendingAllocationApproval)
			{
				CreateNewPivot(EInvoicingPivotActionType.Approve);
			}
			else if (IsEligibleForTransactionPendingAllocationRejection)
			{
				CreateNewPivot(EInvoicingPivotActionType.Reject);
			}
			else if (CanEvaluateEligibility())
			{
				var hasAlreadyBeenQueued = CurrentStatus != ZString.Empty;
				var complianceFieldsChanged = ComplianceFieldsHaveChanged();

				if (IsEligibleToCreatePivot())
				{
					if (!hasAlreadyBeenQueued)
					{
						var pivot = CreateNewPivot();
						DiscardPivotIfEInvoicingIsDisabled(pivot);
					}
					else if (hasAlreadyBeenQueued && complianceFieldsChanged)
					{
						// Concurrency: the original pivot status field must be changed so that concurrency policy can detect if service task has run.
						DiscardPivot(MostRecentPivot, ZString.Empty);
						CreateNewPivot();
					}

					EInvoicingActionProvider?.OnEvaluateEligibilityAndQueue(Transaction);
				}
				else if (hasAlreadyBeenQueued && complianceFieldsChanged)
				{
					DiscardPivot(MostRecentPivot, AccEInvoicingTransactionPivot.ErrorDescriptionWhenNoLongerEligible());
				}
				else if (!hasAlreadyBeenQueued && (EInvoicingPivotStatusProvider?.CanCreateNotEligibleForEInvoicingPivot(Transaction) ?? false))
				{
					CreateNewPivot(actionType: string.Empty, EInvoicingPivotState.NotEligible);
				}
			}
		}

		public bool IsEligibleToCreatePivot()
			=> ElectronicInvoicingEligibilityDecider.IsEligible(Transaction);

		public bool IsPreEInvoicingTransaction() => HasSpecialPivotWhenEInvoicingIsManuallyDisabled() || !HasEReportingComplianceDateReached();

		public bool HasEReportingComplianceDateReached()
		{
			var complianceDate = GetEInvoicingComplianceDate();
			return EInvoicingPreEligibilityProvider.CanEvaluateByComplianceDate(Transaction, complianceDate);
		}

		#region Implementation

		bool IsEInvoicingEnabled => AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.GetValueWithoutFallback(Transaction.AH_GC.ToGuid(), Guid.Empty, Guid.Empty);

		DateTime GetEInvoicingComplianceDate()
		{
			if (Transaction.AH_Ledger == LedgerTypes.AccountsPayable)
			{
				return AccountingMasterFilesRegistry.Instance.EReportingComplianceDateForPayables.GetValueWithoutFallback(Transaction.AH_GC.ToGuid(), Guid.Empty, Guid.Empty);
			}
			else
			{
				return AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.GetValueWithoutFallback(Transaction.AH_GC.ToGuid(), Guid.Empty, Guid.Empty);
			}
		}

		public bool CanEvaluateEligibility()
		{
			var canQueueByComplianceDate = HasEReportingComplianceDateReached();
			if (!canQueueByComplianceDate)
			{
				return false;
			}

			var canQueueByTransaction = EInvoicingPreEligibilityProvider.CanEvaluateByTransaction(Transaction);
			if (!canQueueByTransaction)
			{
				return false;
			}

			if (this.IsInvalidStatusToReQueue())
			{
				return false;
			}

			return true;
		}

		bool IsEligibleForTransactionPendingAllocationApproval =>
			Transaction is InvoicingBase invoicingBase
			&& EInvoicingRejectionFunctionalityProvider != null
			&& invoicingBase.HasPendingApprovalRequest()
			&& IsEligibleToCreatePivot()
			&& EInvoicingRejectionFunctionalityProvider.IsTransactionEligibleToCreateApprovalRequest(Transaction);

		bool IsEligibleForTransactionPendingAllocationRejection =>
			Transaction is InvoicingBase invoicingBase
			&& EInvoicingRejectionFunctionalityProvider != null
			&& invoicingBase.HasPendingRejectionRequest()
			&& IsEligibleToCreatePivot()
			&& EInvoicingRejectionFunctionalityProvider.IsTransactionEligibleToCreateRejectionRequest(Transaction);

		bool ComplianceFieldsHaveChanged()
			=> Transaction.AH_TransactionReferenceInfo.HasChanges
			|| Transaction.AH_ComplianceSubTypeInfo.HasChanges;

		public AccEInvoicingTransactionPivot CreateNewPivot(string actionType = "", string pivotStatus = "")
		{
			var transactionPivot = Transaction.Factory.New<AccEInvoicingTransactionPivot>();
			transactionPivot.AIP_ParentID = PK;
			transactionPivot.SetCompanyAndCountryCode(Transaction.Company);
			transactionPivot.AIP_Status = !string.IsNullOrEmpty(pivotStatus) ? pivotStatus : GetInitialPivotStatus();
			transactionPivot.AIP_ActionType = !string.IsNullOrEmpty(actionType) ? actionType : GetPivotActionType();
			mostRecentPivotValue = transactionPivot;
			return transactionPivot;
		}

		void DiscardPivot(AccEInvoicingTransactionPivot pivot, ZString description)
		{
			pivot.AIP_Status = EInvoicingPivotState.Discarded;
			pivot.AIP_ErrorDescription = description;
		}

		void DiscardPivotIfEInvoicingIsDisabled(AccEInvoicingTransactionPivot pivot)
		{
			bool isAP = Transaction.AH_Ledger == LedgerTypes.AccountsPayable;
			if (isAP && !IsPayablesEInvoicingEnabled || !isAP && !IsEInvoicingEnabled)
			{
				var registry = AccountingMasterFilesRegistry.Instance;
				var registryItem = isAP ? registry.EnableEInvoicingFunctionalityForPayables : registry.EnableEInvoicingFunctionality;
				var description = FormattableString.Invariant($"E-Reporting/E-Invoicing is disabled for this login company. Please review the Registry: {registryItem.HumanReadableRegistryPath()}");
				DiscardPivot(pivot, description);
			}
			else if (pivot.AIP_Status == EInvoicingPivotState.Discarded)
			{
				var descriptionDCD = FormattableString.Invariant($"Non-tax transactions are not eligible for E-Invoicing");
				DiscardPivot(pivot, descriptionDCD);
			}
		}

		string GetPivotActionType() => EInvoicingPivotActionTypeProvider?.GetPivotActionType(Transaction) ?? GetPivotActionTypeDefault();

		string GetPivotActionTypeDefault()
		{
			//🚩🚩🚩 No new country specific logic is to be added here. If this logic is not correct for your country, please use EInvoicingPivotActionTypeProvider.

			//WARNING: When eligibility is evaluated from GovernmentInvoice bizo, Transaction cannot be cast to TransactionHeader successfully.
			//Relying on TransactionHeader is dangerous if overriding compliance sub-type is possible for your country via ClassAInvoiceForm.

			var th = Transaction as TransactionHeader;
			if ((th?.Company?.GC_RN_NKCountryCode ?? ZString.Empty) == CountryCodes.KoreaSouth)
			{
				return EInvoicingPivotActionType.Submit;
			}
			else if (Transaction.IsReversal())
			{
				// India: No differentiation between Reversal or Amending transactions in this predicate and it does not care.
				return EInvoicingPivotActionType.Cancel;
			}
			else if (((th?.IsAmendingCreditNote ?? false) || ((th?.IsAmendingInvoice ?? false) && AccountingConfigurationRegistry.Instance.VietnamIssuePositiveAdjustmentViaAmendWithInvoice.Value)) && (th.Company?.GC_RN_NKCountryCode ?? ZString.Empty) == CountryCodes.VietNam)
			{
				return EInvoicingPivotActionType.Adjustment;
			}
			else if ((th?.IsAmendingTransaction ?? false) && SupportsWaitForOriginalTransactionForAmending)
			{
				return EInvoicingPivotActionType.Amend;
			}
			else
			{
				return EInvoicingPivotActionType.Submit;
			}
		}

		string GetInitialPivotStatus()
		{
			var initialPivotStatus = EInvoicingPivotStatusProvider?.GetInitialPivotStatus((ITransactionHeader)Transaction);
			if (!string.IsNullOrEmpty(initialPivotStatus))
			{
				return initialPivotStatus;
			}

			var registry = AccountingMasterFilesRegistry.Instance;
			var gcGuid = Transaction.AH_GC.ToGuid();
			initialPivotStatus =
				Transaction.AH_Ledger == LedgerTypes.AccountsReceivable
				? registry.EReportingSubmitPivotDefaultStatus.GetValueWithoutFallback(gcGuid, Guid.Empty, Guid.Empty)
				: Transaction.AH_Ledger == LedgerTypes.AccountsPayable
					? registry.EReportingSubmitPivotDefaultStatusForPayables.GetValueWithoutFallback(gcGuid, Guid.Empty, Guid.Empty)
					: string.Empty;

			if (string.IsNullOrEmpty(initialPivotStatus))
			{
				initialPivotStatus = EInvoicingPivotState.Queued;
			}
			return initialPivotStatus;
		}

		bool IsPayablesEInvoicingEnabled => AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.GetValueWithoutFallback(Transaction.AH_GC.ToGuid(), Guid.Empty, Guid.Empty);

		AccEInvoicingBatch EInvoicingBatch
		{
			get
			{
				var batchPK = MostRecentPivot?.AIP_AIB ?? ZGuid.Empty;
				return batchPK.IsValid ? Transaction.Factory.Load<AccEInvoicingBatch>(batchPK) : null;
			}
		}

		AccTransactionHeaderAuthorisationRecord AuthorisationRecord
		{
			get
			{
				if (authorisationRecord == null)
				{
					var authRecordType = EInvoicingComplianceInfo?.GetAccTransactionHeaderAuthorisationRecordType();
					if (!string.IsNullOrEmpty(authRecordType))
					{
						var query = new ZQuery(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentTableCode, AccTransactionHeaderSchema.Constants.Prefix);
						query.AddToFilter(AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentId, PK);
						query.AddToFilter(AccTransactionHeaderAuthorisationRecordSchema.AHF_RecordType, authRecordType);
						authorisationRecord = Transaction.Factory.LoadTop1<AccTransactionHeaderAuthorisationRecord>(query);
					}
				}

				return authorisationRecord;
			}
		}

		AccTransactionHeaderAuthorisationRecord authorisationRecord;

		bool HasSpecialPivotWhenEInvoicingIsManuallyDisabled()
		{
			var pivot = MostRecentPivot;
			return pivot != null
				&& pivot.AIP_Status == EInvoicingPivotState.Discarded
				&& pivot.AIP_ErrorDescription.StartsWith(AccEInvoicingTransactionPivot.BaseErrorDescriptionWhenEInvoicingDisabled);
		}

		#endregion
	}
}
