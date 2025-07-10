using System;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.Accounting.Registry.Business
{
	public sealed class AccountingRegistryProvider : IAccountingRegistryProvider
	{
		bool IAccountingRegistryProvider.EnableControllingAgentFunctionalityAndValidations
		{
			get => OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.Value;
#if DEBUG
			set => OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
#endif
		}

		bool IAccountingRegistryProvider.EnableControllingCustomerFunctionalityAndValidations
		{
			get => OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.Value;
#if DEBUG
			set => OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
#endif
		}

		bool IAccountingRegistryProvider.EnableBulkDisbursementJobsClosure
		{
			get => AccountingConfigurationRegistry.Instance.EnableBulkDisbursementJobsClosure.Value;
#if DEBUG
			set => AccountingConfigurationRegistry.Instance.EnableBulkDisbursementJobsClosure.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
#endif
		}

		bool IAccountingRegistryProvider.GetAllowFuturePostingOfCashBookTransactions(Guid companyPk)
		{
			return AccountingConfigurationRegistry.Instance.AllowFuturePostingOfCashBookTransactions.GetValueWithoutFallback(companyPk, Guid.Empty, Guid.Empty);
		}

		DateTime IAccountingRegistryProvider.LastUTCDateToDisableComplianceBookAfterDbRestored
		{
			get => AccountingMasterFilesRegistry.Instance.LastUTCDateToDisableComplianceBookAfterDbRestored.Value;
#if DEBUG
			set => AccountingMasterFilesRegistry.Instance.LastUTCDateToDisableComplianceBookAfterDbRestored.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
#endif
		}

		bool IAccountingRegistryProvider.EnableTaxBranchFeature
		{
			get => AccountingMasterFilesRegistry.Instance.EnableTaxBranchFeature.Value;
#if DEBUG
			set => AccountingMasterFilesRegistry.Instance.EnableTaxBranchFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
#endif
		}

		public bool EnableReceivablesCashAdvanceFunctionality
		{
			get => AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.Value;
#if DEBUG
			set => AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
#endif
		}

		bool IAccountingRegistryProvider.EnablePayablesInvoiceProcessingPortal
		{
			get => AccountingConfigurationRegistry.Instance.EnablePayablesInvoiceProcessingPortal.Value;
#if DEBUG
			set => AccountingConfigurationRegistry.Instance.EnablePayablesInvoiceProcessingPortal.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
#endif
		}

		bool IAccountingRegistryProvider.EnableADAW
		{
			get => GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.Value;
#if DEBUG
			set => GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
#endif
		}

		Guid IAccountingRegistryProvider.ElectronicProcessingChargeCode
		{
			get => AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.Value;
#if DEBUG
			set => AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
#endif
		}

		Guid IAccountingRegistryProvider.ElectronicProcessingChargeDisbursementClearingAccount
		{
			get => AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeDisbursementClearingAccount.Value;
#if DEBUG
			set => AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeDisbursementClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
#endif
		}

		Guid IAccountingRegistryProvider.ElectronicProcessingChargePayableClearingAccount
		{
			get => AccountingConfigurationRegistry.Instance.ElectronicProcessingChargePayableClearingAccount.Value;
#if DEBUG
			set => AccountingConfigurationRegistry.Instance.ElectronicProcessingChargePayableClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
#endif
		}

		bool IAccountingRegistryProvider.EnableAssetManagementFunctionality
		{
			get => AccountingMasterFilesRegistry.Instance.EnableAssetManagementFunctionality.Value;
#if DEBUG
			set => AccountingMasterFilesRegistry.Instance.EnableAssetManagementFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
#endif
		}

		bool IAccountingRegistryProvider.EnableGenerateJournalEntriesForPostedAccountingTransactions
		{
			get => AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.Value;
#if DEBUG
			set => AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
#endif
		}
}
}
