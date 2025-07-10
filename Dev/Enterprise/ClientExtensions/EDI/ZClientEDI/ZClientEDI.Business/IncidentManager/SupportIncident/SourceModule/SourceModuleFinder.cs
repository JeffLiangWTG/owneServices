using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.CustomerService.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class SourceModuleFinder : NonPersistentBusinessObject, IObsoleteValidation
	{
		public SourceModuleFinder(ZString productCode, ModuleListType moduleType, ZString findReason, BusinessObjectFactory factory)
			: base(factory)
		{
			this.ProductCode = productCode;
			this.ModuleType = moduleType;
			this.findReason = findReason;
		}

		public readonly ZString ProductCode;
		public readonly ModuleListType ModuleType;

		#region Properties

		#region Find Reason

		public ZString FindReason
		{
			get { return findReason; }
		}
		readonly ZString findReason;

		#endregion

		#region ModuleFilter

		[List("Lookups.ModuleFilterList")]
		[MaxLength(3)]
		public ZString ModuleFilter
		{
			get { return moduleFilter; }
			set
			{
				if (moduleFilter != value)
				{
					SetNonPersistentPropertyValue(ModuleFilterInfo, ref moduleFilter, value);
					RefreshSourceModules();
				}
			}
		}
		ZString moduleFilter;

		public ZPropertyInfo ModuleFilterInfo
		{
			get { return GetZPropertyInfo(nameof(ModuleFilter)); }
		}

		#endregion

		#region ProductAreaFilter

		[List("Lookups.ProductAreaList")]
		[MaxLength(3)]
		public ZString ProductAreaFilter
		{
			get { return productAreaFilter; }
			set
			{
				if (productAreaFilter != value)
				{
					SetNonPersistentPropertyValue(ProductAreaFilterInfo, ref productAreaFilter, value);
					RefreshSourceModules();
				}
			}
		}
		ZString productAreaFilter;

		public ZPropertyInfo ProductAreaFilterInfo
		{
			get { return GetZPropertyInfo(nameof(ProductAreaFilter)); }
		}

		#endregion

		#endregion

		#region SourceModules

		public ModuleMappingWithSourceModuleCollection SourceModules
		{
			get
			{
				if (sourceModules == null)
				{
					sourceModules = new ModuleMappingWithSourceModuleCollection();
					RefreshSourceModules();
				}

				return sourceModules;
			}
		}
		ModuleMappingWithSourceModuleCollection sourceModules;

		public void RefreshSourceModules(string descriptionFilter = "")
		{
			using (SourceModules.SuspendListChanged())
			{
				SourceModules.RemoveAll();

				foreach (var pair in AllModuleMappingWithSourceModules)
				{
					if (
						(ModuleFilter.IsEmpty || pair.ModuleCode.EqualsIgnoringCase(ModuleFilter))
						&& (ProductAreaFilter.IsEmpty || pair.ProductArea.EqualsIgnoringCase(ProductAreaFilter))
						&& (string.IsNullOrEmpty(descriptionFilter) || pair.SourceModuleDescription.Contains(descriptionFilter, StringComparison.OrdinalIgnoreCase)))
					{
						SourceModules.Add(pair);
					}
				}
			}
			SourceModules.RefreshBinding();
		}

		IList<ModuleMappingWithSourceModule> AllModuleMappingWithSourceModules
		{
			get
			{
				if (allModuleMappingWithSourceModules == null)
				{
					allModuleMappingWithSourceModules = new List<ModuleMappingWithSourceModule>();

					var productRegistryItem = EDIDataRegistry.GetProductAreaModuleMappingsRegistryItem(ModuleType);
					if (productRegistryItem != null)
					{
						var product = productRegistryItem.Value.GetProductByCode(ProductCode);
						if (product != null)
						{
							var productAreaModuleMappings = product.ModuleMappings;
							var registrySourceModules = EDIDataRegistry.Instance.SourceModules.Value;

							foreach (SourceModule sourceModule in registrySourceModules)
							{
								if (sourceModule.IsSelectableForOverride && SourceModule.IsModuleTypeCompatible(ModuleType, sourceModule.ModuleListType))
								{
									var defaultModule = sourceModule.DefaultModule;
									if (!defaultModule.IsEmpty)
									{
										var defaultModuleMapping = productAreaModuleMappings.GetMapping(defaultModule);
										if (defaultModuleMapping != null)
										{
											allModuleMappingWithSourceModules.Add(new ModuleMappingWithSourceModule(defaultModuleMapping, sourceModule));
										}
									}
								}
							}

							foreach (ProductAreaModuleMapping moduleMapping in product.ModuleMappings)
							{
								foreach (ProductAreaSourceModuleMapping overriddenSourceModuleMapping in moduleMapping.SourceModuleMappings)
								{
									var sourceModule = registrySourceModules.GetSourceModule(overriddenSourceModuleMapping.Code, ModuleType, ProductCode);
									if (sourceModule != null && sourceModule.IsSelectableForOverride && sourceModule.DefaultModule != moduleMapping.ModuleCode)
									{
										allModuleMappingWithSourceModules.Add(new ModuleMappingWithSourceModule(moduleMapping, sourceModule));
									}
								}
							}
						}
					}
				}

				return allModuleMappingWithSourceModules;
			}
		}
		IList<ModuleMappingWithSourceModule> allModuleMappingWithSourceModules;

		#endregion

		#region Lookups

		public SourceModuleFinderLookups Lookups
		{
			get { return lookups ?? (lookups = new SourceModuleFinderLookups(this)); }
		}
		SourceModuleFinderLookups lookups;

		#endregion
	}
}
