using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

static class EDIMessageExtension
{
	public static bool SignatureRequiresAmendmentMetadata(this EDIMessage message)
		=> message.EM_MessageType == EDIMessageTypeList.Codes.Amendment && message.EM_ApplicationReference == EDIMessageApplicationReferenceList.Codes.TemporaryStorage;

	public static bool IsCancellationRequest(this EDIMessage message)
		=> message.EM_MessageType == EDIMessageTypeList.Codes.Cancellation;
}
