using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageBillCollection<TemporaryStorageBill, TemporaryStorageHeader>))]
	sealed class TemporaryStorageBillCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestTemporaryStorageMasterBillCollection()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var billCollection = new TemporaryStorageBillCollection<TemporaryStorageBill, TemporaryStorageHeader>(header);
			AssertType<TemporaryStorageHeader>(billCollection.Master);
		}

		public void TestSetDefaultsForNewChild()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var billCollection = new TemporaryStorageBillCollection<TemporaryStorageBill, TemporaryStorageHeader>(header);
			var bill = billCollection.AddNew();
			AssertType<TemporaryStorageBill>(bill);
			AssertEquals(TemporaryStorageBillKindList.Codes.HWB, bill.ABL_BolType);
		}

		public void TestAllowNew()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var billCollection = new TemporaryStorageBillCollection<TemporaryStorageBill, TemporaryStorageHeader>(header);

			header.AMA_Calc_HasHouseConsignment = false;
			Assert(!((IBindingList)billCollection).AllowNew);

			header.AMA_Calc_HasHouseConsignment = true;
			Assert(((IBindingList)billCollection).AllowNew);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new TemporaryStorageBillCollection<TemporaryStorageBill, TemporaryStorageHeader>(Factory.New<TemporaryStorageHeader>());

		public void TestCreateAdditionalFilter()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.HasNoMasterBill = true;
			Assert("No MasterBill is expected in Bills collection when HasNoMasterBill is true.", !header.Bills.Contains(header.MasterBill));

			header.HasNoMasterBill = false;
			Assert("MasterBill is expected in Bills collection when HasNoMasterBill is false.", header.Bills.Contains(header.MasterBill));
		}
	}
}
