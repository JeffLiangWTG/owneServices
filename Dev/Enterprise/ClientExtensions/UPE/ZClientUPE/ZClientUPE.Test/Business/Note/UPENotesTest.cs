using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(UPENotes))]
	public class UPENotesTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAddNewNote()
		{
			var cusHawb = Factory.NewWithValidTestData<UPECusHAWB>(TestBusinessObjectKind.MinimumRequiredToSave);
			var note1 = cusHawb.Notes.AddNew(false, UPEPredefinedNoteTypes.Instance.Level1Record.Description, "");
			note1.ST_NoteText = "Note1";
			var note2 = cusHawb.Notes.AddNew(false, UPEPredefinedNoteTypes.Instance.Level1Record.Description, "");
			note2.ST_NoteText = "Note2";

			AssertEquals(note1.PK, note2.PK);
			var notes = cusHawb.Notes.FindByDescription(UPEPredefinedNoteTypes.Instance.Level1Record.Description);
			AssertEquals("There should be 1 Level1Record Note", 1, notes.Length);
			AssertEquals("Level1Record Note Content", note2.ST_NoteText, notes[0].ST_NoteText);

			Factory.Save();
			var note3 = cusHawb.Notes.AddNew(false, UPEPredefinedNoteTypes.Instance.Level1Record.Description, "");
			note3.ST_NoteText = "Note3";

			AssertEquals(note1.PK, note3.PK);
			notes = cusHawb.Notes.FindByDescription(UPEPredefinedNoteTypes.Instance.Level1Record.Description);
			AssertEquals("There should be 1 Level1Record Note", 1, notes.Length);
			AssertEquals("Level1Record Note Content", note3.ST_NoteText, notes[0].ST_NoteText);

			var testNote1 = cusHawb.Notes.AddNew(true, "TestNote", "");
			var testNote2 = cusHawb.Notes.AddNew(true, "TestNote", "");

			var testNotes = cusHawb.Notes.FindByDescription("TestNote");
			AssertEquals("There should be 2 TestNotes", 2, testNotes.Length);
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new UPENotes(Factory.New<UPECusHAWB>());
		}
	}
}
