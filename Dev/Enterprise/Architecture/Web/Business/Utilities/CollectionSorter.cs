using System;
using System.Collections;
using System.ComponentModel;

namespace Enterprise.ZArchitecture.Web.Business.Utilities
{
	/// <summary>
	/// Summary description for TrackingSorter.
	/// </summary>
	public class WebCollectionSorter : IComparer
	{
		protected string fSortProperty;

		protected ListSortDirection fDirection;

		public WebCollectionSorter(string sortProperty)
		{
			fSortProperty = sortProperty;
		}

		public WebCollectionSorter(string sortProperty, ListSortDirection direction)
		{
			fSortProperty = sortProperty;
			fDirection = direction;
		}

		public override bool Equals(object obj)
		{
			WebCollectionSorter rhs = obj as WebCollectionSorter;
			return
				rhs != null &&
				rhs.fDirection == fDirection &&
				rhs.fSortProperty == fSortProperty;
		}

		public override int GetHashCode()
		{
			return fSortProperty.GetHashCode();
		}

		#region Properties

		public string SortProperty
		{
			get { return fSortProperty; }
		}

		public ListSortDirection Direction
		{
			get { return fDirection; }
		}

		#endregion Properties

		#region IComparer Members

		public virtual int Compare(object x, object y)
		{
			IComparable sortObjX = x != null ? ZPropertyAccessor.Get(x, SortProperty) as IComparable : null;
			IComparable sortObjY = y != null ? ZPropertyAccessor.Get(y, SortProperty) as IComparable : null;
			return CompareCore(sortObjX, sortObjY);
		}

		protected int CompareCore(IComparable sortObjX, IComparable sortObjY)
		{
			int result = 0;

			if (sortObjX != null && sortObjY != null)
			{
				if (Direction == ListSortDirection.Ascending)
				{
					result = sortObjX.CompareTo(sortObjY);
				}
				else
				{
					result = sortObjY.CompareTo(sortObjX);
				}
			}
			else if (sortObjX == null && sortObjY != null)
			{
				result = Direction == ListSortDirection.Ascending ? -1 : 1;
			}
			else if (sortObjX != null && sortObjY == null)
			{
				result = Direction == ListSortDirection.Ascending ? 1 : -1;
			}
			return result;
		}

		#endregion
	}
}
