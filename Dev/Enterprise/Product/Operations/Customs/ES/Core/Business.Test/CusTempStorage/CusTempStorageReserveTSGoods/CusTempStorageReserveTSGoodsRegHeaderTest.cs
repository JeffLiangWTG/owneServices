using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing;

[TestedType(typeof(CusTempStorageReserveTSGoodsRegHeader))]
sealed class CusTempStorageReserveTSGoodsRegHeaderTest : NonPersistentBusinessObjectTestCase
{
	public void TestNew()
	{
		CusTempStorageReserveTSGoodsTestHelper.SetUniversalReferencePackageTypesTestData(Factory);
		Factory.Save();

		CombineAssertions(() =>
		{
			var header = GetNewBusinessObject() as CusTempStorageReserveTSGoodsRegHeader;
			AssertType<CusTempStorageReserveTSGoodsRegHeader>(header);
			AssertEquals(200, header.TotalPackages);
			AssertEquals(1000m, header.TotalGrossWeight);
			AssertEquals(expected: false, header.IsPackageTypeBulk);

			header = CusTempStorageReserveTSGoodsTestHelper.GetCusTempStorageReserveTSGoodsRegHeader(Factory, isBulk: true);
			Assert(header.IsPackageTypeBulk);
		});
	}

	public void TestGetRegLines() => CombineAssertions(() =>
	{
		var header = GetNewBusinessObject() as CusTempStorageReserveTSGoodsRegHeader;
		var lines = header.ReserveTSGoodsCollection.Cast<CusTempStorageReserveTSGoodsRegLine>();
		AssertType<CusTempStorageReserveTSGoodsRegLineCollection>("Type", header.ReserveTSGoodsCollection);
		AssertEquals(2, lines.Count());
	});

	public void TestOnLinePropertyUpdated() => CombineAssertions(() =>
	{
		var header = GetNewBusinessObject() as CusTempStorageReserveTSGoodsRegHeader;
		AssertEquals(2, header.ReserveTSGoodsCollection.Count);

		var line1 = header.ReserveTSGoodsCollection[0];
		AssertOnLinePropertyUpdated(line1, 200, 1000m, hasErrors: true);

		var line2 = header.ReserveTSGoodsCollection[1];
		AssertOnLinePropertyUpdated(line2, 0, 0m, hasErrors: false);

		line2.PackagesToUse = 200;
		AssertOnLinePropertyUpdated(line2, 200, 1000m, hasErrors: true);
		AssertOnLinePropertyUpdated(line1, 0, 0m, hasErrors: false);

		line2.PackagesToUse = 100;
		AssertOnLinePropertyUpdated(line2, 100, 500m, hasErrors: false);
		AssertOnLinePropertyUpdated(line1, 100, 500m, hasErrors: false);

		line2.GrossWeightToUse = 100;
		AssertOnLinePropertyUpdated(line2, 100, 100m, hasErrors: false);
		AssertOnLinePropertyUpdated(line1, 100, 900m, hasErrors: true);
	});

	void AssertOnLinePropertyUpdated(CusTempStorageReserveTSGoodsRegLine line, int packages, decimal grossWeight, bool hasErrors)
	{
		AssertEquals(packages, line.PackagesToUse);
		AssertEquals(grossWeight, line.GrossWeightToUse);
		AssertEquals(expected: hasErrors, line.HasErrors);
	}

	public void TestRemoveAllReserveTSGoodsCollection()
	{
		var header = GetNewBusinessObject() as CusTempStorageReserveTSGoodsRegHeader;
		header.RemoveAllReserveTSGoodsCollection();
		AssertEquals(0, header.ReserveTSGoodsCollection.Count);
	}

	protected override BusinessObject GetNewBusinessObject() => CusTempStorageReserveTSGoodsTestHelper.GetCusTempStorageReserveTSGoodsRegHeader(Factory);
}
