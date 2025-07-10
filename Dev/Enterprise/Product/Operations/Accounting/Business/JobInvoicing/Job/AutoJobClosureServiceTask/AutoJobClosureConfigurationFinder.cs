using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public static class AutoJobClosureConfigurationFinder
	{
		public static JobClosureConfiguration FindBestMatchingJobClosureConfiguration(ZGuid companyPK, string jobType, string direction, string mode, ZGuid departmentPK, ZString configurationType)
		{
			var ranker = new StringColumnValueRanker();
			ranker.Add(JobClosureConfiguration.Schema.JobType, new IZType[] { (ZString)jobType, (ZString)JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All });
			ranker.Add(JobClosureConfiguration.Schema.DirectionCode, new IZType[] { (ZString)direction, (ZString)FreightShipmentDirection.Code.All, ZString.Empty });
			ranker.Add(JobClosureConfiguration.Schema.Mode, new IZType[] { (ZString)mode, (ZString)JobConfigurationSelectorLookups.ModeAdditionalCodes.All, ZString.Empty });
			ranker.Add(JobClosureConfiguration.Schema.DepartmentPK, new IZType[] { departmentPK, ZGuid.Empty });
			ranker.Add(JobClosureConfiguration.Schema.ConfigurationType, new IZType[] { configurationType });

			var regValue = AccountingConfigurationRegistry.Instance.JobClosureConfigurationSetup.GetFallBackValueAtAllLevels(companyPK.ToGuid(), Guid.Empty, Guid.Empty);
			var configurations = regValue?.ConfigurationCollection.OfType<JobClosureConfiguration>();
			var matchedConfigurations = (ranker.GetBestMatch(configurations) ?? Enumerable.Empty<JobClosureConfiguration>()).ToArray();

			return matchedConfigurations?.FirstOrDefault();
		}

		public static IEnumerable<ZGuid> GetCompanyPKsThatHaveAutoJoClosureConfiguration()
		{
			var companyPKs = new List<ZGuid>();
			var factory = new BusinessObjectFactory();
			var companies = AccountingUtils.GetAllActiveCompanies(factory);
			foreach (var company in companies)
			{
				var regValue = AccountingConfigurationRegistry.Instance.JobClosureConfigurationSetup.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
				if (regValue?.ConfigurationCollection?.Count > 0)
				{
					companyPKs.Add(company.PK);
				}
			}
			return companyPKs;
		}
	}
}
