using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class VerboseLoggingRegistryItem : StronglyTypedRegistryItem<VerboseLoggingCollection>
	{
		public VerboseLoggingRegistryItem(
			string name,
			MultilingualString category,
			MultilingualString caption,
			MultilingualString hint,
			RegistryStorageFlags storage,
			RegistryOptions registryOptions)
			: base(new RegistryItemImpl(
				name,
				category,
				caption,
				hint,
				new VerboseLoggingRegistryDataType(),
				storage,
				registryOptions))
		{
		}
	}
}
