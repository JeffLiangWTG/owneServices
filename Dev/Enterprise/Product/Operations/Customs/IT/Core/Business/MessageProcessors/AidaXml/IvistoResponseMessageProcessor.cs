using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Customs.IT.MessageDefinitions.Declaration.Export.Ivisto;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

sealed class IvistoResponseMessageProcessor : XmlIncomingMessageProcessor
{
	public IvistoResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => IvistoResponseMessageProcessorFriendlyName;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { EDIMessageTypeList.Codes.IvistoResponse };

	protected override void ProcessResponse(IXmlCustomsLinkedObjectAdapter entryAdapter, EDIMessage receivedMessage, EDIMessage originalSentMessage)
	{
		var responseMessage = new Ucc6IvistoResponseMessage(receivedMessage.EM_MessageText);
		if (!IsPositive(responseMessage))
		{
			return;
		}

		var (exitDate, exitOffice, exitResult) = GetExitInfo(responseMessage);
		if (exitDate.IsEmpty && exitOffice.IsEmpty && exitResult.IsEmpty)
		{
			return;
		}

		entryAdapter.SetStatusAsExitCompleted();
		entryAdapter.UpdateOrInsertIvistoEntryNumber(exitDate, exitOffice, exitResult);
	}

	#region Implementation

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Messsage string")]
	const string IvistoResponseMessageProcessorFriendlyName = "UCC6 Ivisto Response Message Processor";

	bool IsPositive(IResponseMessage responseMessage) => positiveStatusCollection.Contains(responseMessage.ResponseStatus);

	readonly ImmutableArray<ZString> positiveStatusCollection = new ZString[]
	{
		Ucc6AcknowledgementStatusList.Codes.ElaborationOkWithoutResult,
		Ucc6AcknowledgementStatusList.Codes.ElaborationOkWithResult,
	}.ToImmutableArray();

	static (ZDateTime ExitDate, ZString ExitOffice, ZString ExitResult) GetExitInfo(Ucc6IvistoResponseMessage responseMessage)
	{
		var ivistoResponse = (IIvistoResponse)responseMessage.Data;
		return (new ZDateTime(ivistoResponse.ExitDate), ivistoResponse.ExitOffice, ivistoResponse.ExitResult);
	}

	#endregion
}
