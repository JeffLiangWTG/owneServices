using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	public class ProxiedNotePropertyManager
	{
		public ProxiedNotePropertyManager(BusinessObject parent, PredefinedNoteType noteType)
			: this(parent, parent.GetNotes(), noteType)
		{
		}

		public ProxiedNotePropertyManager(BusinessObject parent, Notes notesList, PredefinedNoteType noteType)
		{
			this.notesList = notesList;
			this.noteType = noteType;
			StmNote[] notes = notesList.FindByDescription(noteType.Description);
			if (notes.Length > 0)
			{
				note = notes[0];
				parent.RegisterEditableChildObject(note);
			}
		}

		readonly Notes notesList;
		readonly PredefinedNoteType noteType;

		StmNote note;

		public ZString Value
		{
			get
			{
				return note != null && !note.IsDeleted ? note.ST_NoteDataAsText : ZString.Empty;
			}

			set
			{
				if (value.IsEmpty)
				{
					if (note != null)
					{
						note.Delete();
						note = null;
					}
				}
				else
				{
					if (note != null)
					{
						note.ST_NoteDataAsText = value;
					}
					else
					{
						note = notesList.AddNew(noteType.IsCustomNoteType, noteType.Description, value);
					}
				}
			}
		}

		public int MaxLength
		{
			get { return noteType.TextOnlyMaxLength; }
		}
	}
}
