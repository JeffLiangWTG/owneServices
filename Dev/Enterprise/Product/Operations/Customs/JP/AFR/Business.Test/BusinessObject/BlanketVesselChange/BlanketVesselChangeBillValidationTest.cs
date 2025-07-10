using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class BlanketVesselChangeBillValidationTest : BusinessObjectValidationTestCase
	{
		public void TestJPM_Send_CMV()
		{
			var header = Factory.New<JPAFRHeader>();

			header.Bills.AddNew();
			header.Bills.AddNew();
			AssertEquals(2, header.Bills.Count);

			var vesselChange = new BlanketVesselChange(header);
			AssertEquals(2, vesselChange.BlanketVesselChangeBills.Count);

			vesselChange.JPM_BlanketChange = ZBool.False;

			var vesselChangeBill1 = vesselChange.BlanketVesselChangeBills[0];
			var vesselChangeBill2 = vesselChange.BlanketVesselChangeBills[1];

			vesselChangeBill1.JPM_Send = ZBool.False;
			vesselChangeBill2.JPM_Send = ZBool.False;
			AssertHasMessageError(vesselChangeBill1.JPM_SendInfo, ValidationConstants.MessageSending.NoBillsHaveBeenChecked);
			AssertHasMessageError(vesselChangeBill2.JPM_SendInfo, ValidationConstants.MessageSending.NoBillsHaveBeenChecked);

			vesselChangeBill1.JPM_Send = ZBool.False;
			vesselChangeBill2.JPM_Send = ZBool.True;
			AssertHasMessageError(vesselChangeBill1.JPM_SendInfo, ValidationConstants.MessageSending.AnyBillIsLeftUnchecked);
			AssertNoNotifications(vesselChangeBill2.JPM_SendInfo);

			vesselChangeBill1.JPM_Send = ZBool.True;
			vesselChangeBill2.JPM_Send = ZBool.True;
			AssertNoNotifications(vesselChangeBill1.JPM_SendInfo);
			AssertNoNotifications(vesselChangeBill2.JPM_SendInfo);
		}
	}
}
