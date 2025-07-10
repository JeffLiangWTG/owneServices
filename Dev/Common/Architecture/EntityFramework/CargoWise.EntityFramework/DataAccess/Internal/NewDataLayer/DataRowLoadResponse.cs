using System;
using System.Data;

namespace CargoWise.EntityFramework
{
	public class DataRowLoadResponse
	{
		public DataRowLoadResponse(string tableName, ZQuery query, DataRow[] allRows, DataRow[] newRows)
		{
			if (tableName == null)
			{
				throw new ArgumentNullException(nameof(tableName));
			}
			if (query == null)
			{
				throw new ArgumentNullException(nameof(query));
			}
			if (allRows == null)
			{
				throw new ArgumentNullException(nameof(allRows));
			}
			if (newRows == null)
			{
				throw new ArgumentNullException(nameof(newRows));
			}

			this.allRows = allRows;
			this.newRows = newRows;
			this.query = query;
			this.tableName = tableName;
		}

		public string TableName
		{
			get { return tableName; }
		}

		public ZQuery Query
		{
			get { return query; }
		}

		public DataRow[] AllRows
		{
			get { return allRows; }
		}

		public DataRow[] NewRows
		{
			get { return newRows; }
		}

		readonly DataRow[] allRows;
		readonly DataRow[] newRows;
		readonly ZQuery query;
		readonly string tableName;
	}
}
