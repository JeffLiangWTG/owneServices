using System;
using System.Collections.Generic;
using CargoWise.Bi.Registration.PowerBi;
using Enterprise.Integration;
using Newtonsoft.Json.Linq;

namespace CargoWise.Bi.Deployment.ReportingServices.Testing
{
	public class PowerBiDeployerTestConcrete : PowerBiDeployer
	{
		public PowerBiDeployerTestConcrete(ILogger logger) : base(logger)
		{ }
		protected override BiReportCategory ReportCategory => BiReportCategory.Analytics;
		protected override bool IsDataSourceConfigured(JObject dataSourceObj) => true;
		protected override void SetDataSources(IEnumerable<PowerBiItem> items)
		{ }
		protected override string BaseFolder => "Analytics";
		public override PowerBiCatalogItem[] ListChildCatalogItems(string path, bool recursive)
		{
			throw new Exception();
		}

		protected override void SetDatasetDataSource(PowerBiItem item, string parentFolder)
		{
			throw new NotImplementedException();
		}

		protected override void CreateDatasetDataSource(string model, string parentFolder)
		{
			throw new NotImplementedException();
		}
	}
}
