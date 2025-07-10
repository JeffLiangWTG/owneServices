using ServiceManager.Shared.Abstractions;

namespace ServiceManager.Common.Abstractions;
public interface IServiceTaskLoaderFactory
{
	IServiceTaskLoader CreateServiceTaskLoader();
}

