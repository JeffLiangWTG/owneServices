using System.Collections.Generic;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment
{
	public interface IRegistryItemSetLocator
	{
		IEnumerable<IRegistryItem> GetAllRegistryItems();
		IEnumerable<RegistryItemSet> GetRegistryItemSets();
	}
}
