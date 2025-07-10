using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	[TestedType(typeof(AddEdiCommissionAgreementCompanyAutoAddCountryItem))]
	class AddEdiCommissionAgreementCompanyAutoAddCountryItemTest : NonPersistentBusinessObjectTestCase
	{
	}

	class AddEdiCommissionAgreementCompanyAutoAddCountryItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCountryCode()
		{
			var collection = new AddEdiCommissionAgreementCompanyAutoAddCountryItemCollection(Factory);
			var item1 = collection.AddNew();
			item1.ValidateCountryCode();
			AssertNoErrors(item1.CountryCodeInfo);

			var item2 = collection.AddNew();
			item1.ValidateCountryCode();
			AssertPropertyIsUniqueInCollectionValidationError(item1.CountryCodeInfo, true);

			item1.CountryCode = "ZZ";
			AssertListValidationInvalidCodeError(item1.CountryCodeInfo, true);
			AssertPropertyIsUniqueInCollectionValidationError(item1.CountryCodeInfo, false);

			item1.CountryCode = "AU";
			AssertNoErrors(item1.CountryCodeInfo);

			item2.CountryCode = "AU";
			AssertPropertyIsUniqueInCollectionValidationError(item2.CountryCodeInfo, true);
		}
	}
}
