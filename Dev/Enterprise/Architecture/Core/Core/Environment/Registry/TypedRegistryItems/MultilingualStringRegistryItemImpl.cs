using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class MultilingualStringRegistryItemImpl : RegistryItemImpl
	{
		public MultilingualStringRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage)
			: base(name, category, caption, hint, dataType, storage)
		{
		}

		public MultilingualStringRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, IRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, object defaultValue)
			: this(name, category, caption, hint, dataType, editorInfo, storage, options, defaultValue, false)
		{
		}

		public MultilingualStringRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, IRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, object defaultValue, bool useDefaultDefaultValue)
			: base(name, caption, hint, dataType, editorInfo, storage, options, defaultValue, useDefaultDefaultValue, category)
		{
		}

		protected override bool CheckValueDataTypeCore(object value)
		{
			return DataType.DataType.IsInstanceOfType(value) || value is MultilingualString;
		}
	}
}
