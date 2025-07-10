using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(ClientLicenceFeeCollectionNonDependent))]
	public class ClientLicenceFeeCollectionNonDependentTest : ActiveBusinessObjectCollectionTestCase<ClientLicenceFeeCollectionNonDependent>
	{
		public void TestAdhocCollection()
		{
			var collection = new ClientLicenceFeeCollectionNonDependent(Factory);
			var item1 = Factory.New<ClientLicenceFee>();
			var item2 = Factory.New<ClientLicenceFee>();
			collection.Add(item1);
			collection.Add(item2);
			AssertEquals(2, collection.Count);
		}

		protected override ClientLicenceFeeCollectionNonDependent GetCollectionToTest()
		{
			return new ClientLicenceFeeCollectionNonDependent(Factory);
		}
	}
}
