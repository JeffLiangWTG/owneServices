namespace ServiceManager.Integration.Abstractions
{
	public interface IHostedServiceQueue
	{
		QueueResult QueueResult { get; }
		string Name { get; }
		string ServiceTaskCode { get; }
	}
}
