using System.Data;
using CargoWise.Schema;

namespace CargoWise.Data
{
	public interface IZSqlSaverFactory
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		IZSqlSaver GetSqlSaver(DataSet set, DbConnection connection, IApplicationSchemaResolver schemaResolver);
	}
}
