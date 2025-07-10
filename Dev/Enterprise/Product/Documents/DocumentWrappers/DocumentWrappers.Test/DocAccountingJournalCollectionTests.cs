using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocAccountingJournalCollection))]
	sealed class DocAccountingJournalCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocAccountingJournalCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var glJournal = Factory.NewWithValidTestData<GLJournal>();
			var accountingJournal = new GLAccountingJournal(glJournal, new ReadOnlyBusinessObjectFactory());
			return DocAccountingJournal.New(accountingJournal, Factory);
		}

		protected override DocAccountingJournalCollection GetCollectionToTest()
		{
			return new DocAccountingJournalCollection(Factory);
		}
	}
}
