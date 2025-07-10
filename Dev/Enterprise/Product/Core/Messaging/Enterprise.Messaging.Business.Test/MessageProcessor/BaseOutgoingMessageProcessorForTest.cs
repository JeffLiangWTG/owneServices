using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;

namespace Enterprise.Messaging.Business.MessageProcessor.Testing
{
	class BaseOutgoingMessageProcessorForTest : BaseOutgoingMessageProcessor
	{
		public BaseOutgoingMessageProcessorForTest(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZQuery MessageFilter => new ZQuery();

		protected override void HandleBeforeMessageProcessing(NonDependentEDIMessageCollection readyMessages, BusinessObjectFactory factory)
		{
			foreach (EDIMessage message in readyMessages)
			{
				message.EM_Status = EDIMessage.Status.Sent;
			}
		}
	}
}
