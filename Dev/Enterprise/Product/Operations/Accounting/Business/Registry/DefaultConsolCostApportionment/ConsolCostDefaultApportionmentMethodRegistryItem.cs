using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class ConsolCostDefaultApportionmentMethodRegistryItem : StronglyTypedRegistryItem<ConsolCostDefaultApportionmentMethodConfiguration>
	{
		public ConsolCostDefaultApportionmentMethodRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new ConsolCostDefaultApportionmentMethodDataType(), storage))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.ConsolCostDefaultApportionmentMethodRegistryItemEditor, Enterprise.Accounting.GUI")]
#if DEBUG
	internal
#endif
	class ConsolCostDefaultApportionmentMethodDataType : NonPersistentBusinessObjectRegistryDataType<ConsolCostDefaultApportionmentMethodConfiguration>
	{
		public ConsolCostDefaultApportionmentMethodDataType()
		{
		}
	}
}
