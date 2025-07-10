using System;

namespace Enterprise.Integration
{
	public interface IAccountingRegistryProvider
	{
		bool EnableBulkDisbursementJobsClosure
		{
			get;
#if DEBUG
			set;
#endif
		}
		bool GetAllowFuturePostingOfCashBookTransactions(Guid companyPk);
		bool EnableControllingAgentFunctionalityAndValidations
		{
			get;
#if DEBUG
			set;
#endif
		}
		bool EnableControllingCustomerFunctionalityAndValidations
		{
			get;
#if DEBUG
			set;
#endif
		}

		DateTime LastUTCDateToDisableComplianceBookAfterDbRestored
		{
			get;
#if DEBUG
			set;
#endif
		}

		bool EnableTaxBranchFeature
		{
			get;
#if DEBUG
			set;
#endif
		}

		bool EnableReceivablesCashAdvanceFunctionality
		{
			get;
#if DEBUG
			set;
#endif
		}

		bool EnablePayablesInvoiceProcessingPortal
		{
			get;
#if DEBUG
			set;
#endif
		}

		bool EnableADAW
		{
			get;
#if DEBUG
			set;
#endif
		}

		Guid ElectronicProcessingChargeCode
		{
			get;
#if DEBUG
			set;
#endif
		}

		Guid ElectronicProcessingChargeDisbursementClearingAccount
		{
			get;
#if DEBUG
			set;
#endif
		}

		Guid ElectronicProcessingChargePayableClearingAccount
		{
			get;
#if DEBUG
			set;
#endif
		}
		bool EnableAssetManagementFunctionality
		{
			get;
#if DEBUG
			set;
#endif
		}
		bool EnableGenerateJournalEntriesForPostedAccountingTransactions
		{
			get;
#if DEBUG
			set;
#endif
		}
	}
}
