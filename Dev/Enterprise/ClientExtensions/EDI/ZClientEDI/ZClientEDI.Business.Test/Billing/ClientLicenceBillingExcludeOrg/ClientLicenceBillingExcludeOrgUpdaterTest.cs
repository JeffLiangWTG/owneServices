using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(ClientLicenceBillingExcludeOrgUpdater))]
	public class ClientLicenceBillingExcludeOrgUpdaterTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ClientLicenceBillingExcludeOrgUpdater(Factory);
		}

		#endregion
	}
}
