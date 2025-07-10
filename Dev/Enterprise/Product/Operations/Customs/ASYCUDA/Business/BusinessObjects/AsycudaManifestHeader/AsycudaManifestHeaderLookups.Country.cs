using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public partial class AsycudaManifestHeaderLookups
	{
		public CodeDescriptionPairList CommodityCodeList
		{
			get
			{
				var countryCode = CountryCode;
				return Factory.GetCachedValue("ASYCUDA.CommodityCodeList." + countryCode, () =>
				{
					var list = AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, countryCode,
						Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CommodityCode);
					list.SortByDescription();
					return list;
				});
			}
		}

		ZString CountryCode => Parent.AMA_RN_NKCountry;

		public virtual CodeDescriptionPairList RegistrationStatusList
		{
			get
			{
				var dataGroupingCode = Parent.DataGrouping;
				return Factory.GetCachedValue("ASYCUDA.RegistrationStatusList." + dataGroupingCode, () =>
				{
					var list = Parent.MessageStatusProvider?.GetRegistrationStatusList(Factory, dataGroupingCode, ZString.Empty);
					list?.Sort();
					return list ?? new CodeDescriptionPairList();
				});
			}
		}

		public CodeDescriptionPairList SpecificCircumstanceList => SpecificCircumstanceListCore;

		protected virtual CodeDescriptionPairList SpecificCircumstanceListCore
		{
			get { return Factory.GetCachedValue<SpecificCircumstanceList>(); }
		}

		public CodeDescriptionPairList MethodOfPaymentList
		{
			get { return AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, CountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ICSMethodOfPayment); }
		}

		public CodeDescriptionPairList SpecialMentionsList
		{
			get { return AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, CountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ICSSpecialMentions); }
		}
	}
}
