namespace Enterprise.ArchiveManager.Test
{
	struct TestArchiveableRelationship
	{
		internal TestArchiveableRelationship(string parentTableName, string parentKeyColumnReferencedByChild, string childFKTableName, string childFKColumnName, bool isReversed)
		{
			ParentTableName = parentTableName;
			ParentKeyColumnReferencedByChild = parentKeyColumnReferencedByChild;
			ChildFKTableName = childFKTableName;
			ChildFKColumnName = childFKColumnName;
			IsReversed = isReversed;
		}

		internal string ParentTableName { get; private set; }

		internal string ParentKeyColumnReferencedByChild { get; private set; }

		internal string ChildFKTableName { get; private set; }

		internal string ChildFKColumnName { get; private set; }

		internal bool IsReversed { get; private set; }

		public override string ToString()
			=> $"{ParentTableName}, {ParentKeyColumnReferencedByChild}, {ChildFKTableName}, {ChildFKColumnName}, {IsReversed}";
	}
}
