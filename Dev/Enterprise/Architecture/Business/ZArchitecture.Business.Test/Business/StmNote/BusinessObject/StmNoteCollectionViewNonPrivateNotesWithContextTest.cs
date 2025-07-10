using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(StmNoteCollectionViewNonPrivateNotesWithContext))]
	sealed class StmNoteCollectionViewNonPrivateNotesWithContextTest : BusinessObjectCollectionViewTestCase<StmNoteCollectionViewNonPrivateNotesWithContext>
	{
		protected override StmNoteCollectionViewNonPrivateNotesWithContext GetCollectionToTest()
		{
			var parent = Factory.New<DummyBizOWithRelatedNotes>();
			return new StmNoteCollectionViewNonPrivateNotesWithContext(new Notes(parent), StmNoteContexts.Default, ZGuid.Empty);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<StmNote>();
		}
	}
}
