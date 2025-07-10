using Enterprise.Integration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.MailManager.Integration
{
	public interface IMessageProcessor<T>
	{
		bool Process(IMessageProcessorContext context, T bo);
		bool IsMatch(IMessageProcessorContext context, T bo);
	}

	public interface IMessageProcessorContext
	{
		ILogger Logger { get; }
		T GetFilterInstance<T>() where T : new();
	}

	[ThreadSafe]
	public interface IMessageProcessorFactory
	{
		IMessageProcessor<T> GetProcessor<T>();
		IMessageProcessor<T> GetProcessor<T>(string tableName);
		IMessageProcessorContext GetContext(ILogger logger);
	}
}
