using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class OtherRelevantLawValidationTest : CargoWise.EntityFramework.Testing.BusinessObjectValidationTestCase
	{
		public void TestCheckCY_Code()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			var orl = bill.OtherRelevantLaws.AddNew();
			CombineAssertions(() =>
			{
				orl.CY_Data = "AD";
				orl.CY_Code = "!@";
				AssertNoNotifications(orl.CY_CodeInfo);
				orl.CY_Code = ZString.Empty;
				AssertNoNotifications(orl.CY_CodeInfo);
				orl.CY_Code = "AD";
				AssertNoNotifications(orl.CY_CodeInfo);
			});
		}

		public void TestCheckCY_Data()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			var orl = bill.OtherRelevantLaws.AddNew();
			CombineAssertions(() =>
			{
				orl.CY_Data = "!@";
				AssertHasMessageErrorContaining(orl.CY_DataInfo, ListValidation.InvalidCodeMessageError);
				orl.CY_Data = ZString.Empty;
				AssertHasWarning(orl.CY_DataInfo, ValidationConstants.Bill.EmptyCusCodeDataWillNotBeIncluded);
				orl.CY_Data = "AD";
				AssertNoNotifications(orl.CY_DataInfo);
			});
		}

		public void TestCheckCY_Order()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill = header.Bills.AddNew();
			var orl1 = bill.OtherRelevantLaws.AddNew();
			var orl2 = bill.OtherRelevantLaws.AddNew();
			var orl3 = bill.OtherRelevantLaws.AddNew();
			var orl4 = bill.OtherRelevantLaws.AddNew();
			var orl5 = bill.OtherRelevantLaws.AddNew();
			var orl6 = bill.OtherRelevantLaws.AddNew();

			AssertEquals((short)1, orl1.CY_Order);
			AssertEquals((short)2, orl2.CY_Order);
			AssertEquals((short)3, orl3.CY_Order);
			AssertEquals((short)4, orl4.CY_Order);
			AssertEquals((short)5, orl5.CY_Order);
			AssertEquals((short)6, orl6.CY_Order);
			AssertNoNotifications(orl1.CY_OrderInfo);
			AssertNoNotifications(orl2.CY_OrderInfo);
			AssertNoNotifications(orl3.CY_OrderInfo);
			AssertNoNotifications(orl4.CY_OrderInfo);
			AssertNoNotifications(orl5.CY_OrderInfo);
			AssertHasWarningContaining(orl6.CY_OrderInfo, ValidationConstants.Bill.CusCodeExceedingMaximumNumber("Other Relevant Law", 5));
		}
	}
}
