using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	public class DefaultModuleToProductAreaMappingHelperTest : BusinessObjectLookupsTestCase
	{
		public void TestAllModulesAreMapped()
		{
			var moduleList = new SupportIncidentLookups(Factory).GetModuleList(ProductTypes.Codes.Enterprise);

			var fixedEnterpriseDefaults = DefaultModuleToProductAreaMappingHelper.GetEnterpriseDefaultsModule();
			var fixedCr8Defaults = DefaultModuleToProductAreaMappingHelper.GetCr8DefaultsModule();
			var fixedCr9Defaults = DefaultModuleToProductAreaMappingHelper.GetCr9DefaultsModule();

			var missingModules = new List<string>();
			foreach (CodeDescriptionPair item in moduleList)
			{
				if (
					(!fixedEnterpriseDefaults.ContainsKey(item.Code) || string.IsNullOrEmpty(fixedEnterpriseDefaults[item.Code])) &&
					(!fixedCr8Defaults.ContainsKey(item.Code) || string.IsNullOrEmpty(fixedCr8Defaults[item.Code])) &&
					(!fixedCr9Defaults.ContainsKey(item.Code) || string.IsNullOrEmpty(fixedCr9Defaults[item.Code]))
					)
				{
					missingModules.Add($"{item.Code} ({item.Description})");
				}
			}

			AssertEquals($"The modules below are missing from the default list or don't have a product area set in the default list. Please, add the missing Modules and Product Areas to DefaultModuleToProductAreaMappingHelper:\n{string.Join("\n", missingModules)}", 0, missingModules.Count);
		}
	}
}
