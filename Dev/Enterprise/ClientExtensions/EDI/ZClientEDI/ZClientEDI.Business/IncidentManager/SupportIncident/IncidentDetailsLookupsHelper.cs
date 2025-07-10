using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.CustomerService.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public static class IncidentDetailsLookupsHelper
	{
		#region Product

		public static CodeDescriptionPairList ProductList
		{
			get
			{
				var productTypes = (CodeDescriptionPairList)new ProductTypes();
				productTypes.Sort();
				return productTypes;
			}
		}

		#endregion

		#region Criticality

		public static CodeDescriptionPairList CriticalityList
		{
			get { return new IncidentApprovalLookups(null).CriticalityList; }
		}

		#endregion

		#region Module

		public static CodeDescriptionPairList GetModuleListEnabledModulesOnly(IIncidentDetailsSource parent)
		{
			var moduleListEnabledModulesOnly = new CodeDescriptionPairList();
			if (!parent.Product.IsEmpty)
			{
				AddModulesProductNotEmpty(moduleListEnabledModulesOnly, parent);
			}
			else
			{
				AddModulesProductEmpty(moduleListEnabledModulesOnly, parent);
			}
			return moduleListEnabledModulesOnly;
		}

		static void AddModulesProductNotEmpty(CodeDescriptionPairList moduleListEnabledModulesOnly, IIncidentDetailsSource parent)
		{
			foreach (CodeDescriptionPair moduleList in GetModuleList(parent.Factory, parent.ModuleType, parent.Product, parent.ProductArea))
			{
				var moduleMapping = GetProductAreaModuleMapping(parent.Product, moduleList.Code, parent.Criticality);
				if (moduleMapping?.IsEnabled ?? false)
				{
					moduleListEnabledModulesOnly.Add(moduleList);
				}
			}
		}

		static void AddModulesProductEmpty(CodeDescriptionPairList moduleListEnabledModulesOnly, IIncidentDetailsSource parent)
		{
			foreach (CodeDescriptionPair moduleList in GetModuleList(parent.Factory, parent.ModuleType, parent.Product, parent.ProductArea))
			{
				var moduleMapping = GetProductAreaModuleMapping(moduleList.Code);
				if (moduleMapping?.IsEnabled ?? false)
				{
					moduleListEnabledModulesOnly.Add(moduleList);
				}
			}
		}

		static ProductAreaModuleMapping GetProductAreaModuleMapping(ZString module)
		{
			return
				EDIDataRegistry.Instance.SystemProductMappings.Value.GetMapping(module) ??
				EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.Value.GetMapping(module) ??
				EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.Value.GetMapping(module);
		}

		static ProductAreaModuleMapping GetProductAreaModuleMapping(ZString product, ZString module, ZString criticality)
		{
			return criticality == Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement
				? EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.Value.GetMapping(product, module)
				: criticality == Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest
					? EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.Value.GetMapping(product, module)
					: EDIDataRegistry.Instance.SystemProductMappings.Value.GetMapping(product, module)
						?? EDIDataRegistry.Instance.ProductAreaIncidentCr8Mappings.Value.GetMapping(product, module)
						?? EDIDataRegistry.Instance.ProductAreaIncidentCr9Mappings.Value.GetMapping(product, module);
		}

		#endregion

		#region Module List

		public static CodeDescriptionPairList GetModuleList(BusinessObjectFactory factory)
		{
			return GetModuleList(factory, ZString.Empty);
		}

		public static CodeDescriptionPairList GetModuleList(BusinessObjectFactory factory, ZString product)
		{
			return GetModuleList(factory, ModuleListType.Unspecified, product, ZString.Empty);
		}

		public static CodeDescriptionPairList GetModuleList(BusinessObjectFactory factory, ModuleListType moduleListType, ZString product, ZString productArea)
		{
			return factory.GetCachedValue("ModuleList:" + moduleListType.ToString() + product.PadRight(3) + productArea.PadRight(3),
				delegate
				{
					return new SupportIncidentModuleListBuilder().Build(moduleListType, product, productArea);
				});
		}

		#endregion

		#region ServiceType

		public static CodeDescriptionPairList GetServiceTypeList(IIncidentDetailsSource parent)
		{
			var result = new CodeDescriptionPairList();

			SystemProductCollection registryProductCollection = null;
			if (parent.Criticality == Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR8_ComplianceRequirement)
			{
				registryProductCollection = EDIDataRegistry.Instance.ServiceTypeCr8Mappings.Value;
			}
			else if (parent.Criticality == Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR9_CustomerServiceRequest)
			{
				registryProductCollection = EDIDataRegistry.Instance.ServiceTypeCr9Mappings.Value;
			}
			else
			{
				registryProductCollection = EDIDataRegistry.Instance.ServiceTypeMappings.Value;
			}

			var product = registryProductCollection.Cast<SystemProduct>().FirstOrDefault(p => p.Code.EqualsIgnoringCase(parent.Product));
			if (product != null)
			{
				var module = product.ServiceTypeModuleMappings.Cast<ServiceTypeProductAreaModuleMapping>().FirstOrDefault(p => p.ProductArea.EqualsIgnoringCase(parent.ProductArea) &&
																											p.ModuleCode.EqualsIgnoringCase(parent.Module));

				if (module != null)
				{
					foreach (var item in module.ServiceTypeMappings.Cast<ServiceTypeModuleMapping>())
					{
						result.Add(new CodeDescriptionPair((string)item.Code, (string)item.Description));
					}
				}
			}

			return result;
		}

		#endregion
	}
}
