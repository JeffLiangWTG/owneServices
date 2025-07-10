using CargoWise.Schema;

namespace Enterprise.DbUpgrader.Shared
{
	public class KeyDefinition
	{
		public KeyDefinition(SchemaPKColumn pkColumn, SchemaGuidColumn childColumn, bool isMidLevelMaster = false)
		{
			PKColumn = pkColumn;
			ChildColumn = childColumn;
			IsMidLevelMaster = isMidLevelMaster;
		}
		public SchemaPKColumn PKColumn { get; }
		public SchemaGuidColumn ChildColumn { get; }
		public bool IsMidLevelMaster { get; }

		public string ChildTableName => ChildColumn.TableName;
		public string ChildFkColumnName => ChildColumn.Name;
		public string ChildClusterKey => ChildColumn.ColumnPrefix + Schema.ClusterKeyColumnSuffix;
		public string ParentTableName => PKColumn.TableName;
		public string ParentPkColumnName => PKColumn.Name;
		public string ParentClusterKey => PKColumn.ColumnPrefix + Schema.ClusterKeyColumnSuffix;
	}
}
