using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class EdiDepositAdjustValidationTest : BusinessObjectValidationTestCase
	{
		public void TestDEA_RX_NKCurrency()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.CreateAndLoadLicenceForOrg();
			var company = org.LicCompany;
			var adjust = Factory.NewWithValidTestData<EdiDepositAdjust>();
			adjust.DEA_OH = org.PK;
			adjust.DEA_RX_NKCurrency = "AUD";
			adjust.DEA_ChargeCode = EDIDataRegistry.Instance.OdplDepositChargeCode.Value;
			AssertNoErrors(adjust.DEA_RX_NKCurrencyInfo);
			adjust.DEA_RX_NKCurrency = "";

			company.SetMonthlyUsageDepositBalanceForTest(0, "AUD");
			adjust.DEA_RX_NKCurrency = "USD";
			AssertNoErrors(adjust.DEA_RX_NKCurrencyInfo);
			adjust.DEA_RX_NKCurrency = "";

			company.SetMonthlyUsageDepositBalanceForTest(123, "AUD");
			adjust.DEA_RX_NKCurrency = "AUD";
			AssertNoErrors(adjust.DEA_RX_NKCurrencyInfo);

			adjust.DEA_RX_NKCurrency = "USD";
			AssertHasError(adjust.DEA_RX_NKCurrencyInfo, "Currency must match balance currency");

			adjust.DEA_ChargeCode = EDIDataRegistry.SecurityDepositChargeCode;
			adjust.DEA_RX_NKCurrency = "SGD";
			AssertNoErrors(adjust.DEA_RX_NKCurrencyInfo);
		}

		[TestDate(2020, 5, 1)]
		public void TestDEA_RX_NKCurrency_WhenInDatabase()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			org.CreateAndLoadLicenceForOrg();
			var company = org.LicCompany;
			var adjust = Factory.New<EdiDepositAdjust>();
			adjust.DEA_OH = org.PK;
			adjust.DEA_RX_NKCurrency = "AUD";
			adjust.DEA_ChargeCode = "DEPOSIT";
			adjust.DEA_Amount = 100m;
			adjust.DEA_GC = Env.CurrentCompanyPK;
			AssertNoErrors(adjust);
			Factory.Save();

			var balance = company.DepositBalances.Cast<DepositBalance>().Single(x => x.ChargeCode == adjust.DEA_ChargeCode);
			balance.CurrencyCode = "USD";
			balance.Amount = 50m;
			adjust.RunPreSaveValidation();
			AssertNoErrors("no error if in database", adjust);
		}
	}
}