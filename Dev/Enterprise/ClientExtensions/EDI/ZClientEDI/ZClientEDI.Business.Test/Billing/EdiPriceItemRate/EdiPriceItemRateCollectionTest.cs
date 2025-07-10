using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(EdiPriceItemRateCollection))]
	internal class EdiPriceItemRateCollectionTest : ActiveBusinessObjectCollectionTestCase<EdiPriceItemRateCollection>
	{
		public void TestRelationshipDefaultsForNewElement()
		{
			EdiPriceItemRate item = Collection.AddNew();
			AssertEquals("Master", Item1.PK, item.PIR_L7);
		}

		#region Implementation

		ClientLicencePriceHeader PriceHeader;
		ClientLicencePriceItem Item1;

		protected override Type GetExpectedCollectionType()
		{
			return typeof(EdiPriceItemRateCollection);
		}

		protected override EdiPriceItemRateCollection GetCollectionToTest()
		{
			LicenceCompany licenceCompany = Factory.NewWithValidTestData<LicenceCompany>();
			PriceHeader = licenceCompany.PriceHeaders.AddNew();
			Item1 = PriceHeader.Items.AddNew();
			return new EdiPriceItemRateCollection(Item1);
		}

		#endregion
	}
}
