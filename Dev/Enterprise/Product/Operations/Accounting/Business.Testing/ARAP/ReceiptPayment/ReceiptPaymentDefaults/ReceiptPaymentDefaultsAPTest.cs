using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	public class ReceiptPaymentDefaultsAPTest : ReceiptPaymentDefaultsTest
	{
		public void TestStaticGetDefaultBankAccount()
		{
			APPayment aPPay = Factory.NewWithValidTestData<APPayment>();
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();

			AccBankAccount bank_DiffCompany = GetNewBankAccountWithCompany(company);
			testOrg.CompanyData.OB_AB_APDefaultBankAccount = bank_DiffCompany.PK;
			Factory.Save();

			aPPay.AH_OH = testOrg.PK;
			Assert("Bank account should not default to field on CompanyData since it is from a different company",
				ReceiptPaymentDefaultsAP.GetDefaultBankAccount(aPPay.Header).IsEmpty);

			AccBankAccount bank_CurrentCompany = GetNewBankAccountWithCompany(GlbCompany.CurrentCompany);
			testOrg.CompanyData.OB_AB_APDefaultBankAccount = bank_CurrentCompany.PK;
			Factory.Save();

			aPPay = Factory.NewWithValidTestData<APPayment>();
			aPPay.AH_OH = testOrg.PK;
			AssertEquals("Bank account should default to AP Bank field on Company Data",
				bank_CurrentCompany.PK, ReceiptPaymentDefaultsAP.GetDefaultBankAccount(aPPay.Header));
		}

		public override void TestGetDefaultBankAccount()
		{
			APPayment aPPay = Factory.NewWithValidTestData<APPayment>();
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();

			AccBankAccount bank_DiffCompany = GetNewBankAccountWithCompany(company);
			testOrg.CompanyData.OB_AB_APDefaultBankAccount = bank_DiffCompany.PK;
			Factory.Save();

			aPPay.AH_OH = testOrg.PK;
			Assert("Bank account should not default to field on CompanyData since it is from a different company",
				new ReceiptPaymentDefaultsAP(aPPay).GetDefaultBankAccount().IsEmpty);

			AccBankAccount bank_CurrentCompany = GetNewBankAccountWithCompany(GlbCompany.CurrentCompany);
			testOrg.CompanyData.OB_AB_APDefaultBankAccount = bank_CurrentCompany.PK;
			Factory.Save();

			aPPay = Factory.NewWithValidTestData<APPayment>();
			aPPay.AH_OH = testOrg.PK;
			AssertEquals("Bank account should default to AP Bank field on Company Data",
				bank_CurrentCompany.PK, new ReceiptPaymentDefaultsAP(aPPay).GetDefaultBankAccount());
		}

		protected AccBankAccount GetNewBankAccountWithCompany(GlbCompany company)
		{
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_GC = company.PK;
			return bank;
		}
	}
}
