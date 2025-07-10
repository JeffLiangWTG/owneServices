using System;
using System.Data;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.RuntimeOptions;

namespace Enterprise.DocumentEngine.DataProviders
{
	public abstract class DocumentDataTableProvider : TableProvider
	{
		public override DataTable GetDataTable(string tableName, string dataSourceString, Report report, bool isFirstTable)
		{
			return GetDataTable(tableName, dataSourceString, report, isFirstTable, -1);
		}

		public override DataTable GetDataTable(string tableName, string dataSourceString, Report report, bool isFirstTable, int maximumNumberOfRows)
		{
			return GetDataTable(report.FilterCollection);
		}

		protected DataTable GetDataTable(CollectionOfIFilter filters)
		{
			foreach (IFilter filter in filters)
			{
				if (filter is PrimaryKeyFilter)
				{
					return GetDataTable((Guid)(filter.SqlParameters()[0]).Value);
				}
			}
			throw new DocumentEngineException("Could not find primary key filter");
		}

		protected virtual DataTable GetDataTable(Guid pK)
		{
			return null;
		}
	}
}
