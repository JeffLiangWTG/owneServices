namespace GlowIndexQueryService.Business
{
	public class SearchFieldEntityLookup
	{
		public SearchFieldEntityLookup(string entityType, string valuePath, string tableName, string hierarchyColumnName, string hierarchyColumnValue)
		{
			EntityType = entityType;
			ValuePath = valuePath;
			TableName = tableName;
			HierarchyColumnName = hierarchyColumnName;
			HierarchyColumnValue = hierarchyColumnValue;
		}

		public string EntityType { get; }
		public string ValuePath { get; }
		public string TableName { get; }
		public string HierarchyColumnName { get; }
		public string HierarchyColumnValue { get; }
	}
}
