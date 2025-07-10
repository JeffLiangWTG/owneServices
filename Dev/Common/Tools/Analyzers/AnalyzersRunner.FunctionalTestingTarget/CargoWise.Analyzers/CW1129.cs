//CW1129:Db Index Defrag Deprecated Analyzer

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1129
	{
		public void UnsupportedSqlFeature()
		{
			_ = $"DBCC INDEXDEFRAG ('Odyssey', '[dbo].[Rpt_BI_AuditDB]', IX_Rpt_BI_AuditDB_IncrementId);";
		}
	}
}
