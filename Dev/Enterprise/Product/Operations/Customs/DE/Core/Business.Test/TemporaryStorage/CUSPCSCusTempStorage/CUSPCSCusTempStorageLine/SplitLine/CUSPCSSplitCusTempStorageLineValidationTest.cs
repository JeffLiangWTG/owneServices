using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	class CUSPCSSplitCusTempStorageLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckTSL_PackageQty_Between()
		{
			CusTempStorageLineValidationTestHelper.AssertPackageQtyIsBetween1And99999(storageLine.TSL_PackageQtyInfo);
		}

		public void TestCheckTSL_PackageQty_IsULDRequiresPackageCountToBe1()
		{
			CusTempStorageLineValidationTestHelper.AssertPackageQtyIs1IfIsULD(storageLine.TSL_PackageQtyInfo, () => storageLine.Validation.ValidateTSL_PackageQty());
		}

		public void TestCheckTSL_PackageQty_IsSingleCountPackageTypeRequiresPackageCountToBe1()
		{
			CusTempStorageLineValidationTestHelper.AssertPackageQtyIs1IfIsSingleCountPackageType(storageLine.TSL_PackageQtyInfo);
		}

		public void TestCheckTSL_GoodsDescritpion()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageLine.TSL_GoodsDescriptionInfo);
		}

		public void TestCheckTSL_GrossWeight()
		{
			ValidationTestHelper.AssertValueCannotBeZeroMessageError(storageLine.TSL_GrossWeightInfo);
		}

		public void TestCheckTSL_RN_NKDepartureCountry()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageLine.TSL_RN_NKDepartureCountryInfo);
		}

		public void TestCheckTSL_OwnerReferenceNumber()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageLine.TSL_OwnerReferenceNumberInfo);
		}

		public void TestCheckTSL_OwnerReferenceType()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageLine.TSL_OwnerReferenceTypeInfo);
		}

		public void TestCheckTSL_PackageType()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageLine.TSL_PackageTypeInfo);
		}

		public void TestCheckTSL_UnionStatus()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(storageLine.TSL_UnionStatusInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			storageLine = Factory.New<CUSPCSSplitCusTempStorageLine>();
		}
		CUSPCSSplitCusTempStorageLine storageLine;
	}
}
