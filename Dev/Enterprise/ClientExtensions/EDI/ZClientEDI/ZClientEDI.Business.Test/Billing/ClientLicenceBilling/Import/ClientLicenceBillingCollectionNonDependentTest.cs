using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(ClientLicenceBillingCollectionNonDependent))]
	public class ClientLicenceBillingCollectionNonDependentTest : ActiveBusinessObjectCollectionTestCase<ClientLicenceBillingCollectionNonDependent>
	{
		public void TestAdhocCollection()
		{
			var collection = new ClientLicenceBillingCollectionNonDependent(Factory);
			var item1 = Factory.New<ClientLicenceBilling>();
			var item2 = Factory.New<ClientLicenceBilling>();
			collection.Add(item1);
			collection.Add(item2);
			AssertEquals(2, collection.Count);
		}

		protected override ClientLicenceBillingCollectionNonDependent GetCollectionToTest()
		{
			return new ClientLicenceBillingCollectionNonDependent(Factory);
		}
	}
}
