using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IL.GUI.Testing
{
	sealed class BaseInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestTariffColumnNotExists_UniversalTariffFalse()
		{
			using (var control = new BaseInvoiceLineUserControl())
			{
				AssertNotNull(control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(BaseJobComInvoiceLine.Schema.JI_Tariff));
			}
		}
	}
}
