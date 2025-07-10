using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.GB.GVMS.Testing
{
	public class GVMSIncomingMessageProcessorTest : TestCaseWithFactory
	{
		public void TestMessageProcessors()
		{
			var processor = new GVMSIncomingMessageProcessorForTest(new LoggingInformation());
			Assert(processor.MessageProcessors.Any(x => x.GetType() == typeof(GVMSResponseMessageProcessor)));
		}
	}

	public class GVMSIncomingMessageProcessorForTest : GVMSIncomingMessageProcessor
	{
		public GVMSIncomingMessageProcessorForTest(LoggingInformation logger) : base(logger)
		{
		}

		public new List<ApplicationTypeMessageProcessor> MessageProcessors => base.MessageProcessors;
	}
}
