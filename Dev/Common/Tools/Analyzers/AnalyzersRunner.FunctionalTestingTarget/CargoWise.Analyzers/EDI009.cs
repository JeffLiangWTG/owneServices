using Enterprise.Integration;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class EDI009
	{
		public void Method(ILogger logger)
		{
			//EDI009:Service Task Logs should be in English Only
			logger.Log(LogType.Information, Res.GetString("resource key", "english text"));
		}
	}
}
