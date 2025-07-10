using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.NotesNoteCollection))]
	sealed class NotesNoteCollectionTest : ValueObjectCollectionTestCase
	{
		public void TestCompileTimeCheck()
		{
			Xsd.NotesNoteCollection value = null;
			value = new Xsd.Notes().Note;
			AssertNotNull("The line above was probably commented out", value);
		}

		public void TestGetNote()
		{
			Xsd.NotesNoteCollection collection = new Xsd.NotesNoteCollection();

			Xsd.NotesNote note1 = collection.AddNew();
			note1.NoteType = Xsd.NotesNoteNoteType.FaxEmailTransmissionLog;

			Xsd.NotesNote note2 = collection.AddNew();
			note2.NoteType = Xsd.NotesNoteNoteType.HandlingInstructions;

			Xsd.NotesNote note3 = collection.AddNew();
			note3.NoteType = Xsd.NotesNoteNoteType.MarksAndNumbers;

			AssertEquals("Correct note type should be returned", note2, collection.GetNote(Xsd.NotesNoteNoteType.HandlingInstructions));
			AssertEquals("Correct note type should be returned", note1, collection.GetNote(Xsd.NotesNoteNoteType.FaxEmailTransmissionLog));
			AssertEquals("Correct note type should be returned", note3, collection.GetNote(Xsd.NotesNoteNoteType.MarksAndNumbers));
			AssertEquals("No note should be returned", null, collection.GetNote(Xsd.NotesNoteNoteType.OutturnNotes));
		}

		public void TestAppendToNote()
		{
			Xsd.NotesNoteCollection collection = new Xsd.NotesNoteCollection();
			Xsd.NotesNote existingNote = collection.AddNew();
			existingNote.NoteType = Xsd.NotesNoteNoteType.MarksAndNumbers;
			existingNote.NoteData = "The first line";

			AssertEquals("Precondition: existing note count", 1, collection.Count);
			AssertNotNull("Should have existing Marks and numbers note", collection.GetNote(Xsd.NotesNoteNoteType.MarksAndNumbers));

			collection.AppendToNote(Xsd.NotesNoteNoteType.MarksAndNumbers, " and the second line");
			AssertEquals("Collection should still only have one note", 1, collection.Count);
			AssertEquals("Note data should have existing + new line", "The first line and the second line", collection.GetNote(Xsd.NotesNoteNoteType.MarksAndNumbers).NoteData);
		}

		public void TestAppendToNoteWithNoExistingNote()
		{
			Xsd.NotesNoteCollection collection = new Xsd.NotesNoteCollection();
			Xsd.NotesNote existingNote = collection.AddNew();
			existingNote.NoteType = Xsd.NotesNoteNoteType.MarksAndNumbers;
			existingNote.NoteData = "Marks and Numbers Note";

			AssertEquals("Precondition: existing note count", 1, collection.Count);
			AssertNotNull("Should have existing Marks and numbers note", collection.GetNote(Xsd.NotesNoteNoteType.MarksAndNumbers));

			collection.AppendToNote(Xsd.NotesNoteNoteType.DetailedGoodsDescription, "Detailed Goods Description Note");
			AssertEquals("Collection should now have two notes", 2, collection.Count);
			AssertEquals("Note data for marks and numbers", "Marks and Numbers Note", collection.GetNote(Xsd.NotesNoteNoteType.MarksAndNumbers).NoteData);
			AssertEquals("Note data for detailed godos description", "Detailed Goods Description Note", collection.GetNote(Xsd.NotesNoteNoteType.DetailedGoodsDescription).NoteData);
		}
	}
}
