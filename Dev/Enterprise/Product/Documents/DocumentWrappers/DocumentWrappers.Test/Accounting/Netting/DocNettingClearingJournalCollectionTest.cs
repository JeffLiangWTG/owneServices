using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Netting;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocNettingClearingJournalCollection))]
	sealed class DocNettingClearingJournalCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocNettingClearingJournalCollection>
	{
		protected override DocNettingClearingJournalCollection GetCollectionToTest()
		{
			return DocNettingClearingJournalCollection.New(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return DocNettingClearingJournal.New(new NettingClearingJournal(), Factory);
		}
	}
}
