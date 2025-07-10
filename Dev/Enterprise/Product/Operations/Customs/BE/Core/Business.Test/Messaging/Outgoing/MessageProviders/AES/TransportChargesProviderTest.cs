using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business.Testing;

class TransportChargesProviderTest : Customs.Business.Testing.DataProviderTestCase<TransportChargesProvider>
{
	public void TestMethodOfPayment()
	{
		invoiceHeader.ZG_TransportChargesMethodOfPayment = "M";
		AssertEquals("M", provider.MethodOfPayment);
	}

	protected override TransportChargesProvider GetProvider() => provider;

	protected override void SetUp()
	{
		base.SetUp();
		invoiceHeader = Factory.NewWithValidTestData<JobComInvoiceHeader>();
		provider = new TransportChargesProvider(invoiceHeader);
	}
	JobComInvoiceHeader invoiceHeader;
	TransportChargesProvider provider;
}
