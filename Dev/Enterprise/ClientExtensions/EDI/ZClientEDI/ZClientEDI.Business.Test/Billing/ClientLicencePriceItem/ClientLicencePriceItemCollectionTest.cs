using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(ClientLicencePriceItemCollection))]
	internal class ClientLicencePriceItemCollectionTest : ActiveBusinessObjectCollectionTestCase<ClientLicencePriceItemCollection>
	{
		public void TestFindByCode()
		{
			AssertNull(Collection.FindByCode(""));
			AssertNull(Collection.FindByCode(null));
			AssertNull(Collection.FindByCode("CCC"));

			Collection.AddNew().L7_Code = "AAA";
			Collection.AddNew().L7_Code = "BBB";

			AssertNull(Collection.FindByCode(""));
			AssertNull(Collection.FindByCode(null));
			AssertNull(Collection.FindByCode("CCC"));
			AssertEquals("AAA", Collection.FindByCode("AAA").L7_Code);
			AssertEquals("BBB", Collection.FindByCode("BBB").L7_Code);
		}

		public void TestRelationshipDefaultsForNewElement()
		{
			ClientLicencePriceItem item = Collection.AddNew();
			AssertEquals("Master", PriceHeader.PK, item.L7_L6);
		}

		#region Implementation

		ClientLicencePriceHeader PriceHeader;

		protected override Type GetExpectedCollectionType()
		{
			return typeof(ClientLicencePriceItemCollection);
		}

		protected override ClientLicencePriceItemCollection GetCollectionToTest()
		{
			LicenceCompany licenceCompany = Factory.NewWithValidTestData<LicenceCompany>();
			PriceHeader = licenceCompany.PriceHeaders.AddNew();
			return new ClientLicencePriceItemCollection(PriceHeader);
		}

		#endregion
	}
}
