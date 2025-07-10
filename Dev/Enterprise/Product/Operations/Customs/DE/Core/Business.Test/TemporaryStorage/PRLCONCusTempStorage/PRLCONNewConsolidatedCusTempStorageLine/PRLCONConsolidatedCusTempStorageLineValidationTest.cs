using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	class PRLCONConsolidatedCusTempStorageLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckTSL_GoodsDescription()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(consolLine.TSL_GoodsDescriptionInfo);
		}

		public void TestTSL_GrossWeight()
		{
			ValidationTestHelper.AssertValueCannotBeZeroMessageError(consolLine.TSL_GrossWeightInfo);
		}

		public void TestCheckTSL_PackageType()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(consolLine.TSL_PackageTypeInfo);
		}

		public void TestCheckTSL_PackageQty_Between()
		{
			CusTempStorageLineValidationTestHelper.AssertPackageQtyIsBetween1And99999(consolLine.TSL_PackageQtyInfo);
		}

		public void TestCheckTSL_PackageQty_NotGreaterThanTotalNumberOfPackages()
		{
			const string packageQtyGreaterThanLinesQtyMessageError = "Package Count must not be greater than the total number of packages to consolidate 75";
			var storageDec = Factory.New<CusTempStorageJobHeader>().PRLCONCusTempStorageDecs.AddNew();
			consolLine = storageDec.ConsolidatedLine;

			CombineAssertions(() =>
			{
				var storageLine1 = storageDec.CusTempStorageLines.AddNew();
				storageLine1.TSL_PackageQty = 50;
				var storageLine2 = storageDec.CusTempStorageLines.AddNew();
				storageLine2.TSL_PackageQty = 25;

				consolLine.TSL_PackageQty = 76;
				AssertHasMessageError("Too big", consolLine.TSL_PackageQtyInfo, packageQtyGreaterThanLinesQtyMessageError);

				consolLine.TSL_PackageQty = 75;
				AssertNoMessageError("Valid", consolLine.TSL_PackageQtyInfo, packageQtyGreaterThanLinesQtyMessageError);
			});
		}

		public void TestCheckTSL_PackageQty_IsULDRequiresPackageCountToBe1()
		{
			CusTempStorageLineValidationTestHelper.AssertPackageQtyIs1IfIsULD(consolLine.TSL_PackageQtyInfo, () => consolLine.Validation.ValidateTSL_PackageQty());
		}

		public void TestCheckTSL_PackageQty_IsSingleCountPackageTypeRequiresPackageCountToBe1()
		{
			CusTempStorageLineValidationTestHelper.AssertPackageQtyIs1IfIsSingleCountPackageType(consolLine.TSL_PackageQtyInfo);
		}

		public void TestCheckTSL_RN_NKDepartureCountry()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(consolLine.TSL_RN_NKDepartureCountryInfo);
		}

		public void TestCheckTSL_UnionStatus()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(consolLine.TSL_UnionStatusInfo, "~", DEUnionStatusList.Codes.F);
		}

		public void TestCheckTSL_OwnerReferenceType()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(consolLine.TSL_OwnerReferenceTypeInfo);
		}

		public void TestCheckTSL_OwnerReferenceNumber()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(consolLine.TSL_OwnerReferenceNumberInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			consolLine = Factory.New<PRLCONConsolidatedCusTempStorageLine>();
		}
		PRLCONConsolidatedCusTempStorageLine consolLine;
	}
}
