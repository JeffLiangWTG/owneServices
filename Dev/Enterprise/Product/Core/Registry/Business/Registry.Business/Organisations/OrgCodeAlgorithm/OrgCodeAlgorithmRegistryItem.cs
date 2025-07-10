using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class OrgCodeAlgorithmRegistryItem : StronglyTypedRegistryItem<OrgCodeAlgorithm>
	{
		public OrgCodeAlgorithmRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, OrgCodeAlgorithm defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new OrgCodeAlgorithmRegistryDataType(defaultValue.AlgorithmType), RegistryStorageFlags.System, defaultValue))
		{
		}

		public OrgCodeAlgorithmRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryOptions options, OrgCodeAlgorithm defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new OrgCodeAlgorithmRegistryDataType(defaultValue.AlgorithmType), RegistryStorageFlags.System, options, defaultValue))
		{
		}
	}
}
