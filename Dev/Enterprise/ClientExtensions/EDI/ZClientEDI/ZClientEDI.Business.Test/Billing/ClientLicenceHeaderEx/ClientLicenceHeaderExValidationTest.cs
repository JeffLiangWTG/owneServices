using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class ClientLicenceHeaderExValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckL0_RX_NKFixedMaintenanceCurrency()
		{
			LicenceHeader lic = BillingTestHelper.CreateLicence(Factory, "ABC");
			lic.Billing.Validation.ValidateAll();
			AssertNoErrors(lic.Billing.L0_FixedMaintenanceAmountInfo);
			AssertNoErrors(lic.Billing.L0_RX_NKFixedMaintenanceCurrencyInfo);

			lic.Billing.L0_FixedMaintenanceAmount = 10;
			lic.Billing.Validation.ValidateAll();
			AssertNoErrors(lic.Billing.L0_FixedMaintenanceAmountInfo);
			AssertHasErrors(lic.Billing.L0_RX_NKFixedMaintenanceCurrencyInfo);
		}

		public void TestCheckL0_Surcharge()
		{
			LicenceHeader lic = BillingTestHelper.CreateLicence(Factory, "ABC");
			lic.Billing.Validation.ValidateAll();
			AssertNoErrors(lic.Billing.L0_SurchargeInfo);

			lic.Billing.L0_Surcharge = 25;
			lic.Billing.Validation.ValidateAll();
			AssertNoErrors(lic.Billing.L0_SurchargeInfo);

			lic.Billing.L0_Surcharge = 100;
			lic.Billing.Validation.ValidateAll();
			AssertNoErrors(lic.Billing.L0_SurchargeInfo);

			lic.Billing.L0_Surcharge = 101;
			lic.Billing.Validation.ValidateAll();
			AssertHasErrors(lic.Billing.L0_SurchargeInfo);

			lic.Billing.L0_Surcharge = -1;
			lic.Billing.Validation.ValidateAll();
			AssertNoErrors(lic.Billing.L0_SurchargeInfo);

			lic.Billing.L0_Surcharge = -100;
			lic.Billing.Validation.ValidateAll();
			AssertNoErrors(lic.Billing.L0_SurchargeInfo);

			lic.Billing.L0_Surcharge = -101;
			lic.Billing.Validation.ValidateAll();
			AssertHasErrors(lic.Billing.L0_SurchargeInfo);
		}
	}
}