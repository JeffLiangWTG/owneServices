using System;

namespace ServiceManager.Integration.Abstractions;

public interface IHostedServiceQueueProviderAttribute
{
	string ServiceTaskCode { get; }
	string QueueName { get; }
	Type Type { get; }
}
