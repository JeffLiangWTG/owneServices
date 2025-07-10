using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	public class DocBuilderDataSourceRegistryItem : StronglyTypedRegistryItem<DocBuilderDataSource>
	{
		public DocBuilderDataSourceRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, DocBuilderDataSource defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new DocBuilderDataSourceDataType(), null, storage, options, defaultValue, false))
		{
		}
	}

	[RegistryEditor("Enterprise.DocumentEngineCore.GUI.Registry.DocBuilderDataSourceRegistryItemEditor, Enterprise.DocumentEngineCore.GUI")]
	class DocBuilderDataSourceDataType : NonPersistentBusinessObjectRegistryDataType<DocBuilderDataSource>
	{
		public DocBuilderDataSourceDataType()
		{
		}
	}
}
