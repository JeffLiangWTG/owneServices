using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using Enterprise.DataTransfer.Native.DB;
using Enterprise.DataTransfer.Native.DB.Keys;
using Enterprise.DataTransfer.Native.DB.Sql;

namespace Enterprise.DataTransfer.Native.Common.Definitions.Associations
{
	public class ManyToManyAssociation : AssociationDefinition
	{
		public ManyToManyAssociation(IEntityDefinition from, IEntityDefinition to, ImmutableList<Relation> relation)
			: base(from, to, Cardinality.OneToMany, relation)
		{
			var invalidRelation = relation.FirstOrDefault(x => x.Keys.Length > 1);
			if (invalidRelation != null)
			{
				throw new NativeXMLUserVisibleException(string.Format(CultureInfo.InvariantCulture, "Composite Keys is not implemented to support the many to many relationship from {0} table to {1} table[{2}]",
					invalidRelation.From.Name, invalidRelation.To.Name, string.Join(",", invalidRelation.Keys.Select(x => x.FromKey + ":" + x.ToKey))));
			}
		}

		//First Element is Parent Relation(To Relation)
		//Second Element is Child Relation(From Relation)

		public override Table JunctionTable => Relation[0].From;

		public override Relation GetRelation(IEntityDefinition definition)
		{
			if (definition == From)
			{
				return Relation[1];
			}
			else if (definition == To)
			{
				return Relation[0];
			}
			throw new NativeXMLUserVisibleException(string.Format(CultureInfo.InvariantCulture, "Could not find the matching Relation with {0}", definition.EntityName));
		}

		public Key ParentJunctionFk => Relation[0].Keys[0].FromKey;

		public Key ChildJunctionFk => Relation[1].Keys[0].FromKey;
	}
}
