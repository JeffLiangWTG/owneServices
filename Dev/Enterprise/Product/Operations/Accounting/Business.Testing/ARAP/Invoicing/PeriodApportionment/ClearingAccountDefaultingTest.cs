using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing.PeriodApportionment
{
	public class ClearingAccountDefaultingTest : TestCaseWithFactory
	{
		public void TestClearingAccoundDefaulting()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			//AP Clearing
			var drAccRegistry = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "7200.00.00"));
			var drAccChargeCode = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "7300.00.00"));
			var drAccGLPOverride = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "7500.00.00"));

			//AR Clearing
			var crAccRegistry = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "8310.00.00"));
			var crAccChargeCode = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "8210.00.00"));
			var crAccGLPOverride = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "8220.00.00"));

			AccountingConfigurationRegistry.Instance.PeriodApportionmentAPClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, drAccRegistry.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.PeriodApportionmentARClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, crAccRegistry.PK.ToGuid());

			var chargeCode = testObjectCreator.FRT;
			var glpOverride = chargeCode.GLPostingOverrides.AddNew();
			glpOverride.Y1_ConsolidationAccountingCategoryClass = ConsolidatedAccountingCategoryClassList.Codes.All;
			glpOverride.Y1_GE = testObjectCreator.FIADepartment.PK;

			chargeCode.AC_AG_CostClearingAccount = drAccChargeCode.PK;
			chargeCode.AC_AG_RevenueClearingAccount = crAccChargeCode.PK;
			glpOverride.Y1_AG_CST_Clearing = drAccGLPOverride.PK;
			glpOverride.Y1_AG_REV_Clearing = crAccGLPOverride.PK;

			Factory.Save();

			var apInvoice = testObjectCreator.CreateInvoice(typeof(APInvoice), testObjectCreator.AUD, 1m, testObjectCreator.Creditor1);
			var arInvoice = testObjectCreator.CreateInvoice(typeof(ARInvoice), testObjectCreator.AUD, 1m, testObjectCreator.Debtor1);

			apInvoice.AH_GE = testObjectCreator.FIADepartment.PK;
			arInvoice.AH_GE = testObjectCreator.FIADepartment.PK;

			var apInvoiceLine = (InvoicingLineBase)apInvoice.Lines.AddNew();
			var arInvoiceLine = (InvoicingLineBase)arInvoice.Lines.AddNew();

			apInvoiceLine.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.Default;
			arInvoiceLine.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.Default;

			void assertAPClearing(AccGLHeader acc) => AssertEquals(acc?.AG_AccountNum, apInvoiceLine.PeriodClearingGLAccount?.AG_AccountNum);
			void assertARClearing(AccGLHeader acc) => AssertEquals(acc?.AG_AccountNum, arInvoiceLine.PeriodClearingGLAccount?.AG_AccountNum);

			assertAPClearing(null);
			assertARClearing(null);

			apInvoiceLine.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.EquallyOverPeriods;
			arInvoiceLine.PeriodApportionmentMethod = PeriodApportionmentMethods.Codes.EquallyOverPeriods;

			assertAPClearing(drAccRegistry);
			assertARClearing(crAccRegistry);

			apInvoiceLine.AL_AC = chargeCode.PK;
			arInvoiceLine.AL_AC = chargeCode.PK;

			assertAPClearing(drAccGLPOverride);
			assertARClearing(crAccGLPOverride);

			glpOverride.Delete();
			apInvoiceLine.PeriodApportionment.DefaultClearingAccount();
			arInvoiceLine.PeriodApportionment.DefaultClearingAccount();

			assertAPClearing(drAccChargeCode);
			assertARClearing(crAccChargeCode);
		}

		public void TestClearingAccountValidationAndLookups()
		{
			var acc1 = Factory.NewWithValidTestData<AccGLHeader>();
			var acc2 = Factory.NewWithValidTestData<AccGLHeader>();
			var acc3 = Factory.NewWithValidTestData<AccGLHeader>();
			var acc4 = Factory.NewWithValidTestData<AccGLHeader>();
			var acc5 = Factory.NewWithValidTestData<AccGLHeader>();
			var acc6 = Factory.NewWithValidTestData<AccGLHeader>();
			var acc7 = Factory.NewWithValidTestData<AccGLHeader>();

			acc1.AG_AccountNum = "9121.00.00";
			acc2.AG_AccountNum = "9122.00.00";
			acc3.AG_AccountNum = "9123.00.00";
			acc4.AG_AccountNum = "9124.00.00";
			acc5.AG_AccountNum = "9125.00.00";
			acc6.AG_AccountNum = "9126.00.00";
			acc7.AG_AccountNum = "9127.00.00";

			acc1.AG_AccountType = Enterprise.Core.Constants.AccountType.ProfitAndLossAccount;
			acc2.AG_AccountType = Enterprise.Core.Constants.AccountType.ProfitAndLossAccount;
			acc3.AG_AccountType = Enterprise.Core.Constants.AccountType.BalanceSheetAccount;
			acc4.AG_AccountType = Enterprise.Core.Constants.AccountType.BalanceSheetAccount;
			acc5.AG_AccountType = Enterprise.Core.Constants.AccountType.Alternate;
			acc6.AG_AccountType = Enterprise.Core.Constants.AccountType.Consolidation;
			acc7.AG_AccountType = Enterprise.Core.Constants.AccountType.BalanceSheetAccount;

			acc1.AG_IsGlobal = true;
			acc2.AG_IsGlobal = true;
			acc3.AG_IsGlobal = true;
			acc4.AG_IsGlobal = true;
			acc5.AG_IsGlobal = true;
			acc6.AG_IsGlobal = true;
			acc7.AG_IsGlobal = true;

			acc1.AG_ControlAccount = true;
			acc2.AG_ControlAccount = false;
			acc3.AG_ControlAccount = true;
			acc4.AG_ControlAccount = false;
			acc5.AG_ControlAccount = true;
			acc6.AG_ControlAccount = true;
			acc7.AG_ControlAccount = false;

			acc1.AG_DisallowDirectPosting = false;
			acc2.AG_DisallowDirectPosting = false;
			acc3.AG_DisallowDirectPosting = false;
			acc4.AG_DisallowDirectPosting = false;
			acc5.AG_DisallowDirectPosting = false;
			acc6.AG_DisallowDirectPosting = false;
			acc7.AG_DisallowDirectPosting = true;

			Factory.Save();

			var testObjectCreator = new TestObjectCreator(Factory);
			var cc = testObjectCreator.FRT;

			void assertClearingAccount(AccGLHeader acc, bool error = false)
			{
				cc.AC_AG_CostClearingAccount = acc.PK;
				cc.AC_AG_RevenueClearingAccount = acc.PK;

				cc.Validation.ValidateAC_AG_CostClearingAccount();
				cc.Validation.ValidateAC_AG_RevenueClearingAccount();

				if (!error)
				{
					AssertNoErrors(cc.AC_AG_CostClearingAccountInfo);
					AssertNoErrors(cc.AC_AG_RevenueClearingAccountInfo);
				}
				else
				{
					AssertHasError(cc.AC_AG_CostClearingAccountInfo, "Clearing Account must be a Balance Sheet account and must allow Direct Posting.");
					AssertHasError(cc.AC_AG_RevenueClearingAccountInfo, "Clearing Account must be a Balance Sheet account and must allow Direct Posting.");
				}
			}

			assertClearingAccount(acc1, true);
			assertClearingAccount(acc2, true);
			assertClearingAccount(acc3, true);
			assertClearingAccount(acc4);
			assertClearingAccount(acc5, true);
			assertClearingAccount(acc6, true);
			assertClearingAccount(acc7, true);
		}

		public void TestGlobalToLocalChargeCodeCopy()
		{
			var acc2 = Factory.NewWithValidTestData<AccGLHeader>();
			var acc4 = Factory.NewWithValidTestData<AccGLHeader>();
			acc2.AG_AccountNum = "9122.00.00";
			acc4.AG_AccountNum = "9124.00.00";
			acc2.AG_AccountType = Enterprise.Core.Constants.AccountType.BalanceSheetAccount;
			acc4.AG_AccountType = Enterprise.Core.Constants.AccountType.BalanceSheetAccount;
			acc2.AG_IsGlobal = true;
			acc4.AG_IsGlobal = true;
			acc2.AG_ControlAccount = false;
			acc4.AG_ControlAccount = false;
			acc2.AG_DisallowDirectPosting = false;
			acc4.AG_DisallowDirectPosting = false;

			Factory.Save();

			var globalCC = Factory.NewWithValidTestData<AccChargeCode>();
			globalCC.AC_Desc = "test";
			globalCC.AC_ChargeType = ChargeType.NonAccrual;
			globalCC.AC_GC = ZGuid.Empty;
			globalCC.AC_Code = "COVID19";

			globalCC.AC_AG_CostClearingAccount = acc2.PK;
			globalCC.AC_AG_RevenueClearingAccount = acc4.PK;

			Factory.Save();

			var localCCQuery = new ZQuery(AccChargeCodeSchema.AC_Code, "COVID19");
			localCCQuery.AddToFilter(AccChargeCodeSchema.AC_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty);
			var localCCs = Factory.Load<AccChargeCode>(localCCQuery);

			AssertEquals(2, localCCs.Length);

			foreach (var localCC in localCCs)
			{
				AssertEquals(acc2.PK, localCC.AC_AG_CostClearingAccount);
				AssertEquals(acc4.PK, localCC.AC_AG_RevenueClearingAccount);
			}
		}

		public void TestClearingAccountReadOnly()
		{
			var acc2 = Factory.NewWithValidTestData<AccGLHeader>();
			var acc4 = Factory.NewWithValidTestData<AccGLHeader>();
			acc2.AG_AccountNum = "9122.00.00";
			acc4.AG_AccountNum = "9124.00.00";

			var cc = Factory.NewWithValidTestData<AccChargeCode>();

			cc.AC_ChargeType = ChargeType.NonAccrual;
			cc.AC_AG_CostClearingAccount = acc2.PK;
			cc.AC_AG_RevenueClearingAccount = acc4.PK;

			AssertEquals(acc2.PK, cc.AC_AG_CostClearingAccount);
			AssertEquals(acc4.PK, cc.AC_AG_RevenueClearingAccount);
			Assert(!cc.AC_AG_CostClearingAccountInfo.ReadOnly);
			Assert(!cc.AC_AG_RevenueClearingAccountInfo.ReadOnly);

			cc.AC_ChargeType = ChargeType.Revenue;
			AssertEquals(ZGuid.Empty, cc.AC_AG_CostClearingAccount);
			AssertEquals(acc4.PK, cc.AC_AG_RevenueClearingAccount);
			Assert(cc.AC_AG_CostClearingAccountInfo.ReadOnly);
			Assert(!cc.AC_AG_RevenueClearingAccountInfo.ReadOnly);

			cc.AC_ChargeType = ChargeType.NonAccrual;
			cc.AC_AG_CostClearingAccount = acc2.PK;
			cc.AC_AG_RevenueClearingAccount = acc4.PK;
			cc.AC_ChargeType = ChargeType.Overhead;

			AssertEquals(acc2.PK, cc.AC_AG_CostClearingAccount);
			AssertEquals(ZGuid.Empty, cc.AC_AG_RevenueClearingAccount);
			Assert(!cc.AC_AG_CostClearingAccountInfo.ReadOnly);
			Assert(cc.AC_AG_RevenueClearingAccountInfo.ReadOnly);

			cc.AC_ChargeType = ChargeType.NonAccrual;
			cc.AC_AG_CostClearingAccount = acc2.PK;
			cc.AC_AG_RevenueClearingAccount = acc4.PK;
			cc.AC_ChargeType = ChargeType.Comment;

			AssertEquals(ZGuid.Empty, cc.AC_AG_CostClearingAccount);
			AssertEquals(ZGuid.Empty, cc.AC_AG_RevenueClearingAccount);
			Assert(cc.AC_AG_CostClearingAccountInfo.ReadOnly);
			Assert(cc.AC_AG_RevenueClearingAccountInfo.ReadOnly);
		}
	}
}
