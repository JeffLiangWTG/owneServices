//CW1128:Db DbReindex Deprecated Analyzer

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1128
	{
		public void UnsupportedSqlFeature()
		{
			_ = $"DBCC DBREINDEX ('[dbo].[Rpt_BI_AuditDB]', ' ', 70);";
		}
	}
}
