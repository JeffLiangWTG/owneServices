using System;
using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	public class VisualBoardTaskComparer : Comparer<ITaskOrderable>
	{
		public override int Compare(ITaskOrderable x, ITaskOrderable y)
		{
			if (x == null && y == null)
			{
				return 0;
			}

			var compares = new Func<int>[] {
				() => CompareNull(x, y),
				() => CompareStatus(x, y, ProcessTaskStatusCodeList.Codes.Working),
				() => CompareStatus(x, y, ProcessTaskStatusCodeList.Codes.Suspended),
				() => CompareIsCurrent(x, y),
				() => new VisualBoardTaskPropertiesComparer().Compare(x, y)
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

		public static int CompareNull(object x, object y)
		{
			if (x == null)
			{
				return y == null ? 0 : -1;
			}

			if (y == null)
			{
				return 1;
			}

			return 0;
		}

		static int CompareStatus(ITaskOrderable x, ITaskOrderable y, string status)
		{
			if (x.Status == y.Status)
			{
				return 0;
			}
			else if (x.Status == status)
			{
				return -1;
			}
			else if (y.Status == status)
			{
				return 1;
			}
			return 0;
		}

		static int CompareIsCurrent(ITaskOrderable x, ITaskOrderable y)
		{
			return y.IsCurrent.CompareTo(x.IsCurrent);
		}
	}
}
