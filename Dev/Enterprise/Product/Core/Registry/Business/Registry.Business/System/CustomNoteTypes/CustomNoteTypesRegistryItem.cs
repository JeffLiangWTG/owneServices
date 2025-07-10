using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class CustomNoteTypesRegistryItem : StronglyTypedRegistryItem<CustomNoteTypes>
	{
		public CustomNoteTypesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new CustomNoteTypesRegistryDataType(), storage))
		{
		}

		public CustomNoteTypesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new CustomNoteTypesRegistryDataType(), storage, options))
		{
		}

		protected override void SetValueCore(Guid companyOrOwnerPK, Guid branchPK, Guid departmentPK, object newValue)
		{
			base.SetValueCore(companyOrOwnerPK, branchPK, departmentPK, newValue);
			PredefinedNoteTypes.Instance.ClearCacheOfAllNotes();
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.CustomNoteTypesRegistryItemEditor, Enterprise.Registry.GUI")]
	class CustomNoteTypesRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CustomNoteTypes>
	{
		public CustomNoteTypesRegistryDataType()
		{
		}
	}
}
