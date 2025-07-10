using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Warehouse
{
	public class ABCAnalysisCategoryRegistryItem : StronglyTypedRegistryItem<ABCAnalysisCategoryCollection>
	{
		public ABCAnalysisCategoryRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, ABCAnalysisCategoryCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ABCAnalysisCategoryRegistryDataType(), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.ABCAnalysisCategoryRegistryItemEditor, Enterprise.Registry.GUI")]
	class ABCAnalysisCategoryRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ABCAnalysisCategoryCollection>
	{
	}
}
