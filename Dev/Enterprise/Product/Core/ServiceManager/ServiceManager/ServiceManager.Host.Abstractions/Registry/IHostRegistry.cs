namespace ServiceManager.Host.Abstractions;

public interface IHostRegistry
{
	void Initialize();
	void Refresh();
}
