using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing;

[TestedType(typeof(TemporaryStorageDutyAndTaxCollection))]
sealed class TemporaryStorageDutyAndTaxCollectionTest : BusinessObjectCollectionTestCase
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		return header.Bills.AddNew().PackedItems.AddNew().DutiesAndTaxes;
	}

	public void TestReadOnly() => CombineAssertions(() =>
	{
		var dutyAndTaxCollection = GetCollectionToTest();
		AssertEquals("AllowNew", false, dutyAndTaxCollection.AllowNew);
		AssertEquals("AllowRemove", false, dutyAndTaxCollection.AllowRemove);
	});
}
