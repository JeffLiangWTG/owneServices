//CW1130:Db Index Key Property Deprecated Analyzer

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1130
	{
		public void UnsupportedSqlFeature()
		{
			_ = "SELECT INDEXKEY_PROPERTY(OBJECT_ID('[dbo].[Rpt_BI_AuditDB]', 'U'), 1, 1, 'IncrementId') AS [Increment ID];";
		}
	}
}
