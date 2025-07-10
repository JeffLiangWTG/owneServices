using System.Collections.Generic;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	public class ProposedNetworkEntityEqualityComparer : IEqualityComparer<IProposedNetworkEntity>
	{
		public bool Equals(IProposedNetworkEntity x, IProposedNetworkEntity y)
		{
			// We compare references first for mocked entities which may not implement IsSameEntity and also use IsSameEntity as different entities may be considered equal if they have same PKs (like when we search the node by passing a shape, not ShapeNetworkEntity in BM).
			// After WI00215026 is done, this comparer may be considered as redundant.
			return x == y || x.IsSameEntity(y);
		}

		public int GetHashCode(IProposedNetworkEntity obj)
		{
			return obj.GetHashCode();
		}
	}
}
