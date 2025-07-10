using System;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using Enterprise.DataTransfer.Native.DB;
using Enterprise.DataTransfer.Native.DB.Keys;
using Enterprise.DataTransfer.Native.DB.Sql;
using Enterprise.DataTransfer.Native.Utils;
using Enterprise.DataTransfer.Native.Utils.Models;

namespace Enterprise.DataTransfer.Native.Common.Definitions.Associations
{
	/// <summary>
	// 1. A -> B: 
	// A is child, B is parent
	// A = from, B = to
	// 2. A <- C -> B : C is junction table, A is Child, B is Parent
	// A = Child.To, C = Child.From
	// B = Parent.To, C = Parent.From
	/// </summary>
	public class AssociationDefinition : Edge<IEntityDefinition>
	{
		public AssociationDefinition(IEntityDefinition from, IEntityDefinition to, Cardinality cardinality, ImmutableList<Relation> relation)
			: base(from, to)
		{
			Cardinality = cardinality;
			Relation = new ReadOnlyCollection<Relation>(relation);
		}

		public static AssociationDefinition New(EntityDefinition parent, EntityDefinition child, AssociationInfo info)
		{
			var result = info.ThroughTable.IsEmpty()
				? new AssociationConstructor(parent, child, info.Cardinality).WithKeys(info.ParentKeys)
				: new ManyToManyAssociationConstructor(parent, child).Through(info.ThroughTable, info.ChildKey, info.ParentKeys);
			result.WhereThisColumnIsNull = info.WhereThisColumnIsNull;
			result.AdditionalKey = info.AdditionalKey;
			result.AdditionalKeyRef = info.AdditionalKeyRef;
			result.LinkChild = info.LinkChild;
			return result;
		}

		public virtual Cardinality Cardinality { get; }

		public virtual Relation GetRelation(IEntityDefinition definition)
		{
			if (definition.Equals(From) || definition.Equals(To))
			{
				return Relation[0];
			}

			throw new ArgumentException();
		}

		public virtual Table JunctionTable => null;

		public ReadOnlyCollection<Key> ForeignKeys => new ReadOnlyCollection<Key>(Relation[0].Keys.Select(x => x.FromKey).Where(x => x is ForeignKey).ToList());

		public bool IsExternal => From.IsExternal || To.IsExternal;

		public string WhereThisColumnIsNull { get; private set; }

		public string AdditionalKey { get; private set; }

		public string AdditionalKeyRef { get; private set; }

		public bool LinkChild { get; private set; }

		public virtual ReadOnlyCollection<Relation> Relation { get; }
	}
}
