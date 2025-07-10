using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using Enterprise.DataTransfer.Native.DB;
using Enterprise.DataTransfer.Native.DB.Keys;
using Enterprise.DataTransfer.Native.DB.Sql;

namespace Enterprise.DataTransfer.Native.Common.Definitions.Associations
{
	public class ManyToManyAssociationConstructor
	{
		readonly EntityDefinition parent;
		readonly EntityDefinition child;

		internal ManyToManyAssociationConstructor(EntityDefinition parent, EntityDefinition child)
		{
			this.parent = parent;
			this.child = child;
		}

		public ManyToManyAssociation Through(string tableName, string childFK, List<AssociationKeyInfo> parentFKs)
		{
			if (parentFKs.Count != 1)
			{
				throw new NativeXMLUserVisibleException(string.Format(CultureInfo.InvariantCulture, "Composite Keys is not implemented to support the many to many relationship in {0} table[{1}]",
						tableName, string.Join(",", parentFKs.Select(x => x.ParentKey + ":" + x.RefKey))));
			}

			return BuildManyToManyAssociation(tableName, childFK, parentFKs[0].ParentKey);
		}

		ManyToManyAssociation BuildManyToManyAssociation(string through, string childFKName, string parentFKName)
		{
			var from = child.Table;
			var to = parent.Table;
			var throughTable = Table.Get(through);
			var parentFK = ForeignKey.Build(throughTable.Columns[parentFKName], to);
			var parentRelation = Relation.Build(parentFK);

			var childFK = ForeignKey.Build(throughTable.Columns[childFKName], from);
			var childRelation = Relation.Build(childFK);
			var relation = ImmutableList.Create(new[] { parentRelation, childRelation });

			return new ManyToManyAssociation(from: child,
				to: parent,
				relation: relation);
		}
	}
}
