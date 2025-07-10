using System.Collections.Generic;
using System.Linq;
using Enterprise.Messaging.MessageProcessors;
using NUnit.Framework;

namespace Enterprise.Customs.HK.ServiceTasks.Testing
{
	class ProcessorTest : TestCase
	{
		public void TestProcessorContainsTraxonResponseProcessor()
		{
			var messageProcessors = new ProcessForTest().GetMessageProcessors_Exposed();
			AssertNotNull(messageProcessors.SingleOrDefault(x => x is TraxonResponseProcessor));
		}

		class ProcessForTest : Processor
		{
			public List<ApplicationTypeMessageProcessor> GetMessageProcessors_Exposed() => base.GetMessageProcessors();
		}
	}
}
