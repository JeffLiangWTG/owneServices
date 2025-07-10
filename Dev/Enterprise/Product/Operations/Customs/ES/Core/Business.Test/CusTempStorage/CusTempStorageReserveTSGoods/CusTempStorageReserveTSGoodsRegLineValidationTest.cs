using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing;

[TestedType(typeof(CusTempStorageReserveTSGoodsRegLineValidation))]
sealed class CusTempStorageReserveTSGoodsRegLineValidationTest : TestCaseWithFactory
{
	public void TestValidatePackagesToUse()
	{
		CombineAssertions(() =>
		{
			var line = CusTempStorageReserveTSGoodsTestHelper.GetCusTempStorageReserveTSGoodsRegLine(Factory);
			AssertNoErrorContaining(line.PackagesToUseInfo, line.Validation.ExceedRemainingPackageQuantities);
			line.PackagesToUse = 200;
			AssertHasErrorContaining(line.PackagesToUseInfo, line.Validation.ExceedRemainingPackageQuantities);
		});
	}

	public void TestValidateGrossWeightToUse()
	{
		CombineAssertions(() =>
		{
			var line = CusTempStorageReserveTSGoodsTestHelper.GetCusTempStorageReserveTSGoodsRegLine(Factory);
			AssertNoErrorContaining(line.GrossWeightToUseInfo, line.Validation.ExceedRemainingGrossWeight);
			line.GrossWeightToUse = 600;
			AssertHasErrorContaining(line.GrossWeightToUseInfo, line.Validation.ExceedRemainingGrossWeight);
		});
	}
}
