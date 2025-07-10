using Enterprise.BatchProcessor;

namespace Enterprise.Messaging.Business.Testing
{
	public interface IMessageProcessorForTest
	{
		void Execute();
		void ExecuteBatch();
		int FactorySaveCount { get; }
		LoggingInformation Logger { get; set; }
		int MessagesProcessed { get; set; }
		bool MessageShouldBeProcessedInASeparateFactoryExposed { get; }
		int ApplicationTypeProcessorFindCount { get; }
	}
}
