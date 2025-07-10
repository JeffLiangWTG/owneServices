using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	class PRLCONCusTempStorageLineToConsolidateValidationTest : BusinessObjectValidationTestCase
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

		public void TestCheckTSL_LineNo()
		{
			CombineAssertions(() =>
			{
				storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
				storageLine.TSL_LineNo = -1;
				AssertNoError("AWB; -1", storageLine.TSL_LineNoInfo, MandatoryValidation.ValueCannotBeNegative);

				storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
				ValidationTestHelper.AssertErrorIfValueIsNegative(storageLine.TSL_LineNoInfo, "STH_IdentificationIndicator = REG");
			});
		}

		public void TestCheckTSL_ReferenceNumberLine()
		{
			CombineAssertions(() =>
			{
				storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
				storageLine.TSL_ReferenceNumberLine = -1;
				AssertNoMessageError("AWB; -1", storageLine.TSL_ReferenceNumberLineInfo, MandatoryValidation.ValueCannotBeNegative);

				storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
				ValidationTestHelper.AssertErrorIfValueIsNegative(storageLine.TSL_ReferenceNumberLineInfo, "STH_IdentificationIndicator = REG");
			});
		}

		public void TestCheckTSL_PackageQty_Between()
		{
			CusTempStorageLineValidationTestHelper.AssertPackageQtyIsBetween1And99999(storageLine.TSL_PackageQtyInfo);
		}

		public void TestCheckTSL_OwnerReferenceNumber_AWB()
		{
			var messageError = MandatoryValidation.YouHaveNotEnteredMessage(CusTempStorageLineValidation.OwnerReferenceNoHumanReadableName);
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageLine.TSL_OwnerReferenceNumberInfo, messageError);
		}

		public void TestCheckTSL_OwnerReferenceNumber_NotAWB()
		{
			var messageError = MandatoryValidation.YouHaveNotEnteredMessage(CusTempStorageLineValidation.OwnerReferenceNoHumanReadableName);
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			ValidationTestHelper.AssertFieldIsNotMandatory(storageLine.TSL_OwnerReferenceNumberInfo, messageError);
		}

		public void TestCheckTSL_ReferenceNumber_MandatoryREG()
		{
			var messageError = MandatoryValidation.YouHaveNotEnteredMessage(CusTempStorageLineValidation.ReferenceHumanReadableName);
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageLine.TSL_ReferenceNumberInfo, messageError);
		}

		public void TestCheckTSL_ReferenceNumber_MandatoryNotREG()
		{
			var messageError = MandatoryValidation.YouHaveNotEnteredMessage(CusTempStorageLineValidation.ReferenceHumanReadableName);
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			ValidationTestHelper.AssertFieldIsNotMandatory(storageLine.TSL_ReferenceNumberInfo, messageError);
		}

		public void TestCheckTSL_ReferenceNumber_UniqueCombination_SameReferenceNumber()
		{
			const string messageError = "and ATB No. TESTATB1 already exists for this declaration.";
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;

			CombineAssertions(() =>
			{
				storageLine.TSL_ReferenceNumberLine = 1;
				storageLine.TSL_ReferenceNumber = "TESTATB1";

				var storageLine2 = storageDec.CusTempStorageLines.AddNew();
				storageLine2.TSL_ReferenceNumberLine = 2;
				storageLine2.TSL_ReferenceNumber = "TESTATB1";
				AssertNoMessageErrorContaining("No Duplicate", storageLine2.TSL_ReferenceNumberInfo, messageError);

				storageLine2.TSL_ReferenceNumberLine = 1;
				storageLine2.Validation.ValidateTSL_ReferenceNumber();
				AssertHasMessageErrorContaining("Duplicate", storageLine2.TSL_ReferenceNumberInfo, messageError);
			});
		}

		public void TestCheckTSL_ReferenceNumber_UniqueCombination_SameReferenceNumberLine()
		{
			const string messageError = "The combination of ATB Line No. 1 and ATB No. ";
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;

			CombineAssertions(() =>
			{
				storageLine.TSL_ReferenceNumberLine = 1;
				storageLine.TSL_ReferenceNumber = "TESTATB1";

				var storageLine2 = storageDec.CusTempStorageLines.AddNew();
				storageLine2.TSL_ReferenceNumberLine = 1;
				storageLine2.TSL_ReferenceNumber = "TESTATB2";
				AssertNoMessageErrorContaining("No Duplicate", storageLine2.TSL_ReferenceNumberInfo, messageError);

				storageLine2.TSL_ReferenceNumber = "TESTATB1";
				AssertHasMessageErrorContaining("Duplicate", storageLine2.TSL_ReferenceNumberInfo, messageError);
			});
		}

		public void TestCheckTSL_ReferenceNumber_AtlasRegistrationNumberOrMRN()
		{
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			Business.Testing.TestHelper.AssertMRNFormatOr21CharactersLongNotValidated("TSL_ReferenceNumber should not be validated as Atlas / MRN for AWB", storageLine.TSL_ReferenceNumberInfo);
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			Business.Testing.TestHelper.AssertMRNFormatValidatedOr21CharactersLong("TSL_ReferenceNumber should be validated as Atlas / MRN for REG", storageLine.TSL_ReferenceNumberInfo);
		}

		public void TestCheckTSL_CustodianIdentifier_NotAWB()
		{
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			ValidationTestHelper.AssertFieldIsNotMandatory(storageLine.TSL_CustodianIdentifierInfo);
		}

		public void TestCheckTSL_CustodianIdentifier_AWB()
		{
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageLine.TSL_CustodianIdentifierInfo);
		}

		public void TestCheckTSL_CustodianIdentifier_EoriMismatch()
		{
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
			DE.Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
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

		protected override void SetUp()
		{
			base.SetUp();
			var customerOrg = Factory.NewWithValidTestData<OrgHeader>();
			var storageHeader = Factory.New<CusTempStorageJobHeader>();
			storageHeader.SJH_OH_Customer = customerOrg.PK;
			storageDec = storageHeader.PRLCONCusTempStorageDecs.AddNew();
			storageLine = storageDec.CusTempStorageLines.AddNew();
		}
		PRLCONCusTempStorageDec storageDec;
		PRLCONCusTempStorageLineToConsolidate storageLine;
	}
}
