using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	public class HiddenRtfNote : HiddenNote
	{
		public HiddenRtfNote(IStmNoteParent parent) : base(parent)
		{
		}

		public ZBlob Rtf
		{
			get { return HasNote ? Note.ST_NoteData : ZBlob.Empty; }
			set { Note.ST_NoteData = value; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		protected internal override ZString Description
		{
			get { return "Blob Details"; }
		}

		protected override bool IsEmpty
		{
			get { return HasNote && Note.ST_NoteData.IsEmpty; }
		}
	}
}
