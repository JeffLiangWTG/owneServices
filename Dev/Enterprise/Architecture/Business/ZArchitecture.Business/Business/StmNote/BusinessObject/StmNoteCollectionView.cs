using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Business
{
	public class StmNoteCollectionView : BusinessObjectCollectionView<StmNote>
	{
		public StmNoteCollectionView(IStmNoteParent parent)
			: this(parent, typeof(StmNote))
		{
		}

		public StmNoteCollectionView(IStmNoteParent parent, Type typeOfElements)
			: base(parent.Notes.AllElements)
		{
			if (!typeof(StmNote).IsAssignableFrom(typeOfElements))
			{
				throw new ArgumentException("StmNoteCollectionView can only wrap a collection of StmNotes or its sub-classes.");
			}

			this.Parent = parent;
			fTypeOfElements = typeOfElements;
			fShowRelatedNotes = true;
			Rebuild();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return fTypeOfElements;
		}

		readonly Type fTypeOfElements;

		/// <summary>
		/// Event fired when a new note is added. Contains a reference to the added Note.
		/// </summary>
		internal event NoteAddedEventHandler NoteAddedInternal;

		protected override void RebuildOnConstruction()
		{
			// don't rebuild as we have not yet set this.Parent
		}

		protected override bool AllowNewCore
		{
			get { return EnvProxy.Instance.Security.NotesNew.IsAllowed; }
		}

		#region Deleting Notes

		public event EventHandler OnAttemptedToDeleteRelatedNote;
		public event NonDeletableNoteEventHandler OnAttemptedToDeleteNonDeletableNote;

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			if (elementToDelete == null)
			{
				throw new ArgumentNullException(nameof(elementToDelete), "RemoveAndDelete(BusinessObject ElementToDelete), ElementToDelete was null.");
			}

			StmNote note = (StmNote)elementToDelete;

			if (IsRelatedNote(note))
			{
				if (OnAttemptedToDeleteRelatedNote != null)
				{
					OnAttemptedToDeleteRelatedNote(this, EventArgs.Empty);
				}
			}
			else if (!IsDeletingAllNotes && note.IsReadOnlyAfterAdd)
			{
				if (OnAttemptedToDeleteNonDeletableNote != null)
				{
					OnAttemptedToDeleteNonDeletableNote(this, new NonDeletableNoteEventArgs(note.ST_Description));
				}
			}
			else
			{
				base.RemoveAndDelete(elementToDelete);
			}
		}

		public override void RemoveAndDeleteAll()
		{
			IsDeletingAllNotes = true;
			base.RemoveAndDeleteAll();
		}

		bool IsDeletingAllNotes;

		#endregion

		#region Rebuilding the View

		public ZBool ShowRelatedNotes
		{
			get { return fShowRelatedNotes; }
			set
			{
				if (fShowRelatedNotes != value)
				{
					fShowRelatedNotes = value;
					Rebuild();
				}
			}
		}

		public ZBool ShowNotesForAllCompanies
		{
			get { return fShowNotesForAllCompanies; }
			set
			{
				if (fShowNotesForAllCompanies != value)
				{
					fShowNotesForAllCompanies = value;
					Rebuild();
				}
			}
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			if (element == null)
			{
				throw new ArgumentNullException(nameof(element), "IsThisPartOfTheCollection(BusinessObject Element), Element was null.");
			}

			return (ShowRelatedNotes || !IsRelatedNote((StmNote)element)) && HasPermissionToViewNote((StmNote)element);
		}

		bool HasPermissionToViewNote(StmNote note)
		{
			return ShowNotesForAllCompanies || !note.IsInDatabase || note.ST_GC_RelatedCompany.IsEmpty ||
				note.ST_GC_RelatedCompany == EnvProxy.Instance.CurrentCompany.PK;
		}

		ZBool fShowRelatedNotes;
		ZBool fShowNotesForAllCompanies; //= true; // Will set to false when loading Notes form on GUI.

		#endregion

		#region Sync stuff happening in the *View* with the *Collection*

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			if (child == null)
			{
				throw new ArgumentNullException(nameof(child), "SetCollectionRelationships(BusinessObject Child), Child was null.");
			}

			base.SetCollectionRelationships(child);

			if (!IsRelatedNote((StmNote)child))
			{
				((ILegacyBusinessObjectCollectionInternals)Parent.Notes.ElementsInternal).SetCollectionRelationships(child);
			}
		}

		protected override void RemoveCollectionRelationshipsCore(BusinessObject child, bool forDelete)
		{
			if (child == null)
			{
				throw new ArgumentNullException(nameof(child), "RemoveCollectionRelationships(BusinessObject Child), Child was null.");
			}

			base.RemoveCollectionRelationshipsCore(child, forDelete);

			if (!IsRelatedNote((StmNote)child) && HasPermissionToViewNote((StmNote)child))
			{
				Parent.Notes.ElementsInternal.RemoveCollectionRelationships(child, forDelete);
				if (Parent.Notes.ElementsInternal.Contains(child))
				{
					((System.ComponentModel.IBindingList)Parent.Notes.ElementsInternal).Remove(child);
					if (forDelete)
					{
						((IBusinessObjectCollectionInternals)Parent.Notes.ElementsInternal).HasChangesFromDelete = true;
					}
				}
			}
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			if (child == null)
			{
				throw new ArgumentNullException(nameof(child), "SetDefaultsForNewChild(BusinessObject Child), Child was null.");
			}

			base.SetDefaultsForNewChild(child);

			Parent.Notes.ElementsInternal.SetupNewElementButDoNotAddIt(child, true);
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			if (bizOAdded == null)
			{
				throw new ArgumentNullException(nameof(bizOAdded), "OnAdded(BusinessObject BizOAdded), BizOAdded was null.");
			}

			base.OnAdded(bizOAdded);

			if (IsRelatedNote((StmNote)bizOAdded) || (!((StmNote)bizOAdded).IsBelongingToCurrentLoginCompany && !EnvProxy.Instance.Security.NotesModifyAllCompany.IsAllowed))
			{
				bizOAdded.ReadOnly = true;
			}
			else if (!Parent.Notes.ElementsInternal.Contains(bizOAdded))
			{
				Parent.Notes.ElementsInternal.Add(bizOAdded);
			}

			if (NoteAddedInternal != null)
			{
				NoteAddedInternal(new NoteAddedEventArgs((StmNote)bizOAdded));
			}
		}

		#endregion

		#region Implementation

		public IStmNoteParent Parent { get; private set; }

		protected virtual bool IsRelatedNote(StmNote note)
		{
			return (!note.IsDeleted && note.ST_ParentID != Parent.NotesParentPK);
		}

		#endregion
	}

	#region Note Added Event Information

	/// <summary>
	/// Event Arguments to use for the NoteAdded event.
	/// </summary>
	public class NoteAddedEventArgs : EventArgs
	{
		public NoteAddedEventArgs(StmNote noteAdded)
		{
			this.NoteAdded = noteAdded;
		}

		public StmNote NoteAdded;
	}

	/// <summary>
	/// Event Handler for the NoteAdded event.
	/// </summary>
	public delegate void NoteAddedEventHandler(NoteAddedEventArgs args);

	#endregion
}
