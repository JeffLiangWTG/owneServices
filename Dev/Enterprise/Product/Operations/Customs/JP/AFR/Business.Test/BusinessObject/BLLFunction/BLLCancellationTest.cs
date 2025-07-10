using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.JP.AFR;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(BLLCancellation))]
	class BLLCancellationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLookups()
		{
			var bllFunction = new BLLCancellation(Factory.New<JPAFRHeader>(), BLLFunctionCode.CancelMerge);
			AssertType<BLLCancellationLookups>(bllFunction.Lookups);
		}

		public void TestJPM_BillOfLadingNumber_CancelSplit_WithoutSelectedAFRBill()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill3 = header.Bills.AddNew("bill3");
			bill3.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			header.Bills.AddNew("bill4");

			var bllRegistration = new BLLRegistration(header, BLLFunctionCode.RegisterSplit, bill1);
			bllRegistration.JPM_ChangeReasonCode = AFRBLLChangeReasonCodeList.Codes.ChangeInCargoOperation;
			AssertEquals(bill1, bllRegistration.MasterAFRBill);
			AssertEquals(2, bllRegistration.AvailableBills.Count);
			AssertEquals("bill2", bllRegistration.AvailableBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill3", bllRegistration.AvailableBills[1].JPM_BillOfLadingNumber);
			bllRegistration.SelectBill(bllRegistration.AvailableBills[0]);
			bllRegistration.SelectBill(bllRegistration.AvailableBills[0]);
			AssertEquals(2, bllRegistration.SelectedBills.Count);
			AssertEquals("bill2", bllRegistration.SelectedBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill3", bllRegistration.SelectedBills[1].JPM_BillOfLadingNumber);
			bill1.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			bill2.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			bill3.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			bllRegistration.Factory.Save();

			AssertNotNull(bill1.BLLFunctionInfo);
			AssertEquals(1, bill1.BLLFunctionInfo.JP_FunctionCode);
			AssertEquals("2", bill1.BLLFunctionInfo.JP_ChangeReasonCode);
			AssertEquals($"bill2{BLLFunctionInfoTest.SplitChar}bill3", bill1.BLLFunctionInfo.JP_LinkedBills);

			var bllCancellation = new BLLCancellation(header, BLLFunctionCode.CancelSplit);
			AssertEquals("", bllCancellation.JPM_BillOfLadingNumber);
			AssertEquals("", bllCancellation.JPM_ChangeReasonCode);
			AssertNull(bllCancellation.MasterAFRBill);
			AssertEquals(0, bllCancellation.AvailableBills.Count);
			AssertEquals(0, bllCancellation.SelectedBills.Count);

			bllCancellation.JPM_BillOfLadingNumber = "bill1";
			AssertEquals("bill1", bllCancellation.JPM_BillOfLadingNumber);
			AssertEquals("2", bllCancellation.JPM_ChangeReasonCode);
			AssertEquals(bill1, bllCancellation.MasterAFRBill);
			AssertEquals(0, bllCancellation.AvailableBills.Count);
			AssertEquals(2, bllCancellation.SelectedBills.Count);
			AssertEquals("bill2", bllCancellation.SelectedBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill3", bllCancellation.SelectedBills[1].JPM_BillOfLadingNumber);
		}

		public void TestJPM_BillOfLadingNumber_CancelMerge_WithoutSelectedAFRBill()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill3 = header.Bills.AddNew("bill3");
			bill3.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			header.Bills.AddNew("bill4");

			var bllRegistration = new BLLRegistration(header, BLLFunctionCode.RegisterMerge, bill1);
			bllRegistration.JPM_ChangeReasonCode = AFRBLLChangeReasonCodeList.Codes.ChangeInCargoOperation;
			AssertEquals(bill1, bllRegistration.MasterAFRBill);
			AssertEquals(2, bllRegistration.AvailableBills.Count);
			AssertEquals("bill2", bllRegistration.AvailableBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill3", bllRegistration.AvailableBills[1].JPM_BillOfLadingNumber);
			bllRegistration.SelectBill(bllRegistration.AvailableBills[0]);
			bllRegistration.SelectBill(bllRegistration.AvailableBills[0]);
			AssertEquals(2, bllRegistration.SelectedBills.Count);
			AssertEquals("bill2", bllRegistration.SelectedBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill3", bllRegistration.SelectedBills[1].JPM_BillOfLadingNumber);
			bill1.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			bill2.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			bill3.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			bllRegistration.Factory.Save();

			AssertNotNull(bill1.BLLFunctionInfo);
			AssertEquals(3, bill1.BLLFunctionInfo.JP_FunctionCode);
			AssertEquals("2", bill1.BLLFunctionInfo.JP_ChangeReasonCode);
			AssertEquals($"bill2{BLLFunctionInfoTest.SplitChar}bill3", bill1.BLLFunctionInfo.JP_LinkedBills);

			var bllCancellation = new BLLCancellation(header, BLLFunctionCode.CancelMerge);
			AssertEquals("", bllCancellation.JPM_BillOfLadingNumber);
			AssertEquals("", bllCancellation.JPM_ChangeReasonCode);
			AssertNull(bllCancellation.MasterAFRBill);
			AssertEquals(0, bllCancellation.AvailableBills.Count);
			AssertEquals(0, bllCancellation.SelectedBills.Count);

			bllCancellation.JPM_BillOfLadingNumber = "bill1";
			AssertEquals("bill1", bllCancellation.JPM_BillOfLadingNumber);
			AssertEquals("2", bllCancellation.JPM_ChangeReasonCode);
			AssertEquals(bill1, bllCancellation.MasterAFRBill);
			AssertEquals(0, bllCancellation.AvailableBills.Count);
			AssertEquals(2, bllCancellation.SelectedBills.Count);
			AssertEquals("bill2", bllCancellation.SelectedBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill3", bllCancellation.SelectedBills[1].JPM_BillOfLadingNumber);
		}

		public void TestJPM_BillOfLadingNumber_CancelSwitch_WithoutSelectedAFRBill()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill3 = header.Bills.AddNew("bill3");
			bill3.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			header.Bills.AddNew("bill4");

			var bllRegistration = new BLLRegistration(header, BLLFunctionCode.RegisterSwitch, bill1);
			bllRegistration.JPM_ChangeReasonCode = AFRBLLChangeReasonCodeList.Codes.ChangeInCargoOperation;
			AssertEquals(bill1, bllRegistration.MasterAFRBill);
			AssertEquals(2, bllRegistration.AvailableBills.Count);
			AssertEquals("bill2", bllRegistration.AvailableBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill3", bllRegistration.AvailableBills[1].JPM_BillOfLadingNumber);
			bllRegistration.SelectBill(bllRegistration.AvailableBills[0]);
			AssertEquals(1, bllRegistration.SelectedBills.Count);
			AssertEquals("bill2", bllRegistration.SelectedBills[0].JPM_BillOfLadingNumber);
			bill1.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			bill2.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			bllRegistration.Factory.Save();

			AssertNotNull(bill1.BLLFunctionInfo);
			AssertEquals(2, bill1.BLLFunctionInfo.JP_FunctionCode);
			AssertEquals("2", bill1.BLLFunctionInfo.JP_ChangeReasonCode);
			AssertEquals("bill2", bill1.BLLFunctionInfo.JP_LinkedBills);

			var bllCancellation = new BLLCancellation(header, BLLFunctionCode.CancelSwitch);
			AssertEquals("", bllCancellation.JPM_BillOfLadingNumber);
			AssertEquals("", bllCancellation.JPM_ChangeReasonCode);
			AssertNull(bllCancellation.MasterAFRBill);
			AssertEquals(0, bllCancellation.AvailableBills.Count);
			AssertEquals(0, bllCancellation.SelectedBills.Count);

			bllCancellation.JPM_BillOfLadingNumber = "bill1";
			AssertEquals("bill1", bllCancellation.JPM_BillOfLadingNumber);
			AssertEquals("2", bllCancellation.JPM_ChangeReasonCode);
			AssertEquals(bill1, bllCancellation.MasterAFRBill);
			AssertEquals(1, bllCancellation.AvailableBills.Count);
			AssertEquals("bill3", bllCancellation.AvailableBills[0].JPM_BillOfLadingNumber);
			AssertEquals(1, bllCancellation.SelectedBills.Count);
			AssertEquals("bill2", bllCancellation.SelectedBills[0].JPM_BillOfLadingNumber);
		}

		public void TestJPM_BillOfLadingNumber_CancelSplit_WithSelectedAFRBill()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill3 = header.Bills.AddNew("bill3");
			bill3.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			header.Bills.AddNew("bill4");

			var bllRegistration = new BLLRegistration(header, BLLFunctionCode.RegisterSplit, bill1);
			bllRegistration.JPM_ChangeReasonCode = AFRBLLChangeReasonCodeList.Codes.ChangeInCargoOperation;
			AssertEquals(bill1, bllRegistration.MasterAFRBill);
			AssertEquals(2, bllRegistration.AvailableBills.Count);
			AssertEquals("bill2", bllRegistration.AvailableBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill3", bllRegistration.AvailableBills[1].JPM_BillOfLadingNumber);
			bllRegistration.SelectBill(bllRegistration.AvailableBills[0]);
			bllRegistration.SelectBill(bllRegistration.AvailableBills[0]);
			AssertEquals(2, bllRegistration.SelectedBills.Count);
			AssertEquals("bill2", bllRegistration.SelectedBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill3", bllRegistration.SelectedBills[1].JPM_BillOfLadingNumber);
			bill1.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			bill2.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			bill3.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			bllRegistration.Factory.Save();

			AssertNotNull(bill1.BLLFunctionInfo);
			AssertEquals(1, bill1.BLLFunctionInfo.JP_FunctionCode);
			AssertEquals("2", bill1.BLLFunctionInfo.JP_ChangeReasonCode);
			AssertEquals($"bill2{BLLFunctionInfoTest.SplitChar}bill3", bill1.BLLFunctionInfo.JP_LinkedBills);

			var bllCancellation = new BLLCancellation(header, BLLFunctionCode.CancelSplit, bill1);
			AssertEquals("bill1", bllCancellation.JPM_BillOfLadingNumber);
			AssertEquals("2", bllCancellation.JPM_ChangeReasonCode);
			AssertEquals(bill1, bllCancellation.MasterAFRBill);
			AssertEquals(0, bllCancellation.AvailableBills.Count);
			AssertEquals(2, bllCancellation.SelectedBills.Count);
			AssertEquals("bill2", bllCancellation.SelectedBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill3", bllCancellation.SelectedBills[1].JPM_BillOfLadingNumber);
		}

		public void TestJPM_BillOfLadingNumber_CancelMerge_WithSelectedAFRBill()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill3 = header.Bills.AddNew("bill3");
			bill3.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			header.Bills.AddNew("bill4");

			var bllRegistration = new BLLRegistration(header, BLLFunctionCode.RegisterMerge, bill1);
			bllRegistration.JPM_ChangeReasonCode = AFRBLLChangeReasonCodeList.Codes.ChangeInCargoOperation;
			AssertEquals(bill1, bllRegistration.MasterAFRBill);
			AssertEquals(2, bllRegistration.AvailableBills.Count);
			AssertEquals("bill2", bllRegistration.AvailableBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill3", bllRegistration.AvailableBills[1].JPM_BillOfLadingNumber);
			bllRegistration.SelectBill(bllRegistration.AvailableBills[0]);
			bllRegistration.SelectBill(bllRegistration.AvailableBills[0]);
			AssertEquals(2, bllRegistration.SelectedBills.Count);
			AssertEquals("bill2", bllRegistration.SelectedBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill3", bllRegistration.SelectedBills[1].JPM_BillOfLadingNumber);
			bill1.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			bill2.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			bill3.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			bllRegistration.Factory.Save();

			AssertNotNull(bill1.BLLFunctionInfo);
			AssertEquals(3, bill1.BLLFunctionInfo.JP_FunctionCode);
			AssertEquals("2", bill1.BLLFunctionInfo.JP_ChangeReasonCode);
			AssertEquals($"bill2{BLLFunctionInfoTest.SplitChar}bill3", bill1.BLLFunctionInfo.JP_LinkedBills);

			var bllCancellation = new BLLCancellation(header, BLLFunctionCode.CancelMerge, bill1);
			AssertEquals("bill1", bllCancellation.JPM_BillOfLadingNumber);
			AssertEquals("2", bllCancellation.JPM_ChangeReasonCode);
			AssertEquals(bill1, bllCancellation.MasterAFRBill);
			AssertEquals(0, bllCancellation.AvailableBills.Count);
			AssertEquals(2, bllCancellation.SelectedBills.Count);
			AssertEquals("bill2", bllCancellation.SelectedBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill3", bllCancellation.SelectedBills[1].JPM_BillOfLadingNumber);
		}

		public void TestJPM_BillOfLadingNumber_CancelSwitch_WithSelectedAFRBill()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill3 = header.Bills.AddNew("bill3");
			bill3.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			header.Bills.AddNew("bill4");

			var bllRegistration = new BLLRegistration(header, BLLFunctionCode.RegisterSwitch, bill1);
			bllRegistration.JPM_ChangeReasonCode = AFRBLLChangeReasonCodeList.Codes.ChangeInCargoOperation;
			AssertEquals(bill1, bllRegistration.MasterAFRBill);
			AssertEquals(2, bllRegistration.AvailableBills.Count);
			AssertEquals("bill2", bllRegistration.AvailableBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill3", bllRegistration.AvailableBills[1].JPM_BillOfLadingNumber);
			bllRegistration.SelectBill(bllRegistration.AvailableBills[0]);
			AssertEquals(1, bllRegistration.SelectedBills.Count);
			AssertEquals("bill2", bllRegistration.SelectedBills[0].JPM_BillOfLadingNumber);
			bill1.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			bill2.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			bllRegistration.Factory.Save();

			AssertNotNull(bill1.BLLFunctionInfo);
			AssertEquals(2, bill1.BLLFunctionInfo.JP_FunctionCode);
			AssertEquals("2", bill1.BLLFunctionInfo.JP_ChangeReasonCode);
			AssertEquals("bill2", bill1.BLLFunctionInfo.JP_LinkedBills);

			var bllCancellation = new BLLCancellation(header, BLLFunctionCode.CancelSwitch, bill1);
			AssertEquals("bill1", bllCancellation.JPM_BillOfLadingNumber);
			AssertEquals("2", bllCancellation.JPM_ChangeReasonCode);
			AssertEquals(bill1, bllCancellation.MasterAFRBill);
			AssertEquals(1, bllCancellation.AvailableBills.Count);
			AssertEquals("bill3", bllCancellation.AvailableBills[0].JPM_BillOfLadingNumber);
			AssertEquals(1, bllCancellation.SelectedBills.Count);
			AssertEquals("bill2", bllCancellation.SelectedBills[0].JPM_BillOfLadingNumber);
		}

		public void TestSelectedBillsAndAvailableBills_CancelSplit()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill3 = header.Bills.AddNew("bill3");
			bill3.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			header.Bills.AddNew("bill4");

			var bllRegistration = new BLLRegistration(header, BLLFunctionCode.RegisterSplit, bill1);
			AssertEquals(bill1, bllRegistration.MasterAFRBill);
			AssertEquals(2, bllRegistration.AvailableBills.Count);
			AssertEquals("bill2", bllRegistration.AvailableBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill3", bllRegistration.AvailableBills[1].JPM_BillOfLadingNumber);
			bllRegistration.SelectBill(bllRegistration.AvailableBills[0]);
			bllRegistration.SelectBill(bllRegistration.AvailableBills[0]);
			AssertEquals(2, bllRegistration.SelectedBills.Count);
			AssertEquals("bill2", bllRegistration.SelectedBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill3", bllRegistration.SelectedBills[1].JPM_BillOfLadingNumber);
			bill1.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			bill2.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			bill3.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			bllRegistration.Factory.Save();

			AssertNotNull(bill1.BLLFunctionInfo);
			AssertEquals(1, bill1.BLLFunctionInfo.JP_FunctionCode);
			AssertEquals("", bill1.BLLFunctionInfo.JP_ChangeReasonCode);
			AssertEquals($"bill2{BLLFunctionInfoTest.SplitChar}bill3", bill1.BLLFunctionInfo.JP_LinkedBills);

			var bllCancellation = new BLLCancellation(header, BLLFunctionCode.CancelSplit, bill1);
			AssertEquals(bill1, bllCancellation.MasterAFRBill);
			AssertEquals(0, bllCancellation.AvailableBills.Count);
			AssertEquals(2, bllCancellation.SelectedBills.Count);
			AssertEquals("bill2", bllCancellation.SelectedBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill3", bllCancellation.SelectedBills[1].JPM_BillOfLadingNumber);
		}

		public void TestSelectedBillsAndAvailableBills_CancelMerge()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill3 = header.Bills.AddNew("bill3");
			bill3.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			header.Bills.AddNew("bill4");

			var bllRegistration = new BLLRegistration(header, BLLFunctionCode.RegisterMerge, bill1);
			AssertEquals(bill1, bllRegistration.MasterAFRBill);
			AssertEquals(2, bllRegistration.AvailableBills.Count);
			AssertEquals("bill2", bllRegistration.AvailableBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill3", bllRegistration.AvailableBills[1].JPM_BillOfLadingNumber);
			bllRegistration.SelectBill(bllRegistration.AvailableBills[0]);
			bllRegistration.SelectBill(bllRegistration.AvailableBills[0]);
			AssertEquals(2, bllRegistration.SelectedBills.Count);
			AssertEquals("bill2", bllRegistration.SelectedBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill3", bllRegistration.SelectedBills[1].JPM_BillOfLadingNumber);
			bill1.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			bill2.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			bill3.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			bllRegistration.Factory.Save();

			AssertNotNull(bill1.BLLFunctionInfo);
			AssertEquals(3, bill1.BLLFunctionInfo.JP_FunctionCode);
			AssertEquals("", bill1.BLLFunctionInfo.JP_ChangeReasonCode);
			AssertEquals($"bill2{BLLFunctionInfoTest.SplitChar}bill3", bill1.BLLFunctionInfo.JP_LinkedBills);

			var bllCancellation = new BLLCancellation(header, BLLFunctionCode.CancelMerge, bill1);
			AssertEquals(bill1, bllCancellation.MasterAFRBill);
			AssertEquals(0, bllCancellation.AvailableBills.Count);
			AssertEquals(2, bllCancellation.SelectedBills.Count);
			AssertEquals("bill2", bllCancellation.SelectedBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill3", bllCancellation.SelectedBills[1].JPM_BillOfLadingNumber);
		}

		public void TestSelectedBillsAndAvailableBills_CancelSwitch()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill3 = header.Bills.AddNew("bill3");
			bill3.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			header.Bills.AddNew("bill4");

			var bllRegistration = new BLLRegistration(header, BLLFunctionCode.RegisterSwitch, bill1);
			AssertEquals(bill1, bllRegistration.MasterAFRBill);
			AssertEquals(2, bllRegistration.AvailableBills.Count);
			AssertEquals("bill2", bllRegistration.AvailableBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill3", bllRegistration.AvailableBills[1].JPM_BillOfLadingNumber);
			bllRegistration.SelectBill(bllRegistration.AvailableBills[0]);
			AssertEquals(1, bllRegistration.SelectedBills.Count);
			AssertEquals("bill2", bllRegistration.SelectedBills[0].JPM_BillOfLadingNumber);
			bill1.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			bill2.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			bllRegistration.Factory.Save();

			AssertNotNull(bill1.BLLFunctionInfo);
			AssertEquals(2, bill1.BLLFunctionInfo.JP_FunctionCode);
			AssertEquals("", bill1.BLLFunctionInfo.JP_ChangeReasonCode);
			AssertEquals("bill2", bill1.BLLFunctionInfo.JP_LinkedBills);

			var bllCancellation = new BLLCancellation(header, BLLFunctionCode.CancelSwitch, bill1);
			AssertEquals(bill1, bllCancellation.MasterAFRBill);
			AssertEquals(1, bllCancellation.AvailableBills.Count);
			AssertEquals("bill3", bllCancellation.AvailableBills[0].JPM_BillOfLadingNumber);
			AssertEquals(1, bllCancellation.SelectedBills.Count);
			AssertEquals("bill2", bllCancellation.SelectedBills[0].JPM_BillOfLadingNumber);
		}

		public void TestRegisteredBills()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			header.Bills.AddNew("bill3");
			var bill4 = header.Bills.AddNew("bill4");
			bill4.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			bill4.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill5 = header.Bills.AddNew("bill5");
			bill5.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			bill5.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;

			var bllFunction = new BLLCancellation(header, BLLFunctionCode.CancelSplit);
			AssertEquals(4, bllFunction.RegisteredBills.Count);
			AssertEquals("bill1", bllFunction.RegisteredBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill2", bllFunction.RegisteredBills[1].JPM_BillOfLadingNumber);
			AssertEquals("bill4", bllFunction.RegisteredBills[2].JPM_BillOfLadingNumber);
			AssertEquals("bill5", bllFunction.RegisteredBills[3].JPM_BillOfLadingNumber);

			bllFunction = new BLLCancellation(header, BLLFunctionCode.CancelSwitch);
			AssertEquals(4, bllFunction.RegisteredBills.Count);
			AssertEquals("bill1", bllFunction.RegisteredBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill2", bllFunction.RegisteredBills[1].JPM_BillOfLadingNumber);
			AssertEquals("bill4", bllFunction.RegisteredBills[2].JPM_BillOfLadingNumber);
			AssertEquals("bill5", bllFunction.RegisteredBills[3].JPM_BillOfLadingNumber);

			bllFunction = new BLLCancellation(header, BLLFunctionCode.CancelSwitch);
			AssertEquals(4, bllFunction.RegisteredBills.Count);
			AssertEquals("bill1", bllFunction.RegisteredBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill2", bllFunction.RegisteredBills[1].JPM_BillOfLadingNumber);
			AssertEquals("bill4", bllFunction.RegisteredBills[2].JPM_BillOfLadingNumber);
			AssertEquals("bill5", bllFunction.RegisteredBills[3].JPM_BillOfLadingNumber);
		}

		public void TestSendEnabled_CancelSplit_WithBusinessLogic()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill3 = header.Bills.AddNew("bill3");
			bill3.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			header.Bills.AddNew("bill4");

			var bllRegistration = new BLLRegistration(header, BLLFunctionCode.RegisterSplit, bill1);
			AssertEquals(bill1, bllRegistration.MasterAFRBill);
			AssertEquals(2, bllRegistration.AvailableBills.Count);
			AssertEquals("bill2", bllRegistration.AvailableBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill3", bllRegistration.AvailableBills[1].JPM_BillOfLadingNumber);
			bllRegistration.SelectBill(bllRegistration.AvailableBills[0]);
			bllRegistration.SelectBill(bllRegistration.AvailableBills[0]);
			AssertEquals(2, bllRegistration.SelectedBills.Count);
			AssertEquals("bill2", bllRegistration.SelectedBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill3", bllRegistration.SelectedBills[1].JPM_BillOfLadingNumber);
			bill1.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			bill2.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			bill3.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			bllRegistration.Factory.Save();

			AssertNotNull(bill1.BLLFunctionInfo);
			AssertEquals(1, bill1.BLLFunctionInfo.JP_FunctionCode);
			AssertEquals("", bill1.BLLFunctionInfo.JP_ChangeReasonCode);
			AssertEquals($"bill2{BLLFunctionInfoTest.SplitChar}bill3", bill1.BLLFunctionInfo.JP_LinkedBills);

			var bllCancellation = new BLLCancellation(header, BLLFunctionCode.CancelSplit, bill1);
			AssertEquals(bill1, bllCancellation.MasterAFRBill);
			AssertEquals(0, bllCancellation.AvailableBills.Count);
			AssertEquals(2, bllCancellation.SelectedBills.Count);
			AssertEquals("bill2", bllCancellation.SelectedBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill3", bllCancellation.SelectedBills[1].JPM_BillOfLadingNumber);

			AssertEquals(true, bllCancellation.SendEnabled);
		}

		public void TestSendEnabled_CancelSplit_WithoutBusinessLogic()
		{
			var header = Factory.New<JPAFRHeader>();
			var bllCancellation = new BLLCancellation(header, BLLFunctionCode.CancelSplit);
			AssertEquals(false, bllCancellation.SendEnabled);

			bllCancellation.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
			AssertEquals(false, bllCancellation.SendEnabled);

			bllCancellation.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
			AssertEquals(true, bllCancellation.SendEnabled);

			bllCancellation.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
			AssertEquals(true, bllCancellation.SendEnabled);

			bllCancellation.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
			AssertEquals(true, bllCancellation.SendEnabled);

			bllCancellation.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
			AssertEquals(true, bllCancellation.SendEnabled);

			bllCancellation.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
			AssertEquals(true, bllCancellation.SendEnabled);

			bllCancellation.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
			AssertEquals(true, bllCancellation.SendEnabled);

			bllCancellation.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
			AssertEquals(true, bllCancellation.SendEnabled);

			bllCancellation.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
			AssertEquals(true, bllCancellation.SendEnabled);

			bllCancellation.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
			AssertEquals(true, bllCancellation.SendEnabled);

			bllCancellation.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
			AssertEquals(false, bllCancellation.SendEnabled);
		}

		public void TestSendEnabled_CancelMerge_WithBusinessLogic()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill3 = header.Bills.AddNew("bill3");
			bill3.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			header.Bills.AddNew("bill4");

			var bllRegistration = new BLLRegistration(header, BLLFunctionCode.RegisterMerge, bill1);
			AssertEquals(bill1, bllRegistration.MasterAFRBill);
			AssertEquals(2, bllRegistration.AvailableBills.Count);
			AssertEquals("bill2", bllRegistration.AvailableBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill3", bllRegistration.AvailableBills[1].JPM_BillOfLadingNumber);
			bllRegistration.SelectBill(bllRegistration.AvailableBills[0]);
			bllRegistration.SelectBill(bllRegistration.AvailableBills[0]);
			AssertEquals(2, bllRegistration.SelectedBills.Count);
			AssertEquals("bill2", bllRegistration.SelectedBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill3", bllRegistration.SelectedBills[1].JPM_BillOfLadingNumber);
			bill1.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			bill2.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			bill3.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			bllRegistration.Factory.Save();

			AssertNotNull(bill1.BLLFunctionInfo);
			AssertEquals(3, bill1.BLLFunctionInfo.JP_FunctionCode);
			AssertEquals("", bill1.BLLFunctionInfo.JP_ChangeReasonCode);
			AssertEquals($"bill2{BLLFunctionInfoTest.SplitChar}bill3", bill1.BLLFunctionInfo.JP_LinkedBills);

			var bllCancellation = new BLLCancellation(header, BLLFunctionCode.CancelMerge, bill1);
			AssertEquals(bill1, bllCancellation.MasterAFRBill);
			AssertEquals(0, bllCancellation.AvailableBills.Count);
			AssertEquals(2, bllCancellation.SelectedBills.Count);
			AssertEquals("bill2", bllCancellation.SelectedBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill3", bllCancellation.SelectedBills[1].JPM_BillOfLadingNumber);

			AssertEquals(true, bllCancellation.SendEnabled);
		}

		public void TestSendEnabled_CancelMerge_WithoutBusinessLogic()
		{
			var header = Factory.New<JPAFRHeader>();
			var bllCancellation = new BLLCancellation(header, BLLFunctionCode.CancelMerge);
			AssertEquals(false, bllCancellation.SendEnabled);

			bllCancellation.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
			AssertEquals(false, bllCancellation.SendEnabled);

			bllCancellation.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
			AssertEquals(true, bllCancellation.SendEnabled);

			bllCancellation.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
			AssertEquals(true, bllCancellation.SendEnabled);

			bllCancellation.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
			AssertEquals(true, bllCancellation.SendEnabled);

			bllCancellation.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
			AssertEquals(true, bllCancellation.SendEnabled);

			bllCancellation.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
			AssertEquals(true, bllCancellation.SendEnabled);

			bllCancellation.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
			AssertEquals(true, bllCancellation.SendEnabled);

			bllCancellation.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
			AssertEquals(true, bllCancellation.SendEnabled);

			bllCancellation.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
			AssertEquals(true, bllCancellation.SendEnabled);

			bllCancellation.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
			AssertEquals(true, bllCancellation.SendEnabled);

			bllCancellation.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
			AssertEquals(false, bllCancellation.SendEnabled);
		}

		public void TestSendEnabled_CancelSwitch_WithBusinessLogic()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			var bill3 = header.Bills.AddNew("bill3");
			bill3.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			header.Bills.AddNew("bill4");

			var bllRegistration = new BLLRegistration(header, BLLFunctionCode.RegisterSwitch, bill1);
			AssertEquals(bill1, bllRegistration.MasterAFRBill);
			AssertEquals(2, bllRegistration.AvailableBills.Count);
			AssertEquals("bill2", bllRegistration.AvailableBills[0].JPM_BillOfLadingNumber);
			AssertEquals("bill3", bllRegistration.AvailableBills[1].JPM_BillOfLadingNumber);
			bllRegistration.SelectBill(bllRegistration.AvailableBills[0]);
			AssertEquals(1, bllRegistration.SelectedBills.Count);
			AssertEquals("bill2", bllRegistration.SelectedBills[0].JPM_BillOfLadingNumber);
			bill1.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			bill2.JPB_MessageStatus = MessageStatusList.Codes.AwaitingBLLRegistration;
			bllRegistration.Factory.Save();

			AssertNotNull(bill1.BLLFunctionInfo);
			AssertEquals(2, bill1.BLLFunctionInfo.JP_FunctionCode);
			AssertEquals("", bill1.BLLFunctionInfo.JP_ChangeReasonCode);
			AssertEquals("bill2", bill1.BLLFunctionInfo.JP_LinkedBills);

			var bllCancellation = new BLLCancellation(header, BLLFunctionCode.CancelSwitch, bill1);
			AssertEquals(bill1, bllCancellation.MasterAFRBill);
			AssertEquals(1, bllCancellation.AvailableBills.Count);
			AssertEquals("bill3", bllCancellation.AvailableBills[0].JPM_BillOfLadingNumber);
			AssertEquals(1, bllCancellation.SelectedBills.Count);
			AssertEquals("bill2", bllCancellation.SelectedBills[0].JPM_BillOfLadingNumber);

			AssertEquals(true, bllCancellation.SendEnabled);
		}

		public void TestSendEnabled_CancelSwitch_WithoutBusinessLogic()
		{
			var header = Factory.New<JPAFRHeader>();
			var bllCancellation = new BLLCancellation(header, BLLFunctionCode.CancelSwitch);
			AssertEquals(false, bllCancellation.SendEnabled);

			bllCancellation.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
			AssertEquals(true, bllCancellation.SendEnabled);

			bllCancellation.SelectedBills.Add(new BLLFunctionBill(header.Bills.AddNew()));
			AssertEquals(false, bllCancellation.SendEnabled);
		}

		public void TestSelectEnabled()
		{
			var header = Factory.New<JPAFRHeader>();
			AssertEquals(false, new BLLCancellation(header, BLLFunctionCode.CancelSplit).SelectEnabled);
			AssertEquals(false, new BLLCancellation(header, BLLFunctionCode.CancelMerge).SelectEnabled);
			AssertEquals(false, new BLLCancellation(header, BLLFunctionCode.CancelSwitch).SelectEnabled);
		}

		public void TestUnselectEnabled()
		{
			var header = Factory.New<JPAFRHeader>();
			AssertEquals(false, new BLLCancellation(header, BLLFunctionCode.CancelSplit).UnselectEnabled);
			AssertEquals(false, new BLLCancellation(header, BLLFunctionCode.CancelMerge).UnselectEnabled);
			AssertEquals(false, new BLLCancellation(header, BLLFunctionCode.CancelSwitch).UnselectEnabled);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return BLLFunction.New(Factory.New<JPAFRHeader>(), BLLFunctionCode.CancelSplit);
		}
	}
}
