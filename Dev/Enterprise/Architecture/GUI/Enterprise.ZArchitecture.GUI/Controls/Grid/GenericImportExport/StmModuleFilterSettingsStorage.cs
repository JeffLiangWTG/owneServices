using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.DataMapping
{
	public class StmModuleFilterSettingsStorage : ISettingsStorage
	{
		public StmModuleFilterSettingsStorage(string contextPrefix, string contextKey)
		{
			this.contextPrefix = contextPrefix;
			this.contextKey = contextKey;
		}

		public IEnumerable<string> GetSavedSettings()
		{
			var result = Array.ConvertAll(GetModuleFilters(), moduleFilter => moduleFilter.S9_FilterNameMultilingual.GetUnresolvedString());
			Array.Sort(result);
			return result;
		}

		public string LoadSettings(string name)
		{
			var moduleFilter = GetModuleFilter(name);
			if (moduleFilter != null)
			{
				return moduleFilter.S9_FilterData.ToUTF8();
			}

			return null;
		}

		public bool HasSecurityRight()
		{
			return Env.Security.SaveDataImportWizardSettings.IsAllowed;
		}

		public void ShowSecurityError()
		{
			Env.Security.SaveDataImportWizardSettings.ShowError();
		}

		public void RemoveSettings(string name)
		{
			var moduleFilter = GetModuleFilter(name);
			if (moduleFilter != null && !moduleFilter.S9_IsSystem)
			{
				moduleFilter.Delete();
				SaveChanges();
			}
		}

		public void SaveSettings(string name, string settings)
		{
			var moduleFilter = GetModuleFilter(name);
			if (moduleFilter == null)
			{
				moduleFilter = factory.New<StmModuleFilter>();
				moduleFilter.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				moduleFilter.S9_ModuleID = contextPrefix + contextKey;
				moduleFilter.S9_FilterName = name;
			}

			if (!moduleFilter.S9_IsSystem)
			{
				moduleFilter.S9_FilterData = Encoding.UTF8.GetBytes(settings);
				SaveChanges();
			}
		}

		void SaveChanges()
		{
			try
			{
				factory.Save();
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleZSaveConcurrencyException(ex, NotificationHandler.Instance);
			}
		}

		StmModuleFilter[] GetModuleFilters()
		{
			if (contextKey == null || contextKey.Length == 0)
			{
				return Array.Empty<StmModuleFilter>();
			}
			else
			{
				return factory.Load<StmModuleFilter>(GetModuleFiltersQuery());
			}
		}

		StmModuleFilter GetModuleFilter(string name)
		{
			var result = GetModuleFiltersQuery();
			result.AddToFilter(StmModuleFilterSchema.S9_FilterName, name);
			return factory.LoadTop1<StmModuleFilter>(result);
		}

		ZQuery GetModuleFiltersQuery()
		{
			var systemQuery = new ZQuery(StmModuleFilterSchema.S9_GC, null);
			systemQuery.AddToFilter(StmModuleFilterSchema.S9_IsSystem, true);

			var companyQuery = new ZQuery(StmModuleFilterSchema.S9_GC, EnvProxy.Instance.CurrentCompany.PK);
			companyQuery.AddToFilter(systemQuery, JoinCondition.Or);

			var result = new ZQuery(StmModuleFilterSchema.S9_ModuleID, contextPrefix + contextKey);
			result.AddToFilter(companyQuery);
			return result;
		}

#if DEBUG
		internal
#endif
		BusinessObjectFactory factory = new BusinessObjectFactory();
		readonly string contextKey;
		readonly string contextPrefix;
	}
}
