using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public abstract class MilestoneEventUpdatesRegistryItem<T> : StronglyTypedRegistryItem<T>, IMilestoneEventUpdatesRegistryItem
		where T : MilestoneEventUpdatesCollection
	{
		public MilestoneEventUpdatesRegistryItem(string name, MultilingualString caption, MultilingualString hint, MultilingualString category, IMilestoneEventUpdatesRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions options, MilestoneEventUpdatesCollection defaultValue)
			: base(new RegistryItemImpl(name, caption, hint, dataType, null, storage, options, defaultValue, false, new MultilingualString[] { category }))
		{
		}
	}

	public interface IMilestoneEventUpdatesRegistryDataType : IRegistryDataType
	{
	}

	public abstract class MilestoneEventUpdatesRegistryDataType<T> : NonPersistentBusinessObjectRegistryDataType<T>, IMilestoneEventUpdatesRegistryDataType
		where T : MilestoneEventUpdatesCollection
	{
		public MilestoneEventUpdatesRegistryDataType()
		{
		}
	}
}
