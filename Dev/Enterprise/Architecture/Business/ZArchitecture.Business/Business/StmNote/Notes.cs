using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public class Notes : ZLogsOrNotes
	{
		public Notes(IStmNoteParent parent)
			: base((BusinessObject)parent)
		{
			if (Factory != null && Parent.IsInDatabase)
			{
				Factory.AddFetchHint(StmNoteSchema.ST_ParentID, Parent.NotesParentPK);
			}
		}

		public virtual new StmNote AddNew()
		{
			return (StmNote)base.AddNew();
		}

		public virtual StmNote AddNew(bool isCustomDescription, ZString description, ZString noteText)
		{
			StmNote note = (StmNote)base.AddNew();
			note.ST_IsCustomDescription = isCustomDescription;
			note.ST_Description = description;
			note.ST_NoteDataAsText = noteText;

			return note;
		}

		public virtual StmNote AddNew(bool isCustomDescription, ZString description, FormattedRtfString rtfString)
		{
			StmNote note = (StmNote)base.AddNew();
			note.ST_IsCustomDescription = isCustomDescription;
			note.ST_Description = description;
			note.ST_NoteData = ZBlob.FromUTF8(rtfString.ToRtf());

			return note;
		}

		/// <summary>
		/// This event is fired whenever a Note is added to a business object.
		/// The event will contain the actual Note object added in it's event args.
		/// </summary>
		public event NoteAddedEventHandler NoteAdded
		{
			add { VisibleNotes.NoteAddedInternal += value; }
			remove { VisibleNotes.NoteAddedInternal -= value; }
		}

		public new IStmNoteParent Parent
		{
			get { return (IStmNoteParent)base.Parent; }
		}

		#region ZLogsOrNotes Overrides

		protected override BusinessObject[] GetBusinessObjectsWithRelatedElements()
		{
			var result = Parent.BusinessObjectsWithRelatedNotes;
			if (Factory != null && Parent.IsInDatabase)
			{
				foreach (var element in result)
				{
					if (element != null)
					{
						Factory.AddFetchHint(StmNoteSchema.ST_ParentID, element is IStmNoteParent stmNoteParent ? stmNoteParent.NotesParentPK : element.PK);
					}
				}
			}
			return result;
		}

		protected override SchemaColumn ElementForeignKeyColumn
		{
			get { return StmNoteSchema.ST_ParentID; }
		}

		protected override Type ElementType
		{
			get { return typeof(StmNote); }
		}

		protected override BusinessObjectFactory ElementsFactory
		{
			get { return Parent.NotesFactory; }
		}

		protected override ZQuery GetRelatedElementsFilter(BusinessObject relatedBizO)
		{
			if (relatedBizO == null)
			{
				throw new NullReferenceException("GetRelatedElementsFilter(BusinessObject RelatedBizO), RelatedBizO was null.");
			}

			return new StmNoteNonPublicNotesWithContextQuery(relatedBizO, GetNoteContextsForRelatedBizObject(relatedBizO));
		}

		protected override ZQuery RebuildAllElementsInternalQuery
		{
			get
			{
				ZQuery query = new ZQuery(ElementForeignKeyColumn, Parent.NotesParentPK);
				query.AddToFilter(StmNoteSchema.ST_Table, Parent.NotesParentTableName);
				query.ReLoadExistingRows = true;
				return query;
			}
		}

		protected override BusinessObjectCollection GetNewElementsCollection()
		{
			return new StmNoteCollection(Parent, ((BusinessObject)Parent).Factory);
		}

		protected override BusinessObjectCollection GetNewAllElementsCollection()
		{
			return new StmNoteCollectionWithRelatedElements(Parent);
		}

		protected override bool IsAllElementsUpdatedByDataRefresh => true;

		protected override IBusinessObjectCollectionView GetNewVisibleElementsCollectionView()
		{
			return new StmNoteCollectionView(Parent);
		}

		protected override BusinessObjectCollection GetElementsCollectionFromRelatedBizObject(BusinessObject relatedBizObject)
		{
			if (relatedBizObject == null)
			{
				throw new NullReferenceException("GetElementsCollectionFromRelatedBizObject(BusinessObject relatedBizObject), relatedBizObject was null.");
			}

			var relatedBizObjectAsNoteParent = relatedBizObject as IStmNoteParent
				?? throw new InvalidCastException("GetElementsCollectionFromRelatedBizObject(BusinessObject relatedBizObject), relatedBizObject must be an IStmNoteParent");

			var relatedContext = GetNoteContextsForRelatedBizObject(relatedBizObject);
			var results = relatedBizObjectAsNoteParent.Notes.NonPrivateNotesWithContext(relatedContext, base.Parent.PK);

			return results;
		}

		protected virtual StmNoteContexts GetNoteContextsForRelatedBizObject(BusinessObject relatedBizObject)
		{
			return Parent.NoteContextsForRelatedNotes;
		}

		internal StmNoteCollectionViewNonPrivateNotesWithContext NonPrivateNotesWithContext(StmNoteContexts validContexts, ZGuid parentPKToExcludeNotes)
		{
			if (nonPrivateNotesWithContext == null)
			{
				nonPrivateNotesWithContext = new StmNoteCollectionViewNonPrivateNotesWithContext(this, validContexts, parentPKToExcludeNotes);
			}
			else if (nonPrivateNotesWithContext.ValidNoteContexts != validContexts)
			{
				nonPrivateNotesWithContext.ValidNoteContexts = validContexts;
				nonPrivateNotesWithContext.Rebuild();
			}

			return nonPrivateNotesWithContext;
		}

		StmNoteCollectionViewNonPrivateNotesWithContext nonPrivateNotesWithContext;

		#endregion

		#region Visible Notes

		/// <summary>
		/// THIS IS ONLY VISIBLE FOR BINDING. PLEASE DO NOT USE DIRECTLY.
		/// </summary>
		public ZBool ShowRelatedNotes
		{
			get { return VisibleNotes.ShowRelatedNotes; }
			set { VisibleNotes.ShowRelatedNotes = value; }
		}

		/// <summary>
		/// THIS IS ONLY VISIBLE FOR BINDING. PLEASE DO NOT USE DIRECTLY.
		/// </summary>
		public ZBool ShowNotesForAllCompanies
		{
			get { return VisibleNotes.ShowNotesForAllCompanies; }
			set { VisibleNotes.ShowNotesForAllCompanies = value; }
		}

		public bool ShowNotesForAllCompanies_ReadOnly
		{
			get { return !EnvProxy.Instance.Security.NotesViewAllCompany.IsAllowed; }
		}

		/// <summary>
		/// THIS IS ONLY VISIBLE FOR BINDING. PLEASE DO NOT USE DIRECTLY.
		/// </summary>
		[BusinessObjectTestExclude]
		public StmNoteCollectionView VisibleNotes
		{
			get { return (StmNoteCollectionView)VisibleElements; }
		}

		#endregion

		#region Client Visible Notes

		/// <summary>
		/// Notes with Visibility set to Public. Mostly used in Web projects and web services
		/// </summary>
		public StmNote[] ClientVisibleNotes
		{
			get { return FindByVisibility(StmNoteVisibility.PUB); }
		}

		#endregion Client Visible Notes

		#region Has Notes / Has Related Notes

		/// <summary>
		/// Does the business object have notes in the Factory / DB?
		/// </summary>
		public bool HasNotes
		{
			get
			{
				bool result;

				if (IsElementsLoaded)
				{
					result = ElementsInternal.Count > 0;
				}
				else
				{
					// check if we have at least one note in the factory / DB
					ZQuery query = new ZQuery(StmNoteSchema.ST_ParentID, Parent.NotesParentPK);
					query.AddToFilter(StmNoteSchema.ST_Table, Parent.NotesParentTableName);
					query.FetchOnlyFromLocalCache = !((BusinessObject)Parent).IsInDatabase;
					query.AddToFilter(new StmNoteQuery(), JoinCondition.And);
					result = (ElementsFactory.LoadTop1(typeof(StmNote), query) != null);
				}

				return result;
			}
		}

		/// <summary>
		/// Do any of the related business objects have notes in the Factory / DB?
		/// </summary>
		public bool HasRelatedNotes
		{
			get { return RelatedElements.Count > 0; }
		}

		/// <summary>
		/// Do any of the related business objects, that are visible currently (due to Show Notes for all Companies) have notes in the Factory / DB?
		/// </summary>
		public bool HasVisibleRelatedNotes
		{
			get { return GetAllRelatedNotesVisibleToCurrentCompany().Any(); }
		}

		#endregion

		#region Getting / Searching for Notes

		public BusinessObjectCollection GetAllNotes()
		{
			return ElementsInternal;
		}

		public IEnumerable<StmNote> GetAllNotesVisibleToCurrentCompany()
		{
			return ElementsInternal.Cast<StmNote>().Where(n => n.ST_GC_RelatedCompany.IsEmpty || n.ST_GC_RelatedCompany == EnvProxy.Instance.CurrentCompany.PK);
		}

		public IEnumerable<StmNote> GetAllRelatedNotesVisibleToCurrentCompany()
		{
			return ShowNotesForAllCompanies ? RelatedElements.Cast<StmNote>() : RelatedElements.Cast<StmNote>().Where(n => CheckRelatedElementState(n) && (n.ST_GC_RelatedCompany.IsEmpty || n.ST_GC_RelatedCompany == EnvProxy.Instance.CurrentCompany.PK));
		}

		bool CheckRelatedElementState(StmNote relatedElement)
		{
			if (relatedElement.IsRowDeletedOrDetachedOrNull)
			{
				var parentCollections = ((IBusinessObjectInternals)relatedElement).ParentCollections;
				StringBuilder stringBuilder = new StringBuilder().Append((NoResString)"dataRowState: ")
					.AppendLine(((IBusinessObjectInternals)relatedElement).Row.RowState.ToString())
					.Append((NoResString)"parent: ")
					.AppendLine(Parent.NotesParentTableName)
					.Append((NoResString)"relatedElement.ParentCollections: ")
					.AppendLine(parentCollections?.Length.ToString());

				if (parentCollections != null)
				{
					foreach (var parentCollection in parentCollections)
					{
						if (parentCollection.Contains(relatedElement))
						{
							stringBuilder.Append((NoResString)"relatedElement.ParentCollection type: ")
								.AppendLine(parentCollection.GetType()?.Name)
								.Append((NoResString)"relatedElement.ParentCollection.CompleteFilter: ")
								.AppendLine(parentCollection.CompleteFilter?.LiteralTextADO);
						}
					}
				}
				ErrorReporter.ReportDeveloperExceptionOnce("RelatedElement has been deleted", stringBuilder.ToString(), new InvalidOperationException("RelatedElement has been deleted"));
				return false;
			}
			return true;
		}

		public StmNote FindByPK(ZGuid notePK)
		{
			StmNote result;

			if (IsElementsLoaded)
			{
				result = (StmNote)ElementsInternal.FindByPK(notePK);
			}
			else
			{
				if (notePK.IsValid)
				{
					ZQuery filter = new ZQuery();
					filter.AddToFilter(StmNoteSchema.PK, notePK);
					filter.AddToFilter(ElementsInternalFilter);
					StmNote[] possibleNotes = (StmNote[])ElementsFactory.Load(ElementType, filter);
					result = (possibleNotes.Length == 0) ? null : possibleNotes[0];
				}
				else
				{
					result = null;
				}
			}

			if (result != null)
			{
				result.Master = Parent;
			}
			return result;
		}

		public StmNote[] FindByDescription(string description, bool includeRelatedNotes, bool fallBack = true)
		{
			return FindByDescription(description, includeRelatedNotes, SQLComparisonOperator.Equal, fallBack);
		}

		public StmNote[] FindByDescription(string description, bool includeRelatedNotes, SQLComparisonOperator comparisonOperator, bool fallBack = true)
		{
			StmNote[] result;

			if (description == null)
			{
				result = Array.Empty<StmNote>();
			}
			else
			{
				var noteType = Parent.NoteTypes.NoteTypeByDescription(description);
				if (noteType != null)
				{
					description = noteType.MultilingualDescription.GetUnresolvedString();
				}
				ZQuery descriptionQuery = new ZQuery(StmNoteSchema.ST_Description, comparisonOperator, description);
				result = Find(descriptionQuery);
				if (includeRelatedNotes && result.Length == 0 && fallBack)
				{
					result = (StmNote[])RelatedElements.Find(descriptionQuery);
				}
				else if (includeRelatedNotes && !fallBack)
				{
					result = result.Concat((StmNote[])RelatedElements.Find(descriptionQuery)).ToArray();
				}
			}

			return result;
		}

		public StmNote[] FindByDescription(string description)
		{
			return FindByDescription(description, false);
		}

		public StmNote[] FindByVisibility(StmNoteVisibility visibility)
		{
			if (visibility == StmNoteVisibility.DOC)
			{
				throw new ArgumentException("Cannot search for Visibility.DOC as BusinessObject.Notes can never contain DOC notes.");
			}

			StmNote[] result = Find(new ZQuery(StmNoteSchema.ST_NoteType, visibility.ToString()));
			SortBySystemCreateTimeDescending(result);

			return result;
		}

		StmNote[] Find(ZQuery filter)
		{
			StmNote[] result;

			if (IsElementsLoaded)
			{
				result = (StmNote[])ElementsInternal.Find(filter);
				SortBySystemCreateTimeDescending(result);
			}
			else
			{
				ZQuery clonedFilter = filter.ShallowClone();
				clonedFilter.AddToFilter(ElementsInternalFilter);
				clonedFilter.FetchOnlyFromLocalCache = !((BusinessObject)Parent).IsInDatabase;
				result = (StmNote[])ElementsFactory.Load(ElementType, clonedFilter);
				foreach (StmNote note in result)
				{
					note.Master = Parent;
				}
			}

			return result;
		}

		void SortBySystemCreateTimeDescending(StmNote[] notes)
		{
			Array.Sort(notes, (x, y) => y.ST_SystemCreateTimeUtc.CompareTo(x.ST_SystemCreateTimeUtc));
		}

		#endregion

		#region Security

		public bool HasSecurityToDeleteNotes
		{
			get { return EnvProxy.Instance.Security.NotesDelete.IsAllowed; }
		}

		public bool HasAllNoteSecurity
		{
			get
			{
				ISecurityProxy security = EnvProxy.Instance.Security;
				return security.Notes.IsAllowed &&
					security.NotesDelete.IsAllowed &&
					security.NotesEdit.IsAllowed &&
					security.NotesNew.IsAllowed &&
					security.NotesNewCustomNote.IsAllowed;
			}
		}

		#endregion

		#region Fetch hint

		public void AddFetchHintForVisibleNotes()
		{
			foreach (StmNote note in VisibleNotes)
			{
				note.Factory.AddFetchHint(GlbStaffSchema.GS_Code, note.ST_SystemCreateUser);
			}
		}

		#endregion
	}
}
