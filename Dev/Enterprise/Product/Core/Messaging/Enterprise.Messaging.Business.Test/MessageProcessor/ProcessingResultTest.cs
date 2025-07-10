using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Messaging.Business.Test.MessageProcessor
{
	sealed class ProcessingResultTest : TestCase
	{
		public void TestProcessingResultConversions()
		{
			ProcessingResult<int> processingResult;
			ProcessingResult<int> processingResultWithDiscardReason;
			processingResult = 1;
			processingResultWithDiscardReason = new ProcessingResult<int>(1, (NoResString)"some reason");
			AssertEquals(1, (int)processingResult);
			AssertEquals(1, (int)processingResultWithDiscardReason);
			AssertEquals(new ProcessingResult<int>(1), processingResult);
			AssertNotEquals(processingResult, processingResultWithDiscardReason);
		}

		public void TestNew()
		{
			AssertType<ProcessingResult<int>>(ProcessingResult.New(1));
			AssertType<ProcessingResult<int>>(ProcessingResult.New(1, (NoResString)"123"));
		}
	}
}
