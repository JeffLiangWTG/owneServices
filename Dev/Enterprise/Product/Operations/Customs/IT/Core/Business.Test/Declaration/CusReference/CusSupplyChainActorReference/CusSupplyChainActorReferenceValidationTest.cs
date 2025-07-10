using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class CusSupplyChainActorReferenceValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCFR_Reference_Mandatory_OwnerWithNoCusCodes()
	{
		var orgHeader = Factory.New<OrgHeader>();
		var cusSupplyChainActorReference = Factory.New<CusSupplyChainActorReference>();
		orgHeader.OH_Code = "ABCZXY";
		const string errorMessage = "The selected Owner does not contain an EOR code";
		CombineAssertions(() =>
		{
			cusSupplyChainActorReference.CFR_OA_Owner = orgHeader.MainAddress.PK;
			cusSupplyChainActorReference.Validation.ValidateCFR_Reference();
			AssertHasErrorContaining("Has error message", cusSupplyChainActorReference.CFR_ReferenceInfo, errorMessage);
			cusSupplyChainActorReference.CFR_Reference = "EOR1";
			AssertNoErrorContaining("No error message2", cusSupplyChainActorReference.CFR_ReferenceInfo, errorMessage);
		});
	}
}
