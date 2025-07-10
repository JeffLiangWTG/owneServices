namespace Enterprise.AuditDataServices.ArchiveManager.Helpers
{
	public class Target(string parentTableColumn, string parentIDColumn, string tableName, string pKName, bool parentTableColumnStoresCode)
	{
		public string ParentTableColumn { get; private set; } = parentTableColumn;
		public string ParentIDColumn { get; private set; } = parentIDColumn;
		public string TableName { get; private set; } = tableName;
		public string PKName { get; private set; } = pKName;
		public bool ParentTableColumnStoresCode { get; private set; } = parentTableColumnStoresCode;
	}
}
