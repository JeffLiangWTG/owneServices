using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	sealed class CusExitHeaderValidationBaseOnlyTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCXH_OA_Carrier()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(Factory.New<CusExitHeader>().CXH_OA_CarrierInfo);
		}

		public void TestCheckCXH_OA_Carrier_EORI()
		{
			var exitHeader = Factory.New<CusExitHeader>();
			var targetInfo = exitHeader.CXH_OA_CarrierInfo;

			var eoriRequiredMessage = "The selected Organization must have a valid EORI code.";
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			exitHeader.CXH_OA_Carrier = carrier.MainAddress.PK;
			AssertHasMessageError("EORI code needed.", targetInfo, eoriRequiredMessage);

			carrier.SetCustomsCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, Factory.Load<RefCountry>(Core.Constants.CountryGuids.Germany), "DE1234567");
			exitHeader.Validation.ValidateCXH_OA_Carrier();
			AssertNoMessageError("EORI code needed(valid code, validation passes).", targetInfo, eoriRequiredMessage);
		}

		public void TestCheckCXH_GB_Branch()
		{
			var thisCompany = Factory.New<GlbCompany>();
			thisCompany.GC_Code = "ABC";
			var otherCompany = Factory.New<GlbCompany>();
			otherCompany.GC_Code = "UVW";
			var exitHeader = Factory.New<CusExitHeader>();
			exitHeader.CXH_GC_Company = thisCompany.PK;
			var validbranch = thisCompany.Branches.AddNew();
			validbranch.GB_Code = "DEF";
			var invalidBranch = otherCompany.Branches.AddNew();
			invalidBranch.GB_Code = "XYZ";
			Factory.Save();

			CombineAssertions(() =>
			{
				var messageText = "The selected branch does not belong to the same company as the job.";
				exitHeader.CXH_GB_Branch = invalidBranch.PK;
				AssertHasError("Branch is not a member of the exit header company", exitHeader.CXH_GB_BranchInfo, messageText);

				exitHeader.CXH_GB_Branch = ZGuid.Invalid;
				AssertNoError("Branch is a member of the exit header company", exitHeader.CXH_GB_BranchInfo, messageText);
				AssertHasErrorContaining("Branch is not a member of the exit header company", exitHeader.CXH_GB_BranchInfo, ListValidation.InvalidCodeError);
				AssertNoErrorContaining("Branch is not a member of the exit header company", exitHeader.CXH_GB_BranchInfo, MandatoryValidation.MustBeEntered);

				exitHeader.CXH_GB_Branch = ZGuid.Empty;
				AssertNoError("Branch is a member of the exit header company", exitHeader.CXH_GB_BranchInfo, messageText);
				AssertNoErrorContaining("Branch is not a member of the exit header company", exitHeader.CXH_GB_BranchInfo, ListValidation.InvalidCodeError);
				AssertHasErrorContaining("Branch is not a member of the exit header company", exitHeader.CXH_GB_BranchInfo, MandatoryValidation.MustBeEntered);

				exitHeader.CXH_GB_Branch = validbranch.PK;
				AssertNoErrors("Branch is a member of the exit header company", exitHeader.CXH_GB_BranchInfo);
			});
		}
	}
}
