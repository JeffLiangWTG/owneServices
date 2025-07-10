using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.CountryCompliance.Implementation.DominicanRepublic
{
	class DominicanRepublicRegistryItemsDefaultValues : DefaultValuesForCountrySpecificRegistryItems
	{
		protected override ReadOnlyCodeDescriptionPairList EInvoicingReversalCodes
			=> new CodeDescriptionPairList() { new CodeDescriptionPair("1", (NoResString)"Anula el NCF modificado") };

		protected override ReadOnlyCodeDescriptionPairList EInvoicingAmendmentCodes
			=> new ReadOnlyCodeDescriptionPairList(
				new CodeDescriptionPairList
				{
					new CodeDescriptionPair("1", (NoResString)"Anula el NCF modificado"),
					new CodeDescriptionPair("2", (NoResString)"Corrige Texto del Comprobante Fiscal modificado"),
					new CodeDescriptionPair("3", (NoResString)"Corrige Montos del NCF modificado")
				});
	}
}
