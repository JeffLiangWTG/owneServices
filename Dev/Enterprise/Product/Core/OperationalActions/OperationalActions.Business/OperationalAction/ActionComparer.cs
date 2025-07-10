using System.Collections;
using System.Collections.Generic;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class ActionComparer : IComparer<OperationalAction>, IComparer
	{
		public ActionComparer()
			: this(false) { }

		public ActionComparer(bool invert)
		{
			this.invert = invert;
		}

		public static int Compare(OperationalAction x, OperationalAction y)
		{
			int result = MenuPathComparer.ComparePath(x.SU_MenuPathMultilingual, y.SU_MenuPathMultilingual);

			if (result == 0)
			{
				result = x.SU_MenuIndex.CompareTo(y.SU_MenuIndex);
				if (result == 0)
				{
					result = x.SU_MenuName.CompareTo(y.SU_MenuName);
				}
			}

			return result;
		}

		public static int Compare(OperationalAction x, OperationalAction y, bool invert)
		{
			int result = Compare(x, y);
			return invert ? -result : result;
		}

		#region IComparer<OperationalAction> Members

		int IComparer<OperationalAction>.Compare(OperationalAction x, OperationalAction y)
		{
			return Compare(x, y, invert);
		}

		#endregion

		#region IComparer Members

		int IComparer.Compare(object x, object y)
		{
			return Compare((OperationalAction)x, (OperationalAction)y, invert);
		}

		#endregion

		readonly bool invert;
	}
}
