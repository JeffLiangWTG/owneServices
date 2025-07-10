using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.Testing
{
	[TestedType(typeof(WithholdingJournalForDisplayCollection))]
	class WithholdingJournalForDisplayCollectionTest : NonPersistentBusinessObjectCollectionTestCase<WithholdingJournalForDisplayCollection>
	{
		public void TestCollectionIsNotEditable()
		{
			var collection = GetCollectionToTest();
			Assert("AllowNew", !collection.AllowNew);
			Assert("AllowRemove", !collection.AllowRemove);
		}

		protected override WithholdingJournalForDisplayCollection GetCollectionToTest()
		{
			return new WithholdingJournalForDisplayCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new WithholdingJournalForDisplay(Factory.NewWithValidTestData<APJournal>(), ZDate.Empty);
		}
	}
}
