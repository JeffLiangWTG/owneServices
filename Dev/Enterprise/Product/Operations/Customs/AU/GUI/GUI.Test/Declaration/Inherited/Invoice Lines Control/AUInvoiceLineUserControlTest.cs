using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class AUInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestInvoiceLineFilterBusinessObject()
		{
			using (var control = new AUInvoiceLineUserControl())
			{
				AssertType<AUInvoiceLineFilterBusinessObject>(control.FilterBusinessObject);
			}
		}
	}
}
