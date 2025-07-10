using System;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStoragePackedItemLookups : AsycudaPackedItemLookups
	{
		public TemporaryStoragePackedItemLookups(AutoAsycudaPackedItem parent) : base(parent)
		{
		}

		public new TemporaryStoragePackedItem Parent => (TemporaryStoragePackedItem)base.Parent;

		public CodeDescriptionPairList GrossWeightUQList
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		public CodeDescriptionPairList NetWeightUQList
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		public RefCurrencyCollection Currencies => new RefCurrencyCollection(Factory);

		public CodeDescriptionPairList CountryOfOriginList => Factory.GetCachedCountryNC008List(DataGrouping);

		public CodeDescriptionPairList CustomsUnitOfQuantityList => RefCusCodeListTypes.GetCachedList(Factory,
			DataGrouping,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ,
			Parent.ValuationDate);

		ZString DataGrouping => Parent.Bill?.Header?.DataGrouping ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		public CodeDescriptionPairList AdditionalCodeList => Customs.Business.UniversalReferenceDataHelper.GetDynamicAdditionalCodeList(Parent.UniversalTariff,
			CachedListOfAdditionalCodeDescriptions,
			new IZZRateSelectionCriteria[] { Parent.AllApplicableRatesSelectionCriteria },
			new IZZConditionSelectionCriteria[] { Parent.ConditionSelectionCriteria },
			conditionTypesToExclude: Array.Empty<ZString>(),
			vatCriteria: null,
			tariffAdditionalCodeCriteria: null);

		public CodeDescriptionPairList CachedListOfAdditionalCodeDescriptions => RefCusCodeListTypes.GetCachedList(Factory,
			DataGrouping,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes,
			ZDateTime.Today,
			languageCode: TranslationHelper.GetCurrentLanguageCode());

		public TariffViewCollection TariffList
		{
			get
			{
				var dataGrouping = DataGrouping;
				var effectiveDate = ZDateTime.Today;
				var tariffType = Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff;

				return Factory.GetCachedValue("TemporaryStoragePackageItemTariffList" + dataGrouping + effectiveDate + tariffType, delegate
				{
					var collection = new TariffViewCollection(Factory, dataGrouping, tariffType, effectiveDate);
					return collection;
				});
			}
		}

		public ZZRefCusCodeListCombinedCollection ChemicalSubstanceCodeList
		{
			get
			{
				var dataGroupingCode = Parent.Bill?.Header?.DataGrouping ?? Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
				return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, dataGroupingCode, new ZString[] { UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS }, ZDateTime.Today, null, RefCusCodeListTypes.IncludeParentDataGroupingOptions.ChildFirstThenParent);
			}
		}
	}
}
