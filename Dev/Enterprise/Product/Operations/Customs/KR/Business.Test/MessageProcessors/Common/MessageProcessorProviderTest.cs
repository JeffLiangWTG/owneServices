using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class MessageProcessorProviderTest : TestCaseWithFactory
	{
		public void TestGetProcessor()
		{
			var processorProvider = new MessageProcessorProvider();
			AssertType<MessageR20Processor>(processorProvider.GetProcessor(ElectronicDocumentTypeList.Codes._R20));
		}

		public void TestGetProcessorThrowExceprtion()
		{
			var processorProvider = new MessageProcessorProvider();
			AssertNoExceptionThrown("No exception is raised if one processor is found.", () => processorProvider.GetProcessor("BBB", GetType().Assembly));
			AssertExceptionThrown<InvalidOperationException>("An exception is thrown when there is more than one process with the same attribute.", () => processorProvider.GetProcessor("AAA", GetType().Assembly));
		}

		#region Test Processor Data
		[CodeAlive("This code is only test data.")]
		[MessageType("AAA")]
		public class TestProcessorData1 : IMessageProcessor
		{
			public void Process(EDIMessage message)
			{
			}
		}
		[CodeAlive("This code is only test data.")]
		[MessageType("AAA")]
		public class TestProcessorData2 : IMessageProcessor
		{
			public void Process(EDIMessage message)
			{
			}
		}
		[CodeAlive("This code is only test data.")]
		[MessageType("BBB")]
		public class TestProcessorData3 : IMessageProcessor
		{
			public void Process(EDIMessage message)
			{
			}
		}
		#endregion
	}
}
