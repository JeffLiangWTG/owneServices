using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration.Testing;

public class JobComInvoiceHeaderCollectionTest : Customs.Business.Testing.BaseJobComInvoiceHeaderCollectionTest
{
	public void TestAtLeastOneInvoiceHasABuyer()
	{
		JobDeclaration dec = Factory.New<JobDeclaration>();
		InvoiceHeaderActiveCollection invs = dec.Invoices;
		JobComInvoiceHeader inv = invs.AddNew();
		AssertEquals(false, invs.AtLeastOneInvoiceHasABuyer);
		inv.JZ_OH_Buyer = Factory.New<OrgHeader>().PK;
		AssertEquals(true, invs.AtLeastOneInvoiceHasABuyer);
	}

	public void TestAtLeastOneInvoiceHasASupplier()
	{
		JobDeclaration dec = Factory.New<JobDeclaration>();
		InvoiceHeaderActiveCollection invs = dec.Invoices;
		JobComInvoiceHeader inv = invs.AddNew();
		AssertEquals(false, invs.AtLeastOneInvoiceHasASupplier);
		inv.JZ_OH_Supplier = Factory.New<OrgHeader>().PK;
		AssertEquals(true, invs.AtLeastOneInvoiceHasASupplier);
	}
}
