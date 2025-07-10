using System;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.EMCS.Messaging;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class IE905MessageProcessor : EMCSMessageProcessor<IIE905>
	{
		public IE905MessageProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("5489CA47-E174-4131-8F0B-2CAC91B8E40C", "EMCS IE905 Message Processor");

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EMCSInboundEDIMessage message, IIE905 provider)
		{
		}
	}
}
