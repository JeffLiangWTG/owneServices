using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class DateTimeRegistryItem : StronglyTypedRegistryItem<DateTime>
	{
		public DateTimeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, category, caption, hint, storage, RegistryOptions.Default)
		{
		}

		public DateTimeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: this(name, category, caption, hint, new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Short), storage, options, DateTime.MinValue, false)
		{
		}

		public DateTimeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, DateTime defaultValue)
			: this(name, category, caption, hint, new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Short), storage, RegistryOptions.Default, defaultValue, false)
		{
		}

		public DateTimeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, DateTime defaultValue)
			: this(name, category, caption, hint, new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Short), storage, options, defaultValue, false)
		{
		}

		public DateTimeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, DateTimeRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, DateTime defaultValue, bool useDefaultDefaultValue)
			: this(new RegistryItemImpl(name, category, caption, hint, RegistryDataTypes.DateTimeType, editorInfo, storage, options, defaultValue, useDefaultDefaultValue))
		{
		}

		public DateTimeRegistryItem(IRegistryItem inner) : base(inner)
		{
		}

		protected override void SetValueCore(Guid companyOrOwnerPK, Guid branchPK, Guid departmentPK, object newValue)
		{
			base.SetValueCore(companyOrOwnerPK, branchPK, departmentPK, newValue ?? DateTime.MinValue);
		}
	}
}
