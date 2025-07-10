using System;
using System.ComponentModel;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.Business.Utilities
{
	public class ActualEstimateCollectionSorter : WebCollectionSorter
	{
		public ActualEstimateCollectionSorter(string actualProperty, string estimatedProperty) : base("")
		{
			fActualProperty = actualProperty;
			fEstimatedProperty = estimatedProperty;
		}

		public ActualEstimateCollectionSorter(string actualProperty, string estimatedProperty, ListSortDirection direction)
			: this(actualProperty, estimatedProperty)
		{
			fDirection = direction;
		}

		public string ActualProperty
		{
			get { return fActualProperty; }
		}

		readonly string fActualProperty;

		public string EstimatedProperty
		{
			get { return fEstimatedProperty; }
		}

		readonly string fEstimatedProperty;

		#region IComparer Members

		public override int Compare(object x, object y)
		{
			IComparable sortObjX = x != null ? ZPropertyAccessor.Get(x, ActualProperty) as IComparable : null;
			IComparable sortObjY = y != null ? ZPropertyAccessor.Get(y, ActualProperty) as IComparable : null;

			IZType sortPropX = x != null ? ZPropertyAccessor.Get(x, ActualProperty) as IZType : null;
			if (sortPropX == null || sortPropX.IsEmpty)
			{
				sortObjX = x != null ? ZPropertyAccessor.Get(x, EstimatedProperty) as IComparable : null;
			}

			IZType sortPropY = y != null ? ZPropertyAccessor.Get(y, ActualProperty) as IZType : null;
			if (sortPropY == null || sortPropY.IsEmpty)
			{
				sortObjY = y != null ? ZPropertyAccessor.Get(y, EstimatedProperty) as IComparable : null;
			}
			return CompareCore(sortObjX, sortObjY);
		}
		#endregion

	}
}
