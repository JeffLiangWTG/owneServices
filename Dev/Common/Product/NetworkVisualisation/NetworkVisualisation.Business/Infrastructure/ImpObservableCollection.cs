using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.Business
{
	/// <summary>
	/// An implementation of observable collection that contains a duplicate internal
	/// list that is retained momentarily after the list is cleared.
	/// This is so that observers can undo events, etc on the list after it has been cleared and
	/// raised a CollectionChanged event with a Reset action.
	/// </summary>
	public class ImpObservableCollection<T> : ObservableCollection<T>, IObservableReloadableCollection<T>, ICloneable
	{
		protected AllSelectedEntities entityProviderFunction;
		bool inCollectionChangedEvent;

		#region Constructors

		public ImpObservableCollection()
		{
		}

		public ImpObservableCollection(IEnumerable<T> range)
		{
			AddRange(range);
		}

		public ImpObservableCollection(AllSelectedEntities rangeGetter)
			: this(rangeGetter())
		{
			this.entityProviderFunction = rangeGetter;
		}

		#endregion

		public void AddRange(IEnumerable<T> range)
		{
			using (Suspend(NotifyCollectionChangedAction.Add))
			{
				foreach (T item in range)
				{
					Add(item);
				}
			}
		}

		public void RemoveRange(IEnumerable<T> range)
		{
			using (Suspend(NotifyCollectionChangedAction.Remove))
			{
				foreach (T item in range)
				{
					Remove(item);
				}
			}
		}

		public void ResetForBinding()
		{
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			NetworkVisualisationErrorReporter.ReportIfAssertionFailed(!inCollectionChangedEvent);

			base.OnCollectionChanged(e);

			inCollectionChangedEvent = true;

			try
			{
				if (suspender != null && suspender.Action == e.Action)
				{
					suspender.AggregateEventsForSuspend(e);
				}
				else
				{
					NotifyCollectionChangedCore(e);
				}
			}
			finally
			{
				inCollectionChangedEvent = false;
			}
		}

		protected internal virtual void NotifyCollectionChangedCore(NotifyCollectionChangedEventArgs e)
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

		protected virtual void OnItemsAdded(ICollection items)
		{
			if (ItemsAdded != null)
			{
				ItemsAdded(this, new CollectionItemsChangedEventArgs(items));
			}
		}

		#region Suspend

		ImpObservableSuspender suspender;

		protected IDisposable Suspend(NotifyCollectionChangedAction action)
		{
			return new ImpObservableSuspender(this, action, false);
		}

		protected IDisposable Suppress(NotifyCollectionChangedAction action)
		{
			return new ImpObservableSuspender(this, action, true);
		}

		class ImpObservableSuspender : IDisposable
		{
			internal ImpObservableSuspender(ImpObservableCollection<T> collection, NotifyCollectionChangedAction action, bool suppressNotifyOnDispose)
			{
				this.collection = collection;

				previous = collection.suspender;
				collection.suspender = this;

				Action = action;
				modifiedItems = new List<T>();
				this.suppressNotifyOnDispose = suppressNotifyOnDispose;
			}

			readonly bool suppressNotifyOnDispose;
			bool disposedValue;
			readonly ImpObservableCollection<T> collection;
			readonly List<T> modifiedItems;
			readonly ImpObservableSuspender previous;

			internal NotifyCollectionChangedAction Action { get; private set; }

			protected virtual void Dispose(bool disposing)
			{
				if (!disposedValue)
				{
					if (disposing)
					{
						collection.suspender = previous;

						if (!suppressNotifyOnDispose)
						{
							collection.NotifyCollectionChangedCore(GetNotificationChangedEventArgs());
						}
					}

					disposedValue = true;
				}
			}

			public void Dispose()
			{
				Dispose(true);
			}

			internal void AggregateEventsForSuspend(NotifyCollectionChangedEventArgs e)
			{
				if (e.NewItems != null)
				{
					foreach (var item in e.NewItems)
					{
						modifiedItems.Add((T)item);
					}
				}
				if (e.OldItems != null)
				{
					foreach (var item in e.OldItems)
					{
						modifiedItems.Add((T)item);
					}
				}
			}

			NotifyCollectionChangedEventArgs GetNotificationChangedEventArgs()
			{
				return new NotifyCollectionChangedEventArgs(Action, modifiedItems);
			}
		}

		#endregion

		protected virtual void OnItemsRemoved(ICollection items)
		{
			if (ItemsRemoved != null)
			{
				ItemsRemoved(this, new CollectionItemsChangedEventArgs(items));
			}
		}

		/// <summary>
		/// Event raised when items have been added.
		/// </summary>
		public event EventHandler<CollectionItemsChangedEventArgs> ItemsAdded;

		/// <summary>
		/// Event raised when items have been removed.
		/// </summary>
		public event EventHandler<CollectionItemsChangedEventArgs> ItemsRemoved;

		public object Clone()
		{
			var clone = new ImpObservableCollection<T>();

			foreach (ICloneable obj in this)
			{
				clone.Add((T)obj.Clone());
			}

			return clone;
		}

		#region IObservableReloadableCollection Members

		public void Reload()
		{
			Reload(entityProviderFunction);
		}

		public delegate IEnumerable<T> AllSelectedEntities();

		protected void Reload(AllSelectedEntities getNewEntities)
		{
			ClearEntitiesForReload();
			OnReloading(getNewEntities);

			if (Reloaded != null)
			{
				Reloaded(this, EventArgs.Empty);
			}

			ResetForBinding();
		}

		protected virtual void ClearEntitiesForReload()
		{
			OnItemsRemoved(this);
			ClearItems();
		}

		protected virtual void OnReloading(AllSelectedEntities getNewEntities)
		{
			if (getNewEntities != null)
			{
				AddRange(getNewEntities());
			}
		}

		public event EventHandler Reloaded;

		#endregion
	}
}
