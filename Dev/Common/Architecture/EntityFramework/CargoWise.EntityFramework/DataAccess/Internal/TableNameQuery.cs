namespace CargoWise.EntityFramework
{
	public struct TableNameQuery
	{
		public TableNameQuery(string tableName, ZQuery query)
		{
			TableName = tableName;
			Query = query;
		}

		public readonly string TableName;
		public readonly ZQuery Query;
	}
}
