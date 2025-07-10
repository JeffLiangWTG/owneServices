using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using CustomsConstants = Enterprise.Core.Constants.Customs.Universal;
using TestHelper = Enterprise.Customs.DE.Business.Testing.TestHelper;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	class CusTempStorageLineValidationBaseOnlyTest : BusinessObjectValidationTestCase
	{
		public void TestCheckTSL_CustodianIdentifier_Mandatory()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageLine.TSL_CustodianIdentifierInfo);
		}

		public void TestCheckTSL_CustodianIdentifier_EoriMismatch()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var storageHeader = Factory.New<CusTempStorageJobHeader>();
			storageHeader.SJH_OH_Customer = organisation.PK;
			var storageDec = CUSPRLCusTempStorageDec.New(storageHeader);
			storageDec.CusTempStorageLines.Add(storageLine);

			TestHelper.CreateCL010CoutryList(Factory);
			CusTempStorageLineValidationTestHelper.AssertEORINumberMatchesOrganizationsEORINumber(storageLine.TSL_CustodianIdentifierInfo, storageLine.TSL_OA_CustodianInfo, "Custodian");
		}

		public void TestCheckTSL_CustodianIdentifier_EoriMissing()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var storageHeader = Factory.New<CusTempStorageJobHeader>();
			storageHeader.SJH_OH_Customer = organisation.PK;
			var storageDec = CUSPRLCusTempStorageDec.New(storageHeader);
			storageDec.CusTempStorageLines.Add(storageLine);

			TestHelper.CreateCL010CoutryList(Factory);
			CusTempStorageLineValidationTestHelper.AssertEORINumberExistsInOrganization(storageLine.TSL_CustodianIdentifierInfo, () => storageLine.Validation.ValidateTSL_CustodianIdentifier(), storageLine.TSL_OA_CustodianInfo);
		}

		public void TestCheckTSL_CustodianIdentifierBranchNo_Mandatory()
		{
			const string warning = "Branch should be captured if known.";
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertNoWarningIfNotEntered(storageLine.TSL_CustodianIdentifierBranchNoInfo, warning, "CustodianIdentifier is empty");
				storageLine.TSL_CustodianIdentifier = "ABC";
				ValidationTestHelper.AssertWarningIfNotEntered(storageLine.TSL_CustodianIdentifierBranchNoInfo, warning, "CustodianIdentifier not empty");
			});
		}

		public void TestCheckTSL_CustodianIdentifierBranchNo_4Digits()
		{
			CusTempStorageLineValidationTestHelper.AssertEORIBranchHas4Digits(storageLine.TSL_CustodianIdentifierBranchNoInfo);
		}

		public void TestCheckTSL_CustodianIdentifierBranchNo_MismatchAddressesBranchNo()
		{
			CusTempStorageLineValidationTestHelper.AssertEORIBranchEqualsAddressesEORIBranch(storageLine.TSL_CustodianIdentifierBranchNoInfo, storageLine.TSL_OA_CustodianInfo);
		}

		public void TestCheckTSL_CustodianIdentifierBranchNo_PremissesAddressHasEoriBranchNo()
		{
			CusTempStorageLineValidationTestHelper.AssertEORIBranchExistsInPremiseAddress(storageLine.TSL_CustodianIdentifierBranchNoInfo, () => storageLine.Validation.ValidateTSL_CustodianIdentifierBranchNo(), storageLine.TSL_OA_CustodianInfo);
		}

		public void TestCheckTSL_GoodsOwnerIdentifier_Mandatory()
		{
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertFieldIsNotMandatory(storageLine.TSL_GoodsOwnerIdentifierInfo, MandatoryValidation.YouHaveNotEntered, "No GoodsOwner");

				var orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_Code = "DETEST";
				storageLine.TSL_OA_GoodsOwner = orgHeader.MainAddress.PK;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageLine.TSL_GoodsOwnerIdentifierInfo, MandatoryValidation.YouHaveNotEntered, "Has GoodsOwner, empty TSL_GoodsOwnerIdentifier");
			});
		}

		public void TestCheckTSL_GoodsOwnerIdentifier_EoriMismatch()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var storageHeader = Factory.New<CusTempStorageJobHeader>();
			storageHeader.SJH_OH_Customer = organisation.PK;
			var storageDec = CUSPRLCusTempStorageDec.New(storageHeader);
			storageDec.CusTempStorageLines.Add(storageLine);
			TestHelper.CreateCL010CoutryList(Factory);

			CusTempStorageLineValidationTestHelper.AssertEORINumberMatchesOrganizationsEORINumber(storageLine.TSL_GoodsOwnerIdentifierInfo, storageLine.TSL_OA_GoodsOwnerInfo, "Disposal Entitled Trader");
		}

		public void TestCheckTSL_GoodsOwnerIdentifier_EoriMissing()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var storageHeader = Factory.New<CusTempStorageJobHeader>();
			storageHeader.SJH_OH_Customer = organisation.PK;
			var storageDec = CUSPRLCusTempStorageDec.New(storageHeader);
			storageDec.CusTempStorageLines.Add(storageLine);

			TestHelper.CreateCL010CoutryList(Factory);
			CusTempStorageLineValidationTestHelper.AssertEORINumberExistsInOrganization(storageLine.TSL_GoodsOwnerIdentifierInfo, () => storageLine.Validation.ValidateTSL_GoodsOwnerIdentifier(), storageLine.TSL_OA_GoodsOwnerInfo);
		}

		public void TestCheckTSL_GoodsOwnerIdentifierBranchNo_4Digits()
		{
			CusTempStorageLineValidationTestHelper.AssertEORIBranchHas4Digits(storageLine.TSL_GoodsOwnerIdentifierBranchNoInfo);
		}

		public void TestCheckTSL_GoodsOwnerIdentifierBranchNo_MismatchAddressesBranchNo()
		{
			CusTempStorageLineValidationTestHelper.AssertEORIBranchEqualsAddressesEORIBranch(storageLine.TSL_GoodsOwnerIdentifierBranchNoInfo, storageLine.TSL_OA_GoodsOwnerInfo);
		}

		public void TestCheckTSL_GoodsOwnerIdentifierBranchNo_PremissesAddressHasEoriBranchNo()
		{
			CusTempStorageLineValidationTestHelper.AssertEORIBranchExistsInPremiseAddress(storageLine.TSL_GoodsOwnerIdentifierBranchNoInfo, () => storageLine.Validation.ValidateTSL_GoodsOwnerIdentifierBranchNo(), storageLine.TSL_OA_GoodsOwnerInfo);
		}

		public void TestCheckTSL_LocationOfGoods_List()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var storageHeader = Factory.New<CusTempStorageJobHeader>();
			storageHeader.SJH_OH_Customer = organisation.PK;
			var storageDec = CUSPRLCusTempStorageDec.New(storageHeader);
			storageDec.CusTempStorageLines.Add(storageLine);

			TestHelper.CreateFacilityCodeTypeWithCusCode(Factory);
			var custodianOrg = Factory.NewWithValidTestData<OrgHeader>();
			TestHelper.CreateTSTAuthorizationWithLinkedCustomsOffice(custodianOrg);
			Factory.Save();

			storageHeader.SJH_CustomsOffice = "DE000001";
			CombineAssertions(() =>
			{
				storageLine.Validation.ValidateTSL_LocationOfGoods();
				AssertNoMessageError("Custodian null", storageLine.TSL_LocationOfGoodsInfo, ListValidation.InvalidCodeMessageError);

				storageLine.TSL_OA_Custodian = ZGuid.Empty;
				storageLine.TSL_LocationOfGoods = "99";
				AssertNoMessageError("Invalid Custodian", storageLine.TSL_LocationOfGoodsInfo, ListValidation.InvalidCodeMessageError);

				storageLine.TSL_OA_Custodian = custodianOrg.MainAddress.PK;
				storageLine.Validation.ValidateTSL_LocationOfGoods();
				AssertHasMessageError("99 is not a valid LocationOfGoods for Custodian", storageLine.TSL_LocationOfGoodsInfo, ListValidation.InvalidCodeMessageError);

				storageLine.TSL_LocationOfGoods = "01";
				AssertNoMessageError("01 is a valid LocationOfGoods for Custodian", storageLine.TSL_LocationOfGoodsInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckTSL_LocationOfGoods_NumericValue()
		{
			var numericMessageError = "Goods Location must be numeric and between 1 and 99";
			CombineAssertions(() =>
			{
				storageLine.Validation.ValidateTSL_LocationOfGoods();
				AssertNoMessageError("No value entered", storageLine.TSL_LocationOfGoodsInfo, numericMessageError);

				storageLine.TSL_LocationOfGoods = "1";
				AssertNoMessageError("Numeric value entered", storageLine.TSL_LocationOfGoodsInfo, numericMessageError);

				storageLine.TSL_LocationOfGoods = "AA";
				AssertHasMessageError("Non numeric value entered", storageLine.TSL_LocationOfGoodsInfo, numericMessageError);
			});
		}

		public void TestCheckTSL_LocationOfGoods_NumericRange()
		{
			var rangeMessageError = "Goods Location must be between 1 and 99";
			CombineAssertions(() =>
			{
				storageLine.Validation.ValidateTSL_LocationOfGoods();
				AssertNoMessageError("No value entered", storageLine.TSL_LocationOfGoodsInfo, rangeMessageError);

				storageLine.TSL_LocationOfGoods = "AA";
				AssertNoMessageError("Non numeric value entered", storageLine.TSL_LocationOfGoodsInfo, rangeMessageError);

				storageLine.TSL_LocationOfGoods = "1";
				AssertNoMessageError("Valid numeric Value entered", storageLine.TSL_LocationOfGoodsInfo, rangeMessageError);

				storageLine.TSL_LocationOfGoods = "0";
				AssertHasMessageError("Too small value entered", storageLine.TSL_LocationOfGoodsInfo, rangeMessageError);

				storageLine.TSL_LocationOfGoods = "100";
				AssertHasMessageError("Too big value entered", storageLine.TSL_LocationOfGoodsInfo, rangeMessageError);
			});
		}

		public void TestCheckTSL_GoodsType()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(storageLine.TSL_GoodsTypeInfo, "Q", DEGoodsTypeList.Codes.A);
		}

		public void TestCheckTSL_OwnerReferenceType()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(storageLine.TSL_OwnerReferenceTypeInfo, "NUM", OwnerReferenceTypeList.Codes.AWB);
		}

		public void TestCheckTSL_PackageType()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var storageHeader = Factory.New<CusTempStorageJobHeader>();
			storageHeader.SJH_OH_Customer = organisation.PK;
			var storageDec = CUSPRLCusTempStorageDec.New(storageHeader);
			storageDec.CusTempStorageLines.Add(storageLine);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(CustomsConstants.RefDataGrouping.Codes.UnitedNationsRecommendations);
			helper.CreateNewOrGetExistingCusCodeType(CustomsConstants.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UN Package code List");
			helper.CreateNewOrGetExistingCusCodeList(CustomsConstants.RefDataGrouping.Codes.UnitedNationsRecommendations,
				CustomsConstants.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"VQ",
				"VOLUME PACKAGE",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			ValidationTestHelper.AssertInvalidCodeMessageError(storageLine.TSL_PackageTypeInfo, "ZZ", "VQ");
		}

		public void TestCheckTSL_RN_NKDepartureCountry()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(storageLine.TSL_RN_NKDepartureCountryInfo, "ZZ", Core.Constants.CountryCodes.Latvia);
		}

		protected override void SetUp()
		{
			base.SetUp();
			storageLine = Factory.New<CusTempStorageLineBaseForTest>();
		}
		CusTempStorageLineBaseForTest storageLine;
	}
}
