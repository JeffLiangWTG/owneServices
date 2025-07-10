using System.Collections.Generic;
using System.Globalization;
using CargoWise.Common;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade
{
	public class PopulatedTable
	{
		public PopulatedTable(string tableDb, string tableSchemaName, string tableName, string preAddDb, string preAddTableName)
		{
			TargetTableDb = tableDb;
			TargetTableSchema = tableSchemaName;
			TargetTableName = tableName;
			TargetTableFullName = string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}", tableDb.QuoteName(), tableSchemaName.QuoteName(), tableName.QuoteName());
			PreAddTableFullName = string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}", preAddDb.QuoteName(), tableSchemaName.QuoteName(), preAddTableName.QuoteName());
		}

		public string TargetTableDb;
		public string TargetTableSchema;
		public string TargetTableName;
		public string TargetTableFullName;
		public string TargetTablePKColumnName;
		public string TargetTablePKColumnType;
		public TargetTableStatus? TargetTableStatus;
		public long TargetTableSize;

		public string PreAddTableFullName;

		public string OrderByExpression;

		public int PopulatingWatermark;

		public List<PopulatedColumn> ColumnList = new List<PopulatedColumn>();
	}
}
