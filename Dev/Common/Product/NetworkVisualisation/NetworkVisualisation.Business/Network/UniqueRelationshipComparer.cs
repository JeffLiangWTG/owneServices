using System.Collections.Generic;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	public class UniqueRelationshipComparer : EqualityComparer<IEntityRelationship>
	{
		public override bool Equals(IEntityRelationship x, IEntityRelationship y)
		{
			return x.From == y.From && x.To == y.To;
		}

		public override int GetHashCode(IEntityRelationship obj)
		{
			var fromHash = obj.From == null ? 0 : obj.From.GetHashCode();
			var toHash = obj.To == null ? 0 : obj.To.GetHashCode();

			// Based on this algorythm:
			// https://stackoverflow.com/questions/892618/create-a-hashcode-of-two-numbers
			// GetHashCode(r1) may equal to GetHashCode(r1) where r1 and r2 are relationships in the following only cases:
			// 0) r1.From == r2.From and r1.To == r2.To which is the intention of the hash function
			// 1) r1.From == r2.To and r1.To == r2.From which is fine (some entity is connected to itself hypotetically)
			// 2) some other cases like
			//			r2.From.GetHashCode() == r1.From.GetHashCode() - 1 and
			//			r2.To.GetHashCode() == r1.To.GetHashCode() + 31
			//			the probability of such coincidences is low
			//			(4 entity hash codes need to meet the conditions and the number of relationships on diagrams if normally low)
			// 3) coincidence due to integer overflow - the probability of coincidence is negligible in this case
			return unchecked(fromHash * 31 + toHash);
		}
	}
}
