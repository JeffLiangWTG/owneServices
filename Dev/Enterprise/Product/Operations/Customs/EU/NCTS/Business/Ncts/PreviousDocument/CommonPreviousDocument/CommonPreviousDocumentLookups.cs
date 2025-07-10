using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CommonPreviousDocumentLookups : EU.Business.Declaration.MultiLineAddInfos.PreviousDocumentLookups
	{
		public CommonPreviousDocumentLookups(CommonPreviousDocument parent)
			: base(parent)
		{
		}

		public ZZRefCusCodeListCombinedCollection TypeCodeList
		{
			get
			{
				var levelAttributeValue = Parent.LevelAttributeValue;
				var dataGroupingCode = DataGroupingCode;
				return Factory.GetCachedValue(TypeCodeListCachedKey, delegate
				{
					var result = GetTypeCodeList(false, levelAttributeValue, dataGroupingCode);
					result.Load();
					return result.Count == 0 ? GetDefaultTypeCodeList(levelAttributeValue, dataGroupingCode) : result;
				});
			}
		}

		protected virtual string TypeCodeListCachedKey => "CommonPreviousDocumentLookups.TypeCodeList.Level_" + Parent.LevelAttributeValue + ".DataGroupingCode_" + DataGroupingCode;

		protected virtual ZZRefCusCodeListCombinedCollection GetDefaultTypeCodeList(ZString levelAttributeValue, ZString dataGroupingCode)
		{
			var parentDataGroupingCode = RefDataGrouping.GetParentDataGroupingCode(Factory, dataGroupingCode);
			return parentDataGroupingCode.IsEmpty ? GetTypeCodeList(false, levelAttributeValue, RefDataGroupingCodes.EuropeanUnionEUN) : GetTypeCodeList(true, levelAttributeValue, dataGroupingCode);
		}

		protected virtual ZZRefCusCodeListCombinedCollection GetTypeCodeList(bool includeParent, string levelAttributeValue, string dataGroupingCode)
		{
			return CusSupportingInfoHelper.GetTypeCodeList(Factory
				, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfNCTS
				, includeParent
				, levelAttributeValue
				, dataGroupingCode);
		}

		public new CommonPreviousDocument Parent => (CommonPreviousDocument)base.Parent;

		protected ZString DataGroupingCode => Parent.Parent?.DataGroupingCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
	}
}
