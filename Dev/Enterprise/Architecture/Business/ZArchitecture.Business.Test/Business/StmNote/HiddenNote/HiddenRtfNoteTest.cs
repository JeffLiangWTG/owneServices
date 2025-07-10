using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class HiddenRtfNoteTest : TestCaseWithFactory
	{
		public void TestSetAndGetBlob()
		{
			AssertEquals("Precondition - DummyWithBlob.Details should be empty.", true, DummyWithBlob.Details.IsEmpty);

			DummyWithBlob.Details = ZBlob.FromAscii("IGWT");
			AssertEquals("DummyWithBlob.Details should be set correctly.", "IGWT", DummyWithBlob.Details.ToAscii());
		}

		public void TestRetrieveBlobFromDB()
		{
			AssertEquals("Precondition - DummyWithBlob.Details should be empty.", true, DummyWithBlob.Details.IsEmpty);

			DummyWithBlob.Details = ZBlob.FromAscii("IGWT");
			DummyWithBlob.Factory.Save();

			var dummyWithBlobFromDB = Factory.Load<DummyWithBlob>(DummyWithBlob.PK);
			AssertEquals("DummyWithBlobFromDB.Details should have been loaded from the DB correctly.", "IGWT", dummyWithBlobFromDB.Details.ToAscii());

			var note = Factory.LoadTop1<HiddenStmNote>(DummyWithBlob.BlobNoteExposed.FilterExposed);
			AssertEquals("Note should have been loaded from the DB with ST_NoteData set correctly.", "IGWT", note.ST_NoteData.ToAscii());
		}

		public void TestSetBlobEmptyDeletesNoteInDB()
		{
			AssertEquals("Precondition - DummyWithBlob.Details should be empty.", true, DummyWithBlob.Details.IsEmpty);

			DummyWithBlob.Details = ZBlob.FromAscii("IGWT");
			DummyWithBlob.Factory.Save();
			var note = Factory.LoadTop1<HiddenStmNote>(DummyWithBlob.BlobNoteExposed.FilterExposed);
			AssertNotNull("DummyWithBlob.Details.IsEmpty = false, an StmNote record should exist in the factory.", note);

			DummyWithBlob.Details = ZBlob.Empty;
			DummyWithBlob.Factory.Save();
			note = Factory.LoadTop1<HiddenStmNote>(DummyWithBlob.BlobNoteExposed.FilterExposed);
			AssertNull("DummyWithBlob.Details.IsEmpty = true, an StmNote record should *not* exist in the factory.", note);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			DummyWithBlob = Factory.New<DummyWithBlob>();
		}

		DummyWithBlob DummyWithBlob;

		#endregion
	}
}
