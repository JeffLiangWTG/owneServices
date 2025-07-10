using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.DocumentWrappers.Accounting.TaxFramework;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocAccountingJournalTaxDetailCollection))]
	sealed class DocAccountingJournalTaxDetailCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocAccountingJournalTaxDetailCollection>
	{
		protected override DocAccountingJournalTaxDetailCollection GetCollectionToTest()
		{
			return DocAccountingJournalTaxDetailCollection.New(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var mock = new Mock<IAccountingJournalTaxDetail>();
			return DocAccountingJournalTaxDetail.New(mock.Object, Factory);
		}
	}
}
