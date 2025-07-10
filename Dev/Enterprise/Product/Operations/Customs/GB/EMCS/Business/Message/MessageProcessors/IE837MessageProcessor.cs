using System;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.EMCS.Messaging;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class IE837MessageProcessor : EMCSMessageProcessor<IIE837>
	{
		public IE837MessageProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("43FD7CFC-7C0D-495C-9266-52B5155FC1CA", "EMCS IE837 Message Processor");

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EMCSInboundEDIMessage message, IIE837 provider)
		{
		}
	}
}
