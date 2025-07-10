using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class LegTypeCollectionRegistryItem : StronglyTypedRegistryItem<LegTypeCollection>
	{
		public LegTypeCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, LegTypeCollection defaultValues)
			: base(new RegistryItemImpl(name, category, caption, hint, new LegTypeRegistryDataType(), storage, defaultValues))
		{
		}

		#region class LegTypeRegistryDataType

		[RegistryEditor("Enterprise.Registry.GUI.LegTypesRegistryItemEditor, Enterprise.Registry.GUI")]
		internal class LegTypeRegistryDataType : FallbackMergedRegistryBusinessObjectCollectionDataType<LegTypeCollection>
		{
			public LegTypeRegistryDataType()
			{
			}
		}

		#endregion
	}
}
