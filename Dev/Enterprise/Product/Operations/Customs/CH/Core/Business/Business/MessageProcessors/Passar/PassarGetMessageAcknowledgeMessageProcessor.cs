using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public sealed class PassarGetMessageAcknowledgeMessageProcessor : PassarGetMessageAcknowledgeMessageProcessor<IPassarResponseDetail>
{
	public PassarGetMessageAcknowledgeMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"Passar Get Message Acknowledge Message Processor";

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.Undefined };

	internal protected override bool LinkMessageToCompany => true;

	protected override IPassarResponseDetail DeserializeResponse(CHEDIMessage message) => null;

	protected override void ProcessResponseMessage(CHEDIMessage message, IPassarResponseDetail customsResponse) { }
}
