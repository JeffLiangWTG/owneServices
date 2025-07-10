using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	class BulkAddDiscountValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidate()
		{
			var org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.OH_Code = "ORGAAASYD";
			org1.CreateAndLoadLicenceForOrg();
			org1.LicCompany.LicEnterprise.LE_EnterpriseCode = "AAA";

			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org2.OH_Code = "ORGBBBSYD";
			org2.CreateAndLoadLicenceForOrg();
			org2.LicCompany.LicEnterprise.LE_EnterpriseCode = "BBB";

			var billingDiscount1 = org1.LicCompany.SelfBilling.BillingDiscounts.AddNew();
			var billingDiscount2 = org2.LicCompany.SelfBilling.BillingDiscounts.AddNew();

			var newDiscount = new BulkAddDiscount();
			var validation = new BulkAddDiscountValidation(newDiscount, new ClientLicenceBillingDiscount[] { billingDiscount1, billingDiscount2 });

			billingDiscount1.L5_SystemCode = "AAA";
			billingDiscount2.L5_SystemCode = "BBB";
			validation.ValidateSystemCode();
			AssertEquals(true, newDiscount.SystemCodeInfo.HasError("ORGAAASYD - Enter a valid selection."));
			AssertEquals(true, newDiscount.SystemCodeInfo.HasError("ORGBBBSYD - Enter a valid selection."));

			billingDiscount1.L5_Type = "AAA";
			billingDiscount2.L5_Type = "BBB";
			validation.ValidateDiscountType();
			AssertEquals(true, newDiscount.DiscountTypeInfo.HasError("ORGAAASYD - Enter a valid selection."));
			AssertEquals(true, newDiscount.DiscountTypeInfo.HasError("ORGBBBSYD - Enter a valid selection."));

			billingDiscount1.L5_Type = BillingConstants.DiscountType.ModuleSpecific;
			billingDiscount2.L5_Type = BillingConstants.DiscountType.ModuleSpecific;
			billingDiscount1.L5_ModuleCode = "AAA";
			billingDiscount2.L5_ModuleCode = "BBB";
			validation.ValidateModuleCode();
			AssertEquals(true, newDiscount.ModuleCodeInfo.HasError("ORGAAASYD - Enter a valid selection."));
			AssertEquals(true, newDiscount.ModuleCodeInfo.HasError("ORGBBBSYD - Enter a valid selection."));

			billingDiscount1.L5_Type = BillingConstants.DiscountType.Volume;
			billingDiscount2.L5_Type = BillingConstants.DiscountType.Volume;
			billingDiscount1.L5_BreakAmount = -100;
			billingDiscount2.L5_BreakAmount = -100;
			validation.ValidateBreakAmount();
			AssertEquals(true, newDiscount.BreakAmountInfo.HasError("ORGAAASYD - Please enter a 'Break Amount' greater than or equal to 0."));
			AssertEquals(true, newDiscount.BreakAmountInfo.HasError("ORGBBBSYD - Please enter a 'Break Amount' greater than or equal to 0."));

			billingDiscount1.L5_BreakUnits = "AAA";
			billingDiscount2.L5_BreakUnits = "BBB";
			validation.ValidateBreakUnits();
			AssertEquals(true, newDiscount.BreakUnitsInfo.HasError("ORGAAASYD - Enter a valid selection."));
			AssertEquals(true, newDiscount.BreakUnitsInfo.HasError("ORGBBBSYD - Enter a valid selection."));

			billingDiscount1.L5_Type = BillingConstants.DiscountType.MinimumFee;
			billingDiscount2.L5_Type = BillingConstants.DiscountType.MinimumFee;
			billingDiscount1.L5_Units = -100;
			billingDiscount2.L5_Units = -100;
			validation.ValidateUnits();
			AssertEquals(true, newDiscount.UnitsInfo.HasError("ORGAAASYD - Please enter a 'Units' greater than 0."));
			AssertEquals(true, newDiscount.UnitsInfo.HasError("ORGBBBSYD - Please enter a 'Units' greater than 0."));

			billingDiscount1.L5_Type = BillingConstants.DiscountType.Volume;
			billingDiscount2.L5_Type = BillingConstants.DiscountType.Volume;
			billingDiscount1.L5_Discount = 200;
			billingDiscount2.L5_Discount = 200;
			validation.ValidateDiscount();
			AssertEquals(true, newDiscount.DiscountInfo.HasError("ORGAAASYD - Please enter a 'Discount' less than or equal to 100."));
			AssertEquals(true, newDiscount.DiscountInfo.HasError("ORGBBBSYD - Please enter a 'Discount' less than or equal to 100."));

			billingDiscount1.L5_Type = BillingConstants.DiscountType.Special;
			billingDiscount2.L5_Type = BillingConstants.DiscountType.Special;
			billingDiscount1.L5_Description = "";
			billingDiscount2.L5_Description = "";
			validation.ValidateDescription();
			AssertEquals(true, newDiscount.DescriptionInfo.HasError("ORGAAASYD - Please enter a value."));
			AssertEquals(true, newDiscount.DescriptionInfo.HasError("ORGBBBSYD - Please enter a value."));

			billingDiscount1.L5_StartDate = new ZDateTime(2016, 2, 16);
			billingDiscount2.L5_StartDate = new ZDateTime(2016, 2, 16);
			validation.ValidateStartDate();
			AssertEquals(true, newDiscount.StartDateInfo.HasError("ORGAAASYD - Discount should start on the first day of the month."));
			AssertEquals(true, newDiscount.StartDateInfo.HasError("ORGBBBSYD - Discount should start on the first day of the month."));

			billingDiscount1.L5_EndDate = new ZDateTime(2016, 3, 16);
			billingDiscount2.L5_EndDate = new ZDateTime(2016, 3, 16);
			validation.ValidateEndDate();
			AssertEquals(true, newDiscount.EndDateInfo.HasError("ORGAAASYD - Discount should end on the last day of the month."));
			AssertEquals(true, newDiscount.EndDateInfo.HasError("ORGBBBSYD - Discount should end on the last day of the month."));
		}
	}
}