using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(ClientLicenceFeeFlattenedCollection))]
	public class ClientLicenceFeeFlattenedCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ClientLicenceFeeFlattenedCollection>
	{
		protected override ClientLicenceFeeFlattenedCollection GetCollectionToTest()
		{
			return new ClientLicenceFeeFlattenedCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ClientLicenceFeeFlattened();
		}
	}
}
