using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace CargoWise.EntityFramework
{
	internal delegate IComparer GetPropertyComparerDelegate(PropertyDescriptor property, ListSortDirection direction);

	internal class MultiPropertyComparer : IComparer
	{
		public MultiPropertyComparer(GetPropertyComparerDelegate getPropertyComparerDelegate, ListSortDescriptionCollection sortDescriptions)
		{
			this.getPropertyComparerDelegate = getPropertyComparerDelegate;
			this.SortDescriptions = sortDescriptions;
		}

		public readonly ListSortDescriptionCollection SortDescriptions;

		#region Equals / GetHashCode

		public override bool Equals(object obj)
		{
			MultiPropertyComparer rhs = obj as MultiPropertyComparer;
			return
				rhs != null &&
				ArrayEquals(SortDescriptions, rhs.SortDescriptions);
		}

		public override int GetHashCode()
		{
			return
				SortDescriptions.Count == 0 ?
				0 :
				SortDescriptions[0].PropertyDescriptor.GetHashCode();
		}

		static bool ArrayEquals(ListSortDescriptionCollection lhs, ListSortDescriptionCollection rhs)
		{
			if (lhs.Count != rhs.Count)
			{
				return false;
			}
			for (int i = 0; i < lhs.Count; i++)
			{
				if (lhs[i].PropertyDescriptor != rhs[i].PropertyDescriptor &&
					lhs[i].SortDirection != rhs[i].SortDirection)
				{
					return false;
				}
			}
			return true;
		}

		#endregion

		#region IComparer Members

		public int Compare(BusinessObject x, BusinessObject y)
		{
			int result = 0;
			foreach (PropertyComparer comparer in Comparers)
			{
				result = comparer.Compare(x, y);
				if (result != 0)
				{
					break;
				}
			}
			return result;
		}

		int IComparer.Compare(object x, object y)
		{
			return Compare((BusinessObject)x, (BusinessObject)y);
		}

		#endregion

		#region Implementation

		readonly GetPropertyComparerDelegate getPropertyComparerDelegate;

		IEnumerable<IComparer> Comparers
		{
			get
			{
				for (int i = 0; i < SortDescriptions.Count; i++)
				{
					yield return getPropertyComparerDelegate(SortDescriptions[i].PropertyDescriptor, SortDescriptions[i].SortDirection);
				}
			}
		}

		#endregion
	}
}
