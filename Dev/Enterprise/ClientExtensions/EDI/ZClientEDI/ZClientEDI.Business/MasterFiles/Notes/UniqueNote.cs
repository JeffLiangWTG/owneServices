using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	/// <summary>
	/// Manages a unique StmNote, handling loading and creating, etc.
	/// A unique note has either zero or one instance with a particular description 
	/// in all the notes of the parent biz object.
	/// 
	/// The parent bizO should have overridden NoteTypesCore and
	/// returned a non-empty collection. If not, the StmNote will
	/// be automatically forced to ST_IsCustomDescription = true (see StmNote.Master)
	/// which is not allowed for TextOnly notes.
	/// </summary>
	public class UniqueNote
	{
		public UniqueNote(EnterpriseBusinessObject bizo, PredefinedNoteType noteType)
		{
			this.bizo = bizo;
			this.noteType = noteType;
		}

		//TODO: Here has an edge case that is not covered. Please check WI00678013
		public ZString Text
		{
			get
			{
				EnsureLoaded();
				return note != null && !note.IsDeleted ? note.ST_NoteText : ZString.Empty;
			}
			set
			{
				if (value != Text)
				{
					if (value.IsEmpty)
					{
						if (note != null)
						{
							if (!note.IsDeleted)
							{
								note.Delete();
							}
							note = null;
						}
					}
					else
					{
						if (note == null || note.IsDeleted)
						{
							note = bizo.Notes.AddNew();
							note.ST_Description = noteType.Description;
							note.ST_IsCustomDescription = false;
							note.ST_NoteType = noteType.DefaultVisibility.ToString();
							note.ST_Table = bizo.TableName;
						}

						note.ST_NoteText = value;
					}
				}
			}
		}

		public ZBlob Blob
		{
			get
			{
				EnsureLoaded();
				return note != null && !note.IsDeleted ? note.ST_NoteData : ZBlob.Empty;
			}
			set
			{
				if (value != Blob)
				{
					if (value.IsEmpty || string.IsNullOrEmpty(ORtfTextUtil.RtfToText(value.ToUTF8())))
					{
						if (note != null)
						{
							if (!note.IsDeleted)
							{
								note.Delete();
							}
							note = null;
						}
					}
					else
					{
						if (note == null || note.IsDeleted)
						{
							note = bizo.Notes.AddNew();
							note.ST_Description = noteType.Description;
						}

						note.ST_NoteData = value;
					}
				}
			}
		}

		public bool IsInDatabase
		{
			get
			{
				EnsureLoaded();
				return note != null && note.IsInDatabase;
			}
		}

		public ZGuid NotePK
		{
			get
			{
				EnsureLoaded();
				return note != null ? note.PK : ZGuid.Empty;
			}
		}

		public bool Synchronise()
		{
			bool result = false;

			if (note != null && !note.IsDeleted && (!note.ST_NoteText.IsEmpty || !note.ST_NoteData.IsEmpty))
			{
				ZQuery query = new ZDBOnlyQuery(typeof(StmNote));
				query.AddToFilter(StmNoteSchema.ST_ParentID, note.ST_ParentID);
				query.AddToFilter(StmNoteSchema.ST_Table, note.ST_Table);
				query.AddToFilter(StmNoteSchema.ST_Description, note.ST_Description);
				StmNote[] existingNotes = note.Factory.Load<StmNote>(query);

				if (existingNotes.Length > 0)
				{
					foreach (StmNote existingNote in existingNotes)
					{
						if (existingNote.PK != note.PK)
						{
							existingNote.Delete();
							result = true;
						}
					}
				}
			}

			return result;
		}

		void EnsureLoaded()
		{
			if (!loaded)
			{
				loaded = true;
				note = bizo.Notes.FindByDescription(noteType.Description).FirstOrDefault();
			}
		}

		readonly EnterpriseBusinessObject bizo;
		readonly PredefinedNoteType noteType;
		protected StmNote note;
		bool loaded;

		public bool CopyPersistentValuesTo(StmNote copyTo)
		{
			EnsureLoaded();

			if (note != null)
			{
				copyTo.CopyPersistentValuesFrom(note);
				return true;
			}

			return false;
		}

		public void Delete()
		{
			EnsureLoaded();
			if (note != null)
			{
				note.Delete();
			}
		}

		public bool HasChanges
		{
			get { return note != null && note.HasChanges; }
		}
	}
}

