using CargoWise.Bi.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Resource.Version;

namespace Enterprise.DbUpgrader.Startup
{
	public class BiDatabaseChecker
	{
		public BiDatabaseChecker(DbConnection biConnection, string biDbName)
		{
			this.biConnection = biConnection;
			this.biDbName = biDbName;
		}
		readonly DbConnection biConnection;
		readonly string biDbName;

		public bool ForceBiDatabaseUpgrade
		{
			get
			{
				var result = true;

				if (biConnection != null)
				{
					if (biConnection.DatabaseExists(biDbName))
					{
						var mainDbSchemaVersion = SchemaVersion.Application.ToString();
						var biDbSchemaVersion = DataUtils.LoadDbExtendedProperty(biConnection, BiConstants.MainDbSchemaVersionExtPtyName, biDbName);

						result = (mainDbSchemaVersion != biDbSchemaVersion);
					}
				}
				else
				{
					result = false;
				}

				return result;
			}
		}
	}
}
