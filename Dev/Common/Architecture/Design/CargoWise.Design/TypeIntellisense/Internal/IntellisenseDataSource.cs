using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Common.Testing;

namespace CargoWise.Design.TypeIntellisense
{
	/// <summary>
	/// A list of namespace or type items that the user can select from a partial type path.
	/// </summary>
	internal class IntellisenseDataSource : Component, IBindingList
	{
		public IntellisenseDataSource(IEnumerable<Type> types, SynchronizationContext syncContext)
			: this(types, syncContext, Array.Empty<string>())
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "internal purpose string")]
		public IntellisenseDataSource(IEnumerable<Type> types, SynchronizationContext syncContext, string[] usingNamespaces)
		{
			this.types = types;
			UISyncContext = syncContext;
			UsingNamespaces = usingNamespaces;

			DataColumn column = new DataColumn("Item", typeof(IntellisenseDataSourceItem));
			DataColumn nameColumn = new DataColumn("DisplayName", typeof(string));
			list.Table.Columns.Add(column);
			list.Table.Columns.Add(nameColumn);
			list.Sort = nameColumn.ColumnName;

			list.ListChanged += new ListChangedEventHandler(DataViewList_ListChanged);
			PartialNameAsync = "";

			DisposableLeakListener.Instance.RegisterDisposable(this);
		}

		/// <summary>
		/// Get the namespaces that have been specified by using statements in user code.
		/// </summary>
		public string[] UsingNamespaces { get; private set; }

		/// <summary>
		/// Get the current path to the names in the list.
		/// </summary>
		public string NamePath
		{ get { return namePath; } }

		/// <summary>
		/// Get or set the partial name that has been entered by the user. Populates the list synchronously.
		/// </summary>
		public string PartialName
		{
			get { return partialName; }
			set { SetPartialName(value, false); }
		}
		string partialName;

		/// <summary>
		/// When the PartialName property has changed.
		/// </summary>
		public event EventHandler PartialNameChanged;

		/// <summary>
		/// Get or set the partial name that has been entered by the user. Populates the list asynchronously.
		/// </summary>
		public string PartialNameAsync
		{
			get { CheckNotDisposed(); return partialName; }
			set { CheckNotDisposed(); SetPartialName(value, true); }
		}

		/// <summary>
		/// Get the right-most partial name, for example System.Wind returns 'Wind'.
		/// </summary>
		public string RightMostPartialName { get; private set; }

		/// <summary>
		/// Get the widest display name. Use this to size the list box.
		/// </summary>
		public string WidestDisplayName { get; private set; }

		void SetWidestDisplayName(string widestDisplayName)
		{
			CheckNotDisposed();
			WidestDisplayName = widestDisplayName;
			if (WidestDisplayNameChanged != null)
			{
				Invoke(delegate
{
	if (!disposed && WidestDisplayNameChanged != null)
	{
		WidestDisplayNameChanged(this, EventArgs.Empty);
	}
});
			}
		}

		/// <summary>
		/// When WidestDisplayName has changed.
		/// </summary>
		public event EventHandler WidestDisplayNameChanged;

		/// <summary>
		/// Is the data source fully populated yet?
		/// </summary>
		public bool FullyPopulated
		{ get { return fullyPopulated; } }
		bool fullyPopulated;

		void UpdateFullyPopulated(bool value)
		{
			fullyPopulated = value;
			OnFullyPopulatedChanged(EventArgs.Empty);
		}

		protected virtual void OnFullyPopulatedChanged(EventArgs e)
		{
			FullyPopulatedChanged?.Invoke(this, e);
		}

		/// <summary>
		/// When the FullyPopulated property has changed. This event is fired on a different thread.
		/// </summary>
		public event EventHandler FullyPopulatedChanged;

		/// <summary>
		/// Find an item match that is close to the current PartialName.
		/// </summary>
		public int FindNearestMatch()
		{ return FindNearestMatch(RightMostPartialName); }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "internal purpose string")]
		int FindNearestMatch(string name)
		{
			CheckNotDisposed();

			int result = -1;
			if (Count > 0)
			{
				if (string.IsNullOrEmpty(name))
				{
					result = 0;
				}
				else
				{
					string expr = "DisplayName like " + DataTableUtil.GetRowValueAsTokenisedAdoFilterStringValue_ForLikeStartsWith(name);
					DataRow[] rows = list.Table.Select(expr);

					result = (rows.Length == 0) ? -1 : list.Find(rows[0]["DisplayName"]);
					if (result == -1)
					{
						result = FindNearestMatch(name.Substring(0, name.Length - 1));
					}
				}
			}
			return result;
		}

		public event ListChangedEventHandler BeforeListChanged;
		public event ListChangedEventHandler ListChanged;
		public event ListChangedEventHandler AfterListChanged;

		public IntellisenseDataSourceItem this[int i]
		{ get { return (IntellisenseDataSourceItem)list[i][0]; } }

		#region IList Members

		public bool IsReadOnly
		{ get { return true; } }

		object IList.this[int index]
		{
			get { return this[index]; }
			set { throw new NotSupportedException(); }
		}

		public void RemoveAt(int index)
		{ throw new NotSupportedException(); }

		public void Insert(int index, object value)
		{ throw new NotSupportedException(); }

		public void Remove(object value)
		{ throw new NotSupportedException(); }

		bool IList.Contains(object value)
		{ throw new NotSupportedException(); }

		public void Clear()
		{ throw new NotSupportedException(); }

		public int IndexOf(object value)
		{ throw new NotSupportedException(); }

		public int Add(object value)
		{ throw new NotSupportedException(); }

		public bool IsFixedSize
		{ get { return false; } }

		#endregion

		#region ICollection Members

		public bool IsSynchronized
		{ get { return false; } }

		public int Count
		{ get { return list.Count; } }

		public void CopyTo(Array array, int index)
		{ throw new NotSupportedException(); }

		public object SyncRoot
		{ get { return null; } }

		#endregion

		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			foreach (DataRowView item in list)
			{
				yield return item[0];
			}
		}

		#endregion

		#region IBindingList Members

		public void AddIndex(PropertyDescriptor property)
		{ throw new NotSupportedException(); }

		public bool AllowNew
		{ get { return false; } }

		public void ApplySort(PropertyDescriptor property, ListSortDirection direction)
		{ throw new NotSupportedException(); }

		public PropertyDescriptor SortProperty
		{ get { throw new NotSupportedException(); } }

		public int Find(PropertyDescriptor property, object key)
		{ throw new NotSupportedException(); }

		public bool SupportsSorting
		{ get { return false; } }

		public bool IsSorted
		{ get { return false; } }

		public bool AllowRemove
		{ get { return false; } }

		public bool SupportsSearching
		{ get { return false; } }

		public ListSortDirection SortDirection
		{ get { throw new NotSupportedException(); } }

		public bool SupportsChangeNotification
		{ get { return true; } }

		public void RemoveSort()
		{ throw new NotSupportedException(); }

		public object AddNew()
		{ throw new NotSupportedException(); }

		public bool AllowEdit
		{ get { return false; } }

		public void RemoveIndex(PropertyDescriptor property)
		{ throw new NotSupportedException(); }

		#endregion

		#region IDisposable Members

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule", Justification = "Development tools")]
		protected override void Dispose(bool isNotFinalising)
		{
			base.Dispose(isNotFinalising);
			lock (disposeMutex)
			{
				disposing = true;
				populateEvent.Set();
				if (isNotFinalising)
				{
					using (EnumeratorInterruptedException.InterruptEnumeratorOnThread(thread))
					{
						for (int i = 0; i < 100; i++)
						{
							populateEvent.Set();
							Application.DoEvents();
							if (thread == null || !thread.IsAlive)
							{
								break;
							}
							Thread.Sleep(0);
						}
					}
					list.Table.Clear();
					DisposableLeakListener.Instance.UnRegisterDisposable(this);
				}
				disposed = true;
			}
		}

		readonly object disposeMutex = new object();

		#endregion

		#region Implementation

		readonly IEnumerable<Type> types;
		readonly SynchronizationContext UISyncContext;
		readonly DataView list = new DataTable().DefaultView;

		string namePath;
		Thread thread;
		bool stopPopulating;
		bool isPopulating;
		readonly AutoResetEvent populateEvent = new AutoResetEvent(false);
		bool disposing;
		bool disposed;

		void SetPartialName(string value, bool async)
		{
			string oldTypeNamePath = namePath;

			partialName = value;
			namePath = "";
			RightMostPartialName = "";
			int lastDotIndex = value.LastIndexOf('.');
			if (lastDotIndex != -1)
			{
				namePath = value.Substring(0, lastDotIndex);
				RightMostPartialName = value.Substring(lastDotIndex + 1);
			}
			else
			{
				RightMostPartialName = value;
			}

			if (oldTypeNamePath != namePath)
			{
				PopulateAsync();
			}
			if (!async)
			{
				WaitForPopulateToComplete();
			}
			PartialNameChanged?.Invoke(this, EventArgs.Empty);
		}

		readonly object startThreadMutex = new object();
		void PopulateAsync()
		{
			CheckNotDisposed();
			lock (startThreadMutex)
			{
				if (thread == null)
				{
					thread = new Thread(new ThreadStart(OnPopulateThreadStart));
					thread.Priority = ThreadPriority.BelowNormal;
					thread.Start();
				}
			}
			isPopulating = true;
			stopPopulating = true;
			populateEvent.Set();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule", Justification = "Baseline issue")]
		void WaitForPopulateToComplete()
		{
			while (isPopulating && thread != null && thread.IsAlive)
			{
				Application.DoEvents();
				Thread.Sleep(0);
			}
			Thread.Sleep(1);
			isPopulating = false;
		}

		void OnPopulateThreadStart()
		{
			try
			{
				while (!disposing && !disposed)
				{
					populateEvent.WaitOne();
					stopPopulating = false;
					if (!disposing && !disposed)
					{
						Populate();
					}
				}
			}
			catch (ObjectDisposedException)
			{
			}
			catch (Exception ex) when (this.IsDesignMode())
			{
				Invoke(delegate
				{
					ErrorReporter.ReportOnce(ex.Message, ex);
				});
			}
		}

		/// <summary>
		/// Populate this list with the intellisense items. You can call this on another thread while this
		/// list is bound to a user interface so it updates asyncronously.
		/// </summary>
		void Populate()
		{
			UpdateFullyPopulated(false);
			try
			{
				SetWidestDisplayName("");
				Invoke(delegate
				{ list.Table.Clear(); });

				ArrayList usingNSPrefixes = new ArrayList();
				usingNSPrefixes.Add("");
				foreach (string usingNS in UsingNamespaces)
				{
					usingNSPrefixes.Add(usingNS + ".");
				}

				Hashtable addedItems = new Hashtable();
				foreach (string usingNS in usingNSPrefixes)
				{
					if (!disposing)
					{
						using (EnumeratorInterruptedException.AllowEnumerationInterruptionOnCurrentThread())
						{
							try
							{
								AddTypesInUsingNSPrefix(addedItems, usingNS);
							}
							catch (EnumeratorInterruptedException)
							{
							}
						}
					}
				}
			}
			finally
			{
				stopPopulating = false;
				isPopulating = false;
				UpdateFullyPopulated(true);
			}
		}

		void AddTypesInUsingNSPrefix(Hashtable addedItemsHash, string usingNS)
		{
			foreach (Type type in types)
			{
				if (disposing || stopPopulating)
				{
					break;
				}
				string typeNameIncludingNested = type.FullName.Replace("+", ".");
				IntellisenseDataSourceItem item = BuildItemIfStartsWithUsingNS(usingNS, typeNameIncludingNested, type);
				InvokeAddIfNotContains(addedItemsHash, item);
			}
		}

		IntellisenseDataSourceItem BuildItemIfStartsWithUsingNS(string usingNSPrefix, string typeNameIncludingNested, Type type)
		{
			IntellisenseDataSourceItem result = null;
			if (typeNameIncludingNested.StartsWith(usingNSPrefix, StringComparison.OrdinalIgnoreCase))
			{
				string typeNameWithoutUsingNS = typeNameIncludingNested.Substring(usingNSPrefix.Length);
				if (namePath.Length == 0 ||
						typeNameWithoutUsingNS.StartsWith(namePath + ".", StringComparison.OrdinalIgnoreCase))
				{
					string nameToAdd = typeNameWithoutUsingNS.Substring(
							namePath.Length == 0 ? 0 : namePath.Length + 1);
					int firstDotIndex = nameToAdd.IndexOf('.');
					bool isNS = false;
					if (firstDotIndex != -1)
					{
						nameToAdd = nameToAdd.Substring(0, firstDotIndex);
						isNS = true;
					}

					result = new IntellisenseDataSourceItem(nameToAdd, usingNSPrefix + nameToAdd, type, isNS);
				}
			}
			return result;
		}

		void DataViewList_ListChanged(object sender, ListChangedEventArgs e)
		{
			BeforeListChanged?.Invoke(this, e);
			try
			{
				ListChanged?.Invoke(this, e);
			}
			finally
			{
				AfterListChanged?.Invoke(this, e);
			}
		}

		void CheckNotDisposed()
		{
			if (disposed)
			{
				throw new ObjectDisposedException(GetType().FullName);
			}
		}

		#region Thread Safe Methods

		void Invoke(SendOrPostCallback method)
		{
			if (UISyncContext == null)
			{
				method.DynamicInvoke(new object[1] { null });
			}
			else
			{
				try
				{
					UISyncContext.Send(method, null);
				}
				catch (ObjectDisposedException)
				{
					method.DynamicInvoke(new object[1] { null });
				}
			}
		}

		void InvokeAddIfNotContains(Hashtable addedItemsHash, IntellisenseDataSourceItem item)
		{
			if (item != null && !addedItemsHash.Contains(item))
			{
				Invoke(delegate
{
	if (!disposed)
	{
		Add(item);
	}
});
				addedItemsHash.Add(item, null);
			}
		}

		void Add(IntellisenseDataSourceItem item)
		{
			list.Table.Rows.Add(new object[] { item, item.DisplayName }).EndEdit();
			if (item.DisplayName.Length > WidestDisplayName.Length)
			{
				SetWidestDisplayName(item.DisplayName);
				WidestDisplayNameChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		#endregion
		#endregion
	}
}
