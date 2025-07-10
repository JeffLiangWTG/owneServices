using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsBillAdditionalDocumentLookups : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoLookups
	{
		public NctsBillAdditionalDocumentLookups(NctsBillAdditionalDocument parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList SubTypeList
		{
			get
			{
				var isNew = (Parent.CSI_Status == NctsUnloadedStateList.Codes.NEW);

				return Factory.GetCachedValue($"NctsBillAdditionalDocumentLookups.SubTypeList.IsNew_{isNew}", () =>
				{
					var list = new AdditionalInfoSubTypeList();
					if (Parent.IsArrivalMovement && isNew)
					{
						list.RemoveCode(AdditionalInfoSubTypeList.Codes.AdditionalInformation);
					}
					return list;
				});
			}
		}

		public ZZRefCusCodeListCombinedCollection TypeCodeList
		{
			get
			{
				var subType = Parent.GetCodeTypeBySubType();
				var dataGroupingCode = DataGroupingCode;
				return subType.IsEmpty ? new ZZRefCusCodeListCombinedCollection(Factory) : Factory.GetCachedValue("NctsBillAdditionalDocumentLookups.TypeCodeList.SubType_" + subType + ".DataGroupingCode_" + dataGroupingCode, delegate
				{
					var result = GetTypeCodeList(false, subType, dataGroupingCode);
					result.Load();
					return result.Count == 0 ? GetDefaultTypeCodeList(subType, dataGroupingCode) : result;
				});
			}
		}

		protected virtual ZZRefCusCodeListCombinedCollection GetDefaultTypeCodeList(string subType, string dataGroupingCode)
		{
			var parentDataGroupingCode = RefDataGrouping.GetParentDataGroupingCode(Factory, dataGroupingCode);
			return parentDataGroupingCode.IsEmpty ? GetTypeCodeList(false, subType, RefDataGroupingCodes.EuropeanUnionEUN) : GetTypeCodeList(true, subType, dataGroupingCode);
		}

		protected virtual ZZRefCusCodeListCombinedCollection GetTypeCodeList(bool includeParent, string subType, string dataGroupingCode)
		{
			return CusSupportingInfoHelper.GetTypeCodeList(Factory
				, subType
				, includeParent
				, UniversalReferenceConstants.RefCusCodeListLevelTypes.House
				, dataGroupingCode);
		}

		public new NctsBillAdditionalDocument Parent => (NctsBillAdditionalDocument)base.Parent;

		protected ZString DataGroupingCode => Parent.Parent?.DataGroupingCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		public override CodeDescriptionPairList StatusList => NctsHelper.GetCachedNctsBillAdditionalDocumentStatusList(Factory, removeNewCode: Parent.CSI_Status != NctsBillAdditionalDocumentStatusList.Codes.NEW);
	}
}
