using System.Collections.Generic;

namespace CargoWise.Bi.Registration.PowerBi.Logistics
{
	class PowerBIReportItemForTest : PowerBiItem
	{
		readonly BiReportCategory reportCategory;
		readonly string reportName;
		readonly bool shouldSessionBePerpetualInGlow;

		public override Dictionary<string, string> GetQueryStringFilters(string companyCode, string countryCode, string branchCode)
		{
			var filters = new Dictionary<string, string>();
			filters.Add("Company Branch/Company Code", companyCode);
			filters.Add("Company Branch Helper/Company Code", companyCode);
			if (!string.IsNullOrEmpty(countryCode))
			{
				filters.Add("Company Country/Country Code", countryCode);
			}
			return filters;
		}

		public override string Name => reportName;

		public override string DataSourceModel => "Test Datasource Model";

		public override BiReportCategory Category => reportCategory;

		public override bool ShouldSessionBePerpetualInGLOW => shouldSessionBePerpetualInGlow;
	}
}
