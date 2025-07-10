using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.Transfer.Testing
{
	public class BankTransferReversalValidationTest : TestCaseWithFactory
	{
		public void TestValidateAll()
		{
			BankTransfer testBankTransfer = new BankTransfer(Factory, null);
			testBankTransfer.IsReverseTransaction = true;
			testBankTransfer.AH_PostDate = ZDateTime.Empty;
			testBankTransfer.FinanceChargeOSAmount = -20m;
			testBankTransfer.Validation.ValidateAll();

			AssertEquals(true, testBankTransfer.AH_PostDateInfo.HasErrors());
			AssertEquals(false, testBankTransfer.FinanceChargeOSTotalInfo.HasErrors());
		}

		[TestDate(2006, 10, 21)]
		public void TestValidateAH_PostDate()
		{
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ZDateTime originalPostDate = new ZDateTime(2006, 10, 20);
			AccountingPeriodCalculator periodCalculator = new AccountingPeriodCalculator(Factory);
			AccPeriodManagement accPeriod = periodCalculator.GetPeriodManagementFromDate(originalPostDate);
			if (accPeriod == null)
			{
				accPeriod = Factory.New<AccPeriodManagement>();
				accPeriod.AM_GC_Company = GlbCompany.CurrentCompany.PK;
				accPeriod.AM_Period = periodCalculator.GetPeriodFromDate(originalPostDate);
				accPeriod.AM_Year = (short)(accPeriod.AM_Period / 100);
				accPeriod.AM_StartDate = new ZDateTime(originalPostDate.Year, originalPostDate.Month, 1);
				accPeriod.AM_EndDate = accPeriod.AM_StartDate.AddMonths(1).AddDays(-1);
			}
			Factory.Save();

			BankTransfer testBankTransfer = new BankTransfer(Factory, null);
			testBankTransfer.AH_PostDate = originalPostDate;

			ReversingFactory reversingFactory = new ReversingFactory();
			ReversingBase reversing = reversingFactory.NewReversing(testBankTransfer);
			reversing.Reverse();

			BankTransfer reverseBankTransfer = reversing.ReverseTransaction as BankTransfer;
			AssertEquals("AH_PostDateInfo.HasErrors()", false, reverseBankTransfer.AH_PostDateInfo.HasErrors());

			reverseBankTransfer.AH_PostDate = originalPostDate.AddDays(-1);
			AssertEquals("AH_PostDateInfo.HasErrors()", true, reverseBankTransfer.AH_PostDateInfo.HasErrors());
			string expectedError = "Reversing post date cannot be before the original post date of '" + testBankTransfer.AH_PostDate.ToShortDateString() + "'.";
			AssertEquals("Contains(ExpectedError)", true, reverseBankTransfer.AH_PostDateInfo.GetErrors().Contains(expectedError));

			reverseBankTransfer.AH_PostDate = originalPostDate;
			AssertEquals("AH_PostDateInfo.HasErrors()", false, reverseBankTransfer.AH_PostDateInfo.HasErrors());

			reverseBankTransfer.AH_PostDate = originalPostDate.AddDays(1);
			AssertEquals("AH_PostDateInfo.HasErrors()", false, reverseBankTransfer.AH_PostDateInfo.HasErrors());
		}
	}
}