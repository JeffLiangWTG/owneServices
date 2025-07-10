using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class ReferenceInvoiceManualLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBRStateList()
		{
			var parent = Factory.New<ReferenceInvoiceManual>();
			var listBRState = parent.Lookups.BRStateList;
			AssertEquals(27, listBRState.Count);
		}

		public void TestModelOfNotaFiscalList()
		{
			var parent = Factory.New<ReferenceInvoiceManual>();
			var listModel = parent.Lookups.ModelOfNotaFiscalList;
			AssertEquals(2, listModel.Count);
		}
	}
}
