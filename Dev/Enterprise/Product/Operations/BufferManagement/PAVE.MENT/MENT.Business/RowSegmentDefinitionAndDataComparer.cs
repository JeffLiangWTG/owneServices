using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.PAVE.MENT.Business
{
	class RowSegmentDefinitionAndDataComparer : IComparer<RowSegmentDefinitionAndData[]>
	{
		public int Compare(RowSegmentDefinitionAndData[] x, RowSegmentDefinitionAndData[] y)
		{
			var orderedX = x.OrderBy(r => r.Order).ToArray();
			var orderedY = y.OrderBy(r => r.Order).ToArray();

			var minLength = Math.Min(x.Length, y.Length);

			int result = 0;

			for (var i = 0; i < minLength; i++)
			{
				var xValue = orderedX[i].Value;
				var yValue = orderedY[i].Value;

				result = CompareIZTypes(xValue, yValue);

				if (result != 0)
				{
					break;
				}
			}

			if (result == 0)
			{
				if (x.Length > y.Length)
				{
					result = 1;
				}
				else if (x.Length < y.Length)
				{
					result = -1;
				}
				else
				{
					result = 0;
				}
			}

			return result;
		}

		int CompareIZTypes(IZType first, IZType second)
		{
			int result = 0;
			if (first.DataType == second.DataType)
			{
				result = first.CompareTo(second);
			}
			else
			{
				ErrorReporter.ReportOnce("UnableToCompareTypes", string.Format(CultureInfo.InvariantCulture, "first: {0}, second: {1}", first.DataType.ToString(), second.DataType.ToString()));
			}

			return result;
		}
	}
}
