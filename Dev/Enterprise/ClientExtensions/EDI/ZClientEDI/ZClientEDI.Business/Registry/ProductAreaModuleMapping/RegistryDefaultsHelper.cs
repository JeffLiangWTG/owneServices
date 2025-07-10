using System.Collections.Generic;
using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.CustomerService.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.Registry.ProductAreaModuleMapping
{
	public static class RegistryDefaultsHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "This is meant to specify an exact product name.")]
		public static SystemProductCollection GetEnterpriseDefaults()
		{
			var collection = new SystemProductCollection();
			var enterprise = collection.AddNew(ProductTypes.Codes.Enterprise, ProductTypes.Descriptions.EnterpriseCW1, true);
			var defaults = DefaultModuleToProductAreaMappingHelper.GetEnterpriseDefaultsModule();
			var productAreas = EDIDataRegistry.Instance.ProductAreas.Value.GetAllCodes().ToList();

			foreach (var menuSection in new MandatoryCustomerServiceMenuSectionList().Values)
			{
				enterprise.ModuleMappings.AddNew(menuSection.Code, menuSection.Description.GetUnresolvedString(), GetProductArea(defaults, productAreas, menuSection.Code), false);
			}

			foreach (var menuSection in new ModuleTreeCustomerServiceMenuSectionList().Values)
			{
				if (!enterprise.ModuleMappings.ContainsCode(menuSection.Code))
				{
					enterprise.ModuleMappings.AddNew(menuSection.Code, menuSection.Description.GetUnresolvedString(), GetProductArea(defaults, productAreas, menuSection.Code), false);
				}
			}

			foreach (var menuSection in InternalMenuSections)
			{
				if (!enterprise.ModuleMappings.ContainsCode(menuSection.Key))
				{
					enterprise.ModuleMappings.AddNew(menuSection.Key, menuSection.Value, GetProductArea(defaults, productAreas, menuSection.Key), false, true);
				}
			}

			return collection;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "This is meant to specify an exact product name.")]
		public static SystemProductCollection GetCr8Defaults()
		{
			var collection = new SystemProductCollection(ModuleListType.Cr8);
			var enterprise = collection.AddNew(ProductTypes.Codes.Enterprise, ProductTypes.Descriptions.EnterpriseCW1, true);
			var defaults = DefaultModuleToProductAreaMappingHelper.GetCr8DefaultsModule();
			var productAreas = EDIDataRegistry.Instance.ProductAreas.Value.GetAllCodes().ToList();

			foreach (ICodeDescription cr8Module in new IncidentApprovalLookups(null).Cr8ModuleList)
			{
				enterprise.ModuleMappings.AddNew(cr8Module.Code, cr8Module.Description, GetProductArea(defaults, productAreas, cr8Module.Code), false);
			}

			return collection;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "This is meant to specify an exact product name.")]
		public static SystemProductCollection GetCr9Defaults()
		{
			var collection = new SystemProductCollection(ModuleListType.Cr9);
			var enterprise = collection.AddNew(ProductTypes.Codes.Enterprise, ProductTypes.Descriptions.EnterpriseCW1, true);
			var defaults = DefaultModuleToProductAreaMappingHelper.GetCr9DefaultsModule();
			var productAreas = EDIDataRegistry.Instance.ProductAreas.Value.GetAllCodes().ToList();

			foreach (ICodeDescription cr9Module in new IncidentApprovalLookups(null).Cr9ModuleList)
			{
				enterprise.ModuleMappings.AddNew(cr9Module.Code, cr9Module.Description, GetProductArea(defaults, productAreas, cr9Module.Code), false);
			}

			return collection;
		}

		static ZString GetProductArea(Dictionary<string, string> defaults, List<string> productAreas, string module)
		{
			var result = ZString.Empty;
			if (defaults.ContainsKey(module))
			{
				var productArea = defaults[module];
				if (productAreas.Contains(productArea))
				{
					result = productArea;
				}
			}
			return result;
		}

		static Dictionary<string, string> InternalMenuSections
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "TRN", "Internal - ediTrainingManager" },
					{ "INC", "Internal - ediIncidentManager" },
					{ "INT", "Internal Development" },
					{ "JRB", "Job Related Billing" }
				};
			}
		}
	}
}
