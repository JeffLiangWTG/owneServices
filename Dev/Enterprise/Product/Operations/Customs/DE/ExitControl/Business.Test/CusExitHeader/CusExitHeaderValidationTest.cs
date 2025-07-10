using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.ExitControl.Business.Testing
{
	sealed class CusExitHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCXH_OA_Carrier()
		{
			const string message = "Carrier is missing";
			DE.Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var carrierAddress = carrier.MainAddress;

			CombineAssertions(() =>
			{
				exitHeaderValidation.ValidateCXH_OA_Carrier();
				AssertNoMessageErrorContaining("CXH_OA_Carrier is empty", exitHeader.CXH_OA_CarrierInfo, message);

				exitHeader.CXH_OA_Carrier = carrierAddress.PK;
				AssertHasMessageErrorContaining("EORI number is empty and branch is empty", exitHeader.CXH_OA_CarrierInfo, message);

				var eorCode = carrier.CustomsCodes.AddNew();
				eorCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Greece;
				eorCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
				eorCode.OK_CustomsRegNo = "EOR001";
				exitHeaderValidation.ValidateCXH_OA_Carrier();
				AssertHasMessageErrorContaining("EORI number isn't empty and branch is empty", exitHeader.CXH_OA_CarrierInfo, message);

				var ebsCode = carrierAddress.CustomsCodes.AddNew();
				ebsCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix;
				ebsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
				ebsCode.OK_CustomsRegNo = "EBS001";
				exitHeaderValidation.ValidateCXH_OA_Carrier();
				AssertNoMessageErrorContaining("EORI number isn't empty and branch isn't empty", exitHeader.CXH_OA_CarrierInfo, message);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			exitHeader = Factory.New<CusExitHeader>();
			exitHeaderValidation = exitHeader.Validation;
		}

		CusExitHeader exitHeader;
		CusExitHeaderValidation exitHeaderValidation;
	}
}
