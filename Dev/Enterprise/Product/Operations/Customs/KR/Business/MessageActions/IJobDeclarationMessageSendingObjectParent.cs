using static Enterprise.Customs.KR.Messaging.MessageFunctions;

namespace Enterprise.Customs.KR.Business
{
	public interface IJobDeclarationMessageSendingObjectParent
	{
		string MessageType { get; }
		MessageSender GetMessageSender(MessageFunctionCode messageFunctionCode);
		bool IsFaultPartyRelevant { get; }
		bool IsReasonCodeRelevant { get; }
		bool IsDateOfFinalPriceRelevant { get; }
	}
}
