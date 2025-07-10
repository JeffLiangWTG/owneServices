namespace GlowIndexQueryService.Common
{
	class LookupEntityDto
	{
		public LookupEntityDto(string lookupSource, string valuePath, string tableName, string hierarchyColumnName, string hierarchyColumnValue)
		{
			LookupSource = lookupSource;
			ValuePath = valuePath;
			TableName = tableName;
			HierarchyColumnName = hierarchyColumnName;
			HierarchyColumnValue = hierarchyColumnValue;
		}

		public string LookupSource { get; }
		public string ValuePath { get; }
		public string TableName { get; }
		public string HierarchyColumnName { get; }
		public string HierarchyColumnValue { get; }
	}
}
