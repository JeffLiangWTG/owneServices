using CargoWise.EntityFramework;
using CargoWise.Schema;

namespace Enterprise.Customs.ManifestBase
{
	public static class QueryHelper
	{
		public static ZQuery QueryWithFetchOnlyFromLocalCache(SchemaColumn schemaColumn, object value, bool isInDatabase)
		{
			return new ZQuery(schemaColumn, value)
			{
				FetchOnlyFromLocalCache = !isInDatabase
			};
		}
	}
}

