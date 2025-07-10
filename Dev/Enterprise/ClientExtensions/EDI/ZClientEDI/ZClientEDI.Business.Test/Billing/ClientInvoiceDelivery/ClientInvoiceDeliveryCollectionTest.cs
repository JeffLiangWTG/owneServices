using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(ClientInvoiceDeliveryCollection))]
	internal class ClientInvoiceDeliveryCollectionTest : ActiveBusinessObjectCollectionTestCase<ClientInvoiceDeliveryCollection>
	{
		public void TestRelationshipDefaultsForNewElement()
		{
			ClientInvoiceDelivery billing = Collection.AddNew();
			AssertEquals("Master", Master.PK, billing.L9_LC);
		}

		public void TestFindByServerAndSystem()
		{
			ClientInvoiceDelivery billing1 = AddServerAndSystem(Collection, "SV1", "AAA");
			AssertEquals(billing1.PK, Collection.FindByServerAndSystem("SV1", "AAA").PK);
			AssertNull("no match", Collection.FindByServerAndSystem("SV2", "AAA"));
			AssertNull("no match", Collection.FindByServerAndSystem("SV1", "BBB"));

			ClientInvoiceDelivery billing2 = AddServerAndSystem(Collection, "", "AAA");
			AssertEquals(billing2.PK, Collection.FindByServerAndSystem("SV2", "AAA").PK);

			ClientInvoiceDelivery billing3 = AddServerAndSystem(Collection, "SV2", BillingConstants.BillingSystem.All);
			ClientInvoiceDelivery billing4 = AddServerAndSystem(Collection, "", BillingConstants.BillingSystem.All);

			AssertEquals(billing1.PK, Collection.FindByServerAndSystem("SV1", "AAA").PK);
			AssertEquals(billing4.PK, Collection.FindByServerAndSystem("SV1", "BBB").PK);
			AssertEquals(billing3.PK, Collection.FindByServerAndSystem("SV2", "BBB").PK);
			AssertEquals(billing2.PK, Collection.FindByServerAndSystem("SV3", "AAA").PK);
			AssertEquals(billing4.PK, Collection.FindByServerAndSystem("SV3", "CCC").PK);
		}

		ClientInvoiceDelivery AddServerAndSystem(ClientInvoiceDeliveryCollection collection, string server, string system)
		{
			ClientInvoiceDelivery result = collection.AddNew();
			result.L9_ServerCode = server;
			result.L9_SystemCode = system;
			return result;
		}

		#region Implementation

		LicenceCompany Master;

		protected override Type GetExpectedCollectionType()
		{
			return typeof(ClientInvoiceDeliveryCollection);
		}

		protected override ClientInvoiceDeliveryCollection GetCollectionToTest()
		{
			Master = Factory.NewWithValidTestData<LicenceCompany>();
			return new ClientInvoiceDeliveryCollection(Master);
		}

		#endregion
	}
}
