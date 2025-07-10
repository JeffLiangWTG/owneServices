using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	public class ReceiptPaymentDefaultsARTest : ReceiptPaymentDefaultsTest
	{
		#region GetDefaultBankAccount Test

		public void TestStaticGetDefaultBankAccount()
		{
			RefCurrency testCurrency = Factory.NewWithValidTestData<RefCurrency>();

			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_IsDebtor = true;
			testOrg.CompanyData.OB_RX_NKARDDefltCurrency = testCurrency.RX_Code;

			AccBankAccount defaultBankCurrentBranch = GetNewBankAccountWithCurrency(testCurrency, true);
			defaultBankCurrentBranch.AB_GB = GlbBranch.CurrentBranch.PK;

			AccBankAccount defaultBank = GetNewBankAccountWithCurrency(testCurrency, true);
			defaultBank.AB_GB = ZGuid.Empty;

			AccBankAccount testNonDefaultBank = GetNewBankAccountWithCurrency(testCurrency, false);

			AccBankAccount localCurrencyBank = GetNewBankAccountWithCurrency(GlbCompany.CurrentCompany.LocalCurrency, true);

			OrgHeader testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg2.OH_IsDebtor = true;
			testOrg2.CompanyData.OB_RX_NKARDDefltCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			OrgHeader testOrg3 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg3.OH_IsDebtor = true;
			testOrg3.CompanyData.OB_RX_NKARDDefltCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			OrgDebtorGroup debtorGoup1 = Factory.NewWithValidTestData<OrgDebtorGroup>();
			testOrg3.CompanyData.OB_OJ_ARDebtorGroup = debtorGoup1.PK;
			debtorGoup1.DefaultBankAccountPK = testNonDefaultBank.PK;

			Factory.Save();

			ARReceipt aRRec = Factory.NewWithValidTestData<ARReceipt>();
			aRRec.AH_OH = testOrg.PK;
			AssertEquals("Bank account should be DefaultBankCurrentBranch", defaultBankCurrentBranch.PK,
				ReceiptPaymentDefaultsAR.GetDefaultBankAccount(aRRec.Header));

			aRRec.AH_OH = testOrg2.PK;
			AssertEquals("Bank account should be DefaultBank2", localCurrencyBank.PK,
				ReceiptPaymentDefaultsAR.GetDefaultBankAccount(aRRec.Header));

			testOrg2.CompanyData.OverrideBankAccountFromDebtorGroup = true;
			testOrg2.CompanyData.ARBankAccountToDisplay = testNonDefaultBank.PK;

			aRRec.AH_OH = testOrg2.PK;
			AssertEquals("Bank account should be NonDefaultBank", testNonDefaultBank.PK,
				ReceiptPaymentDefaultsAR.GetDefaultBankAccount(aRRec.Header));

			aRRec.AH_OH = testOrg3.PK;
			AssertEquals("Bank account should be NonDefaultBank", testNonDefaultBank.PK,
				ReceiptPaymentDefaultsAR.GetDefaultBankAccount(aRRec.Header));

			debtorGoup1.OverrideRegistryCurrencyToBankSetting = true;
			OrgDebtorGroupBankCurrentOverride bankOverride = debtorGoup1.OrgDebtorGroupBankCurrentOverrideCollection.AddNew();
			bankOverride.PB_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			bankOverride.PB_AB = localCurrencyBank.PK;

			AssertEquals("Bank account should be LocalCurrencyBank", localCurrencyBank.PK,
				ReceiptPaymentDefaultsAR.GetDefaultBankAccount(aRRec.Header));
		}

		public override void TestGetDefaultBankAccount()
		{
			RefCurrency testCurrency = Factory.NewWithValidTestData<RefCurrency>();

			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_IsDebtor = true;
			testOrg.CompanyData.OB_RX_NKARDDefltCurrency = testCurrency.RX_Code;

			AccBankAccount defaultBankCurrentBranch = GetNewBankAccountWithCurrency(testCurrency, true);
			defaultBankCurrentBranch.AB_GB = GlbBranch.CurrentBranch.PK;

			AccBankAccount defaultBank = GetNewBankAccountWithCurrency(testCurrency, true);
			defaultBank.AB_GB = ZGuid.Empty;

			AccBankAccount testNonDefaultBank = GetNewBankAccountWithCurrency(testCurrency, false);

			AccBankAccount localCurrencyBank = GetNewBankAccountWithCurrency(GlbCompany.CurrentCompany.LocalCurrency, true);

			OrgHeader testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg2.OH_IsDebtor = true;
			testOrg2.CompanyData.OB_RX_NKARDDefltCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			OrgHeader testOrg3 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg3.OH_IsDebtor = true;
			testOrg3.CompanyData.OB_RX_NKARDDefltCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			OrgDebtorGroup debtorGoup1 = Factory.NewWithValidTestData<OrgDebtorGroup>();
			testOrg3.CompanyData.OB_OJ_ARDebtorGroup = debtorGoup1.PK;
			debtorGoup1.DefaultBankAccountPK = testNonDefaultBank.PK;

			Factory.Save();

			ARReceipt aRRec = Factory.NewWithValidTestData<ARReceipt>();
			aRRec.AH_OH = testOrg.PK;
			AssertEquals("Bank account should be DefaultBankCurrentBranch", defaultBankCurrentBranch.PK,
				new ReceiptPaymentDefaultsAR(aRRec).GetDefaultBankAccount());

			aRRec.AH_OH = testOrg2.PK;
			AssertEquals("Bank account should be DefaultBank2", localCurrencyBank.PK,
				new ReceiptPaymentDefaultsAR(aRRec).GetDefaultBankAccount());

			testOrg2.CompanyData.OverrideBankAccountFromDebtorGroup = true;
			testOrg2.CompanyData.ARBankAccountToDisplay = testNonDefaultBank.PK;

			aRRec.AH_OH = testOrg2.PK;
			AssertEquals("Bank account should be NonDefaultBank", testNonDefaultBank.PK,
				new ReceiptPaymentDefaultsAR(aRRec).GetDefaultBankAccount());

			aRRec.AH_OH = testOrg3.PK;
			AssertEquals("Bank account should be NonDefaultBank", testNonDefaultBank.PK,
				new ReceiptPaymentDefaultsAR(aRRec).GetDefaultBankAccount());

			debtorGoup1.OverrideRegistryCurrencyToBankSetting = true;
			OrgDebtorGroupBankCurrentOverride bankOverride = debtorGoup1.OrgDebtorGroupBankCurrentOverrideCollection.AddNew();
			bankOverride.PB_RX_NKCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			bankOverride.PB_AB = localCurrencyBank.PK;

			AssertEquals("Bank account should be LocalCurrencyBank", localCurrencyBank.PK,
				new ReceiptPaymentDefaultsAR(aRRec).GetDefaultBankAccount());
		}

		#endregion

		protected AccBankAccount GetNewBankAccountWithCurrency(RefCurrency currency, ZBool isDefaultForAR)
		{
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_RX_NKAccountCurrency = currency.RX_Code;
			bank.AB_IsDefaultReceiptBankAccount = isDefaultForAR;
			return bank;
		}
	}
}
