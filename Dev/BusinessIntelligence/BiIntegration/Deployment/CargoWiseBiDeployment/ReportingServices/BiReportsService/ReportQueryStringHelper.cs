using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Bi.Registration.PowerBi;

namespace CargoWise.Bi.Deployment.ReportingServices
{
	public static class ReportQueryStringHelper
	{
		#region SuppressResourceStringsCheckRegion
		public static string GetQueryFilterString(PowerBiItem reportItem, string companyCode, string countryCode, string branchCode)
		{
			var filters = reportItem?.GetQueryStringFilters(companyCode, countryCode, branchCode);
			if (filters != null)
			{
				return "&filter=" + Uri.EscapeDataString(
							string.Join(" and ", filters.Select(item => GetFilterItemString(item))
						)
				);
			}
			return string.Empty;
		}

		public static string GetFilterItemString(KeyValuePair<string, string> filterItem)
		{
			var keyString = filterItem.Key.Replace(" ", "_x0020_");
			return $"{keyString} eq {filterItem.Value}";
		}

		#endregion
	}
}
