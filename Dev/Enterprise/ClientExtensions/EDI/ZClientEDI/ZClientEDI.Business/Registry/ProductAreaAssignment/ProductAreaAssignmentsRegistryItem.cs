using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class ProductAreaAssignmentsRegistryItem : StronglyTypedRegistryItem<ProductAreaAssignmentCollection, ProductAreaAssignmentCollection>
	{
		public ProductAreaAssignmentsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, category, caption, hint, storage, new ProductAreaAssignmentCollection())
		{
		}

		public ProductAreaAssignmentsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, ProductAreaAssignmentCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ProductAreaAssignmentsRegistryDataType(), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Client.EDI.Registry.GUI.ProductAreaAssignmentsRegistryEditor, ZClientEDI")]
	public class ProductAreaAssignmentsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ProductAreaAssignmentCollection>
	{
		public ProductAreaAssignmentsRegistryDataType()
		{
		}
	}
}

