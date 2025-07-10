using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace CargoWise.NetworkVisualisation.Business
{
	/// <summary>
	/// ImpObservableSet is like ImpObservableCollection, but it guarantees:
	/// 1) Any items added or removed do not raise additional add/remove events when the collection is reloaded.
	/// 2) All elements in the collection are unique.
	/// </summary>
	public class ImpObservableSet<T> : ImpObservableCollection<T>
		where T : IEquatable<T>
	{
		HashSet<T> inner = new HashSet<T>();
		bool ignoreDuplicationCheckWithinSafeOperation;

		public ImpObservableSet()
		{
		}

		public ImpObservableSet(AllSelectedEntities rangeGetter)
			: base(rangeGetter)
		{
		}

		public ImpObservableSet(IEnumerable<T> range)
			: base(range)
		{
		}

		#region ObservableCollection Overrides

		protected override void InsertItem(int index, T item)
		{
			if (ignoreDuplicationCheckWithinSafeOperation || inner.Add(item))
			{
				base.InsertItem(index, item);
			}
		}

		protected override void RemoveItem(int index)
		{
			if (ignoreDuplicationCheckWithinSafeOperation || inner.Remove(this[index]))
			{
				base.RemoveItem(index);
			}
		}

		protected override void SetItem(int index, T item)
		{
			throw new InvalidOperationException("Setting the index of an item is meaningless in a set.");
		}

		protected override void ClearItems()
		{
			base.ClearItems();
			inner.Clear();
		}

		#endregion

		#region Reload override

		protected internal override void NotifyCollectionChangedCore(NotifyCollectionChangedEventArgs e)
		{
			if (e.OldItems != null)
			{
				OnItemsRemoved(e.OldItems);
			}

			if (e.NewItems != null)
			{
				OnItemsAdded(e.NewItems);
			}
		}

		protected override void ClearEntitiesForReload()
		{
			// Do nothing.
			// It's faster to track entities that are added/removed and only raise events accordingly.
		}

		protected override void OnReloading(AllSelectedEntities getNewEntities)
		{
			var newInner = new HashSet<T>();
			var elementsAdded = new List<T>();

			if (getNewEntities != null)
			{
				foreach (var item in getNewEntities())
				{
					if (newInner.Add(item) && !inner.Contains(item))
					{
						elementsAdded.Add(item);
					}
				}
			}

			base.ClearItems(); // Call base directly to avoid clearing inner collection.

			using (Suppress(NotifyCollectionChangedAction.Add))
			{
				try
				{
					ignoreDuplicationCheckWithinSafeOperation = true;
					foreach (T item in newInner)
					{
						Add(item);
					}
				}
				finally
				{
					ignoreDuplicationCheckWithinSafeOperation = false;
				}
			}

			var oldElements = inner;
			oldElements.ExceptWith(newInner);
			var elementsRemoved = oldElements.ToList();

			inner = newInner; // Replace the old set to remove references to old elements.
			OnItemsRemoved(elementsRemoved);
			OnItemsAdded(elementsAdded);
		}

		/// <summary>
		/// Update has performance advantages over reload when the collection gets very big.
		/// </summary>
		public void Update()
		{
			if (entityProviderFunction != null)
			{
				var elementsAdded = new HashSet<T>();

				using (Suspend(NotifyCollectionChangedAction.Add))
				{
					foreach (T item in entityProviderFunction())
					{
						Add(item);
						elementsAdded.Add(item);
					}
				}

				var oldInner = inner;
				oldInner.ExceptWith(elementsAdded);

				using (Suspend(NotifyCollectionChangedAction.Remove))
				{
					try
					{
						ignoreDuplicationCheckWithinSafeOperation = true;
						RemoveRange(oldInner);
					}
					finally
					{
						ignoreDuplicationCheckWithinSafeOperation = false;
					}
				}

				inner = elementsAdded;
			}
			else
			{
				throw new InvalidOperationException("No provider function.");
			}
		}

		#endregion
	}
}
