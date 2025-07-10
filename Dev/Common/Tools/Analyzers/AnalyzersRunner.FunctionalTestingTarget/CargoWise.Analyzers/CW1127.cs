//CW1127:Db Set Offsets Unavailable Analyzer

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1127
	{
		public void UnsupportedSqlConfiguration()
		{
			_ = "DBCC SET OFFSETS";
		}
	}
}
