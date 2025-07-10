using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;

using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public abstract class HiddenNote
	{
		protected HiddenNote(IStmNoteParent parent)
			: this(parent, true)
		{
		}

		protected HiddenNote(IStmNoteParent parent, bool enableFetchHint)
		{
			this.Parent = parent;
			if (enableFetchHint)
			{
				IFetchHint hint = new HiddenNoteFetchHint(this);
				Parent.NotesFactory.AddFetchHint(hint);
			}
			Parent.NotesFactory.Saving += new BusinessObjectFactory.SavingEventHandler(Factory_Saving);
		}

		public void Delete()
		{
			if (HasNote) // Must not change to note - during delete note may not yet be initialised
			{
				Note.Delete();
				note = null;
			}
		}

		public bool HasNote
		{
			get
			{
				EnsureDeletedNoteIsNull();
				if (note == null)
				{
					note = GetExistingNote();
				}
				return note != null;
			}
		}

		public bool HasLoadedNote
		{
			get
			{
				EnsureDeletedNoteIsNull();
				return note != null;
			}
		}

		void EnsureDeletedNoteIsNull()
		{
			if (note != null && note.IsDeleted)
			{
				note = null;
			}
		}

		HiddenStmNote GetExistingNote()
		{
			ZQuery filter = GetExistingNoteQuery();
			return Parent.NotesFactory.LoadTop1<HiddenStmNote>(filter);
		}

		public virtual ZQuery GetExistingNoteQuery()
		{
			ZQuery filter = new ZQuery();
			filter.FetchOnlyFromLocalCache = !((BusinessObject)Parent).IsInDatabase;
			filter.AddToFilter(StmNoteSchema.ST_ParentID, Parent.NotesParentPK);
			filter.AddToFilter(StmNoteSchema.ST_Table, Parent.NotesParentTableName);
			filter.AddToFilter(StmNoteSchema.ST_Description, DescriptionWrapper);
			filter.AddToFilter(StmNoteSchema.ST_NoteType, nameof(StmNoteVisibility.DOC));
			return filter;
		}

		protected HiddenStmNote Note
		{
			get
			{
				if (note == null)
				{
					note = GetExistingNote();
					if (note == null)
					{
						note = (HiddenStmNote)Parent.NotesFactory.New(typeof(HiddenStmNote));
						note.SuspendValidation();
						note.ST_ParentID = Parent.NotesParentPK;
						note.ST_Table = Parent.NotesParentTableName;
						note.ST_Description = DescriptionWrapper;
					}
				}

				return note;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Message are only sent back to us")]
		ZString DescriptionWrapper
		{
			get
			{
				if (Description == ZString.Empty)
				{
					string message = "Description of HiddenNote (type=" + this.GetType().Name + ") cannot be null or Empty";
#if DEBUG
					throw new ArgumentException(message);
#else
					string key = "[CS00168944 - Assign to EGI] " + message;
					CargoWise.Common.ErrorReporter.ReportOnce(key, message);
#endif
				}
				return Description;
			}
		}

		protected internal abstract ZString Description
		{
			get;
		}

		protected abstract bool IsEmpty
		{
			get;
		}

		protected virtual void Factory_Saving(BusinessObjectFactory factory)
		{
			EnsureDeletedNoteIsNull();
			if (note != null && IsEmpty)
			{
				Delete();
			}
		}

		protected internal readonly IStmNoteParent Parent;
		HiddenStmNote note;

		public void FetchForDelete()
		{
			if (HasNote)
			{
				Note.FetchStrategy.FetchForDelete();
				Parent.NotesFactory.AddFetchHint(StmUniversalCopySchema.SUC_CopyObjectId, Note.PK);
			}
		}
	}
}
