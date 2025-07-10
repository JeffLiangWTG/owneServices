#if NETCOREAPP
using CargoWise.Data.SqlProxy.Interface.Models;

namespace CargoWise.Data.SqlProxy.Interface.Converters;

public static class DataReaderSerializer
{
	public static async IAsyncEnumerable<SqlProxyReaderResponseItem> SerializeDataReader(DisposableSqlDataReaderWrapper dataReader)
	{
		try
		{
			do
			{
				// Write the result header
				yield return new SqlReaderResponseHeader
				{
					Columns = Enumerable.Range(0, dataReader.FieldCount)
						.Select(i => new SqlReaderResponseHeaderColumn()
						{
							Name = dataReader.GetName(i),
							Type = dataReader.GetDataTypeName(i)
						})
						.ToArray(),
					RecordsAffected = dataReader.RecordsAffected,
					HasRows = dataReader.HasRows
				};

				// Write all results
				while (await dataReader.ReadAsync())
				{
					yield return new SqlReaderResponseRow
					{
						Depth = dataReader.Depth,
						Values = Enumerable.Range(0, dataReader.FieldCount)
							.Select(i => new SqlValue(dataReader.GetSqlValue(i), dataReader.GetDataTypeName(i)))
							.ToArray()
					};
				}

				if (!await dataReader.NextResultAsync())
				{
					break;
				}

			} while (true);
		}
		finally
		{
			await dataReader.DisposeAsync();
		}
	}
}
#endif
