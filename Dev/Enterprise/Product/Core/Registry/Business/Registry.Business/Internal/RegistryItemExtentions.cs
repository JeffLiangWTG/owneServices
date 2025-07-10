using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public static class RegistryItemExtentions
	{
		/// <summary>
		/// Gets the registry value, caching the result in the passed in factory.
		/// </summary>
		public static TGet GetFactoryCachedValue<TGet, TSet>(this StronglyTypedRegistryItem<TGet, TSet> registryItem, BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<TGet>("RegistryCache:" + registryItem.Name, delegate
			{
				return registryItem.Value;
			});
		}
	}
}
