using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;

namespace Enterprise.Messaging.MessageProcessors.Testing
{
	class BranchCustomsApplicationTypeMessageProcessorTest : TestCaseWithFactory
	{
		public void TestMessageFilter()
		{
			var processor = new BranchCustomsApplicationTypeMessageProcessorTestHelper(new LoggingInformation());
			AssertEquals(true, processor.MessageFilter.IsEmpty);
		}

		public void TestRequiresPreProcessing()
		{
			var processor = new BranchCustomsApplicationTypeMessageProcessorTestHelper(new LoggingInformation());
			AssertEquals(true, processor.RequiresPreProcessing);
		}
	}
}
