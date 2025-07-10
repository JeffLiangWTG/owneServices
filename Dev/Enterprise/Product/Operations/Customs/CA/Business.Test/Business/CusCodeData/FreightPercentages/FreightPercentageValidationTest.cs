using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class FreightPercentageValidationTest : CusCodeDataValidationTest
	{
		public new void TestCheckCY_Code()
		{
			var freightPercentage1 = OrgImpAddInfo.FreightPercentages.AddNew();
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(freightPercentage1.CY_CodeInfo, "???", "ROA");
			freightPercentage1.CY_Code = "ROA";
			AssertNoError(freightPercentage1.CY_CodeInfo, "The Type has been duplicated and must be unique.");
			var freightPercentage2 = OrgImpAddInfo.FreightPercentages.AddNew();
			freightPercentage2.CY_Code = "ROA";
			AssertHasError(freightPercentage2.CY_CodeInfo, "The Type has been duplicated and must be unique.");
			freightPercentage1.Validation.ValidateCY_Code();
			AssertHasError(freightPercentage1.CY_CodeInfo, "The Type has been duplicated and must be unique.");
		}

		OrgHeader OrgHeader
		{
			get { return orgHeader ?? (orgHeader = Factory.New<OrgHeader>()); }
		}
		OrgHeader orgHeader;

		OrgImpAddInfo OrgImpAddInfo
		{
			get { return orgImpAddInfo ?? (orgImpAddInfo = OrgImpAddInfo.Get(OrgHeader)); }
		}
		OrgImpAddInfo orgImpAddInfo;
	}
}
