using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class SimplifiedLVSJobDocAddressValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOrganisationPK()
		{
			var simplifiedLVS = new SimplifiedLVS(Factory);
			simplifiedLVS.OnlyValidateCargoListHeaderProperty = false;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(simplifiedLVS.SupplierDocumentaryAddress.OrganisationPKInfo, "Vendor");
		}
	}
}
