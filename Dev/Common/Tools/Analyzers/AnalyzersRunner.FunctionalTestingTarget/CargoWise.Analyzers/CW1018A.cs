using System.Configuration;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1018A
	{
		public void Method()
		{
			//CW1018A:Http Application Db App Settings Rule
			_ = ConfigurationManager.AppSettings["ServerName"];
			_ = ConfigurationManager.AppSettings["DatabaseName"];
		}
	}
}
