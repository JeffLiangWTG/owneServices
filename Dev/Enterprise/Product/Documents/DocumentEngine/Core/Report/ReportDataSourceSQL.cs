
namespace Enterprise.DocumentEngine
{
	public class ReportSQLSource
	{
		public ReportSQLSource(string tableName, string selectStatement)
		{
			this.TableName = tableName;
			this.SelectStatement = selectStatement;
		}
		public readonly string TableName;
		public readonly string SelectStatement;
	}
}
