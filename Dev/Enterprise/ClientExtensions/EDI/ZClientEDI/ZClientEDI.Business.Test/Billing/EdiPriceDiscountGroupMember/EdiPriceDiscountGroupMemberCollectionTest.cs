using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(EdiPriceDiscountGroupMemberCollection))]
	internal class EdiPriceDiscountGroupMemberCollectionTest : ActiveBusinessObjectCollectionTestCase<EdiPriceDiscountGroupMemberCollection>
	{
		public void TestRelationshipDefaultsForNewElement()
		{
			EdiPriceDiscountGroupMember item = Collection.AddNew();
			AssertEquals("Master", PriceHeader.L6_DiscountCode, item.HeaderDiscount.PHD_Version);
		}

		#region Implementation

		ClientLicencePriceHeader PriceHeader;
		EdiPriceHeaderDiscount HeaderDiscount;

		protected override Type GetExpectedCollectionType()
		{
			return typeof(EdiPriceDiscountGroupMemberCollection);
		}

		protected override EdiPriceDiscountGroupMemberCollection GetCollectionToTest()
		{
			LicenceCompany licenceCompany = Factory.NewWithValidTestData<LicenceCompany>();
			PriceHeader = licenceCompany.PriceHeaders.AddNew();
			PriceHeader.L6_DiscountCode = "V22";
			HeaderDiscount = PriceHeader.StlDiscounts.AddNew();
			return new EdiPriceDiscountGroupMemberCollection(PriceHeader);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var bizo = Factory.New<EdiPriceDiscountGroupMember>();
			bizo.PGM_PHD = HeaderDiscount.PK;
			return bizo;
		}

		#endregion
	}
}
