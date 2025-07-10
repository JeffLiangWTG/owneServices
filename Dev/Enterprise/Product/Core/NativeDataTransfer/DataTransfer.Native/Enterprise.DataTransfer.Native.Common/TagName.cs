namespace Enterprise.DataTransfer.Native.Common
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "const string")]
	public static class TagName
	{
		// Common Attribute
		public const string Name = "Name";
		public const string Type = "Type";

		// Criteria
		public const string CriteriaGroup = "CriteriaGroup";
		public const string Criteria = "Criteria";

		// Update
		public const string Action = "Action";

		//XML Renderer
		public const string PrimaryKey = "PK";

		public const string ForeignKey = "ForeignKey";
		public const string ForeignKeyElement = "ForeignKeyElement";
		public const string ForeignKeyReferTable = "Table";
		public const string ForeignKeyValue = "Value";

		public const string Key = "Key";
		public const string KeyElement = "KeyElement";
		public const string KeyField = "Field";

		// Entity Definition
		public const string EntitySetName = "EntitySetName";
		public const string Entities = "Entities";
		public const string Entity = "Entity";
		public const string TableName = "TableName";
		public const string Root = "Root";
		public const string FieldName = "FieldName";

		// Association Definition
		public const string Associations = "Associations";
		public const string Association = "Association";
		public const string AssociationKey = "AssociationKey";
		public const string Parent = "To";
		public const string Child = "From";
		public const string Through = "Through";
		public const string ChildFk = "ChildFK";
		public const string Cardinality = "Cardinality";

		// Definition
		public const string NativeSchemaSet = "NativeSchemaSet";
		public const string NoImportReason = "NoImportReason";
		public const string TableSuffix = "Suffix";
		public const string EntityName = "EntityName";
		public const string HasCustomColumns = "HasCustomFields";
		public const string IsUpdateOrInsert = "UpdateOrInsertBehaviour";
		public const string IsExternal = "External";
		public const string IsShowAll = "ShowAll";
		public const string IsCompact = "Compact";
		public const string IncludeParentTableCode = "IncludeParentTableCode";
		public const string ExcludedProperty = "ExcludedProperty";
		public const string ExcludedFromImportProperty = "ExcludedFromImportProperty";
		public const string ExcludedFromExportProperty = "ExcludedFromExportProperty";
		public const string IncludedProperty = "IncludedProperty";
		public const string RequiresAdditionOfActionEqualsMerge = "RequiresAdditionOfActionEqualsMerge";  // This tells us to add Action="MERGE" to a node when writing it our XML(exporting). Required under some circumstances for re-import to inform the engine to walk the parents of non root entities
		public const string StripPropertyCRLF = "StripPropertyCRLF";
		public const string ExcludeFromStripProperty = "ExcludeFromStripProperty";
		public const string UniqueCriteria = "UniqueCriteria";
		public const string ExcludedProperies = "ExcludedProperties";
		public const string Property = "Property";
		public const string Behaviour = "Behaviour";
		public const string DateRangeStartField = "DateRangeStartField";
		public const string DateRangeEndField = "DateRangeEndField";
		public const string OptionalEntityCondition = "OptionalEntityCondition";
		public const string UseBatching = "UseBatching";

		//CodeMapping
		public const string Relationship = "Relationship";
		public const string RelationshipResolver = "RelationshipResolver";
	}
}
