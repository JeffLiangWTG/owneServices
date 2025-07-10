using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	class CUSPCSConsolidatedCusTempStorageLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckTSL_OwnerReferenceType_AWB()
		{
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageLine.TSL_OwnerReferenceTypeInfo);
		}

		public void TestCheckTSL_OwnerReferenceType_NotAWB()
		{
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			ValidationTestHelper.AssertFieldIsNotMandatory(storageLine.TSL_OwnerReferenceTypeInfo);
		}

		public void TestCheckTSL_CustodianIdentifier_AWB()
		{
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageLine.TSL_CustodianIdentifierInfo);
		}

		public void TestCheckTSL_CustodianIdentifier_NotAWB()
		{
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			ValidationTestHelper.AssertFieldIsNotMandatory(storageLine.TSL_CustodianIdentifierInfo);
		}

		public void TestCheckTSL_CustodianIdentifier_EoriMismatch()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			storageDec.StorageHeader.SJH_OH_Customer = organisation.PK;
			Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			CusTempStorageLineValidationTestHelper.AssertEORINumberMatchesOrganizationsEORINumberIfIsAWB(storageLine.TSL_CustodianIdentifierInfo, () => storageLine.Validation.ValidateTSL_CustodianIdentifier(), storageLine.TSL_OA_CustodianInfo, "Custodian");
		}

		public void TestCheckTSL_CustodianIdentifier_EoriMissing()
		{
			CusTempStorageLineValidationTestHelper.AssertEORINumberExistsInOrganizationIfIsAWB(storageLine.TSL_CustodianIdentifierInfo, () => storageLine.Validation.ValidateTSL_CustodianIdentifier(), storageLine.TSL_OA_CustodianInfo);
		}

		public void TestCheckTSL_CustodianIdentifierBranchNo_4Digits()
		{
			CusTempStorageLineValidationTestHelper.AssertEORIBranchHas4DigitsIfIsAWB(storageLine.TSL_CustodianIdentifierBranchNoInfo);
		}

		public void TestCheckTSL_CustodianIdentifierBranchNo_MismatchAddressesBranchNo()
		{
			CusTempStorageLineValidationTestHelper.AssertEORIBranchEqualsAddressesEORIBranchIfIsAWB(storageLine.TSL_CustodianIdentifierBranchNoInfo, () => storageLine.Validation.ValidateTSL_CustodianIdentifierBranchNo(), storageLine.TSL_OA_CustodianInfo);
		}

		public void TestCheckTSL_CustodianIdentifierBranchNo_PremissesAddressHasEoriBranchNo()
		{
			CusTempStorageLineValidationTestHelper.AssertEORIBranchExistsInPremiseAddressIfIsAWB(storageLine.TSL_CustodianIdentifierBranchNoInfo, () => storageLine.Validation.ValidateTSL_CustodianIdentifierBranchNo(), storageLine.TSL_OA_CustodianInfo);
		}

		public void TestCheckTSL_CustodianIdentifierBranchNo_NotAWB()
		{
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			storageLine.Validation.ValidateTSL_CustodianIdentifierBranchNo();
			AssertNoNotifications(storageLine.TSL_CustodianIdentifierBranchNoInfo);
		}

		public void TestCheckTSL_GoodsOwnerIdentifier_NotAWB()
		{
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			ValidationTestHelper.AssertFieldIsNotMandatory(storageLine.TSL_CustodianIdentifierInfo);
		}

		public void TestCheckTSL_GoodsOwnerIdentifier_AWB()
		{
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageLine.TSL_CustodianIdentifierInfo);
		}

		public void TestCheckTSL_GoodsOwnerIdentifier_EoriMismatch()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			storageDec.StorageHeader.SJH_OH_Customer = organisation.PK;
			Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			CusTempStorageLineValidationTestHelper.AssertEORINumberMatchesOrganizationsEORINumberIfIsAWB(storageLine.TSL_GoodsOwnerIdentifierInfo, () => storageLine.Validation.ValidateTSL_GoodsOwnerIdentifier(), storageLine.TSL_OA_GoodsOwnerInfo, "Disposal Entitled Trader");
		}

		public void TestCheckTSL_GoodsOwnerIdentifier_EoriMissing()
		{
			CusTempStorageLineValidationTestHelper.AssertEORINumberExistsInOrganizationIfIsAWB(storageLine.TSL_GoodsOwnerIdentifierInfo, () => storageLine.Validation.ValidateTSL_GoodsOwnerIdentifier(), storageLine.TSL_OA_GoodsOwnerInfo);
		}

		public void TestCheckTSL_GoodsOwnerIdentifierBranchNo_4Digits()
		{
			CusTempStorageLineValidationTestHelper.AssertEORIBranchHas4DigitsIfIsAWB(storageLine.TSL_GoodsOwnerIdentifierBranchNoInfo);
		}

		public void TestCheckTSL_GoodsOwnerIdentifierBranchNo_MismatchAddressesBranchNo()
		{
			CusTempStorageLineValidationTestHelper.AssertEORIBranchEqualsAddressesEORIBranchIfIsAWB(storageLine.TSL_GoodsOwnerIdentifierBranchNoInfo, () => storageLine.Validation.ValidateTSL_GoodsOwnerIdentifierBranchNo(), storageLine.TSL_OA_GoodsOwnerInfo);
		}

		public void TestCheckTSL_GoodsOwnerIdentifierBranchNo_PremissesAddressHasEoriBranchNo()
		{
			CusTempStorageLineValidationTestHelper.AssertEORIBranchExistsInPremiseAddressIfIsAWB(storageLine.TSL_GoodsOwnerIdentifierBranchNoInfo, () => storageLine.Validation.ValidateTSL_GoodsOwnerIdentifierBranchNo(), storageLine.TSL_OA_GoodsOwnerInfo);
		}

		public void TestCheckTSL_OwnerReferenceNumber_REG()
		{
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageLine.TSL_OwnerReferenceNumberInfo, $"{MandatoryValidation.YouHaveNotEntered} a Reference");
		}

		public void TestCheckTSL_OwnerReferenceNumber_AWB()
		{
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageLine.TSL_OwnerReferenceNumberInfo, $"{MandatoryValidation.YouHaveNotEntered} an Owner Reference No.");
		}

		public void TestCheckTSL_ReferenceNumber_AtlasRegistrationNumberOrMRN()
		{
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			Business.Testing.TestHelper.AssertMRNFormatOr21CharactersLongNotValidated("TSL_ReferenceNumber should not be validated as Atlas / MRN for AWB", storageLine.TSL_OwnerReferenceNumberInfo);
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			Business.Testing.TestHelper.AssertMRNFormatValidatedOr21CharactersLong("TSL_ReferenceNumber should be validated as Atlas / MRN for REG", storageLine.TSL_OwnerReferenceNumberInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			storageDec = Factory.New<CusTempStorageJobHeader>().CUSPCSCusTempStorageDecs.AddNew();
			storageLine = storageDec.ConsolidatedCusTempStorageLine;
		}
		CUSPCSCusTempStorageDec storageDec;
		CUSPCSConsolidatedCusTempStorageLine storageLine;
	}
}
