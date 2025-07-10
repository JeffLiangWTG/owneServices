using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsPreviousDocumentPhase5Lookups : NctsPreviousDocumentLookups
	{
		public NctsPreviousDocumentPhase5Lookups(NctsPreviousDocument parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList UnitOfQuantityList => RefCusCodeListTypes.GetCachedList(Factory, Parent.GoodsItem.DataGroupingCode, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, Parent.GoodsItem.ValuationDate);

		public override CodeDescriptionPairList UnitOfQuantity2List => NctsPackageLookups.GetPackageUnitTypeList(Factory);

		public override ICollection CodeList
		{
			get
			{
				var dataGroupingCode = Parent.GoodsItem.DataGroupingCode;
				return Factory.GetCachedValue("NctsPreviousDocumentPhase5Lookups.CodeList.DataGroupingCode_" + dataGroupingCode, delegate
				{
					var result = GetTypeCodeList(false, dataGroupingCode);
					result.Load();
					return result.Count == 0 ? GetDefaultTypeCodeList(dataGroupingCode) : result;
				});
			}
		}

		protected virtual ZZRefCusCodeListCombinedCollection GetDefaultTypeCodeList(ZString dataGroupingCode)
		{
			var parentDataGroupingCode = RefDataGrouping.GetParentDataGroupingCode(Factory, dataGroupingCode);
			return parentDataGroupingCode.IsEmpty ? GetTypeCodeList(false, RefDataGroupingCodes.EuropeanUnionEUN) : GetTypeCodeList(true, dataGroupingCode);
		}

		protected virtual ZZRefCusCodeListCombinedCollection GetTypeCodeList(bool includeParent, string dataGroupingCode)
		{
			return CusSupportingInfoHelper.GetTypeCodeList(Factory
				, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfNCTS
				, includeParent
				, RefCusCodeListLevelType.Item
				, dataGroupingCode);
		}
	}
}
