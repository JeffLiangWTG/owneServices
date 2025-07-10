using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	public interface IZQueryFactory
	{
		ZQuery Create(SchemaColumn schemaColumn, object value);
	}
}
