//CW1131:Db Sp_DbCmptLevel Deprecated Analyzer

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1131
	{
		public void UnsupportedSqlFeature()
		{
			_ = "exec SP_DBCMPTLEVEL 'Odyssey', 10";
		}
	}
}
