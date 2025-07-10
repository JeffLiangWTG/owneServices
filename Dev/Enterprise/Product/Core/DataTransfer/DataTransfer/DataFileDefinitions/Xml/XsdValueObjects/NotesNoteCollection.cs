using System.Xml.Serialization;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	public class NotesNoteCollection : Xsd.AutoNotesNoteCollection
	{
		public Xsd.NotesNote GetNote(Xsd.NotesNoteNoteType noteType)
		{
			foreach (Xsd.NotesNote note in this)
			{
				if (note.NoteType == noteType)
				{
					return note;
				}
			}
			return null;
		}

		/// <summary>
		/// Appends value to note of type NoteType. If that note doesn't exist, it will be created.
		/// </summary>
		public void AppendToNote(Xsd.NotesNoteNoteType noteType, string value)
		{
			Xsd.NotesNote matchingNote = GetNote(noteType);
			if (matchingNote == null)
			{
				matchingNote = AddNew();
				matchingNote.NoteType = noteType;
			}

			matchingNote.NoteData += value;
		}
	}
}
