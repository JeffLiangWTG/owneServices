using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.xTMessaging.Business;
using Enterprise.xTMessaging.Shared;
using InboundInterchangeProcessor = Enterprise.xTMessaging.ServiceTasks.InboundInterchangeProcessor;

namespace Enterprise.Client.EDI.ServiceTasks.TestDirectXt
{
	public class TestDirectXtInboundProcessor : InboundInterchangeProcessor
	{
		public TestDirectXtInboundProcessor(ILogger logger) : base(logger) { }

		protected override IReceiveHandler GetSaveToEDIInterchangeHandler()
		{
			return new SaveToEDIInterchangeTestHandler(logger);
		}

		protected override bool CheckIsEnabled() => true;
	}

	public class SaveToEDIInterchangeTestHandler : SaveToEDIInterchangeHandler
	{
		public SaveToEDIInterchangeTestHandler(ILogger logger) : base(logger) { }

		protected override ZString TransportType => EDIInterchange.TransportType.tXT;
	}
}
