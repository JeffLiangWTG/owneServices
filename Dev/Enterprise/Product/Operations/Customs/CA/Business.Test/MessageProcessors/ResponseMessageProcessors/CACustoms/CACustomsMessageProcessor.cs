using System.Linq;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.MessageProcessors.Testing;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	public class CACustomsMessageProcessorTest : ApplicationTypeMessageProcessorTest
	{
		public void TestOverrides()
		{
			var processor = new CACustomsMessageProcessor(logger);
			AssertEquals("ApplicationCode", EDIMessage.ApplicationCodes.CACustoms, processor.ApplicationCode);
			AssertEquals("MessageFriendlyName", "CA Customs", processor.MessageFriendlyName);
		}

		public void TestGetMessageProcessor()
		{
			var message = Factory.New<CARMDailyNoticeMessage>();
			var cacMessageProcessor = new CACustomsMessageProcessor(logger);
			AssertNoExceptionThrown(() =>
			{
				cacMessageProcessor.ProcessMessage(message);
			});
			AssertEquals(EDIMessage.Status.Failed, message.EM_Status);
			AssertEquals(1, logger.Logs.Count());
			AssertEquals("Processing message failed : Daily Notice message has incorrect message subtype : ", logger.Logs.ToArray()[0].Message);
			logger.ClearLogs();

			var message3 = Factory.New<EDIMessage>();
			AssertNoExceptionThrown(() =>
			{
				cacMessageProcessor.ProcessMessage(message3);
			});

			AssertEquals(EDIMessage.Status.Failed, message3.EM_Status);
			AssertEquals(1, logger.Logs.Count());
			AssertEquals("Can't find a valid processor for this message.", logger.Logs.ToArray()[0].Message);
		}

		protected override void SetUp()
		{
			base.SetUp();
			logger = new LoggingInformation();
		}

		LoggingInformation logger;
	}
}
