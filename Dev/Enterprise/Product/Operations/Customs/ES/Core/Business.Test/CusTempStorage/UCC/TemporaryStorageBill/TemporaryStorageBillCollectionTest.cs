using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing;

[TestedType(typeof(TemporaryStorageBillCollection<TemporaryStorageBill, TemporaryStorageHeader>))]
sealed class TemporaryStorageBillCollectionTest : BusinessObjectCollectionTestCase
{
	public void TestSetDefaultsForNewChild()
	{
		var header = Factory.New<TemporaryStorageHeader>();

		CombineAssertions(() =>
		{
			var billCollection = new TemporaryStorageBillCollection<TemporaryStorageBill, TemporaryStorageHeader>(header);
			var bill = billCollection.AddNew();

			AssertEquals("Bill Type should be HWB by default", EU.Business.TemporaryStorageBillKindList.Codes.HWB, bill.ABL_BolType);

			header.MovementOfContainersOnly = true;
			var bill2 = billCollection.AddNew();

			AssertEquals("Bill Type should be MOC when Movement of Containers Only", TemporaryStorageBill.ChildMocCode, bill2.ABL_BolType);
		});
	}

	protected override BusinessObjectCollection GetCollectionToTest() => new TemporaryStorageBillCollection<TemporaryStorageBill, TemporaryStorageHeader>(Factory.New<TemporaryStorageHeader>());
}
