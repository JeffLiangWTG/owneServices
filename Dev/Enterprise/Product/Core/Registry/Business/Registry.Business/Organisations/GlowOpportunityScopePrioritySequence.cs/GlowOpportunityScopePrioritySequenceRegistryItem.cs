using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class GlowOpportunityScopePrioritySequenceRegistryItem : StronglyTypedRegistryItem<GlowOpportunityScopePrioritySequenceCollection>
	{
		public GlowOpportunityScopePrioritySequenceRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, GlowOpportunityScopePrioritySequenceCollection defaultValue)
			: this(name, category, caption, hint, storage, RegistryOptions.Default, defaultValue)
		{
		}

		public GlowOpportunityScopePrioritySequenceRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, GlowOpportunityScopePrioritySequenceCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new GlowOpportunityScopePrioritySequenceRegistryDataType(), storage, options, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.GlowOpportunityScopePrioritySequenceRegistryItemEditor, Enterprise.Registry.GUI")]
	public class GlowOpportunityScopePrioritySequenceRegistryDataType : NonPersistentBusinessObjectRegistryDataType<GlowOpportunityScopePrioritySequenceCollection>
	{
	}
}
