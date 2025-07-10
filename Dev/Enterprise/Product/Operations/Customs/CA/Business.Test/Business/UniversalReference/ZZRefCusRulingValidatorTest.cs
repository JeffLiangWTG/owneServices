using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ZZRefCusRulingValidatorTest : TestCaseWithFactory
	{
		public void TestValidateAuthorityNumberMatchesRemissionType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var ruling = helper.CreateOrGetRefCusRuling(Core.Constants.CountryCodes.Canada, "C002", "2", ZDate.Today, ZDate.Today.AddDays(1));

			Factory.Save();

			var dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_Code = string.Empty;

			using (dummyBO.SuspendValidationTesting())
			{
				ZZRefCusRulingValidator.ValidateAuthorityNumberMatchesRemissionType(dummyBO.Z0_CodeInfo, "W", ruling);
				AssertHasMessageError(dummyBO.Z0_CodeInfo, ZZRefCusRulingValidator.AuthorityNumberDoesNotMatchRemissionType);

				dummyBO.Z0_CodeInfo.ClearAllNotifications();
				ZZRefCusRulingValidator.ValidateAuthorityNumberMatchesRemissionType(dummyBO.Z0_CodeInfo, "2", ruling);
				AssertNoMessageError(dummyBO.Z0_CodeInfo, ZZRefCusRulingValidator.AuthorityNumberDoesNotMatchRemissionType);
			}
		}

		public void TestValidateSpecialAuthorityNumber()
		{
			var warning = "Special Authority Number not found in Rulings table. Either add a new Ruling (F3) or select a valid Ruling (F4).";
			var error = "Special Authority Number is found but is associated with another organization and cannot be used with this organization.";
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var cusRuling1 = helper.CreateOrGetRefCusRuling(Core.Constants.CountryCodes.Canada, "C001", "2", ZDate.Today, ZDate.Today.AddDays(1));
			cusRuling1.ZZX_OA_AppliesTo = org1.MainAddress.PK;

			var cusRuling2 = helper.CreateOrGetRefCusRuling(Core.Constants.CountryCodes.Canada, "C002", "2", ZDate.Today, ZDate.Today.AddDays(1));

			Factory.Save();

			var dummyBO = Factory.New<DummyBusinessObject>();
			dummyBO.Z0_Code = string.Empty;

			using (dummyBO.SuspendValidationTesting())
			{
				ZZRefCusRulingValidator.ValidateSpecialAuthorityNumber(Factory, dummyBO.Z0_CodeInfo);
				AssertNoWarning(dummyBO.Z0_CodeInfo, warning);
				AssertNoError(dummyBO.Z0_CodeInfo, error);

				dummyBO.Z0_Code = "~";
				ZZRefCusRulingValidator.ValidateSpecialAuthorityNumber(Factory, dummyBO.Z0_CodeInfo);
				AssertHasWarning(dummyBO.Z0_CodeInfo, warning);

				dummyBO.Z0_Code = "C001";
				ZZRefCusRulingValidator.ValidateSpecialAuthorityNumber(Factory, dummyBO.Z0_CodeInfo, org1.PK);
				AssertNoWarning(dummyBO.Z0_CodeInfo, warning);
				AssertNoError(dummyBO.Z0_CodeInfo, error);

				ZZRefCusRulingValidator.ValidateSpecialAuthorityNumber(Factory, dummyBO.Z0_CodeInfo, org2.PK);
				AssertHasError(dummyBO.Z0_CodeInfo, error);

				dummyBO.Z0_Code = "C002";
				ZZRefCusRulingValidator.ValidateSpecialAuthorityNumber(Factory, dummyBO.Z0_CodeInfo, org1.PK);
				AssertNoError(dummyBO.Z0_CodeInfo, error);

				ZZRefCusRulingValidator.ValidateSpecialAuthorityNumber(Factory, dummyBO.Z0_CodeInfo, org2.PK);
				AssertNoError(dummyBO.Z0_CodeInfo, error);
			}
		}
	}
}
