using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business
{
	public class HiddenTextNote : HiddenNote
	{
		public HiddenTextNote(IStmNoteParent parent, string description = null) : this(parent, true, description)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Internaly identifier not shown to users.")]
		public HiddenTextNote(IStmNoteParent parent, bool enableFetchHint, string description = null) : base(parent, enableFetchHint)
		{
			this.description = description ?? "Text Details";
		}

		readonly string description;

		public ZString Text
		{
			get { return HasNote ? Note.ST_NoteText : ZString.Empty; }
			set { Note.ST_NoteText = value; }
		}

		protected internal override ZString Description
		{
			get { return description; }
		}

		protected override bool IsEmpty
		{
			get { return Note.ST_NoteText.IsEmpty; }
		}
	}
}
