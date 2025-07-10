using Enterprise.Accounting.CountryCompliance.Interfaces;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.CountryCompliance.Implementation.Chile
{
	class ChileRegistryItemsDefaultValues : DefaultValuesForCountrySpecificRegistryItems
	{
		protected override ReadOnlyCodeDescriptionPairList EInvoicingReversalCodes
			=> new ReadOnlyCodeDescriptionPairList(
				new CodeDescriptionPairList
				{
					new CodeDescriptionPair("1", (NoResString)"Anula Documento de Referencia"),
				});

		protected override ReadOnlyCodeDescriptionPairList EInvoicingAmendmentCodes
			=> new ReadOnlyCodeDescriptionPairList(
				new CodeDescriptionPairList
				{
					new CodeDescriptionPair("1", (NoResString)"Anula Documento de Referencia"),
					new CodeDescriptionPair("2", (NoResString)"Corrige Texto Documento de Referencia"),
					new CodeDescriptionPair("3", (NoResString)"Corrige montos")
				});
	}
}

