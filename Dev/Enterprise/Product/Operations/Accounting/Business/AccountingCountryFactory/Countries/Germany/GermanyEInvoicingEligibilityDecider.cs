using CargoWise.Application;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.Accounting.Business.EInvoicing.FeatureConfiguration;
using Enterprise.Accounting.Business.EInvoicing.Germany;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class GermanyEInvoicingEligibilityDecider : IEInvoicingEligibilityDecider
	{
		readonly EInvoicingFeatureSettingsReader settingsReader;

		public GermanyEInvoicingEligibilityDecider()
		{
			var featureControlManager = ObjectFactory.Get<IFeatureControlManager>();
			settingsReader = new EInvoicingFeatureSettingsReader(CountryCodes.Germany, featureControlManager);
		}

		string IEInvoicingEligibilityDecider.GetAdditionalTraceLog(IEInvoicingEligibilityLiteTransaction transaction)
			=> $"Transaction category: {transaction.OrgHeader.Category}, Debtor country: {transaction.InvoiceOrgAddressOverride.CountryCode}, Enabled features: {string.Join(", ", settingsReader.Value.Features)}";

		bool IEInvoicingEligibilityDecider.IsTransactionEligible(IEInvoicingEligibilityLiteTransaction transaction)
		{
			var isB2BEnabled = settingsReader.HasFeature(GermanyEInvoicingFeatureFlags.B2B);

			if (transaction.OrgHeader.Category == OrgConstants.Category.Government)
			{
				return IsTransactionEligibleForB2G(transaction);
			}
			else if (isB2BEnabled)
			{
				return IsTransactionEligibleForB2B(transaction);
			}

			// Transaction category is not government and not business (or business is not activated)
			return false;
		}

		/// <summary>
		/// A B2G transaction is eligible for Germany if
		/// - the ledger type = AccountsReceivable
		/// - the transaction is an invoice or a credit note
		/// - the debtor is flagged as governmental
		/// - the debtor is German
		/// - the invoice sender is German: check sender nationality is not needed here because it is checked in calling method
		///     => this method is only called if login company = German
		/// </summary>
		/// <returns>True if the B2G transaction is eligible to be included in eInvoicing</returns>
		bool IsTransactionEligibleForB2G(IEInvoicingEligibilityLiteTransaction transaction)
		{
			return (transaction.Ledger == LedgerTypes.AccountsReceivable)
				&& (transaction.TransactionType == TransactionTypes.Invoice || transaction.TransactionType == TransactionTypes.CreditNote)
				&& (transaction.InvoiceOrgAddressOverride.CountryCode == CountryCodes.Germany);	// Debtor country
		}

		/// <summary>
		/// A B2B transaction is eligible for Germany if
		/// - the B2B eInvoicing functionality is enabled via the feature flag
		/// - the ledger type = AccountsReceivable
		/// - the transaction is an invoice, credit note or adjustment note
		/// - the debtor is flagged as business
		/// - the debtor is German
		/// - the invoice sender is German: check sender nationality is not needed here because it is checked in calling method
		///     => this method is only called if login company = German
		/// </summary>
		/// <returns>True if the B2G transaction is eligible to be included in eInvoicing</returns>
		bool IsTransactionEligibleForB2B(IEInvoicingEligibilityLiteTransaction transaction)
		{
			return (transaction.Ledger == LedgerTypes.AccountsReceivable)
				&& (transaction.TransactionType == TransactionTypes.Invoice || transaction.TransactionType == TransactionTypes.CreditNote || transaction.TransactionType == TransactionTypes.AdjustmentNote)
				&& (transaction.InvoiceOrgAddressOverride.CountryCode == CountryCodes.Germany)	// Debtor country
				&& (transaction.OrgHeader.Category == OrgConstants.Category.Business);
		}
	}
}
