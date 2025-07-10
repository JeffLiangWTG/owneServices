using System.Collections.Generic;

namespace ServiceManager.Shared.Abstractions
{
	public interface IServiceTaskLoader
	{
		IServiceTask? Load(string serviceTaskCode);
		IEnumerable<IServiceTask> LoadAll();
	}
}
