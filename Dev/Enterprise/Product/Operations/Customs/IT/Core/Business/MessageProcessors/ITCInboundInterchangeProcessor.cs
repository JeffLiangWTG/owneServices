using System;
using System.Threading;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business;

public class ITCInboundInterchangeProcessor : InboundInterchangeProcessor
{
	public static void UnpackInterchanges(LoggingInformation logger, CancellationToken token)
	{
		using (var processor = new ITCInboundInterchangeProcessor(logger))
		{
			((IInboundInterchangeProcessor)processor).Execute(token);
		}
	}

	protected ITCInboundInterchangeProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string[] ApplicationCodes => new string[]
	{
		EDIMessage.ApplicationCodes.ITCustoms,
		Enterprise.Messaging.Integration.ApplicationCodeList.Codes.ITCustomsXTrade,
	};

	protected override Type TypeOfInterchangeToCreate() => typeof(EDIInterchange);

	protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange) => InboundMessageCreatorFactory.GetNew(interchange);

	protected override bool AddNoteOnException => true;
}
