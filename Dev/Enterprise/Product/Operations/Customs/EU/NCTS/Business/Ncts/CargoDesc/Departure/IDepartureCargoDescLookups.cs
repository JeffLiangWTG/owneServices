using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using OrgSupplierPartCollection = Enterprise.Customs.Business.OrgSupplierPartCollection;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public interface IDepartureCargoDescLookups
	{
		CodeDescriptionPairList CachedListOfAdditionalCodeDescriptions { get; }

		ConsigneeCollection ConsigneeList { get; }

		CodeDescriptionPairList CountryOfDestinationList { get; }

		CodeDescriptionPairList CountryOfDispatchList { get; }

		CodeDescriptionPairList CountryOfOriginList { get; }

		CodeDescriptionPairList CustomsUnitOfQuantityList { get; }

		ZZRefCusCodeListCombinedCollection CusCodeList { get; }

		RefCurrencyCollection Currencies { get; }

		RefCurrencyCollection LinePriceCurrencies { get; }

		CodeDescriptionPairList DeclarationTypeList { get; }

		TariffViewCollection Tariffs { get; }

		CodeDescriptionPairList TaxOrFeeCodeList { get; }

		CodeDescriptionPairList TransportChargesModeOfPaymentList { get; }

		CodeDescriptionPairList WeightUnitList { get; }
		CodeDescriptionPairList AdditionalCodeList { get; }

		OrgSupplierPartCollection Parts { get; }

		CodeDescriptionPairList BondedWhsUnitQtyList { get; }
	}
}
