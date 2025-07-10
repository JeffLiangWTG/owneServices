using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public sealed class DynamicBusinessObjectCollection : DynamicBusinessObjectCollection<DynamicBusinessObject>
	{
		public DynamicBusinessObjectCollection(BusinessObjectFactory factory)
			: base(factory) { }
	}

	[System.Diagnostics.DebuggerDisplay("Count = {Count}")]
	public class DynamicBusinessObjectCollection<T> :
			ZCustomTypeDescriptor,
			IBusiness,
			INeedTable,
			IDataRefreshBusSubscriber,
			IBusinessObjectCollection,
			IBusinessObjectCollectionInternals,
			IEnumerable<T>
	where T : DynamicBusinessObject
	{
		public DynamicBusinessObjectCollection(BusinessObjectFactory factory)
		{
			this.factory = factory;
			collection = new SortableList();
		}

		ZDataTable INeedTable.Table
		{
			get { return dataTable; }
		}
		ZDataTable dataTable;

		public IEnumerable<TResult> Select<TResult>(Func<T, TResult> selector)
		{
			foreach (T item in this)
			{
				yield return selector(item);
			}
		}

		public IEnumerable<T> Where(Func<T, bool> predicate)
		{
			foreach (T item in this)
			{
				if (predicate(item))
				{
					yield return item;
				}
			}
		}

		IDisposable IBusinessObjectCollection.SuspendListChanged()
		{
			return DisposableAction.NoAction;
		}

		IDisposable IBusinessObjectCollection.SuspendAdditionallyForImport()
		{
			return DisposableAction.NoAction;
		}

		public void RemoveAll()
		{
			bool fireListChanged = collection.Count > 0;
			RemoveAllWithoutListChanged();
			if (fireListChanged)
			{
				ListReset();
			}
		}

		protected void RemoveAllWithoutListChanged()
		{
			collection.Clear();
		}

		public int Count
		{
			get { return collection.Count; }
		}

		public BusinessObjectFactory Factory
		{
			get { return factory; }
		}

		#region PreFetching

		IBusinessObjectCollectionFetchStrategy IBusinessObjectCollection.FetchStrategy
		{
			get { return new EmptyFetchStrategy(); }
		}

		class EmptyFetchStrategy : IBusinessObjectCollectionFetchStrategy
		{
			void IBusinessObjectCollectionFetchStrategy.FetchForValidate()
			{
			}

			void IBusinessObjectCollectionFetchStrategy.FetchForView(BusinessObject[] businessObjects, TableColumn[] columns)
			{
			}

			public event EventHandler<FetchForViewEventArgs> AdditionalFetchForView { add { } remove { } }

			void IFetchStrategy.FetchForBind()
			{
			}
		}

		#endregion

		public T this[int index]
		{
			get { return collection[index]; }
		}

		public Type TypeOfElements
		{
			get { return typeof(T); }
		}

		Type IBusinessObjectCollection.GetTypeOfElementsFromPK(ZGuid pK)
		{
			return TypeOfElements;
		}

		/// <summary>
		/// Start managing this collection for DataRefresh, i.e. whenever a record of the given type is saved
		/// to the database in this application, this collection will be reloaded.
		/// Note that this also includes any record saved to the same tables used by the given list of business object types
		/// </summary>
		public void ManageForDataRefresh(params Type[] businessObjectTypesToListenToForChanges)
		{
			if (rawSqlQuery == null)
			{
				ErrorReporter.ReportOnce("ManageForDataRefresh", "DynamicBusinessObjectCollection cannot have Data Refresh Management started until Load has been called");
				return;
			}
			DataRefreshManager manager = new DataRefreshManager();

			StopManagingForDataRefresh(businessObjectTypesToListenToForChanges);

			foreach (Type bizType in businessObjectTypesToListenToForChanges)
			{
				if (!bizType.IsSubclassOf(typeof(BusinessObject)))
				{
					throw new ArgumentException("Types passed into ManageForDataRefresh() must be derived from BusinessObject. Error on type : " + bizType.ToString());
				}
				else
				{
					string tableName = BusinessObjectFactory.GetTableNameFromType(bizType);
					manager.StartManaging(tableName, this);
				}
			}

			this.businessObjectTypesToListenToForChanges = businessObjectTypesToListenToForChanges;
		}

		public void StopManagingForDataRefresh(params Type[] businessObjectTypesToListenToForChanges)
		{
			DataRefreshManager manager = new DataRefreshManager();
			if (this.businessObjectTypesToListenToForChanges != null)
			{
				foreach (Type bizType in this.businessObjectTypesToListenToForChanges)
				{
					string tableName = BusinessObjectFactory.GetTableNameFromType(bizType);
					manager.StopManaging(tableName, this);
				}
			}
		}

		/// <summary>
		/// Reloads the collection using the previous RawSqlQuery and Params used to Load this collection.
		/// </summary>
		public void Reload()
		{
			Load(rawSqlQuery, parameters);
		}

		bool IBusinessObjectCollectionInternals.HasChangesFromDatabase()
		{
			return false; //this one is kinda hard to write and out of scope
		}

		public void Load(string queryText)
		{
			Load(queryText, new ZSqlParameterCollection());
		}

		public void Load(string queryText, ZSqlParameter[] queryParameters, int? cmdTimeout = null)
		{
			Load(queryText, new ZSqlParameterCollection(queryParameters), cmdTimeout);
		}

		public void Load(string queryText, ZSqlParameterCollection queryParameters, int? cmdTimeout = null)
		{
			this.rawSqlQuery = queryText;
			if (this.rawSqlQuery != null)
			{
				this.parameters = queryParameters;
				RemoveAllWithoutListChanged();

				var dataQuery = GetDataQuery(queryText, queryParameters, cmdTimeout);
				var bizObjs = Factory.LoadDynamicNonPersistent<T>(dataQuery, out var resultDataTable);

				this.dataTable = resultDataTable;
				collection.AddRange(bizObjs);
				ListReset();
			}
		}

		ZNonPersistentDataQuery GetDataQuery(string queryText, ZSqlParameterCollection queryParameters, int? cmdTimeout = null)
		{
			return new ZNonPersistentDataQuery(queryText, queryParameters, simpleConstruct: false, cmdTimeout);
		}

		#region Suspend/ResumeValidation

		int validationSuspendSemaphore;

		public void SuspendValidation()
		{
			validationSuspendSemaphore++;
		}

		public void ResumeValidation()
		{
			validationSuspendSemaphore--;
		}

		public bool IgnoreValidationSuspended
		{
			get { return ignoreValidationSuspended; }
			set
			{
				ignoreValidationSuspended = value;
				foreach (IBusiness child in this)
				{
					child.IgnoreValidationSuspended = value;
				}
			}
		}
		bool ignoreValidationSuspended;

		public bool IsValidationSuspended
		{
			get { return !IgnoreValidationSuspended && validationSuspendSemaphore > 0; }
		}

		#endregion

		#region HumanReadableName

		public virtual ZString HumanReadableName
		{
			get { return Res.GetString("DynamicBusinessObjectCollection|HumanReadableName", "record"); }
		}

		#endregion

		#region IBusiness

		void IBusiness.RunPreSaveValidationFetch(bool executeFetchHints)
		{
		}

		#endregion

		#region GetItemProperties override

		protected override PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
		{
			PropertyDescriptorCollection result = null;
			if (listAccessors == null && Count >= 1)
			{
				result = collection[0].GetProperties();
			}
			else
			{
				result = new PropertyDescriptorCollection(null);
			}
			return result;
		}

		#endregion

		#region Sort

		internal BusinessObjectCollectionSorter Sorter
		{
			get
			{
				if (sorter == null)
				{
					sorter = new BusinessObjectCollectionSorter(this);
				}
				return sorter;
			}
		}

		BusinessObjectCollectionSorter sorter;

		bool IBindingList.IsSorted
		{
			get { return Sorter.IsSorted; }
		}

		ListSortDirection IBindingList.SortDirection
		{
			get { return Sorter.SortDirection; }
		}

		PropertyDescriptor IBindingList.SortProperty
		{
			get { return Sorter.SortProperty; }
		}

		void IBindingList.RemoveSort()
		{
			Sorter.RemoveSort();
		}

		void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction)
		{
			Sorter.ApplySort(property, direction);
			OnSortChanged(EventArgs.Empty);
		}

		void ISortable.ApplySort(IComparer comparer)
		{
			throw new NotSupportedException();
		}

		public void ApplySort(SortInfo sort)
		{
			Sorter.Sort(sort.PropertyName, sort.Direction);
			OnSortChanged(EventArgs.Empty);
		}

		protected virtual IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			PropertyComparer result;
			string listProviderPropertyName;
			if (guidListMapper != null && guidListMapper.TryGetValue(property.Name, out listProviderPropertyName))
			{
				result = new ZGuidPropertyComparer(property, direction, listProviderPropertyName);
			}
			else if (isTimeSet != null && isTimeSet.Contains(property.Name))
			{
				result = new TimeComparer(property, direction);
			}
			else
			{
				result = new PropertyComparer(property, direction);
			}
			return result;
		}

		public SortInfo SortInformation
		{
			get { return Sorter.SortInformation; }
		}

		public event EventHandler SortChanged;

		void OnSortChanged(EventArgs e)
		{
			if (SortChanged != null)
			{
				SortChanged(this, EventArgs.Empty);
			}
		}

		#endregion

		#region IBusinessObjectCollection Members

		BusinessObject IBusinessObjectCollection.AddNew()
		{
			throw new NotSupportedException();
		}

		bool IBusinessObjectCollection.Contains(BusinessObject businessObject)
		{
			if (businessObject is T)
			{
				foreach (BusinessObject element in this)
				{
					if ((object)element == businessObject)
					{
						return true;
					}
				}
			}
			return false;
		}

		bool IBusinessObjectCollection.Contains(ZGuid pk)
		{
			throw new NotImplementedException();
		}

		void IBusinessObjectCollection.AddRange(IEnumerable businessObjects)
		{
			throw new NotSupportedException();
		}

		void IBusinessObjectCollection.AddGuidListMapping(string columnName, string listName)
		{
			if (guidListMapper == null)
			{
				guidListMapper = new Dictionary<string, string>();
			}
			guidListMapper[columnName] = listName;
		}
		Dictionary<string, string> guidListMapper;

		public virtual void AddIsTime(string propertyName)
		{
			if (isTimeSet == null)
			{
				isTimeSet = new HashSet<string>();
			}
			isTimeSet.Add(propertyName);
		}
		HashSet<string> isTimeSet;

		bool IBusinessObjectCollection.IsLoaded
		{
			get { return rawSqlQuery != null; }
		}

		BusinessObject IBusinessObjectCollection.FindByPK(ZGuid pk)
		{
			foreach (BusinessObject businessObject in this)
			{
				if (businessObject.PK == pk)
				{
					return businessObject;
				}
			}
			return null;
		}

		BusinessObject[] IBusinessObjectCollection.ToArray()
		{
			return collection.ToArray();
		}

		BusinessObject[] IBusinessObjectCollection.Find(ZQuery filter)
		{
			throw new NotSupportedException();
		}

		ISortable IBusinessObjectCollection.Elements
		{
			get { return collection; }
		}

		void IBusinessObjectCollection.Remove(BusinessObject businessObject)
		{
			throw new NotSupportedException();
		}

		void IBusinessObjectCollection.RemoveFromRelationship(BusinessObject businessObject)
		{
			throw new NotSupportedException();
		}

		void IBusinessObjectCollection.Delete(BusinessObject businessObject)
		{
			businessObject.Delete();
		}

		bool IBusinessObjectCollection.ReadOnly
		{
			get { return false; }
		}

		ZQuery IBusinessObjectCollection.CompleteFilter
		{
			get { throw new NotSupportedException(); }
		}

		ZQuery IBusinessObjectCollection.RelationshipFilter
		{
			get { throw new NotSupportedException(); }
		}

		IComparer IBusinessObjectCollection.GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
		{
			return GetComparerForSort(property, direction);
		}

		int IBusinessObjectCollection.IndexOf(IBusiness bizObj, int startIndex, int countToSearchFromStartIndex)
		{
			return collection.IndexOf((T)bizObj, startIndex, countToSearchFromStartIndex);
		}

		PropertyDescriptor IBusinessObjectCollection.ListPropertyDescriptor { get; set; }
		object IBusinessObjectCollection.Parent { get; set; }

		#endregion

		#region IBusiness Members

		void IBusiness.Delete()
		{
			throw new NotSupportedException("Cannot delete a DynamicBusinessObjectCollection!");
		}

		string IBusiness.TableName
		{
			get { return ""; }
		}

		bool IBusiness.CanContinueWithSave
		{
			get { return true; }
		}

		void IBusiness.RunPreSaveValidation()
		{
			// no validation in dynamic collections
		}

		void IBusiness.MarkAsNeedingValidationIncludingChildren()
		{
			// no validation in dynamic collections
		}

		void IBusiness.ValidateIfQuickAndImprovesPreSaveValidationPerformance()
		{
			// no validation in dynamic collections
		}

		IBusiness[] IBusiness.Children
		{
			get { return Array.Empty<IBusiness>(); }
		}

		void IBusiness.NotifyRegisteredChildEditable()
		{
		}

		bool IBusiness.CanDeleteForDataRefresh => false;

		void IBusiness.DeleteForDataRefresh() => throw new NotSupportedException("Cannot delete a DynamicBusinessObjectCollection!");

		#endregion

		#region IBusinessObjectState Members

		void IBusinessObjectState.ClearHasChangesIncludingChildren()
		{
			foreach (IBusinessObjectState child in this)
			{
				child.ClearHasChangesIncludingChildren();
			}
		}

		void IBusinessObjectState.RefreshBindingIncludingChildren()
		{
			ListReset();
			foreach (IBusiness child in this)
			{
				child.RefreshBindingIncludingChildren();
			}
		}

		uint IBusinessObjectState.LastChangeNumber
		{
			get { return 0; }
		}

		void IBusinessObjectState.IncrementReadOnlyIncludingChildren()
		{
		}

		void IBusinessObjectState.DecrementReadOnlyIncludingChildren(bool decrementToZero)
		{
		}

		bool IBusinessObjectState.HasChanges
		{
			get { return false; }
			set { throw new NotSupportedException(); }
		}

		bool IBusinessObjectState.HasChangesNotIncludingChildren
		{
			get { return false; }
		}

		event EventHandler<HasChangesChangedEventArgs> IBusinessObjectState.HasChangesChanged
		{
			add { } //this one cannot be changed
			remove { } //this one cannot be changed
		}

		bool IBusinessObjectState.IsInDatabase => false;

		bool IBusinessObjectState.IsInDatabaseIncludingChildren
		{
			get { return true; }
		}

		event EventHandler IBusinessObjectState.UpdatedByDataRefreshIncludingChildren
		{
			add { } //this one cannot be changed
			remove { } //this one cannot be changed
		}

		event EventHandler<NotificationsChangedEventArgs> IBusinessObjectState.NotificationsChanged
		{
			add { } //this one cannot be changed
			remove { } //this one cannot be changed
		}

		#endregion

		#region IBindingList Members

		void IBindingList.AddIndex(PropertyDescriptor property)
		{
		}

		bool IBindingList.AllowNew
		{
			get { return false; }
		}

		int IBindingList.Find(PropertyDescriptor property, object key)
		{
			return -1;
		}

		bool IBindingList.SupportsSorting
		{
			get { return true; }
		}

		bool IBindingList.AllowRemove
		{
			get { return false; }
		}

		bool IBindingList.SupportsSearching
		{
			get { return false; }
		}

		event ListChangedEventHandler IBindingList.ListChanged
		{
			add { ListChanged += value; }
			remove { ListChanged -= value; }
		}

		event ListChangedEventHandler ListChanged;

		bool IBindingList.SupportsChangeNotification
		{
			get { return true; }
		}

		object IBindingList.AddNew()
		{
			return null;
		}

		bool IBindingList.AllowEdit
		{
			get { return false; }
		}

		void IBindingList.RemoveIndex(PropertyDescriptor property)
		{
		}

		#endregion

		#region IList Members

		bool IList.IsReadOnly
		{
			get { return true; }
		}

		object IList.this[int index]
		{
			get { return ((IList)collection)[index]; }
			set { ((IList)collection)[index] = value; }
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
			return ((IList)collection).Contains((T)value);
		}

		void IList.Clear()
		{
			throw new NotSupportedException();
		}

		int IList.IndexOf(object value)
		{
			return ((IList)collection).IndexOf((T)value);
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
			get { return false; }
		}

		void ICollection.CopyTo(Array array, int index)
		{
			throw new NotSupportedException();
		}

		object ICollection.SyncRoot
		{
			get { return null; }
		}

		#endregion

		#region IEnumerable Members

		public IEnumerator<T> GetEnumerator()
		{
			return collection.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return collection.GetEnumerator();
		}

		#endregion

		#region INotificationProvider

		IEnumerable<INotification> INotificationProvider.Notifications
		{
			get { return new ZCollectionNotificationProviderHelper(this).Notifications; }
		}

		bool INotificationProvider.HasNotifications()
		{
			return new ZCollectionNotificationProviderHelper(this).HasNotifications();
		}

		bool INotificationProvider.HasNotifications(INotificationType type)
		{
			return new ZCollectionNotificationProviderHelper(this).HasNotifications(type);
		}

		INotificationType INotificationProvider.GetHighestSeverityNotificationType()
		{
			return new ZCollectionNotificationProviderHelper(this).GetHighestSeverityNotificationType();
		}

		#endregion

		#region IIdentified Members

		ZGuid IIdentified.Identifier
		{
			get { return ZGuid.Empty; }
		}

		#endregion

		#region IDataRefreshBusSubscriber Members

		void IDataRefreshBusSubscriber.UpdatedByDataRefresh(IEnumerable<object> publishedObjects)
		{
			Reload();
		}

		bool IDataRefreshBusSubscriber.IncludeDeletedObjectsInRefresh => false;

		#endregion

		#region IBusinessObjectCollectionInternals Members

		void IBusinessObjectCollectionInternals.FireListResetEvent()
		{
			ListReset();
		}

		bool IBusinessObjectCollectionInternals.HasChangesFromDelete
		{
			get { return false; }
			set { }
		}

		bool IBusinessObjectCollectionInternals.MastersAreInDatabase
		{
			get { return false; } // No masters
		}

		bool IBusinessObjectCollectionInternals.MastersAreDeleted
		{
			get { return false; } // No masters
		}

		bool IBusinessObjectCollectionInternals.IsListChangedSuspended
		{
			get { return false; } // SuspendListChanged does nothing
		}

		#endregion

		#region Implementation

#if DEBUG
		public string RawQuery => rawSqlQuery;
#endif

		Type[] businessObjectTypesToListenToForChanges;
		string rawSqlQuery;
		ZSqlParameterCollection parameters;

		readonly SortableList collection;
		readonly BusinessObjectFactory factory;

		public class SortableList : List<T>, ISortable
		{
			#region ISortable Members

			void ISortable.ApplySort(IComparer comparer)
			{
				this.Sort(delegate(T x, T y)
				{
					return comparer.Compare(x, y);
				});
			}

			#endregion
		}

		protected void ListReset()
		{
			if (ListChanged != null)
			{
				ListChanged(this, new ListChangedEventArgs(ListChangedType.Reset, -1, -1));
			}
		}

		#endregion
	}
}
