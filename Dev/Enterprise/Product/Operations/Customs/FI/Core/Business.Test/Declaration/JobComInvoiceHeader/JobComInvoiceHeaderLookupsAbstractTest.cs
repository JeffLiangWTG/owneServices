using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FI.Business.Testing;

[TestsSubclassesOf(typeof(JobComInvoiceHeaderLookups))]
abstract class JobComInvoiceHeaderLookupsAbstractTest<T> : BusinessObjectLookupsTestCase
	where T : JobComInvoiceHeaderLookups
{
	protected override void SetUp()
	{
		base.SetUp();
		jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = MessageType;
		invoice = jobDeclaration.Invoices.AddNew();
		lookups = GetLookups();
	}
	protected JobDeclaration jobDeclaration;
	protected JobComInvoiceHeader invoice;
	protected T lookups;

	protected abstract string MessageType { get; }
	protected abstract T GetLookups();
}
