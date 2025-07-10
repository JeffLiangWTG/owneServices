using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using Enterprise.DataTransfer.Native.Common.Exceptions;

namespace Enterprise.DataTransfer.Native.Common.Definitions.Associations
{
	public class AssociationCollection : IEnumerable<AssociationDefinition>
	{
		public ICollection<AssociationDefinition> ParentAssociations
		{
			get { return parentAssociations; }
		}
		readonly HashSet<AssociationDefinition> parentAssociations = new HashSet<AssociationDefinition>();

		public ICollection<AssociationDefinition> ChildAssociations
		{
			get { return childAssociations; }
		}
		readonly HashSet<AssociationDefinition> childAssociations = new HashSet<AssociationDefinition>();

		public AssociationDefinition MainAssociation
		{
			get;
			internal set;
		}

		public IEnumerable<AssociationDefinition> Associations
		{
			get
			{
				return ParentAssociations.Union(ChildAssociations);
			}
		}

		public AssociationDefinition this[IEntityDefinition from, IEntityDefinition to]
		{
			get
			{
				var results = Associations.Where(a => a.From == from && a.To == to);
				if (results.Count() != 1)
				{
					throw new ResultNotFoundException("Could not find exception");
				}
				return results.First();
			}
		}

		#region Enumerator

		public IEnumerator<AssociationDefinition> GetEnumerator()
		{
			return Associations.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}