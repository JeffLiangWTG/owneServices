using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Customs.Common.JP.AFR;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class BLLCancellationLookupsTest : BusinessObjectValidationTestCase
	{
		public void TestRegisteredBillList_CancelSplit()
		{
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
			AssertEquals(1, bllCancellation.Lookups.RegisteredBillList.Count);
			AssertEquals("bill1", ((ICodeDescription)bllCancellation.Lookups.RegisteredBillList[0]).Code);
			AssertEquals("bill1", ((ICodeDescription)bllCancellation.Lookups.RegisteredBillList[0]).Description);
		}

		public void TestRegisteredBillList_CancelMerge()
		{
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
			AssertEquals(1, bllCancellation.Lookups.RegisteredBillList.Count);
			AssertEquals("bill1", ((ICodeDescription)bllCancellation.Lookups.RegisteredBillList[0]).Code);
			AssertEquals("bill1", ((ICodeDescription)bllCancellation.Lookups.RegisteredBillList[0]).Description);
		}

		public void TestRegisteredBillList_CancelSwitch()
		{
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
			AssertEquals(1, bllCancellation.Lookups.RegisteredBillList.Count);
			AssertEquals("bill1", ((ICodeDescription)bllCancellation.Lookups.RegisteredBillList[0]).Code);
			AssertEquals("bill1", ((ICodeDescription)bllCancellation.Lookups.RegisteredBillList[0]).Description);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<JPAFRHeader>();
			bill1 = header.Bills.AddNew("bill1");
			bill1.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			bill2 = header.Bills.AddNew("bill2");
			bill2.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			bill3 = header.Bills.AddNew("bill3");
			bill3.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			header.Bills.AddNew("bill4");
		}
		JPAFRHeader header;
		JPAFRBills bill1;
		JPAFRBills bill2;
		JPAFRBills bill3;
	}
}
