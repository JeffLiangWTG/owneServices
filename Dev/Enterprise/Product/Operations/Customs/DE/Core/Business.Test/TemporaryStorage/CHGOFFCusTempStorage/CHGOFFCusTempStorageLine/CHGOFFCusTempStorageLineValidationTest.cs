using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	sealed class CHGOFFCusTempStorageLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckTSL_LineNo_REG()
		{
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			CusTempStorageLineValidationTestHelper.AssertLineNoIsUnique(storageLine.TSL_LineNoInfo);
		}

		public void TestCheckTSL_LineNo_AWB()
		{
			var messageError = "The combination of Line No. 1 and ATB No. AT/B/15/000001/03/2000/6000 already exists for this declaration";
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;
			storageDec.STH_OwnerReferenceNumber = "AT/B/15/000001/03/2000/6000";
			storageLine.TSL_LineNo = 1;

			CombineAssertions(() =>
			{
				var storageLine2 = storageDec.CusTempStorageLines.AddNew();
				storageLine2.TSL_LineNo = 1;
				AssertNoMessageError("AWB - same LineNo", storageLine2.TSL_LineNoInfo, messageError);

				storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
				storageLine2.Validation.ValidateTSL_LineNo();
				AssertHasMessageError("REG - same LineNo", storageLine2.TSL_LineNoInfo, messageError);
			});
		}

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

		protected override void SetUp()
		{
			base.SetUp();
			storageDec = Factory.New<CusTempStorageJobHeader>().CHGOFFCusTempStorageDecs.AddNew();
			storageLine = storageDec.CusTempStorageLines.AddNew();
		}
		CHGOFFCusTempStorageDec storageDec;
		CHGOFFCusTempStorageLine storageLine;
	}
}
