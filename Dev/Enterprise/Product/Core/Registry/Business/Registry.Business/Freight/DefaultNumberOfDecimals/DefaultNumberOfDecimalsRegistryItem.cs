using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class DefaultNumberOfDecimalsRegistryItem : StronglyTypedRegistryItem<DefaultNumberOfDecimalsCollection>
	{
		public DefaultNumberOfDecimalsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, DefaultNumberOfDecimalsCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new DefaultNumberOfDecimalsRegistryDataType(defaultValue.Module), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.DefaultNumberOfDecimalsRegistryItemEditor, Enterprise.Registry.GUI")]
	public class DefaultNumberOfDecimalsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DefaultNumberOfDecimalsCollection>
	{
		public DefaultNumberOfDecimalsRegistryDataType()
			: this(Module.Freight)
		{
		}

		public DefaultNumberOfDecimalsRegistryDataType(Module module)
		{
			this.Module = module;
		}

		public Module Module { get; internal set; }

		protected override DefaultNumberOfDecimalsCollection DeserialiseCore(byte[] value)
		{
			var defaultNumberOfDecimals = base.DeserialiseCore(value);
			defaultNumberOfDecimals.Module = Module;

			return defaultNumberOfDecimals;
		}
	}
}
