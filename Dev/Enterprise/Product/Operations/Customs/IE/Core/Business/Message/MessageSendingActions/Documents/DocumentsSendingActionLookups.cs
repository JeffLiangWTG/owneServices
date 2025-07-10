using Enterprise.Customs.IE.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business
{
	public class DocumentsSendingActionLookups : CusEntryHeaderMessageSendingActionLookups
	{
		public DocumentsSendingActionLookups(DocumentsSendingAction parent) : base(parent) { }

		public override CodeDescriptionPairList SendingActionTypeList => Factory.GetCachedValue(
			"Enterprise.Customs.IE.Business.DocumentsSendingActionLookups|SendingActionTypeList",
			() =>
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(AESOutgoingMessageTypeList.Codes.DocumentUpload, AESOutgoingMessageTypeList.Descriptions.DocumentUpload);
				return result;
			}
		);
	}
}
