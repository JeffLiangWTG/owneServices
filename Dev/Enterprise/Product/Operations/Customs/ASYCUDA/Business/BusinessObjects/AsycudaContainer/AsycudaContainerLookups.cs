using System;
using System.Collections;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ManifestBase.Extensions;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaContainerLookups : ManifestBase.AsycudaContainerLookups
	{
		public AsycudaContainerLookups(AsycudaContainer parent)
			: base(parent)
		{
		}

		protected new AsycudaContainer Parent => (AsycudaContainer)base.Parent;

		public virtual CodeDescriptionPairList EmptyFullList => Factory.GetCachedValue<EmptyFullIndicatorList>();

		public CodeDescriptionPairList SealingPartyList => GetCodeDescriptionPairList(RefCusMapTypeList.Codes.STYPE, CommonContainerLookups.GetCachedValue_SealParty_List(Factory));

		public CodeDescriptionPairList SealTypeList => GetSealTypeListCore();

		public CodeDescriptionPairList CommodityCodes => AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, Parent.Header?.AMA_RN_NKCountry ?? ZString.Empty, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CommodityCode);

		public CodeDescriptionPairList WeightCodes => Factory.GetWeightUQList();

		public CodeDescriptionPairList UnloadingStatesList => Factory.GetCachedValue<UnloadingStates>();

		protected virtual CodeDescriptionPairList GetSealTypeListCore() => GetCodeDescriptionPairList(RefCusMapTypeList.Codes.MSELT, Factory.GetCachedValue<SealTypeList>());

		CodeDescriptionPairList GetCodeDescriptionPairList(string refCusMapType, IEnumerable completeCodeList)
		{
			var countryCode = Parent.Header?.AMA_RN_NKCountry ?? ZString.Empty;
			return Factory.GetCachedValue(FormattableString.Invariant($"Enterprise.Customs.ASYCUDA.Business.AsycudaContainerLookups.{refCusMapType}.{countryCode}"), () =>
			{
				var countryMappings = ZZRefCusMapCombined.GetCW1CodeToCustomsCodeMapping(Factory, countryCode, refCusMapType, ZDateTime.Now);
				if (countryMappings.Count == 0)
				{
					countryMappings = ZZRefCusMapCombined.GetCW1CodeToCustomsCodeMapping(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, refCusMapType, ZDateTime.Now);
				}

				var result = new CodeDescriptionPairList();
				foreach (var codePair in completeCodeList.OfType<CodeDescriptionPair>())
				{
					if (countryMappings.ContainsKey(codePair.Code))
					{
						result.Add(codePair);
					}
				}
				return result;
			});
		}
	}
}
