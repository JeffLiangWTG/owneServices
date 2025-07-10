using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using Enterprise.DataTransfer.Native.DB.Keys;
using Enterprise.DataTransfer.Native.DB.Sql;
using Enterprise.DataTransfer.Native.Utils;

namespace Enterprise.DataTransfer.Native.Common.Definitions.Associations
{
	/// <summary>
	/// Inner class Constructor. 
	/// In order child create an immutable Association
	/// </summary>
	public class AssociationConstructor
	{
		readonly EntityDefinition parent;
		readonly EntityDefinition child;
		readonly string cardinality;

		internal AssociationConstructor(EntityDefinition parent, EntityDefinition child, string cardinality)
		{
			this.parent = parent;
			this.child = child;
			this.cardinality = cardinality;
		}

		public AssociationDefinition WithKeys(List<AssociationKeyInfo> parentKeys)
		{
			var keys = parentKeys.Select(x => BuildKey(x.ParentKey, x.RefKey));
			var relation = Relation.Build(keys.ToArray());
			return BuildAssociation(relation);
		}

		#region Implementation

		Key BuildKey(string keyName, string referenceKeyName)
		{
			var from = child.Table;
			var to = parent.Table;
			var keyColumn = from.Columns[keyName];
			if (referenceKeyName.IsEmpty())
			{
				return ForeignKey.Build(keyColumn, to);
			}
			else
			{
				var candidateKey = ForeignKey.Build(keyColumn, to.Columns[referenceKeyName]);
				// Only NaturalKey, which does not follow name convention, should be override. Otherwise please leave RefKey XML attribute blank.
				return referenceKeyName == candidateKey.ReferenceColumnDef.Name ? candidateKey : new Key(candidateKey)
				{
					ReferenceTable = parent.Table,
					ReferenceColumnDef = parent.Table.Columns[referenceKeyName]
				};
			}
		}

		AssociationDefinition BuildAssociation(Relation relation)
		{
			return new AssociationDefinition(from: child,
				to: parent,
				cardinality: BuildCardinality(cardinality),
				relation: ImmutableList.Create(new[] { relation }));
		}

		static Cardinality BuildCardinality(string str)
		{
			if (str == "*")
			{
				return Cardinality.OneToMany;
			}

			if (str == "1")
			{
				return Cardinality.OneToOne;
			}
			return Cardinality.OneToMany;
		}

		#endregion
	}
}
