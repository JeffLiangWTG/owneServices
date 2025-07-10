using CargoWiseOne.ResourceStrings;

namespace Enterprise.Integration.Compliance
{
	public interface ICountryComplianceInfo
	{
		string GetBusinessRegistrationCode();
		string GetConsumptionTaxCode();
		string GetConsumptionTaxRegistrationCode();
		string GetComplianceSequencePrefixErrorMessage();
		string GetComplianceSequencePrefixRegex();
		string GetLocalBusinessRegNoCodeType();
		string GetRecipientLocalBusinessRegNumberCodeType();
		string GetRecipientLocalBusinessReg2NumberCodeType();
		string GetRecipientLocalBusinessRegHeading();
		string GetRecipientLocalBusinessReg2Heading();
		bool? GetIsReciprocal();
		bool? GetIsRightHandSideAdressCountry();
		string GetRecipientTaxIDHeading();
		bool? HasExtraTaxInfo();
		string GetExtraTaxDescription(string extraTaxTypeCode);
		string GetComplianceVersionNo();
		ResourceStringData GetExtraTaxOSAmountCaption();
		ResourceStringData GetExtraTaxLocalAmountCaption();
		ResourceStringData GetTaxOSAmountCaption();
		ResourceStringData GetTaxLocalAmountCaption();
		bool GetIsTransactionSequencingRequired(string ledger, string transactionType);

		#region Registry Item Defaults

		// NOTE: prefer to use IComplianceRegistryDefaultProvider in MasterFiles rather than adding more defaults here.

		bool? GetDefaultValueForDisplayRecipientTaxIDRegistry();

		#endregion
	}
}
