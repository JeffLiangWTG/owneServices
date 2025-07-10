using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class DecimalRegistryItem : StronglyTypedRegistryItem<decimal>
	{
		public DecimalRegistryItem(IRegistryItem inner) : base(inner)
		{
		}

		public DecimalRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, category, caption, hint, storage, 0m)
		{
		}

		public DecimalRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: this(name, category, caption, hint, storage, options, 0m)
		{
		}

		public DecimalRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, decimal defaultValue)
			: this(name, category, caption, hint, storage, RegistryOptions.Default, defaultValue)
		{
		}

		public DecimalRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, decimal defaultValue)
			: this(name, category, caption, hint, null, storage, options, defaultValue, double.MinValue, double.MaxValue)
		{
		}

		public DecimalRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, NumericRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, decimal defaultValue, double lowerBound, double upperBound)
			: base(new RegistryItemImpl(name, category, caption, hint, new DecimalRegistryDataType(lowerBound, upperBound), editorInfo, storage, options, defaultValue))
		{
		}
	}
}
