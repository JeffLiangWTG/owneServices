using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.CreditStatus;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class ARBalanceTest : TestCaseWithFactory
	{
		public void TestGetOutstandingBalance()
		{
			var rate1 = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "GST", Enterprise.MasterFiles.Business.AccTaxRate.Types.Rated, 10);
			var chargeCode = BillingTestHelper.CreateChargeCode(Factory, rate1, "TESTCREDIT");
			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var lic2 = BillingTestHelper.CreateLicence(Factory, "BBB");
			var org1pk = lic1.Company.LC_OH.ToGuid();
			var org2pk = lic2.Company.LC_OH.ToGuid();

			var today = ZDateTime.Today;

			BillingTestHelper.CreateInvoice(Factory, lic1, "TESTCREDIT", 100, 1, "", 0).AH_DueDate = ZDateTime.Empty;
			BillingTestHelper.CreateInvoice(Factory, lic1, "TESTCREDIT", 800, 1, "", 0).AH_DueDate = today.AddDays(3);
			BillingTestHelper.CreateInvoice(Factory, lic2, "TESTCREDIT", 500, 1, "", 0).AH_DueDate = today.AddDays(5);

			var notDueInvoice = BillingTestHelper.CreateInvoice(Factory, lic2, "TESTCREDIT", 10000, 1, "", 0);
			notDueInvoice.AH_DueDate = today.AddDays(60);

			lic2.Company.Header.CompanyData.OB_ARCreditLimit = 1000m;

			Factory.Save();

			var singleAccessor = new ARAPDataAccessor();

			var amounts = ARBalance.GetOutstandingBalance(new Guid[] { org1pk, org2pk }, Env.CurrentCompany.PK, today.AddDays(6).ToDateTime());
			CombineAssertions(() =>
			{
				AssertEquals(-990m, amounts[org1pk]);
				AssertEquals(-550m, amounts[org2pk]);
				AssertEquals(2, amounts.Count);
			});

			amounts = ARBalance.GetOutstandingBalance(new Guid[] { org1pk, org2pk }, Env.CurrentCompany.PK, today.AddDays(5).ToDateTime());
			CombineAssertions(() =>
			{
				AssertEquals(-990m, amounts[org1pk]);
				AssertEquals(1, amounts.Count);
			});

			amounts = ARBalance.GetOutstandingBalance(new Guid[] { org1pk, org2pk }, Env.CurrentCompany.PK, today.AddDays(2).ToDateTime());
			CombineAssertions(() =>
			{
				AssertEquals(-110m, amounts[org1pk]);
				AssertEquals(1, amounts.Count);
			});

			var amount = ARBalance.GetOutstandingBalance(org1pk, Env.CurrentCompany.PK, today.AddDays(6).ToDateTime());
			AssertEquals(-990m, amount);
			amount = ARBalance.GetOutstandingBalance(org2pk, Env.CurrentCompany.PK, today.AddDays(6).ToDateTime());
			AssertEquals(-550m, amount);
		}
	}
}