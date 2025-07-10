namespace ServiceManager.Host.Abstractions;

public interface IHostStartupCommandResolver
{
	IHostStartupCommand Resolve();
}
