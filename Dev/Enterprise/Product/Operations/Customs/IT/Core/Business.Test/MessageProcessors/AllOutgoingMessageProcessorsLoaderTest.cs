using System;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class AllOutgoingMessageProcessorsLoaderTest : TestCase
{
	public void TestGetAllThrowsException()
	{
		AssertExceptionThrown<ArgumentNullException>(() => AllOutgoingMessageProcessorsLoader.GetAll(null));
	}

	public void TestGetAll()
	{
		var logger = new LoggingInformationForTesting();
		var allOutgoingMessageProcessors = AllOutgoingMessageProcessorsLoader.GetAll(logger).ToArray();
		AssertEquals("Number of outgoing message processors", 3, allOutgoingMessageProcessors.Length);
		CombineAssertions(() =>
		{
			AssertType<SadOutgoingMessageProcessor>("Processor at 0", allOutgoingMessageProcessors[0]);
			AssertType<SingleWindowOutgoingMessageProcessor>("Processor at 1", allOutgoingMessageProcessors[1]);
			AssertType<XTradeOutgoingMessageProcessor>("Processor at 2", allOutgoingMessageProcessors[2]);
		});
	}
}
