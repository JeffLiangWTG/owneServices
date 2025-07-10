using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaPackedItemLookups : ManifestBase.AsycudaPackedItemLookups
	{
		public AsycudaPackedItemLookups(AsycudaPackedItem parent)
			: base(parent)
		{
		}

		public new AsycudaPackedItem Parent => (AsycudaPackedItem)base.Parent;

		public virtual CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<MessageStatusCodeList>();

		public CodeDescriptionPairList CustomsStatusList => CountryHelper.GetCustomsManifestStatusList(Factory, Parent.CountryCode);

		public static CodeDescriptionPairList GetGoodsTypeListForCountry(BusinessObjectFactory factory, ZString country)
		{
			return AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(factory, country, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GoodsType);
		}

		public CodeDescriptionPairList CustomsUQList
		{
			get { return AsycudaUniversalReference.RefCusCodeListTypes.GetCachedUntranslatableList(Factory, Parent.CountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes); }
		}

		public TariffViewCollection TariffList
		{
			get
			{
				var applicationBusinessProvider = Parent.Header?.ApplicationBusinessProvider;
				var dataGrouping = applicationBusinessProvider?.PackedItemTariffDataGrouping ?? string.Empty;
				var effectiveDate = Parent.EffectiveDateForDutyRate;
				var tariffType = applicationBusinessProvider?.PackedItemTariffType ?? string.Empty;

				return Factory.GetCachedValue("ManifestPackageItemTariff" + dataGrouping + effectiveDate + tariffType, delegate
				{
					var collection = new TariffViewCollection(Factory, dataGrouping, tariffType, effectiveDate);
					collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.RefCusTariffFilters.EffectiveDate, "Property1", effectiveDate, false));
					return collection;
				});
			}
		}

		public virtual RefCurrencyCollection Currencies
		{
			get { return new RefCurrencyCollection(Factory); }
		}

		public virtual CodeDescriptionPairList GrossWeightUQList
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		public virtual CodeDescriptionPairList NetWeightUQList
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		public RefCountryCollection Countries
		{
			get { return new RefCountryCollection(Factory); }
		}
	}
}
