using System;
using System.ComponentModel;
using CargoWise.Common;

namespace CargoWise.EntityFramework
{
	internal class BusinessObjectCollectionSorter
	{
		public BusinessObjectCollectionSorter(IBusinessObjectCollection collection)
		{
			if (collection == null)
			{
				throw new ArgumentNullException(nameof(collection), "Collection must not be null.");
			}
			this.Collection = collection;
		}

		public bool IsSorted => sortState != SortState.Unsorted; // Historically, we are setting state to "Sorted" before sort has actually completed.

		public ListSortDirection SortDirection
		{
			get { return fSortDirection; }
		}

		public bool IsSorting => sortState == SortState.Sorting;

		public PropertyDescriptor SortProperty
		{
			get { return fSortProperty; }
			private set
			{
				if (TestPropertyIsIComparable(value))
				{
					fSortProperty = value;
				}
			}
		}

		bool TestPropertyIsIComparable(PropertyDescriptor value)
		{
			if (value != null
					&& !typeof(IComparable).IsAssignableFrom(value.PropertyType))
			{
				ErrorReporter.ReportOnce("UnComparableSortProperty", string.Format(
					"Attempted to assign to SortProperty a PropertyDescriptor of PropertyType that is not IComparable.\r\nComponentType = {0}, Category = {1}, Description = {2}, DisplayName = {3}, Name = {4}, PropertyType = {5}"
					, value.ComponentType.ToString(), value.Category, value.Description, value.DisplayName, value.Name, value.PropertyType.ToString()));
				return false;
			}
			return true;
		}

		public void RemoveSort()
		{
			SortProperty = null;
			sortState = SortState.Unsorted;
			((IBusinessObjectCollectionInternals)Collection).FireListResetEvent();
		}

		public void ApplySort(PropertyDescriptor property, ListSortDirection direction)
		{
			SortProperty = property;
			fSortDirection = direction;
			if (SortProperty != null)
			{
				sortState = SortState.Sorting;
				try
				{
					Collection.Elements.ApplySort(Collection.GetComparerForSort(SortProperty, direction));
					((IBusinessObjectCollectionInternals)Collection).FireListResetEvent();
				}
				finally
				{
					sortState = SortState.Sorted;
				}
			}
		}

		public void Sort(string propertyName, ListSortDirection direction)
		{
			PropertyDescriptorCollection descriptorCollection;
			if (Collection is IDynamicBusinessObjectCollection && Collection is ITypedList)
			{
				descriptorCollection = ((ITypedList)Collection).GetItemProperties(null);
			}
			else
			{
				descriptorCollection = ZCustomTypeDescriptor.GetProperties(Collection.TypeOfElements);
			}

			PropertyDescriptor propertyDesc = descriptorCollection[propertyName];
			if (propertyDesc == null)
			{
				if (Collection.Count > 0)
				{
					throw new ApplicationException("Could not find property to sort: " + propertyName);
				}
			}
			else if (TestPropertyIsIComparable(propertyDesc))
			{
				Collection.ApplySort(propertyDesc, direction);
			}
		}

		public void Resort()
		{
			if (SortDescriptions != null && Collection is IBindingListView view)
			{
				view.ApplySort(SortDescriptions);
			}
			else
			{
				Collection.ApplySort(SortProperty, SortDirection);
			}
		}

		public SortInfo SortInformation
		{
			get { return IsSorted ? new SortInfo(SortProperty.Name, SortDirection) : null; }
		}

		readonly IBusinessObjectCollection Collection;
		SortState sortState;
		ListSortDirection fSortDirection = ListSortDirection.Ascending;
		PropertyDescriptor fSortProperty;

		#region SortDescriptions

		public void ApplySorts(ListSortDescriptionCollection sortDescriptions, EntityFramework.SortState state)
		{
			SortDescriptions = sortDescriptions;
			foreach (ListSortDescription sortDescription in sortDescriptions)
			{
				ApplySort(sortDescription.PropertyDescriptor, sortDescription.SortDirection);
				state.ApplySort(sortDescription.PropertyDescriptor, sortDescription.SortDirection);
			}
		}

		public void RemoveSorts()
		{
			SortDescriptions = null;
		}

		public ListSortDescriptionCollection SortDescriptions
		{
			get { return sortDescriptions; }
			private set
			{
				sortDescriptions = value;
			}
		}

		ListSortDescriptionCollection sortDescriptions;

		#endregion

		enum SortState
		{
			Unsorted,
			Sorting,
			Sorted
		}
	}
}
