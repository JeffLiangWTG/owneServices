using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public class RelationshipSet<TBizo, TPivot>
		where TBizo : BusinessObject
		where TPivot : BusinessObject
	{
		public RelationshipSet(HashSet<TBizo> nodes, HashSet<TPivot> pivots)
		{
			Nodes = nodes;
			Pivots = pivots;
		}

		public HashSet<TBizo> Nodes { get; private set; }
		public HashSet<TPivot> Pivots { get; private set; }
	}
}
