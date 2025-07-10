using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class FreeWaitingTimeRegistryItem : StronglyTypedRegistryItem<FreeWaitingTimeCollection>
	{
		public FreeWaitingTimeRegistryItem(
			string name,
			MultilingualString category,
			MultilingualString caption,
			MultilingualString hint,
			RegistryStorageFlags storage,
			FreeWaitingTimeCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new FreeWaitingTimeRegistryDataType(), storage, defaultValue))
		{
		}

		public FreeWaitingTimeRegistryItem(
			string name,
			MultilingualString category,
			MultilingualString caption,
			MultilingualString hint,
			RegistryStorageFlags storage,
			RegistryOptions options,
			FreeWaitingTimeCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new FreeWaitingTimeRegistryDataType(), storage, options, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.FreeWaitingTimeRegistryItemEditor, Enterprise.Registry.GUI")]
	class FreeWaitingTimeRegistryDataType : NonPersistentBusinessObjectRegistryDataType<FreeWaitingTimeCollection>
	{
		public FreeWaitingTimeRegistryDataType()
		{
		}
	}
}
