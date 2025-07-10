using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class ITAddInfoTaxValidationTest : EUAddInfoTaxValidationTest
{
	public void TestCheckDuplicatePortTaxEntered()
	{
		var helper = new ITUniversalReferenceTestDataHelper(Factory);
		helper.SetupHarbourRates();

		const string expectedError = "Port Tax can only be entered once";

		var org = Factory.NewWithValidTestData<OrgHeader>();
		var product = (OrgSupplierPart)MasterFiles.Business.OrgSupplierPart.New(Factory);
		product.OP_PartNum = "POOPY";
		var relationship = product.RelatedOrganisations.AddNew();
		relationship.OU_Relationship = "BTH";
		relationship.OU_OH = org.PK;
		var pivot = product.PivotsForBinding.AddNew();
		pivot.CI_ChildType = Common.ClassificationType.IMP;
		var taxLine = pivot.Taxes.AddNew().Data;

		taxLine.G4_Type = "9AA";
		AssertNoErrorContaining("When only one port tax, no message error expected", taxLine.G4_TypeInfo, expectedError);

		var taxLineDuplicate = pivot.Taxes.AddNew().Data;
		taxLineDuplicate.G4_Type = "9AB";
		AssertHasErrorContaining("When two or more port taxes, message error expected", taxLineDuplicate.G4_TypeInfo, expectedError);
	}
}
