using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class BooleanRegistryItem : StronglyTypedRegistryItem<bool>
	{
		public BooleanRegistryItem(IRegistryItem inner) : base(inner)
		{
		}

		public BooleanRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, bool defaultValue)
			: this(name, category, caption, hint, storage, RegistryOptions.Default, defaultValue)
		{
		}

		public BooleanRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, bool defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new BooleanRegistryDataType(), null, storage, options, defaultValue, false))
		{
		}

		public BooleanRegistryItem(string name, MultilingualString[] categories, MultilingualString caption, MultilingualString hint, IRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, bool defaultValue)
			: base(new RegistryItemImpl(name, caption, hint, new BooleanRegistryDataType(), editorInfo, storage, options, defaultValue, false, categories))
		{
		}

		public BooleanRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, bool defaultValue, BooleanRegistryDataType registryDataType)
			: base(new RegistryItemImpl(name, category, caption, hint, registryDataType, null, storage, options, defaultValue, false))
		{
		}

		protected override void SetValueCore(Guid companyOrOwnerPK, Guid branchPK, Guid departmentPK, object newValue)
		{
			base.SetValueCore(companyOrOwnerPK, branchPK, departmentPK, newValue);

			if (ValueChanged != null)
			{
				ValueChanged(this, EventArgs.Empty);
			}
		}

		internal event EventHandler ValueChanged;
	}
}
