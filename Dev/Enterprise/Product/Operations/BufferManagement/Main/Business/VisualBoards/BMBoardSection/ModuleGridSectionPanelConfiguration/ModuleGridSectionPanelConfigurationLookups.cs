using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Business
{
	public class ModuleGridSectionPanelConfigurationLookups : ZLookups
	{
		public ModuleGridSectionPanelConfigurationLookups(ModuleGridSectionPanelConfiguration parent)
			: base(parent)
		{
		}

		new ModuleGridSectionPanelConfiguration Parent
		{
			get { return (ModuleGridSectionPanelConfiguration)base.Parent; }
		}

		public StmModuleFilterCollection ModuleFilters
		{
			get
			{
				var collection = new StmModuleFilterCollection(Factory);

				if (!Parent.ModuleName.Equals(string.Empty))
				{
					collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Module", "Property", Parent.ModuleName, false));
				}
				return collection;
			}
		}

		public CodeDescriptionPairList AllModules
		{
			get { return Factory.GetCachedValue("b25f1669-8fb3-4976-b707-ddb7b3554ed0", GetAllModules); }
		}

		static CodeDescriptionPairList GetAllModules()
		{
			var list = new CodeDescriptionPairList();
			var set = new HashSet<ModuleIdentifier>(ReportModules.GetReportModules());
			foreach (var module in ModuleIDs.AllIncludingClientModules.OrderBy(m => m.Name))
			{
				if (!set.Contains(module))
				{
					list.AddPair(module.Name, module.Description);
				}
			}
			return list;
		}
	}
}
