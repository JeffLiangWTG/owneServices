using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class HiddenTextNoteTest : TestCaseWithFactory
	{
		public void TestSetAndGetText()
		{
			AssertEquals("Precondition - DummyWithText.Details should be empty.", true, DummyWithText.Details.IsEmpty);

			DummyWithText.Details = "IGWT";
			AssertEquals("DummyWithText.Details should be set correctly.", "IGWT", DummyWithText.Details);
		}

		public void TestRetrieveTextFromDB()
		{
			AssertEquals("Precondition - DummyWithText.Details should be empty.", true, DummyWithText.Details.IsEmpty);

			DummyWithText.Details = "IGWT";
			DummyWithText.Factory.Save();

			var dummyWithTextFromDB = Factory.Load<DummyWithText>(DummyWithText.PK);
			AssertEquals("DummyWithTextFromDB.Details should have been loaded from the DB correctly.", "IGWT", dummyWithTextFromDB.Details);

			var note = Factory.LoadTop1<HiddenStmNote>(DummyWithText.TextNoteExposed.FilterExposed);
			AssertEquals("Note should have been loaded from the DB with ST_NoteText set correctly.", "IGWT", note.ST_NoteText);
		}

		public void TestSetTextEmptyDeletesNoteInDB()
		{
			AssertEquals("Precondition - DummyWithText.Details should be empty.", true, DummyWithText.Details.IsEmpty);

			DummyWithText.Details = "IGWT";
			DummyWithText.Factory.Save();
			var note = Factory.LoadTop1<HiddenStmNote>(DummyWithText.TextNoteExposed.FilterExposed);
			AssertNotNull("DummyWithText.Details.IsEmpty = false, an StmNote record should exist in the factory.", note);

			DummyWithText.Details = "";
			DummyWithText.Factory.Save();
			note = Factory.LoadTop1<HiddenStmNote>(DummyWithText.TextNoteExposed.FilterExposed);
			AssertNull("DummyWithText.Details.IsEmpty = true, an StmNote record should *not* exist in the factory.", note);
		}

		public void TestDescription()
		{
			var bizo = Factory.New<DummyEnterpriseBusinessObject>();
			HiddenTextNote note = new HiddenTextNote(bizo, "some note");
			AssertEquals("some note", note.Description);

			note = new HiddenTextNote(bizo);
			AssertEquals("Text Details", note.Description);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			DummyWithText = Factory.New<DummyWithText>();
		}

		DummyWithText DummyWithText;

		#endregion
	}
}
