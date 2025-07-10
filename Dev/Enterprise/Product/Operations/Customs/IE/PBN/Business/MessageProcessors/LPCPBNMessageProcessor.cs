using System;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.PBN.Messaging;

namespace Enterprise.Customs.IE.PBN.Business;

public class LPCPBNMessageProcessor : PBNMessageProcessor<LPCPBNProvider>
{
	public LPCPBNMessageProcessor(LoggingInformation logger, Type messageObjectType) : base(logger, messageObjectType)
	{
	}

	protected override string MessageFriendlyNameCore => Res.GetString("31A194FC-518B-40B1-A0C2-D16F86634950", "PBN LPC Message Processor");

	protected override Type MessageInterpreterType => typeof(LPCPBNMessageInterpreter);

	protected override void ProcessMessageCore(BusinessObjectFactory factory, PBNInboundEDIMessage message, LPCPBNProvider provider)
	{
		base.ProcessMessageCore(factory, message, provider);
		message.EM_ApplicationReference = provider.PbnID;
	}
}
