using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public partial class AsycudaBillLookups
	{
		public RefCountryCollection Countries
		{
			get
			{
				RefCountryCollection result = null;
				var countryCode = Parent.Header?.AMA_RN_NKCountry ?? ZString.Empty;
				if (!countryCode.IsEmpty)
				{
					result = Factory.GetCachedValue("AsycudaBillLookups.Countries_" + countryCode, delegate
					{
						var list = new RefCountryCollection(Factory);
						var f = new ZQuery();
						f.AddToFilter(RefCountrySchema.RN_Code, countryCode);
						list.AdditionalFilter = f;
						list.ApplySort(RefCountrySchema.RN_Desc.Name, System.ComponentModel.ListSortDirection.Ascending);
						return list;
					});
				}
				return result;
			}
		}

		public BaseJobDeclarationCollection CustomsJobNumberList
		{
			get
			{
				var countryCode = Parent.CountryCode;
				if (countryCode.IsEmpty)
				{
					countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				}
				return new BaseJobDeclarationCollection(Factory, countryCode);
			}
		}

		public static CodeDescriptionPairList GetShipmentTypes(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<ShipmentTypeList>();
		}

		public CodeDescriptionPairList ShipmentTypes
		{
			get
			{
				if ((AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, Parent.CountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ShipmentType) is { } shipmentTypes) && shipmentTypes.Count > 0)
				{
					return shipmentTypes;
				}

				return GetShipmentTypes(Factory);
			}
		}

		public virtual CodeDescriptionPairList CustomsEntryNumberTypes
		{
			get { return AsycudaUniversalReference.RefCusCodeListTypes.GetCachedUntranslatableList(Factory, Parent.CountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsEntryNumberTypes); }
		}

		public virtual ICollection Locations
		{
			get { return Parent?.Header?.CountryHelper?.GetLocationsOfGoodsList(Parent.CountryCode); }
		}

		public virtual CodeDescriptionPairList MessageStatusList => Factory.GetCachedValue<MessageStatusCodeList>();

		public virtual CodeDescriptionPairList CustomsStatusList => CountryHelper.GetCustomsManifestStatusList(Factory, Parent.CountryCode);

		public virtual IBusinessObjectCollection BillIssuers
		{
			get
			{
				var countryCode = Parent.CountryCode.IsEmpty ? Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping : Parent.CountryCode.ToString(); // The collection demands a non-blank country
				return ZZRefCarrierCombinedCollection.GetCachedCollection(Parent.Factory, countryCode, Core.Constants.Customs.Universal.RefCarrierAttributeNames.CARGOCARRIER, Parent.ABL_Calc_AMA_TransportMode);
			}
		}
	}
}
