using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Module;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Module
{
	public class MiscRequestMessagesFilterLookups : CommonFilterLookups
	{
		public MiscRequestMessagesFilterLookups(MiscRequestMessagesFilterStripBusinessObject filterBizObj)
			: base(filterBizObj)
		{
		}

		public CodeDescriptionPairList RequestMessageTypeList
		{
			get
			{
				return Factory.GetCachedValue("RequestMessageTypeList", delegate
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(ElectronicDocumentTypeList.Codes._5AC, ElectronicDocumentTypeList.Descriptions._5AC);
					result.AddPair(ElectronicDocumentTypeList.Codes._5GW, ElectronicDocumentTypeList.Descriptions._5GW);
					return result;
				});
			}
		}

		public CodeDescriptionPairList RequestEntryTypeList
		{
			get
			{
				return Factory.GetCachedValue("RequestEntryTypeList", delegate
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(SharedJobMessageTypeList.Codes.Import, SharedJobMessageTypeList.Descriptions.Import);
					result.AddPair(SharedJobMessageTypeList.Codes.Export, SharedJobMessageTypeList.Descriptions.Export);
					return result;
				});
			}
		}

		public CodeDescriptionPairList StatusList
		{
			get
			{
				return Factory.GetCachedValue("StatusList", delegate
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(CustomsMessageStatusTypeList.Codes.OriginalSent, CustomsMessageStatusTypeList.Descriptions.OriginalSent);
					result.AddPair(CustomsMessageStatusTypeList.Codes.ErrorSendingOriginal, CustomsMessageStatusTypeList.Descriptions.ErrorSendingOriginal);
					result.AddPair(CustomsMessageStatusTypeList.Codes.OriginalRejected, CustomsMessageStatusTypeList.Descriptions.OriginalRejected);
					result.AddPair(CustomsMessageStatusTypeList.Codes.OriginalAccepted, CustomsMessageStatusTypeList.Descriptions.OriginalAccepted);
					return result;
				});
			}
		}

		public ZZRefCusCodeListCombinedCollection CustomsOffices => new ZZRefCusCodeListCombinedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Now);
	}
}
