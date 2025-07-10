using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.Business
{
	public static class ServiceTaskEnvironmentChecker
	{
		public static string CheckCNSWClientSetting() => AnyCompanyHasCNSWClientSetting ? string.Empty : GetRegistryHasNotBeenConfiguredMessage(CNCustomsDataRegistry.Instance.CNSWClientSetting);

		static string GetRegistryHasNotBeenConfiguredMessage(IRegistryItem registryItem) => $"The registry setting '{registryItem.GetLocationInEnglish()}' has not been configured.";

		static bool AnyCompanyHasCNSWClientSetting => anyCompanyHasCNSWClientSetting ?? (anyCompanyHasCNSWClientSetting = DoesAnyCompanyHasRegistry(CNCustomsDataRegistry.Instance.CNSWClientSetting)).Value;

		[ThreadStatic]
		static bool? anyCompanyHasCNSWClientSetting;

		static bool DoesAnyCompanyHasRegistry(IRegistryItem registryItem)
		{
			var result = false;
			var factory = new BusinessObjectFactory();
			var companyPKs = GlbCompany.GetActiveCompanies(Core.Constants.CountryCodes.China, factory).Select(x => x.PK).ToArray();
			if (companyPKs.Length > 0)
			{
				var query = new ZQuery(StmDataSchema.SD_Name, registryItem.Name);
				query.AddToFilter(StmDataSchema.SD_Owner, companyPKs);
				query.AddToFilter(StmDataSchema.SD_BinaryValue, SQLComparisonOperator.NotEqual, null);
				result = factory.Exists(typeof(StmData), query);
			}
			return result;
		}

		public static void Reset() => anyCompanyHasCNSWClientSetting = null;
	}
}
