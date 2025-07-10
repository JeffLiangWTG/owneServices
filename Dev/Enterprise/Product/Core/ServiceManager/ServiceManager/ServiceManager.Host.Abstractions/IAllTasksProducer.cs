namespace ServiceManager.Host.Abstractions;

public interface IAllTasksProducer
{
	void Add(IRunnableServiceTask task);
	void Clear();
}
