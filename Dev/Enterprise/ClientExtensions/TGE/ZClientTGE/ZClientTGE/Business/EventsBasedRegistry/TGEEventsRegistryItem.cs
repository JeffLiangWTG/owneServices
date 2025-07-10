using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.TGE.Business
{
	public class TGEEventsRegistryItem : StronglyTypedRegistryItem<TGEEventRegistryBusinessObjectCollection>
	{
		public TGEEventsRegistryItem(string name, string category, string caption, string hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, (NoResString)category, (NoResString)caption, (NoResString)hint, new TGEEventsRegistryDataType(), storage))
		{
		}
	}

	[RegistryEditor("Enterprise.Client.TGE.Business.TGEEventsRegistryItemEditor, ZClientTGE")]
	internal class TGEEventsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<TGEEventRegistryBusinessObjectCollection>
	{
		public TGEEventsRegistryDataType()
		{
		}
	}
}
