using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business
{
	[TestedType(typeof(ClientLicenceBillingExcludeOrgCollection))]
	public class ClientLicenceBillingExcludeOrgCollectionTest : ActiveBusinessObjectCollectionTestCase<ClientLicenceBillingExcludeOrgCollection>
	{
		#region Implementation

		protected override ClientLicenceBillingExcludeOrgCollection GetCollectionToTest()
		{
			return new ClientLicenceBillingExcludeOrgCollection(Factory);
		}

		#endregion
	}
}
