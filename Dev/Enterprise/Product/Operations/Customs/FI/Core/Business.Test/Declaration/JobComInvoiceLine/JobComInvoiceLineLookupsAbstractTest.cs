using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FI.Business.Testing;

[TestsSubclassesOf(typeof(JobComInvoiceLineLookups))]
abstract class JobComInvoiceLineLookupsAbstractTest<T> : BusinessObjectLookupsTestCase
	where T : JobComInvoiceLineLookups
{
	protected override void SetUp()
	{
		base.SetUp();
		jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = MessageType;
		invoice = jobDeclaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		lookups = GetLookups();
	}
	protected JobDeclaration jobDeclaration;
	protected JobComInvoiceHeader invoice;
	protected JobComInvoiceLine invoiceLine;
	protected T lookups;

	protected abstract string MessageType { get; }
	protected abstract T GetLookups();
}
