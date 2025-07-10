using CargoWise.Types;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class NotificationForwardingPartyValidationTest : CargoWise.EntityFramework.Testing.BusinessObjectValidationTestCase
	{
		public void TestCheckCY_Code()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			var nfp = bill.NotificationForwardingParties.AddNew();
			nfp.CY_Code = "!@";
			AssertNoNotifications(nfp.CY_CodeInfo);
			nfp.CY_Code = ZString.Empty;
			AssertNoNotifications(nfp.CY_CodeInfo);
			nfp.CY_Code = "AM";
			AssertNoNotifications(nfp.CY_CodeInfo);
		}

		public void TestCheckCY_Data()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			var nfp = bill.NotificationForwardingParties.AddNew();
			nfp.CY_Data = "!@";
			AssertHasMessageErrorContaining(nfp.CY_DataInfo, ValidationConstants.Bill.NotificationForwardingPartyInvalid);
			nfp.CY_Data = ZString.Empty;
			AssertHasWarningContaining(nfp.CY_DataInfo, ValidationConstants.Bill.EmptyCusCodeDataWillNotBeIncluded);
			nfp.CY_Data = "AM";
			AssertHasMessageErrorContaining(nfp.CY_DataInfo, ValidationConstants.Bill.NotificationForwardingPartyInvalid);
			nfp.CY_Data = "A_M";
			AssertHasMessageErrorContaining(nfp.CY_DataInfo, ValidationConstants.Bill.NotificationForwardingPartyInvalid);
			nfp.CY_Data = "A M";
			AssertHasMessageErrorContaining(nfp.CY_DataInfo, ValidationConstants.Bill.NotificationForwardingPartyInvalid);
			nfp.CY_Data = "AAMN";
			AssertHasMessageErrorContaining(nfp.CY_DataInfo, ValidationConstants.Bill.NotificationForwardingPartyInvalid);
			nfp.CY_Data = "AAM";
			AssertHasMessageErrorContaining(nfp.CY_DataInfo, ValidationConstants.Bill.NotificationForwardingPartyInvalid);
			nfp.CY_Data = "ABCDE";
			AssertNoNotifications(nfp.CY_DataInfo);
		}

		public void TestCheckCY_Order()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			var nfp1 = bill.NotificationForwardingParties.AddNew();
			var nfp2 = bill.NotificationForwardingParties.AddNew();
			var nfp3 = bill.NotificationForwardingParties.AddNew();
			var nfp4 = bill.NotificationForwardingParties.AddNew();

			AssertEquals((short)1, nfp1.CY_Order);
			AssertEquals((short)2, nfp2.CY_Order);
			AssertEquals((short)3, nfp3.CY_Order);
			AssertEquals((short)4, nfp4.CY_Order);

			AssertNoNotifications(nfp1.CY_OrderInfo);
			AssertNoNotifications(nfp2.CY_OrderInfo);
			AssertNoNotifications(nfp3.CY_OrderInfo);
			AssertHasWarningContaining(nfp4.CY_OrderInfo, ValidationConstants.Bill.CusCodeExceedingMaximumNumber("Notification Forwarding Party", 3));
		}
	}
}
