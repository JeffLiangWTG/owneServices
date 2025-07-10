using System.Collections.Generic;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public interface IRegistryRelatedFilesLocator
	{
		IEnumerable<string> GetRelatedFilePaths(IEnumerable<RegistryItemSet> sets);
	}
}
