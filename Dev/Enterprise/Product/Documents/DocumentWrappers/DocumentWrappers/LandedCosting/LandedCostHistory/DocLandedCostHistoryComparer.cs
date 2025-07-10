using System.Collections;
using Enterprise.LandedCosting.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocLandedCostHistoryComparer : IComparer
	{
		#region IComparer Members

		public int Compare(object x, object y)
		{
			DocLandedCostHistory docHistoryX = x as DocLandedCostHistory;
			DocLandedCostHistory docHistoryY = y as DocLandedCostHistory;
			if (docHistoryX == null && docHistoryY != null)
			{
				return -1;
			}
			else if (docHistoryX != null && docHistoryY == null)
			{
				return 1;
			}
			else if (docHistoryX == null && docHistoryY == null)
			{
				return 0;
			}

			if (docHistoryX.LCHistory == null && docHistoryY.LCHistory != null)
			{
				return -1;
			}
			else if (docHistoryX.LCHistory != null && docHistoryY.LCHistory == null)
			{
				return 1;
			}
			else if (docHistoryX.LCHistory == null && docHistoryY.LCHistory == null)
			{
				return 0;
			}

			return new DistributeeLineComparer().Compare(docHistoryX.LCHistory, docHistoryY.LCHistory);
		}

		#endregion
	}
}
