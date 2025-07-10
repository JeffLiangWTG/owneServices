#if DEBUG
using System.Collections;

namespace CargoWise.EntityFramework
{
	public class InstantiationTimeComparer : IComparer
	{
		public int Compare(object x, object y) => ((BusinessObject)x).InstantiationTime.CompareTo(((BusinessObject)y).InstantiationTime);
		public override bool Equals(object obj) => obj is InstantiationTimeComparer;
		public override int GetHashCode() => typeof(InstantiationTimeComparer).GetHashCode();
	}
}
#endif
