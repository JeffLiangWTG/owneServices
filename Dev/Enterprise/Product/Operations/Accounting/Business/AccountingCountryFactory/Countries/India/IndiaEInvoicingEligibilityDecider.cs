using System.Diagnostics.CodeAnalysis;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class IndiaEInvoicingEligibilityDecider : IEInvoicingEligibilityDecider
	{
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Developer logs should be in English")]
		string IEInvoicingEligibilityDecider.GetAdditionalTraceLog(IEInvoicingEligibilityLiteTransaction transaction)
		{
			return @$"Compliance Sub Type ({transaction.ComplianceSubType}) is valid: {transaction.HasEligibleComplianceSubType()}
Is OrgHeader GST Registered {transaction.OrgHeader.HasAnyRegistrationCode(OrgCusCode.CodeTypes.GSTCode, CountryCodes.India)}
Is OrgHeader GST Unreported {transaction.OrgHeader.HasRegistrationCode(OrgCusCode.CodeTypes.GSTCode, CountryCodes.India, IndiaComplianceInfo.UnreportedRegistrationNumber)}
Place of Supply: {transaction.PlaceOfSupply}";
		}

		bool IEInvoicingEligibilityDecider.IsTransactionEligible(IEInvoicingEligibilityLiteTransaction transaction)
			=> transaction.Ledger == LedgerTypes.AccountsReceivable
			&& (transaction.TransactionType == TransactionTypes.Invoice || transaction.TransactionType == TransactionTypes.CreditNote)
			&& transaction.OrgHeader.HasAnyRegistrationCode(OrgCusCode.CodeTypes.GSTCode, CountryCodes.India)
			&& transaction.HasEligibleComplianceSubType()
			&& !(transaction.OrgHeader.HasRegistrationCode(OrgCusCode.CodeTypes.GSTCode, CountryCodes.India, IndiaComplianceInfo.UnreportedRegistrationNumber)
				&& transaction.PlaceOfSupply != PlaceOfSupplyListProvider.Codes.OutsideTheLoginCountry);
	}
}
