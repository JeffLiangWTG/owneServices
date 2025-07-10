using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	sealed class EMCSJobComInvoiceHeaderLookupsTest : JobComInvoiceHeaderLookupsTest
	{
		public void TestJZ_JZ_GroupInvoiceFK_List()
		{
			var invoiceHeader = Factory.New<EMCSJobComInvoiceHeader>();
			var lookups = invoiceHeader.Lookups;

			var list = lookups.JZ_JZ_GroupInvoiceFK_List;
			AssertEquals(typeof(EMCSGroupHeaderCollection), list.GetType());
		}
	}
}
