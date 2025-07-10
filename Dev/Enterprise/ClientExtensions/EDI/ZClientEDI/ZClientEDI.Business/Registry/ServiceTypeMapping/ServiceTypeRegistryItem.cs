using System;
using System.Linq;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class ServiceTypeRegistryItem : StronglyTypedRegistryItem<SystemProductCollection, SystemProductCollection>
	{
		public ServiceTypeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, ServiceTypeRegistryEditorInfo editorInfo, RegistryStorageFlags storage)
			: this(name, category, caption, hint, editorInfo, storage, new SystemProductCollection(editorInfo.ModuleListType))
		{
		}

		public ServiceTypeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, ServiceTypeRegistryEditorInfo editorInfo, RegistryStorageFlags storage, SystemProductCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ServiceTypeRegistryDataType(defaultValue), editorInfo, storage, RegistryOptions.Default, defaultValue))
		{
			if (defaultValue != null && defaultValue.ProductCriticality == CustomerService.Business.ModuleListType.Unspecified)
			{
				defaultValue.ProductCriticality = editorInfo.ModuleListType;
			}
		}
	}

	public class ServiceTypeRegistryDataType : NonPersistentBusinessObjectRegistryDataType<SystemProductCollection>
	{
		public ServiceTypeRegistryDataType(SystemProductCollection defaultCollection)
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
			foreach (var item in result)
			{
				if (defaultCollection.ProductCriticality != CustomerService.Business.ModuleListType.Unspecified && defaultCollection.ProductCriticality != item.ProductCriticality)
				{
					item.ProductCriticality = defaultCollection.ProductCriticality;
				}

				var matchingMandatoryProduct = defaults.FirstOrDefault(defaultProduct => string.Equals(defaultProduct.Code, item.Code, StringComparison.OrdinalIgnoreCase));
				if (matchingMandatoryProduct != null)
				{
					item.IsProductReadOnly = matchingMandatoryProduct.IsProductReadOnly;
					item.Description = matchingMandatoryProduct.Description;

					var defaultModules = matchingMandatoryProduct.ServiceTypeModuleMappings.Cast<ServiceTypeProductAreaModuleMapping>().Where(module => module.IsModuleReadOnly);
					var modulesToAdd = defaultModules.ToDictionary(module => module.ModuleCode);

					foreach (var moduleMapping in item.ServiceTypeModuleMappings)
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
						item.ServiceTypeModuleMappings.AddNew(module.Value.ModuleCode, module.Value.ModuleDescription, module.Value.ProductArea, true);
					}
				}

				item.ServiceTypeModuleMappings.ProductCode = item.Code;
				item.ServiceTypeModuleMappings.ProductCriticality = item.ProductCriticality;
				foreach (var mapping in item.ServiceTypeModuleMappings)
				{
					mapping.ProductCode = item.Code;
					mapping.ProductCriticality = item.ProductCriticality;
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
