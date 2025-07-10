using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	public class ZQueryFactory : IZQueryFactory
	{
		public ZQuery Create(SchemaColumn schemaColumn, object value) => new ZQuery(schemaColumn, value);
	}
}
