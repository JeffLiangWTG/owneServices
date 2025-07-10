using CargoWise.Bi.Registration.PowerBi;

namespace CargoWise.Bi.Deployment.ReportingServices.Testing
{
	class PowerBiReportTestCase : PowerBiItem
	{
		public override string Name => "TestName"; // Report name
		public override string DataSourceModel => "WarehouseModel";
		public override BiReportCategory Category => BiReportCategory.Warehouse;
		public override BiReportType ReportType => BiReportType.Paginated;
	}
}
