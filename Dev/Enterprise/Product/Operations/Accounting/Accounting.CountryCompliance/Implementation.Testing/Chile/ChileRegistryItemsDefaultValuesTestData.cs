using Enterprise.Accounting.CountryCompliance.Implementation.Testing.CountrySpecificRegistryDefaultValues;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.CountryCompliance.Implementation.CountrySpecificRegistryDefaultValues.Testing
{
	class ChileRegistryItemsDefaultValuesTestData : DefaultTestDataForCountrySpecificRegistryItems
	{
		public override ReadOnlyCodeDescriptionPairList EInvoicingReversalCodesDefault
		{
			get
			{
				return new ReadOnlyCodeDescriptionPairList(
						new CodeDescriptionPairList
						{
							new CodeDescriptionPair("1", (NoResString)"Anula Documento de Referencia")
						}
					);
			}
		}

		public override ReadOnlyCodeDescriptionPairList EInvoicingAmendmentCodesDefault
		{
			get
			{
				return new ReadOnlyCodeDescriptionPairList(
						new CodeDescriptionPairList
						{
							new CodeDescriptionPair("1", (NoResString)"Anula Documento de Referencia"),
							new CodeDescriptionPair("2", (NoResString)"Corrige Texto Documento de Referencia"),
							new CodeDescriptionPair("3", (NoResString)"Corrige montos")
						}
					);
			}
		}
	}
}
