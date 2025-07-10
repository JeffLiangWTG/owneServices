using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(EdiPriceHeaderDiscountCollection))]
	internal class EdiPriceHeaderDiscountCollectionTest : ActiveBusinessObjectCollectionTestCase<EdiPriceHeaderDiscountCollection>
	{
		public void TestRelationshipDefaultsForNewElement()
		{
			EdiPriceHeaderDiscount item = Collection.AddNew();
			AssertEquals("Master", PriceHeader.L6_DiscountCode, item.PHD_Version);
		}

		#region Implementation

		ClientLicencePriceHeader PriceHeader;

		protected override Type GetExpectedCollectionType()
		{
			return typeof(EdiPriceHeaderDiscountCollection);
		}

		protected override EdiPriceHeaderDiscountCollection GetCollectionToTest()
		{
			LicenceCompany licenceCompany = Factory.NewWithValidTestData<LicenceCompany>();
			PriceHeader = licenceCompany.PriceHeaders.AddNew();
			PriceHeader.L6_DiscountCode = "V22";
			return new EdiPriceHeaderDiscountCollection(PriceHeader);
		}

		#endregion
	}
}
