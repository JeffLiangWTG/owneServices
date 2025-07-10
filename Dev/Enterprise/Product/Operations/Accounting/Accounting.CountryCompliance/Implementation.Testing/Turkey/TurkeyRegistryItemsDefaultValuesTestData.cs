using CargoWise.Types;
using Enterprise.Accounting.CountryCompliance.Implementation.Testing.CountrySpecificRegistryDefaultValues;
using Enterprise.Core;

namespace Enterprise.Accounting.CountryCompliance.Implementation.CountrySpecificRegistryDefaultValues.Testing
{
	class TurkeyRegistryItemsDefaultValuesTestData : DefaultTestDataForCountrySpecificRegistryItems
	{
		public override ZString ThirdPartyEInvoiceDocType => Constants.RefDocTypes.MiscellaneousDocument;

		public override string TaxMessageIsMandatoryPayables => Constants.TaxMessageMandatoryOptionConstants.RequiredWhenTaxHasExtraElementOrZeroTax;

		public override string TaxMessageIsMandatoryReceivables => Constants.TaxMessageMandatoryOptionConstants.RequiredWhenTaxHasExtraElementOrZeroTax;
	}
}
