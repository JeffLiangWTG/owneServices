using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.DocumentMenu.Testing
{
	sealed class AdditionalNoteProviderTest : TestCaseWithFactory
	{
		public void TestNotDocumentSupportableReturnsEmpty()
		{
			var notDocumentSupportable = Factory.NewWithValidTestData<StmMenuItem>();
			Assert("PRE: Doesnt implement IDocumentSupportable", !(notDocumentSupportable is IDocumentSupportable));

			var previousDbHitCount = Db.Connection.ExecutedCommandCount;
			AssertEquals(0, new AdditionalNoteProvider(notDocumentSupportable).AdditionalNotes.Count());
			AssertEquals("We should avoid hitting the DB if we already know the bizo won't have any DocumentNotes", previousDbHitCount, Db.Connection.ExecutedCommandCount);
		}

		public void TestDocumentSupportableWithNoNoteCreated()
		{
			var documentSupportableWithNoNote = Factory.NewWithValidTestData<OrgHeader>();
			Assert("PRE: Implements IDocumentSupportable", documentSupportableWithNoNote is IDocumentSupportable);

			Factory.Save();

			AssertEquals(0, new AdditionalNoteProvider(documentSupportableWithNoNote).AdditionalNotes.Count());
		}

		public void TestDocumentSupportableWithDocumentNote()
		{
			var bizo = Factory.NewWithValidTestData<OrgHeader>();
			Assert("PRE: Implements IDocumentSupportable", bizo is IDocumentSupportable);

			var note = DocumentNote.LoadNote(bizo);
			Factory.Save();

			AssertEquals(note.PK, new AdditionalNoteProvider(bizo).AdditionalNotes.Single().PK);
		}
	}
}
