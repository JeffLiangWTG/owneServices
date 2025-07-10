using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public interface IBusinessObjectCollectionWithRelatedElements
	{
		void RemoveAllRelatedElements();
	}

	public interface IBusinessObjectCollectionWithMaster
	{
		BusinessObject Master { get; }
	}
}

namespace Enterprise.ZArchitecture.Business
{
	/// TODO :
	///  Rename this class (to what?),
	///  Implement IEnumerator + IEnumerable
	///  Ensure the 'AllElements' implements IBusinessObjectCollectionWithRelatedElements
	///  Ensure 'GetElementsCollectionFromRelatedBizObject()' implements IBusinessObjectCollectionWithRelatedElements
	///  Report error if attempting to add incorrect BizObj type in AddWithoutLoading()
	public abstract class ZLogsOrNotes : NonPersistentBusinessObject, IObsoleteValidation
	{
		protected ZLogsOrNotes(BusinessObject parent)
			: base(parent == null ? null : parent.Factory)
		{
			if (parent == null && (object)parent == null)
			{
				throw new ArgumentNullException(nameof(parent), "Constructor Notes(BusinessObject Parent), Parent was null.");
			}
			this.Parent = parent;
		}

		public readonly BusinessObject Parent;

		/// <summary>
		/// Gets the record count from the DATABASE (this may not be the same as the count in the FACTORY!).
		/// </summary>
		public int DatabaseCount
		{
			get { return ElementsFactory.GetDatabaseCount(ElementType, new ZQuery(ElementForeignKeyColumn, Parent.PK)); }
		}

		#region Adding Elements

		public virtual void Add(BusinessObject bizObject)
		{
			AddWithoutLoading(bizObject);
		}

		public virtual BusinessObject AddNew()
		{
			return AddNewWithType(ElementType);
		}

		protected internal virtual BusinessObject AddNew(Type businessObjectType)
		{
			return AddNewWithType(businessObjectType);
		}

		BusinessObject AddNewWithType(Type businessObjectType)
		{
			var bizObject = ElementsFactory.New(businessObjectType);
			AddWithoutLoading(bizObject);

			return bizObject;
		}

		#endregion

		#region Abstract

		protected abstract BusinessObject[] GetBusinessObjectsWithRelatedElements();
		protected abstract SchemaColumn ElementForeignKeyColumn { get; }
		protected abstract Type ElementType { get; }

		protected abstract ZQuery GetRelatedElementsFilter(BusinessObject relatedBizO);
		protected abstract BusinessObjectCollection GetNewElementsCollection();
		protected abstract BusinessObjectCollection GetNewAllElementsCollection();
		protected abstract IBusinessObjectCollectionView GetNewVisibleElementsCollectionView();
		protected abstract BusinessObjectCollection GetElementsCollectionFromRelatedBizObject(BusinessObject relatedBizObject);

		#endregion

		#region Elements / New Elements

		internal BusinessObjectCollection ElementsInternal
		{
			get
			{
				if (fElementsInternal == null)
				{
					fElementsInternal = GetNewElementsCollection();
					Parent.RegisterEditableChildObject(fElementsInternal);
					fElementsInternal.Load();

					if (IsNewElementsPopulated)
					{
						fElementsInternal.AddRange(NewElements.Where(element => element != null && !element.IsDeleted));
						Parent.UnRegisterEditableChildObject(NewElements);
						fNewElements = null;
					}
				}
				return fElementsInternal;
			}
		}

		protected ZQuery ElementsInternalFilter
		{
			get
			{
				ZQuery result;

				if (IsElementsLoaded)
				{
					result = ((IBusinessObjectCollection)ElementsInternal).CompleteFilter;
				}
				else // don't want to load the collection just to get the filters
				{
					result = ((IBusinessObjectCollection)GetNewElementsCollection()).CompleteFilter;
				}

				return result;
			}
		}

		protected internal bool IsElementsLoaded
		{
			get { return fElementsInternal != null; }
		}

		protected bool IsNewElementsPopulated
		{
			get { return fNewElements != null; }
		}

		/// <summary>
		/// For performance we store all new elements in this hash until such time that ElementsInternal is accessed.
		/// </summary>
		protected BusinessObjectCollection NewElements
		{
			get
			{
				if (fNewElements == null)
				{
					fNewElements = GetNewElementsCollection();
					Parent.RegisterEditableChildObject(fNewElements);

					// RegisterEditableChildObject can potentially call ElementsInternal 
					// which can set fNewElements to null, hence the following check
					if (fNewElements == null)
					{
						return ElementsInternal;
					}
				}
				return fNewElements;
			}
		}

		public void RemoveAndDeleteAll()
		{
			if (IsElementsLoaded || Parent.IsInDatabase)
			{
				ElementsInternal.RemoveAndDeleteAll();
			}
			else if (IsNewElementsPopulated)
			{
				NewElements.RemoveAndDeleteAll();
			}
		}

		protected override void OnSaveRollback()
		{
			base.OnSaveRollback();

			for (var i = (fElementsInternal?.Count ?? 0) - 1; i >= 0; i--)
			{
				var log = fElementsInternal.ElementAt(i);
				if (DataRowIsNullOrDetached(((INeedRow)log).Row))
				{
					fElementsInternal.Remove(log);
				}
			}
		}

		bool DataRowIsNullOrDetached(DataRow row)
		{
			return row == null || row.RowState == DataRowState.Detached;
		}

		#endregion

		#region All Elements (Includes Related)

		internal BusinessObjectCollection AllElements
		{
			get
			{
				if (!IsAllElementsLoaded || RelatedBusinessObjectsHaveChanged)
				{
					RebuildAllElements();
				}
				return fAllElements;
			}
		}

		protected void RebuildAllElements()
		{
			RebuildAllElementsInternal(false);
		}

		void RebuildAllElementsFromDB()
		{
			RebuildAllElementsInternal(true);
		}

		protected virtual ZQuery RebuildAllElementsInternalQuery
		{
			get
			{
				ZQuery query = new ZQuery(ElementForeignKeyColumn, Parent.PK);
				query.ReLoadExistingRows = true;
				return query;
			}
		}

		bool inRebuildAllElementsInternal;
		void RebuildAllElementsInternal(bool alwaysLoadFromDB)
		{
			inRebuildAllElementsInternal = true;
			try
			{
				using (((ISingleElementListInternal)Parent).SuspendListChanged())
				{
					// add elements from the parent..
					if (alwaysLoadFromDB)
					{
						if (IsAllElementsLoaded)
						{
							fAllElements.RemoveAll();
						}

						if (IsElementsLoaded)
						{
							// Note, query can't be empty or ReLoadExistingRows will be ignored
							ElementsInternal.Load(RebuildAllElementsInternalQuery); // reload parent elements from DB
						}

						if (IsAllElementsLoaded)
						{
							AddElementsInternalToAllElementsWithDataRefresh();
						}
					}
					else if (!IsAllElementsLoaded)
					{
						fAllElements = GetNewAllElementsCollection();
						AddElementsInternalToAllElementsWithDataRefresh();
					}
					else
					{
						((IBusinessObjectCollectionWithRelatedElements)fAllElements).RemoveAllRelatedElements(); // only remove related elements
					}

					if (IsAllElementsLoaded)
					{
						if (alwaysLoadFromDB)
						{
							fAllElements.AddRange(RelatedElementsFromDB);
						}
						else
						{
							fAllElements.AddRange(RelatedElements);
						}
					}
				}
			}
			finally
			{
				if (IsAllElementsLoaded)
				{
					((IBusinessObjectCollectionInternals)fAllElements).FireListResetEvent();
				}
				inRebuildAllElementsInternal = false;
			}
		}

		void AddElementsInternalToAllElementsWithDataRefresh()
		{
			var elements = ElementsInternal;
			fAllElements.AddRange(elements);
			if (elements.IsManagedForDataRefresh && IsAllElementsUpdatedByDataRefresh)
			{
				IBindingList bindingList = elements;
				bindingList.ListChanged -= ElementsInternal_ListChanged;
				bindingList.ListChanged += ElementsInternal_ListChanged;
			}
		}

		// Disable by default since only currently needed for Notes, not Logs
		protected virtual bool IsAllElementsUpdatedByDataRefresh => false;

		/// <summary>
		/// Publish ElementsInternal data refresh list changes to AllElements.
		/// Only new elements are added. Deleted or updated elements are not effected, since that's how data refresh works for BusinessObjectCollection.
		/// 
		/// AllElements is not updated by data refresh since it is a non-dependant collection.
		/// However AllElements is just a combination of ElementsInternal plus RelatedElements.
		/// Since ElementsInternal is updated by data refresh automatically for Notes (it derives from DependentBusinessObjectCollection)
		/// it makes sense that data refresh changes to ElementsInternal are also published to AllElements.
		/// RelatedElements is also sync'd elsewhere in his class.
		///
		/// The effect of this syncing is that a Save in one form that creates new notes in another bizo
		/// causes another form that shows those notes to be updated.
		/// E.g., adding a consol note from the the shipment form will show the note in the consol form.
		/// </summary>
		void ElementsInternal_ListChanged(object sender, ListChangedEventArgs e)
		{
			if (!inRebuildAllElementsInternal && fAllElements != null && fElementsInternal != null)
			{
				// AddRange will ignore elements that are already present
				fAllElements.AddRange(fElementsInternal);
			}
		}

		protected bool IsAllElementsLoaded
		{
			get { return (fAllElements != null); }
		}

		#region Related Elements

		internal BusinessObjectCollection RelatedElements
		{
			get
			{
				if (!IsRelatedElementsLoaded || ForceReloadRelatedElementsOnNextAccess || RelatedBusinessObjectsHaveChanged)
				{
					LoadRelatedElements();
					ForceReloadRelatedElementsOnNextAccess = false;
				}

				return fRelatedElements;
			}
		}

		public bool ForceReloadRelatedElementsOnNextAccess { get; set; }

		BusinessObjectCollection RelatedElementsFromDB
		{
			get
			{
				if (!IsRelatedElementsLoaded)
				{
					LoadRelatedElements();
				}
				else
				{
					LoadRelatedElementsFromDB();
				}

				return fRelatedElements;
			}
		}

		public void LoadRelatedElements()
		{
			LoadRelatedElementsInternal(false);
		}

		void LoadRelatedElementsFromDB()
		{
			LoadRelatedElementsInternal(true);
		}

		void LoadRelatedElementsInternal(bool alwaysLoadFromDB)
		{
			if (!IsRelatedElementsLoaded)
			{
				fRelatedElements = GetNewAllElementsCollection();
			}
			else
			{
				((ILegacyBusinessObjectCollectionInternals)fRelatedElements).RemoveAllButLeaveRelationshipsIntact();
			}

			BusinessObject[] businessObjectsWithRelatedElements = GetBusinessObjectsWithRelatedElements();
			if (businessObjectsWithRelatedElements != null)
			{
				currentBusinessObjectsWithRelatedElements = new IBusiness[businessObjectsWithRelatedElements.Length];

				for (int i = 0; i < businessObjectsWithRelatedElements.Length; i++)
				{
					currentBusinessObjectsWithRelatedElements[i] = businessObjectsWithRelatedElements[i];

					if (businessObjectsWithRelatedElements[i] != null)
					{
						AddElementsFromBusinessObject(businessObjectsWithRelatedElements[i], alwaysLoadFromDB, fRelatedElements);
						ShouldRebuildAllElements = true;
					}
				}
			}
		}

		void AddElementsFromBusinessObject(BusinessObject relatedBizO, bool alwaysLoadFromDB, BusinessObjectCollection collectionToAddTo)
		{
			if (relatedBizO != null)
			{
				BusinessObjectCollection elements = GetElementsCollectionFromRelatedBizObject(relatedBizO);

				// we need to remove the delegate first so that we don't add it twice
				CollectionCountChangedEventHandler countChangedDelegate = new CollectionCountChangedEventHandler(ElementsForRelatedBusinessObject_CountChanged);
				elements.CountChanged -= countChangedDelegate;
				elements.CountChanged += countChangedDelegate;

				if (alwaysLoadFromDB && !(elements is StmNoteCollectionViewNonPrivateNotesWithContext))
				{
					ZQuery query = new ZQuery();
					query.ReLoadExistingRows = true;
					elements.Load(query); // reload elements from DB

					// TODO : this is probably wrong, ie it will ignore hte additional filter **************************
				}
				else
				{
					collectionToAddTo.AddRange(elements);
				}
			}
		}

		protected bool IsRelatedElementsLoaded
		{
			get { return (fRelatedElements != null); }
		}

		bool ShouldRebuildAllElements;

		BusinessObjectCollection fRelatedElements;

		#endregion

		#region Has Related Elements

		internal bool HasRelatedElements
		{
			get
			{
				bool result = false;

				if (IsRelatedElementsLoaded)
				{
					result = RelatedElements.Count > 0;
				}
				else
				{
					BusinessObject[] businessObjectsWithRelatedElements = GetBusinessObjectsWithRelatedElements();
					if (businessObjectsWithRelatedElements != null && businessObjectsWithRelatedElements.Length > 0)
					{
						foreach (BusinessObject relatedBizO in businessObjectsWithRelatedElements)
						{
							if (relatedBizO != null)
							{
								ZQuery relatedElementsFilter = GetRelatedElementsFilter(relatedBizO);
								if (ElementsFactory.LoadTop1(ElementType, relatedElementsFilter) != null)
								{
									result = true;
									break;
								}
							}
						}
					}
				}

				return result;
			}
		}

		#endregion

		#region Related Business Objects Have Changed?

		/// <summary>
		/// Has the module developer assigned a new business object to an item in their 'RelatedBusinessObjects' array?
		/// (eg. BizO.RelatedBusinessEntitiesWithLogs or BizO.RelatedBusinessEntitiesWithNotes)
		/// </summary>
		protected bool RelatedBusinessObjectsHaveChanged
		{
			get
			{
				bool result = false;

				if (IsAllElementsLoaded)
				{
					BusinessObject[] businessObjectsWithRelatedElements = GetBusinessObjectsWithRelatedElements();
					if (currentBusinessObjectsWithRelatedElements == null || businessObjectsWithRelatedElements == null)
					{
						result = (currentBusinessObjectsWithRelatedElements != businessObjectsWithRelatedElements);
					}
					else if (currentBusinessObjectsWithRelatedElements.Length != businessObjectsWithRelatedElements.Length)
					{
						result = true;
					}
					else
					{
						for (int i = 0; !result && i < currentBusinessObjectsWithRelatedElements.Length; i++)
						{
							if (currentBusinessObjectsWithRelatedElements[i] != businessObjectsWithRelatedElements[i])
							{
								result = true;
							}
						}
					}
				}

				return result;
			}
		}

		#endregion

		#region Synchronising Related Business Object Elements

		void ElementsForRelatedBusinessObject_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (CurrentBusinessObjectsWithRelatedElementsContains(((IBusinessObjectCollectionWithMaster)sender).Master))
			{
				if (IsAllElementsLoaded)
				{
					if (e.ItemAdded)
					{
						AllElements.Add(e.BizObject);
					}
					else if (AllElements.Contains(e.BizObject) && !e.BizObject.IsDeleted)
					{
						AllElements.Remove(e.BizObject);
					}
				}
			}
			else
			{
				((BusinessObjectCollection)(sender)).CountChanged -= new CollectionCountChangedEventHandler(ElementsForRelatedBusinessObject_CountChanged);
			}
		}

		bool CurrentBusinessObjectsWithRelatedElementsContains(IBusiness bizEntity)
		{
			foreach (IBusiness otherBizEntity in currentBusinessObjectsWithRelatedElements)
			{
				if (otherBizEntity == bizEntity)
				{
					return true;
				}
			}

			return false;
		}

		#endregion

		#endregion

		#region Visible Elements

		protected IBusinessObjectCollectionView VisibleElements
		{
			get
			{
				if (fVisibleElements == null)
				{
					fVisibleElements = GetNewVisibleElementsCollectionView();
				}
				else if (!Parent.IsDeleted && (!IsAllElementsLoaded || RelatedBusinessObjectsHaveChanged || ShouldRebuildAllElements))
				{
					RebuildAllElements();
					ShouldRebuildAllElements = false;
				}

				return fVisibleElements;
			}
		}
		IBusinessObjectCollectionView fVisibleElements;

		protected virtual bool VisibleElementsAreReadOnlyForSecurity
		{
			get { return false; }
		}

		#endregion

		#region Elements not in DB

		protected ArrayList ElementsNotInDB
		{
			get
			{
				ArrayList result;

				if (IsElementsLoaded)
				{
					result = GetElementsNotInDB(ElementsInternal);
				}
				else if (IsNewElementsPopulated)
				{
					result = GetElementsNotInDB(NewElements);
				}
				else
				{
					result = new ArrayList();
				}

				return result;
			}
		}

		ArrayList GetElementsNotInDB(BusinessObjectCollection collection)
		{
			ArrayList result = new ArrayList();

			foreach (EnterpriseBusinessObject bizO in collection)
			{
				if (!bizO.IsInDatabase)
				{
					result.Add(bizO);
				}
			}

			return result;
		}

		#endregion

		#region Reload Elements from DB

		protected void ReloadFromDB()
		{
			if (IsElementsLoaded)
			{
				RebuildAllElementsFromDB();
			}
			else
			{
				RebuildAllElements();
			}
		}

		#endregion

		#region Implementation

		protected void AddWithoutLoading(BusinessObject bizObj)
		{
			if (IsElementsLoaded)
			{
				ElementsInternal.Add(bizObj);
			}
			else
			{
				NewElements.Add(bizObj);
			}

			if (IsAllElementsLoaded)
			{
				AllElements.Add(bizObj);
			}
		}

		protected void RemoveAndDeleteWithoutLoading(BusinessObject bizObj)
		{
			if (IsElementsLoaded)
			{
				if (ElementsInternal.Contains(bizObj))
				{
					ElementsInternal.RemoveAndDelete(bizObj);
				}
			}
			else if (NewElements.Contains(bizObj))
			{
				NewElements.RemoveAndDelete(bizObj);
			}
		}

		protected virtual BusinessObjectFactory ElementsFactory
		{
			get { return Parent.Factory; }
		}

		BusinessObjectCollection fAllElements;
		BusinessObjectCollection fElementsInternal;
		BusinessObjectCollection fNewElements;
		IBusiness[] currentBusinessObjectsWithRelatedElements;

		#endregion
	}
}
