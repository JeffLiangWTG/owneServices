using System;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.PBN.Messaging;

namespace Enterprise.Customs.IE.PBN.Business;

public class LPBPBNMessageProcessor : PBNMessageProcessor<LPBPBNProvider>
{
	public LPBPBNMessageProcessor(LoggingInformation logger, Type messageObjectType) : base(logger, messageObjectType)
	{
	}

	protected override string MessageFriendlyNameCore => Res.GetString("F9CFCACF-C61D-45FE-BC6B-51E4922AA204", "PBN LPB Message Processor");

	protected override Type MessageInterpreterType => typeof(LPBPBNMessageInterpreter);

	protected override void ProcessMessageCore(BusinessObjectFactory factory, PBNInboundEDIMessage message, LPBPBNProvider provider)
	{
		base.ProcessMessageCore(factory, message, provider);
		if (provider.Status == PBNCustomsStatusList.EnglishDescriptions.Proceed || provider.Status == PBNCustomsStatusList.EnglishDescriptions.Incomplete || provider.Status == PBNCustomsStatusList.EnglishDescriptions.CheckedIn)
		{
			message.EM_ApplicationReference = provider.PbnID;
		}
	}
}
