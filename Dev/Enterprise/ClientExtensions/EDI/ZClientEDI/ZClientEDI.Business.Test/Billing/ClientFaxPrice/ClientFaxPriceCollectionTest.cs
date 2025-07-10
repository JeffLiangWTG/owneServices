using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business
{
	[TestedType(typeof(ClientFaxPriceCollection))]
	public class ClientFaxPriceCollectionTest : ActiveBusinessObjectCollectionTestCase<ClientFaxPriceCollection>
	{
		public void TestDefaultsForNewElementCore()
		{
			BulkClientFaxPriceUpdater updater = new BulkClientFaxPriceUpdater(Factory);
			ClientFaxPriceCollection collection = new ClientFaxPriceCollection(Factory, updater);
			updater.MonthAndYearPeriod = new ZDateTime(2013, 7, 26);

			ClientFaxPrice price = collection.AddNew();
			AssertEquals(7, price.CFP_Month.ToZInt());
			AssertEquals(2013, price.CFP_Year.ToZInt());
		}

		public void TestHasChanges()
		{
			ClientFaxPriceCollection collection = new ClientFaxPriceCollection(Factory);
			Assert("Has no changes", !collection.HasChanges);

			ClientFaxPrice price = collection.AddNew();
			Assert("Has changes due to new item", collection.HasChanges);

			Factory.Save();
			Assert("Has no changes after a successful save", !collection.HasChanges);

			price.CFP_RX_NKCurrencyCode = "AUD";
			Assert("Has changes due to edited item", collection.HasChanges);

			Factory.Save();
			Assert("Has no changes after a successful save", !collection.HasChanges);

			price.Delete();
			Assert("Has changes due to deleted item", collection.HasChanges);

			Factory.Save();
			Assert("Has no changes after a successful save", !collection.HasChanges);
		}

		#region Implementation

		protected override ClientFaxPriceCollection GetCollectionToTest()
		{
			return new ClientFaxPriceCollection(Factory);
		}

		#endregion
	}
}
