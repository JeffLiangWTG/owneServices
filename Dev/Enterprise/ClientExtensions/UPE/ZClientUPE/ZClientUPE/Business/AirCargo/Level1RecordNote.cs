using System;
using CargoWise.Types;
using Enterprise.Customs.SG.Access.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.UPE.Business
{
	public class Level1RecordNote
	{
		public Level1RecordNote(UPECusHAWB cusHAWB)
		{
			this.CusHAWB = cusHAWB;
		}
		public Level1RecordNote(AsycudaBill asycudaBill)
		{
			this.AsycudaBill = asycudaBill;
		}

		readonly UPECusHAWB CusHAWB;
		readonly AsycudaBill AsycudaBill;

		public ZString Text
		{
			get
			{
				StmNote note = this.Note;
				return note == null ? ZString.Empty : note.ST_NoteText;
			}
			set
			{
				StmNote note = this.Note;
				if (value.IsEmpty && note != null)
				{
					note.Delete();
					note = null;
				}
				else
				{
					if (note == null)
					{
						if (CusHAWB != null)
						{
							note = CusHAWB.Notes.AddNew(false, UPEPredefinedNoteTypes.Instance.Level1Record.Description, string.Empty);
						}
						else if (AsycudaBill != null)
						{
							note = AsycudaBill.Notes.AddNew(false, UPEPredefinedNoteTypes.Instance.Level1Record.Description, string.Empty);
						}
					}
					note.ST_IsCustomDescription = false;
					note.ST_NoteText = value;
				}
			}
		}

		#region Implementation

		StmNote Note
		{
			get
			{
				if (CusHAWB != null)
				{
					StmNote[] notes = CusHAWB.Notes.FindByDescription(UPEPredefinedNoteTypes.Instance.Level1Record.Description);
					return (notes.Length == 0) ? null : notes[0];
				}
				else if (AsycudaBill != null)
				{
					try
					{
						StmNote[] notes = AsycudaBill.Notes.FindByDescription(UPEPredefinedNoteTypes.Instance.Level1Record.Description);
						return (notes.Length == 0) ? null : notes[0];
					}
					catch (InvalidCastException)
					{ return null; }
				}
				return null;
			}
		}

		#endregion
	}
}
