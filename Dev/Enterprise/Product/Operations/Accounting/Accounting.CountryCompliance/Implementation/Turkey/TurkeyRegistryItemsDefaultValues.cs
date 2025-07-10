using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.Core;

namespace Enterprise.Accounting.CountryCompliance.Implementation.Turkey
{
	class TurkeyRegistryItemsDefaultValues : DefaultValuesForCountrySpecificRegistryItems
	{
		protected override string ThirdPartyEInvoiceDocType => Constants.RefDocTypes.MiscellaneousDocument;
		protected override string TaxMessageIsMandatoryPayables => Constants.TaxMessageMandatoryOptionConstants.RequiredWhenTaxHasExtraElementOrZeroTax;
		protected override string TaxMessageIsMandatoryReceivables => Constants.TaxMessageMandatoryOptionConstants.RequiredWhenTaxHasExtraElementOrZeroTax;
	}
}
