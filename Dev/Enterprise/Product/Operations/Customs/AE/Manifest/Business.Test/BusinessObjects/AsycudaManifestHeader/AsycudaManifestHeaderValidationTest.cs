using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

sealed class AsycudaManifestHeaderValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckAMA_RL_NKPortOfFirstArrival_IsNotMandatory() 
	{
		var header = Factory.New<AsycudaManifestHeader>();
		ValidationTestHelper.AssertFieldIsNotMandatory(header.AMA_RL_NKPortOfFirstArrivalInfo);
	}

	public void TestCheckAMA_OA_Carrier() => CombineAssertions(() =>
	{
		var header = Factory.New<AsycudaManifestHeader>();
		var org = Factory.New<OrgHeader>();
		var orgAddress = org.Addresses.AddNew();
		AEManifestValidationHelper.AssertOrgHasContactInfo(orgAddress, header.AMA_OA_CarrierInfo, header.Validation.ValidateAMA_OA_Carrier);
	});

	public void TestCheckAMA_OA_Carrier_AssertOrgHasMPC() => CombineAssertions(() =>
	{
		var errorMessage = "Carrier Requires a 'MPCI Party Id (MPC)' Registration Code to be Configured";
		var header = Factory.New<AsycudaManifestHeader>();
		var orgHeader = Factory.New<OrgHeader>();
		header.AMA_OA_Carrier = orgHeader.PK;
		AssertNoMessageErrors(header.AMA_OA_CarrierInfo);
		var orgAddress = orgHeader.Addresses.AddNew();
		header.AMA_OA_Carrier = orgAddress.PK;
		AssertHasMessageErrorContaining(header.AMA_OA_CarrierInfo, errorMessage);

		var code = orgHeader.CustomsCodes.AddNew();
		code.OK_CodeType = OrgCusCode.UnitedArabEmiratesCodeTypes.MPCINumber;
		code.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedArabEmirates;
		code.OK_CustomsRegNo = "123";
		header.Validation.ValidateAMA_OA_Carrier();
		AssertNoMessageErrorContaining(header.AMA_OA_CarrierInfo, errorMessage);
	});

	public void TestCheckAMA_VoyageIsNotMandatory()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		ValidationTestHelper.AssertFieldIsNotMandatory(header.AMA_VoyageInfo);
	}
}
