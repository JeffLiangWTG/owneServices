using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.IN.Business;

public abstract class BaseMessageProcessor
{
	protected BaseMessageProcessor(EDIMessage message, LoggingInformation logger)
	{
		Message = message;
		Logger = logger;
	}

	public BusinessObject GetLinkedObject()
	{
		Logger.DebugLog($"Get LinkedObject by {ProcessorName}");
		return GetLinkedObjectCore();
	}

	public bool Process()
	{
		Logger.DebugLog($"Message processing by {ProcessorName}");
		return ProcessCore();
	}

	protected EDIMessage Message { get; }
	protected LoggingInformation Logger { get; }

	protected abstract ZString ProcessorName { get; }

	protected abstract bool ProcessCore();
	protected abstract BusinessObject GetLinkedObjectCore();
}
