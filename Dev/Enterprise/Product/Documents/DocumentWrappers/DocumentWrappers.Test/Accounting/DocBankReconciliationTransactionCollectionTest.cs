using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.CashBook;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocBankReconciliationTransactionCollection))]
	sealed class DocBankReconciliationTransactionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocBankReconciliationTransactionCollection>
	{
		protected override DocBankReconciliationTransactionCollection GetCollectionToTest()
		{
			return new DocBankReconciliationTransactionCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return DocBankReconciliationTransaction.New(Factory.NewWithValidTestData<BankReconTransaction>(), Factory);
		}
	}
}
