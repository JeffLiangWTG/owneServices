using CargoWise.Application;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.DB.Helpers
{
	public static class TableNameHelper
	{
		public static string GetTableNameFromPrefix(string prefix)
		{
			var schema = EnterpriseSchema.GetTableSchemaFromColumnNamePrefix(prefix) ?? GetSynonymNameHackForTablesInOtherDatabases(prefix);
			return schema != null ? schema.TableName : null;
		}

		static ITableSchema GetSynonymNameHackForTablesInOtherDatabases(string prefix)
		{
			switch (prefix)
			{
				case USCCountrySchema.Constants.Prefix: return USCCountrySchema.Instance;  // UC is in OdysseyRefDb_Ent_US but there is a synonym in Odyssey. 
				case RefCusTaxOrFeeSchema.Constants.Prefix: return RefCusTaxOrFeeSchema.Instance;  // ZZF is in OdysseyRefDb_Ent_ZZ but there is a synonym in Odyssey. 
				case RefDataGroupingSchema.Constants.Prefix: return RefDataGroupingSchema.Instance;  // ZZF is in OdysseyRefDb_Ent_ZZ but there is a synonym in Odyssey. 
				default: return null;
			}
		}

		public static string GetPrefixFromTableName(string tableName)
		{
			return ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(tableName);
		}
	}
}
