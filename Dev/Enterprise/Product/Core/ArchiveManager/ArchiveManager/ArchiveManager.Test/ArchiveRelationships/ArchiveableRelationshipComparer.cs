using System.Collections.Generic;
using Enterprise.ArchiveManager.Engine;

namespace Enterprise.ArchiveManager.Test
{
	public class ArchiveableRelationshipComparer : IEqualityComparer<ArchiveableRelationship>
	{
		public bool Equals(ArchiveableRelationship x, ArchiveableRelationship y)
			=> x.ParentName == y.ParentName
				&& x.ParentKeyColumnReferencedByChild == y.ParentKeyColumnReferencedByChild
				&& x.ChildName == y.ChildName
				&& x.ChildFKColumn == y.ChildFKColumn
				&& x.IsReversed == y.IsReversed;

		public int GetHashCode(ArchiveableRelationship obj)
			=> obj.ParentName.GetHashCode()
				^ obj.ParentKeyColumnReferencedByChild.GetHashCode()
				^ obj.ChildName.GetHashCode()
				^ obj.ChildFKColumn.GetHashCode()
				^ obj.IsReversed.GetHashCode();
	}
}
