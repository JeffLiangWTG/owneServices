using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	class REXDISCusTempStorageSumALineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckTSL_CustodianIdentifier_AWB()
		{
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageLine.TSL_CustodianIdentifierInfo);
		}

		public void TestCheckTSL_CustodianIdentifier_REG()
		{
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			ValidationTestHelper.AssertFieldIsNotMandatory(storageLine.TSL_CustodianIdentifierInfo);
		}

		public void TestCheckTSL_CustodianIdentifier_SIN()
		{
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.SIN;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageLine.TSL_CustodianIdentifierInfo);
		}

		public void TestCheckTSL_CustodianIdentifierBranchNo_AWB()
		{
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			storageLine.TSL_CustodianIdentifier = "DE12345";
			ValidationTestHelper.AssertWarningIfNotEntered(storageLine.TSL_CustodianIdentifierBranchNoInfo, CustodianIdentifierBranchNoWarning);
		}

		public void TestCheckTSL_CustodianIdentifierBranchNo_REG()
		{
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			storageLine.TSL_CustodianIdentifier = "DE12345";
			ValidationTestHelper.AssertNoWarningIfNotEntered(storageLine.TSL_CustodianIdentifierBranchNoInfo, CustodianIdentifierBranchNoWarning);
		}

		public void TestCheckTSL_CustodianIdentifierBranchNo_SIN()
		{
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.SIN;
			storageLine.TSL_CustodianIdentifier = "DE12345";
			ValidationTestHelper.AssertWarningIfNotEntered(storageLine.TSL_CustodianIdentifierBranchNoInfo, CustodianIdentifierBranchNoWarning);
		}

		public void TestCheckTSL_OwnerReferenceType_AWB()
		{
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageLine.TSL_OwnerReferenceTypeInfo);
		}

		public void TestCheckTSL_OwnerReferenceType_REG()
		{
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			ValidationTestHelper.AssertFieldIsNotMandatory(storageLine.TSL_OwnerReferenceTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckTSL_OwnerReferenceType_SIN()
		{
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.SIN;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageLine.TSL_OwnerReferenceTypeInfo);
		}

		public void TestCheckTSL_LineNo()
		{
			CombineAssertions(() =>
			{
				storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
				storageLine.TSL_LineNo = 0;
				AssertNoMessageErrors("AWB - TSL_LineNo should not have any validation, as it is not seen by the user", storageLine.TSL_LineNoInfo);

				storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
				storageLine.Validation.ValidateTSL_LineNo();
				AssertNoMessageErrors("REG - TSL_LineNo should not have any validation, as it is not seen by the user", storageLine.TSL_LineNoInfo);
			});
		}

		public void TestCheckTSL_ReferenceNumberLine()
		{
			CombineAssertions(() =>
			{
				storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
				ValidationTestHelper.AssertValueCannotBeZeroMessageError(storageLine.TSL_ReferenceNumberLineInfo, "REG");

				storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
				storageLine.TSL_ReferenceNumberLine = 0;
				AssertNoMessageErrorContaining("AWB", storageLine.TSL_ReferenceNumberLineInfo, MandatoryValidation.ValueCannotBeZero);
			});
		}

		public void TestCheckATNumber_REG()
		{
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageLine.ReferenceNumberInfo);
		}

		public void TestCheckATNumber_NotREG()
		{
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			ValidationTestHelper.AssertFieldIsNotMandatory(storageLine.ReferenceNumberInfo);
		}

		public void TestCheckTSL_OwnerReferenceNumber_AWB()
		{
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageLine.TSL_OwnerReferenceNumberInfo);
		}

		public void TestCheckTSL_OwnerReferenceNumber_REG()
		{
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			ValidationTestHelper.AssertFieldIsNotMandatory(storageLine.TSL_OwnerReferenceNumberInfo);
		}

		public void TestCheckTSL_OwnerReferenceNumber_SIN()
		{
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.SIN;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageLine.TSL_OwnerReferenceNumberInfo);
		}

		public void TestCheckTSL_ReferenceNumber_AtlasRegistrationNumberMRN()
		{
			var propertyInfo = storageLine.ReferenceNumberInfo;
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;

			storageLine.ReferenceNumber = "DEDE586601055987B7";
			AssertNoMessageErrorContaining($"MRN number mismatches RegEx", propertyInfo, "Please enter a MRN in the following format with only numbers and upper case letters");

			storageLine.ReferenceNumber = "AT123456789012345678";
			AssertNoMessageError("no MRN: valid length of 21 characters", propertyInfo, "The Reference must have 18 characters (MRN) or 21 characters (ATLAS Reg. No.).");

			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;

			storageLine.ReferenceNumber = "21DE12345678901234";
			AssertNoNotifications("Valid MRN number", propertyInfo);

			storageLine.ReferenceNumber = "DEDE586601055987B7";
			AssertHasMessageErrorContaining("MRN number mismatches RegEx", propertyInfo, "Please enter a MRN in the following format with only numbers and upper case letters");

			storageLine.ReferenceNumber = "21DE27364916384835";
			AssertHasMessageErrorContaining("Invalid MRN check digit", propertyInfo, "MRN does not have a valid last digit");

			storageLine.ReferenceNumber = "21AB27364916384830";
			AssertHasMessageErrorContaining($"Invalid MRN country code", propertyInfo, "MRN does not contain a valid country/region code");

			const string validLengthMessageError = "The Reference must have 18 characters (MRN) or 21 characters (ATLAS Reg. No.).";
			storageLine.ReferenceNumber = "AT1234567890123456789";
			AssertNoMessageError("no MRN: valid length of 21 characters", propertyInfo, validLengthMessageError);

			storageLine.ReferenceNumber = "AT123456789012345678";
			AssertHasMessageError("no MRN: invalid length != 21 characters", propertyInfo, validLengthMessageError);
		}

		protected override void SetUp()
		{
			base.SetUp();
			storageDec = Factory.New<REXDISCusTempStorageDec>();
			storageLine = storageDec.CusTempStorageLines.AddNew().SumALine;
		}
		REXDISCusTempStorageDec storageDec;
		REXDISCusTempStorageSumALine storageLine;
		const string CustodianIdentifierBranchNoWarning = "Branch should be captured if known.";
	}
}
