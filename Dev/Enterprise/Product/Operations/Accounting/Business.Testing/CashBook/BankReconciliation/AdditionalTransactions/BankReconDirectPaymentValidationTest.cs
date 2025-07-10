using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.CashBook.DirectPayment.Testing;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.Testing
{
	[TestedType(typeof(BankReconDirectPayment))]
	class BankReconDirectPaymentValidationTest : DirectPaymentValiationTest
	{
		[TestDate(2012, 1, 1)]
		public void TestCheckAH_InvoiceDate_InCollection()
		{
			AssertDateInCollection(TestBizO.AH_InvoiceDateInfo);

			TestBizO = Factory.NewWithValidTestData<BankReconDirectPayment>();
			AssertDateInPaymentCollection(TestBizO.AH_InvoiceDateInfo);
		}

		[TestDate(2012, 1, 1)]
		public void TestCheckAH_PostDate_InCollection()
		{
			Period period = Factory.New<Period>();
			period.AM_GC_Company = GlbCompany.CurrentCompany.PK;
			period.AM_StartDate = new ZDateTime(2006, 10, 1);
			period.AM_EndDate = new ZDateTime(2006, 10, 30);

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertDateInCollection(TestBizO.AH_PostDateInfo);

			TestBizO = Factory.NewWithValidTestData<BankReconDirectPayment>();
			AssertDateInPaymentCollection(TestBizO.AH_PostDateInfo);
		}

		void AssertDateInCollection(ZPropertyInfo info)
		{
			DirectTransactionHeaderBaseCollection collection = new DirectTransactionHeaderBaseCollection(Factory, new ZDateTime(2006, 10, 10));
			collection.Add(TestBizO);

			string errorMessage = info.Description + " cannot be after statement date of '" + collection.StatementDate.ToShortDateString() + "'.";
			info.Value = new ZDateTime(2006, 10, 9);
			AssertEquals("Should not contain error", false, info.HasError(errorMessage));

			info.Value = new ZDateTime(2006, 10, 11);
			AssertEquals("Should contain error", true, info.HasError(errorMessage));
		}

		void AssertDateInPaymentCollection(ZPropertyInfo info)
		{
			BankReconDirectPaymentCollection collection = new BankReconDirectPaymentCollection(Factory);
			collection.StatementDate = new ZDateTime(2006, 10, 10);
			collection.Add(TestBizO);

			string errorMessage = info.Description + " cannot be after statement date of '" + collection.StatementDate.ToShortDateString() + "'.";
			info.Value = new ZDateTime(2006, 10, 9);
			AssertEquals("Should not contain error", false, info.HasError(errorMessage));

			info.Value = new ZDateTime(2006, 10, 11);
			AssertEquals("Should contain error", true, info.HasError(errorMessage));
		}

		public override void TestChequeOrReferenceValidationIsDisabledForAutoAllocationMode()
		{
			Assert("Auto allocation is disabled for this class", true);
		}
	}
}
