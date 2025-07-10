using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class TransportModeCombinationBufferTimeRegistryItem : StronglyTypedRegistryItem<TransportModeCombinationBufferTimeCollection>
	{
		public TransportModeCombinationBufferTimeRegistryItem(
			string name,
			MultilingualString category,
			MultilingualString caption,
			MultilingualString hint,
			RegistryStorageFlags storage,
			TransportModeCombinationBufferTimeCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new TransportModeCombinationBufferTimeRegistryDataType(), storage, defaultValue))
		{
		}
	}
}
