using System;
using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	class VisualBoardBucketTaskComparer : Comparer<ITaskOrderable>
	{
		public override int Compare(ITaskOrderable x, ITaskOrderable y)
		{
			if (x == null)
			{
				throw new ArgumentNullException(nameof(x));
			}
			else if (y == null)
			{
				throw new ArgumentNullException(nameof(y));
			}
			else
			{
				var result = CompareStatus(x, y, ProcessTaskStatusCodeList.Codes.Working);
				if (result == 0)
				{
					result = CompareStatus(x, y, ProcessTaskStatusCodeList.Codes.Suspended);
				}

				if (result == 0)
				{
					result = CompareReleaseSequence(x, y);
				}

				return result;
			}
		}

		static int CompareReleaseSequence(ITaskOrderable x, ITaskOrderable y)
		{
			var headerX = x.WorkflowOrderable;
			var headerY = y.WorkflowOrderable;

			if (headerX == headerY)
			{
				return 0;
			}
			else if (headerX.ReleaseSequence < headerY.ReleaseSequence)
			{
				return -1;
			}
			else if (headerY.ReleaseSequence < headerX.ReleaseSequence)
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
	}
}
