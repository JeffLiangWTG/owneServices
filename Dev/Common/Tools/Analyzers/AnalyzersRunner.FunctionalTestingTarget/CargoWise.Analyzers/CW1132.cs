//CW1132:Db SpLock Deprecated Analyzer

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1132
	{
		public void UnsupportedSqlFeature()
		{
			_ = "exec SP_LOCK";
		}
	}
}
