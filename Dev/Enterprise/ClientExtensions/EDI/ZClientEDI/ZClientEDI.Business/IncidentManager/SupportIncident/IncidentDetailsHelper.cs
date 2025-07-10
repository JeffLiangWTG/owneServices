using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.CustomerService.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public static class IncidentDetailsHelper
	{
		public static ZString FindProductArea(IIncidentDetailsSource parent)
		{
			return FindProductArea(parent.Product, parent.Criticality, parent.Module, parent.SourceModuleId);
		}

		public static ZString FindProductArea(ZString product, ZString criticality, ZString module, ZString sourceModuleId)
		{
			var moduleMapping = GetProductAreaModuleMapping(product, module, criticality);
			if (moduleMapping != null)
			{
				return moduleMapping.GetProductAreaWithSourceModule(sourceModuleId);
			}

			return product == ProductTypes.Codes.Enterprise
				? FindProductAreaByLegacyModule(criticality, module, sourceModuleId)
				: ZString.Empty;
		}

		public static ProductAreaModuleMapping GetProductAreaModuleMapping(ZString product, ZString module, ZString criticality)
		{
			var moduleListType = IncidentApprovalLookups.GetModuleListType(criticality);
			switch (moduleListType)
			{
				case ModuleListType.MenuSection:
					return EDIDataRegistry.Instance.SystemProductMappings.Value.GetMapping(product, module);

				case ModuleListType.Cr8:
					return EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.Value.GetMapping(product, module);

				case ModuleListType.Cr9:
					return EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.Value.GetMapping(product, module);

				default:
					return
						EDIDataRegistry.Instance.SystemProductMappings.Value.GetMapping(product, module) ??
						EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.Value.GetMapping(product, module) ??
						EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.Value.GetMapping(product, module);
			}
		}

		static ZString FindProductAreaByLegacyModule(ZString criticality, ZString module, ZString sourceModuleId)
		{
			var legacyMapping = GetLegacyModuleMapping(module, criticality);
			if (legacyMapping != null)
			{
				var mappedModule = legacyMapping.ModuleMapping;
				var mappedCriticality = !string.IsNullOrEmpty(legacyMapping.CriticalityMapping) ? legacyMapping.CriticalityMapping : criticality;

				var moduleMapping = GetProductAreaModuleMapping(ProductTypes.Codes.Enterprise, mappedModule, mappedCriticality);
				if (moduleMapping != null)
				{
					return moduleMapping.GetProductAreaWithSourceModule(sourceModuleId);
				}
			}

			return ZString.Empty;
		}

		static LegacyModuleMapping GetLegacyModuleMapping(ZString module, ZString criticality)
		{
			var moduleListType = IncidentApprovalLookups.GetModuleListType(criticality);
			switch (moduleListType)
			{
				case ModuleListType.MenuSection:
					return EDIDataRegistry.Instance.LegacyMenuSectionMappings.Value.GetMapping(module);

				case ModuleListType.Cr8:
					return EDIDataRegistry.Instance.LegacyCr8ModuleMappings.Value.GetMapping(module);

				case ModuleListType.Cr9:
					return EDIDataRegistry.Instance.LegacyCr9ModuleMappings.Value.GetMapping(module);

				default:
					return
						EDIDataRegistry.Instance.LegacyMenuSectionMappings.Value.GetMapping(module) ??
						EDIDataRegistry.Instance.LegacyCr8ModuleMappings.Value.GetMapping(module) ??
						EDIDataRegistry.Instance.LegacyCr9ModuleMappings.Value.GetMapping(module);
			}
		}
	}
}
