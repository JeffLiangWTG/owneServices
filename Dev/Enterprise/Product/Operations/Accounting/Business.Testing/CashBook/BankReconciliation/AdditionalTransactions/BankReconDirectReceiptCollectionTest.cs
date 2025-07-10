using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.Testing
{
	[TestedType(typeof(BankReconDirectReceiptCollection))]
	public class BankReconDirectReceiptCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new BankReconDirectReceiptCollection(Factory);
		}
	}
}
