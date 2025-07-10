using Enterprise.Accounting.CountryCompliance.Implementation.Testing.CountrySpecificRegistryDefaultValues;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.CountryCompliance.Implementation.CountrySpecificRegistryDefaultValues.Testing
{
	class DominicanRepublicRegistryItemsDefaultValuesTestData : DefaultTestDataForCountrySpecificRegistryItems
	{
		public override ReadOnlyCodeDescriptionPairList EInvoicingReversalCodesDefault
			=> new ReadOnlyCodeDescriptionPairList(
				new CodeDescriptionPairList
				{
					new CodeDescriptionPair("1", (NoResString)"Anula el NCF modificado")
				});

		public override ReadOnlyCodeDescriptionPairList EInvoicingAmendmentCodesDefault
			=> new ReadOnlyCodeDescriptionPairList(
				new CodeDescriptionPairList
				{
					new CodeDescriptionPair("1", (NoResString)"Anula el NCF modificado"),
					new CodeDescriptionPair("2", (NoResString)"Corrige Texto del Comprobante Fiscal modificado"),
					new CodeDescriptionPair("3", (NoResString)"Corrige Montos del NCF modificado")
				});
	}
}
