using Enterprise.Customs.IE.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business
{
	public class AISUCC5MessageSendingActionLookups : CusEntryHeaderMessageSendingActionLookups
	{
		public AISUCC5MessageSendingActionLookups(AISUCC5MessageSendingAction parent) : base(parent) { }

		public override CodeDescriptionPairList SendingActionTypeList
		{
			get
			{
				return Factory.GetCachedValue("Enterprise.Customs.IE.Business.AISMessageSendingActionLookups.SendingActionTypeList_UCC5", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(AISOutgoingMessageTypeList.Codes.AmendmentRequest, AISOutgoingMessageTypeList.Descriptions.AmendmentRequest);
					result.AddPair(AISOutgoingMessageTypeList.Codes.InvalidationRequest, AISOutgoingMessageTypeList.Descriptions.InvalidationRequest);
					result.AddPair(AISOutgoingMessageTypeList.Codes.CustomsDeclaration, AISOutgoingMessageTypeList.Descriptions.CustomsDeclaration);
					result.AddPair(AISOutgoingMessageTypeList.Codes.PresentationNotification, AISOutgoingMessageTypeList.Descriptions.PresentationNotification);
					result.Sort();
					return result;
				});
			}
		}

		public override CodeDescriptionPairList SendingActionTypeListForDisplay
		{
			get
			{
				return Factory.GetCachedValue("Enterprise.Customs.IE.Business.AISMessageSendingActionLookups.SendingActionTypeListForDisplay_UCC5", () =>
				{
					var result = new CodeDescriptionPairList();
					result.AddPair(AISOutgoingMessageTypeList.Codes.AmendmentRequest, AISOutgoingMessageTypeListForDisplay.Codes.AmendmentRequest, AISOutgoingMessageTypeListForDisplay.Descriptions.AmendmentRequest);
					result.AddPair(AISOutgoingMessageTypeList.Codes.InvalidationRequest, AISOutgoingMessageTypeListForDisplay.Codes.InvalidationRequest, AISOutgoingMessageTypeListForDisplay.Descriptions.InvalidationRequest);
					result.AddPair(AISOutgoingMessageTypeList.Codes.CustomsDeclaration, AISOutgoingMessageTypeListForDisplay.Codes.CustomsDeclaration, AISOutgoingMessageTypeListForDisplay.Descriptions.CustomsDeclaration);
					result.AddPair(AISOutgoingMessageTypeList.Codes.PresentationNotification, AISOutgoingMessageTypeListForDisplay.Codes.PresentationNotification, AISOutgoingMessageTypeListForDisplay.Descriptions.PresentationNotification);
					result.Sort();
					return result;
				});
			}
		}
	}
}
