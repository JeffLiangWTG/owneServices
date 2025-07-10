using Enterprise.CustomerService.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class LegacyModuleMappingsRegistryItem : StronglyTypedRegistryItem<LegacyModuleMappingCollection, LegacyModuleMappingCollection>
	{
		public LegacyModuleMappingsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, LegacyModuleMappingsRegistryEditorInfo editorInfo, RegistryStorageFlags storage, ModuleListType moduleType)
			: this(name, category, caption, hint, editorInfo, storage, new LegacyModuleMappingCollection(moduleType))
		{
		}

		public LegacyModuleMappingsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, LegacyModuleMappingsRegistryEditorInfo editorInfo, RegistryStorageFlags storage, LegacyModuleMappingCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new LegacyModuleMappingsRegistryDataType(defaultValue.LegacyModuleType), editorInfo, storage, RegistryOptions.Default, defaultValue))
		{
		}
	}

	public class LegacyModuleMappingsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<LegacyModuleMappingCollection>
	{
		public LegacyModuleMappingsRegistryDataType(ModuleListType legacyModuleType)
		{
			this.legacyModuleType = legacyModuleType;
		}

		readonly ModuleListType legacyModuleType;

		protected override LegacyModuleMappingCollection DeserialiseCore(byte[] value)
		{
			var result = base.DeserialiseCore(value);
			foreach (LegacyModuleMapping mapping in result)
			{
				mapping.LegacyModuleType = legacyModuleType;
			}

			return result;
		}
	}
}

