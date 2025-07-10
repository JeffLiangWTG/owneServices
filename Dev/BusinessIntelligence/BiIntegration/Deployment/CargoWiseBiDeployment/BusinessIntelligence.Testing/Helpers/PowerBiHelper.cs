using System;
using CargoWise.Bi.Deployment.ReportingServices;
using CargoWise.Bi.Registration.PowerBi;

namespace CargoWise.Bi.BusinessIntelligence.Testing
{
	public static class PowerBiHelper
	{
		public static void DeployReport(string reportName, string resourceName, BiReportType reportType, string parentFolder, string tabularModelSuffixName)
		{
			var deployer = new AnalyticsReportDeployer(null);
			var report = new TestReport(reportName, tabularModelSuffixName);
			if (deployer.FolderExists(parentFolder))
			{
				deployer.DeleteCatalogItem(parentFolder);
			}

			var parent = @"/";
			foreach (var folderName in parentFolder.Split(new[] { @"/" }, StringSplitOptions.RemoveEmptyEntries))
			{
				deployer.CreateFolder(folderName, parent);
				parent = parent + "/" + folderName;
			}
			deployer.CreateReport(reportName, resourceName, reportType, parentFolder);
			deployer.SetDataSource(report, parentFolder);
		}
	}

	class TestReport : PowerBiItem
	{
		public TestReport(string name, string datasourceModel)
		{
			Name = name;
			DataSourceModel = datasourceModel;
		}

		public override string Name { get; } // Report name

		public override string DataSourceModel { get; }

		public override BiReportCategory Category => BiReportCategory.All;
	}
}
