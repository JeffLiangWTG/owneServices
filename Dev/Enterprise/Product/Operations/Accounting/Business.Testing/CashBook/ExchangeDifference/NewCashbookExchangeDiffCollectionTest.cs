using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.ExchangeDifference.Testing
{
	[TestedType(typeof(NewCashbookExchangeDiffCollection))]
	public class NewCashbookExchangeDiffCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new NewCashbookExchangeDiffCollection(Factory.New<NewCashbookExchangeDiffHeader>());
		}

		public override void TestLoad()
		{
			Assert("The NewCashbookExchangeDiff parent does not save in database thus cannot be loaded from the factory.", true);
		}
	}
}
