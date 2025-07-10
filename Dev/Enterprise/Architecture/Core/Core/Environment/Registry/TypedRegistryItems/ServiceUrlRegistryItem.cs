using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class ServiceUrlRegistryItem : StronglyTypedRegistryItem<string>
	{
		public ServiceUrlRegistryItem(IRegistryItem inner) : base(inner)
		{
		}

		public ServiceUrlRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, string defaultValue)
			: this(name, category, caption, hint, storage, RegistryOptions.Default, defaultValue)
		{
		}

		public ServiceUrlRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, string defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ServiceUrlRegistryDataType(), null, storage, options, defaultValue, false))
		{
		}

		public ServiceUrlRegistryItem(string name, MultilingualString[] categories, MultilingualString caption, MultilingualString hint, IRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, string defaultValue)
			: base(new RegistryItemImpl(name, caption, hint, new ServiceUrlRegistryDataType(), editorInfo, storage, options, defaultValue, false, categories))
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
