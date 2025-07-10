using System;

namespace Enterprise.ZArchitecture.Business
{
	public class NonDeletableNoteEventArgs : EventArgs
	{
		public NonDeletableNoteEventArgs(string noteDescription) : base()
		{
			fNoteDescription = noteDescription;
		}

		public string NoteDescription
		{
			get { return fNoteDescription; }
		}

		readonly string fNoteDescription;
	}

	public delegate void NonDeletableNoteEventHandler(object sender, NonDeletableNoteEventArgs e);
}
