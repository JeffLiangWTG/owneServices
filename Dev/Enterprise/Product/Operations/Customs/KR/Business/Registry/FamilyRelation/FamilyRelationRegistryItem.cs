using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.KR.Business
{
	public class FamilyRelationRegistryItem : StronglyTypedRegistryItem<FamilyRelationCollection>
	{
		public FamilyRelationRegistryItem(string name, MultilingualString category, string caption, string hint, FamilyRelationCollection defaultValue)
			: this(name, category, caption, hint, RegistryStorageFlags.System, defaultValue)
		{
		}

		public FamilyRelationRegistryItem(string name, MultilingualString category, string caption, string hint, RegistryStorageFlags storage, FamilyRelationCollection defaultValue)
			: base(new FamilyRelationRegistryItemImpl(name, category, caption, hint, storage, defaultValue))
		{
		}

		class FamilyRelationRegistryItemImpl : RegistryItemImpl
		{
			public FamilyRelationRegistryItemImpl(string name, MultilingualString category, string caption, string hint, RegistryStorageFlags storage, FamilyRelationCollection defaultValue)
				: base(name, category, (NoResString)caption, (NoResString)hint, new FamilyRelationRegistryDataType(), storage, defaultValue)
			{
			}
		}
	}

	[RegistryEditor("Enterprise.Customs.KR.GUI.FamilyRelationRegistryItemEditor, Enterprise.Customs.KR.GUI")]
	public class FamilyRelationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<FamilyRelationCollection>
	{
	}
}
