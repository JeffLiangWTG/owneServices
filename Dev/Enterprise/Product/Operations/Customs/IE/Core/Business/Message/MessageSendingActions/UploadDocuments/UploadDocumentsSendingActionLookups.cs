using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.IE.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business
{
	public class UploadDocumentsSendingActionLookups : CusEntryHeaderMessageSendingActionLookups
	{
		public UploadDocumentsSendingActionLookups(UploadDocumentsSendingAction parent) : base(parent) { }

		public override CodeDescriptionPairList SendingActionTypeList
		{
			get
			{
				var isIM446Available = IsIM446Available;
				return Factory.GetCachedValue($"UploadDocumentsSendingActionLookups|SendingActionTypeList|{isIM446Available}", () =>
				{
					var list = new AISUploadDocumentsMessageTypeList();
					if (!isIM446Available)
					{
						list.RemoveCode(AISUploadDocumentsMessageTypeList.Codes.IM446);
					}
					return list;
				});
			}
		}

		public override CodeDescriptionPairList SendingActionTypeListForDisplay
		{
			get
			{
				var isIM446Available = IsIM446Available;
				return Factory.GetCachedValue($"UploadDocumentsSendingActionLookups|SendingActionTypeListForDisplay|{IsIM446Available}", () =>
				{
					var result = new CodeDescriptionPairList();
					if (isIM446Available)
					{
						result.AddPair(AISUploadDocumentsMessageTypeList.Codes.IM446, AISUploadDocumentsMessageTypeListForDisplay.Codes.IM446, AISUploadDocumentsMessageTypeListForDisplay.Descriptions.IM446);
					}
					result.AddPair(AISUploadDocumentsMessageTypeList.Codes.IM483, AISUploadDocumentsMessageTypeListForDisplay.Codes.IM483, AISUploadDocumentsMessageTypeListForDisplay.Descriptions.IM483);
					return result;
				});
			}
		}

		bool IsIM446Available => EUCustomsDataRegistry.Instance.EnableCentralizedClearanceForImport.Value && Parent is UploadDocumentsSendingAction action && !action.IsUCC5;
	}
}
