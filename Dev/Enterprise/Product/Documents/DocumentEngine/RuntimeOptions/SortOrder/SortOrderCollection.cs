using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	[Serializable]
	public class SortOrderCollection : NonPersistentBusinessObject, IEnumerable<IBindableBooleanItem>, IObsoleteValidation
	{
		readonly List<SortOrder> SortOrders = new List<SortOrder>();

		IEnumerator<IBindableBooleanItem> IEnumerable<IBindableBooleanItem>.GetEnumerator()
		{
			foreach (SortOrder element in SortOrders)
			{
				yield return element;
			}
		}

		public SortOrder SelectedOrder
		{
			get
			{
				foreach (SortOrder s in this)
				{
					if (s.Selected)
					{
						return s;
					}
				}

				return new SortOrder("", "NULL");
			}
			set
			{
				for (int i = 0; i < Count; i++)
				{
					this[i].Selected = this[i] == value;
				}
				if (SelectedOrder == null)
				{
					throw new InvalidOperationException("There is no such sortorders in the collection.");
				}
			}
		}

		public SortOrder Add(string displayName, string fieldList)
		{
			return Add(new SortOrder(displayName, fieldList));
		}

		public SortOrder Add(SortOrder sortOrder)
		{
			RegisterEditableChildObject(sortOrder);
			SortOrders.Add(sortOrder);
			return sortOrder;
		}

		public int Count
		{
			get
			{
				return SortOrders.Count;
			}
		}

		public SortOrder this[int index]
		{
			get
			{
				return SortOrders[index];
			}
			internal set
			{
				UnRegisterEditableChildObject(SortOrders[index]);
				SortOrders[index] = value;
				RegisterEditableChildObject(SortOrders[index]);
			}
		}

		public new SortOrder this[string displayName]
		{
			get
			{
				foreach (SortOrder sortOrder in this)
				{
					if (sortOrder.DisplayName == displayName)
					{
						return sortOrder;
					}
				}
				return null;
			}
		}

		public void Clear()
		{
			foreach (SortOrder sortOrder in SortOrders)
			{
				UnRegisterEditableChildObject(sortOrder);
			}
			SortOrders.Clear();
		}

		public SortOrder DefaultOrder { get; private set; }

		public void UpdateDefaultOrder()
		{
			foreach (SortOrder sortOrder in this)
			{
				if (sortOrder.Selected)
				{
					DefaultOrder = sortOrder;
					break;
				}
			}
		}
	}
}
