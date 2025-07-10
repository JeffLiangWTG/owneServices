using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

sealed class AddInfoJobComInvoiceHeaderLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestTransportChargesModeOfPayment()
	{
		var header = Factory.New<JobComInvoiceHeader>();
		AssertEquals("Netherlands should contain CusCodeList items of type MOP from TransportChargesMethodOfPaymentList", "A, B, C, D, H, Y, Z", header.AddInfoLookups.TransportChargesMethodOfPaymentList.CodesAsString);
	}
}
