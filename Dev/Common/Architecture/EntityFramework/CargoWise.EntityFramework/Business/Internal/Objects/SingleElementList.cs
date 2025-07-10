using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;

namespace CargoWise.EntityFramework
{
	public interface ISingleElementListInternal
	{
		bool IsListChangeSuspended
		{
			get;
		}
		IDisposable SuspendListChanged();
	}

	/// <summary>
	/// SingleElementList implements IBindingList. This means it gets a CurrencyManager
	/// rather than a PropertyManager at bind time.
	/// This class was formerly SingleElementList - now all partial classes to reduce memory footprint of reflection
	/// </summary>
	public abstract partial class BusinessObject : ZCustomTypeDescriptor, IBindingList, ISingleElementListInternal
	{
		#region Implementation

#if DEBUG
		internal bool isRefreshed_Debug;
#endif

		protected internal virtual void OnElementChanged()
		{
#if DEBUG
			isRefreshed_Debug = true;
#endif
			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, 0));
		}

		protected internal virtual void OnElementReset()
		{
			OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
		}

		internal event ListChangedEventHandler ListChangedInternal;

		public bool IsBound => (ListChangedInternal?.GetInvocationList().Any(i => i.Target.IsCurrencyManager()
			|| (i.Target is IBindingTracked bindingTracked && bindingTracked.IsBound)) ?? false);

		internal void OnListChanged(ListChangedEventArgs e)
		{
			if (ListChangedInternal != null && !IsListChangeSuspended)
			{
				try
				{
					ListChangedInternal(this, e);
				}
				catch (NullReferenceException) { }
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (string.IsNullOrEmpty(ex.Source))
					{
						ex.Source = GetType().FullName;
					}
					else
					{
						ex.Source += ", " + GetType().FullName;
					}
					throw;
				}
			}
		}

		#endregion

		#region IList Members

		bool IList.IsReadOnly
		{
			get { return false; }
		}

		object IList.this[int index]
		{
			get { return this; }
			set { throw new NotSupportedException(); }
		}

		void IList.RemoveAt(int index)
		{
			throw new NotSupportedException();
		}

		void IList.Insert(int index, object value)
		{
			throw new NotSupportedException();
		}

		void IList.Remove(object value)
		{
			throw new NotSupportedException();
		}

		bool IList.Contains(object value)
		{
			throw new NotSupportedException();
		}

		void IList.Clear()
		{
			throw new NotSupportedException();
		}

		int IList.IndexOf(object value)
		{
			throw new NotSupportedException();
		}

		int IList.Add(object value)
		{
			throw new NotSupportedException();
		}

		bool IList.IsFixedSize
		{
			get { return true; }
		}

		#endregion

		#region ICollection Members

		bool ICollection.IsSynchronized
		{
			get { throw new NotSupportedException(); }
		}

		int ICollection.Count
		{
			get { return 1; }
		}

		void ICollection.CopyTo(Array array, int index)
		{
			array.SetValue(this, 0);
		}

		object ICollection.SyncRoot
		{
			get { return this; }
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			yield return this;
		}

		#endregion

		#region IBindingList Members

		event ListChangedEventHandler IBindingList.ListChanged
		{
			add { ListChangedInternal += value; }
			remove { ListChangedInternal -= value; }
		}

		void IBindingList.AddIndex(PropertyDescriptor property)
		{
			throw new NotSupportedException("");
		}

		bool IBindingList.AllowNew
		{
			get { return false; }
		}

		void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction)
		{
			throw new NotSupportedException("");
		}

		PropertyDescriptor IBindingList.SortProperty
		{
			get { throw new NotSupportedException(""); }
		}

		int IBindingList.Find(PropertyDescriptor property, object key)
		{
			throw new NotSupportedException("");
		}

		bool IBindingList.SupportsSorting
		{
			get { return false; }
		}

		bool IBindingList.IsSorted
		{
			get { return false; }
		}

		bool IBindingList.AllowRemove
		{
			get { return false; }
		}

		bool IBindingList.SupportsSearching
		{
			get { return false; }
		}

		ListSortDirection IBindingList.SortDirection
		{
			get { throw new NotSupportedException(""); }
		}

		bool IBindingList.SupportsChangeNotification
		{
			get { return true; }
		}

		void IBindingList.RemoveSort()
		{
			throw new NotSupportedException("");
		}

		object IBindingList.AddNew()
		{
			throw new NotSupportedException("");
		}

		bool IBindingList.AllowEdit
		{
			get { return true; }
		}

		void IBindingList.RemoveIndex(PropertyDescriptor property)
		{
			throw new NotSupportedException("");
		}

		#endregion

		#region ISingleElementListInternal Members

		internal bool IsListChangeSuspended
		{
			get { return ListChangedSemaphore > 0; }
		}

		bool ISingleElementListInternal.IsListChangeSuspended
		{
			get { return IsListChangeSuspended; }
		}

		internal IDisposable SuspendListChanged()
		{
			return new ListChangedSuspender(this);
		}

		sealed class ListChangedSuspender : IDisposable
		{
			public ListChangedSuspender(BusinessObject singleElementList)
			{
				this.singleElementList = singleElementList;
				NumericUtil.IncreaseIndexSafe(ref singleElementList.ListChangedSemaphore);
				CargoWise.Common.Testing.DisposableLeakListener.Instance.RegisterDisposable(this);
			}

			public void Dispose()
			{
				if (!isDisposed)
				{
					NumericUtil.DecreaseIndexSafe(ref singleElementList.ListChangedSemaphore);
					isDisposed = true;
					CargoWise.Common.Testing.DisposableLeakListener.Instance.UnRegisterDisposable(this);
				}
			}
			bool isDisposed;

			readonly BusinessObject singleElementList;
		}

		IDisposable ISingleElementListInternal.SuspendListChanged()
		{
			return SuspendListChanged();
		}

		byte ListChangedSemaphore;

		#endregion
	}
}
