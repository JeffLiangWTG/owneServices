using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.ServiceTasks;

namespace Enterprise.Client.EDI.ServiceTasks.TestDirectXt
{
	public class TestDirectXtOutboundProcessor : OutboundInterchangeProcessor
	{
		public TestDirectXtOutboundProcessor(ILogger logger) : base(logger) { }

		protected override string TransportType => EDIInterchangeTransportTypeList.Codes.tXT;
	}
}
