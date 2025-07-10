namespace Enterprise.AuditDataServices.Subscription.Testing
{
	using System;
	using System.Data;
	using CargoWise.Database.Shared;
	using CargoWise.Schema;

	class SchemaStringColumnForTest : SchemaStringColumn
	{
		public SchemaStringColumnForTest(ITableSchema tableSchema, string name, int ordinal, SqlDbType sqlDbType, object defaultValue, bool isNullable, int maxLength, bool isLiteralOnly = false, bool isNonBlankFilteredIndexParticipant = false, string tvpName = null)
			: base(tableSchema, name, ordinal, sqlDbType, defaultValue, isNullable, maxLength, isLiteralOnly, isNonBlankFilteredIndexParticipant, tvpName ?? TVPHelper.GetTVPName(sqlDbType, maxLength))
		{
		}
		public SchemaStringColumnForTest(string name, SqlDbType sqlType = SqlDbType.NVarChar) : this(new TableSchemaForTest(), name, 0, sqlType, string.Empty, true, 100)
		{ }
	}

	[WTG.StaticAnalysis.Annotation.Immutable]
	class TableSchemaForTest : ITableSchema
	{
		public string SqlSchemaName => "dbo";

		public string TableName => "AuditDataCourierTestTable";

		public string DatabaseName => string.Empty;

		public SchemaPKColumn PK => throw new NotImplementedException();

		public string PkIndexName => "OG_PK";

		public SchemaColumnCollection All => throw new NotImplementedException();

		public SchemaColumn GetSchemaColumn(string columnName)
		{
			throw new NotImplementedException();
		}
	}
}
