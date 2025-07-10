using System.Data;
using CargoWise.Data;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	public sealed class ZSqlSaverFactory : IZSqlSaverFactory
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public IZSqlSaver GetSqlSaver(DataSet set, DbConnection connection, IApplicationSchemaResolver schemaResolver)
			=> new ZSqlSaverWithRetry(set, connection, schemaResolver);
	}
}
