using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

class AddInfoJobComInvoiceHeaderLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestTransportChargesMethodOfPaymentList()
	{
		var lookups = new AddInfoJobComInvoiceHeaderLookups(new AddInfoJobComInvoiceHeader(Factory.New<JobComInvoiceHeader>().JZ_AddInfoInfo));
		var transportChargesMethodOfPaymentList = lookups.TransportChargesMethodOfPaymentList;
		CombineAssertions(() =>
		{
			AssertEquals("Codes", "A, B, C, D, H, Y, Z", transportChargesMethodOfPaymentList.CodesAsString);
			AssertSame("Cached", lookups.TransportChargesMethodOfPaymentList, transportChargesMethodOfPaymentList);
		});
	}
}
