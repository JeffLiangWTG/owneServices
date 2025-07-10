using System.Threading;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using NUnit.Framework;

namespace Enterprise.Customs.GB.ICS.Testing
{
	public class ICSOutgoingMessageProcessorTests : TestCaseWithFactory
	{
		[TestDate(2015, 10, 30, 09, 36, 0)]
		public void TestProcess()
		{
			ICSMessageSenderTestHelper.TestProcess(() =>
			{
				var processor = new ICSOutgoingMessageProcessor(new LoggingInformation());
				processor.ProcessMessage(CancellationToken.None);
			});
		}
	}
}
