using Enterprise.DocumentEngineIntegration.RollUpSort;

namespace Enterprise.DocumentWrappers
{
	internal static class DocLineSorter
	{
		public static int CompareBySequence(ISortableDocLine x, ISortableDocLine y)
		{
			if (x == null || y == null)
			{
				return 0;
			}

			int xPrintOrder = x.OrgLevelSortOrder;
			int yPrintOrder = y.OrgLevelSortOrder;

			if (xPrintOrder.CompareTo(yPrintOrder) == 0)
			{
				xPrintOrder = x.ChargePrintSeqSortOrder;
				yPrintOrder = y.ChargePrintSeqSortOrder;

				if (xPrintOrder.CompareTo(yPrintOrder) == 0)
				{
					xPrintOrder = x.UserEnteredSortOrder;
					yPrintOrder = y.UserEnteredSortOrder;
				}
			}

			int zeroCaseInverter = xPrintOrder != 0 && yPrintOrder != 0 ? 1 : -1;
			int result = xPrintOrder.CompareTo(yPrintOrder) * zeroCaseInverter;

			return result == 0
					? x.AlphabeticalSortOrder.CompareTo(y.AlphabeticalSortOrder)
					: result;
		}

		public static int CompareByUserEntered(ISortableDocLine x, ISortableDocLine y)
		{
			if (x == null || y == null)
			{
				return 0;
			}

			int zeroCaseInverter = x.UserEnteredSortOrder != 0 && y.UserEnteredSortOrder != 0 ? 1 : -1;
			int result = x.UserEnteredSortOrder.CompareTo(y.UserEnteredSortOrder) * zeroCaseInverter;

			return result == 0
				? x.AlphabeticalSortOrder.CompareTo(y.AlphabeticalSortOrder)
				: result;
		}

		public static int CompareAlphabetically(ISortableDocLine x, ISortableDocLine y)
		{
			if (x == null || y == null)
			{
				return 0;
			}

			int result = x.AlphabeticalSortOrder.CompareTo(y.AlphabeticalSortOrder);

			return result == 0
				? CompareByUserEntered(x, y)
				: result;
		}
	}
}
