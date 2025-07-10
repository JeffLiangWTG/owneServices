using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZBindingContext : BindingContext
	{
		public ZBindingContext()
		{
			var listManagers = typeof(BindingContext).GetField("listManagers", BindingFlags.NonPublic | BindingFlags.Instance) ?? typeof(BindingContext).GetField("_listManagers", BindingFlags.NonPublic | BindingFlags.Instance);
			listManagers.SetValue(this, new BindingContextHashtable(this));
		}

		public new event EventHandler CollectionChanged;

		protected override void OnCollectionChanged(CollectionChangeEventArgs e)
		{
			base.OnCollectionChanged(e);
			var manager = e.Element as CurrencyManager;
			if (e.Action == CollectionChangeAction.Add && manager != null)
			{
				if (previousManager != null)
				{
					previousManager.PositionChanged -= new EventHandler(positionChangedHandler.OnPositionChanged);
					previousManager.DataError -= new BindingManagerDataErrorEventHandler(Manager_DataError);
				}
				positionChangedHandler = new CurrencyManagerPositionChangedHandler();
				manager.PositionChanged += new EventHandler(positionChangedHandler.OnPositionChanged);
				manager.DataError += new BindingManagerDataErrorEventHandler(Manager_DataError);
				previousManager = manager;
			}

			if (CollectionChanged != null)
			{
				CollectionChanged(this, EventArgs.Empty);
			}
		}

		CurrencyManagerPositionChangedHandler positionChangedHandler;
		CurrencyManager previousManager;

		void Manager_DataError(object sender, BindingManagerDataErrorEventArgs e)
		{
			//ErrorReporter.ReportOnce("Exception from CurrencyManager, this may be the root cause of other exceptions.", e.Exception);
		}

		#region BindingContextHashtable

		protected class BindingContextHashtable : Hashtable
		{
			public BindingContextHashtable(ZBindingContext bindingContext)
			{
				this.bindingContext = bindingContext;
			}

			public override void Add(object key, object value)
			{
				try
				{
					base.Add(key, value);
				}
				catch (ArgumentException)
				{
					// very occastionally new CurrencyManager() causes ListChanged to fire, which comes back into here from an OnHandleCreate
				}

				var target = ((WeakReference)value).Target;
				var cm = target as CurrencyManager;

				if (cm != null)
				{
					var parentManagerField = cm.GetType().GetField("parentManager", BindingFlags.NonPublic | BindingFlags.Instance);
					if (parentManagerField != null)
					{
						var parentManager = parentManagerField.GetValue(cm) as CurrencyManager;
						if (parentManager != null)
						{
							var currentItemChangedMethod = cm.GetType().GetMethod("ParentManager_CurrentItemChanged", BindingFlags.NonPublic | BindingFlags.Instance);
							var handlerToRemove = (EventHandler)Activator.CreateInstance(typeof(EventHandler), new object[] { cm, currentItemChangedMethod.MethodHandle.GetFunctionPointer() });
							parentManager.CurrentItemChanged -= handlerToRemove;
							var changedHandler = new ParentCurrentChangedHandler(cm);

							parentManager.CurrentChanged -= changedHandler.ParentCurrentChanged;
							parentManager.CurrentChanged += changedHandler.ParentCurrentChanged;

							changedHandler.ParentCurrentChanged(parentManager, EventArgs.Empty);
						}
					}
				}

				bindingContext.OnCollectionChanged(new CollectionChangeEventArgs(CollectionChangeAction.Add, target));
			}

			readonly ZBindingContext bindingContext;
		}

		#endregion

		#region ParentCurrentChangedHandler

		protected class ParentCurrentChangedHandler
		{
			public ParentCurrentChangedHandler(CurrencyManager childManager)
			{
				this.childManager = childManager;
			}

			protected readonly CurrencyManager childManager;
			object lastCurrent;

			public void ParentCurrentChanged(object sender, EventArgs e)
			{
				var parentManager = (CurrencyManager)sender;

				if (parentManager.Count > 0)
				{
					if (parentManager.List is IActiveBusinessObjectCollection && ActiveBusinessObjectCollection.CollectionsAreRefreshedOnSave &&
						lastCurrent != null && lastCurrent == parentManager.GetCurrent())
					{
						return;
					}

					lastCurrent = parentManager.GetCurrent();
					childManager.GetType().GetMethod("ParentManager_CurrentItemChanged", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(childManager, new object[] { sender, e });
				}
				else
				{
					lastCurrent = null;

					childManager.GetType().GetMethod("SetDataSource", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(childManager, new object[] { new TempList() });
					childManager.GetType().GetField("listposition", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(childManager, -1);

					childManager.GetType().GetMethod("OnPositionChanged", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(childManager, new object[] { EventArgs.Empty });
					childManager.GetType().GetMethod("OnCurrentChanged", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(childManager, new object[] { EventArgs.Empty });
					childManager.GetType().GetMethod("OnCurrentItemChanged", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(childManager, new object[] { EventArgs.Empty });
				}
			}
		}

		internal class TempList : IBindingList, IBusiness, IBusinessObjectCollection, IDoNotHaveFactory
		{
			public IDisposable SuspendListChanged()
			{
				return DisposableAction.NoAction;
			}

			public IDisposable SuspendAdditionallyForImport()
			{
				return DisposableAction.NoAction;
			}

			#region IBindingList Members

			void IBindingList.AddIndex(PropertyDescriptor property)
			{
			}

			object IBindingList.AddNew()
			{
				return null;
			}

			bool IBindingList.AllowEdit
			{
				get { return false; }
			}

			bool IBindingList.AllowNew
			{
				get { return false; }
			}

			bool IBindingList.AllowRemove
			{
				get { return false; }
			}

			void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction)
			{
			}

			int IBindingList.Find(PropertyDescriptor property, object key)
			{
				return 0;
			}

			bool IBindingList.IsSorted
			{
				get { return false; }
			}

			event ListChangedEventHandler IBindingList.ListChanged
			{
				add { }
				remove { }
			}

			void IBindingList.RemoveIndex(PropertyDescriptor property)
			{
			}

			void IBindingList.RemoveSort()
			{
			}

			ListSortDirection IBindingList.SortDirection
			{
				get { return ListSortDirection.Ascending; }
			}

			PropertyDescriptor IBindingList.SortProperty
			{
				get { return null; }
			}

			bool IBindingList.SupportsChangeNotification
			{
				get { return true; }
			}

			bool IBindingList.SupportsSearching
			{
				get { return false; }
			}

			bool IBindingList.SupportsSorting
			{
				get { return false; }
			}

			#endregion

			#region IList Members

			public int IndexOf(BusinessObject bo, int from, int count)
			{
				return -1;
			}

			int IList.Add(object value)
			{
				throw new NotImplementedException("The method or operation is not implemented.");
			}

			void IList.Clear()
			{
			}

			bool IList.Contains(object value)
			{
				return false;
			}

			int IList.IndexOf(object value)
			{
				return -1;
			}

			void IList.Insert(int index, object value)
			{
			}

			bool IList.IsFixedSize
			{
				get { return true; }
			}

			bool IList.IsReadOnly
			{
				get { return true; }
			}

			void IList.Remove(object value)
			{
			}

			void IList.RemoveAt(int index)
			{
			}

			object IList.this[int index]
			{
				get { throw new NotImplementedException("The method or operation is not implemented."); }
				set { throw new NotImplementedException("The method or operation is not implemented."); }
			}

			#endregion

			#region ICollection Members

			void ICollection.CopyTo(Array array, int index)
			{
			}

			int ICollection.Count
			{
				get { return 0; }
			}

			bool ICollection.IsSynchronized
			{
				get { return false; }
			}

			object ICollection.SyncRoot
			{
				get { return this; }
			}

			#endregion

			#region IEnumerable Members

			IEnumerator IEnumerable.GetEnumerator()
			{
				return new DummyIEnumerator();
			}

			class DummyIEnumerator : IEnumerator
			{
				#region IEnumerator Members

				public object Current
				{
					get { throw new NotImplementedException("The method or operation is not implemented."); }
				}

				public bool MoveNext()
				{
					return false;
				}

				public void Reset()
				{
					throw new NotImplementedException("The method or operation is not implemented.");
				}

				#endregion
			}

			#endregion

			#region IBusiness Members

			public BusinessObjectFactory Factory
			{
				get { throw new NotSupportedException(); }
			}

			void IBusiness.SuspendValidation()
			{
			}

			void IBusiness.ResumeValidation()
			{
			}

			bool IBusiness.IgnoreValidationSuspended
			{
				get
				{
					return false;
				}
				set
				{
				}
			}

			bool IBusiness.IsValidationSuspended
			{
				get { return false; }
			}

			void IBusiness.Delete()
			{
			}

			string IBusiness.TableName
			{
				get { return ""; }
			}

			ZString IBusiness.HumanReadableName
			{
				get { return ""; }
			}

			bool IBusiness.CanContinueWithSave
			{
				get { return false; }
			}

			void IBusiness.RunPreSaveValidation()
			{
			}

			void IBusiness.RunPreSaveValidationFetch(bool executeHints)
			{
			}

			void IBusiness.MarkAsNeedingValidationIncludingChildren()
			{
			}

			void IBusiness.ValidateIfQuickAndImprovesPreSaveValidationPerformance()
			{
			}

			IBusiness[] IBusiness.Children
			{
				get { return Array.Empty<IBusiness>(); }
			}

			void IBusiness.NotifyRegisteredChildEditable()
			{
			}

			#endregion

			#region IBusinessObjectState Members

			public void IncrementReadOnlyIncludingChildren()
			{
			}

			public void DecrementReadOnlyIncludingChildren(bool decrementToZero)
			{
			}

			public bool HasChanges
			{
				get
				{
					throw new NotImplementedException("The method or operation is not implemented.");
				}
				set
				{
					throw new NotImplementedException("The method or operation is not implemented.");
				}
			}

			public bool HasChangesNotIncludingChildren
			{
				get { throw new NotImplementedException("The method or operation is not implemented."); }
			}

			public uint LastChangeNumber
			{
				get { throw new NotImplementedException("The method or operation is not implemented."); }
			}

			public bool IsInDatabase => throw new NotImplementedException("The method or operation is not implemented.");

			public bool IsInDatabaseIncludingChildren
			{
				get { throw new NotImplementedException("The method or operation is not implemented."); }
			}

			public void ClearHasChangesIncludingChildren()
			{
				throw new NotImplementedException("The method or operation is not implemented.");
			}

			public void RefreshBindingIncludingChildren()
			{
				throw new NotImplementedException("The method or operation is not implemented.");
			}

			public event EventHandler<HasChangesChangedEventArgs> HasChangesChanged { add { } remove { } }
			public event EventHandler UpdatedByDataRefreshIncludingChildren { add { } remove { } }
			public event EventHandler<NotificationsChangedEventArgs> NotificationsChanged { add { } remove { } }

			#endregion

			#region IBusinessObjectCollection Members

			BusinessObject IBusinessObjectCollection.AddNew()
			{
				return null;
			}

			bool IBusinessObjectCollection.Contains(BusinessObject businessObject)
			{
				return false;
			}

			bool IBusinessObjectCollection.Contains(ZGuid pk)
			{
				return false;
			}

			void IBusinessObjectCollection.AddRange(IEnumerable businessObjects)
			{
			}

			public Type TypeOfElements
			{
				get { return typeof(object); }
			}

			public Type GetTypeOfElementsFromPK(ZGuid pk)
			{
				return TypeOfElements;
			}

			public BusinessObject[] ToArray()
			{
				throw new NotImplementedException("The method or operation is not implemented.");
			}

			public BusinessObject[] Find(ZQuery filter)
			{
				throw new NotImplementedException("The method or operation is not implemented.");
			}

			public ISortable Elements
			{
				get { throw new NotImplementedException("The method or operation is not implemented."); }
			}

			void IBusinessObjectCollection.Remove(BusinessObject businessObject)
			{
				throw new NotSupportedException();
			}

			public void RemoveFromRelationship(BusinessObject businessObject)
			{
				throw new NotImplementedException("The method or operation is not implemented.");
			}

			public void Delete(BusinessObject businessObject)
			{
				throw new NotImplementedException("The method or operation is not implemented.");
			}

			public void AddGuidListMapping(string propertyName, string listName)
			{
			}

			public void AddIsTime(string propertyName)
			{
			}

			public IBusinessObjectCollectionFetchStrategy FetchStrategy
			{
				get { return null; }
			}

			public bool IsLoaded
			{
				get { return true; }
			}

			public BusinessObject FindByPK(ZGuid pk)
			{
				return null;
			}

			public void ApplySort(SortInfo sort)
			{
				throw new NotSupportedException();
			}

			public SortInfo SortInformation
			{
				get { return null; }
			}

			public bool IsNonCommittedCollectionElement(BusinessObject element)
			{
				return false;
			}

			bool IBusinessObjectCollection.ReadOnly
			{
				get { return true; }
			}

			ZQuery IBusinessObjectCollection.CompleteFilter
			{
				get { return ZQuery.NoResultQuery; }
			}

			ZQuery IBusinessObjectCollection.RelationshipFilter
			{
				get { return ZQuery.NoResultQuery; }
			}

			public IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
			{
				throw new NotSupportedException();
			}

			int IBusinessObjectCollection.IndexOf(IBusiness bizObj, int startIndex, int countToSearchFromStartIndex)
			{
				return -1;
			}

			public int Count
			{
				get { return 0; }
			}

			event EventHandler IBusinessObjectCollection.SortChanged
			{
				add { }
				remove { }
			}

			PropertyDescriptor IBusinessObjectCollection.ListPropertyDescriptor { get; set; }
			object IBusinessObjectCollection.Parent { get; set; }

			bool IBusiness.CanDeleteForDataRefresh => throw new NotImplementedException();
			void IBusiness.DeleteForDataRefresh() => throw new NotImplementedException();

			#endregion

			#region ISortable Members

			void ISortable.ApplySort(IComparer comparer)
			{
			}

			#endregion

			#region INotificationProvider Members

			IEnumerable<INotification> INotificationProvider.Notifications
			{
				get { yield break; }
			}

			bool INotificationProvider.HasNotifications()
			{
				return false;
			}

			bool INotificationProvider.HasNotifications(INotificationType type)
			{
				return false;
			}

			INotificationType INotificationProvider.GetHighestSeverityNotificationType()
			{
				return null;
			}

			#endregion

			#region IIdentified Members

			ZGuid IIdentified.Identifier
			{
				get { return ZGuid.Empty; }
			}

			#endregion
		}

		#endregion

		#region CurrencyManagerPositionChangedHandler

		protected class CurrencyManagerPositionChangedHandler
		{
			public CurrencyManagerPositionChangedHandler()
			{
			}

			internal void OnPositionChanged(object sender, EventArgs e)
			{
				if (!inPositionChanged)
				{
					inPositionChanged = true;
					try
					{
						Manager = (CurrencyManager)sender;
						if (Manager.Position == -1)
						{
							var suspendPushData_PreviousValue = SuspendPushData;
							SuspendPushData = true;

							try
							{
								var onCurrentItemChangedMethod = typeof(CurrencyManager).GetMethod("OnCurrentItemChanged", BindingFlags.NonPublic | BindingFlags.Instance);
								onCurrentItemChangedMethod.Invoke(Manager, new object[] { e });
							}
							finally
							{
								SuspendPushData = suspendPushData_PreviousValue;
							}
						}
					}
					finally
					{
						inPositionChanged = false;
					}
				}
			}

			#region Implementation

			bool inPositionChanged;
			CurrencyManager Manager;
			[ThreadStatic]
			static FieldInfo suspendPushInfo;

			bool SuspendPushData
			{
				get { return (bool)SuspendPushInfo.GetValue(Manager); }
				set { SuspendPushInfo.SetValue(Manager, value); }
			}

			FieldInfo SuspendPushInfo
			{
				get
				{
					if (suspendPushInfo == null)
					{
						suspendPushInfo = typeof(CurrencyManager).GetField("suspendPushDataInCurrentChanged", BindingFlags.NonPublic | BindingFlags.Instance);
					}
					return suspendPushInfo;
				}
			}

			#endregion
		}

		#endregion
	}
}
