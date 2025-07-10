#if DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.Common;

namespace CargoWise.ComponentModel.Testing
{
	public class KBindingList<T> : BindingList<T>, ITypedList, ICollectionAlwaysReturnElementsForBinding
	{
		protected virtual PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
		{
			return TypedListHelper.GetItemProperties(typeof(T), listAccessors, GetItemProperties);
		}

		protected virtual PropertyDescriptorCollection GetItemProperties(Type type)
		{
			return PropertyDescriptorCollectionWithWrappingProperties.FromType(type);
		}

		protected virtual string GetListName(PropertyDescriptor[] listAccessors)
		{
			return GetType().Name;
		}

		#region Sorting

		PropertyDescriptor sortProperty;
		ListSortDirection sortDirection;

		protected override bool SupportsSortingCore
		{
			get { return true; }
		}

		protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
		{
			ClearItems();
			List<T> list = new List<T>(Items);
			Items.Clear();
			list.Sort(delegate(T lhs, T rhs)
			{
				int result = Comparer.Default.Compare(prop.GetValue(lhs), prop.GetValue(rhs));
				if (direction == ListSortDirection.Descending)
				{
					result = -result;
				}
				return result;
			});
			foreach (T item in list)
			{
				Items.Add(item);
			}
			sortProperty = prop;
			sortDirection = direction;
		}

		protected override bool IsSortedCore
		{
			get { return sortProperty != null; }
		}

		protected override PropertyDescriptor SortPropertyCore
		{
			get { return sortProperty; }
		}

		protected override ListSortDirection SortDirectionCore
		{
			get { return sortDirection; }
		}

		#endregion

		#region ITypedList

		PropertyDescriptorCollection ITypedList.GetItemProperties(PropertyDescriptor[] listAccessors)
		{
			return GetItemProperties(listAccessors);
		}

		string ITypedList.GetListName(PropertyDescriptor[] listAccessors)
		{
			return GetListName(listAccessors);
		}

		#endregion

		#region ICollectionAlwaysReturnElementsForBinding Members

		int inAlwaysReturnElementsForBinding;

		IDisposable ICollectionAlwaysReturnElementsForBinding.AlwaysReturnElementsForBinding()
		{
			inAlwaysReturnElementsForBinding++;
			bool addedItem = Count == 0;
			T item = default(T);
			if (addedItem)
			{
				item = AddNew();
			}
			return new DisposableAction(delegate
			{
				inAlwaysReturnElementsForBinding--;
				if (addedItem)
				{
					Remove(item);
				}
			});
		}

		bool ICollectionAlwaysReturnElementsForBinding.InAlwaysReturnElementsForBinding
		{
			get { return inAlwaysReturnElementsForBinding > 0; }
		}

		#endregion
	}
}
#endif
