using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocGenericTransactionLineCollection))]
	public class DocGenericTransactionLineCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocGenericTransactionLineCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			DirectPaymentLine line = Factory.New<DirectPaymentLine>();
			return DocGenericTransactionLine.New(line, Factory);
		}

		protected override DocGenericTransactionLineCollection GetCollectionToTest()
		{
			return new DocGenericTransactionLineCollection(Factory);
		}
	}
}
