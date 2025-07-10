using System.Collections.Generic;
using CargoWise.Schema;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	public class ZQueryFactoryForTest : IZQueryFactory
	{
		public IEnumerable<ZQuery> Queries => queries;

		readonly List<ZQuery> queries = new List<ZQuery>();

		public ZQuery Create(SchemaColumn schemaColumn, object value)
		{
			var query = new ZQuery(schemaColumn, value);
			queries.Add(query);
			return query;
		}
	}
}
