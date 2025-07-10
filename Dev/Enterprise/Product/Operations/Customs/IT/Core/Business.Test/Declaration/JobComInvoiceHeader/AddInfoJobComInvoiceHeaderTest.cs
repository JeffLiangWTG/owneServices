using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(AddInfoJobComInvoiceHeader))]
sealed class AddInfoJobComInvoiceHeaderTest : NonPersistentBusinessObjectTestCase
{
	public void TestValidationType()
	{
		AssertType<AddInfoJobComInvoiceHeaderValidation>(addInfoInvoiceHeader.Validation);
	}

	public void TestDeclaration()
	{
		AssertNull("When invoice does not link to a JobDeclaration, AddInfoJobComInvoiceHeader.Declaration should be null", addInfoInvoiceHeader.Parent.JobDeclaration);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var jobComInvoiceHeader = Factory.New<JobComInvoiceHeader>();
		return new AddInfoJobComInvoiceHeader(jobComInvoiceHeader.JZ_AddInfoInfo);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var jobComInvoiceHeader = Factory.New<JobComInvoiceHeader>();
		addInfoInvoiceHeader = new AddInfoJobComInvoiceHeader(jobComInvoiceHeader.JZ_AddInfoInfo);
	}
	AddInfoJobComInvoiceHeader addInfoInvoiceHeader;
}
