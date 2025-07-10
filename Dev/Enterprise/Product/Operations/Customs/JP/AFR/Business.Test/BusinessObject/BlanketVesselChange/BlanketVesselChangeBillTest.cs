using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.JP.AFR;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	[TestedType(typeof(BlanketVesselChangeBill))]
	class BlanketVesselChangeBillTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaults()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			bill.JPB_BillNumber = "MB10232398";
			bill.JPB_ReleaseStatus = AFRBillCustomsStatusList.Codes.Registered;
			bill.JPB_MessageStatus = MessageStatusList.Codes.ClearHouseBillRegistration;
			var sendingBill = new BlanketVesselChangeBill(bill, BlanketVesselChange);

			AssertEquals(AFRBillCustomsStatusList.Codes.Registered, sendingBill.JPM_ReleaseStatus);
			AssertEquals(MessageStatusList.Codes.ClearHouseBillRegistration, sendingBill.JPM_MessageStatus);
			AssertEquals("MB10232398", sendingBill.JPM_BillOfLadingNumber);
			AssertEquals(true, sendingBill.JPM_Send);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			return new BlanketVesselChangeBill(bill, BlanketVesselChange);
		}

		BlanketVesselChange BlanketVesselChange => blanketVesselChange ?? (blanketVesselChange = new BlanketVesselChange(Factory.New<JPAFRHeader>()));
		BlanketVesselChange blanketVesselChange;
	}
}
