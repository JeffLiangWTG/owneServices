using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common.JP.AFR;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(BLLRegistration))]
	class BLLRegistrationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSelectBill()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill3 = header.Bills.AddNew("bill3");
			var bill4 = header.Bills.AddNew("bill4");
			bill4.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill5 = header.Bills.AddNew("bill5");
			bill5.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			CreateBLLFunctionInfo(bill4);
			CreateBLLFunctionInfo(bill5);
			Factory.Save();

			var bllFunction = new BLLRegistration(header, BLLFunctionCode.RegisterSplit);
			AssertEquals(0, bllFunction.SelectedBills.Count);
			AssertEquals(2, bllFunction.AvailableBills.Count);
			AssertEquals("bill1", bllFunction.AvailableBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill2", bllFunction.AvailableBills[1].JPM_BillOfLadingNumber);

			bllFunction.SelectBill(bllFunction.AvailableBills[0]);
			AssertEquals(1, bllFunction.AvailableBills.Count);
			AssertEquals("bill2", bllFunction.AvailableBills[0].JPM_BillOfLadingNumber);
			AssertEquals(1, bllFunction.SelectedBills.Count);
			AssertEquals("bill1", bllFunction.SelectedBills[0].JPM_BillOfLadingNumber);
		}

		public void TestUnselectBill()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill3 = header.Bills.AddNew("bill3");
			var bill4 = header.Bills.AddNew("bill4");
			bill4.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill5 = header.Bills.AddNew("bill5");
			bill5.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			CreateBLLFunctionInfo(bill4);
			CreateBLLFunctionInfo(bill5);
			Factory.Save();

			var bllFunction = new BLLRegistration(header, BLLFunctionCode.RegisterSplit);
			AssertEquals(0, bllFunction.SelectedBills.Count);
			AssertEquals(2, bllFunction.AvailableBills.Count);
			AssertEquals("bill1", bllFunction.AvailableBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill2", bllFunction.AvailableBills[1].JPM_BillOfLadingNumber);

			bllFunction.SelectBill(bllFunction.AvailableBills[0]);
			AssertEquals(1, bllFunction.AvailableBills.Count);
			AssertEquals("bill2", bllFunction.AvailableBills[0].JPM_BillOfLadingNumber);
			AssertEquals(1, bllFunction.SelectedBills.Count);
			AssertEquals("bill1", bllFunction.SelectedBills[0].JPM_BillOfLadingNumber);

			bllFunction.UnselectBill(bllFunction.SelectedBills[0]);
			AssertEquals(0, bllFunction.SelectedBills.Count);
			AssertEquals(2, bllFunction.AvailableBills.Count);
			AssertEquals("bill2", bllFunction.AvailableBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill1", bllFunction.AvailableBills[1].JPM_BillOfLadingNumber);
		}

		public void TestSelectedBills()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill3 = header.Bills.AddNew("bill3");
			var bill4 = header.Bills.AddNew("bill4");
			bill4.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill5 = header.Bills.AddNew("bill5");
			bill5.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			CreateBLLFunctionInfo(bill4);
			CreateBLLFunctionInfo(bill5);
			Factory.Save();

			var bllFunction = new BLLRegistration(header, BLLFunctionCode.RegisterSplit);
			AssertEquals(0, bllFunction.SelectedBills.Count);
			AssertEquals(2, bllFunction.AvailableBills.Count);
			AssertEquals("bill1", bllFunction.AvailableBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill2", bllFunction.AvailableBills[1].JPM_BillOfLadingNumber);

			bllFunction.SelectBill(bllFunction.AvailableBills[0]);
			AssertEquals(1, bllFunction.AvailableBills.Count);
			AssertEquals("bill2", bllFunction.AvailableBills[0].JPM_BillOfLadingNumber);
			AssertEquals(1, bllFunction.SelectedBills.Count);
			AssertEquals("bill1", bllFunction.SelectedBills[0].JPM_BillOfLadingNumber);
		}

		public void TestAvailavleBills()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill3 = header.Bills.AddNew("bill3");
			var bill4 = header.Bills.AddNew("bill4");
			bill4.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill5 = header.Bills.AddNew("bill5");
			bill5.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			CreateBLLFunctionInfo(bill4);
			CreateBLLFunctionInfo(bill5);
			Factory.Save();

			var bllFunction = new BLLRegistration(header, BLLFunctionCode.RegisterSplit);
			AssertEquals(2, bllFunction.AvailableBills.Count);
			AssertEquals("bill1", bllFunction.AvailableBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill2", bllFunction.AvailableBills[1].JPM_BillOfLadingNumber);

			bllFunction.JPM_BillOfLadingNumber = "bill1";
			AssertEquals(1, bllFunction.AvailableBills.Count);
			AssertEquals("bill2", bllFunction.AvailableBills[0].JPM_BillOfLadingNumber);

			bllFunction = new BLLRegistration(header, BLLFunctionCode.RegisterSplit, bill1);
			AssertEquals(1, bllFunction.AvailableBills.Count);
			AssertEquals("bill2", bllFunction.AvailableBills[0].JPM_BillOfLadingNumber);
		}

		void CreateBLLFunctionInfo(JPAFRBills bill)
		{
			var bllFunctionInfo = Factory.New<BLLFunctionInfo>();
			bllFunctionInfo.B7_ParentID = bill.PK;
			bllFunctionInfo.B7_ParentTableCode = bill.TablePrefix;
			bllFunctionInfo.B7_Type = CusAddInfoTypeAttribute.Codes.JPAFRBLLFunction;
		}

		public void TestRegisteredBills()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill3 = header.Bills.AddNew("bill3");
			var bill4 = header.Bills.AddNew("bill4");
			bill4.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill5 = header.Bills.AddNew("bill5");
			bill5.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			CreateBLLFunctionInfo(bill4);
			CreateBLLFunctionInfo(bill5);
			Factory.Save();

			var bllFunction = new BLLRegistration(header, BLLFunctionCode.RegisterSplit);
			AssertEquals(2, bllFunction.RegisteredBills.Count);
			AssertEquals("bill1", bllFunction.RegisteredBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill2", bllFunction.RegisteredBills[1].JPM_BillOfLadingNumber);

			bllFunction = new BLLRegistration(header, BLLFunctionCode.RegisterSplit, bill1);
			AssertEquals(2, bllFunction.RegisteredBills.Count);
			AssertEquals("bill1", bllFunction.RegisteredBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill2", bllFunction.RegisteredBills[1].JPM_BillOfLadingNumber);
		}

		public void TestSendEnabled_Split()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill3 = header.Bills.AddNew("bill3");
			bill3.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill4 = header.Bills.AddNew("bill4");

			var bllFunction = new BLLRegistration(header, BLLFunctionCode.RegisterSplit);
			AssertEquals(3, bllFunction.AvailableBills.Count);
			AssertEquals("bill1", bllFunction.AvailableBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill2", bllFunction.AvailableBills[1].JPM_BillOfLadingNumber);
			AssertEquals("bill3", bllFunction.AvailableBills[2].JPM_BillOfLadingNumber);

			AssertEquals(false, bllFunction.SendEnabled);

			bllFunction.SelectBill(bllFunction.AvailableBills[0]);
			AssertEquals(false, bllFunction.SendEnabled);

			bllFunction.SelectBill(bllFunction.AvailableBills[0]);
			AssertEquals(true, bllFunction.SendEnabled);
		}

		public void TestSendEnabled_Merge()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill3 = header.Bills.AddNew("bill3");
			bill3.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill4 = header.Bills.AddNew("bill4");

			var bllFunction = new BLLRegistration(header, BLLFunctionCode.RegisterMerge);
			AssertEquals(3, bllFunction.AvailableBills.Count);
			AssertEquals("bill1", bllFunction.AvailableBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill2", bllFunction.AvailableBills[1].JPM_BillOfLadingNumber);
			AssertEquals("bill3", bllFunction.AvailableBills[2].JPM_BillOfLadingNumber);

			AssertEquals(false, bllFunction.SendEnabled);

			bllFunction.SelectBill(bllFunction.AvailableBills[0]);
			AssertEquals(false, bllFunction.SendEnabled);

			bllFunction.SelectBill(bllFunction.AvailableBills[0]);
			AssertEquals(true, bllFunction.SendEnabled);
		}

		public void TestSendEnabled_Switch()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill3 = header.Bills.AddNew("bill3");
			bill3.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill4 = header.Bills.AddNew("bill4");

			var bllFunction = new BLLRegistration(header, BLLFunctionCode.RegisterSwitch);
			AssertEquals(3, bllFunction.AvailableBills.Count);
			AssertEquals("bill1", bllFunction.AvailableBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill2", bllFunction.AvailableBills[1].JPM_BillOfLadingNumber);
			AssertEquals("bill3", bllFunction.AvailableBills[2].JPM_BillOfLadingNumber);

			AssertEquals(false, bllFunction.SendEnabled);

			bllFunction.SelectBill(bllFunction.AvailableBills[0]);
			AssertEquals(true, bllFunction.SendEnabled);

			bllFunction.SelectBill(bllFunction.AvailableBills[0]);
			AssertEquals(false, bllFunction.SendEnabled);
		}

		public void TestSelectEnabled_Split()
		{
			var header = Factory.New<JPAFRHeader>();
			AssertEquals(false, new BLLRegistration(header, BLLFunctionCode.RegisterSplit).SelectEnabled);

			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill3 = header.Bills.AddNew("bill3");
			bill3.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill4 = header.Bills.AddNew("bill4");
			bill4.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill5 = header.Bills.AddNew("bill5");
			bill5.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill6 = header.Bills.AddNew("bill6");
			bill6.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill7 = header.Bills.AddNew("bill7");
			bill7.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill8 = header.Bills.AddNew("bill8");
			bill8.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill9 = header.Bills.AddNew("bill9");
			bill9.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill10 = header.Bills.AddNew("bill10");
			bill10.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;

			var bllFunction = new BLLRegistration(header, BLLFunctionCode.RegisterSplit);
			AssertEquals(true, bllFunction.SelectEnabled);

			//select 1st bill
			bllFunction.SelectBill(bllFunction.AvailableBills[0]);
			AssertEquals(true, bllFunction.SelectEnabled);

			//select 2nd bill
			bllFunction.SelectBill(bllFunction.AvailableBills[0]);
			AssertEquals(true, bllFunction.SelectEnabled);

			//select 3rd bill
			bllFunction.SelectBill(bllFunction.AvailableBills[0]);
			AssertEquals(true, bllFunction.SelectEnabled);

			//select 4th bill
			bllFunction.SelectBill(bllFunction.AvailableBills[0]);
			AssertEquals(true, bllFunction.SelectEnabled);

			//select 5th bill
			bllFunction.SelectBill(bllFunction.AvailableBills[0]);
			AssertEquals(true, bllFunction.SelectEnabled);

			//select 6th bill
			bllFunction.SelectBill(bllFunction.AvailableBills[0]);
			AssertEquals(true, bllFunction.SelectEnabled);

			//select 7th bill
			bllFunction.SelectBill(bllFunction.AvailableBills[0]);
			AssertEquals(true, bllFunction.SelectEnabled);

			//select 8th bill
			bllFunction.SelectBill(bllFunction.AvailableBills[0]);
			AssertEquals(true, bllFunction.SelectEnabled);

			//select 9th bill
			bllFunction.SelectBill(bllFunction.AvailableBills[0]);
			AssertEquals(true, bllFunction.SelectEnabled);

			//select 10th bill
			bllFunction.SelectBill(bllFunction.AvailableBills[0]);
			AssertEquals(false, bllFunction.SelectEnabled);
		}

		public void TestSelectEnabled_Switch()
		{
			var header = Factory.New<JPAFRHeader>();
			AssertEquals(false, new BLLRegistration(header, BLLFunctionCode.RegisterSwitch).SelectEnabled);

			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;

			var bllFunction = new BLLRegistration(header, BLLFunctionCode.RegisterSplit);
			AssertEquals(true, bllFunction.SelectEnabled);

			//select 1st bill
			bllFunction.SelectBill(bllFunction.AvailableBills[0]);
			AssertEquals(false, bllFunction.SelectEnabled);
		}

		public void TestSelectEnabled_Merge()
		{
			var header = Factory.New<JPAFRHeader>();
			AssertEquals(false, new BLLRegistration(header, BLLFunctionCode.RegisterMerge).SelectEnabled);

			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill3 = header.Bills.AddNew("bill3");
			bill3.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill4 = header.Bills.AddNew("bill4");
			bill4.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill5 = header.Bills.AddNew("bill5");
			bill5.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill6 = header.Bills.AddNew("bill6");
			bill6.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill7 = header.Bills.AddNew("bill7");
			bill7.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill8 = header.Bills.AddNew("bill8");
			bill8.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill9 = header.Bills.AddNew("bill9");
			bill9.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill10 = header.Bills.AddNew("bill10");
			bill10.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;

			var bllFunction = new BLLRegistration(header, BLLFunctionCode.RegisterSplit);
			AssertEquals(true, bllFunction.SelectEnabled);

			//select 1st bill
			bllFunction.SelectBill(bllFunction.AvailableBills[0]);
			AssertEquals(true, bllFunction.SelectEnabled);

			//select 2nd bill
			bllFunction.SelectBill(bllFunction.AvailableBills[0]);
			AssertEquals(true, bllFunction.SelectEnabled);

			//select 3rd bill
			bllFunction.SelectBill(bllFunction.AvailableBills[0]);
			AssertEquals(true, bllFunction.SelectEnabled);

			//select 4th bill
			bllFunction.SelectBill(bllFunction.AvailableBills[0]);
			AssertEquals(true, bllFunction.SelectEnabled);

			//select 5th bill
			bllFunction.SelectBill(bllFunction.AvailableBills[0]);
			AssertEquals(true, bllFunction.SelectEnabled);

			//select 6th bill
			bllFunction.SelectBill(bllFunction.AvailableBills[0]);
			AssertEquals(true, bllFunction.SelectEnabled);

			//select 7th bill
			bllFunction.SelectBill(bllFunction.AvailableBills[0]);
			AssertEquals(true, bllFunction.SelectEnabled);

			//select 8th bill
			bllFunction.SelectBill(bllFunction.AvailableBills[0]);
			AssertEquals(true, bllFunction.SelectEnabled);

			//select 9th bill
			bllFunction.SelectBill(bllFunction.AvailableBills[0]);
			AssertEquals(true, bllFunction.SelectEnabled);

			//select 10th bill
			bllFunction.SelectBill(bllFunction.AvailableBills[0]);
			AssertEquals(false, bllFunction.SelectEnabled);
		}

		public void TestUnselectEnabled()
		{
			var header = Factory.New<JPAFRHeader>();
			AssertEquals(false, new BLLRegistration(header, BLLFunctionCode.RegisterSwitch).SelectEnabled);

			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;

			var bllFunction = new BLLRegistration(header, BLLFunctionCode.RegisterSplit);
			AssertEquals(true, bllFunction.SelectEnabled);
			AssertEquals(false, bllFunction.UnselectEnabled);

			//select 1st bill
			bllFunction.SelectBill(bllFunction.AvailableBills[0]);
			AssertEquals(false, bllFunction.SelectEnabled);
			AssertEquals(true, bllFunction.UnselectEnabled);
		}

		public void TestJPM_BillOfLadingNumber()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill3 = header.Bills.AddNew("bill3");
			var bill4 = header.Bills.AddNew("bill4");
			bill4.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill5 = header.Bills.AddNew("bill5");
			bill5.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			CreateBLLFunctionInfo(bill4);
			CreateBLLFunctionInfo(bill5);
			Factory.Save();

			var bllFunction = new BLLRegistration(header, BLLFunctionCode.RegisterSplit);
			AssertEquals(0, bllFunction.SelectedBills.Count);
			AssertEquals(2, bllFunction.AvailableBills.Count);
			AssertEquals("bill1", bllFunction.AvailableBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill2", bllFunction.AvailableBills[1].JPM_BillOfLadingNumber);

			bllFunction.SelectBill(bllFunction.AvailableBills[0]);
			AssertEquals(1, bllFunction.AvailableBills.Count);
			AssertEquals("bill2", bllFunction.AvailableBills[0].JPM_BillOfLadingNumber);
			AssertEquals(1, bllFunction.SelectedBills.Count);
			AssertEquals("bill1", bllFunction.SelectedBills[0].JPM_BillOfLadingNumber);

			bllFunction.JPM_BillOfLadingNumber = "bill1";
			AssertEquals(0, bllFunction.SelectedBills.Count);
			AssertEquals(1, bllFunction.AvailableBills.Count);
			AssertEquals("bill2", bllFunction.AvailableBills[0].JPM_BillOfLadingNumber);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return BLLFunction.New(Factory.New<JPAFRHeader>(), BLLFunctionCode.RegisterSplit);
		}
	}
}
