namespace Enterprise.BusinessObjectGenerator
{
	public class AddInfoBusinessObjectInfo : BusinessObjectInfo
	{
		public AddInfoBusinessObjectInfo(
			BusinessObjectInfo businessObjectInfo,
			string oldPrefix,
			string baseValidationClassName,
			string baseLookupsClassName,
			string underlyingTableName,
			string parentSchema,
			string parentView,
			string childTableName,
			AutoProperty[] childTableProperties)
			: base(
				businessObjectInfo.FileName,
				businessObjectInfo.IsInZArchitectureSolution,
				businessObjectInfo.IsPersistent,
				businessObjectInfo.Namespace,
				businessObjectInfo.NamespaceOfSchema,
				businessObjectInfo.BaseClassName,
				businessObjectInfo.SqlSchemaName,
				businessObjectInfo.Table,
				businessObjectInfo.DecimalScaleTable,
				businessObjectInfo.ForeignKeysTable,
				businessObjectInfo.DateTimeOffsetScaleTable,
				businessObjectInfo.RefDbCountry,
				businessObjectInfo.RefDbType,
				businessObjectInfo.DbTypes,
				businessObjectInfo.UniqueKeys,
				businessObjectInfo.CanForceUpdateNaturalKeyCacheColumns,
				businessObjectInfo.MasterFiles,
				businessObjectInfo.PkIndex,
				businessObjectInfo.Indexes,
				businessObjectInfo.MasterFileReference,
				businessObjectInfo.PreventDelete,
				businessObjectInfo.LiteralOnlyColumns,
				businessObjectInfo.NonBlankFilteredIndexColumns,
				businessObjectInfo.Tables,
				businessObjectInfo.ConvertZStringToWesternEuropeanCharacters)
		{
			BaseValidationClassName = baseValidationClassName;
			BaseLookupsClassName = baseLookupsClassName;
			OldPrefix = oldPrefix;
			UnderlyingTableName = underlyingTableName;
			ParentSchema = parentSchema;
			ParentView = parentView;
			ChildTableName = childTableName;
			ChildTableProperties = childTableProperties;
		}

		public readonly string BaseValidationClassName;
		public readonly string BaseLookupsClassName;
		public readonly string OldPrefix;
		public readonly string UnderlyingTableName;
		public readonly string ParentSchema;
		public readonly string ParentView;
		public readonly string ChildTableName;
		public readonly AutoProperty[] ChildTableProperties;
	}
}
