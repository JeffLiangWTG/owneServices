using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using static Enterprise.MasterFiles.Business.AccountingPeriodTestHelper;

namespace Enterprise.Accounting.Business.Testing
{
	public class JobInvoicingReverserValidationHelperTest : TestCaseWithFactory
	{
		public void TestValidate()
		{
			new AccountingPeriodTestHelper(Factory).PostPeriodsForEntireYear(ZDateTime.Today.Year, GlbCompany.CurrentCompany.PK, CalendarType.CalendarYear);

			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			Factory.Save();

			var originalInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			originalInvoice.Lines.Add(TestObjectCreator.CreateRevenueLine(charge, originalInvoice.PK));
			Factory.Save();

			originalInvoice.GenerateReverseTransaction(true);
			var reversedInvoice = originalInvoice.ReverseTransaction as TransactionHeader;

			var expectedError = @"This date does not fall into a valid accounting period’s date range.
Please go to Manage > General Ledger > Period Management > Set Up Next Accounting Year, to ensure there is an accounting period for the date you wish to post to.";

			var newFactory = new BusinessObjectFactory();
			new TestObjectCreator(newFactory).DeleteAllPeriodsForCurrentCompany();
			Factory.SetContext(BusinessContext.ReverseDateForm);

			AssertEquals(false, reversedInvoice.AH_PostDateInfo.HasError(expectedError));
			JobInvoicingReverserValidationHelper.Validate(reversedInvoice);
			AssertEquals(true, reversedInvoice.AH_PostDateInfo.HasError(expectedError));
		}

		public void TestHasErrors()
		{
			new AccountingPeriodTestHelper(Factory).PostPeriodsForEntireYear(ZDateTime.Today.Year, GlbCompany.CurrentCompany.PK, CalendarType.CalendarYear);

			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "charge1", TestObjectCreator.AUD, 10m, TestObjectCreator.Creditor1, TestObjectCreator.AUD, 10m, TestObjectCreator.AALSHI);
			charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
			Factory.Save();

			var originalInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "AR001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			originalInvoice.Lines.Add(TestObjectCreator.CreateRevenueLine(charge, originalInvoice.PK));
			Factory.Save();

			originalInvoice.GenerateReverseTransaction(true);
			var reversedInvoice = originalInvoice.ReverseTransaction as TransactionHeader;

			var newFactory = new BusinessObjectFactory();
			new TestObjectCreator(newFactory).DeleteAllPeriodsForCurrentCompany();
			Factory.SetContext(BusinessContext.ReverseDateForm);

			AssertEquals(false, JobInvoicingReverserValidationHelper.HasErrors(reversedInvoice));
			JobInvoicingReverserValidationHelper.Validate(reversedInvoice);
			AssertEquals(true, JobInvoicingReverserValidationHelper.HasErrors(reversedInvoice));
		}

		protected TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
