//CW1126:Db Ansi_Null Ansi_Padding Concat_Null_Yields_Null Always On Analyzer

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1126
	{
		public void UnsupportedSqlConfiguration()
		{
			_ = "SET ANSI_NULLS ON";
		}
	}
}
