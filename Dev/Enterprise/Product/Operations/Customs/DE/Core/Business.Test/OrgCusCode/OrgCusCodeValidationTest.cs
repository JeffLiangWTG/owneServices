using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	class OrgCusCodeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOK_CodeType_APITypeRequiresEBSTypeWithSameAddress()
		{
			const string errorMessage = "Code Type 'API' requires also a record of Type 'EBS' for the same Premises Address.";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = orgHeader.CustomsCodes.AddNew();

			CombineAssertions(() =>
			{
				cusCode.OK_OA_PremisesAddress = orgHeader.MainAddress.PK;
				cusCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber;
				AssertHasError("No EBS type code", cusCode.OK_CodeTypeInfo, errorMessage);

				var cusCode2 = CreateCusCodeWithPremisesAddress(orgHeader, GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, "1111");
				cusCode.Validation.ValidateOK_CodeType();
				AssertHasError("Has EBS type code without same address", cusCode.OK_CodeTypeInfo, errorMessage);

				cusCode2.OK_OA_PremisesAddress = orgHeader.MainAddress.PK;
				cusCode.Validation.ValidateOK_CodeType();
				AssertNoError("Has EBS type code with same address", cusCode.OK_CodeTypeInfo, errorMessage);
			});
		}

		public void TestCheckOK_CodeType_EBSTypeRequiresEORType()
		{
			const string errorMessage = "Code Type 'EBS' requires also a record of Type 'EOR'.";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = orgHeader.CustomsCodes.AddNew();

			CombineAssertions(() =>
			{
				cusCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix;
				AssertHasError("No EOR type code", cusCode.OK_CodeTypeInfo, errorMessage);

				var cusCode2 = CreateCusCode(orgHeader, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "1111");
				cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Greece;
				cusCode.Validation.ValidateOK_CodeType();
				AssertNoError("Has EOR type code of GR", cusCode.OK_CodeTypeInfo, errorMessage);

				cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Italy;
				cusCode.Validation.ValidateOK_CodeType();
				AssertNoError("Has EOR type code of IT", cusCode.OK_CodeTypeInfo, errorMessage);
			});
		}

		public void TestCheckOK_CodeType_EPITypeRequiresTENType()
		{
			const string errorMessage = "You have not entered a Trader Excise Number (TEN).";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = orgHeader.CustomsCodes.AddNew();

			CombineAssertions(() =>
			{
				cusCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.EMCSParticipantIdentificationNumber;
				AssertHasError("No TEN type code", cusCode.OK_CodeTypeInfo, errorMessage);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, "1111", Core.Constants.CountryCodes.Greece);
				cusCode.Validation.ValidateOK_CodeType();
				AssertNoError("Has TEN type code", cusCode.OK_CodeTypeInfo, errorMessage);
			});
		}

		public void TestCheckOK_CodeType_WPITypeRequiresTIDTypeWithSameAddress()
		{
			const string errorMessage = "You have not entered a Trader ID (TID) for Premises Address.";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = orgHeader.CustomsCodes.AddNew();

			CombineAssertions(() =>
			{
				cusCode.OK_OA_PremisesAddress = orgHeader.MainAddress.PK;
				cusCode.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.EMCSWarehouseParticipantIdentificationNumber;
				AssertHasError("No TID type code", cusCode.OK_CodeTypeInfo, errorMessage);

				var cusCode2 = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID, "1111", Core.Constants.CountryCodes.Greece);
				cusCode2.OK_OA_PremisesAddress = orgHeader.Addresses.AddNew().PK;
				cusCode.Validation.ValidateOK_CodeType();
				AssertHasError("Has TID type code without same address", cusCode.OK_CodeTypeInfo, errorMessage);

				cusCode2.OK_OA_PremisesAddress = orgHeader.MainAddress.PK;
				cusCode.Validation.ValidateOK_CodeType();
				AssertNoError("Has TID type code with same address", cusCode.OK_CodeTypeInfo, errorMessage);
			});
		}

		OrgCusCode CreateCusCode(OrgHeader orgHeader, ZString codeType, ZString regNo)
		{
			var result = orgHeader.CustomsCodes.AddNew();
			result.OK_CodeType = codeType;
			result.OK_CustomsRegNo = regNo;
			return result;
		}

		OrgCusCode CreateCusCodeWithPremisesAddress(OrgHeader orgHeader, ZString codeType, ZString regNo)
		{
			var result = CreateCusCode(orgHeader, codeType, regNo);
			result.OK_OA_PremisesAddress = orgHeader.Addresses.AddNew().PK;
			return result;
		}
	}
}
