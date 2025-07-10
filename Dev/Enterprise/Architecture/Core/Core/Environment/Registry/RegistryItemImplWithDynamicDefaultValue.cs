using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class RegistryItemImplWithDynamicDefaultValue : RegistryItemImpl
	{
		public RegistryItemImplWithDynamicDefaultValue(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage, DefaultValueGetter defaultValueGetter)
			: this(name, category, caption, hint, dataType, storage, RegistryOptions.Default, defaultValueGetter)
		{
		}

		public RegistryItemImplWithDynamicDefaultValue(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions options, DefaultValueGetter defaultValueGetter)
			: this(name, new MultilingualString[] { category }, caption, hint, dataType, null, storage, options, defaultValueGetter)
		{
		}

		public RegistryItemImplWithDynamicDefaultValue(string name, MultilingualString[] categories, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage, DefaultValueGetter defaultValueGetter)
			: this(name, categories, caption, hint, dataType, null, storage, RegistryOptions.Default, defaultValueGetter)
		{
		}

		public RegistryItemImplWithDynamicDefaultValue(string name, MultilingualString[] categories, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, IRegistryEditorInfo editorInfo, RegistryStorageFlags storage, DefaultValueGetter defaultValueGetter)
			: this(name, categories, caption, hint, dataType, editorInfo, storage, RegistryOptions.Default, defaultValueGetter)
		{
		}

		public RegistryItemImplWithDynamicDefaultValue(string name, MultilingualString[] categories, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, IRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, DefaultValueGetter defaultValueGetter)
			: base(name, caption, hint, dataType, editorInfo, storage, options, defaultValueGetter, true, categories)
		{
			this.defaultValueGetter = defaultValueGetter;
		}

		protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return defaultValueGetter.Invoke(companyPK, branchPK, departmentPK);
		}

		readonly DefaultValueGetter defaultValueGetter;
		public delegate object DefaultValueGetter(Guid companyPK, Guid branchPK, Guid departmentPK);
	}
}
