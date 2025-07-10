using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

class AccountingSummaryResponseMessageProcessor : XmlIncomingMessageProcessor
{
	public AccountingSummaryResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"Accounting Summary Response Message Processor";

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => [EDIMessageTypeList.Codes.AccountingSummaryRequest];

	protected override void ProcessResponse(IXmlCustomsLinkedObjectAdapter entryAdapter, EDIMessage receivedMessage, EDIMessage originalSentMessage)
	{
		receivedMessage.EM_MessageSubType = originalSentMessage.EM_MessageSubType;
	}
}
