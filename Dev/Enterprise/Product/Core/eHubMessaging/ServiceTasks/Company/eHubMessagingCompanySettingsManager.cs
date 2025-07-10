using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	class eHubMessagingCompanySettingsManager : ICompanySettingsManager
	{
		public GlbCompany[] Companies
		{
			get { return companies ?? (companies = GetCompanies()); }
		}

		public ICompanySettings GetSetting(GlbCompany company)
		{
			if (settings == null)
			{
				settings = InitialiseSettings();
			}

			return settings.Where(s => s.Key == company.PK).Select(s => s.Value).FirstOrDefault();
		}

		#region Implementation

		static GlbCompany[] GetCompanies()
		{
			var filter = new ZQuery(GlbCompanySchema.GC_IsActive, true);
			filter.AddToFilter(GlbCompanySchema.GC_Code, SQLComparisonOperator.NotEqual, "DEM");
			return new GlbCompanyCollection(new BusinessObjectFactory() { RefreshEnabled = false }, filter).ToArray();
		}

		GlbCompany[] companies;

		Dictionary<ZGuid, ICompanySettings> InitialiseSettings()
		{
			return Companies.ToDictionary(company => company.PK, company => CreateCompanySettings(company.PK.ToGuid()));
		}

		protected virtual ICompanySettings CreateCompanySettings(Guid companyPK)
		{
			return new eHubMessagingCompanySettings(companyPK);
		}

		public void ClearCache()
		{
			settings = null;
			companies = null;
		}

		Dictionary<ZGuid, ICompanySettings> settings;

		#endregion
	}
}
