using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

[TestedType(typeof(AddInfoJobComInvoiceHeader))]
class AddInfoJobComInvoiceHeaderTest : NonPersistentBusinessObjectTestCase
{
	public void TestValidation()
	{
		AssertType<AddInfoJobComInvoiceHeaderValidation>(AddInfoJobComInvoiceHeader.Validation);
	}

	public void TestLookups()
	{
		AssertType<AddInfoJobComInvoiceHeaderLookups>(AddInfoJobComInvoiceHeader.Lookups);
	}

	protected override BusinessObject GetNewBusinessObject() => AddInfoJobComInvoiceHeader;

	AddInfoJobComInvoiceHeader AddInfoJobComInvoiceHeader => addInfoJobComInvoiceHeader ?? (addInfoJobComInvoiceHeader = new AddInfoJobComInvoiceHeader(Factory.New<JobComInvoiceHeader>().JZ_AddInfoInfo));
	AddInfoJobComInvoiceHeader addInfoJobComInvoiceHeader;
}
