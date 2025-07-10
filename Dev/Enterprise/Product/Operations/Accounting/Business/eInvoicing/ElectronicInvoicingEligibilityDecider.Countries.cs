using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.EInvoicing
{
	public partial class ElectronicInvoicingEligibilityDecider
	{
		bool ItalySpecificTransactionHeaderEligibilityFunction(ITransactionHeaderWrapper header)
		=> (header.AH_Ledger == LedgerTypes.AccountsReceivable
				|| (header.AH_Ledger == LedgerTypes.AccountsPayable && header.Lines.Any(x => x.TaxRate != null && x.TaxRate.IsRVS)))
				&& (header.AH_TransactionType == TransactionTypes.Invoice
					|| header.AH_TransactionType == TransactionTypes.CreditNote
					|| header.AH_TransactionType == TransactionTypes.AdjustmentNote);

		bool TaxCoreSpecificTransactionHeaderEligibilityFunction(ITransactionHeaderWrapper header)
			=> header.AH_Ledger == LedgerTypes.AccountsReceivable
			&& (header.AH_TransactionType == TransactionTypes.Invoice || header.AH_TransactionType == TransactionTypes.CreditNote)
			&& !header.AH_IsDisbursementCalc
			&& !header.Lines.All(line => line.TaxRate?.IsNotReportable ?? true);

		bool TaiwanSpecificComplianceDocumentHeaderEligibilityFunction(IComplianceDocumentHeaderWrapper header)
			=> header.ADH_Ledger == LedgerTypes.AccountsReceivable
			&& (header.ADH_TransactionType == TransactionTypes.Invoice || header.ADH_TransactionType == TransactionTypes.CreditNote)
			&& TaiwanComplianceInfo.GetEInvoicingEligibleComplianceSubTypeList().Contains(header.ADH_ComplianceSubType);

		bool TurkeySpecificTransactionHeaderEligibilityFunction(ITransactionHeaderWrapper header)
			=> new ElectronicInvoicingEligibilityDeciderForTurkey(header, () => IsEligibleComplianceSubType(Constants.CountryCodes.Turkey, header.AH_ComplianceSubType)).IsTransactionHeaderEligible;

		bool HungarySpecificTransactionHeaderEligibilityFunction(ITransactionHeaderWrapper header)
		{
			return header.AH_Ledger == LedgerTypes.AccountsReceivable
				&& (header.AH_TransactionType == TransactionTypes.Invoice || header.AH_TransactionType == TransactionTypes.CreditNote);
		}

		bool UruguaySpecificTransactionHeaderEligibilityFunction(ITransactionHeaderWrapper header)
			=> header.AH_Ledger == LedgerTypes.AccountsReceivable
			&& (header.AH_TransactionType == TransactionTypes.Invoice || header.AH_TransactionType == TransactionTypes.CreditNote)
			&& (IsEligibleComplianceSubType(Constants.CountryCodes.Uruguay, header.AH_ComplianceSubType));

		bool ArgentinaSpecificTransactionHeaderEligibilityFunction(ITransactionHeaderWrapper header)
			=> header.AH_Ledger == LedgerTypes.AccountsReceivable
			&& (header.AH_TransactionType == TransactionTypes.Invoice || header.AH_TransactionType == TransactionTypes.CreditNote)
			&& IsEligibleComplianceSubType(Constants.CountryCodes.Argentina, header.AH_ComplianceSubType);

		#region Vietnam

		bool VietnamSpecificTransactionHeaderEligibilityFunction(ITransactionHeaderWrapper header)
			=> header.AH_Ledger == LedgerTypes.AccountsReceivable
			&& (IsEligibleVNInvoice(header) || IsEligibleVNCreditNotes(header));

		bool IsEligibleVNInvoice(ITransactionHeaderWrapper header)
		{
			return header.AH_TransactionType == TransactionTypes.Invoice
				&& IsEligibleComplianceSubType(Constants.CountryCodes.VietNam, header.AH_ComplianceSubType)
				&& !string.IsNullOrEmpty(header.AH_TransactionReference);
		}

		bool IsEligibleVNCreditNotes(ITransactionHeaderWrapper header)
		{
			return header.AH_TransactionType == TransactionTypes.CreditNote
				&& header.GetAnyRelatedTransactionsWereEligible()
				&& (IsEligibleVNCancelRequest(header) || IsEligibleVNAdjustmentRequest(header));
		}

		bool IsEligibleVNCancelRequest(ITransactionHeaderWrapper header)
		{
			return header.AH_IsCancelled;
		}

		bool IsEligibleVNAdjustmentRequest(ITransactionHeaderWrapper header)
		{
			return !string.IsNullOrEmpty(header.AH_TransactionReference)
				&& header.IsAmendingCreditNote
				&& AccountingConfigurationRegistry.Instance.VietnamEInvoicingAdjustment.Value;
		}

		#endregion

		bool MexicoSpecificTransactionHeaderEligibilityFunction(ITransactionHeaderWrapper header)
			=> header.AH_Ledger == LedgerTypes.AccountsReceivable
			&& (header.AH_TransactionType == TransactionTypes.Invoice || header.AH_TransactionType == TransactionTypes.CreditNote)
			&& IsEligibleComplianceSubType(Constants.CountryCodes.Mexico, header.AH_ComplianceSubType);

		bool KoreaSouthSpecificTransactionHeaderEligibilityFunction(ITransactionHeaderWrapper header)
		{
			if (header.AH_Ledger != LedgerTypes.AccountsReceivable
				|| (header.AH_TransactionType != TransactionTypes.Invoice && header.AH_TransactionType != TransactionTypes.CreditNote))
			{
				return false;
			}

			if (header.AH_TransactionType == TransactionTypes.CreditNote && string.IsNullOrEmpty(header.OriginalTransaction?.EInvoicingStatus))
			{
				return false;
			}

			if (header.AH_TransactionType == TransactionTypes.Invoice && IsReversalTransactionOfStandAloneARCreditNote())
			{
				return false;
			}

			if(!IsComplianceSubTypeOrTypeCodeEligibility(header))
			{
				return false;
			}

			bool IsComplianceSubTypeOrTypeCodeEligibility(ITransactionHeaderWrapper header)
			{
				var typeCode = KoreaSouthEInvoicingHelper.GetTaxInvoiceDocumentTypeCode(header.AH_ComplianceSubType, header.Lines.Select(x => x.TaxRate?.TaxType).WhereNotNull(),
					() => header.HasOriginalTransaction, header.AH_GSTAmount);

				return IsComplianceSubTypeFeatureEnabled()
				   ? !header.AH_ComplianceSubType.IsEmpty
				   : !string.IsNullOrEmpty(typeCode);
			}

			bool IsReversalTransactionOfStandAloneARCreditNote()
			{
				return header.IsReverseTransaction
						&& header.HasOriginalTransaction
						&& header.OriginalTransaction.AH_Ledger == LedgerTypes.AccountsReceivable
						&& header.OriginalTransaction.AH_TransactionType == TransactionTypes.CreditNote
						&& !header.OriginalTransaction.HasOriginalTransaction;
			}

			return true;
		}

		bool BrazilSpecificTransactionHeaderEligibilityFunction(ITransactionHeaderWrapper header)
			=> header.AH_Ledger == LedgerTypes.AccountsReceivable
			&& (header.AH_TransactionType == TransactionTypes.Invoice
				|| (header.AH_TransactionType == TransactionTypes.CreditNote && header.AH_IsCancelled))
			&& IsEligibleComplianceSubType(Constants.CountryCodes.Brazil, header.AH_ComplianceSubType)
			&& header.TaxTransactions.Any(x => x.TaxSystemCode == "ISS");

		bool EgyptSpecificTransactionHeaderEligibilityFunction(ITransactionHeaderWrapper header)
			=> header.AH_Ledger == LedgerTypes.AccountsReceivable
			&& (header.AH_TransactionType == TransactionTypes.Invoice || header.AH_TransactionType == TransactionTypes.CreditNote);

		bool IsEligibleComplianceSubType(string countryCode, ZString complianceSubType) =>
			CountryComplianceFactory.GetICountryEligibleComplianceSubTypeForEInvoice(countryCode)?.IsComplianceSubTypeElegibleForEInvoicing(complianceSubType) ?? false;

		bool IsComplianceSubTypeFeatureEnabled()
		{
			var featureData = ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(CargoWise.Definitions.LicenceFeatureCodeList.Codes.AccountingEInvoicingKoreaSouthComplianceSubTypeFeature);

			if (featureData != null && featureData.TryDeserializeParameterAsJson<AccountingEInvoicingKoreaSubTypesFeatureControlData>(out var subTypesFeatureControlData))
			{
				return subTypesFeatureControlData?.IsComplianceSubTypeFeatureEnabled ?? false;
			}

			return false;
		}
	}
}
