using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class StmNoteCollectionViewTestCase : TestCaseWithFactory
	{
		public void TestIsRelatedNoteReturnsFalseOnDeletedNote()
		{
			var parent = DummyEnterpriseBusinessObject.New(Factory);
			var collection = new StmNoteCollectionViewForTest(parent);
			var note = Factory.New<StmNote>();
			AssertEquals("Precondition", true, collection.IsRelatedNoteForTesting(note));
			note.Delete();
			AssertEquals(false, collection.IsRelatedNoteForTesting(note));
		}

		class StmNoteCollectionViewForTest : StmNoteCollectionView
		{
			public StmNoteCollectionViewForTest(IStmNoteParent parent)
				: base(parent, typeof(StmNote))
			{
			}

			public bool IsRelatedNoteForTesting(StmNote note) => IsRelatedNote(note);
		}
	}
}
