using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.JP.AFR;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(BLLFunction))]
	class BLLFunctionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestBLLFunctionInfoSaved()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill3 = header.Bills.AddNew("bill3");
			bill3.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;

			var bllFunction = new BllFunctionForTest(header, BLLFunctionCode.RegisterSplit, bill1)
			{
				JPM_ChangeReasonCode = AFRBLLChangeReasonCodeList.Codes.ChangeInCargoOperation
			};
			bllFunction.SelectedBills.AddRange(new BLLFunctionBill(bill2), new BLLFunctionBill(bill3));

			bllFunction.Factory.Save();

			AssertNotNull(bill1.BLLFunctionInfo);
			AssertEquals((int)BLLFunctionCode.RegisterSplit, bill1.BLLFunctionInfo.JP_FunctionCode);
			AssertEquals("2", bill1.BLLFunctionInfo.JP_ChangeReasonCode);
			AssertEquals($"bill2{BLLFunctionInfoTest.SplitChar}bill3", bill1.BLLFunctionInfo.JP_LinkedBills);
		}

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() =>
			{
				var bllFunctionForTest = new BllFunctionForTest(null, BLLFunctionCode.RegisterSplit);
			});

			AssertNoExceptionThrown(() =>
			{
				var bllFunctionForTest = new BllFunctionForTest(Factory.New<JPAFRHeader>(), BLLFunctionCode.RegisterSplit);
			});

			AssertNoExceptionThrown(() =>
			{
				var header = Factory.New<JPAFRHeader>();
				var bill = header.Bills.AddNew();
				var bllFunctionForTest = new BllFunctionForTest(header, BLLFunctionCode.RegisterSplit, bill);
			});
		}

		public void TestJPM_BillOfLadingNumber()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew("bill1");
			bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;

			var bllFunction = new BllFunctionForTest(header, BLLFunctionCode.RegisterSplit);
			AssertEquals(ZString.Empty, bllFunction.JPM_BillOfLadingNumber);

			bllFunction = new BllFunctionForTest(header, BLLFunctionCode.RegisterSplit, bill);
			AssertEquals(ZString.Empty, bllFunction.JPM_BillOfLadingNumber);
		}

		public void TestJPM_BillOfLadingNumberReadOnly()
		{
			var header = Factory.New<JPAFRHeader>();

			var bllFunctionForTest = new BllFunctionForTest(header, BLLFunctionCode.RegisterSplit);
			AssertEquals(false, bllFunctionForTest.JPM_BillOfLadingNumberReadOnly);

			var bill = header.Bills.AddNew();
			bllFunctionForTest = new BllFunctionForTest(header, BLLFunctionCode.RegisterSplit, bill);
			AssertEquals(true, bllFunctionForTest.JPM_BillOfLadingNumberReadOnly);
		}

		public void TestMasterAFRBill()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;

			var bllFunctionForTest = new BllFunctionForTest(header, BLLFunctionCode.RegisterSplit);
			AssertNull(bllFunctionForTest.MasterAFRBill);
			bllFunctionForTest.JPM_BillOfLadingNumber = "bill2";
			AssertNotNull(bllFunctionForTest.MasterAFRBill);
			AssertEquals(bill2, bllFunctionForTest.MasterAFRBill);

			bllFunctionForTest = new BllFunctionForTest(header, BLLFunctionCode.RegisterSplit, bill1);
			AssertNotNull(bllFunctionForTest.MasterAFRBill);
			AssertEquals(bill1, bllFunctionForTest.MasterAFRBill);
		}

		public void TestSelectBill()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;

			var bllFunction = new BllFunctionForTest(header, BLLFunctionCode.RegisterSplit);
			AssertEquals(0, bllFunction.SelectedBills.Count);
			AssertEquals(0, bllFunction.AvailableBills.Count);
			bllFunction.AvailableBills.AddRange(new BLLFunctionBill(bill1), new BLLFunctionBill(bill2));
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

			var bllFunction = new BllFunctionForTest(header, BLLFunctionCode.RegisterSplit);
			AssertEquals(0, bllFunction.AvailableBills.Count);
			AssertEquals(0, bllFunction.SelectedBills.Count);
			bllFunction.SelectedBills.AddRange(new BLLFunctionBill(bill1), new BLLFunctionBill(bill2));
			AssertEquals(2, bllFunction.SelectedBills.Count);
			AssertEquals("bill1", bllFunction.SelectedBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill2", bllFunction.SelectedBills[1].JPM_BillOfLadingNumber);

			bllFunction.UnselectBill(bllFunction.SelectedBills[0]);
			AssertEquals(1, bllFunction.SelectedBills.Count);
			AssertEquals("bill2", bllFunction.SelectedBills[0].JPM_BillOfLadingNumber);
			AssertEquals(1, bllFunction.AvailableBills.Count);
			AssertEquals("bill1", bllFunction.AvailableBills[0].JPM_BillOfLadingNumber);
		}

		public void TestSelectedBills()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			header.Bills.AddNew("bill3");

			var bllFunction = new BllFunctionForTest(header, BLLFunctionCode.RegisterSplit);
			AssertEquals(0, bllFunction.SelectedBills.Count);
			AssertEquals(0, bllFunction.AvailableBills.Count);
			bllFunction.AvailableBills.AddRange(new BLLFunctionBill(bill1), new BLLFunctionBill(bill2));
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
			var bllFunction = new BllFunctionForTest(header, BLLFunctionCode.RegisterSplit);
			AssertNotNull(bllFunction.AvailableBills);
			AssertEquals(0, bllFunction.AvailableBills.Count);
		}

		public void TestRegisteredBills()
		{
			var header = Factory.New<JPAFRHeader>();
			var bllFunction = new BllFunctionForTest(header, BLLFunctionCode.RegisterSplit);
			AssertNotNull(bllFunction.RegisteredBills);
			AssertEquals(0, bllFunction.RegisteredBills.Count);
		}

		public void TestSelectEnabled()
		{
			var header = Factory.New<JPAFRHeader>();
			var bllFunction = new BllFunctionForTest(header, BLLFunctionCode.RegisterSplit);
			AssertEquals(false, bllFunction.SelectEnabled);
		}

		public void TestUnselectEnabled()
		{
			var header = Factory.New<JPAFRHeader>();
			var bllFunction = new BllFunctionForTest(header, BLLFunctionCode.RegisterSplit);
			AssertEquals(false, bllFunction.UnselectEnabled);
		}

		public void TestSendEnabled()
		{
			var header = Factory.New<JPAFRHeader>();
			var bllFunction = new BllFunctionForTest(header, BLLFunctionCode.RegisterSplit);
			AssertEquals(false, bllFunction.SendEnabled);
		}

		public void TestFunctionCode()
		{
			var header = Factory.New<JPAFRHeader>();
			var bllFunction = new BllFunctionForTest(header, BLLFunctionCode.RegisterSplit);
			AssertEquals(BLLFunctionCode.RegisterSplit, bllFunction.FunctionCode);
		}

		public void TestNewInstanceType()
		{
			AssertType<BLLRegistration>(BLLFunction.New(Factory.New<JPAFRHeader>(), BLLFunctionCode.RegisterSplit));
			AssertType<BLLRegistration>(BLLFunction.New(Factory.New<JPAFRHeader>(), BLLFunctionCode.RegisterSwitch));
			AssertType<BLLRegistration>(BLLFunction.New(Factory.New<JPAFRHeader>(), BLLFunctionCode.RegisterMerge));
			AssertType<BLLCancellation>(BLLFunction.New(Factory.New<JPAFRHeader>(), BLLFunctionCode.CancelSplit));
			AssertType<BLLCancellation>(BLLFunction.New(Factory.New<JPAFRHeader>(), BLLFunctionCode.CancelSwitch));
			AssertType<BLLCancellation>(BLLFunction.New(Factory.New<JPAFRHeader>(), BLLFunctionCode.CancelMerge));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<JPAFRHeader>();
			return new BllFunctionForTest(header, BLLFunctionCode.RegisterSplit);
		}
	}

	class BllFunctionForTest : BLLFunction
	{
		public BllFunctionForTest(JPAFRHeader header, BLLFunctionCode functionCode, JPAFRBills bill = null) : base(header, functionCode, bill)
		{
		}

		public override ZBool SelectEnabled => false;
		public override ZBool UnselectEnabled => false;
		public override ZBool SendEnabled => false;
	}
}
