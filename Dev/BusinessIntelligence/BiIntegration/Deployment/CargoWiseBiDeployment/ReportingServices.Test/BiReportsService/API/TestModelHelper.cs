using CargoWise.Data;

namespace CargoWise.Bi.Deployment.ReportingServices.Testing
{
	class TestModelHelper : AnalyticsModelHelper
	{
		public override string LogisticsModel => $"{Db.DatabaseName}_LogisticsModel_WebServiceTest";
	}
}
