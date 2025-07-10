using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public abstract class NctsCommonCargoDescLookups : CusInBondCargoDescLookups
	{
		public NctsCommonCargoDescLookups(NctsCommonCargoDesc parent)
			: base(parent)
		{
		}

		public TariffViewCollection Tariffs => TariffViewCollection.GetCachedCollection(Factory, Parent.DataGroupingCode, Parent.TariffType, EffectiveDate);

		public CodeDescriptionPairList WeightUnitList => new CodeDescriptionPairList(OLookUpEditType.Weight);

		new NctsCommonCargoDesc Parent => (NctsCommonCargoDesc)base.Parent;

		public ZZRefCusCodeListCombinedCollection CusCodeList
		{
			get
			{
				var listAttributeFilers = Parent.Header?.Configuration?.GoodsItemsConfiguration?.GetCusCodeListAttributeFilters() ?? [];

				var collection = ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory,
					CusCodeListDataGroupingCode,
					[EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS],
					EffectiveDate,
					listAttributeFilers);

				var filterBusinessObjectDefaults = collection.FilterBusinessObjectDefaults;
				filterBusinessObjectDefaults.RemoveAll();

				var harmonisedTariff = Parent.BY_HarmonisedTariff;
				if (!harmonisedTariff.IsEmpty)
				{
					filterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.AttributeName,
						"Property",
						(ZString)Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CombinedNomenclatureCode,
						isRemovable: false));
					filterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.ZZRefCusCodeListFilters.AttributeValue,
						"Property",
						harmonisedTariff.SubstringSafe(0, 6),
						isRemovable: false));
				}

				return collection;
			}
		}

		public ZString CusCodeListDataGroupingCode => CusCodeListDataGroupingCodeCore;

		protected virtual ZString CusCodeListDataGroupingCodeCore => Parent.DataGroupingCode;

		protected ZDateTime EffectiveDate => Parent.ValuationDate;

		public CodeDescriptionPairList CountryOfOriginList => Factory.GetCachedCountryNC008List(Parent.DataGroupingCode);

		public CodeDescriptionPairList CustomsUnitOfQuantityList => RefCusCodeListTypes.GetCachedList(Factory, Parent.DataGroupingCode, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, Parent.ValuationDate);

		public CodeDescriptionPairList AdditionalCodeList => Customs.Business.UniversalReferenceDataHelper.GetDynamicAdditionalCodeList(Parent.UniversalTariff, CachedListOfAdditionalCodeDescriptions, new IZZRateSelectionCriteria[] { Parent.AllApplicableRatesSelectionCriteria }, new IZZConditionSelectionCriteria[] { Parent.ConditionSelectionCriteria }, Array.Empty<ZString>(), null, null);

		public CodeDescriptionPairList CachedListOfAdditionalCodeDescriptions => RefCusCodeListTypes.GetCachedList(Factory,
																	Parent.DataGroupingCode,
																	Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes,
																	Parent.ValuationDate,
																	languageCode: TranslationHelper.GetCurrentLanguageCode());
	}
}
