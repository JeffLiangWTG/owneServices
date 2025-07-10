using System.Collections.Generic;

namespace ServiceManager.Host.Abstractions;

public interface IAllTasksConsumer
{
	bool TryGetByCode(string code, out IRunnableServiceTask result);

	IEnumerable<IRunnableServiceTask> GetAll();

	int Count { get; }
}
