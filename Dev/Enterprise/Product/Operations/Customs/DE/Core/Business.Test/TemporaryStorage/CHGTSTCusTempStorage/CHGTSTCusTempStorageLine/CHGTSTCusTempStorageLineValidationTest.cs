using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using TestHelper = Enterprise.Customs.DE.Business.Testing.TestHelper;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	class CHGTSTCusTempStorageLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckTSL_LineNo()
		{
			CusTempStorageLineValidationTestHelper.AssertLineNoIsUnique(storageLine.TSL_LineNoInfo);
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

		public void TestCheckTSL_CustodianIdentifier_EoriMismatch()
		{
			var storageHeader = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			storageHeader.CHGTSTCusTempStorageDecs.Add(storageDec);
			TestHelper.CreateCL010CoutryList(Factory);
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

		public void TestCheckTSL_LocationOfGoodsList_Mandatory()
		{
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageLine.TSL_LocationOfGoodsInfo);
		}

		public void TestCheckTSL_LocationOfGoods_List()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var storageHeader = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			storageHeader.CHGTSTCusTempStorageDecs.Add(storageDec);
			storageHeader.SJH_OH_Customer = organisation.PK;

			TestHelper.CreateFacilityCodeTypeWithCusCode(Factory);
			var branchOrgProxy = Factory.New<OrgHeader>();
			branchOrgProxy.OH_Code = "BRANORGPROX";
			TestHelper.CreateTSTAuthorizationWithLinkedCustomsOffice(branchOrgProxy);
			Factory.Save();

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = branchOrgProxy.PK;
			storageDec.StorageHeader.SJH_CustomsOffice = "DE000001";
			storageDec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.AWB;

			ValidationTestHelper.AssertInvalidCodeMessageError(storageLine.TSL_LocationOfGoodsInfo, "99", "01");
		}

		protected override void SetUp()
		{
			base.SetUp();
			storageDec = Factory.New<CHGTSTCusTempStorageDec>();
			storageLine = storageDec.CusTempStorageLines.AddNew();
		}
		CHGTSTCusTempStorageDec storageDec;
		CHGTSTCusTempStorageLine storageLine;
	}
}
