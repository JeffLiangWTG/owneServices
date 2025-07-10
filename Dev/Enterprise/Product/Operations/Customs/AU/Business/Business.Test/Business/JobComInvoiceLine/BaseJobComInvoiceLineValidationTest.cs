using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public abstract class BaseJobComInvoiceLineValidationTest : TestCaseWithFactory
	{
		#region Implementation
		protected JobDeclaration jobDec;
		protected JobComInvoiceHeader invoiceHeader;
		protected JobComInvoiceLine testInvoiceLine;
		protected JobComInvoiceLineValidation testInvoiceLineValidation;
		protected abstract JobComInvoiceLineValidation GetNewValidationProvider(JobComInvoiceLine invoiceLine);

		protected override void SetUp()
		{
			base.SetUp();
			jobDec = Factory.New<JobDeclaration>();
			invoiceHeader = jobDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			testInvoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			testInvoiceLineValidation = GetNewValidationProvider(testInvoiceLine);
		}

		#endregion
	}
}
