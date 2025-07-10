using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class PreviousProcedureParentProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new PreviousProcedureParentProvider(null));
		}

		public void TestPreviousDocuments()
		{
			AssertSame(invoiceLine.PreviousProcedures, provider.PreviousDocuments);
		}

		public void TestPreviousDocumentMaster()
		{
			AssertSame(invoiceLine.PreviousProcedureMaster, provider.PreviousDocumentMaster);
		}

		public void TestJobDeclaration()
		{
			AssertSame(invoiceLine.Declaration, provider.JobDeclaration);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			provider = new PreviousProcedureParentProvider(invoiceLine);
		}

		JobComInvoiceLine invoiceLine;
		PreviousProcedureParentProvider provider;
	}
}
