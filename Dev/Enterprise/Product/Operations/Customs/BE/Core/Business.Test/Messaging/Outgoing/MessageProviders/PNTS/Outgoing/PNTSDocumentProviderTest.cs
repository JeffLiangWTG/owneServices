using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class PNTSDocumentProviderTest : Customs.Business.Testing.DataProviderTestCase<PNTSDocumentProvider>
{
	public void TestReferenceNumber()
	{
		bill.ABL_BillNumber = "BillNumber";
		AssertEquals("BillNumber", provider.ReferenceNumber);
	}

	public void TestType()
	{
		bill.TypeOfBillDocument = "Type";
		AssertEquals("Type", provider.Type);
	}

	protected override PNTSDocumentProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		bill = Factory.New<TemporaryStorageBill>();
		provider = new PNTSDocumentProvider(bill);
	}

	TemporaryStorageBill bill;
	PNTSDocumentProvider provider;
}
