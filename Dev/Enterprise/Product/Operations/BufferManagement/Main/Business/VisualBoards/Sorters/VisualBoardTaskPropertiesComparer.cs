using System;
using System.Collections.Generic;

namespace Enterprise.BufferManagement.Business
{
	public class VisualBoardTaskPropertiesComparer : Comparer<ITaskOrderable>
	{
		public override int Compare(ITaskOrderable x, ITaskOrderable y)
		{
			if (x == null && y == null)
			{
				return 0;
			}

			var compares = new Func<int>[] {
				() => VisualBoardTaskComparer.CompareNull(x, y),
				() => CompareNudge(x, y),
				() => CompareReleaseDate(x, y),
				() => CompareSequence(x, y),
				() => CompareTaskID(x, y)
			};

			var result = 0;

			foreach (var compare in compares)
			{
				result = compare();

				if (result != 0)
				{
					return result;
				}
			}

			return result;
		}

		static int CompareNudge(ITaskOrderable x, ITaskOrderable y)
		{
			return -1 * x.Nudge.CompareTo(y.Nudge); // Invert the result because higher nudge comes first
		}

		static int CompareReleaseDate(ITaskOrderable x, ITaskOrderable y)
		{
			var xReleaseDate = x.ReleaseDate;
			var yReleaseDate = y.ReleaseDate;

			if (xReleaseDate == null && yReleaseDate == null)
			{
				return 0;
			}

			var result = VisualBoardTaskComparer.CompareNull(xReleaseDate, yReleaseDate);

			if (result != 0)
			{
				return result;
			}

			return xReleaseDate.CompareTo(yReleaseDate);
		}

		static int CompareSequence(ITaskOrderable x, ITaskOrderable y)
		{
			return x.Sequence.CompareTo(y.Sequence);
		}

		static int CompareTaskID(ITaskOrderable x, ITaskOrderable y)
		{
			return string.Compare(x.TaskID, y.TaskID, StringComparison.OrdinalIgnoreCase);
		}
	}
}
