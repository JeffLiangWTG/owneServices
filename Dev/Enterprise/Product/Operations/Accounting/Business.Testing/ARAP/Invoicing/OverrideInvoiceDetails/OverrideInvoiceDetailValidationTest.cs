using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public abstract class OverrideInvoiceDetailValidationTest : TransactionHeaderValidationTest
	{
		public override void TestCheckAH_PostDate()
		{
			InvoicingBase header = (InvoicingBase)Factory.New(InvoiceType);

			BusinessObjectFactory periodFactory = new BusinessObjectFactory();
			AccountingPeriodCalculator periodCalculator = new AccountingPeriodCalculator(periodFactory);

			AccPeriodManagement accPeriod = periodCalculator.GetPeriodManagementFromDate(ZDateTime.Today);
			if (accPeriod == null)
			{
				accPeriod = periodFactory.New<AccPeriodManagement>();
				accPeriod.AM_GC_Company = GlbCompany.CurrentCompany.PK;
				accPeriod.AM_Period = periodCalculator.GetPeriodFromDate(ZDateTime.Today);
				accPeriod.AM_Year = (short)(accPeriod.AM_Period / 100);
				accPeriod.AM_StartDate = new ZDateTime(ZDateTime.Today.Year, ZDateTime.Today.Month, 1);
				accPeriod.AM_EndDate = accPeriod.AM_StartDate.AddMonths(1).AddDays(-1);
			}
			periodFactory.Save();

			SetBusinessContext(header);
			AssertType("Precondition: correct validation type tested here", GetValidation(header).GetType(), header.Validation);

			header.AH_PostDate = ZDateTime.Today;
			AssertEquals("Period is open", false, header.AH_PostDateInfo.HasErrors());

			accPeriod.AM_IsSubLedgerClosed = true;
			accPeriod.AM_IsGeneralLedgerClosed = true;
			periodFactory.Save();
			header.Validation.ValidateAH_PostDate();
			AssertEquals("Period closed but description is able to be edited", false, header.AH_PostDateInfo.HasErrors());

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Assert(!header.AllowBackPosting);
			header.AH_PostDate = ZDateTime.Today.AddDays(-1);
			header.Validation.ValidateAH_PostDate();
			AssertEquals("Should still have no error for past date", false, header.AH_PostDateInfo.HasErrors());
		}

		protected abstract OverrideInvoiceDetailValidation GetValidation(TransactionHeader parent);
		protected abstract Type InvoiceType { get; }

		protected override Type HeaderType => InvoiceType;

		protected abstract void SetBusinessContext(InvoicingBase invoice);
	}
}
