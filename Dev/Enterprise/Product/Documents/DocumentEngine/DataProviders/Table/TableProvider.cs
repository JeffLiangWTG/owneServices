using System.Data;

namespace Enterprise.DocumentEngine.DataProviders
{
	public abstract class TableProvider
	{
		protected TableProvider()
		{
		}

		public abstract DataTable GetDataTable(string tableName, string dataSourceString, Report report, bool needsToAddWhereClause);

		public abstract DataTable GetDataTable(string tableName, string dataSourceString, Report report, bool needsToAddWhereClause, int maximumNumberOfRows);

		public virtual string GetValidSelectStatement(string section, string dataSourceString, Report reportObject, int maximumNumberOfRows)
		{
			return dataSourceString;
		}

		public virtual bool HandlesSortInternally
		{
			get { return true; }
		}

		public string DataSourceString
		{
			protected set;
			get;
		}
	}
}

