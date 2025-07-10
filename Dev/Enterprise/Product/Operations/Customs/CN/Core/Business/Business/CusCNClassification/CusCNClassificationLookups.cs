using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	public class CusCNClassificationLookups : AutoCusCNClassificationLookups
	{
		public CusCNClassificationLookups(AutoCusCNClassification parent)
			: base(parent)
		{
		}

		public new CusCNClassification Parent => (CusCNClassification)base.Parent;

		public CusClassPartPivot CusClassPartPivot => Parent.Pivot;

		public ChildTariffViewCollection CIQTariffList
		{
			get
			{
				var ciTariff = CusClassPartPivot.CI_TariffNum;

				var result = ChildTariffViewCollection.GetNewCollection(
					Factory,
					Core.Constants.CountryCodes.China,
					Constants.UniversalReferenceConstants.CusTariffTypes.ChinaCIQTariff,
					ZDateTime.Today,
					Universal.Constants.TariffTypes.HarmonizedSystem,
					ciTariff
				);

				if (!ciTariff.IsEmpty && Parent.CNC_CIQTariff.IsEmpty)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Customs.Universal.Constants.RefCusTariffFilters.TariffCode, "Property", ciTariff, true));
				}

				return result;
			}
		}

		public CodeDescriptionPairList EndUseList => Factory.GetCachedValue<EndUseList>();

		#region Districts & Regions

		public ZZRefCusCodeListCombinedCollection OrigDistrictList
		{
			get
			{
				var result = CNRefCusCodeListTypes.GetDistrictList(Factory, ZDateTime.Today);

				if (CusClassPartPivot.CNC_OriginDistrict.IsEmpty && !CusClassPartPivot.CNC_OriginRegion.IsEmpty)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Code", "Property", CusClassPartPivot.CNC_OriginRegion.Left(4)));
				}
				else if (result.FilterBusinessObjectDefaults.ContainsDefaultFor("Code:Property"))
				{
					result.FilterBusinessObjectDefaults.Remove("Code:Property");
				}
				return result;
			}
		}

		public ZZRefCusCodeListCombinedCollection DestDistrictList
		{
			get
			{
				var result = CNRefCusCodeListTypes.GetDistrictList(Factory, ZDateTime.Today);

				if (CusClassPartPivot.CNC_DestinationDistrict.IsEmpty && !CusClassPartPivot.CNC_DestinationRegion.IsEmpty)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Code", "Property", CusClassPartPivot.CNC_DestinationRegion.Left(4)));
				}
				else if (result.FilterBusinessObjectDefaults.ContainsDefaultFor("Code:Property"))
				{
					result.FilterBusinessObjectDefaults.Remove("Code:Property");
				}

				return result;
			}
		}

		public ZZRefCusCodeListCombinedCollection OrigRegionList
		{
			get
			{
				var result = CNRefCusCodeListTypes.GetRegionList(Factory, ZDateTime.Today);

				if (CusClassPartPivot.CNC_OriginRegion.IsEmpty && !CusClassPartPivot.CNC_OriginDistrict.IsEmpty)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Code", "Property", CusClassPartPivot.CNC_OriginDistrict.Left(4)));
				}
				else if (result.FilterBusinessObjectDefaults.ContainsDefaultFor("Code:Property"))
				{
					result.FilterBusinessObjectDefaults.Remove("Code:Property");
				}
				return result;
			}
		}

		public ZZRefCusCodeListCombinedCollection DestRegionList
		{
			get
			{
				var result = CNRefCusCodeListTypes.GetRegionList(Factory, ZDateTime.Today);

				if (CusClassPartPivot.CNC_DestinationRegion.IsEmpty && !CusClassPartPivot.CNC_DestinationDistrict.IsEmpty)
				{
					result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Code", "Property", CusClassPartPivot.CNC_DestinationDistrict.Left(4)));
				}
				else if (result.FilterBusinessObjectDefaults.ContainsDefaultFor("Code:Property"))
				{
					result.FilterBusinessObjectDefaults.Remove("Code:Property");
				}
				return result;
			}
		}

		#endregion

		public ZZRefCusCodeListCombinedCollection CIQOriginStateList
		{
			get
			{
				var result = CNRefCusCodeListTypes.GetStatesList(Factory, ZDateTime.Today);
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Universal.Constants.ZZRefCusCodeListFilters.Code, "Property", new ZString(CusClassPartPivot.CountryOfOrigin?.RN_IsoNumericUNM49Code)));

				return result;
			}
		}

		public CodeDescriptionPairList UNDGPackageTypes => Factory.GetCachedValue<UNDGPackageTypeList>();

		public CodeDescriptionPairList TradeUnitQtyList => Factory.GetCachedValue<RefCusPackListProvider>().GetCIPCustomsPackList(Factory, Core.Constants.CountryCodes.China);
	}
}
