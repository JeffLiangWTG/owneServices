using CargoWise.Integration;

namespace CargoWise.Data
{
	public interface IZSqlSaver
	{
		IChangedTableNames Save(bool compress = true);
	}
}
