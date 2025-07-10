using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	[TestedType(typeof(ClientPremiumServiceCollection))]
	internal class ClientPremiumServiceCollectionTest : ActiveBusinessObjectCollectionTestCase<ClientPremiumServiceCollection>
	{
		public void TestRelationshipDefaultsForNewElement()
		{
			var item = Collection.AddNew();
			AssertEquals("Master", Master.PK, item.CPS_LD);
		}

		#region Implementation

		LicenceDatabase Master;

		protected override Type GetExpectedCollectionType()
		{
			return typeof(ClientPremiumServiceCollection);
		}

		protected override ClientPremiumServiceCollection GetCollectionToTest()
		{
			Master = Factory.NewWithValidTestData<LicenceDatabase>();
			return new ClientPremiumServiceCollection(Master);
		}

		#endregion
	}
}
