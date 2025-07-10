using System;
using System.Collections.Generic;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.AuditDataServices.Business;

public class TableInfo : ICloneable
{
	public TableInfo(string tableName, string tablePrefix, SchemaColumn pkSchemaColumn, string sqlSchemaName, string codeProperty, Type relatedBizObjType,
		HashSet<ZGuid> guids)
	{
		TableName = tableName;
		TablePrefix = tablePrefix;
		PkSchemaColumn = pkSchemaColumn;
		SqlSchemaName = sqlSchemaName;
		CodeProperty = codeProperty;
		RelatedBizObjType = relatedBizObjType;
		Guids = guids;
	}

	public string TableName { get; set; }
	public string TablePrefix { get; set; }
	public SchemaColumn PkSchemaColumn { get; set; }

	public string SqlSchemaName { get; set; }

	public string CodeProperty { get; set; }

	public ZDateTime UtcTimeTo { get; set; }

	public Type RelatedBizObjType { get; }

	public HashSet<ZGuid> Guids { get; set; }

	public object Clone()
	{
		return MemberwiseClone();
	}
}
