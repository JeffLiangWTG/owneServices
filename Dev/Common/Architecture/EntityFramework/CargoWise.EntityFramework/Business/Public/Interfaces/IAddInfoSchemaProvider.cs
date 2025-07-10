using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	public interface IAddInfoSchemaProvider
	{
		ITableSchema AddInfoTableSchema { get; }
	}
}
