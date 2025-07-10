using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing;

[TestedType(typeof(CusTempStorageReserveTSGoodsRegLine))]
sealed class CusTempStorageReserveTSGoodsRegLineTest : NonPersistentBusinessObjectTestCase
{
	public void TestNew() => CombineAssertions(() =>
	{
		var line = GetNewBusinessObject() as CusTempStorageReserveTSGoodsRegLine;
		AssertType<CusTempStorageReserveTSGoodsRegLine>(line);
		AssertEquals("REF", line.TSDNumber);
		AssertEquals("1", line.TSDItemNumber);
		AssertEquals("AA", line.PackageType);
		AssertEquals("Location 1", line.Location);
		AssertEquals("Owner Ref", line.Reference);
		AssertEquals(expected: false, line.IsPackageTypeBulk);
		AssertEquals(100, line.RemainingPackageQty);
		AssertEquals(0, line.PackagesToUse);
		AssertEquals(500m, line.RemainingGrossWeight);
		AssertEquals(0m, line.GrossWeightToUse);
	});

	public void TestValidation()
	{
		AssertType<CusTempStorageReserveTSGoodsRegLineValidation>((GetNewBusinessObject() as CusTempStorageReserveTSGoodsRegLine).Validation);
	}

	protected override BusinessObject GetNewBusinessObject() => CusTempStorageReserveTSGoodsTestHelper.GetCusTempStorageReserveTSGoodsRegLine(Factory);
}
