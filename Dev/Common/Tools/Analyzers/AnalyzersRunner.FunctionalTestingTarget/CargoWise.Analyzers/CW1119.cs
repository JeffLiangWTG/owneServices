//CW1119:Do Not Use SL_EventTime Table Column Analyzer

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1119
	{
		public void Method()
		{
			_ = "SELECT [SL_EventTime] FROM [MainDb].[dbo].[StmALog] WHERE [SL_EventTime] > '2021-01-01 12:00:00';";
		}
	}
}
