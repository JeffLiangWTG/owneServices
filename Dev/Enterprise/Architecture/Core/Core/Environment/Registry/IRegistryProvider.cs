using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Core.Environment.Registry
{
	public interface IRegistryProvider
	{
		IRegistry CreateRegistry(IRegistryItemVisibility visibility);
	}
}
