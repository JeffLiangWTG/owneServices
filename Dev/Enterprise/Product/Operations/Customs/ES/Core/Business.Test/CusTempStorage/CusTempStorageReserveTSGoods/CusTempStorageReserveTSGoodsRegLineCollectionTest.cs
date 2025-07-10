using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing;

[TestedType(typeof(CusTempStorageReserveTSGoodsRegLineCollection))]
sealed class CusTempStorageReserveTSGoodsRegLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CusTempStorageReserveTSGoodsRegLineCollection>
{
	public void TestCollection() => CombineAssertions(() =>
	{
		var collection = GetCollectionToTest();
		{
			AssertEquals(expected: false, collection.AllowNew);
			AssertExceptionThrown<InvalidOperationException>(() => { collection.AddNew(); });
		}
	});

	protected override CusTempStorageReserveTSGoodsRegLineCollection GetCollectionToTest() => [];

	protected override BusinessObject GetNewElementToAddToTheCollection() => CusTempStorageReserveTSGoodsTestHelper.GetCusTempStorageReserveTSGoodsRegLine(Factory);
}
