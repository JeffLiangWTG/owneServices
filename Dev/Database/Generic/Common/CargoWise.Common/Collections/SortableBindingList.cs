using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace CargoWise.Common.Collections
{
	public class SortableBindingList<T> : BindingList<T>
	{
		ListSortDirection sortDirection;
		bool isSorted;
		PropertyDescriptor sortProperty;

		#region BindingList members
		protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
		{
			try
			{
				var type = typeof(T);
				var getter = GetPropertyGetter(prop, type);
				var listToSort = GetListToSort();
				var comparer = new ZComparer<T>((first, second) =>
									(direction == ListSortDirection.Ascending ? 1 : -1) *
									Comparer.Default.Compare(getter.GetValue(first), getter.GetValue(second)));
				var changes = listToSort.StableSortWithReorderCheck(comparer);

				sortDirection = direction;
				isSorted = true;
				sortProperty = prop;
				if (changes)
				{
					OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
				}
			}
			catch
			{
				isSorted = false;
				sortProperty = null;
				throw;
			}
		}

		protected override ListSortDirection SortDirectionCore => sortDirection;

		protected override bool SupportsSortingCore => true;

		protected override bool IsSortedCore => isSorted;

		protected override PropertyDescriptor SortPropertyCore => sortProperty;

		#endregion

		[SuppressMessage("Microsoft.Globalization", "CA1305")]
		List<T> GetListToSort()
		{
			var listToSort = Items as List<T>
				?? throw new InvalidCastException($"BindingList Items property is not of type {typeof(List<T>)}");

			return listToSort;
		}
		[SuppressMessage("Microsoft.Globalization", "CA1305")]
		static PropertyInfo GetPropertyGetter(PropertyDescriptor property, Type type)
		{
			Argument.NotNull(type, nameof(type));

			var getter = type.GetProperty(property.Name)
				?? throw new ArgumentException("Invalid property name");
			if (getter.PropertyType.GetInterface(nameof(IComparable)) == null)
			{
				throw new ArgumentException($"Sort property must be of a type that implements {nameof(IComparable)}");
			}

			return getter;
		}
	}
}
