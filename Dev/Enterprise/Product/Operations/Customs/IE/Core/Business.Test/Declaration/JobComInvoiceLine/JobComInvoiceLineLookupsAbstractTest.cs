using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestsSubclassesOf(typeof(JobComInvoiceLineLookups))]
	abstract class JobComInvoiceLineLookupsAbstractTest : BusinessObjectLookupsTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = MessageType;
			var invoice = jobDeclaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();

			lookups = GetLookups();
		}

		protected JobDeclaration jobDeclaration;

		protected JobComInvoiceLine invoiceLine;

		protected JobComInvoiceLineLookups lookups;

		protected abstract string MessageType { get; }

		protected abstract JobComInvoiceLineLookups GetLookups();
	}
}
