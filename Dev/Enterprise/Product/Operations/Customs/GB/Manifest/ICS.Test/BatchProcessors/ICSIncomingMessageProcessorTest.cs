using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.GB.ICS.Testing
{
	public class ICSIncomingMessageProcessorTest : TestCaseWithFactory
	{
		public void TestMessageProcessors()
		{
			var processor = new ICSIncomingMessageProcessorForTest(new LoggingInformation());
			Assert(processor.MessageProcessors.Any(x => x.GetType() == typeof(IcsNorthernIrelandResponseMessageProcessor)));
			Assert(processor.MessageProcessors.Any(x => x.GetType() == typeof(CC304AResponseMessageProcessor)));
			Assert(processor.MessageProcessors.Any(x => x.GetType() == typeof(CC305AResponseMessageProcessor)));
			Assert(processor.MessageProcessors.Any(x => x.GetType() == typeof(CC316AResponseMessageProcessor)));
			Assert(processor.MessageProcessors.Any(x => x.GetType() == typeof(CC324AResponseMessageProcessor)));
			Assert(processor.MessageProcessors.Any(x => x.GetType() == typeof(CC325AResponseMessageProcessor)));
			Assert(processor.MessageProcessors.Any(x => x.GetType() == typeof(CC328AResponseMessageProcessor)));
			Assert(processor.MessageProcessors.Any(x => x.GetType() == typeof(CC351AResponseMessageProcessor)));
		}
	}

	public class ICSIncomingMessageProcessorForTest : ICSIncomingMessageProcessor
	{
		public ICSIncomingMessageProcessorForTest(LoggingInformation logger) : base(logger)
		{
		}

		public new List<ApplicationTypeMessageProcessor> MessageProcessors => base.MessageProcessors;
	}
}
