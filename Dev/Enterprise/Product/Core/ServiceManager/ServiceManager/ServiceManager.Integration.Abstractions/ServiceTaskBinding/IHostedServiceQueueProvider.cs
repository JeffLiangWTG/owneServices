namespace ServiceManager.Integration.Abstractions
{
	public interface IHostedServiceQueueProvider
	{
		QueueResult QueueResult { get; }
	}
}
