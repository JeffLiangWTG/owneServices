using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Core;
using InterchangeTypes = Enterprise.Customs.IT.Business.MessageProcessorConstants.InterchangeTypes;

namespace Enterprise.Customs.IT.Business;

sealed class XTradeErrorResponseMessageProcessor : XmlIncomingMessageProcessor
{
	public XTradeErrorResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"UCC6 XTrade Error Response Message Processor";

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { InterchangeTypes.Ucc6XTradeErrorType };

	protected override void ProcessResponse(IXmlCustomsLinkedObjectAdapter entryAdapter, EDIMessage receivedMessage, EDIMessage originalSentMessage)
	{
		var messageBody = receivedMessage.EM_MessageText;

		if (messageBody.Contains("<UniversalInterchange ")
			&& XDocument.Parse(messageBody)?.Root?.XPathSelectElement((NoResString)"//*[local-name()='UniversalInterchange']//*[local-name()='Body']")?.FirstNode is XElement bodyNode)
		{
			var responseMessage = new UniversalEventWrapper(bodyNode.ToString());

			if (new Ucc6XTradeErrorResponseMessageHelper().IsSentInterchangeTypeApplicableForMessageProcessing(originalSentMessage)
				&& entryAdapter.IsAwaitingMessage
				&& !responseMessage.HasBusinessError)
			{
				entryAdapter.SetStatusAsFailedForTransmission();
				entryAdapter.ProcessBondedWarehouseIfRequired(Logger, receivedMessage);
			}
		}
	}
}
