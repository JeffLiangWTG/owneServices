using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsAdditionalInfoPhase5Lookups : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoLookups
	{
		public NctsAdditionalInfoPhase5Lookups(NctsAdditionalInfo parent)
			: base(parent)
		{
		}

		public override CodeDescriptionPairList SubTypeList
		{
			get
			{
				var parent = Parent;
				var parentIsGoodsItem = parent.ParentAsGoodsItem != null;
				var parentIsArrival = parent.IsArrival;
				var isNew = (parent.CSI_Status == NctsUnloadedStateList.Codes.NEW);
				return Factory.GetCachedValue($"NctsAdditionalInfoPhase5Lookups.SubTypeList.ParentIsGoodsItem_{parentIsGoodsItem}.ParentIsArrival_{parentIsArrival}.IsNew{isNew}", () => GetSubTypeList(parentIsGoodsItem, parentIsArrival, isNew));
			}
		}

		CodeDescriptionPairList GetSubTypeList(bool parentIsGoodsItem, bool parentIsArrival, bool isNew)
		{
			var list = new AdditionalInfoSubTypeList();
			if (parentIsGoodsItem)
			{
				if (!parentIsArrival)
				{
					list.RemoveCode(AdditionalInfoSubTypeList.Codes.TransportDocument);
				}
				else if (isNew)
				{
					list.RemoveCode(AdditionalInfoSubTypeList.Codes.AdditionalInformation);
				}
			}
			return list;
		}

		public override ICollection CodeList
		{
			get
			{
				var parent = Parent;
				var codeListType = parent.CodeListType;
				var parentIsHeader = parent.Parent is NctsHeader or NctsCommonMovementHeader;
				var dataGroupingCode = parent.ParentAsNctsHeader?.DefaultDataGroupingCode ?? parent.ParentAsArrivalMovementHeader?.DefaultDataGroupingCode ?? parent.ParentAsGoodsItem?.DataGroupingCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				var level = parentIsHeader ? EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Header : EU.Business.UniversalReferenceConstants.RefCusCodeListLevelType.Item;
				return Factory.GetCachedValue("NctsAdditionalInfoPhase5Lookups.CodeList.CodeListType_" + codeListType + ".Level_" + level + ".DataGroupingCode_" + dataGroupingCode, delegate
				{
					var result = GetCodeList(false, dataGroupingCode, codeListType, level);
					result.Load();
					return result.Count == 0 ? GetDefaultCodeList(dataGroupingCode, codeListType, level) : result;
				});
			}
		}

		protected virtual ZZRefCusCodeListCombinedCollection GetDefaultCodeList(ZString dataGroupingCode, ZString codeListType, string level)
		{
			var parentDataGroupingCode = RefDataGrouping.GetParentDataGroupingCode(Factory, dataGroupingCode);
			return parentDataGroupingCode.IsEmpty ? GetCodeList(false, RefDataGroupingCodes.EuropeanUnionEUN, codeListType, level) : GetCodeList(true, dataGroupingCode, codeListType, level);
		}

		protected virtual ZZRefCusCodeListCombinedCollection GetCodeList(bool includeParent, string dataGroupingCode, string codeListType, string level)
		{
			return CusSupportingInfoHelper.GetTypeCodeList(Factory
				, codeListType
				, includeParent
				, level
				, dataGroupingCode);
		}

		public override CodeDescriptionPairList StatusList => NctsHelper.GetCachedNctsBillAdditionalDocumentStatusList(Factory, removeNewCode: Parent.CSI_Status != NctsBillAdditionalDocumentStatusList.Codes.NEW);

		protected new NctsAdditionalInfo Parent => (NctsAdditionalInfo)base.Parent;
	}
}
