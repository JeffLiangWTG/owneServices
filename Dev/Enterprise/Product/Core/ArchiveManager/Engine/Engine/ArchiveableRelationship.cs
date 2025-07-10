using CargoWise.Schema;

namespace Enterprise.ArchiveManager.Engine
{
	public class ArchiveableRelationship
	{
		public ArchiveableRelationship(string parentName, SchemaColumn parentPKColumn, SchemaColumn parentKeyColumnReferencedByChild, string childName, SchemaColumn childPKColumn, SchemaColumn childFKColumn, bool isReversed)
		{
			ParentPKColumn = parentPKColumn;
			ParentKeyColumnReferencedByChild = parentKeyColumnReferencedByChild;
			ChildFKColumn = childFKColumn;
			ChildPKColumn = childPKColumn;
			ParentName = parentName;
			ChildName = childName;
			IsReversed = isReversed;
		}

		public string ParentName { get; private set; }

		public SchemaColumn ParentPKColumn { get; private set; }

		public SchemaColumn ParentKeyColumnReferencedByChild { get; private set; }

		public string ChildName { get; private set; }

		public SchemaColumn ChildPKColumn { get; private set; }

		public SchemaColumn ChildFKColumn { get; private set; }

		public bool IsReversed { get; private set; }
	}
}

