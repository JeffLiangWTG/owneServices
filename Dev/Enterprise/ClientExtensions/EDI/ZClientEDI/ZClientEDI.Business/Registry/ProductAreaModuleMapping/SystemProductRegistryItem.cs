using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class SystemProductRegistryItem : TranslatableRegistryItem<SystemProductCollection, SystemProductCollection>
	{
		public SystemProductRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, SystemProductRegistryEditorInfo editorInfo, RegistryStorageFlags storage)
			: this(name, category, caption, hint, editorInfo, storage, new SystemProductCollection(editorInfo.ModuleListType))
		{
		}

		public SystemProductRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, SystemProductRegistryEditorInfo editorInfo, RegistryStorageFlags storage, SystemProductCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new SystemProductRegistryDataType(defaultValue), editorInfo, storage, RegistryOptions.Default, defaultValue))
		{
			if (defaultValue != null && defaultValue.ProductCriticality == CustomerService.Business.ModuleListType.Unspecified)
			{
				defaultValue.ProductCriticality = editorInfo.ModuleListType;
			}
		}

		public override bool IsTranslatable => true;

		public override IEnumerable<ResourceString> DefaultStrings => Array.Empty<ResourceString>();

		public override int MaxLength => ProductAreaModuleMapping.Schema.ModuleDescriptionMaxLength;

		public override IEnumerable<string> GetCaptions(SystemProductCollection value)
		{
			foreach (var systemProduct in EDIDataRegistry.Instance.SystemProductMappings.Value)
			{
				foreach (var module in systemProduct.ModuleMappings)
				{
					yield return module.ModuleDescription;
				}
			}
		}

		protected override SystemProductCollection Convert(SystemProductCollection value)
		{
			foreach (var item in value)
			{
				foreach (var module in item.ModuleMappings)
				{
					using (module.GetValidationSuspender())
					{
						module.ModuleDescriptionMultilingual = GetMultilingualString(module.ModuleDescription);
					}
				}
			}

			return value;
		}
	}

	public class SystemProductRegistryDataType : NonPersistentBusinessObjectRegistryDataType<SystemProductCollection>
	{
		public SystemProductRegistryDataType(SystemProductCollection defaultCollection)
			: base()
		{
			this.defaultCollection = defaultCollection;
		}

		readonly SystemProductCollection defaultCollection;

		protected override SystemProductCollection DeserialiseCore(byte[] value)
		{
			var defaults = defaultCollection.Cast<SystemProduct>().Where(product => product.IsProductReadOnly);
			var itemsToAdd = defaults.ToDictionary(mapping => mapping.Code);

			SystemProductCollection result = base.DeserialiseCore(value);
			foreach (SystemProduct item in result)
			{
				var matchingMandatoryProduct = defaults.FirstOrDefault(defaultProduct => string.Equals(defaultProduct.Code, item.Code, StringComparison.OrdinalIgnoreCase));
				if (matchingMandatoryProduct != null)
				{
					item.IsProductReadOnly = matchingMandatoryProduct.IsProductReadOnly;
					item.Description = matchingMandatoryProduct.Description;

					var defaultModules = matchingMandatoryProduct.ModuleMappings.Cast<ProductAreaModuleMapping>().Where(module => module.IsModuleReadOnly);
					var modulesToAdd = defaultModules.ToDictionary(module => module.ModuleCode);

					foreach (ProductAreaModuleMapping moduleMapping in item.ModuleMappings)
					{
						var mandatoryModule = defaultModules.FirstOrDefault(defaultModule => string.Equals(defaultModule.ModuleCode, moduleMapping.ModuleCode));

						if (mandatoryModule != null)
						{
							moduleMapping.IsModuleReadOnly = mandatoryModule.IsModuleReadOnly;
							moduleMapping.ModuleDescription = mandatoryModule.ModuleDescription;
						}

						modulesToAdd.Remove(moduleMapping.ModuleCode);
					}

					foreach (var module in modulesToAdd)
					{
						item.ModuleMappings.AddNew(module.Value.ModuleCode, module.Value.ModuleDescription, module.Value.ProductArea, true);
					}
				}

				itemsToAdd.Remove(item.Code);
			}

			foreach (var item in itemsToAdd.Values)
			{
				result.AddNew(item.Code, item.Description, true);
			}

			return result;
		}
	}
}
