using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsSupportingDocumentPhase5Lookups : NctsSupportingDocumentLookups
	{
		public NctsSupportingDocumentPhase5Lookups(NctsSupportingDocument parent) : base(parent)
		{
		}

		public override ZZRefCusCodeListCombinedCollection TypeCodeList
		{
			get
			{
				var dataGroupingCode = DataGroupingCode;
				return Factory.GetCachedValue("NctsSupportingDocumentPhase5Lookups.TypeCodeList.DataGroupingCode_" + dataGroupingCode, delegate
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
				, RefCusCodeListType.Code.SupportingDocumentOfNCTS
				, includeParent
				, Parent.LevelAttributeValue
				, dataGroupingCode);
		}

		public override CodeDescriptionPairList StatusList
		{
			get
			{
				var isNew = Parent.CSI_Status == SupportingDocumentStatusList.Codes.NEW;
				return Factory.GetCachedValue($"EU.NCTS.NctsSupportingDocumentPhase5Lookups.StatusList_isNew{isNew}", () =>
				{
					var list = new SupportingDocumentStatusList();
					if (!isNew)
					{
						list.RemoveCode(SupportingDocumentStatusList.Codes.NEW);
					}

					return list;
				});
			}
		}
	}
}
