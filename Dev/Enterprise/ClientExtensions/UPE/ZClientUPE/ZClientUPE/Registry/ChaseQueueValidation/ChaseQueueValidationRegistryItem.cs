using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE.Registry.Business
{
	public class ChaseQueueValidationRegistryItem : StronglyTypedRegistryItem<ChaseQueueValidationCollection>
	{
		public ChaseQueueValidationRegistryItem(string name, string category, string caption, string hint)
			: this(name, category, caption, hint, new ChaseQueueValidationCollection())
		{
		}

		public ChaseQueueValidationRegistryItem(string name, string category, string caption, string hint, ChaseQueueValidationCollection defaultValue)
			: base(new RegistryItemImpl(name, (NoResString)category, (NoResString)caption, (NoResString)hint, new ChaseQueueValidationRegistryDataType(), RegistryStorageFlags.System, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Client.UPE.Registry.GUI.ChaseQueueValidationRegistryItemEditor, ZClientUPE")]
	class ChaseQueueValidationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ChaseQueueValidationCollection>
	{
		public ChaseQueueValidationRegistryDataType()
		{
		}
	}
}
