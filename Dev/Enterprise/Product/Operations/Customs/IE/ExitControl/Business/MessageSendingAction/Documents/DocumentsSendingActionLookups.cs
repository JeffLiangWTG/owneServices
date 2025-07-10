using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class DocumentsSendingActionLookups : ZLookups
	{
		public DocumentsSendingActionLookups(DocumentsSendingAction sendingAction) : base(sendingAction) { }

		public CodeDescriptionPairList SendingActionTypeList => Factory.GetCachedValue(
			"Enterprise.Customs.IE.ExitControl.Business.DocumentsSendingActionLookups|SendingActionTypeList",
			() =>
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(AESOutgoingMessageTypeList.Codes.DocumentUpload, AESOutgoingMessageTypeList.Descriptions.DocumentUpload);
				return result;
			}
		);
	}
}
