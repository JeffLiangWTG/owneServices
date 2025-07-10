using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestsSubclassesOf(typeof(JobComInvoiceLineValidation))]
abstract class JobComInvoiceLineValidationAbstractTest<T> : BusinessObjectValidationTestCase
	where T : JobComInvoiceLineValidation
{
	protected override void SetUp()
	{
		base.SetUp();
		jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = MessageType;
		invoice = jobDeclaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		validation = GetValidation();
	}
	protected JobDeclaration jobDeclaration;
	protected JobComInvoiceHeader invoice;
	protected JobComInvoiceLine invoiceLine;
	protected T validation;

	protected abstract string MessageType { get; }

	protected abstract T GetValidation();
}
