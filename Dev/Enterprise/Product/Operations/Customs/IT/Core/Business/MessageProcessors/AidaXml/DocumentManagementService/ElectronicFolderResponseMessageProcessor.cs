using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

sealed class ElectronicFolderResponseMessageProcessor : XmlIncomingMessageProcessor
{
	public ElectronicFolderResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected sealed override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageProcessorConstants.InterchangeTypes.ElectronicFolderResponseType };

	protected override string MessageFriendlyNameCore => Ucc6ResponseMessageProcessorFriendlyName;

	protected override void ProcessResponse(IXmlCustomsLinkedObjectAdapter entryAdapter, EDIMessage receivedMessage, EDIMessage originalSentMessage)
	{
		var responseMessage = new ElectronicFolderResponseMessage(receivedMessage.EM_MessageText);

		var controlChannelStrategy = GetControlChannelStrategy(responseMessage);
		if (controlChannelStrategy == null)
		{
			return;
		}

		var controlChannel = controlChannelStrategy.GetControlChannel();
		if (string.IsNullOrWhiteSpace(controlChannel))
		{
			return;
		}

		entryAdapter.SetCustomsChannel(controlChannel);
	}

	IControlChannelStrategy GetControlChannelStrategy(ElectronicFolderResponseMessage responseMessage)
	{
		var responseWrapper = responseMessage.GetResponseWrapper();
		var code = responseWrapper.ResponseStatusCode;
		if (string.Equals(code, ControlChannelStrategyForCodeDZero24.Code_D024, StringComparison.OrdinalIgnoreCase))
		{
			return new ControlChannelStrategyForCodeDZero24(responseWrapper);
		}

		if (string.Equals(code, ControlChannelStrategyForCodeZero.Code_Zero, StringComparison.OrdinalIgnoreCase))
		{
			return new ControlChannelStrategyForCodeZero(responseWrapper);
		}

		return null;
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant response message")]
	const string Ucc6ResponseMessageProcessorFriendlyName = "UCC6 Electronic Folder Query Response Message Processor";
}
