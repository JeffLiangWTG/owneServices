using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.CashBook.ExchangeDifference.Testing
{
	public class CashbookExchangeDiffCalculation_InnerTest : TestCaseWithFactory
	{
		public void TestOSBankBalance()
		{
			Bank.AB_OpenOSBalance = 50m;
			Factory.Save();

			CashbookExchangeDiffCalculation calculator = new CashbookExchangeDiffCalculation(Bank);
			calculator.PostDate = ZDateTime.Today;
			AssertEquals("OSBankBalance should be 50", 50m, calculator.FOSBankBalance_ForTestOnly);
		}

		public void TestLocalBankBalance()
		{
			Bank.AB_OpenBalance = 60m;
			Bank.AB_OpenOSBalance = 70m;
			Factory.Save();

			CashbookExchangeDiffCalculation calculator = new CashbookExchangeDiffCalculation(Bank);
			calculator.PostDate = ZDateTime.Today;
			AssertEquals("LocalBankBalance should be 60", 60m, calculator.FLocalBankBalance_ForTestOnly);
		}

		#region LocalAmount

		public void TestLocalAmount()
		{
			CreateTestTransactions();

			CashbookExchangeDiffCalculation calculator = new CashbookExchangeDiffCalculation(Bank);
			calculator.PostDate = ZDateTime.Today;
			AssertEquals("LocalAmount should be should be 45", 45m, calculator.FLocalAmount_ForTestOnly);
		}

		#endregion

		#region BankCurrencyBalance

		public void TestBankCurrencyBalance()
		{
			CreateTestTransactions();

			CashbookExchangeDiffCalculation calculator = new CashbookExchangeDiffCalculation(Bank);
			calculator.PostDate = ZDateTime.Today;
			AssertEquals("BankCurrencyBalance should be 60", 60m, calculator.BankCurrencyBalance);
		}

		#endregion

		#region LocalAmountBeforeAdjustment

		public void TestLocalAmountBeforeAdjustment()
		{
			Bank.AB_OpenOSBalance = 90m;
			Bank.AB_OpenBalance = 35m;

			DirectPayment.DirectPayment dPY = Factory.NewWithValidTestData<DirectPayment.DirectPayment>();
			dPY.AH_OSTotal = -11m;
			dPY.AH_InvoiceAmount = -10m;
			dPY.AH_GSTAmount = -1m;
			dPY.AH_AB = Bank.PK;
			dPY.AH_ExchangeRate = 1m;
			dPY.Lines.AddNew(dPY.DependentTransactionLineType);
			dPY.Lines[0].AL_LineAmount = -10m;
			dPY.Lines[0].AL_OSAmount = -11m;
			dPY.Lines[0].AL_GSTVAT = -1m;

			ARReceipt aRRec = Factory.NewWithValidTestData<ARReceipt>();
			aRRec.AH_AB = Bank.PK;
			aRRec.AH_ExchangeRate = 1m;
			aRRec.AH_OSTotal = -100m;
			aRRec.AH_InvoiceAmount = -100m;
			aRRec.AH_OutstandingAmount = -100m;

			Factory.Save();

			CashbookExchangeDiffCalculation calculator = new CashbookExchangeDiffCalculation(Bank);
			calculator.PostDate = ZDateTime.Today;
			AssertEquals("Local Amount before adjustment should be 124", 124m, calculator.LocalAmountBeforeAdjustment);
		}

		#endregion

		#region CurrentExchangeRate

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestCurrentExchangeRate()
		{
			bool originalIsReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			GlbCompany currentCompany = newFactory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			currentCompany.GC_IsReciprocal = true;
			newFactory.Save();
			Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));

			try
			{
				CreateTestTransactions();

				CashbookExchangeDiffCalculation calculation = new CashbookExchangeDiffCalculation(Bank);
				calculation.PostDate = ZDateTime.Today;
				AssertEquals("Exchange Rate should be 1.166667", 1.166667m, calculation.CurrentExchangeRate);

				currentCompany.GC_IsReciprocal = false;
				newFactory.Save();
				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));

				AssertEquals("Exchange Rate should be 0.857143", 0.857143m, calculation.CurrentExchangeRate);
			}
			finally
			{
				currentCompany.GC_IsReciprocal = originalIsReciprocal;
				newFactory.Save();
				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
			}
		}

		#endregion

		#region LocalAmountAfterAdjustment

		public void TestGetLocalAmountAfterAdjustment()
		{
			CreateTestTransactions();

			CashbookExchangeDiffCalculation calculator = new CashbookExchangeDiffCalculation(Bank);
			calculator.PostDate = ZDateTime.Today;
			AssertEquals("BankCurrencyBalance should be 60", 60m, calculator.BankCurrencyBalance);

			calculator = new CashbookExchangeDiffCalculation(Bank);
			calculator.PostDate = ZDateTime.Today;
			AssertEquals("Local amount should be 30.00", 30.00m, calculator.GetLocalAmountAfterAdjustment(2));
		}

		#endregion

		#region ForeignCurrencyGainLoss

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestForeignCurrencyGainLoss()
		{
			bool originalIsReciprocal = GlbCompany.CurrentCompany.GC_IsReciprocal;

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			GlbCompany currentCompany = newFactory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			currentCompany.GC_IsReciprocal = false;
			newFactory.Save();
			Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));

			try
			{
				ARReceipt aRRec = Factory.NewWithValidTestData<ARReceipt>();
				aRRec.AH_AB = Bank.PK;
				aRRec.AH_ExchangeRate = 1.8m;
				aRRec.AH_OSTotal = -90m;
				aRRec.AH_InvoiceAmount = -50m;
				aRRec.AH_OutstandingAmount = -50m;

				APPayment aPPay = Factory.NewWithValidTestData<APPayment>();
				aPPay.AH_AB = Bank.PK;
				aPPay.AH_ExchangeRate = 30m / 14m;
				aPPay.AH_OSTotal = 30m;
				aPPay.AH_InvoiceAmount = 14m;
				aPPay.AH_OutstandingAmount = 14m;

				Bank.AB_OpenOSBalance = 40m;
				Bank.AB_OpenBalance = 20m;

				Factory.Save();
				// 100 / ExRate - 56
				CashbookExchangeDiffCalculation calculator = new CashbookExchangeDiffCalculation(Bank);
				calculator.PostDate = ZDateTime.Today;
				AssertEquals("Local amount difference is -6", -6m, calculator.GetForeignCurrencyGainLoss(2m));
			}
			finally
			{
				currentCompany.GC_IsReciprocal = originalIsReciprocal;
				newFactory.Save();
				Env.SetUserContext(new UserContext(Env.CurrentUser.LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()));
			}
		}

		#endregion

		#region AmountValuesCaches

		public void TestAmountValuesCaches()
		{
			CreateTestTransactions();

			CashbookExchangeDiffCalculation calculator = new CashbookExchangeDiffCalculation(Bank);
			calculator.PostDate = ZDateTime.Today;

			AssertEquals("BankCurrencyBalance should be 60", 60m, calculator.BankCurrencyBalance);
			AssertEquals("Local Amount before adjustment should be 45", 45m, calculator.LocalAmountBeforeAdjustment);

			calculator.PostDate = ZDateTime.Today.AddDays(1); //Call the load method

			var dpy = Factory.NewWithValidTestData<DirectPayment.DirectPayment>();
			dpy.AH_OSTotal = -20m;
			dpy.AH_InvoiceAmount = -20m;
			dpy.AH_AB = Bank.PK;
			dpy.AH_ExchangeRate = 1m;
			dpy.Lines.AddNew(dpy.DependentTransactionLineType);
			dpy.Lines[0].AL_LineAmount = -20m;
			dpy.Lines[0].AL_OSAmount = -20m;

			Factory.Save();

			calculator.PostDate = ZDateTime.Today; //Call the load method
			AssertEquals("BankCurrencyBalance shouldn't be changed because of the cache", 60m, calculator.BankCurrencyBalance);
			AssertEquals("Local Amount before adjustment shouldn't be changed because of the cache", 45m, calculator.LocalAmountBeforeAdjustment);

			calculator.ClearAmountValuesCaches();

			calculator.PostDate = ZDateTime.Today.AddDays(1); //Call the load method
			AssertEquals("BankCurrencyBalance should be changed because the cache has been cleared", 40m, calculator.BankCurrencyBalance);
			AssertEquals("Local Amount before adjustment should be changed because the cache has been cleared", 25m, calculator.LocalAmountBeforeAdjustment);
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			Bank = Factory.NewWithValidTestData<AccBankAccount>();
			Bank.AB_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			Factory.Save();
		}

		AccBankAccount Bank;

		void CreateTestTransactions()
		{
			DirectReceipt.DirectReceipt dRC = Factory.NewWithValidTestData<DirectReceipt.DirectReceipt>();
			dRC.AH_AB = Bank.PK;
			dRC.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			dRC.AH_ExchangeRate = 1;
			dRC.Lines.AddNew();
			DirectTransactionLineBase dRCLine = (DirectTransactionLineBase)dRC.Lines[0];
			dRCLine.AL_OSExTaxAmount = 100m;

			DirectPayment.DirectPayment dPY = Factory.NewWithValidTestData<DirectPayment.DirectPayment>();
			dPY.AH_AB = Bank.PK;
			dPY.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			dPY.AH_OSExTaxAmount = 30m;
			dPY.AH_ExchangeRate = 2M / 3M;
			dPY.Lines.AddNew();
			DirectTransactionLineBase dPYLine = (DirectTransactionLineBase)dPY.Lines[0];
			dPYLine.AL_OSExTaxAmount = 30m;

			APPayment aPPay = Factory.NewWithValidTestData<APPayment>();
			aPPay.AH_AB = Bank.PK;
			aPPay.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			aPPay.AH_ExchangeRate = 0.5M;
			aPPay.AH_OSTotal = 20m;
			aPPay.AH_InvoiceAmount = 10m;
			aPPay.AH_OutstandingAmount = 10m;

			Bank.AB_OpenOSBalance = 10m;

			Factory.Save();
		}
	}
}
