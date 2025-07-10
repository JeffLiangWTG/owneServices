using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using Enterprise.Core.Modules;
using Enterprise.Integration.Modules;

namespace Enterprise.ZArchitecture.Modules
{
	public static class ModuleTreeLoaderHelpers
	{
		public static void AddClientSpecificModule(ModuleSection section, ModuleIdentifier moduleID)
		{
			MainFormModule module = new MainFormModule(moduleID);
			if (!section.Modules.ContainsKey(module.ID))
			{
				section.Modules.Add(module);
			}
		}

		public static void AddModule(ModuleSection section, ModuleIdentifier moduleID, Action<MainFormModule> moduleSetup = null)
		{
			AddModuleIf(true, section, moduleID, moduleSetup);
		}

		public static void AddModuleIf(bool condition, ModuleSection section, ModuleIdentifier moduleID, Action<MainFormModule> moduleSetup = null)
		{
			var currentCountry = CurrentCountry;
			foreach (var country in GetCountriesForModule(moduleID, currentCountry))
			{
				section.Modules.AddIf(condition && currentCountry == country, () => GetModule(moduleID, country, moduleSetup));
			}
		}

		public static void AddModulesIf(bool condition, ModuleSection section, params ModuleIdentifier[] moduleIDs)
		{
			foreach (var moduleId in moduleIDs)
			{
				AddModuleIf(condition, section, moduleId, null);
			}
		}

		public static void InsertModuleBefore(ModuleIdentifier moduleIdToInsertBefore, ModuleSection section, ModuleIdentifier moduleID, Action<MainFormModule> moduleSetup = null)
		{
			InsertModuleBeforeIf(moduleIdToInsertBefore, true, section, moduleID, moduleSetup);
		}

		public static void InsertModuleBeforeIf(ModuleIdentifier moduleIdToInsertBefore, bool condition, ModuleSection section, ModuleIdentifier moduleID, Action<MainFormModule> moduleSetup = null)
		{
			var currentCountry = CurrentCountry;
			foreach (var country in GetCountriesForModule(moduleID, currentCountry))
			{
				section.Modules.InsertBeforeIf(moduleIdToInsertBefore.ToString(), condition && currentCountry == country, () => GetModule(moduleID, country, moduleSetup));
			}
		}

		public static void InsertModulesBeforeIf(ModuleIdentifier moduleIdToInsertBefore, bool condition, ModuleSection section, params ModuleIdentifier[] moduleIDs)
		{
			foreach (var moduleId in moduleIDs)
			{
				InsertModuleBeforeIf(moduleIdToInsertBefore, condition, section, moduleId, null);
			}
		}

		public static void InsertModuleAfter(ModuleIdentifier moduleIdToInsertAfter, ModuleSection section, ModuleIdentifier moduleID, Action<MainFormModule> moduleSetup = null)
		{
			InsertModuleAfterIf(moduleIdToInsertAfter, true, section, moduleID, moduleSetup);
		}

		public static void InsertModuleAfterIf(ModuleIdentifier moduleIdToInsertAfter, bool condition, ModuleSection section, ModuleIdentifier moduleID, Action<MainFormModule> moduleSetup = null)
		{
			var currentCountry = CurrentCountry;
			foreach (var country in GetCountriesForModule(moduleID, currentCountry))
			{
				section.Modules.InsertAfterIf(moduleIdToInsertAfter.ToString(), condition && currentCountry == country, () => GetModule(moduleID, country, moduleSetup));
			}
		}

		public static void InsertModulesAfterIf(ModuleIdentifier moduleIdToInsertAfter, bool condition, ModuleSection section, params ModuleIdentifier[] moduleIDs)
		{
			foreach (var moduleId in moduleIDs)
			{
				InsertModuleAfterIf(moduleIdToInsertAfter, condition, section, moduleId, null);
			}
		}

		public static MainFormModule GetModule(ModuleIdentifier moduleID, string country, Action<MainFormModule> moduleSetup)
		{
			var moduleInfo = ObjectFactory.Get<IModuleFactory>().GetRegisteredModuleInfo(moduleID, country, true);
			if (moduleInfo != null)
			{
				var module = new MainFormModule(moduleInfo);
				if (moduleSetup != null)
				{
					moduleSetup(module);
				}
				return module;
			}
			return null;
		}

		public static string CurrentCountry
		{
			get { return ObjectFactory.Get<IModuleEnvironment>().CurrentCompanyCountryCode; }
		}

		static IEnumerable<string> GetCountriesForModule(ModuleIdentifier moduleID, string currentCountry)
		{
			var countryOverridesForModule = ObjectFactory.Get<IModuleFactory>().GetCountryOverridesRegisteredForModule(moduleID).Where(country => !string.IsNullOrEmpty(country));
			var hasCurrentCountry = false;
			foreach (var countryOverride in countryOverridesForModule)
			{
				if (countryOverride == currentCountry)
				{
					hasCurrentCountry = true;
				}

				yield return countryOverride;
			}

			if (!hasCurrentCountry)
			{
				yield return currentCountry;
			}
		}
	}
}
