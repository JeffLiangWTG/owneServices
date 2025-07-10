using System.Collections;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class PackingGroupLineNumberComparer : IComparer
	{
		#region IComparer Members

		public int Compare(object x, object y)
		{
			IPackingGroup lineX = (IPackingGroup)x;
			IPackingGroup lineY = (IPackingGroup)y;
			int result = lineX.HouseContainerNumber.CompareTo(lineY.HouseContainerNumber);
			return result;
		}

		#endregion
	}
}
