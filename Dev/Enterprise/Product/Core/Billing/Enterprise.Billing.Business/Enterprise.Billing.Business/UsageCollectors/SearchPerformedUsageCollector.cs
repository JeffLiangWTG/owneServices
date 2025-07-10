using System.Text.RegularExpressions;
using CargoWise.Application;
using Enterprise.Billing.Integration;
using Enterprise.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Billing.Business;

public class SearchPerformedUsageCollector : ISearchPerformedUsageCollector
{
	public void Report(string searchType, string moduleId, string filters)
	{
		if (ObjectFactory.Get<IGlowRegistry>().IndexSearchUsageCollector)
		{
			UsageCollector.Report(UsageFeatures.Codes.SearchPerformed, new (string, object)[]
			{
				(UsageProperties.SearchType, searchType),
				(UsageProperties.ModuleID, moduleId),
				(UsageProperties.Filters, searchType == SearchPerformedType.PaveSql ? CleanSqlString(filters) : filters)
			});
		}
	}

	string CleanSqlString(string query)
	{
		return Regex.Replace(query.Replace("\r", " ").Replace("\n", " ").Replace("\t", " "), @"\s+", " ").Trim();
	}
}
