using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(BulkAddDiscount))]
	public class BulkAddDiscountTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPopulateClientLicenceBillingDiscount()
		{
			var newDiscount = new BulkAddDiscount();
			newDiscount.SystemCode = "AAA";
			newDiscount.SubCode = "BBB";
			newDiscount.DiscountType = "CCC";
			newDiscount.ModuleCode = "DDD";
			newDiscount.BreakAmount = 100;
			newDiscount.Units = 200;
			newDiscount.BreakUnits = "EEE";
			newDiscount.Discount = 300;
			newDiscount.StartDate = new ZDateTime(2016, 2, 16);
			newDiscount.EndDate = new ZDateTime(2016, 3, 16);
			newDiscount.Duration = 1;
			newDiscount.Description = "description";
			newDiscount.Comment = "comment";

			var billingDiscount = Factory.New<ClientLicenceBillingDiscount>();
			newDiscount.PopulateClientLicenceBillingDiscount(billingDiscount);

			AssertEquals("AAA", billingDiscount.L5_SystemCode);
			AssertEquals("BBB", billingDiscount.L5_SubCode);
			AssertEquals("CCC", billingDiscount.L5_Type);
			AssertEquals("DDD", billingDiscount.L5_ModuleCode);
			AssertEquals((ZDecimal)100, billingDiscount.L5_BreakAmount);
			AssertEquals((ZInt)200, billingDiscount.L5_Units);
			AssertEquals("EEE", billingDiscount.L5_BreakUnits);
			AssertEquals((ZDecimal)300, billingDiscount.L5_Discount);
			AssertEquals(new ZDateTime(2016, 2, 16), billingDiscount.L5_StartDate);
			AssertEquals(new ZDateTime(2016, 3, 16), billingDiscount.L5_EndDate);
			AssertEquals((ZShort)1, billingDiscount.L5_Duration);
			AssertEquals("description", billingDiscount.L5_Description);
			AssertEquals("comment", billingDiscount.L5_Comment);
		}

		public void TestPropertiesReadOnly()
		{
			var newDiscount = new BulkAddDiscount();
			AssertEquals(true, newDiscount.SubCode_ReadOnly);
			AssertEquals(true, newDiscount.ModuleCode_ReadOnly);
			AssertEquals(false, newDiscount.BreakAmount_ReadOnly);
			AssertEquals(true, newDiscount.Units_ReadOnly);
			AssertEquals(false, newDiscount.Discount_ReadOnly);

			newDiscount.SystemCode = BillingConstants.BillingSystem.AirlineMessaging;
			AssertEquals(false, newDiscount.SubCode_ReadOnly);

			newDiscount.DiscountType = BillingConstants.DiscountType.ModuleSpecific;
			AssertEquals(false, newDiscount.ModuleCode_ReadOnly);
			AssertEquals(true, newDiscount.BreakAmount_ReadOnly);

			newDiscount.DiscountType = BillingConstants.DiscountType.MinimumFee;
			AssertEquals(false, newDiscount.Units_ReadOnly);
			AssertEquals(true, newDiscount.Discount_ReadOnly);
		}
	}
}
