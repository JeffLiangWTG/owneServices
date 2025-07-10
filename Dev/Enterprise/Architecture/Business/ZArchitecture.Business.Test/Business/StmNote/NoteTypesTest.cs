using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class NoteTypesTest : TestCase
	{
		public void TestAdd()
		{
			AssertEquals(0, Notes.Count);

			Notes.Add(PredefinedNoteTypes.Instance.DetailedGoodsDescription);
			AssertEquals(1, Notes.Count);
			AssertNotNull(Notes.NoteTypeByDescription(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description));
		}

		public void TestAddWithCollection()
		{
			AssertEquals(0, Notes.Count);
			NoteTypeCollection tempColl = new NoteTypeCollection();
			tempColl.Add(PredefinedNoteTypes.Instance.AgentNotes);
			tempColl.Add(PredefinedNoteTypes.Instance.AutoRatingAuditLog);
			tempColl.Add(PredefinedNoteTypes.Instance.BookingNotes);

			Notes.Add(tempColl);
			AssertEquals(3, Notes.Count);
			AssertNotNull(Notes.NoteTypeByDescription(PredefinedNoteTypes.Instance.AgentNotes.Description));
			AssertNotNull(Notes.NoteTypeByDescription(PredefinedNoteTypes.Instance.AutoRatingAuditLog.Description));
			AssertNotNull(Notes.NoteTypeByDescription(PredefinedNoteTypes.Instance.BookingNotes.Description));
		}

		public void TestIsOnlyOneAllowedForDescription()
		{
			Notes.Add(PredefinedNoteTypes.Instance.DetailedGoodsDescription);
			AssertEquals(true, Notes.IsOnlyOneAllowedForDescription(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description));
			AssertEquals(false, Notes.IsOnlyOneAllowedForDescription("asdyhuio;"));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			Notes = new NoteTypeCollection();
		}

		NoteTypeCollection Notes;

		#endregion
	}
}
