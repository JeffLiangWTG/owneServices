using System.Collections;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class UniqueIdentifierMessageLineComparer : IComparer
	{
		#region IComparer Members

		public int Compare(object x, object y)
		{
			int result = 0;

			if (x != y && x is UniqueIdentifierMessageLine && y is UniqueIdentifierMessageLine)
			{
				UniqueIdentifierMessageLine lineX = (UniqueIdentifierMessageLine)x;
				UniqueIdentifierMessageLine lineY = (UniqueIdentifierMessageLine)y;

				result = lineX.UniqueIdentifier.CompareTo(lineY.UniqueIdentifier);
			}

			return result;
		}

		#endregion
	}
}
